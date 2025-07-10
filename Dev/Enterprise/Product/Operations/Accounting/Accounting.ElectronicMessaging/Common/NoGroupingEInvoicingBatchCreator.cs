using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	/// <summary>
	/// Creates EInvoicing Batches with a 1:1 mapping; no grouping or aggregation is applied.
	/// </summary>
	/// <remarks>
	/// If you wish to use a different grouping, follow the conventions in Spain and Taiwan.
	/// </remarks>
	public sealed class NoGroupingEInvoicingBatchCreator : EInvoicingBatchCreatorBase
	{
		public NoGroupingEInvoicingBatchCreator(GlbCompany company) : base(company)
		{
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(t => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { t })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}