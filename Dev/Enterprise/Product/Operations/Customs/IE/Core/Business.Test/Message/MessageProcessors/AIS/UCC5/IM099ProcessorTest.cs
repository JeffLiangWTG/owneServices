using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;
using IM099Provider = Enterprise.Customs.IE.Messaging.UCC5.IM099Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM099Processor))]
	sealed class IM099ProcessorTest : EntryHeaderMessageProcessorTest<IM099Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM099Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override ZString MessageText => Serialize(GetMessageObject(RemarksTypeList.RefundApplicationRequired));

		Im099 GetMessageObject(string remarks)
		{
			return new Im099
			{
				Declaration = new DeclarationType
				{
					Lrn25 = "LRN123456789",
					DateLimitOfResponse = "20230811",
					Remarks = remarks,
					CustomsOffices = new DeclarationTypeCustomsOffices { CustomsOfficeLodgement = "LCO12345" },
				}
			};
		}

		protected override ZString MessageFriendlyName => "IM099: General Notification and Request Information";

		protected override IM099Processor Processor => new IM099Processor(logger, typeof(Im099));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"IM099: General Notification and Request Information<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Date Limit of Response</td><td>2023-08-11</td></tr><tr><td>Customs Office Lodgement</td><td>LCO12345</td></tr><tr><td>Remarks</td><td>Refund Application Required</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail("IM099: General Notification and Request Information",
				new[] { "A General Notification and Request Information (IM099) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });

			var refundDuty = (RefundDuty)messageAttachee.MergedLines[0].RefundDuties.First();

			AssertEquals("TaxType", "A00", refundDuty.TaxType);
			AssertEquals("TaxAmountConfirmedRelease", 2m, refundDuty.TaxAmountConfirmedRelease);
			AssertEquals("TaxAmountConfirmedAmendment", 1.7m, refundDuty.TaxAmountConfirmedAmendment);
			AssertEquals("TaxAmountDifferenceForRefunds", 0.3m, refundDuty.TaxAmountDifferenceForRefunds);
			AssertEquals("TaxAmountOfDutyToBeRepaid", 0m, refundDuty.TaxAmountOfDutyToBeRepaid);
		}

		public void TestEntryStatus()
		{
			var (messageAttachee, _) = SetupAndProcessMessage(Serialize(GetMessageObject("Refund application required")));
			AssertEquals("RAR", messageAttachee.CH_EntryStatus);

			(messageAttachee, _) = SetupAndProcessMessage(Serialize(GetMessageObject("Insufficient fund")));
			AssertEquals("INS", messageAttachee.CH_EntryStatus);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			(result.messageAttachee.MergedLines.FirstOrDefault() ?? result.messageAttachee.MergedLines.AddNew()).CL_LineNumber = 1;

			var previous429Message = Factory.New<AISUCC5InboundEDIMessage>();
			previous429Message.EM_ApplicationCode = "IE5";
			previous429Message.EM_MessageType = AISInterchangeTypeList.Codes.IM429;
			previous429Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, AISInterchangeProcessorTestHelper.GetStandardUCC5IM429Text(), includeResponseWrap: false, includeEncoding: false);
			previous429Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			previous429Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previous429Message);

			var previous404Message = Factory.New<AISUCC5InboundEDIMessage>();
			previous404Message.EM_ApplicationCode = "IE5";
			previous404Message.EM_MessageType = AISInterchangeTypeList.Codes.IM404;
			previous404Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, Serialize(IM404ProcessorTest.GetMessageObject()), includeResponseWrap: false, includeEncoding: false);
			previous404Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1).AddHours(1);
			previous404Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previous404Message);

			return result;
		}
	}
}
