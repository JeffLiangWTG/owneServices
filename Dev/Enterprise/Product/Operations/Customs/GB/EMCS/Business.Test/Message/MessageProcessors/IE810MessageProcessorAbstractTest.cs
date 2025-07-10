using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE810MessageProcessor))]
	abstract class IE810MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE810MessageProcessor, IIE810>
	{
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
					AssertEquals("JE_MessageStatus should not be set when is consignee.", ZString.Empty, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set CAN.", EntryStatusList.Codes.CAN, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					MessageProcessorNotificationTestHelper.AssertEmail("EMCS Cancellation of e-AD Response", new[] { "Your EMCS Declaration for Job E00000810 has been canceled. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE810;

		protected override IE810MessageProcessor Processor => new IE810MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_MessageStatus should have been set RCV when is consignor.", EDIMessageStatusList.Codes.Received, declaration.JE_MessageStatus);
			AssertEquals("JE_EntryStatus should have been set CAN.", EntryStatusList.Codes.CAN, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Cancellation of e-AD Response", new[] { "Your EMCS Declaration for Job E00000810 has been canceled. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
