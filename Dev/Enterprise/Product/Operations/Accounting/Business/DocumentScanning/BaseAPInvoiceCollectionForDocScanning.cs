namespace Enterprise.Accounting.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.Base.Transaction;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	public abstract class BaseAPInvoiceCollectionForDocScanning : TransactionHeaderCollection
	{
		public BaseAPInvoiceCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		public BaseAPInvoiceCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypeForFilter);
			return result;
		}

		protected abstract ZString TransactionTypeForFilter { get; }
	}
}

