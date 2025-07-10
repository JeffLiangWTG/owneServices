using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

sealed class TemporaryStorageMessagingProviderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessagingProviderTest<TemporaryStorageMessagingProvider>
{
	public new void TestDefaultingOfMessageType() => CombineAssertions(() =>
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
		header.CustomsStatus = "XYZ";
		var sendingObject = new TemporaryStorageMessageSendingObject(header);
		AssertEquals("When Message Mode is 'TF' then the default message is 'TRF'", PNTSEntryTypeList.Codes.TransferNotification, sendingObject.MessageType);

		header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
		header.CustomsStatus = "XYZ";
		sendingObject = new TemporaryStorageMessageSendingObject(header);
		AssertEquals("When Message Mode is 'DC' then the default message is 'DCN'", PNTSEntryTypeList.Codes.DeconsolidationNotification, sendingObject.MessageType);

		header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
		sendingObject = new TemporaryStorageMessageSendingObject(header);
		AssertEquals("When Message Mode is 'TC' then the default message is '115'", PNTSEntryTypeList.Codes.CombinedTemporaryStorage, sendingObject.MessageType);
	});

	public new void TestSendMessage()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var provider = GetProvider(header);

		var sendingObjectCTS = new TemporaryStorageMessageSendingObject(header);
		sendingObjectCTS.MessageType = PNTSEntryTypeList.Codes.CombinedTemporaryStorage;
		var sendingObjectDCN = new TemporaryStorageMessageSendingObject(header);
		sendingObjectDCN.MessageType = PNTSEntryTypeList.Codes.DeconsolidationNotification;

		CombineAssertions("Check messages can be sent correctly", () =>
		{
			provider.SendMessage(sendingObjectCTS, new SendsMessagesToCustomsShutterUpperer(), null);
			AssertEquals(1, header.Messages.Count);
			provider.SendMessage(sendingObjectDCN, new SendsMessagesToCustomsShutterUpperer(), null);
			AssertEquals(2, header.Messages.Count);
		});
	}

	protected override TemporaryStorageMessagingProvider GetProvider(EU.Business.CusTempStorage.TemporaryStorageHeader header) => header.MessagingProvider as TemporaryStorageMessagingProvider;
}
