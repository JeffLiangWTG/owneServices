using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class GatewaySellToCostSynchroniser
	{
		public static void Synchronise(Job job)
		{
			if (IsGatewaySynchronizable(job))
			{
				var apportionmentsListing = ((IJobCostingPlugIn)job.Parent).GetApportionments(true);
				var initialState = apportionmentsListing.State;

				try
				{
					var sellChargesToSynchronise = GetGatewaySellChargesToSynchronise(job);

					if (sellChargesToSynchronise.Any())
					{
						var consolCosts = apportionmentsListing.CostsCollection
							.Where(x => !x.ApportionmentCharges.ArePostedWithJobRevenueJournal)
							.ToList();

						apportionmentsListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();

						sellChargesToSynchronise.Where(x => x.JR_AT_SellGSTRate.IsValid && x.SellAccountIsOrgProxy).ForEach(y => y.JR_AT_SellGSTRate = ZGuid.Empty);

						var parentBizO = job.Parent as BusinessObject;

						using (parentBizO.SetTempContext(BusinessContext.EnableDirectSettingConsolCostParent))
						{
							var costCopiedFromGatewayCharges = FindOrCreateSyhchronizedConsolCosts(sellChargesToSynchronise, apportionmentsListing, consolCosts);
							DeleteConsolCosts(consolCosts.Where(cost => !costCopiedFromGatewayCharges.Contains(cost)));
						}
					}
					else
					{
						using (apportionmentsListing.DoNotCreateShipmentJob())
						{
							var consolCosts = apportionmentsListing.CostsCollection
								.Where(x => !x.ApportionmentCharges.ArePostedWithJobRevenueJournal)
								.ToList();
							DeleteConsolCosts(consolCosts);
						}
					}
					RemoveInvalidChargesFromApportionment(apportionmentsListing);
				}
				finally
				{
					if (IsProcessingOnFactorySavingBeforeTransaction(job) &&
						(initialState == ApportionmentListingStates.Cleaned || initialState == ApportionmentListingStates.NotLoaded) &&
						apportionmentsListing.State == ApportionmentListingStates.Loaded)
					{
						apportionmentsListing.ReleaseMutexesOnUnusedJobs();
					}
				}
			}
		}

		static bool IsProcessingOnFactorySavingBeforeTransaction(Job job)
		{
#if DEBUG
			if (simulateIsProcessingOnFactorySavingBeforeTransaction_ForTestOnly)
			{
				simulateIsProcessingOnFactorySavingBeforeTransaction_ForTestOnly = false;
				return true;
			}
			else
			{
#endif
				return ((IBusinessObjectFactoryInternals)job.Factory).IsProcessingOnFactorySavingBeforeTransaction;
#if DEBUG
			}
#endif
		}

		static bool IsGatewaySynchronizable(Job job)
		{
			if (job == null)
			{
				return false;
			}

			var jobParent = job.Parent;
			if (!jobParent.IsGatewayBillingEnabled() || jobParent.GatewayAgent() == default)
			{
				return false;
			}

			if (!(jobParent is IJobCostingPlugIn costPlugIn) || !costPlugIn.CostSupporter.Shipments.Any())
			{
				return false;
			}

			if (!(jobParent is BusinessObject))
			{
				return false;
			}

			if (job.JH_GC != GlbCompany.CurrentCompany.PK)
			{
				return false;
			}

			return true;
		}

		internal static bool IsGatewaySynchronizable(Charge charge)
		{
			var result = !charge.JR_OSSellAmt.IsEmpty;
			result = result && !charge.IsRevenuePosted;
			result = result && charge.InternalFieldsPointToSameEntity();
			result = result && charge.IsDebtorGatewayAgent();

			return result;
		}

		public static ZString GetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessage(Job job)
		{
			return IsGatewaySynchronizable(job) ? ((IJobCostingPlugIn)job.Parent).GetApportionments().GetLockedChildShipmentsJobsErrorMessage() : ZString.Empty;
		}

		public static bool IsRecommendToEnableAutoJRJForGatewaySellApportionment(Job job)
		{
			var result = false;
			if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && IsGatewaySynchronizable(job))
			{
				if (job.Charges.Cast<Charge>().Any(x => !x.JR_OSSellAmt.IsEmpty
												&& !x.IsRevenuePosted
												&& x.IsDebtorGatewayAgent()
												&& x.JR_Calc_RelatedJobNumber.IsEmpty
												&& x.ChargeCode.AC_ChargeType == Core.Constants.ChargeType.Margin
												&& x.ChargeCode.AC_MarginPercentage != 0))
				{
					result = true;
				}
			}
			return result;
		}

		#region Getting Appropriate Gateway Sell Charges to Synchronise

		/// <summary>
		/// Currently Apportionments do not allow more than one charge code per Consol Cost. To avoid a read-only validation error on the
		/// Gateway Apportioment Tab only valid charges with unique charge codes get synchronised.
		/// </summary>
		static List<Charge> GetGatewaySellChargesToSynchronise(Job job)
		{
			var possibleGatewaySellCharges = job.Charges
				.Where(IsGatewaySynchronizable)
				.GroupBy(charge => charge.JR_AC)
				.ToArray();

			var result = new List<Charge>();

			foreach (var chargesByChargeCode in possibleGatewaySellCharges)
			{
				if (chargesByChargeCode.Count() == 1)
				{
					result.AddRange(chargesByChargeCode);
				}
			}

			return result;
		}

		static void DeleteConsolCosts(IEnumerable<JobConsolCost> consolCosts)
		{
			BusinessObjectFactory factory = null;
			var consolCostPKs = new List<ZGuid>();
			foreach (JobConsolCost consolCost in consolCosts)
			{
				if (!consolCost.IsDeleted)
				{
					consolCostPKs.Add(consolCost.PK);
					if (consolCost.CalculationStrategy is JobConsolCost.ConsolCostCalculationStrategyWithCalculations strategy)
					{
						strategy.HandleDelete();
					}
					else
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"CalculationStrategy is of type: {consolCost.CalculationStrategy.GetType()}. Expected type: {typeof(JobConsolCost.ConsolCostCalculationStrategyWithCalculations)}"));
					}

					if (factory == null)
					{
						factory = consolCost.Factory;
					}
				}
			}

			if (factory != null)
			{
				var jobCharges = factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_E6_GatewaySellHeader, consolCostPKs));
				Array.ForEach(jobCharges, (x) => x.JR_E6_GatewaySellHeader = ZGuid.Empty);
			}
		}

