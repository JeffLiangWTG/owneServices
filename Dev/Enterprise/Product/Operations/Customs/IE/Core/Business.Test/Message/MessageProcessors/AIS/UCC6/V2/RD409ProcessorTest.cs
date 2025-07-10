using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD409;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RD409Processor))]
	sealed class RD409ProcessorTest : EntryHeaderMessageProcessorTest<RD409Processor, AISInboundEDIMessage, AISOutboundEDIMessage, RD409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD409;

		protected override ZString MessageText => Serialize(GenerateMessage(approved: true, null));

		protected override ZString MessageFriendlyName => "RD409: Deposit Refund Application Decision";

		protected override RD409Processor Processor => new RD409Processor(logger, typeof(Rd409));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.RefundApplicationAccepted, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, RD409MessageInterpreterTest.ExpectedInterpretation(approved: true, "&nbsp;"));

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application Accepted." },
				new string[] { "staff1@where.com" });
		}

		internal static Rd409 GenerateMessage(bool approved, string reasonNotApproved)
		{
			return new Rd409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "ApplicationReferId",
					Date = "20230811",
					Applicant = "AN",
					DepositRefundApplicationApproved = approved,
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
	}
}
