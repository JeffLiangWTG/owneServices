using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	class HouseWrapperTest : TestCaseWithFactory
	{
		public void TestHouseWrapper()
		{
			var manifestHeader = CreateAndPopulateManifestHeader();
			IManifest wrapper = new ManifestWrapper(manifestHeader);
			var bills = wrapper.Bills;

			AssertEquals(2, bills.Count);

			var house = bills.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(1, house.QuantityOfPackItems);
				AssertEquals("(H)QRHW20050055C", house.HouseNumber);
				AssertEquals(ZBool.True, house.BLToTheOrder);
				AssertEquals(ZBool.False, house.DeliveryPlaceAbroad);
				AssertEquals(ZString.Empty, house.DeliveryCountryAbroadCode);
				AssertEquals(ZBool.True, house.ForeignConsigneeIndicator);
				AssertEquals(ZString.Empty, house.ConsigneeData);
				AssertEquals("1996", house.ForeignConsigneePassportNumber);
				AssertEquals("CONSIGNEE", house.ForeignConsigneeName);
				AssertEquals("1996", house.ConsigneeRegNumber);
				AssertEquals("SHIPPER", house.ShipperID);
				AssertEquals("20231130", house.BillIsueDate);
				AssertEquals("GOODS", house.GoodsDescription);
				AssertEquals(1000m, house.Volume);
				AssertEquals("UYMVD", house.OriginPort);
				AssertEquals("BRSAO", house.DestinationPort);
				AssertEquals("1234", house.NotifyRegNumber);
				AssertEquals("NOTIFY", house.NotifyID);
				AssertEquals(500m, house.FreightCost);
				AssertEquals("220", house.CurrencyFreightCost);
				AssertEquals("P", house.PaymentCode);
				AssertEquals(FRTModeList.Codes.HP, house.DeliveryMode);
				AssertEquals("I", house.CargoClass);
				AssertEquals("ABC", house.DestinationCountryState);
				AssertEquals("BG", house.DischargePortTerminalOperator);
				AssertEquals("BR", house.OriginCountryCode);
				AssertEquals(ZBool.False, house.BLServiceIndicator);
				AssertEquals("210696", house.OriginalCEMercanteNumber);
				AssertEquals(ZBool.False, house.ContainerIndicator);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "OUT", RefCusMapTypeList.Codes.Currency, true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);

			Factory.Save();
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			header.MasterBill.ABL_BillIssueDate = new ZDate(2023, 11, 30);

			CreateAndPopulateHouseBill(header);
			_ = header.Bills.AddNew();

			return header;
		}

		void CreateAndPopulateHouseBill(AsycudaManifestHeader header)
		{
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "ABC";
			state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			var unloco = new RefUNLOCO.Loader(Factory).Load("BRSAO");
			unloco.RL_RW = state.PK;

			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_ManifestQty = 2;
			bill.ABL_RL_NKFinalDestination = unloco.RL_Code;
			bill.To_Order = ZBool.True;
			bill.ABL_ConsigneeName = "CONSIGNEE";
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Uruguay;
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_ConsigneeRegNo = "1996";
			bill.ABL_ShipperRegNo = "1891";
			bill.ABL_ShipperName = "SHIPPER";
			bill.ABL_GoodsDescription = "GOODS";
			bill.ABL_Volume = 1000;
			bill.BL_Service = ZBool.False;
			bill.FRTMode = FRTModeList.Codes.HP;
			bill.ABL_GoodsLocation = "BG";
			bill.ABL_RN_NKSellerCountry = "BR";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_NotifyPartyRegNo = "1234";
			bill.ABL_NotifyPartyName = "NOTIFY";
			bill.ABL_FreightValue = 500;
			bill.ABL_RX_NKFreightValueCurrency = "USD";
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.CustomsOwnNumber = "210696";

			bill.Packs.AddNew();
		}
	}
}
