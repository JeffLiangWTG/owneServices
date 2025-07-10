using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalInformationWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationWrapper>
	{
		protected override AdditionalInformationWrapper GetProvider()
		{
			var cusSupportingInfo = Factory.New<CusSupportingInfo>();
			cusSupportingInfo.CSI_Code = "ZZZ";
			cusSupportingInfo.CSI_Description = "desc";
			return AdditionalInformationWrapper.New(cusSupportingInfo, string.Empty);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to Core.Constants.CountryCodes.France as customs office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var cusSupportingInfo = Factory.New<CusSupportingInfo>();
			cusSupportingInfo.CSI_Code = "ZZZ";
			cusSupportingInfo.CSI_Description = "desc";

			var wrapper = AdditionalInformationWrapper.New(cusSupportingInfo, Core.Constants.CountryCodes.France);
			AssertEquals("SupportingDocumentWrapper: CcQualifier should be empty as customsOffice starts with FR", string.Empty, wrapper.CcQualifier);

			wrapper = AdditionalInformationWrapper.New(cusSupportingInfo, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("SupportingDocumentWrapper: CcQualifier should be equal to FR as customsOffice doesn't starts with FR", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestText()
		{
			AssertEquals("Text should be equal to CSI_Description.", "desc", Provider.Text);
		}

		public void TestCode()
		{
			AssertEquals("Code should be equal to CSI_Code.", "ZZZ", Provider.Code);
		}
	}
}
