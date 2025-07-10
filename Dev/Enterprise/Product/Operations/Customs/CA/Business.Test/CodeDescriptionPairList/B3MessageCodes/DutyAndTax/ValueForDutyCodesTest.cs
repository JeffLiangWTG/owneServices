using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ValueForDutyCodesTest : TestCaseWithFactory
	{
		public void TestIsRelatedFirms()
		{
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsComputedValue);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsDeductiveValue);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithAdjustments);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue);
			AssertRelatedResult(true, ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods);

			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsDeductiveValue);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsIdenticalGoods);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithAdjustments);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue);
			AssertRelatedResult(false, ValueForDutyCodes.Codes.UnrelatedFirmsSimilarGoods);
		}

		public void TestIsUnrelatedFirms()
		{
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsComputedValue);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsDeductiveValue);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithAdjustments);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue);
			AssertUnrelatedResult(false, ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods);

			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsDeductiveValue);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsIdenticalGoods);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithAdjustments);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue);
			AssertUnrelatedResult(true, ValueForDutyCodes.Codes.UnrelatedFirmsSimilarGoods);
		}

		void AssertRelatedResult(bool result, string code)
		{
			AssertEquals(result, ValueForDutyCodes.IsRelatedFirms(code));
		}

		void AssertUnrelatedResult(bool result, string code)
		{
			AssertEquals(result, ValueForDutyCodes.IsUnrelatedFirms(code));
		}
	}
}
