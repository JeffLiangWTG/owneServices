using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC561C;
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
	[TestedType(typeof(CC561CProcessor))]
	class CC561CProcessorTest : ExitControlMessageProcessorTest<CC561CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC561CProvider>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.ControlledForExit, messageAttachee.CER_Status);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, messageAttachee.CER_MessageStatus);

			var jobNumber = messageAttachee.Header.CXH_JobReference;
			var messageInterpretation = $@"An Exit Control Decision Notification message has been received from Customs for Job {jobNumber} through the IE561 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Notification Date &amp; Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Control Type</td><td>10 - Documentary controls</td></tr><tr><td>Control Text</td><td>Documentary Control Type 1</td></tr><tr><td>Control Type</td><td>20 - Documentary controls 2</td></tr><tr><td>Control Text</td><td>Documentary Control Type 2</td></tr><tr><td>Control Type</td><td>50 - Other</td></tr><tr><td>Control Text</td><td>Documentary Control Type 3</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override (CusExitHeader declaration, CusExitReport messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "20", "Documentary controls 2");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "50", "Other");
			Factory.Save();
			return base.CreateSetupData();
		}

		protected override ZString MessageFriendlyName => "CC561C: EXIT CONTROL DECISION NOTIFICATION";

		protected override CC561CProcessor Processor => new CC561CProcessor(logger, typeof(Cc561C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE561;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC561CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
