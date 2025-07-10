using System.Collections.Generic;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class NonHeaderAPReconciliationCluster : APReconciliationCluster
	{
		public NonHeaderAPReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, string clusterKey, IEnumerable<APReconciliationNode> nodes)
			: this(clusterAmount, clusterCurrency, localCurrency, clusterKey, nodes, null)
		{
		}

		public NonHeaderAPReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, string clusterKey, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypesProvider)
			: base(clusterAmount, clusterCurrency, localCurrency, nodes, accrualFilterTypesProvider)
		{
			this.clusterKey = clusterKey;
		}

		readonly string clusterKey;

		protected override string GetClusterKey() => clusterKey;
	}
}
