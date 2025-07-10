using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineColsDirectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQCD_Direction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLRT", "COLS - Direction Request Type", "AU");
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLRT", "5", "ABC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var colsDirection = Factory.New<QuarantineColsDirection>();
			ValidationTestHelper.AssertInvalidCodeMessageError(colsDirection.QCD_DirectionInfo, "ZZ", "ABC");
		}
	}
}
