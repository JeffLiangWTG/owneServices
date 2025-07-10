using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[UseSnapshotProtection]
	sealed class NumberFountainProxyTest : TestCase
	{
		public void TestGenerateCacheDoesNotCommitTransaction()
		{
			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				var countBefore = Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.StmNumberCache INNER JOIN dbo.StmNums ON SG_SN = SN_ID AND SN_Name = 'ContainerLogicalNumber' AND SG_IsUsed = 0");
				AssertEquals(0, countBefore);

				var numberFountain = new NumberFountain.NumberFountains().ContainerLogicalNumber;
				var proxy = new NumberFountainProxy(numberFountain);
				Db.Connection.BeginTransaction();
				try
				{
					// this call GenerateCache (by side effect, on another connection)
					var value = proxy.GetNextFormatted(Db.Connection); // value not important

					using (var secondConnection = Db.NewExtraConnectionToMainDb())
					{
						secondConnection.BeginTransaction();
						var value2 = proxy.GetNextFormatted(secondConnection); // value is now important
						AssertGreaterThan(value2, value);
					}
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		public void TestGenerateCacheCommitsTransaction_FailsInTestCaseIfNotInTransaction()
		{
			var numberFountain = new NumberFountain.NumberFountains().ContainerLogicalNumber;
			var proxy = (INumberFountainProxy)new NumberFountainProxy(numberFountain);
			var connection = Db.Connection;
			AssertEquals(false, connection.IsInTransaction);
			AssertEquals(false, connection.IsInTransactionOtherThanTransactionedTestCase);

			using (var manager = connection.BeginTransactionWithManager())
			{
				proxy.GetNextFormatted(connection); // value not important - This should not fail if wrapped in a tran, which is what we want during tests.
			}

			Exception thrownException = null;
			try
			{
				proxy.GetNextFormatted(connection);
			}
			catch (Exception e)
			{
				thrownException = e;
			}
			AssertContains("No transaction found", thrownException.Message);
			AssertEquals(typeof(Exception), thrownException.GetType());
		}
	}
}
