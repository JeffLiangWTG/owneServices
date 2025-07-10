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
[TestedType(typeof(RenameGlbReleaseNoteReadColumnsTransformation))]
public sealed class RenameGlbReleaseNoteReadColumnsTransformationTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new RenameGlbReleaseNoteReadColumnsTransformation();

	protected override void PrepareTestData()
	{
		EnsureOldColumnExists(OldStaffColumnName, NewStaffColumnName);
		EnsureOldColumnExists(OldReleaseNoteColumnName, NewReleaseNoteColumnName);

		var sqlInsertStaffRecord = $@"INSERT INTO dbo.{GlbStaffSchema.Constants.TableName}({GlbStaffSchema.Constants.PK}, {GlbStaffSchema.Constants.GS_Code}, {GlbStaffSchema.Constants.GS_SystemCreateTimeUtc}, {GlbStaffSchema.Constants.GS_SystemCreateUser}, {GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc}, {GlbStaffSchema.Constants.GS_SystemLastEditUser})
VALUES(0x4, 'CO1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')";
		var sqlInsertReleaseNoteReadRecord = $@"INSERT INTO dbo.{GlbReleaseNoteReadSchema.Constants.TableName}({GlbReleaseNoteReadSchema.Constants.PK}, GR_GF, GR_GS, {GlbReleaseNoteReadSchema.Constants.GR_IsValid}, {GlbReleaseNoteReadSchema.Constants.GR_SystemCreateUser}, {GlbReleaseNoteReadSchema.Constants.GR_SystemCreateTimeUtc})
VALUES(0x2, 0x3, 0x4, 1, 'E', GETUTCDATE())";

		TestConnection.ExecuteNonQuery(sqlInsertStaffRecord);
		TestConnection.ExecuteNonQuery(sqlInsertReleaseNoteReadRecord);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("Old column 'GR_GS' does not exist", false, DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, OldStaffColumnName));
		AssertEquals("Old column 'GR_GF' does not exist", false, DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, OldReleaseNoteColumnName));
		AssertEquals("New column 'GR_GS_Staff' exist", true, DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, NewStaffColumnName));
		AssertEquals("New column 'GR_ReleaseNoteID' exist", true, DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, NewReleaseNoteColumnName));

		var sql = $"SELECT {GlbReleaseNoteReadSchema.Constants.GR_IsValid} FROM dbo.{GlbReleaseNoteReadSchema.Constants.TableName} WHERE {GlbReleaseNoteReadSchema.Constants.PK} = 0x2 AND {NewReleaseNoteColumnName} = 0x3 AND {NewStaffColumnName} = 0x4";

		AssertEquals("Data is still there", true, TestConnection.ExecuteScalar<bool>(sql));
	}

	void EnsureOldColumnExists(string oldColumnName, string newColumnName)
	{
		if (DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, newColumnName))
		{
			new DbColumnDependencyRemover(GlbReleaseNoteReadSchema.Constants.TableName, newColumnName).DropRelateObjects(TestConnection);
			DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, GlbReleaseNoteReadSchema.Constants.TableName, newColumnName, oldColumnName);
		}
		else if (!DbObjectCreator.ColumnExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, oldColumnName))
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GlbReleaseNoteReadSchema.Constants.TableName, oldColumnName, ColumnInfo);
		}
	}

	const string NewStaffColumnName = "GR_GS_Staff";
	const string NewReleaseNoteColumnName = "GR_ReleaseNoteID";
	const string OldStaffColumnName = "GR_GS";
	const string OldReleaseNoteColumnName = "GR_GF";
	const string ColumnInfo = "uniqueidentifier";
}
