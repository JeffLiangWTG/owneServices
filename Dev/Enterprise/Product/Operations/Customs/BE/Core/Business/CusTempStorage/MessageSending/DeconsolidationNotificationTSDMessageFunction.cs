using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class DeconsolidationNotificationTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => PNTSEntryTypeList.Codes.DeconsolidationNotification;
}