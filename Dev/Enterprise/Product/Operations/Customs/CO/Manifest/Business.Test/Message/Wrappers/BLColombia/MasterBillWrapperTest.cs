using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class MasterBillWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestMasterBillWrapper()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Colombia);
			helper.CreateCusMapType(COWrappersConstants.CityMapType, "OUT", COWrappersConstants.CityMapType, true);
			helper.CreateCusMapType(COWrappersConstants.StateMapType, "OUT", COWrappersConstants.StateMapType, true);
			helper.CreateCusMap(COWrappersConstants.CityMapType, "COBOG", "123", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Colombia);
			helper.CreateCusMap(COWrappersConstants.StateMapType, "CUN", "10", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Colombia);

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			PopulateManifestHeader();
			CreateAndPopulateMasterBill();
			CreateAndPopulateHouseBill();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var master = wrapper.Master;

			CombineAssertions(() =>
			{
				AssertEquals(11667803932049d, master.DocumentNumber);
				AssertEquals("10", master.VoyageDocumentType);
				AssertEquals("20", master.AdministrationCode);
				AssertEquals("30", master.LoadingArrangement);
				AssertEquals("10", master.StateCode);
				AssertEquals("123", master.CityCode);
				AssertEquals("MEDUQ2394375", master.TransportDocumentNumber);
				AssertEquals("21/06/2022", master.TransportDocumentDate.ToShortDateString());
				AssertEquals("DGContact - 092702171", master.DangerousGoodsContact);
			});
		}

		void PopulateManifestHeader()
		{
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "CUN";
			state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			var unloco = new RefUNLOCO.Loader(Factory).Load("COBOG");
			unloco.RL_RW = state.PK;

			header.AMA_CustomsOffice = "20";
			header.CargoDisposition = "30";
			header.TravelDocumentType = "10";
			header.AMA_MasterBillIssueDate = new ZDate(2022, 06, 21);
			header.AMA_RL_NKPortOfDischarge = unloco.RL_Code;
		}

		void CreateAndPopulateMasterBill()
		{
			var bill = header.MasterBill;

			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_BillIssueDate = new ZDate(2022, 06, 21);
		}

		void CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillIssueDate = new ZDate(2022, 06, 21);

			CreateAndPopulatePack(bill);
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			var pack = bill.Packs.AddNew();

			var undg = pack.UNDGs.AddNew();
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "DGContact";
			undg.DGContact.OC_Phone = "092702171";
			undg.DGContact.OC_OH = orgHeader.PK;
		}
	}
}
