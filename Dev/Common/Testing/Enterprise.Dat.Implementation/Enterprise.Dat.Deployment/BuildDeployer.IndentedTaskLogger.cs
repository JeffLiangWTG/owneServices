using System;
using CargoWise.Common;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	partial class BuildDeployer
	{
		internal sealed class IndentedTaskLogger : ITaskLogger
		{
			readonly ITaskLogger taskLogger;

			public IndentedTaskLogger(ITaskLogger taskLogger)
			{
				this.taskLogger = taskLogger;
			}

			public void RecordInfo(string message)
			{
				if (level > 0)
				{
					message = "    " + message.Replace("\n", "\n    ");
				}

				taskLogger.RecordInfo(message);
			}

			int level;
			public IDisposable RecordTask(string taskInfo)
			{
				level++;
				var disposable = taskLogger.RecordTask(taskInfo);
				return new DisposableAction(() =>
				{
					disposable?.Dispose();
					level--;
				});
			}
		}
	}
}
