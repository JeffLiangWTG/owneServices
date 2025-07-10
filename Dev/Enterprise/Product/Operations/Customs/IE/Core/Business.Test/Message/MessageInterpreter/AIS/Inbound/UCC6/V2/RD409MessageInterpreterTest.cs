using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RD409MessageInterpreter))]
	sealed class RD409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RD409MessageInterpreter, RD409Provider>
	{
		public void TestWhenInvalidationDecisionIsFalse()
		{
			var message = AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(RD409ProcessorTest.GenerateMessage(approved: false, "Reason")));
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals(ExpectedInterpretation(approved: false, "Reason").RemoveLineBreakingsAndIndents(), interpreter.GetInterpretation().RemoveLineBreakingsAndIndents());
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD409;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(RD409ProcessorTest.GenerateMessage(approved: true, null)));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => ExpectedInterpretation(approved: true, "&nbsp;");

		protected override RD409Provider GetProvider(TextReader reader) => new RD409Provider(new MailBoxItemProvider<Rd409>(reader).Message);

		internal static ZString ExpectedInterpretation(ZBool approved, string reasonNotApproved) => $@"[RD409 – Deposit Refund Application Decision] has been received and linked to job B00001000. Decision: Deposit Refund Application {DecisionWord(approved)}.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Application Reference ID</td><td>ApplicationReferId</td></tr>
				<tr><td>Date</td><td>20230811</td></tr>
				<tr><td>Applicant EORI Number</td><td>AN</td></tr>
				<tr><td>Deposit Refund Application Approved</td><td>{approved}</td></tr>
				<tr><td>Reason Not Approved</td><td>{reasonNotApproved}</td></tr>
				<tr><td>Statement of the Decision Taking Customs Authority</td><td>Statement of the Decision Taking Customs Authority</td></tr>
			</table>";

		static string DecisionWord(ZBool approved) => approved ? "Accepted" : "Rejected";
	}
}
