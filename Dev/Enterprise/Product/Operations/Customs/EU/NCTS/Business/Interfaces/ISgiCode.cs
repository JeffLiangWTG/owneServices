using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface ISgiCode
	{
		ZString Code { get; }
		ZDecimal Qty { get; }
	}
}