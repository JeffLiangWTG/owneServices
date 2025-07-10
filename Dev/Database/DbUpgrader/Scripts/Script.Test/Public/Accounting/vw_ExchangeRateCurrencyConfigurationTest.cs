using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(vw_ExchangeRateCurrencyConfiguration))]
	class vw_ExchangeRateCurrencyConfigurationTest : DbCreateIndexedViewScriptTest
	{
		#region Job Config Pivot

		public void TestSameCurrencyCodesForDifferentJobConfigs()
		{
			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, serviceDirection: "IMP", code2: "TDR");
					var configPk2 = InsertJobConfig(TestConnection, serviceDirection: "EXP", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});
		}

		public void TestDistinctCurrencyCodesForTheSameJobConfig()
		{
			AssertNoExceptionThrown("Allow to add different currency codes to the same Job Exchange Rate configuration.",
				() =>
				{
					var configPk = InsertJobConfig(TestConnection, code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk, code: "USD");
				});
		}

		public void TestDistinctCurrencyCodesForDifferentJobConfigs()
		{
			AssertNoExceptionThrown("Allow to add different currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, serviceDirection: "IMP", code2: "TDR");
					var configPk2 = InsertJobConfig(TestConnection, serviceDirection: "EXP", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "USD");
				});
		}

		public void TestDuplicateCurrencyCodesForTheSameJobConfig()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();

				try
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						var configPk = InsertJobConfig(connection, code2: "TDR");
						InsertJobConfigPivot(connection, configPK: configPk, code: "AUD");
						InsertJobConfigPivot(connection, configPK: configPk, code: "AUD");
					});

					AssertContains("Cannot insert duplicate key row in object 'dbo.AccJobConfigPivot' with unique index 'NR_UX__JCT_JCF_JobConfig_JCT_Code_JCT_ParentId_JCT_StartDate'.", ex.Message);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestDuplicateCurrencyCodesForDifferentJobConfigs()
		{
			AssertExceptionThrownSafe(ExpectedErrorMsgForDuplicateCurrencyCode,
				(connection) =>
				{
					var config1Pk = InsertJobConfig(connection, code2: "TDR");
					var config2Pk = InsertJobConfig(connection, code2: "TDR");
					InsertJobConfigPivot(connection, configPK: config1Pk, code: "AUD");
					InsertJobConfigPivot(connection, configPK: config2Pk, code: "AUD");
				});
		}

		public void TestDuplicateCurrencyCodesForTheSameJobConfig_StartDate()
		{
			var startDate = new DateTime(2025, 02, 05);
			var expiryDate = new DateTime(2025, 03, 05);
			var configPk = InsertJobConfig(TestConnection, code2: "TDR");

			AssertNoExceptionThrown("Allow to add the same currency codes to same Job Exchange Rate configuration different date range", () =>
			{
				InsertJobConfigPivot(TestConnection, configPK: configPk, code: "AUD", startDate: startDate, expiryDate: expiryDate);
				InsertJobConfigPivot(TestConnection, configPK: configPk, code: "AUD", startDate: expiryDate.AddDays(2), expiryDate: expiryDate.AddDays(9));
			});

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				InsertJobConfigPivot(TestConnection, configPK: configPk, code: "AUD", startDate: startDate, expiryDate: expiryDate);
			});

			AssertContains("Cannot insert duplicate key row in object 'dbo.AccJobConfigPivot' with unique index 'NR_UX__JCT_JCF_JobConfig_JCT_Code_JCT_ParentId_JCT_StartDate'.", ex.Message);
		}

		public void TestDuplicateCurrencyCodesForDifferentJobConfigsMixed()
		{
			AssertExceptionThrownSafe(ExpectedErrorMsgForDuplicateCurrencyCode,
				(connection) =>
				{
					var config1Pk = InsertJobConfig(connection, code2: "TDR");
					var config2Pk = InsertJobConfig(connection, code2: "TDR");
					InsertJobConfigPivot(connection, configPK: config1Pk, code: "AUD");
					InsertJobConfigPivot(connection, configPK: config1Pk, code: "EUR");
					InsertJobConfigPivot(connection, configPK: config2Pk, code: "USD");
					InsertJobConfigPivot(connection, configPK: config2Pk, code: "AUD");
				});
		}

		public void TestDuplicatedCurrencyCodes_Company()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = TestDbHelper.DefaultCompanyPK;

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, companyPK: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, companyPK: TestDbHelper.OtherCompanyPK, code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_GC", SqlDbType.UniqueIdentifier, configPk2, targetValue);
		}

		public void TestDuplicatedCurrencyCodes_Ledger()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = "AR";

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, ledger: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, ledger: "AP", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_Ledger", SqlDbType.Char, configPk2, targetValue);
		}

		public void TestDuplicatedCurrencyCodes_JobType()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = "SHP";

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, jobType: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, jobType: "FCN", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_JobType", SqlDbType.Char, configPk2, targetValue);
		}

		public void TestDuplicatedCurrencyCodes_ServiceDirection()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = "IMP";

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, serviceDirection: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, serviceDirection: "EXP", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_ServiceDirection", SqlDbType.Char, configPk2, targetValue);
		}

		public void TestDuplicatedCurrencyCodes_TransportMode()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = "AIR";

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, transportMode: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, transportMode: "SEA", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_TransportMode", SqlDbType.Char, configPk2, targetValue);
		}

		public void TestDuplicatedCurrencyCodes_InvoiceCurrencyType()
		{
			Guid configPk2 = Guid.Empty;
			var targetValue = "LOC";

			AssertNoExceptionThrown("Allow to add the same currency codes to different Job Exchange Rate configuration.",
				() =>
				{
					var configPk1 = InsertJobConfig(TestConnection, invoiceCurrencyType: targetValue, code2: "TDR");
					configPk2 = InsertJobConfig(TestConnection, invoiceCurrencyType: "FOR", code2: "TDR");
					InsertJobConfigPivot(TestConnection, configPK: configPk1, code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPk2, code: "AUD");
				});

			AssertDuplicatedCurrencyCodesCore("JCF_InvoiceCurrencyType", SqlDbType.VarChar, configPk2, targetValue);
		}

		void AssertDuplicatedCurrencyCodesCore(string columnName, SqlDbType dbType, Guid configPK, object targetValue)
		{
			AssertExceptionThrownSafe(ExpectedErrorMsgForDuplicateCurrencyCode,
				(connection) =>
				{
					var sql = $"UPDATE dbo.AccJobConfig SET {columnName} = @Value WHERE JCF_PK = @PK";
					using (var cmd = TestConnection.Command(sql))
					{
						cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, configPK);
						cmd.AddParameter("@Value", dbType, targetValue);
						cmd.ExecuteNonQuery();
					}
				});
		}

		public void TestDistinctCurrencyCodes_Company()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, companyPK: TestDbHelper.DefaultCompanyPK, code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, companyPK: null, code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		public void TestDistinctCurrencyCodes_Ledger()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, ledger: "AR", code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, ledger: "AP", code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		public void TestDistinctCurrencyCodes_JobType()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, jobType: "SHP", code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, jobType: "ALL", code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		public void TestDistinctCurrencyCodes_ServiceDirection()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, serviceDirection: "IMP", code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, serviceDirection: "ALL", code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		public void TestDistinctCurrencyCodes_TransportMode()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, transportMode: "AIR", code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, transportMode: "ALL", code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		public void TestDistinctCurrencyCodes_InvoiceCurrencyType()
		{
			AssertDistinctCurrencyCodesCore(() =>
			{
				var config1Pk = InsertJobConfig(TestConnection, invoiceCurrencyType: "FOR", code2: "TDR");
				var config2Pk = InsertJobConfig(TestConnection, invoiceCurrencyType: "", code2: "TDR");
				return new Guid[] { config1Pk, config2Pk };
			});
		}

		void AssertDistinctCurrencyCodesCore(Func<Guid[]> prepareJobConfigFun)
		{
			AssertNoExceptionThrown("Allow to add different currency codes to multiple Job Exchange Rate configuration.",
				() =>
				{
					var configPKs = prepareJobConfigFun();
					InsertJobConfigPivot(TestConnection, configPK: configPKs[0], code: "AUD");
					InsertJobConfigPivot(TestConnection, configPK: configPKs[0], code: "EUR");
					InsertJobConfigPivot(TestConnection, configPK: configPKs[1], code: "USD");
				});
		}

		#endregion

		#region Implementation

		void AssertExceptionThrownSafe(string expectedExceptionMessage, Action<DbConnection> codeToRun)
		{
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				newConnection.BeginTransaction();
				var ex = AssertExceptionThrown<SqlException>(expectedExceptionMessage, delegate { codeToRun(newConnection); });
				AssertContains(expectedExceptionMessage, ex.Message);
				newConnection.RollbackTransaction();
			}
		}

		Guid InsertJobConfig(DbConnection connection, string parentPrefix = "", Guid? companyPK = null, Guid? parentPK = null, string ledger = "AR", string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string invoiceCurrencyType = "", string code2 = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_InvoiceCurrencyType, JCF_Code2, JCF_Code3, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser) 
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @InvoiceCurrencyType, @Code2, '', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "ERT");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK ?? TestDbHelper.DefaultCompanyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@JobType", SqlDbType.Char, jobType);
				cmd.AddParameter("@Direction", SqlDbType.Char, serviceDirection);
				cmd.AddParameter("@Mode", SqlDbType.Char, transportMode);
				cmd.AddParameter("@InvoiceCurrencyType", SqlDbType.VarChar, invoiceCurrencyType);
				cmd.AddParameter("@Code2", SqlDbType.VarChar, code2);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertJobConfigPivot(DbConnection connection, Guid? configPK = null, string code = "", string parentPrefix = "", Guid? parentPK = null, DateTime? startDate = null, DateTime? expiryDate = null, string exRateType = "BUY")
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfigPivot (JCT_PK, JCT_JCF_JobConfig, JCT_Code, JCT_ParentId, JCT_ParentTableCode, JCT_ExRateType, JCT_StartDate, JCT_ExpiryDate, JCT_SystemCreateTimeUtc, JCT_SystemCreateUser, JCT_SystemLastEditTimeUtc, JCT_SystemLastEditUser) 
VALUES(@PK, @ConfigPK, @Code, @ParentPK, @ParentPrefix, @ExRateType, @StartDate, @ExpiryDate, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigPK", SqlDbType.UniqueIdentifier, configPK);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK ?? (object)DBNull.Value);
				cmd.AddParameter("@Code", SqlDbType.VarChar, code);
				cmd.AddParameter("@ExRateType", SqlDbType.VarChar, exRateType);
				cmd.AddParameter("@StartDate", SqlDbType.Date, startDate ?? (object)DBNull.Value);
				cmd.AddParameter("@ExpiryDate", SqlDbType.Date, expiryDate ?? (object)DBNull.Value);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		const string ExpectedErrorMsgForDuplicateCurrencyCode = "Cannot insert duplicate key row in object 'dbo.vw_ExchangeRateCurrencyConfiguration' with unique index 'NR_UC__vw_ExchangeRateCurrencyConfiguration'.";

		#endregion
	}
}

