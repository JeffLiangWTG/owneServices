using System;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing;

[TestedType(typeof(UpdateEmptyStmNoteContextToDefaultValue))]
public class UpdateEmptyStmNoteContextToDefaultValueTest : DataTransformationTestCase
{
	readonly Guid testGuid = Guid.NewGuid();

	readonly Guid testGuidWithNullParentId = Guid.NewGuid();

	protected override void PrepareTestData()
	{
		Db.Connection.ExecuteNonQuery(@"IF (OBJECT_ID('Constraint_ST_NoteContext_NoCheck', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE dbo.StmNote NOCHECK CONSTRAINT Constraint_ST_NoteContext_NoCheck
										END");
		var helper = new TestDbHelper(TestConnection);
		helper.Insert(StmNoteSchema.Constants.TableName, new
		{
			ST_PK = testGuid,
			ST_ParentID = Guid.NewGuid(),
			ST_Table = "JobDeclaration",
			ST_NoteContext = ""
		});

		helper.Insert(StmNoteSchema.Constants.TableName, new
		{
			ST_PK = testGuidWithNullParentId,
			ST_Table = "JobDeclaration",
			ST_NoteContext = ""
		});
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateEmptyStmNoteContextToDefaultValue();
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_NoteContext = ''"));
		AssertEquals("???", TestConnection.ExecuteScalar<string>($"SELECT ST_NoteContext FROM dbo.StmNote WHERE ST_PK = '{testGuid}'"));
		AssertEquals("???", TestConnection.ExecuteScalar<string>($"SELECT ST_NoteContext FROM dbo.StmNote WHERE ST_PK = '{testGuidWithNullParentId}'"));
	}
}