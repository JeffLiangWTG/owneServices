using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class AdditionalInformationProviderTest : DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestIAdditionalInformation()
		{
			Assert("Should implement IAdditionalInformation", Provider is IAdditionalInformation);
		}

		public void TestCode()
		{
			AssertEquals("20100", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("RN1", Provider.Text);
		}

		protected override AdditionalInformationProvider GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "20100";
			supportingInfo.CSI_Description = "RN1";
			return AdditionalInformationProvider.New(supportingInfo);
		}
	}
}
