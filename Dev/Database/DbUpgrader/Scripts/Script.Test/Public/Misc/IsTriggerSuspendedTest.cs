using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(IsTriggerSuspended))]
	class IsTriggerSuspendedTest : DbCreateScriptTest
	{
		#region TestIsTriggerSuspended

		public void TestIsTriggerSuspended()
		{
			foreach (var trigger in SuspendedTriggersTest.SuspendableTriggers)
			{
				using (TestConnection.BeginTransactionWithManager())
				{
					var commandSQL = $"SELECT Result FROM dbo.IsTriggerSuspended('{trigger}')";
					AssertEquals("Trigger is not suspended, function should return 0.", 0, TestConnection.ExecuteScalar(commandSQL));

					TestConnection.ExecuteNonQuery($"EXEC sp_getapplock @Resource = '{trigger}', @LockMode = 'Shared', @LockOwner = 'Transaction', @DbPrincipal = 'public'");
					AssertEquals("Trigger is suspended in *current* transaction, function should return 1.", 1, TestConnection.ExecuteScalar(commandSQL));
				}
			}
		}
		#endregion
	}
}
