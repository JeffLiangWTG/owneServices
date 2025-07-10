using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class MessageProcessorTest<TMessageProcessor, T> : TestCaseWithFactory
		where TMessageProcessor : MessageProcessor<T>
	{
		protected TMessageProcessor Processor { get; private set; }

		public void TestPreProcessMessage_WithExistingLinkedObject()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, "1000");

			var incomingMessage = GetIncomingMessage("0001");
			incomingMessage.EM_LinkedObject = manifestHeader;

			Processor.PreProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();

				AssertEquals("Log", string.Empty, concatenatedUserLogStrings);
				AssertEquals("EM_Status", Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, incomingMessage.EM_Status);
			});
		}

		public void TestPreProcessMessageFailed_WhenLinkedObjectIsNull()
		{
			const int RetryCount = 2;
			CombineAssertions("pre process failed", () =>
			{
				var incomingMessage = GetIncomingMessageWithInvalidLinkedObject();

				for (var  i = 0; i <= MessageProcessor<T>.RetryCount; i++)
				{
					Processor.PreProcessMessage(incomingMessage);

					if (i < RetryCount)
					{
						AssertEquals("Status should be unchanged to allow retrying", Messaging.Integration.EDIMessageStatusList.Codes.Queued, incomingMessage.EM_Status);
						AssertEquals("Retry count should be increased", i + 1, incomingMessage.EM_RetryCount);
						AssertLessThan("HeldUntil should be updated to a future time", ZDateTime.UtcNow, incomingMessage.EM_HeldUntilDate);
					}
					else
					{
						AssertEquals("Status should be set discarded after all retries", Messaging.Integration.EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
						AssertEquals("Retry count should be reset", 0, incomingMessage.EM_RetryCount.ToZInt());

						var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
						var noteForDiscardedMessage = string.Format("Unable to find manifest header for message (Number:{0}, Type:{1}); message status set to DISCARDED.",
																													incomingMessage.EM_MessageNum,
																													incomingMessage.EM_MessageType);
						AssertContains("Should log for discarded message", noteForDiscardedMessage, concatenatedUserLogStrings);
						AssertEquals("Should add note for discarded message", noteForDiscardedMessage, incomingMessage.Notes.FindByDescription("Customs Message Error").SingleOrDefault()?.GetUnSerializeNoteText());
					}
				}
			});
		}

		public void TestProcessSkippedOnPreProcessingError()
		{
			CombineAssertions(() =>
			{
				var incomingMessage = GetIncomingMessageWithInvalidLinkedObject();
				Processor.ProcessMessage(incomingMessage);
				AssertEquals("Process should quit when status was not set to PreProcessedOK", Messaging.Integration.EDIMessageStatusList.Codes.Queued, incomingMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_DeserializeEM_MessageText_ShouldUseTextOrEvenBetterUseMessageDataAsItsCompressed_True()
		{
			var (incomingMessage, header) = PrepareDataToEnableGetLinkedObjectViaSessionGuid();
			var incomingMessageInDifferentFactory = NewFactory().Load<TestEdiMessage>(incomingMessage.PK);
			Processor.PreProcessMessage(incomingMessageInDifferentFactory);
			AssertEquals(EDIMessageStatusList.Codes.PreProcessedOK, incomingMessageInDifferentFactory.EM_Status);
		}

		public void TestPreProcessMessage_DeserializeEM_MessageText_ShouldUseTextOrEvenBetterUseMessageDataAsItsCompressed_False()
		{
			using(SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (incomingMessage, header) = PrepareDataToEnableGetLinkedObjectViaSessionGuid();
				if (!incomingMessage.EM_MessageData.IsEmpty && incomingMessage.EM_MessageText.IsEmpty)
				{
					incomingMessage.EM_MessageText = MessageEncoding.UTF8WithoutBOM.GetString(incomingMessage.EM_MessageData);
					incomingMessage.EM_MessageData = ZBlob.Empty;
				}
				var incomingMessageInDifferentFactory = NewFactory().Load<TestEdiMessage>(incomingMessage.PK);
				Processor.PreProcessMessage(incomingMessageInDifferentFactory);
				AssertEquals(EDIMessageStatusList.Codes.PreProcessedOK, incomingMessageInDifferentFactory.EM_Status);
			}
		}

		protected virtual TestEdiMessage GetIncomingMessageWithInvalidLinkedObject()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, "112233");

			return GetIncomingMessage("223344");
		}

		public void TestPreProcessMessage_FindManifestHeaderBySessionGUID()
		{
			var (incomingMessage, header) = PrepareDataToEnableGetLinkedObjectViaSessionGuid();
			var incomingMessageInDifferentFactory = NewFactory().Load<TestEdiMessage>(incomingMessage.PK);
			Processor.PreProcessMessage(incomingMessageInDifferentFactory);

			CombineAssertions(() =>
			{
				AssertEquals("Pre Process Success", Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, incomingMessageInDifferentFactory.EM_Status);
				AssertEquals("Should find the manifest header from the SessionGUID of incomingInterchange.", header.PK, incomingMessageInDifferentFactory.EM_LinkUniqueID);
				AssertEquals("Retry count should be reset", 0, incomingMessageInDifferentFactory.EM_RetryCount.ToZInt());
			});
		}

		protected string GetAllConcatenatedUserLogStrings() => string.Concat(logger.UserLogStrings.Cast<string>());

		protected abstract TestEdiMessage GetIncomingMessage(string primaryReferenceNumber);

		protected virtual string StatusOfOutgoingEDIMessageToFindHeaderBySessionGUID => EDIMessage.Status.Sent;

		protected override void SetUp()
		{
			base.SetUp();

			logger = new LoggingInformation();
			Processor = GetNewResponseMessageProcessor(logger);
		}

		LoggingInformation logger;

		protected abstract TMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger);

		(TestEdiMessage, AsycudaManifestHeader) PrepareDataToEnableGetLinkedObjectViaSessionGuid()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var outgoingMessage = Factory.NewWithValidTestData<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IC2;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = StatusOfOutgoingEDIMessageToFindHeaderBySessionGUID;
			outgoingMessage.EM_MessageNum = "ICS2TST00001";
			outgoingMessage.EM_LinkedObject = manifestHeader;

			var outboundInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outboundInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			outboundInterchange.EI_Status = EDIInterchange.Status.Sent;
			outboundInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			outboundInterchange.EI_InterchangeNum = "ICS2TST00001";
			outboundInterchange.ContainedMessages.Add(outgoingMessage);

			var incomingMessage = GetIncomingMessage("TEST012345");
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			incomingMessage.EM_MessageNum = "ICS2TST00002";

			var incomingInterchange = incomingMessage.Interchange ?? Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			incomingInterchange.EI_SessionGUID = outboundInterchange.EI_SessionGUID;
			incomingInterchange.EI_InterchangeNum = "ICS2TST00002";
			incomingMessage.EM_EI = incomingInterchange.PK;
			Factory.Save();
			return (incomingMessage, manifestHeader);
		}
	}
}
