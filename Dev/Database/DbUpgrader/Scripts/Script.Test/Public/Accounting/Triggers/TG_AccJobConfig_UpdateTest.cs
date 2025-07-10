using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccJobConfig_Update))]
	class TG_AccJobConfig_UpdateTest : DBCreateTriggerScriptTest
	{
		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateJobType()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", jobType: "SHP", code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", jobType: "AGS", code: "BUY", code2: "TDR");
			
			AssertDuplicatedCurrencyCode("JCF_JobType", SqlDbType.Char, configPk1, configPk2, "SHP");
		}

		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateLedger()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", ledger: "AR", code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", ledger: "AP", code: "BUY", code2: "TDR");

			AssertDuplicatedCurrencyCode("JCF_Ledger", SqlDbType.Char, configPk1, configPk2, "AR");
		}

		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateCompany()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", companyPK: TestDbHelper.DefaultCompanyPK, code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", companyPK: TestDbHelper.OtherCompanyPK, code: "BUY", code2: "TDR");

			AssertDuplicatedCurrencyCode("JCF_GC", SqlDbType.UniqueIdentifier, configPk1, configPk2, TestDbHelper.DefaultCompanyPK);
		}

		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateServiceDirection()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", serviceDirection: "EXP", code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", serviceDirection: "IMP", code: "BUY", code2: "TDR");

			AssertDuplicatedCurrencyCode("JCF_ServiceDirection", SqlDbType.Char, configPk1, configPk2, "EXP");
		}

		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateTransportMode()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", transportMode: "SEA", code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", transportMode: "AIR", code: "BUY", code2: "TDR");

			AssertDuplicatedCurrencyCode("JCF_TransportMode", SqlDbType.Char, configPk1, configPk2, "SEA");
		}

		public void TestDifferentExRateConfig_WithValidCurrencyDateRange_UpdateInvoiceCurrencyType()
		{
			var configPk1 = DbHelper.InsertAccJobConfig("ERT", invoiceCurrencyType: "LOC", code: "BUY", code2: "TDR");
			var configPk2 = DbHelper.InsertAccJobConfig("ERT", invoiceCurrencyType: "FOR", code: "BUY", code2: "TDR");

			AssertDuplicatedCurrencyCode("JCF_InvoiceCurrencyType", SqlDbType.Char, configPk1, configPk2, "LOC");
		}

		void AssertDuplicatedCurrencyCode(string columnName, SqlDbType dbType, Guid defaultConfigPK, Guid changingConfigPK, object targetValue)
		{
			var startDate = new DateTime(2025, 02, 01);
			var expiryDate = new DateTime(2025, 03, 01);
			DbHelper.InsertAccJobConfigPivot(defaultConfigPK, code: "USD", exRateType: "BUY", startDate: startDate, expiryDate: expiryDate);
			DbHelper.InsertAccJobConfigPivot(changingConfigPK, code: "USD", exRateType: "BUY", startDate: expiryDate.AddDays(1), expiryDate: expiryDate.AddDays(2));

			AssertDbException("Duplicate Currency Code is used in other Exchange Rate Currency configuration.",
				() =>
				{
					var sql = $"UPDATE dbo.AccJobConfig SET {columnName} = @Value, JCF_SystemLastEditTimeUtc = GETUTCDATE(), JCF_SystemLastEditUser = 'TST' WHERE JCF_PK = @PK";
					using (var cmd = TestConnection.Command(sql))
					{
						cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, changingConfigPK);
						cmd.AddParameter("@Value", dbType, targetValue);
						cmd.ExecuteNonQuery();
					}
				});
		}

		void AssertDbException(string expectedMessage, AnonymousMethod dbAction) => AssertExceptionThrown<SqlException>(
			$"Should be: {expectedMessage}",
			$"{expectedMessage}\r\nThe transaction ended in the trigger. The batch has been aborted.", dbAction);
	}
}
