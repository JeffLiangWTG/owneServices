using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestDeleteManuallyEnteredCanNumberForShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var manuallyEnteredInThisCountry1 = Factory.New<CusEntryNumber>();
			manuallyEnteredInThisCountry1.CE_EntryIsSystemGenerated = false;
			manuallyEnteredInThisCountry1.CE_ParentID = shipment.PK;
			manuallyEnteredInThisCountry1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			manuallyEnteredInThisCountry1.CE_EntryNum = "AAABBB111";
			manuallyEnteredInThisCountry1.CE_EntryType = CusEntryNumberTypes.Australia.CAN;

			var manuallyEnteredInThisCountry2 = Factory.New<CusEntryNumber>();
			manuallyEnteredInThisCountry2.CE_EntryIsSystemGenerated = false;
			manuallyEnteredInThisCountry2.CE_ParentID = shipment.PK;
			manuallyEnteredInThisCountry2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			manuallyEnteredInThisCountry2.CE_EntryNum = "AAABBB000";
			manuallyEnteredInThisCountry2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;

			var manuallyEnteredInOtherCountry = Factory.New<CusEntryNumber>();
			manuallyEnteredInOtherCountry.CE_EntryIsSystemGenerated = false;
			manuallyEnteredInOtherCountry.CE_ParentID = shipment.PK;
			manuallyEnteredInOtherCountry.CE_RN_NKCountryCode = "NZ";
			manuallyEnteredInOtherCountry.CE_EntryNum = "AAABBB222";

			declaration.JE_JS = shipment.PK;

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRClearMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Declaration Entry Number", "AAAAAJJN7", declaration.DeclarationNumber);
			var logEvent = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CES"));
			AssertEquals("Should have CES log event", "AAAAAJJN7", logEvent[0].SL_Reference);
			AssertEquals("ManuallyEnteredInOtherCountry stays", false, manuallyEnteredInOtherCountry.IsDeleted);
			AssertEquals("ManuallyEnteredInThisCountry1 deleted", true, manuallyEnteredInThisCountry1.IsDeleted);
			AssertEquals("ManuallyEnteredInThisCountry2 stays", false, manuallyEnteredInThisCountry2.IsDeleted);
		}

		public void TestProcessResponseWhenEntryHeaderExists()
		{
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var outboundMessage = entryHeader.Messages.AddNew(typeof(CMREXDMessage));
			outboundMessage.EM_MessageSubType = "ORG";
			outboundMessage.EM_Status = EDIMessage.Status.Sent;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRClearMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Response message is linked with Entry Header", entryHeader, incomingMessage.EM_LinkedObject);
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
			AssertEquals("No warning about declaration not found", 0, logger.Logs.Count(x => x.Type == Integration.LogType.Warning && x.Message.Contains("The incoming message is not responding to a declaration. Can't continue.")));
		}

		public void TestClearResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRClearMessage.txt")).Replace("\r\n", "");
			StmALog mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull("Customs Cleared log not added", mostRecentLog);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
			mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared);
			AssertNotNull("Customs Cleared log added now", mostRecentLog);
		}

		public void TestErrorResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRErrorMessage.txt")).Replace("\r\n", "");
			StmALog mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived);
			AssertNull("CustomsImpedementLog not added", mostRecentLog);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.ErrorOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived);
			AssertNotNull("CustomsImpedementLog added now", mostRecentLog);
		}

		public void TestRejectionResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRRejectionMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.FailOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		public void TestRejectionForReplacement()
		{
			outgoingMessage.EM_MessageSubType = "AMD";
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRRejectionMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.FailReplacement.Code, declaration.JE_EntryStatus);
		}

		public void TestBlankCANIfWithdrawn()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRClearMessage.txt")).Replace("\r\n", "").Replace("CLEAR", "WITHDRAWN");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals("DeclarationNumber", ZString.Empty, declaration.DeclarationNumber);
		}

		public void TestWithdrawResponseWhenEntryHeaderExists()
		{
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXDRClearMessage.txt")).Replace("\r\n", "").Replace("CLEAR", "WITHDRAWN");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals("EntryHeaderStatus", CustomsEntryStatus.ClearWithdrawal.Code, entryHeader.CH_Status);
		}

		public void TestIntermittantStatusUpdateFail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S10033087";
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = "S10033087";
			outgoingMessage.EM_MessageText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+830:::EXD+S10033087/ME21:1+9'
LOC+9+AUMEL::6'
LOC+12+MYTPP::6'
LOC+28+VN::6'
DTM+129:20181023:102'
GIS+N:79:95'
GIS+N:107:95'
GIS+N:141:95'
RFF+AWH:A'
PAC+++C:67:95'
PAC+++OT:146:95'
TDT+20+++11'
NAD+CN+++DELPHI INDUSTRY CO., LTD++HO CHI MINH CITY'
NAD+GO+62004586690/001::95'
MOA+39::AUD'
MOA+63:48849:AUD'
UNS+D'
CST+1+I::95'
FTX+AAA+++ADHESIVES BASED ON RUBBER OR PLASTICS (INCL. ARTIFICIAL RESINS)'
LOC+27++AU-VI::6'
MEA+WT++KG:10296'
MEA+ABW++KG:9708'
MOA+63:48849'
RFF+HS:35069100'
UNS+S'
CNT+11:0'
CNT+36:1'
UNT+29+1'".Replace("\r\n", "");
			incomingMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXDR+2601 E47J HH18:001+11'
FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'
NAD+MR+33118455566::95'
RFF+ABO:S10033087/ME21::001'
RFF+ACW:EXD'
RFF+AFM:9'
RFF+ED:AC7KJ649P'
CNT+5:0001'
UNT+10+000001'".Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "CMR";
			interchange.EI_From = "FGX776L";
			interchange.EI_To = "AAA336C";
			interchange.EI_InterchangeNum = "496884";

			var responseControlMessage = Factory.New<EDIMessage>();
			responseControlMessage.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseControlMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseControlMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseControlMessage.EM_Status = EDIMessage.Status.Queued;
			responseControlMessage.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+496884+FGX776L::FGX776L+AAA336C+7'UCM+1+CUSDEC:D:99B:UN+7'UNT+4+000001'";

			var ctlProcessor = new CMRCTLMessageProcessor(new LoggingInformation());
			ctlProcessor.ProcessMessage(responseControlMessage);
			AssertEquals("", EDIMessage.Status.Received, responseControlMessage.EM_Status);
			AssertEquals("EntryStatus should not be affected by processing CONTRL message out of sequence", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.EXDR;

		protected override ZString GetExpectedMessageName() => "Export Declaration Response(EXDR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00111733";
			declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			outgoingMessage = declaration.Messages.AddNew(typeof(CMREXDMessage));
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			processor = new TestHelperEXDRMessageProcessor(logger);
		}

		protected override Type IncomingMessageType => typeof(CMREXDRMessage);

		TestHelperEXDRMessageProcessor processor;
		JobDeclaration declaration;

		sealed class TestHelperEXDRMessageProcessor : EXDRMessageProcessor
		{
			public TestHelperEXDRMessageProcessor(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
