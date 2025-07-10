using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class CcpHelper
{
	public static ZString GetWarehouseCodeFromCcp(ZString ccpValue)
	{
		return ccpValue.SubstringSafe(1, 7);
	}
}
