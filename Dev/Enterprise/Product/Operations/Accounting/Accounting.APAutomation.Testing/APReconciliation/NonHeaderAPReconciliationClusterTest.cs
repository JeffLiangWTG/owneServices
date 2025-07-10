using System.Collections.Generic;
using Enterprise.Accounting.APAutomation.APReconciliation;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class NonHeaderAPReconciliationClusterTest : APReconciliationClusterTest
	{
		protected override string ExpectedFailureReasonWhenAmountIsZero => string.Empty;
		protected override APReconciliationResultTypes ExpectedReconciliationResultWhenAmountIsZero => APReconciliationResultTypes.MergeWithHeaderCluster;
		protected override string ExpectedFailureReasonWhenNoMatchFoundErrorCode => string.Empty;
		protected override string ExpectedFailureReasonWhenNoMatchFound => string.Empty;
		protected override APReconciliationResultTypes ExpectedReconciliationResultWhenNoMatchFound => APReconciliationResultTypes.MergeWithHeaderCluster;
		protected override APReconciliationCluster GetCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypesProvider = null) => new NonHeaderAPReconciliationCluster(clusterAmount, clusterCurrency, localCurrency, "NON_HEADER_CLUSTER", nodes, accrualFilterTypesProvider);
	}
}
