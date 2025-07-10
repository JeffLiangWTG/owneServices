using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE881MessageProcessor))]
	abstract class IE881MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE881MessageProcessor, IIE881>
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
				AssertEquals("Declaration - Message Status", ZString.Empty, declaration.JE_MessageStatus);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, incomingMessage.EM_Status);
			});
		}

		protected abstract void UpdateRequestRejectedReason2_1();
		public void TestProcessMessageCore_MessageProcessed_RequestRejected()
		{
			UpdateRequestRejectedReason2_1();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Empty Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message - Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				MessageProcessorNotificationTestHelper.AssertEmail("EMCS Manual Closure Rejected", new[] { $"Your request for a Manual Closure of Job {declaration.JE_DeclarationReference} has been rejected. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
			});
		}

		protected abstract void UpdateRejectedReason2_0();
		public void TestGenerateEmail_Rejected_RejectedReason_0()
		{
			var complementReason = "Just because we can muahahaha!";
			UpdateRejectedReason2_0();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Empty Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message - Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				MessageProcessorNotificationTestHelper.AssertEmail("EMCS Manual Closure Rejected", new[] { "Manual Closure Rejection Reason: 0 - Other", $"Rejection Complement: {complementReason}" }, new string[] { "staff1@where.com" });
			});
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE881;

		protected override ZString MessageText => EMCSXmlObjectSerializer.Serialize(ie881);

		protected override IE881MessageProcessor Processor => new IE881MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set MAN.", EntryStatusList.Codes.MAN, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Manual Closure Accepted", new[] { $"Your EMCS Declaration for Job {declaration.JE_DeclarationReference} has been closed manually. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}

		protected abstract TMessageType CreateDefaultIE881Type();
		protected TMessageType ie881;

		protected override void SetUp()
		{
			base.SetUp();
			ie881 = CreateDefaultIE881Type();
		}
	}
}
