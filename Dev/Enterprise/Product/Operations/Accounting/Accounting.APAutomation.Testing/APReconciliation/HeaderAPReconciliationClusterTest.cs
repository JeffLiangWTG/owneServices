using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class HeaderAPReconciliationClusterTest : APReconciliationClusterTest
	{
		public void TestMergeWithAnotherNonheaderCluster()
		{
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			var node1 = new APReconciliationNode("Node1", mockLineProvider.Object);
			var cluster = new HeaderAPReconciliationCluster(200M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1 });

			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);
			var nhCluster = new NonHeaderAPReconciliationCluster(100M, clusterCurrency: "AUD", localCurrency: "AUD", "NH_Cluster", new[] { node2 });

			(cluster as ISupportMergingAPReconciliationCluster).MergeWithAnotherCluster(nhCluster);

			var icluster = cluster as IAPReconciliationCluster;
			AssertEquals(true, icluster.IsHeaderCluster);
			AssertEquals(300M, icluster.TotalAmount);
			AssertEquals("AUD", icluster.Currency);
			AssertEquals("HEADER_CLUSTER", icluster.Key);
			AssertEquals(2, icluster.Nodes.Count());
		}

		public void TestMergeWithAnotherHeaderCluster()
		{
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			var node1 = new APReconciliationNode("Node1", mockLineProvider.Object);
			var cluster = new HeaderAPReconciliationCluster(200M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1 });

			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);
			var hCluster = new HeaderAPReconciliationCluster(100M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node2 });

			AssertExceptionThrown<InvalidOperationException>("A header cluster cannot be merged with another header cluster.", () => (cluster as ISupportMergingAPReconciliationCluster).MergeWithAnotherCluster(hCluster));
		}
		protected override string ExpectedFailureReasonWhenAmountIsZero => "";
		protected override APReconciliationResultTypes ExpectedReconciliationResultWhenAmountIsZero => APReconciliationResultTypes.Success;
		protected override string ExpectedFailureReasonWhenNoMatchFoundErrorCode => AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound;
		protected override string ExpectedFailureReasonWhenNoMatchFound => "[HEADER_CLUSTER]: No matching accruals found";
		protected override APReconciliationResultTypes ExpectedReconciliationResultWhenNoMatchFound => APReconciliationResultTypes.Failed;
		protected override APReconciliationCluster GetCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypesProvider = null) => new HeaderAPReconciliationCluster(clusterAmount, clusterCurrency, localCurrency, nodes, accrualFilterTypesProvider);
	}
}
