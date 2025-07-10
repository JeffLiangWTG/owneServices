using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.RemoteDeviceManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.RemoteDeviceManagement.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeDeviceTemplateFlag))]
	[UseSnapshotProtection]
	class TG_CannotChangeDeviceTemplateFlagTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_CDH_IsTemplate_UpdateToTrue_ShouldPrevent() => TestIsTemplateUpdate(from: false, to: true);
		public void TestTrigger_CDH_IsTemplate_UpdateToFalse_ShouldPrevent() => TestIsTemplateUpdate(from: true, to: false);
		public void TestTrigger_CDH_IsTemplate_UpdateToSame_ShouldNotThrow() => TestIsTemplateUpdate(from: true, to: true, expectException: false);

		void TestIsTemplateUpdate(bool from, bool to, bool expectException = true)
		{
			var deviceHeader = InsertDeviceHeader(isTemplate: from);
			var updateSQL = $@"UPDATE dbo.DmgDeviceHeader SET CDH_IsTemplate = '{to}', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
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

		DmgDeviceHeader InsertDeviceHeader(bool isTemplate = false)
		{
			var sql = new SqlQueryBuilder();
			var device = new DmgDeviceHeader()
			{
				CDH_ModelID = "D",
				CDH_IsTemplate = isTemplate,
				CDH_Identifier = "01",
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
				"The IsTemplate flag cannot be modified for an existing record.",
				() => TestConnection.ExecuteNonQuery(updateSQL),
				true);
		}
		#endregion
	}
}

