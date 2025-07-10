#if DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.Application;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.Startup
{
	public class DatDbUpgraderDirector : DbUpgraderDirector
	{
		public DatDbUpgraderDirector()
		{
			upgradeLog = new StringBuilder();
		}

		protected override ValidationResponse DoUpgrade()
		{
			return ObjectFactory.Get<IDbUpgraderRunner>().FullSilentUpgrade(UpgradeVersionInfo, softwareUpgrade, AppendToLogToFile);
		}

		protected override bool LoginForUpgrade()
		{
			return true;
		}

		protected internal override void ShowDenyUpgradeMessage(string messageSufix, string denyReason, string denyReasonFullExplanation)
		{
			AppendToLogToFile(UpgradeEventType.TaskFailed, (denyReasonFullExplanation ?? denyReason) + " - " + messageSufix);
		}

		protected override void ShowSoftwareUpgradeSuccessMessage(Version version, string information)
		{
		}

		protected override void AppendToLogToFile(UpgradeEventType eventType, string text)
		{
			if (eventType == UpgradeEventType.TaskFailed)
			{
				upgradeLog.AppendLine();
			}
			upgradeLog.Append(LogTimestamp);
			upgradeLog.Append('\t');
			if (eventType == UpgradeEventType.SubtaskStarted)
			{
				upgradeLog.Append('\t');
			}
			upgradeLog.Append(text);
			upgradeLog.Append(System.Environment.NewLine);
			if (eventType == UpgradeEventType.TaskFailed)
			{
				upgradeLog.AppendLine();
			}
		}

		public static string UpgradeLog
		{
			get { return upgradeLog.ToString(); }
		}

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Exposing log to DAT test startup")]
		static StringBuilder upgradeLog;
	}
}
#endif
