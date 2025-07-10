using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class AmendmentRequestTSDMessageFunction : TemporaryStorageMessageFunction
{
	protected override ZString MessageTypeCore => TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD;
}
