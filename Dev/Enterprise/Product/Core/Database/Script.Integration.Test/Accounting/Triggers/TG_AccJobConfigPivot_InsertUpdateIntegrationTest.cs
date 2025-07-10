using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	class TG_AccJobConfigPivot_InsertUpdateIntegrationTest : TransactionedTestCase
	{
		#region Cash Advance Defaulting

		public void TestTriggerForInsertingCashAdvanceDefaultingChargeCodePivot()
		{
			AssertExceptionThrownSafe("Invalid Parent Charge Code PK.",
				new Action<DbConnection>(
					(connection) =>
					{
						var config1Pk = InsertCashAdvanceDefaultingJobConfig(connection, defaultingOption: "INC");
						InsertCashAdvanceDefaultingJobConfigPivot(connection, configPK: config1Pk, parentPrefix: "AC", parentPK: Guid.NewGuid());
					}));

			var config2Pk = InsertCashAdvanceDefaultingJobConfig(TestConnection, defaultingOption: "INC");
			var companyChargeCodePk = TestHelper.InsertChargeCode(CompanyPK, "TSTCODE");
			AssertNoExceptionThrown("Charge Code TSTCODE.", () => InsertCashAdvanceDefaultingJobConfigPivot(TestConnection, configPK: config2Pk, parentPrefix: "AC", parentPK: companyChargeCodePk));
		}

		public void TestTriggerForUpdatingCashAdvanceDefaultingChargeCodePivot()
		{
			AssertExceptionThrownSafe("Invalid Parent Charge Code PK.",
				new Action<DbConnection>(
					(connection) =>
					{
						var config1Pk = InsertCashAdvanceDefaultingJobConfig(connection, defaultingOption: "INC");
						var companyChargeCode1Pk = InsertChargeCode(connection, "TSTCODE", "BRK", CompanyPK);
						var pivot1Pk = InsertCashAdvanceDefaultingJobConfigPivot(connection, configPK: config1Pk, parentPrefix: "AC", parentPK: companyChargeCode1Pk);
						UpdateCashAdvanceDefaultingJobConfigPivot(connection, pivotPK: pivot1Pk, parentPrefix: "AC", parentPK: Guid.NewGuid());
					}));

			var config2Pk = InsertCashAdvanceDefaultingJobConfig(TestConnection, defaultingOption: "INC");
			var companyChargeCode2Pk = InsertChargeCode(TestConnection, "TSTCODE", "BRK", CompanyPK);
			var companyChargeCode3Pk = InsertChargeCode(TestConnection, "TSTCODE2", "BRK", CompanyPK);
			var pivot2Pk = InsertCashAdvanceDefaultingJobConfigPivot(TestConnection, configPK: config2Pk, parentPrefix: "AC", parentPK: companyChargeCode2Pk);
			AssertNoExceptionThrown("Charge Code TSTCODE2.", () => UpdateCashAdvanceDefaultingJobConfigPivot(TestConnection, pivotPK: pivot2Pk, parentPrefix: "AC", parentPK: companyChargeCode3Pk));
		}

		public void TestInsertUpdateIntegrationTest_GivenInvalidChargeGroup_ShouldThrowException()
		{
			AssertExceptionThrownSafe("Invalid Charge Group for Cash Advance Defaulting configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var config1Pk = InsertCashAdvanceDefaultingJobConfig(connection, defaultingOption: "INC");
						InsertCashAdvanceDefaultingJobConfigPivot(connection, configPK: config1Pk, code: "123");
					}));
		}

		public void TestInsertUpdateIntegrationTest_GivenAllChargeGroups_ShouldReturnNoException()
		{
			foreach (var chargeGroup in GetAllChargeGroups())
			{
				var config2Pk = InsertCashAdvanceDefaultingJobConfig(TestConnection, defaultingOption: "INC");
				AssertNoExceptionThrown($"A new charge group '{chargeGroup}' has been added. Please update TG_AccJobConfigPivot_InsertUpdate trigger accordingly.", () => InsertCashAdvanceDefaultingJobConfigPivot(TestConnection, configPK: config2Pk, code: chargeGroup));
			}
		}

		public void TestTriggerForUpdatingCashAdvanceDefaultingChargeGroupPivot()
		{
			AssertExceptionThrownSafe("Invalid Charge Group for Cash Advance Defaulting configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var config1Pk = InsertCashAdvanceDefaultingJobConfig(connection, defaultingOption: "INC");
						var pivot1Pk = InsertCashAdvanceDefaultingJobConfigPivot(connection, configPK: config1Pk, code: "BRK");
						UpdateCashAdvanceDefaultingJobConfigPivot(connection, pivotPK: pivot1Pk, code: "123");
					}));

			foreach (var chargeGroup in GetAllChargeGroups())
			{
				var config2Pk = InsertCashAdvanceDefaultingJobConfig(TestConnection, defaultingOption: "INC");
				var pivot2Pk = InsertCashAdvanceDefaultingJobConfigPivot(TestConnection, configPK: config2Pk, code: "BRK");
				AssertNoExceptionThrown($"A new charge group '{chargeGroup}' has been added. Please update TG_AccJobConfigPivot_InsertUpdate trigger accordingly.", () => UpdateCashAdvanceDefaultingJobConfigPivot(TestConnection, pivot2Pk, code: chargeGroup));
			}
		}

		string[] GetAllChargeGroups()
		{
			var type = typeof(Enterprise.MasterFiles.Business.ChargeCodeGroupList.Codes);
			return (from field in type.GetFields() where field.IsLiteral && field.IsPublic && field.FieldType == typeof(string) select (string)field.GetRawConstantValue()).ToArray();
		}

		#endregion

		#region Exchange Rate Currency Configuration

		public void TestIdenticalExRateConfigurationsCannotHaveTheSameCurrency_WithValidDateRange_Insert()
		{
			AssertExceptionThrownSafe("Duplicate Currency Code is used in other Exchange Rate Currency configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk1 = InsertExchangeRateConfig(connection);
						var exRateconfigPk2 = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk1, currency: "USD", startDate: StartDate, expiryDate: ExpiryDate);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk2, currency: "USD", startDate: ExpiryDate.AddDays(1), expiryDate: ExpiryDate.AddDays(6));
					}));
		}

		public void TestIdenticalExRateConfigurationsCannotHaveTheSameCurrency_WithValidDateRange_Update()
		{
			AssertExceptionThrownSafe("Duplicate Currency Code is used in other Exchange Rate Currency configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk1 = InsertExchangeRateConfig(connection);
						var exRateconfigPk2 = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk1, currency: "AUD", startDate: StartDate, expiryDate: ExpiryDate);
						var pivotPk = InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk2, currency: "USD", startDate: ExpiryDate.AddDays(1), expiryDate: ExpiryDate.AddDays(6));
						UpdateExchangeRateCurrencyConfiguration(connection, pivotPk, currency: "AUD", startDate: ExpiryDate.AddDays(1), expiryDate: ExpiryDate.AddDays(6));
					}));
		}

		public void TestSameExRateConfigurationsCannotHaveOverlappingDateRange_Insert()
		{
			AssertNoExceptionThrownSafe("Allow to add same currency for same configuration with correct date range",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: StartDate.AddDays(1), expiryDate: StartDate.AddDays(2));
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: StartDate.AddDays(3), expiryDate: StartDate.AddDays(4));
					}));
			AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(-1), ExpiryDate.AddDays(1));
			AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(1), ExpiryDate.AddDays(-1));
			AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(-5), StartDate);
			AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(ExpiryDate, ExpiryDate.AddDays(5));
			AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(1), ExpiryDate);
		}

		public void TestSameExRateConfigurationsCannotHaveOverlappingDateRange_Update()
		{
			AssertNoExceptionThrownSafe("Allow to update same currency for same configuration with correct date range",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: StartDate, expiryDate: ExpiryDate);
						var pivotPk = InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: ExpiryDate.AddDays(1), expiryDate: ExpiryDate.AddDays(6));
						UpdateExchangeRateCurrencyConfiguration(connection, pivotPk, currency: "USD", startDate: StartDate.AddDays(-5), expiryDate: StartDate.AddDays(-2));
					}));
			AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(-1), ExpiryDate.AddDays(1));
			AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(1), ExpiryDate.AddDays(-1));
			AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(-5), StartDate);
			AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(ExpiryDate, ExpiryDate.AddDays(5));
			AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(StartDate.AddDays(1), ExpiryDate);
		}

		void AssertInsertOverlappingDateExchangeRateCurrencyConfiguration(DateTime startDate, DateTime expiryDate)
		{
			AssertExceptionThrownSafe("Overlapping date range for Exchange Rate Currency configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: StartDate, expiryDate: ExpiryDate);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: startDate, expiryDate: expiryDate);
					}));
		}

		void AssertUpdateOverlappingDateExchangeRateCurrencyConfiguration(DateTime startDate, DateTime expiryDate)
		{
			AssertExceptionThrownSafe("Overlapping date range for Exchange Rate Currency configuration.",
				new Action<DbConnection>(
					(connection) =>
					{
						var exRateconfigPk = InsertExchangeRateConfig(connection);
						InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: StartDate, expiryDate: ExpiryDate);
						var pivotPk = InsertExchangeRateCurrencyConfiguration(connection, configPk: exRateconfigPk, currency: "USD", startDate: ExpiryDate.AddDays(1), expiryDate: ExpiryDate.AddDays(6));
						UpdateExchangeRateCurrencyConfiguration(connection, pivotPk, currency: "USD", startDate: startDate, expiryDate: expiryDate);
					}));
		}

		public void TestTriggerInsertForExchangeRateCurrencyConfiguration_ExchangeRateType()
		{
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "***")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "XYZ")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "C99")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "C10")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "C00")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "L99")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "L10")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => InsertExchangeRateCurrencyConfiguration(connection, exRateType: "L00")));
		}

		public void TestTriggerUpdateForExchangeRateCurrencyConfiguration_ExchangeRateType()
		{
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "***")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "XYZ")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "C99")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "C10")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "C00")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "L99")));
			AssertNoExceptionThrownSafe("Correct Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "L10")));
			AssertExceptionThrownSafe("Incorrect Exchange Rate Type code for an Exchange Rate configuration.", new Action<DbConnection>((connection) => UpdateExchangeRateCurrencyConfiguration(connection, exRateType: "L00")));
		}

		#endregion

		#region Helpers

		void AssertExceptionThrownSafe(string expectedExceptionMessage, Action<DbConnection> codeToRun)
		{
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				newConnection.BeginTransaction();
				AssertExceptionThrown<SqlException>("Should be: " + expectedExceptionMessage, expectedExceptionMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate
				{ codeToRun(newConnection); });
				newConnection.RollbackTransaction();
			}
		}

		void AssertNoExceptionThrownSafe(string expectedExceptionMessage, Action<DbConnection> codeToRun)
		{
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				newConnection.BeginTransaction();
				AssertNoExceptionThrown(() => codeToRun(newConnection));
				newConnection.RollbackTransaction();
			}
		}

		Guid InsertExchangeRateConfig(DbConnection connection)
		{
			Guid pk = Guid.NewGuid();
			string sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code2, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "ERT");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, DBNull.Value);
				cmd.AddParameter("@Ledger", SqlDbType.Char, "AR");
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, string.Empty);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code2", SqlDbType.Char, "TDR");
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertCashAdvanceDefaultingJobConfig(DbConnection connection, string parentPrefix = "", Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string defaultingOption = "ALL")
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId,  JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Code3, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "CAD");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@Code", SqlDbType.VarChar, defaultingOption);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertCashAdvanceDefaultingJobConfigPivot(DbConnection connection, Guid? configPK = null, string code = "", string parentPrefix = "",   Guid? parentPK = null)
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfigPivot (JCT_PK, JCT_JCF_JobConfig, JCT_Code, JCT_ParentId, JCT_ParentTableCode, JCT_SystemCreateTimeUtc, JCT_SystemCreateUser, JCT_SystemLastEditTimeUtc, JCT_SystemLastEditUser) 
VALUES(@PK, @ConfigPK, @Code, @ParentPK, @ParentPrefix, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigPK", SqlDbType.UniqueIdentifier, configPK);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@Code", SqlDbType.VarChar, code);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertExchangeRateCurrencyConfiguration(DbConnection connection, Guid? configPk = null, string exRateType = "BUY", string currency = "", DateTime? startDate = null, DateTime? expiryDate = null)
		{
			if (configPk == null)
			{
				configPk = InsertExchangeRateConfig(connection);
			}
			
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfigPivot (JCT_PK, JCT_JCF_JobConfig, JCT_Code, JCT_ExRateType, JCT_StartDate, JCT_ExpiryDate, JCT_SystemCreateTimeUtc, JCT_SystemCreateUser, JCT_SystemLastEditTimeUtc, JCT_SystemLastEditUser) 
VALUES(@PK, @ConfigPK, @Currency, @ExRateType, @StartDate, @ExpiryDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigPK", SqlDbType.UniqueIdentifier, configPk);
				cmd.AddParameter("@ExRateType", SqlDbType.VarChar, exRateType);
				cmd.AddParameter("@Currency", SqlDbType.VarChar, currency);
				cmd.AddParameter("@StartDate", SqlDbType.Date, startDate ?? (object)DBNull.Value);
				cmd.AddParameter("@ExpiryDate", SqlDbType.Date, expiryDate ?? (object)DBNull.Value);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		void UpdateCashAdvanceDefaultingJobConfigPivot(DbConnection connection, Guid pivotPK, string code = "", string parentPrefix = "", Guid? parentPK = null)
		{
			var sql = @"UPDATE dbo.AccJobConfigPivot
						SET JCT_ParentTableCode = @ParentPrefix,
							JCT_ParentId = @ParentPK,
							JCT_Code = @Code,
							JCT_SystemLastEditTimeUtc = GETUTCDATE(),
							JCT_SystemLastEditUser = 'TST'
						WHERE JCT_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pivotPK);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@Code", SqlDbType.VarChar, code);
				cmd.ExecuteNonQuery();
			}
		}

		void UpdateExchangeRateCurrencyConfiguration(DbConnection connection, Guid? pivotPk = null, string exRateType = "BUY", string currency = "", DateTime? startDate = null, DateTime? expiryDate = null)
		{
			if (pivotPk == null)
			{
				pivotPk = InsertExchangeRateCurrencyConfiguration(connection);
			}

			var sql = @"UPDATE dbo.AccJobConfigPivot
						SET JCT_Code = @Currency,
							JCT_ExRateType = @ExRateType,
							JCT_StartDate = @StartDate,
							JCT_ExpiryDate = @ExpiryDate,
							JCT_SystemLastEditTimeUtc = GETUTCDATE(),
							JCT_SystemLastEditUser = 'TST'
						WHERE JCT_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pivotPk);
				cmd.AddParameter("@ExRateType", SqlDbType.VarChar, exRateType);
				cmd.AddParameter("@Currency", SqlDbType.VarChar, currency);
				cmd.AddParameter("@StartDate", SqlDbType.Date, startDate ?? (object)DBNull.Value);
				cmd.AddParameter("@ExpiryDate", SqlDbType.Date, expiryDate ?? (object)DBNull.Value);
				cmd.ExecuteNonQuery();
			}
		}

		Guid InsertChargeCode(DbConnection connection, string code, string chargeGroup, Guid companyPK)
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_GC) VALUES(@PK, @Code, @ChargeGroup, @Company)";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@Code", SqlDbType.VarChar, code);
				cmd.AddParameter("@ChargeGroup", SqlDbType.VarChar, chargeGroup);
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper = new TestDbHelper(TestConnection);
			CompanyPK = TestDbHelper.DefaultCompanyPK;
			StartDate = new DateTime(2025, 02, 01);
			ExpiryDate = new DateTime(2025, 03, 01);
		}

		TestDbHelper TestHelper;
		Guid CompanyPK;
		DateTime StartDate;
		DateTime ExpiryDate;
	}
}


