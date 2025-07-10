using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	[ModuleID(ModuleId.PaymentBatch)]
	public class AccPaymentBatchCollection : BusinessObjectCollection<AccPaymentBatch>
	{
		public AccPaymentBatchCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		public AccPaymentBatchCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{ }

		protected override ZQuery CreateRelationshipFilter() =>
			new ZQuery(AccPaymentBatchSchema.APB_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(AccPaymentBatchSchema.APB_RX_NKBatchCurrency, SQLComparisonOperator.Equal, string.Empty);

		protected override bool AllowNewCore => false;
	}
}
