using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE819MessageProcessor))]
	abstract class IE819MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE819MessageProcessor, IIE819>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE819;

		protected override ZString MessageFriendlyName => "EMCS IE819 Message Processor";

		protected override IE819MessageProcessor Processor => new IE819MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set ALT.", EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Notification of Alert or Rejection",
				new[] { "Your EMCS Declaration for Job E00000810 received an Alert or Rejection. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void AssertEndToEndProcessing()
		{
			AssertEquals("JE_EntryStatus should have been set ALT.", EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Notification of Alert or Rejection",
				new[] { "Your EMCS Declaration for Job E00000810 received an Alert or Rejection. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}

		public void TestEndToEndProcessing_WhenIsConsignee()
		{
			CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("JE_MessageStatus should have been set RCV", EDIMessage.Status.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set ALT.", EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
					MessageProcessorNotificationTestHelper.AssertEmail(
						"EMCS Notification of Alert or Rejection",
						new[] { "Your EMCS Declaration for Job E00000810 received an Alert or Rejection. For details please follow the link to the Job." },
						new string[] { "staff1@where.com" });
				});
			}
		}
	}
}
