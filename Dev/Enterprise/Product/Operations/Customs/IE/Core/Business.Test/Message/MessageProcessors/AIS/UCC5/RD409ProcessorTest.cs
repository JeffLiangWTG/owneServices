using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD409;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using RD409Provider = Enterprise.Customs.IE.Messaging.UCC5.RD409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RD409Processor))]
	sealed class RD409ProcessorTest : EntryHeaderMessageProcessorTest<RD409Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, RD409Provider>
	{
		public void TestAssertWhenDepositRefundApplicationApprovedIsFalse()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			var messageText = Serialize(GetMessageObject());
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.RefundApplicationAccepted, messageAttachee.CH_EntryStatus);

				AssertMessageInterpretation(incomingMessage, @"[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application Accepted.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>ApplicationReferId</td></tr><tr><td>Date</td><td>20230811</td></tr><tr><td>Applicant EORI Number</td><td>AN</td></tr><tr><td>Deposit Refund Application Approved</td><td>Y</td></tr><tr><td>Reason Not Approved</td><td>&nbsp;</td></tr><tr><td>Statement of the Decision Taking Customs Authority</td><td>Statement of the Decision Taking Customs Authority</td></tr></table><br />
<br />Amount of Deposit Refund<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Amount of Deposit Refund</td><td>Payer EORI for Refund</td></tr><tr><td>123.45</td><td>PR</td></tr></table>");

				MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
					new[] { "[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application Accepted." },
					new string[] { "staff1@where.com" });
			}
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD409;

		protected override ZString MessageText => Serialize(GetMessageObject(false, "Reason Not Approved"));

		Rd409 GetMessageObject(bool depositRefundApplicationApproved = true, string reasonNotApproved = "")
		{
			return new Rd409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "ApplicationReferId",
					Date = "20230811",
					Applicant = "AN",
					DepositRefundApplicationApproved = depositRefundApplicationApproved,
					ReasonNotApproved = reasonNotApproved,
					StatementOfTheDecisionTakingCustomsAuthority = "Statement of the Decision Taking Customs Authority"
				},
				DepositRefundDetails = new DepositRefundDetailsType
				{
					AmountOfDepositRefund = 123.45m,
					PayerEoriForRefund = "PR"
				}
			};
		}

		protected override ZString MessageFriendlyName => "RD409: Deposit Refund Application Decision";

		protected override RD409Processor Processor => new RD409Processor(logger, typeof(Rd409));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.RefundApplicationRejected, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application Rejected.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>ApplicationReferId</td></tr><tr><td>Date</td><td>20230811</td></tr><tr><td>Applicant EORI Number</td><td>AN</td></tr><tr><td>Deposit Refund Application Approved</td><td>N</td></tr><tr><td>Reason Not Approved</td><td>Reason Not Approved</td></tr><tr><td>Statement of the Decision Taking Customs Authority</td><td>Statement of the Decision Taking Customs Authority</td></tr></table><br />
<br />Amount of Deposit Refund<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Amount of Deposit Refund</td><td>Payer EORI for Refund</td></tr><tr><td>123.45</td><td>PR</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application Rejected." },
				new string[] { "staff1@where.com" });
		}
	}
}
