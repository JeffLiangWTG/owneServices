using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.RemoteDeviceManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.RemoteDeviceManagement.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeDeviceModelIdAndDescription))]
	[UseSnapshotProtection]
	class TG_CannotChangeDeviceModelIdAndDescriptionTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_UpdateModelID_WhenIsTemplate_ShouldAllow() => TestUpdateModelIdAndDescription(isTemplate: true, model: "M", description: "D", newModel: "foo", newDescription: "D");
		public void TestTrigger_UpdateModelID_WhenNotTemplate_ShouldPrevent() => TestUpdateModelIdAndDescription(isTemplate: false, model: "M", description: "D", newModel: "foo", newDescription: "D", expectException: true);

		public void TestTrigger_UpdateDescription_WhenIsTemplate_ShouldAllow() => TestUpdateModelIdAndDescription(isTemplate: true, model: "M", description: "D", newModel: "M", newDescription: "bar");
		public void TestTrigger_UpdateDescription_WhenNotTemplate_ShouldPrevent() => TestUpdateModelIdAndDescription(isTemplate: false, model: "M", description: "D", newModel: "M", newDescription: "bar", expectException: true);

		public void TestTrigger_UpdateModelAndDescription_WhenIsTemplate_ShouldAllow() => TestUpdateModelIdAndDescription(isTemplate: true, model: "M", description: "D", newModel: "foo", newDescription: "bar");
		public void TestTrigger_UpdateModelAndDescription_WhenNotTemplate_ShouldPrevent() => TestUpdateModelIdAndDescription(isTemplate: false, model: "M", description: "D", newModel: "foo", newDescription: "bar", expectException: true);
		public void TestTrigger_UpdateModelAndDescriptionToSameValue_WhenNotTemplate_ShouldAllow() => TestUpdateModelIdAndDescription(isTemplate: false, model: "foo", description: "bar", newModel: "foo", newDescription: "bar");

		void TestUpdateModelIdAndDescription(bool isTemplate, string model, string description, string newModel, string newDescription, bool expectException = false)
		{
			var deviceHeader = InsertDeviceHeader(isTemplate: isTemplate, model: model, description: description);
			var updateSQL = $"UPDATE dbo.DmgDeviceHeader SET CDH_ModelID = '{newModel}', CDH_Description = '{newDescription}', CDH_Status = 'DES' WHERE CDH_PK = '{deviceHeader.PK}'";
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

		DmgDeviceHeader InsertDeviceHeader(bool isTemplate, string model, string description)
		{
			var sql = new SqlQueryBuilder();
			var device = new DmgDeviceHeader()
			{
				CDH_ModelID = model,
				CDH_IsTemplate = isTemplate,
				CDH_Identifier = "01",
				CDH_Description = description,
				CDH_DeviceKind = "UNK",
				CDH_DeviceIdentifier = "D01",
				CDH_Status = "ACT"
			}.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return device;
		}

		void AssertSqlExceptionThrown(string updateSQL)
		{
			AssertExceptionThrown(
				typeof(SqlException),
				"The Device Model ID or Description can only be modified for a device template.",
				() => TestConnection.ExecuteNonQuery(updateSQL),
				true);
		}
		#endregion
	}
}

