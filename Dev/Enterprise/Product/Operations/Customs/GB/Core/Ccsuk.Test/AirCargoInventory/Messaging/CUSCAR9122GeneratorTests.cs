using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class CUSCAR9122GeneratorTests : TestCaseWithFactory
	{
		public void TestNprIsCalculatedJustBeforeGeneration()
		{
			// Otherwise, if the user makes a changes to the OutTurns grid and presses send withotu first saving, the generator will not see the new NPR until saving after generating.
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = CreateMawbForTest(false, Factory);
			basic.Profile = "CUKAIR98LHRBAC";
			basic.NumberOfPiecesReceived = 101;
			basic.OutTurns.AddNew().C5_PackagesOutturned = 99;
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertContains("QTY+48:99'", result);
			AssertNotContains("QTY+48:101'", result);
			AssertEquals(99, (int)basic.NumberOfPiecesReceived);
			basic.OutTurns[0].C5_PackagesOutturned = 0;
			generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertNotContains("An FRI should NOT send NPR=0", "QTY+48:0'", result);
		}

		public void TestFRIWithoutNominatedAgent()
		{
			var mawb = CreateMawbForTest(true, Factory);
			mawb.AgentBadge = "";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertNotContains("NAD", result);
			mawb.AgentBadge = "ABC";
			generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("NAD", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_Arrived()
		{
			var mawb = CreateMawbForTest(true, Factory);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedMaster.Replace(System.Environment.NewLine, ""), result);
		}

		public void TestFRIFRCStatus2()
		{
			var mawb = CreateMawbForTest(true, Factory);
			mawb.Status2Granted = true;
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertContains("GIS+S2Y'", result);

			mawb.Status2Granted = false;
			generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2N'", result);

			mawb.Status2Granted = true;
			generator = new CUSCARGeneratorFRC(mawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2Y'", result);

			mawb.Status2Granted = false;
			generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertContains("GIS+S2N'", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_InterestingPorts()
		{
			var mawb = CreateMawbForTest(true, Factory);
			mawb.AirportOfArrival = "STN";
			mawb.AirportOfDestination = "MAN";
			mawb.AirportOfOrigin = "FRANT";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertContains("LOC+11:STN:145:3::KLM:129:ZZZ+84:XAT:145:3+85:MAN:145:3'", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_PrearrivalMaster()
		{
			var mawb = CreateMawbForTest(true, Factory);
			mawb.AirportOfArrival = "MAN";
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friPrearrivalMaster.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_PrearrivalBasic()
		{
			var basic = CreateMawbForTest(false, Factory);
			basic.AirportOfArrival = "MAN";
			basic.NumberOfPiecesReceived = 0;
			basic.NumberOfPiecesExpected = 0;
			basic.CM_ArrivalDate = ZDateTime.Empty;
			var ec = new ErrorCollector();
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(basic, ec);
			string result = generator.MakeMessageText();
			AssertContains("At least one of NPX and NPR must be present", ec.GetErrorsAsString());
			basic.NumberOfPiecesExpected = 15;
			generator = new CUSCARGeneratorFRI(basic, ec);
			result = generator.MakeMessageText();
			AssertEquals(friPrearrivalBasic.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_MasterAsShed()
		{
			var mawb = CreateMawbForTest(true, Factory);
			mawb.AgentBadge = "DAN";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(mawb, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedMasterForAgentDAN.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_BasicAsShed()
		{
			var basic = CreateMawbForTest(false, Factory);
			basic.AgentBadge = "DAN";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedBasicForAgentDAN.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_BasicWithCommunityHandlingCodes()
		{
			var basic = CreateMawbForTest(false, Factory);
			basic.AgentBadge = "DAN";
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "CoL";
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "RcM";
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "X¬Y";
			var chcForSplitShouldNotGoInMessage = basic.CommunityHandlingCodes.AddNew().Data;
			chcForSplitShouldNotGoInMessage.C4_CommunityHandlingCode = "XXX";
			chcForSplitShouldNotGoInMessage.C4_SplitReferenceToWhichThisPertains = "99";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedBasicForAgentDAN.Replace(System.Environment.NewLine, ""), result);  // No CHC yet
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertEquals(friExampleArrivedBasicWithCommunityHandlingCodes.Replace(System.Environment.NewLine, ""), result);
			AssertContains("<tr><td>Community Handling Code</td><td>COL - Cool Goods</td>", generator.MessageInterpretation);
			AssertContains("<tr><td>Community Handling Code</td><td>RCM - Corrosive</td>", generator.MessageInterpretation);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRI_BasicWithCommunityHandlingCodesNoGIS()
		{
			var basic = CreateMawbForTest(false, Factory);
			basic.AgentBadge = "DAN";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			string result = generator.MakeMessageText();
			AssertNotContains("Should not contain an empty GIS CHC segment when not using v3 messages", "GIS+:131'", result);
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			generator = new CUSCARGeneratorFRI(basic, new ErrorCollector());
			result = generator.MakeMessageText();
			AssertNotContains("Should have NO GIS CHC segment", "GIS+:131'", result);
			AssertNotContains("Should have NO GIS CHC segment", ":131'", result);
			AssertNotNull("Should not explode when accessing this property", generator.MessageInterpretation);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRIBadgeHasNoCredentials()
		{
			var ec = new ErrorCollector();
			var mawb = CreateMawbForTest(true, Factory);
			CUSCARGeneratorFRI generator = new CUSCARGeneratorFRI(mawb, ec);
			mawb.AgentBadge = "DAN";
			mawb.Profile = "SENDERPIMA";
			string result = generator.MakeMessageText();
			AssertEquals(0, ec.ErrorCount);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRXBasic()
		{
			var ec = new ErrorCollector();
			var basic = CreateMawbForTest(false, Factory);
			basic.AirportOfArrival = "STN"; // LSA not valid, must enter STN	
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRX(basic, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frxBasic.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRXBasicEvenWhenV3IsEnabled()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestFRXBasic();
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRXConsol()
		{
			var ec = new ErrorCollector();
			var mawb = CreateMawbForTest(true, Factory);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRX(mawb, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frxConsol.Replace(System.Environment.NewLine, ""), result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRC()
		{
			var ec = new ErrorCollector();
			var mawb = CreateMawbForTest(true, Factory);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRC(mawb, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frcMaster.Replace(System.Environment.NewLine, ""), result);
			mawb.NumberOfPiecesReceived = 0;
			generator = new CUSCARGeneratorFRC(mawb, ec);
			result = generator.MakeMessageText();
			AssertContains("An FRC should send NPR=0, otherwise CCSUK will not know about the new value", "QTY+48:0'", result);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFRCStatus1()
		{
			var mawb = CreateMawbForTest(true, Factory);
			RunFrcStatus1Test(mawb);
		}

		internal static void RunFrcStatus1Test(ICcsukCusAwb awb)
		{
			var ec = new ErrorCollector();
			awb.NumberOfPiecesReceived = awb.NumberOfPiecesExpected;
			awb.Status1Date = new ZDateTime(1986, 3, 12, 4, 27, 0);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFRC(awb, ec);
			string result = generator.MakeMessageText();
			AssertContains("50:8603120427:201", result);

			GBCustomsDataRegistry.Instance.CcsukAllowStatus1DateInFrc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			generator = new CUSCARGeneratorFRC(awb, ec);
			result = generator.MakeMessageText();
			AssertNotContains("50:8603120427:201", result);

			awb.NumberOfPiecesReceived = 0;
			GBCustomsDataRegistry.Instance.CcsukAllowStatus1DateInFrc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			generator = new CUSCARGeneratorFRC(awb, ec);
			result = generator.MakeMessageText();
			awb.Status1Date = ZDateTime.Empty;
			AssertNotContains("50:8603120427:201", result);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestFRCSplitBasic()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var ec = new ErrorCollector();
			var basic = CreateMawbForTest(false, Factory);
			basic.Profile = "CUKAIR98LHRBAC";
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "69";
			splitBasic.Weight = 70m;
			splitBasic.NumberOfPiecesExpected = (ZShort)71;
			var ot = basic.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = "69";
			ot.C5_PackagesOutturned = 72;
			splitBasic.NumberOfPiecesReceived = (ZShort)72;
			var generator = new CUSCARGeneratorFRC(splitBasic, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frcSplitBasic.Replace(System.Environment.NewLine, "").Replace("KLM", "BAC"), result);
			splitBasic.AgentBadge = "DJC";
			generator = new CUSCARGeneratorFRC(splitBasic, ec);
			result = generator.MakeMessageText();
			AssertEquals(frcSplitBasic.Replace(System.Environment.NewLine, "").Replace("LXA", "DJC").Replace("KLM", "BAC"), result);
			AssertContains(">Shipment Description Code<", generator.MessageInterpretation);
		}

		[TestDate(1986, 03, 12, 04, 27, 0)]
		public void TestFRCSplitBasicShowsStatus1FromSplitNotParent()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var ec = new ErrorCollector();
			var basic = CreateMawbForTest(false, Factory);
			basic.Profile = "CUKAIR98LHRBAC";
			basic.NumberOfPiecesExpected = 100;
			var split01 = basic.Splits.AddNew();
			split01.SplitReference = "01";
			split01.NumberOfPiecesExpected = (ZShort)60;
			split01.Weight = 70m;
			var split02 = basic.Splits.AddNew();
			split02.SplitReference = "02";
			split02.NumberOfPiecesExpected = (ZShort)40;
			var ot1 = basic.OutTurns.AddNew();
			ot1.SplitReferenceToWhichThisPertains = "01";
			ot1.C5_PackagesOutturned = 60;
			Factory.Save();
			var generator = new CUSCARGeneratorFRC(split01, ec);
			string result = generator.MakeMessageText();
			AssertEquals(frcSplitBasicWithStatus1.Replace(System.Environment.NewLine, "").Replace("KLM", "BAC"), result);
		}

		public void TestFCSBasic()
		{
			var splits = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory);
			var ec = new ErrorCollector();
			var basic = CreateMawbForTest(false, Factory);
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "ABC";
			CUSCARGeneratorBase generator = new CUSCARGeneratorFCS(basic, ec, splits.SplitLines);
			string result = generator.MakeMessageText();
			AssertEquals(fcsBasic.Replace(System.Environment.NewLine, ""), result);
			var interpretation = generator.MessageInterpretation;
			AssertContains(@"<h3>Split Manipulation 801-12345678</h3>
							<p>
								Operation: FCS
							</p> 
							<table", interpretation);
			AssertContains(@"<th>Split Reference</th><th>Pieces</th><th>Weight</th></tr></thead><tr><td>01</td><td>2</td><td>33KG</td></tr><tr><td>02</td><td>2</td><td>12.67KG</td></tr><tr><td>03</td><td>1</td><td>10KG</td></tr></table>",
				interpretation);
		}

		public void TestFCSBasicEvenWhenV3IsEnabled()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestFCSBasic();
		}

		public void TestFCSHouse()
		{
			var splits = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory);
			var ec = new ErrorCollector();
			var basic = CreateMawbForTest(true, Factory);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFCS(basic.ChildBills[0], ec, splits.SplitLines);
			string result = generator.MakeMessageText();
			AssertEquals(fcsHouse.Replace(System.Environment.NewLine, ""), result);
			var interpretation = generator.MessageInterpretation;
			AssertContains(@"<h3>Split Manipulation 801-12345678-87654321</h3>
							<p>
								Operation: FCS
							</p> 
							<table", interpretation);
			AssertContains(@"<th>Split Reference</th><th>Pieces</th><th>Weight</th></tr></thead><tr><td>01</td><td>2</td><td>33KG</td></tr><tr><td>02</td><td>2</td><td>12.67KG</td></tr><tr><td>03</td><td>1</td><td>10KG</td></tr></table>",
				interpretation);
		}

		public void TestFCSRemoveAllSplits()
		{
			var npSplitsAndFlightData = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForRemoveAllTest(Factory);
			var ec = new ErrorCollector();
			var mawb = CreateMawbForTest(false, Factory);
			CUSCARGeneratorBase generator = new CUSCARGeneratorFCS(mawb, ec, npSplitsAndFlightData.SplitLines);
			string result = generator.MakeMessageText();
			AssertEquals(fcsExampleRemoveAllSplits.Replace(System.Environment.NewLine, ""), result);
		}

		public static CusMAWB CreateMawbForTest(bool createHawbToo, BusinessObjectFactory factory)
		{
			var mawb = factory.New<CusMAWB>();
			mawb.CM_MAWB = "80112345678";
			mawb.CargoTerminalOperator = "KLM";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.AirportOfArrival = "MAN";
			mawb.AirportOfOrigin = "USLAX";
			mawb.CM_ArrivalDate = ZDateTime.Now; // Agi
			mawb.AirportOfDestination = "LHR";
			mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			mawb.NumberOfPiecesExpected = 15;
			mawb.WeightCode = "KG";
			mawb.Weight = 3000m;
			mawb.DescriptionOfGoods = "COLUMBIAN FLOUR";
			mawb.CM_FlightNo = "BA112";
			mawb.NumberOfPiecesReceived = 68;
			mawb.AgentBadge = "LXA";
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			mawb.Profile = "CUKFFW98000LXA";
			if (createHawbToo)
			{
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_HAWB = "87654321";
				hawb.AirportOfOrigin = "USLAX";
			}
			return mawb;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential();
		}

		internal const string friExampleArrivedMasterForAgentDAN = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:M'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+DAN'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		internal const string friExampleArrivedBasicForAgentDAN = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+DAN'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		// Note, version 3 message
		internal const string friExampleArrivedBasicWithCommunityHandlingCodes = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:3:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678'
GIS+S2Y'
GIS+T:121'
GIS+COL:131'
GIS+RCM:131'
GIS+XY:131'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+DAN'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+16+<<MSGNO PLACEHOLDER>>'";

		readonly string friPrearrivalMaster = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:M'
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

		readonly string friPrearrivalBasic = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678'
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

		readonly string frcMaster = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++HWB:M'
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

		readonly string frcSplitBasic = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++ACD::69'
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

		readonly string frcSplitBasicWithStatus1 = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109503+<<SYSCAR>>'
BGM+:::FRC+80112345678+++ACD::01+50:8603120427:201'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:860312:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:60'
QTY+48:60'
MEA+WT++KGM:70'
UNT+13+<<MSGNO PLACEHOLDER>>'";

		readonly string frxBasic = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109504+<<SYSCAR>>'
BGM+:::FRX+80112345678'
TDT+20'
LOC+11:STN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
UNT+5+<<MSGNO PLACEHOLDER>>'";

		readonly string frxConsol = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109504+<<SYSCAR>>'
BGM+:::FRX+80112345678+++HWB:M'
TDT+20'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
UNT+5+<<MSGNO PLACEHOLDER>>'";

		readonly string fcsBasic = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109505+<<SYSCAR>>'
BGM+:::FCS+80112345678'
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

		readonly string fcsExampleRemoveAllSplits = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109505+<<SYSCAR>>'
BGM+:::FCS+80112345678'
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

		internal const string friExampleArrivedMaster = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:M'
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
	}
}
