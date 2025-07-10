using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC1_RateOverrideReasonCode()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_RateOverrideReasonCode = "AAA";
			AssertHasMessageError("When code is Invalid", charge.C1_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			charge.C1_RateOverrideReasonCode = ILRateOverrideReasonList.Codes.Override;
			AssertNoMessageError("When code is valid", charge.C1_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
