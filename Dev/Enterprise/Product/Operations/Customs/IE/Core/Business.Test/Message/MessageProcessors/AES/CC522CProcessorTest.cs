using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC522C;
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
	[TestedType(typeof(CC522CProcessor))]
	class CC522CProcessorTest : ExitControlMessageProcessorTest<CC522CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC522CProvider>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.ExitReleaseRejected, messageAttachee.CER_Status);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, messageAttachee.CER_MessageStatus);
			var jobNumber = messageAttachee.Header.CXH_JobReference;
			var messageInterpretation = $@"An Exit Release Rejection message has been received from Customs for Job {jobNumber} through the IE522 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit rejection motivation code</td><td>B</td></tr><tr><td>Exit rejection motivation</td><td>Exit Rejection Motivation B</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC522C: EXIT RELEASE REJECTION";

		protected override CC522CProcessor Processor => new CC522CProcessor(logger, typeof(Cc522C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE522;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC522CText("21IEDUB11A782454R2");
	}
}
