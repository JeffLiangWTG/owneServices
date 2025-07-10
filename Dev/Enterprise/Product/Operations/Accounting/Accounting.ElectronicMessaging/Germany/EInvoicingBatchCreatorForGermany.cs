using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class EInvoicingBatchCreatorForGermany : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreatorForGermany(GlbCompany company) : base(company)
		{
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(t => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { t })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}
