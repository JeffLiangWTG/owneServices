using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.RemoteDeviceManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.RemoteDeviceManagement.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeKeyIdentifier))]
	[UseSnapshotProtection]
	class TG_CannotChangeKeyIdentifierTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_CDH_Identifier_Update_ShouldPrevent()
		{
			var deviceHeader = InsertDeviceHeader(identifier: "01");
			var updateSQL = $@"UPDATE dbo.DmgDeviceHeader SET CDH_Identifier = 'foo', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
			AssertSqlExceptionThrown(updateSQL);
		}

		public void TestTrigger_CDH_Identifier_UpdateToSameValue_ShouldNotThrow()
		{
			var deviceHeader = InsertDeviceHeader(identifier: "01");
			var updateSQL = $@"UPDATE dbo.DmgDeviceHeader SET CDH_Identifier = '01', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(updateSQL));
		}

		#region Implementation

		DmgDeviceHeader InsertDeviceHeader(string identifier)
		{
			var sql = new SqlQueryBuilder();
			var device = new DmgDeviceHeader()
			{
				CDH_ModelID = "D",
				CDH_IsTemplate = false,
				CDH_Identifier = identifier,
				CDH_Description = "My device",
				CDH_DeviceKind = "UNK",
				CDH_DeviceIdentifier = "D01",
				CDH_Status = "ACT",
			}.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return device;
		}

		void AssertSqlExceptionThrown(string updateSQL)
		{
			AssertExceptionThrown(
				typeof(SqlException),
				"The Identifier key cannot be modified for an existing record.",
				() => TestConnection.ExecuteNonQuery(updateSQL),
				true);
		}
		#endregion
	}
}

