using System;
using System.Collections.Generic;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class HeaderAPReconciliationCluster : APReconciliationCluster, ISupportMergingAPReconciliationCluster
	{
		public HeaderAPReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes)
			: base(clusterAmount, clusterCurrency, localCurrency, nodes)
		{
		}

		public HeaderAPReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypesProvider)
			: base(clusterAmount, clusterCurrency, localCurrency, nodes, accrualFilterTypesProvider)
		{
		}

		void ISupportMergingAPReconciliationCluster.MergeWithAnotherCluster(IAPReconciliationCluster cluster)
		{
			if (!cluster.IsHeaderCluster)
			{
				Nodes.AddRange(cluster.Nodes);
				ClusterAmount += cluster.TotalAmount;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("b18e886b-a20e-4acc-bfc1-2caed204afda", "A header cluster cannot be merged with another header cluster."));
			}
		}

		protected override string GetClusterKey() => "HEADER_CLUSTER";

		protected override bool IsHeaderCluster => true;
	}
}
