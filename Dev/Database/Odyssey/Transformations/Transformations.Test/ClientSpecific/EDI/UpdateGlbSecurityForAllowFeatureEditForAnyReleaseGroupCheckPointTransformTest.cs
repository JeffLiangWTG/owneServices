using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateGlbSecurityForAllowFeatureEditForAnyReleaseGroupCheckPointTransform))]
	internal class UpdateGlbSecurityForAllowFeatureEditForAnyReleaseGroupCheckPointTransformTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlbSecurityForAllowFeatureEditForAnyReleaseGroupCheckPointTransform();
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GS, GU_SystemCreateTimeUtc, GU_SystemCreateUser, GU_SystemLastEditTimeUtc, GU_SystemLastEditUser)
	VALUES (NEWID(), 1, 'FeatureControl', '70EFA270-3F0F-479C-9AED-0009455622E2', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GG, GU_SystemCreateTimeUtc, GU_SystemCreateUser, GU_SystemLastEditTimeUtc, GU_SystemLastEditUser)
	VALUES (NEWID(), 1, 'FeatureControlEdit', '94755E71-A87A-4034-8DFA-785773A49607', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("A record with GU_SecurityItemIsAllowed equal to 0 should be added.", 1, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.GlbSecurity where GU_SecurityRight = 'AllowFeatureEditForAnyReleaseGroup' and GU_GS = '70EFA270-3F0F-479C-9AED-0009455622E2' and GU_SecurityItemIsAllowed = 0"));
			AssertEquals("A record with GU_SecurityItemIsAllowed equal to 0 should be added.", 1, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.GlbSecurity where GU_SecurityRight = 'AllowFeatureEditForAnyReleaseGroup' and GU_GG = '94755E71-A87A-4034-8DFA-785773A49607' and GU_SecurityItemIsAllowed = 0"));
		}
	}
}
