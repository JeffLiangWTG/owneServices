using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.EHO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

sealed class EHOMessageSendingTests : eHubMessaging.Tests.EndToEndMessageProcessing.EHO.EHOMessageSendingTests
{
	protected override ConfigurationForTestFailureSendingMessage GetConfigurationDataForTestFailureSendingMessage() => new ConfigurationForTestFailureSendingMessage(Core.Constants.CountryCodes.Italy, "ITM", SADConstants.CustomsInterchangeType.IdocR);

	protected override EDIMessage GetNewEDIMessageForTestFailureSendingMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "AWO";

		var messageNumber = "1";
		var messageText = "TEXT";

		var ediMessage = entryHeader.Messages.AddNew();
		ediMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		ediMessage.EM_Status = EDIMessage.Status.Queued;
		ediMessage.EM_MessageNum = messageNumber;
		ediMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber);
		ediMessage.EM_MessageText = messageText ?? "TEST MESSAGE TEXT " + messageNumber;

		ediMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;

		return ediMessage;
	}

	protected override void AssertProcessedInterchangeInTestFailureSendingMessage(EDIInterchange processedInterchange)
	{
		AssertEquals("[PRE-CONDITION] Interchange.ContainedMessages Count", 1, processedInterchange.ContainedMessages.Count);
		var message = processedInterchange.ContainedMessages[0];
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		AssertNotNull("[PRE-CONDITION] Linked Entry Header should be not null", entryHeader);
		AssertEquals("CH_EntryStatus", "FFT", entryHeader.CH_Status);
	}
}
