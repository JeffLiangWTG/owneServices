using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryHeaderChargesValidationTests : BusinessObjectValidationTestCase
	{
		public void TestCheckC1_RateOverrideReasonCode()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_RateOverrideReasonCode = "AAA";
			AssertHasMessageError(charge.C1_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			charge.C1_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			AssertNoMessageError(charge.C1_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
