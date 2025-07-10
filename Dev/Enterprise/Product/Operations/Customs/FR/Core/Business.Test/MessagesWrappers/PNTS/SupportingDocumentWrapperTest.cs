using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class SupportingDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentWrapper>
	{
		public void TestType()
		{
			AssertEquals("Wrapper Type should equal supportingInfo CSI_Code.", "Code", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Wrapper ReferenceNumber should equal supportingInfo CSI_ReferenceNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		protected override SupportingDocumentWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_Code = "Code";
			return SupportingDocumentWrapper.New(supportingInfo);
		}
	}
}
