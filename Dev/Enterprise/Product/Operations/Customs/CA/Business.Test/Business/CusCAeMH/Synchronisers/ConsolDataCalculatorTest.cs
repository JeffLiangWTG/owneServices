using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolDataCalculatorTest : Customs.Business.Testing.ConsolDataCalculatorTest
	{
		protected override Customs.Business.ConsolDataCalculator CreateNewCalculator(ForwardingConsol consol)
		{
			var masterBill = Factory.New<CusCAeMHMaster>();
			masterBill.BP_ParentID = consol.PK;
			masterBill.BP_ParentTableCode = consol.TablePrefix;
			masterBill.BP_ModeOfTransport = TransportTypeList.Codes.Sea;

			return new ConsolDataCalculator(consol, masterBill);
		}

		protected override RefUNLOCO CreatePortInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CATOR");
		}

		protected override RefUNLOCO CreatePortInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CABTT");
		}

		protected override RefUNLOCO CreatePortInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CABUB");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
		}

		public void TestFirstCountryDate()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals(ZDateTime.Empty, caCalculator.FirstCountryETADate);
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2016, 10, 3);
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = PortNotInTheCountry2.RL_Code;
			transport.JW_RL_NKDiscPort = PortInTheCountry2.RL_Code;
			transport.JW_ETA = new ZDateTime(2016, 10, 1);
			transport.JW_ATA = new ZDateTime(2016, 10, 2);
			AssertEquals("ETA", new ZDateTime(2016, 10, 1), caCalculator.FirstCountryETADate);
			AssertEquals("ATA", new ZDateTime(2016, 10, 2), caCalculator.FirstCountryATADate);
		}

		public void TestGetDischargePort()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			consol.JK_RL_NKPortOfFirstArrival = string.Empty;
			AssertEquals(string.Empty, caCalculator.GetDischargePort());

			consol.JK_RL_NKPortOfFirstArrival = PortInTheCountry1.RL_Code;
			AssertEquals("CATOR", caCalculator.GetDischargePort());
		}

		public void TestGetSubMasterCCN()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals(string.Empty, caCalculator.GetSubMasterCCN());

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryNum = "010001";
			number1.CE_EntryType = "PCN";
			AssertEquals("010001", caCalculator.GetSubMasterCCN());
		}

		public void TestGetPrimaryCCN()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals(string.Empty, caCalculator.GetSubMasterCCN());

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryNum = "010001";
			number1.CE_EntryType = "CCN";
			AssertEquals("010001", caCalculator.GetPrimaryCCN());
		}

		public void TestGetCBSADischargePortAndSubLocation_NotSeaAirMode()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals("CBSA Disc port not fallback, default Empty", string.Empty, caCalculator.GetCBSADischargePort());
			AssertEquals("CBSA sub-location not fallback, default Empty", ZString.Empty, caCalculator.GetSubLocation());

			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			var orgCTO = Factory.New<OrgHeader>();
			var ctoCusCOC = orgCTO.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0011", canada);
			var ctoCusCCP = orgCTO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0012", canada);
			ctoCusCOC.OK_OA_PremisesAddress = orgCTO.MainAddress.PK;
			ctoCusCCP.OK_OA_PremisesAddress = orgCTO.MainAddress.PK;

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "CAZZZ";
			var locoMapRoad = Factory.New<RefLocoMap>();
			locoMapRoad.RY_LocalPortCode = "ZZZZ";
			locoMapRoad.RY_RL_NKLocoPort = "CAZZZ";
			locoMapRoad.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapRoad.RY_SystemUsage = CALocoMapSystemUsageList.Codes.ROA;
			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "XXXX";
			locoMapSub.RY_RL_NKLocoPort = "CAZZZ";
			locoMapSub.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
				consol.JK_RL_NKDischargePort = "CAZZZ";
				consol.JK_RL_NKLoadPort = "USMIA";

				AssertEquals("1 Transport Leg, CBSA Disc port from CTO", "0011", caCalculator.GetCBSADischargePort());
				AssertEquals("1 Transport Leg, SubLocation From CTO", "0012", caCalculator.GetSubLocation());
			}
		}

		public void TestGetCBSADischargePort_Air()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals("CBSA Disc port not fallback, default Empty", string.Empty, caCalculator.GetCBSADischargePort());

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("CBSA Disc port fallback UNLOCO", "1111", caCalculator.GetCBSADischargePort());

			consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
			AssertEquals("CBSA Disc port default from CTO", "0011", caCalculator.GetCBSADischargePort());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "3333", caCalculator.GetCBSADischargePort());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "CA001";
			leg2.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "3333", caCalculator.GetCBSADischargePort());
		}

		public void TestGetCBSADischargePort_Air_NoDefaultingCustomsCodes()
		{
			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var caCalculator = calculator as ConsolDataCalculator;
				AssertEquals("CBSA Disc port not fallback, default Empty", string.Empty, caCalculator.GetCBSADischargePort());

				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				var transport = consol.Transports[0];
				transport.JW_RL_NKDiscPort = "CA002";
				var leg1 = consol.Transports.AddNew();
				leg1.JW_RL_NKLoadPort = "CA002";
				leg1.JW_RL_NKDiscPort = "CA001";
				var leg2 = consol.Transports.AddNew();
				leg2.JW_RL_NKLoadPort = "CA001";
				leg2.JW_RL_NKDiscPort = "USLAX";

				AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "3333", caCalculator.GetCBSADischargePort());
			}
		}

		public void TestGetCBSADischargePort_Sea()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals("CBSA Disc port not fallback, default Empty", string.Empty, caCalculator.GetCBSADischargePort());

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";

			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "2222", caCalculator.GetCBSADischargePort());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", caCalculator.GetCBSADischargePort());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", caCalculator.GetCBSADischargePort());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "CA001";
			leg2.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", caCalculator.GetCBSADischargePort());
		}

		public void TestGetSubLocation_Air()
		{
			var caCalculator = calculator as ConsolDataCalculator;
			AssertEquals("CBSA sub-location not fallback, default Empty", ZString.Empty, caCalculator.GetSubLocation());

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("CBSA sub-location fallback UNLOCO", "SUB1", caCalculator.GetSubLocation());

			consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
			AssertEquals("CBSA sub-location default from CTO", "0012", caCalculator.GetSubLocation());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("CBSA sub-location Default from Carrier Air CTO", ZString.Empty, caCalculator.GetSubLocation());

			transport.JW_OA_CarrierAddress = carrierAddress.PK;
			AssertEquals("CBSA sub-location Default from Carrier Air CTO", "0042", caCalculator.GetSubLocation());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("CBSA sub-location Default from Carrier Air CTO", "0042", caCalculator.GetSubLocation());

			transport.JW_RL_NKDiscPort = "CA002";
			leg1.JW_RL_NKLoadPort = "CA002";
			leg1.JW_RL_NKDiscPort = "CA001";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "CA001";
			leg2.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("CBSA sub-location Default from Carrier Air CTO", "0042", caCalculator.GetSubLocation());
		}

		OrgHeader orgCTO;
		OrgHeader orgCFS;
		OrgAddress carrierAddress;

		protected override void SetUp()
		{
			base.SetUp();

			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			orgCTO = Factory.NewWithValidTestData<OrgHeader>();
			var ctoCusCOC = orgCTO.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0011", canada);
			ctoCusCOC.OK_OA_PremisesAddress = orgCTO.MainAddress.PK;
			var ctoCusCCP = orgCTO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0012", canada);
			ctoCusCCP.OK_OA_PremisesAddress = orgCTO.MainAddress.PK;

			orgCFS = Factory.NewWithValidTestData<OrgHeader>();
			var cfsCusCOC = orgCFS.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0021", canada);
			cfsCusCOC.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;
			var cfsCusCCP = orgCFS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0022", canada);
			cfsCusCCP.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;

			var arrivalAt = Factory.NewWithValidTestData<OrgHeader>();
			var arrCusCOC = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0031", canada);
			arrCusCOC.OK_OA_PremisesAddress = arrivalAt.MainAddress.PK;
			var arrCusCCP = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0032", canada);
			arrCusCCP.OK_OA_PremisesAddress = arrivalAt.MainAddress.PK;

			var officeHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCOCOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0041", canada);
			var cusCCPOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0042", canada);
			carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			var carrier = carrierAddress.Header;
			var airCTO = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO.O5_OA_AgentOfficeAddress = officeHeader.MainAddress.PK;
			airCTO.O5_PortOrCountry = "CA002";

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
