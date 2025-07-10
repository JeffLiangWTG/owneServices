using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles;

[UseSnapshotProtection]
[TestedType(typeof(RenameP0V_P0_ParentColumnTransformation))]
sealed class RenameP0V_P0_ParentColumnTransformationTest :  DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new RenameP0V_P0_ParentColumnTransformation();

	protected override void PrepareTestData()
	{
		RenameColumn(ProcessTemplateValidationSchema.Constants.TableName, ProcessTemplateValidationSchema.Constants.P0V_P0_WorkflowTemplate, "P0V_P0_Parent");
		TestConnection.ExecuteNonQuery($"INSERT INTO dbo.ProcessTaskTemplate(P0_PK, P0_IsSystem, P0_IsActive, P0_Name, P0_SystemCreateTimeUtc, P0_SystemCreateUser, P0_SystemLastEditTimeUtc, P0_SystemLastEditUser) VALUES('{templatePK}', 0, 1,'TEMPLATE 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		TestConnection.ExecuteNonQuery($"INSERT INTO dbo.ProcessTemplateValidation(P0V_PK, P0V_P0_Parent, P0V_Description, P0V_Severity, P0V_SystemCreateTimeUtc, P0V_SystemCreateUser, P0V_SystemLastEditTimeUtc, P0V_SystemLastEditUser) VALUES (0x1,'{templatePK}','ABRACADABRA', 'ERR', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("Old column doesn't exist", false, DbObjectCreator.ColumnExists(TestConnection, ProcessTemplateValidationSchema.Constants.TableName, "P0V_P0_Parent"));
		AssertEquals("New column exists", true, DbObjectCreator.ColumnExists(TestConnection, ProcessTemplateValidationSchema.Constants.TableName, ProcessTemplateValidationSchema.Constants.P0V_P0_WorkflowTemplate));
		AssertEquals("Data is still there", "ABRACADABRA", TestConnection.ExecuteScalar<string>($"SELECT P0V_Description FROM dbo.ProcessTemplateValidation WHERE P0V_PK = 0x1 AND P0V_P0_WorkflowTemplate = '{templatePK}'"));
	}

	void RenameColumn(string tableName, string oldColumnName, string newColumnName)
	{
		new DbColumnDependencyRemover(tableName, oldColumnName).DropRelateObjects(TestConnection);
		DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, tableName, oldColumnName, newColumnName);
	}

	readonly Guid templatePK = Guid.NewGuid();
}
