using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterSynchroniserTest : TestCaseWithFactory
	{
		public void TestCustDischargePortAndSubLocationSynchronization_NotSeaAirMode()
		{
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
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
				var helper = new CusCAeMHTestHelper(Factory);
				var consol = helper.Consol;
				consol.JK_TransportMode = Constants.TransportModes.Road;
				consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
				consol.JK_RL_NKDischargePort = "CAZZZ";
				consol.JK_RL_NKLoadPort = "USMIA";

				var masterBill = helper.MasterBill;
				var synchroniser = new CusCAeMHMasterSynchroniser(masterBill, consol);
				synchroniser.SetEnabled(true, false);
				synchroniser.Synchronise();

				AssertEquals(TransportTypeList.Codes.Road, masterBill.BP_ModeOfTransport);
				AssertEquals("1 Transport Leg, CBSA Disc port from CTO", "0011", masterBill.BP_CBSADischargePort);
				AssertEquals("1 Transport Leg, SubLocation From CTO", "0012", masterBill.BP_CBSADischargeSubLocation);
			}
		}

		public void TestCustDischargePortSynchronization_Air()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CA001";

			var masterBill = helper.MasterBill;
			var synchroniser = new CusCAeMHMasterSynchroniser(masterBill, consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("one transport leg default", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
			AssertEquals("NLAMS", transport.JW_RL_NKDiscPort);
			AssertEquals("NLAMS", leg1.JW_RL_NKLoadPort);
			AssertEquals("CA001", leg1.JW_RL_NKDiscPort);

			AssertEquals("CBSA Disc port not fallback, default Empty", ZString.Empty, masterBill.BP_CBSADischargePort);

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("AIR transport : Scenario 1 – Originating outside CA, one and final destination port is in CA", () =>
				{
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port fallback UNLOCO", "1111", masterBill.BP_CBSADischargePort);
					consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
					AssertEquals("CBSA Disc port default from CTO", "0011", masterBill.BP_CBSADischargePort);
				});

				CombineAssertions("AIR transport : Scenario 2 – Originating outside CA, transit via CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					AssertEquals("Legs count", 2, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "3333", masterBill.BP_CBSADischargePort);
				});

				CombineAssertions("AIR transport : Scenario 3 – Originating outside CA, transit via CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					var leg2 = consol.Transports.AddNew();
					leg2.JW_RL_NKLoadPort = "CA001";
					leg2.JW_RL_NKDiscPort = "USLAX";
					AssertEquals("Legs count", 3, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					AssertEquals("CA001", leg2.JW_RL_NKLoadPort);
					AssertEquals("USLAX", leg2.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "3333", masterBill.BP_CBSADischargePort);
				});
			}
		}

		public void TestCustDischargePortSynchronization_Sea()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CA001";

			var masterBill = helper.MasterBill;
			var synchroniser = new CusCAeMHMasterSynchroniser(masterBill, consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("one transport leg default", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
			AssertEquals("NLAMS", transport.JW_RL_NKDiscPort);
			AssertEquals("NLAMS", leg1.JW_RL_NKLoadPort);
			AssertEquals("CA001", leg1.JW_RL_NKDiscPort);

			AssertEquals("CBSA Disc port not fallback, default Empty", ZString.Empty, masterBill.BP_CBSADischargePort);

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Ocean transport : Scenario 1 – Originating outside CA, one and final destination port in CA", () =>
				{
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "2222", masterBill.BP_CBSADischargePort);
				});

				CombineAssertions("Ocean transport : Scenario 2 – Originating outside CA, transit port in CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					AssertEquals("Legs count", 2, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", masterBill.BP_CBSADischargePort);
				});

				CombineAssertions("Ocean transport : Scenario 3 – Originating outside CA, transit port in CA and final destination port is out CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "USLAX";
					AssertEquals("Legs count", 2, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("USLAX", leg1.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", masterBill.BP_CBSADischargePort);
				});

				CombineAssertions("Ocean transport : Scenario 3a – Originating outside CA, multiple transhipment ports in CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					var leg2 = consol.Transports.AddNew();
					leg2.JW_RL_NKLoadPort = "CA001";
					leg2.JW_RL_NKDiscPort = "USLAX";
					AssertEquals("Legs count", 3, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					AssertEquals("CA001", leg2.JW_RL_NKLoadPort);
					AssertEquals("USLAX", leg2.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA arrival port", "4444", masterBill.BP_CBSADischargePort);
				});
			}
		}

		public void TestSubLocationSynchronization_Air()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CA001";

			var masterBill = helper.MasterBill;
			var synchroniser = new CusCAeMHMasterSynchroniser(masterBill, consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("one transport leg default", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";
			AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
			AssertEquals("NLAMS", transport.JW_RL_NKDiscPort);
			AssertEquals("NLAMS", leg1.JW_RL_NKLoadPort);
			AssertEquals("CA001", leg1.JW_RL_NKDiscPort);

			AssertEquals("CBSA sub-location not fallback, default Empty", ZString.Empty, masterBill.BP_CBSADischargeSubLocation);

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("AIR transport : Scenario 1 – Originating outside CA, one and final destination port is in CA", () =>
				{
					synchroniser.Synchronise();
					AssertEquals("CBSA sub-location fallback UNLOCO", "SUB1", masterBill.BP_CBSADischargeSubLocation);
					consol.JK_OA_ArrivalCTOAddress = orgCTO.MainAddress.PK;
					AssertEquals("CBSA sub-location default from CTO", "0012", masterBill.BP_CBSADischargeSubLocation);
				});

				CombineAssertions("AIR transport : Scenario 2 – Originating outside CA, transit via CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					AssertEquals("Legs count", 2, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA sub-location Default from Carrier Air CTO", ZString.Empty, masterBill.BP_CBSADischargeSubLocation);

					transport.JW_OA_CarrierAddress = carrierAddress.PK;
					synchroniser.Synchronise();
					AssertEquals("CBSA sub-location Default from Carrier Air CTO", "0042", masterBill.BP_CBSADischargeSubLocation);
				});

				CombineAssertions("AIR transport : Scenario 3 – Originating outside CA, transit via CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					var leg2 = consol.Transports.AddNew();
					leg2.JW_RL_NKLoadPort = "CA001";
					leg2.JW_RL_NKDiscPort = "USLAX";
					AssertEquals("Legs count", 3, consol.Transports.Count);
					AssertEquals("SGSIN", transport.JW_RL_NKLoadPort);
					AssertEquals("CA002", transport.JW_RL_NKDiscPort);
					AssertEquals("CA002", leg1.JW_RL_NKLoadPort);
					AssertEquals("CA001", leg1.JW_RL_NKDiscPort);
					AssertEquals("CA001", leg2.JW_RL_NKLoadPort);
					AssertEquals("USLAX", leg2.JW_RL_NKDiscPort);
					synchroniser.Synchronise();
					AssertEquals("CBSA sub-location Default from Carrier Air CTO", "0042", masterBill.BP_CBSADischargeSubLocation);
				});
			}
		}

		public void TestSubLocationSynchronization_Sea()
		{
			Assert("Sub-Location not default in this case", true);
		}

		public void TestCusCAeMHMasterSynchroniserSEAMode()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "CAZZZ";
			var locoMapSea = Factory.New<RefLocoMap>();
			locoMapSea.RY_LocalPortCode = "ZZZZ";
			locoMapSea.RY_RL_NKLocoPort = "CAZZZ";
			locoMapSea.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSea.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "XXXX";
			locoMapSub.RY_RL_NKLocoPort = "CAZZZ";
			locoMapSub.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var masterBill = helper.MasterBill;
			var synchroniser = new CusCAeMHMasterSynchroniser(masterBill, consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals(TransportTypeList.Codes.Sea, masterBill.BP_ModeOfTransport);

			consol.JK_MasterBillNum = "NYKSMST072513";
			AssertEquals("NYKSMST072513", masterBill.BP_MasterBill);

			var number = consol.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN1";
			AssertEquals("CCN1", masterBill.BP_PrimaryCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("CCN1", masterBill.BP_PrimaryCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			AssertEquals(ZString.Empty, masterBill.BP_PrimaryCCN);
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCN2";
			AssertEquals("CCN2", masterBill.BP_PrimaryCCN);
			consol.Numbers.Remove(number);
			AssertEquals(ZString.Empty, masterBill.BP_PrimaryCCN);

			number = consol.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			number.CE_EntryNum = "PCN1";
			AssertEquals("PCN1", masterBill.BP_MasterHouseCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("PCN1", masterBill.BP_MasterHouseCCN);
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			AssertEquals(ZString.Empty, masterBill.BP_MasterHouseCCN);
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			number.CE_EntryNum = "PCN2";
			AssertEquals("PCN2", masterBill.BP_MasterHouseCCN);
			consol.Numbers.Remove(number);
			AssertEquals(ZString.Empty, masterBill.BP_MasterHouseCCN);

			consol.JK_RL_NKDischargePort = "CAZZZ";
			consol.JK_RL_NKLoadPort = "USMIA";
			var transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today.AddDays(3);
			transport.JW_ATA = ZDateTime.Today.AddDays(4);
			AssertEquals("CAZZZ", masterBill.BP_RL_NKDiscPort);
			AssertEquals(ZDateTime.Today.AddDays(3), masterBill.BP_ETA);
			AssertEquals(ZDateTime.Today.AddDays(4), masterBill.BP_ATA);
		}

		OrgHeader orgCTO;
		OrgHeader orgCFS;
		OrgAddress carrierAddress;

		protected override void SetUp()
		{
			base.SetUp();

			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
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
