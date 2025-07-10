using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IBP",
	"Import Builds Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.AutoDeploy.ImportBuildsServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)
]

namespace Enterprise.Client.EDI.AutoDeploy
{
	public class ImportBuildsServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ImportReleaseBuilds(token);
			CleanupOldReleaseBuilds(ServiceLogger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void ImportReleaseBuilds(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Processing builds to import ...");
			ServiceLogger.Information("Archive Path: " + EDIDataRegistry.Instance.MasterPackageArchivePath);

			DateTime lastCheckTime = EDIDataRegistry.Instance.LastIBPPackageCheck;
			DateTime newCheckTime = DateTime.UtcNow;
			bool shouldUpdateLastCheckTime = true;
			foreach (var package in new DirectoryInfo(EDIDataRegistry.Instance.MasterPackageArchivePath).GetFiles().Where(f => f.CreationTimeUtc > lastCheckTime).Select(f => f.FullName))
			{
				token.ThrowIfCancellationRequested();
				ServiceLogger.Information("Processing " + package);
				try
				{
					Factory.SetContext(EDIConstants.BusinessContext.ImportBuildsServiceTask);
					using (var importer = new PackageImporter(Factory, package))
					{
						if (importer.IsAlreadyImported())
						{
							ServiceLogger.Information("Version already imported " + package);
						}
						else
						{
							ServiceLogger.Information("Importing " + package);
							importer.Import();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					shouldUpdateLastCheckTime = false;
					ServiceLogger.Error("Failed to import " + package + " " + ex);
					var exType = ex.GetType();
					if (exType != typeof(InvalidDataException))
					{
						ErrorReporter.ReportOnce("Failed to import", package + ' ' + ex.Message, ex);
					}
				}
			}
			if (shouldUpdateLastCheckTime || newCheckTime - lastCheckTime > TimeSpan.FromDays(7))
			{
				EDIDataRegistry.Instance.LastIBPPackageCheck = newCheckTime;
			}

			ServiceLogger.Log(LogType.Information, "Finished to process builds to import ...");
		}

		public static void CleanupOldReleaseBuilds(ILogger logger)
		{
			logger.Log(LogType.Information, "Cleaning up old release builds...");

			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(ReleaseBuild));

			query.AddToFilter(ReleaseBuildSchema.HL_IsActive, true);
			query.AddToFilter(ReleaseBuildSchema.HL_ExeVersionDate, SQLComparisonOperator.LessThan, ZDateTime.Now.AddDays(-7));
			query.AddToFilter(ReleaseBuildSchema.HL_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise });
			var filterSQL = $@"({ReleaseBuildSchema.Constants.HL_ReleaseStatus} = 'ALP' 
					or exists (select 1 from {ReleaseBuildSchema.Constants.TableName} latestOnBranch
								where latestOnBranch.{ReleaseBuildSchema.Constants.HL_MajorVersion} = {ReleaseBuildSchema.Constants.TableName}.{ReleaseBuildSchema.Constants.HL_MajorVersion}
								and latestOnBranch.{ReleaseBuildSchema.Constants.HL_MinorVersion} = {ReleaseBuildSchema.Constants.TableName}.{ReleaseBuildSchema.Constants.HL_MinorVersion}
								and latestOnBranch.{ReleaseBuildSchema.Constants.HL_Release} = {ReleaseBuildSchema.Constants.TableName}.{ReleaseBuildSchema.Constants.HL_Release}
								and latestOnBranch.{ReleaseBuildSchema.Constants.HL_Patch} > {ReleaseBuildSchema.Constants.TableName}.{ReleaseBuildSchema.Constants.HL_Patch}))
					and not exists (select 1 from {LicenceDatabaseSchema.Constants.TableName} 
								where {LicenceDatabaseSchema.Constants.LD_HL_CurrentRunningVersion} = {ReleaseBuildSchema.Constants.PK}
								and {LicenceDatabaseSchema.Constants.LD_LastHeartbeat} > dateadd(day, -30, getdate()))";
			query.AddFilterAndZSQLParameterCollection(filterSQL, null);

			var releaseBuilds = factory.Load<ReleaseBuild>(query);

			var filesToDelete = releaseBuilds.Where(rb => !string.IsNullOrEmpty(rb.HL_PackagePath)).Select(rb => rb.HL_PackagePath).ToList();
			logger.Log(LogType.Information, filesToDelete.Count + " files to delete");
			foreach (var file in filesToDelete)
			{
				logger.Log(LogType.Information, "Deleting " + file);
				if (File.Exists(file))
				{
					File.Delete(file);
				}
			}

			foreach (var item in releaseBuilds)
			{
				item.HL_IsActive = false;
			}

			factory.Save();

			logger.Log(LogType.Information, "Finished cleaning up old release builds");
		}

		public const string ProcessName = "Import Builds";

		#region Implementation

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
