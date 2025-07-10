using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	sealed class CusMawbLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestTypeOfLookups()
		{
			cusMAWB = Factory.New<CusMAWB>();
			AssertType(typeof(CusMAWBLookups), cusMAWB.Lookups);
		}

		public void TestAgentLookups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsAgentCode, "Customs Agent");
			var agent = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.CustomsAgentCode, "ZPE", "Torque Leeds", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			cusMAWB = Factory.New<CusMAWB>();
			Assert("Agent lookups should have badge ZPE in the list, coming from RefZZ", cusMAWB.Lookups.AgentsList.ContainsCode("ZPE"));
		}

		public void TestShedsPerAirport()
		{
			ShedTest.CreateShed(Factory, "GB", "LBAELX", "LBAELX  TORQUE LOGISTICS LIMITED at Leeds / Bradford", portName: "Leeds Bradford");
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", portName: "Heathrow");
			ShedTest.CreateShed(Factory, "GB", "LHRBBB", "Cns Shed to be excluded", portName: "Heathrow", siteIdAttribute: "SITE1");
			ShedTest.CreateShed(Factory, "GB", "STNBAC", "CLARIDON GROUP LTD at LONDON STANSTED AIRPORT", portName: "Stansted");
			ShedTest.CreateShed(Factory, "GB", "STNFHS", "FEDERAL EXPRESS EUROPE INC at LONDON STANSTED AIRPORT", portName: "Stansted");
			ShedTest.CreateShed(Factory, "GB", "XYZDJC", "Shed without an AirportName attribute");
			Factory.Save();

			cusMAWB = Factory.New<CusMAWB>();
			AssertEquals("Sheds lookup should contain ELX for LBA", true, cusMAWB.Lookups.ShedsList.ContainsCode("LBAELX"));
			AssertEquals("Sheds lookup should contain BAC for LHR", true, cusMAWB.Lookups.ShedsList.ContainsCode("LHRBAC"));
			AssertEquals("Sheds lookup should contain BAC for STN", true, cusMAWB.Lookups.ShedsList.ContainsCode("STNBAC"));
			AssertEquals("Sheds lookup should contain FHS for STN", true, cusMAWB.Lookups.ShedsList.ContainsCode("STNFHS"));
			AssertEquals("Sheds lookup should NOT contain DJC", false, cusMAWB.Lookups.ShedsList.ContainsCode("XYZDJC"));
			AssertEquals("Sheds lookup should NOT contain LHRBBB because it has the site attribute", false, cusMAWB.Lookups.ShedsList.ContainsCode("LHRBBB"));

			cusMAWB.CargoTerminalOperator = "";
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LBA"));
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LHR"));
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("STN"));
			AssertEquals(false, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("XYZ"));

			cusMAWB.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			AssertEquals(false, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LBA"));
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LHR"));
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("STN"));

			cusMAWB.CargoTerminalOperatorAirportAndShed = "STNFHS";
			AssertEquals(false, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LBA"));
			AssertEquals(false, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("LHR"));
			AssertEquals(true, cusMAWB.Lookups.UkInventoryControlledAirportsList.ContainsCode("STN"));
		}

		public void TestBoringLookupsCode()
		{
			cusMAWB = Factory.New<CusMAWB>();
			AssertEquals("Weights lookup should contain Kilos", true, cusMAWB.Lookups.WeightUnitList.ContainsCode("kg"));
		}

		public void TestProfile()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000LXA";
			AssertEquals("CUKFFW98000LXA", cusMawb.Profile);
			Factory.Save();
			var cusMawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(cusMawb.PK);
			AssertEquals("CUKFFW98000LXA", cusMawbReloaded.Profile);

			AssertEquals("When selecting an agent profile, Agent field is readonly", true, cusMawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Agent field is set", "LXA", cusMawb.AgentBadge);
			AssertEquals("When selecting an agent profile, Airport and Shed field is not readonly", false, cusMawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Number of pieces received field is readonly", true, cusMawb.NumberOfPiecesReceivedInfo.ReadOnly);
			cusMawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals("When selecting a shed profile, Agent field is not readonly", false, cusMawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is readonly", true, cusMawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is updated", "BAC", cusMawb.CargoTerminalOperator);
			AssertEquals("When selecting a shed profile, Airport field is updated", "LHR", cusMawb.CargoTerminalOperatorAirport);
			AssertEquals("When selecting a shed profile, Number of pieces received field is not readonly", false, cusMawb.NumberOfPiecesReceivedInfo.ReadOnly);
		}
		CusMAWB cusMAWB;
	}
}
