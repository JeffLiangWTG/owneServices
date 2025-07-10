using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public abstract class FROutgoingMessageProcessorBaseTest<TOutgoingMessageProcessor> : TestCaseWithFactory
		where TOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public void TestMessagesCanBeProcessedCorrectly()
		{
			var messageAllSatisfied = GetMessageAllSatisfied();

			var messageUnsatisfiedApplicationCode = GetUnsatisfiedMessage(msg => msg.EM_ApplicationCode = meaninglessPlaceHolder);

			var messageUnsatisfiedReceiveTransmit = GetUnsatisfiedMessage(msg => msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive);

			var messageUnsatisfiedStatus = GetUnsatisfiedMessage(msg => msg.EM_Status = EDIMessageStatusList.Codes.Cancelled);

			var messageUnsatisfiedBranch = GetUnsatisfiedMessage(
				msg =>
				{
					var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
					var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
					anotherBranch.GB_GC = anotherCompany.PK;
					msg.EM_GB = anotherBranch.PK;
				});

			var messageUnsatisfiedMessageType = GetUnsatisfiedMessage(msg => msg.EM_MessageType = meaninglessPlaceHolder);
			Factory.Save();

			var processor = (TOutgoingMessageProcessor)Activator.CreateInstance(typeof(TOutgoingMessageProcessor), new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			messageAllSatisfied.Reload();
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals($"Only one messageAllSatisfied can be processed by {nameof(TOutgoingMessageProcessor)}.", 1, interchanges.Length);
			var interchangeCreated = interchanges.First();

			MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchangeCreated,
			expectedApplicationCode: "GMD",
			expectedInterchangeType: ExpectedInterchangeType,
			expectedReceiveTransmit: "TRX",
			expectedStatus: "HQU",
			expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			expectedTo: FRCustomsDataRegistry.Instance.RecipientID.Value,
			expectedBodyText: ExpectedInterchangeBodyText(messageAllSatisfied),
			expectedHeaderText: $"<SenderID>EDIEDIDAT</SenderID><RecipientID>EASYLO2TEST_EAD</RecipientID><InterchangeType>{ExpectedInterchangeType}</InterchangeType><InterchangeNumber>",
			message: $"MessageType:{MessageType}&MessageSubType:{MessageSubType} Interchange properties");

			CombineAssertions(() =>
			{
				AssertEquals("Created interchange should be linked to input message.", interchangeCreated.PK, messageAllSatisfied.EM_EI);
				AssertEquals("messageAllSatisfied.EM_Status should be SNT.", EDIMessageStatusList.Codes.Sent, messageAllSatisfied.EM_Status);

				messageUnsatisfiedApplicationCode.Reload();
				messageUnsatisfiedReceiveTransmit.Reload();
				messageUnsatisfiedStatus.Reload();
				messageUnsatisfiedBranch.Reload();
				messageUnsatisfiedMessageType.Reload();
				AssertEquals("messageUnsatisfiedApplicationCode.EM_Status should be QUE.", EDIMessageStatusList.Codes.Queued, messageUnsatisfiedApplicationCode.EM_Status);
				AssertEquals("messageUnsatisfiedReceiveTransmit.EM_Status should be QUE.", EDIMessageStatusList.Codes.Queued, messageUnsatisfiedReceiveTransmit.EM_Status);
				AssertEquals("messageUnsatisfiedStatus.EM_Status should be CAN.", EDIMessageStatusList.Codes.Cancelled, messageUnsatisfiedStatus.EM_Status);
				AssertEquals("messageUnsatisfiedBranch.EM_Status should be QUE.", EDIMessageStatusList.Codes.Queued, messageUnsatisfiedBranch.EM_Status);
				AssertEquals("messageUnsatisfiedMessageType.EM_Status should be QUE.", EDIMessageStatusList.Codes.Queued, messageUnsatisfiedMessageType.EM_Status);
			});
		}

		TestEDIMessage GetMessageAllSatisfied()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CorrelationID = "0000005856";

			var message = Factory.NewWithValidTestData<TestEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = MessageText;
			message.EM_MessageType = MessageType;
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageSubType = MessageSubType;

			return message;
		}

		protected virtual ZString MessageText => "MessageText";

		protected virtual ZString ExpectedInterchangeBodyText(TestEDIMessage message) => message.EM_MessageText;

		protected abstract ZString MessageType { get; }

		protected abstract ZString MessageSubType { get; }

		protected abstract ZString ExpectedInterchangeType { get; }

		TestEDIMessage GetUnsatisfiedMessage(Action<TestEDIMessage> corruptiveAction)
		{
			var unsatisfiedMessage = GetMessageAllSatisfied();
			corruptiveAction(unsatisfiedMessage);

			return unsatisfiedMessage;
		}

		const string meaninglessPlaceHolder = "XXX";
	}
}
