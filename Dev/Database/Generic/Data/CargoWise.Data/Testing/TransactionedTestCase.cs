#if DEBUG

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;

namespace NUnit.Framework
{
	public abstract class TransactionedTestCase : TestCase, ITransactionalTestInternals
	{
		protected override void RunTestInAnotherThreadWithTimeout(Action<CancellationToken> action, TimeSpan timeout)
		{
			throw new NotSupportedException("Please manage connections yourself - this will not work as you expect");
		}

		protected virtual DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		public void RestartTransaction()
		{
			if (TestConnection.AppTransactionCount == 1)
			{
				TestConnection.RollbackTransaction();
				TestConnection.BeginTransaction();
			}
			else
			{
				throw new ApplicationException("Cannot restart transaction - Transaction count is " + TestConnection.AppTransactionCount.ToString());
			}
		}

		protected void RegisterDbConnectionToRollback(DbConnection connection)
		{
			if (!SkipTransaction && !ConnectionsInTransactions.Contains(connection))
			{
				AssertEquals(TransactionLevelErrorMessages.Initial, 0, connection.AppTransactionCount);
				connection.BeginTransaction();
				ConnectionsInTransactions.Add(connection);
				AssertEquals(TransactionLevelErrorMessages.Before, 1, connection.AppTransactionCount);
			}
		}

		bool SkipTransaction
		{
			get
			{
				var methodAttributes = (UseSnapshotProtectionAttribute[])RunMethod.GetCustomAttributes(typeof(UseSnapshotProtectionAttribute), true);
				var classAttributes = (UseSnapshotProtectionAttribute[])GetType().GetCustomAttributes(typeof(UseSnapshotProtectionAttribute), true);
				return classAttributes.Concat(methodAttributes).Any(attribute => attribute.SkipTransaction);
			}
		}

		void RollbackRegisteredConnections()
		{
			foreach (DbConnection connection in ConnectionsInTransactions)
			{
				int currentTransactionCount = connection.AppTransactionCount;

				while (currentTransactionCount > 0)
				{
					connection.RollbackTransaction();
					AssertEquals("Transaction count should lower by 1 as it is rolledback", currentTransactionCount - 1, connection.AppTransactionCount);
					currentTransactionCount = connection.AppTransactionCount;
				}

				AssertEquals("Transaction count should be 0 after test", 0, connection.AppTransactionCount);
				AssertEquals("Connection should not be in transaction", false, connection.IsInTransaction);
			}
			ConnectionsInTransactions.Clear();
		}

		readonly System.Collections.Generic.List<DbConnection> ConnectionsInTransactions = new System.Collections.Generic.List<DbConnection>();

		protected IDisposable RunNonTransactioned()
		{
			RollbackRegisteredConnections();
			inTransactionedTestCase = false;
			return new DisposableAction(() =>
			{
				RegisterDbConnectionToRollback(TestConnection);
				inTransactionedTestCase = true;
			});
		}

		public static class TransactionLevelErrorMessages
		{
			public const string Initial = "Initial Transaction Level";
			public const string Before = "Transaction Level before running test";
			public const string After = "Transactions begun did not match commits and rollbacks. Transaction Level after running test";
		}

		protected void AssertTableRowCount(string tableName, int expectedRowCount, string message = null)
		{
			var assertionMessage = FormattableString.Invariant($"Expected {expectedRowCount} row(s) in table {tableName}. {message}");
			var count = TestConnection.ExecuteScalar("SELECT COUNT(*) FROM " + tableName);

			AssertEquals(assertionMessage, expectedRowCount, count);
		}

		#region InTransactionedTestCase

		public static bool InTransactionedTestCase
		{
			get { return inTransactionedTestCase; }
		}
		static bool inTransactionedTestCase;

		#region ISetInternalFlagInSeparateAppDomain Members

		void ITransactionalTestInternals.SetInTransactionedTestCase()
		{
			inTransactionedTestCase = true;
		}

		#endregion

		#region InReflectionTest

		public static bool InReflectionTest => inReflectionTest;

		static bool inReflectionTest;

		IDisposable ITransactionalTestInternals.SetInReflectionTestTemporary() => new DisposableAction(() => inReflectionTest = true, () => inReflectionTest = false);

		#endregion

		#endregion

		#region RunBare

		public override void RunBare()
		{
			DoRunBare();
		}

		protected void DoRunBare()
		{
			try
			{
				lock (this)
				{
					try
					{
						RegisterDbConnectionToRollback(TestConnection);
						inTransactionedTestCase = true;

						OnBeforeBaseTestCaseRunBare();

						base.RunBare();
						if (!SkipTransaction)
						{
							AssertEquals(TransactionLevelErrorMessages.After, 1, TestConnection.AppTransactionCount);
						}
					}
					finally
					{
						if (TestConnection != Db.Connection)
						{
							TestConnection.Dispose();
						}
					}
				}
			}
			finally
			{
				inTransactionedTestCase = false;
				RollbackRegisteredConnections();
				OnAfterBaseTestCaseRunBare();
			}
		}

		protected virtual void OnBeforeBaseTestCaseRunBare()
		{
		}

		protected virtual void OnAfterBaseTestCaseRunBare() // For drop database which have to wait for db connection rollback
		{
		}

		protected override void SetUp()
		{
			RunClientDbCreateScripts(); //Need it to happen after UseSnapshotProtectionAttribute runs so it's not in the snapshot (matters when skipSnapshot is true).
			base.SetUp();
		}

		public static void RunClientDbCreateScripts()
		{
			Type type = AssemblyLoader.LoadAssembly("Enterprise.ZArchitecture.Core").GetType("Enterprise.ZArchitecture.Core.Testing.ClientDbSchemaCreationForTesting");
			type.InvokeMember("RunClientDbCreateScripts", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), new object[] { false }, CultureInfo.InvariantCulture);
		}

		#endregion
	}
}

#endif
