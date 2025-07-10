using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class CombinedTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.CombinedTSD;
}
