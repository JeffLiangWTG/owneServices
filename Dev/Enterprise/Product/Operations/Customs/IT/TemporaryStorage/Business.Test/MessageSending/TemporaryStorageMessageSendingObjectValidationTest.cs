using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
{
	public void TestValidateShouldSend()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var sendingObject = new TemporaryStorageMessageSendingObject(header);

		header.AMA_JobReference = "TS001";
		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.TechnicalFailure;
		header.CustomsStatus = PNTSCustomsStatusList.Codes.PartialActivated;
		sendingObject.ShouldSend = true;
		AssertNoWarnings(sendingObject.ShouldSendInfo);

		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;
		sendingObject.ShouldSend = true;
		var expectedWarningMessage = $"The Entry {header.AMA_JobReference} was already sent and is already registered or waiting for a message from Customs. Resending this entry could result in a duplicated declaration.";
		AssertHasWarningContaining(sendingObject.ShouldSendInfo, expectedWarningMessage);

		sendingObject.ShouldSend = false;
		AssertNoWarnings(sendingObject.ShouldSendInfo);
	}
}
