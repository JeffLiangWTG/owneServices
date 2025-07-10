using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq.Protected;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;
using JobDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.Chief.CusRes.Testing
{
	public sealed class CusResResponseProcessor_Contrl_TEST : CusResAndDtiResponseProcessorTest
	{
		public void TestParseControlWithNoResponseMsgIdNumber()
		{
			var contrlWithNoTestParseControlWithNoResponseMsgIdNumber = "UNH+CONTRL0017122233+CONTRL:2:2:UN'UCI++++4+'UNT+3+CONTRL0017122233',";
			SetupButDoNotRun(contrlWithNoTestParseControlWithNoResponseMsgIdNumber, "XYZ", "Anything");
			Factory.Save();
			AssertNoExceptionThrown(() => RunAndReload());
		}

		public void TestParseControlWithDuffCAR()
		{
			// Common access reference present but it is not a GUID
			var receivedText = "UNH+00477075335000+CONTRL:1:912:UN+Pooooopy'UCI+CUKFFW98000CAR:IATA+CUKCTM98CHFEXP:IATA+16859+1'UCM+16950+CUSDEC:D:04A:UN:IATA01'UCX+1'FTX+AAA+++MESSAGE STORED FOR LATER TRANSMISSION.'UNT+6+00477075335000'";
			SetupButDoNotRun(receivedText, "XYZ", "Anything");
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "16859";
			outgoingInterchange.EI_ReceiveTransmit = "TRX";
			outgoingInterchange.EI_ApplicationCode = "ABC";
			originalOutboundEdiMessage.EM_EI = outgoingInterchange.PK;
			Factory.Save();
			RunAndReload();
			AssertEquals(receivedText, entry.Messages.LastIncomingMessage.EM_MessageText);
		}

		[TestDate(2008, 12, 11)]
		public void TestCnsContrlRejectionWithFreetextTwo()
		{
			string inbound = @"UNB+UNOA:2+CNSCHIEFEDI+CNSJJB::CNSJJB+090914:1627+41627169960100++CHIEFLIVE++++'UNH+09437188306724+CONTRL:4:1:UN+<<SYSCAR>>'UCI+DTICHIEFEDI+CNSAAW+CHIEF+4'UCM+2343233+CUSDEC:D:04A:UN:109700+4'UCS+11'UCD+10+2:2'UCS+22+3'UCD+6+3'UNT+8+09437188306724'UNZ+1+41627169960100'";
			DoGenericTestingforAllScenarios(inbound, ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, cnsOutbound);
			AssertEquals("1 email", 1, Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("<td>BADMUCR</td><td>RFF+UCN:BADMUCR</td><td>Seg 11 (RFF) 2:2</td>"));
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("<td>DANIEL</td><td>PCI++DANIEL+EN</td><td>Seg 22 (PCI) 3:</td></tr>"));
		}

		[TestDate(2008, 12, 11)]
		public void TestMcpContrlRejectionWithProperGroupTwos()
		{
			string inbound = @"UNA\#.? {UNB#UNOA\1#FCPSYS#FCPCAW\\#090915\1334#090915133455687{UNH#09091513343130#CONTRL\4\1\UN#<<SYSCAR>>{UCI#DTICHIEFEDI#FCPCAW#CHIEF#4{UCM#2343233#CUSDEC\D\04A\UN\109780#4{UCS#4#4{UCD#4#3\1{UNT#6#09091513343130{UNZ#1#090915133455687{";
			SetupButDoNotRun(inbound, ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly, mcpOutbound);
			EDIInterchange outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_HeaderText = @"UNA\#.? {UNB####090928\1341#22{"; // just needed so that we can work out the char set of the outgoing message so that we can dissect it. 
			outgoingInterchange.EI_From = "X";
			outgoingInterchange.EI_To = "Y";
			outgoingInterchange.ContainedMessages.Add(originalOutboundEdiMessage);

			Factory.Save();
			RunAndReload();

			AssertEquals("1 email", 1, Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("The following <font color='red'>CONTRL rejection</font> message was received from CHIEF."));
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("<tr><td>4 - Service segment missing/invalid</td><td>GBLBA</td><td>LOC+9+GBLBA</td><td>Seg 4 (LOC) 3:1</td></tr>"));

			AssertEquals("Cus entry header entry status should be blank if not initially AWR", "", entry.CH_EntryStatus);
			AssertEquals("Cus entry header message status should be REJ", MessageStatusList.Codes.SentAndRejected, entry.CH_Status);
			AssertEquals("Status of original outbound cusdec message", EDIMessage.Status.Rejected, this.originalOutboundEdiMessage.EM_Status);
		}

		public void TestCnsContrlRejectionWithFreetext()
		{
			// This tests we can understand 912:1 as well as D:04A, and that we can understand both segment-referenced CONTRLs an freetext errors
			string fullReceivedInterchangeString = @"UNB+UNOA:2+CNSCHIEFEDI+CNSJJB::CNSJJB+090914:1627+41627169960100++CHIEFLIVE++++'UNH+41627169960100+CONTRL:1:912:UN+<<SYSCAR>>'UCI+2+CNSCHIEFEDI::21+CNSJJB+4'UCM+2343233+CUSDEC:D:04A:UN:109730+4+C3'FTX+AAI+++CNS CCMI authorisation failure. User not authorised for Agent role'UNT+5+41627169960100'UNZ+1+41627169960100'";
			SetupButDoNotRun(fullReceivedInterchangeString, ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, cnsOutbound);
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			Factory.Save();
			RunAndReload();
			AssertEquals("1 email", 1, Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("CNS CCMI authorisation failure. User not authorised for Agent role"));
			entry.Reload();
			AssertEquals(EntryStatusList.Codes.SentAndInitiallyRejected, entry.CH_EntryStatus);
			AssertEquals(MessageStatusList.Codes.SentAndRejected, entry.CH_Status);
		}

		EDIMessage receivedEdiMessage;
		EDIMessage originalOutboundEdiMessage;
		CusEntryHeader entry;
		readonly string cnsOutbound = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109730+23BBA0EDC2824D89A3209469F23327A0'BGM+EFD::109++9'CST++EXD+2'LOC+9+GBLBA'LOC+35+GB'LOC+36+HK'LOC+14+::109'GEI+5+ACC:PI:109'SEL+SEAL1++EN'RFF+ABO:9GB545733236000-B00001120:'RFF+UCN:BadMucr'TDT+13++1+++++:::MCP:AW'NAD+CZ+GB945390992000++CARGOWISE EDI (UK) LTD+314 MIDSUMMER+MILTON KEYNES+EN+MK9+GB'NAD+CN+++BAND POWER LIMITED+FLAT 915 9/F 2 KAI HING ROAD+HONG KONG+EN+NA+HK'NAD+DT+GB545733236000++ELITE GROUP LOGISTICS LIMITED+WORTLEY MOOR ROAD+LEEDS+EN+LS12 4JH+GB'UNS+D'DMS+B00001120/545454'CST++1000001+09023000'LOC+27+GB'MEA+AAR++KGM:1'PAC+1++CG'PCI++Daniel+EN'MOA+123:1'RFF+AAQ:DANU1234567'RFF+ZZZ'IMD+++:::TEA AND COFFEE::EN'DOC+998:::380+DAN INV1:::EN::Z'UNS+S'CNT+11:6'UNT+30+<<MSGNO PLACEHOLDER>>'";
		readonly string mcpOutbound = @"UNH#<<MSGNO PLACEHOLDER>>#CUSDEC\D\04A\UN\109730#23BBA0EDC2824D89A3209469F23327A0{BGM#EFD\\109##9{CST##EXD#2{LOC#9#GBLBA{LOC#35#GB{LOC#36#HK{LOC#14#\\109{GEI#5#ACC\PI\109{SEL#SEAL1##EN{RFF#ABO\9GB545733236000-B00001120\{RFF#UCN\BadMucr{TDT#13##1#####\\\MCP\AW{NAD#CZ#GB945390992000##CARGOWISE EDI (UK) LTD#314 MIDSUMMER#MILTON KEYNES#EN#MK9#GB{NAD#CN###BAND POWER LIMITED#FLAT 915 9/F 2 KAI HING ROAD#HONG KONG#EN#NA#HK{NAD#DT#GB545733236000##ELITE GROUP LOGISTICS LIMITED#WORTLEY MOOR ROAD#LEEDS#EN#LS12 4JH#GB{UNS#D{DMS#B00001120/545454{CST##1000001#09023000{LOC#27#GB{MEA#AAR##KGM\1{PAC#1##CG{PCI##Daniel#EN{MOA#123\1{RFF#AAQ\DANU1234567{RFF#ZZZ{IMD###\\\TEA AND COFFEE\\EN{DOC#998\\\380#DAN INV1\\\EN\\Z{UNS#S{CNT#11\6{UNT#30#<<MSGNO PLACEHOLDER>>{";

		void SetupEmails()
		{
			GlbStaff mcpMessageStaff;
			GlbGroup groupZZ1;
			GlbStaff staffZ1;
			GlbStaff staffZ2;

			groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dan@pretend.email.com";

			staffZ2 = groupZZ1.Staff.AddNew();
			staffZ2.GS_Code = "Z2";
			staffZ2.GS_LoginName = "z2";
			staffZ2.GS_EmailAddress = "dan@pretend.email.com";

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			mcpMessageStaff = group.Staff.AddNew();
			mcpMessageStaff.GS_Code = "ZAC";
			mcpMessageStaff.GS_LoginName = "~2";
			mcpMessageStaff.GS_EmailAddress = "postmaster@pretendemail.com";
			Factory.Save();

			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		public static string ReplaceSysCarPlaceholdWithEntryPkForTest(CusEntryHeader cusEntry, string fullReceivedInterchangeString)
		{
			fullReceivedInterchangeString = fullReceivedInterchangeString.Replace("<<SYSCAR>>", cusEntry.PK.ToString().Replace("-", ""));
			return fullReceivedInterchangeString;
		}

		[TestDate(2008, 12, 11)]
		void DoGenericTestingforAllScenarios(string receivedText, string appCodeThatCreatedOutboundMessage, string textOfOutgoingEdiMessage)
		{
			SetupButDoNotRun(receivedText, appCodeThatCreatedOutboundMessage, textOfOutgoingEdiMessage);
			Factory.Save();
			RunAndReload();
		}

		public void TestInboundChiefContrlErrors()
		{
			//One UCM error and multiple [Group2] errors. Where [Group2] contains one UCS error and multiple UCD errors
			string inboundChiefContrlUcsUcdError = @"UNA\#.? {UNB#UNOA\1#FCPSYS#FCPCAW\\FCPCAW#120214\1000#120214100049219{UNH#10319194172541#CONTRL\4\1\UN#<<SYSCAR>>{UCI#DTICHIEFEDI#CNSAAW#CHIEF#4{UCM#10045#CUSDEC\D\04A\UN\109701#4#10#UNH#1\2{UCS#1#3{UCD#4#1\2{UCD#5#2\1{UCS#2#6{UCD#7#1{UCD#8#2\1{UNT#8#10319194172541{UNZ#1#23{";
			setupContrlDataElement0085Error(inboundChiefContrlUcsUcdError);
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("received from CHIEF"));
			Assert("UCM Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("10 - Data element attribute error"));
			Assert("UCD Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("4 - Service segment missing/invalid"));
			Assert("UCD Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("5 - Trailer count error 0074 in UNT"));
			Assert("UCD Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("7 - Recipient identification "));
			Assert("UCD Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("8 - Too many data elements"));
		}

		public void TestInboundEDCSContrlErrors()
		{
			//One UCI error and multiple UCM errors
			string inboundEDCSContrlUcmError = @"UNA\#.? {UNB#UNOA\2#EDRCHIEF#CUK98000DAT\\CUK000DAT#050731\1017#23##CHIEFLIVE{UNH#10319209133733#CONTRL\2\2\UN#<<SYSCAR>>{UCI#77#CUK98000DAT\\CUK000DAT#EDRCHIEF#4#G49#1#2{UCM#00001#CUSDEC\2\912\UN\109210#4#13#UNH{UCM#00001#CUSDEC\2\912\UN\109210#4#G47{UNT#3#120824132215{UNZ#1#23{";
			setupContrlDataElement0085Error(inboundEDCSContrlUcmError);
			Assert(Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("received from EDRCHIEF"));
			Assert("UCI Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("G49 - One or more messages have been rejected. The response interchange contains"));
			Assert("UCM Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("13 - Missing interchange segment"));
			Assert("UCM Error", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("G47 - Message too long"));
		}

		void setupContrlDataElement0085Error(ZString contrlMessage)
		{
			SetupButDoNotRun(contrlMessage, ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, cnsOutbound);
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			Factory.Save();
			RunAndReload();
			entry.Reload();
		}

		void RunAndReload()
		{
			RunProcessor();
			receivedEdiMessage.Reload();
			originalOutboundEdiMessage.Reload();
			entry.Reload();
			//AssertEquals("Status of on-hand received message", EDIMessage.Status.Received, receivedEdiMessage.EM_Status);
		}

		void SetupButDoNotRun(string receivedText, string appCodeThatCreatedOutboundMessage, string textOfOutgoingEdiMessage)
		{
			SetupEmails();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";

			this.entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			this.entry.EntryNumber = "123456";
			this.entry.CH_MessageType = ApplicationCodeList.Codes.GbEdifactShared;
			this.entry.CH_Status = "QUE";
			CusEntryLine line = this.entry.MergedLines.AddNew();
			line.CL_LineNumber = 1;

			if (receivedText.StartsWith("UNH"))
			{
				receivedEdiMessage = Factory.New<GbEDIMessage>();
				receivedEdiMessage.EM_MessageText = ReplaceSysCarPlaceholdWithEntryPkForTest(entry, receivedText);
				receivedEdiMessage.MessageNumberStrategy = new Business.GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbEdifactShared);
				receivedEdiMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.GbEdifactShared;
				receivedEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			}
			else
			{
				EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(this.Factory, ReplaceSysCarPlaceholdWithEntryPkForTest(entry, receivedText), EDIInterchange.ApplicationCodes.GbEdifactShared);
				receivedEdiMessage = interchange.ContainedMessages[0];
			}

			var mockOriginalCusdecMessage = Factory.NewMoq<EDIMessageDummyForTest_2343233>();
			mockOriginalCusdecMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2343233");
			this.originalOutboundEdiMessage = mockOriginalCusdecMessage.Object;
			this.originalOutboundEdiMessage.EM_ApplicationCode = appCodeThatCreatedOutboundMessage;
			this.originalOutboundEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			this.originalOutboundEdiMessage.EM_LinkedObject = entry;
			this.originalOutboundEdiMessage.EM_MessageText = textOfOutgoingEdiMessage;
			this.originalOutboundEdiMessage.EM_ApplicationReference = "8-B00001000";
		}
	}
	public class EDIMessageDummyForTest_2343233 : EDIMessage
	{
		public EDIMessageDummyForTest_2343233(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "2343233";
		}
	}
}
