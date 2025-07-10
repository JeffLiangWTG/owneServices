using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(TranshipmentRemoval))]
	class TranshipmentRemovalTests : EnterpriseBusinessObjectTestCase
	{
		public void TestICusUnderbond_TranshipmentRemovalCorrectlySetup()
		{
			var basic = Factory.New<CusMAWB>();
			var tsr = basic.TSRs.AddNew();

			AssertType<TranshipmentRemoval>(Factory.Load<CusUnderbond>(tsr.PK));
			AssertType<TranshipmentRemoval>(Factory.Load<Integration.Customs.GB.CCSUK.ICusUnderbond_TranshipmentRemoval>(tsr.PK));
		}

		public void TestSetDefaultValues()
		{
			var basic = Factory.New<CusMAWB>();
			var tsr = basic.TSRs.AddNew();
			AssertEquals(CusUnderbond.Schema.C4_ApplicationCode, CusUnderbondApplicationCodeList.Codes.GBTranshipmentRemoval, tsr.C4_ApplicationCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var basic = Factory.New<CusMAWB>();
			return basic.TSRs.AddNew();
		}

		public void TestReadOnly()
		{
			var ub = (CusUnderbond)GetNewBusinessObject();
			AssertEquals(false, ub.ReadOnly);
			ub.C4_Status = EDIMessage.Status.Acknowledged;
			AssertEquals(false, ub.ReadOnly);
			ub.C4_Status = EDIMessage.Status.Pending;
			AssertEquals(false, ub.ReadOnly);
			ub.C4_Status = EDIMessage.Status.Cancelled;
			AssertEquals(true, ub.ReadOnly);
		}

		public void TestCurrencyCodeAndValueOfGoods()
		{
			var basic = Factory.New<CusMAWB>();
			basic.MasterLevelHouseHelper.CS_GoodsValue = 10m;
			basic.MasterLevelHouseHelper.CS_RX_NKGoodsCurrency = "GBP";
			var ub1 = basic.TSRs.AddNew();
			var ub2 = basic.TSRs.AddNew();
			ub1.ValueOfGoods = 11m;
			ub1.CurrencyCode = "USD";
			ub2.ValueOfGoods = 12m;
			ub2.CurrencyCode = "AUD";
			AssertEquals("USD", ub1.CurrencyCode);
			AssertEquals("AUD", ub2.CurrencyCode);
			AssertEquals(11m, ub1.ValueOfGoods);
			AssertEquals(12m, ub2.ValueOfGoods);
			AssertEquals("GBP", basic.MasterLevelHouseHelper.CS_RX_NKGoodsCurrency);
			AssertEquals(10m, basic.MasterLevelHouseHelper.CS_GoodsValue);
			Factory.Save();
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			var ub1Reloaded = basicReloaded.Factory.Load<TranshipmentRemoval>(ub1.PK);
			var ub2Reloaded = basicReloaded.Factory.Load<TranshipmentRemoval>(ub2.PK);
			AssertEquals("USD", ub1Reloaded.CurrencyCode);
			AssertEquals("AUD", ub2Reloaded.CurrencyCode);
			AssertEquals(11m, ub1Reloaded.ValueOfGoods);
			AssertEquals(12m, ub2Reloaded.ValueOfGoods);
			AssertEquals("GBP", basicReloaded.MasterLevelHouseHelper.CS_RX_NKGoodsCurrency);
			AssertEquals(10m, basicReloaded.MasterLevelHouseHelper.CS_GoodsValue);
		}

		public void TestAgentsReference()
		{
			var basic = Factory.New<CusMAWB>();
			var ub = basic.TSRs.AddNew();
			ub.C4_SendersMessageReference = "U2345678911234567892";
			AssertEquals("U2345678911234567892", ub.C4_SendersMessageReference);
			AssertEquals("23456789", ub.AgentsReference);

			ub.AgentsReference = "U987654321";
			AssertEquals("U987654321", ub.C4_SendersMessageReference);
			AssertEquals("98765432", ub.AgentsReference);

			ub.AgentsReference = "123456789";
			AssertEquals("U123456789", ub.C4_SendersMessageReference);
			AssertEquals("12345678", ub.AgentsReference);
		}

		public void TestDescription()
		{
			var basic = Factory.New<CusMAWB>();
			var tsr = basic.TSRs.AddNew();
			tsr.AirportOrCountryOfDestination = "DAN";
			AssertEquals("Transhipment removal to DAN", tsr.Description);
		}

		public void TestSplitReferenceToWhichThisRemovalPertains()
		{
			var basic = Factory.New<CusMAWB>();
			var tsr = basic.TSRs.AddNew();
			AssertEquals("Is readonly because no splits exists", true, tsr.SplitReferenceToWhichThisRemovalPertainsInfo.ReadOnly);
			var split01 = basic.Splits.AddNew();
			AssertEquals("Is not readonly because splits exists so should be able to choose one", false, tsr.SplitReferenceToWhichThisRemovalPertainsInfo.ReadOnly);

			tsr.SplitReferenceToWhichThisRemovalPertains = "69";
			AssertEquals("69", tsr.SplitReferenceToWhichThisRemovalPertains);
			Factory.Save();
			AssertEquals("69", new BusinessObjectFactory().Load<TranshipmentRemoval>(tsr.PK).SplitReferenceToWhichThisRemovalPertains);

			var split02 = basic.Splits.AddNew();
			split02.SplitReference = "02";
			split02.NumberOfPiecesExpected = 2;
			tsr.SplitReferenceToWhichThisRemovalPertains = "02";
			AssertEquals(2, tsr.NoPackagesExpected);
		}

		public void TestParentsSplitsThatAreNotLocked()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			var split3 = basic.Splits.AddNew();
			var split4 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split3.SplitReference = "03";
			split4.SplitReference = "04";
			split1.NumberOfPiecesExpected = 1;
			split2.NumberOfPiecesExpected = 200;
			split3.NumberOfPiecesExpected = 300;
			split4.NumberOfPiecesExpected = 400;
			var tsr = basic.TSRs.AddNew();
			AssertEquals(4, tsr.Lookups.ParentsSplitsThatAreNotLocked.Count);
			split2.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestAccepted, ZDateTime.BrettsBirthday);
			split3.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			var collection = tsr.Lookups.ParentsSplitsThatAreNotLocked;
			AssertEquals(2, collection.Count);
			AssertEquals("01", collection[0].Code);
			AssertEquals("04", collection[1].Code);
			AssertEquals("SRF 01, 1 piece", collection[0].Description);
			AssertEquals("SRF 04, 400 pieces", collection[1].Description);
		}

		[StressTest]
		public void TestLookups()
		{
			var awb = Factory.New<CusMAWB>();
			var tsr = awb.TSRs.AddNew();
			AssertType(typeof(TranshipmentRemovalLookups), tsr.Lookups);
			var allLookups = tsr.Lookups.PortOfShipmentList;
			tsr.OnwardMode = ModesOfTransportCodes.Codes.Air;
			var airLookups = tsr.Lookups.PortOfShipmentList;
			tsr.OnwardMode = ModesOfTransportCodes.Codes.Maritime;
			var seaLookups = tsr.Lookups.PortOfShipmentList;

			AssertNotEquals(airLookups.Count, seaLookups.Count);
			AssertNotEquals(airLookups.Count, allLookups.Count);

			AssertEquals("Air lookups contain LHR, Heathrow", "HEATHROW APT/LONDON", airLookups["LHR"].Description.ToUpper());
			AssertEquals("Air lookups contain STN, Stansted", "STANSTED APT/LONDON", airLookups["STN"].Description.ToUpper());
			AssertEquals("Air lookups doesn't contain LSA", null, airLookups["LSA"]);
			AssertEquals("Air lookups contain SOU, Southampton", "SOUTHAMPTON", airLookups["SOU"].Description.ToUpper());
			AssertEquals("Air lookups doesn't contain SHP", null, airLookups["SHP"]);

			AssertEquals("Sea lookups contain SHP, Southampton", "SOUTHAMPTON", seaLookups["SHP"].Description.Trim().ToUpper());
			AssertEquals("Sea lookups doesn't contain LSA", null, seaLookups["LSA"]);
			AssertEquals("Sea lookups doesn't contain SOU", null, seaLookups["SOU"]);
			AssertEquals("Sea lookups doesn't contain STN", null, seaLookups["STN"]);
			AssertEquals("Sea lookups contains AOP, All Other Ports", "ALL OTHER (SEA) PORTS", seaLookups["AOP"].Description.ToUpper());

			AssertEquals("Lookups without a mode is empty", 0, allLookups.Count);

			AssertEquals(1, tsr.Lookups.Currencies.Find(x => x.RX_Code == "USD").Count());
			AssertEquals(1, tsr.Lookups.Carriers.Find(x => x.RM_TwoCharacterCode == "BA").Count());
			var nonUkAirports = tsr.Lookups.NonUkAirportsCollection;
			AssertEquals(1, nonUkAirports.Count(x => x.RL_Code == "FRANT"));
			AssertEquals(1, nonUkAirports.Count(x => x.RL_Code == "AUSYD"));
			AssertEquals(false, nonUkAirports.Any(x => x.RL_Code == "GBLHR"));
		}

		public void TestTranshipmentEntryNumber()
		{
			var tsr = Factory.New<TranshipmentRemoval>();
			tsr.TranshipmentEntryNumber = "12345678";
			AssertEquals("1234567", tsr.TranshipmentEntryNumber); // NB, 7 chars
			Factory.Save();
			var tsrReloaded = new BusinessObjectFactory().Load<TranshipmentRemoval>(tsr.PK);
			AssertEquals("1234567", tsrReloaded.TranshipmentEntryNumber);
		}
	}

	[TestedType(typeof(InterAirportRemoval))]
	class InterAirportRemovalTests : EnterpriseBusinessObjectTestCase
	{
		public void TestICusUnderbond_InterAirportRemovalCorrectlySetup()
		{
			var basic = Factory.New<CusMAWB>();
			var underbond = basic.IARs.AddNew();
			AssertType<InterAirportRemoval>(Factory.Load<CusUnderbond>(underbond.PK));
			AssertType<InterAirportRemoval>(Factory.Load<Integration.Customs.GB.CCSUK.ICusUnderbond_InterAirportRemoval>(underbond.PK));
		}

		public void TestSetDefaultValues()
		{
			var basic = Factory.New<CusMAWB>();
			var iar = basic.IARs.AddNew();
			AssertEquals(CusUnderbond.Schema.C4_ApplicationCode, CusUnderbondApplicationCodeList.Codes.GBInterAirportRemoval, iar.C4_ApplicationCode);
		}

		public void TestDescription()
		{
			var basic = Factory.New<CusMAWB>();
			var iar = basic.IARs.AddNew();
			iar.AirportOrCountryOfDestination = "LHR";
			iar.NewShedId = "BAC";
			AssertEquals("Inter-airport removal to LHRBAC", iar.Description);
		}

		[StressTest]
		public void TestLookupsForIAR()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRACS", "AIR CANADA  at Heathrow", portName: "Heathrow");
			ShedTest.CreateShed(Factory, "GB", "STNBAC", "CLARIDON GROUP LTD at LONDON STANSTED AIRPORT", chiefPort: "LSA", portName: "Stansted");
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			var ub = mawb.IARs.AddNew();
			AssertType(typeof(InterAirportRemovalLookups), ub.Lookups);
			ub.OnwardMode = ModesOfTransportCodes.Codes.Air;
			Assert(ub.Lookups.AirportsOfDestinationList.ContainsCode("LHR"));
			Assert(!ub.Lookups.AirportsOfDestinationList.ContainsCode("FXT"));  // sea port
			Assert(!ub.Lookups.AirportsOfDestinationList.ContainsCode("LKH"));  // military port
			Assert(ub.Lookups.AirportsOfDestinationList.ContainsCode("STN"));  // Stansted standard code

			AssertEquals(1, ub.Lookups.Carriers.Count(x => x.RM_TwoCharacterCode == "BA"));

			Assert(ub.Lookups.ShedsList.ContainsCode("ACS"));
			ub.AirportOfDestination = "STN";
			Assert(!ub.Lookups.ShedsList.ContainsCode("ACS"));
			Assert(ub.Lookups.ShedsList.ContainsCode("BAC"));

			ub.AirportOfDestination = "YYY";
			ub.NewShedId = "ZZZ";
			AssertHasMessageErrorContaining(ub.AirportOfDestinationInfo, "not valid");
			AssertHasMessageErrorContaining(ub.AirportOfDestinationInfo, "not in the list");
			AssertHasMessageErrorContaining(ub.NewShedIdInfo, "not valid");
			ub.AirportOfDestination = AirportsOfDestinationIAR.Codes.DummyAirportForSdcTMGreaterThanE;
			AssertEquals(1, ub.Lookups.ShedsList.Count);
			ub.NewShedId = ub.Lookups.ShedsList[0].Code;
			AssertEquals("XXX", ub.NewShedId);
			AssertNoMessageErrorContaining(ub.AirportOfDestinationInfo, "not valid");
			AssertNoMessageErrorContaining(ub.AirportOfDestinationInfo, "not in the list");
			AssertNoMessageErrorContaining(ub.NewShedIdInfo, "not valid");
		}

		public void TestDefaults()
		{
			var ub = Factory.New<InterAirportRemoval>();
			AssertEquals("40", ub.OnwardMode);
		}
	}

	[TestedType(typeof(InterShedRemoval))]
	class InterShedRemovalTests : EnterpriseBusinessObjectTestCase
	{
		public void TestICusUnderbond_InterShedRemovalCorrectlySetup()
		{
			var basic = Factory.New<CusMAWB>();
			var underbond = basic.ISRs.AddNew();
			AssertType<InterShedRemoval>(Factory.Load<CusUnderbond>(underbond.PK));
			AssertType<InterShedRemoval>(Factory.Load<Integration.Customs.GB.CCSUK.ICusUnderbond_InterShedRemoval>(underbond.PK));
		}

		public void TestSetDefaultValues()
		{
			var basic = Factory.New<CusMAWB>();
			var isr = basic.ISRs.AddNew();
			AssertEquals(CusUnderbond.Schema.C4_ApplicationCode, CusUnderbondApplicationCodeList.Codes.GBInterShedRemoval, isr.C4_ApplicationCode);
		}

		public void TestDescription()
		{
			var basic = Factory.New<CusMAWB>();
			var isr = basic.ISRs.AddNew();
			isr.NewShedId = "BAC";
			AssertEquals("Inter-shed removal to BAC", isr.Description);
		}

		public void TestLookups()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRACS", "AIR CANADA  at Heathrow", portName: "Heathrow");
			ShedTest.CreateShed(Factory, "GB", "STNBAC", "CLARIDON GROUP LTD at LONDON STANSTED AIRPORT", chiefPort: "LSA", portName: "Stansted");
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			var ub = mawb.ISRs.AddNew();
			AssertType(typeof(CusUnderbondLookups), ub.Lookups);
			Assert(ub.Lookups.ShedsList.ContainsCode("ACS"));
			mawb.AirportOfDestination = "STN";
			Assert(!ub.Lookups.ShedsList.ContainsCode("ACS"));
			Assert(ub.Lookups.ShedsList.ContainsCode("BAC"));
		}

		public void TestAirportOfDestinationComesFromParent()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CargoTerminalOperatorAirport = "ABC";
			var ub = basic.ISRs.AddNew();
			AssertEquals("ABC", ub.AirportOrCountryOfDestination);
		}
	}

	[TestedType(typeof(Fallback))]
	class FallbackTests : EnterpriseBusinessObjectTestCase
	{
		public void TestICusUnderbond_FallbackCorrectlySetup()
		{
			var basic = Factory.New<CusMAWB>();
			var underbond = basic.FBKs.AddNew();
			AssertType<Fallback>(Factory.Load<CusUnderbond>(underbond.PK));
			AssertType<Fallback>(Factory.Load<Integration.Customs.GB.CCSUK.ICusUnderbond_Fallback>(underbond.PK));
		}

		public void TestSetDefaultValues()
		{
			var basic = Factory.New<CusMAWB>();
			var fbk = basic.FBKs.AddNew();
			AssertEquals(CusUnderbond.Schema.C4_ApplicationCode, CusUnderbondApplicationCodeList.Codes.GBFallback, fbk.C4_ApplicationCode);
		}

		public void TestDescription()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var fbk = hawb.FBKs.AddNew();
			AssertEquals("Fallback", fbk.Description);
		}

		public void TestNoPackagesExpected()
		{
			var mawb = Factory.New<CusMAWB>();
			ICcsukCusAwb hawb = mawb.ChildBills.AddNew();
			hawb.NumberOfPiecesExpected = 69;
			var fbk = hawb.FBKs.AddNew();
			AssertEquals(0, fbk.NoPackagesExpected);
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_PiecesManifested = 69;
			var fbk2 = hawb2.FBKs.AddNew();
			AssertEquals("Packages not propogated because of the ongoing uncommitted row problem", 0, hawb2.FBKs[0].NoPackagesExpected);
		}

		public void TestLookups()
		{
			var ub = Factory.New<Fallback>();
			AssertType(typeof(CusUnderbondLookups), ub.Lookups);
		}
	}

	[TestedType(typeof(CusUnderbondLookups))]
	class CusUnderbondLookupsTest : Customs.Business.Testing.CusUnderbondLookupsTest
	{
		protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<Fallback>();
	}

	[TestedType(typeof(InterAirportRemovalLookups))]
	class InterAirportRemovalLookupsTest : Customs.Business.Testing.CusUnderbondLookupsTest
	{
		protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<InterAirportRemoval>();
	}

	[TestedType(typeof(TranshipmentRemovalLookups))]
	class TranshipmentRemovalLookupsTest : Customs.Business.Testing.CusUnderbondLookupsTest
	{
		protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<TranshipmentRemoval>();
	}

	[TestedType(typeof(CusUnderbondCollection<TranshipmentRemoval>))]
	class CusUnderbondCollectionTests_TSR : BusinessObjectCollectionTestCase
	{
		public void TestTSRCollection()
		{
			var cusMawb1 = Factory.New<CusMAWB>();
			var tsr = Factory.New<TranshipmentRemoval>();
			cusMawb1.TSRs.Add(tsr);
			AssertEquals(1, cusMawb1.TSRs.Count);
			AssertEquals(0, cusMawb1.ISRs.Count);
			AssertEquals(0, cusMawb1.IARs.Count);
			AssertEquals(tsr, cusMawb1.TSRs[0]);
		}

		public void TestNonFallbackUnderbondCollectionSplit()
		{
			var basic = Factory.New<CusMAWB>();
			var splitBasic1 = basic.Splits.AddNew();
			var splitBasic2 = basic.Splits.AddNew();
			var ubBasic = splitBasic1.TSRs.AddNew();
			AssertEquals(ubBasic, splitBasic1.TSRs[0]);
			AssertEquals(ubBasic, basic.TSRs[0]);
			AssertEquals("Two collections are the same instance, not merely copies", splitBasic1.TSRs, basic.TSRs);

			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var splitHouse1 = house.Splits.AddNew();
			var splitHouse2 = house.Splits.AddNew();
			var splitHouseUb = splitHouse1.TSRs.AddNew();
			AssertEquals(splitHouseUb, splitHouse1.TSRs[0]);
			AssertEquals(splitHouseUb, house.TSRs[0]);
			AssertEquals("Two collections are the same instance, not merely copies", splitHouse1.TSRs, house.TSRs);
		}

		public void TestAllowNew()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.Profile = "CUKFFW98000LXA"; // agent
			cusMawb.NumberOfPiecesReceived = 1;
			AssertEquals(true, cusHawb.TSRs.AllowNew);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(false, cusHawb.TSRs.AllowNew);

			cusMawb.NumberOfPiecesReceived = 1;
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, cusHawb.TSRs.AllowNew);
			cusHawb.Profile = "CUKAIR98LHRBAC"; // shed
			AssertEquals(false, cusHawb.TSRs.AllowNew);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA"; // agent
			basic.NumberOfPiecesReceived = 1;
			basic.NumberOfPiecesExpected = 1;
			basic.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, basic.TSRs.AllowNew);
			basic.SetCustomsActionCode("CA", ZDateTime.Now);
			AssertEquals(false, basic.TSRs.AllowNew);
		}

		public void TestTawbTsrDischargeError()
		{
			var basic = Factory.New<CusMAWB>();
			basic.AirportOfArrival = "LHR";
			basic.AirportOfDestination = "SYD";
			var tsr = basic.TSRs.AddNew();
			tsr.C4_RL_NKDischargePort = "BNE";
			AssertHasErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs may not be removed to anywhere except the port of discharge");
			tsr.C4_RL_NKDischargePort = "SYD";
			AssertNoErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs may not be removed to anywhere except the port of discharge");
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusUnderbondCollection<TranshipmentRemoval>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return new CusUnderbondCollection<TranshipmentRemoval>(cusMawb.MasterLevelHouseHelper);
		}
	}

	[TestedType(typeof(CusUnderbondCollection<InterAirportRemoval>))]
	class CusUnderbondCollectionTests_IAR : BusinessObjectCollectionTestCase
	{
		public void TestIARCollection()
		{
			var cusMawb1 = Factory.New<CusMAWB>();
			var iAR = Factory.New<InterAirportRemoval>();
			cusMawb1.IARs.Add(iAR);
			AssertEquals(0, cusMawb1.TSRs.Count);
			AssertEquals(0, cusMawb1.ISRs.Count);
			AssertEquals(1, cusMawb1.IARs.Count);
			AssertEquals(iAR, cusMawb1.IARs[0]);
		}

		public void TestAllowNew()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.Profile = "CUKFFW98000LXA"; // agent
			cusMawb.NumberOfPiecesReceived = 1;
			AssertEquals(true, cusHawb.IARs.AllowNew);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(false, cusHawb.IARs.AllowNew);
			cusHawb.Profile = "CUKAIR98LHRBAC"; // shed
			AssertEquals(false, cusHawb.IARs.AllowNew);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA"; // agent
			basic.NumberOfPiecesReceived = 1;
			basic.NumberOfPiecesExpected = 1;
			basic.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, basic.IARs.AllowNew);

			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			AssertEquals("Cannot make new IAR when have not yet uploaded P5'd record", false, basic.IARs.AllowNew);
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			AssertEquals("After sending message, should allow new record", true, basic.IARs.AllowNew);

			basic.SetCustomsActionCode("CA", ZDateTime.Now);
			AssertEquals("No new underbond with this CAC", false, basic.IARs.AllowNew);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InterAirportRemoval>();
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusUnderbondCollection<InterAirportRemoval>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return new CusUnderbondCollection<InterAirportRemoval>(cusMawb.MasterLevelHouseHelper);
		}
	}

	[TestedType(typeof(CusUnderbondCollection<InterShedRemoval>))]
	class CusUnderbondCollectionTests_ISR : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InterShedRemoval>();
		}

		public void TestISRCollection()
		{
			var cusMawb1 = Factory.New<CusMAWB>();
			var iSR = Factory.New<InterShedRemoval>();
			cusMawb1.ISRs.Add(iSR);
			AssertEquals(0, cusMawb1.TSRs.Count);
			AssertEquals(1, cusMawb1.ISRs.Count);
			AssertEquals(0, cusMawb1.IARs.Count);
			AssertEquals(iSR, cusMawb1.ISRs[0]);
		}

		public void TestAllowNew()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);

			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.NumberOfPiecesReceived = 1;
			cusMawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals(true, cusMawb.ISRs.AllowNew);
			cusMawb.Profile = "CUKFFW98000LXA";
			AssertEquals(false, cusMawb.ISRs.AllowNew);

			var cusHawb = cusMawb.ChildBills.AddNew();
			cusMawb.NumberOfPiecesReceived = 1;
			AssertEquals(false, cusHawb.ISRs.AllowNew);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(false, cusHawb.ISRs.AllowNew);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC"; // shed
			basic.NumberOfPiecesReceived = 1;
			basic.NumberOfPiecesExpected = 1;
			basic.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, basic.ISRs.AllowNew);
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			AssertEquals("Cannot make new ISR when have not yet uploaded P5'd record", false, basic.ISRs.AllowNew);
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			AssertEquals("After sending message, should allow new record", true, basic.ISRs.AllowNew);
			basic.SetCustomsActionCode("CA", ZDateTime.Now);
			AssertEquals(false, basic.ISRs.AllowNew);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusUnderbondCollection<InterShedRemoval>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return new CusUnderbondCollection<InterShedRemoval>(cusMawb.MasterLevelHouseHelper);
		}
	}

	[TestedType(typeof(AllCusUnderbondsCollection))]
	class AllCusUnderbondsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return cusHawb.IARs.AddNew();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusHawb = cusMawb.ChildBills.AddNew();
			return new AllCusUnderbondsCollection(cusHawb);
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		public void TestNPXDefaults()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_PiecesManifested = 567;
			var iar = hawb.IARs.AddNew();
			AssertEquals(567, iar.NoPackagesExpected);
			var isr = hawb.ISRs.AddNew();
			AssertEquals(567, isr.NoPackagesExpected);
			var tsr = hawb.TSRs.AddNew();
			AssertEquals(567, tsr.NoPackagesExpected);
			var fbk = hawb.FBKs.AddNew();
			fbk.NoPackagesExpected = 567;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var hawbReloaded = newFactory.Load<CusHAWB>(hawb.PK);
			AssertEquals(567, hawbReloaded.IARs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloaded.ISRs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloaded.TSRs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloaded.FBKs[0].NoPackagesExpected);
			hawbReloaded.CS_PiecesManifested = 789;
			hawbReloaded.Factory.Save();

			var anotherNewFactory = new BusinessObjectFactory();
			var hawbReloadedAgain = anotherNewFactory.Load<CusHAWB>(hawb.PK);
			AssertEquals(567, hawbReloadedAgain.IARs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloadedAgain.ISRs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloadedAgain.TSRs[0].NoPackagesExpected);
			AssertEquals(567, hawbReloadedAgain.FBKs[0].NoPackagesExpected);
		}

		CusHAWB cusHawb;
	}

	[TestedType(typeof(CusUnderbondCollection<Fallback>))]
	class CusUnderbondCollectionTests_FBK : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<Fallback>();
		}

		public void TestFBKCollection()
		{
			var cusHawb = Factory.New<CusHAWB>();
			var fbk = Factory.New<Fallback>();
			cusHawb.FBKs.Add(fbk);
			AssertEquals(0, cusHawb.TSRs.Count);
			AssertEquals(0, cusHawb.ISRs.Count);
			AssertEquals(0, cusHawb.IARs.Count);
			AssertEquals(1, cusHawb.FBKs.Count);
			AssertEquals(fbk, cusHawb.FBKs[0]);
		}

		public void TestAllowNew()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.Profile = "CUKFFW98000LXA"; // agent
			AssertEquals("Cannot create fallback for prearrival", false, cusHawb.FBKs.AllowNew);
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, cusHawb.FBKs.AllowNew);
			cusHawb.Profile = "CUKAIR98LHRBAC"; // shed
			AssertEquals(false, cusHawb.FBKs.AllowNew);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA"; // agent
			basic.NumberOfPiecesReceived = 1;
			basic.NumberOfPiecesExpected = 1;
			basic.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, basic.FBKs.AllowNew);
			basic.SetCustomsActionCode("CA", ZDateTime.Now);
			AssertEquals(false, basic.FBKs.AllowNew);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusUnderbondCollection<Fallback>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return cusMawb.FBKs;
		}
	}

	class CusUnderbondValidationTest : TestCaseWithFactory
	{
		//SplitReferenceToWhichThisRemovalPertains
		public void TestCheckC4_PackageType()
		{
			var basic = Factory.New<CusMAWB>();
			var ub = basic.TSRs.AddNew();
			ub.Validation.ValidateC4_PackageType();
			AssertNoErrorContaining(ub.C4_PackageTypeInfo, "split");
			ub.SplitReferenceToWhichThisRemovalPertains = "Y";
			AssertNoErrorContaining(ub.C4_PackageTypeInfo, "split");
			var split = basic.Splits.AddNew();
			ub.SplitReferenceToWhichThisRemovalPertains = "";
			AssertHasErrorContaining(ub.C4_PackageTypeInfo, "split");
			ub.SplitReferenceToWhichThisRemovalPertains = "X";
			AssertNoErrorContaining(ub.C4_PackageTypeInfo, "split");

			split.SplitReference = "69";
			ub.SplitReferenceToWhichThisRemovalPertains = "01";
			AssertHasMessageErrorContaining(ub.C4_PackageTypeInfo, "Please select a split from the list");
			ub.SplitReferenceToWhichThisRemovalPertains = "69";
			AssertNoMessageErrorContaining(ub.C4_PackageTypeInfo, "Please select a split from the list");
		}

		public void TestCheckC4_Outurned()
		{
			var underbond = Factory.New<CusUnderbondForTest>();
			underbond.C4_Outurned = ZDateTime.Now;
			Assert("No notifications", !underbond.C4_OuturnedInfo.HasNotifications());
		}

		[RemovalType("XXX")]
		class CusUnderbondForTest : CusUnderbond
		{
			public CusUnderbondForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Description
			{
				get { return "CusUnderbond for TEST"; }
			}

			public override ZString RemovalTypeHuman
			{
				get { return Description; }
			}
		}

		public void TestHumanReadableName()
		{
			var underbond = Factory.New<CusUnderbondForTest>();
			AssertEquals("CusUnderbond for TEST", underbond.HumanReadableName);
		}

		public void TestCheckC4_ParentID_WholeAwb()
		{
			var basic1 = Factory.New<CusMAWB>();
			RunCheckC4_ParentIDForRemovalType_Status1(basic1, delegate(ICcsukCusAwb awb)
			{ return awb.TSRs.AddNew(); });

			var basic2 = Factory.New<CusMAWB>();
			RunCheckC4_ParentIDForRemovalType_Status1(basic2, delegate(ICcsukCusAwb awb)
			{ return awb.IARs.AddNew(); });
			var basic3 = Factory.New<CusMAWB>();
			RunCheckC4_ParentIDForRemovalType_Status1(basic3, delegate(ICcsukCusAwb awb)
			{ return awb.ISRs.AddNew(); });

			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			RunCheckC4_ParentIDForRemovalType_Status1(hawb1, delegate(ICcsukCusAwb awb)
			{ return awb.TSRs.AddNew(); });
			var hawb2 = mawb.ChildBills.AddNew();
			RunCheckC4_ParentIDForRemovalType_Status1(hawb2, delegate(ICcsukCusAwb awb)
			{ return awb.IARs.AddNew(); });
			var hawb3 = mawb.ChildBills.AddNew();
			RunCheckC4_ParentIDForRemovalType_Status1(hawb3, delegate(ICcsukCusAwb awb)
			{ return awb.ISRs.AddNew(); });

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.CustomsEntryHeaders.AddNew().EntryNumber = "123";
			hawb2.CS_JE_CustomsFormalEntry = declaration.PK;
			var tsr6 = hawb2.TSRs.AddNew();
			AssertHasRowMessageErrorContaining(tsr6, "entry are not allowed");

			hawb2.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			var tsr7 = hawb2.TSRs.AddNew();
			AssertNoRowMessageErrorContaining(tsr7, "entry are not allowed");
		}

		void RunCheckC4_ParentIDForRemovalType_Status1(ICcsukCusAwb awb, MakeNewUnderbond underbondMaker)
		{
			awb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
			var ub1 = underbondMaker(awb);
			AssertHasRowMessageErrorContaining(ub1, "Split, wait for or set NPR=NPX");
			awb.NumberOfPiecesReceived = 5;
			awb.NumberOfPiecesExpected = 5;
			awb.Status1Date = ZDateTime.BrettsBirthday;
			var ub2 = underbondMaker(awb);
			AssertNoRowErrorContaining(ub2, "Split, wait for or set NPR=NPX");

			awb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport;
			var ub3 = underbondMaker(awb);
			AssertHasRowMessageErrorContaining(ub3, "SDC=C or E are not allowed");
			awb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromOutsideEC;
			var ub4 = underbondMaker(awb);
			AssertHasRowMessageErrorContaining(ub4, "SDC=C or E are not allowed");
			AssertNoRowErrorContaining(ub4, "SDC=C or E are not allowed");
		}

		[TestedType(typeof(InterShedRemovalValidation))]
		class InterShedRemovalValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
		{
			public override void TestC4_OutturnedValidation()
			{
				Underbond.C4_Outurned = ZDateTime.Empty;
				AssertNoNotifications(Underbond.C4_OuturnedInfo);
			}

			public void TestValidationType()
			{
				var ub = Factory.New<InterShedRemoval>();
				AssertType(typeof(InterShedRemovalValidation), ub.Validation);
			}

			public void TestCheckC4_DischargePremiseID()
			{
				var ub = Factory.New<InterShedRemoval>();
				ub.NewShedId = "X";
				AssertNoMessageErrorContaining(ub.NewShedIdInfo, "enter");
				ub.NewShedId = "";
				AssertHasMessageErrorContaining(ub.NewShedIdInfo, "enter");
			}

			public void TestTawbIsr()
			{
				var basic = Factory.New<CusMAWB>();
				var isr = basic.ISRs.AddNew();
				isr.Validation.ValidateAll();
				AssertNoMessageErrorContaining(isr.C4_ParentIDInfo, "Through AWB");
				basic.AirportOfArrival = "AAA";
				basic.AirportOfDestination = "BBB";
				isr.Validation.ValidateAll();
				AssertHasMessageErrorContaining(isr.C4_ParentIDInfo, "Through AWB");
			}
			protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<InterShedRemoval>();
		}

		public void TestTawbIsrDischargeError()
		{
			var basic = Factory.New<CusMAWB>();
			basic.AirportOfArrival = "LHR";
			basic.AirportOfDestination = "LHR";
			var isr = basic.ISRs.AddNew();
			isr.Validation.ValidateAll();
			AssertNoErrorContaining(isr.C4_RL_NKDischargePortInfo, "Through AWBs may not be removed to anywhere except the port of discharge");
			basic.AirportOfDestination = "MAN";
			isr.Validation.ValidateAll();
			AssertHasErrorContaining(isr.C4_RL_NKDischargePortInfo, "Through AWBs may not be removed to anywhere except the port of discharge");
		}

		[TestedType(typeof(FallbackValidation))]
		class FallbackValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
		{
			public override void TestC4_OutturnedValidation()
			{
				Underbond.C4_Outurned = ZDateTime.Empty;
				AssertNoNotifications(Underbond.C4_OuturnedInfo);
			}

			public void TestValidationType()
			{
				var ub = Factory.New<Fallback>();
				AssertType(typeof(FallbackValidation), ub.Validation);
			}
			protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<Fallback>();
		}

		[TestedType(typeof(TranshipmentRemovalValidation))]
		class TranshipmentRemovalValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
		{
			public override void TestC4_OutturnedValidation()
			{
				Underbond.C4_Outurned = ZDateTime.Empty;
				AssertNoNotifications(Underbond.C4_OuturnedInfo);
			}

			public void TestValidationType()
			{
				var ub = Factory.New<TranshipmentRemoval>();
				AssertType(typeof(TranshipmentRemovalValidation), ub.Validation);
			}

			public void TestValidateLRI()
			{
				var cusMawb = Factory.New<CusMAWB>();
				var ub = cusMawb.TSRs.AddNew();
				ub.Validation.ValidateAll();
				AssertHasErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = YesNoList.Codes.Yes;
				AssertNoErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = YesNoList.Codes.No;
				AssertNoErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = "X";
				AssertHasErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
			}

			public void TestOnwardAwbFormatting()
			{
				var basic = Factory.New<CusMAWB>();
				var ub = basic.IARs.AddNew();
				RunnerForTestOnwardAwbFormattingValidation(ub.OnwardCarrierInfo, ub.C4_MAWBInfo);
			}

			internal static void RunnerForTestOnwardAwbFormattingValidation(ZPropertyInfo carrierInfo, ZPropertyInfo awbInfo)
			{
				AssertNoNotifications("Pre-req", awbInfo);
				awbInfo.Value = ZString.Empty;
				AssertNoNotifications(awbInfo);
				carrierInfo.Value = new ZString("xxx");
				awbInfo.Value = new ZString("LHR");
				AssertHasMessageErrorContaining(awbInfo, "MAWB number must be 11 characters");
				awbInfo.Value = new ZString("LHR11112222");
				AssertNoMessageErrorContaining(awbInfo, "MAWB number must be 11 characters");
				awbInfo.Value = new ZString("???12345678");
				AssertHasMessageErrorContaining(awbInfo, "MAWP must be 3 letters or digits; MAWN must be 8 digits");
				awbInfo.Value = new ZString("12512345675");
				AssertNoMessageError(awbInfo, "MAWP must be 3 letters or digits; MAWN must be 8 digits");
			}

			public void TestOnwardAwbAndCarrierValidation()
			{
				var cusMawb = Factory.New<CusMAWB>();
				var ub = cusMawb.TSRs.AddNew();
				var carrierInfo = ub.OnwardCarrierInfo;
				var awbInfo = ub.OnwardAirWaybillNumberInfo;
				RunnerForTestOnwardAwbCarrierValidation(carrierInfo, awbInfo);
			}

			internal static void RunnerForTestOnwardAwbCarrierValidation(ZPropertyInfo carrierInfo, ZPropertyInfo awbInfo)
			{
				AssertNoMessageErrorContaining(carrierInfo, "Onward");
				AssertNoMessageErrorContaining(awbInfo, "Onward");
				carrierInfo.Value = new ZString("BA");
				AssertHasMessageErrorContaining(carrierInfo, "Onward");
				AssertHasMessageErrorContaining(awbInfo, "Onward");
				awbInfo.Value = new ZString("123");
				AssertNoMessageErrorContaining(carrierInfo, "Onward");
				AssertNoMessageErrorContaining(awbInfo, "Onward");
				carrierInfo.Value = ZString.Empty;
				AssertHasMessageErrorContaining(carrierInfo, "Onward");
				AssertHasMessageErrorContaining(awbInfo, "Onward");
				awbInfo.Value = ZString.Empty;
				AssertNoMessageErrorContaining(carrierInfo, "Onward");
				AssertNoMessageErrorContaining(awbInfo, "Onward");
			}

			public void TestCheckC4_RL_NKDischargePort_GB()
			{
				var basic = Factory.New<CusMAWB>();
				RunBritishAwbTranshipmentTest(basic);
				var hawb = basic.ChildBills.AddNew();
				RunBritishAwbTranshipmentTest(hawb);
			}

			static void RunBritishAwbTranshipmentTest(ICcsukCusAwb awb)
			{
				awb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
				var tsr1 = awb.TSRs.AddNew();
				tsr1.AirportOrCountryOfDestination = "GBLON";
				AssertHasMessageErrorContaining(tsr1.AirportOrCountryOfDestinationInfo, "Destination may not be in GB");
				tsr1.AirportOrCountryOfDestination = "USATL";
				AssertNoMessageErrorContaining(tsr1.AirportOrCountryOfDestinationInfo, "Destination may not be in GB");
				AssertNoMessageErrorContaining(tsr1.AirportOrCountryOfDestinationInfo, "enter");
				tsr1.AirportOrCountryOfDestination = "";
				AssertHasMessageErrorContaining(tsr1.AirportOrCountryOfDestinationInfo, "enter");
			}

			public void TestCheckC4_RL_NKDischargePort_Through()
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.AirportOfOrigin = "USJFK";
				mawb.AirportOfDestination = "LHR";
				mawb.AirportOfArrival = "LHR";
				var hawb = mawb.ChildBills.AddNew();
				RunThroughAwbRemovalTest(hawb);

				var basic = Factory.New<CusMAWB>();
				RunThroughAwbRemovalTest(basic);
			}

			static void RunThroughAwbRemovalTest(ICcsukCusAwb awb)
			{
				awb.AirportOfOrigin = "USJFK";
				awb.AirportOfDestination = "ORD";
				awb.AirportOfArrival = "LHR";
				AssertEquals("Pre req - IsThroughAwb", true, awb.IsThroughAwb);

				var tsr = awb.TSRs.AddNew();
				tsr.AirportOrCountryOfDestination = "USLAX";
				AssertHasErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs");

				tsr.AirportOrCountryOfDestination = "LAX";
				tsr.Validation.ValidateC4_ParentID();
				AssertHasErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs");

				tsr.AirportOrCountryOfDestination = "ORD";
				tsr.Validation.ValidateC4_ParentID();
				AssertNoErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs");

				tsr.AirportOrCountryOfDestination = "USORD";
				tsr.Validation.ValidateC4_ParentID();
				AssertNoErrorContaining(tsr.C4_RL_NKDischargePortInfo, "Through AWBs");
			}
			protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<TranshipmentRemoval>();
		}

		delegate CusUnderbond MakeNewUnderbond(ICcsukCusAwb a);

		[TestedType(typeof(InterAirportRemovalValidation))]
		class InterAirportRemovalValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
		{
			public override void TestC4_OutturnedValidation()
			{
				Underbond.C4_Outurned = ZDateTime.Empty;
				AssertNoNotifications(Underbond.C4_OuturnedInfo);
			}

			public void TestValidationType()
			{
				var ub = Factory.New<InterAirportRemoval>();
				AssertType(typeof(InterAirportRemovalValidation), ub.Validation);
			}

			public void TestValidateLRI()
			{
				var cusMawb = Factory.New<CusMAWB>();
				var ub = cusMawb.IARs.AddNew();
				ub.Validation.ValidateAll();
				AssertHasErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = YesNoList.Codes.Yes;
				AssertNoErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = YesNoList.Codes.No;
				AssertNoErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
				ub.LicenseRestrictionInd = "X";
				AssertHasErrorContaining(ub.LicenseRestrictionIndInfo, "Licence");
			}

			public void TestOnwardAwbFormatting()
			{
				var basic = Factory.New<CusMAWB>();
				var ub = basic.IARs.AddNew();
				TranshipmentRemovalValidationTest.RunnerForTestOnwardAwbFormattingValidation(ub.OnwardCarrierInfo, ub.C4_MAWBInfo);
			}

			public void TestOnwardAwbAndCarrierValidation()
			{
				var cusMawb = Factory.New<CusMAWB>();
				var ub = cusMawb.IARs.AddNew();
				var carrierInfo = ub.OnwardCarrierInfo;
				var awbInfo = ub.OnwardAirWaybillNumberInfo;
				TranshipmentRemovalValidationTest.RunnerForTestOnwardAwbCarrierValidation(carrierInfo, awbInfo);
			}

			public void TestCheckC4_DischargePremiseID()
			{
				var cusMawb = Factory.New<CusMAWB>();
				var iar = cusMawb.IARs.AddNew();
				iar.C4_RL_NKDischargePort = "LHR";
				iar.NewShedId = "";
				AssertHasMessageErrorContaining(iar.NewShedIdInfo, "Heathrow");
				iar.C4_RL_NKDischargePort = "LGW";
				AssertNoMessageErrorContaining(iar.NewShedIdInfo, "Heathrow");
				iar.C4_RL_NKDischargePort = "LHR";
				iar.NewShedId = "BAC";
				AssertNoMessageErrorContaining(iar.NewShedIdInfo, "Heathrow");
			}

			public void TestCheckShedAndAirportCombination()
			{
				ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
				Factory.Save();

				var mawb = Factory.New<CusMAWB>();
				var iar = mawb.IARs.AddNew();
				iar.NewShedId = "";
				iar.AirportOfDestination = "";
				AssertNoMessageErrorContaining(iar.NewShedIdInfo, "This shed-airport combination is not valid.");
				AssertNoMessageErrorContaining(iar.AirportOfDestinationInfo, "This shed-airport combination is not valid.");
				iar.NewShedId = "RHS"; //COA combo
				iar.AirportOfDestination = "DSA";
				AssertHasMessageErrorContaining(iar.NewShedIdInfo, "This shed-airport combination is not valid.");
				AssertHasMessageErrorContaining(iar.AirportOfDestinationInfo, "This shed-airport combination is not valid.");
				iar.NewShedId = "BAC";
				AssertHasMessageErrorContaining(iar.NewShedIdInfo, "This shed-airport combination is not valid.");
				AssertHasMessageErrorContaining(iar.AirportOfDestinationInfo, "This shed-airport combination is not valid.");
				iar.AirportOfDestination = "LHR";
				AssertNoMessageErrorContaining(iar.NewShedIdInfo, "This shed-airport combination is not valid.");
				AssertNoMessageErrorContaining(iar.AirportOfDestinationInfo, "This shed-airport combination is not valid.");
			}
			protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<InterAirportRemoval>();
		}
	}

	class CusUnderbondTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var bizObj = Factory.New<Fallback>();
			var row = ((INeedRow)bizObj).Row;
			var typeDecider = new CusUnderbondTypeDecider();
			foreach ((string code, Type type) testData in new[]
			{
					(CusUnderbondApplicationCodeList.Codes.GBFallback, typeof(Fallback)),
					(CusUnderbondApplicationCodeList.Codes.GBInterAirportRemoval, typeof(InterAirportRemoval)),
					(CusUnderbondApplicationCodeList.Codes.GBInterShedRemoval, typeof(InterShedRemoval)),
					(CusUnderbondApplicationCodeList.Codes.GBTranshipmentRemoval, typeof(TranshipmentRemoval))
				})
			{
				bizObj.C4_ApplicationCode = testData.code;
				AssertEquals(testData.code, testData.type, typeDecider.GetTypeForLoad(row, Factory));
			}

			var fallback = Factory.New<Fallback>();
			var interAirportRemoval = Factory.New<InterAirportRemoval>();
			var interShedRemoval = Factory.New<InterShedRemoval>();
			var transhipmentRemoval = Factory.New<TranshipmentRemoval>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertEquals("Fallback", typeof(Fallback), newFactory.Load<CusUnderbond>(fallback.PK).GetType());
			AssertEquals("InterAirportRemoval", typeof(InterAirportRemoval), newFactory.Load<InterAirportRemoval>(interAirportRemoval.PK).GetType());
			AssertEquals("InterShedRemoval", typeof(InterShedRemoval), newFactory.Load<InterShedRemoval>(interShedRemoval.PK).GetType());
			AssertEquals("TranshipmentRemoval", typeof(TranshipmentRemoval), newFactory.Load<CusUnderbond>(transhipmentRemoval.PK).GetType());
		}
	}
}
