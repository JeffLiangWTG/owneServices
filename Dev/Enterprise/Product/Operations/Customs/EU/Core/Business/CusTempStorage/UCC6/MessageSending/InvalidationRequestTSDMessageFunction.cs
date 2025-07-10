using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class InvalidationRequestTSDMessageFunction : TemporaryStorageMessageFunction
{
	public InvalidationRequestTSDMessageFunction(TemporaryStorageMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}
	readonly TemporaryStorageMessageSendingObject messageSendingObject;

	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD;

	public ZString VOCReason => messageSendingObject.VOCReason;
}
