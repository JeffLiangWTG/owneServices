using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RFPMessage))]
	public class RFPMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("Application Code", EDIMessage.ApplicationCodes.EXDOC, message.EM_ApplicationCode);
			AssertEquals("Link Table Name", QuarantineExDocHeader.Schema.TableName, message.EM_LinkTable);
			AssertEquals("TestMessage", false, message.EM_IsTestMessage);
		}

		public void TestMessageNumberAllocation()
		{
			var message1 = message;
			Assert("PreCondition Empty Message Number", message1.EM_MessageNum.IsEmpty);
			message1.EM_MessageText = ExampleMessage;
			Factory.Save();
			Assert("First Message: Message Number not Empty", !message1.EM_MessageNum.IsEmpty);
			var message2 = Factory.New<RFPMessage>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineExDocHeader2 = declaration2.Invoices.AddNew().QuarantineExDocHeader;
			message2.EM_LinkedObject = quarantineExDocHeader2;
			message2.EM_MessageText = ExampleMessage;
			Factory.Save();
			int messageNumber1 = int.Parse(message1.EM_MessageNum);
			int messageNumber2 = int.Parse(message2.EM_MessageNum);
			Assert("Message number comparison", messageNumber1 < messageNumber2);
		}

		public void TestRemovalOfMessageNumberPlaceHolder()
		{
			Assert("PreCondition Empty Message Number", message.EM_MessageNum.IsEmpty);
			message.EM_MessageText = ExampleMessage;
			Assert("PreCondition Place Holder Exists in Message Text", message.EM_MessageText.IndexOf(EDIMessage.MessageNumberPlaceHolder) > -1);
			Factory.Save();
			Assert("Message Number Place Holder is Removed", message.EM_MessageText.IndexOf(EDIMessage.MessageNumberPlaceHolder) == -1);
		}

		public void TestRemovalOfSendersReferencePlaceHolder()
		{
			Assert("PreCondition Empty Message Number", message.EM_MessageNum.IsEmpty);
			message.EM_MessageText = ExampleMessage;
			Assert("PreCondition Place Holder Exists in Message Text", message.EM_MessageText.IndexOf(EDIMessage.SendersReferencePlaceHolder) > -1);
			Factory.Save();
			Assert("Message Number Place Holder is Removed", message.EM_MessageText.IndexOf(EDIMessage.SendersReferencePlaceHolder) == -1);
		}

		public void TestRemovalOfAuthorisingOfficerID()
		{
			message.EM_MessageText = ExampleMessage;
			Assert("Pre-Condition Authorising Officer ID is there", message.EM_MessageText.Contains("GRAHB"));
			Assert("Formatted message text doesn't have authorising officer", !message.EM_FormattedMessageText.Contains("GRAHB"));
			Assert("Formatted message text contains ****", message.EM_FormattedMessageText.Contains("******"));
			AssertMultilineASCIIEquals("Formatted Message is", FormattedMessage, message.EM_FormattedMessageText);
			AssertMultilineASCIIEquals("Interpreted Message is", FormattedMessage, message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<RFPMessage>();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			message.EM_LinkedObject = helper.Header1.QuarantineExDocHeader;
		}
		RFPMessage message;

		const string ExampleMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+9+BNE'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'LOC+91+SYD'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'MEA+TE+ADE+CEL:-2.75'GIS+A::AQ:PHC'GIS+Y::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+Y::AQ:ACS'PNA+EX+1000'PNA+CN+++++10:KWING KWONG'ADR++5:456 HIGH ST+TAIPEI+654321+TW+:::TWSTATE'TDT+12+V123+1+++++:::ADMIRALENGRACHT'DTM+136:20040130:102'PRC+IN:PP:AQ'PNA+FO+10004'PNA+AV+GRAHB'LIN+1'MEA+AAA+SQ+KGM:1456.000'PIA+5+XCA BP:CC'PIA+5+1000:BP'PIA+5+99999998:HS'IMD+++IN:::BEEF'IMD+++UHC:::BEEF'MOA+63:425.00'PAC+50+3+CT::AQ'PCI++NM/A1234/ENDV'EQD+CN+MARU2103333'SEL+654321'PRC+SL:PP:AQ'DTM+194:20040101:102'DTM+206:20040105:102'PNA+MP+780'PRC+PK:PP:AQ'DTM+194:20040108:102'DTM+206:20040108:102'PNA+MP+1004'UNT+47+<<MSGNO PLACEHOLDER>>'";
		const string FormattedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801
BGM+M::AQ:9++13
LOC+9+BNE
LOC+12+TWTPE
LOC+8+TAIPEI
LOC+36+TW
LOC+30+AU
LOC+91+SYD
RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>
RFF+AAE:AAEETF6AL
MEA+TE+ADE+CEL:-2.75
GIS+A::AQ:PHC
GIS+Y::AQ:SC
GIS+N::AQ:SM
GIS+N::AQ:SP
GIS+N::AQ:SST
GIS+N::AQ:QI
GIS+Y::AQ:ACS
PNA+EX+1000
PNA+CN+++++10:KWING KWONG
ADR++5:456 HIGH ST+TAIPEI+654321+TW+:::TWSTATE
TDT+12+V123+1+++++:::ADMIRALENGRACHT
DTM+136:20040130:102
PRC+IN:PP:AQ
PNA+FO+10004
PNA+AV+******
LIN+1
MEA+AAA+SQ+KGM:1456.000
PIA+5+XCA BP:CC
PIA+5+1000:BP
PIA+5+99999998:HS
IMD+++IN:::BEEF
IMD+++UHC:::BEEF
MOA+63:425.00
PAC+50+3+CT::AQ
PCI++NM/A1234/ENDV
EQD+CN+MARU2103333
SEL+654321
PRC+SL:PP:AQ
DTM+194:20040101:102
DTM+206:20040105:102
PNA+MP+780
PRC+PK:PP:AQ
DTM+194:20040108:102
DTM+206:20040108:102
PNA+MP+1004
UNT+47+<<MSGNO PLACEHOLDER>>";
	}
}