#if DEBUG
		[ThreadStatic]
		internal static bool simulateIsProcessingOnFactorySavingBeforeTransaction_ForTestOnly;

		internal static void DeleteConsolCosts_TestOnly(IEnumerable<JobConsolCost> consolCosts)
		{
			DeleteConsolCosts(consolCosts);
		}
#endif

		#endregion

		#region Synchronise Sell Charges

		static List<JobConsolCost> FindOrCreateSyhchronizedConsolCosts(IEnumerable<Charge> sellChargesToSynchronise, ApportionmentListing apportionmentsList, IList<JobConsolCost> consolCosts)
		{
			var costChargesCopiedFromGateway = new List<JobConsolCost>();

			foreach (var sellCharge in sellChargesToSynchronise)
			{
				if (!sellCharge.IsDeleted && !sellCharge.IsDeleting)
				{
					var consolCost = consolCosts.FirstOrDefault(cost => sellCharge.JR_E6_GatewaySellHeader == cost.PK);

					if (consolCost != null)
					{
						if (!DoesCostMatchChargeValues(consolCost, sellCharge))
						{
							CopyConsolCostFromGatewaySell(consolCost, sellCharge);
						}
					}
					else
					{
						consolCost = apportionmentsList.CostsCollection.TryAddNew();
						if (consolCost != null)
						{
							CopyConsolCostFromGatewaySell(consolCost, sellCharge);
						}
					}

					if (consolCost != null)
					{
						costChargesCopiedFromGateway.Add(consolCost);
					}
				}
			}

			return costChargesCopiedFromGateway;
		}

		static void CopyConsolCostFromGatewaySell(JobConsolCost consolCostGeneratedFromGatewaySell, Charge sellCharge)
		{
			using (consolCostGeneratedFromGatewaySell.GetValidationSuspender())
			{
				consolCostGeneratedFromGatewaySell.SellChargeFromSellToCostSynchronisation = sellCharge;
				consolCostGeneratedFromGatewaySell.E6_AC_ChargeCode = sellCharge.JR_AC;
				consolCostGeneratedFromGatewaySell.E6_OH_Creditor = sellCharge.JR_OH_SellAccount;
				consolCostGeneratedFromGatewaySell.E6_RX_NKCurrency = sellCharge.JR_RX_NKSellCurrency;
				consolCostGeneratedFromGatewaySell.E6_ExchangeRate = sellCharge.JR_OSSellExRate;
				consolCostGeneratedFromGatewaySell.E6_OSCostAmount = sellCharge.JR_OSSellAmt;
				consolCostGeneratedFromGatewaySell.E6_AT_TaxRate = sellCharge.JR_AT_SellGSTRate;
				consolCostGeneratedFromGatewaySell.SetTaxDateSafe(sellCharge.JR_SellTaxDate);
				consolCostGeneratedFromGatewaySell.E6_A9_VATClass = sellCharge.JR_A9_SellVATClass;

				var sellChargeDescription = AppendGatewayLinkToRevenueCalcDesc(sellCharge);
				consolCostGeneratedFromGatewaySell.CostCalculationDescription = sellChargeDescription;

				using (sellCharge.GetValidationSuspender())
				{
					sellCharge.JR_E6_GatewaySellHeader = consolCostGeneratedFromGatewaySell.PK;
					consolCostGeneratedFromGatewaySell.E6_GatewaySellChargeID = sellCharge.PK;

					foreach (ApportionSplitCharge apportionedCharge in consolCostGeneratedFromGatewaySell.ApportionmentCharges)
					{
						using (apportionedCharge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
						{
							apportionedCharge.CopyInternalFieldsOver(sellCharge);
							apportionedCharge.SetContext(BusinessContext.AutoJobRevenueJournal);
						}
						apportionedCharge.JR_E6_GatewaySellHeader = consolCostGeneratedFromGatewaySell.PK;
						apportionedCharge.CostCalculationDescription = sellChargeDescription;
					}
				}

				sellCharge.Validation.ValidateRow();
			}
		}

		static bool DoesCostMatchChargeValues(JobConsolCost consolCost, Charge sellCharge)
		{
			bool result = consolCost.E6_AC_ChargeCode == sellCharge.JR_AC
				&& consolCost.E6_OH_Creditor == sellCharge.JR_OH_SellAccount
				&& consolCost.E6_RX_NKCurrency == sellCharge.JR_RX_NKSellCurrency
				&& consolCost.E6_ExchangeRate == sellCharge.JR_OSSellExRate
				&& consolCost.E6_OSCostAmount == sellCharge.JR_OSSellAmt
				&& consolCost.E6_AT_TaxRate == sellCharge.JR_AT_CostGSTRate
				&& consolCost.E6_TaxDate == sellCharge.JR_CostTaxDate
				&& consolCost.E6_A9_VATClass == sellCharge.JR_A9_CostVATClass
				&& consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.InternalJob == sellCharge.InternalJob && x.InternalBranch == sellCharge.InternalBranch && x.InternalDept == sellCharge.InternalDept);

			return result;
		}

