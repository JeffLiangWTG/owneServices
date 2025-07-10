using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public abstract class APReconciliationCluster : IAPReconciliationCluster
	{
		protected APReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes)
			: this(clusterAmount, clusterCurrency, localCurrency, nodes, null)
		{
		}

		protected APReconciliationCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypeProvider)
		{
			Nodes = new List<APReconciliationNode>();
			Nodes.AddRange(nodes);
			ClusterAmount = clusterAmount;
			ClusterCurrency = clusterCurrency;
			AccrualFilterTypeProvider = accrualFilterTypeProvider ?? new APReconciliationAccrualFilterTypesProvider();
			this.localCurrency = localCurrency;
		}
		protected decimal ClusterAmount;
		protected string ClusterCurrency;
		readonly protected List<APReconciliationNode> Nodes;
		readonly protected IAPReconciliationAccrualFilterTypesProvider AccrualFilterTypeProvider;
		readonly string localCurrency;

		#region IAPReconciliationCluster implementation

		string IAPReconciliationCluster.Key => GetClusterKey();

		decimal IAPReconciliationCluster.TotalAmount => ClusterAmount;

		string IAPReconciliationCluster.Currency => ClusterCurrency;

		bool IAPReconciliationCluster.IsHeaderCluster => IsHeaderCluster;

		IEnumerable<APReconciliationNode> IAPReconciliationCluster.Nodes => Nodes;

		APReconciliationProcessingResult IAPReconciliationCluster.TryToReconcile(IAccrualSummator summator) => TryToReconcile(summator);

		#endregion

		public void AddNodes(params APReconciliationNode[] newNodes)
		{
			Nodes.AddRange(newNodes);
		}

		#region Reconciliation

		protected APReconciliationProcessingResult TryToReconcile(IAccrualSummator summator)
		{
			if (ClusterAmount == 0 && !IsHeaderCluster)
			{
				return ExitReconciliation();
			}

			var tracer = ObjectFactory.Get<ITracer>();
			tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => (NoResString)"Starting reconciliation for cluster...");

			//Gathering all reconcilable accruals
			var allCandidateReconciliationLines = new List<APReconciliationLine>();
			APReconciliationProcessingResult lastReconResult = default;

			var shouldMatchOnLocalAmount = ClusterCurrency == localCurrency;

			//Accruals will be loaded using a specific filter
			foreach (var accrualFilterType in AccrualFilterTypeProvider.GetFilterTypes()) //Ordering based on priority
			{
				//Loading accruals using the filter for all nodes
				var candidateReconciliationLinesThatMatchCurrentFilter = new List<APReconciliationLine>();
				foreach (var node in Nodes)
				{
					tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"Collecting accruals for job: {node.Name} with filter type {accrualFilterType}."));
					var lines = node.LineProvider.GetLines(accrualFilterType).ToList();
					if (lines.Count == 0)
					{
						tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"No accruals found for filter type {accrualFilterType}."));
					}
					else
					{
						tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"Found {lines.Count} accruals for filter type {accrualFilterType}: {System.Environment.NewLine}{string.Join(System.Environment.NewLine, lines.Select(l => $"{l.Description} -> {l.OSCurrency}{l.OSExTaxAmount:C}"))}"));
						if (!shouldMatchOnLocalAmount && candidateReconciliationLinesThatMatchCurrentFilter.Any(l => l.OSCurrency != ClusterCurrency))
						{
							return new APReconciliationProcessingResult()
							{
								Result = APReconciliationResultTypes.Failed,
								FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency,
								FailureReason = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency, GetClusterKey()),
								FailureReasonAsLogableError = ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency)
							};
						}
						candidateReconciliationLinesThatMatchCurrentFilter.AddRange(lines);
					}
				}

				var amountFieldSelector = (Func<APReconciliationLine, decimal>)(shouldMatchOnLocalAmount
					? line => line.LocalExTaxAmount
					: line => line.OSExTaxAmount);

				//Find the set of accruals that can be reconciled
				if (candidateReconciliationLinesThatMatchCurrentFilter.Count > 0)
				{
					allCandidateReconciliationLines.AddRange(candidateReconciliationLinesThatMatchCurrentFilter);

					allCandidateReconciliationLines = allCandidateReconciliationLines
						.DistinctBy(x => x.LineIdentifier)
						.ToList();

					//Always check for a single matching accrual first
					var matchingAccrual = FindSingleMatchingAccrual(allCandidateReconciliationLines, amountFieldSelector);
					if (matchingAccrual != null)
					{
						return new APReconciliationProcessingResult()
						{
							Result = APReconciliationResultTypes.Success,
							ReconciliableAccruals = new[] { matchingAccrual }
						};
					}

					//If there is NO single matching Accrual then try to combine accruals that add up to the cluster amount.
					//If NO combination is returned  then expand accrual search space.
					lastReconResult = TryToReconcileByCombiningMultipleAccruals(allCandidateReconciliationLines, summator, amountFieldSelector);
					if (lastReconResult != null && lastReconResult.Result == APReconciliationResultTypes.Success)
					{
						return lastReconResult;
					}
					else
					{
						continue;
					}
				}
			}

			tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => (NoResString)"End of reconciliation for cluster...");

			//If there is no candidate lines,
			if (!allCandidateReconciliationLines.Any())
			{
				if (IsHeaderCluster && ClusterAmount == 0M)
				{
					//instead of a failed error message, an empty collection of APReconciliationLine will be returned.
					return new APReconciliationProcessingResult()
					{
						Result = APReconciliationResultTypes.Success,
						ReconciliableAccruals = Array.Empty<APReconciliationLine>()
					};
				}
				else
				{
					//return a failed error message
					return new APReconciliationProcessingResult()
					{
						Result = APReconciliationResultTypes.Failed,
						FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.NoAccrual,
						FailureReason = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.NoAccrual, GetClusterKey()),
						FailureReasonAsLogableError = ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.NoAccrual)
					};
				} 
			}

			//this will return why the last attempt of reconciliation failed.
			return lastReconResult ?? ExitReconciliation();

			APReconciliationProcessingResult ExitReconciliation()
			{
				if (IsHeaderCluster)
				{
					return new APReconciliationProcessingResult()
					{
						Result = APReconciliationResultTypes.Failed,
						FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound,
						FailureReason = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound, GetClusterKey()),
						FailureReasonAsLogableError = ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound)
					}; 
				}
				else
				{
					return new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.MergeWithHeaderCluster };
				}
			}
		}

		APReconciliationLine FindSingleMatchingAccrual(IEnumerable<APReconciliationLine> allCandidateAccruals, Func<APReconciliationLine, decimal> amountFieldSelector)
		{
			var matches = allCandidateAccruals.Where(line => amountFieldSelector(line) == ClusterAmount).ToList();
			return matches.Count == 1 ? matches.Single() : null;
		}

		APReconciliationProcessingResult TryToReconcileByCombiningMultipleAccruals(List<APReconciliationLine> allCandidateAccruals, IAccrualSummator summator, Func<APReconciliationLine, decimal> amountFieldSelector)
		{
			try
			{
				var matchingSets = summator.Sumup(allCandidateAccruals, ClusterAmount, amountFieldSelector);
				if (matchingSets.Count() == 1)
				{
					return new APReconciliationProcessingResult()
					{
						Result = APReconciliationResultTypes.Success,
						ReconciliableAccruals = matchingSets.First().Accruals
					};
				}
				else
				{
					return null;
				}
			}
			catch (APAReconciliationTooManyMatchesFoundException)
			{
				return new APReconciliationProcessingResult()
				{
					Result = APReconciliationResultTypes.Failed,
					FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound,
					FailureReason = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound, GetClusterKey()),
					FailureReasonAsLogableError = ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound)
				};
			}
			catch (APAReconciliationTimeoutException)
			{
				return new APReconciliationProcessingResult()
				{
					Result = APReconciliationResultTypes.Failed,
					FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout,
					FailureReason = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout, GetClusterKey()),
					FailureReasonAsLogableError = ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout)
				};
			}
		}

		protected ILogableError ConvertToLogableError(string errorKey) =>
			AccDraftInvoiceProcessingErrors.ConvertToLogableError(errorKey, AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, GetClusterKey());

		#endregion

		protected abstract string GetClusterKey();
		protected virtual bool IsHeaderCluster => false;
	}
}
