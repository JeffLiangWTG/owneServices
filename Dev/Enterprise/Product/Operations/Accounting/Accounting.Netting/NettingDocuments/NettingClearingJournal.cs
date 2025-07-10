using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingClearingJournal : NonPersistentBusinessObject, ISupportTransactionReference
	{
		public NettingClearingJournal()
		{
		}

		public NettingClearingJournal(NettingStatement statement)
		{
			Statement = statement;
		}

		readonly NettingStatement Statement;

		public ZString NettingPeriod => Statement?.Period?.NSP_Period ?? ZString.Empty;

		public ZString Ledger { get; set; }
		public ZString OrgCode { get; set; }
		public ZString CompanyCode { get; set; }
		public ZString ParticipatingOrgCode { get; set; }
		public ZString ParticipatingCompanyCode { get; set; }
		public ZString TransactionCurrency { get; set; }
		public ZDecimal TransactionAmount { get; set; }
		public ZString TransactionReference { get; set; }
		public ZString TransactionType { get; set; }
		public ZGuid OrgPK { get; set; }
		public ZGuid ParticipantOrgPK { get; set; }
		public ZString Description { get; set; }
		public ZBool OfferOrRequest { get; set; }

		public ZGuid TransactionPK
		{
			get; set;
		}

		public NettingTransactionReference TransactionRef
		{
			get
			{
				if (transactionRef == null)
				{
					Statement?.InstantiateTransactionReferencesForNettingClearingJournals();
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
					Statement?.InstantiateTransactionLineReferencesForNettingClearingJournals();
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
