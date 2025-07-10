using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration.CalculateTaxForCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class TaxCalculatorHelperForCharge : ICalculateOSSellTaxForCharge, ICalculateOSCostTaxForCharge
	{
		public static (ZDecimal OSSellGSTAmount, ZDecimal LocalSellGSTAmount) CalculateSellGSTAmounts(BusinessObjectFactory factory, LineDetails line, ChargeSellDetails charge)
		{
			ZDecimal osSellGSTAmount = 0;
			ZDecimal localSellGSTAmount = 0;
			ZDecimal? localSellGSTAmountBasedOnLocalSellAmount = null;

			var chargeSellCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, charge.SellCurrencyCode);
			var chargeSellTaxRate = factory.Load<AccTaxRate>(charge.SellTaxRatePK);

			if (JobCharge.GetIsRevenuePosted(line.Type))
			{
				if (chargeSellCurrency != null && line.CurrencyCode != chargeSellCurrency.Code)
				{
					osSellGSTAmount = TaxAmountCalculator.GetCurrentCompany(factory, charge.CompanyPK).ExchangeRate.LocalToForeign(line.LocalTaxAmount, charge.SellExRate, chargeSellCurrency.Code);
				}
				else
				{
					osSellGSTAmount = line.OSAmount - charge.SellOSAmount;
				}

				localSellGSTAmount = line.LocalTaxAmount;
			}
			else
			{
				//osSellGSTAmount calculations
				if (TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(charge.CompanyPK))
				{
					osSellGSTAmount = TaxAmountCalculator.GetOSTaxAmount(factory, charge.SellOSAmount, chargeSellTaxRate, charge.TaxRate, charge.EffectiveExtraTaxRate, GetLocalSellGSTAmountBasedOnLocalSellAmount(), charge.SellExRate, chargeSellCurrency, charge.CompanyPK);
				}
				else
				{
					osSellGSTAmount = TaxAmountCalculator.GetOSTaxAmount(factory, charge.SellOSAmount, chargeSellTaxRate, charge.TaxRate, charge.EffectiveExtraTaxRate, chargeSellCurrency, charge.CompanyPK);
				}

				if (chargeSellCurrency != null && charge.SellExRate == 1)
				{
					osSellGSTAmount = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGSTForRevenueCharge(charge.ChargeCodePK, chargeSellCurrency.Code, charge.SellOSAmount, osSellGSTAmount, charge.CountryCode, charge.CompanyPK);
				}

				//localSellGSTAmount calculations
				if (charge.SellExRate == 1)
				{
					localSellGSTAmount = osSellGSTAmount;
				}
				else if (JobCharge.GetIsBillingInLocalCurrency(charge.SellInvoiceCurrencyCode, charge.LocalCurrencyCode, charge.InvoiceType))
				{
					localSellGSTAmount = GetLocalSellGSTAmountBasedOnLocalSellAmount();
				}
				else
				{
					localSellGSTAmount = TaxAmountCalculator.GetLocalTaxAmount(factory, charge.CompanyPK, charge.LocalSellAmount, chargeSellTaxRate, charge.TaxRate, charge.EffectiveExtraTaxRate, osSellGSTAmount, charge.SellExRate);
				}
			}

			return (localSellGSTAmount.IsEmpty ? ZDecimal.Zero : osSellGSTAmount, localSellGSTAmount);

			ZDecimal GetLocalSellGSTAmountBasedOnLocalSellAmount() => (ZDecimal)(localSellGSTAmountBasedOnLocalSellAmount
				?? (localSellGSTAmountBasedOnLocalSellAmount = chargeSellTaxRate != null && chargeSellTaxRate.IsMexicoNeedExtraType
				? TaxAmountCalculator.GetTaxAmountFromExTaxAmount(charge.LocalSellAmount, chargeSellTaxRate, charge.TaxRate ?? 0, charge.EffectiveExtraTaxRate ?? 0, TaxAmountCalculator.GetCurrentCompany(factory, charge.CompanyPK).LocalCurrency.Decimals)
				: TaxAmountCalculator.GetLocalTaxAmount(factory, charge.CompanyPK, charge.LocalSellAmount, chargeSellTaxRate, charge.TaxRate, charge.EffectiveExtraTaxRate)));
		}

		ZDecimal ICalculateOSSellTaxForCharge.CalculateOSSellGSTAmount(LineDetails line, ChargeSellDetails charge, ref object cache)
		{
			if (cache == null)
			{
				cache = new BusinessObjectFactory();
			}

			return CalculateSellGSTAmounts((BusinessObjectFactory)cache, line, charge).OSSellGSTAmount;
		}

		ZDecimal ICalculateOSCostTaxForCharge.CalculateOSCostGSTAmountWhenNotOverridden(ZDecimal costAmount, ZString costCurrency, ZGuid taxRatePK, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZGuid chargeCodePK, ZGuid jr_E6, string countryCode, ZGuid companyPK, ref Object cache)
		{
			if (cache == null)
			{
				cache = new BusinessObjectFactory();
			}

			return BaseCharge.JobChargeOSGSTCalculationStrategyStaticHelper.CalculateOsCostTaxAmount((BusinessObjectFactory)cache, costAmount, costCurrency, taxRatePK, rate, effectiveExtraRate, chargeCodePK, jr_E6, countryCode, companyPK);
		}
	}
}
