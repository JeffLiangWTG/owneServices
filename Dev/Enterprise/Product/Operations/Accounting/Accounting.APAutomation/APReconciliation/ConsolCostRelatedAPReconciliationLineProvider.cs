using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation.Helpers;
using Enterprise.Accounting.Business.APReconciliation;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class ConsolCostRelatedAPReconciliationLineProvider : IAPReconciliationLineProvider
	{
		public ConsolCostRelatedAPReconciliationLineProvider(ZGuid consolPk, AccDraftInvoiceHeader draftInvoice)
		{
			this.consolPk = consolPk;
			this.draftInvoice = draftInvoice;
		}
		readonly ZGuid consolPk;
		readonly AccDraftInvoiceHeader draftInvoice;

		IEnumerable<APReconciliationLine> IAPReconciliationLineProvider.GetLines(APReconciliationAccrualFilterTypes accrualFilterType)
		{
			if (!TryGetConsolCostFilter(accrualFilterType, out var consolCostFilter))
			{
				return Enumerable.Empty<APReconciliationLine>();
			}

			return GetRelatedJobChargeLines(accrualFilterType)
				.Concat(GetConsolJobChargeLines(accrualFilterType))
				.Concat(GetConsolCostLines(consolCostFilter));
		}

		IEnumerable<APReconciliationLine> GetRelatedJobChargeLines(APReconciliationAccrualFilterTypes accrualFilterType)
		{
			var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
			var consol = cache.GetConsol(consolPk);
			return consol.CostSupporter.ShipmentsListPKs.SelectMany(j => GetJobChargeLines(j, accrualFilterType)).ToList();
		}

		IEnumerable<APReconciliationLine> GetConsolJobChargeLines(APReconciliationAccrualFilterTypes accrualFilterType) => GetJobChargeLines(consolPk, accrualFilterType);

		IEnumerable<APReconciliationLine> GetJobChargeLines(ZGuid jobPk, APReconciliationAccrualFilterTypes accrualFilterType)
		{
			var jobChargeLineProvider = new JobChargeRelatedAPReconciliationLineProvider(jobPk, draftInvoice) as IAPReconciliationLineProvider;
			return jobChargeLineProvider.GetLines(accrualFilterType);
		}

		IEnumerable<APReconciliationLine> GetConsolCostLines(Func<JobConsolCost, bool> consolCostFilter)
		{
			var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
			var consolCosts = cache.GetConsolCosts(consolPk);
			return consolCosts.Where(c => MandatoryFilters.Invoke(c) && consolCostFilter.Invoke(c))
				.Select(ConvertToReconciliationLine);
		}

		APReconciliationLine ConvertToReconciliationLine(JobConsolCost cost)
		{
			var rLine = new APReconciliationLine();
			rLine.CreditorPk = cost.E6_OH_Creditor;
			rLine.LineIdentifier = cost.PK;
			rLine.Description = FormattableString.Invariant($"{cost.Consol?.JK_UniqueConsignRef}-{cost.ChargeCode.AC_Code}-{cost.E6_Description}");
			rLine.LineType = APReconciliationLineTypes.ConsolCost;
			rLine.OSExTaxAmount = cost.E6_OSCostAmount;
			rLine.OSCurrency = cost.E6_RX_NKCurrency;
			rLine.LocalExTaxAmount = cost.E6_LocalCostAmount;
			rLine.LocalCurrency = cost.Company.LocalCurrency.RX_Code;
			rLine.ExchangeRate = cost.E6_ExchangeRate;
			rLine.ParentId = cost.E6_ParentID;

			new APReconciliationCreditorInvoicedExchangeRateProvider(draftInvoice).TrySetExchangeRateAndAmounts(rLine, cost.Company.GetExchangeRate());

			return rLine;
		}

		bool TryGetConsolCostFilter(APReconciliationAccrualFilterTypes accrualFilterType, out Func<JobConsolCost, bool> consolCostFilter)
		{
			consolCostFilter = null;
			switch (accrualFilterType)
			{
				case APReconciliationAccrualFilterTypes.MatchingCreditor:
					consolCostFilter = c => c.E6_OH_Creditor == draftInvoice.AIH_OH_Creditor;
					return true;

				case APReconciliationAccrualFilterTypes.EmptyCreditor:
					consolCostFilter = c => c.E6_OH_Creditor.IsEmpty;
					return true;

				case APReconciliationAccrualFilterTypes.SettlementGroupCreditors:
				{
					var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
					var settlementGroupCreditorPks = cache.GetSettlementGroupCreditorPKs();
					if (settlementGroupCreditorPks.Count == 0)
					{
						return false;
					}

					consolCostFilter = c => settlementGroupCreditorPks.Contains(c.E6_OH_Creditor);
						return true;
				}

				case APReconciliationAccrualFilterTypes.AllOtherCreditors:
					consolCostFilter = c => !c.E6_OH_Creditor.IsEmpty && c.E6_OH_Creditor != draftInvoice.AIH_OH_Creditor;
					return true;
			}

			return false;
		}

		Func<JobConsolCost, bool> MandatoryFilters =>
				 c => (draftInvoice.IsInLocalCurrency || c.E6_RX_NKCurrency == draftInvoice.AIH_RX_NKTransactionCurrency)
							&& GetAccrualAmountFilter().Invoke(c)
							&& !c.IsPosted;

		Func<JobConsolCost, bool> GetAccrualAmountFilter()
		{
			var shouldConsiderNegativeAccrual = AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.GetFallBackValueAtAllLevels(draftInvoice.Company.PK.ToGuid(), draftInvoice.Branch.PK.ToGuid(), draftInvoice.Department.PK.ToGuid());

			return shouldConsiderNegativeAccrual
				? ((c) => c.E6_OSCostAmount != 0)
				: (draftInvoice.AIH_TransactionType == TransactionTypes.CreditNote
					? (c) => false
					: (c) => c.E6_OSCostAmount > 0);
		}
	}
}
