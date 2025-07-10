using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS
{
	[TestedType(typeof(RemoveReviewProcessWithInvalidFilterType))]
	public class RemoveReviewProcessWithInvalidFilterTypeTest : DataTransformationTestCase
	{
		readonly Guid reviewProcessId = Guid.Parse("7D90CD77-4183-4FFE-9AEC-576385E8ACE0");

		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveReviewProcessWithInvalidFilterType();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery($@"
DELETE FROM dbo.ReviewProcess WHERE RPR_PK = '{reviewProcessId}';
INSERT INTO dbo.ReviewProcess (
	[RPR_PK],
	[RPR_Name],
	[RPR_Type],
	[RPR_Status],
	[RPR_EffectiveDate],
	[RPR_SubmissionDate],
	[RPR_RX_NKCurrency],
	[RPR_S9_EmployeesInReview],
	[RPR_PrimaryHierarchy],
	[RPR_OverrideHierarchy],
	[RPR_SystemCreateTimeUtc],
	[RPR_SystemCreateUser],
	[RPR_SystemLastEditTimeUtc],
	[RPR_SystemLastEditUser],
	[RPR_ConfigType],
	[RPR_ExchangeRateEffectiveDate],
	[RPR_GC_Company]
) VALUES
(
	'{reviewProcessId}',
	'Test',
	'C&P',
	'UNS',
	'2024-05-02',
	'2024-05-02 00:00:00 +10:00',
	'',
	null,
	'DRM',
	'',
	'2024-03-13 03:48:00',
	'ZZN',
	'2024-04-08 03:07:00',
	'TNG',
	'',
	null,
	null
);");
			Assert(Db.Connection.Exists($"FROM dbo.ReviewProcess WHERE RPR_PK = '{reviewProcessId}'"));
		}

		protected override void AssertTransformationResults()
		{
			Assert(!Db.Connection.Exists($"FROM dbo.ReviewProcess WHERE RPR_PK = '{reviewProcessId}'"));
		}
	}
}
