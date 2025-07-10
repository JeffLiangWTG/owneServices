using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class PreLodgedTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
}
