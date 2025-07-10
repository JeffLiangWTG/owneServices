using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;
using static CargoWise.Bi.Product.DataLoad.Testing.EdwEtlExecutionTest;

namespace CargoWise.Bi.Product.Manager.Business
{
	class CdcInformationTest : TransactionedTestCase
	{
		public void TestCdcInformationValues()
		{
			var cdcInfo = new CdcInformation();
			cdcInfo.RefreshInfo();

			AssertEquals("ServerName", Db.ServerName, cdcInfo.ServerName);
			AssertEquals("Database Name", Db.DatabaseName, cdcInfo.DatabaseName);

			if (TestingState.IsRunningOnDAT)
			{
				AssertEquals("Is CDC Enabled", false, cdcInfo.IsCdcEnabled);
				AssertEquals("CDC Enabled Tables", 0, cdcInfo.NumberOfCdcEnabledTables);
				Assert("CDC Schema Errors", string.IsNullOrEmpty(cdcInfo.CdcSchemaErrors));
			}
			else
			{
				AssertEquals("Is CDC Enabled", true, cdcInfo.IsCdcEnabled);
				AssertEquals("CDC Enabled Tables", BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Count(t => t.GetCdcColumnConfigRows().Any(c => c.CdcEnabled)), cdcInfo.NumberOfCdcEnabledTables);
				AssertEquals("CDC Schema Errors", "Querying...", cdcInfo.CdcSchemaErrors);
			}
		}

		[UseSnapshotProtection]
		public void TestCdcErrorsBinding()
		{
			var cdcInfo = new CdcInformation();
			cdcInfo.RefreshInfo();

			using (var testConnection = Db.NewAdminConnection())
			{
				var scanner = new CdcScannerForTest();
				SetupCdcError(testConnection);

				try
				{
					var processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
					Fail("Expected CdcException to be thrown.");
				}
				catch (CdcException ex)
				{
					CombineAssertions($"Actual message: {ex.Message}", () =>
					{
						Assert("Expected message: (22863) Failed to insert rows into Change Data Capture change tables. Refer to previous errors in the current session to identify the cause and correct any associated problems.",
							ex.Message.Contains("(22863) Failed to insert rows into Change Data Capture change tables. Refer to previous errors in the current session to identify the cause and correct any associated problems."));
						Assert("Expected message: (8134) Divide by zero error encountered.",
							ex.Message.Contains("(8134) Divide by zero error encountered."));
					});

					cdcInfo.RefreshInfo();
					AssertEquals("Should have 2 errors", 2, cdcInfo.CdcErrors.Count);

					var errorNumbers = cdcInfo.CdcErrors.Select(error => error.error_number).ToList();
					var errorMessages = cdcInfo.CdcErrors.Select(error => error.error_message).ToList();

					CombineAssertions(() =>
					{
						Assert("Error number should contain 22863", errorNumbers.Contains(22863));
						Assert("Error number should contain 8134", errorNumbers.Contains(8134));

						Assert("Error messages should contain 'insert rows' error", errorMessages.Contains("Failed to insert rows into Change Data Capture change tables. Refer to previous errors in the current session to identify the cause and correct any associated problems."));
						Assert("Error messages should contain 'divide by zero' error", errorMessages.Contains("Divide by zero error encountered."));
					});
				}
			}
		}

		public void TestPopulateCdcSchemaErrors()
		{
			AssertNoExceptionThrown(() => { new CdcInformation().PopulateCdcSchemaErrors(); });
		}

		void SetupCdcError(AdminConnection testConnection)
		{
			CdcDatabase.Enable(testConnection, Db.DatabaseName);

			string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB char(1));
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = @"
					CREATE TRIGGER [cdc].[TR_II_dbo_TestTable_CT] ON [cdc].[dbo_CdcScannerTest$Table_CT]
					INSTEAD OF INSERT AS
					BEGIN
						DECLARE @error int = 1/0
					END";
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = "INSERT dbo.CdcScannerTest$Table VALUES (1,'X'), (2,'Y');";
			testConnection.ExecuteNonQuery(sqlText);
		}
	}
}
