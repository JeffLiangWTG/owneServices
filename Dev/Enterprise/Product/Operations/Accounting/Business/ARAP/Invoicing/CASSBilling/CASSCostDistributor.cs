using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSCostDistributor
	{
		public CASSCostDistributor(CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate)
		{
			Argument.NotNull(billingLine, "CASSBillingLine");
			this.billingLine = billingLine;
			this.componentList = billingLine.AggregatedCostLine.CostComponentList;
			this.gstTaxRate = gstTaxRate;
			this.freeGSTTaxRate = freeGSTTaxRate;
		}
		readonly CASSBillingLine billingLine;
		readonly AccTaxRate gstTaxRate;
		readonly AccTaxRate freeGSTTaxRate;
		readonly CodeDescriptionPairList componentList;

		#region Invoice Line Creation related functions

		public IEnumerable<InvoiceLineInfo> GetInvoiceLineInfoWithDistribution(bool isTaxApplicable)
		{
			return GetInvoiceLineInfo(true, isTaxApplicable);
		}

		public IEnumerable<InvoiceLineInfo> GetInvoiceLineInfoWithoutDistribution(bool isTaxApplicable)
		{
			return GetInvoiceLineInfo(false, isTaxApplicable);
		}

		IEnumerable<InvoiceLineInfo> GetInvoiceLineInfo(bool distribute, bool isTaxApplicable)
		{
			var result = new List<InvoiceLineInfo>();

			Action<InvoiceLineInfo> add = (x) => { if (x != null) { result.Add(x); } };

			Func<ZDecimal, AccTaxRate> getEffectiveTaxRate = (amount) =>
																{
																	var isGSTApplicableValidated = billingLine.IsGSTApplicable && amount != 0;
																	ThrowCASSGstRegistryNotSetIfApplicable(isTaxApplicable, isGSTApplicableValidated);
																	return isTaxApplicable ? (isGSTApplicableValidated ? gstTaxRate : freeGSTTaxRate) : null;
																};

			if (IsCostAndTaxOppositeSigned)
			{
				if (gstTaxRate == null)
				{
					throw new CASSGstRegistryNotSetException(CASSBilling.CASSGstNotSetErrorMessage());
				}

				add(InvoiceLineInfo.New(billingLine.CASSCostValue * CostMultiplier + billingLine.CASSCostTaxValue * (100 / gstTaxRate.GetRate(ZDate.Today)) * TaxMultiplier
											 , 0
											 , getEffectiveTaxRate(0)
											 , distribute ? DistributeCostsWhenCostAndTaxOppositeSigned(CostMultiplier, TaxMultiplier) : null));

				add(InvoiceLineInfo.New(billingLine.CASSCostTaxValue * (100 / gstTaxRate.GetRate(ZDate.Today))
											 , billingLine.IsGSTApplicable ? billingLine.CASSCostTaxValue : new ZDecimal(0M)
											 , getEffectiveTaxRate(billingLine.CASSCostTaxValue)
											 , distribute ? DistributeTaxWhenCostAndTaxOppositeSigned() : null));

				add(InvoiceLineInfo.New(billingLine.CASSCostAdjustedValue * CostMultiplier + billingLine.CASSCostTaxAdjustedValue * (100 / gstTaxRate.GetRate(ZDate.Today)) * TaxMultiplier
											 , 0
											 , getEffectiveTaxRate(0)
											 , distribute ? DistributeAdjustedCostsWhenCostAndTaxOppositeSigned(CostMultiplier, TaxMultiplier) : null));

				add(InvoiceLineInfo.New(billingLine.CASSCostTaxAdjustedValue * (100 / gstTaxRate.GetRate(ZDate.Today))
											 , billingLine.IsGSTApplicable ? billingLine.CASSCostTaxAdjustedValue : new ZDecimal(0M)
											 , getEffectiveTaxRate(billingLine.CASSCostTaxAdjustedValue)
											 , distribute ? DistributeAdjustedTaxWhenCostAndTaxOppositeSigned() : null));
			}
			else
			{
				add(InvoiceLineInfo.New(billingLine.CASSCostValue
											 , billingLine.IsGSTApplicable ? billingLine.CASSCostTaxValue : new ZDecimal(0M)
											 , getEffectiveTaxRate(billingLine.CASSCostTaxValue)
											 , distribute ? DistributeCostAndTax() : null));

				add(InvoiceLineInfo.New(billingLine.CASSCostAdjustedValue
											 , billingLine.IsGSTApplicable ? billingLine.CASSCostTaxAdjustedValue : new ZDecimal(0M)
											 , getEffectiveTaxRate(billingLine.CASSCostTaxAdjustedValue)
											 , distribute ? DistributeAdjustedCostAndTax() : null));
			}

			return result;
		}

		#endregion

		#region Cost and Tax Distribution Functions

		IEnumerable<CASSCostDistributorInfo> DistributeCostAndTax()
		{
			return DistributeCostAndTax(false);
		}

		IEnumerable<CASSCostDistributorInfo> DistributeAdjustedCostAndTax()
		{
			return DistributeCostAndTax(true);
		}

		IEnumerable<CASSCostDistributorInfo> DistributeCostsWhenCostAndTaxOppositeSigned(int costMultiplier, int taxMultiplier)
		{
			var result = new List<CASSCostDistributorInfo>();

			var costDistributions = DistributeComponentCost(false);
			var taxDistributions = DistributeTax(false);

			foreach (ZGuid chargeCodePK in ChargeCodePKs)
			{
				var costDistributedToChargeCode = costDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Cost);
				var taxDistributedToChargeCode = taxDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Tax);
				var costWithTax = costDistributedToChargeCode * costMultiplier + taxDistributedToChargeCode * (100.0M / gstTaxRate.GetRate(ZDate.Today)) * taxMultiplier;

				AddCostDistributionInfo(result, chargeCodePK, ZString.Empty, costWithTax, 0);
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeTaxWhenCostAndTaxOppositeSigned()
		{
			var result = new List<CASSCostDistributorInfo>();

			var taxDistributions = DistributeTax(false);
			foreach (ZGuid chargeCodePK in ChargeCodePKs)
			{
				var taxDistributedToChargeCode = taxDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Tax);
				var costDistributedToChargeCode = taxDistributedToChargeCode * (100.0M / gstTaxRate.GetRate(ZDate.Today));

				AddCostDistributionInfo(result, chargeCodePK, ZString.Empty, costDistributedToChargeCode, taxDistributedToChargeCode);
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeAdjustedCostsWhenCostAndTaxOppositeSigned(int costMultiplier, int taxMultiplier)
		{
			var result = new List<CASSCostDistributorInfo>();

			var costDistributions = DistributeComponentCost(true);
			var taxDistributions = DistributeTax(true);

			foreach (ZGuid chargeCodePK in ChargeCodePKs)
			{
				var costDistributedToChargeCode = costDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Cost);
				var taxDistributedToChargeCode = taxDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Tax);
				var costWithTax = costDistributedToChargeCode * costMultiplier + taxDistributedToChargeCode * (100.0M / gstTaxRate.GetRate(ZDate.Today)) * taxMultiplier;

				AddCostDistributionInfo(result, chargeCodePK, ZString.Empty, costWithTax, 0);
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeAdjustedTaxWhenCostAndTaxOppositeSigned()
		{
			var result = new List<CASSCostDistributorInfo>();

			var taxDistributions = DistributeTax(true);
			foreach (ZGuid chargeCodePK in ChargeCodePKs)
			{
				var taxDistributedToChargeCode = taxDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Tax);
				var costDistributedToChargeCode = taxDistributedToChargeCode * (100.0M / gstTaxRate.GetRate(ZDate.Today));

				AddCostDistributionInfo(result, chargeCodePK, ZString.Empty, costDistributedToChargeCode, taxDistributedToChargeCode);
			}

			return result;
		}

		#endregion

		#region Implementation

		ZGuid[] ChargeCodePKs
		{
			get
			{
				if (chargeCodePKs == null)
				{
					chargeCodePKs = BillingLine.AggregatedCostLine.GetCASSChargeCodePKsFromRegistry();
				}
				return chargeCodePKs;
			}
		}
		ZGuid[] chargeCodePKs;

		CASSBillingLine BillingLine
		{
			get
			{
				return billingLine;
			}
		}

		bool HasAccrual
		{
			get
			{
				return BillingLine.SystemCostAccrualValue != 0m;
			}
		}

		bool IsCostAndTaxOppositeSigned
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_IsGSTRegistered
						&& billingLine.CASSCostTaxValue != 0
						&& ((billingLine.CASSCostValue > 0 && billingLine.CASSCostTaxValue < 0) || (billingLine.CASSCostValue < 0 && billingLine.CASSCostTaxValue > 0));
			}
		}

		int TaxMultiplier { get { return billingLine.CASSCostTaxValue < 0 ? -1 : 1; } }

		int CostMultiplier { get { return TaxMultiplier * -1; } }

		void ThrowCASSGstRegistryNotSetIfApplicable(bool currentCompanyIsGSTApplicable, bool isGSTApplicableValidated)
		{
			if ((isGSTApplicableValidated && currentCompanyIsGSTApplicable && gstTaxRate == null) || (currentCompanyIsGSTApplicable && freeGSTTaxRate == null))
			{
				throw new CASSGstRegistryNotSetException(CASSBilling.CASSGstNotSetErrorMessage());
			}
		}

		IEnumerable<CASSCostDistributorInfo> DistributeCostAndTax(bool isAdjustment)
		{
			var result = new List<CASSCostDistributorInfo>();

			var costDistributions = DistributeComponentCost(isAdjustment);
			var taxDistributions = DistributeTax(isAdjustment);

			foreach (ZGuid chargeCodePK in ChargeCodePKs)
			{
				var costDistributedToChargeCode = costDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Cost);
				var taxDistributedToChargeCode = taxDistributions.Where(x => x.ChargeCodePK == chargeCodePK).Sum(x => x.Tax);

				AddCostDistributionInfo(result, chargeCodePK, ZString.Empty, costDistributedToChargeCode, taxDistributedToChargeCode);
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeComponentCost(bool isAdjustment)
		{
			var result = new List<CASSCostDistributorInfo>();
			var multiplier = (isAdjustment ? -1 : 1);

			foreach (CodeDescriptionPair component in componentList)
			{
				var componentAmount = BillingLine.AggregatedCostLine.GetComponentAmount(isAdjustment, component.Code);
				result.AddRange(DistributeAmountAmongChargeCodes(component.Code, componentAmount, multiplier, false));
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeTax(bool isAdjustment)
		{
			var result = new List<CASSCostDistributorInfo>();
			var multiplier = (isAdjustment ? -1 : 1);

			foreach (CodeDescriptionPair taxComponent in BillingLine.AggregatedCostLine.VATComponentList)
			{
				var taxComponentAmount = BillingLine.AggregatedCostLine.GetComponentAmount(isAdjustment, taxComponent.Code);
				if (taxComponentAmount != 0)
				{
					var taxDistributionByCostComponent = BillingLine.AggregatedCostLine.GetVATApportionedToCostComponents(isAdjustment, taxComponent.Code);
					foreach (KeyValuePair<ZString, decimal> item in taxDistributionByCostComponent)
					{
						result.AddRange(DistributeAmountAmongChargeCodes(item.Key, item.Value, multiplier, true));
					}
				}
			}

			return result;
		}

		IEnumerable<CASSCostDistributorInfo> DistributeAmountAmongChargeCodes(ZString componentCode, ZDecimal amount, int multiplier, bool isTax)
		{
			var minAmount = BillingLine.CASSCostCurrency.CurrencyMinAmount();
			var setCurrencyAdjustedAmount = new Func<ZDecimal, ZDecimal>(inputAmnt => (inputAmnt != 0 && Math.Abs(inputAmnt) < Math.Abs(minAmount)) ? (Math.Sign(inputAmnt) != Math.Sign(minAmount) ? (ZDecimal)(minAmount * (-1)) : minAmount) : inputAmnt);
			var result = new List<CASSCostDistributorInfo>();
			if (amount != 0)
			{
				foreach (ZGuid chargeCodePK in ChargeCodePKs)
				{
					var distributionRatio = GetCostDistributionRatio(componentCode, chargeCodePK);
					if (distributionRatio != 0)
					{
						var apportionedAmount = new ZDecimal(amount * distributionRatio);
						var cost = setCurrencyAdjustedAmount(isTax ? 0 : apportionedAmount * multiplier);
						var tax = setCurrencyAdjustedAmount(isTax ? apportionedAmount * multiplier : 0);
						AddCostDistributionInfo(result, chargeCodePK, componentCode, cost, tax);
					}
				}

				AdjustValuesToSumCorrectlyAfterDistribution(result.Where(x => x.ComponentName == componentCode).ToList(),
																							 amount * multiplier,
																							 (x) => { return isTax ? x.Tax : x.Cost; },
																							 (x, y) =>
																							 {
																								 result.Remove(x);
																								 var c = isTax ? 0 : y;
																								 var t = isTax ? y : 0;
																								 AddCostDistributionInfo(result, x.ChargeCodePK, x.ComponentName, c, t);
																							 });
			}
			return result;
		}

		ZDecimal GetCostDistributionRatio(string cassComponent, ZGuid chargeCodePK)
		{
			ZDecimal result = ZDecimal.Zero;

			var chargeCodes = BillingLine.AggregatedCostLine.GetCASSChargeCodePKsFromRegistry(cassComponent);
			if (chargeCodes != null && chargeCodes.Contains(chargeCodePK))
			{
				var hasACRForThisChargeCode = false;
				var acrAmountForThisChargeCode = 0M;
				var totalACRAmountForThisCASSComponent = 0M;

				if (HasAccrual)
				{
					var accrualAmounts = BillingLine.GetAccrulaAmountsByChargeCodes();
					if (accrualAmounts.ContainsKey(chargeCodePK))
					{
						totalACRAmountForThisCASSComponent = accrualAmounts.Where(x => chargeCodes.Contains(x.Key)).Sum(x => x.Value.Item1);
						if (totalACRAmountForThisCASSComponent != 0)
						{
							acrAmountForThisChargeCode = accrualAmounts[chargeCodePK].Item1;
							hasACRForThisChargeCode = true;
						}
					}
				}

				if (hasACRForThisChargeCode)
				{
					result = acrAmountForThisChargeCode / totalACRAmountForThisCASSComponent;
				}
				else
				{
					result = 1.0M / chargeCodes.Length;
				}
			}

			return result;
		}

		void AdjustValuesToSumCorrectlyAfterDistribution(List<CASSCostDistributorInfo> distributions, ZDecimal originalTotalAmount, Func<CASSCostDistributorInfo, ZDecimal> getDistributedAmount, Action<CASSCostDistributorInfo, ZDecimal> setAdjustedDistributedAmount)
		{
			if (distributions != null && distributions.Count > 0 && BillingLine.CASSCostCurrency != null)
			{
				var sortedItems = from item in distributions where getDistributedAmount(item) != 0 orderby Math.Abs(getDistributedAmount(item)) descending select item;
				var totalOnSplitItems = sortedItems.Sum(item => getDistributedAmount(item));

				var localCurrencyMinAmount = BillingLine.CASSCostCurrency.CurrencyMinAmount();
				var minLocalAmount = localCurrencyMinAmount * sortedItems.Count(x => getDistributedAmount(x) != 0);
				if (Math.Abs(originalTotalAmount) < minLocalAmount)
				{
					var numberOfChargesToUpdate = (int)decimal.Round((minLocalAmount - Math.Abs(originalTotalAmount)) / localCurrencyMinAmount, 0);

					var itemsToUpdate = sortedItems.Reverse().Take(numberOfChargesToUpdate).ToArray();
					foreach (var itemToUpdate in itemsToUpdate)
					{
						setAdjustedDistributedAmount(itemToUpdate, 0m);
					}
					return;
				}

				ZDecimal amountDifference = originalTotalAmount - totalOnSplitItems;

				if (amountDifference != 0m)
				{
					var completed = false;
					var lastCharge = sortedItems.Last();

					foreach (var item in sortedItems)
					{
						completed = ApplyMaxPossibleAdjustment(item, getDistributedAmount(item), originalTotalAmount, setAdjustedDistributedAmount, ref amountDifference);
						if (completed)
						{
							break;
						}
						else if (item.Equals(lastCharge))
						{
							setAdjustedDistributedAmount(item, 0m); // Last attempt to distribute rounding error by zeroing one charge. If it is not enough will get an unapportioned amount
						}
					}
				}
			}
		}

		bool ApplyMaxPossibleAdjustment(CASSCostDistributorInfo item, ZDecimal chargeAmount, ZDecimal totalAmount, Action<CASSCostDistributorInfo, ZDecimal> setAdjustedDistributedAmount, ref ZDecimal adjustment)
		{
			if (BillingLine.CASSCostCurrency == null)
			{
				return false; // Cannot do anything without currency
			}

			var result = false;

			int amountSign = Math.Sign(chargeAmount);
			if (amountSign == 0)    // Current Amount is zero try to find out Header Amount Sign
			{
				amountSign = Math.Sign(totalAmount) != 0 ?
					 Math.Sign(totalAmount) :   // Use Total Amount as a fallback to figure out Amount sign for charge. 
					 Math.Sign(adjustment);     // If Total Amount is 0 we use Adjustment sign
			}

			var maxAdjustment = amountSign == Math.Sign(adjustment) ?
								adjustment :                // if Amount and Adjustment have the same sign we can update Amount by full Adjustment
								(ZDecimal)(-(chargeAmount - BillingLine.CASSCostCurrency.CurrencyMinAmount() * amountSign));    // For Amount and Adjustment with different sign we can update Amount to be not less than 0.01 or 1 with the same sign. 
																											// That is why We substract and reverse sign to get largest Adjustment that can be applied to Amount
			if (maxAdjustment != 0m)
			{
				if (Math.Abs(adjustment) <= Math.Abs(maxAdjustment))
				{
					setAdjustedDistributedAmount(item, chargeAmount + adjustment);
					adjustment = ZDecimal.Zero;
					result = true;
				}
				else
				{
					setAdjustedDistributedAmount(item, chargeAmount + maxAdjustment);
					adjustment -= maxAdjustment;
				}
			}
			return result;
		}

		void AddCostDistributionInfo(List<CASSCostDistributorInfo> distributions, ZGuid chargeCodePK, ZString componentCode, ZDecimal cost, ZDecimal tax)
		{
			if (cost != 0M || tax != 0M)
			{
				distributions.Add(new CASSCostDistributorInfo(chargeCodePK, componentCode, cost, tax));
			}
		}

		#endregion

		public class CASSCostDistributorInfo
		{
			public CASSCostDistributorInfo(ZGuid chargeCodePK, ZDecimal cost, ZDecimal tax)
			{
				this.ChargeCodePK = chargeCodePK;
				this.ComponentName = "";
				this.Cost = cost;
				this.Tax = tax;
			}

			public CASSCostDistributorInfo(ZGuid chargeCodePK, ZString componentName, ZDecimal cost, ZDecimal tax)
			{
				this.ChargeCodePK = chargeCodePK;
				this.ComponentName = componentName;
				this.Cost = cost;
				this.Tax = tax;
			}

			public override bool Equals(Object obj)
			{
				var castedVal = (CASSCostDistributorInfo)obj;
				return castedVal.ChargeCodePK == ChargeCodePK && castedVal.ComponentName == ComponentName && castedVal.Cost == Cost && castedVal.Tax == Tax;
			}

			public static bool operator ==(CASSCostDistributorInfo x1, CASSCostDistributorInfo x2)
			{
				return x1.ChargeCodePK == x2.ChargeCodePK && x1.ComponentName == x2.ComponentName && x1.Cost == x2.Cost && x1.Tax == x2.Tax;
			}

			public static bool operator !=(CASSCostDistributorInfo x1, CASSCostDistributorInfo x2)
			{
				return x1.ChargeCodePK != x2.ChargeCodePK || x1.ComponentName != x2.ComponentName || x1.Cost != x2.Cost || x1.Tax != x2.Tax;
			}

			public override int GetHashCode()
			{
				return ChargeCodePK.GetHashCode() ^ ComponentName.GetHashCode() ^ Cost.GetHashCode() ^ Tax.GetHashCode();
			}

			public readonly ZGuid ChargeCodePK;
			public readonly ZString ComponentName;
			public readonly ZDecimal Cost;
			public readonly ZDecimal Tax;
		}

		public class InvoiceLineInfo
		{
			InvoiceLineInfo(ZDecimal exTaxAmount, ZDecimal taxAmount, AccTaxRate taxRate, IEnumerable<CASSCostDistributorInfo> distributionInfo)
			{
				this.exTaxAmount = exTaxAmount;
				this.taxAmount = taxAmount;
				this.taxRate = taxRate;
				this.distributionInfo = distributionInfo;
			}
			readonly ZDecimal exTaxAmount;
			readonly ZDecimal taxAmount;
			readonly AccTaxRate taxRate;
			readonly IEnumerable<CASSCostDistributorInfo> distributionInfo;

			public static InvoiceLineInfo New(ZDecimal exTaxAmount, ZDecimal taxAmount, AccTaxRate taxRate, IEnumerable<CASSCostDistributorInfo> distributionInfo)
			{
				InvoiceLineInfo result = null;
				if (exTaxAmount != 0)
				{
					result = new InvoiceLineInfo(exTaxAmount, taxAmount, taxRate, distributionInfo);
				}
				return result;
			}

			public ZDecimal ExTaxAmount { get { return exTaxAmount; } }
			public ZDecimal TaxAmount { get { return taxAmount; } }
			public AccTaxRate TaxRate { get { return taxRate; } }
			public IEnumerable<CASSCostDistributorInfo> DistributionInfo { get { return distributionInfo; } }
		}

		#region Test Only Functions

#if DEBUG
		public IEnumerable<CASSCostDistributorInfo> DistributeCostAndTax_ForTestOnly()
		{
			return DistributeCostAndTax();
		}

		public IEnumerable<CASSCostDistributorInfo> DistributeAdjustedCostAndTax_ForTestOnly()
		{
			return DistributeAdjustedCostAndTax();
		}

		public IEnumerable<CASSCostDistributorInfo> DistributeCostsWhenCostAndTaxOppositeSigned_ForTestOnly(int costMultiplier, int taxMultiplier)
		{
			return DistributeCostsWhenCostAndTaxOppositeSigned(costMultiplier, taxMultiplier);
		}

		public IEnumerable<CASSCostDistributorInfo> DistributeTaxWhenCostAndTaxOppositeSigned_ForTestOnly()
		{
			return DistributeTaxWhenCostAndTaxOppositeSigned();
		}

		public IEnumerable<CASSCostDistributorInfo> DistributeAdjustedCostsWhenCostAndTaxOppositeSigned_ForTestOnly(int costMultiplier, int taxMultiplier)
		{
			return DistributeAdjustedCostsWhenCostAndTaxOppositeSigned(costMultiplier, taxMultiplier);
		}

		public IEnumerable<CASSCostDistributorInfo> DistributeAdjustedTaxWhenCostAndTaxOppositeSigned_ForTestOnly()
		{
			return DistributeAdjustedTaxWhenCostAndTaxOppositeSigned();
		}

#endif
		#endregion
	}
}
