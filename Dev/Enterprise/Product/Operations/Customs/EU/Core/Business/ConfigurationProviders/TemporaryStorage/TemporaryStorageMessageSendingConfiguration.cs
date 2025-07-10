using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingConfiguration
	{
		public BaseMessageSendingObjectParent GetNewMessageSendingObjectParent(TemporaryStorageHeader header) => GetNewMessageSendingObjectParentCore(header);

		protected virtual BaseMessageSendingObjectParent GetNewMessageSendingObjectParentCore(TemporaryStorageHeader header) => new TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);

		public TemporaryStorageMessageSendingObject GetNewMessageSendingObject(TemporaryStorageHeader header) => GetNewMessageSendingObjectCore(header);

		protected virtual TemporaryStorageMessageSendingObject GetNewMessageSendingObjectCore(TemporaryStorageHeader header) => new TemporaryStorageMessageSendingObject(header);
	}
}
