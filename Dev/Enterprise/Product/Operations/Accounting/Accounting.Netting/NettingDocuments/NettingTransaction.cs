using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingTransaction : NonPersistentBusinessObject, ISupportTransactionReference
	{
		public NettingTransaction()
			: base()
		{
		}

		public NettingTransaction(NettingStatement statement)
			: this()
		{
			Argument.NotNull(statement, "Statement");
			Statement = statement;
		}

		readonly NettingStatement Statement;
		public ZGuid TransactionPK { get; set; }

		public ZString Ledger { get; set; }
		public ZString OrgCode { get; set; }
		public ZString MatchingTransactionReference { get; set; }
		public ZString ParticipatingOrgCode { get; set; }
		public ZDecimal NettingSystemExchangeRate { get; set; }
		public ZDecimal ParticipantExchangeRate { get; set; }
		public ZString Currency { get; set; }
		public ZDecimal Amount { get; set; }
		public ZDecimal NettingSystemAmount { get; set; }
		public ZString NettingCurrency { get; set; }
		public ZString ParticipantCurrency { get; set; }
		public ZDecimal ParticipantAmount { get; set; }
		public ZDecimal ReceivableAmount { get; set; }
		public ZDecimal PayableAmount { get; set; }
		public ZString CompanyCode { get; set; }
		public ZString ParticipantCompanyCode { get; set; }
		public ZString ParticipantCompanyName { get; set; }
		public NettingTransactionReference TransactionRef
		{
			get
			{
				if (transactionRef == null)
				{
					Statement?.InstantiateTransactionReferencesForNettingTransactions();
				}

				return transactionRef ?? (transactionRef = new NettingTransactionReference());
			}
			set
			{
				transactionRef = value;
			}
		}
		NettingTransactionReference transactionRef;

		public NettingTransactionLineReference TransactionLineRef
		{
			get
			{
				if (transactionLineRef == null)
				{
					Statement?.InstantiateTransactionLineReferencesForNettingTransacitons();
				}

				return transactionLineRef ?? (transactionLineRef = new NettingTransactionLineReference());
			}
			set
			{
				transactionLineRef = value;
			}
		}
		NettingTransactionLineReference transactionLineRef;
	}
}
