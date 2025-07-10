using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class TemporaryStorageMessageSendingConfiguration : EU.Business.CusTempStorage.TemporaryStorageMessageSendingConfiguration
{
	protected override BaseMessageSendingObjectParent GetNewMessageSendingObjectParentCore(EU.Business.CusTempStorage.TemporaryStorageHeader header) => new TemporaryStorageMessageSendingObjectParent((TemporaryStorageHeader)header);

	protected override EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject GetNewMessageSendingObjectCore(EU.Business.CusTempStorage.TemporaryStorageHeader header) => new TemporaryStorageMessageSendingObject((TemporaryStorageHeader)header);
}
