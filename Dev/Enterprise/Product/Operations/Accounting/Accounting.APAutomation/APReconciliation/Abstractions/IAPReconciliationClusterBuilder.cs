using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationClusterBuilder
	{
		/// <summary>
		// We will have a collection of ZERO or MORE non-header clusters(`NonHeaderAPReconciliationCluster`) and exactly ONE header cluster(`HeaderAPReconciliationCluster`).

		// A non-header cluster has a cluster subtotal >= 0, (in M1 since we remove the subtotal field from UI, so this amount is always 0)
		// A header cluster will have calculated sub total.Calculation formula is -> Head cluster subtotal = Invoice Total - (total of all non header cluster subtotal)

		// A `NonHeaderAPReconciliationCluster` can have one or more `APReconciliationNode`.
		// A `HeaderAPReconciliationCluster` can have one or more `APReconciliationNode`.

		// Furthermore, we need to sort the clusters in a way so that the Header cluster reamins in the last position.
		// This ordering will make sure that the Header cluster is reconcilied LAST.
		/// </summary>
		IEnumerable<IAPReconciliationCluster> Build(AccDraftInvoiceHeader draftInvoice);
	}
}
