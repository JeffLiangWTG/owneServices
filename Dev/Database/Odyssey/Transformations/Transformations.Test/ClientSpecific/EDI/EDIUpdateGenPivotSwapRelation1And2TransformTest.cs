using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(EDIUpdateGenPivotSwapRelation1And2Transform))]
	internal class EDIUpdateGenPivotSwapRelation1And2TransformTest : DataTransformationTestCase
	{
		static readonly string WrongDataNeedToBeDeletedQuerySql = "SELECT COUNT(*) FROM dbo.GenPivot WHERE XX_PK IN ('8F070915-9053-483A-804F-2C7B30310b01', '8F070915-9053-483A-804F-2C7B30310b02');";

		static readonly string WrongDataNeedToBeUpdatedQuerySql = "SELECT COUNT(*) FROM dbo.GenPivot WHERE XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'WKI' AND XX_Relation2TableCode = 'IM' AND XX_PK NOT IN ('8F070915-9053-483A-804F-2C7B30310b01', '8F070915-9053-483A-804F-2C7B30310b02');";

		static readonly string CorrectDataNotIncludeAlreadyExistsQuerySql = "SELECT COUNT(*) FROM dbo.GenPivot WHERE XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'IM' AND XX_Relation2TableCode = 'WKI' AND XX_AutoVersion = 1;";

		static readonly string CorrectDataAlreadyExistsQuerySql = "SELECT COUNT(*) FROM dbo.GenPivot WHERE XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'IM' AND XX_Relation2TableCode = 'WKI' AND XX_AutoVersion = 0;";

		protected override void AssertTransformationResults()
		{
			AssertEquals(0, TestConnection.ExecuteScalar<int>(WrongDataNeedToBeDeletedQuerySql));
			AssertEquals(0, TestConnection.ExecuteScalar<int>(WrongDataNeedToBeUpdatedQuerySql));
			AssertEquals(9, TestConnection.ExecuteScalar<int>(CorrectDataNotIncludeAlreadyExistsQuerySql));
			AssertEquals(2, TestConnection.ExecuteScalar<int>(CorrectDataAlreadyExistsQuerySql));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EDIUpdateGenPivotSwapRelation1And2Transform();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "GenPivot", @"
CREATE TABLE dbo.GenPivot
(
	[XX_PK] [uniqueidentifier] NOT NULL,
	[XX_AutoVersion] SMALLINT NOT NULL DEFAULT 0,
	[XX_RelationType] [varchar](3) NOT NULL,
	[XX_Relation1ID] [uniqueidentifier] NOT NULL,
	[XX_Relation2ID] [uniqueidentifier] NOT NULL,
	[XX_Sequence] [int] NOT NULL,
	[XX_Relation1TableCode] [varchar](3) NOT NULL,
	[XX_Relation2TableCode] [varchar](3) NOT NULL,
	[XX_SystemCreateTimeUtc] [smalldatetime] NULL,
	[XX_SystemCreateUser] [varchar](3) NOT NULL,
	[XX_SystemLastEditTimeUtc] [smalldatetime] NULL,
	[XX_SystemLastEditUser] [varchar](3) NOT NULL,
);");

			var sql = $@"
-- wrong data, need to be deleted
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310b01', 0, 'WRK', 'A81F79CF-02FC-4461-885C-18CD844A2B01', 'A006B996-532A-40F1-B8EB-426441188D01', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310b02', 0, 'WRK', 'A81F79CF-02FC-4461-885C-18CD844A2B02', 'A006B996-532A-40F1-B8EB-426441188D02', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
-- correct data, don't need to do anything
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310b03', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188D01', 'A81F79CF-02FC-4461-885C-18CD844A2B01', 0, 'IM', 'WKI', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310b04', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188D02', 'A81F79CF-02FC-4461-885C-18CD844A2B02', 0, 'IM', 'WKI', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
-- wrong data, need to be updated
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C01', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E01', 'A81F79CF-02FC-4461-885C-18CD844A2C01', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C02', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E02', 'A81F79CF-02FC-4461-885C-18CD844A2C02', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C03', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E03', 'A81F79CF-02FC-4461-885C-18CD844A2C03', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C04', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E04', 'A81F79CF-02FC-4461-885C-18CD844A2C04', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C05', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E05', 'A81F79CF-02FC-4461-885C-18CD844A2C05', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C06', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E06', 'A81F79CF-02FC-4461-885C-18CD844A2C06', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C07', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E07', 'A81F79CF-02FC-4461-885C-18CD844A2C07', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C08', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E08', 'A81F79CF-02FC-4461-885C-18CD844A2C08', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
INSERT INTO dbo.GenPivot (XX_PK, XX_AutoVersion, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C09', 0, 'WRK', 'A006B996-532A-40F1-B8EB-426441188E09', 'A81F79CF-02FC-4461-885C-18CD844A2C09', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');
";
			Db.Connection.ExecuteNonQuery(sql);
			AssertEquals(2, TestConnection.ExecuteScalar<int>(WrongDataNeedToBeDeletedQuerySql));
			AssertEquals(9, TestConnection.ExecuteScalar<int>(WrongDataNeedToBeUpdatedQuerySql));
			AssertEquals(0, TestConnection.ExecuteScalar<int>(CorrectDataNotIncludeAlreadyExistsQuerySql));
			AssertEquals(2, TestConnection.ExecuteScalar<int>(CorrectDataAlreadyExistsQuerySql));

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiBilledUsage", "CREATE TABLE dbo.EdiBilledUsage (ID INT)");
			AssertEquals(true, DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"));
		}
	}
}
