using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Scim;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Scim
{
	[TestedType(typeof(RemoveExternalIdForSystemGroups))]
	public class RemoveExternalIdForSystemGroupsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveExternalIdForSystemGroups();
		}

		Guid pk1 = Guid.NewGuid();
		Guid pk2 = Guid.NewGuid();
		Guid pk3 = Guid.NewGuid();
		Guid pk4 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var sql = @$"
INSERT INTO GlbGroup (GG_PK, GG_Code, GG_IsSystemDefined, GG_ExternalId, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{pk1}', 'GG_001', 0, 'ext1', getdate(), 'E', getdate(), 'E');
INSERT INTO GlbGroup (GG_PK, GG_Code, GG_IsSystemDefined, GG_ExternalId, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{pk2}', 'GG_002', 1, 'ext2', getdate(), 'E', getdate(), 'E');
INSERT INTO GlbGroup (GG_PK, GG_Code, GG_IsSystemDefined, GG_ExternalId, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{pk3}', 'GG_003', 0, 'ext3', getdate(), 'E', getdate(), 'E');
INSERT INTO GlbGroup (GG_PK, GG_Code, GG_IsSystemDefined, GG_ExternalId, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{pk4}', 'GG_004', 1, 'ext4', getdate(), 'E', getdate(), 'E');
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();
			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM GlbGroup where GG_IsSystemDefined = 1 AND GG_ExternalId <> ''"));
			AssertEquals(1, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM GlbGroup where GG_PK = '{pk1}' and GG_ExternalId <> ''"));
			AssertEquals(1, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM GlbGroup where GG_PK = '{pk3}' and GG_ExternalId <> ''"));
		}
	}
}
