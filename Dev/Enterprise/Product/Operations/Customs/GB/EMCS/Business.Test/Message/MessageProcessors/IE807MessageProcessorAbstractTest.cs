using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE807MessageProcessor))]
	abstract class IE807MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE807MessageProcessor, IIE807>
	{
		protected abstract void UpdateAdministrativeReferenceCode();
		public void TestEndToEndProcessing_WhenNoDeclarationMatched()
		{
			UpdateAdministrativeReferenceCode();
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

		protected abstract void UpdateComplementaryInformation();
		public void TestGenerateEmail_NoComplementaryInformation()
		{
			UpdateComplementaryInformation();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);

			MessageProcessorNotificationTestHelper.AssertEmailNotContains("EMCS Interruption of Movement", new[] { "Complementary Information" });
		}

		protected abstract void UpdateReferenceControlReport();
		public void TestGenerateEmail_NoControlReportNumber()
		{
			UpdateReferenceControlReport();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);

			MessageProcessorNotificationTestHelper.AssertEmailNotContains("EMCS Interruption of Movement", new[] { "Control Report" });
		}

		protected abstract void UpdateReferenceEventReport();
		public void TestGenerateEmail_NoEventReportNumber()
		{
			UpdateReferenceEventReport();

			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);

			MessageProcessorNotificationTestHelper.AssertEmailNotContains("EMCS Interruption of Movement", new[] { "Event Report" });
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE807;

		protected override ZString MessageText => EMCSXmlObjectSerializer.Serialize(ie807);

		protected override IE807MessageProcessor Processor => new IE807MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set INT.", EntryStatusList.Codes.INT, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Interruption of Movement", new[] { $"Your EMCS Declaration for Job {declaration.JE_DeclarationReference} received a notification that the movement has been interrupted. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}

		protected abstract TMessageType CreateDefaultIE807Type();
		protected TMessageType ie807;

		protected override void SetUp()
		{
			base.SetUp();
			ie807 = CreateDefaultIE807Type();
		}
	}
}
