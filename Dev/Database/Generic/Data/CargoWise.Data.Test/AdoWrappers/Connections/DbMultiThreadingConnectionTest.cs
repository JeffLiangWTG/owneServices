using System;
using System.Threading;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbMultiThreadingConnectionTest : TestCase
	{
		public void TestUseDbConectionCorrectly()
		{
			var result = "";
			var errMsg = "";

			ErrorReporter.Clear();

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();

				using (Db.DisposableActionForDbConnection())
				{
					result = (string)Db.Connection.ExecuteScalar("SELECT 'OK'");
				}

				errMsg = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});

			thread.Start();
			thread.Join();

			AssertEquals("OK", result);
			AssertEquals("", errMsg);
		}

		public void TestDisposeThreadConnection()
		{
			var originalHashCode = 0;
			var beforeHashCode = 0;
			var afterHashCode = 0;
			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					originalHashCode = Db.Connection.GetHashCode();
					beforeHashCode = Db.Connection.GetHashCode();

					Db.DisposeThreadConnection();
					using (Db.DisposableActionForDbConnection())
					{
						afterHashCode = Db.Connection.GetHashCode();
					}
				}
			});

			thread.Start();
			thread.Join();

			AssertEquals("Connection should be reused", originalHashCode, beforeHashCode);
			AssertNotEquals("Connection should be reset", originalHashCode, afterHashCode);
		}

		public void TestDisposableActionForDbConnectionInsideATransaction()
		{
			var errMsg1 = "";
			var errMsg2 = "";

			var thread = new Thread(() =>
			{
				try
				{
					ErrorReporter.Clear();
					using (Db.Connection)
					{
						errMsg1 = ErrorReporter.LastMessageReported;

						Db.Connection.BeginTransaction();
						Db.Connection.ExecuteNonQuery("SELECT 'OK'");

						using (Db.DisposableActionForDbConnection())
						{
							Db.Connection.ExecuteScalar("SELECT 'OK'");
						}

						Db.Connection.CommitTransaction();

						ErrorReporter.Clear();
					}
				}
				catch (Exception ex)
				{
					errMsg2 = ex.ToString();
				}
			});

			thread.Start();
			thread.Join();

			AssertEquals("PRE: Should have reported accessing DbConnection without disposableaction", ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, errMsg1);
			AssertEquals("", errMsg2);
		}

		public void TestElevateToAdminConnectionForUpgrade()
		{
			var result1 = "";
			var result2 = "";
			var errMsg1 = "";
			var errMsg2 = "";

			MainConnection.Instance.ThreadSentry.RelinquishThreadOwnership();
			try
			{
				var thread = new Thread(() =>
				{
					MainConnection.Instance.ThreadSentry.TakeThreadOwnership();

					using (Db.DisposableActionForDbConnection())
					{
						var dbConnection = Db.Connection;

						using ((Db.Instance as IDbUpgradeSupport).ElevateToAdminConnectionForUpgrade())
						{
							errMsg1 = ErrorReporter.LastMessageReported;
							ErrorReporter.Clear();
							result1 = (string)Db.Connection.ExecuteScalar("SELECT 'OK'");
						}

						result2 = (string)dbConnection.ExecuteScalar("SELECT 'OK2'");
						errMsg2 = ErrorReporter.LastMessageReported;
					}

					MainConnection.Instance.ThreadSentry.RelinquishThreadOwnership();
				});

				thread.Start();
				thread.Join();
			}
			finally
			{
				MainConnection.Instance.ThreadSentry.TakeThreadOwnership();
			}

			AssertEquals("OK", result1);
			AssertEquals("OK2", result2);
			AssertEquals("", errMsg1);
			AssertEquals("", errMsg2);
		}

		public void TestUseReportUsingDbConnection()
		{
			var messageReported = "";
			var key2 = "";

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();
				using (var dbConnection = Db.Connection)
				{
					messageReported = ErrorReporter.LastMessageReported;
					ErrorReporter.Clear();
				}
				using (var dbConnection = Db.Connection)
				{
					key2 = ErrorReporter.LastKeyReported;
					ErrorReporter.Clear();
				}
			});

			thread.Start();
			thread.Join();

			AssertContains("Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()", messageReported);
			AssertEquals("", key2);
		}

		/// <summary>
		/// When in WinForm Environment (Global.IsWeb = FALSE) and isUsingMainConnectionAcrossAllThreads = FALSE
		/// The Main Thread should use the MainConnection Instance and all other threads should use its own separate DbConnection.
		/// </summary>
		public void TestDbConnectionMultiThreadingInstances_ForWinFormEnvironment()
		{
			try
			{
				Db.Instance.IsWebTestOverride = false;

				InitialiseMainThreadConnectionsAndAssertPreconditions();
				AssertMainThreadAlwaysUseMainConnectionInstance_IfNotIsWeb();

				StartMultipleThreadsAndStoreDbConnectionInstance();

				AssertMainConnectionInstanceIsTheSameInAllThreads();
				AssertOtherThreadsUseOwnSeparateExtraConnection_IfIsWebOrNotUsingMainConnectionForAllThreads();
				AssertSameExtraConnectionAlwaysUsedInAGivenNonMainThread();
			}
			finally
			{
				Db.Instance.IsWebTestOverride = null;
			}
		}

		/// <summary>
		/// When in WinForm Environment (Global.IsWeb = FALSE) and isUsingMainConnectionAcrossAllThreads = TRUE
		/// All threads should use the MainConnection Instance.
		/// </summary>
		public void TestDbConnectionMultiThreadingInstances_ForWinFormEnvironmentUsingMainConnectionAcrossAllThreads()
		{
			try
			{
				Db.Instance.IsWebTestOverride = false;

				using ((Db.Instance as IDbConnectionMultiThreadControl).UseMainConnectionAcrossAllThreadsForTests())
				{
					InitialiseMainThreadConnectionsAndAssertPreconditions();
					AssertMainThreadAlwaysUseMainConnectionInstance_IfNotIsWeb();

					StartMultipleThreadsAndStoreDbConnectionInstance();

					AssertMainConnectionInstanceIsTheSameInAllThreads();
					AssertEquals("Db.Connection Type From Other Thread", typeof(MainConnection), Connection1FromOtherThread.GetType());
					AssertEquals("The same Db.Connection object should be returned for every thread", true, Object.ReferenceEquals(Connection1FromOtherThread, OriginalMainConnectionForTest));
					AssertSameExtraConnectionAlwaysUsedInAGivenNonMainThread();
				}
			}
			finally
			{
				Db.Instance.IsWebTestOverride = null;
			}
		}

		/// <summary>
		/// When in Web Environment (Global.IsWeb = TRUE), it should use a separate DbConnection for each thread.
		/// Even setting isUsingMainConnectionAcrossAllThreads = TRUE will not change this behaviour.
		/// </summary>
		public void TestDbConnectionMultiThreadingInstances_ForWebEnvironment()
		{
			try
			{
				Db.Instance.IsWebTestOverride = true;

				using ((Db.Instance as IDbConnectionMultiThreadControl).UseMainConnectionAcrossAllThreadsForTests())
				using (Db.DisposableActionForDbConnection())
				{
					InitialiseMainThreadConnectionsAndAssertPreconditions();
					AssertEquals("Db.Connection Type in the original thread", typeof(RestrictedWriterConnection), OriginalConnectionForTest.GetType());
					Assert("Db.Connection should not return MainConnection Instance in the original thread", !Object.ReferenceEquals(OriginalConnectionForTest, OriginalMainConnectionForTest));

					StartMultipleThreadsAndStoreDbConnectionInstance();

					AssertMainConnectionInstanceIsTheSameInAllThreads();
					AssertOtherThreadsUseOwnSeparateExtraConnection_IfIsWebOrNotUsingMainConnectionForAllThreads();
					AssertSameExtraConnectionAlwaysUsedInAGivenNonMainThread();
				}
			}
			finally
			{
				Db.Instance.IsWebTestOverride = null;
			}
		}

		void InitialiseMainThreadConnectionsAndAssertPreconditions()
		{
			OriginalConnectionForTest = Db.Connection;
			OriginalMainConnectionForTest = MainConnection.Instance;

			AssertNotNull("[PRECONDITION] OriginalConnectionForTest is not null", OriginalConnectionForTest);
			AssertNotNull("[PRECONDITION] OriginalMainConnectionForTest is not null", OriginalMainConnectionForTest);

			AssertEquals("MainConnection.Instance should always be the same object", true, Object.ReferenceEquals(OriginalMainConnectionForTest, MainConnection.Instance));
			AssertEquals("Db.Connection should the same object in the same thread", true, Object.ReferenceEquals(OriginalConnectionForTest, Db.Connection));
		}

		void AssertMainConnectionInstanceIsTheSameInAllThreads()
		{
			AssertEquals("MainConnectionFromOtherThread Type", typeof(MainConnection), MainConnectionFromOtherThread.GetType());
			AssertEquals("The same MainConnection Instance should be returned for all threads", true, Object.ReferenceEquals(MainConnectionFromOtherThread, OriginalMainConnectionForTest));
		}

		void AssertMainThreadAlwaysUseMainConnectionInstance_IfNotIsWeb()
		{
			Assert("[PRECONDITION] Should NOT be in Web Environment", !Db.Instance.IsWebTestOverride.Value);
			AssertEquals("Db.Connection Type in the original thread", typeof(MainConnection), OriginalConnectionForTest.GetType());
			AssertEquals("Db.Connection should return MainConnection Instance in the original thread", true, Object.ReferenceEquals(OriginalConnectionForTest, OriginalMainConnectionForTest));
		}

		void AssertOtherThreadsUseOwnSeparateExtraConnection_IfIsWebOrNotUsingMainConnectionForAllThreads()
		{
			AssertEquals("Db.Connection Type From Other Thread", typeof(RestrictedWriterConnection), Connection1FromOtherThread.GetType());
			Assert("A different Db.Connection should be returned for each thread", !Object.ReferenceEquals(Connection1FromOtherThread, OriginalConnectionForTest));
		}

		void AssertSameExtraConnectionAlwaysUsedInAGivenNonMainThread()
		{
			Assert("1st and 2nd Connection from same other thread should be equal", Object.ReferenceEquals(Connection2FromOtherThread, Connection1FromOtherThread));
		}

		void StartMultipleThreadsAndStoreDbConnectionInstance()
		{
			Thread otherThread = null;

			try
			{
				otherThread = new Thread(new ThreadStart(MultiThreadingTestMethod));
				otherThread.Start();
			}
			finally
			{
				if (otherThread != null)
				{
					otherThread.Join(10000);
				}
			}
		}

		void MultiThreadingTestMethod()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Connection1FromOtherThread = Db.Connection;
				Connection2FromOtherThread = Db.Connection;
				MainConnectionFromOtherThread = MainConnection.Instance;
			}
		}

		DbConnection OriginalConnectionForTest;
		DbConnection OriginalMainConnectionForTest;
		DbConnection Connection1FromOtherThread;
		DbConnection Connection2FromOtherThread;
		DbConnection MainConnectionFromOtherThread;

		protected override void TearDown()
		{
			if (OriginalConnectionForTest != null)
			{
				OriginalConnectionForTest.Dispose();
			}

			if (OriginalMainConnectionForTest != null)
			{
				OriginalMainConnectionForTest.Dispose();
			}

			if (MainConnectionFromOtherThread != null)
			{
				MainConnectionFromOtherThread.Dispose();
			}
		}
	}
}
