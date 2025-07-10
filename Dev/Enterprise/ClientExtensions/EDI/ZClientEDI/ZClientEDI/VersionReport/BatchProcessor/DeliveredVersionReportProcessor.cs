
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	/// <summary>
	/// Processes a message indicating an upgrade has been imported into a system.
	/// Sent by StmUpgradeImporter.
	/// </summary>
	class DeliveredVersionReportProcessor : VersionReportProcessor
	{
		internal string sender;

		public DeliveredVersionReportProcessor(string sender, ILogger logger)
			: base(logger)
		{
			this.sender = sender;
		}

		public DeliveredVersionReportProcessor()
			: this(null, null)
		{
		}

		protected override void ProcessVersionReport(EDIVersionReport report)
		{
			LicenceDatabase database = GetLicenceDatabase(report);
			if (database != null)
			{
				ReleaseBuild sentVersion = database.SentVersion;
				if ((sentVersion == null) || (sentVersion.ExeVersion != report.CurrentVersion))
				{
					ReleaseBuild newBuild = GetReleaseBuild(report.CurrentVersion);
					database.LD_HL_CurrentSentVersion = (newBuild != null) ? newBuild.PK : ZGuid.Empty;
					MarkScheduledUpgradesAsReceived(database,  newBuild);
					Factory.Save();

					if (newBuild != null)
					{
						Log(LogType.Information, @"LicenceDatabase [{0}/{1}/{2}] received update v{3}",
							report.EnterpriseCode, report.CompanyCode, report.PhysicalServerID,
							report.CurrentVersion);
					}
					else
					{
						Log(LogType.Warning, @"LicenceDatabase [{0}/{1}/{2}] received unknown build v{3}",
							report.EnterpriseCode, report.CompanyCode, report.PhysicalServerID,
							report.CurrentVersion);
					}
				}
			}
			else
			{
				Log(LogType.Warning, @"LicenceDatabase [{0}/{1}/{2}] from sender {3} is unknown",
					report.EnterpriseCode, report.CompanyCode, report.PhysicalServerID,
					sender);
			}
		}

		protected override string GetReportId()
		{
			return "DeliveredVersionReport";
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			if (ServiceLogger != null)
			{
				ServiceLogger.Log(logType, string.Format(format, args));
			}
		}

		protected void MarkScheduledUpgradesAsReceived(LicenceDatabase database, ReleaseBuild build)
		{
			if (database != null && build != null)
			{
				UpgradesToClientCollection upgrades = GetScheduledUpgrades(database, build);
				foreach (UpgradesToClient upgrade in upgrades)
				{
					upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Received;
				}
			}
		}

		UpgradesToClientCollection GetScheduledUpgrades(LicenceDatabase database, ReleaseBuild build)
		{
			UpgradesToClientCollection result = new UpgradesToClientCollection(Factory);

			ZQuery filter = new ZQuery(UpgradesToClientSchema.L1_LD, SQLComparisonOperator.Equal, database.PK);
			filter.AddToFilter(UpgradesToClientSchema.L1_HL, SQLComparisonOperator.Equal, build.PK);
			filter.AddToFilter(UpgradesToClientSchema.L1_CurrentStatus, SQLComparisonOperator.Equal, UpgradesToClientStatus.Codes.Processed);

			result.Load(filter);
			return result;
		}
	}
}
