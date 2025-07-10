using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public abstract class CustomsValueCalculator
	{
		protected CustomsValueCalculator(CustomsValuationConfiguration configuration)
		{
			this.configuration = configuration;
		}

		protected readonly CustomsValuationConfiguration configuration;

		protected Func<JobComInvCharge, bool> discountNotYetAppliedToLines = (charge) => charge.IsDiscount && !charge.J7_IsIncludedInITOT;
		protected Func<JobComInvCharge, bool> dutiableChargesNotIncludedInLines = (charge) => charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT == charge.IsDiscount);
		protected Func<JobComInvCharge, bool> nonDutiableChargesIncludedInLines = (charge) => !charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT != charge.IsDiscount);

		public abstract ZDecimal GetCustomsValue(ICustomsValueCalculationDataProvider line);

		#region Util

		protected static ZDecimal GetTotal(CurrencyConverter currencyConverter, IEnumerable<JobComInvCharge> charges, Func<JobComInvCharge, bool> shouldIncludeCharge)
		{
			Money result = Money.Empty;

			if (charges != null)
			{
				foreach (JobComInvCharge charge in charges.Where(shouldIncludeCharge))
				{
					result = currencyConverter.Add(result, charge.Money);
				}
			}
			return result.Amount;
		}

		#endregion
	}

	public class CustomsValueByFactorCalculator : CustomsValueCalculator
	{
		public CustomsValueByFactorCalculator(ICustomsValueCalculationDataProviderForInvoiceHeader factorProvider, CustomsValuationByFactorConfiguration configuration)
			: base(configuration)
		{
			this.factorProvider = factorProvider;
			currencyConverter = factorProvider.CurrencyConverter;
		}

		readonly ICustomsValueCalculationDataProviderForInvoiceHeader factorProvider;
		readonly CurrencyConverter currencyConverter;
		CustomsValuationByFactorConfiguration Configuration => (CustomsValuationByFactorConfiguration)configuration;

		public ZDecimal CustomsFactor
		{
			get
			{
				if (!customsFactor.HasValue)
				{
					customsFactor = CalculateCustomsFactor();
				}
				return customsFactor.Value;
			}
		}
		ZDecimal? customsFactor;

		ZDecimal CalculateCustomsFactor()
		{
			var lineTotal = factorProvider.LinePriceAmount;
			var divisor = lineTotal;
			ZDecimal? divident = null;

			var allCharges = factorProvider.AllCharges;

			if (Configuration.ApplyDiscountBeforeFactorCalculation)
			{
				divisor = divisor - GetTotal(currencyConverter, allCharges, discountNotYetAppliedToLines);
			}

			var lineLevelCharges = Configuration.LineLevelCharges;
			if (divisor != ZDecimal.Zero)
			{
				var lineLevelChargesIncludedInLine = GetTotal(currencyConverter, allCharges, (charge) => (!lineLevelCharges.Contains(charge.J7_ChargeType.ToString()) && dutiableChargesNotIncludedInLines(charge)));
				var chargesIrrelevantButIncludedInLinePrice = GetTotal(currencyConverter, allCharges, (charge) => (!lineLevelCharges.Contains(charge.J7_ChargeType.ToString()) && nonDutiableChargesIncludedInLines(charge)));
				var cvInInvoiceCurrency = lineTotal + lineLevelChargesIncludedInLine - chargesIrrelevantButIncludedInLinePrice;
				divident = factorProvider.CurrencyConverter?.ConvertExact(new Money(cvInInvoiceCurrency, factorProvider.InvoiceCurrency), configuration.LocalCurrency)?.Amount;
			}

			ZDecimal result = (divident ?? 0m) / divisor;
			result = result.Round(Configuration.NumberOfDecimalPlacesForCustomsFactor);
			return result;
		}

		public override ZDecimal GetCustomsValue(ICustomsValueCalculationDataProvider line)
		{
			var linePrice = line.LinePriceAmount;
			var allCharges = line.AllCharges;
			if (Configuration.ApplyDiscountBeforeFactorCalculation)
			{
				linePrice = linePrice - GetTotal(currencyConverter, allCharges, discountNotYetAppliedToLines);
			}

			var lineLevelCharges = Configuration.LineLevelCharges;
			var lineLevelChargesIncludedInLine = GetTotal(currencyConverter, allCharges, (charge) => lineLevelCharges.Contains(charge.J7_ChargeType.ToString()) && nonDutiableChargesIncludedInLines(charge));
			var lineLevelChargesNotIncludedInLine = GetTotal(currencyConverter, allCharges, (charge) => lineLevelCharges.Contains(charge.J7_ChargeType.ToString()) && dutiableChargesNotIncludedInLines(charge));
			return (linePrice - lineLevelChargesIncludedInLine) * CustomsFactor + lineLevelChargesNotIncludedInLine;
		}
	}
}
