using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.InboundParsersTests.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk.Testing
{
	internal class CcsukWrapperTest : TestCaseWithFactory
	{
		public void TestNewFromAwb()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12387654321";
			var w = CcsukWrapper.New(basic, Factory);
			AssertEquals("123-87654321", w.MAWBHAWBSPLIT);
			var hawb = basic.ChildBills.AddNew();
			hawb.CS_HAWB = "11223344";
			w = CcsukWrapper.New(hawb, Factory);
			AssertEquals("123-87654321-11223344", w.MAWBHAWBSPLIT);
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "69";
			w = CcsukWrapper.New(splitHouse, Factory);
			AssertEquals("123-87654321-11223344/69", w.MAWBHAWBSPLIT);

			var basic2 = Factory.New<CusMAWB>();
			basic2.CM_MAWB = "22287654321";
			var splitBasic = basic2.Splits.AddNew();
			splitBasic.SplitReference = "70";
			w = CcsukWrapper.New(splitBasic, Factory);
			AssertEquals("222-87654321/70", w.MAWBHAWBSPLIT);
		}

		public void TestNumberOfPiecesRelevantToThisRendering()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12387654321";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 5;
			var w = CcsukWrapper.New(basic, Factory);
			AssertContains("5 OF 10", w.PARTIALNOP);
		}

		public void TestNewFromFsnEdiMessageBasic()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CW/10/12SEP1200/000456/I LIKE SMALL BUTTS'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var basic = Factory.New<CusMAWB>();
			var message = basic.Messages.AddNew();
			var gbEdiMessage = Factory.Load<GbEDIMessage>(message.PK);
			gbEdiMessage.EM_MessageType = "CIM";
			gbEdiMessage.EM_MessageSubType = "FSN";
			gbEdiMessage.EM_MessageText = cim;
			var wrapper = CcsukWrapper.New(gbEdiMessage, Factory);
			AssertNotNull(wrapper);
			AssertType(typeof(CcsukWrapperFromFsn), wrapper);
			var declarationForBasic = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			AssertEquals(declarationForBasic, ((CcsukWrapperFromFsn)wrapper).Declaration);
		}

		public void TestNewFromFsnEdiMessageHawb()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675-87654321:
