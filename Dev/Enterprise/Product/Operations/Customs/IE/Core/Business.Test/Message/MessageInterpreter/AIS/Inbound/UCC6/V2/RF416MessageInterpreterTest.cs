using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RF416MessageInterpreter))]
	sealed class RF416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RF416MessageInterpreter, RF416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF416;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(RF416ProcessorTest.GenerateMessage()));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => ExpectedInterpretation;

		protected override RF416Provider GetProvider(TextReader reader) => new RF416Provider(new MailBoxItemProvider<Rf416>(reader).Message);

		internal static ZString ExpectedInterpretation => $@"A Refund Application Rejection (RF416) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr>
				<tr><td>Rejection Date and Time</td><td>02-Aug-23</td></tr>
				<tr><td>Rejection Reason</td><td>Test Reason</td></tr>
				<tr><td>Decision Taking Customs Authority</td><td>IE123456</td></tr>
			</table><br />
			<br />
			{AISInterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation}";
	}
}
