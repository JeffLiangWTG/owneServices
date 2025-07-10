using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using RD416Provider = Enterprise.Customs.IE.Messaging.UCC5.RD416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RD416MessageInterpreter))]
	class RD416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, RD416MessageInterpreter, RD416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD416;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(GenerateMessage()));
		}

		protected override RD416Provider GetProvider(TextReader reader) => new RD416Provider(new MailBoxItemProvider<Rd416>(reader).Message);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Deposit Refund Application Rejection (RD416) has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u8</td></tr><tr><td>Rejection Date</td><td>02-Aug-23</td></tr><tr><td>Rejection Reason</td><td>Test Reason</td></tr><tr><td>Applicant</td><td>XY</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>Missing value</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>Element too short</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";

		Rd416 GenerateMessage()
		{
			return new Rd416
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u8",
					RejectionDate = "20230802",
					RejectionReason = "Test Reason",
					Applicant = "XY",
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			};
		}
	}
}
