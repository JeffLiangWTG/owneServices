using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ChequeTransaction
{
	public partial class ChequeTransactionHeaderCollection : BusinessObjectCollection<ChequeTransactionHeader>
	{
		public ChequeTransactionHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter() =>
			new ZQuery(AccPaymentBatchSchema.APB_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(AccPaymentBatchSchema.APB_RX_NKBatchCurrency, SQLComparisonOperator.NotEqual, string.Empty);

		protected override bool AllowNewCore => false;
	}
}
