using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.APTransaction)]
	public class APInvoiceForClaimCollection : APTransactionHeaderCollection
	{
		public APInvoiceForClaimCollection(IQueryClaim queryClaim, ZQuery filter) : base(queryClaim, filter)
		{
		}

		public new APInvoice this[int index]
		{
			get
			{
				return (APInvoice)Elements[index];
			}
		}

		public new APInvoice AddNew()
		{
			return (APInvoice)base.AddNew();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, Enterprise.ZArchitecture.Core.LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.Invoice);
			return query;
		}
	}
}

