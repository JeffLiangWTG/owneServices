using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_ReferenceIsNotEmpty()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(additionalFiscalReference.CFR_ReferenceInfo, additionalFiscalReference.CFR_CodeInfo, expectedMessage: "You have not entered an Identification Number.");
		}

		public void TestCheckCFR_Code()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();

			var targetInfo = additionalFiscalReference.CFR_CodeInfo;
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(targetInfo, additionalFiscalReference.CFR_ReferenceInfo, expectedMessage: "You have not entered a Type.");
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "???", EUICS2AdditionalFiscalReferenceTypes.Codes.FR5);
		}
	}
}
