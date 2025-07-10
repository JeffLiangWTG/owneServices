using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class CcsukInterchangePackagerServiceTaskTest : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		public void TestInfoGetsLoggedWhenInterchangeIsPreparedAndMsgsArePacked()
		{
			var oneMessage = SetupOutboundMessage(null, "ONE");
			oneMessage.EM_MessageOwner = "FRED";
			oneMessage.EM_ApplicationReference = "BOBBY";
			oneMessage.EM_Status = "QUE";
			oneMessage.EM_MessageType = "CIM";
			Factory.Save();

			RunProcessors(runSenders: true);

			AssertContains("Information|Message ONE has been packaged into interchange.", log.ToString());
			AssertNotContains("Information|Interchange is prepared for upload. Application Code: GBG, message number: ONE.", log.ToString());
		}

		public void TestTwoQueuedMessagesArePackedInOneRun()
		{
			var oneMessage = SetupOutboundMessage(null, "ONE");
			var twoMessage = SetupOutboundMessage(null, "TWO");
			oneMessage.EM_MessageOwner = "FRED";
			twoMessage.EM_MessageOwner = "BILL";
			oneMessage.EM_ApplicationReference = "BOBBY";
			twoMessage.EM_ApplicationReference = "TOMMY";
			oneMessage.EM_Status = "QUE";
			twoMessage.EM_Status = "QUE";
			oneMessage.EM_MessageType = "CIM";
			twoMessage.EM_MessageType = "CDC";
			Factory.Save();
			RunProcessors(runSenders: true);
			oneMessage.Reload();
			twoMessage.Reload();
			AssertNotNull(oneMessage.Interchange);
			AssertNotNull(twoMessage.Interchange);
			AssertNotEquals(oneMessage.Interchange, twoMessage.Interchange);
			AssertContains("FRED", oneMessage.Interchange.EI_From);
			AssertContains("BILL", twoMessage.Interchange.EI_From);
			AssertContains("BOBBY", oneMessage.Interchange.EI_To);
			AssertContains("TOMMY", twoMessage.Interchange.EI_To);
			AssertEquals("QUE", oneMessage.Interchange.EI_Status);
			AssertEquals("QUE", twoMessage.Interchange.EI_Status);
			AssertContains("ONE", oneMessage.Interchange.EI_BodyText);
			AssertContains("TWO", twoMessage.Interchange.EI_BodyText);
			AssertEquals("PND", oneMessage.EM_Status);
			AssertEquals("PND", twoMessage.EM_Status);
		}

		public void TestRecipientPimas()
		{
			SetupCredentials("CUK");
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var mawb = Factory.New<CusMAWB>();

			// Sending messages on an Entry
			declaration.JE_MessageType = "EXP";
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToChiefX", new FakeFunctionForTest(), entry.Messages, "CUKCTM98CHFEXP");
			declaration.JE_MessageType = "IMP";
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToChiefX", new FakeFunctionForTest(), entry.Messages, "CUKCTM98CHFIMP");
			declaration.JE_MessageType = "ZZZ";
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToChiefX", new FakeFunctionForTest(), entry.Messages, "", "CCSUK package only knows how to send to CHIEF for imports or exports");

			//Sending messages on a mawb:
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new CcsukTransmissionMessageFunction.CUKFSR.FSA(), mawb.Messages, "ToCommDb");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToShed", new CcsukTransmissionMessageFunction.CIM.FSR(), mawb.Messages, "ToShed");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new CcsukTransmissionMessageFunction.CUKFSR.FSN(), mawb.Messages, "ToCommDb");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new CcsukTransmissionMessageFunction.CUSCAR.FCS(null), mawb.Messages, "ToCommDb");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToShedXX", new CcsukTransmissionMessageFunction.CIM.FRD(null), mawb.Messages, "ToShedXX");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new FakeFunctionForTest(), mawb.Messages, "", "CcsukInterchangeProvider doesn't know how to send to the recipient defined for this message type");

			// Standalone messages
			SetupNewOutboundMessageForPimaTestsAndRunTask("Not a PIMA", new CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry(), null, "CUKSYS98COMMDB"); // an FSR/ENQ message goes to the CommDB even though the application reference is not the recipient PIMA			

			// CCSUK messages on a consol
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new CcsukTransmissionMessageFunction.CUKFSR.FsaForExport(), consol.Messages, "ToCommDb");
			SetupNewOutboundMessageForPimaTestsAndRunTask("ToCommDb", new CcsukTransmissionMessageFunction.CUKFSR.FSA(), consol.Messages, "ToCommDb");
			SetupNewOutboundMessageForPimaTestsAndRunTask("Anything", new CcsukTransmissionMessageFunction.CUKG2G(), consol.Messages, "CUKSYS98CCSNES");
		}

		public void TestCanUnderstandLevelBSeparators()
		{
			// This test also checks that we can correctly fallback the notification email group.  We set-up NotificationCcsukGenral but the code will actually ask for NotificationCcsukGenralText, where no value is explicitly defined, and it will fall back up one level to get the value for NotificationCcsukGenral. 
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukGenral, Guid.Empty, Factory);
			GlbGroup postmastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			postmastersGroup.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();

			string textLevelB = @"UNBUNOB2CUKFFW98000WISIATACUKAIR98LHRCWEIATA110216234890AUNH99GENRAL0912UNBGMTXTZZZMSGUSERFTXAAAHELLO CWE THIS IS WIS  -- DANIELUNT599UNZ190";
			var receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(this.Factory, textLevelB, "CUK");
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			Factory.Save();
			RunProcessors();
			Factory.Save();
			receivedMessage.Reload();
			AssertEquals("Expect no errors parsing level B messages", "RCV", receivedMessage.EM_Status);
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestReceiveFsaForSentFsr()
		{
			var inboundFsaText = "UNH+x+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++21232132132+7:1012021520:201'FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED'DOC+740+21232132132+97:1012021538:201++++OLD'GIS+T:121:ZZZ'GIS+PAI:109:109'TDT+20'LOC+84:ATL:145:3+85:LHR:145:3+27:US+11:LHR:145:3::BAC:129:ZZZ:BAC'TDT+12++40'NAD+CB+CAR+CARGOWISE'GDS+2'QTY+118:1'MEA+WT++KGM:1.0'FTX+AAA+++CONSOLIDATION'UNT+15+463'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			var sentMessage = SetupOutboundMessage(mawb.Messages);
			sentMessage.EM_MessageSubType = "FSA";
			var inboundMessage = SetupInboundMessage(inboundFsaText, sentMessage);
			mawb.Factory.Save();
			Factory.Save();
			RunProcessors();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);

			var lastIncomingMessage = mawb.Messages.LastIncomingMessage;
			AssertEquals("Mawb message count", 2, mawb.Messages.Count);
			AssertEquals(sentMessage.PK, mawb.Messages.LastOutgoingMessage.PK);
			AssertEquals(inboundMessage.PK, lastIncomingMessage.PK);
			AssertEquals(EDIMessage.Schema.EM_MessageType, "FSA", lastIncomingMessage.EM_MessageType);
			AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FSA", lastIncomingMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", lastIncomingMessage.EM_Status);
			AssertContains("BASIC CONSIGNMENT RECORD RETRIEVED</h3>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th>Header Data</th><th>Value</th></tr></thead><tr><td>Air Waybill</td><td>21232132132</td></tr></table><BR/>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th>OLD Air waybill</th><th>21232132132</th></tr></thead><tr><td>Consignment Reference Number</td><td>21232132132</td></tr><tr><td>Consignment Reference Number Type</td><td>740</td></tr><tr><td>Old Or New Data Indicator</td><td>OLD</td></tr><tr><td>Inward Leg</td><td> Airport of arrival=LHR, Shed=BAC BAC<br>	Airport of destination=LHR<br>	Airport of origin=ATL<br>	Country of origin=US</td></tr><tr><td>Onward Leg</td><td>&nbsp;</td></tr><tr><td>Shipment Description Code</td><td>T</td></tr><tr><td>Pre Arrival Indicator</td><td>Y</td></tr><tr><td>Onward Transport Means</td><td>40</td></tr><tr><td>Agent Code</td><td>CAR</td></tr><tr><td>Agent Name</td><td>CARGOWISE</td></tr><tr><td>Description Of Goods</td><td>CONSOLIDATION</td></tr><tr><td>NPX</td><td>1</td></tr><tr><td>Weight</td><td>1.0</td></tr><tr><td>Weight Code</td><td>KGM</td>", lastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestReceiveFsaE0ForSentCusdec()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCukFsaReport, Guid.Empty, Factory);
			var inboundFsaText = "UNH+JUM1BEQVDRTJY0+CUKFSA:1:912:BT'BGM+:::E0+69696969000+7:1012101707:201'DOC+740+69696969000'GIS+23:117:ZZZ'TDT+20++40'LOC+11:ABC:145:3::XX-:129:ZZZ'CST+0+048:110:ZZZ'FTX+IRT+++INVENTORY REFERENCE CONTAINS INCORRECT DATA:SHED CODE INVALID'RFF+ACF:120'RFF+TN:000626A+141:20101210:102'RFF+ABE:B0001035'QTY+66:1'UNT+13+JUM1BEQVDRTJY0'";
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "120-000626A";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2010, 12, 10);
			var sentMessage = SetupOutboundMessage(entry.Messages);
			var inboundMessage = SetupInboundMessage(inboundFsaText, entry);
			entry.Factory.Save();
			Factory.Save();
			RunProcessors();

			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);  // crappy refresh
			var lastIncomingMessage = entry.Messages.LastIncomingMessage;
			AssertEquals("Entry message count", 2, entry.Messages.Count);
			AssertEquals(sentMessage.PK, entry.Messages.LastOutgoingMessage.PK);
			AssertEquals(inboundMessage.PK, lastIncomingMessage.PK);
			AssertEquals(EDIMessage.Schema.EM_MessageType, "FSA", lastIncomingMessage.EM_MessageType);
			AssertEquals(EDIMessage.Schema.EM_MessageSubType, "E0", lastIncomingMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", lastIncomingMessage.EM_Status);
			AssertContains("<h3>FSA: Inventory Failure Report E0 </h3>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th>Header Data</th><th>Value</th></tr></thead><tr><td>Air Waybill</td><td>69696969000</td></tr>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th> Air waybill</th><th>69696969000</th></tr></thead><tr><td>Consignment Reference Number</td><td>69696969000</td></tr><tr><td>Consignment Reference Number Type</td><td>740</td></tr><tr><td>Inward Leg</td><td> Airport of arrival=ABC, Shed=XX- </td></tr><tr><td>Consignment Type</td><td>23</td></tr><tr><td>Entry Processing Unit</td><td>120</td></tr><tr><td>Entry Number</td><td>000626A</td></tr><tr><td>Entry Date</td><td>10-Dec-10 00:00:00</td></tr><tr><td>Agents Reference Number</td><td>B0001035</td></tr><tr><td>Num Packages Entered</td><td>1</td></tr><tr><td>Inventory Return Code IRC</td><td>048</td></tr><tr><td>Inventory Return Code Meaning</td><td>The consignment reference has been input in an invalid format.</td></tr><tr><td>IRC Text</td><td>INVENTORY REFERENCE CONTAINS INCORRECT DATA SHED CODE INVALID</td></tr></table>", lastIncomingMessage.EM_MessageInterpretation);
			dec.Reload();
			AssertEquals("048", entry.CH_IrcInventoryReturnCode);
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestReceiveFsaE0ForSentCusdecWithMRN()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCukFsaReport, Guid.Empty, Factory);
			var inboundFsaText = "UNH+MSGREF+CUKFSA:4:912:BT'BGM++11177777777'DOC+740+11177777777+97:9308091055:201'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+AZ123++++AZ:172:3++178:930809:101'LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'NAD+CB+FRF+FREDS FORWARDING'GDS+2'QTY+118:25'QTY+48:25'MEA+WT++KGM:100'DTM+7:9308091055:201'FTX+AAA+++SMALL WIDGETS'CST++CC:117:ZZZ+01:120:109+3:141:109+000:110:ZZZ'FTX+CAT+++CUSTOMS CLEARED'FTX+IRT+++INVENTORY RETURN CODE TEXT'DTM+176:9308091320:201'RFF+ACF:131'RFF+TN:20GB34F8Y2O2CX8PT4+141:20200509:102'RFF+ABE:12345678AAAABBBBCCCC'QTY+66:25'UNT +23+MSGREF";
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "20GB34F8Y2O2CX8PT4";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2020, 05, 09);
			entry.CusEntryNumber.CE_EntryType = "MRN";
			var sentMessage = SetupOutboundMessage(entry.Messages);
			var inboundMessage = SetupInboundMessage(inboundFsaText, entry);
			entry.Factory.Save();
			Factory.Save();
			RunProcessors();

			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);  // crappy refresh
			var lastIncomingMessage = entry.Messages.LastIncomingMessage;
			AssertEquals("Entry message count", 2, entry.Messages.Count);
			AssertEquals(sentMessage.PK, entry.Messages.LastOutgoingMessage.PK);
			AssertEquals(inboundMessage.PK, lastIncomingMessage.PK);
			AssertEquals(EDIMessage.Schema.EM_MessageType, "FSA", lastIncomingMessage.EM_MessageType);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", lastIncomingMessage.EM_Status);
			AssertContains("<h3>Freight Status Enquiry Answer (FSA)</h3>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th>Header Data</th><th>Value</th></tr></thead><tr><td>Air Waybill</td><td>11177777777</td></tr>", lastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<th> Air waybill</th><th>11177777777</th></tr></thead><tr><td>Consignment Reference Number</td><td>11177777777</td></tr><tr><td>Consignment Reference Number Type</td><td>740</td></tr><tr><td>Inward Leg</td><td>AZ AZ123 09-Aug-93 00:00:00<br> Airport of arrival=LHR, Shed=KLM AAA<br>	Airport of destination=LHR<br>	Airport of origin=LAX<br>	Country of origin=US</td></tr><tr><td>Shipment Description Code</td><td>T</td></tr><tr><td>Consignment Type</td><td>23</td></tr><tr><td>Inbound Carrier Code</td><td>AZ</td></tr><tr><td>Inbound Flight Number</td><td>AZ123</td></tr><tr><td>Date Of Arrival</td><td>09-Aug-93 00:00:00</td></tr><tr><td>Agent Code</td><td>FRF</td></tr><tr><td>Agent Name</td><td>FREDS FORWARDING</td></tr><tr><td>Description Of Goods</td><td>SMALL WIDGETS</td></tr><tr><td>NPX</td><td>25</td></tr><tr><td>NPR</td><td>25</td></tr><tr><td>Weight</td><td>100</td></tr><tr><td>Weight Code</td><td>KGM</td></tr><tr><td>Status 1 Date</td><td>09-Aug-93 10:55:00</td></tr><tr><td>Entry Processing Unit</td><td>131</td></tr><tr><td>Entry Number</td><td>20GB34F8Y2O2CX8PT4</td></tr><tr><td>Entry Date</td><td>09-May-20 00:00:00</td></tr><tr><td>Agents Reference Number</td><td>12345678AAAABBBBCCCC</td></tr><tr><td>Num Packages Entered</td><td>25</td></tr><tr><td>Route</td><td>3</td></tr><tr><td>Customs Clearance Status</td><td>01</td></tr><tr><td>Customs Action Code</td><td>CC</td></tr><tr><td>Customs Action Text</td><td>CUSTOMS CLEARED</td></tr><tr><td>Date Of Customs Action</td><td>09-Aug-93 13:20:00</td></tr><tr><td>Inventory Return Code IRC</td><td>000</td></tr><tr><td>Inventory Return Code Meaning</td><td>Entry details successfully matched with CCS-UK.</td></tr><tr><td>IRC Text</td><td>INVENTORY RETURN CODE TEXT</td></tr></table>", lastIncomingMessage.EM_MessageInterpretation);
			dec.Reload();
			AssertEquals("000", entry.CH_IrcInventoryReturnCode);
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestReceiveContrlForSentFsrStandalone()
		{
			var inboundControlText = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - ENTRY OR REQUEST NOT YET MADE'UNT+8+JSR1BEHZHXV7Y0'";
			var sentMessage = SetupOutboundMessage(null);
			sentMessage.EM_MessageType = "FSR";
			sentMessage.EM_MessageSubType = "ENQ";
			var inboundMessage = SetupInboundMessage(inboundControlText, sentMessage);
			Factory.Save();

			RunProcessors();
			inboundMessage.Reload();
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", inboundMessage.EM_Status);
			AssertEquals(sentMessage.PK, inboundMessage.EM_LinkUniqueID);
		}

		public void TestReceiveContrlForMessageLinkedToEntryNotAwb()
		{
			var inboundControlText = "UNH+002F3131017000+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000WIS:IATA+CUKCTM98CHFEXP:IATA+00348131016002+1'UCM+00348131016003+CUSDEC:D:04A:UN:IATA01'UCX+1'FTX+AAA+++MESSAGE STORED FOR LATER TRANSMISSION.'UNT+6+002F3131017000'";
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "AAA";
			entry.CH_Status = "BBB";
			var outboundMessage = SetupOutboundMessage(entry.Messages);
			var receivedMessage = SetupInboundMessage(inboundControlText, entry); // see... it's linked to the entry
			Factory.Save();
			RunProcessors();
			receivedMessage.Reload();
			outboundMessage.Reload();
			entry.Reload();
			declaration.Reload();
			AssertEquals("Sent " + EDIMessage.Schema.EM_Status, EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertEquals("Received " + EDIMessage.Schema.EM_Status, EDIMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals("Received message (sub)type", "CTLACK", receivedMessage.EM_MessageType + receivedMessage.EM_MessageSubType);
			AssertEquals(entry.PK, receivedMessage.EM_LinkUniqueID);
			AssertEndsWith(CusEntryHeader.Schema.CH_Status + " updated", Customs.Common.EU.MessageStatusList.Codes.OK, entry.CH_Status);
			AssertEndsWith(CusEntryHeader.Schema.CH_EntryStatus + " untouched", "AAA", entry.CH_EntryStatus);
		}

		public void TestReceiveContrlForSentFsr()
		{
			var inboundControlText = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - ENTRY OR REQUEST NOT YET MADE'UNT+8+JSR1BEHZHXV7Y0'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			var sentMessage = SetupOutboundMessage(mawb.Messages);
			var inboundMessage = SetupInboundMessage(inboundControlText, sentMessage);
			Factory.Save();

			RunProcessors();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			mawb.Reload();
			mawb.Messages.Load();

			var lastIncomingMessage = mawb.Messages.LastIncomingMessage;
			AssertEquals("Mawb message count", 2, mawb.Messages.Count);
			AssertEquals(sentMessage.PK, mawb.Messages.LastOutgoingMessage.PK);
			AssertEquals(inboundMessage.PK, lastIncomingMessage.PK);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", lastIncomingMessage.EM_Status);
			AssertContains(@"<html> <h3>CONTRL</h3>
<b>
REJECTED - ENTRY OR REQUEST NOT YET MADE
</b>
<p>Outgoing message #msgNum (FOO) was rejected</p>
", lastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestReceiveContrlForSentMessage()
		{
			var inboundControlText_Duplicate = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - RECORD ALREADY EXISTS'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_ParseError = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++DB-CLP MESSAGE PARSE FAILED - DATA ELEMENT ERROR'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_MasterNotYetCreated = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++MASTER NOT YET CREATED'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_SecondLevelValidation = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - AIRPORT OF ORIGIN DOES NOT EXIST'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_FirstLevelValidation = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++DB-CLP MESSAGE PARSE FAILED - OTHER ERROR'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_CusDecReason1 = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - NOP IN MESSAGE NOT EQUAL TO NPX IN CONSIGNMENT'UNT+8+JSR1BEHZHXV7Y0'";
			var inboundControlText_CusDecReason2 = "UNH+JSR1BEHZHXV7Y0+CONTRL:1:912:UN+<<SYSCAR>>'UCI+CUKFFW98000CAR/:IATA+CUKSYS98COMMDB/+152+4'UCM+466:9AE6CD0C4E414397941CA78B78541388+CUKFSR:1:912:BT'UCX+4+6'UCR+2'UCD+3:000'FTX+AAA+++REJECTED - STATUS 3 ALREADY SET ON CONSIGNMENT'UNT+8+JSR1BEHZHXV7Y0'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			var sentMessageOnHawb = SetupOutboundMessage(hawb.Messages);
			sentMessageOnHawb.EM_MessageType = CcsukTransmissionMessageFunction.CUSCAR.FRI.Code;
			sentMessageOnHawb.EM_MessageSubType = CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode;

			RunControlParsePresenceUpdaterTest(inboundControlText_Duplicate, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.OnCommDb, "Rejected cos duplicate, known to be on network");
			RunControlParsePresenceUpdaterTest(inboundControlText_ParseError, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NotOnCommDb, "Parser error on job whose status is not 'On CommDB' means go to 'Not on commDB'");
			RunControlParsePresenceUpdaterTest(inboundControlText_MasterNotYetCreated, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NotOnCommDb, "Not on network. House without master cannot exist on network");
			RunControlParsePresenceUpdaterTest(inboundControlText_FirstLevelValidation, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NotOnCommDb, "Very first message is rejected with a first level failure - not on network");
			RunControlParsePresenceUpdaterTest(inboundControlText_SecondLevelValidation, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NotOnCommDb, "Very first message is rejected with a second level failure - not on network");

			sentMessageOnHawb.EM_MessageType = CcsukTransmissionMessageFunction.CUSDEC.Code;
			RunControlParsePresenceUpdaterTest(inboundControlText_CusDecReason1, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.OnCommDb, "A cusdec rejected with this reason means consignment is on network");
			RunControlParsePresenceUpdaterTest(inboundControlText_CusDecReason2, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.OnCommDb, "A cusdec rejected with this reason means consignment is on network");

			sentMessageOnHawb.EM_MessageType = CcsukTransmissionMessageFunction.CUSCAR.FRI.Code;
			var evenOlderMessage = SetupOutboundMessage(hawb.Messages);
			evenOlderMessage.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			RunControlParsePresenceUpdaterTest(inboundControlText_FirstLevelValidation, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck, "SECOND message (an FRI) is rejected with a first level failure - unknown");
			RunControlParsePresenceUpdaterTest(inboundControlText_SecondLevelValidation, hawb, sentMessageOnHawb, PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck, "SECOND message  (an FRI) is rejected with a second level failure - unknown");
		}

		void RunControlParsePresenceUpdaterTest(string inboundControlText_Duplicate, CusHAWB hawb, EDIMessage sentMessageOnHawb, string expectedPresence, string failureTextForDeveloper)
		{
			hawb.PresenceOnNetworkStatus = "YYY";
			var inboundMessage = SetupInboundMessage(inboundControlText_Duplicate, sentMessageOnHawb);
			Factory.Save();
			RunProcessors();
			Factory.ClearQueryCache();
			hawb.Reload();
			AssertEquals(failureTextForDeveloper, expectedPresence, hawb.PresenceOnNetworkStatus);
		}

		public void TestReceiveContrlForSentCusdecWithNonGuidCommonAccessReference()
		{
			var inboundControlText = "UNH+JCL1BHYMJALHW0+CONTRL:1:912:UN+101/U00000065'UCI+CUKFFW98000CAR/+CUKSYS98COMMDB/+77+3'UCM+101:101U00000065+CUSDEC:2:912:UN'UCX+3+6'UCR+2'UCD+2:000+6'FTX+AAA+++REJECTED - AIRPORT MUST BE IN FALLBACK TO PROCESS THIS MESSAGE'UNT+8+JCL1BHYMJALHW0'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			var sentMessage = SetupOutboundMessage(hawb.Messages, "101");
			var underbondBad = hawb.IARs.AddNew();
			var underbondGood = hawb.IARs.AddNew();
			underbondBad.C4_SendersMessageReference = "BLAH";
			underbondGood.C4_SendersMessageReference = "U00000065";
			var inboundMessage = SetupInboundMessage(inboundControlText, sentMessage);
			mawb.Factory.Save();
			Factory.Save();

			RunProcessors();
			sentMessage.Reload();
			inboundMessage.Reload();
			hawb.Messages.Load();
			AssertEquals("Hawb message count", 2, hawb.Messages.Count);
			AssertEquals(sentMessage.PK, hawb.Messages[0].PK);
			AssertEquals(inboundMessage.PK, hawb.Messages[1].PK);
			AssertEquals("REJ", sentMessage.EM_Status);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", inboundMessage.EM_Status);
			AssertContains(@"<html> <h3>CONTRL</h3>
<b>
REJECTED - AIRPORT MUST BE IN FALLBACK TO PROCESS THIS MESSAGE
</b>", inboundMessage.EM_MessageInterpretation);
			underbondGood.Reload();
			AssertEquals("REJ", underbondGood.C4_Status);
		}

		public void TestReceiveContrlPositive()
		{
			var inboundControlText = "UNH+JCL1BHYMJALHW0+CONTRL:1:912:UN+101/U00000065'UCI+CUKFFW98000CAR/+CUKSYS98COMMDB/+77+3'UCM+101:101U00000065+CUSDEC:2:912:UN'UCX+1'FTX+AAA+++MESSAGE STORED FOR LATER TRANSMISSION.'UNT+8+JCL1BHYMJALHW0'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			var sentMessage = SetupOutboundMessage(hawb.Messages, "101");
			var inboundMessage = SetupInboundMessage(inboundControlText, sentMessage);
			mawb.Factory.Save();
			Factory.Save();

			RunProcessors();
			sentMessage.Reload();
			inboundMessage.Reload();
			hawb.Messages.Load();

			AssertEquals("Hawb message count", 2, hawb.Messages.Count);
			AssertEquals(sentMessage.PK, hawb.Messages[0].PK);
			AssertEquals(inboundMessage.PK, hawb.Messages[1].PK);
			AssertEquals("ACK", sentMessage.EM_Status);
			AssertEquals(EDIMessage.Schema.EM_Status, "RCV", inboundMessage.EM_Status);
			AssertContains(@"<html> <h3>CONTRL</h3>
<b>
MESSAGE STORED FOR LATER TRANSMISSION.
</b>
<p>Outgoing message #101 (FOO) was acknowledged</p>
</html>", inboundMessage.EM_MessageInterpretation);
		}

		[TestDate(2023, 1, 1)]
		public void TestRetryProcessingIncomingMessage()
		{
			var retryIntervalMinutes = (int)CargoFactMessageProcessor.RetryInterval.TotalMinutes;

			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
			var interchange1 = Factory.NewMoq<EDIInterchange>().Object;
			interchange1.EI_From = "CCSUK";
			interchange1.EI_To = "WISETECHGLOBAL";
			interchange1.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message1 = Factory.NewMoq<EDIMessage>().Object;
			message1.EM_EI = interchange1.PK;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = cim;
			message1.EM_MessageNum = "1001";
			Factory.Save();

			for (var i = 0; i < CargoFactMessageProcessor.MaxRetryAttempts; i++)
			{
				RunProcessors();
				message1.Reload();
				AssertContains("CargoFact processor: Could not find or process consignment for message number 1001 but will retry", log[log.Count - 1]);
				AssertEquals("CIM", message1.EM_MessageType);
				AssertEquals("FSN", message1.EM_MessageSubType);
				AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals("Message held date", ZDateTime.Now.AddMinutes(retryIntervalMinutes), message1.EM_HeldUntilDate);
				TestDateAttribute.AddMinutes(retryIntervalMinutes);
			}

			RunProcessors();
			message1.Reload();
			AssertContains("CargoFact processor: Could not find or process consignment for message number 1001", log[log.Count - 1]);
			AssertNotContains("but will retry", log[log.Count - 1]);
			AssertEquals("CIM", message1.EM_MessageType);
			AssertEquals("FSN", message1.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Error, message1.EM_Status);

			var interchange2 = Factory.NewMoq<EDIInterchange>().Object;
			interchange2.EI_From = "CCSUK";
			interchange2.EI_To = "CUKAIR98LHRVIO";
			interchange2.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.NewMoq<EDIMessage>().Object;
			message2.EM_EI = interchange2.PK;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = cim;
			message2.EM_MessageNum = "1002";
			Factory.Save();

			for (var i = 0; i < CargoFactMessageProcessor.MaxRetryAttempts; i++)
			{
				RunProcessors();
				message2.Reload();
				AssertContains("CargoFact processor: Could not find or process consignment for message number 1002 but will retry", log[log.Count - 1]);
				AssertEquals("CIM", message2.EM_MessageType);
				AssertEquals("FSN", message2.EM_MessageSubType);
				AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
				AssertEquals("Message held date", ZDateTime.Now.AddMinutes(retryIntervalMinutes), message2.EM_HeldUntilDate);
				TestDateAttribute.AddMinutes(retryIntervalMinutes);
			}

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "050-42011002";
			mawb.Profile = "CUKAIR98LHRVIO";
			Factory.Save();

			RunProcessors();
			message2.Reload();
			AssertContains("Parse CIM FSN for consignment 050-42011002, message number 1002", log[log.Count - 1]);
			AssertEquals("CIM", message2.EM_MessageType);
			AssertEquals("FSN", message2.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Received, message2.EM_Status);
		}

		EDIMessage SetupOutboundMessage(BusinessObjectCollection boCollection, string messageNumber = "msgNum")
		{
			var message = Factory.New<DummyEDIMessage_CcsukInterchangePackagerServiceTaskTest>();
			message.GetMessageReferenceNumberReturns = messageNumber;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			if (boCollection != null)
			{
				boCollection.Add(message);
			}
			message.EM_MessageType = "FOO";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageText = "<<MSGNO PLACEHOLDER>> outbound";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			return message;
		}

		EDIMessage SetupInboundMessage(string inboundText, BusinessObject outboundObjectForPK)
		{
			var message = Factory.New<DummyEDIMessage_CcsukInterchangePackagerServiceTaskTest>();
			message.GetMessageReferenceNumberReturns = "msgNum";
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(inboundText, outboundObjectForPK);
			return message;
		}

		void SetupNewOutboundMessageForPimaTestsAndRunTask(string applicationReference, CcsukTransmissionMessageFunction how, BusinessObjectCollection linkedObjectMessages, string expectedRecipient, string expectedErrorMessageInLogBecauseCouldNotSend = null)
		{
			var msg = SetupOutboundMessage(linkedObjectMessages);
			msg.EM_ApplicationReference = applicationReference;
			msg.EM_MessageType = how.MessageType;
			msg.EM_MessageSubType = how.MessageSubType;
			msg.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			RunProcessors(runSenders: true, clearLog: true);

			msg.Reload();
			var ediInterchange = msg.Interchange;
			AssertEquals("PND", msg.EM_Status);
			AssertNotNull(ediInterchange);
			AssertEquals("Recipient PIMA in EI_To for message of type " + how.MessageSubType, expectedRecipient, ediInterchange.EI_To);

			if (expectedErrorMessageInLogBecauseCouldNotSend != null)
			{
				AssertContains(expectedErrorMessageInLogBecauseCouldNotSend, log[1]);
				AssertEquals("BGB-CUK-IntProviderCannotGetRecipientPima", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		void SetupCredentials(string mnemonic)
		{
			var badge = new BadgeCodeSetting();
			var cred = new CredentialsSetting();
			badge.BadgeCode = mnemonic;
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			cred.BadgeCode = badge.BadgeCode;
			cred.Company = "DEF";
			cred.Printer = "CUKDANIEL";
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.Value;
			badges.Add(badge);
			var creds = GBCustomsDataRegistry.Instance.Credentials.Value;
			creds.Add(cred);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);
		}

		protected override void SetUp()
		{
			base.SetUp();
			branchEnvironment = DisposableEnvironment.ForBranch(Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		class FakeFunctionForTest : CcsukTransmissionMessageFunction
		{
			public override string MessageSubType
			{
				get { return "XXX"; }
			}
			public override string MessageType
			{
				get { return "YYY"; }
			}
		}

		sealed class DummyEDIMessage_CcsukInterchangePackagerServiceTaskTest : EDIMessage
		{
			public DummyEDIMessage_CcsukInterchangePackagerServiceTaskTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

			protected override string GetMessageReferenceNumber()
			{
				return GetMessageReferenceNumberReturns;
			}
		}
	}
}
