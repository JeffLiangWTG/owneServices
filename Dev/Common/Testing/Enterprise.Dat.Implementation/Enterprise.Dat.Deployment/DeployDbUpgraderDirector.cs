#if DEBUG
using Dat.Integration;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Startup;

namespace Enterprise.Dat.Implementation
{
	class DeployDbUpgraderDirector : DatDbUpgraderDirector
	{
		public DeployDbUpgraderDirector(ITaskLogger taskLogger)
		{
			this.logger = taskLogger;
		}

		protected override void AppendToLogToFile(UpgradeEventType eventType, string text)
		{
			logger.RecordInfo(eventType + "\t" + text);
		}

		readonly ITaskLogger logger;
	}
}
#endif
