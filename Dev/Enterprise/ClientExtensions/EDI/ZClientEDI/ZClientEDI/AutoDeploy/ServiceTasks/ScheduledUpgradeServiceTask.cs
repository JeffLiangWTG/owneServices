using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SUP",
	"Scheduled Upgrade Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.AutoDeploy.ScheduledUpgradeServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)
]

namespace Enterprise.Client.EDI.AutoDeploy
{
	public class ScheduledUpgradeServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				UpgradesToClientCollection collection = new UpgradesToClientCollection(Factory);
				ZDateTime executionLimit = ZDateTime.Now.AddSeconds(MaxExecutingDuration);
				fTotalNumberOfRowsLoaded = 0;

				ServiceLogger.Log(LogType.Information, "Processing scheduled upgrades ...");
				int totalUpgradesProcessed = 0;
				int totalUpgradesFailed = 0;

				do
				{
					token.ThrowIfCancellationRequested();
					int processedUpgrades = 0;
					int failedUpgrades = 0;

					collection.Load(UpgradesToClientFilter);

					if (collection.Count > 0)
					{
						fTotalNumberOfRowsLoaded += collection.Count;

						collection.Sort(null as IComparer);
						List<UpgradesToClient> similarUpgrades = new List<UpgradesToClient>();

						foreach (UpgradesToClient upgrade in collection)
						{
							token.ThrowIfCancellationRequested();
							upgrade.L1_ActualDateTime = ZDateTime.Now;

							if (upgrade.LicDatabase.HasMultipleEnterprises)
							{
								upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Failed;
								++failedUpgrades;
								ServiceLogger.Log(LogType.Warning,
									String.Format(CultureInfo.InvariantCulture, "Processing upgrade for the client {0}, server {1} failed because the database has multiple enterprises.",
									upgrade.Organisation.OH_Code,
									upgrade.LicDatabase.LD_ServerCode));
							}
							else
							{
								if ((similarUpgrades.Count > 0 && !upgrade.CouldBeGroupedWith(similarUpgrades[0])))
								{
									int processed = SendUpgradesGroup(similarUpgrades.ToArray());
									processedUpgrades += processed;
									failedUpgrades += (similarUpgrades.Count - processed);

									similarUpgrades.Clear();
								}

								similarUpgrades.Add(upgrade);
							}
						}

						if (similarUpgrades.Count > 0)
						{
							int processed = SendUpgradesGroup(similarUpgrades.ToArray());
							processedUpgrades += processed;
							failedUpgrades += (similarUpgrades.Count - processed);

							similarUpgrades.Clear();
						}

						Factory.Save();

						ServiceLogger.Log(LogType.Information, String.Format(CultureInfo.InvariantCulture, "Processed {0}, failed {1}.", processedUpgrades, failedUpgrades));
						totalUpgradesProcessed += processedUpgrades;
						totalUpgradesFailed += failedUpgrades;
					}
				} while (collection.Count > 0 && ZDateTime.Now < executionLimit);

