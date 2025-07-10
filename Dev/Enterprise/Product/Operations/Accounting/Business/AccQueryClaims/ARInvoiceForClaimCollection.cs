using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.ARTransaction)]
	public class ARInvoiceForClaimCollection : ARTransactionHeaderCollection
	{
		public ARInvoiceForClaimCollection(IQueryClaim queryClaim, ZQuery filter) : base(queryClaim, filter)
		{
		}

		public new ARInvoice this[int index]
		{
			get
			{
				return (ARInvoice)Elements[index];
			}
		}

		public new ARInvoice AddNew()
		{
			return (ARInvoice)base.AddNew();
		}
	}
}

