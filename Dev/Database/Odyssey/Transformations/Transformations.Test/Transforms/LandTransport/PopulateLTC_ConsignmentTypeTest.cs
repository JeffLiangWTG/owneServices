using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.LandTransport;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.LandTransport
{
	[TestedType(typeof(PopulateLTC_ConsignmentType))]
	class PopulateLTC_ConsignmentTypeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new PopulateLTC_ConsignmentType();
			var manager = new DummyUpgradeManager();
			transformation.Initialise(manager: manager);

			return transformation;
		}

		#region Main case

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists("DtbConsignment", "Constraint_LTC_ConsignmentType", Db.Connection);

			var sql = $@"
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'fdd429d2-648c-4895-8f9f-06e90ded2be5'

DECLARE @consignment1PK UNIQUEIDENTIFIER = '6b581b3f-1b20-4f77-aa2f-8dae1b466614'
DECLARE @consignment2PK UNIQUEIDENTIFIER = 'c512c955-4f8b-419a-b57c-59b5e8e00195'

INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_JobID, LTC_ConnoteNumber, LTC_JobType, LTC_Status, LTC_Direction, LTC_GB_Branch, LTC_KM_Booking, LTC_IsRouteOverridden, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
	VALUES
	(@consignment1PK, 'CN0001', 'CNNote001', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-14 02:23:00', 'TST', GETUTCDATE(), 'TST'),
	(@consignment2PK, 'CN0002', 'CNNote002', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-17 02:23:00', 'TST', GETUTCDATE(), 'TST');

";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			var numberOfConsignmentRows = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM DtbConsignment where LTC_ConsignmentType='LTC'");
			AssertEquals("All Consignment Types that were previously empty should be set to LTC", 2, numberOfConsignmentRows);
		}

		#endregion

		#region Edge cases

		public void TestTransformationShouldRun_WhenColumnPresentButEmpty()
		{
			PrepareTestData();
			RunTransformation();
			AssertTransformationResults();
			RunTransformation();
			AssertTransformationResults();
		}

		public void TestTransformationWorks_WhenDtbConsignmentTableEmpty()
		{
			AssertTableIsEmpty();
			AssertNoExceptionThrown(RunTransformation);
			AssertNoExceptionThrown(RunTransformation);
			AssertTableIsEmpty();
		}

		#endregion

		#region Implementation

		void AssertTableIsEmpty()
		{
			var numberOfRows = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM DtbConsignment");
			AssertEquals("The consignment table should be empty.", 0, numberOfRows);
		}

		#endregion
	}
}
