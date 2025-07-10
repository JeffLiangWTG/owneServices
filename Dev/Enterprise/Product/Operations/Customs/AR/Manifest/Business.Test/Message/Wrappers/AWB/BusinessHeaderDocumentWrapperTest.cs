using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BusinessHeaderDocumentWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBusinessHeaderDocumentWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "23456ABCD";

			IHouseWaybill wrapper = new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Original);
			IBusinessHeaderDocument businessHeader = wrapper.BusinessHeader;
			ICarrierAuthentication carrierAuthentication = businessHeader.CarrierAuthentication;

			CombineAssertions(() =>
			{
				AssertEquals("23456ABCD", businessHeader.ID);
				AssertEquals("K. WILSON", businessHeader.ConsignorAuthenticationSignatory);

				AssertEquals("2022-09-09T00:00:00", carrierAuthentication.ActualDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("J. LIM", carrierAuthentication.Signatory);
				AssertEquals("City", carrierAuthentication.AuthenticationLocationName);
			});
		}

		void PopulateManifestHeader()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "123456";
			orgHeader1.OH_FullName = "K. WILSON";
			var orgAddress1 = orgHeader1.MainAddress;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "210696";
			orgHeader2.OH_FullName = "J. LIM";
			var orgAddress2 = orgHeader2.MainAddress;
			orgAddress2.City = "City";

			header.ShippingAgentOrgPK = orgHeader1.PK;
			header.ShippingAgentOrg.Addresses.Add(orgAddress1);
			header.AMA_OA_Carrier = orgAddress2.PK;
			header.AMA_MasterBillIssueDate = new ZDate(2022, 09, 09);

			Factory.Save();
		}
	}
}
