using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class AdditionalInformationWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationWrapper>
	{
		public void TestCode()
		{
			AssertEquals("Code should equal CSI_Code.", "Code", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("Text should equal CSI_ReferenceNumber.", "ReferenceNumber", Provider.Text);
		}

		protected override AdditionalInformationWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_Code = "Code";
			return AdditionalInformationWrapper.New(supportingInfo);
		}
	}
}
