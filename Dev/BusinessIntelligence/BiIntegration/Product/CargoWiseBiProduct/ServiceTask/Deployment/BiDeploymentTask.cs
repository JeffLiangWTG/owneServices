using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.BiDeploymentTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.BiDeploymentTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.BiDeploymentTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1week",
	DefaultScheduleRunEvery = "24hours",
	ActiveByDefault = true)
]

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace CargoWise.Bi.Product.ServiceTask
{
	#region SuppressResourceStringsCheckRegion

	public class BiDeploymentTask : ServiceProviderImpl, IBiNotificationSource
	{
		public const string ServiceTaskCode = "BID";
		public const string ServiceTaskName = "Business Intelligence Deployment";

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string CheckDataWarehouseServerIsConfigured() => HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(SystemDataRegistry.Instance.BiDataWarehouseServer);

		[HostedServiceRequirement]
		public static string CheckAnalysisServerIsConfigured() => HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(SystemDataRegistry.Instance.BiAnalysisServer);

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		[HostedServiceRequirement]
		public static string CheckBiReportIsEnabled() => SystemDataRegistry.Instance.EnableBiReport.Value ? string.Empty : $"{SystemDataRegistry.Instance.EnableBiReport.Name} is false";

		public override void RunTask(CancellationToken token)
		{
			string unsatisfiedRequirements;
			using (var analysisServicesUpgrader = new SsasProjectUpgrader(ServiceLogger))
			{
				if (CheckRequirements(analysisServicesUpgrader, out unsatisfiedRequirements))
				{
					DeploySsasCubesIfRequired(analysisServicesUpgrader);
					if (!token.IsCancellationRequested)
					{
						ProcessSsasCubes(analysisServicesUpgrader);
					}

					if (!token.IsCancellationRequested)
					{
						DeployAnalyticsReportsIfRequired();
					}

					if (token.IsCancellationRequested)
					{
						ServiceLogger.Log(LogType.Warning, "The BI deployment task has been cancelled.");
					}
				}
				else
				{
					ServiceLogger.Log(LogType.Debug, "The following BI deployment requirements were not satisfied :" + unsatisfiedRequirements);
				}
			}
		}

		#region Requirements Checking

		bool EdwDatabaseExistsAndUpdated()
		{
			var result = false;

			if (!string.IsNullOrEmpty(DataWarehouseServer))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(dataWarehouseServer, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(Db.EdwDatabaseName))
					{
						var edwVersion = BiMasterState.GetBiDatabaseExtPty(biConnection, Db.EdwDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName);
						result = SchemaVersion.Application.ToString() == edwVersion;
					}
				}
			}

			return result;
		}

		string DataWarehouseServer
		{
			get
			{
				return dataWarehouseServer ??
					(dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dataWarehouseServer;

		bool CheckRequirements(SsasProjectUpgrader analysisServicesUpgrader, out string unsatisfiedRequirements)
		{
			unsatisfiedRequirements = "";

			if (!EdwDatabaseExistsAndUpdated())
			{
				unsatisfiedRequirements += string.Format(CultureInfo.InvariantCulture, "\r\n[{0}] database doesn't exist in server [{1}] or isn't up to date.", Db.EdwDatabaseName, DataWarehouseServer);
			}

			string ssasRequirementFailure;

			if (!analysisServicesUpgrader.CheckRequirements(out ssasRequirementFailure))
			{
				unsatisfiedRequirements += "\r\n" + ssasRequirementFailure;
			}

			return string.IsNullOrWhiteSpace(unsatisfiedRequirements);
		}

		#endregion

		#region Deployment

		#region SSAS

		void DeploySsasCubesIfRequired(SsasProjectUpgrader analysisServicesUpgrader)
		{
			if (!analysisServicesUpgrader.AreAllCubesDeployedAndUpdated(checkReaderRole: true))
			{
				analysisServicesUpgrader.Upgrade();
				ssasUpgraded = true;
			}
		}

		bool ssasUpgraded;

		void ProcessSsasCubes(SsasProjectUpgrader analysisServicesUpgrader)
		{
			if (analysisServicesUpgrader.AreAllCubesDeployedAndUpdated(checkReaderRole: false))
			{
				analysisServicesUpgrader.PartitionSsasModels();
				analysisServicesUpgrader.ProcessSsasModels();
			}
		}

		#endregion

		#region Power BI

		void DeployAnalyticsReportsIfRequired()
		{
			try
			{
				DeployAnalyticsReportsIfRequiredUnsafe();
			}
			catch (HttpRequestException ex)
			{
				if (IsWTGInternalSystem)
				{
					ServiceLogger.Log(LogType.Warning, "There was a problem deploying analytics reports.", ex);
				}
				else
				{
					throw;
				}
			}
			catch (WebException ex)
			{
				if ((((HttpWebResponse)ex.Response).StatusDescription == "Unprocessable Entity"))
				{
					if (IsWTGInternalSystem)
					{
						ServiceLogger.Log(LogType.Warning, "Power BI Server version is old. Upgrade the server or update BI servers registry to use a newer version.", ex);
					}
					else
					{
						ServiceLogger.Log(LogType.Error, "Power BI Server version is old. Upgrade the server or update BI servers registry to use a newer version.", ex);
						throw;
					}
				}
				else
				{
					if (IsWTGInternalSystem)
					{
						ServiceLogger.Log(LogType.Warning, "There was a problem deploying analytics reports.", ex);
					}
					else
					{
						throw;
					}
				}
			}
			catch (PowerBiException ex)
			{
				if (IsWTGInternalSystem)
				{
					ServiceLogger.Log(LogType.Warning, "There was a problem deploying analytics reports.", ex);
				}
				else
				{
					throw;
				}
			}
		}

		bool IsWTGInternalSystem
		{
			get
			{
				if (isWtgInternal == null)
				{
					try
					{
						var registration = ObjectFactory.Get<IProductRegistration>();
						isWtgInternal = registration.IsWiseTechGlobalInternalSystem();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						isWtgInternal = false;
					}
				}
				return isWtgInternal.Value;
			}
		}
		bool? isWtgInternal;

		void DeployAnalyticsReportsIfRequiredUnsafe()
		{
			var reportUpgrader = new AnalyticsReportUpgrader(ServiceLogger);
			var deployer = new AnalyticsReportDeployer(ServiceLogger);
			if (CheckPowerBiServerRequirements(reportUpgrader, deployer) &&
				(ssasUpgraded || reportUpgrader.UpgradeRequired || !CheckPowerBiServerHealthStatus(reportUpgrader, deployer)))
			{
				reportUpgrader.Upgrade(deployer);
			}
		}

		bool CheckPowerBiServerRequirements(AnalyticsReportUpgrader reportUpgrader, AnalyticsReportDeployer deployer)
		{
			ServiceLogger.Log(LogType.Debug, "Checking Power BI Server requirements");
			return reportUpgrader.CheckRequirements(deployer);
		}

		public bool CheckPowerBiServerHealthStatus(AnalyticsReportUpgrader reportUpgrader, AnalyticsReportDeployer deployer)
		{
			var isPowerBiHealthy = true;
			try
			{
				isPowerBiHealthy = reportUpgrader.CheckPowerBiServerHealthStatus(deployer);
			}
			catch (WebException ex) when (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
			{
				var message = string.Format(CultureInfo.InvariantCulture, "The user has insufficient permission to perform this operation.");
				ServiceLogger.Log(LogType.Error, message, ex);
			}

			return isPowerBiHealthy;
		}

		#endregion

		#endregion

		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion
	}
}

#endregion
