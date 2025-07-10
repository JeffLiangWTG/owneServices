using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.RemoteDeviceManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.RemoteDeviceManagement.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeDeviceIdentifierOnceSet))]
	[UseSnapshotProtection]
	class TG_CannotChangeDeviceIdentifierOnceSetTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_WhenIsTemplate_And_DeviceIdentifier_IsBlank_ShouldAllowUpdate() => TestUpdateDeviceIdentifier(isTemplate: true, hasValue: false);
		public void TestTrigger_WhenIsTemplate_And_DeviceIdentifier_HasValue_ShouldAllowUpdate() => TestUpdateDeviceIdentifier(isTemplate: true, hasValue: true);
		public void TestTrigger_WhenNotTemplate_And_DeviceIdentifier_IsBlank_ShouldAllowUpdate() => TestUpdateDeviceIdentifier(isTemplate: false, hasValue: false);
		public void TestTrigger_WhenNotTemplate_And_DeviceIdentifier_HasValue_ShouldPreventUpdate() => TestUpdateDeviceIdentifier(isTemplate: false, hasValue: true, expectException: true);
		public void TestTrigger_WhenNotTemplate_And_DeviceIdentifier_HasValue_ShouldPreventValueReset() => TestUpdateDeviceIdentifier(isTemplate: false, hasValue: true, expectException: true, newValue: string.Empty);
		public void TestTrigger_WhenNotTemplate_And_UpdatedToSameValue_ShouldNotThrow() => TestUpdateDeviceIdentifier(isTemplate: false, hasValue: true, expectException: false, initialValue: "Pepe Pecas", newValue: "Pepe Pecas");

		void TestUpdateDeviceIdentifier(bool isTemplate, bool hasValue, bool expectException = false, string initialValue = "foo", string newValue = "bar")
		{
			var deviceHeader = InsertDeviceHeader(isTemplate, hasValue ? initialValue : string.Empty);
			var updateSQL = $"UPDATE dbo.DmgDeviceHeader SET CDH_DeviceIdentifier = '{newValue}', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
			if (expectException)
			{
				AssertSqlExceptionThrown(updateSQL);
			}
			else
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(updateSQL));
			}
		}

		#region Implementation

		DmgDeviceHeader InsertDeviceHeader(bool isTemplate, string deviceIdentifier)
		{
			var sql = new SqlQueryBuilder();
			var device = new DmgDeviceHeader()
			{
				CDH_ModelID = "D",
				CDH_IsTemplate = isTemplate,
				CDH_Identifier = "01",
				CDH_Description = "My device",
				CDH_DeviceKind = "UNK",
				CDH_DeviceIdentifier = deviceIdentifier,
				CDH_Status = "ACT",
			}.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return device;
		}

		void AssertSqlExceptionThrown(string updateSQL)
		{
			AssertExceptionThrown(
				typeof(SqlException),
				"The Device Identifier can only be modified when the device is a template.",
				() => TestConnection.ExecuteNonQuery(updateSQL),
				true);
		}
		#endregion
	}
}

