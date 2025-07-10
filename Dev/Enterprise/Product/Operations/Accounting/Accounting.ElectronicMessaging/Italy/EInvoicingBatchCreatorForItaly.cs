using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class EInvoicingBatchCreatorForItaly : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreatorForItaly(GlbCompany company)
			: base(company)
		{ }

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(g => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { g })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}