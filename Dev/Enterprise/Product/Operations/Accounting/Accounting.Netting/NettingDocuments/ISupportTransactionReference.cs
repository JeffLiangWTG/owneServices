using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public interface ISupportTransactionReference
	{
		ZGuid TransactionPK { get; }
		NettingTransactionReference TransactionRef { get; set; }
		NettingTransactionLineReference TransactionLineRef { get; set; }
	}
}