CSN/CW/10/12SEP1200/000456/I LIKE SMALL BUTTS'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var message = hawb.Messages.AddNew();
			var gbEdiMessage = Factory.Load<GbEDIMessage>(message.PK);
			gbEdiMessage.EM_MessageType = "CIM";
			gbEdiMessage.EM_MessageSubType = "FSN";
			gbEdiMessage.EM_MessageText = cim;
			var wrapper = CcsukWrapper.New(gbEdiMessage, Factory);
			AssertNotNull(wrapper);
			AssertType(typeof(CcsukWrapperFromFsn), wrapper);
			var declarationForBasic = ((ICcsukCusAwb)hawb).CreateNewStandaloneCDSDeclaration();
			AssertEquals(declarationForBasic, ((CcsukWrapperFromFsn)wrapper).Declaration);
		}

		public void TestAllPropertiesFsnIar()
		{
			GB.Ccsuk.AirCargoInventory.Testing.LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			GbEDIMessage inboundFsnCwMessage;
			CusMAWB basic;
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCW, false, "CUKFFW98XXXYYY");
			basic.AgentBadge = "LXA";
			inboundFsnCwMessage.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			inboundFsnCwMessage.EM_MessageSubType = CIMFSN.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			basic.Messages.Add(inboundFsnCwMessage); // normally done by the service task processor before wrappers are thought of; here we must link manually
			basic.NumberOfPiecesExpected = 70;
			var fsnWrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertType(typeof(CcsukWrapperFromFsn), fsnWrapper);
			Assert("Expected 'Daniel Test Agent'. Was " + fsnWrapper.AGENTNAME, fsnWrapper.AGENTNAME.Contains("Daniel Test Agent", System.StringComparison.InvariantCultureIgnoreCase));
			AssertEquals("000456", fsnWrapper.AGENTREF);
			AssertEquals("LXA", fsnWrapper.BADGE);
			AssertEquals("", fsnWrapper.LICENCEINDICATOR1);
			AssertEquals("", fsnWrapper.LICENCEINDICATOR2);
			AssertEquals("BAC", fsnWrapper.NEWSHED);
			AssertEquals("PART RELEASE FOR 10 OF 70 PACKAGES ONLY", fsnWrapper.PARTIALNOP);
			AssertEquals("PART RELEASE", fsnWrapper.PARTRELEASE);
			AssertEquals("", fsnWrapper.POS);
			AssertEquals("", fsnWrapper.T1STATEMENT);
			AssertEquals("for XXXYYY", fsnWrapper.RECIPIENT);
		}

		public void TestAllPropertiesFsnSplit()
		{
			GbEDIMessage inboundFsnCwMessage;
			CusMAWB basic;
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCwSplit);
			var split = basic.Splits.AddNew();
			split.HandlingInformation = "Handle with care";
			split.SplitReference = "03";
			Factory.Save();
			inboundFsnCwMessage.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			inboundFsnCwMessage.EM_ApplicationReference = "03";
			inboundFsnCwMessage.EM_MessageSubType = CIMFSN.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			basic.Messages.Add(inboundFsnCwMessage); // normally done by the service task processor before wrappers are thought of; here we must link manually
			basic.NumberOfPiecesExpected = 70;
			var fsnWrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertType(typeof(CcsukWrapperFromFsn), fsnWrapper);
			AssertEndsWith("", "/03", fsnWrapper.MAWBHAWBSPLIT);
			AssertEquals("", fsnWrapper.REMARKS);
			var declarationForSplit = ((ICcsukCusAwb)split).CreateNewStandaloneCDSDeclaration();
			AssertEquals(declarationForSplit, ((CcsukWrapperFromFsn)fsnWrapper).Declaration);
			AssertEquals("", fsnWrapper.RECIPIENT);
		}

		GbEDIMessage CreateFsnTsr(out CusMAWB basic)
		{
			GbEDIMessage inboundFsnCwMessage;
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCW, true);
			inboundFsnCwMessage.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			inboundFsnCwMessage.EM_MessageSubType = CIMFSN.Code; // normally done by the service task processor before wrappers are thought of; here we must set manually
			var tsr = basic.TSRs[0];
			tsr.OnwardCarrier = "BD";
			tsr.AirportOrCountryOfDestination = "FRABB";
			basic.CargoTerminalOperator = "LCS";
			basic.CargoTerminalOperatorAirport = "LTN";
			basic.CM_FlightNo = "BA1234";
			basic.Messages.Add(inboundFsnCwMessage); // normally done by the service task processor before wrappers are thought of; here we must link manually
			return inboundFsnCwMessage;
		}

		public void TestAllPropertiesFsnTsr()
		{
			ShedTest.CreateShed(Factory, "GB", "LTNLCS", "LONDON LUTON CARGO at Luton");
			Factory.Save();

			GB.Ccsuk.AirCargoInventory.Testing.LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			CusMAWB basic;
			GbEDIMessage inboundFsnCwMessage = CreateFsnTsr(out basic);
			basic.AgentBadge = "LXA";
			var wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertType(typeof(CcsukWrapperFromFsn), wrapper);
			Assert("Expected 'Daniel Test Agent'. Was " + wrapper.AGENTNAME, wrapper.AGENTNAME.Contains("Daniel Test Agent", System.StringComparison.InvariantCultureIgnoreCase));
			AssertEquals("000456", wrapper.AGENTREF);
			AssertEquals("LXA", wrapper.BADGE);
			AssertContains("RESTRICTED", wrapper.LICENCEINDICATOR1);
			AssertContains("DECLARED", wrapper.LICENCEINDICATOR2);
			AssertEquals("", wrapper.NEWSHED);
			AssertContains("", wrapper.PARTIALNOP);
			AssertEquals("", wrapper.PARTRELEASE);
			AssertEquals("MAN", wrapper.POS);
			AssertContains(" T1 ", wrapper.T1STATEMENT);
			AssertContains("T012345", wrapper.TRN);
			var fsnWrapper = (CcsukWrapperFromFsn)wrapper;
			AssertContains("LUTON", fsnWrapper.SHEDNAME);
			AssertEquals("BA", fsnWrapper.INWARDCARRIER);
			AssertEquals("BD British Midland Airways Ltd.", fsnWrapper.ONWARDCARRIER);
			AssertEquals("XAB", fsnWrapper.NEWAOD);
			var declarationForBasic = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			AssertEquals(declarationForBasic, fsnWrapper.Declaration);
		}

		public void TestPartAndLastPartReleases_AllReceivedButReleasedInParts()
		{
			CusMAWB basic;
			GbEDIMessage inboundFsnCwMessage = CreateFsnTsr(out basic);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			var wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("", wrapper.PARTIALNOP);
			AssertEquals("", wrapper.PARTRELEASE);
			basic.ReleaseThisNumberOfPieces(6, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("PART RELEASE", wrapper.PARTRELEASE);
			AssertContains("6 OF 10", wrapper.PARTIALNOP);
			Thread.Sleep(10);  // To ensure that we are getting the most recent activity.
			basic.ReleaseThisNumberOfPieces(4, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("LAST PART RELEASE", wrapper.PARTRELEASE);
			AssertContains("4 OF 10", wrapper.PARTIALNOP);
		}

		public void TestPartAndLastPartReleases_ReceivedAndReleaseInParts()
		{
			CusMAWB basic;
			GbEDIMessage inboundFsnCwMessage = CreateFsnTsr(out basic);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 9;
			var wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("", wrapper.PARTIALNOP);
			AssertEquals("", wrapper.PARTRELEASE);
			basic.ReleaseThisNumberOfPieces(4, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("PART RELEASE", wrapper.PARTRELEASE);
			AssertContains("4 OF 10", wrapper.PARTIALNOP);
			Thread.Sleep(10);  // To ensure that we are getting the most recent activity.
			basic.ReleaseThisNumberOfPieces(5, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("PART RELEASE", wrapper.PARTRELEASE);  // NPR is 9 and have now released 9, but this is not the last part
			AssertContains("5 OF 10", wrapper.PARTIALNOP);
			Thread.Sleep(10);  // To ensure that we are getting the most recent activity.
			basic.NumberOfPiecesReceived = 10;
			basic.ReleaseThisNumberOfPieces(1, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("LAST PART RELEASE", wrapper.PARTRELEASE); // now it's the last part
			AssertContains("1 OF 10", wrapper.PARTIALNOP);
		}

		public void TestPartAndLastPartReleases_ReceiveAndReleaseAllInOneGo()
		{
			CusMAWB basic;
			GbEDIMessage inboundFsnCwMessage = CreateFsnTsr(out basic);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			var wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("", wrapper.PARTIALNOP);
			AssertEquals("", wrapper.PARTRELEASE);
			basic.ReleaseThisNumberOfPieces(10, NumberOfPiecesReleasedHelper.ShedEvent);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("", wrapper.PARTRELEASE); // this is the first and final release but it's not the last part release.
			AssertContains("", wrapper.PARTIALNOP);
		}

		public void TestPartAndLastPartReleases_ReceiveAndReleaseAllAvailableButNotAllExpectedInOneGo()
		{
			CusMAWB basic;
			GbEDIMessage inboundFsnCwMessage = CreateFsnTsr(out basic);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 7;
			var wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("", wrapper.PARTIALNOP);
			AssertEquals("", wrapper.PARTRELEASE);

			basic.ReleaseThisNumberOfPieces(7, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("PART RELEASE", wrapper.PARTRELEASE); // this is the first release, for all 7 savailable pieces, but it's not the full/final release
			AssertContains("7 OF 10", wrapper.PARTIALNOP);

			Thread.Sleep(10);  // To ensure that getting the most recent release log really does get the most recent.

			basic.ReleaseThisNumberOfPieces(1, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("PART RELEASE", wrapper.PARTRELEASE);
			AssertContains("1 OF 10", wrapper.PARTIALNOP);

			Thread.Sleep(10);  // To ensure that getting the most recent release log really does get the most recent.

			basic.ReleaseThisNumberOfPieces(2, NumberOfPiecesReleasedHelper.AgentC1Event);
			wrapper = CcsukWrapper.New(inboundFsnCwMessage, Factory);
			AssertEquals("LAST PART RELEASE", wrapper.PARTRELEASE);
			AssertContains("2 OF 10", wrapper.PARTIALNOP);
		}

		[TestDate(1986, 3, 12, 16, 1, 0)]
		public void TestAllNonVirtualProperties_Hawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.AirportOfOrigin = "USATL";
			hawb.AirportOfOrigin = "USORD";
			mawb.CM_MAWB = "125-12345678";
			mawb.AirportOfDestination = "MAN";
			hawb.AirportOfDestination = "LHR";
			hawb.CS_HAWB = "87654321";
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "CWE";
			hawb.AgentBadge = "DAN";
			hawb.CS_Weight = 69m;
			hawb.CS_WeightUQ = "KG";
			hawb.CS_GoodsDescription = "STUFF";
			hawb.LatestCustomsActionText = "PUSSY GALORE";
			hawb.CS_RX_NKGoodsCurrency = "GBP";
			hawb.CS_GoodsValue = 69.69m;
			hawb.CS_PiecesManifested = 500;
			hawb.CS_PiecesLanded = 600;

			var message = hawb.Messages.AddNew();
			var w = new CcsukWrapper(message, hawb, Factory);
			AssertEquals("ORD", w.AOO);
			AssertEquals("LHR", w.AOD);
			AssertEquals("CWE", w.SHED);
			AssertEquals("125-12345678-87654321", w.MAWBHAWBSPLIT);
			AssertEquals("LHR", w.AIRPORT);
			AssertEquals("DAN", w.BADGE);
			AssertEquals("69KG", w.WEIGHT);
			AssertEquals("STUFF", w.DESCRIPTION);
			AssertEquals("PUSSY GALORE", w.CAT);
			AssertEquals("GBP", w.CURRENCY);
			AssertEquals("69", w.VALUE);
			AssertEquals("500", w.NPX);
			AssertEquals("600", w.NPR);
			AssertEquals("", w.CAD);
			AssertEquals("", w.CAC);
			hawb.SetCustomsActionCode("DC", ZDateTime.Now);
			AssertEquals("12-Mar-1986 16:01", w.CAD);
			AssertEquals("DC", w.CAC);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Foo";
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("Foo", w.AGENTREF);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Poop";
			mawb.CM_JK = consol.PK;
			AssertEquals("Poop", w.AGENTREF);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Bar";
			hawb.CS_JS = shipment.PK;
			AssertEquals("Bar", w.AGENTREF);
		}

		public void TestAllNonVirtualProperties_Mawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_RL_NKLoadPort = "USATL";
			mawb.CM_MAWB = "125-12345678";
			mawb.CM_RL_NKDischargePort = "LHR";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "CWE";
			mawb.AgentBadge = "DAN";
			mawb.Weight = 69m;
			mawb.WeightCode = "KG";
			mawb.DescriptionOfGoods = "STUFF";
			mawb.LatestCustomsActionText = "PUSSY GALORE";
			mawb.NumberOfPiecesExpected = 500;
			mawb.NumberOfPiecesReceived = 600;

			var message = mawb.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = new ZDateTime(1987, 12, 11, 1, 2, 3);
			var w = new CcsukWrapper(message, mawb, Factory);
			AssertEquals("ATL", w.AOO);
			AssertEquals("LHR", w.AOD);
			AssertEquals("CWE", w.SHED);
			AssertEquals("125-12345678", w.MAWBHAWBSPLIT);
			AssertEquals("LHR", w.AIRPORT);
			AssertEquals("DAN", w.BADGE);
			AssertEquals("69KG", w.WEIGHT);
			AssertEquals("STUFF", w.DESCRIPTION);
			AssertEquals("PUSSY GALORE", w.CAT);
			AssertEquals("", w.CURRENCY);
			AssertEquals("", w.VALUE);
			AssertEquals("500", w.NPX);
			AssertEquals("600", w.NPR);
		}
	}
}
