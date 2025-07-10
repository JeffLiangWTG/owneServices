using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RF409MessageInterpreter))]
	sealed class RF409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RF409MessageInterpreter, RF409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF409;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(RF409ProcessorTest.GenerateMessage()));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => ExpectedInterpretation;

		protected override RF409Provider GetProvider(TextReader reader) => new RF409Provider(new MailBoxItemProvider<Rf409Type>(reader).Message);

		internal static ZString ExpectedInterpretation => @"Deposit Refund Application Decision (RF409) has been received and linked to job B00001000. Decision: Refund Application Rejected.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr>
				<tr><td>Application Decision Code Type</td><td>5</td></tr>
				<tr><td>Refund Application Accepted</td><td>N</td></tr>
				<tr><td>Decision Taking Customs Authority</td><td>Au123456</td></tr>
				<tr><td>MRN</td><td>IE2345678912345678</td></tr>
				<tr><td>Time limit for completion of formalities</td><td>102</td></tr>
				<tr><td>Statement of the Decision Taking Customs Authority</td><td>STA_OF_DEC</td></tr>
				<tr><td>Description of Grounds</td><td>DESC_OF_GROUNDS</td></tr>
			</table><br />
			<br />
			General Remarks<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence</td><td>General Remarks</td></tr>
				<tr><td>1</td><td>ETU</td></tr>
				<tr><td>2</td><td>RM2</td></tr>
			</table>";
	}
}
