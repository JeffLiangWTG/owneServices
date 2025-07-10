using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHHouseSynchroniser_NotSeaAirMode()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			shipment.JS_HouseBill = "SHCRHSE072513";
			shipment.JS_TransportMode = Constants.TransportModes.Road;

			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var org = Factory.New<OrgHeader>();
			var cfsCusCOC = org.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0001", canada);
			cfsCusCOC.OK_OA_PremisesAddress = org.MainAddress.PK;

			helper.Consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			helper.Consol.JK_TransportMode = Constants.TransportModes.Road;
			var transport1 = helper.Consol.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Road;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "CATOR";

			var transport2 = helper.Consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_RL_NKLoadPort = "CATOR";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var container1 = helper.Consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000077";
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 77;
			packLine1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packLine1.JL_JC = container1.PK;

			var packLine2 = shipment.InnerPackLines.AddNew();
			packLine2.JL_PackageCount = 33;
			packLine2.JL_F3_NKPackType = Constants.PkgUnit.Carton;
			packLine2.JL_Length = 100;
			packLine2.JL_Width = 50;
			packLine2.JL_Height = 50;

			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("MoveType 24", eMHMovementTypeList.Codes.Import, house.BW_MovementType);

			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("MoveType 23", eMHMovementTypeList.Codes.InTransit, house.BW_MovementType);

			transport2.JW_RL_NKDiscPort = "CAVAN";
			synchroniser.Synchronise();

			AssertEquals("Release Customs Port synchronized", "0001", house.BW_CBSAReleasePort);
			AssertEquals("Release Sub Location synchronized, but no value", ZString.Empty, house.BW_CBSAReleaseSubLocation);

			var cfsCusCCP = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1234", canada);
			cfsCusCCP.OK_OA_PremisesAddress = org.MainAddress.PK;

			helper.Consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			helper.Consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			AssertEquals("MoveType 24", eMHMovementTypeList.Codes.Import, house.BW_MovementType);
			AssertEquals("Release Customs Port synchronized", "0001", house.BW_CBSAReleasePort);
			AssertEquals("Release Sub Location synchronized", "1234", house.BW_CBSAReleaseSubLocation);

			var org2 = Factory.New<OrgHeader>();
			var cfsCusCCP2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "5678", canada);
			cfsCusCCP2.OK_OA_PremisesAddress = org2.MainAddress.PK;
			var cfsCusCOC2 = org2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0002", canada);
			cfsCusCOC2.OK_OA_PremisesAddress = org2.MainAddress.PK;

			shipment.JS_OA_ImportReleaseDepot = org2.MainAddress.PK;

			AssertEquals("Release Customs Port synchronized", "0002", house.BW_CBSAReleasePort);
			AssertEquals("Release Sub Location synchronized", "5678", house.BW_CBSAReleaseSubLocation);
		}

		public void TestDefaultBrokerSecondaryNotifySynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			var importBrokerDocAddress = house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.ImportBroker);
			AssertNotNull(importBrokerDocAddress);
			AssertEquals(null, importBrokerDocAddress.Address);
			var importBroker = Factory.New<OrgHeader>();
			importBroker.OH_IsShippingLine = true;
			importBroker.OH_IsShippingProvider = true;
			importBroker.OH_FullName = "Consignee";
			importBroker.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			importBroker.MainAddress.OA_Address1 = "Address 1";
			shipment.JS_OH_ImportBroker = importBroker.PK;
			var orgCustom = shipment.ImportBroker.CustomsCodes.AddNew();
			orgCustom.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			orgCustom.OK_CodeType = OrgCusCode.CACodeTypes.AccountSecurityCode;
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals(importBroker.MainAddress.OA_Address1, importBrokerDocAddress.Address.Address1);

			var importBroker2 = Factory.New<OrgHeader>();
			importBroker2.OH_IsShippingLine = true;
			importBroker2.OH_IsShippingProvider = true;
			importBroker2.OH_FullName = "Consignee2";
			importBroker2.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			importBroker2.MainAddress.OA_Address1 = "Address 2";
			shipment.JS_OH_ImportBroker = importBroker2.PK;
			AssertEquals(importBroker2.MainAddress.OA_Address1, importBrokerDocAddress.Address.Address1);
		}

		public void TestSendingAgentsSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			var sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_IsShippingLine = true;
			sendAgent.OH_IsShippingProvider = true;
			sendAgent.OH_FullName = "Consignee";
			sendAgent.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			sendAgent.MainAddress.OA_Address1 = "Main Address";
			var address2 = sendAgent.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			address2.OA_Address1 = "Pickup Address";
			var consol = house.MasterBill.Consol;
			consol.JK_OA_SendingForwarderAddress = sendAgent.MainAddress.PK;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			var mainaddress = house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Consolidator);
			AssertNotNull(mainaddress);
			AssertEquals("Main Address", mainaddress.E2_Address1, sendAgent.MainAddress.OA_Address1);
			var pickupaddress = house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.PlaceOfConsolidation);
			AssertNotNull(pickupaddress);
			AssertEquals("Pickup Address", pickupaddress.E2_Address1, address2.OA_Address1);

			address2.OA_Address1 = "Second Pickup Address";
			AssertEquals("Pickup Address", pickupaddress.E2_Address1, address2.OA_Address1);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("", mainaddress.E2_Address1);
			AssertEquals("", pickupaddress.E2_Address1);
		}

		public void TestCusCAeMHHouseSynchroniser()
		{
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			shipment.JS_HouseBill = "SHCRHSE072513";
			AssertEquals("SHCRHSE072513", house.BW_HouseBill);
			synchroniser.Synchronise();
			AssertEquals("MoveType 24", eMHMovementTypeList.Codes.Import, house.BW_MovementType);

			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			synchroniser = new CusCAeMHHouseSynchroniser(house, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			var transportLast = helper.Consol.Transports.Last() as Transport;
			transportLast.JW_RL_NKDiscPort = "USCHI";
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("MoveType 23", eMHMovementTypeList.Codes.InTransit, house.BW_MovementType);

			transportLast.JW_RL_NKDiscPort = "CATOR";
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("MoveType 24", eMHMovementTypeList.Codes.Import, house.BW_MovementType);

			var number = shipment.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN";
			AssertEquals("CCN", house.BW_HouseCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("CCN", house.BW_HouseCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			AssertEquals(ZString.Empty, house.BW_HouseCCN);
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN1";
			AssertEquals("CCN1", house.BW_HouseCCN);

			number = shipment.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference;
			number.CE_EntryNum = "UCR";
			AssertEquals("UCR", house.BW_UCR);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("UCR", house.BW_UCR);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UBR;
			AssertEquals(ZString.Empty, house.BW_UCR);
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference;
			number.CE_EntryNum = "UCR1";
			AssertEquals("UCR1", house.BW_UCR);

			shipment.JS_ActualWeight = 1000m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals(1000m, house.BW_Weight);
			AssertEquals(CanadianUnitOfWeightList.Codes.Kilogram, house.BW_WeightUQ);

			shipment.JS_ActualWeight = 0.1m;
			AssertEquals("Weight rounding", 1m, house.BW_Weight);
			shipment.JS_ActualWeight = 1.8m;
			AssertEquals("Weight rounding", 2m, house.BW_Weight);

			shipment.JS_ActualVolume = 900m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			AssertEquals(900m, house.BW_Volume);
			AssertEquals(CustomsUnitOfMeasureList.Codes.CubicMetre, house.BW_VolumeUQ);

			Assert("Is not a consolidation", !house.BW_IsMasterHouse);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 77;
			packLine1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;

			var packLine2 = shipment.InnerPackLines.AddNew();
			packLine2.JL_PackageCount = 33;
			packLine2.JL_F3_NKPackType = Constants.PkgUnit.Carton;
			packLine2.JL_Length = 100;
			packLine2.JL_Width = 50;
			packLine2.JL_Height = 50;

			var transport = helper.Consol.Transports.Last() as Transport;
			transport.JW_RL_NKDiscPort = "CAYYZ";
			synchroniser.Synchronise();
			AssertEquals("MoveType 24", eMHMovementTypeList.Codes.Import, house.BW_MovementType);
			transport.JW_RL_NKDiscPort = "USCHI";
			synchroniser.Synchronise();
			AssertEquals("MoveType 23", eMHMovementTypeList.Codes.InTransit, house.BW_MovementType);
		}

		OrgHeader shpCFS;
		OrgHeader orgCFS;
		OrgAddress carrierAddress;
		OrgAddress arrivalAtAddress;
		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			shpCFS = Factory.NewWithValidTestData<OrgHeader>();
			var ctoCusCOC = shpCFS.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0011", canada);
			ctoCusCOC.OK_OA_PremisesAddress = shpCFS.MainAddress.PK;
			var ctoCusCCP = shpCFS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0012", canada);
			ctoCusCCP.OK_OA_PremisesAddress = shpCFS.MainAddress.PK;

			orgCFS = Factory.NewWithValidTestData<OrgHeader>();
			var cfsCusCOC = orgCFS.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0021", canada);
			cfsCusCOC.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;
			var cfsCusCCP = orgCFS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0022", canada);
			cfsCusCCP.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;

			var arrivalAt = Factory.NewWithValidTestData<OrgHeader>();
			arrivalAtAddress = arrivalAt.MainAddress;
			var arrCusCOC = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0031", canada);
			arrCusCOC.OK_OA_PremisesAddress = arrivalAtAddress.PK;
			var arrCusCCP = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0032", canada);
			arrCusCCP.OK_OA_PremisesAddress = arrivalAtAddress.PK;

			var officeHeader = Factory.NewWithValidTestData<OrgHeader>();
			carrierAddress = officeHeader.MainAddress;
			var cusCOCOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0041", canada);
			cusCOCOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var cusCCPOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0042", canada);
			cusCCPOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var airCTO = officeHeader.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO.O5_OA_AgentOfficeAddress = officeHeader.MainAddress.PK;
			airCTO.O5_PortOrCountry = "CA";

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "CA001";

			var locoMapAir = Factory.New<RefLocoMap>();
			locoMapAir.RY_LocalPortCode = "1111";
			locoMapAir.RY_RL_NKLocoPort = "CA001";
			locoMapAir.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapAir.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;
			var locoMapSea = Factory.New<RefLocoMap>();
			locoMapSea.RY_LocalPortCode = "2222";
			locoMapSea.RY_RL_NKLocoPort = "CA001";
			locoMapSea.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSea.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "SUB1";
			locoMapSub.RY_RL_NKLocoPort = "CA001";
			locoMapSub.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "CA002";
			var locoMapAir2 = Factory.New<RefLocoMap>();
			locoMapAir2.RY_LocalPortCode = "3333";
			locoMapAir2.RY_RL_NKLocoPort = "CA002";
			locoMapAir2.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapAir2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;
			var locoMapSea2 = Factory.New<RefLocoMap>();
			locoMapSea2.RY_LocalPortCode = "4444";
			locoMapSea2.RY_RL_NKLocoPort = "CA002";
			locoMapSea2.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSea2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			Factory.Save();
		}
	}
}
