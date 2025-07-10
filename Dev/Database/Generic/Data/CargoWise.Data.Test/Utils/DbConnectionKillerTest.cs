using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using NUnit.Framework;
using static System.FormattableString;

namespace CargoWise.Data.Testing
{
	sealed class DbConnectionKillerTest : TestCase
	{
		public void TestKillOtherConnections()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

				using (DbConnection anotherConnection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					anotherConnection.EnsureIsOpen();

					bool hasTestConnBeenClosed = false;
					((IDbConnectionInternals)anotherConnection).ADOConnection.StateChange +=
						new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasTestConnBeenClosed = true);

					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					anotherConnection.EnsureIsOpen();
					AssertEquals("Has test connection been closed (current database was master)?", false, hasTestConnBeenClosed);

					((ICurrentDbControl)anotherConnection).UseDatabase(Db.DatabaseName);
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					anotherConnection.EnsureIsOpen();
					AssertEquals("Has test connection been closed (current database was main DB)?", true, hasTestConnBeenClosed);
				}
			}
		}

		public void TestKillOtherConnectionsUsingAuxiliaryConnection()
		{
			using (var connectionToKeep = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				bool hasConnectionToKeepBeenClosed = false;
				((IDbConnectionInternals)connectionToKeep).ADOConnection.StateChange +=
					new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasConnectionToKeepBeenClosed = true);

				DbConnectionKiller.KillOtherConnections(connectionToKeep, Db.AuditDatabaseName);
				connectionToKeep.EnsureIsOpen();
				AssertEquals("Has connection-to-keep been closed?", false, hasConnectionToKeepBeenClosed);

				using (connectionToKeep.BeginTransactionWithManager())
				{
					using (var otherConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
					{
						otherConnection.EnsureIsOpen();

						bool hasOtherConnectionBeenClosed = false;
						((IDbConnectionInternals)otherConnection).ADOConnection.StateChange +=
							new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasOtherConnectionBeenClosed = true);

						DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(Db.AuditDatabaseName, connectionToKeep, (s) => { }, DateTime.MaxValue, Timeout.InfiniteTimeSpan);
						connectionToKeep.EnsureIsOpen();
						AssertEquals("Has connection-to-keep been closed?", false, hasConnectionToKeepBeenClosed);
						otherConnection.EnsureIsOpen();
						AssertEquals("Has other connection been closed?", true, hasOtherConnectionBeenClosed);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_TwoTransactions_SameDB()
		{
			using (var mainConnection = Db.NewAdminConnection())
			{
				PrepareTestTable(mainConnection);

				using (var user_1_connection = Db.NewAdminConnection(Db.DatabaseName))
				using (var user_2_connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					user_1_connection.EnsureIsOpen();
					var user_1_state_changed = false;
					((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => user_1_state_changed = true;
					user_1_connection.BeginTransaction();
					user_1_connection.ExecuteNonQuery(SQL_UserTransaction());

					user_2_connection.EnsureIsOpen();
					var user_2_state_changed = false;
					((IDbConnectionInternals)user_2_connection).ADOConnection.StateChange += (sender, e) => user_2_state_changed = true;
					user_2_connection.BeginTransaction();
					user_2_connection.ExecuteNonQuery(SQL_UserTransaction());

					AssertEquals("Has user_1 connection been closed?", false, user_1_state_changed);
					AssertEquals("Has user_2 connection been closed?", false, user_2_state_changed);

					AssertEquals(true, DbConnectionKiller.KillOtherConnections(mainConnection, Db.DatabaseName, (message) => Logger.Add(message)));

					try
					{ user_1_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_1 connection been closed?", true, user_1_state_changed);

					try
					{ user_2_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_2 connection been closed?", true, user_2_state_changed);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_TwoTransactions_DifferentDB()
		{
			using (var mainConnection = Db.NewAdminConnection())
			{
				PrepareTestTable(mainConnection);

				using (var user_1_connection = Db.NewAdminConnection(Db.DatabaseName))
				using (var user_2_connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					user_1_connection.EnsureIsOpen();
					var user_1_state_changed = false;
					((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => user_1_state_changed = true;
					user_1_connection.BeginTransaction();
					user_1_connection.ExecuteNonQuery(SQL_UserTransaction());

					user_2_connection.EnsureIsOpen();
					var user_2_state_changed = false;
					((IDbConnectionInternals)user_2_connection).ADOConnection.StateChange += (sender, e) => user_2_state_changed = true;
					user_2_connection.BeginTransaction();
					user_2_connection.ExecuteNonQuery(SQL_UserTransaction(Db.DatabaseName));

					AssertEquals("Has user_1 connection been closed?", false, user_1_state_changed);
					AssertEquals("Has user_2 connection been closed?", false, user_2_state_changed);

					AssertEquals(true, DbConnectionKiller.KillOtherConnections(mainConnection, Db.DatabaseName, (message) => Logger.Add(message)));

					try
					{ user_1_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_1 connection been closed?", true, user_1_state_changed);

					try
					{ user_2_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_2 connection been closed?", true, user_2_state_changed);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_OneTransaction_OneUser()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				PrepareTestTable(Db.AdminConnection);

				using (var user_1_connection = Db.NewAdminConnection(Db.DatabaseName))
				using (var user_2_connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					user_1_connection.EnsureIsOpen();
					var user_1_state_changed = false;
					((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => user_1_state_changed = true;
					user_1_connection.BeginTransaction();
					user_1_connection.ExecuteNonQuery(SQL_UserTransaction());

					user_2_connection.EnsureIsOpen();
					var user_2_state_changed = false;
					((IDbConnectionInternals)user_2_connection).ADOConnection.StateChange += (sender, e) => user_2_state_changed = true;

					AssertEquals("Has user_1 connection been closed?", false, user_1_state_changed);
					AssertEquals("Has user_2 connection been closed?", false, user_2_state_changed);

					AssertEquals(true, DbConnectionKiller.KillOtherConnections(Db.AdminConnection, Db.DatabaseName, (message) => Logger.Add(message)));

					try
					{ user_1_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_1 connection been closed?", true, user_1_state_changed);

					try
					{ user_2_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_2 connection been closed?", true, user_2_state_changed);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_OneTransaction_NoUsers()
		{
			using (var mainConnection = Db.NewAdminConnection())
			{
				PrepareTestTable(mainConnection);
				DbConnectionKiller.KillOtherConnections(mainConnection, Db.DatabaseName);

				using (var user_1_connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					var user_1_SPID = user_1_connection.SPID;

					var user_1_state_changed = false;
					((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => user_1_state_changed = true;
					user_1_connection.BeginTransaction();
					user_1_connection.ExecuteNonQuery(SQL_UserTransaction());

					AssertEquals("Has user_1 connection been closed?", false, user_1_state_changed);

					AssertEquals(true, DbConnectionKiller.KillOtherConnections(mainConnection, Db.DatabaseName, (message) => Logger.Add(message)));

					try
					{ user_1_connection.EnsureIsOpen(); }
					catch { }
					AssertEquals("Has user_1 connection been closed?", true, user_1_state_changed);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_OneSelect_SameDB()
		{
			using (var user_1_connection = Db.NewAdminConnection(Db.DatabaseName))
			using (var source = new CancellationTokenSource())
			{
				var token = source.Token;

				user_1_connection.EnsureIsOpen();
				var user_1_state_changed = false;
				((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => user_1_state_changed = true;

				user_1_connection.ThreadSentry.RelinquishThreadOwnership();

				var isRunning = false;
				var task = Task.Run(async () =>
				{
					user_1_connection.ThreadSentry.TakeThreadOwnership();

					using (var cmd = user_1_connection.Command(SQL_UserSelect()))
					{
						user_1_connection.ThreadSentry.RelinquishThreadOwnership();

						try
						{
							isRunning = true;
							await cmd.ExecuteNonQueryAsync(token);
						}
						catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.SevereError)
						{
							// connection has been killed
						}
						catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.GeneralNetworkError)
						{
							// A transport-level error has occurred when receiving results from the server. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)
						}
						catch (ObjectDisposedException)
						{
							// The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.
						}
						catch (InvalidOperationException ex) when (ex.Message.Contains("The connection is closed."))
						{
							// The command may complete just as the connection gets closed, eat that exception.
						}
					}
				});

				while (!isRunning)
				{
					Thread.Sleep(1);
				}

				Thread.Sleep(1000);

				try
				{
					AssertEquals("Has user_1 connection been closed?", false, user_1_state_changed);

					AssertEquals(true, DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(Db.DatabaseName, Db.Connection, (message) => Logger.Add(message), DateTime.MaxValue, Timeout.InfiniteTimeSpan));
				}
				finally
				{
					user_1_connection.ThreadSentry.TakeThreadOwnership();

					source.Cancel();
					task.Wait();

					user_1_connection.EnsureIsOpen();
					AssertEquals("Has user_1 connection been closed?", true, user_1_state_changed);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestKillOtherConnections_OneSelect_DifferentDB()
		{
			using (var stateChangedEvent = new AutoResetEvent(false))
			using (var isRunningEvent = new AutoResetEvent(false))
			using (var user_1_connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (var source = new CancellationTokenSource())
			{
				var token = source.Token;

				user_1_connection.EnsureIsOpen();
				((IDbConnectionInternals)user_1_connection).ADOConnection.StateChange += (sender, e) => stateChangedEvent.Set();
				user_1_connection.ThreadSentry.RelinquishThreadOwnership();

				var task = Task.Run(async () =>
				{
					user_1_connection.ThreadSentry.TakeThreadOwnership();

					using (var cmd = user_1_connection.Command(SQL_UserSelect(Db.DatabaseName)))
					{
						user_1_connection.ThreadSentry.RelinquishThreadOwnership();

						try
						{
							isRunningEvent.Set();
							await cmd.ExecuteNonQueryAsync(token);
						}
						catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.SevereError)
						{
							// connection has been killed
						}
						catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.GeneralNetworkError)
						{
							// A transport-level error has occurred when receiving results from the server. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)
						}
						catch (ObjectDisposedException)
						{
							// The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.
						}
						catch (InvalidOperationException ex) when (ex.Message.Contains("The connection is closed."))
						{
							// connection has been killed
						}
					}
				});

				Assert("Timeout running the background task", isRunningEvent.WaitOne(TimeSpan.FromSeconds(30)));

				try
				{
					AssertEquals("Has user_1 connection been closed?", false, stateChangedEvent.WaitOne(TimeSpan.FromSeconds(1)));
					AssertEquals(true, DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(Db.DatabaseName, Db.Connection, (message) => Logger.Add(message), DateTime.MaxValue, Timeout.InfiniteTimeSpan));
				}
				finally
				{
					user_1_connection.ThreadSentry.TakeThreadOwnership();

					source.Cancel();
					task.Wait();

					user_1_connection.EnsureIsOpen();
					AssertEquals("Has user_1 connection been closed?", true, stateChangedEvent.WaitOne(TimeSpan.FromSeconds(30)));
				}
			}
		}

		public void TestKillOtherConnectionsUsingAuxiliaryConnection_OnlyKillsExistingConnectionsThatHangAround()
		{
			using (var connectionToKeep = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				bool hasConnectionToKeepBeenClosed = false;
				((IDbConnectionInternals)connectionToKeep).ADOConnection.StateChange +=
					new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasConnectionToKeepBeenClosed = true);

				DbConnectionKiller.KillOtherConnections(connectionToKeep, Db.AuditDatabaseName);
				connectionToKeep.EnsureIsOpen();
				AssertEquals("Has connection-to-keep been closed?", false, hasConnectionToKeepBeenClosed);

				using (connectionToKeep.BeginTransactionWithManager())
				using (var otherConnection1 = Db.NewAdminConnection(Db.AuditDatabaseName))
				{
					otherConnection1.EnsureIsOpen();
					var connectionClosedStopwatch = new Stopwatch();
					bool hasOtherConnection1BeenClosed = false;
					((IDbConnectionInternals)otherConnection1).ADOConnection.StateChange +=
						new StateChangeEventHandler((object sender, StateChangeEventArgs e) =>
						{
							hasOtherConnection1BeenClosed = true;
							connectionClosedStopwatch.Stop();
						});
					Thread.Sleep(1000); //Ensure the first connection occurs before the loginCutoffTime

					using (var killCompleteEvent = new AutoResetEvent(false))
					{
						var secondConnectionTask = Task.Run(() =>
						{
							Thread.Sleep(500); // Wait until the loginCutoffTime has been established.
							using (var otherConnection2 = Db.NewAdminConnection(Db.AuditDatabaseName))
							{
								otherConnection2.EnsureIsOpen();
								bool hasOtherConnection2BeenClosed = false;
								((IDbConnectionInternals)otherConnection2).ADOConnection.StateChange +=
									new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasOtherConnection2BeenClosed = true);

								AssertEquals("Timeout waiting for kill to complete", true, killCompleteEvent.WaitOne(5000));
								otherConnection2.EnsureIsOpen();
								AssertEquals("Has other connection 2 been closed?", false, hasOtherConnection2BeenClosed);
							}
						});

						connectionClosedStopwatch.Start();
						var loginCutoffTime = DbConnectionKiller.GetSqlServerDateForLoginCutoff(otherConnection1);
						Thread.Sleep(TimeSpan.FromSeconds(1));
						DbConnectionKiller.KillOtherConnectionsUsingAuxiliaryConnection(Db.AuditDatabaseName, connectionToKeep, feedbackMethod: null, loginCutoffTime: loginCutoffTime, lockTimeout: Timeout.InfiniteTimeSpan);
						killCompleteEvent.Set();
						AssertEquals("Second Connection Task failed to complete", true, secondConnectionTask.Wait(5000));

						connectionToKeep.EnsureIsOpen();
						AssertEquals("Has connection-to-keep been closed?", false, hasConnectionToKeepBeenClosed);
						otherConnection1.EnsureIsOpen();
						AssertEquals("Has other connection 1 been closed?", true, hasOtherConnection1BeenClosed);
						AssertGreaterThan("Should have been a delay before the kill", connectionClosedStopwatch.ElapsedMilliseconds, 750);
					}
				}
			}
		}

		public void TestKillAllConnectionsOfTheCurrentProcess()
		{
			var (hostName, hostProcessId) = DbConnectionKiller.GetCurrentProcessInfo_Exposed();
			var existingConnections = DbConnectionKiller.GetConnectionsCount_ForTest(hostName, hostProcessId);

			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				// Arrange
				connection1.EnsureIsOpen();
				connection2.EnsureIsOpen();

				AssertEquals("Precondition", existingConnections + 2, DbConnectionKiller.GetConnectionsCount_ForTest(hostName, hostProcessId));

				// Act
				DbConnectionKiller.KillAllConnectionsOfTheCurrentProcess(feedbackMethod: null);

				// Assert
				AssertEquals("All process connections have been killed", 0, DbConnectionKiller.GetConnectionsCount_ForTest(hostName, hostProcessId));
			}
		}

		public void TestRollingBackTimeOutThrowsException()
		{
			var testTimeout = TimeSpan.FromMilliseconds(10);

			Exception expectedException = null;
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				connection1.EnsureIsOpen();
				adminConnection.EnsureIsOpen();

				using (DbConnectionKiller.SetRollingBackTimeout_ForTest(testTimeout))
				{
					try
					{
						DbConnectionKiller.SkipExecution_ForTest.Value = true;
						DbConnectionKiller.PendingRollback(
							adminConnection,
							new List<DbConnectionKiller.ConnectionInfo>(new[] { new DbConnectionKiller.ConnectionInfo(Invariant($"{connection1.SPID}")) }),
							feedbackMethod: null);
					}
					catch (Exception exception)
					{
						expectedException = exception;
					}
				}
			}

			AssertNotNull(expectedException);
			Assert(
				expectedException.Message,
				expectedException.Message.StartsWith(
					Invariant($"Killing user connections has exceeded timeout limit of {testTimeout.TotalMinutes:0} minutes."),
					StringComparison.OrdinalIgnoreCase));
		}

		#region Implementation

		List<string> Logger { get; } = new List<string>();

		static void PrepareTestTable(DbConnection connection)
		{
			connection.ExecuteNonQuery(@"
if (OBJECT_ID(N'dbo._TestTable', N'U') is NOT NULL) DROP TABLE dbo._TestTable;
CREATE TABLE dbo._TestTable (id int);
"
				);
		}

		string SQL_UserTransaction(string dbName = null)
		{
			dbName = string.IsNullOrWhiteSpace(dbName) ? "" : dbName.QuoteName() + ".";
			return FormattableString.Invariant($@"
DECLARE
	@i int = 1

while (@i <= 10)
begin
	INSERT {dbName}dbo._TestTable (id)
	SELECT
		id = SNS_Number
	FROM
		{dbName}dbo.StmNumberSequence
	WHERE 1=1
		AND SNS_Number BETWEEN 1 AND 1000

	SET @i += 1
end

"
				);
		}

		string SQL_UserSelect(string dbName = null)
		{
			dbName = string.IsNullOrWhiteSpace(dbName) ? "" : dbName.QuoteName() + ".";
			return FormattableString.Invariant($@"
SELECT TOP (1)
	id = NULL
FROM
	{dbName}dbo.StmNumberSequence      AS A
	JOIN {dbName}dbo.StmNumberSequence AS B ON B.SNS_Number - A.SNS_Number > 1000000
OPTION (MAXDOP 1)

"
				);
		}

		#endregion // Implementation
	}
}
