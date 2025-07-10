using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainGetAppLock))]
	class FountainGetAppLockTest : NumberFountainTestCase
	{
		public void TestFountainAppLockAquired()
		{
			var name = FountainName;
			var owner = FountainOwner;
			var fountainId = 9001;

			using (var newConnection = Db.NewExtraConnectionToMainDb())
			using (newConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
			{
				Helper.AssertAppLockMode(newConnection, fountainId, expected: Helper.AppLockMode.NoLock);
				Helper.AssertAppLockAvailable(newConnection, fountainId, expected: true);
				Helper.AssertAppLockMode(TestConnection, fountainId, expected: Helper.AppLockMode.NoLock);
				Helper.AssertAppLockAvailable(TestConnection, fountainId, expected: true);

				GetAppLock(newConnection, name, owner, fountainId);

				Helper.AssertAppLockMode(newConnection, fountainId, expected: Helper.AppLockMode.Exclusive);
				Helper.AssertAppLockAvailable(newConnection, fountainId, expected: true);
				Helper.AssertAppLockMode(TestConnection, fountainId, expected: Helper.AppLockMode.NoLock);
				Helper.AssertAppLockAvailable(TestConnection, fountainId, expected: false);
			}
		}

		public void TestException_FountainLockNotAcquired()
		{
			var name = FountainName;
			var owner = FountainOwner;
			var fountainId = 9001;

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
			{
				GetAppLock(blocker, name, owner, fountainId);
				Helper.AssertAppLockMode(blocker, fountainId, Helper.AppLockMode.Exclusive);

				using (TestConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						GetAppLock(TestConnection, name, owner, fountainId);
					});
					AssertContains($"[=FOUNTAIN_LOCK_NOT_ACQUIRED=]-1 (Reason: The lock request timed out (0 ms), ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.ToString(), ignoreCase: true);
				}
			}
		}

		#region Implementation

		void GetAppLock(DbConnection connection, string name, Guid owner, int fountainId)
		{
			using (var cmd = connection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, Helper.FountainNameMaxLength, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, null);
				cmd.GetParameter("@Resource").Direction = ParameterDirection.Output;

				cmd.ExecuteNonQuery();
				AssertEquals(Helper.GetAppLockResource(fountainId), cmd.GetParameterValue("@Resource"));
			}
		}
		#endregion // Implementation
	}
}

