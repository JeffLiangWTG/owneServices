using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.CusRes.Testing
{
	sealed class CusResResponseProcessor_UkCtrl_TEST : CusResAndDtiResponseProcessorTest
	{
		[TestDate(2008, 12, 11)]
		public void TestUkCtrlForConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var mockOutboundEdiMessage = Factory.NewMoq<EDIMessageDummyForTest_84>();
			mockOutboundEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("84");
			var sentMessage = mockOutboundEdiMessage.Object;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.EM_ReceiveTransmit = "TRX";
			sentMessage.EM_MessageType = GbDes242MessageFunction.Eac;
			sentMessage.EM_MessageSubType = GbCusDecMessageFunctionsList.Codes.Close;
			consol.Messages.Add(sentMessage);
			var interchangeString = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder("UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+10018712328708+UKCTRL:1:912:UK:109100'UCM+1678:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+1'UNT+4+10018712328708'UNZ+1+1024C163353000'", sentMessage);
			var receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString, EDIMessage.ApplicationCodes.GbEdifactShared);
			Factory.Save();
			RunProcessor();
			var consolReloadedWowOurCaching = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consolReloadedWowOurCaching.Messages.Load();
			AssertEquals(2, consolReloadedWowOurCaching.Messages.Count);
			var incomingMessage = consolReloadedWowOurCaching.Messages.LastIncomingMessage;
			AssertEquals(receivedInterchange.ContainedMessages[0].PK, incomingMessage.PK);
			AssertEquals("UKCTRL", incomingMessage.EM_ApplicationReference);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consolReloadedWowOurCaching, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("A positive ACK to a CLS request should close the consol", true, wrapper.MawbExportHelper.ME_ChiefConsolIsClosed);
			AssertEquals("ACK", incomingMessage.EM_MessageType);
		}

		[TestDate(2008, 12, 11)]
		public void TestUkCtrlErrorForConsolForE3481ActuallyClosesConsol()
		{
			RunUkctrlErrorForConsol("UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+11248939652460+UKCTRL:1:912:UK:109100'UCM+8091:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+4+54'UCR+0+54'UCD+0+54'FTX+AAO+++E10003 ERRORS ON DOCUMENT'UCR+3'UCD+2:2+6'FTX+AAO+++E3481 MUCR IS ALREADY SHUT'UNT+10+11248939652460'UNZ+1+1024C163353000'",
				new[] { "MUCR IS ALREADY SHUT", "Note, processing of this message has ensured that consol C0000123 shows as closed" },
				true
				);
		}

		[TestDate(2008, 12, 11)]
		public void TestUkCtrlErrorInterpretationShowsDUCR()
		{
			RunUkctrlErrorForConsol(ukCtrlResponse1ForDucr, new[] { "E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST", "Cited UCR: 0GB896458895023-B00031317" }, null, ukCinvRequestForUcrTest);
		}

		[TestDate(2008, 12, 11)]
		public void TestUkCtrlErrorInterpretationShowsMUCR()
		{
			RunUkctrlErrorForConsol(ukCtrlResponse2ForMucr, new[] { "E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST", "Cited UCR: A:00012345678" }, null, ukCinvRequestForUcrTest);
		}

		[TestDate(2008, 12, 11)]
		public void TestUkCtrlErrorInterpretationWithIndexOutOfRangeOnSegmentIndex()
		{
			RunUkctrlErrorForConsol(ukCtrlResponse3ForError, new[] { "E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST" }, null, ukCinvRequestForUcrTest); // message still attached to consol even if it coudl not have its MUCR extracted due to out-of-range segment ID
		}

		// Line 'em up so you can see their differences --->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->-here--V
		const string ukCinvRequestForUcrTest = @"UNH+21233+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAC:105:109'RFF+ABO:0GB896458895023-B00031317:'RFF+UCN:A?:00012345678'UNS+D'UNS+S'UNT+7+21233'";
		const string ukCtrlResponse1ForDucr = @"UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+12654572125412+UKCTRL:1:912:UK:109100'UCM+21233:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+4+54'UCR+0+54'UCD+0+54'FTX+AAO+++E10003 ERRORS ON DOCUMENT'UCR+3'UCD+2:2+6'FTX+AAO+++E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST'UNT+10+12654572125412'UNZ+1+1024C163353000'";
		const string ukCtrlResponse2ForMucr = @"UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+12654572125412+UKCTRL:1:912:UK:109100'UCM+21233:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+4+54'UCR+0+54'UCD+0+54'FTX+AAO+++E10003 ERRORS ON DOCUMENT'UCR+4'UCD+2:2+6'FTX+AAO+++E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST'UNT+10+12654572125412'UNZ+1+1024C163353000'";
		const string ukCtrlResponse3ForError = @"UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+12654572125412+UKCTRL:1:912:UK:109100'UCM+21233:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+4+54'UCR+0+54'UCD+0+54'FTX+AAO+++E10003 ERRORS ON DOCUMENT'UCR+99'UCD+2:2+6'FTX+AAO+++E408 UNIQUE CONSIGNMENT REFERENCE DOES NOT EXIST'UNT+10+12654572125412'UNZ+1+1024C163353000'";

		[TestDate(2008, 12, 11)]
		public void TestUkCtrlErrorForConsol()
		{
			RunUkctrlErrorForConsol("UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKAIR98LHRCAX:IATA+110930:1633+1024C163353000+++A'UNH+11248939652460+UKCTRL:1:912:UK:109100'UCM+8091:<<SYSCAR>>+UKCINV:D:00A:UN:109001'UCX+4+54'UCR+0+54'UCD+0+54'FTX+AAO+++E10003 ERRORS ON DOCUMENT'UCR+3'UCD+2:2+6'FTX+AAO+++E3464 REFUSED - USER ROLE NOT MATCHED ON MUCR ARRIVALS'UNT+10+11248939652460'UNZ+1+1024C163353000'",
				new[] { "USER ROLE NOT MATCHED ON MUCR ARRIVALS" },
				null
				);
		}

		void RunUkctrlErrorForConsol(string receivedInterchangeText, string[] snippetsOfTextExpectedInEmail, bool? expectConsolToBeClosed, string sentRequestMessageText = null)
		{
			SetupEmails();
			GBCustomsDataRegistry.Instance.NotificationChiefNegativeResponsesUkctrlNak.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0000123";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var mockOutboundEdiMessage = Factory.NewMoq<EDIMessageDummyForTest_84>();
			mockOutboundEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("84");
			var sentMessage = mockOutboundEdiMessage.Object;
			sentMessage.EM_MessageText = (sentRequestMessageText ?? "Anything " + EDIMessage.MessageNumberPlaceHolder);
			sentMessage.EM_ReceiveTransmit = "TRX";
			sentMessage.EM_MessageType = GbDes242MessageFunction.Eac;
			sentMessage.EM_MessageSubType = GbCusDecMessageFunctionsList.Codes.Close;
			consol.Messages.Add(sentMessage);
			var interchangeString = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(receivedInterchangeText, sentMessage);
			var receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString, EDIMessage.ApplicationCodes.GbEdifactShared);
			Factory.Save();
			RunProcessor();
			var consolReloadedWowOurCaching = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consolReloadedWowOurCaching.Messages.Load();
			AssertEquals(2, consolReloadedWowOurCaching.Messages.Count);
			var incomingMessage = consolReloadedWowOurCaching.Messages.LastIncomingMessage;
			AssertEquals(receivedInterchange.ContainedMessages[0].PK, incomingMessage.PK);
			AssertEquals("UKCTRL", incomingMessage.EM_ApplicationReference);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consolReloadedWowOurCaching, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, wrapper.MawbExportHelper.Messages.Count);
			AssertEquals("NAK", incomingMessage.EM_MessageType);
			var body = Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Last().Body;
			foreach (var snippet in snippetsOfTextExpectedInEmail)
			{
				AssertContains(snippet, body);
			}
			if (expectConsolToBeClosed.HasValue)
			{
				AssertEquals("Expect consol to be closed - " + expectConsolToBeClosed.Value.ToString(), expectConsolToBeClosed.Value, wrapper.MawbExportHelper.ME_ChiefConsolIsClosed);
			}
		}

		[TestDate(2008, 12, 11)]
		public void TestNakFromEac()
		{
			CreateDeclaration();
			string nackMessagereceived = "UNH+09411285047242+UKCTRL:1:912:UK:109100'" +
					"UCM+123:<<SYSCAR>>+UKCINV:D:00A:UN:109001'" +
					"UCX+4+54'" +
					"UCR+0+54'" +
					"UCD+0+54'" +
					"FTX+AAO+++E10003 Errors on Document'" +
					"UCR+3'UCD+2:2+6'" +
					"FTX+AAI+++E1231 UCR / Part  has no associated Masters'" +  // Note FTX type is AAI, not the usual AAO
					"UNT+10+09411285047242'";

			SetUpAndRun(nackMessagereceived, GbCusDecMessageFunctionsList.Codes.Disassociate);

			AssertEquals("Check status of received UKCTRL msg from D8 is 'RCV'", EDIMessage.Status.Received, ukCtrlMessageReceived.EM_Status);
			AssertEquals("Check status of uploaded message to D8 is 'REJ'", EDIMessage.Status.Rejected, sentMessage.EM_Status);
			AssertEquals("Check entry status of cus entry header is unchanged", "ABC", cusEntry.CH_EntryStatus);
			AssertEquals("Check message status of cus entry header is error", MessageStatusList.Codes.SentAndRejected, cusEntry.CH_Status);
			AssertContains(@"E10003 Errors on Document
<BR/>
E1231 UCR / Part  has no associated Masters</b>", ukCtrlMessageReceived.EM_MessageInterpretation);
		}

		[TestDate(2008, 12, 11)]
		public void TestAckFromEacDis()
		{
			CreateDeclaration();
			string ackMessagereceived = @"UNH+09411288914116+UKCTRL:1:912:UK:109100'" +
								 @"UCM+123:<<SYSCAR>>+UKCINV:D:00A:UN:109001'" +
								 @"UCX+1'" +
								 @"UNT+4+09411288914116'";
			SetUpAndRun(ackMessagereceived, GbCusDecMessageFunctionsList.Codes.Disassociate);

			AssertEquals("Check status of received control msg from D8 is 'RCV'", EDIMessage.Status.Received, ukCtrlMessageReceived.EM_Status);
			AssertEquals("Check status of uploaded message to D8 is 'ACK'", EDIMessage.Status.Acknowledged, sentMessage.EM_Status);
			AssertEquals("Check entry status of cus entry header is unchanged", "ABC", cusEntry.CH_EntryStatus);
			AssertEquals("Check message status of cus entry header is received", MessageStatusList.Codes.OK, cusEntry.CH_Status);
			AssertContains("ACK received. Message #123 (EAC/DIS) has been accepted", ukCtrlMessageReceived.EM_MessageInterpretation);
			AssertEquals("MUCR should have been wiped, so long as we check using a new factory", "", new BusinessObjectFactory().Load<JobDeclaration>(cusEntry.Declaration.PK).JE_MasterUCR);
			AssertEquals(cusEntry, ukCtrlMessageReceived.EM_LinkedObject);
		}

		[TestDate(2008, 12, 11)]
		public void TestAckFromEac_Close_OneAttachedConsol()
		{
			CreateDeclaration();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_TransportMode = TransportTypeList.Codes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			jobDec.JE_JS = consol.Shipments.AddNew().PK;
			RunEacCloseAdsTest(consol);
		}

		[TestDate(2008, 12, 11)]
		public void TestAckFromEac_Close_SeveralAttachedConsolsOnlyOneRelevant()
		{
			CreateDeclaration();
			var goodExportAirConsol = Factory.New<ForwardingConsol>();
			goodExportAirConsol.JK_RL_NKLoadPort = "GBLHR";
			goodExportAirConsol.JK_RL_NKDischargePort = "AUSYD";
			goodExportAirConsol.JK_TransportMode = TransportTypeList.Codes.Air;
			var badExportSeaConsol = Factory.New<ForwardingConsol>();
			badExportSeaConsol.JK_RL_NKLoadPort = "GBLHR";
			badExportSeaConsol.JK_RL_NKDischargePort = "AUSYD";
			badExportSeaConsol.JK_TransportMode = TransportTypeList.Codes.Sea;
			var irrelevantExportConsol = Factory.New<ForwardingConsol>();
			irrelevantExportConsol.JK_RL_NKLoadPort = "AUSYD";
			irrelevantExportConsol.JK_RL_NKDischargePort = "NZAKL";
			irrelevantExportConsol.JK_TransportMode = TransportTypeList.Codes.Air;
			var importConsol = Factory.New<ForwardingConsol>();
			importConsol.JK_RL_NKLoadPort = "USNYC";
			importConsol.JK_RL_NKDischargePort = "GBLHR";
			importConsol.JK_TransportMode = TransportTypeList.Codes.Air;
			var shipment = Factory.New<ForwardingShipment>();
			jobDec.JE_JS = shipment.PK;
			shipment.Consols.Add(importConsol);
			shipment.Consols.Add(goodExportAirConsol);
			shipment.Consols.Add(irrelevantExportConsol);
			jobDec.JE_JS = shipment.PK;
			RunEacCloseAdsTest(goodExportAirConsol);
		}

		void RunEacCloseAdsTest(ForwardingConsol consolExpectedToHaveAdsOnIt)
		{
			string ackMessagereceived = @"UNH+09411288914116+UKCTRL:1:912:UK:109100'" +
								 @"UCM+123:<<SYSCAR>>+UKCINV:D:00A:UN:109001'" +
								 @"UCX+1'" +
								 @"UNT+4+09411288914116'";
			SetUpAndRun(ackMessagereceived, GbCusDecMessageFunctionsList.Codes.Close);
			AssertEquals("Check status of received control msg from Chief is 'RCV'", EDIMessage.Status.Received, ukCtrlMessageReceived.EM_Status);
			ZQuery consolLog = new ZQuery(StmALogSchema.SL_Parent, consolExpectedToHaveAdsOnIt.PK);
			consolLog.AddToFilter(StmALogSchema.SL_Reference, GbConstants.AirlineDeliveryScheduleCode);
			ZQuery declarationLog = new ZQuery(StmALogSchema.SL_Parent, jobDec.PK);
			declarationLog.AddToFilter(StmALogSchema.SL_Reference, GbConstants.AirlineDeliveryScheduleCode);
			AssertEquals("No 'ADS' log event for declaration", 0, Factory.Load<StmALog>(declarationLog).Length);
			AssertEquals("No 'ADS' log event for consol", 0, Factory.Load<StmALog>(consolLog).Length);
			AssertNotContains("The job's consol was found and an 'ADS' event was added", ukCtrlMessageReceived.EM_MessageInterpretation);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, consolExpectedToHaveAdsOnIt.PK));
			AssertNull("Print job for ADS should NOT have been queued", printJob);
		}

		void SetUpAndRun(string ackMessagereceived, string outboundRequestType)
		{
			SetupEmails();

			// Put a CusEntryHeader in the DB			
			cusEntry = jobDec.CustomsEntryHeaders.AddNew();
			cusEntry.CH_MessageType = EDIMessage.ApplicationCodes.GbEdifactShared;
			cusEntry.CH_EntryStatus = "ABC";
			var cusLine = cusEntry.MergedLines.AddNew();
			// This is a real outgoing CUSDEC - shown as SENT
			string outboundEacInterchangeText = @"UNB+UNOA:2+CUKFFW98000DBU:IATA:GEMS292423+CUKCTM98CHFEXP:IATA+091027:1644+GEMS292423'UNH+292423+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAC:105:109'RFF+ABO:9GB625512459000-S007109-001'RFF+UCN:A?:69566857991'UNS+D'UNS+S'UNT+7+292423'UNZ+1+GEMS292423'";
			outboundEacInterchangeText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(outboundEacInterchangeText, cusEntry);

			EDIInterchange ediInterchangeCusdecToUpload = EDIInterchange.CreateNewInterchangeFromString(Factory, outboundEacInterchangeText, "CNC");
			ediInterchangeCusdecToUpload.EI_Status = EDIInterchange.Status.Sent;
			ediInterchangeCusdecToUpload.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			ediInterchangeCusdecToUpload.EI_ApplicationCode = ApplicationCodeList.Codes.GbEdifactShared;

			Factory.Save();

			// The factory creates an EdiMessagwe at this point but assume, wrongly, that it's a SENT message. We need to update the test db to show
			// that the message, too, was outbound.  Otherwise our processor will nab it too.
			sentMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, ediInterchangeCusdecToUpload.PK));
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_MessageNum = "123";
			sentMessage.EM_MessageType = "EAC";
			sentMessage.EM_MessageSubType = outboundRequestType;
			cusEntry.Messages.Add(sentMessage);

			// This is a real UKCTRL message from Destin8 - let's pretend that we just downloaded it.
			string fullReceivedInterchangeString = @"UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKFFW98000DBU:IATA:GEMS292423+091027:1641+201D2164149000'"
													+ GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(ackMessagereceived, cusEntry)
													+ "UNZ+1+201D2164149000'";

			EDIInterchange ukCtrlAckInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, fullReceivedInterchangeString, EDIMessage.ApplicationCodes.GbEdifactShared);
			ukCtrlAckInterchange.EI_Status = EDIInterchange.Status.Queued;
			ukCtrlAckInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			ukCtrlMessageReceived = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, ukCtrlAckInterchange.PK));

			RunProcessor();

			ukCtrlAckInterchange.Reload();
			ediInterchangeCusdecToUpload.Reload();
			sentMessage.Reload();
			ukCtrlMessageReceived.Reload();
			cusEntry.Reload();
		}

		void CreateDeclaration()
		{
			jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MasterBill = "12587654321";
			jobDec.JE_MasterUCR = "A:123456";
		}

		EU.Business.Declaration.CusEntryHeader cusEntry;
		EDIMessage sentMessage;
		EDIMessage ukCtrlMessageReceived;
		GlbStaff mcpMessageStaff;
		GlbGroup groupZZ1;
		GlbStaff staffZ1;
		GlbStaff staffZ2;
		JobDeclaration jobDec;

		void SetupEmails()
		{
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

			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}
	}
	public class EDIMessageDummyForTest_84 : EDIMessage
	{
		public EDIMessageDummyForTest_84(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "84";
		}
	}
}
