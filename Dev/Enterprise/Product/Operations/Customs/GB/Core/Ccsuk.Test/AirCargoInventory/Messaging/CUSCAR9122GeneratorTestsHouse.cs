using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class CUSCAR9122GeneratorTestForHawb : TestCaseWithFactory
	{
		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_HouseWithCommunityHandlingCodes()
		{
			var hawb = CreateMawbAndHawbForTest();
			hawb.AirportOfArrival = "AOA";
			hawb.AirportOfDestination = "AOD";
			hawb.AirportOfOrigin = "USAOO";
			hawb.ShipmentDescriptionCode = "X";
			hawb.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "COL";
			hawb.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "RCM";
			var chcForSplitShouldNotGoInMessage = hawb.CommunityHandlingCodes.AddNew().Data;
			chcForSplitShouldNotGoInMessage.C4_CommunityHandlingCode = "XXX";
			chcForSplitShouldNotGoInMessage.C4_SplitReferenceToWhichThisPertains = "99";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedHouse.Replace(System.Environment.NewLine, ""), result);  // No CHC yet
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedHouseWithCommunityHandlingCodes.Replace(System.Environment.NewLine, ""), result);
			AssertContains("<tr><td>Community Handling Code</td><td>COL - Cool Goods</td>", generator.MessageInterpretation);
			AssertContains("<tr><td>Community Handling Code</td><td>RCM - Corrosive</td>", generator.MessageInterpretation);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_Arrived()
		{
			var hawb = CreateMawbAndHawbForTest();
			hawb.AirportOfArrival = "AOA";
			hawb.AirportOfDestination = "AOD";
			hawb.AirportOfOrigin = "USAOO";
			hawb.ShipmentDescriptionCode = "X";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedHouse.Replace(System.Environment.NewLine, ""), result);
			var interpretation = generator.MessageInterpretation;
			AssertContains(@"<h3>Insert Freight Record 801-12345678-87654321</h3>
							<p>
								Operation: FRI", interpretation);
			AssertContains(@"<th>Field</th><th>Value</th></tr></thead><tr><td>HAWB</td><td>87654321</td></tr><tr><td>Responsible Party ID</td><td>LXA",
				interpretation);
			AssertContains(@"<td>Shipment Description Code</td><td>X</td>",
				interpretation);
		}

		public void TestFRIFRCStatus2()
		{
			var hawb = CreateMawbAndHawbForTest();
			hawb.Status2Granted = true;
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertContains("GIS+S2Y'", result);

			hawb.Status2Granted = false;
			generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2N'", result);

			hawb.Status2Granted = true;
			generator = new CUSCARGeneratorFRC(hawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2Y'", result);

			hawb.Status2Granted = false;
			generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2N'", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_InterestingPorts()
		{
			var hawb = CreateMawbAndHawbForTest();
			hawb.AirportOfArrival = "STN";
			hawb.AirportOfDestination = "MAN";
			hawb.AirportOfOrigin = "FRANT";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertContains("LOC+11:STN:145:3::KLM:129:ZZZ+84:XAT:145:3+85:MAN:145:3'", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_PrearrivaHouse()
		{
			var hawb = CreateMawbAndHawbForTest();
			hawb.CS_PiecesLanded = 0;
			hawb.MAWB.CM_ArrivalDate = ZDateTime.Empty;
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(hawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friPrearrivalHouse.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRX()
		{
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRX(hawb, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frxHouse.Replace(System.Environment.NewLine, ""), result);
			AssertContains(@"<h3>Delete Freight Record 801-12345678-87654321</h3>
							<p>
								Operation: FRX
							</p> 
							<table", generator.MessageInterpretation);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRC()
		{
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRC(hawb, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frcHouse.Replace(System.Environment.NewLine, ""), result);
			AssertContains(@"<h3>Edit Freight Record 801-12345678-87654321</h3>
							<p>
								Operation: FRC
							</p> 
							<table", generator.MessageInterpretation);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFRCStatus1()
		{
			var hawb = CreateMawbAndHawbForTest();
			CUSCAR9122GeneratorTests.RunFrcStatus1Test(hawb);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRCSplitHouse()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			hawb.Profile = "CUKAIR98LHRBAC";
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.SplitReference = "69";
			split.Weight = 70;
			split.NumberOfPiecesExpected = (ZShort)71;
			var ot = hawb.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = "69";
			ot.C5_PackagesOutturned = 72;
			var generator = new CUSCARGeneratorFRC(split, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frcSplitHouse.Replace(System.Environment.NewLine, "").Replace("KLM", "BAC"), result);
			AssertContains("<h3>Edit Freight Record 801-12345678-87654321/69</h3>", generator.MessageInterpretation);
			split.AgentBadge = "DJC";
			generator = new CUSCARGeneratorFRC(split, ec);
			result = generator.MakeMessageText();
			AssertEquals(frcSplitHouse.Replace(System.Environment.NewLine, "").Replace("LXA", "DJC").Replace("KLM", "BAC"), result);
			AssertContains(">Shipment Description Code<", generator.MessageInterpretation);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRCSplitHouseWithCHC()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			hawb.Profile = "CUKAIR98LHRBAC";
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.SplitReference = "69";
			split.Weight = 70;
			split.NumberOfPiecesExpected = (ZShort)71;
			var ot = hawb.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = "69";
			ot.C5_PackagesOutturned = 72;
			var chc1 = hawb.CommunityHandlingCodes.AddNew().Data;
			var chc2 = hawb.CommunityHandlingCodes.AddNew().Data;
			var chc3 = hawb.CommunityHandlingCodes.AddNew().Data;
			chc1.C4_SplitReferenceToWhichThisPertains = "01";
			chc2.C4_SplitReferenceToWhichThisPertains = "69";
			chc3.C4_SplitReferenceToWhichThisPertains = "69";
			chc1.C4_CommunityHandlingCode = "AAA";
			chc2.C4_CommunityHandlingCode = "BBB";
			chc3.C4_CommunityHandlingCode = "CCC";
			var generator = new CUSCARGeneratorFRC(split, ec);
			var result = generator.MakeMessageText();
			AssertEquals(frcSplitHouseWithCHC.Replace(System.Environment.NewLine, "").Replace("KLM", "BAC"), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFCS()
		{
			var splits = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory);
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			hawb.ShipmentDescriptionCode = "T";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFCS(hawb, ec, splits.SplitLines);
			string result = generator.MakeMessageText();
			AssertEquals(fcsHouse.Replace(System.Environment.NewLine, ""), result);
			AssertContains(@"<th>Split Reference</th><th>Pieces</th><th>Weight</th></tr></thead><tr><td>01</td><td>2</td><td>33KG</td></tr><tr><td>02</td><td>2</td><td>12.67KG</td></tr><tr><td>03</td><td>1</td><td>10KG</td></tr></table>",
				generator.MessageInterpretation);
		}

		public void TestFCSToRemoveAllSplits()
		{
			var npSplitsAndFlightData = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForRemoveAllTest(Factory);
			var ec = new ErrorCollector();
			var hawb = CreateMawbAndHawbForTest();
			CUSCARGeneratorBase generator = new CUSCARGeneratorFCS(hawb, ec, npSplitsAndFlightData.SplitLines);
			var result = generator.MakeMessageText();
			AssertEquals(fcsHouseRemovalAllSplits.Replace(System.Environment.NewLine, ""), result);
			AssertNotContains("weight details", ec.GetErrorsAsString());
		}

		CusHAWB CreateMawbAndHawbForTest()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			hawb.CS_GoodsDescription = "COLUMBIAN FLOUR";
			hawb.CS_PiecesManifested = 15;
			hawb.CS_PiecesLanded = 68;
			hawb.CS_Weight = 3000m;
			return hawb;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential();
		}

		readonly string friExampleArrivedHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+X:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:AOA:145:3::KLM:129:ZZZ+84:AOO:145:3+85:AOD:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		readonly string friPrearrivalHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
MEA+WT++KGM:3000'
UNT+12+<<MSGNO PLACEHOLDER>>'";

		readonly string frcSplitHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++HWB:87654321:69'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:71'
QTY+48:72'
MEA+WT++KGM:70'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		readonly string frcSplitHouseWithCHC = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:3:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++HWB:87654321:69'
GIS+S2Y'
GIS+T:121'
GIS+BBB:131'
GIS+CCC:131'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:71'
QTY+48:72'
MEA+WT++KGM:70'
UNT+15+<<MSGNO PLACEHOLDER>>'";

		readonly string frcHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		readonly string frxHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109504+<<SYSCAR>>'
BGM+:::FRX+80112345678+++HWB:87654321'
TDT+20'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
UNT+5+<<MSGNO PLACEHOLDER>>'";

		readonly string fcsHouse = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109505+<<SYSCAR>>'
BGM+:::FCS+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+P:121'
TDT+20'
LOC+11:MAN:145:3::KLM:129:ZZZ'
GID+01'
QTY+118:2'
MEA+WT++KGM:33'
GID+02'
QTY+118:2'
MEA+WT++KGM:12.67'
GID+03'
QTY+118:1'
MEA+WT++KGM:10'
UNT+16+<<MSGNO PLACEHOLDER>>'";

		readonly string fcsHouseRemovalAllSplits = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109505+<<SYSCAR>>'
BGM+:::FCS+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+P:121'
TDT+20'
LOC+11:MAN:145:3::KLM:129:ZZZ'
GID+01'
QTY+118:5'
MEA+WT++KGM:12389'
GID+02'
QTY+118:0'
MEA+WT++KGM:0'
GID+03'
QTY+118:0'
MEA+WT++KGM:0'
UNT+16+<<MSGNO PLACEHOLDER>>'";

		// Note, version 3 message
		const string friExampleArrivedHouseWithCommunityHandlingCodes = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:3:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+X:121'
GIS+COL:131'
GIS+RCM:131'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:AOA:145:3::KLM:129:ZZZ+84:AOO:145:3+85:AOD:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+15+<<MSGNO PLACEHOLDER>>'";
	}
}
