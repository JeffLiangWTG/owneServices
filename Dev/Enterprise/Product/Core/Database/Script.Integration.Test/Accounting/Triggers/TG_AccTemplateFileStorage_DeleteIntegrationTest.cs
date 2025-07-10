using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	sealed class TG_AccTemplateFileStorage_DeleteIntegrationTest : TransactionedTestCase
	{
		public void TestAccTemplateFileStorageDelete()
		{
			AssertExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
						(connection) =>
						{
							var pk = TestDataCreator.InsertTESJobConfig(connection, companyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsReceivable);
							DeleteTemplateFile(connection, pk);
						}));

			AssertNoExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
					(connection) =>
					{
						var pk = TestDataCreator.InsertTemplateFile(connection, companyPK, "ART");
						DeleteTemplateFile(connection, pk);
					}));

			AssertExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
					(connection) =>
					{
						var pkAR = TestDataCreator.InsertTemplateFile(connection, companyPK, "ART");
						var pkAP = TestDataCreator.InsertTESJobConfig(connection, companyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsPayable);

						DeleteTemplateFile(connection, pkAP);
					}));

			AssertNoExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
				(connection) =>
				{
					var pkAR = TestDataCreator.InsertTemplateFile(connection, companyPK, "ART");
					var pkAP = TestDataCreator.InsertTESJobConfig(connection, companyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsPayable);

					DeleteTemplateFile(connection, pkAR);
				}));

			AssertExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
					(connection) =>
					{
						TestDbHelper dbHelper = new TestDbHelper(connection);
						var testCompanyPK = dbHelper.InsertCompany("TST", "Test Company",
						  Enterprise.Core.Constants.CurrencyCodes.Turkey, Enterprise.Core.Constants.CountryCodes.Turkey, false, false);
						var pkAR = TestDataCreator.InsertTemplateFile(connection, companyPK, "ART");
						var pkAP = TestDataCreator.InsertTESJobConfig(connection, testCompanyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsPayable);

						DeleteTemplateFile(connection, pkAP);
					}));

			AssertNoExceptionThrownSafe("Delete operation NOT allowed on TemplateFileStorage for records which are already effective.",
				new Action<DbConnection>(
				(connection) =>
				{
					TestDbHelper dbHelper = new TestDbHelper(connection);
					var testCompanyPK = dbHelper.InsertCompany("TST", "Test Company",
					  Enterprise.Core.Constants.CurrencyCodes.Turkey, Enterprise.Core.Constants.CountryCodes.Turkey, false, false);
					var pkAR = TestDataCreator.InsertTemplateFile(connection, companyPK, "ART");
					var pkAP = TestDataCreator.InsertTESJobConfig(connection, testCompanyPK, "", Guid.Empty, "ART", ledger: LedgerTypeCodes.AccountsPayable);

					DeleteTemplateFile(connection, pkAR);
				}));
		}

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
				AssertNoExceptionThrown(expectedExceptionMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate
				{ codeToRun(newConnection); });
				newConnection.RollbackTransaction();
			}
		}

		int DeleteTemplateFile(DbConnection connection, Guid configPK)
		{
			var sql = @"DELETE FROM dbo.AccTemplateFileStorage WHERE TFS_PK = @PK";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, configPK);
				return cmd.ExecuteNonQuery();
			}
		}

		readonly Guid companyPK = TestDbHelper.DefaultCompanyPK;
	}
}

