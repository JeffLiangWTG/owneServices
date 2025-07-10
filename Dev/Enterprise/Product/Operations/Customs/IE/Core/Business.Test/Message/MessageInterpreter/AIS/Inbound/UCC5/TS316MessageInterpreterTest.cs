using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS316MessageInterpreter))]
	sealed class TS316MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS316MessageInterpreter, TS316Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.TS316;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "MAN0001000", AISUCC5InterchangeProcessorTestHelper.GetTS316Text("LRN001", new DateTime(2023, 09, 07)));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A TSD Rejection (TS316) message has been received for Job MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">TS316 – Temporary Storage Declaration Rejection message</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Declaration Rejection Date</td><td>07-Sep-23</td></tr><tr><td>Declaration Rejection Reason</td><td>reason</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>Missing value</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>Element too short</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";

		protected override TS316Provider GetProvider(TextReader reader) => new TS316Provider(new Messaging.MailBoxItemProvider<Ts316>(reader).Message);
	}
}
