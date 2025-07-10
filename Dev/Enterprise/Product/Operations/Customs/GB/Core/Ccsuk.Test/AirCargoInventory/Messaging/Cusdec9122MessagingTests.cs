using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class Cusdec9122MessagingTests : TestCaseWithFactory
	{
		// NB - I took the examples from section 20.4 of the specs doc, then tweaked them. I fixed the edifact errors in the samples (really!), changed them from house to master examples, and added the ":145:3" qualifier for the destination airport (LOC/85) for the TSR (for us we treat these qualifiers as mandatory). I changed the value from 100 to 100000 for the sea TSR example, and fixed "GBP" (was "GB" in several examples)

		public void TestBlockingOnUnderbond_FBK()
		{
			Mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
			var underbond = GetNewFBK();

			errorCollector = new ErrorCollector();
			var generator = new CusdecGeneratorFBK(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = new ZDateTime(2024, 12, 1);
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorFBK(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = ZDateTime.Empty;
			underbond.SplitReferenceToWhichThisRemovalPertains = "11";
			Mawb.Splits.AddNew();
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorFBK(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());
		}

		public void TestBlockingOnUnderbond_IAR()
		{
			Mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
			var underbond = GetNewIAR();

			errorCollector = new ErrorCollector();
			var generator = new CusdecGeneratorIAR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = new ZDateTime(2024, 12, 1);
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorIAR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = ZDateTime.Empty;
			underbond.SplitReferenceToWhichThisRemovalPertains = "11";
			Mawb.Splits.AddNew();
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorIAR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());
		}

		public void TestBlockingOnUnderbond_ISR()
		{
			Mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
			var underbond = GetNewISR();

			errorCollector = new ErrorCollector();
			var generator = new CusdecGeneratorISR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = new ZDateTime(2024, 12, 1);
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorISR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = ZDateTime.Empty;
			underbond.SplitReferenceToWhichThisRemovalPertains = "11";
			Mawb.Splits.AddNew();
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorISR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());
		}

		public void TestBlockingOnUnderbond_TSR()
		{
			Mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights;
			var underbond = GetNewTSR();

			errorCollector = new ErrorCollector();
			var generator = new CusdecGeneratorTSR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = new ZDateTime(2024, 12, 1);
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorTSR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			Mawb.Status1Date = ZDateTime.Empty;
			underbond.SplitReferenceToWhichThisRemovalPertains = "11";
			Mawb.Splits.AddNew();
			errorCollector = new ErrorCollector();
			generator = new CusdecGeneratorTSR(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());
		}

		public void TestProfileErrorCollector_FBK()
		{
			var underbond = GetNewFBK();
			TestProfileErrorCollector_Runner(underbond, delegate
			{ return new CusdecGeneratorFBK(Mawb, errorCollector, underbond); }, true);
		}

		public void TestProfileErrorCollector_IAR()
		{
			var underbond = GetNewIAR();
			TestProfileErrorCollector_Runner(underbond, delegate
			{ return new CusdecGeneratorIAR(Mawb, errorCollector, underbond); }, true);
		}

		public void TestProfileErrorCollector_TSR()
		{
			var underbond = GetNewTSR();
			TestProfileErrorCollector_Runner(underbond, delegate
			{ return new CusdecGeneratorTSR(Mawb, errorCollector, underbond); }, true);
		}

		public void TestProfileErrorCollector_ISR()
		{
			var underbond = GetNewISR();
			TestProfileErrorCollector_Runner(underbond, delegate
			{ return new CusdecGeneratorISR(Mawb, errorCollector, underbond); }, false);
		}

		public void TestNotAllowedForSDC()
		{
			errorCollector = new ErrorCollector();
			var iar = GetNewIAR();
			Mawb.ShipmentDescriptionCode = "M";
			var generator = new CusdecGeneratorIAR(Mawb, errorCollector, iar);
			generator.MakeMessageText();
			AssertContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			errorCollector = new ErrorCollector();
			Mawb.ShipmentDescriptionCode = "T";
			generator = new CusdecGeneratorIAR(Mawb, errorCollector, iar);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());
			errorCollector = new ErrorCollector();
			Mawb.ShipmentDescriptionCode = "E";
			generator = new CusdecGeneratorIAR(Mawb, errorCollector, iar);
			generator.MakeMessageText();
			AssertNotContains("European SDC", errorCollector.GetErrorsAsString());

			errorCollector = new ErrorCollector();
			Mawb.ShipmentDescriptionCode = "M";
			Mawb.NumberOfPiecesExpected = 101;
			Mawb.NumberOfPiecesReceived = 101;
			Mawb.Status1Date = ZDateTime.BrettsBirthday;
			generator = new CusdecGeneratorIAR(Mawb, errorCollector, iar);
			generator.MakeMessageText();
			AssertNotContains("The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", errorCollector.GetErrorsAsString());

			errorCollector = new ErrorCollector();
			Mawb.ShipmentDescriptionCode = "E";
			var fbkGenerator = new CusdecGeneratorFBK(Mawb, errorCollector, GetNewFBK());
			fbkGenerator.MakeMessageText();
			AssertContains("European SDC", errorCollector.GetErrorsAsString());
		}

		void TestProfileErrorCollector_Runner(CusUnderbond underbond, GeneratorMaker generatorMaker, bool shedExpectedDeniedPermission)
		{
			errorCollector = new ErrorCollector();
			var generator = generatorMaker(Mawb, errorCollector, underbond);
			Mawb.Profile = "CUKAIR98LHRYYY"; // shed
			generator.MakeMessageText();
			AssertEquals(shedExpectedDeniedPermission, errorCollector.GetErrorsAsString().Contains("This profile/PIMA is not allowed to make"));

			Mawb.Profile = "CUKFFW98000XXX"; // agent
			errorCollector = new ErrorCollector();
			generator = generatorMaker(Mawb, errorCollector, underbond);
			generator.MakeMessageText();
			AssertEquals(!shedExpectedDeniedPermission, errorCollector.GetErrorsAsString().Contains("This profile/PIMA is not allowed to make"));
		}

		public void TestBasicVersusHouseVersusSplitHouseVersusSplitBasic()
		{
			var iar = GetNewIAR();
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000XXX";
			basic.CM_MAWB = "12312345678";
			var consol = Factory.New<CusMAWB>();
			consol.Profile = "CUKFFW98000XXX";
			consol.CM_MAWB = "12312345678";
			var hawb = consol.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";

			var generator = new CusdecGeneratorIAR(hawb, new ErrorCollector(), iar);
			var result = generator.MakeMessageText();
			AssertContains("BGM+IAR+12312345678+++HWB:87654321'", result);

			generator = new CusdecGeneratorIAR(basic, new ErrorCollector(), iar);
			result = generator.MakeMessageText();
			AssertContains("BGM+IAR+12312345678'", result);

			iar.SplitReferenceToWhichThisRemovalPertains = "69";
			generator = new CusdecGeneratorIAR(basic, new ErrorCollector(), iar);
			result = generator.MakeMessageText();
			AssertContains("BGM+IAR+12312345678+++ACD::69'", result);

			generator = new CusdecGeneratorIAR(hawb, new ErrorCollector(), iar);
			result = generator.MakeMessageText();
			AssertContains("BGM+IAR+12312345678+++HWB:87654321:69'", result);
		}

		public void TestIAR()
		{
			var iar = GetNewIAR();
			var generator = new CusdecGeneratorIAR(Mawb, new ErrorCollector(), iar);
			var result = generator.MakeMessageText();
			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109603+<<SYSCAR>>'
BGM+IAR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:MAN:145:3::NSI:129:ZZZ'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expected.Replace(System.Environment.NewLine, ""), result);

			iar.OnwardAirWaybillNumber = "";
			iar.OnwardCarrier = "";
			generator = new CusdecGeneratorIAR(Mawb, new ErrorCollector(), iar);
			result = generator.MakeMessageText();
			var expected2 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109603+<<SYSCAR>>'
BGM+IAR+11177777777+++HWB:M'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:MAN:145:3::NSI:129:ZZZ'
TDT+12++40'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+10+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expected2.Replace(System.Environment.NewLine, ""), result);
		}

		public void TestIARNoShed()
		{
			var iar = GetNewIAR();
			iar.NewShedId = "";
			var generator = new CusdecGeneratorIAR(Mawb, new ErrorCollector(), iar);
			var result = generator.MakeMessageText();
			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109603+<<SYSCAR>>'
BGM+IAR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:MAN:145:3'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expected.Replace(System.Environment.NewLine, ""), result);
		}

		public void TestIARSplit()
		{
			var hawb = Mawb.ChildBills[0];
			var split = hawb.Splits.AddNew();
			var iar = hawb.IARs.AddNew();
			split.SplitReference = "69";
			iar.SplitReferenceToWhichThisRemovalPertains = "69";
			var generator = new CusdecGeneratorIAR(hawb, new ErrorCollector(), iar);
			var result = generator.MakeMessageText();
			AssertContains(@"BGM+IAR+11177777777+++HWB:12345678:69'", result);
			AssertContains("<h4>111-77777777-12345678/69</h4>", generator.MessageInterpretation);
		}

		public void TestISR_Consol()
		{
			TestIsrRunner(false);
		}

		public void TestISR_Basic()
		{
			TestIsrRunner(true);
		}

		void TestIsrRunner(bool isBasic)
		{
			Mawb.Profile = "CUKAIR98LHRSID";
			var bill = Mawb;
			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109605+<<SYSCAR>>'
BGM+ISR+11177777777+++HWB:M'
LOC+11:LHR:145:3::SID:129:ZZZ+85:LHR:145:3::NSI:129:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+7+<<MSGNO PLACEHOLDER>>'
";
			if (isBasic)
			{
				expected = expected.Replace("+++HWB:M", "");
				Mawb.ChildBills.RemoveAndDelete(Mawb.ChildBills[0]);
			}
			var generator = new CusdecGeneratorISR(Mawb, new ErrorCollector(), GetNewISR());
			var result = generator.MakeMessageText();
			AssertEquals(expected.Replace(System.Environment.NewLine, ""), result);
		}

		public void TestTSR_Air()
		{
			var tsr = GetNewTSR();
			Mawb.NumberOfPiecesExpected = 678;
			mawb.TSRs.Add(tsr);
			tsr.NoPackagesExpected = 789;
			tsr.PortOfShipment = "FRCDG";
			Mawb.AirportOfArrival = "MAN";
			var ec = new ErrorCollector();
			var generator = new CusdecGeneratorTSR(Mawb, ec, tsr);
			tsr.AirportOrCountryOfDestination = "LAX";
			tsr.ValueOfGoods = 10m; // value but no currency - omit the MOA segment
			var result = generator.MakeMessageText();
			string expectedAirWithAirport = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:MAN:145:3::SID:129:ZZZ+85:LAX:145:3+5:CDG:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:789'
UNT+11+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expectedAirWithAirport.Replace(System.Environment.NewLine, ""), result);

			tsr.ValueOfGoods = 10m;
			tsr.CurrencyCode = "GBP";
			tsr.AirportOrCountryOfDestination = "US";
			tsr.OnwardAirWaybillNumber = "";
			tsr.OnwardCarrier = "";
			generator = new CusdecGeneratorTSR(Mawb, ec, tsr);
			result = generator.MakeMessageText();
			string expectedAirWithCountry = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+ABE:4321'
