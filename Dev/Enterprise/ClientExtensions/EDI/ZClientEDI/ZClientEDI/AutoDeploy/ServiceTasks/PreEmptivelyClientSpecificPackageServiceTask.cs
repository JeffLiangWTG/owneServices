using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	"PLU",
	"Pre-emptively package and request client-specific upgrades",
	"CSP",
	typeof(Enterprise.Client.EDI.AutoDeploy.PreEmptivelyClientSpecificPackageServiceTask),
	ActiveByDefault = true,
	CanRunInAnyBranch = true,
	IsScheduleReadOnly = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "30Minutes"
	)
]

namespace Enterprise.Client.EDI.AutoDeploy
{
	public class PreEmptivelyClientSpecificPackageServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var watch = Stopwatch.StartNew();
				var allDatabases = GetDatabases();
				var clientSpecificDbLatestBuilds = FindLatestBuildForDatabases(allDatabases, youMustReactToThisToken);
				watch.Stop();

				if (clientSpecificDbLatestBuilds.Count > 0)
				{
					var selectedDBCount = clientSpecificDbLatestBuilds.SelectMany(x => x.Value).Count();
					ServiceLogger.Information($"{allDatabases.Count()} databases were read, and newer available versions existed for {selectedDBCount} databases({watch.Elapsed.TotalMilliseconds} ms)");
				}
				else
				{
					return;
				}

				var upgradeRequestQuery = new ZQuery(UpgradesToClientSchema.L1_LD, clientSpecificDbLatestBuilds.SelectMany(x => x.Value.Select(y => y.PK)))
					.AddToFilter(UpgradesToClientSchema.L1_HL, clientSpecificDbLatestBuilds.Select(x => x.Key.PK));
				var existingRequests = Factory.Load<UpgradesToClient>(upgradeRequestQuery);

				foreach (var item in clientSpecificDbLatestBuilds)
				{
					if (youMustReactToThisToken.IsCancellationRequested)
					{
						ServiceLogger.Warning("Task was cancelled during posting request");
						youMustReactToThisToken.ThrowIfCancellationRequested();
					}

					var releaseBuild = item.Key;
					var databases = item.Value;
					var upgradeFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var upgradeRequests = new UpgradeRequestCollection(upgradeFactory);

					foreach (var database in databases)
					{
						if (existingRequests.Any(x => x.L1_LD == database.PK && x.L1_HL == releaseBuild.PK))
						{
							continue;
						}

						upgradeRequests.Add(new UpgradeRequest(upgradeFactory, database.LicEnterprise?.Organisation, database) { NotifyUser = false });
					}

					try
					{
						var result = PostUpgrades(releaseBuild, upgradeRequests);
						if (result != null)
						{
							if (!string.IsNullOrEmpty(result.Message))
							{
								ServiceLogger.Information(result.Message);
							}

							if (!result.IsError)
							{
								var databaseListDescription = string.Join(";", upgradeRequests.Select(x => x.LicDatabase.LD_DatabaseNumber));
								ServiceLogger.Information($"{releaseBuild.VersionNumberDisplayText} will be deployed to {result.CreatedUpgradesToClientsCount} databases ({databaseListDescription}).");
							}
							else
							{
								ServiceLogger.Warning($"Cannot place upgrade: {releaseBuild.VersionNumberDisplayText}");
							}
						}
					}
					catch (Exception ex)
					{
						var databasesNumbers = string.Join(",", databases.Select(x => x.LD_DatabaseNumber.ToString()));
						ErrorReporter.ReportOnce("[Pre-deploy]Posting Upgrade Failed", $"ReleaseVersion:{releaseBuild.VersionNumber}; DatabaseNumbers:{databasesNumbers}", ex);
					}
				}
			}
		}

		protected IEnumerable<LicenceDatabase> GetDatabases()
		{
			var configStringQuery = new ZQuery(LicenceDatabaseSchema.LD_ScheduleStateUPG, SQLComparisonOperator.Contains, "<CONFIGSTRING>Y")
				.AddToFilter(JoinCondition.Or, LicenceDatabaseSchema.LD_ScheduleStateUPG, SQLComparisonOperator.StartsWith, "Y,");

			var query = new ZQuery(LicenceDatabaseSchema.LD_IsActive, true)
				.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production)
				.AddToFilter(LicenceDatabaseSchema.LD_Product, ProductTypes.CargoWiseNextAgreementCompatibleProducts)
				.AddToFilter(LicenceDatabaseSchema.LD_AvailableUpgradeMethod, UpgradeMethods.Codes.Http)
				.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, SQLComparisonOperator.NotEqual, LicenceConstants.NotHostedWithCargoWise)
				.AddToFilter(configStringQuery)
				.AddToFilter(LicenceDatabaseSchema.LD_NextRunTimeUtcUPG, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddHours(2))
				.AddToFilter(LicenceDatabaseSchema.LD_NextRunTimeUtcUPG, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddHours(2).AddMinutes(30));

			return Factory.Load<LicenceDatabase>(query);
		}

		protected IDictionary<ReleaseBuild, IList<LicenceDatabase>> FindLatestBuildForDatabases(IEnumerable<LicenceDatabase> databases, CancellationToken cancellationToken)
		{
			var result = new Dictionary<ReleaseBuild, IList<LicenceDatabase>>();

			foreach (var database in databases)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					ServiceLogger.Warning("Task was cancelled during loading packages");
					cancellationToken.ThrowIfCancellationRequested();
				}

				var latestBuild = BuildsDictionary.GetLatestAvailableBuild(database.LD_Product, database.LD_ReleaseRing, takeWeeklyBuildsInsteadOfLatest: true);
				if (latestBuild != null && database.CurrentVersion != null && latestBuild.VersionNumber.CompareTo(database.CurrentVersion.VersionNumber) > 0 && latestBuild.ClientSpecificCodes.Contains(database.EnterpriseCode))
				{
					if (result.ContainsKey(latestBuild))
					{
						result[latestBuild].Add(database);
					}
					else
					{
						result[latestBuild] = new List<LicenceDatabase>() { database };
					}
				}
			}

			return result;
		}

		IPostUpgradeStatus PostUpgrades(ReleaseBuild build, UpgradeRequestCollection upgrades)
		{
			if (upgrades.Count > 0)
			{
				var upgrader = new UpgradeRequestCollectionContainer(upgrades.Factory, upgrades)
				{
					IsSendViaDefault = true,
					SendEmailNotificationAutomatically = false,
					ReleaseBuildPK = build.PK,
				};

				// upgradeFactory is saved here
				return upgrader.PlaceUpgradesToClients();
			}

			return null;
		}

		#region Implementation

		public LatestReleaseBuildsDictionary BuildsDictionary
		{
			get
			{
				if (buildsDictionary == null)
				{
					buildsDictionary = new LatestReleaseBuildsDictionary(Factory);
					BuildsDictionary.Load();
				}

				return buildsDictionary;
			}
		}
		LatestReleaseBuildsDictionary buildsDictionary;

		protected BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;

		#endregion
	}
}
