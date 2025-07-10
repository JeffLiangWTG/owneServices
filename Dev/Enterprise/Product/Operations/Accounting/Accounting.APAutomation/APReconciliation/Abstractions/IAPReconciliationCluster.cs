using System.Collections.Generic;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationCluster
	{
		string Key { get; }
		decimal TotalAmount { get; }
		string Currency { get; }
		bool IsHeaderCluster { get; }
		IEnumerable<APReconciliationNode> Nodes { get; }
		APReconciliationProcessingResult TryToReconcile(IAccrualSummator summator);
	}

	public interface ISupportMergingAPReconciliationCluster : IAPReconciliationCluster
	{
		void MergeWithAnotherCluster(IAPReconciliationCluster cluster);
	}
}
