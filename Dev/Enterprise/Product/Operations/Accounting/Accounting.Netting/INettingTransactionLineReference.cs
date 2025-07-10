using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public interface INettingTransactionLineReference
	{
		ZString Type { get; set; }
		ZString Reference { get; set; }
		ZGuid TransactionLinePK { get; }
		ZGuid NettingPeriodPK { get; }
	}
}
