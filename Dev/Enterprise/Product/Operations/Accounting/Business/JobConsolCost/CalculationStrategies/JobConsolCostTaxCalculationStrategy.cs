using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		abstract class JobConsolCostTaxCalculationStrategy
		{
			protected JobConsolCostTaxCalculationStrategy(JobConsolCost cost)
			{
				Argument.NotNull(cost, "JobConsolCost");
				Cost = cost;
			}
			protected readonly JobConsolCost Cost;

			#region E6_OSGSTAmount

			public abstract ZDecimal GetE6_OSGSTAmount();

			public abstract void SetE6_OSGSTAmount(ZDecimal value);

			#endregion

			#region E6_OSExtraTaxAmount & E6_OSGSTRealAmount

			public ZDecimal GetE6_OSExtraTaxAmount() => CalculateOSExtraAmount();

			public ZDecimal GetE6_OSGSTRealAmount() => CalculateOSGSTRealTaxAmount();

			#endregion

			#region Reset GST Amount & Overridden Flag

			public void ResetGSTAmount()
			{
				ResetGSTAmountCore();
				RefreshGSTRelatedProperties();
			}

			protected abstract void ResetGSTAmountCore();

			public void ResetGSTAmountOverriddenFlag()
			{
				//when IsGSTInclusiveAmount is true, then the Cost has APInvoiceConsolCostCollection as ParentCollection.
				//In that case, we always keep E6_IsTaxAmountOverridden = true.
				if (Cost.E6_IsTaxAmountOverridden != IsAPInvoiceConsolCost && !Cost.IsPosted)
				{
					Cost.E6_IsTaxAmountOverridden = IsAPInvoiceConsolCost;
				}
			}

			public void RefreshGSTRelatedProperties()
			{
				Cost.E6_OSExtraTaxAmountInfo.RefreshBinding();
				Cost.E6_OSGSTRealAmountInfo.RefreshBinding();
			}

			#endregion

			public ZDecimal GetOSCostAmountFromGSTInclusiveAmount(ZDecimal gstInclusiveAmount)
			{
				return Cost.IsGSTInclusiveAmount ? (Cost.TaxRate != null ? (ZDecimal)AccountingUtils.Round(new ZDecimal((100 * gstInclusiveAmount / (100 + TaxRateAmount + ExtraTaxRateAmount))), Cost.Currency) : gstInclusiveAmount) : Cost.E6_OSCostAmount;
			}

			public ZDecimal TaxRateAmount => (taxRateAmount ?? (taxRateAmount = OSGSTCalculationStrategyStaticHelper.GetTaxRateAmount(Cost.Factory, Cost.E6_AT_TaxRate, Cost.E6_TaxDate))).Value;
			ZDecimal? taxRateAmount;

			ZDecimal ExtraTaxRateAmount => (extraTaxRateAmount ?? (extraTaxRateAmount = OSGSTCalculationStrategyStaticHelper.GetExtraTaxRateAmount(Cost.Factory, Cost.E6_AT_TaxRate, Cost.E6_TaxDate))).Value;
			ZDecimal? extraTaxRateAmount;

			#region Helper Functions

			public bool IsAPInvoiceConsolCost
			{
				get
				{
					var collection = ((IBusinessObjectInternals)Cost).ParentCollections.FirstOrDefault();
					return collection is APInvoiceConsolCostCollection;
				}
			}

			protected ZDecimal CalculateTaxAmount()
			{
				return OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(Cost.E6_OSCostAmount, TaxRateAmount, ExtraTaxRateAmount, Cost.Currency, Cost.Factory, Cost.E6_AT_TaxRate);
			}

			ZDecimal CalculateOSGSTRealTaxAmount()
			{
				var result = ZDecimal.Zero;
				if (Cost.TaxRate != null && ExtraTaxRateAmount + TaxRateAmount != 0)
				{
					result = AccountingUtils.Round(GetE6_OSGSTAmount() * TaxRateAmount / (ExtraTaxRateAmount + TaxRateAmount), Cost.Currency);
				}
				return result;
			}

			ZDecimal CalculateOSExtraAmount()
			{
				var result = ZDecimal.Zero;
				if (Cost.TaxRate != null && ExtraTaxRateAmount + TaxRateAmount != 0)
				{
					result = AccountingUtils.Round(GetE6_OSGSTAmount() * ExtraTaxRateAmount / (ExtraTaxRateAmount + TaxRateAmount), Cost.Currency);
				}
				return result;
			}

			#endregion
		}

		public static class OSGSTCalculationStrategyStaticHelper
		{
			public static ZDecimal CalculateTaxAmountCore(ZDecimal exAmount, ZDecimal taxRateAmount, ZDecimal extraTaxRateAmount, RefCurrency currency, BusinessObjectFactory factory, ZGuid taxRatePK)
			{
				ZDecimal result = 0m;
				var currencyDecimals = currency == null ? GlbCompany.CurrentCompany.GetLocalDecimals() : currency.Decimals;
				AccTaxRate rate;
				if (taxRatePK.IsValid
					&& (rate = factory.Load<AccTaxRate>(taxRatePK)) != null)
				{
					result = AccountingUtils.Round(TaxAmountCalculator.GetTaxAmountFromExTaxAmount(exAmount, rate, taxRateAmount, extraTaxRateAmount, currencyDecimals), currency);
				}
				return result;
			}

			public static ZDecimal GetTaxRateAmount(BusinessObjectFactory factory, ZGuid taxRatePK, ZDate taxDate)
			{
				ZDecimal result = 0m;
				AccTaxRate rate;
				if (taxRatePK.IsValid
					&& (rate = factory.Load<AccTaxRate>(taxRatePK)) != null)
				{
					result = rate.GetRate(taxDate);
				}
				return result;
			}

			public static ZDecimal GetExtraTaxRateAmount(BusinessObjectFactory factory, ZGuid taxRatePK, ZDate taxDate)
			{
				ZDecimal result = 0m;
				AccTaxRate rate;
				if (taxRatePK.IsValid
					&& (rate = factory.Load<AccTaxRate>(taxRatePK)) != null)
				{
					result = rate.GetEffectiveExtraRate(taxDate);
				}
				return result;
			}
		}

		class JobConsolCostTaxCalculationWhenNotOverridden : JobConsolCostTaxCalculationStrategy
		{
			public JobConsolCostTaxCalculationWhenNotOverridden(JobConsolCost cost)
				: base(cost)
			{
			}

			public override ZDecimal GetE6_OSGSTAmount() => CalculateTaxAmount();

			public override void SetE6_OSGSTAmount(ZDecimal value)
			{
				var message = string.Format(CultureInfo.CurrentCulture, $@"Trying to set GST amount when E6_IsTaxAmountOverridden is false.");
				ErrorReporter.ReportOnce("SettingGSTAmountAtConsolLevelWhenE6_IsTaxAmountOverriddenIsFalse_1", message);
			}

			protected override void ResetGSTAmountCore()
			{
				Cost.E6_OSGSTAmount = ZDecimal.Zero;
			}
		}

		class JobConsolCostTaxCalculationWhenOverridden : JobConsolCostTaxCalculationStrategy
		{
			public JobConsolCostTaxCalculationWhenOverridden(JobConsolCost cost)
				: base(cost)
			{
				Argument.NotNull(cost, nameof(cost));
			}

			public override ZDecimal GetE6_OSGSTAmount() => Cost.E6_OSGSTAmount;

			public override void SetE6_OSGSTAmount(ZDecimal amt)
			{
				if (Cost.Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice))
				{
					var defaultOSTaxAmount = CalculateTaxAmount();
					if (defaultOSTaxAmount != amt)
					{
						if (!Cost.Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
						{
							Cost.Factory.SetContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost);
						}
					}
					else
					{
						if (Cost.Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
						{
							Cost.Factory.RemoveContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost);
						}
					}
				}
				Cost.E6_OSGSTAmount = amt;
				Cost.E6_OSGSTAmount_CalcInfo.RefreshBinding();
				Cost.ApportionGSTCharges();
				RefreshGSTRelatedProperties();
			}

			protected override void ResetGSTAmountCore()
			{
				SetE6_OSGSTAmount(CalculateTaxAmount());
			}
		}
	}
}
