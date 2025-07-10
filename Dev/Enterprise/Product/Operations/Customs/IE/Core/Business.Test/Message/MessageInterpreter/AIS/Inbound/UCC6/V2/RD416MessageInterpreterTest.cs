using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RD416MessageInterpreter))]
	sealed class RD416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RD416MessageInterpreter, RD416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD416;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(RD416ProcessorTest.GenerateMessage()));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => ExpectedInterpretation;

		protected override RD416Provider GetProvider(TextReader reader) => new RD416Provider(new MailBoxItemProvider<Rd416Type>(reader).Message);

		internal static ZString ExpectedInterpretation => $@"A Deposit Refund Application Rejection (RD416) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u8</td></tr>
				<tr><td>Rejection Date</td><td>02-Aug-23</td></tr>
				<tr><td>Rejection Reason</td><td>Test Reason</td></tr>
				<tr><td>Applicant EORI Number</td><td>XY</td></tr>
			</table><br />
			<br />
			{AISInterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation}";
	}
}
