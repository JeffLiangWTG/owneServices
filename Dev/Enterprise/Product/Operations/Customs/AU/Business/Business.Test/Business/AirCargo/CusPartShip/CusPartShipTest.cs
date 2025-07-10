using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusPartShip))]
	sealed class CusPartShipTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUsesTranshipmentPortOnUnderbond()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)partShip).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)partShip).DefaultTranshipmentPort);
		}

		public void TestWarehouseLocationCaption()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertEquals("Warehouse Location:", partShip.WarehouseLocationCaption);
		}

		public void TestCanSendWithoutDelay()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			Assert(((ICusUnderbondDependentCollectionParent)partShip).CanSendWithoutDelay);
		}

		public void TestChargableWeightCaption()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertEquals("Chargeable Weight:", partShip.ChargableWeightCaption);
		}

		public void TestStatusNeedsRecalculation()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertEquals(false, partShip.StatusNeedsRecalculation);
			partShip.Messages.AddNew().HasChanges = true;
			AssertEquals(true, partShip.StatusNeedsRecalculation);
		}

		public void TestCalculator()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertNotNull("Calculator", partShip.Calculator);
		}

		public void TestDetails()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			partShip.CG_FlightNo = "QF123";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
			AssertEquals("Details", "QF123 : 04-Jul-05", partShip.Details);
		}

		public void TestShortDescription()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			partShip.CG_FlightNo = "QF123";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
			AssertEquals("ShortDescription", "QF123 : 04-Jul-05", partShip.ShortDescription);
		}

		public void TestUnderbondHumanReadableName()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			hAWB.CS_HAWB = "12345";
			partShip.CG_FlightNo = "QF123";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
			AssertEquals("UnderbondHumanReadableName", "MasterBill 12345 - Part Shipment: QF123, 04-Jul-05", partShip.UnderbondHumanReadableName);
		}

		public void TestUnderbonds()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertNotNull(partShip.Underbonds);
		}

		public void TestOutturnableLines()
		{
			CusPartShip partShip = (CusPartShip)GetNewBusinessObject();
			AssertEquals("OutturnableLines.Length", 0, partShip.OutturnableLines.Length);
		}

		public void TestLoadFromSendersReference()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "M00001000";
			CusPartShip partShip = hAWB.PartShips.AddNew();
			partShip.CG_MessageReference = "000001";
			AssertEquals("LoadFromSendersReference", partShip, CusPartShip.LoadFromSendersReference(Factory, "PM00001000/000001"));
			AssertEquals("NoException", null, CusPartShip.LoadFromSendersReference(Factory, "PM00001000"));
		}

		public void TestIUnderbondMovementRequestHeaderProvider_GetHeader()
		{
			var underbond = Factory.New<CusUnderbond>();
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			AssertEquals("HeaderType", typeof(CTOCusPartShipUnderbondMovementRequestHeader), ((IUnderbondMovementRequestHeaderProvider)partShip).GetHeader(underbond).GetType());
		}

		public void TestIUnderbondMovementRequestHeaderProvider_GetHeaderForNonCTOPartShip()
		{
			var underbond = Factory.New<CusUnderbond>();
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			AssertEquals("HeaderType", typeof(HouseCusPartShipUnderbondMovementRequestHeader), ((IUnderbondMovementRequestHeaderProvider)partShip).GetHeader(underbond).GetType());
		}

		public void TestIUnderbondMovementRequestHeaderProvider_GetHeaderForMAWBPartShip()
		{
			var underbond = Factory.New<CusUnderbond>();
			var mAWB = Factory.New<CusMAWB>();
			CusPartShip partShip = mAWB.PartShips.AddNew();
			AssertEquals("HeaderType", typeof(MasterCusPartShipUnderbondMovementRequestHeader), ((IUnderbondMovementRequestHeaderProvider)partShip).GetHeader(underbond).GetType());
		}

		public void TestIsBureau()
		{
			var underbond = Factory.New<CusUnderbond>();
			var masterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = masterBill.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			IUnderbondMovementRequestHeader header = ((IUnderbondMovementRequestHeaderProvider)partShip).GetHeader(underbond);
			masterBill.CM_IsBureau = false;
			AssertEquals("IsBureau", false, header.IsBureau);
			masterBill.CM_IsBureau = true;
			AssertEquals("IsBureau", true, header.IsBureau);
		}

		public void TestStatusIsReadonly()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			AssertEquals("ReadOnly", true, partShip.CG_CustomsStatusInfo.ReadOnly);
		}

		public void TestCanDeleteFromICusHAWBCollection()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			AssertEquals(false, partShip.CanDeleteFromICusHAWBCollection);
		}

		public void TestLoadFromICTOCusHAWBInformationProvider()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			hAWB.CS_HAWB = "123";
			partShip.CG_FlightNo = "QF111";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 30);

			CTOCusHAWBTest.TestHelperCTOHAWBInformation info = new CTOCusHAWBTest.TestHelperCTOHAWBInformation();
			info.ArrivalDate = new ZDateTime(2005, 7, 30);
			info.FlightNumber = "QF111";
			info.MAWB = "123";
			AssertEquals("Result", partShip, CusPartShip.Load(Factory, info));
		}

		public void TestLoadFromICTOCusHAWBInformationProviderDoesLoadNonCTOHouseBills()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusPartShip partShip = hAWB.PartShips.AddNew();
			hAWB.CS_HAWB = "123";
			partShip.CG_FlightNo = "QF111";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 30);

			CTOCusHAWBTest.TestHelperCTOHAWBInformation info = new CTOCusHAWBTest.TestHelperCTOHAWBInformation();
			info.ArrivalDate = new ZDateTime(2005, 7, 30);
			info.FlightNumber = "QF111";
			info.MAWB = "123";
			AssertEquals("Result", null, CusPartShip.Load(Factory, info));
		}

		public void TestMAWB()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusPartShip partShip = mAWB.PartShips.AddNew();
			AssertEquals("MAWB", mAWB, partShip.MAWB);
		}

		public void TestUnderbondHumanReadableNameOnMAWB()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "1";
			CusPartShip partShip = mAWB.PartShips.AddNew();
			partShip.CG_FlightNo = "QF123";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
			AssertEquals("UnderbondHumanReadableName", "MasterBill 1 - Part Shipment: QF123, 04-Jul-05", partShip.UnderbondHumanReadableName);
		}

		public void TestCS_MessageReference()
		{
			PartShip.CG_MessageReference = "123";
			AssertEquals("CS_MessageReference", "123", PartShip.CS_MessageReference);
		}

		public void TestCS_MessageReferenceInfo()
		{
			AssertEquals("CS_MessageReferenceInfo.Readonly", true, PartShip.CS_MessageReferenceInfo.ReadOnly);
		}

		public void TestCS_HAWB()
		{
			PartShip.HouseBill.CS_HAWB = "123";
			AssertEquals("CS_HAWB", "123", PartShip.CS_HAWB);
		}

		public void TestCS_HAWBInfo()
		{
			AssertEquals("CS_HAWBInfo.Readonly", true, PartShip.CS_HAWBInfo.ReadOnly);
		}

		public void TestCS_RL_NKLoadPort()
		{
			PartShip.CG_RL_NKLoadPort = "123";
			AssertEquals("CS_RL_NKLoadPort", "123", PartShip.CS_RL_NKLoadPort);
		}

		public void TestCS_RL_NKLoadPortInfo()
		{
			AssertEquals("CS_RL_NKLoadPortInfo.Readonly", true, PartShip.CS_RL_NKLoadPortInfo.ReadOnly);
		}

		public void TestCS_RL_NKOrigin()
		{
			PartShip.HouseBill.CS_RL_NKOrigin = "123";
			AssertEquals("CS_RL_NKOrigin", "123", PartShip.CS_RL_NKOrigin);
		}

		public void TestCS_RL_NKOriginInfo()
		{
			AssertEquals("CS_RL_NKOriginInfo.Readonly", true, PartShip.CS_RL_NKOriginInfo.ReadOnly);
		}

		public void TestCS_RL_NKDestination()
		{
			PartShip.HouseBill.CS_RL_NKDestination = "123";
			AssertEquals("CS_RL_NKDestination", "123", PartShip.CS_RL_NKDestination);
		}

		public void TestCS_RL_NKDestinationInfo()
		{
			AssertEquals("CS_RL_NKDestinationInfo.Readonly", true, PartShip.CS_RL_NKDestinationInfo.ReadOnly);
		}

		public void TestCS_Weight()
		{
			PartShip.HouseBill.CS_Weight = 123m;
			AssertEquals("CS_Weight", 123m, PartShip.CS_Weight);
		}

		public void TestCS_WeightInfo()
		{
			AssertEquals("CS_WeightInfo.Readonly", true, PartShip.CS_WeightInfo.ReadOnly);
		}

		public void TestCS_WeightUQ()
		{
			PartShip.HouseBill.CS_WeightUQ = "KG";
			AssertEquals("CS_WeightUQ", "KG", PartShip.CS_WeightUQ);
		}

		public void TestCS_WeightUQInfo()
		{
			AssertEquals("CS_WeightUQInfo.Readonly", true, PartShip.CS_WeightUQInfo.ReadOnly);
		}

		public void TestCS_GoodsValue()
		{
			PartShip.HouseBill.CS_GoodsValue = 123m;
			AssertEquals("CS_GoodsValue", 123m, PartShip.CS_GoodsValue);
		}

		public void TestCS_GoodsValueInfo()
		{
			AssertEquals("CS_GoodsValueInfo.Readonly", true, PartShip.CS_GoodsValueInfo.ReadOnly);
		}

		public void TestCS_RX_NKGoodsCurrency()
		{
			PartShip.HouseBill.CS_RX_NKGoodsCurrency = "123";
			AssertEquals("CS_RX_NKGoodsCurrency", "123", PartShip.CS_RX_NKGoodsCurrency);
		}

		public void TestCS_RX_NKGoodsCurrencyInfo()
		{
			AssertEquals("CS_RX_NKGoodsCurrencyInfo.Readonly", true, PartShip.CS_RX_NKGoodsCurrencyInfo.ReadOnly);
		}

		public void TestCS_GoodsDescription()
		{
			PartShip.HouseBill.CS_GoodsDescription = "123";
			AssertEquals("CS_GoodsDescription", "123", PartShip.CS_GoodsDescription);
		}

		public void TestCS_GoodsDescriptionInfo()
		{
			AssertEquals("CS_GoodsDescriptionInfo.Readonly", true, PartShip.CS_GoodsDescriptionInfo.ReadOnly);
		}

		public void TestCS_CustomsStatus()
		{
			PartShip.CG_CustomsStatus = "123";
			AssertEquals("CS_CustomsStatus", "123", PartShip.CS_CustomsStatus);
		}

		public void TestCS_CustomsStatusInfo()
		{
			AssertEquals("CS_CustomsStatusInfo.Readonly", true, PartShip.CS_CustomsStatusInfo.ReadOnly);
		}

		public void TestCS_IsMasterHouse()
		{
			PartShip.HouseBill.CS_IsMasterHouse = false;
			AssertEquals("CS_IsMasterHouse", false, PartShip.CS_IsMasterHouse);
		}

		public void TestCS_IsMasterHouseInfo()
		{
			AssertEquals("CS_IsMasterHouseInfo.Readonly", true, PartShip.CS_IsMasterHouseInfo.ReadOnly);
		}

		public void TestCS_PiecesManifested()
		{
			PartShip.CG_PiecesManifested = (short)123;
			AssertEquals("CS_PiecesManifested", (short)123, PartShip.CS_PiecesManifested);
		}

		public void TestCS_PiecesManifestedInfo()
		{
			AssertEquals("CS_PiecesManifestedInfo.Readonly", true, PartShip.CS_PiecesManifestedInfo.ReadOnly);
		}

		public void TestCS_FreightPrepaidCollect()
		{
			PartShip.HouseBill.CS_FreightPrepaidCollect = "123";
			AssertEquals("CS_FreightPrepaidCollect", "123", PartShip.CS_FreightPrepaidCollect);
		}

		public void TestCS_FreightPrepaidCollectInfo()
		{
			AssertEquals("CS_FreightPrepaidCollectInfo.Readonly", true, PartShip.CS_FreightPrepaidCollectInfo.ReadOnly);
		}

		public void TestCS_WarehouseLocation()
		{
			PartShip.HouseBill.CS_WarehouseLocation = "123";
			AssertEquals("CS_WarehouseLocation", "123", PartShip.CS_WarehouseLocation);
		}

		public void TestCS_WarehouseLocationInfo()
		{
			AssertEquals("CS_WarehouseLocationInfo.Readonly", true, PartShip.CS_WarehouseLocationInfo.ReadOnly);
		}

		public void TestCS_FolioReference()
		{
			PartShip.HouseBill.CS_FolioReference = "123";
			AssertEquals("CS_FolioReference", "123", PartShip.CS_FolioReference);
		}

		public void TestCS_FolioReferenceInfo()
		{
			AssertEquals("CS_FolioReferenceInfo.Readonly", true, PartShip.CS_FolioReferenceInfo.ReadOnly);
		}

		public void TestCS_ShipmentType()
		{
			PartShip.HouseBill.CS_ShipmentType = "123";
			AssertEquals("CS_ShipmentType", "123", PartShip.CS_ShipmentType);
		}

		public void TestCS_ShipmentTypeInfo()
		{
			AssertEquals("CS_ShipmentTypeInfo.Readonly", true, PartShip.CS_ShipmentTypeInfo.ReadOnly);
		}

		public void TestCusHAWBLookups_Lookups()
		{
			AssertEquals("Lookups", PartShip.HouseBill.Lookups, ((ICusHAWBBase)PartShip).Lookups);
		}

		public void TestCS_ConsigneeCity()
		{
			PartShip.HouseBill.CS_ConsigneeCity = "123";
			AssertEquals("CS_ConsigneeCity", "123", PartShip.CS_ConsigneeCity);
		}

		public void TestCS_ConsigneeCityInfo()
		{
			AssertEquals("CS_ConsigneeCityInfo.Readonly", true, PartShip.CS_ConsigneeCityInfo.ReadOnly);
		}

		public void TestCS_ConsigneeName()
		{
			PartShip.HouseBill.CS_ConsigneeName = "123";
			AssertEquals("CS_ConsigneeName", "123", PartShip.CS_ConsigneeName);
		}

		public void TestCS_ConsigneeNameInfo()
		{
			AssertEquals("CS_ConsigneeNameInfo.Readonly", true, PartShip.CS_ConsigneeNameInfo.ReadOnly);
		}

		public void TestCS_ConsigneePhone()
		{
			PartShip.HouseBill.CS_ConsigneePhone = "123";
			AssertEquals("CS_ConsigneePhone", "123", PartShip.CS_ConsigneePhone);
		}

		public void TestCS_ConsigneePhoneInfo()
		{
			AssertEquals("CS_ConsigneePhoneInfo.Readonly", true, PartShip.CS_ConsigneePhoneInfo.ReadOnly);
		}

		public void TestCS_ConsigneePostcode()
		{
			PartShip.HouseBill.CS_ConsigneePostcode = "123";
			AssertEquals("CS_ConsigneePostcode", "123", PartShip.CS_ConsigneePostcode);
		}

		public void TestCS_ConsigneePostcodeInfo()
		{
			AssertEquals("CS_ConsigneePostcodeInfo.Readonly", true, PartShip.CS_ConsigneePostcodeInfo.ReadOnly);
		}

		public void TestCS_ConsigneeState()
		{
			PartShip.HouseBill.CS_ConsigneeState = "123";
			AssertEquals("CS_ConsigneeState", "123", PartShip.CS_ConsigneeState);
		}

		public void TestCS_ConsigneeStateInfo()
		{
			AssertEquals("CS_ConsigneeStateInfo.Readonly", true, PartShip.CS_ConsigneeStateInfo.ReadOnly);
		}

		public void TestCS_ConsigneeStreet()
		{
			PartShip.HouseBill.CS_ConsigneeStreet = "123";
			AssertEquals("CS_ConsigneeStreet", "123", PartShip.CS_ConsigneeStreet);
		}

		public void TestCS_ConsigneeStreetInfo()
		{
			AssertEquals("CS_ConsigneeStreetInfo.Readonly", true, PartShip.CS_ConsigneeStreetInfo.ReadOnly);
		}

		public void TestCS_ConsigneeStreet2()
		{
			PartShip.HouseBill.CS_ConsigneeStreet2 = "321";
			AssertEquals("CS_ConsigneeStreet2", "321", PartShip.CS_ConsigneeStreet2);
		}

		public void TestCS_ConsigneeStreet2Info()
		{
			AssertEquals("CS_ConsigneeStreet2Info.Readonly", true, PartShip.CS_ConsigneeStreet2Info.ReadOnly);
		}

		public void TestCS_ConsignorName()
		{
			PartShip.HouseBill.CS_ConsignorName = "123";
			AssertEquals("CS_ConsignorName", "123", PartShip.CS_ConsignorName);
		}

		public void TestCS_ConsignorNameInfo()
		{
			AssertEquals("CS_ConsignorNameInfo.Readonly", true, PartShip.CS_ConsignorNameInfo.ReadOnly);
		}

		public void TestCS_ConsignorPostcode()
		{
			PartShip.HouseBill.CS_ConsignorPostcode = "123";
			AssertEquals("CS_ConsignorPostcode", "123", PartShip.CS_ConsignorPostcode);
		}

		public void TestCS_ConsignorPostcodeInfo()
		{
			AssertEquals("CS_ConsignorPostcodeInfo.Readonly", true, PartShip.CS_ConsignorPostcodeInfo.ReadOnly);
		}

		public void TestCS_ConsignorState()
		{
			PartShip.HouseBill.CS_ConsignorState = "123";
			AssertEquals("CS_ConsignorState", "123", PartShip.CS_ConsignorState);
		}

		public void TestCS_ConsignorStateInfo()
		{
			AssertEquals("CS_ConsignorStateInfo.Readonly", true, PartShip.CS_ConsignorStateInfo.ReadOnly);
		}

		public void TestCS_ConsignorCity()
		{
			PartShip.HouseBill.CS_ConsignorCity = "123";
			AssertEquals("CS_ConsignorCity", "123", PartShip.CS_ConsignorCity);
		}

		public void TestCS_ConsignorCityInfo()
		{
			AssertEquals("CS_ConsignorCityInfo.Readonly", true, PartShip.CS_ConsignorCityInfo.ReadOnly);
		}

		public void TestCS_ConsignorStreet()
		{
			PartShip.HouseBill.CS_ConsignorStreet = "123";
			AssertEquals("CS_ConsignorStreet", "123", PartShip.CS_ConsignorStreet);
		}

		public void TestCS_ConsignorStreetInfo()
		{
			AssertEquals("CS_ConsignorStreetInfo.Readonly", true, PartShip.CS_ConsignorStreetInfo.ReadOnly);
		}

		public void TestCS_ConsignorStreet2()
		{
			PartShip.HouseBill.CS_ConsignorStreet2 = "321";
			AssertEquals("CS_ConsignorStreet2", "321", PartShip.CS_ConsignorStreet2);
		}

		public void TestCS_ConsignorStreet2Info()
		{
			AssertEquals("CS_ConsignorStreet2Info.Readonly", true, PartShip.CS_ConsignorStreet2Info.ReadOnly);
		}

		public void TestCS_OH_Consignee()
		{
			PartShip.HouseBill.CS_OH_Consignee = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("CS_OH_Consignee", PartShip.HouseBill.CS_OH_Consignee, PartShip.CS_OH_Consignee);
		}

		public void TestCS_OH_ConsigneeInfo()
		{
			AssertEquals("CS_OH_ConsigneeInfo.Readonly", true, PartShip.CS_OH_ConsigneeInfo.ReadOnly);
		}

		public void TestCS_OH_Consignor()
		{
			PartShip.HouseBill.CS_OH_Consignor = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("CS_OH_Consignor", PartShip.HouseBill.CS_OH_Consignor, PartShip.CS_OH_Consignor);
		}

		public void TestCS_OH_ConsignorInfo()
		{
			AssertEquals("CS_OH_ConsignorInfo.Readonly", true, PartShip.CS_OH_ConsignorInfo.ReadOnly);
		}

		public void TestCS_RN_NKConsigneeCountry()
		{
			PartShip.HouseBill.CS_RN_NKConsigneeCountry = "US";
			AssertEquals("CS_RN_NKConsigneeCountry", "US", PartShip.CS_RN_NKConsigneeCountry);
		}

		public void TestCS_RN_NKConsigneeCountryInfo()
		{
			AssertEquals("CS_RN_NKConsigneeCountryInfo.Readonly", true, PartShip.CS_RN_NKConsigneeCountryInfo.ReadOnly);
		}

		public void TestCS_RN_NKConsignorCountry()
		{
			PartShip.HouseBill.CS_RN_NKConsignorCountry = "US";
			AssertEquals("CS_RN_NKConsignorCountry", "US", PartShip.CS_RN_NKConsignorCountry);
		}

		public void TestCS_RN_NKConsignorCountryInfo()
		{
			AssertEquals("CS_RN_NKConsignorCountryInfo.Readonly", true, PartShip.CS_RN_NKConsignorCountryInfo.ReadOnly);
		}

		public void TestCS_ChargableWeight()
		{
			PartShip.HouseBill.CS_ChargableWeight = 10m;
			AssertEquals("CS_ChargableWeight", 10m, PartShip.CS_ChargableWeight);
		}

		public void TestCS_ChargableWeightInfo()
		{
			AssertEquals("CS_ChargableWeightInfo.Readonly", true, PartShip.CS_ChargableWeightInfo.ReadOnly);
		}

		public void TestCS_RS_NK_ServiceLevel()
		{
			PartShip.HouseBill.CS_RS_NK_ServiceLevel = "123";
			AssertEquals("CS_RS_NK_ServiceLevel", "123", PartShip.CS_RS_NK_ServiceLevel);
		}

		public void TestCS_RS_NK_ServiceLevelInfo()
		{
			AssertEquals("CS_RS_NK_ServiceLevelInfo.Readonly", true, PartShip.CS_RS_NK_ServiceLevelInfo.ReadOnly);
		}

		public void TestCS_IsSelfAssessedClearance()
		{
			PartShip.HouseBill.CS_IsSelfAssessedClearance = true;
			AssertEquals("CS_IsSelfAssessedClearance", true, PartShip.CS_IsSelfAssessedClearance);
		}

		public void TestCS_IsSelfAssessedClearanceInfo()
		{
			AssertEquals("CS_IsSelfAssessedClearanceInfo.Readonly", true, PartShip.CS_IsSelfAssessedClearanceInfo.ReadOnly);
		}

		public void TestCS_IsPersonalEffects()
		{
			PartShip.HouseBill.CS_IsPersonalEffects = true;
			AssertEquals("CS_IsPersonalEffects", true, PartShip.CS_IsPersonalEffects);
		}

		public void TestCS_IsPersonalEffectsInfo()
		{
			AssertEquals("CS_IsPersonalEffectsInfo.Readonly", true, PartShip.CS_IsPersonalEffectsInfo.ReadOnly);
		}

		public void TestPartShips()
		{
			AssertEquals("Lookups", PartShip.HouseBill.PartShips, ((ICusHAWBBase)PartShip).PartShips);
		}

		public void TestCS_PaymentTypeCaption()
		{
			AssertEquals("CS_PaymentTypeCaption", PartShip.HouseBill.CS_PaymentTypeCaption, PartShip.CS_PaymentTypeCaption);
		}

		public void TestCS_PaymentTypeCaptionInfo()
		{
			AssertEquals("CS_PaymentTypeCaptionInfo.ReadOnly", true, PartShip.CS_PaymentTypeCaptionInfo.ReadOnly);
		}

		public void TestCS_FreightPrepaidCollectForBinding()
		{
			PartShip.HouseBill.CS_FreightPrepaidCollectForBinding = "X";
			AssertEquals("X", PartShip.CS_FreightPrepaidCollectForBinding);
		}

		public void TestCS_FreightPrepaidCollectForBindingInfo()
		{
			AssertEquals("Incorrect property name", CusHAWBBase.Schema.CS_FreightPrepaidCollectForBinding, PartShip.CS_FreightPrepaidCollectForBindingInfo.Name);
			AssertEquals("Incorrect MaxLength", CusHAWBSchema.CS_FreightPrepaidCollect.MaxLength, PartShip.CS_FreightPrepaidCollectForBindingInfo.MaxLength);
			Assert("Has to be read-only", PartShip.CS_FreightPrepaidCollectInfo.ReadOnly);
		}

		public void TestCS_ShipmentTypeForBinding()
		{
			PartShip.HouseBill.CS_ShipmentTypeForBinding = "Y";
			AssertEquals("Y", PartShip.CS_ShipmentTypeForBinding);
		}

		public void TestCS_ShipmentTypeForBindingInfo()
		{
			AssertEquals("Incorrect property name", CusHAWBBase.Schema.CS_ShipmentTypeForBinding, PartShip.CS_ShipmentTypeForBindingInfo.Name);
			AssertEquals("Incorrect MaxLength", CusHAWBSchema.CS_ShipmentType.MaxLength, PartShip.CS_ShipmentTypeForBindingInfo.MaxLength);
			Assert("Has to be read-only", PartShip.CS_ShipmentTypeForBindingInfo.ReadOnly);
		}

		public void TestCusPartShipImplementsIEDIMessageParent()
		{
			PartShip.CG_FlightNo = "QF026";
			PartShip.CG_ArrivalDate = new ZDateTime(2005, 9, 26);
			AssertEquals("MessageHistoryHeading", PartShip.UnderbondHumanReadableName, ((IDetailsTabPageHeadingProvider)PartShip).Heading);
		}

		public void TestGoodsValueInLocalCurrency()
		{
			PartShip.HouseBill.CS_GoodsValue = 100m;
			AssertEquals("GoodsValueInLocalCurrency", PartShip.HouseBill.GoodsValueInLocalCurrency, PartShip.GoodsValueInLocalCurrency);
		}

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = (CusPartShip)GetNewBusinessObject());

		protected override BusinessObject GetNewBusinessObject()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			return hAWB.PartShips.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusPartShip result = (CusPartShip)GetNewBusinessObject();
			result.CG_FlightNo = "QF03";
			result.CG_ArrivalDate = ZDateTime.Now;
			result.CG_PiecesLanded = (short)5;
			result.CG_PiecesManifested = 10;
			result.CG_RL_NKLoadPort = "USLAX";
			result.CG_RL_NKDischargePort = "AUSYD";
			result.CG_CustomsStatus = AirCargoMessage.NewStatus.NotSent;
			return result;
		}
	}
}
