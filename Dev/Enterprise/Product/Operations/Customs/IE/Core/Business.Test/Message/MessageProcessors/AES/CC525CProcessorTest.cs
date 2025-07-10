using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC525C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC525CProcessor))]
	class CC525CProcessorTest : ExitControlMessageProcessorTest<CC525CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC525CProvider>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.ReleasedForExit, messageAttachee.CER_Status);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, messageAttachee.CER_MessageStatus);
			var jobNumber = messageAttachee.Header.CXH_JobReference;
			var messageInterpretation = $@"An Exit Release notification message has been received from Customs for Job {messageAttachee.Header.CXH_JobReference} through the IE525 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Released for Exit</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Release Date</td><td>18-Sep-71</td></tr><tr><td>Storing Flag</td><td>N</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC525C: EXIT RELEASE NOTIFICATION";

		protected override CC525CProcessor Processor => new CC525CProcessor(logger, typeof(Cc525C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE525;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC525CText("21IEDUB11A782454R2", "0");
	}
}