LOC+11:MAN:145:3::SID:129:ZZZ+28:US+5:CDG:139'
TDT+12++40'
GIS+LIC:109:ZZZ'
MOA+14+40:10:GBP:1'
UNS+D'
UNS+S'
CNT+11:789'
UNT+11+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expectedAirWithCountry.Replace(System.Environment.NewLine, ""), result);
			tsr.PortOfShipment = "";
			ec = new ErrorCollector();
			generator = new CusdecGeneratorTSR(Mawb, ec, tsr);
			result = generator.MakeMessageText();
			AssertContains("Port of shipment", ec.GetErrorsAsString());
		}

		public void TestTSR_Sea()
		{
			var tsr = GetNewTSR();
			var ec = new ErrorCollector();
			var generator = new CusdecGeneratorTSR(Mawb, ec, tsr);
			tsr.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.No;
			Mawb.AirportOfArrival = "MAN";
			tsr.AirportOrCountryOfDestination = "FR";
			tsr.OnwardMode = "10";
			tsr.OnwardCarrier = "XX";
			tsr.CurrencyCode = "GBP";
			tsr.ValueOfGoods = 0m; // currency but no value - no MOA
			var result = generator.MakeMessageText();
			string expectedNotAir = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:MAN:145:3::SID:129:ZZZ+28:FR+5:QQD:139'
TDT+12++10'
UNS+D'
UNS+S'
CNT+11:5'
UNT+10+<<MSGNO PLACEHOLDER>>'
";
			AssertEquals(expectedNotAir.Replace(System.Environment.NewLine, ""), result);
			AssertEquals("", ec.GetErrorsAsString());
		}

		public void TestTSR_NoLRI()
		{
			var tsr = GetNewTSR();
			var ec = new ErrorCollector();
			var generator = new CusdecGeneratorTSR(Mawb, ec, tsr);
			tsr.LicenseRestrictionInd = "X";
			Mawb.AirportOfArrival = "MAN";
			tsr.AirportOrCountryOfDestination = "FR";
			tsr.OnwardMode = "10";
			var result = generator.MakeMessageText();
			AssertContains("LRI", ec.GetErrorsAsString());
		}

		public void TestFBKs()
		{
			CusdecGeneratorFBK.IsAllowedToMakeFallbacks = false;
			var hawb = Mawb.ChildBills[0];
			var fbk = GetNewFBK();
			hawb.FBKs.Add(fbk);
			var ec = new ErrorCollector();
			var generator = new CusdecGeneratorFBK(hawb, ec, fbk);
			fbk.AgentsReference = "12345678";
			fbk.NoPackagesExpected = 68;
			Factory.Save();
			AssertEquals("Pre-req: C4_SendersMessageReference is populated", "U12345678", fbk.C4_SendersMessageReference);
			var result = generator.MakeMessageText();
			AssertContains("security rights", ec.GetErrorsAsString());
			CusdecGeneratorFBK.IsAllowedToMakeFallbacks = true;
			ec = new ErrorCollector();
			generator = new CusdecGeneratorFBK(hawb, ec, fbk);
			AssertEquals("", ec.GetErrorsAsString());
			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109601+<<SYSCAR>>'
BGM+FBK+11177777777+++HWB:12345678'
RFF+ABE:12345678'
LOC+11:LHR:145:3::SID:129:ZZZ'
UNS+D'
UNS+S'
CNT+11:68'
UNT+8+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expected.Replace(System.Environment.NewLine, ""), result);
			AssertContains(@"<h3>Fallback Request (FBK)</h3>							
							<h4>111-77777777-12345678</h4>", generator.MessageInterpretation);
			AssertContains(@"><td>Airport of receipt</td><td>LHR</td></tr><tr><td>Current Shed</td><td>SID</td></tr><tr><td>Airport of Destination</td><td>MAN</td></tr><tr><td>NPX</td><td>68</td></tr><tr><td>Reference</td><td>12345678</td></tr></table>", generator.MessageInterpretation);
		}

		public void TestAirportOrCountryOfDestination_Port2Iata()
		{
			var tsr = GetNewTSR();
			var ec = new ErrorCollector();
			var generator = new CusdecGeneratorTSR(Mawb, ec, tsr);

			tsr.AirportOrCountryOfDestination = "FRABB";
			var result = generator.MakeMessageText();
			var expectedWithAirportForUnloco = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:XAB:145:3+5:QQD:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedWithAirportForUnloco.Replace(System.Environment.NewLine, ""), result);
			AssertContains("<td>Airport of Destination (&amp; IATA)</td><td>FRABB (XAB)</td>", tsr.ToString());

			tsr.AirportOrCountryOfDestination = "US";
			result = generator.MakeMessageText();
			var expectedWithCountry = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+28:US+5:QQD:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedWithCountry.Replace(System.Environment.NewLine, ""), result);
			AssertContains("<td>Airport of Destination</td><td>US</td>", tsr.ToString());

			tsr.AirportOrCountryOfDestination = "JFK";
			result = generator.MakeMessageText();
			var expectedWithAirport = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:JFK:145:3+5:QQD:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedWithAirport.Replace(System.Environment.NewLine, ""), result);
			AssertContains("<td>Airport of Destination</td><td>JFK</td>", tsr.ToString());

			tsr.AirportOrCountryOfDestination = "@@XXX";
			result = generator.MakeMessageText();
			var expectedWithInvalidPort = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:XXX:145:3+5:QQD:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedWithInvalidPort.Replace(System.Environment.NewLine, ""), result);

			tsr.AirportOrCountryOfDestination = "USCHI";
			result = generator.MakeMessageText();
			var expectedWithAirportForPort2 = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:2:912:UN:109604+<<SYSCAR>>'
