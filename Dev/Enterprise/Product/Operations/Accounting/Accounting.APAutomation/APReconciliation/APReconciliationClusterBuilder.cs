using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationClusterBuilder : IAPReconciliationClusterBuilder
	{
		public APReconciliationClusterBuilder()
		{
		}

		IEnumerable<IAPReconciliationCluster> IAPReconciliationClusterBuilder.Build(AccDraftInvoiceHeader draftInvoice)
		{
			var result = new List<IAPReconciliationCluster>();
			var sign = draftInvoice.AIH_TransactionType == TransactionTypes.CreditNote ? -1 : 1;
			var headerClusterAmount = draftInvoice.AIH_ExpectedOSExTaxAmount * sign;
			var headerClusterNodes = new List<APReconciliationNode>();
			foreach (AccDraftInvoiceJobCluster jobCluster in draftInvoice.JobClusters)
			{
				var signedJobClusterAmount = jobCluster.AIC_Amount * sign;
				var nodes = new List<APReconciliationNode>();
				foreach (AccDraftInvoiceJob job in jobCluster.OperationalJobs)
				{
					var node = APReconciliationNodeProvider.GetAPReconciliationNode(job, draftInvoice);
					if (node != null)
					{
						nodes.Add(node);
					}
				}
				if (signedJobClusterAmount == 0)
				{
					headerClusterNodes.AddRange(nodes);
				}
				else
				{
					var nonHeaderCluster = new NonHeaderAPReconciliationCluster(signedJobClusterAmount, draftInvoice.AIH_RX_NKTransactionCurrency, draftInvoice.Company.GC_RX_NKLocalCurrency, jobCluster.PK.ToString(), nodes);
					result.Add(nonHeaderCluster);
					headerClusterAmount -= signedJobClusterAmount;
				}
			}

			var headerCluster = new HeaderAPReconciliationCluster(headerClusterAmount, draftInvoice.AIH_RX_NKTransactionCurrency, draftInvoice.Company.GC_RX_NKLocalCurrency, headerClusterNodes);
			result.Add(headerCluster);

			return result;
		}
	}
}
