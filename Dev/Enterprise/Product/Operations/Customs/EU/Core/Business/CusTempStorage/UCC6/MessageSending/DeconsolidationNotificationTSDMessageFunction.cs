using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class DeconsolidationNotificationTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.DeconsolidationNotificationTSD;
}
