using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM862;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM862Processor))]
	class IM862ProcessorTest : EntryHeaderMessageProcessorTest<IM862Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM862Provider>
	{
		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM862;
		protected override IM862Processor Processor => new IM862Processor(logger, typeof(Im862));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM862;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => AISInterchangeProcessorTestHelper.GetStandardIM862Text("21IEDUB11A782454R2", "Reason", "123456789123456789123456789456123456");

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"A Declaration Amendment Request Cancellation (IM862) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>123456789123456789123456789456123456</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>Reason</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "Declaration Amendment Request Cancellation (IM862) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			(var declaration, var entry, var outgoingMessage, var incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = "<GREETING>HELLO</GREETING>";
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("MovementReferenceNumber", ZString.Empty, entry.MovementReferenceNumber);
					AssertEquals("CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
					AssertEquals("EM_Status", EDIMessage.Status.Failed, incomingMessage.EM_Status);
				});
			}
		}
	}
}
