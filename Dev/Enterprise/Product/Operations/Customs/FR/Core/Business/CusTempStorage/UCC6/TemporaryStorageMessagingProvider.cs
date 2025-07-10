using Enterprise.Customs.EU.Business.CusTempStorage;
using EUCusTempStorage = Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageMessagingProvider : EUCusTempStorage.TemporaryStorageMessagingProvider
	{
		protected override TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction)
		{
			return new MessageSending.TemporaryStorageMessageBuilder(messageSendingObject, messageFunction);
		}
	}
}