#if DEBUG
		internal
#endif
 static ZBlob AppendGatewayLinkToRevenueCalcDesc(Charge charge)
		{
			var existingRevenueCalcDesc = charge.RevenueCalculationDescription;
			var messageText = Res.GetString("d66dfabf-e861-45b2-a163-3b810f2b0a34", "Gateway Sell is apportioned.");

			if (!existingRevenueCalcDesc.IsEmpty && existingRevenueCalcDesc.ToAscii().Contains(messageText))
			{
				return existingRevenueCalcDesc;
			}

			var message = existingRevenueCalcDesc.IsEmpty
				? messageText
				: string.Format("{0}\r\n\r\n{1}", existingRevenueCalcDesc.ToAscii(), messageText);

			var messageAsBlob = ZBlob.FromUTF8(message);
			charge.RevenueCalculationDescription = messageAsBlob;
			charge.RevenueCalculationDescriptionInfo.RefreshBinding();

			return messageAsBlob;
		}

		#endregion

		static void RemoveInvalidChargesFromApportionment(ApportionmentListing apportionmentsList)
		{
			apportionmentsList
				.CostsCollection
				.Cast<JobConsolCost>()
				.ToList()
				.ForEach(c =>
			{
				if (!c.IsDeleted)
				{
					c.RemoveNonApplicableCharges();
				}
			});

			apportionmentsList.CostsCollection.RefreshBinding();
		}
	}
}
