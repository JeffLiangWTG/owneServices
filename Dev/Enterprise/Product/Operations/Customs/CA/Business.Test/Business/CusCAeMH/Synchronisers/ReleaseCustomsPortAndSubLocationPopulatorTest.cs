using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ReleaseCustomsPortAndSubLocationPopulatorTest : TestCaseWithFactory
	{
		public void TestGetReleaseCustomsPort_Air()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var shipment = helper.Shipment;
			var house = helper.House;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";

			var populator = new ReleaseCustomsPortAndSubLocationPopulator(shipment, house);

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("AIR transport : Scenario 1 – Originating outside CA, one and final destination port is in CA", () =>
				{
					consol.JK_OA_UnpackDepotAddress = orgCFS.MainAddress.PK;
					AssertEquals("Release Customs Port fallback CFS on Consol", "0021", populator.GetReleaseCustomsPort());
					shipment.JS_OA_ImportReleaseDepot = shpCFS.MainAddress.PK;
					AssertEquals("Release Customs Port default from CFS on Shipment", "0011", populator.GetReleaseCustomsPort());
				});

				CombineAssertions("AIR transport : Scenario 2 – Originating outside CA, transit via CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					AssertEquals("Release Customs Port default from CFS on Shipment", "0011", populator.GetReleaseCustomsPort());
				});

				CombineAssertions("AIR transport : Scenario 3 – Originating outside CA, transit via CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "USLAX";
					AssertEquals("CBSA Disc port Default from UNLOCO for last CA arrival port", "3333", populator.GetReleaseCustomsPort());
				});
			}
		}

		public void TestGetReleaseCustomsPort_Sea()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var shipment = helper.Shipment;
			var house = helper.House;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";

			var populator = new ReleaseCustomsPortAndSubLocationPopulator(shipment, house);
			AssertEquals("Dest./Exit Sub-location not fallback, default Empty", ZString.Empty, populator.GetReleaseCustomsPort());

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Ocean transport : Scenario 1 – Originating outside CA, one and final destination port in CA", () =>
				{
					shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
					shipment.JS_OA_ImportReleaseDepot = shpCFS.MainAddress.PK;
					AssertEquals("Release Customs Port default from CFS on Shipment  on LCL mode", "0011", populator.GetReleaseCustomsPort());

					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					consol.JK_OA_ArrivalCTOAddress = orgCFS.MainAddress.PK;
					AssertEquals("Release Customs Port fallback CFS on Consol on FCL mode", "0021", populator.GetReleaseCustomsPort());
				});

				CombineAssertions("Ocean transport : Scenario 2 – Originating outside CA, transit port in CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
					AssertEquals("Release Customs Port default from CFS on Shipment on LCL mode", "0011", populator.GetReleaseCustomsPort());

					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					AssertEquals("Release Customs Port fallback CFS on Consol on FCL mode", "0021", populator.GetReleaseCustomsPort());
				});

				CombineAssertions("Ocean transport : Scenario 3 – Originating outside CA, transit port in CA and final destination port is out CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "USLAX";
					shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
					AssertEquals("Release Customs Port default from CFS on Shipment on LCL mode", "0011", populator.GetReleaseCustomsPort());

					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					transport.JW_OA_ArrivalLocation = carrierAddress.PK;

					AssertEquals("Release Customs Port from ArrivalLocation on FCL mode", "0041", populator.GetReleaseCustomsPort());
				});

				CombineAssertions("Ocean transport : Scenario 3a – Originating outside CA, multiple transhipment ports in CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					var leg2 = consol.Transports.AddNew();
					leg2.JW_RL_NKLoadPort = "CA001";
					leg2.JW_RL_NKDiscPort = "USLAX";

					leg2.JW_OA_DepartureLocation = carrierAddress.PK;
					AssertEquals("CBSA Disc port Default from UNLOCO for first CA Departure Location port", "0041", populator.GetReleaseCustomsPort());
				});
			}
		}

		public void TestGetReleaseSubLocation_Air()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var shipment = helper.Shipment;
			var house = helper.House;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";

			var populator = new ReleaseCustomsPortAndSubLocationPopulator(shipment, house);
			AssertEquals("Dest./Exit Sub-location not fallback, default Empty", ZString.Empty, populator.GetReleaseSubLocation());

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("AIR transport : Scenario 1 – Originating outside CA, one and final destination port is in CA", () =>
				{
					consol.JK_OA_UnpackDepotAddress = orgCFS.MainAddress.PK;
					AssertEquals("Release SubLocation fallback CFS on Consol", "0022", populator.GetReleaseSubLocation());
					shipment.JS_OA_ImportReleaseDepot = shpCFS.MainAddress.PK;
					AssertEquals("Release SubLocation default from CFS on Shipment", "0012", populator.GetReleaseSubLocation());
				});

				CombineAssertions("AIR transport : Scenario 2 – Originating outside CA, transit via CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					AssertEquals("Release Customs Port default from CFS on Shipment", "0012", populator.GetReleaseSubLocation());
				});

				CombineAssertions("AIR transport : Scenario 3 – Originating outside CA, transit via CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "USLAX";
					transport.JW_OA_CarrierAddress = carrierAddress.PK;
					AssertEquals("CBSA Disc port Default from UNLOCO for last CA arrival port", "0042", populator.GetReleaseSubLocation());
				});
			}
		}

		public void TestGetReleaseSubLocation_Sea()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var shipment = helper.Shipment;
			var house = helper.House;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NLAMS";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NLAMS";
			leg1.JW_RL_NKDiscPort = "CA001";

			var populator = new ReleaseCustomsPortAndSubLocationPopulator(shipment, house);
			AssertEquals("Dest./Exit Sub-location not fallback, default Empty", ZString.Empty, populator.GetReleaseSubLocation());

			using (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Ocean transport : Scenario 1 – Originating outside CA, one and final destination port in CA", () =>
				{
					shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
					shipment.JS_OA_ImportReleaseDepot = shpCFS.MainAddress.PK;
					AssertEquals("Release SubLocation default from CFS on Shipment", "0012", populator.GetReleaseSubLocation());

					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					consol.JK_OA_ArrivalCTOAddress = orgCFS.MainAddress.PK;
					AssertEquals("Release SubLocation default from CTO on Consol", "0022", populator.GetReleaseSubLocation());
				});

				CombineAssertions("Ocean transport : Scenario 2 – Originating outside CA, transit port in CA and final destination port is in CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					transport.JW_OA_DepartureLocation = carrierAddress.PK;
					AssertEquals("Release Customs Port default from Departure Location", "0022", populator.GetReleaseSubLocation());
				});

				CombineAssertions("Ocean transport : Scenario 3 – Originating outside CA, transit port in CA and final destination port is out CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "USLAX";
					shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
					AssertEquals("Release SubLocation default from CFS on Shipment", "0012", populator.GetReleaseSubLocation());

					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					transport.JW_OA_ArrivalLocation = arrivalAtAddress.PK;
					AssertEquals("Release SubLocation default from Carrier's Arrival Location", "0032", populator.GetReleaseSubLocation());
				});

				CombineAssertions("Ocean transport : Scenario 3a – Originating outside CA, multiple transhipment ports in CA and final destination port outside CA", () =>
				{
					transport.JW_RL_NKDiscPort = "CA002";
					leg1.JW_RL_NKLoadPort = "CA002";
					leg1.JW_RL_NKDiscPort = "CA001";
					var leg2 = consol.Transports.AddNew();
					leg2.JW_RL_NKLoadPort = "CA001";
					leg2.JW_RL_NKDiscPort = "USLAX";
					leg2.JW_OA_DepartureLocation = carrierAddress.PK;
					AssertEquals("Release Customs Port default from Departure Location", "0042", populator.GetReleaseSubLocation());
				});
			}
		}

		OrgHeader shpCFS;
		OrgHeader orgCFS;
		OrgAddress carrierAddress;
		OrgAddress arrivalAtAddress;

		protected override void SetUp()
		{
			base.SetUp();
			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
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
