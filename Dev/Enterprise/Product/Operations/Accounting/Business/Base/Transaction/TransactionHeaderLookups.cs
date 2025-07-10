using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderLookups : AccTransactionHeaderLookups
	{
		public TransactionHeaderLookups(TransactionHeader parent)
			: base(parent)
		{
		}

		public sealed override OrgHeaderCollection Headers
		{
			get { return HeadersCore; }
		}

		protected virtual OrgHeaderCollection HeadersCore
		{
			get
			{
				OrgHeaderCollection result;
				switch (Parent.AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						result = DebtorsList;
						break;
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.UnapprovedPayableTransactions:
					case LedgerTypes.TransactionsPendingAllocation:
					case LedgerTypes.IncompleteTransactions:
						result = CreditorsList;
						break;
					default:
						result = base.Headers;
						break;
				}
				return result;
			}
		}

		public OrgHeaderCollection DebtorsList
		{
			get { return FindboxLookupCollections.GetDebtorCollection(Factory); }
		}

		public CreditorCollection CreditorsList
		{
			get { return FindboxLookupCollections.GetCreditorCollection(Factory); }
		}

		public OrganisationsFindBoxCollection OrgHeaderCollection
		{
			get { return FindboxLookupCollections.GetOrganisationsFindBoxCollection(Factory); }
		}
	}
}