BGM+TSR+11177777777+++HWB:M'
RFF+740:11166666666'
RFF+ABE:4321'
LOC+11:LHR:145:3::SID:129:ZZZ+85:MDW:145:3+5:QQD:139'
TDT+12++40+++CC:172:3'
GIS+LIC:109:ZZZ'
UNS+D'
UNS+S'
CNT+11:5'
UNT+11+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedWithAirportForPort2.Replace(System.Environment.NewLine, ""), result);
		}

		CusMAWB mawb;
		CusMAWB Mawb
		{
			get { return mawb ?? (mawb = GetNewMawb()); }
		}

		CusMAWB GetNewMawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.AgentBadge = "CUK";
			mawb.Profile = "CUKFFW98000XXX"; //agent
			mawb.CargoTerminalOperator = "SID";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CM_MAWB = "11177777777";
			mawb.AirportOfDestination = "LHR";
			mawb.AirportOfArrival = "LHR";
			mawb.NumberOfPiecesExpected = 5;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			hawb.CS_PiecesManifested = 69;
			return mawb;
		}

		T GetNewUnderbond<T>() where T : CusUnderbond
		{
			var underbond = Factory.New<T>();
			underbond.AgentsReference = "4321";
			underbond.AirportOrCountryOfDestination = "MAN";
			return underbond;
		}

		InterAirportRemoval GetNewIAR()
		{
			var underbond = Mawb.IARs.AddNew();
			underbond.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			underbond.OnwardAirWaybillNumber = "11166666666";
			underbond.AgentsReference = "4321";
			underbond.OnwardCarrier = "CC";
			underbond.NewShedId = "NSI";
			underbond.AirportOrCountryOfDestination = "MAN";
			return underbond;
		}

		InterShedRemoval GetNewISR()
		{
			var underbond = Mawb.ISRs.AddNew();
			underbond.AgentsReference = "4321";
			underbond.NewShedId = "NSI";
			return underbond;
		}

		Fallback GetNewFBK()
		{
			return GetNewUnderbond<Fallback>();
		}

		TranshipmentRemoval GetNewTSR()
		{
			var underbond = GetNewUnderbond<TranshipmentRemoval>();
			underbond.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			underbond.OnwardAirWaybillNumber = "11166666666";
			underbond.OnwardCarrier = "CC";
			underbond.OnwardMode = "40"; // air
			underbond.PortOfShipment = "QQD";  // Dover
			return underbond;
		}

		delegate CusdecGeneratorBase GeneratorMaker(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond ub);
		ErrorCollector errorCollector;
	}
}
