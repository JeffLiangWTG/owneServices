using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	class HeaderWrapperTest : TestCaseWithFactory
	{
		public void TestHeaderWrapper()
		{
			var manifestHeader = CreateAndPopulateManifestHeader();
			IManifest wrapper = new ManifestWrapper(manifestHeader);
			var header = wrapper.Header;

			CombineAssertions(() =>
			{
				AssertEquals(1, header.QuantityOfHouseBills);
				AssertEquals("1891", header.DesconsolidatorAgent);
				AssertEquals("210696", header.MasterNumber);
				AssertEquals("1996", header.Company);
			});
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123457";
			orgHeader.OH_FullName = "Party1";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.NVOCCReference, "1996");

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1234";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			header.AMA_OA_ShippingAgent = orgAddress.PK;

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "2";
			orgHeader.OH_FullName = "Party2";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "1891");

			orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1345";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			header.CustomsOwnNumber = "210696";

			header.Bills.AddNew();

			return header;
		}
	}
}
