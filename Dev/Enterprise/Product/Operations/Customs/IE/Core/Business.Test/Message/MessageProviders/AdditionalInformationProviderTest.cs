using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AdditionalInformationProviderTest : DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestIAdditionalInformation()
		{
			Assert("Should implement IAdditionalInformation", Provider is IAdditionalInformation);
		}

		public void TestCode()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "A";
			var provider = AdditionalInformationProvider.New(additionalInfo);
			AssertEquals("A", provider.Code);

			additionalInfo.CSI_Code = "VFT";
			AssertEquals("VFT", provider.Code);
		}

		public void TestText()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Description = "Test222";
			var provider = AdditionalInformationProvider.New(additionalInfo);
			AssertEquals("Test222", provider.Text);

			additionalInfo.CSI_Description = "More testing";
			AssertEquals("More testing", provider.Text);
		}

		protected override AdditionalInformationProvider GetProvider() => AdditionalInformationProvider.New(Factory.New<AdditionalInfo>());
	}
}
