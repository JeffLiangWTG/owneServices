using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using RD409Provider = Enterprise.Customs.IE.Messaging.UCC5.RD409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RD409MessageInterpreter))]
	sealed class RD409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, RD409MessageInterpreter, RD409Provider>
	{
		public void TestWhenInvalidationDecisionIsFalse()
		{
			var message = CreateIncomingMessage();
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals(@"[RD409 – Deposit Refund Application Decision] has been received and linked to job B00000012. Decision: Deposit Refund Application Accepted.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>ApplicationReferId</td></tr><tr><td>Date</td><td>20230811</td></tr><tr><td>Applicant EORI Number</td><td>AN</td></tr><tr><td>Deposit Refund Application Approved</td><td>Y</td></tr><tr><td>Reason Not Approved</td><td>&nbsp;</td></tr><tr><td>Statement of the Decision Taking Customs Authority</td><td>Statement of the Decision Taking Customs Authority</td></tr></table><br />
<br />Amount of Deposit Refund<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Amount of Deposit Refund</td><td>Payer EORI for Refund</td></tr><tr><td>123.45</td><td>PR</td></tr></table>".RemoveLineBreakingsAndIndents(),
				interpreter.GetInterpretation().RemoveLineBreakingsAndIndents()
			);
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD409;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			return CreateIncomingMessage(false, "Reason Not Approved");
		}

		AISUCC5InboundEDIMessage CreateIncomingMessage(bool depositRefundApplicationApproved = true, string reasonNotApproved = "")
		{
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", $@"<q1:RD409 xmlns:q1=""http://www.ros.ie/schemas/customs/RD409"">
  <q1:Header>
    <q1:ApplicationReferenceId>ApplicationReferId</q1:ApplicationReferenceId>
    <q1:Date>20230811</q1:Date>
    <q1:Applicant>AN</q1:Applicant>
    <q1:DepositRefundApplicationApproved>{depositRefundApplicationApproved.ToString().ToLower()}</q1:DepositRefundApplicationApproved>
    <q1:ReasonNotApproved>{reasonNotApproved}</q1:ReasonNotApproved>
    <q1:StatementOfTheDecisionTakingCustomsAuthority>Statement of the Decision Taking Customs Authority</q1:StatementOfTheDecisionTakingCustomsAuthority>
  </q1:Header>
  <q1:DepositRefundDetails>
    <q1:AmountOfDepositRefund>123.45</q1:AmountOfDepositRefund>
    <q1:PayerEORIForRefund>PR</q1:PayerEORIForRefund>
  </q1:DepositRefundDetails>
</q1:RD409>");
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"[RD409 – Deposit Refund Application Decision] has been received and linked to job B00000012. Decision: Deposit Refund Application Rejected.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>ApplicationReferId</td></tr><tr><td>Date</td><td>20230811</td></tr><tr><td>Applicant EORI Number</td><td>AN</td></tr><tr><td>Deposit Refund Application Approved</td><td>N</td></tr><tr><td>Reason Not Approved</td><td>Reason Not Approved</td></tr><tr><td>Statement of the Decision Taking Customs Authority</td><td>Statement of the Decision Taking Customs Authority</td></tr></table><br />
<br />Amount of Deposit Refund<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Amount of Deposit Refund</td><td>Payer EORI for Refund</td></tr><tr><td>123.45</td><td>PR</td></tr></table>";

		protected override RD409Provider GetProvider(TextReader reader) => new RD409Provider(new MailBoxItemProvider<Rd409>(reader).Message);
	}
}
