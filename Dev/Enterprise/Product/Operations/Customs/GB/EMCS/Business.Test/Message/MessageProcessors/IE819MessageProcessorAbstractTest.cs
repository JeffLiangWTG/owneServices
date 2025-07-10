using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE819MessageProcessor))]
	abstract class IE819MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE819MessageProcessor, IIE819>
	{
		public void TestEndToEndProcessing_WhenIsConsignee()
		{
			base.CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("JE_MessageStatus should have been set RCV", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set ALT.", EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of Alert or Rejection", new[] { "Your EMCS Declaration for Job E00000810 received an Alert or Rejection. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE819;

		protected override IE819MessageProcessor Processor => new IE819MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set ALT.", EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of Alert or Rejection", new[] { "Your EMCS Declaration for Job E00000810 received an Alert or Rejection. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
