using System.Collections.Generic;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	class BiInformationTest : TestCase
	{
		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public static void TestDisableEtlForAllTables()
		{
			using (var manager = new SsasModelManager())
			using (var connection = Db.NewAdminConnection())
			{
				Assert("Some tables should be enabled", manager.GetCountOfEnabledTables() > 0);

				manager.DisableEtlForAllTabularModelsQuery();
				manager.EnsureEnableEtlTrueOnStagingTableStateOnlyForEnabledTabularModels();

				AssertEquals("No tables should be enabled", manager.GetCountOfEnabledTables(), 0);
			}
		}

		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public static void TestDisableEtlOnModel_AllTablesDisabled()
		{
			using (var manager = new SsasModelManager())
			using (var connection = Db.NewAdminConnection())
			{
				manager.EnableEtlForAllTabularModels();
				manager.DisableEtlForAllStagingTableStatesQuery();

				CombineAssertions("All tables should be disabled:\r\n", () =>
				{
					Assert("WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});

				manager.DisableEtlOnModel(new List<ZString>() { "Warehouse Model" });

				CombineAssertions("The following staging tables have the wrong value for EnableEtl:\r\n", () =>
				{
					Assert("Just in Warehouse Model: WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});

				CombineAssertions("The following staging tables have the wrong value for EnableEtl:\r\n", () =>
				{
					Assert("Just in Warehouse Model: WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});
			}
		}

		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public static void TestDisableEtlOnModel_AllTablesEnabled()
		{
			using (var manager = new SsasModelManager())
			using (var connection = Db.NewAdminConnection())
			{
				manager.EnableEtlForAllTabularModels();
				manager.EnableEtlForAllStagingTableStatesQuery();

				CombineAssertions("All tables should be enabled:\r\n", () =>
				{
					Assert("WhsPick", manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});

				manager.DisableEtlOnModel(new List<ZString>() { "Warehouse Model" });

				CombineAssertions("The following staging tables have the wrong value for EnableEtl:\r\n", () =>
				{
					Assert("Just in Warehouse Model: WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});

				//manager.EnableEtlOnModel(new List<ZString>() { "Sales Model" });

				CombineAssertions("The following staging tables have the wrong value for EnableEtl:\r\n", () =>
				{
					Assert("Just in Warehouse Model: WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});
			}
		}

		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public static void TestEnableEtlOnModel()
		{
			using (var manager = new SsasModelManager())
			using (var connection = Db.NewAdminConnection())
			{
				manager.DisableEtlForAllTabularModelsQuery();
				manager.DisableEtlForAllStagingTableStatesQuery();

				manager.DisableEtlOnModel(new List<ZString>() { "Warehouse Model" });

				CombineAssertions("The following staging tables have the wrong value for EnableEtl:\r\n", () =>
				{
					Assert("Just in Warehouse Model: WhsPick", !manager.IsStagingTableEtlEnabled("dbo.WhsPick"));
				});
			}
		}

		public static void TestGetDependentTablesForTabularModels()
		{
			using (var manager = new SsasModelManager())
			{
				var dataSet = new BiAutomationConfigDataSet();
				var linkedTableConfig = GetTestLinkedTableConfig(dataSet);

				AssertDependentTables(manager, linkedTableConfig,
					models: new List<ZString>() { "WarehouseModel" },
					expectedDependentTables: new List<ZString>() {
						new ZString("dbo.Test1"),
						new ZString("dbo.Test2"),
						new ZString("dbo.Test3")
					});
			}
		}

		static void AssertDependentTables(SsasModelManager manager, BiAutomationConfigDataSet.LinkedTableDataTable linkedTableConfig, List<ZString> models, List<ZString> expectedDependentTables)
		{
			var dependentTables = manager.GetDependentTablesFor(models, linkedTableConfig);
			AssertContainsExactElementsInAnyOrder(expectedDependentTables, dependentTables);
		}

		static BiAutomationConfigDataSet.LinkedTableDataTable GetTestLinkedTableConfig(BiAutomationConfigDataSet dataSet)
		{
			var linkedTableConfig = dataSet.LinkedTable;

			var tabularModelRow1 = dataSet.TabularModel.AddTabularModelRow("WarehouseModel");
			linkedTableConfig.AddLinkedTableRow(tabularModelRow1, "dbo", "Test1");
			linkedTableConfig.AddLinkedTableRow(tabularModelRow1, "dbo", "Test2");
			linkedTableConfig.AddLinkedTableRow(tabularModelRow1, "dbo", "Test3");

			return linkedTableConfig;
		}

		public void TestDatabaseInfo()
		{
			var biInfo = new BiInformation();
			biInfo.RefreshInfo();

			AssertNotNull("Main database info", biInfo.MainDbInfo);
			AssertNotNull("Audit database info", biInfo.AuditDbInfo);
			AssertNotNull("EDW database info", biInfo.EdwDbInfo);
			//AssertNotNull("Analysis server info", biInfo.AnalysisServerInfo);
		}

		public void TestHasErrors()
		{
			var biInfo = new BiInformationForTest();
			biInfo.DataWarehouseServer_Override = "TestDataWarehouseServer";
			biInfo.AnalysisServer_Override = "TestAnalysisServer";
			biInfo.RefreshInfo();

			var errorMessage = biInfo.GetErrorMessage();

			CombineAssertions("Actual error log:\r\n" + errorMessage, () =>
			{
				Assert("Expected error message: The server was not found or was not accessible.", errorMessage.Contains("The server was not found or was not accessible."));
			});
		}

		class BiInformationForTest : BiInformation
		{
			public string DataWarehouseServer_Override { get; set; }
			public string AnalysisServer_Override { get; set; }

			public override string DataWarehouseServer
			{
				get
				{
					if (!string.IsNullOrEmpty(DataWarehouseServer_Override))
					{
						return DataWarehouseServer_Override;
					}
					else
					{
						return base.DataWarehouseServer;
					}
				}
			}

			public override string AnalysisServer
			{
				get
				{
					if (!string.IsNullOrEmpty(AnalysisServer_Override))
					{
						return AnalysisServer_Override;
					}
					else
					{
						return base.AnalysisServer;
					}
				}
			}
		}
	}
}
