using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class HouseWrapperTest : TestCaseWithFactory
	{
		public void TestHouseWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill(header);

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var house = wrapper.Master.Houses.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals(11667803932049d, house.DocumentID);
				AssertEquals("123", house.LoadDisposition);
				AssertEquals("10", house.StateCode);
				AssertEquals("123", house.CityCode);
				AssertEquals("211110890018", house.Deposit);
				AssertEquals("12", house.VoyageDocumentType);
				AssertEquals("BILL1", house.TransportDocumentNumber);
				AssertEquals("31/01/2023", house.TransportDocumentDate.ToShortDateString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

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
		}

		void CreateAndPopulateHouseBill(AsycudaManifestHeader header)
		{
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "CUN";
			state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			var unloco = new RefUNLOCO.Loader(Factory).Load("COBOG");
			unloco.RL_RW = state.PK;

			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "BILL1";
			bill.ABL_BillIssueDate = new ZDate(2023, 01, 31);
			bill.ABL_RL_NKFinalDestination = unloco.RL_Code;
			bill.CargoDisposition = "123";
			bill.TravelDocumentType = "12";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "96";
			orgHeader.OH_FullName = "GLName";
			orgHeader.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.CID, "211110890017", Core.Constants.CountryCodes.Colombia);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "211110890018", Core.Constants.CountryCodes.Colombia);

			bill.ABL_OA_GoodsLocation = orgHeader.MainAddress.PK;
		}
	}
}
