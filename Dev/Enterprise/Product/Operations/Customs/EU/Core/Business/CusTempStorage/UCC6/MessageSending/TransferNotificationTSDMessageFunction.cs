using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class TransferNotificationTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.TransferNotificationTSD;
}
