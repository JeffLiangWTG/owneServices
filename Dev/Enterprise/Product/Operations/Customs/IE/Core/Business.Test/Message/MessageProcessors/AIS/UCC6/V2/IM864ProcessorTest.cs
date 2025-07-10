using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM864;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM864Processor))]
	class IM864ProcessorTest : EntryHeaderMessageProcessorTest<IM864Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM864Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM864;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im864
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType864
			{
				Mrn = "12MRN345ABCDE678R9",
				CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
				InvalidationRequestCancellationReason = "Invalidation Request Cancellation Reason",
			},
		});

		protected override ZString MessageFriendlyName => "IM864: Invalidation Request Cancellation";

		protected override IM864Processor Processor => new IM864Processor(logger, typeof(Im864));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			testData.messageAttachee.CH_EntryStatus = LogicalStatusList.Codes.Sent;
			return testData;
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Control, messageAttachee.CH_EntryStatus);
			AssertMessageInterpretation(incomingMessage, @"An Invalidation Request Cancellation (IM864) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>Invalidation Request Cancellation Reason</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Invalidation Request Cancellation (IM864) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
