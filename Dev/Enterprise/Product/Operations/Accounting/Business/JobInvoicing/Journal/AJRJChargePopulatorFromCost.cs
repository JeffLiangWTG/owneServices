using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing.Journal
{
	public class AJRJChargePopulatorFromCost : IPopulateAJRJCharges
	{
		public AJRJChargePopulatorFromCost(ChargeWithCost fromCharge, ChargeWithCost toCharge)
		{
			FromCharge = fromCharge;
			ToCharge = toCharge;
		}

		void IPopulateAJRJCharges.PopulateJobExchangeRates(Job job)
		{
			var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(FromCharge.Company, ExchangeRateValidLedgerEnum.AR, FromCharge);
			var rate = job.ExchangeRates.AddRate(FromCharge.CostCurrency, FromCharge.JR_OSCostExRate, FromCharge.Branch.GB_OH_OrgProxy, ExchangeRateOrgTypeEnum.Debtor, invoiceCurrencyType, true);
			if (rate != null)
			{
				rate.JF_IsTransformed = true;
				rate.JF_CFXPercent = 0m;
				rate.JF_CFXMinimum = 0m;
			}
		}

		void IPopulateAJRJCharges.PopulateDescription()
		{
			ToCharge.JR_Desc = FromCharge.JR_Desc;
		}

		void IPopulateAJRJCharges.PopulateAccount()
		{
			ToCharge.JR_OH_SellAccount = FromCharge.Branch.GB_OH_OrgProxy;
		}

		void IPopulateAJRJCharges.PopulateCostCurrency()
		{
			ToCharge.JR_RX_NKCostCurrency = FromCharge.JR_RX_NKCostCurrency;
		}

		void IPopulateAJRJCharges.PopulateSellCurrency()
		{
			ToCharge.JR_RX_NKSellCurrency = FromCharge.JR_RX_NKCostCurrency;
		}

		void IPopulateAJRJCharges.PopulateCostCurrencyDependingOnDefaults()
		{
			ToCharge.JR_RX_NKCostCurrency = !ToCharge.DefaultCostCurrencyFromCostAccount.IsEmpty ? ToCharge.DefaultCostCurrencyFromCostAccount : FromCharge.JR_RX_NKCostCurrency;
		}

		void IPopulateAJRJCharges.PopulateSellCurrencyDependingOnDefaults()
		{
			ToCharge.JR_RX_NKSellCurrency = FromCharge.JR_RX_NKCostCurrency;
		}

		void IPopulateAJRJCharges.PopulateCostAmount()
		{
			if ((AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value &&
						!(ToCharge.JR_OH_CostAccount.IsValid && ToCharge.CostAccount.OH_IsCreditor)) || FromCharge.IsManualJobAccrualCharge)
			{
				ToCharge.JR_OSCostAmt = ZDecimal.Zero;
				ToCharge.JR_LocalCostAmt = ZDecimal.Zero;
				ToCharge.JR_OH_CostAccount = ZGuid.Empty;
			}
			else if (FromCharge.IsMarginCharge)
			{
				ToCharge.JR_OSCostAmt = ToCharge.GetCostAmountBasedOnSell();
				ToCharge.JR_LocalCostAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(ToCharge.JR_OSCostAmt, ToCharge.JR_OSCostExRate);
			}
			else
			{
				ToCharge.JR_OSCostAmt = FromCharge.JR_OSCostAmt;
				ToCharge.JR_LocalCostAmt = FromCharge.JR_LocalCostAmt;
			}
		}

		void IPopulateAJRJCharges.PopulateSellAmount()
		{
			ToCharge.JR_OSSellAmt = FromCharge.JR_OSCostAmt;
			ToCharge.JR_LocalSellAmt = FromCharge.JR_LocalCostAmt;
		}

		void IPopulateAJRJCharges.PopulateCostExchangeRate()
		{
			if (ToCharge.CostExchangeRate != null)
			{
				if (ToCharge.CostExchangeRate.Rate == 0m)
				{
					ToCharge.CostExchangeRate.SetBaseRate(FromCharge.JR_OSCostExRate);
				}
			}
			else
			{
				if (ToCharge.JR_OSCostExRate == 0m)
				{
					ToCharge.JR_OSCostExRate = FromCharge.JR_OSCostExRate;
				}
			}
		}

		void IPopulateAJRJCharges.PopulateSellExchangeRate()
		{
			if (ToCharge.RevenueExchangeRate != null)
			{
				ToCharge.RevenueExchangeRate.SetBaseRate(FromCharge.JR_OSCostExRate);
			}
			else
			{
				ToCharge.JR_OSSellExRate = FromCharge.JR_OSCostExRate;
				ToCharge.JR_LineCFX = 0m;
			}
		}

		void IPopulateAJRJCharges.PopulateRelatedJobNumber(Job job)
		{
		}

		void IPopulateAJRJCharges.PopulateSellRatingOverride()
		{
			ToCharge.JR_SellRatingOverride = FromCharge.JR_CostRatingOverride;
		}

		void IPopulateAJRJCharges.PopulateCostRatingOverride()
		{
			ToCharge.JR_CostRatingOverride = FromCharge.JR_CostRatingOverride;
		}

		readonly ChargeWithCost FromCharge;
		readonly ChargeWithCost ToCharge;
	}
}
