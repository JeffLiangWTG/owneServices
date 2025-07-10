using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Workflow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Workflow
{
	[TestedType(typeof(UpdateInvalidRQT_JobType))]
	internal class UpdateInvalidRQT_JobTypeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateInvalidRQT_JobType();

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			DBTransformationTestHelper.DropConstraintIfExists(ExternalRequestTypeSchema.Constants.TableName, "Constraint_RQT_JobType");

			var script = @"
DELETE FROM dbo.ExternalRequestType;
INSERT INTO dbo.ExternalRequestType
(RQT_PK, RQT_IsSystem, RQT_IsActive, RQT_Code, RQT_Description, RQT_JobType, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES(N'007E8627-31FD-470C-A604-8D440213031B', 1, 1, N'GEN', N'Generic', N'', '2024-12-12 12:19:00.000', N'~BP', '2024-12-12 12:19:00.000', N'~BP');
INSERT INTO dbo.ExternalRequestType
(RQT_PK, RQT_IsSystem, RQT_IsActive, RQT_Code, RQT_Description, RQT_JobType, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES(N'9524DA9E-F73D-4C1C-A242-B05447472622', 0, 0, N'XXX', N'XXX', N'X', '2024-12-13 04:11:00.000', N'E', '2024-12-17 06:43:00.000', N'Ea');
INSERT INTO dbo.ExternalRequestType
(RQT_PK, RQT_IsSystem, RQT_IsActive, RQT_Code, RQT_Description, RQT_JobType, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES(N'0F7EB31B-C7F6-4ED8-B877-0266BEC88C6F', 0, 0, N'ORD', N'Order', N'ORD', '2024-12-13 04:11:00.000', N'E', '2024-12-17 06:43:00.000', N'Ea');";

			TestConnection.ExecuteNonQuery(script);
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequestType WHERE RQT_JobType ='ALL' AND RQT_PK = '007E8627-31FD-470C-A604-8D440213031B'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequestType WHERE RQT_JobType ='ALL' AND RQT_PK = '9524DA9E-F73D-4C1C-A242-B05447472622'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequestType WHERE RQT_JobType ='ORD' AND RQT_PK = '0F7EB31B-C7F6-4ED8-B877-0266BEC88C6F'"));
		}
	}
}
