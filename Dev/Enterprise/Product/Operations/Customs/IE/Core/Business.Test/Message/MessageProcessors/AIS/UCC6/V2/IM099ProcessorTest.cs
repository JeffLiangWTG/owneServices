using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM099;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM099Processor))]
	class IM099ProcessorTest : EntryHeaderMessageProcessorTest<IM099Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM099Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText(string remarks = "Remarks001")
		{
			return Serialize(
				new Im099
				{
					ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType099
					{
						Lrn = "LRN001",
						DateLimitOfResponse = new DateTime(2023, 08, 11),
						Remarks = remarks,
					},
					CustomsOfficeOfPresentation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPcoType { ReferenceNumber = "PCO12345" },
					SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
					CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
					Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "ID1" },
				});
		}

		protected override ZString MessageFriendlyName => "IM099: General Notification and Request Information";

		protected override IM099Processor Processor => new IM099Processor(logger, typeof(Im099));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"A General Notification and Request Information (IM099) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Date Limit of Response</td><td>2023-08-11</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr><tr><td>Customs Office of Presentation</td><td>PCO12345</td></tr><tr><td>Supervising Customs Office</td><td>SCO12345</td></tr><tr><td>Customs Office Lodgement</td><td>LCO12345</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A General Notification and Request Information (IM099) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestAssertWithSpecificRemarks()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();

			CombineAssertions(() =>
			{
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "refund application required", AISEntryStatusList.Codes.RefundApplicationRequested);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "REFUND APPLICATION REQUIRED", AISEntryStatusList.Codes.RefundApplicationRequested);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "insufficient fund", AISEntryStatusList.Codes.InsufficientFund);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "Insufficient Fund", AISEntryStatusList.Codes.InsufficientFund);
			});
		}

		void AssertProcessedMessageStatus(AISInboundEDIMessage incomingMessage, CusEntryHeader messageAttachee, string messageText, string expectedStatus)
		{
			var serializedMessageText = GetMessageText(messageText);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, serializedMessageText, includeResponseWrap: false);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			messageAttachee.CH_EntryStatus = string.Empty;

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals($"CH_EntryStatus should correspond to the message remark: {messageText}", expectedStatus, messageAttachee.CH_EntryStatus);
			}
		}
	}
}
