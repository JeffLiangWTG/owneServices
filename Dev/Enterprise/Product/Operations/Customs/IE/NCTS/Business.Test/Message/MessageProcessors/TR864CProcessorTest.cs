using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR864C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR864CProcessor))]
	class TR864CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR864CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR864CProvider>
	{
		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var result = base.CreateSetupData(incomingMessageText);

			var basicTime = new ZDateTime(2023, 2, 27, 10, 0, 0, 123);

			var previousIE029Message = Factory.New<NCTSInboundEDIMessage>();
			previousIE029Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE029; // Represents status REL: Released For Transit
			previousIE029Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardCC029CText("23LRN1234", "23IEROS11A782454R2"), includeResponseWrap: false);
			previousIE029Message.EM_SystemCreateTimeUtc = basicTime;
			previousIE029Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousIE029Message);

			var previousIE028Message = Factory.New<NCTSInboundEDIMessage>();
			previousIE028Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE028; // Represents status MRN: MRN Allocated
			previousIE028Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardCC028CText("23LRN1234", "23IEROS11A782454R2"), includeResponseWrap: false);
			previousIE028Message.EM_SystemCreateTimeUtc = basicTime;
			previousIE028Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousIE028Message);

			var previousTR064Message = Factory.New<NCTSInboundEDIMessage>();
			previousTR064Message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.TR064C;
			previousTR064Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, InterchangeProcessorTestHelper.GetStandardTR064CText("23IEROS11A782454R2"), includeResponseWrap: false);
			previousTR064Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(30);
			previousTR064Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousTR064Message);

			var incomingTR864Message = result.incomingMessage;
			incomingTR864Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(60);
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
			AssertMessageInterpretation(incomingMessage, @"A Declaration Invalidation Request Cancellation (TR864) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>23IEROS11A782454R2</td></tr><tr><td>Case Id</td><td>ID123456</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>Reason for cancellation</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Declaration Invalidation Request Cancellation (TR864) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR864C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR864CText("23IEROS11A782454R2");

		protected override ZString MessageFriendlyName => "TR864C: CANCEL INVALIDATION REQUEST";

		protected override TR864CProcessor Processor => new TR864CProcessor(logger, typeof(Tr864C));
	}
}
