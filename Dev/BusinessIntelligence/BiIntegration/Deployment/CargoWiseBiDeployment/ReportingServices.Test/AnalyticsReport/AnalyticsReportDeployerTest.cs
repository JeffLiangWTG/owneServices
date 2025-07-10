using System.IO;
using System.Linq;
using CargoWise.Bi.Registration;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class AnalyticsReportDeployerTest : PowerBiDeployerTest<AnalyticsReportDeployer>
	{
		protected override AnalyticsReportDeployer TestDeployerInstance => (testDeployerInstance = testDeployerInstance ?? new AnalyticsReportDeployerForTest(TestLogger));
		AnalyticsReportDeployerForTest testDeployerInstance;

		public override void AssertReportsDeployed(PowerBiDeployer deployer)
		{
			CombineAssertions("Missing report in Reporting Service", () =>
			{
				var factory = new BusinessObjectFactory();
				var companies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true)).Where(c => c.HasActiveBranch).Select(c => c.GC_Code);

				var expectedReportList = new DeploymentFileLoader().GetPowerBiReports(BiReportCategory.All);

				var serverContent = deployer.ListChildCatalogItems(deployer.ClientFolderPath, true);
				foreach (var report in expectedReportList)
				{
					Assert(report.BusinessArea + "/" + report.Name, serverContent.Any(c => Path.GetFileNameWithoutExtension(c.Path) == report.Name && report.ReportType.Equals(BiReportType.NonPaginated) ? c.Type == "PowerBIReport" : c.Type == "Report"));
				}
			});
		}

		public override void TestPowerBiCleanRootFolder()
		{
			var deployer = TestDeployerInstance;

			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.SetPowerBiApiUrl(PowerBiWebPortalUrl + "/api/v2.0/");
					deployer.DeployReportFiles();

					deployer.CreateFolder("TestFolder", @"/EDI/DAT/Analytics");
					deployer.RefreshCatalogItems();
					AssertEquals("'/EDI/DAT/Analytics/TestFolder' exists", true, deployer.FolderExists("/EDI/DAT/Analytics/TestFolder"));
					deployer.CleanClientFolders();
					AssertEquals("'/EDI/DAT/Analytics/TestFolder' exists", false, deployer.FolderExists("/EDI/DAT/Analytics/TestFolder"));
				}
				finally
				{
					deployer.CleanClientFolders();
				}
			}
		}

		public override void TestPowerBiFolderStructure()
		{
			var deployer = TestDeployerInstance;

			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.SetPowerBiApiUrl(PowerBiWebPortalUrl + "/api/v2.0/");
					deployer.DeployReportFiles();
					var messages = TestLogger.LogEntries;

					AssertEquals(string.Join("\r\n", messages),
					string.Join("\r\n", @"The following reports have been deployed:",
					"Logistics/Agent Shipments Report",
					"Logistics/Carrier Shipments Report",
					"Logistics/Carrier Transit Reliability",
					"Logistics/Consolidation Late Report",
					"Logistics/Container Shipments Report",
					"Logistics/Export Broker Shipments Report",
					"Logistics/Import Broker Shipments Report",
					"Logistics/Job Declarations Report",
					"Logistics/Job Profit Dashboard",
					"Logistics/Job Profit Summary",
					"Logistics/Job Profit Details",
					"Logistics/LogisticsDataset",
					"Logistics/Main Consol Shipments Report",
					"Logistics/Revenue Job Profit Growth",
					"Logistics/Sailing Shipments Late Report",
					"Logistics/Sea Cargo Consignor Consignee Report",
					"Logistics/Shipment Dashboard",
					"Logistics/Shipment Dashboard New",
					"Logistics/Shipment Profile Report",
					"Logistics/Shipment Profile Report With Dashboard",
					"Logistics/Shipping Line Report",
					"Logistics/Placeholder RDL Report",
					"Telematics/Device Utilization Report",
					"Workflow/Team Throughput And Quality Measures",
					"Workflow/Glow Test Non Standard",
					"Warehouse/3PL Warehouse Dashboard",
					"Finance/Trial Balance Report",
					"Finance/Profit and Loss Report",
					"Finance/TrialBalanceOverviewDataset",
					"Finance/TrialBalanceTransactionsDataset",
					"Sales/Sales Opprotunity Report",
					"Sales/Sales Estimate Report",
					"Productivity/Falcon Report",
					"Telematics/Time In Operation Report",
					"Granting Report User Rights"));

					AssertEquals("'/EDI' exists", true, deployer.FolderExists("/EDI"));
					AssertEquals("'/EDI/DAT' exists", true, deployer.FolderExists("/EDI/DAT"));
					AssertEquals("'/EDI/DAT/Analytics' exists", true, deployer.FolderExists("/EDI/DAT/Analytics"));
				}
				finally
				{
					deployer.CleanClientFolders();
				}
			}
		}

		public override void TestIsDataSourceConfigured()
		{
			var deployer = TestDeployerInstance;

			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.SetPowerBiApiUrl(PowerBiWebPortalUrl + "/api/v2.0/");
					deployer.DeployReportFiles();
					deployer.CheckPowerBiServerHealthStatus();

					var reportUserCredentials = SystemDataRegistry.Instance.BiReportUserCredential.Value;
					var datasourObjs = JToken.Parse((deployer as AnalyticsReportDeployerForTest).JsonDataSourceContent);

					var authType = datasourObjs[0]["DataModelDataSource"]["AuthType"].ToString();
					var dataModelUsername = datasourObjs[0]["DataModelDataSource"]["Username"].ToString();
					var secret = datasourObjs[0]["DataModelDataSource"]["Secret"].ToString();

					var credentialsInServerUserName = datasourObjs[0]["CredentialsInServer"]["UserName"].ToString();
					var password = datasourObjs[0]["CredentialsInServer"]["Password"].ToString();
					var useAsWindowsCredentials = bool.Parse(datasourObjs[0]["CredentialsInServer"]["UseAsWindowsCredentials"].ToString());
					var impersonateAuthenticatedUser = bool.Parse(datasourObjs[0]["CredentialsInServer"]["ImpersonateAuthenticatedUser"].ToString());

					AssertEquals("AuthType should be \"Windows\"", "Windows", authType);
					AssertEquals($"Username should be \"{reportUserCredentials.Domain}\\{reportUserCredentials.UserName}\"", reportUserCredentials.Domain + "\\" + reportUserCredentials.UserName, dataModelUsername);
					AssertEquals($"Secret should be \"{reportUserCredentials.Password}\"", reportUserCredentials.Password, secret);
					AssertEquals("AuthType should be \"Windows\"", "Windows", authType);

					AssertEquals($"ConnectionString should have username of \"{reportUserCredentials.Domain}\\{reportUserCredentials.UserName}\"", reportUserCredentials.Domain + "\\" + reportUserCredentials.UserName, credentialsInServerUserName);
					AssertEquals($"ConnectionString should have password of \"{reportUserCredentials.Password}\"", reportUserCredentials.Password, password);
					AssertEquals("ConnectionString should have UseAsWindowsCredentials of \"true\"", true, useAsWindowsCredentials);
					AssertEquals("ConnectionString should have ImpersonateAuthenticatedUser of \"false\"", false, impersonateAuthenticatedUser);
				}
				finally
				{
					deployer.CleanClientFolders();
				}
			}
		}
	}
}
