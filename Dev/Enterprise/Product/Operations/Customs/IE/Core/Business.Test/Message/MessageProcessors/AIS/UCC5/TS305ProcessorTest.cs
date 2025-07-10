using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS305Processor))]
	sealed class TS305ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS305Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS305Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader header, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("AMA_MessageStatus", LogicalStatusList.Codes.Accepted, header.AMA_MessageStatus);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request Rejection (TS305) message has been received for TSD MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Rejection Date</td><td>05-Sep-23 00:00</td></tr><tr><td>Amendment Rejection Reason</td><td>Test Reason</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "An Amendment Request Rejection (TS305) message has been received for TSD MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "TS305: TSD Amendment Request Rejection";

		protected override TS305Processor Processor => new TS305Processor(logger, typeof(Ts305));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS305;

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS305Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 30, 30), "Test Reason");
	}
}
