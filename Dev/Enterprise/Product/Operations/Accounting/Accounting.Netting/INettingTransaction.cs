using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public interface INettingTransaction
	{
		ZGuid PK { get; }
		ZGuid NettingSystemPK { get; set; }
		ZGuid NettingPeriodPK { get; set; }
		ZGuid IssuerPK { get; set; }
		ZGuid RecipientPK { get; set; }
		ZString Currency { get; set; }
		ZDecimal Amount { get; set; }
		ZString Reference { get; set; }
		ZGuid OriginalTransaction { get; set; }
		ZDateTime Date { get; set; }
		ZDateTime DueDate { get; set; }
		ZString ApprovalStatus { get; set; }
		ZString TransactionType { get; set; }
		INettingTransactionLine AddNewLine();
		INettingTransactionReference AddNewTransactionReference();
		void DeleteLines();
		void DeleteTransactionReferences();
	}
}
