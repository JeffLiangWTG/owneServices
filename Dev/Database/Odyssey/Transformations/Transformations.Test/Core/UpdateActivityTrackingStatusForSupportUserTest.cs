using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateActivityTrackingStatusForSupportUser))]
	public class UpdateActivityTrackingStatusForSupportUserTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var status = TestConnection.ExecuteScalar<string>("SELECT GS_ActivityTrackingStatus FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1Support'");
			AssertEquals("YES", status);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateActivityTrackingStatusForSupportUser();
		}

		protected override void PrepareTestData()
		{
			// Note on: GS_LoginName = 'CW1Support'
			// We are renaming the user from CWSupport to CW1Support to support legacy test on UpdateActivityTrackingStatusForSupportUser
			var updateSql = @"
UPDATE
	dbo.GlbStaff
SET
	GS_ActivityTrackingStatus = 'CMP',
	GS_SystemLastEditTimeUtc = GetUtcDate(),
	GS_SystemLastEditUser = '~BP',
	GS_LoginName = 'CW1Support'
WHERE
	GS_LoginName = 'CWSupport'
 ";
			TestConnection.ExecuteNonQuery(updateSql);
		}
	}
}
