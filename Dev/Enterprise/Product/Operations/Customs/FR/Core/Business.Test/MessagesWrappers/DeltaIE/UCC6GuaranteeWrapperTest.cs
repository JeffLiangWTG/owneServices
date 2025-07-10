using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	public class UCC6GuaranteeWrapperTest : DataProviderTestCase<UCC6GuaranteeWrapper>
	{
		public void TestGuaranteeReference()
		{
			var guaranteeReference = Provider.GuaranteeReference.First();
			CombineAssertions("GuaranteeReference should use GuaranteeReferenceWrapper.", () =>
			{
				AssertNull("AccessCode should equal null", guaranteeReference.AccessCode);
				AssertEquals("AmountToBeCovered should equal 0.", 0d, guaranteeReference.AmountToBeCovered);
				AssertNull("CcQualifier should never be written for France.", guaranteeReference.CcQualifier);
				AssertEquals("CurrencyCode should equal EUR.", Core.Constants.CurrencyCodes.France, guaranteeReference.CurrencyCode);
				AssertEquals("CustomsOfficeOfGuarantee should be empty.", string.Empty, guaranteeReference.CustomsOfficeOfGuarantee.ReferenceNumber);
				AssertEquals("Grn should equal guarantee CPH_Number.", "11DACCCC123456789", guaranteeReference.Grn);
				AssertNull("OtherGuaranteeReference should equal null", guaranteeReference.OtherGuaranteeReference);
			});
		}

		public void TestGuaranteeType()
		{
			AssertEquals("GuaranteeType should equal 1.", "1", Provider.GuaranteeType);
		}

		protected override UCC6GuaranteeWrapper GetProvider()
		{
			var deferredPayment = "11DACCCC123456789";
			return UCC6GuaranteeWrapper.New(deferredPayment);
		}
	}
}
