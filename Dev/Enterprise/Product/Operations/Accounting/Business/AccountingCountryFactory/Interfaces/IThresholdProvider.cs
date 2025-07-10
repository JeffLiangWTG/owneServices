using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IThresholdProvider
	{
		ZDecimal GetThresholdAmount();
	}
}
