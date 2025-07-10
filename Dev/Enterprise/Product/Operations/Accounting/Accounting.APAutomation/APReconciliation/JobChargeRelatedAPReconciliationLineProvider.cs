using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation.Helpers;
using Enterprise.Accounting.Business.APReconciliation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class JobChargeRelatedAPReconciliationLineProvider : IAPReconciliationLineProvider
	{
		public JobChargeRelatedAPReconciliationLineProvider(ZGuid jobParentPk, AccDraftInvoiceHeader draftInvoice)
		{
			this.jobParentPk = jobParentPk;
			this.draftInvoice = draftInvoice;
		}
		readonly AccDraftInvoiceHeader draftInvoice;
		readonly ZGuid jobParentPk;

		IEnumerable<APReconciliationLine> IAPReconciliationLineProvider.GetLines(APReconciliationAccrualFilterTypes accrualFilterType)
		{
			if (!TryGetJobChargeFilter(accrualFilterType, out var jobChargeFilter))
			{
				return Array.Empty<APReconciliationLine>();
			}

			var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
			var charges = cache.GetCharges(jobParentPk).ToList();
			return charges.Where(c => GetMandatoryFilter().Invoke(c) && jobChargeFilter.Invoke(c))
				.Select(ConvertToReconciliationLine);
		}

		APReconciliationLine ConvertToReconciliationLine(JobCharge charge)
		{
			var rLine = new APReconciliationLine();
			rLine.CreditorPk = charge.JR_OH_CostAccount;
			rLine.LineIdentifier = charge.PK;
			rLine.Description = FormattableString.Invariant($"{charge.Job?.JH_JobNum}-{charge.ChargeCode.AC_Code}-{charge.JR_Desc}");
			rLine.LineType = APReconciliationLineTypes.JobCharge;
			rLine.OSExTaxAmount = charge.JR_OSCostAmt;
			rLine.OSCurrency = charge.JR_RX_NKCostCurrency;
			rLine.LocalExTaxAmount = charge.JR_LocalCostAmt;
			rLine.LocalCurrency = charge.Company.LocalCurrency.RX_Code;
			rLine.ExchangeRate = charge.JR_OSCostExRate;
			rLine.ParentId = charge.Job.JH_ParentID;

			new APReconciliationCreditorInvoicedExchangeRateProvider(draftInvoice).TrySetExchangeRateAndAmounts(rLine, charge.Company.GetExchangeRate());

			return rLine;
		}

		bool TryGetJobChargeFilter(APReconciliationAccrualFilterTypes accrualFilterType, out Func<JobCharge, bool> jobChargeFilter)
		{
			jobChargeFilter = null;
			switch (accrualFilterType)
			{
				case APReconciliationAccrualFilterTypes.MatchingCreditor:
					jobChargeFilter = c => c.JR_OH_CostAccount == draftInvoice.AIH_OH_Creditor;
					return true;

				case APReconciliationAccrualFilterTypes.EmptyCreditor:
					jobChargeFilter = c => c.JR_OH_CostAccount.IsEmpty;
					return true;

				case APReconciliationAccrualFilterTypes.SettlementGroupCreditors:
					{
						var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
						var settlementGroupCreditorPks = cache.GetSettlementGroupCreditorPKs();
						if (settlementGroupCreditorPks.Count == 0)
						{
							return false;
						}

						jobChargeFilter = c => settlementGroupCreditorPks.Contains(c.JR_OH_CostAccount);
						return true;
					}

				case APReconciliationAccrualFilterTypes.AllOtherCreditors:
					jobChargeFilter = c => !c.JR_OH_CostAccount.IsEmpty && c.JR_OH_CostAccount != draftInvoice.AIH_OH_Creditor;
					return true;
			}

			return false;
		}

		Func<JobCharge, bool> GetAccrualAmountFilter()
		{
			var shouldConsiderNegativeAccrual = AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.GetFallBackValueAtAllLevels(draftInvoice.Company.PK.ToGuid(), draftInvoice.Branch.PK.ToGuid(), draftInvoice.Department.PK.ToGuid());

			return shouldConsiderNegativeAccrual
				? ((c) => c.JR_OSCostAmt != 0)
				: (draftInvoice.AIH_TransactionType == TransactionTypes.CreditNote
					? (c) => false
					: (c) => c.JR_OSCostAmt > 0);
		}

		Func<JobCharge, bool> GetMandatoryFilter() =>
			c => GetAccrualAmountFilter().Invoke(c) &&
						c.JR_GC == draftInvoice.AIH_GC_Company &&
						(draftInvoice.IsInLocalCurrency || c.JR_RX_NKCostCurrency == draftInvoice.AIH_RX_NKTransactionCurrency) &&
						!c.IsCostPosted &&
						!c.IsApportioned;
	}
}
