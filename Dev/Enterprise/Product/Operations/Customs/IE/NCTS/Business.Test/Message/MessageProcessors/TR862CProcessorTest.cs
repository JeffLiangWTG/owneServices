using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR862C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR862CProcessor))]
	class TR862CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR862CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR862CProvider>
	{
		public void TestGetEntryStatusWithoutAdditionalComparer()
		{
			var testItems = CreateSetupData();

			var processor = new TR862CProcessorRemoveAdditionalComparer(logger, typeof(Tr862C));
			processor.PreProcessMessage(testItems.incomingMessage);
			processor.ProcessMessage(testItems.incomingMessage);

			AssertEquals(
				"Without an additional comparer, the processor gets an IE028 when looking for history message, which should be IE029.",
				NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
				testItems.messageAttachee.BM_CustomsStatus
			);
		}

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var result = base.CreateSetupData(incomingMessageText);

			var basicTime = new ZDateTime(2023, 2, 27, 10, 0, 0, 123);

			var previousIE022Message = Factory.New<NCTSInboundEDIMessage>();
			previousIE022Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE022; // Represents status AMR, comes first when searching descending, to be skipped.
			previousIE022Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardCC022CText("23LRN1234"), includeResponseWrap: false);
			previousIE022Message.EM_SystemCreateTimeUtc = basicTime;
			previousIE022Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousIE022Message);

			var previousIE028Message = Factory.New<NCTSInboundEDIMessage>();
			previousIE028Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE028; // Represents status MRN, comes 2nd when searching descending, logically the earliest message.
			previousIE028Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardCC028CText("23LRN1234", "23IEROS11A782454R2"), includeResponseWrap: false);
			previousIE028Message.EM_SystemCreateTimeUtc = basicTime;
			previousIE028Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousIE028Message);

			var previousIE029Message = Factory.New<NCTSInboundEDIMessage>();
			previousIE029Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE029; // Represents status REL, comes last when searching descending, logically the last message.
			previousIE029Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardCC029CText("23LRN1234", "23IEROS11A782454R2"), includeResponseWrap: false);
			previousIE029Message.EM_SystemCreateTimeUtc = basicTime;
			previousIE029Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousIE029Message);

			var previousTR062Message = Factory.New<NCTSInboundEDIMessage>();
			previousTR062Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.TR062C;
			previousTR062Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardTR062CText("23IEROS11A782454R2"), includeResponseWrap: false);
			previousTR062Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(30);
			previousTR062Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousTR062Message);

			var incomingTR862Message = result.incomingMessage;
			incomingTR862Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(60);
			Factory.Save();

			return result;
		}

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals(
				"Having IE028&IE029 history with the same creation time, should get IE029 as a logically more recent message.",
				NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
				messageAttachee.BM_CustomsStatus
			);
			AssertMessageInterpretation(incomingMessage,
				"A Declaration Amendment Request Cancellation (TR862) message has been received for Job B00001000.<br /><br />" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>Case Id</td><td>Test ID</td></tr>" +
				"<tr><td>Amendment request cancellation Reason</td><td>Test Amendment request cancellation reason</td></tr></table>"
			);
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Declaration Amendment Request Cancellation (TR862) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR862C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR862CText();

		protected override ZString MessageFriendlyName => "TR862C: DECLARATION AMENDMENT REQUEST CANCELLATION";

		protected override TR862CProcessor Processor => new TR862CProcessor(logger, typeof(Tr862C));

		class TR862CProcessorRemoveAdditionalComparer : TR862CProcessor
		{
			public TR862CProcessorRemoveAdditionalComparer(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			protected internal override IComparer<Enterprise.Messaging.Business.EDIMessage> GetAdditionalComparer(ListSortDirection direction) => null;
		}
	}
}
