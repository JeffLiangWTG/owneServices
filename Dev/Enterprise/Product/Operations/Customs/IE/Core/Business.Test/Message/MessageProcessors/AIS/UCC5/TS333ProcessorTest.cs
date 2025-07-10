using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS333Processor))]
	sealed class TS333ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS333Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS333Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("Message status", LogicalStatusList.Codes.Invalid, messageAttachee.AMA_MessageStatus);
			AssertEquals("Customs Status", AISEntryStatusList.Codes.Rejected, messageAttachee.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A Presentation Notification Rejection (TS333) message has been received for Job MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Rejection Date</td><td>05-Sep-23</td></tr><tr><td>Rejection Reason</td><td>reason</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A Presentation Notification Rejection (TS333) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS333;

		protected override ZString MessageFriendlyName => "TS333: Presentation Notification (G3) Rejection";

		protected override TS333Processor Processor => new TS333Processor(logger, typeof(Ts333));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS333Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 30, 30), "reason");
	}
}
