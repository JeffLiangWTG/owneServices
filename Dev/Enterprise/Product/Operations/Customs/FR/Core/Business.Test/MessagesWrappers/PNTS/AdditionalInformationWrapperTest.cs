using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class AdditionalInformationWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationWrapper>
	{
		public void TestCode()
		{
			AssertEquals("Wrapper Code should equal supportingInfo CSI_Code.", "Code", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("Wrapper Text should equal supportingInfo CSI_Description.", "Description", Provider.Text);
		}

		protected override AdditionalInformationWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Description = "Description";
			supportingInfo.CSI_Code = "Code";
			return AdditionalInformationWrapper.New(supportingInfo);
		}
	}
}
