using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using TS333Provider = Enterprise.Customs.IE.Messaging.UCC5.TS333Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS333MessageInterpreter))]
	sealed class TS333MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS333MessageInterpreter, TS333Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS333;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(Factory, messageText: AISUCC5InterchangeProcessorTestHelper.GetTS333Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 30, 30), "test reason"));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Presentation Notification Rejection (TS333) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Rejection Date</td><td>05-Sep-23</td></tr><tr><td>Rejection Reason</td><td>test reason</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>Missing value</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>Element too short</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";

		protected override TS333Provider GetProvider(TextReader reader) => new TS333Provider(new MailBoxItemProvider<Ts333>(reader).Message);
	}
}