				ServiceLogger.Log(LogType.Information, String.Format(CultureInfo.InvariantCulture, "Total processed {0}, failed {1}.", totalUpgradesProcessed, totalUpgradesFailed));
			}
		}

		public int MaxExecutingDuration
		{
			get { return Enterprise.Client.EDI.EDIDataRegistry.Instance.AutoDeployProcessBatchMaxDuration.Value; }
		}

		public const string ProcessName = "Upgrade Schedule";

		#region Implementation

		int SendUpgradesGroup(UpgradesToClient[] upgradeRequests)
		{
			int result = 0;
			try
			{
				// It's possible there could be a mix of WiseTech-hosted and self-hosted systems in the upgrade
				// for a single client. WiseTech-hosted upgrade packages must include Winzor binaries, even
				// if the customer is not licensed for Winzor, because another tenant in WiseCloud sharing the same version
				// may need Winzor.

				foreach (var packageGroup in upgradeRequests.GroupBy(u => u.LicDatabase.IsHostedOnWiseCloud))
				{
					// firstUpgradeRequest will be symptomatic of all upgrades in the group - same requirements (client specific etc)
					var firstUpgradeRequest = packageGroup.First();
					string packagePath = BuildResultPackage(firstUpgradeRequest.Build, firstUpgradeRequest.ClientSpecificCode, GetTargetDirectory(firstUpgradeRequest), packageGroup.Key);

					var sender = GetSender(packagePath, upgradeRequests);

					if (sender != null)
					{
						if (sender.Run())
						{
							foreach (UpgradesToClient upgrade in upgradeRequests)
							{
								ServiceLogger.Log(LogType.Information, String.Format("Processing upgrade for the client {0}, server {1} was sucessfully processed using {2} upgrade method.", upgrade.Organisation.OH_Code, upgrade.LicDatabase.LD_ServerCode, new UpgradeMethods().GetDescriptionFromCode(upgrade.L1_ActualUpgradeMethod)));
								upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Processed;
								result++;
							}
						}
						else
						{
							foreach (var upgrade in upgradeRequests)
							{
								ServiceLogger.Log(LogType.Error, String.Format("Processing upgrade for the client {0}, server {1} failed using {2} upgrade method.", upgrade.Organisation.OH_Code, upgrade.LicDatabase.LD_ServerCode, new UpgradeMethods().GetDescriptionFromCode(upgrade.L1_ActualUpgradeMethod)));
								upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Failed;
							}
						}
					}
					else
					{
						foreach (UpgradesToClient upgrade in upgradeRequests)
						{
							ServiceLogger.Log(LogType.Information, String.Format("Processing upgrade for the client {0}, server {1} failed because it was unable to get an upgrade sender.", upgrade.Organisation.OH_Code, upgrade.LicDatabase.LD_ServerCode, new UpgradeMethods().GetDescriptionFromCode(upgrade.L1_ActualUpgradeMethod)));
							upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Failed;
						}
					}

					try
					{
						if (File.Exists(packagePath))
						{
							File.Delete(packagePath);
						}
					}
					catch (IOException ex)
					{
						ServiceLogger.Log(LogType.Error,
							String.Format("Cannot delete temporary package file {0}. {1}", packagePath, ex.Message));
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				foreach (UpgradesToClient upgrade in upgradeRequests)
				{
					ServiceLogger.Log(LogType.Error, String.Format("Processing upgrade for the client {0}, server {1} failed with exception {2}.",
						upgrade.Organisation.OH_Code, upgrade.LicDatabase.LD_ServerCode, new UpgradeMethods().GetDescriptionFromCode(upgrade.L1_ActualUpgradeMethod)));
					upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Failed;
				}
				ServiceLogger.Log(LogType.Error, String.Format("Exception Details : {0}", e));
				ErrorReporter.ReportOnce("Processing Upgrade Exception", e.Message, e);
			}
			return result;
		}

		ZQuery UpgradesToClientFilter
		{
			get
			{
				ZQuery filter = new ZQuery(UpgradesToClientSchema.L1_CurrentStatus, SQLComparisonOperator.Equal, UpgradesToClientStatus.Codes.Queued);
				filter.AddToFilter(UpgradesToClientSchema.L1_RequestedDateTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
				filter.MaximumRows = 20;

				return filter;
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		string PackageRootDirectory
		{
			get { return Env.TempPath; }
		}

		string GetTargetDirectory(UpgradesToClient upgradeToClient)
		{
			return Path.Combine(PackageRootDirectory, upgradeToClient.EnterpriseCode);
		}

		protected virtual HttpPackageSender GetSender(string packagePath, params UpgradesToClient[] upgradeRequests)
		{
			if (upgradeRequests.Length == 0)
			{
				return null;
			}

			HttpPackageSender sender;

			switch (upgradeRequests[0].L1_ActualUpgradeMethod)
			{
				case UpgradeMethods.Codes.Http:
					{
						sender = new HttpPackageSender(upgradeRequests[0].ClientSpecificCode, packagePath, ServiceLogger, upgradeRequests);
						break;
					}
				default:
					{
						sender = null;
						break;
					}
			}

			return sender;
		}

		protected virtual RuntimePackageBuilder GetPackageBuilder(ReleaseBuilds.Business.ReleaseBuild build, string targetDirectory)
		{
			return new RuntimePackageBuilder(build, targetDirectory);
		}

		string BuildResultPackage(ReleaseBuilds.Business.ReleaseBuild build, string clientSpecificCode, string targetDirectory, bool isHostedOnWiseCloud)
		{
			RuntimePackageBuilder builder = GetPackageBuilder(build, targetDirectory);
			builder.Build(clientSpecificCode, isHostedOnWiseCloud);

			return GetResultPackagePath(builder);
		}

		protected virtual string GetResultPackagePath(RuntimePackageBuilder builder)
		{
			return builder.LastPackagePath;
		}

		public int TotalNumberOfRowsLoaded
		{
			get { return fTotalNumberOfRowsLoaded; }
		}

		int fTotalNumberOfRowsLoaded;

		#endregion
	}
}
