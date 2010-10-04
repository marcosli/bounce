using System;
using System.IO;

namespace Bounce.Framework {
    class Bounce : ITargetBuilderBounce {
        private readonly TextWriter stdout;
        private readonly TextWriter stderr;
        private readonly ITaskLogFactory logFactory;
        public ILog Log { get; private set; }

        public LogOptions LogOptions { get; set; }

        public Bounce(TextWriter stdout, TextWriter stderr, ITaskLogFactory logFactory) {
            this.stdout = stdout;
            this.stderr = stderr;
            this.logFactory = logFactory;
            LogOptions = new LogOptions {CommandOutput = false, LogLevel = LogLevel.Warning, ReportTargetEnd = true, ReportTargetStart = true, ReportTaskEnd = true, ReportTaskStart = true};
        }

        public ITaskScope TaskScope(ITask task, BounceCommand command, string targetName) {
            return CreateTaskScope(task, command, targetName);
        }

        private ITaskScope CreateTaskScope(ITask task, BounceCommand command, string targetName) {
            ILog previousLogger = Log;
            Log = logFactory.CreateLogForTask(task, stdout, stderr, LogOptions);
            if (targetName != null) {
                Log.TaskLog.BeginTarget(task, targetName, command);
            } else {
                Log.TaskLog.BeginTask(task, command);
            }
            return new TaskScope(
                outerLogger => EndTaskLog(task, command, TaskResult.Success, targetName, outerLogger),
                outerLogger => EndTaskLog(task, command, TaskResult.Failure, targetName, outerLogger),
                previousLogger);
        }

        private void EndTaskLog(ITask task, BounceCommand command, TaskResult result, string targetName, ILog outerLogger) {
            if (targetName != null) {
                Log.TaskLog.EndTarget(task, targetName, command, result);
            } else {
                Log.TaskLog.EndTask(task, command, result);
            }
            Log = outerLogger;
        }
    }

    public interface ITaskScope : IDisposable {
        void TaskSucceeded();
    }
}