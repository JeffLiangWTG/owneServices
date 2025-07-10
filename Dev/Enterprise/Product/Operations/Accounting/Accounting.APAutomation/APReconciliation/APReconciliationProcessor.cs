using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using Enterprise.Accounting.APAutomation.APReconciliation.Helpers;
using Enterprise.Accounting.Business.APReconciliation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationProcessor : IAPReconciliationProcessor
	{
		APReconciliationProcessingResult IAPReconciliationProcessor.Reconcile(AccDraftInvoiceHeader draftTransaction)
		{
			if(!Validate(draftTransaction, out var validationErrorMsg))
			{
				return new APReconciliationProcessingResult
				{
					FailureReason = validationErrorMsg,
					Result = APReconciliationResultTypes.Failed
				};
			}

			try
			{
				var clusters = ObjectFactory.Get<IAPReconciliationClusterBuilder>().Build(draftTransaction);

				var rLines = new List<APReconciliationLine>();
				var tracer = ObjectFactory.Get<ITracer>();

				if (draftTransaction.AIH_TransactionType == TransactionTypes.CreditNote && !((bool)AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.GetValueWithFallbackDefault(draftTransaction.AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)))
				{
					var msgParams = new string[] { draftTransaction.AIH_TransactionNumber, AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.HumanReadableRegistryPath() };
					var errorMessage = AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour, msgParams);
					tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => errorMessage);
					return new APReconciliationProcessingResult
					{
						Result = APReconciliationResultTypes.Failed,
						FailureReasonCode = AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour,
						FailureReason = errorMessage,
						FailureReasonAsLogableError = AccDraftInvoiceProcessingErrors.ConvertToLogableError(AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour, AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, msgParams)
					};
				}

				tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => (NoResString)"Starting reconciliation...");

				foreach (var cluster in clusters)
				{
					tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"Starting reconciliation for cluster: {cluster.Key} ({cluster.TotalAmount:C})"));
					var reconciliationDetails = cluster.TryToReconcile(new AccrualSummator());

					var reconciliationResult = reconciliationDetails.Result;

					//Let's merge nonheader cluster with header cluster if possible.
					if (reconciliationResult == APReconciliationResultTypes.MergeWithHeaderCluster)
					{
						if (!cluster.IsHeaderCluster && clusters.FirstOrDefault(c => c.IsHeaderCluster) is ISupportMergingAPReconciliationCluster hCluster)
						{
							hCluster.MergeWithAnotherCluster(cluster);
							continue;
						}
						reconciliationResult = APReconciliationResultTypes.Failed;
					}

					if (reconciliationResult == APReconciliationResultTypes.Failed)
					{
						tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"Reconciliation failed: {reconciliationDetails.FailureReason}"));
						return new APReconciliationProcessingResult()
						{
							FailureReason = reconciliationDetails.FailureReason,
							Result = APReconciliationResultTypes.Failed,
							FailureReasonAsLogableError = reconciliationDetails.FailureReasonAsLogableError
						};
					}
					else if (reconciliationResult == APReconciliationResultTypes.Success)
					{
						tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"Successfully reconciled cluster: Found accruals with amounts {string.Join(" + ", reconciliationDetails.ReconciliableAccruals.Select(acr => $"{acr.OSExTaxAmount:C}"))}."));
						rLines.AddRange(reconciliationDetails.ReconciliableAccruals);
					}
				}

				draftTransaction.HasReconciliationRun = true;
				return new APReconciliationProcessingResult()
				{
					Result = APReconciliationResultTypes.Success,
					ReconciliableAccruals = rLines
				};
			}
			finally
			{
				APReconciliationAccrualSourceCache.Clear<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftTransaction);
			}
		}

		bool Validate(AccDraftInvoiceHeader draftTransaction, out string errorMsg)
		{
			var errorMsgBuilder = new StringBuilder();

			if (!draftTransaction.AIH_OH_Creditor.IsValid)
			{
				errorMsgBuilder.AppendLine(Res.GetString("7AC847AB-D8EE-41CE-B33B-3426329C8121", "Creditor not provided"));
			}

			if (draftTransaction.AIH_ExpectedOSTotalAmount == 0)
			{
				errorMsgBuilder.AppendLine(Res.GetString("01E055F9-73D3-4925-A5C8-ADECE031EEE0", "Transaction Amount not provided"));
			}

			if (!draftTransaction.JobClusters.Any())
			{
				errorMsgBuilder.AppendLine(Res.GetString("ADD4CC12-A504-4907-95A7-0748BEB653A9", "No jobs found"));
			}

			errorMsg = errorMsgBuilder.ToString().TrimEnd();
			return string.IsNullOrEmpty(errorMsg);
		}
	}
}
