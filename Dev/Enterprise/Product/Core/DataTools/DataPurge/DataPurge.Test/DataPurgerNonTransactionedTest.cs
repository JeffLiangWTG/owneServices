using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	sealed class DataPurgerNonTransactionedTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestBackupCatchesDeviceOfflineExceptionAndThrowACustomisedOne()
		{
			var purger = new NoPurgeDataPurger();
			purger.BackupFilePath = @"C:\ThisGotToBeAnInvalidPath_E6D25E508176478C9E072E4F3D244566\AFileName.bak";
			string expectedOutput = String.Format("Backup Failed. The backup path ({0}) is not valid on the database server or access is denied.", purger.BackupFilePath);

			purger.Output = "";
			purger.BackupCatchingExceptions();
			AssertEquals("Output after unsuccessful BackupCatchingExceptions", expectedOutput, purger.Output);

			purger.Output = "";
			purger.BackupAndPurgeSystemWideTransactional();
			Assert("Output after unsuccessful BackupAndPurgeCatchingExceptions", purger.Output.Contains(expectedOutput));
		}

		class NoPurgeDataPurger : DataPurger
		{
			internal override IEnumerable<string> GetPurgeScripts(bool isCompanySpecificPurge)
			{
				yield break;
			}
		}

		[UseSnapshotProtection]
		public void TestPurgeKillExtraConnections()
		{
			using (var firstConnection = Db.NewExtraConnectionToMainDb())
			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				firstConnection.EnsureIsOpen();
				secondConnection.EnsureIsOpen();
				var purger = new DataPurger();
				purger.BackupFilePath = Temp.GetTempFileNameWithExtension("bak");
				try
				{
					var sqlText = $"SELECT COUNT(dbid) FROM sys.sysprocesses WHERE dbid > 0  and hostprocess > 0 and DB_NAME(dbid) = '{Db.Connection.CurrentDatabase}'";

					using (var mainConnection = Db.NewAdminConnection())
					{
						Assert("Extra Connections haven't been closed yet.", (int)mainConnection.ExecuteScalar(sqlText) > 2);

						purger.BackupAndPurgeSystemWideTransactional();

						AssertEquals("Extra Connections should be closed.", 1, (int)mainConnection.ExecuteScalar(sqlText));
					}
				}
				finally
				{
					File.Delete(purger.BackupFilePath);
				}
			}
		}

		#region Purge Without Database Transaction

		public void TestBackupAndPurgeSystemWideWithoutTransactionDoesNotPerformPurgeIfDatabaseTypeIsProduction()
		{
			using (MockProductionEnvironment())
			{
				var purger = new DataPurger();
				purger.BackupAndPurgeSystemWideWithoutTransaction();
				AssertEquals("Purge Output", "No purge performed as Data Purge requires a single atomic transaction in production systems.", purger.Output.ToString().Trim());
			}
		}

		public static IDisposable MockProductionEnvironment()
		{
			var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			var savedDbType = keyForTest.DatabaseTypeForTest;

			keyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			return new DisposableAction(() =>
			{
				keyForTest.DatabaseTypeForTest = savedDbType;
			});
		}

		[UseSnapshotProtection]
		public void TestBackupAndPurgeSystemWideWithoutTransaction()
		{
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE DummyLogged");
			AssertDummyLoggedTableHasRows(expected: false);
			Db.Connection.ExecuteNonQuery(@"INSERT dbo.DummyLogged (ZL2_PK, ZL2_SystemCreateTimeUtc, ZL2_SystemCreateUser, ZL2_SystemLastEditTimeUtc, ZL2_SystemLastEditUser)
			values (newid(), GetUtcDate(), 'USR', GetUtcDate(), 'USR')");
			AssertDummyLoggedTableHasRows(expected: true);

			var purger = new DataPurgerWithScriptError();

			using (var tempDir = new TestTemporaryDirectory())
			{
				purger.BackupFilePath = tempDir.Directory.FullName + Guid.NewGuid().ToString() + ".bak";
				purger.BackupAndPurgeSystemWideWithoutTransaction();
				File.Delete(purger.BackupFilePath);
			}

			// Purge changes before the error were still committed. 
			AssertDummyLoggedTableHasRows(expected: false);
			Assert("Purge Output - transaction warning message", purger.Output.Contains("* Purging in multiple database transactions - Please restore from the backup in case of error *"));
			Assert("Purge Output - test error", purger.Output.Contains("Invalid object name '*~!Some-Non-Existing-Table!~*'"));
		}

		[UseSnapshotProtection]
		public void TestRunStatusFlagForBackupAndPurgeSystemWideWithoutTransaction()
		{
			AssertEquals("CMP", Env.Registry.PurgeDataRunStatusFlag);

			var purger = new DataPurgerWithRunStatusFlagCheck();

			using (var tempDir = new TestTemporaryDirectory())
			{
				purger.BackupFilePath = tempDir.Directory.FullName + Guid.NewGuid().ToString() + ".bak";
				purger.BackupAndPurgeSystemWideWithoutTransaction();
				File.Delete(purger.BackupFilePath);
			}

			AssertEquals("RUN", purger.Flag);
			AssertEquals("CMP", Env.Registry.PurgeDataRunStatusFlag);
		}

		[UseSnapshotProtection]
		public void TestRunStatusFlagForBackupAndPurgeSystemWideWithoutTransaction_WithError()
		{
			AssertEquals("CMP", Env.Registry.PurgeDataRunStatusFlag);

			var purger = new DataPurgerWithScriptError();

			using (var tempDir = new TestTemporaryDirectory())
			{
				purger.BackupFilePath = tempDir.Directory.FullName + Guid.NewGuid().ToString() + ".bak";
				purger.BackupAndPurgeSystemWideWithoutTransaction();
				File.Delete(purger.BackupFilePath);
			}

			AssertEquals("ERR", Env.Registry.PurgeDataRunStatusFlag);
		}

		void AssertDummyLoggedTableHasRows(bool expected)
		{
			AssertEquals("DummyLogged has rows?", expected, Convert.ToBoolean(Db.Connection.ExecuteScalar("IF EXISTS (SELECT 1 FROM dbo.DummyLogged) SELECT 1 ELSE SELECT 0"), CultureInfo.InvariantCulture));
		}

		class DataPurgerWithRunStatusFlagCheck : DataPurger
		{
			public string Flag => flag;
			string flag;

			internal override void RunScriptCollectionDisablingTriggers(DbConnection connection, IEnumerable<string> scripts, int? cmdTimeoutInSeconds = null)
			{
				flag = Env.Registry.PurgeDataRunStatusFlag;
			}
		}

		class DataPurgerWithScriptError : DataPurger
		{
			internal override IEnumerable<string> GetPurgeScripts(bool isCompanySpecificPurge)
			{
				yield return "TRUNCATE TABLE DummyLogged";
				yield return "DELETE FROM [*~!Some-Non-Existing-Table!~*]";
			}
		}

		#endregion

		#region Number Fountains

		/// <summary>
		/// Number Fountains NOT purged by the Operational Purge:
		/// - IncidentApprovalClientRef
		/// - QuoteNumber
		/// </summary>
		public void TestOperationalPurgeAndNumberFountains()
		{
			string incidentApprovalFountainName = "IncidentApprovalClientRef";
			string quoteNumberFountainName = "QuoteNumber";
			string jobNumberFountainName = "JobNumber";

			using (DbConnection testConn = Db.NewAdminConnection())
			{
				try
				{
					testConn.BeginTransaction();

					Env.NumberFountains.IncidentApprovalClientRef.GetNextFormatted(testConn);
					Env.NumberFountains.QuoteNumber.GetNextFormatted(testConn);
					Env.NumberFountains.JobNumber.GetNextFormatted(testConn);

					int jobNumberRecordCount = GetNumberCacheRecordCount(testConn, jobNumberFountainName);
					int incidentApprovalRecordCount = GetNumberCacheRecordCount(testConn, incidentApprovalFountainName);
					int quoteFountainRecordCount = GetNumberCacheRecordCount(testConn, quoteNumberFountainName);

					AssertEquals("IncidentApproval number fountain should be in database", true, incidentApprovalRecordCount > 0);
					AssertEquals("Quote number fountain should be in database", true, quoteFountainRecordCount > 0);
					AssertEquals("JobNumber fountain should be in database", true, jobNumberRecordCount > 0);

					var dataPurger = new DataPurger();
					DataPurger.RunScriptCollection(testConn, dataPurger.GetPurgeScripts());

					int jobNumberRecordCountAfterPurge = GetNumberCacheRecordCount(testConn, jobNumberFountainName);
					int incidentApprovalRecordCountAfterPurge = GetNumberCacheRecordCount(testConn, incidentApprovalFountainName);
					int quoteFountainRecordCountAfterPurge = GetNumberCacheRecordCount(testConn, quoteNumberFountainName);

					AssertEquals("IncidentApproval number fountain should NOT be purged: records after purge", incidentApprovalRecordCount, incidentApprovalRecordCountAfterPurge);
					AssertEquals("Quote number fountain should NOT be purged: records after purge", quoteFountainRecordCount, quoteFountainRecordCountAfterPurge);
					AssertEquals("JobNumber fountain should be purged: records after purge", 0, jobNumberRecordCountAfterPurge);
				}
				finally
				{
					testConn.RollbackTransaction();
				}
			}
		}

		public void TestQuotationPurgeDeletesQuoteNumberFountains()
		{
			string quoteNumberFountainName = "QuoteNumber";

			using (DbConnection testConn = Db.NewAdminConnection())
			{
				try
				{
					testConn.BeginTransaction();

					Env.NumberFountains.QuoteNumber.GetNextFormatted(testConn);
					int quoteNumberRecordCount = GetNumberCacheRecordCount(testConn, quoteNumberFountainName);
					AssertEquals("Quote number fountain should be in database", true, quoteNumberRecordCount > 0);

					DataPurger dataPurger = new DataPurger();
					dataPurger.HasQuotationsPurgeScript = true;
					DataPurger.RunScriptCollection(testConn, dataPurger.GetPurgeScripts());
					int quoteNumberRecordCountAfterPurge = GetNumberCacheRecordCount(testConn, quoteNumberFountainName);
					AssertEquals("Quote number fountain should be purged: records after purge", 0, quoteNumberRecordCountAfterPurge);
				}
				finally
				{
					testConn.RollbackTransaction();
				}
			}
		}

		int GetNumberCacheRecordCount(DbConnection testConn, string fountainName)
		{
			string sqlText = String.Format(@"
				SELECT count(*)
				FROM dbo.StmNums INNER JOIN dbo.StmNumberCache ON SG_SN = SN_ID
				WHERE SN_Name = '{0}'",
				fountainName);

			int result = (int)testConn.ExecuteScalar(sqlText);
			return result;
		}

		#endregion
	}
}
