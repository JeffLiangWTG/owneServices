using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	internal class UCC6GuaranteeReferenceWrapperTest : DataProviderTestCase<UCC6GuaranteeReferenceWrapper>
	{
		public void TestAccessCode()
		{
			AssertNull("AccessCode is null.", Provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			AssertEquals("AmountToBeCovered should equal 0 for DeferredPayment", 0.00D, Provider.AmountToBeCovered);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier should never be written for France.", Provider.CcQualifier);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("CurrencyCode should equal EUR", Core.Constants.CurrencyCodes.France, Provider.CurrencyCode);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			AssertEquals("CustomsOfficeOfGuarantee should be empty", string.Empty, Provider.CustomsOfficeOfGuarantee.ReferenceNumber);
		}

		public void TestGrn()
		{
			AssertEquals("Grn should equal DeferredPayment.", "11DACCCC123456789", Provider.Grn);
		}

		public void TestOtherGuaranteeReference()
		{
			AssertNull("OtherGuaranteeReference should equal null.", Provider.OtherGuaranteeReference);
		}

		protected override UCC6GuaranteeReferenceWrapper GetProvider()
		{
			var deferredPayment = "11DACCCC123456789";
			return UCC6GuaranteeReferenceWrapper.New(deferredPayment);
		}
	}
}
