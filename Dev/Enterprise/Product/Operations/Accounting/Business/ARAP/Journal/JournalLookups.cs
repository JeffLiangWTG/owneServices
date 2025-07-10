using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class JournalLookups : TransactionHeaderLookups
	{
		public JournalLookups(TransactionHeader parent)
			: base(parent)
		{
		}

		public override GlbBranchCollection Branches
		{
			get { return new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_IsActive, true)); }
		}

		public CodeDescriptionPairList TransactionCategories
		{
			get
			{
				CodeDescriptionPairList fTransactionCategories = new CodeDescriptionPairList();
				fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.Standard, Constants.TransactionCategory.Descriptions.Standard);
				fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.TransactionNotFound, Constants.TransactionCategory.Descriptions.TransactionNotFound);
				fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.TransactionAlreadyPaid, Constants.TransactionCategory.Descriptions.TransactionAlreadyPaid);

				if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable && Parent is Journal arJournal && arJournal.IsCashAdvanceJournal)
				{
					fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.CashAdvanceReceived, Constants.TransactionCategory.Descriptions.CashAdvanceReceived);
				}
				if (Parent.AH_Ledger == LedgerTypes.AccountsPayable && Parent is Journal apJournal && apJournal.IsCashAdvanceJournal)
				{
					fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.CashAdvancePaid, Constants.TransactionCategory.Descriptions.CashAdvancePaid);
				}
				if (Parent is Journal journal && journal.IsCashAdvanceJournal)
				{
					fTransactionCategories.AddPair(Constants.TransactionCategory.Codes.CashAdvanceInvoice, Constants.TransactionCategory.Descriptions.CashAdvanceInvoice);
				}
				return fTransactionCategories;
			}
		}
	}
}
