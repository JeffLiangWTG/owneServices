using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using TemporaryStorageHeader = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class ConsignmentHeaderMasterLevelWrapperTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentHeaderMasterLevelWrapper>
	{
		public void TestArrivalTransportMeans()
		{
			CombineAssertions("ArrivalTransportMeans should use ArrivalTransportMeansWrapper", () =>
			{
				AssertType<ArrivalTransportMeansWrapper>("ArrivalTransportMeans should be of type ArrivalTransportMeansWrapper", Provider.ArrivalTransportMeans);
				AssertEquals("IdentificationNumber should match arrivalTransportMeans IdentificationNumber.", "IMO001122", Provider.ArrivalTransportMeans.IdentificationNumber);
			});
		}

		public void TestCarrier()
		{
			CombineAssertions("Carrier should use CarrierWrapper", () =>
			{
				AssertEquals("IdentificationNumber should equal carrier EORI.", "FR12345678", Provider.Carrier.IdentificationNumber);
				AssertEquals("Name should equal carrier OH_FullName.", "CarrierName", Provider.Carrier.Name);
			});
		}

		public void TestConsignmentHouseLevel()
		{
			CombineAssertions("ConsignmentHouseLevel should use ConsignmentHouseLevelWrapper", () =>
			{
				AssertType<Collection<IConsignmentHouseLevel>>("ConsignmentHouseLevel should be of type ConsignmentHouseLevelWrapper", Provider.ConsignmentHouseLevel);
				AssertEquals("ConsignmentHouseLevel should gather all temporayStorageHeader bills where ABL_BolType ==  HWB.", 2, Provider.ConsignmentHouseLevel.Count);
				AssertEquals("TotalGrossMass should match Housebill ABL_GrossWeight.", 32.47m, Provider.ConsignmentHouseLevel.ElementAt(0).TotalGrossMass);
			});
		}

		public void TestConsignmentMasterLevel()
		{
			CombineAssertions("ConsignmentMasterLevel should use ConsignmentMasterLevelWrapper", () =>
			{
				AssertType<ConsignmentMasterLevelWrapper>("ConsignmentMasterLevel should be of type ConsignmentMasterLevelWrapper", Provider.ConsignmentMasterLevel);
				AssertEquals("ConsignmentHouseLevel should be based on temporayStorageHeader bill where ABL_BolType == BOL data.", "UCRNumber1", Provider.ConsignmentMasterLevel.ReferenceNumberUCR.ReferenceNumberUCRProperty);
				AssertEquals("TotalGrossMass should match fist bill ABL_GrossWeight.", 77.65m, Provider.ConsignmentMasterLevel.TotalGrossMass);
			});
		}

		public void TestConsignmentMasterLevel_IsNull()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.HasNoMasterBill = true;
			var provider = ConsignmentHeaderMasterLevelWrapper.New(header);
			AssertNull("ConsignmentMasterLevel should be null in case HasNoMasterBill is true.", provider.ConsignmentMasterLevel);

			header.HasNoMasterBill = false;
			AssertNotNull("ConsignmentMasterLevel should not be null in case HasNoMasterBill is false.", provider.ConsignmentMasterLevel);
		}

		public void TestLocationOfGoods()
		{
			CombineAssertions("LocationOfGoods should use LocationOfGoodsWrapper", () =>
			{
				AssertType<LocationOfGoodsWrapper>("LocationOfGoods should be of type LocationOfGoodsWrapper", Provider.LocationOfGoods);
				AssertEquals("AdditionalIdentifier should be empty", "ident", Provider.LocationOfGoods.AdditionalIdentifier);
			});
		}

		public void TestPlaceOfUnloading()
		{
			AssertEquals("PlaceOfUnloading should use PlaceOfUnloadingWrapper.", "FRBER", Provider.PlaceOfUnloading.UnLoCode);
		}

		public void TestWarehouse()
		{
			CombineAssertions("Warehouse should use WarehouseWrapper", () =>
			{
				AssertType<WarehouseWrapper>("Warehouse should be of type WarehouseWrapper", Provider.Warehouse);
				AssertEquals("Identifier should be based on temporayStorageHeader cusAuthorisationUsage AGC_Number.", "cod", Provider.Warehouse.Identifier);
			});
		}

		protected override ConsignmentHeaderMasterLevelWrapper GetProvider()
		{
			var temporayStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			Factory.Save();

			var arrivalTransportMeans = Factory.New<Customs.Business.CusTransportMeans>();
			arrivalTransportMeans.TPM_ParentID = temporayStorageHeader.PK;
			arrivalTransportMeans.TPM_ParentTableCode = "AMA";
			arrivalTransportMeans.TPM_IdentificationNumber = "IMO001122";

			Factory.Save();

			var authorizationUsage = temporayStorageHeader.AuthorizationUsageOrNew;
			authorizationUsage.FillWithValidTestData();
			authorizationUsage.AGC_Number = "cod";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.FillWithValidTestData();
			carrier.OH_Code = "CARRIER";
			carrier.OH_FullName = "CarrierName";
			carrier.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);
			temporayStorageHeader.AMA_OA_Carrier = carrier.MainAddress.PK;

			var bolBill = temporayStorageHeader.Bills[0];
			bolBill.ABL_UCRNumber = "UCRNumber1";
			bolBill.ABL_GrossWeight = 77.65m;
			bolBill.ABL_RL_NKPortOfDischarge = "FRBER";

			var bill2 = temporayStorageHeader.Bills.AddNew();
			CreateBill(bill2);
			bill2.ABL_AMA = temporayStorageHeader.PK;
			bill2.ABL_UCRNumber = "UCRNumber2";
			bill2.ABL_GrossWeight = 32.47m;
			bill2.ABL_BolType = "HWB";

			var bill3 = temporayStorageHeader.Bills.AddNew();
			CreateBill(bill3);
			bill3.ABL_AMA = temporayStorageHeader.PK;
			bill3.ABL_UCRNumber = "UCRNumber3";
			bill3.ABL_GrossWeight = 40.21m;
			bill3.ABL_BolType = "HWB";

			temporayStorageHeader.GoodsLocation.CGL_AdditionalIdentifier = "ident";

			return ConsignmentHeaderMasterLevelWrapper.New(temporayStorageHeader);
		}

		void CreateBill(TemporaryStorageBill bill)
		{
			bill.ABL_GrossWeight = 1.00;
			bill.ABL_BillNumber = "1X";

			var addressConsignee = Factory.New<OrgAddress>();
			addressConsignee.OA_City = "Insomnia";
			addressConsignee.OA_RN_NKCountryCode = "LS";
			addressConsignee.OA_PostCode = "000000";
			addressConsignee.OA_Address1 = "Kings Street";
			addressConsignee.OA_Address2 = "No.001";

			var addressConsignor = Factory.New<OrgAddress>();
			addressConsignor.OA_City = "Sleep";
			addressConsignor.OA_RN_NKCountryCode = "FR";
			addressConsignor.OA_PostCode = "000001";
			addressConsignor.OA_Address1 = "Queens Street";
			addressConsignor.OA_Address2 = "No.002";

			var addressNotify = Factory.New<OrgAddress>();
			addressNotify.OA_City = "BaldursGate";
			addressNotify.OA_RN_NKCountryCode = "SP";
			addressNotify.OA_PostCode = "000002";
			addressNotify.OA_Address1 = "Blushing Mermaid";
			addressNotify.OA_Address2 = "No.003";

			var header = Factory.New<OrgHeader>();
			var contact = header.Contacts.AddNew();
			contact.OC_Email = "a@a.com";
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			header.LocalBusinessRegNoObject.OK_CustomsRegNo = "1";
			header.OH_FullName = "Name";
			addressConsignee.OA_OH = header.PK;
			addressConsignor.OA_OH = header.PK;
			addressNotify.OA_OH = header.PK;

			bill.ABL_OA_Consignee = addressConsignee.PK;
			bill.ABL_ConsigneeRegNoType = "2";
			bill.ABL_OA_Shipper = addressConsignor.PK;
			bill.ABL_ShipperRegNoType = "3";
			bill.ABL_OA_NotifyParty = addressNotify.PK;
			bill.ABL_NotifyPartyRegNoType = "4";

			var container = Factory.NewWithValidTestData<AsycudaContainer>();
			container.ACN_ContainerNumber = "ContainerNumber";
			container.ACN_EmptyFullIndicator = "FUL";
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 3;
			pack.APA_MarksAndNumbers = "Marks & numbers";
			pack.APA_PackUQ = "CTN";
			pack.ContainerPK = container.PK;
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			var item = pack.PackedItem;
			item.API_GoodsDescription = "GoodsDescription";
			item.API_Tariff = "2345167890";
			item.API_GrossWeight = 55.33m;
			item.API_GrossWeightUQ = "KG";

			bill.ContainerPK = container.PK;
		}
	}
}
