using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(FixChargeCodeOverridesForJobTypeBRK))]
	internal class FixChargeCodeOverridesForJobTypeBRKTest : DataTransformationTestCase
	{
		public FixChargeCodeOverridesForJobTypeBRKTest()
		{
			TestDbHelper = new TestDbHelper(TestConnection);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new FixChargeCodeOverridesForJobTypeBRK();

		protected override void AssertTransformationResults()
		{
			AssertCreditorOverride();
			AssertBranchOverride();
		}

		void AssertCreditorOverride()
		{
			var sql = @"SELECT ACC_JobType
						FROM dbo.AccChargeCreditorOverride
						WHERE ACC_Direction = 'ALL' AND ACC_TransportMode = 'ALL'";
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, dataTable.Rows.Count);
			AssertEquals("BRK", dataTable.Rows[0][0].ToString());
		}

		void AssertBranchOverride()
		{
			var sql = @"SELECT YA_JobType
						FROM dbo.AccChargeBranchOverride
						WHERE YA_Direction = 'ALL' AND YA_TransportMode = 'ALL'";
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, dataTable.Rows.Count);
			AssertEquals("BRK", dataTable.Rows[0][0].ToString());
		}

		protected override void PrepareTestData()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var chargeCodePK = TestDbHelper.InsertChargeCode(companyPK, "CC1");

			TestDbHelper.InsertAccChargeCreditorOverride(chargeCodePK, "BRK", creditorRole: "OSA");
			TestDbHelper.InsertAccChargeCreditorOverride(chargeCodePK, "SHP", "EXP", "SEA", creditorRole: "OSA");

			TestDbHelper.InsertAccChargeBranchOverride(chargeCodePK, "BRK");
			TestDbHelper.InsertAccChargeBranchOverride(chargeCodePK, "SHP", "IMP", "AIR");
		}

		readonly TestDbHelper TestDbHelper;
	}
}
