using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(XtMessageErrorResponseProcessor))]
	sealed class XtMessageErrorResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessedQCH_MessageStatus()
		{
			(string messageType, string preReqQCH_MessageStatus, string expectedQCH_MessageStatus)[] testCases = new (string, string, string)[] {
				(AUCOLSMessageTypeList.Codes.AddNewLodgement, COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse, COLSHeaderStatusList.Codes.FailedAddLodgement),
				(AUCOLSMessageTypeList.Codes.AddAdditionalDocument, COLSHeaderStatusList.Codes.AwaitingAddAdditionalDocumentResponse, COLSHeaderStatusList.Codes.FailedAddDocument),
				(AUCOLSMessageTypeList.Codes.MakeAnEnquiry, COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse, COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithoutDocs),
				(AUCOLSMessageTypeList.Codes.MakeAnEnquiry, COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse, COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithDocs),
				(AUCOLSMessageTypeList.Codes.RequestAReassessment, COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse, COLSHeaderStatusList.Codes.FailedReassessmentWithoutDocs),
				(AUCOLSMessageTypeList.Codes.RequestAReassessment, COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse, COLSHeaderStatusList.Codes.FailedReassessmentWithDocs),
				(AUCOLSMessageTypeList.Codes.LodgementStatus, COLSHeaderStatusList.Codes.AwaitingGetLodgementStatusResponse, COLSHeaderStatusList.Codes.FailedGetLodgementStatus),
				(AUCOLSMessageTypeList.Codes.PaymentStatus, COLSHeaderStatusList.Codes.AwaitingGetPaymentResponse, COLSHeaderStatusList.Codes.FailedGetPaymentStatus),
				(AUCOLSMessageTypeList.Codes.SwitchAepLodgement, COLSHeaderStatusList.Codes.AwaitingSwitchAepRequest, COLSHeaderStatusList.Codes.FailedSwitchAepRequest),
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					var preReqQCH_MessageStatusHasValue = !string.IsNullOrEmpty(testCase.preReqQCH_MessageStatus);
					colsHeader.QCH_MessageStatus = preReqQCH_MessageStatusHasValue ? testCase.preReqQCH_MessageStatus : ZString.Empty;

					ProcessMessage(testCase.messageType, colsHeader);

					var preReqDescription = preReqQCH_MessageStatusHasValue ? $" ({testCase.preReqQCH_MessageStatus})" : string.Empty;
					AssertEquals($"[{testCase.messageType}{preReqDescription}] EM_LinkedObject", colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
					AssertEquals($"[{testCase.messageType}{preReqDescription}] QCH_MessageStatus", testCase.expectedQCH_MessageStatus, colsHeader.QCH_MessageStatus);
					AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Received, inboundMessage.EM_Status);
				}

				colsHeader.QCH_MessageStatus = ZString.Empty;
				logger.ClearLogs();
				ProcessMessage("UNK", colsHeader);
				AssertContains("[Unknown message type] Logger", "Unknown outgoing message type: UNK", string.Join("\r\n", logger.Logs));
				AssertEquals("[Unknown message type] does not change QCH_MessageStatus", ZString.Empty, colsHeader.QCH_MessageStatus);
				AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Failed, inboundMessage.EM_Status);

				colsHeader.QCH_MessageStatus = "UNK";
				logger.ClearLogs();
				ProcessMessage(AUCOLSMessageTypeList.Codes.MakeAnEnquiry, colsHeader);
				AssertContains("[Unknown HeaderStatus] Logger", "Unknown Header Message Status: UNK", string.Join("\r\n", logger.Logs));
				AssertEquals("[Unknown message type] does not change QCH_MessageStatus", "UNK", colsHeader.QCH_MessageStatus);
				AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Failed, inboundMessage.EM_Status);

				logger.ClearLogs();
				ProcessMessage(AUCOLSMessageTypeList.Codes.MakeAnEnquiry, Factory.New<DummyBusinessObject>());
				AssertContains("[Unknown Linked Object] Logger", "Unexpected Linked Object: CargoWise.EntityFramework.Testing.DummyBusinessObject", string.Join("\r\n", logger.Logs));
				AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Failed, inboundMessage.EM_Status);
			});
		}

		public void TestProcessedMessageStatus_CNA()
		{
			(string CSD_MessageStatus, string expectedCSD_MessageStatus)[] testCases = new (string, string)[] {
				(COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse, COLSDocumentStatusList.Codes.FailedDocumentSentResponse),
				(COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse, COLSDocumentStatusList.Codes.FailedLastdocSentResponse),
			};

			CombineAssertions(() =>
			{
				var attachment = colsHeader.EDocPivotCollection.AddNew();

				foreach (var testCase in testCases)
				{
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse;
					attachment.CSD_MessageStatus = testCase.CSD_MessageStatus;
					ProcessMessage(AUCOLSMessageTypeList.Codes.AddAttachment, attachment);
					AssertEquals($"[{testCase.CSD_MessageStatus}] EM_LinkedObject", attachment.PK, inboundMessage.EM_LinkedObject.PK);
					AssertEquals($"[{testCase.CSD_MessageStatus}] header.QCH_MessageStatus", COLSHeaderStatusList.Codes.FailedAddAttachment, colsHeader.QCH_MessageStatus);
					AssertEquals($"[{testCase.CSD_MessageStatus}] CSD_MessageStatus", testCase.expectedCSD_MessageStatus, attachment.CSD_MessageStatus);
					AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Received, inboundMessage.EM_Status);
				}

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse;
				attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;

				var attachment2 = colsHeader.EDocPivotCollection.AddNew();
				attachment2.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
				var pendingDocMessage = Factory.New<EDIMessage>();
				pendingDocMessage.EM_LinkedObject = attachment2;
				pendingDocMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
				pendingDocMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
				pendingDocMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
				pendingDocMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				pendingDocMessage.EM_Status = EDIMessage.Status.Pending;
				colsHeader.ReloadMessages();

				logger.ClearLogs();
				ProcessMessage(AUCOLSMessageTypeList.Codes.AddAttachment, attachment);
				AssertEquals("this document marked as failed", COLSDocumentStatusList.Codes.FailedDocumentSentResponse, attachment.CSD_MessageStatus);
				AssertEquals("next document is queued", EDIMessage.Status.Queued, pendingDocMessage.EM_Status);
				AssertEquals("header status updated for next message", COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse;
				attachment.CSD_MessageStatus = "UNK";
				logger.ClearLogs();
				ProcessMessage(AUCOLSMessageTypeList.Codes.AddAttachment, attachment);

				AssertContains("[Unknown DocumentStatus] Log", "Unknown Attachment Message Status: UNK", string.Join("\r\n", logger.Logs));
				AssertEquals("[Unknown DocumentStatus] does not change CSD_MessageStatus", "UNK", attachment.CSD_MessageStatus);
				AssertEquals("inboundMessage EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.Failed, inboundMessage.EM_Status);
			});
		}

		public void TestProcessMessageStatus_DiscardAttachmentMessages()
		{
			(string messageType, string preReqQCH_MessageStatus, string expectedQCH_MessageStatus)[] testCases = new (string, string, string)[] {
				(AUCOLSMessageTypeList.Codes.AddNewLodgement, COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse, COLSHeaderStatusList.Codes.FailedAddLodgement),
				(AUCOLSMessageTypeList.Codes.AddAdditionalDocument, COLSHeaderStatusList.Codes.AwaitingAddAdditionalDocumentResponse, COLSHeaderStatusList.Codes.FailedAddDocument),
				(AUCOLSMessageTypeList.Codes.MakeAnEnquiry, COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse, COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithDocs),
				(AUCOLSMessageTypeList.Codes.RequestAReassessment, COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse, COLSHeaderStatusList.Codes.FailedReassessmentWithDocs),
			};

			CombineAssertions(() =>
			{
				var attachment = colsHeader.EDocPivotCollection.AddNew();
				attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;

				var pendingDocMessage = Factory.New<EDIMessage>();
				pendingDocMessage.EM_LinkedObject = attachment;
				pendingDocMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
				pendingDocMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
				pendingDocMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
				pendingDocMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				pendingDocMessage.EM_Status = EDIMessage.Status.Pending;

				outboundMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
				colsHeader.ReloadMessages();

				foreach (var testCase in testCases)
				{
					colsHeader.QCH_MessageStatus = testCase.preReqQCH_MessageStatus;
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
					pendingDocMessage.EM_Status = EDIMessage.Status.Pending;
					inboundMessage.EM_MessageSubType = "XXX";

					logger.ClearLogs();
					ProcessMessage(testCase.messageType, colsHeader);
					AssertEquals("message is Received", EDIMessage.Status.Received, inboundMessage.EM_Status);
					AssertEquals("message sub type matches out message", AUCOLSMessageSubTypeList.Codes.NormalAttachment, inboundMessage.EM_MessageSubType);
					AssertEquals("header status updated", testCase.expectedQCH_MessageStatus, colsHeader.QCH_MessageStatus);
					AssertEquals("this document discarded", COLSDocumentStatusList.Codes.Discarded, attachment.CSD_MessageStatus);
					AssertEquals("document message is Discarded", EDIMessage.Status.Discarded, pendingDocMessage.EM_Status);
				}
			});
		}

		void ProcessMessage(string messageType, BusinessObject linkedObject)
		{
			outboundMessage.EM_MessageType = messageType;
			outboundMessage.EM_LinkedObject = linkedObject;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_LinkedObject = null;
			processor.ProcessMessage(inboundMessage);
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new XtMessageErrorResponseProcessor(logger);

		EDIMessage inboundMessage;
		EDIMessage outboundMessage;
		LoggingInformation logger;

		protected override void SetUp()
		{
			logger = new LoggingInformation();

			base.SetUp();

			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;

			inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.XtMessageError;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Header>
	<SenderID>xT</SenderID>
	<RecipientID>HYEDAUCMT</RecipientID>
</Header>
<Body>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
		<Event>
			<EventTime>2023-08-11 02:45:46.733</EventTime>
			<EventType>IRJ</EventType>
			<EventParameters>
				<Reason>Error Message  Contract: xt-contract:/Customs/AU COLS/AU.COLS.Contract.CNL
				Reference Object: xt-httpclientaddress:{81ffd419-f17d-47a0-a27d-addd71702902}
				Reference Object: xt-node:{2b76b3c7-060f-4c25-8cbd-21901ee0a920}
				Error description: Message transmission to https://api.agriculture.gov.au/biosecurity/cargo-lodgements/business-to-government/vendor/v1/lodgements/addNewLodgment rejected by peer: result code 400 not accepted
				Reference Object: xt-httpclientaddress:{81ffd419-f17d-47a0-a27d-addd71702902}
				Reference Object: xt-node:{2b76b3c7-060f-4c25-8cbd-21901ee0a920}
				Error description: Message transmission to https://api.agriculture.gov.au/biosecurity/cargo-lodgements/business-to-government/vendor/v1/lodgements/addNewLodgment rejected by peer: result code 400 not accepted
				Reference Object: xt-httpclientaddress:{81ffd419-f17d-47a0-a27d-addd71702902}
				Reference Object: xt-node:{2b76b3c7-060f-4c25-8cbd-21901ee0a920}
				Error description: Message transmission to https://api.agriculture.gov.au/biosecurity/cargo-lodgements/business-to-government/vendor/v1/lodgements/addNewLodgment rejected by peer: result code 400 not accepted
				Reference Object: xt-httpclientaddress:{81ffd419-f17d-47a0-a27d-addd71702902}
				Reference Object: xt-node:{2b76b3c7-060f-4c25-8cbd-21901ee0a920}
				Error description: Message transmission to https://api.agriculture.gov.au/biosecurity/cargo-lodgements/business-to-government/vendor/v1/lodgements/addNewLodgment rejected by peer: result code 400 not accepted
				</Reason>
				<MessageType>XER</MessageType>
			</EventParameters>
			<ContextCollection>
				<Context>
					<Type>OriginalMessage</Type>
					<Value><![CDATA[{""entryNumber"":""AAAGEN97N"",""branchId"":""AA33HF"",""biconReference"":""ASDFDF"",""importPermitNumber"":""DFDF"",""contactName"":"""",""phoneNumber"":""+61277777777"",""email"":"""",""thirdPartyInd"":""Y"",""thirdPartyEmail"":""JONATHAN.REGAN@WISETECHGLOBAL.COM"",""aaRefNum"":""1234"",""lateLodgementReason"":""CBF"",""lateLodgementDetails"":""RLYCBF"",""directionRequests"":[{""direction"":""fumo pls"",""directionLineContainer"":""AAAGEN97N-1"",""treatmentType"":""fumo"",""location"":""place"",""aaname"":""place"",""aanumber"":""124`""}],""deliveryClassification"":""Metro"",""unpackAddress"":""57 FOVEAUX STREET,SURRY HILLS,2010,New South Wales"",""additionalComment"":""meowmeowme moew"",""generalDeclaration"":""True""}]]></Value>       </Context>       <Context>        <Type>OriginalAttributes</Type>        <Value><![CDATA[custom.MessageType : CNL  version : 8  syncflags : 256  datahashout :   acktimeout : 0  seqidout : 0  flagstext :   custom.SourceParty : HYEDAUCMT  toprot : 1282  knownversion : 0  seqtypeout : 0  fromprot : 903  custom.MessageTrackingID : 6c510ce5-36fa-4150-b70f-0c4675bffd09  creationtime : 1691721760  seqnoout : 0  custom.AU.SubscriptionKey : cdfd202a32a044aebd7a4e6348d7b5e2  curracklevel : 0  state : 5121  parseinfoin.std.sender : HYECMT  msginfo :   internalid : 42329  custom.DestinationParty : xT  datasizeout : 641  laststate : 1025  toobj : xt-node:/Customs/AU COLS/AU.COLS.HttpClient.CNL  fromobj : xt-application:/Framework/Direct Endpoints/HYECMT  datasizein : 641  ackprot : 0  toprot-name : xt-httpclientaddress  priority : 1  acklevel : 0  sequuidout : 00000000-0000-0000-0000-000000000000  msguuid : 1d09bd15-1ed1-4090-8760-f39d7e0ffe2a  fromprot-name : xt-application  datahashin : fd8246378915d1bb72f54770235981c1  msgtype : 1  statename : ST_ERR_OUT  sequuidin : 00000000-0000-0000-0000-000000000000  flags : 0  cfgversion : 398360  ackprot-name : xt-none  filenamein :   toparty :   finalretransmission : 3  contractobj : xt-contract:/Customs/AU COLS/AU.COLS.Contract.CNL  seqtypein : 0  folderin :   owner : 18446744073709551615  msgid : 2271567  filenameout : 2271567  custom.ApplicationCode : COL  archiveflags : 0  httpclient.url : https://api.agriculture.gov.au/biosecurity/cargo-lodgements/business-to-government/vendor/v1/lodgements/addNewLodgment  fromparty :   seqidin : 0  syncreply : 0  seqnoin : 0  wfinst : 00000000-0000-0000-0000-000000000000]]></Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>
</Body>
</UniversalInterchange>";
		}
	}
}
