using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Shared;
using CargoWise.DataProtection;
using Enterprise.DbUpgrader.Resource.Version;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class DbConnectionTest : TestCase
	{
		#region TransactionLogCounterTest

		public void TestAppTransactionCountReturnsNothingByDefault()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var result = connection.GetAppTransactionCountChangedLog();
				AssertNull(result);
			}
		}

		public void TestAppTransactionCountReturnsNothingIfLogIsDisabled()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.DisableAppTransactionCountLog();
				var result = connection.GetAppTransactionCountChangedLog();
				AssertNull(result);
			}
		}

		public void TestErrorIsReportedInAttemptToOpenTransactionInAlreadyCompletedDelayedTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (var delayedTransaction = connection.DelayedTransactionWithManager(new DummyTransactionLockManager()))
				{
					using (new DisposableAction(() => ErrorReporter.Clear()))
					{
						connection.BeginTransaction();
						connection.CommitTransaction();

						delayedTransaction.CommitTransaction();

						connection.BeginTransaction();

						AssertEquals(1, ErrorReporter.TotalErrorCount);
						AssertEquals("Attempting to Open a Transaction in an already-finished delayed transaction. This transaction will most probably be leaked.", ErrorReporter.LastMessageReported);
					}
				}
			}
		}

		public void TestAppTransactionCountLogWithOneTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnableAppTransactionCountLog();
				using (var internalTransactionManager = connection.BeginTransactionWithManager())
				{
					var log = connection.GetAppTransactionCountChangedLog();

					AssertContains(@"Added Stacktraces:
AppTransactionCount: 1 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.RecordCurrentThreadId(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.BeginTransactionCore()
   at CargoWise.Data.DbConnection.BeginTransactionWithManager(Action onAutoRollbackAction)
   at CargoWise.Data.DbConnection.BeginTransactionWithManager()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithOneTransaction()", log);
				}
			}
		}

		public void TestAppTransactionCountLogWithTwoTransactions()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnableAppTransactionCountLog();
				using (var internalTransactionManager = connection.BeginTransactionWithManager())
				using (var anotherInternalTransactionManager = connection.BeginTransactionWithManager())
				{
					anotherInternalTransactionManager.RollbackTransaction();

					var log = connection.GetAppTransactionCountChangedLog();

					AssertContains(@"Added Stacktraces:
AppTransactionCount: 1 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.RecordCurrentThreadId(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.BeginTransactionCore()
   at CargoWise.Data.DbConnection.BeginTransactionWithManager(Action onAutoRollbackAction)
   at CargoWise.Data.DbConnection.BeginTransactionWithManager()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithTwoTransactions()", log);

					AssertContains(@"AppTransactionCount: 2 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.RecordCurrentThreadId(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.BeginTransactionCore()
   at CargoWise.Data.DbConnection.BeginTransactionWithManager(Action onAutoRollbackAction)
   at CargoWise.Data.DbConnection.BeginTransactionWithManager()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithTwoTransactions()", log);
				}
			}
		}

		public void TestAppTransactionCountLogWithFirstNormalTransactionSecondDelayedTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnableAppTransactionCountLog();
				using (var internalTransactionManager = connection.BeginTransactionWithManager())
				using (var delayedTransactionManager = connection.DelayedTransactionWithManager(new DummyTransactionLockManager()))
				{
					delayedTransactionManager.CommitTransaction();

					var log = connection.GetAppTransactionCountChangedLog();

					AssertContains(@"Added Stacktraces:
AppTransactionCount: 1 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.RecordCurrentThreadId(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.BeginTransactionCore()
   at CargoWise.Data.DbConnection.BeginTransactionWithManager(Action onAutoRollbackAction)
   at CargoWise.Data.DbConnection.BeginTransactionWithManager()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithFirstNormalTransactionSecondDelayedTransaction()", log);

					AssertContains(@"AppTransactionCount: 2 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.DelayedTransactionWithManager(ITransactionLockManager transactionLockManager)
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithFirstNormalTransactionSecondDelayedTransaction()", log);

					AssertContains(@"Reduced Stacktraces:
AppTransactionCount: 1 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.CommitTransaction()
   at CargoWise.Data.DbConnection.TransactionManager.Commit()
   at CargoWise.Data.BaseTransactionManager`1.CommitTransaction()
   at CargoWise.Data.DbConnection.DelayedTransactionManager.CommitTransaction()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithFirstNormalTransactionSecondDelayedTransaction()", log);
				}
			}
		}

		public void TestAppTransactionCountLogWithFirstDelayedTransactionSecondNormalTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnableAppTransactionCountLog();
				using (var delayedTransactionManager = connection.DelayedTransactionWithManager(new DummyTransactionLockManager()))
				using (var internalTransactionManager = connection.BeginTransactionWithManager())
				{
					internalTransactionManager.CommitTransaction();

					var log = connection.GetAppTransactionCountChangedLog();

					AssertContains(@"Added Stacktraces:
AppTransactionCount: 2 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.RecordCurrentThreadId(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.BeginTransactionCore()
   at CargoWise.Data.DbConnection.BeginTransactionWithManager(Action onAutoRollbackAction)
   at CargoWise.Data.DbConnection.BeginTransactionWithManager()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithFirstDelayedTransactionSecondNormalTransaction()", log);

					AssertContains(@"Reduced Stacktraces:
AppTransactionCount: 1 - StackTrace:    at CargoWise.Data.DbConnection.ModifyAppTransactionCount(Int32 modifiedCount)
   at CargoWise.Data.DbConnection.CommitTransaction()
   at CargoWise.Data.DbConnection.TransactionManager.Commit()
   at CargoWise.Data.BaseTransactionManager`1.CommitTransaction()
   at CargoWise.Data.Testing.DbConnectionTest.TestAppTransactionCountLogWithFirstDelayedTransactionSecondNormalTransaction()", log);
				}
			}
		}

		#endregion

		public void TestApplicationNameSuffix_SelfReport()
		{
			AssertEquals(DbConnectionConstants.ApplicationNames.CargoWiseOne, Db.Connection.ExecuteScalar("Select APP_NAME()").ToString());
			using (var connection = Db.NewExtraConnectionToMainDb("_asdf"))
			{
				AssertEquals(DbConnectionConstants.ApplicationNames.CargoWiseOne + "_asdf", connection.ExecuteScalar("Select APP_NAME()").ToString());
				connection.CloseConnection();
				connection.EnsureIsOpen();
				AssertEquals(DbConnectionConstants.ApplicationNames.CargoWiseOne + "_asdf", connection.ExecuteScalar("Select APP_NAME()").ToString());
			}
			AssertEquals(DbConnectionConstants.ApplicationNames.CargoWiseOne, Db.Connection.ExecuteScalar("Select APP_NAME()").ToString());
		}

		public void TestApplicationName()
		{
			try
			{
				DbConnection.ApplicationName = "qwerty";

				using (var extraConnection = Db.NewExtraConnectionToMainDb())
				{
					DbConnection.ApplicationName = null;
					AssertEquals("qwerty", extraConnection.ExecuteScalar("Select APP_NAME()").ToString());
					using (var adminConnection = Db.NewAdminConnection())
					{
						var sql = @"SELECT
  conn.session_id,
  host_name,
  program_name,
  nt_domain,
  login_name,
  connect_time,
  last_request_end_time
FROM sys.dm_exec_sessions AS sess
INNER JOIN sys.dm_exec_connections AS conn ON sess.session_id = conn.session_id
order by last_request_end_time asc";
						var applicationNames = new List<string>();
						using (var reader = adminConnection.Command(sql).ExecuteReader())
						{
							while (reader.Read())
							{
								applicationNames.Add(reader.GetString(2));
							}
						}
						Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains("qwerty"));
					}
				}
			}
			finally
			{
				DbConnection.ApplicationName = null;
			}
		}

		public void TestConnection_IsNotOpenedBeforeItIsNeeded()
		{
			var dataProviderFactory = new Mock<SqlDataProviderFactory>(MockBehavior.Strict);
			var mainDbRestrictedWriterLoginName = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

			dataProviderFactory.Setup(x => x.OpenNewDbConnection<RestrictedWriterLoginCredentials>(Db.ServerName, Db.DatabaseName, "CargoWiseOne", 60, false, 0, 0, 0))
				.CallBase()
				.Verifiable();

			using var extraConnection = new ExtraConnectionForTest(dataProviderFactory.Object);

			Assert(extraConnection.State == ConnectionState.Closed);

			dataProviderFactory.Verify(x => x.OpenNewDbConnection<RestrictedWriterLoginCredentials>(Db.ServerName, Db.DatabaseName, "CargoWiseOne", 60, false, 0, 0, 0)
				, Times.Never);
			AssertNoExceptionThrown(() => extraConnection.ExecuteNonQuery("Select 'should not throw'"));
			dataProviderFactory.Verify(x => x.OpenNewDbConnection<RestrictedWriterLoginCredentials>(Db.ServerName, Db.DatabaseName, "CargoWiseOne", 60, false, 0, 0, 0)
				, Times.Once);
			Assert(extraConnection.State == ConnectionState.Open);
		}

		class ExtraConnectionForTest : DbConnection<RestrictedWriterLoginCredentials>
		{
			public ExtraConnectionForTest(IDataProviderFactory dataProviderFactory) : base(dataProviderFactory, Db.ServerName, Db.DatabaseName)
			{
			}

			public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);
			protected override IDbConnection OpenNewDbConnection()
			{
				return base.OpenNewDbConnection();
			}
		}

		public void TestConnection_AfterCLose_CanBeOpened()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				Assert(extraConnection.State == ConnectionState.Closed);
				extraConnection.EnsureIsOpen();
				Assert(extraConnection.State == ConnectionState.Open);
				extraConnection.CloseConnection();
				Assert(extraConnection.State == ConnectionState.Closed);
				extraConnection.EnsureIsOpen();
				AssertNoExceptionThrown(() => extraConnection.ExecuteNonQuery("Select 'should not throw'"));
				Assert(extraConnection.State == ConnectionState.Open);
			}
		}

		public void TestApplicationSuffix()
		{
			var applicationNameSuffix = "_Customized_Document_5eae4ffb-d2e7-40a0-a7a9-bffc98236f57_Preview";
			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb(applicationNameSuffix))
			{
				AssertEquals(DbConnectionConstants.ApplicationNames.CargoWiseOne + applicationNameSuffix, extraConnection.ExecuteScalar("Select APP_NAME()").ToString());
				using (var adminConnection = Db.NewAdminConnection())
				{
					var sql = @"SELECT
  conn.session_id,
  host_name,
  program_name,
  nt_domain,
  login_name,
  connect_time,
  last_request_end_time
FROM sys.dm_exec_sessions AS sess
INNER JOIN sys.dm_exec_connections AS conn ON sess.session_id = conn.session_id
order by last_request_end_time asc";
					var applicationNames = new List<string>();
					using (var reader = adminConnection.Command(sql).ExecuteReader())
					{
						while (reader.Read())
						{
							applicationNames.Add(reader.GetString(2));
						}
					}
					Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Contains(DbConnectionConstants.ApplicationNames.CargoWiseOne + applicationNameSuffix));
				}
			}
		}

		public void TestApplicationNameAndSuffix()
		{
			var applicationNameSuffix = "_Customized_Document_5eae4ffb-d2e7-40a0-a7a9-bffc98236f57_Preview";
			try
			{
				DbConnection.ApplicationName = "qwerty";

				using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb(applicationNameSuffix))
				{
					AssertContains("qwerty" + applicationNameSuffix, extraConnection.ExecuteScalar("Select APP_NAME()").ToString());
					DbConnection.ApplicationName = null;
					using (var adminConnection = Db.NewAdminConnection())
					{
						var sql = @"SELECT
  conn.session_id,
  host_name,
  program_name,
  nt_domain,
  login_name,
  connect_time,
  last_request_end_time
FROM sys.dm_exec_sessions AS sess
INNER JOIN sys.dm_exec_connections AS conn ON sess.session_id = conn.session_id
order by last_request_end_time asc";
						var applicationNames = new List<string>();
						using (var reader = adminConnection.Command(sql).ExecuteReader())
						{
							while (reader.Read())
							{
								applicationNames.Add(reader.GetString(2));
							}
						}

						Assert(applicationNames.Aggregate((x, y) => x + "\r\n" + y), applicationNames.Any(x => x.Contains("qwerty" + applicationNameSuffix)));
					}
				}
			}
			finally
			{
				DbConnection.ApplicationName = null;
			}
		}

		public void TestRefreshLastActivity()
		{
			var originalValue = DbConnection.timeoutInitialized;
			try
			{
				DbConnection.timeoutInitialized = true;

				var oldCount = DbConnection.TimeoutableConnections.Count();
				using (var connectionA = Db.NewExtraConnectionToMainDb())
				{
					using (Db.DisposableActionForDbConnection())
					{
						AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
						AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
						AssertEquals(1 + oldCount, DbConnection.TimeoutableConnections.Count());

						var tries = 0;
						for (; tries < 10; ++tries)
						{
							var timeA = connectionA.TimeSinceLastActive;
							Thread.Sleep(50);
							var timeB = connectionA.TimeSinceLastActive;
							Assert(timeB > timeA);
							connectionA.EnsureIsOpen();
							var timeC = connectionA.TimeSinceLastActive;
							if (timeB > timeC)
							{
								Assert(timeB > timeC);
								break;
							}
							//try up to 10 times before giving up
						}
						if (tries >= 10)
						{
							Fail("EnsureIsOpen() should call RefreshLastActivity(), but isn't");
						}

						connectionA.CloseAndRemoveDueToTimeOut();
						AssertEquals(oldCount, DbConnection.TimeoutableConnections.Count());
						AssertEquals(System.Data.ConnectionState.Closed, connectionA.State);

						AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
						AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
						AssertEquals(1 + oldCount, DbConnection.TimeoutableConnections.Count());

						connectionA.CloseAndRemoveDueToTimeOut();
						AssertEquals(oldCount, DbConnection.TimeoutableConnections.Count());
						AssertEquals(System.Data.ConnectionState.Closed, connectionA.State);
					}
				}
			}
			finally
			{
				DbConnection.timeoutInitialized = originalValue;
			}
		}

		public void TestLockOnTwoDatabases()
		{
			var storageDbName = Db.DatabaseName + "_SD001";
			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, storageDbName);
			}

			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				const string Key = "WowItWorks";
				SqlApplicationLock lock1 = null;
				SqlApplicationLock lock2 = null;
				try
				{
					Assert(Db.Connection.TryGetLock(Key, out lock1));
					Assert(Db.Connection.TryGetLock(Key, out lock2, dbName: connection2.CurrentDatabase + "_SD001"));
					Assert(lock1.IsHoldingLock());
					Assert(lock2.IsHoldingLock());

					SqlApplicationLock lock3 = null;
					using (var conn2 = Db.NewExtraConnectionToMainDb())
					{
						Assert(!conn2.TryGetLock(Key, out lock3, dbName: connection2.CurrentDatabase + "_SD001"));
						AssertNull(lock3);
					}
				}
				finally
				{
					lock1?.Dispose();
					lock2?.Dispose();
				}
			}
		}

		public void TestTrackExecutedCommands()
		{
			AssertNull(Db.Connection.ExecutedCommands);
			AssertEquals(1, Db.Connection.ExecuteScalar<int>("select 1"));
			AssertNull(Db.Connection.ExecutedCommands);

			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals(2, Db.Connection.ExecuteScalar<int>("select 2"));
				AssertEquals(3, Db.Connection.ExecuteScalar<int>("select 3"));

				var paramscommand = Db.Connection.Command("select @privyet");
				paramscommand.AddParameter("@privyet", SqlDbType.Int, 4);
				paramscommand.ExecuteScalar();

				paramscommand = Db.Connection.Command("select @nihao, @gutentag");
				paramscommand.AddParameter("@gutentag", SqlDbType.Int, 6);
				paramscommand.AddParameter("@nihao", SqlDbType.Int, 5);
				paramscommand.ExecuteScalar();

				AssertArrayEqualsByElements(new[] { "select 2", "select 3",
					@"select @privyet
Params
@privyet: 4
",
					@"select @nihao, @gutentag
Params
@gutentag: 6
@nihao: 5
" }, Db.Connection.ExecutedCommands.ToArray());
			}

			AssertNull(Db.Connection.ExecutedCommands);
			AssertEquals(4, Db.Connection.ExecuteScalar<int>("select 4"));
			AssertNull(Db.Connection.ExecutedCommands);
		}

		public void TestTrackExecutedCommands_AndQueryPlans()
		{
			const string query = "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = @Code";

			var queryWithExtraInfo = query + Environment.NewLine + "Params" + Environment.NewLine + "@Code: 'DJT'" + Environment.NewLine;

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: false))
			{
				using var command = Db.Connection.Command(query);
				command.AddParameter("@Code", SqlDbType.VarChar, "DJT");
				command.ExecuteReader().Dispose();

				AssertEquals(1, Db.Connection.ExecutedCommands.Count());
				AssertEquals(1, Db.Connection.ExecutedCommandsAndQueryPlans.Count());

				AssertEquals(queryWithExtraInfo, Db.Connection.ExecutedCommands.Single());
				AssertEquals(queryWithExtraInfo, Db.Connection.ExecutedCommandsAndQueryPlans.Single().Item1);
				AssertEquals(0, Db.Connection.ExecutedCommandsAndQueryPlans.Single().Item2.Count);
			}

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var command = Db.Connection.Command(query))
				{
					command.AddParameter("@Code", SqlDbType.VarChar, "DJT");
					using (var reader = command.ExecuteReader())
					{
						AssertEquals(true, reader.Read());
						AssertEquals(0, reader.GetInt32(0));
					}
				}

				AssertEquals("Extra commands for enabling and disabling SHOWPLAN_XML", 3, Db.Connection.ExecutedCommands.Count());
				AssertEquals("Extra commands for enabling and disabling SHOWPLAN_XML", 3, Db.Connection.ExecutedCommandsAndQueryPlans.Count());

				var queries = Db.Connection.ExecutedCommandsAndQueryPlans.ToArray();

				AssertEquals(queryWithExtraInfo, queries[0].Item1);
				AssertEquals("SET STATISTICS XML ON", queries[1].Item1);
				AssertEquals("SET STATISTICS XML OFF", queries[2].Item1);

				var queryPlanXml = queries[0].Item2.Single().Replace("><", ">\r\n<");
				var planalyzer = new QueryPlanalyzer(queryPlanXml);

				AssertEquals(1, planalyzer.IndexSeeks.Count());

				var indexDetails = planalyzer.IndexSeeks.Single();

				CombineAssertions(() =>
				{
					AssertEquals("GlbStaff", indexDetails.TableName);
					AssertEquals("NR_UC__GS_Code", indexDetails.IndexName);
					AssertEquals("Clustered", indexDetails.IndexKind);
					AssertEquals("GS_Code", indexDetails.ColumnName);
				});
			}
		}

		public void TestTrackExecutedCommands_AndQueryPlans_WhenMultipleStatementsInABatch()
		{
			const string queryBatch =
		@"
SELECT COUNT(*) c INTO #thing FROM dbo.GlbStaff WHERE GS_Code = @Code
SELECT TOP 1 GS_Code from dbo.GlbStaff ORDER BY GS_Code
SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_CompletionStatement = @Statement
";

			string queryWithExtraInfo = queryBatch + Environment.NewLine + "Params" + Environment.NewLine + "@Code: 'DEA'"
										+ Environment.NewLine + "@Statement: 'Ovation of the Seas'" + Environment.NewLine;

			try
			{
				Db.Connection.EnsureIsOpen();

				using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					using (var command = Db.Connection.Command(queryBatch))
					{
						command.AddParameter("@Code", SqlDbType.VarChar, "DEA");
						command.AddParameter("@Statement", SqlDbType.NVarChar, "Ovation of the Seas");
						using (var reader = command.ExecuteReader())
						{
							Assert(reader.Read());
							AssertEquals("~AD", reader.GetString(0));

							Assert(reader.NextResult());
							Assert(reader.Read());
							AssertEquals(0, reader.GetInt32(0));

							AssertEquals(false, reader.NextResult()); // Only two statements produced a result. All the execution plans were skipped transparently.
						}
					}
					AssertSequencesEqual(new[]
					{
						queryWithExtraInfo,
						"SET STATISTICS XML ON",
						"SET STATISTICS XML OFF",
					}, Db.Connection.ExecutedCommands);

					AssertSequencesEqual(new[]
					{
						queryWithExtraInfo,
						"SET STATISTICS XML ON",
						"SET STATISTICS XML OFF",
					}, Db.Connection.ExecutedCommandsAndQueryPlans.Select(t => t.Item1));

					var queries = Db.Connection.ExecutedCommandsAndQueryPlans.ToArray();
					var queryPlans = queries[0].Item2.Select(plan => plan.Replace("><", ">\r\n<")).ToArray();

					AssertEquals("Should be one plan per statement in the batch", 3, queryPlans.Length);

					var planalyzer1 = new QueryPlanalyzer(queryPlans[0]);
					var planalyzer2 = new QueryPlanalyzer(queryPlans[1]);
					var planalyzer3 = new QueryPlanalyzer(queryPlans[2]);

					AssertEquals(1, planalyzer1.IndexSeeks.Count());
					AssertEquals(0, planalyzer1.IndexScans.Count());
					AssertEquals(0, planalyzer1.TableScans.Count());

					CombineAssertions(() =>
					{
						var indexDetails = planalyzer1.IndexSeeks.Single();

						AssertEquals("GlbStaff", indexDetails.TableName);
						AssertEquals("NR_UC__GS_Code", indexDetails.IndexName);
						AssertEquals("Clustered", indexDetails.IndexKind);
						AssertEquals("GS_Code", indexDetails.ColumnName);
					});

					AssertEquals(0, planalyzer2.IndexSeeks.Count());
					AssertEquals(1, planalyzer2.IndexScans.Count());
					AssertEquals(0, planalyzer2.TableScans.Count());

					CombineAssertions(() =>
					{
						var indexDetails = planalyzer2.IndexScans.Single();

						AssertEquals("GlbStaff", indexDetails.TableName);
						AssertEquals("NR_UC__GS_Code", indexDetails.IndexName);
						AssertEquals("Clustered", indexDetails.IndexKind);
						AssertEquals("GS_Code", indexDetails.ColumnName);
					});

					AssertEquals(0, planalyzer3.IndexSeeks.Count());
					AssertEquals(1, planalyzer3.IndexScans.Count());
					AssertEquals(0, planalyzer3.TableScans.Count());

					CombineAssertions(() =>
					{
						var indexDetails = planalyzer3.IndexScans.Single();

						AssertEquals("ProcessHeader", indexDetails.TableName);
						AssertEquals("NR_RC__FH_ParentId", indexDetails.IndexName);
						AssertEquals("Clustered", indexDetails.IndexKind);
						AssertEquals("FH_CompletionStatement", indexDetails.ColumnName);
					});
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("DROP TABLE IF EXISTS #thing");
			}
		}

		[UseSnapshotProtection]
		public void TestLockTimeout()
		{
			var initialLockTimeout = DbConnection.LockTimeout.CachedFromRegistry;
			try
			{
				Db.Connection.CloseConnection();
				AssertConnectionLockTimeout(Db.Connection, initialLockTimeout);

				using (var conn = Db.NewAdminConnection())
				{
					AssertConnectionLockTimeout(conn, initialLockTimeout);
					conn.CloseConnection();
					AssertConnectionLockTimeout(conn, initialLockTimeout);
				}

				using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.SqlMasterDb))
				{
					AssertConnectionLockTimeout(Db.Connection, initialLockTimeout);
					Db.Connection.CloseConnection();
					AssertConnectionLockTimeout(Db.Connection, initialLockTimeout);
				}

				var expectedNew = 2000;
				DbRegistry.LockTimeout.SaveValue(expectedNew, Db.Connection);

				AssertConnectionLockTimeout(Db.Connection, initialLockTimeout);

				using (var conn = Db.NewAdminConnection())
				{
					AssertConnectionLockTimeout(conn, expectedNew);
					conn.CloseConnection();
					AssertConnectionLockTimeout(conn, expectedNew);
				}

				Db.Connection.CloseConnection();
				AssertConnectionLockTimeout(Db.Connection, expectedNew);

				using (var conn = Db.NewExtraConnectionToMainDb())
				{
					AssertConnectionLockTimeout(conn, expectedNew);
					conn.CloseConnection();
					AssertConnectionLockTimeout(conn, expectedNew);
				}
			}
			finally
			{
				DbConnection.LockTimeout.CachedFromRegistry = initialLockTimeout;
			}
		}

		[UseSnapshotProtection]
		public void TestOpeningConnectionWontBeBlockedWhenStmDataAlreadyInAnotherSchemaModificationTransaction()
		{
			// Arrange
			var blockerTask = Task.Run(() => ExecuteBlockerTransactionWithSchMLock());

			// Act & Assert
			AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => OpeningConnectionTask());
			blockerTask.Wait();

			void OpeningConnectionTask()
			{
				Task.Delay(TimeSpan.FromSeconds(2)).Wait();

				using (var extraConnection = Db.NewExtraConnectionToMainDb())
				{
					extraConnection.EnsureIsOpen();
				}
			}

			void ExecuteBlockerTransactionWithSchMLock()
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					using (var transaction = adminConnection.BeginTransactionWithManager())
					{
						using (new DisposableAction(
							() => DbLockout.AcquireTransactionLockout(adminConnection),
							() => DbLockout.ResetTransactionLockout(adminConnection)))
						{
							var sqlSchemaModificationTransaction = $@"
-- Clean up the execution plan
DBCC FREEPROCCACHE;

-- Try aquire a Sch-M lock on dbo.StmData
ALTER TABLE dbo.StmData ADD xxx INT DEFAULT 0;

-- Simulated time-consuming actions
WAITFOR DELAY '00:00:10';";

							adminConnection.ExecuteNonQuery(sqlSchemaModificationTransaction);
						}

						transaction.RollbackTransaction();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestLockTimeoutUsesCachedValueFromRegistryIfConnectionNotToMainDb()
		{
			var initialLockTimeout = DbConnection.LockTimeout.CachedFromRegistry;
			var newLockTimeout = 2000;
			var currentCachedFromRegistry = newLockTimeout + 1000;
			DbRegistry.LockTimeout.SaveValue(newLockTimeout, Db.Connection);
			DbConnection.LockTimeout.CachedFromRegistry = currentCachedFromRegistry;
			try
			{
				using (var conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
				{
					AssertConnectionLockTimeout(conn, currentCachedFromRegistry);

					// Update CachedFromRegistry
					Db.Connection.CloseConnection();
					Db.Connection.EnsureIsOpen();
					conn.CloseConnection();
					using (((ICurrentDbControl)conn).UseDatabase(Db.SqlMsdb))
					{
						AssertConnectionLockTimeout(conn, newLockTimeout);
					}
				}
			}
			finally
			{
				DbConnection.LockTimeout.CachedFromRegistry = initialLockTimeout;
			}
		}

		[UseSnapshotProtection]
		public void TestLockTimeoutDoesNotUseRegistryWhenSchemaVersionCheckIsDisabled()
		{
			var initialLockTimeout = DbConnection.LockTimeout.CachedFromRegistry;
			var newLockTimeout = 2000;
			var currentCachedFromRegistry = newLockTimeout + 1000;
			DbRegistry.LockTimeout.SaveValue(newLockTimeout, Db.Connection);
			DbConnection.LockTimeout.CachedFromRegistry = currentCachedFromRegistry;
			using (Db.DisableSchemaVersionCheck())
			{
				try
				{
					Db.Connection.CloseConnection();
					AssertConnectionLockTimeout(Db.Connection, currentCachedFromRegistry);

					using (var conn = Db.NewAdminConnection())
					{
						AssertConnectionLockTimeout(conn, currentCachedFromRegistry);
						conn.CloseConnection();
						AssertConnectionLockTimeout(conn, currentCachedFromRegistry);
					}

					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.SqlMasterDb))
					{
						AssertConnectionLockTimeout(Db.Connection, currentCachedFromRegistry);
						Db.Connection.CloseConnection();
						AssertConnectionLockTimeout(Db.Connection, currentCachedFromRegistry);
					}

					AssertConnectionLockTimeout(Db.Connection, currentCachedFromRegistry);

					Db.Connection.CloseConnection();
					AssertConnectionLockTimeout(Db.Connection, currentCachedFromRegistry);

					using (var conn = Db.NewExtraConnectionToMainDb())
					{
						AssertConnectionLockTimeout(conn, currentCachedFromRegistry);
						conn.CloseConnection();
						AssertConnectionLockTimeout(conn, currentCachedFromRegistry);
					}
				}
				finally
				{
					DbConnection.LockTimeout.CachedFromRegistry = initialLockTimeout; // Rollback LockTimeout
				}
			}
		}

		public void TestConnectionWillCheckUpgradeAgainIfItsSessionHasBeenKilledWhenInUsed()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			using (var adminConnection = Db.NewAdminConnection())
			{
				extraConnection.EnsureIsOpen();

				adminConnection.BeginTransaction();

				using (new DisposableAction(
					() => DbLockout.AcquireTransactionLockout(adminConnection),
					() => DbLockout.ResetTransactionLockout(adminConnection)))
				{
					DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(adminConnection.CurrentDatabase, adminConnection, null, DateTime.MaxValue, Timeout.InfiniteTimeSpan);

					//Act & Assert
					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => extraConnection.EnsureIsOpen());
				}

				adminConnection.RollbackTransaction();
			}
		}

		public void TestTrackedEventsWhenStateChanged()
		{
			// Arrange
			DatabaseConnectionEventTracker.Instance.Clear();

			// Act
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.EnsureIsOpen();
			}

			// Assert
			var trackedEvents = DatabaseConnectionEventTracker.Instance.ConnectionEventDescription;
			AssertContains("<Count>2</Count>", trackedEvents);
			AssertContains("Status=Open", trackedEvents);
			AssertContains("Status=Closed", trackedEvents);
		}

		public void TestTrackedConnectionWhenOpening()
		{
			// Arrange
			DatabaseConnectionEventTracker.Instance.Clear();
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				// Act
				extraConnection.EnsureIsOpen();

				// Assert
				AssertContains($"[UpgradeCheck Disabled={extraConnection.IsUpgradeCheckDisabled}] [SchemaVersionCheck Disabled={Db.IsSchemaVersionCheckDisabled}] [DatabaseName Initialized={Db.DatabaseNameIsInitialized}] [Main Database={Db.DatabaseName}]", DatabaseConnectionEventTracker.Instance.ConnectionEventDescription);
			}
		}

		public void TestScalarGenericInt()
		{
			AssertEquals(1, Db.Connection.ExecuteScalar<int>("select 1"));
		}

		public void TestScalarGenericString()
		{
			AssertEquals("Hello", Db.Connection.ExecuteScalar<string>("select 'Hello'"));
		}

		public void TestScalarGenericReturningNull()
		{
			AssertExceptionThrown<ArgumentException>("Statement returned a null value", () => Db.Connection.ExecuteScalar<string>("select null"));
		}

		public void TestExecuteNonQuery()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery("create table #TestExecuteNonQuery(A INT NOT NULL, B INT NOT NULL)");
				AssertEquals(3, Db.Connection.ExecuteNonQuery("insert into #TestExecuteNonQuery (A, B) values (1, 0), (2, 0), (3, 0)"));
				AssertEquals(1, Db.Connection.ExecuteNonQuery("update #TestExecuteNonQuery set B=B+1 where A=3"));
				AssertEquals(2, Db.Connection.ExecuteNonQuery("update #TestExecuteNonQuery set B=B+2 where A in (@p1, @p2)", command =>
				{
					command.AddParameter("p1", SqlDbType.Int, 2);
					command.AddParameter("p2", SqlDbType.Int, 3);
				}));
				List<string> actualRows = new List<string>();
				Db.Connection.ExecuteReader("select A, B from #TestExecuteNonQuery order by A, B", r => { actualRows.Add($"{r["A"]}, {r["B"]}"); });
				AssertMultilineASCIIEquals(string.Join("\r\n", new string[] { "1, 0", "2, 2", "3, 3" }), string.Join("\r\n", actualRows));
			}
		}

		#region TestRetry

		public void TestExecuteNonQueryWithRetry_DefaultRetryPolicy_SucceedWithoutRetry()
		{
			var retryCount = 0;

			using (Db.Connection.PrepareRetryContextForTest(null, null))
			{
				Db.Connection.ExecuteNonQueryWithRetry(
					sqlText: "SELECT 'Hello'",
					policy: null,
					retryingEventHandler: (sender, e) => { retryCount++; });

				AssertEquals(0, retryCount);
				AssertEquals(1, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		public void TestExecuteNonQueryWithRetry_DefaultRetryPolicy_SucceedWithTwoRetries()
		{
			var retryCount = 0;

			using (Db.Connection.PrepareRetryContextForTest(
				condition: null,
				action: (sqlText, executionCount) =>
				{
					if (executionCount < 3)
					{
						throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
					}
				}))
			{
				Db.Connection.ExecuteNonQueryWithRetry(
					sqlText: "SELECT 'Hello'",
					policy: null,
					retryingEventHandler: (sender, e) => { retryCount++; });

				AssertEquals(2, retryCount);
				AssertEquals(3, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		public void TestExecuteNonQueryWithRetry_DefaultRetryPolicy_ExceptionWithoutRetry()
		{
			var retryCount = 0;

			using (Db.Connection.PrepareRetryContextForTest(null, null))
			{
				AssertExceptionThrown<SqlException>(() =>
				Db.Connection.ExecuteNonQueryWithRetry(
					sqlText: $"Not a valid SQL statement",
					policy: null,
					retryingEventHandler: (sender, e) => { retryCount++; }));

				AssertEquals(0, retryCount);
				AssertEquals(1, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		public void TestExecuteNonQueryWithRetry_DefaultRetryPolicy_ExceptionWithAllRetries()
		{
			var retryCount = 0;

			using (Db.Connection.PrepareRetryContextForTest(
				condition: null,
				action: (sqlText, executionCount) =>
				{
					throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
				}))
			{
				var sqlException = AssertExceptionThrown<SqlException>(() =>
				Db.Connection.ExecuteNonQueryWithRetry(
					sqlText: $"SELECT 'HELLO'",
					policy: null,
					retryingEventHandler: (sender, e) => { retryCount++; }));

				AssertEquals(DbErrorType.LockTimeoutExpired, new DbErrorMatch(sqlException).ExceptionType);

				AssertEquals(2, retryCount);
				AssertEquals(3, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		class DetectionStrategyForTest : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				return ex is SqlException sqlException
					// 15007
					&& new DbErrorMatch(sqlException).ExceptionType == DbErrorType.InvalidLoginOrNoPermission;
			}
		}

		public void TestExecuteNonQueryWithRetry_CustomRetryPolicy_SucceedWithTwoRetries()
		{
			var retryPolicyForTest = new RetryPolicy(
				new DetectionStrategyForTest(),
				new ExponentialBackoff(
					retryCount: 5,
					minBackoff: TimeSpan.FromMilliseconds(0),
					maxBackoff: TimeSpan.FromSeconds(0),
					deltaBackoff: TimeSpan.FromSeconds(0)));

			var retryCount = 0;

			using (Db.Connection.PrepareRetryContextForTest(
				condition: null,
				action: (sqlText, executionCount) =>
				{
					if (executionCount < 3)
					{
						throw SqlExceptionBuilder.CreateSqlException(15007, "UserLogin");
					}
				}))
			{
				Db.Connection.ExecuteNonQueryWithRetry(
					sqlText: "SELECT 'Hello'",
					policy: retryPolicyForTest,
					retryingEventHandler: (sender, e) => { retryCount++; });

				AssertEquals(2, retryCount);
				AssertEquals(3, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		#endregion // TestRetry

		public void TestExecuteReader()
		{
			var actual = "";
			Db.Connection.ExecuteReader("SELECT val = 'a' UNION ALL SELECT 'b'", (reader) => actual += (string)reader["val"]);

			AssertEquals("ab", actual);

			actual = "";
			Db.Connection.ExecuteReader("SELECT val = @a UNION ALL SELECT 'b'"
				, (cmd) => cmd.AddParameter("@a", SqlDbType.Char, 1, "a")
				, (reader) => actual += (string)reader["val"]
				);

			AssertEquals("ab", actual);
		}

		public void TestBeginTransaction_WhenDbUpgraded_ThrowsUpgradedException()
		{
			using (var conn = new UpgradedDbConnectionForTest(Db.ServerName, Db.DatabaseName))
			{
				AssertExceptionThrown<DatabaseUpgradedException>(() => conn.BeginTransaction());
			}
		}

		public void TestConnectionWithSettings_Connection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				IDbConnectionInternals internals = connection;
				IDbConnectionWithSettings settings = connection;

				connection.BeginTransaction();

				AssertSame(internals.InternalDbConnection, settings.DbConnection);
				AssertSame(internals.InternalDbTransaction, settings.Transaction);

				connection.RollbackTransaction();
			}
		}

		public void TestConnectionWithSettings_CreateCommand()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.DefaultCommandTimeOutInSeconds = 4242;
				IDbConnectionWithSettings settings = connection;

				connection.BeginTransaction();

				using (var command = settings.CreateCommand("Sql Text"))
				{
					AssertSame(settings.DbConnection, command.Connection);
					AssertSame(settings.Transaction, command.Transaction);
					AssertEquals("Sql Text", command.CommandText);
					AssertEquals(4242, command.CommandTimeout);
				}

				connection.RollbackTransaction();
			}
		}

		public class UpgradedDbConnectionForTest : AdminConnection, IDbReconnectionHandling
		{
			public UpgradedDbConnectionForTest(string serverName, string databaseName)
				: base(serverName, databaseName)
			{
			}

			public bool shouldSayDbSchemaHasChanged = true;
			protected override int CompareDbVersions(VersionLabel dbSchemaVersion, VersionLabel dbScriptVersion, VersionLabel dbTransformationVersion)
			{
				return shouldSayDbSchemaHasChanged ? 1 : 0;
			}

			protected override void OpenConnectionWithSplashInfo()
			{
				base.OpenConnectionWithSplashInfo();
				hasConnectedBefore = true;
			}

			internal override ConnectionErrorManager ConnectionErrorHandler
			{
				get
				{
					return connectionErrorHandlerForTest ?? (connectionErrorHandlerForTest = new ConnectionErrorManagerForTest(this));
				}
			}
			ConnectionErrorManagerForTest connectionErrorHandlerForTest;
		}

		class ConnectionErrorManagerForTest : ConnectionErrorManager
		{
			public ConnectionErrorManagerForTest(IDbReconnectionHandling connectionToHandle)
				: base(connectionToHandle)
			{
				this.currentConnectionToHandle = connectionToHandle;
			}

			readonly IDbReconnectionHandling currentConnectionToHandle;

			protected override bool ReconnectIfApplicableCore(Exception e)
			{
				var result = true;

				try
				{
					currentConnectionToHandle.CloseAndReopenConnection();
				}
				catch (CloseReopenWhileConnectingException)
				{
					result = false;
				}

				return result;
			}
		}

		public class UpgradedDbConnectionForTestErrorReport : AdminConnection, IDbReconnectionHandling
		{
			public UpgradedDbConnectionForTestErrorReport(string serverName, string databaseName)
				: base(serverName, databaseName)
			{
			}

			internal override ConnectionErrorManager ConnectionErrorHandler
			{
				get
				{
					return connectionErrorManagerForTestErrorReport ?? (connectionErrorManagerForTestErrorReport = new ConnectionErrorManagerForTestErrorReport(this));
				}
			}

			ConnectionErrorManagerForTestErrorReport connectionErrorManagerForTestErrorReport;
		}

		class ConnectionErrorManagerForTestErrorReport : ConnectionErrorManager
		{
			public ConnectionErrorManagerForTestErrorReport(IDbReconnectionHandling connectionToHandle)
				: base(connectionToHandle)
			{
			}

			protected override bool ReconnectIfApplicableCore(Exception e)
			{
				return false;
			}
		}

		public void TestRunLockedAlreadyFinishedAndGotLock()
		{
			using (var dbConnection = Db.NewExtraConnectionToMainDb())
			{
				var timesCalled = 0;
				var result = dbConnection.RunLocked(
					"ab426a86-6f90-42c8-a9bf-69baa9a607b2",
					(bool firstAttempt) =>
					{
						timesCalled++;
						Assert(firstAttempt);
					},
					() => true
				);
				AssertEquals(LockedProcessResult.AlreadyBeingProcessed, result);
				AssertEquals(0, timesCalled);
			}
		}

		public void TestRunLockedNotAlreadyFinishedAndGotLock()
		{
			using (var dbConnection = Db.NewExtraConnectionToMainDb())
			{
				var timesCalled = 0;
				var result = dbConnection.RunLocked(
					"b609853f-5ea7-46e5-a3ae-c60eff7f1814",
					(bool firstAttempt) =>
					{
						timesCalled++;
						Assert(firstAttempt);
					},
					() => false
				);
				AssertEquals(LockedProcessResult.Completed, result);
				AssertEquals(1, timesCalled);
			}
		}

		public void TestRunLockedLockAlreadyTaken()
		{
			using (var anotherDbConnection = Db.NewExtraConnectionToMainDb())
			{
				Assert(anotherDbConnection.TryGetLock("dac8ce4b-4e92-4182-8423-05e276af23db", out SqlApplicationLock mutex));
				using (mutex)
				{
					using (var dbConnection = Db.NewExtraConnectionToMainDb())
					{
						var timesCalled = 0;
						var result = dbConnection.RunLocked(
							"dac8ce4b-4e92-4182-8423-05e276af23db",
							(bool firstAttempt) => { timesCalled++; },
							() => false
						);
						AssertEquals(LockedProcessResult.AlreadyBeingProcessed, result);
						AssertEquals(0, timesCalled);
					}
				}
			}
		}

		public void TestRunLockedCanBeShared()
		{
			var lockKey = "myLockKeyForATest";
			Assert("Precondition: We can get a shared lock", Db.Connection.TryGetLock(lockKey, out SqlApplicationLock originalMutex, lockMode: SqlApplicationLockMode.Shared));

			using (var anotherDbConnection = Db.NewExtraConnectionToMainDb())
			using (originalMutex)
			{
				var result = anotherDbConnection.RunLocked(lockKey, _ => { }, lockMode: SqlApplicationLockMode.Shared);
				AssertEquals("We are able to take another lock with the same key because the lock mode is shared", LockedProcessResult.Completed, result);
			}
		}

		public void TestRunLockedMaxRetriesExceeded()
		{
			using (var dbConnection = Db.NewExtraConnectionToMainDb())
			{
				var timesCalled = 0;
				var result = dbConnection.RunLocked(
					"096f4594-ab09-4090-a46c-f003c57e9d95",
					(bool firstAttempt) =>
					{
						timesCalled++;
						AssertEquals(timesCalled == 1, firstAttempt);
						throw new SqlLockLostException();
					},
					() => false,
					10
				);
				AssertEquals(LockedProcessResult.Error, result);
				AssertEquals(10, timesCalled);
			}
		}

		public void TestRunLockedMaxRetriesEdgeCase()
		{
			using (var dbConnection = Db.NewExtraConnectionToMainDb())
			{
				var timesCalled = 0;
				var result = dbConnection.RunLocked(
					"096f4594-ab09-4090-a46c-f003c57e9d95",
					(bool firstAttempt) =>
					{
						timesCalled++;
						AssertEquals(timesCalled == 1, firstAttempt);
						if (timesCalled < 10)
						{
							throw new SqlLockLostException();
						}
					},
					() => false,
					10
				);
				AssertEquals(LockedProcessResult.Completed, result);
				AssertEquals(10, timesCalled);
			}
		}

		void AssertConnectionLockTimeout(DbConnection connection, int expected)
		{
			var sql = @"SELECT @@LOCK_TIMEOUT;";

			using (var cmd = connection.Command(sql))
			{
				AssertEquals(expected, (int)cmd.ExecuteScalar());
			}
		}

		public void TestOpenConnection_WhenPasswordIsInvalid_ThrowsSqlException()
		{
			using (var conn = new ExtraConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "muhaha"))
			{
				conn.OpenRetries = 1; // No need for retries that only make the test slower
				AssertExceptionThrown<SqlException>(() => conn.EnsureIsOpen());
			}
		}

		public void TestAllPooledDbConnectionsAreInUse_RestrictedWriterLogin()
		{
			Exception exception = null;

			var connections = new List<ConnectionForTestAllPooledDbConnectionsAreInUse>();

			try
			{
				for (int i = 0; i < 1000; i++)
				{
					var conn = new ConnectionForTestAllPooledDbConnectionsAreInUse(Db.ServerName, Db.DatabaseName);
					connections.Add(conn);
					conn.OpenConnectionCore_Exposed();
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				foreach (var connection in connections)
				{
					connection.Dispose();
					connection.ClearThePool();
				}
			}
			Assert(exception is InvalidOperationException);
			Assert(exception.Message.Contains("Timeout expired."));
			Assert(exception.Message.Contains("The timeout period elapsed prior to obtaining a connection from the pool."));
		}

		public void TestAllPooledDbConnectionsAreInUse_IntegratedSecurity()
		{
			Exception exception = null;
			var connections = new List<IntegratedSecurityConnectionForTestAllPooledDbConnectionsAreInUse>();

			try
			{
				for (int i = 0; i < 1000; i++)
				{
					var conn = new IntegratedSecurityConnectionForTestAllPooledDbConnectionsAreInUse(Db.ServerName, Db.DatabaseName);
					connections.Add(conn);
					conn.OpenConnectionCore_Exposed();
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				foreach (var connection in connections)
				{
					connection.Dispose();
					connection.ClearThePool();
				}
			}
			Assert(exception is InvalidOperationException);
			Assert(exception.Message.Contains("Timeout expired."));
			Assert(exception.Message.Contains("The timeout period elapsed prior to obtaining a connection from the pool."));
		}

		public void TestHasDatabase()
		{
			string mockDbForTest = "MockDbForDbConnectionTest";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest);
				AssertEquals(true, Db.Connection.DatabaseExists(mockDbForTest));
				AssertEquals(false, Db.Connection.DatabaseExists(mockDbForTest + "ZZ"));
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001");
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD002");
			}
		}

		public void TestCommitWillResetTransactionCountEvenWhenItFails()
		{
			// Arrange
			using var connection = Db.NewExtraConnectionToMainDb();
			connection.BeginTransaction();

			AssertEquals(1, connection.AppTransactionCount);

			using (var connection2 = Db.NewAdminConnection())
			{
				connection2.ExecuteNonQuery($"KILL {connection.SPID}");
			}

			// Act & Assert
			var sqlException = AssertExceptionThrown<SqlException>(() => connection.CommitTransaction());

			AssertEquals("Expected exception during commit", DbErrorType.GeneralNetworkError, new DbErrorMatch(sqlException).ExceptionType);
			AssertEquals("Transaction count should reset, despite the error.", 0, connection.AppTransactionCount);
		}

		/// <summary>
		/// At this stage we do not care whether or not a database is using snapshot isolation.
		/// Should snap-shot isolation became the standard our databases, we will assert it returns TRUE.
		/// </summary>
		public void TestIsDbUsingSnapshotIsolation()
		{
			AssertEquals("Master Database using snapshot isolation?", false, Db.Connection.IsDbUsingSnapshotIsolation(Db.SqlMasterDb));

			bool? isUsingSnapshotIsolation = Db.Connection.IsCurrentDbUsingSnapshotIsolation;
			Assert("Variable should have a value", isUsingSnapshotIsolation.HasValue);
		}

		#region SqlApplicationLock

		public void TestFailingToAcquireALockReturnsNull()
		{
			Assert("Precondition: We got a lock", Db.Connection.TryGetLock("TestFailingToAcquireALockReturnsNull", out SqlApplicationLock originalMutex));

			using (var anotherDbConnection = Db.NewExtraConnectionToMainDb())
			using (originalMutex)
			{
				Assert("Precondition: We didn't get a lock", !anotherDbConnection.TryGetLock("TestFailingToAcquireALockReturnsNull", out SqlApplicationLock secondLock));
				AssertNull("We did not get successful lock, so we should get a null back (since our failed lock was disposed and we dont want to leak any strongrefs to disposed objects)", secondLock);
			}
		}

		public void TestHasAquiredLock()
		{
			SqlApplicationLock lock1 = null;
			try
			{
				AssertEquals(true, Db.Connection.TryGetLock("Hagbag", out lock1));
				AssertEquals(true, Db.Connection.HasAquiredLock("Hagbag"));
				AssertEquals(false, Db.Connection.HasAquiredLock("sadbagsagbad"));
				AssertEquals("HasAquiredLock is case insensitive.", true, Db.Connection.HasAquiredLock("HAGBAG"));
				AssertEquals("HasAquiredLock is case insensitive.", true, Db.Connection.HasAquiredLock("hAgBaG"));
			}
			finally
			{
				lock1?.Dispose();
			}

			AssertEquals("Lock was disposed.", false, Db.Connection.HasAquiredLock("hagbag"));
		}

		public void TestHasAquiredLock_DoubleLock()
		{
			SqlApplicationLock lock1 = null;
			SqlApplicationLock lock2 = null;
			try
			{
				try
				{
					AssertEquals(true, Db.Connection.TryGetLock("Hagbag", out lock1));
					AssertEquals(true, Db.Connection.TryGetLock("Hagbag", out lock2));
				}
				finally
				{
					lock1?.Dispose();
				}

				AssertEquals("There is still a lock.", true, Db.Connection.HasAquiredLock("hagbag"));
			}
			finally
			{
				lock2?.Dispose();
			}

			AssertEquals("Both locks were disposed.", false, Db.Connection.HasAquiredLock("hagbag"));
		}

		public void TestReconnectWithNoLivingLocksDoesntThrowExcption()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert("Precondition: We got a lock", connection.TryGetLock("TestReconnectWithNoLivingLocksDoesntThrowExcption_test", out SqlApplicationLock mutex));
				mutex.Dispose();
				connection.CloseConnection();
				AssertNoExceptionThrown(connection.EnsureIsOpen);
			}
		}

		public void TestReconnectWithAnAcquiredLock()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert("Precondition: We got a lock", connection.TryGetLock("TestReconnectWithAnAcquiredLock_test", out SqlApplicationLock mutex));

				using (mutex)
				{
					connection.CloseConnection();
					AssertExceptionThrown<SqlLockLostException>(connection.EnsureIsOpen);
				}
			}
		}

		public void TestReconnectWithAnAcquiredLock_ThrowsSqlLockLostException_UntilLockObjectIsNotDisposed()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert("Precondition: We got a lock", connection.TryGetLock("TestReconnectWithAnAcquiredLock_test3", out SqlApplicationLock mutex));

				using (mutex)
				{
					connection.CloseConnection();
					AssertExceptionThrown<SqlLockLostException>(connection.EnsureIsOpen);
					AssertExceptionThrown<SqlLockLostException>(connection.EnsureIsOpen);
					AssertExceptionThrown<SqlLockLostException>(connection.EnsureIsOpen);
				}
				connection.EnsureIsOpen();
			}
		}

		public SqlApplicationLock CreateManyLocksAndDisposeAllButOne(DbConnection connection, int howMany)
		{
			SqlApplicationLock undisposedLock = null;

			for (int i = 0; i < howMany; i++)
			{
				Assert("Couldn't acquire lock #" + i, connection.TryGetLock(Guid.NewGuid().ToString(), out SqlApplicationLock @lock));

				if (i == howMany / 2 && undisposedLock == null)
				{
					undisposedLock = @lock;
				}
				else
				{
					@lock.Dispose();
				}
			}

			return undisposedLock;
		}

		public void TestReconnectWithSeveralLocks()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var @lock = CreateManyLocksAndDisposeAllButOne(connection, howMany: 10))
			{
				connection.CloseConnection();

				AssertExceptionThrown<SqlLockLostException>(connection.EnsureIsOpen);
			}
		}

		public void TestEnsureNoUndisposedLocks_SqlApplicationLockInfoShouldBeAppenedToExceptionMessage()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var lock1 = CreateManyLocksAndDisposeAllButOne(connection, howMany: 2))
			using (var lock2 = CreateManyLocksAndDisposeAllButOne(connection, howMany: 2))
			using (var lock3 = CreateManyLocksAndDisposeAllButOne(connection, howMany: 2))
			{
				var exceptionMessage = string.Format(CultureInfo.InvariantCulture, "A db reconnect was attempted while undisposed SqlLocks existed. Details below:\r\nSqlApplicationLock{{Key: '{0}', IsDisposed: {1}, WasAcquiredOnLastCheck: {2}}}\r\n", lock1.Key, lock1.IsDisposed, lock1.WasAcquiredOnLastCheck);
				exceptionMessage += string.Format(CultureInfo.InvariantCulture, "SqlApplicationLock{{Key: '{0}', IsDisposed: {1}, WasAcquiredOnLastCheck: {2}}}\r\n", lock2.Key, lock2.IsDisposed, lock2.WasAcquiredOnLastCheck);
				exceptionMessage += string.Format(CultureInfo.InvariantCulture, "SqlApplicationLock{{Key: '{0}', IsDisposed: {1}, WasAcquiredOnLastCheck: {2}}}\r\n", lock3.Key, lock3.IsDisposed, lock3.WasAcquiredOnLastCheck);

				AssertExceptionThrown("SqlApplicationLock information should be appened to excepetion message", typeof(SqlLockLostException), exceptionMessage,
					connection.EnsureNoUndisposedLocks, true);
			}
		}

		#endregion

		public void TestSPIDCoverage()
		{
			Assert(Db.Connection.SPID > 0);
		}

		public void TestLoginTime()
		{
			var loginTime = Db.Connection.LoginTime;
			Thread.Sleep(50);
			var dbServerTime = AdoTestUtils.DbServerTime(Db.Connection);
			Assert(loginTime > dbServerTime.AddDays(-1));
			Assert($"loginTime is {loginTime:dd/MM/yy H:mm:ss.fff}\tdbServerTime is {dbServerTime:dd/MM/yy H:mm:ss.fff}", loginTime < dbServerTime);
		}

		public void TestDefaultTimeOut()
		{
			int oldTimeOut = Db.Connection.DefaultCommandTimeOutInSeconds;

			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.BeginTransaction();
				try
				{
					Db.Connection.DefaultCommandTimeOutInSeconds = 1;

					extraConnection.ExecuteNonQuery("create table JunkTable (PK int)");

					bool gotException = false;
					Stopwatch watch = new Stopwatch();
					watch.Start();

					try
					{
						Db.Connection.ExecuteNonQuery("sp_tables"); // can't execute as the other connection locked sys.objects
					}
					catch (SqlException e)
					{
						gotException = (new DbErrorMatch(e).ExceptionType == DbErrorType.TimeoutExpired);
					}
					if (!gotException)
					{
						Fail("Didn't get SQL timeout exception");
					}

					double queryElapsedSeconds = watch.Elapsed.TotalSeconds;

					// Can't make it tight as SQL server load can vary this number a lot
					Assert(
						"Should have timed out after (1s + query run time), but timed out after: " + queryElapsedSeconds.ToString() +
						" seconds.", queryElapsedSeconds <= 30.0 && queryElapsedSeconds >= 0.95);
				}
				finally
				{
					Db.Connection.DefaultCommandTimeOutInSeconds = oldTimeOut;
					extraConnection.RollbackTransaction();
				}
			}
		}

		public void TestCommitTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					connection.BeginTransaction();
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					connection.CommitTransaction();

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Committed One Row", 1, result);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestCommitTransactionWithManager()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					using (var manager = connection.BeginTransactionWithManager())
					{
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
						manager.CommitTransaction();
					}

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Committed One Row", 1, result);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestTransactionManagerRollsbackAfterDispose()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					using (connection.BeginTransactionWithManager())
					{
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('123')");
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('DEF')");
					}

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Rolled Back 2 Rows", 1, result);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_DelaysCommit()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						delayedTransactionManager.CommitTransaction();
					}

					AssertEquals("5 rows commited", 5, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_HandlesDisconnectIfTransactionNotYetStarted()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						connection1.CloseConnection();

						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						delayedTransactionManager.CommitTransaction();
					}

					AssertEquals("5 rows commited", 5, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_DisconnectFailsTransactionOnceStarted()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						connection1.CloseConnection();
						AssertExceptionThrown<InvalidOperationException>(() => delayedTransactionManager.CommitTransaction());
					}

					AssertEquals("Updates should be rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_Nested_Delayed_In_Delayed()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalDelayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
						{
							using (var internalTransactionManager = connection1.BeginTransactionWithManager())
							{
								connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
								connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
								internalTransactionManager.CommitTransaction();
							}

							internalDelayedTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						delayedTransactionManager.CommitTransaction();
					}

					AssertEquals("5 rows commited", 5, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_Nested_Delayed_In_NonDelayed()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var transactionManager = connection1.BeginTransactionWithManager())
					{
						using (var internalDelayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
						{
							using (var internalTransactionManager = connection1.BeginTransactionWithManager())
							{
								connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
								connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
								internalTransactionManager.CommitTransaction();
							}

							internalDelayedTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						transactionManager.CommitTransaction();
					}

					AssertEquals("5 rows commited", 5, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_MultipleInternalSaves_OnlyOneTransaction()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('789')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('JKL')").ExecuteNonQuery();
						delayedTransactionManager.CommitTransaction();
					}

					AssertEquals("All commits should now be committed", 7, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_MultipleInternalSaves_OneRollback_AllRollback()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
						}

						result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.Command("INSERT INTO TestTransaction VALUES('789')").ExecuteNonQuery();
						connection1.Command("INSERT INTO TestTransaction VALUES('JKL')").ExecuteNonQuery();
						delayedTransactionManager.CommitTransaction();
					}

					AssertEquals("All commits should be rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_MultipleInternalSaves_OneRollback_AllRollback_ExplicitRollback()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
					{
						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						connection1.RollbackTransaction();

						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);
					}

					AssertEquals("All commits should be rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_MultipleInternalSaves_ExceptionRollsBackAll()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					try
					{
						connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
						using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
						{
							using (var internalTransactionManager = connection1.BeginTransactionWithManager())
							{
								connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
								connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
								internalTransactionManager.CommitTransaction();
							}

							int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
							AssertEquals("Only 1 row actually commited until outerscope", 1, result);

							using (var internalTransactionManager = connection1.BeginTransactionWithManager())
							{
								connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
								connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
								internalTransactionManager.CommitTransaction();
							}

							connection1.Command("INSERT INTO TestTransaction VALUES('789')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('JKL')").ExecuteNonQuery();

							throw new InvalidOperationException();
						}
					}
					catch (InvalidOperationException)
					{
					}

					AssertEquals("All commits should be rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionManager_MultipleInternalSaves_ExceptionRollsBackAll_Nested()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					try
					{
						connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
						using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
						{
							using (var internalTransactionManager = connection1.BeginTransactionWithManager())
							{
								connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
								connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
								internalTransactionManager.CommitTransaction();
							}

							int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
							AssertEquals("Only 1 row actually commited until outerscope", 1, result);

							using (var internalDelayedTransactionManager = connection1.DelayedTransactionWithManager(new DummyTransactionLockManager()))
							{
								using (var internalTransactionManager = connection1.BeginTransactionWithManager())
								{
									connection1.Command("INSERT INTO TestTransaction VALUES('456')").ExecuteNonQuery();
									connection1.Command("INSERT INTO TestTransaction VALUES('GHI')").ExecuteNonQuery();
									internalTransactionManager.CommitTransaction();
								}

								internalDelayedTransactionManager.CommitTransaction();
							}

							connection1.Command("INSERT INTO TestTransaction VALUES('789')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('JKL')").ExecuteNonQuery();

							throw new InvalidOperationException();
						}
					}
					catch (InvalidOperationException)
					{
					}

					AssertEquals("All commits should be rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
				}
			}
		}

		public void TestTransactionManagerRollsbackAfterDispose_WithAutoRollbackAction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					var success = true;
					using (connection.BeginTransactionWithManager(onAutoRollbackAction: () => success = false))
					{
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('123')");
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('DEF')");
					}

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Rolled Back 2 Rows", 1, result);
					Assert("Auto rollback action was not performed.", !success);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestRollbackTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");

					connection.BeginTransaction();
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('123')");
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('DEF')");
					connection.RollbackTransaction();

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Rolled Back 2 Rows", 1, result);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestRollbackTransactionOnException()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");

					connection.BeginTransaction();
					try
					{
						connection.ExecuteNonQuery("INSERT INTO #BadTableName VALUES('DEF')");
						connection.CommitTransaction();
					}
					catch (SqlException)
					{
						connection.RollbackTransaction();
					}

					int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
					AssertEquals("Rolled Back 1 Row", 1, result);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestIsInTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				Assert("Test Connection Is In Transaction", connection.IsInTransaction);
				connection.CommitTransaction();
				Assert("Test Connection Transaction Completed Successfully", !connection.IsInTransaction);

				connection.BeginTransaction();
				Assert("Test Connection Is In Transaction", connection.IsInTransaction);
				connection.RollbackTransaction();
				Assert("Test Connection Transaction Did Not Complete Successfully", !connection.IsInTransaction);
			}
		}

		public void TestNestedTransaction_InnerCommit()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				connection.BeginTransaction();
				connection.CommitTransaction();
				connection.RollbackTransaction();
				Assert("Test Connection Transaction Did Not Complete Successfully", !connection.IsInTransaction);
			}
		}

		[ExpectException(typeof(TransactionException))]
		public void TestNestedTransaction_InnerRolledBack()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				connection.BeginTransaction();
				connection.RollbackTransaction();
				connection.CommitTransaction();
			}
		}

		[ExpectNoExceptions()]
		public void TestNoRollback()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.RollbackTransaction();
			}
		}

		[ExpectException(typeof(TransactionException))]
		public void TestNoCommit()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.CommitTransaction();
			}
		}

		public void TestObjectID()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection.ObjectID != Db.Connection.ObjectID);
			}
		}

		public void TestInstantiationThread()
		{
			Thread t = new Thread(new ThreadStart(MakeNewConnection))
			{
				Name = "TestInstantiationThread",
				IsBackground = true
			};
			t.Start();
			t.Join();
			Assert(!Object.ReferenceEquals(ConnectionCreatedOnOtherThread.InstantiationThread, Db.Connection.InstantiationThread));
		}

		DbConnection ConnectionCreatedOnOtherThread;

		void MakeNewConnection()
		{
			ConnectionCreatedOnOtherThread = Db.NewExtraConnectionToMainDb();
			ConnectionCreatedOnOtherThread.Dispose();
		}

		[ExpectNoExceptions()]
		public void TestTransactionRolledbackByTrigger()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					string viewSql =
						"CREATE VIEW TestViewWithTrigger as SELECT [RN_PK] AS [PK], [RN_Code] AS [Code], [RN_Desc] AS [Description] FROM dbo.RefCountry";
					connection.ExecuteNonQuery(viewSql);
					string triggerSql =
						"CREATE TRIGGER TestViewTrigger ON TestViewWithTrigger INSTEAD OF INSERT AS BEGIN ROLLBACK; RAISERROR('ERROR', 1, 16); END";
					connection.ExecuteNonQuery(triggerSql);

					connection.BeginTransaction();

					try
					{
						string insertSql = "insert into TestViewWithTrigger values ('A292C6FA-0627-467C-96B9-8ED1E9653042', 'zX', 'desc')";
						connection.ExecuteNonQuery(insertSql);
						connection.CommitTransaction();
						Fail("Test Trigger is not throwing an error as expected");
					}
					catch (SqlException)
					{
						connection.RollbackTransaction();
					}
				}
				catch (SqlException e)
				{
					if (new DbErrorMatch(e).ExceptionType == DbErrorType.RollBackTranHasNoCorrespondingBeginTran)
					{
						Fail(
							"RollbackTransaction threw error because error in trigger rolled back the transaction automatically. Actual error is: " +
							e.ToString());
					}
					else
					{
						throw;
					}
				}
				finally
				{
					connection.ExecuteNonQuery("DROP TRIGGER TestViewTrigger");
					connection.ExecuteNonQuery("DROP VIEW TestViewWithTrigger");
					DbCommitTracker.Ignore("TestViewWithTrigger");
				}
			}
		}

		public void TestCommandWithNullTimeout()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				DbCommand command = connection.Command("select @@spid", null);
				AssertEquals(connection.DefaultCommandTimeOutInSeconds, command.CommandTimeout);
			}
		}

		public void TestCommandWithNonNullTimeout()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				DbCommand command = connection.Command("select @@spid", 123);
				AssertEquals(123, command.CommandTimeout);
			}
		}

		public void connectionState()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("Initially Closed", ConnectionState.Closed, connection.State);
				connection.EnsureIsOpen();
				AssertEquals("Open", ConnectionState.Open, connection.State);
				connection.CloseConnection();
				AssertEquals("Closed", ConnectionState.Closed, connection.State);
			}
		}

		public void TestTimeoutOverloadSetsTimeout()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				DbCommand command;

				command = connection.Command("SELECT 1", 1);
				AssertEquals("Timeout", 1, command.CommandTimeout);

				command = connection.Command("SELECT 1", 42);
				AssertEquals("Timeout", 42, command.CommandTimeout);
			}
		}

		public void TestSingleRefDatabaseExcluded()
		{
			var allDbs = (List<string>)Db.Connection.GetDatabases(DatabaseType.All & ~(DatabaseType.SingleSharedRef | DatabaseType.EDW));
			Assert(!allDbs.Contains(RefDbTableNameResolver.SingleRefDatabaseName));
		}

		public void TestGetAllDatabasesExceptEdw()
		{
			var allDbs = (List<string>)Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);
			Assert("Main DB is first DB", allDbs[0].Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));

			var operationalDbs = (List<string>)Db.Connection.GetDatabases(DatabaseType.Operational);
			Assert("Main DB is first operational DB", operationalDbs[0].Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));

			AssertEquals("All DB count > Operational DB count", true, allDbs.Count > operationalDbs.Count);

			int nonOperationalDbCount = 0;
			foreach (string db in allDbs)
			{
				if (db.Contains(RefDbTableNameResolver.RefDbAffix))
				{
					nonOperationalDbCount++;
				}
			}
			AssertNotEquals(0, nonOperationalDbCount);

			//Contains Single Ref Database
			if (Db.Connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName))
			{
				nonOperationalDbCount++;
			}

			// Contains User Repository database
			string userRepositoryDb = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;
			if (Db.Connection.DatabaseExists(userRepositoryDb))
			{
				allDbs.Contains(userRepositoryDb);
				nonOperationalDbCount++;
			}

			int biDatabases = 0;
			if (Db.Connection.DatabaseExists(Db.AuditDatabaseName))
			{
				biDatabases++;
			}

			AssertEquals(operationalDbs.Count + nonOperationalDbCount + biDatabases, allDbs.Count);
		}

		public void TestGetAllExclusiveDatabases()
		{
			string mockDb_Main = "MockDbTestGetAllExclusiveDatabases";
			string mockDb_Shared = "CW-RefDb-Xxx-ZZ-000000";
			string mockDb_Exclusive = mockDb_Main + "_RefDb_Aaa_BB";
			string mockDb_SD001 = mockDb_Main + "_SD001";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDb_Main);
				AdoTestUtils.CreateDbIfNotExists(mockDb_Exclusive);
				AdoTestUtils.CreateDbIfNotExists(mockDb_Shared, mockDb_Main);

				using (DbConnection conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDb_Main))
				{
					var exclusiveDbList01 = (List<string>)conn.GetDatabases(DatabaseType.AllExclusive & ~DatabaseType.BI);
					AssertEquals("No synonyms => should get exclusive DB list by name pattern", 2, exclusiveDbList01.Count);
					AssertEquals("Main DB is the 1st database", mockDb_Main, exclusiveDbList01[0]);
					AssertEquals("RefDb Aaa BB is the 2nd database", mockDb_Exclusive, exclusiveDbList01[1]);

					AdoTestUtils.CreateDbIfNotExists(mockDb_SD001);
					string sqlText = string.Format(@"
						CREATE SYNONYM [RefDbXxxZZ_SomeTable] FOR [{0}]..[SomeTable];
						CREATE SYNONYM [RefDbAaaBB_SomeTable] FOR [{1}]..[SomeTable];",
						mockDb_Shared, mockDb_Exclusive);
					conn.ExecuteNonQuery(sqlText);

					var exclusiveDbList02 = (List<string>)conn.GetDatabases(DatabaseType.AllExclusive & ~DatabaseType.BI);
					AssertEquals("Main DB + 2 other exclusive DBs", 3, exclusiveDbList02.Count);
					AssertEquals("Main DB is the first database", mockDb_Main, exclusiveDbList02[0]);
					AssertEquals("Second DB", mockDb_SD001, exclusiveDbList02[1]);
					AssertEquals("Third DB", mockDb_Exclusive, exclusiveDbList02[2]);

					// Clears reference database buffers and gets all database list again
					((IPhysicalRefDbLocation)conn).ClearRefDbNameBuffers();
					var allDbs = (List<string>)conn.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);

					AssertEquals("All DB count", 5, allDbs.Count);
					AssertEquals("Shared DB is the 4th database in all DB list", mockDb_Shared, allDbs[3]);

					foreach (string db in exclusiveDbList02)
					{
						if (RefDbTableNameResolver.IsSharedDatabase(db))
						{
							Fail(string.Format("Shared DB [{0}] should not have been returned by GetAllExclusiveDatabases()", db));
						}
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDb_Main);
				AdoTestUtils.DropDbIfExists(mockDb_Shared, mockDb_Main);
				AdoTestUtils.DropDbIfExists(mockDb_Exclusive);
				AdoTestUtils.DropDbIfExists(mockDb_SD001);
			}
		}

		public void TestGetOperationalRepositoryAndFullRecoveryDbs()
		{
			var mockDb_Main = "MockDbTestDatabase";
			var mockDb_Shared = "CW-RefDb-Xxx-ZZ-000000";
			var mockDb_SharedAG = "CW-AG-RefDb-AG-001-Xxx-AG-000000";
			var mockDb_Exclusive1 = mockDb_Main + "_RefDb_Aaa_AA";
			var mockDb_Exclusive2 = mockDb_Main + "_RefDb_Aaa_BB";
			var mockDb_Exclusive3 = mockDb_Main + "_RefDb_Aaa_CC";
			var mockDb_Exclusive4 = mockDb_Main + "_RefDb_Aaa_DD";
			var mockDb_SD001 = mockDb_Main + "_SD001";
			var mockDb_SD002 = mockDb_Main + "_SD002";
			var mockDb_UserRepository = mockDb_Main + "_UserRepository";

			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Main, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Shared, DbRecoveryModel.Full, mockDb_Main))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_SharedAG, DbRecoveryModel.Full, mockDb_Main))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Exclusive1, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Exclusive2, DbRecoveryModel.Simple))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Exclusive3, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Exclusive4, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_SD001, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_SD002, DbRecoveryModel.Full))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_UserRepository, DbRecoveryModel.Full))
			{
				using (var conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDb_Main))
				{
					conn.ExecuteNonQuery($@"
						CREATE SYNONYM [RefDbXxxZZ_SomeTable] FOR [{mockDb_Shared}]..[SomeTable];
						CREATE SYNONYM [RefDbXxxAG_SomeTable] FOR [{mockDb_SharedAG}]..[SomeTable];
						CREATE SYNONYM [RefDbAaaAA_SomeTable] FOR [{mockDb_Exclusive1}]..[SomeTable];
						CREATE SYNONYM [RefDbAaaBB_SomeTable] FOR [{mockDb_Exclusive2}]..[SomeTable];
						CREATE SYNONYM [RefDbAaaCC_SomeTable] FOR [{mockDb_Exclusive3}]..[SomeTable];
						CREATE SYNONYM [RefDbAaaDD_SomeTable] FOR [{mockDb_Exclusive4}]..[SomeTable];
						");

					var dbList = (List<string>)conn.GetOperationalRepositoryAndFullRecoveryDbs();
					AssertEquals("Main DB + 2 other exclusive DBs", 7, dbList.Count);
					AssertEquals("Main DB is the first database", mockDb_Main, dbList[0]);

					AssertEquals("Database [" + mockDb_Shared + "] exists on the list?", false, dbList.Contains(mockDb_Shared));
					AssertEquals("Database [" + mockDb_SharedAG + "] exists on the list?", false, dbList.Contains(mockDb_SharedAG));
					AssertEquals("Database [" + mockDb_Exclusive1 + "] exists on the list?", true, dbList.Contains(mockDb_Exclusive1));
					AssertEquals("Database [" + mockDb_Exclusive2 + "] exists on the list?", false, dbList.Contains(mockDb_Exclusive2));
					AssertEquals("Database [" + mockDb_Exclusive3 + "] exists on the list?", true, dbList.Contains(mockDb_Exclusive3));
					AssertEquals("Database [" + mockDb_Exclusive4 + "] exists on the list?", true, dbList.Contains(mockDb_Exclusive4));
					AssertEquals("Database [" + mockDb_SD001 + "] exists on the list?", true, dbList.Contains(mockDb_SD001));
					AssertEquals("Database [" + mockDb_SD002 + "] exists on the list?", true, dbList.Contains(mockDb_SD002));
					AssertEquals("Database [" + mockDb_UserRepository + "] exists on the list?", true, dbList.Contains(mockDb_UserRepository));
				}
			}
		}

		public void TestGetAllWriteableDatabases()
		{
			const string mockDbForTest = "MockDbTestGetAllWriteableDatabases";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001");
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD002");

				using (DbConnection conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDbForTest))
				{
					var dbList01 = (List<string>)conn.GetDatabases(DatabaseType.AllWritable, writable: true);
					AssertEquals("Only main DB exists", 1, dbList01.Count);
					AssertEquals("Main DB is first and only database", mockDbForTest, dbList01[0]);

					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + "_SD002");
					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + "_SD001");

					var dbList02 = (List<string>)conn.GetDatabases(DatabaseType.AllWritable, writable: true);
					AssertEquals("Main DB + 2 secondary DBs", 3, dbList02.Count);
					AssertEquals("Main DB is the first database", mockDbForTest, dbList02[0]);
					AssertEquals("Secondary DB (1)", mockDbForTest + "_SD001", dbList02[1]);
					AssertEquals("Secondary DB (2)", mockDbForTest + "_SD002", dbList02[2]);

					conn.AlterDbWriteableState(mockDbForTest + "_SD001", false);

					var dbList03 = (List<string>)conn.GetDatabases(DatabaseType.AllWritable, writable: true);
					AssertEquals("Main DB + 1 writeable secondary DB", 2, dbList03.Count);
					AssertEquals("Main DB is the first database", mockDbForTest, dbList03[0]);
					AssertEquals("Secondary DB SD002 is the 2nd database", mockDbForTest + "_SD002", dbList03[1]);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001");
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD002");
			}
		}

		public void TestGetAllWriteableDatabasesExcludesBiDatabases()
		{
			const string mockDbForTest = "MockDbTestGetAllWriteableDatabasesExclusiveBIDatabases";

			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDbForTest))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDbForTest + "_Audit"))
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDbForTest + "_EDW"))
			{
				using (DbConnection conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDbForTest))
				{
					var dbList = (List<string>)conn.GetDatabases(DatabaseType.AllWritable, writable: true);
					AssertEquals("Only main DB exists", 1, dbList.Count);
					AssertEquals("Main DB is first and only database", mockDbForTest, dbList[0]);
				}
			}
		}

		public void TestUseDatabaseWithoutDisposingIt()
		{
			using (DbConnection conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName))
			{
				MethodToAllowObjectToExitMemory(conn);
				AssertEquals(Db.SqlMasterDb, conn.CurrentDatabase);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				AssertEquals("DB Should NOT go back to original db", Db.SqlMasterDb, conn.CurrentDatabase);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void MethodToAllowObjectToExitMemory(DbConnection conn)
		{
			((ICurrentDbControl)conn).UseDatabase(Db.SqlMasterDb);
		}

		/// <summary>
		/// GetOperationalDatabaseList returns a string array with all operational databases,
		/// where the main DB _must_ be the first element of the array (i.e.: index 0).
		/// </summary>
		public void TestGetOperationalDatabases()
		{
			const string mockDbForTest = "MockDbTestGetOperationalDatabases";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest, Db.DatabaseName);

				using (var conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, mockDbForTest))
				{
					// sets current database to master so the default one can be dropped/recreated
					((ICurrentDbControl)conn).UseDatabase(Db.SqlMasterDb);

					AdoTestUtils.DropDbIfExists(mockDbForTest, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001", Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD002", Db.DatabaseName);

					AssertEquals("Mock DB does not exist", 0, ((List<string>)conn.GetDatabases(DatabaseType.Operational)).Count);

					AdoTestUtils.CreateDbIfNotExists(mockDbForTest);
					var dbList01 = (List<string>)conn.GetDatabases(DatabaseType.Operational);
					AssertEquals("Only main DB exists", 1, dbList01.Count);
					AssertEquals("Main DB exists", mockDbForTest, dbList01[0]);

					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + "_SD002", Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + "_SD001", Db.DatabaseName);

					var dbList02 = (List<string>)conn.GetDatabases(DatabaseType.Operational);
					AssertEquals("Main DB + 2 secondary DBs", 3, dbList02.Count);
					AssertEquals("Main DB exists and is the first DB in the list", mockDbForTest, dbList02[0]);
					AssertEquals("Secondary DB (1)", mockDbForTest + "_SD001", dbList02[1]);
					AssertEquals("Secondary DB (2)", mockDbForTest + "_SD002", dbList02[2]);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001", Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD002", Db.DatabaseName);
			}
		}

		public void TestGetOperationalAndRepositoryDatabases()
		{
			const string mockDbForTest = "MockDbTestGetOperationalAndRepositoryDatabases";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001");
				AdoTestUtils.DropDbIfExists(mockDbForTest + DbUserRepository.RepositoryDbSuffix);

				using (DbConnection conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDbForTest))
				{
					var dbList01 = (List<string>)conn.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository);
					AssertEquals("Only main DB exists", 1, dbList01.Count);
					AssertEquals("Main DB exists", mockDbForTest, dbList01[0]);

					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + "_SD001");
					AdoTestUtils.CreateDbIfNotExists(mockDbForTest + DbUserRepository.RepositoryDbSuffix);

					var operationDbOnlyList = (List<string>)conn.GetDatabases(DatabaseType.Operational);
					AssertEquals("Main DB + SD001", 2, operationDbOnlyList.Count);
					AssertEquals("Main DB is the first database", mockDbForTest, operationDbOnlyList[0]);
					AssertEquals("Second DB", mockDbForTest + "_SD001", operationDbOnlyList[1]);

					var operationalAndRepositoryDbList = (List<string>)conn.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository);
					AssertEquals("Main DB + SD001 + UserRepository", 3, operationalAndRepositoryDbList.Count);
					AssertEquals("Main DB is the first database", mockDbForTest, operationalAndRepositoryDbList[0]);
					AssertEquals("Second DB", mockDbForTest + "_SD001", operationalAndRepositoryDbList[1]);
					AssertEquals("Third DB", mockDbForTest + DbUserRepository.RepositoryDbSuffix, operationalAndRepositoryDbList[2]);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(mockDbForTest + "_SD001");
				AdoTestUtils.DropDbIfExists(mockDbForTest + DbUserRepository.RepositoryDbSuffix);
			}
		}

		public void TestGetDocManagerDbList()
		{
			var db_Main = "MockDbForDbConnectionTest";
			var db_SD001 = db_Main + "_SD001";
			var db_SD002 = db_Main + "_SD002";
			var db_SD003 = db_Main + "_SD003";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(db_Main, Db.DatabaseName);

				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, db_Main))
				{
					// sets current database to master so the default one can be dropped/recreated
					((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb);

					AdoTestUtils.DropDbIfExists(db_Main, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(db_SD001, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(db_SD002, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(db_SD003, Db.DatabaseName);

					AssertEquals("Mock DB does not exist => Operational database count:", 0, connection.GetDatabases(DatabaseType.Operational).Count());
					AssertEquals("eDocs DB count", 0, ((List<string>)connection.GetDatabases(DatabaseType.SD)).Count);

					AdoTestUtils.CreateDbIfNotExists(db_Main);
					var expected = new string[] { db_Main, };
					AssertContainsExactElementsInAnyOrder("Only main DB exists", expected, connection.GetDatabases(DatabaseType.Operational));
					AssertEquals("There are NO DocManager DBs", 0, connection.GetDatabases(DatabaseType.SD).Count());

					AdoTestUtils.CreateDbIfNotExists(db_SD001, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(db_SD002, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(db_SD003, Db.DatabaseName);
					connection.AlterDbWriteableState(db_SD003, false);

					expected = new string[] { db_Main, db_SD001, db_SD002, db_SD003, };
					AssertContainsExactElementsInAnyOrder("Main DB + 3 secondary DBs", expected, connection.GetDatabases(DatabaseType.Operational));

					expected = new string[] { db_SD001, db_SD002, db_SD003, };
					AssertContainsExactElementsInAnyOrder("3 DocManager DBs", expected, connection.GetDatabases(DatabaseType.SD));
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(db_Main, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(db_SD001, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(db_SD002, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(db_SD003, Db.DatabaseName);
			}
		}

		public void TestGetAllReferenceDatabases()
		{
			using (DbConnection conn = Db.NewExtraConnectionToMainDb())
			{
				var dbList = (List<string>)conn.GetDatabases(DatabaseType.ExclusiveRefOrSharedRef);

				AssertEquals("List has some ref DBs?", true, dbList.Count > 0);
				AssertEquals("1st DB = Main DB? " + dbList[0], false, dbList[0].Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));

				for (int i = 0; i < dbList.Count; i++)
				{
					AssertEquals("All DBs are exclusive Ref DBs? " + dbList[i], true,
						dbList[i].StartsWith(Db.DatabaseName + "_" + RefDbTableNameResolver.RefDbAffix, StringComparison.OrdinalIgnoreCase));
				}

				const string testSharedDbName = "CW-RefDb-Xxx-ZZ-000000";
				const string testSharedAvailabilityGroupDbName = "CW-AG-RefDb-ORDWP4-CP1AS1-Xxx-UA-000000";
				int exclusiveDbCount = dbList.Count;

				using (AdoTestUtils.CreateDbDropExistingDisposable(testSharedDbName, Db.DatabaseName))
				using (AdoTestUtils.CreateDbDropExistingDisposable(testSharedAvailabilityGroupDbName, Db.DatabaseName))
				{
					conn.BeginTransaction();

					string sqlText = string.Format(@"
					CREATE SYNONYM [RefDbXxxYY_SomeTable] FOR [CW-RefDb-Xxx-YY-000000]..[SomeTable];
					CREATE SYNONYM [RefDbXxxZZ_SomeTable] FOR [{0}]..[SomeTable];",
						testSharedDbName);
					conn.ExecuteNonQuery(sqlText);
					conn.ExecuteNonQuery($"CREATE SYNONYM [RefDbXxxUA_SomeTable] FOR [{testSharedAvailabilityGroupDbName}]..[SomeTable];");

					// Clears reference database buffers and gets all database list again
					((IPhysicalRefDbLocation)conn).ClearRefDbNameBuffers();
					dbList = (List<string>)conn.GetDatabases(DatabaseType.ExclusiveRefOrSharedRef);
					AssertEquals("List count", exclusiveDbCount + 2, dbList.Count);
					AssertEquals("Shared DB Name", testSharedDbName, dbList[dbList.Count - 1]);
					AssertEquals("Shared DB Name", testSharedAvailabilityGroupDbName, dbList[dbList.Count - 2]);
				}
			}
		}

		public void TestGetReferenceDatabaseName()
		{
			IPhysicalRefDbLocation refDbLocation = Db.Connection;
			refDbLocation.ClearRefDbNameBuffers();

			AssertEquals("single reference database", RefDbTableNameResolver.SingleRefDatabaseName, refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Single, ""));

			AssertEquals("EntCA reference database", Db.DatabaseName + "_RefDb_Ent_CA", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "CA"));
			AssertEquals("TrfNZ reference database", Db.DatabaseName + "_RefDb_Trf_NZ", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ"));

			AssertNull("CmrZZ reference database", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "ZZ"));
			AssertNull("EntUA reference database", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "UA"));

			const string testSharedDbName = "CW-RefDb-Cmr-ZZ-000000";
			const string testSharedAvailabilityGroupDbName = "CW-AG-RefDb-ORDWP4-CP1AS1-Ent-UA-000000";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(testSharedDbName, Db.DatabaseName);
				AdoTestUtils.CreateDbIfNotExists(testSharedAvailabilityGroupDbName, Db.DatabaseName);
				Db.Connection.BeginTransaction();

				string sqlText = string.Format("CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{0}]..[SomeTable]", testSharedDbName);
				Db.Connection.ExecuteNonQuery(sqlText);
				Db.Connection.ExecuteNonQuery($"CREATE SYNONYM [RefDbEntUA_SomeTable] FOR [{testSharedAvailabilityGroupDbName}]..[SomeTable]");

				AssertEquals("CmrZZ reference database", testSharedDbName, refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "ZZ"));
				AssertEquals("EntUA reference database", testSharedAvailabilityGroupDbName, refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "UA"));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
				AdoTestUtils.DropDbIfExists(testSharedDbName, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(testSharedAvailabilityGroupDbName, Db.DatabaseName);
			}

			// Once set to a non-null value the DB name is returned from the cache, even after it's dropped.
			AssertEquals("CmrZZ reference database", testSharedDbName, refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "ZZ"));
			AssertEquals("EntUA reference database", testSharedAvailabilityGroupDbName, refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "UA"));

			// Clears the cache => should return null for the dropped database
			refDbLocation.ClearRefDbNameBuffers();
			AssertNull("CmrZZ reference database", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "ZZ"));
			AssertNull("EntUA reference database", refDbLocation.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "UA"));
		}

		public void TestLoadRefDbNameFromSynonym()
		{
			AssertEquals("TrfCA reference database", Db.DatabaseName + "_RefDb_Trf_CA", ((IPhysicalRefDbLocation)Db.Connection).LoadRefDbNameFromSynonym(Db.DatabaseName, RefDbTypeEnum.Tariff, "CA"));
			AssertNull("CmrZZ reference database", ((IPhysicalRefDbLocation)Db.Connection).LoadRefDbNameFromSynonym(Db.DatabaseName, RefDbTypeEnum.Customs, "ZZ"));

			const string mockDbForTest = "MockDbTestLoadRefDbNameFromSynonym";
			const string testExclusiveDbName = mockDbForTest + "_RefDb_Ent_YY";
			const string testSharedDbName = "CW-RefDb-Cmr-ZZ-000000";
			AdoTestUtils.DropDbIfExists(mockDbForTest);

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest);
				AdoTestUtils.CreateDbIfNotExists(testSharedDbName, Db.DatabaseName);
				AdoTestUtils.CreateDbIfNotExists(testExclusiveDbName);

				using (DbConnection conn = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mockDbForTest))
				{
					IPhysicalRefDbLocation refDbLocation = conn;

					// RefDbs were created but there synonyms in the test database referecing it
					AssertNull("CmrZZ reference database", refDbLocation.LoadRefDbNameFromSynonym(mockDbForTest, RefDbTypeEnum.Customs, "ZZ"));
					AssertNull("EntYY reference database", refDbLocation.LoadRefDbNameFromSynonym(mockDbForTest, RefDbTypeEnum.Enterprise, "YY"));

					// Synonym referencing exclusive EntZA DB created but its name doesn't match the base DB
					string sqlText = string.Format("CREATE SYNONYM [RefDbXxxYY_SomeTable] FOR [{0}]..[SomeTable]", testExclusiveDbName);
					conn.ExecuteNonQuery(sqlText);
					AssertNull("EntYY reference database", refDbLocation.LoadRefDbNameFromSynonym(mockDbForTest, RefDbTypeEnum.Enterprise, "YY"));

					// Matching synonym referencing shared CmrZZ DB created
					sqlText = string.Format("CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{0}]..[SomeTable]", testSharedDbName);
					conn.ExecuteNonQuery(sqlText);
					AssertEquals("CmrZZ reference database", testSharedDbName, refDbLocation.LoadRefDbNameFromSynonym(mockDbForTest, RefDbTypeEnum.Customs, "ZZ"));

					// Matching synonym referencing exclusive EntYY DB created
					sqlText = string.Format("CREATE SYNONYM [RefDbEntYY_SomeTable] FOR [{0}]..[SomeTable]", testExclusiveDbName);
					conn.ExecuteNonQuery(sqlText);
					AssertEquals("EntYY reference database", testExclusiveDbName, refDbLocation.LoadRefDbNameFromSynonym(mockDbForTest, RefDbTypeEnum.Enterprise, "YY"));
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest);
				AdoTestUtils.DropDbIfExists(testExclusiveDbName);
				AdoTestUtils.DropDbIfExists(testSharedDbName, mockDbForTest);
			}
		}

		public void TestOpenConnectionWithSplashInfo_DoesNotRetrieveGuiElementsIfNotGuiTest()
		{
			var connection = new ConnectionWithExposedProtectdMembersForTest();
			connection.OpenConnectionWithSplashInfo_Exposed();

			AssertEquals(false, connection.NewConnectingSplashFormManagerWasAccessed);
		}
		public void TestOpenConnectionWithSplashInfo_RetrievesGuiElementsIfSpecified()
		{
			DbConnection.ShowSplashFormForTests = true;
			var connection = new ConnectionWithExposedProtectdMembersForTest();
			connection.OpenConnectionWithSplashInfo_Exposed();

			AssertEquals(true, connection.NewConnectingSplashFormManagerWasAccessed);
		}

		public void TestDbSuffixByName()
		{
			var mainDbName = Db.Connection.CurrentDatabase;

			AssertEquals(null, Db.Connection.DbSuffixByName(null));
			AssertEquals(null, Db.Connection.DbSuffixByName("   "));
			AssertEquals(string.Empty, Db.Connection.DbSuffixByName(mainDbName));
			AssertEquals("_SD001", Db.Connection.DbSuffixByName(mainDbName + "_SD001"));
			AssertEquals("CW-RefDb-Cmr-AU-000007", Db.Connection.DbSuffixByName("CW-RefDb-Cmr-AU-000007"));
			AssertEquals(null, Db.Connection.DbSuffixByName(mainDbName + "ABCD"));
		}

		public void TestDbNameBySuffix()
		{
			var mainDbName = Db.Connection.CurrentDatabase;

			AssertEquals(mainDbName, Db.Connection.DbNameBySuffix(null));
			AssertEquals(mainDbName, Db.Connection.DbNameBySuffix("   "));
			AssertEquals(mainDbName + "_SD001", Db.Connection.DbNameBySuffix("_SD001"));
			AssertEquals("CW-RefDb-Cmr-AU-000007", Db.Connection.DbNameBySuffix("CW-RefDb-Cmr-AU-000007"));
			AssertEquals(null, Db.Connection.DbNameBySuffix("ABCD"));
		}

		public void TestDbSuffixAndDbName()
		{
			using (var connection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, Db.Connection.CurrentDatabase))
			{
				var dbNames = connection.GetDatabases(DatabaseType.Operational | DatabaseType.ExclusiveRefOrSharedRef).ToArray();
				foreach (var dbName in dbNames)
				{
					if (dbName != RefDbTableNameResolver.SingleRefDatabaseName)
					{
						var suffix = connection.DbSuffixByName(dbName);
						AssertNotNull(suffix);
						Assert(connection.DbNameBySuffix(suffix).Equals(dbName, StringComparison.OrdinalIgnoreCase));
					}
				}
			}
		}

		public void TestIsDbWriteable()
		{
			string mockDbForTest = "MockDbForDbConnectionTest";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(mockDbForTest);

				using (DbConnection conn = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					conn.AlterDbWriteableState(mockDbForTest, false);
					AssertEquals("Is database writeable?", false, conn.IsDbWriteable(mockDbForTest));

					conn.AlterDbWriteableState(mockDbForTest, true);
					AssertEquals("Is database writeable", true, conn.IsDbWriteable(mockDbForTest));
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(mockDbForTest);
			}

			AssertEquals("Is non-existent database writeable?", false, Db.Connection.IsDbWriteable(mockDbForTest));
		}

		public void TestDbAutoCreateStats()
		{
			string testDb = "MockDbForDbConnectionTest";

			try
			{
				AdoTestUtils.CreateDbIfNotExists(testDb);

				using (DbConnection conn = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					conn.AlterDbAutoCreateStats(testDb, false);
					AssertEquals("Auto create statistics?", false, conn.IsDbAutoCreateStats(testDb));

					conn.AlterDbAutoCreateStats(testDb, true);
					AssertEquals("Auto create statistics?", true, conn.IsDbAutoCreateStats(testDb));
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(testDb);
			}

			AssertEquals("Is non-existent database writeable?", false, Db.Connection.IsDbWriteable(testDb));
		}

		public void TestUseDatabase()
		{
			AssertEquals(Db.DatabaseName, Db.Connection.CurrentDatabase);
			using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.SqlMasterDb))
			{
				AssertEquals(Db.SqlMasterDb, Db.Connection.CurrentDatabase);
				using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
				{
					Assert(Db.Connection.CurrentDatabase.Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));
				}
				AssertEquals(Db.SqlMasterDb, Db.Connection.CurrentDatabase);
			}
			Assert(Db.DatabaseName, Db.Connection.CurrentDatabase.Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));
		}

		public void TestUseMasterDb()
		{
			AssertEquals(Db.DatabaseName, Db.Connection.CurrentDatabase);
			using (Db.Connection.UseMasterDb())
			{
				AssertEquals(Db.SqlMasterDb, Db.Connection.CurrentDatabase);
			}
			Assert(Db.DatabaseName, Db.Connection.CurrentDatabase.Equals(Db.DatabaseName, StringComparison.OrdinalIgnoreCase));
		}

		public void TestDatabaseDataFiles()
		{
			string[] dataFiles = Db.Connection.GetDBDataFiles(Db.DatabaseName);
			AssertEquals("DATA files found?", true, dataFiles.Length >= 1);
			AssertEquals("Is there a DATA file with extension (.mdf)?", true, dataFiles.Any(f => f.Trim().EndsWith(".mdf", StringComparison.OrdinalIgnoreCase)));
		}

		public void TestDatabaseLogFiles()
		{
			string[] logFiles = Db.Connection.GetDBLogFiles(Db.DatabaseName);
			AssertEquals("Number of LOG files", 1, logFiles.Length);
			AssertEquals("LOG File extension (.ldf)?", true, logFiles[0].Trim().EndsWith(".ldf", StringComparison.OrdinalIgnoreCase));
		}

		public void TestDatabaseFiles_InvalidDatabase()
		{
			AssertEquals("Invalid DB Name, so no files returned", 0, Db.Connection.GetDBLogFiles("hallaballoza").Length);
		}

		public void TestServerName()
		{
			AssertEquals(Db.ServerName, Db.Connection.ServerName);
		}

		public void TestServerNameReportedByDatabase()
		{
			AssertEquals("Server name should == Environment.MachineName",
				System.Environment.MachineName, Db.Connection.ServerNameReportedByDatabase.Split('\\')[0]);
		}

		public void TestGetServerInstanceName()
		{
			AssertEquals("", DbConnection.GetServerInstanceName("XB268"));
			AssertEquals("", DbConnection.GetServerInstanceName("XB268\\"));
			AssertEquals(" ", DbConnection.GetServerInstanceName("XB268\\ "));
			AssertEquals("Instance", DbConnection.GetServerInstanceName("XB268\\Instance"));
		}

		public void TestGetServerNameWithoutInstance()
		{
			AssertEquals("XB268", DbConnection.GetServerNameWithoutInstance("XB268"));
			AssertEquals("XB268", DbConnection.GetServerNameWithoutInstance("XB268\\"));
			AssertEquals("XB268", DbConnection.GetServerNameWithoutInstance("XB268\\ "));
			AssertEquals("XB268", DbConnection.GetServerNameWithoutInstance("XB268\\Instance"));
		}

		public void TestSqlServerEdition()
		{
			string editionText = Db.Connection.ExecuteScalar("SELECT SERVERPROPERTY('Edition')").ToString();

			DbConnection.SqlServerEdition expectedServerEdition;

			if (
				editionText.StartsWith("Enterprise", StringComparison.OrdinalIgnoreCase)
				|| editionText.StartsWith("Developer", StringComparison.OrdinalIgnoreCase))
			{
				expectedServerEdition = DbConnection.SqlServerEdition.EnterpriseDeveloper;
			}
			else if (
				editionText.StartsWith("Standard", StringComparison.OrdinalIgnoreCase)
				|| editionText.StartsWith("Workgroup", StringComparison.OrdinalIgnoreCase)
				|| editionText.StartsWith("Web", StringComparison.OrdinalIgnoreCase)
				|| editionText.StartsWith("Business Intelligence", StringComparison.OrdinalIgnoreCase))
			{
				expectedServerEdition = DbConnection.SqlServerEdition.StandardWorkgroup;
			}
			else
			{
				expectedServerEdition = DbConnection.SqlServerEdition.Express;
			}

			AssertEquals("ServerEdition", expectedServerEdition, Db.Connection.ServerEdition);
		}

		public void TestSqlSessionProperties()
		{
			using (DbConnection conn = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("ANSI_NULLS", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_NULLS')"));
				AssertEquals("ANSI_PADDING", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_PADDING')"));
				AssertEquals("ANSI_WARNINGS", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_WARNINGS')"));
				AssertEquals("CONCAT_NULL_YIELDS_NULL", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('CONCAT_NULL_YIELDS_NULL')"));
				AssertEquals("NUMERIC_ROUNDABORT", 0, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('NUMERIC_ROUNDABORT')"));
				AssertEquals("QUOTED_IDENTIFIER", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('QUOTED_IDENTIFIER')"));
				AssertEquals("ARITHABORT", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ARITHABORT')"));

				string sqlText = @"
					SET ANSI_WARNINGS OFF;
					SET NUMERIC_ROUNDABORT ON;
					SET QUOTED_IDENTIFIER OFF;
					SET ARITHABORT OFF;";
				conn.ExecuteNonQuery(sqlText);

				AssertEquals("ANSI_WARNINGS after setting", 0, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_WARNINGS')"));
				AssertEquals("NUMERIC_ROUNDABORT after setting", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('NUMERIC_ROUNDABORT')"));
				AssertEquals("QUOTED_IDENTIFIER after setting", 0, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('QUOTED_IDENTIFIER')"));
				AssertEquals("ARITHABORT", 0, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ARITHABORT')"));

				AdoTestUtils.KillConnection(conn);

				AssertEquals("ANSI_NULLS after reconnect", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_NULLS')"));
				AssertEquals("ANSI_PADDING after reconnect", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_PADDING')"));
				AssertEquals("ANSI_WARNINGS after reconnect", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ANSI_WARNINGS')"));
				AssertEquals("CONCAT_NULL_YIELDS_NULL after reconnect", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('CONCAT_NULL_YIELDS_NULL')"));
				AssertEquals("NUMERIC_ROUNDABORT after reconnect", 0, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('NUMERIC_ROUNDABORT')"));
				AssertEquals("QUOTED_IDENTIFIER after reconnect", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('QUOTED_IDENTIFIER')"));
				AssertEquals("ARITHABORT", 1, (int)conn.ExecuteScalar("SELECT SESSIONPROPERTY ('ARITHABORT')"));
			}
		}

		[UseSnapshotProtection]
		public void TestOpenConnection_WhenDbLockoutWithLoginsEnabled_ThrowsUpgradeInProgress()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				DbLockout.AcquireLockout(adminConnection, lockoutReason: LockoutReason.Upgrade, disableLogins: false);

				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() =>
				{
					using (var newConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
					{
						newConnection.EnsureIsOpen();
					}
				});
			}
		}

		[UseSnapshotProtection]
		public void TestNoStackOverflowOnAttemptedReconnectAfterSchemaChange_WithDisableLogins()
			=> AssertNoStackOverflowOnAttemptedReconnectAfterSchemaChange(disableLogins: true);

		[UseSnapshotProtection]
		public void TestNoStackOverflowOnAttemptedReconnectAfterSchemaChange_WithoutDisableLogins()
			=> AssertNoStackOverflowOnAttemptedReconnectAfterSchemaChange(disableLogins: false);

		void AssertNoStackOverflowOnAttemptedReconnectAfterSchemaChange(bool disableLogins)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var actualDbEnv = DbEnv.Instance;
				var databaseMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
				DbEnv.SetDbEnvironment(new BaseDbEnvironment());
				try
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout(LockoutReason.Upgrade, disableLogins: disableLogins));
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.BeginTransaction());
						Db.Connection.CloseConnection();
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(databaseMajorSchemaVersion + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}
					AssertExceptionThrown(typeof(DatabaseUpgradedException), () => Db.Connection.BeginTransaction());
				}
				finally
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(databaseMajorSchemaVersion, adminConnection);
					Db.Connection.ResetDatabaseUpgradedExceptionHasBeenThrown();
					DbEnv.SetDbEnvironment(actualDbEnv);
				}
			}
		}

		public void TestHandleErrorAtMostOnceAfterConnectionFailed()
		{
			// Arrange
			var executeCountOfEnsureIsOpen = 0;

			var internalConnection = new Mock<System.Data.Common.DbConnection>();

			var internalConnectionState = false;
			internalConnection.Setup(x => x.Open()).Throws(CreateGeneralNetworkErrorSqlException);
			internalConnection.Setup(x => x.State).Returns(() => internalConnectionState ? ConnectionState.Open : ConnectionState.Closed);
			internalConnection.Setup(x => x.Close()).Callback(() => internalConnectionState = false);

			var dataProviderFactory = new Mock<IDataProviderFactory>();
			dataProviderFactory.Setup(x => x.NewDbCommand(It.IsAny<string>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
				.Returns<string, IDbConnection, IDbTransaction>((commandText, databaseConnection, transaction) =>
				{
					var cmd = new Mock<IDbCommand>();
					cmd.SetupGet(x => x.CommandText).Returns(commandText);

					var parameters = new Mock<IDataParameterCollection>();
					parameters.Setup(x => x.GetEnumerator()).Returns(() =>
					{
						var enumerator = new Mock<IEnumerator>();
						enumerator.Setup(x => x.MoveNext()).Returns(false);
						return enumerator.Object;
					});

					cmd.SetupGet(x => x.Parameters).Returns(parameters.Object);

					if (commandText == "--Connection.EnsureIsOpen")
					{
						++executeCountOfEnsureIsOpen;
					}

					cmd.Setup(x => x.ExecuteNonQuery()).Throws(CreateGeneralNetworkErrorSqlException);

					return cmd.Object;
				});

			var connection = new Mock<DbConnection>(dataProviderFactory.Object, "whatever-server", "whatever-db", null, null) { CallBase = true };
			connection.Protected().Setup("RunTasksAfterOpenConnection");

			// A non-null DbConnection.internalConnectionUnsafe is required to create DbCommand, otherwise HandleError will be called recursively and cause StackOverflowException.
			// DbConnection.OpenNewDbConnection MUST be set up well in order to get a non-null DbConnection.internalConnectionUnsafe.
			connection.Protected().Setup<IDbConnection>("OpenNewDbConnection")
				.Returns(internalConnection.Object)
				.Callback(() => internalConnectionState = true);
			connection.Setup(x => x.HandleError(It.IsAny<SqlException>()))
				.Callback(connection.Object.EnsureIsOpen); // Invoke EnsureIsOpen() recursively
			connection.Object.OpenRetries = 1;

			// Act
			// Assert
			var sqlException = AssertExceptionThrown<SqlException>(connection.Object.EnsureIsOpen); // Second call which will have connection issue and then HandleError will be called eventually
			AssertEquals("EnsureIsOpen is called for 2 times", sqlException.Message); // The 2rd one is called by HandleError, and we should not enter HandleError again
			AssertEquals(2, executeCountOfEnsureIsOpen);

			System.Data.Common.DbException CreateGeneralNetworkErrorSqlException()
			{
				// To make DbConnection.CloseAndReopenConnection called due to connection issue
				return SqlExceptionBuilder.CreateSqlException(10054, $"EnsureIsOpen is called for {executeCountOfEnsureIsOpen} times");
			}
		}

		[UseSnapshotProtection]
		public void TestCaughtUpgradeExceptionReportsErrorOnReconnect()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			using (var adminConnection = Db.NewAdminConnection())
			{
				var initialDbEnv = DbEnv.Instance;
				extraConnection.EnsureIsOpen();
				try
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					AssertExceptionThrown(typeof(DatabaseUpgradedException), () => FirstDatabaseUpgradeExceptionMethod(extraConnection));

					var mockGuiPlugin = new Mock<IDbConnectionGuiPlugin>();
					var mockDbEnv = new Mock<BaseDbEnvironment>() { CallBase = true };
					mockDbEnv.Setup(x => x.ConnectionGuiPlugin).Returns(mockGuiPlugin.Object);
					mockGuiPlugin.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>())).Verifiable();
					DbEnv.SetDbEnvironment(mockDbEnv.Object);

					extraConnection.BeginTransaction();
					mockGuiPlugin.Verify(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>()), Times.Once());
					AssertEquals("DatabaseUpgradeException has been thrown multiple times.\r\nCheck the call stack for any exception handling that swallows a DatabaseUpgradeException.", ErrorReporter.LastExceptionReported.Message);
					AssertContains("FirstDatabaseUpgradeExceptionMethod", ((IWithRootCauseStackTrace)ErrorReporter.LastExceptionReported).RootCauseStackTrace.ToString());
				}
				finally
				{
					DbEnv.SetDbEnvironment(initialDbEnv);
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) - 1, adminConnection);
					ErrorReporter.Clear();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestThrowsUpgradeExceptionOnNewConnectionIfSchemaVersionIsDifferent()
		{
			// Arrange
			using (var adminConnection = Db.NewAdminConnection())
			{
				var originalSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
				try
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(originalSchemaVersion + 1, adminConnection);
					AssertEquals("Precondition: Schema version increased", originalSchemaVersion + 1, DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection));

					// Act
					// Assert
					AssertExceptionThrown<DatabaseUpgradedException>(() =>
					{
						using (var connection = Db.NewExtraConnectionToMainDb())
						{
							connection.EnsureIsOpen();
						}
					});
				}
				finally
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(originalSchemaVersion, adminConnection);
					ErrorReporter.Clear();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecordDatabaseUpgradedExceptionAndContinue()
		{
			try
			{
				var connectionPoolingMock = new Mock<IConnectionPooling>();
				var dbConnectionGuiPluginMock = new Mock<IDbConnectionGuiPlugin>();
				var dbEnvMock = new Mock<BaseDbEnvironment>();
				dbEnvMock.SetupGet(x => x.ConnectionGuiPlugin).Returns(dbConnectionGuiPluginMock.Object);
				dbEnvMock.SetupGet(x => x.ConnectionPooling).Returns(connectionPoolingMock.Object);
				dbConnectionGuiPluginMock.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>())).Verifiable();

				using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock.Object))
				{
					// Arrange
					Db.Connection.EnsureIsOpen();
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.ExecuteNonQuery("kill " + Db.Connection.SPID);
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}

					AssertEquals(false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);

					// Act
					AssertExceptionThrown<DatabaseUpgradedException>(() => Db.Connection.EnsureIsOpen());

					// Assert
					AssertEquals(true, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
					AssertEquals(ConnectionState.Closed, Db.Connection.State);
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);

					// Act
					AssertNoExceptionThrown(() => Db.Connection.EnsureIsOpen());

					// Assert
					dbConnectionGuiPluginMock.Verify();
					Assert(ErrorReporter.LastExceptionReported is DatabaseUpgradeExceptionCaughtException);
					AssertContains($"{nameof(DatabaseUpgradeException)} has been thrown multiple times", ErrorReporter.LastExceptionReported.Message);
				}
			}
			finally
			{
				Db.Connection.ResetDatabaseUpgradedExceptionHasBeenThrown();
				ErrorReporter.Clear();
			}
		}

		void FirstDatabaseUpgradeExceptionMethod(DbConnection connection)
		{
			connection.BeginTransaction();
		}

		public void TestUpgradeExceptionThrownTwiceOnExtraConnection_ErrorReportIsSent()
		{
			// Mock the database has been upgraded.
			var currentVersion = GlobalServiceProvider.Instance.GetRequiredService<IDatabaseAspectVersions>();
			var versionMock = Mock.Of<IDatabaseAspectVersions>(x => x.SchemaVersion == new VersionLabel(currentVersion.SchemaVersion.Major + 1, 0));
			var originalServiceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
			mockServiceProvider.Setup(x => x.GetService(typeof(IDatabaseAspectVersions))).Returns(versionMock);

			// Ensure ErrorReporter has to open Db.Connection.
			Db.Connection.CloseConnection();
			var mockErrorReporter = new Mock<IErrorReporter>();
			mockErrorReporter.Setup(x => x.Report(It.IsAny<string>(), "", It.IsAny<DatabaseUpgradeExceptionCaughtException>()))
				.Callback(() => Db.Connection.EnsureIsOpen());

			using (ErrorReporter.SetTemporaryInstanceForTest(mockErrorReporter.Object))
			using (DbEnv.SetTemporaryDbEnvironment(new DbEnvironmentWithMockGuiPluginForTest()))
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertExceptionThrown<DatabaseUpgradedException>("PRE", () => extraConnection.EnsureIsOpen());
				AssertEquals("PRE", ConnectionState.Closed, Db.Connection.State);

				AssertNoExceptionThrown(() => extraConnection.EnsureIsOpen());
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				Assert(ErrorReporter.LastExceptionReported is DatabaseUpgradeExceptionCaughtException);
				AssertEquals(ConnectionState.Open, Db.Connection.State);
			}

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestExtraConnectionsCheckSchemaChangeOnReconnect_WithDisableLogins()
			=> AssertExtraConnectionsCheckSchemaChangeOnReconnect(disableLogins: true);

		[UseSnapshotProtection]
		public void TestExtraConnectionsCheckSchemaChangeOnReconnect_WithoutDisableLogins()
			=> AssertExtraConnectionsCheckSchemaChangeOnReconnect(disableLogins: false);

		void AssertExtraConnectionsCheckSchemaChangeOnReconnect(bool disableLogins)
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				DbEnv.SetDbEnvironment(new BaseDbEnvironment());
				using (var extraConnection = Db.NewExtraConnectionToMainDb())
				using (var adminConnection = Db.NewAdminConnection())
				{
					extraConnection.EnsureIsOpen();

					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout(LockoutReason.Upgrade, disableLogins: disableLogins));
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					try
					{
						AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => extraConnection.BeginTransaction());
						// When logins are disabled the extra connection cannot reconnect and stays closed.
						// However, when logins enabled it will connect, so needs to be manually closed
						extraConnection.CloseConnection();
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}
					try
					{
						AssertExceptionThrown(typeof(DatabaseUpgradedException), () => extraConnection.BeginTransaction());
					}
					finally
					{
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) - 1, adminConnection);
					}
				}
			}
			finally
			{
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		#region TestServerSideAutoReconnectEnabled

		/// <summary>
		/// This is on by default on SQL Server 2014
		/// and it causes problems because the program doesn't get to know if it was disconnected,
		/// for instance, due to an upgrade.
		/// </summary>
		public void TestNoServerSideAutoReconnect()
		{
			using (var testCnx = Db.NewExtraConnectionToMainDb())
			{
				testCnx.EnsureIsOpen();

				bool stateChangeFired = false;
				((IDbConnectionInternals)testCnx).ADOConnection.StateChange +=
					new StateChangeEventHandler((s, e) => stateChangeFired = true);

				AdoTestUtils.KillConnection(testCnx);
				testCnx.EnsureIsOpen();
				AssertEquals("Connection state changed?", true, stateChangeFired);
			}
		}

		#endregion

		#region Caching

		public void TestCaching()
		{
			var d1 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
			Thread.Sleep(1100);
			var d2 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
			Assert(d1 != d2);
			using (Db.Connection.StartScalarCaching())
			{
				d1 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
				var d11 = (DateTime)Db.Connection.ExecuteScalar("select coalesce (@a, getdate())", cmd => cmd.AddParameter("@a", SqlDbType.DateTime, DBNull.Value));
				Thread.Sleep(1100);
				d2 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
				var d22 = (DateTime)Db.Connection.ExecuteScalar("select coalesce (@a, getdate())", cmd => cmd.AddParameter("@a", SqlDbType.DateTime, DBNull.Value));
				Assert(d1 == d2);
				Assert(d11 == d22);
			}
			d1 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
			Thread.Sleep(1100);
			d2 = (DateTime)Db.Connection.ExecuteScalar("select getdate()");
			Assert(d1 != d2);
		}

		#endregion

		#region Test Disconnection and Handling

		[ExpectNoExceptions]
		public void TestBeginTransactionHandlesGeneralNetworkError()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AdoTestUtils.KillConnection(connection);
				connection.BeginTransaction();
				connection.RollbackTransaction();
			}
		}

		public void TestConnectionUnableToReconnect()
		{
			ErrorReporter.Instance.Clear();

			using (var connection = new UpgradedDbConnectionForTestErrorReport(Db.ServerName, Db.DatabaseName))
			{
				try
				{
					AdoTestUtils.KillConnection(connection);
					connection.BeginTransaction();
				}
				catch (Exception)
				{
				}

				var currentConnectionToHandle = connection as IDbReconnectionHandling;
				AssertEquals("Expect DeveloperNotificationException", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Expect DeveloperNotificationException Message",
					$"Unable to reconnect connection, state :{currentConnectionToHandle.State}, IsConnecting {currentConnectionToHandle.IsConnecting}, AppTransactionCount {currentConnectionToHandle.AppTransactionCount}",
					ErrorReporter.LastMessageReported);
			}
		}

		public void TestChangingCurrentDatabaseHandlesGeneralNetworkError()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("[PRE-CONDITION] Current DB should be the default one", Db.DatabaseName, connection.CurrentDatabase);

				AdoTestUtils.KillConnection(connection);

				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					AssertEquals("Current DB should be master", Db.SqlMasterDb, connection.CurrentDatabase);
					AdoTestUtils.KillConnection(connection);
				}

				AssertEquals("Current DB should be the default one again", Db.DatabaseName.ToUpper(),
					connection.CurrentDatabase.ToUpper());
			}
		}

		public void TestDoesNotHandleDisconnectionIfApplicationThinksItIsInATransactionContext_MainConnection()
		{
			AssertEnterpriseConnectionDoesNotHandleDisconnectionIfItThinksItIsInATransactionContext(Db.Connection);
		}

		public void TestDoesNotHandleDisconnectionIfApplicationThinksItIsInATransactionContext_ExtraConnection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertEnterpriseConnectionDoesNotHandleDisconnectionIfItThinksItIsInATransactionContext(connection);
			}
		}

		protected void AssertEnterpriseConnectionDoesNotHandleDisconnectionIfItThinksItIsInATransactionContext(DbConnection testConn)
		{
			AssertEquals("[1] Application should NOT be in transaction mode", 0, testConn.AppTransactionCount);
			AssertEquals("[1] Connection should NOT be in a transaction context", false, testConn.IsInTransaction);
			AssertEquals("[1] Connection should NOT be in a transaction context - Direct DB Check", false, IsDatabaseConnectionInATransactionContext(testConn));

			string testTableName01 = "TestDoesNotHandleDisconnectionIfApplicationThinksItIsInATransactionContext01";
			string testTableName02 = "TestDoesNotHandleDisconnectionIfApplicationThinksItIsInATransactionContext02";
			DropTestTableIfExist(testConn, testTableName01);
			DropTestTableIfExist(testConn, testTableName02);

			try
			{
				testConn.BeginTransaction();

				AssertEquals("[2] Application should be in transaction mode", 1, testConn.AppTransactionCount);
				AssertEquals("[2] Connection should be in a transaction context", true, testConn.IsInTransaction);
				AssertEquals("[2] Connection should be in a transaction context - Direct DB Check", true, IsDatabaseConnectionInATransactionContext(testConn));

				string sqlText = string.Format("CREATE TABLE {0} (col1 int)", testTableName01);
				testConn.ExecuteNonQuery(sqlText);

				AssertEquals("Test Table 01 should exist", true, DoesTableExist(testConn, testTableName01));

				// KILL CONNECTION - TO CAUSE DISCONNECTION HANDLING TO BE CALLED
				//   At this stage the connection (which was in a transaction context) dies in DB Server side, 
				//   but the Application still thinks the transaction is active.
				//   To avoid posting to the DB in auto-commit mode the connection won't be reopened in the DB Server
				//   until <DbConnection>.RollbackTransaction is called to reset the Application transaction status.
				//   In the meantime, any attempt to issue a command to the DB will fail.
				AdoTestUtils.KillConnection(testConn);

				sqlText = string.Format("CREATE TABLE {0} (col1 int)", testTableName02);
				try
				{
					testConn.ExecuteNonQuery(sqlText);
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.GeneralNetworkError, new DbErrorMatch(e).ExceptionType);
				}

				// Cannot perform a "Direct DB Check" here. 
				// The command will fail as <DbConnection>.RollbackTransaction hasn't been called yet.
				AssertEquals("[3] Application should be in transaction mode", 1, testConn.AppTransactionCount);
				AssertEquals("[3] Connection should NOT be in a transaction context", false, testConn.IsInTransaction);

				testConn.RollbackTransaction();

				AssertEquals("Test Table 01 should NOT exist", false, DoesTableExist(testConn, testTableName01));
				AssertEquals("Test Table 02 should NOT exist", false, DoesTableExist(testConn, testTableName02));

				AssertEquals("[4] Application should NOT be in transaction mode", 0, testConn.AppTransactionCount);
				AssertEquals("[4] Connection should NOT be in a transaction context", false, testConn.IsInTransaction);
				AssertEquals("[4] Connection should NOT be in a transaction context - Direct DB Check", false, IsDatabaseConnectionInATransactionContext(testConn));
			}
			finally
			{
				if (testConn.AppTransactionCount > 0)
				{
					testConn.RollbackTransaction();
				}
			}
		}

		protected bool IsDatabaseConnectionInATransactionContext(DbConnection conn)
		{
			int transactionCount = Convert.ToInt32(conn.ExecuteScalar("SELECT @@trancount"));
			return (transactionCount > 0);
		}

		protected bool DoesTableExist(DbConnection conn, string tableName)
		{
			string sqlText = string.Format("SELECT count(*) from sys.objects WHERE type = 'U' AND name = '{0}'", tableName);
			int count = Convert.ToInt32(conn.ExecuteScalar(sqlText));
			return (count == 1);
		}

		protected void DropTestTableIfExist(DbConnection conn, string tableName)
		{
			string sqlText = string.Format("IF EXISTS (SELECT null FROM sys.objects WHERE type = 'U' AND name = '{0}') DROP TABLE {0}", tableName);
			conn.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Deadlock priority

		public void TestSetDeadlockPriority_ShouldStoreValue()
		{
			try
			{
				Db.Connection.SetDeadlockPriority(5);
				AssertEquals(5, Db.Connection.DeadlockPriority);
				Db.Connection.SetDeadlockPriority(-10);
				AssertEquals(-10, Db.Connection.DeadlockPriority);
				Db.Connection.SetDeadlockPriority(10);
				AssertEquals(10, Db.Connection.DeadlockPriority);
			}
			finally
			{
				Db.Connection.SetDeadlockPriority(DeadlockPriority.Medium);
			}
		}

		public void TestSetDeadlockPriority_ShouldTranslateEnumToIntValue()
		{
			try
			{
				Db.Connection.SetDeadlockPriority(DeadlockPriority.Low);
				AssertEquals(-5, Db.Connection.DeadlockPriority);
				Db.Connection.SetDeadlockPriority(DeadlockPriority.Medium);
				AssertEquals(0, Db.Connection.DeadlockPriority);
				Db.Connection.SetDeadlockPriority(DeadlockPriority.High);
				AssertEquals(5, Db.Connection.DeadlockPriority);
			}
			finally
			{
				Db.Connection.SetDeadlockPriority(DeadlockPriority.Medium);
			}
		}

		public void TestSetInvalidDeadlockPriority_ShouldThrow()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => Db.Connection.SetDeadlockPriority(-11));
			AssertExceptionThrown(typeof(ArgumentException), () => Db.Connection.SetDeadlockPriority(11));
		}

		public void TestSetDbEnvDeadlockPriority_ShouldSetOnNewConnectionsOnceOpened()
		{
			DbEnv.Instance.DeadlockPriority = 10;
			try
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals("Should not set priority until connection opened", 0, connection.DeadlockPriority);
					connection.EnsureIsOpen();
					AssertEquals(10, connection.DeadlockPriority);
				}
			}
			finally
			{
				DbEnv.Instance.DeadlockPriority = (int)DeadlockPriority.Medium;
			}
		}

		public void TestTemporarySetDeadlockPriority_Dispose_WhenDatabaseUpgradedException()
			=> AssertDispose_WhenDatabaseUpgradedException((connection) => connection.TemporarySetDeadlockPriority(DeadlockPriority.Low));

		public void TestTemporarySetLockTimeout_Dispose_WhenDatabaseUpgradedException()
			=> AssertDispose_WhenDatabaseUpgradedException((connection) => connection.TemporarySetLockTimeout(1));

		void AssertDispose_WhenDatabaseUpgradedException(Func<DbConnection, IDisposable> disposableChange)
		{
			// Mock the database has been upgraded.
			var currentVersion = GlobalServiceProvider.Instance.GetRequiredService<IDatabaseAspectVersions>();
			var versionMock = Mock.Of<IDatabaseAspectVersions>(x => x.SchemaVersion == new VersionLabel(currentVersion.SchemaVersion.Major + 1, 0));
			var originalServiceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
			mockServiceProvider.Setup(x => x.GetService(typeof(IDatabaseAspectVersions))).Returns(versionMock);

			using (DbEnv.SetTemporaryDbEnvironment(new DbEnvironmentWithMockGuiPluginForTest()))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var disposable = disposableChange(extraConnection);
				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					AssertExceptionThrown<DatabaseUpgradedException>(() => ((IDbReconnectionHandling)extraConnection).CloseAndReopenConnection());

					AssertNoExceptionThrown(() => disposable.Dispose());
					AssertEquals("DatabaseUpgradedExceptionHasBeenThrown", true, extraConnection.DatabaseUpgradedExceptionHasBeenThrown);
					AssertEquals("ConnectionState", ConnectionState.Closed, extraConnection.State);
				}
			}
		}

		#endregion

		#region TestIsAlwaysOnEnabled

		public void TestIsAlwaysOnEnabled()
		{
			Assert(!Db.Connection.IsAlwaysOnEnabled);

			try
			{
				Db.Connection.IsAlwaysOnEnabledForTest = true;
				Assert(Db.Connection.IsAlwaysOnEnabled);

				Db.Connection.IsAlwaysOnEnabledForTest = false;
				Assert(!Db.Connection.IsAlwaysOnEnabled);

				Db.Connection.IsAlwaysOnEnabledForTest = null;
				Assert(!Db.Connection.IsAlwaysOnEnabled);
			}
			finally
			{
				Db.Connection.IsAlwaysOnEnabledForTest = null;
			}
		}

		#endregion

		#region ThreadSentry

		public void TestTransactionInThreadOtherThanOwner()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateTransactionTempTable(connection);
				try
				{
					var thread = new Thread(() =>
					{
						connection.BeginTransaction();
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
						connection.CommitTransaction();
					});
					thread.Start();
					thread.Join();
					AssertStartsWith("ThreadSentryCheck", ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage,
						ErrorReporter.LastMessageReported);
					AssertEquals(1, ErrorReporter.TotalErrorCount);
					ErrorReporter.Clear();
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		#endregion

		public void TestExists()
		{
			try
			{
				Db.Connection.BeginTransaction();
				Db.Connection.ExecuteNonQuery("CREATE TABLE dbo._test_exists (id int);");

				AssertEquals("PRECONDITION", false, Db.Connection.Exists("FROM dbo._test_exists"));

				Db.Connection.ExecuteNonQuery("INSERT dbo._test_exists (id) VALUES (1);");
				AssertEquals("Rows exist?", true, Db.Connection.Exists("FROM dbo._test_exists"));
				AssertEquals("id = 1 exists?", true, Db.Connection.Exists("FROM dbo._test_exists WHERE id = 1"));
				AssertEquals("id = 2 exists?", false, Db.Connection.Exists("FROM dbo._test_exists WHERE id = 2"));

				Db.Connection.ExecuteNonQuery("INSERT dbo._test_exists (id) VALUES (2);");
				AssertEquals("Rows exist?", true, Db.Connection.Exists("FROM dbo._test_exists"));
				AssertEquals("id = 1 exists?", true, Db.Connection.Exists("FROM dbo._test_exists WHERE id = 1"));
				AssertEquals("id = 2 exists?", true, Db.Connection.Exists("FROM dbo._test_exists WHERE id = 2"));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#region Implementation

		protected void CreateTransactionTempTable(DbConnection connection)
		{
			DropTransactionTempTable(connection);
			connection.ExecuteNonQuery("CREATE TABLE #TestTransaction (One varchar(3))");
		}

		protected void DropTransactionTempTable(DbConnection connection)
		{
			connection.ExecuteNonQuery("IF (OBJECT_ID('tempdb..#TestTransaction') IS NOT NULL) DROP TABLE #TestTransaction");
		}

		#endregion

		#region SqlApplicationLock related

		public void TestTryGetLock()
		{
			Assert("Precondition: A lock was acquired", Db.Connection.TryGetLock("0C5FFEF3-6814-4327-847A-99B79464320F", out SqlApplicationLock @lock));

			using (@lock)
			{
				Assert("Should have returned the locked state", @lock.IsHoldingLock());
			}
		}

		public void TestTryGetLockWithBadCon()
		{
			var key = "C9FA53F6-771E-49DE-97B4-70376E99448D";
			SqlApplicationLock originalConnectionLock = null, newConnectionLock = null;
			try
			{
				Db.Connection.TryGetLock(key, out originalConnectionLock);

				Assert("Precondition: original lock must be acquired for the next lock to be not possible", originalConnectionLock.IsHoldingLock());

				using (var newConnection = Db.NewExtraConnectionToMainDb())
				{
					Assert("Another connection has a lock, so we shouldn't get one", !newConnection.TryGetLock(key, out newConnectionLock));
				}
			}
			finally
			{
				if (originalConnectionLock != null)
				{
					originalConnectionLock.Dispose();
				}
				if (newConnectionLock != null)
				{
					newConnectionLock.Dispose();
				}
			}
		}

		public void TestTryGetLockListCleaning()
		{
			var appLocks = new List<SqlApplicationLock>();
			const string lockKeyPrefix = "Lock_Key_";
			SqlApplicationLock sqlLock;
			string lockKey;
			for (var i = 0; i < Db.Connection.GetSqlLockThresholds(); i++)
			{
				lockKey = lockKeyPrefix + i;
				Db.Connection.TryGetLock(lockKey, out sqlLock);

				Assert(sqlLock.IsHoldingLock());
				appLocks.Add(sqlLock);
			}

			AssertEquals("Check right number of SQL locks", Db.Connection.GetSqlLockThresholds(), Db.Connection.UndisposedSqlLocksCount());

			foreach (var sqlAppLock in appLocks)
			{
				sqlAppLock.Dispose();
			}
			appLocks.Clear();

			Assert(!Db.Connection.HasUndisposedSqlLocks());
			AssertEquals("Check the count is still the same", Db.Connection.GetSqlLockThresholds(), Db.Connection.UndisposedSqlLocksCount());

			// Check that the list is cleaned when the number of elements in the list becomes more than Db.Connection.GetSqlLockThresholds
			lockKey = lockKeyPrefix + Db.Connection.GetSqlLockThresholds();
			Db.Connection.TryGetLock(lockKey, out sqlLock);
			Assert(sqlLock.IsHoldingLock());
			Assert(Db.Connection.HasUndisposedSqlLocks());
			AssertEquals("Check the count is 1", 1, Db.Connection.UndisposedSqlLocksCount());
			sqlLock.Dispose();
		}

		#endregion

		public void TestTopLevelTransactionsBegunCount()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals(0, connection.TopLevelTransactionsBegunCount);
				connection.BeginTransaction();
				AssertEquals(1, connection.TopLevelTransactionsBegunCount);
				connection.BeginTransaction();
				AssertEquals(1, connection.TopLevelTransactionsBegunCount);
				connection.CommitTransaction();
				AssertEquals(1, connection.TopLevelTransactionsBegunCount);
				connection.CommitTransaction();
				AssertEquals(1, connection.TopLevelTransactionsBegunCount);

				connection.BeginTransaction();
				AssertEquals(2, connection.TopLevelTransactionsBegunCount);
				connection.RollbackTransaction();
				AssertEquals(2, connection.TopLevelTransactionsBegunCount);

				AdoTestUtils.KillConnection(connection);
				connection.BeginTransaction();
				AssertEquals(3, connection.TopLevelTransactionsBegunCount);
				connection.RollbackTransaction();
				AssertEquals(3, connection.TopLevelTransactionsBegunCount);
			}
		}

		#region TestSameTransactionFromMultipleThreads

		public void TestSameTransactionFromMultipleThreadsShouldNotModifyAppTransactionCount()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("AppTransactionCount should to be 0", 0, connection.AppTransactionCount);

				var readyHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
				var thread = new Thread(() =>
				{
					try
					{
						connection.BeginTransactionWithManager();
					}
					finally
					{
						readyHandle.Set();
					}
				});
				thread.Start();
				readyHandle.WaitOne();

				AssertEquals("AppTransactionCount should to be 1", 1, connection.AppTransactionCount);

				var errorReporterMock = new Mock<IErrorReporter>();
				errorReporterMock.Setup(r => r.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
								.Throws(new OutOfMemoryException("Jerry Test Message"));
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					AssertExceptionThrown<OutOfMemoryException>("Should throw OutOfMemoryException", "Jerry Test Message", () => connection.BeginTransactionWithManager());
					AssertEquals("AppTransactionCount still needs to be 1", 1, connection.AppTransactionCount);
				}

				ErrorReporter.Clear();
			}
		}

		public void TestSameTransactionFromMultipleThreads()
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var readyHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
			using (var thread1Data = new ThreadContextData { Connection = connection, ReadyHandle = readyHandle })
			using (var thread2Data = new ThreadContextData { Connection = connection, ReadyHandle = readyHandle })
			using (var thread3Data = new ThreadContextData { Connection = connection, ReadyHandle = readyHandle })
			{
				connection.ThreadSentry.RelinquishThreadOwnership();

				var thread1 = new Thread(Method);
				var thread2 = new Thread(Method);
				var thread3 = new Thread(Method);

				// Act
				thread1.Start(thread1Data);
				thread2.Start(thread2Data);
				thread3.Start(thread3Data);

				thread1Data.ThreadReady.Wait();
				thread2Data.ThreadReady.Wait();
				thread3Data.ThreadReady.Wait();

				readyHandle.Set();

				thread1.Join();
				thread2.Join();
				thread3.Join();

				// Assert
				AssertEquals("SameTransactionInMultipleThreads_WithStackTrace", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				connection.ThreadSentry.TakeThreadOwnership();
			}
		}

		public void TestAppTransactionCountReset_Commit()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var events = new List<(DbConnection sender, DbConnection.AppTransactionCountResetEventArgs args, int appTransactionCount)>();
				connection.AppTransactionCountReset += (s, e) =>
				{
					var senderConnection = (DbConnection)s;
					events.Add((senderConnection, e, senderConnection.AppTransactionCount));
				};

				CreateTransactionTempTable(connection);
				try
				{
					connection.BeginTransaction();
					connection.BeginTransaction();
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					connection.CommitTransaction();
					var eventCountBeforeOutermost = events.Count;
					connection.CommitTransaction();
					AssertEquals("event count", 1, events.Count);
					AssertEquals("event sender", connection, events[0].sender);
					AssertEquals("event arg Committed", true, events[0].args.Committed);
					AssertEquals("appTransactionCount at time of event", 0, events[0].appTransactionCount);
					AssertEquals("inner transactions do not raise event", 0, eventCountBeforeOutermost);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestAppTransactionCountReset_Rollback()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var events = new List<(DbConnection sender, DbConnection.AppTransactionCountResetEventArgs args, int appTransactionCount)>();
				connection.AppTransactionCountReset += (s, e) =>
				{
					var senderConnection = (DbConnection)s;
					events.Add((senderConnection, e, senderConnection.AppTransactionCount));
				};

				CreateTransactionTempTable(connection);
				try
				{
					connection.BeginTransaction();
					connection.BeginTransaction();
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					connection.RollbackTransaction();
					var eventCountBeforeOutermost = events.Count;
					connection.RollbackTransaction();
					AssertEquals("event count", 1, events.Count);
					AssertEquals("event sender", connection, events[0].sender);
					AssertEquals("event arg Committed", false, events[0].args.Committed);
					AssertEquals("appTransactionCount at time of event", 0, events[0].appTransactionCount);
					AssertEquals("inner transactions do not raise event", 0, eventCountBeforeOutermost);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		public void TestAppTransactionCountReset_CloseAndReopen()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var events = new List<(DbConnection sender, DbConnection.AppTransactionCountResetEventArgs args, int appTransactionCount)>();
				connection.AppTransactionCountReset += (s, e) =>
				{
					var senderConnection = (DbConnection)s;
					events.Add((senderConnection, e, senderConnection.AppTransactionCount));
				};

				CreateTransactionTempTable(connection);
				try
				{
					connection.BeginTransaction();
					connection.BeginTransaction();
					connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
					var eventCountBefore = events.Count;

					try
					{
						((IDbReconnectionHandling)connection).CloseAndReopenConnection();
					}
					catch (TransactionException ex)
						when (ex.ErrorType == OdysseyDataErrorType.TransactionRolledBack)
					{
						// SetLockTimeout is always called to change from the sql default on reconnection, tripping our rollback detection when we try to reconnect
						connection.RollbackTransaction();
						connection.RollbackTransaction();
					}

					AssertEquals("event count", 1, events.Count);
					AssertEquals("event sender", connection, events[0].sender);
					AssertEquals("event arg Committed", false, events[0].args.Committed);
					AssertEquals("appTransactionCount at time of event", 0, events[0].appTransactionCount);
					AssertEquals("initial count", 0, eventCountBefore);
				}
				finally
				{
					DropTransactionTempTable(connection);
					DbCommitTracker.Ignore("#TestTransaction");
				}
			}
		}

		class ThreadContextData : IDisposable
		{
			public DbConnection Connection { get; set; }
			public WaitHandle ReadyHandle { get; set; }
			public ManualResetEventSlim ThreadReady { get; } = new ManualResetEventSlim(false);

			public void Dispose()
			{
				ThreadReady.Dispose();
			}
		}

		static void Method(object data)
		{
			var contextData = (ThreadContextData)data;

			Monitor.Enter(connectionMutex);
			contextData.Connection.ThreadSentry.TakeThreadOwnership();

			using (contextData.Connection.BeginTransactionWithManager())
			{
				contextData.Connection.ThreadSentry.RelinquishThreadOwnership();
				Monitor.Exit(connectionMutex);

				contextData.ThreadReady.Set();

				contextData.ReadyHandle.WaitOne();
			}
		}

		static readonly object connectionMutex = new object();

		#endregion

		class DbEnvironmentWithMockGuiPluginForTest : BaseDbEnvironment
		{
			readonly IDbConnectionGuiPlugin connectionGuiPlugin = new Mock<IDbConnectionGuiPlugin>().Object;

			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
		}

		[UseSnapshotProtection()]
		public class SuspendAuditTriggersTest : TestCase
		{
			public void TestExistingTriggerIsSuspended()
			{
				Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.TestTable1 (Col1 int) ");
				Db.Connection.ExecuteNonQuery(@"
CREATE TRIGGER TG_TestTable1_No_Insert ON dbo.TestTable1
AFTER INSERT 
AS 
BEGIN

IF (SELECT SESSION_CONTEXT(N'Suspend_System_Audit_Columns_Guard')) = '1' 
	RETURN

RAISERROR('Insert is not allowed on TestTable1', 16, 1)

END");
				using (Db.Connection.SuspendAuditTriggers())
				{
					AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery("INSERT INTO TestTable1 DEFAULT VALUES"));
				}
			}

			public void TestNestedSuspendAuditTriggers()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");

						using (connection.SuspendAuditTriggers())
						{
							AssertSuspended(connection, expected: "1");

							using (connection.SuspendAuditTriggers())
							{
								AssertSuspended(connection, expected: "1");
							}

							AssertSuspended(connection, expected: "1");
						}

						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestResumedAfterInternalTransactionIsRolledBack()
			{
				// Arrange
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					connection.BeginTransaction();

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
						connection.RollbackTransaction();
						AssertSuspended(connection, expected: "1");
					}

					// Assert
					AssertSuspended(connection, expected: null);
				}
			}

			public void TestNullBinaryValueTakenAsDisabled()
			{
				// Arrange
				DbRegistry.SuspendAuditTriggers.SaveValue(true, Db.Connection);
				_ = Db.Connection.ExecuteNonQuery($@"
UPDATE [StmData]
SET SD_BinaryValue = @value
WHERE SD_Name = @Name", cmd =>
				{
					cmd.AddParameter("@value", SqlDbType.VarBinary, DBNull.Value);
					cmd.AddParameter("@name", SqlDbType.VarChar, DbRegistry.SuspendAuditTriggersName);
				});
				ReloadRegistry();

				// Act
				// Assert
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestForceSuspendAuditTriggers()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestSessionContextResetOnOpenTransaction()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					connection.BeginTransaction();
					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");

						_ = connection.ExecuteNonQuery("CREATE TABLE t1 (id int);");
					}

					AssertSuspended(connection, expected: null);
					AssertEquals(true, connection.AppTransactionCount > 0);
				}
			}

			public void TestSessionContextResetOnBrokenTransaction()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
						using (connection.BeginTransactionWithManager())
						{
							_ = connection.ExecuteNonQuery(@"ROLLBACK;");
						}
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestSessionContextResetOnBrokenTransactionManagerTransactionException()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					_ = AssertExceptionThrown<TransactionException>(() =>
					{
						try
						{
							using (connection.SuspendAuditTriggers())
							{
								AssertSuspended(connection, expected: "1");
								using (connection.BeginTransactionWithManager())
								{
									_ = connection.ExecuteNonQuery(@"ROLLBACK;");
									connection.EnsureIsOpen();
								}
							}
						}
						finally
						{
							AssertSuspended(connection, expected: null);
						}
					});
				}
			}

			public void TestSessionContextResetTransactionRollbackOnServerSide()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					connection.BeginTransaction();
					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
						_ = connection.ExecuteNonQuery(@"ROLLBACK;");
						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestSessionContextResetOnTransactionException()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					_ = AssertExceptionThrown<TransactionException>(() =>
					{
						connection.BeginTransaction();
						using (connection.SuspendAuditTriggers())
						{
							AssertSuspended(connection, expected: "1");
							_ = connection.ExecuteNonQuery("ROLLBACK;");

							connection.EnsureIsOpen();
						}
					});

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestSessionContextResetOnBrokenTransactionFromOuterScope()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					_ = AssertExceptionThrown<TransactionException>(() =>
					{
						try
						{
							using (connection.BeginTransactionWithManager())
							using (connection.SuspendAuditTriggers())
							{
								AssertSuspended(connection, expected: "1");

								_ = connection.ExecuteNonQuery(@"ROLLBACK;");
								connection.EnsureIsOpen();
							}
						}
						finally
						{
							AssertSuspended(connection, expected: null);
						}
					});
				}
			}

			public void TestSessionContextResetOnClosedConnection()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: null);

					_ = AssertExceptionThrown<TransactionException>(() =>
					{
						try
						{
							using (connection.BeginTransactionWithManager())
							using (connection.SuspendAuditTriggers())
							{
								AssertSuspended(connection, expected: "1");

								_ = connection.ExecuteNonQuery(@"ROLLBACK;");
								connection.EnsureIsOpen();
								connection.CloseConnection();
							}
						}
						finally
						{
							AssertSuspended(connection, expected: null);
						}
					});
				}
			}

			public void TestSessionContextResetOnClosedConnectionWhenRegistryEnabled()
			{
				EnableRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
					AssertSuspended(connection, expected: "1");

					using (connection.BeginTransactionWithManager())
					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");

						connection.CloseConnection();

						AssertSuspended(connection, expected: null);
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestSuspendAuditTriggersIsRefreshedAfterConnectionOpen()
			{
				for (var i = 0; i < 3; i++)
				{
					EnableRegistrySuspendAuditTriggersThenReload();

					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						connection.EnsureIsOpen();
						AssertSuspended(connection, expected: "1");
					}

					DeleteRegistrySuspendAuditTriggersThenReload();
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						connection.EnsureIsOpen();
						AssertSuspended(connection, expected: null);
					}
				}
			}

			public void TestSuspendAuditTriggersOnAClosedConnectionWhenRegistryIsTrue()
			{
				EnableRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals(ConnectionState.Closed, connection.State);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: "1");
				}
			}

			public void TestSuspendAuditTriggersOnAClosedConnectionWhenRegistryIsFalse()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals(ConnectionState.Closed, connection.State);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestEnableAllAuditTriggers()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals(ConnectionState.Closed, connection.State);
					AssertSuspended(connection, expected: null);

					using (connection.SuspendAuditTriggers())
					{
						AssertSuspended(connection, expected: "1");

						using (connection.EnableAllAuditTriggers())
						{
							AssertSuspended(connection, expected: "NONE");
						}

						AssertSuspended(connection, expected: "1");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public void TestEnableAllAuditTriggersPreventsOverride()
			{
				DeleteRegistrySuspendAuditTriggersThenReload();

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					using (connection.EnableAllAuditTriggers())
					{
						AssertNoExceptionThrown(() =>
						{
							using (connection.EnableAllAuditTriggers())
							{
							}
						});

						var ex = AssertExceptionThrown<InvalidOperationException>(() =>
						{
							using (connection.SuspendAuditTriggers())
							{
							}
						});

						AssertContains("Cannot override the suspension state of audit triggers.", ex.Message);
						AssertSuspended(connection, expected: "NONE");
					}

					AssertSuspended(connection, expected: null);
				}
			}

			public string GetAuditTriggerSuspendedSessionContext(DbConnection connection)
			{
				const string sql = "SELECT SESSION_CONTEXT(N'" + DbConnection.TriggerKey + "')";

				if (connection.State != ConnectionState.Open)
				{
					return null;
				}

				using var command = ((IDbConnectionInternals)connection).InternalDbConnection.CreateCommand();
				command.CommandText = sql;
				command.Transaction = ((IDbConnectionInternals)connection).InternalDbTransaction;
				var value = command.ExecuteScalar();
				return value == DBNull.Value ? null : (string)value;
			}

			void AssertSuspended(DbConnection dbConnection, string expected)
			{
				AssertEquals(nameof(SuspendAuditTriggersTest), expected, GetAuditTriggerSuspendedSessionContext(dbConnection));
			}

			void EnableRegistrySuspendAuditTriggersThenReload()
			{
				DbRegistry.SuspendAuditTriggers.SaveValue(true, Db.Connection);
				ReloadRegistry();
			}

			void DeleteRegistrySuspendAuditTriggersThenReload()
			{
				_ = Db.Connection.ExecuteNonQuery($@"
DELETE from [StmData]
WHERE SD_Name = @Name", cmd => cmd.AddParameter("@Name", SqlDbType.VarChar, DbRegistry.SuspendAuditTriggersName));
				ReloadRegistry();
			}

			void ReloadRegistry()
			{
				DbConnection.ResetSuspendAuditTriggersFromRegistry_ForTest();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
				}
			}

			protected override void TearDown()
			{
				base.TearDown();

				DeleteRegistrySuspendAuditTriggersThenReload();
			}
		}

		[ExpectNoExceptions]
		public void TestErrorReporterIsAvailable()
		{
			TestNullParam();
			TestNotNullParam();

			ErrorReporter.Clear();

			void TestNullParam()
			{
				AssertNoExceptionThrown(() =>
				{
					var connection = new WithErrorReporterDbConnection(null);
					connection.ReportOnce(nameof(TestNullParam), "message", new Exception());
				});
			}

			void TestNotNullParam()
			{
				// Arrange
				var errorReporter = new Mock<IErrorReporter>();
				errorReporter.Setup(r => r.Report(nameof(TestNotNullParam), "message", It.IsAny<Exception>()))
					.Verifiable(Times.Once);

				var connection = new WithErrorReporterDbConnection(errorReporter.Object);

				// Act
				connection.ReportOnce(nameof(TestNotNullParam), "message", new Exception());

				// Assert
				errorReporter.Verify();
			}
		}

		sealed class WithErrorReporterDbConnection : DbConnection
		{
			public WithErrorReporterDbConnection(IErrorReporter errorReporter) : base(Db.DefaultDataProviderFactory, Db.ServerName, Db.DatabaseName, null, errorReporter)
			{
			}

			public override string UserLogin => throw new NotImplementedException();

			protected override IDbConnection OpenNewDbConnection()
			{
				throw new NotImplementedException();
			}

			public void ReportOnce(string key, string message, Exception ex, params string[] additionalInfoFilterKeys)
			{
				ErrorReporter.ReportOnce(key, message, ex, additionalInfoFilterKeys);
			}
		}
	}
}
