using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.MasterData.Testing
{
	[TestedType(typeof(RemovePatternMatchingDataForConstraints))]
	public class RemovePatternMatchingDataForConstraintsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(4, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingAddress WHERE PMA_ParentTableCode IN ('GS', 'HA', 'OA', 'PER') and PMA_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingAddress WHERE PMA_ParentTableCode NOT IN ('GS', 'HA', 'OA', 'PER')"));

			AssertEquals(4, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingDomain WHERE PMD_ParentTableCode IN ('HA', 'OA', 'OC', 'PU') and PMD_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingDomain WHERE PMD_ParentTableCode NOT IN ('HA', 'OA', 'OC', 'PU')"));

			AssertEquals(6, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingEmail WHERE PME_ParentTableCode IN ('GS', 'HA', 'OA', 'OC', 'OI', 'PER') and PME_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingEmail WHERE PME_ParentTableCode NOT IN ('GS', 'HA', 'OA', 'OC', 'OI', 'PER')"));

			AssertEquals(7, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingName WHERE PMN_ParentTableCode IN ('GS', 'HA', 'OA', 'OC', 'OH', 'P1', 'PER') and PMN_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingName WHERE PMN_ParentTableCode NOT IN ('GS', 'HA', 'OA', 'OC', 'OH', 'P1', 'PER')"));

			AssertEquals(6, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingPhone WHERE PMP_ParentTableCode IN ('GS', 'HA', 'OA', 'OC', 'OI', 'PER') and PMP_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingPhone WHERE PMP_ParentTableCode NOT IN ('GS', 'HA', 'OA', 'OC', 'OI', 'PER')"));

			AssertEquals(6, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingRegCode WHERE PMR_ParentTableCode IN ('GS', 'HA', 'OC', 'OK', 'PER', 'XZ') and PMR_HashedValue = '1566845912'"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingRegCode WHERE PMR_ParentTableCode NOT IN ('GS', 'HA', 'OC', 'OK', 'PER', 'XZ')"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemovePatternMatchingDataForConstraints();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingAddressSchema.Constants.TableName, "Constraint_PMA_ParentTableCode_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingDomainSchema.Constants.TableName, "Constraint_PMD_ParentTableCode_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingEmailSchema.Constants.TableName, "Constraint_PME_ParentTableCode_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingNameSchema.Constants.TableName, "Constraint_PMN_ParentTableCode_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingPhoneSchema.Constants.TableName, "Constraint_PMP_ParentTableCode_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(PatternMatchingRegCodeSchema.Constants.TableName, "Constraint_PMR_ParentTableCode_NoCheck");

			var orgPk = Guid.NewGuid();
			var sql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser)
    VALUES ('{orgPk}', 'ORG01', 'ORGHEADER 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.PatternMatchingAddress (PMA_PK, PMA_ParentTableCode, PMA_ParentId, PMA_PER, PMA_HashedValue, PMA_SystemLastEditTimeUtc, PMA_SystemLastEditUser, PMA_SystemCreateTimeUtc, PMA_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'GS', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.PatternMatchingDomain (PMD_PK, PMD_ParentTableCode, PMD_ParentId, PMD_PER, PMD_HashedValue, PMD_SystemLastEditTimeUtc, PMD_SystemLastEditUser, PMD_SystemCreateTimeUtc, PMD_SystemCreateUser)
	VALUES
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PU', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP');


INSERT INTO dbo.PatternMatchingEmail (PME_PK, PME_ParentTableCode, PME_ParentId, PME_PER, PME_HashedValue, PME_SystemLastEditTimeUtc, PME_SystemLastEditUser, PME_SystemCreateTimeUtc, PME_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'GS', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OI', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.PatternMatchingName (PMN_PK, PMN_ParentTableCode, PMN_ParentId, PMN_PER, PMN_HashedValue, PMN_SystemLastEditTimeUtc, PMN_SystemLastEditUser, PMN_SystemCreateTimeUtc, PMN_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'GS', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OH', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'P1', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.PatternMatchingPhone (PMP_PK, PMP_ParentTableCode, PMP_ParentId, PMP_PER, PMP_HashedValue, PMP_SystemLastEditTimeUtc, PMP_SystemLastEditUser, PMP_SystemCreateTimeUtc, PMP_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'GS', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OI', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.PatternMatchingRegCode (PMR_PK, PMR_ParentTableCode, PMR_ParentId, PMR_PER, PMR_HashedValue, PMR_IsActive, PMR_SystemLastEditTimeUtc, PMR_SystemLastEditUser, PMR_SystemCreateTimeUtc, PMR_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'GS', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
    ('{Guid.NewGuid()}', 'HA', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'XZ', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OK', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'AA', '{Guid.NewGuid()}', NULL, '1566845912', '1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
