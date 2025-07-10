using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalReferenceWrapper>
	{
		protected override AdditionalReferenceWrapper GetProvider()
		{
			var cusSupportingInfo = Factory.New<CusSupportingInfo>();
			cusSupportingInfo.CSI_Code = "ZZZ";
			cusSupportingInfo.CSI_ReferenceNumber = "desc";
			cusSupportingInfo.CSI_LineNo = 1;
			return AdditionalReferenceWrapper.New(cusSupportingInfo, string.Empty);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to Core.Constants.CountryCodes.France as customs office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var cusSupportingInfo = Factory.New<CusSupportingInfo>();
			cusSupportingInfo.CSI_Code = "ZZZ";
			cusSupportingInfo.CSI_ReferenceNumber = "desc";
			cusSupportingInfo.CSI_LineNo = 1;
			var wrapper = AdditionalReferenceWrapper.New(cusSupportingInfo, Core.Constants.CountryCodes.France);
			AssertEquals("CcQualifier should be empty as customs office is France.", string.Empty, wrapper.CcQualifier);

			wrapper = AdditionalReferenceWrapper.New(cusSupportingInfo, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("SupportingDocumentWrapper: CcQualifier should be equal to FR as customsOffice doesn't starts with FR", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestType()
		{
			AssertEquals("Text should be equal to CSI_Code.", "ZZZ", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Code should be equal to CSI_ReferenceNumber.", "desc", Provider.ReferenceNumber);
		}
	}
}
