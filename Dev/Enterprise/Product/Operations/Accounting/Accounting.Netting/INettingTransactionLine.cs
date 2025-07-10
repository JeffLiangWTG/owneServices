using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public interface INettingTransactionLine
	{
		ZGuid PK { get; }
		ZGuid NettingTransactionPK { get; }
		ZString JobReference { get; set; }
		ZDecimal Amount { get; set; }
		ZString TransactionCurrency { get; set; }
		ZBool IsApproved { get; set; }
		ZGuid NettingPeriodPK { get; }
		INettingTransactionLineReference AddNewLineReference();
	}
}
