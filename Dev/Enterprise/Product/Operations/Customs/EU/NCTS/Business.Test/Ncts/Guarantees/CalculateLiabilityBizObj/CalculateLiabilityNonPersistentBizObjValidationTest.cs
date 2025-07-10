using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CalculateLiabilityNonPersistentBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLiabilityPercentage()
		{
			const string error = "Percentage Rate (%) should be a value between 0 and 100.";

			calculateLiabilityBizObj.LiabilityPercentage = -1;
			AssertHasError(calculateLiabilityBizObj.LiabilityPercentageInfo, error);

			calculateLiabilityBizObj.LiabilityPercentage = 25;
			AssertNoError(calculateLiabilityBizObj.LiabilityPercentageInfo, error);

			calculateLiabilityBizObj.LiabilityPercentage = 101;
			AssertHasError(calculateLiabilityBizObj.LiabilityPercentageInfo, error);
		}

		protected override void SetUp()
		{
			calculateLiabilityBizObj = CalculateLiabilityBizObjTestHelper.CreateCalculateLiabilityBizObj(Factory).calculateLiabilityBizObj;
		}

		CalculateLiabilityBizObj calculateLiabilityBizObj;
	}
}
