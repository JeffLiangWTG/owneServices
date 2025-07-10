using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.RemoteDeviceManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.RemoteDeviceManagement.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeDeviceKindOnceSet))]
	[UseSnapshotProtection]
	class TG_CannotChangeDeviceKindOnceSetTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_WhenIsTemplate_WhenKind_AND_ShouldAllowUpdate() => TestUpdateKind(isTemplate: true, kind: "AND");
		public void TestTrigger_WhenIsTemplate_WhenKind_IOS_ShouldAllowUpdate() => TestUpdateKind(isTemplate: true, kind: "IOS");
		public void TestTrigger_WhenIsTemplate_WhenKind_EMB_ShouldAllowUpdate() => TestUpdateKind(isTemplate: true, kind: "EMB");
		public void TestTrigger_WhenIsTemplate_WhenKind_WIN_ShouldAllowUpdate() => TestUpdateKind(isTemplate: true, kind: "WIN");
		public void TestTrigger_WhenIsTemplate_WhenKind_UNK_ShouldAllowUpdate() => TestUpdateKind(isTemplate: true, kind: "UNK", newKind: "AND");
		public void TestTrigger_WhenIsTemplate_WhenKind_SameValue_ShouldNotThrow() => TestUpdateKind(isTemplate: true, kind: "AND", newKind: "AND");

		public void TestTrigger_WhenNotTemplate_WhenKind_AND_ShouldPreventUpdate() => TestUpdateKind(isTemplate: false, kind: "AND", expectException: true);
		public void TestTrigger_WhenNotTemplate_WhenKind_IOS_ShouldPreventUpdate() => TestUpdateKind(isTemplate: false, kind: "IOS", expectException: true);
		public void TestTrigger_WhenNotTemplate_WhenKind_EMB_ShouldPreventUpdate() => TestUpdateKind(isTemplate: false, kind: "EMB", expectException: true);
		public void TestTrigger_WhenNotTemplate_WhenKind_WIN_ShouldPreventUpdate() => TestUpdateKind(isTemplate: false, kind: "WIN", expectException: true);
		public void TestTrigger_WhenNotTemplate_WhenKind_UNK_ShouldAllowUpdate() => TestUpdateKind(isTemplate: false, kind: "UNK", newKind: "AND");
		public void TestTrigger_WhenNotTemplate_WhenKind_SameValue_ShouldNotThrow() => TestUpdateKind(isTemplate: false, kind: "AND", newKind: "AND");

		void TestUpdateKind(bool isTemplate, string kind, string newKind = "UNK", bool expectException = false)
		{
			var deviceHeader = InsertDeviceHeader(isTemplate, kind);
			var updateSQL = $"UPDATE dbo.DmgDeviceHeader SET CDH_DeviceKind = '{newKind}', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
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

		DmgDeviceHeader InsertDeviceHeader(bool isTemplate, string kind)
		{
			var sql = new SqlQueryBuilder();
			var device = new DmgDeviceHeader()
			{
				CDH_ModelID = "D",
				CDH_IsTemplate = isTemplate,
				CDH_Identifier = "01",
				CDH_Description = "My device",
				CDH_DeviceKind = kind,
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
				"The Device Kind can only be modified when the device is a template.",
				() => TestConnection.ExecuteNonQuery(updateSQL),
				true);
		}
		#endregion
	}
}

