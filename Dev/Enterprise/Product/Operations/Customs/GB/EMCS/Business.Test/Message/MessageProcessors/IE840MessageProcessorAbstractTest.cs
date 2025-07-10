using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE840MessageProcessor))]
	abstract class IE840MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE840MessageProcessor, IIE840>
	{
		protected abstract void UpdateAdministrativeRefCode();
		public void TestEndToEndProcessing_WhenNoDeclarationMatched()
		{
			UpdateAdministrativeRefCode();
			var incomingMessage = CreateNewIncomingMessage();
			Processor.PreProcessMessage(incomingMessage);
			ProcessMessage(incomingMessage);
			CreateSetupData();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, incomingMessage.EM_Status);
			});
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE840;

		protected override ZString MessageText => EMCSXmlObjectSerializer.Serialize(ie840);

		protected override IE840MessageProcessor Processor => new IE840MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set EVT.", EntryStatusList.Codes.EVT, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Event Report", new[] { $"Your EMCS Declaration for Job {declaration.JE_DeclarationReference} has received an Event Report. For details please follow the Link to the Job." }, new string[] { "staff1@where.com" });
		}

		protected abstract TMessageType CreateDefaultIE840Type();
		protected TMessageType ie840;

		protected override void SetUp()
		{
			base.SetUp();
			ie840 = CreateDefaultIE840Type();
		}
	}
}
