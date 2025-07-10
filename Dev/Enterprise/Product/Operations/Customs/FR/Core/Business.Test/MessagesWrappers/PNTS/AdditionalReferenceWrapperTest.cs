using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class AdditionalReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalReferenceWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("Wrapper ReferenceNumber should equal supportingInfo CSI_ReferenceNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Wrapper Text should equal supportingInfo CSI_Code.", "Type", Provider.Type);
		}

		protected override AdditionalReferenceWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_Code = "Type";
			return AdditionalReferenceWrapper.New(supportingInfo);
		}
	}
}

