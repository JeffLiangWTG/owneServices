using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterSynchroniserExtensionTest : TestCaseWithFactory
	{
		public void TestIsCanadaPort()
		{
			Assert(new ZString("CABLO").IsCanadaPort());
			Assert(!new ZString("").IsCanadaPort());
			Assert(!new ZString("USCHI").IsCanadaPort());
			Assert(new ZString("CA").IsCanadaPort());
		}

		public void TestGetCusCodesFromOrgAddress()
		{
			AssertEquals("0041", carrierAddress.GetCusCodesFromOrgAddress(OrgCusCode.CACodeTypes.CustomsOfficeCode));
			AssertEquals("0042", carrierAddress.GetCusCodesFromOrgAddress(OrgCusCode.CodeTypes.ControlledPremisesID));
		}

		public void TestGetCusCodesFromCarrierAirCTO()
		{
			var transport = consol.Transports[0];
			transport.JW_OA_CarrierAddress = carrierAddress.PK;

			AssertEquals("Match nothing", ZString.Empty, transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CACodeTypes.CustomsOfficeCode));
			AssertEquals("Match nothing", ZString.Empty, transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID));

			var airCTO2 = carrierAddress.Header.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO2.O5_OA_AgentOfficeAddress = carrierAddress.PK;
			airCTO2.O5_PortOrCountry = "CA001";

			AssertEquals("Match discharge port", "0041", transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CACodeTypes.CustomsOfficeCode));
			AssertEquals("Match discharge port", "0042", transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID));

			airCTO2.O5_PortOrCountry = "CA";

			AssertEquals("Match CA country code", "0041", transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CACodeTypes.CustomsOfficeCode));
			AssertEquals("Match CA country code", "0042", transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID));

			transport.JW_RL_NKDiscPort = "CA002";
			airCTO2.O5_PortOrCountry = "CA002";
			AssertEquals("Match discharge port", "0043", transport.GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID));
		}

		public void TestGetFirstLocoMapFromDischargePort()
		{
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "CA001";

			var result = transport.GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, Constants.TransportModes.Air);
			AssertEquals("1111", result);
			result = transport.GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, Constants.TransportModes.Sea);
			AssertEquals("2222", result);

			result = transport.GetFirstLocoMapFromDischargePort(CACustomsCodeType.SubLocation, Constants.TransportModes.Air);
			AssertEquals("SUB1", result);

			result = transport.GetFirstLocoMapFromDischargePort(CACustomsCodeType.SubLocation, Constants.TransportModes.Sea);
			AssertEquals("SUB1", result);
		}

		ForwardingConsol consol;
		OrgAddress carrierAddress;
		protected override void SetUp()
		{
			base.SetUp();
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var helper = new CusCAeMHTestHelper(Factory);
			consol = helper.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CA001";

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "CA001";

			var locoMapAir = Factory.New<RefLocoMap>();
			locoMapAir.RY_LocalPortCode = "11111";
			locoMapAir.RY_RL_NKLocoPort = "CA001";
			locoMapAir.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapAir.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;
			var locoMapSea = Factory.New<RefLocoMap>();
			locoMapSea.RY_LocalPortCode = "22222";
			locoMapSea.RY_RL_NKLocoPort = "CA001";
			locoMapSea.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSea.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "SUB11";
			locoMapSub.RY_RL_NKLocoPort = "CA001";
			locoMapSub.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			var officeHeader = Factory.NewWithValidTestData<OrgHeader>();
			carrierAddress = officeHeader.MainAddress;
			carrierAddress.OA_RL_NKRelatedPortCode = "CA001";
			var officeAddress = Factory.NewWithValidTestData<OrgAddress>();
			officeAddress.OA_RL_NKRelatedPortCode = "CA002";
			officeAddress.OA_Code = "TEST";
			officeAddress.OA_OH = officeHeader.PK;
			var cusCOCOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0041", canada);
			cusCOCOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var cusCCPOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0042", canada);
			cusCCPOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var cusCCPOffice2 = officeHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0043", canada);
			cusCCPOffice2.OK_OA_PremisesAddress = officeAddress.PK;
			var carrier = carrierAddress.Header;
			var airCTO1 = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO1.O5_OA_AgentOfficeAddress = carrierAddress.PK;
			airCTO1.O5_PortOrCountry = "CA002";

			Factory.Save();
		}
	}
}
