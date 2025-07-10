using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
#if NETFRAMEWORK
using Enterprise.Freight.Business;
#endif
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class InvoiceLinesToConsolCostConvertor
	{
		public static void CreateSeparateConsolCostBasedOnEachLine(
			InvoicingBase apInvoicingBase,
			(IJobCostingPlugIn consol, InvoicingLineBase invoiceLine)[] linesForConsolCostByConsols,
			LinesInfoProvider linesInfoProvider,
			HashSet<Job> jobHeadersToDispose)
		{
			Argument.NotNull(apInvoicingBase, nameof(apInvoicingBase));
			Argument.NotNull(linesForConsolCostByConsols, nameof(linesForConsolCostByConsols));
			Argument.NotNull(linesInfoProvider, nameof(linesInfoProvider));
			Argument.NotNull(jobHeadersToDispose, nameof(jobHeadersToDispose));

			var indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob = new Dictionary<Tuple<ZGuid, ZGuid>, int>();
			var linesToRemove = new List<InvoicingLineBase>();

			foreach (var linesForConsolCostByConsol in linesForConsolCostByConsols)
			{
				var consol = linesForConsolCostByConsol.consol;
				var lineForConsolCost = linesForConsolCostByConsol.invoiceLine;

				if (lineForConsolCost.AL_OSExTaxAmount != 0 && lineForConsolCost.AL_LocalExTaxAmount != 0)
				{
					var isTaxOverridden = linesInfoProvider.IsTaxOverridden(lineForConsolCost);
					var shouldRecalculateExRate = ShouldCalculateExchangeRateByLocalAndOverseaAmounts(apInvoicingBase, lineForConsolCost, linesInfoProvider);
					var consolCost = AddNewConsolCostToInvoiceAndSetValuesFromLine(apInvoicingBase, consol, lineForConsolCost, consol.GetApportionmentMethod(lineForConsolCost.ChargeCode), setTaxRateFromLine: isTaxOverridden, shouldRecalculateExRate: shouldRecalculateExRate);

					TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCost, lineForConsolCost);

					AddSplitChargeJobsIntoJobHeadersToDispose(jobHeadersToDispose, consolCost);

					CacheIndexOfImportedUniversalTransactionLineValues(indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob, lineForConsolCost, consolCost, ignoreLineJob: true);

					linesToRemove.Add(lineForConsolCost);
				}
			}

			using (apInvoicingBase.Lines.SuspendListChanged())
			{
				foreach (var line in linesToRemove)
				{
					apInvoicingBase.Lines.RemoveAndDelete(line);
				}

				apInvoicingBase.ImportAllApportionmentsFromCosting();

				RestoreIndexOfImportedUniversalTransactionLineValues(apInvoicingBase, indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob, ignoreLineJob: true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public static void ConvertToConsolCostRelatedLines(
			BusinessObjectFactory factory,
			InvoicingBase apInvoicingBase,
			bool isCrossLedgerImport,
			(IJobCostingPlugIn consol, IEnumerable<InvoicingLineBase> invoiceLines)[] linesForConsolCostByConsols,
			LinesInfoProvider linesInfoProvider,
			HashSet<Job> jobHeadersToDispose)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(apInvoicingBase, nameof(apInvoicingBase));
			Argument.NotNull(linesForConsolCostByConsols, nameof(linesForConsolCostByConsols));
			Argument.NotNull(linesInfoProvider, nameof(linesInfoProvider));
			Argument.NotNull(jobHeadersToDispose, nameof(jobHeadersToDispose));

			var indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob = new Dictionary<Tuple<ZGuid, ZGuid>, int>();
			var linesToRemove = new List<InvoicingLineBase>();
			var consolCostsRequiringTaxDefaulting = new HashSet<(IJobCostingPlugIn consol, JobConsolCost consolCost)>();
			var splitChargeRelatedLineOSTaxAmount = new Dictionary<ZGuid, ZDecimal>();
			var splitChargeRelatedLineLocalCostAmount = new Dictionary<ZGuid, ZDecimal>();
			foreach (var linesForConsolCostByConsol in linesForConsolCostByConsols)
			{
				var consol = linesForConsolCostByConsol.consol;
				var linesForConsolCost = linesForConsolCostByConsol.invoiceLines;

				if (!linesForConsolCost.Any())
				{
					continue;
				}

				var costsThatSumToZero = GetCostsThatSumToZero(apInvoicingBase, linesForConsolCost);
				var isImportFromGateway = apInvoicingBase.HasContext(BusinessContext.InterCompanyInvoiceImportedFromGatewayConsol);
				foreach (var line in linesForConsolCost)
				{
					if ((isImportFromGateway || (line.AL_JH.IsValid && IsLineJobMatchesAnyConsolShipmentListJob(factory, consol, line)))
							&& !costsThatSumToZero.Contains(line.PK))
					{
						var consolCost = GetOrCreateConsolCost(apInvoicingBase, consol, line, isImportFromGateway);
						var shouldRecalculateExRate = ShouldCalculateExchangeRateByLocalAndOverseaAmounts(apInvoicingBase, line, linesInfoProvider);

						if (!linesInfoProvider.IsTaxOverridden(line)) //since tax is not overridden, the tax needs to be defaulted
						{
							if (!consolCostsRequiringTaxDefaulting.Contains(ValueTuple.Create(consol, consolCost)))
							{
								consolCostsRequiringTaxDefaulting.Add(ValueTuple.Create(consol, consolCost));
							}
						}
						AddSplitChargeJobsIntoJobHeadersToDispose(jobHeadersToDispose, consolCost);

						using (consolCost.GetValidationSuspender())
						{
							if (isImportFromGateway && line.RelatedJobFromIntercompanyInvoiceImport.JobPk.IsEmpty)
							{
								SetConsolCostAmountsAndIsFinalFlag(consolCost, line, shouldRecalculateExRate);
								linesToRemove.Add(line);
							}
							else
							{
								foreach (ApportionSplitCharge splitCharge in consolCost.ApportionmentCharges)
								{
									if (splitCharge.JR_JH == line.AL_JH || splitCharge.Job.JH_ParentID == line.RelatedJobFromIntercompanyInvoiceImport.JobPk)
									{
										splitCharge.JR_OSCostAmt += line.AL_OSExTaxAmount;

										if (shouldRecalculateExRate)
										{
											splitChargeRelatedLineLocalCostAmount[splitCharge.PK] = splitChargeRelatedLineLocalCostAmount.GetValueOrDefault(splitCharge.PK) + line.AL_LocalExTaxAmount;
										}

										if (line.Factory.HasAnyOfContexts(BusinessContext.InterCompanyInvoiceImport, BusinessContext.AllocatingTransaction))
										{
											if (splitChargeRelatedLineOSTaxAmount.ContainsKey(splitCharge.PK))
											{
												splitChargeRelatedLineOSTaxAmount[splitCharge.PK] += line.AL_OSTaxAmount;
											}
											else
											{
												splitChargeRelatedLineOSTaxAmount.Add(splitCharge.PK, line.AL_OSTaxAmount);
											}
										}

										if (!isCrossLedgerImport)
										{
											splitCharge.IsFinal = line.AL_IsFinalCharge;
										}

										CacheIndexOfImportedUniversalTransactionLineValues(indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob, line, consolCost);
										linesToRemove.Add(line);
										break;
									}
								}
							}
							if (isImportFromGateway)
							{
								consolCost.IsFinal = AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.Value;
							}
						}
					}
				}
			}

			foreach (var values in consolCostsRequiringTaxDefaulting)
			{
				var parameters = values.consol.GetTaxCalculationParameters();
				parameters.Organisation = values.consolCost.Creditor;
				if (values.consolCost.PlaceOfSupplyLocation != null)
				{
					parameters.FixedPlaceOfSupply = values.consolCost.PlaceOfSupplyLocation;
				}
				if (values.consolCost.ChargeCode != null)
				{
					var taxRate = values.consolCost.ChargeCode.GetGSTRate(parameters, out var taxMessage);
					if (taxRate != null)
					{
						values.consolCost.E6_AT_TaxRate = taxRate.PK;
						values.consolCost.E6_A9_VATClass = taxMessage;
					}
				}
			}

			using (apInvoicingBase.Lines.SuspendListChanged())
			{
				foreach (var line in linesToRemove)
				{
					apInvoicingBase.Lines.RemoveAndDelete(line);
				}

				CalculateE6_OSCostAmountAndValidateAmounts(apInvoicingBase, splitChargeRelatedLineOSTaxAmount, splitChargeRelatedLineLocalCostAmount);

				apInvoicingBase.ImportAllApportionmentsFromCosting();

				RestoreIndexOfImportedUniversalTransactionLineValues(apInvoicingBase, indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob);
			}
		}

		static bool IsLineJobMatchesAnyConsolShipmentListJob(BusinessObjectFactory factory, IJobCostingPlugIn consol, InvoicingLineBase line)
		{
			bool result = false;
			foreach (var shipment in consol.CostSupporter.ShipmentsList)
			{
				var shipmentJob = new Job.Loader(factory, shipment).Load();
				if (shipmentJob != null && shipmentJob.PK == line.AL_JH)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		static JobConsolCost GetOrCreateConsolCost(InvoicingBase apInvoicingBase, IJobCostingPlugIn consol, InvoicingLineBase line, bool isImportFromGateway = false)
		{
			var factory = apInvoicingBase.Factory;
			var relatedApportionCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(factory, consol.PK, line);

			JobConsolCost consolCost = null;
			foreach (JobConsolCost jobConsolCost in apInvoicingBase.ConsolCosting.ConsolCosts)
			{
				if (jobConsolCost.E6_AC_ChargeCode == line.AL_AC &&
					jobConsolCost.E6_AT_TaxRate == line.AL_AT &&
					jobConsolCost.E6_ParentID == consol.CostSupporter.PK &&
					(!apInvoicingBase.IsAPTransactionConvertedFromTransactionPendingAllocation || jobConsolCost.E6_RX_NKCurrency == line.AL_RX_NKTransactionCurrency) &&
					(!isImportFromGateway || IsRelatedJobSupportMergingCosts(jobConsolCost, line)) &&
					TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(jobConsolCost, relatedApportionCharge))
				{
					consolCost = jobConsolCost;
					if (ObjectFactory.Get<IElectronicProcessingChargeProvider>().ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(consolCost.E6_AC_ChargeCode, consolCost.E6_GC))
					{
						consolCost.ApportionmentCharges?.Cast<ApportionSplitCharge>().Where(x => x.Job.PK == line.AL_JH).ForEach(x => x.JR_Desc = line.AL_Desc);
					}
					break;
				}
			}

			if (consolCost == null)
			{
				var allocationMethod = isImportFromGateway && line.RelatedJobFromIntercompanyInvoiceImport.JobPk.IsEmpty ? string.Empty : AllocationMethod.Manual;
				consolCost = AddNewConsolCostToInvoiceAndSetValuesFromLine(apInvoicingBase, consol, line, allocationMethod, false);
				TransactionImportJobChargeMappingProvider.SetRelatedConsolCost(consolCost, relatedApportionCharge);
			}

			TransactionImportJobChargeMappingProvider.SetRelatedApportionCharge(factory, line.PK, consolCost, new[] { relatedApportionCharge });

			return consolCost;
		}

		static bool IsRelatedJobSupportMergingCosts(JobConsolCost consolCost, InvoicingLineBase line)
		{
			var isConsolCostShipmentRelated = !consolCost.RelatedJobPkFromIntercompanyInvoiceImport.IsEmpty;
			var isLineShipmentRelated = !line.RelatedJobFromIntercompanyInvoiceImport.JobPk.IsEmpty;
			return isConsolCostShipmentRelated == isLineShipmentRelated;
		}

		static JobConsolCost AddNewConsolCostToInvoiceAndSetValuesFromLine(InvoicingBase apInvoicingBase, IJobCostingPlugIn consol, InvoicingLineBase lineForConsolCost, ZString allocationMethod, bool setAmountsAndIsFinal = true, bool setTaxRateFromLine = true, bool shouldRecalculateExRate = false)
		{
			var consolCost = apInvoicingBase.ConsolCosting.ConsolCosts.AddNew();
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.CostSupporter.PK, consol.CostSupporter.Type);
			consolCost.RelatedJobPkFromIntercompanyInvoiceImport = lineForConsolCost.RelatedJobFromIntercompanyInvoiceImport.JobPk;

			using (consolCost.GetSuspenderForUnapprovedTransactonConverter())
			{
				consolCost.E6_AC_ChargeCode = lineForConsolCost.AL_AC;
				if (ObjectFactory.Get<IElectronicProcessingChargeProvider>().ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(consolCost.E6_AC_ChargeCode, consolCost.E6_GC))
				{
					consolCost.ApportionmentCharges?.Cast<ApportionSplitCharge>().Where(x => x.Job.PK == lineForConsolCost.AL_JH).ForEach(x => x.JR_Desc = lineForConsolCost.AL_Desc);
				}
				if (apInvoicingBase.IsAPTransactionConvertedFromTransactionPendingAllocation)
				{
					consolCost.E6_RX_NKCurrency = lineForConsolCost.AL_RX_NKTransactionCurrency;
				}
				consolCost.E6_SupplyType = lineForConsolCost.AL_SupplyType;
				if (setTaxRateFromLine)
				{
					consolCost.E6_AT_TaxRate = lineForConsolCost.AL_AT;
					if (lineForConsolCost.Factory.HasContext(BusinessContext.InterCompanyInvoiceImport) && !lineForConsolCost.AL_A9_VATClass.IsEmpty)
					{
						consolCost.E6_A9_VATClass = lineForConsolCost.AL_A9_VATClass;
					}
					consolCost.E6_TaxDate = lineForConsolCost.AL_TaxDate;
				}

				if (consolCost.E6_AT_TaxRate.IsValid)
				{
					if (lineForConsolCost.AL_TaxDate.IsValid)
					{
						consolCost.E6_TaxDate = lineForConsolCost.AL_TaxDate;
					}
					else
					{
						var costSupporter = consol.CostSupporter;
						if (costSupporter != null)
						{
							var taxDateOption = AccountingUtils.GetTaxDateDefaultingOptionForCostSupporter(costSupporter, LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsolCode);
							if (taxDateOption != null)
							{
								if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.InvoiceDate)
								{
									consolCost.E6_TaxDate = apInvoicingBase.AH_InvoiceDate.Date;
								}
								else if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.Today)
								{
									consolCost.E6_TaxDate = ZDate.Today;
								}
							}
						}
					}
				}
			}
			if (!allocationMethod.IsEmpty)
			{
				consolCost.E6_ApportionmentMethod = allocationMethod;
			}

			if (setAmountsAndIsFinal)
			{
				SetConsolCostAmountsAndIsFinalFlag(consolCost, lineForConsolCost, shouldRecalculateExRate);
			}

			return consolCost;
		}

		static void SetConsolCostAmountsAndIsFinalFlag(JobConsolCost consolCost, InvoicingLineBase lineForConsolCost, bool shouldRecalculateExRate)
		{
			var osExTaxAmount = consolCost.E6_OSCostAmount + lineForConsolCost.AL_OSExTaxAmount;
			var localExTaxAmount = consolCost.E6_LocalCostAmount + lineForConsolCost.AL_LocalExTaxAmount;
			var osTaxAmount = consolCost.E6_OSGSTAmount_Calc + lineForConsolCost.AL_OSTaxAmount;

			consolCost.E6_OSCostAmount = osExTaxAmount;
			if (shouldRecalculateExRate)
			{
				consolCost.E6_LocalCostAmount = localExTaxAmount;
			}
			consolCost.E6_OSGSTAmount_Calc = osTaxAmount;
			consolCost.IsFinal = lineForConsolCost.AL_IsFinalCharge;
		}

		static HashSet<ZGuid> GetCostsThatSumToZero(InvoicingBase apInvoicingBase, IEnumerable<InvoicingLineBase> linesForConsolCost)
		{
			var cost = apInvoicingBase.IsAPTransactionConvertedFromTransactionPendingAllocation ?
				linesForConsolCost.GroupBy(l => new { ChargeCode = l.AL_AC, TaxID = l.AL_AT, Currency = l.AL_RX_NKTransactionCurrency }) :
				linesForConsolCost.GroupBy(l => new { ChargeCode = l.AL_AC, TaxID = l.AL_AT, Currency = ZString.Empty });

			return cost.Where(costs => costs.Sum(os => os.AL_OSExTaxAmount) == 0 || (costs.All(x => x.AL_ExchangeRate != 0) && costs.Sum(lo => lo.AL_LocalExTaxAmount) == 0))
			.SelectMany(costs => costs.Select(z => z.PK)).ToHashSet();
		}

		static void CalculateE6_OSCostAmountAndValidateAmounts(InvoicingBase apInvoicingBase, Dictionary<ZGuid, ZDecimal> splitChargeRelatedLineOSTaxAmount, Dictionary<ZGuid, ZDecimal> splitChargeRelatedLineLocalCostAmount)
		{
			foreach (JobConsolCost cost in apInvoicingBase.ConsolCosting.ConsolCosts)
			{
				cost.E6_OSCostAmount = cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt);

				if (cost.ApportionmentCharges.All(x => splitChargeRelatedLineLocalCostAmount.ContainsKey(x.PK)))
				{
					cost.E6_LocalCostAmount = cost.ApportionmentCharges.Sum(x => splitChargeRelatedLineLocalCostAmount.TryGetValue(x.PK, out var value) ? value : 0m);
				}

				if (splitChargeRelatedLineOSTaxAmount.Count > 0)
				{
					ZDecimal osTaxAmountTotal = 0m;
					foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
					{
						if (splitChargeRelatedLineOSTaxAmount.TryGetValue(charge.PK, out var osTaxAmount))
						{
							osTaxAmountTotal += osTaxAmount;
						}
					}
					cost.E6_OSGSTAmount_Calc = osTaxAmountTotal;
				}

				cost.Validation.ValidateUnApportionedAmount();
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					charge.Validation.ValidateJR_OSCostAmt();
				}
			}
		}

		static void RestoreIndexOfImportedUniversalTransactionLineValues(InvoicingBase apInvoicingBase,
			Dictionary<Tuple<ZGuid, ZGuid>, int> indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob,
			bool ignoreLineJob = false)
		{
			foreach (InvoicingLineBase line in apInvoicingBase.Lines)
			{
				int indexOfImportedUniversalTransactionLine;
				var key = Tuple.Create(line.ImportedApportionmentID, ignoreLineJob ? ZGuid.Empty : line.AL_JH);
				if (indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob.TryGetValue(key, out indexOfImportedUniversalTransactionLine))
				{
					line.IndexOfImportedUniversalTransactionLine = indexOfImportedUniversalTransactionLine;
				}
			}
		}

		static void CacheIndexOfImportedUniversalTransactionLineValues(Dictionary<Tuple<ZGuid, ZGuid>, int> indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob,
			InvoicingLineBase line, JobConsolCost consolCost, bool ignoreLineJob = false)
		{
			var key = Tuple.Create(consolCost.PK, ignoreLineJob ? ZGuid.Empty : line.AL_JH);
			indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob[key] = line.IndexOfImportedUniversalTransactionLine;
		}

		static void AddSplitChargeJobsIntoJobHeadersToDispose(HashSet<Job> jobHeadersToDispose, JobConsolCost consolCost)
		{
			jobHeadersToDispose.UnionWith(consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Where(splitCharge => splitCharge.JR_JH.IsValid && splitCharge.InvoicingJob != null).Select(splitCharge => splitCharge.InvoicingJob));
		}

		static bool ShouldCalculateExchangeRateByLocalAndOverseaAmounts(InvoicingBase apLine, InvoicingLineBase lineForConsolCost, LinesInfoProvider linesInfoProvider)
		{
			return apLine.IsAPTransactionConvertedFromTransactionPendingAllocation && linesInfoProvider.IsLocalAmountApplicable(lineForConsolCost);
		}
	}
}
