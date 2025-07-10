using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing.Journal
{
	public class AJRJCostPopulatorFromCharge : IPopulateAJRJCharges
	{
		public AJRJCostPopulatorFromCharge(ChargeWithCost fromCharge, ChargeWithCost toCharge)
		{
			FromCharge = fromCharge;
			ToCharge = toCharge;
		}

		void IPopulateAJRJCharges.PopulateJobExchangeRates(Job job)
		{
			job.ExchangeRates.AddRate(FromCharge.SellCurrency, FromCharge.JR_OSSellExRate, FromCharge.Branch.GB_OH_OrgProxy, ExchangeRateOrgTypeEnum.Creditor, InvoiceCurrencyType.NotApplicable, true);
		}

		void IPopulateAJRJCharges.PopulateDescription()
		{
			ToCharge.JR_Desc = FromCharge.ShouldDefaultLocalChargeDescription ? FromCharge.ChargeCode.AC_LocalLanguageDescription : FromCharge.ChargeCode.AC_DescMultilingual;
		}

		void IPopulateAJRJCharges.PopulateAccount()
		{
			ToCharge.JR_OH_CostAccount = FromCharge.Branch.GB_OH_OrgProxy;
		}

		void IPopulateAJRJCharges.PopulateCostCurrency()
		{
			ToCharge.JR_RX_NKCostCurrency = FromCharge.JR_RX_NKSellCurrency;
		}

		void IPopulateAJRJCharges.PopulateSellCurrency()
		{
			ToCharge.JR_RX_NKSellCurrency = FromCharge.JR_RX_NKSellCurrency;
		}

		void IPopulateAJRJCharges.PopulateCostCurrencyDependingOnDefaults()
		{
			ToCharge.JR_RX_NKCostCurrency = FromCharge.JR_RX_NKSellCurrency;
		}

		void IPopulateAJRJCharges.PopulateSellCurrencyDependingOnDefaults()
		{
			ToCharge.JR_RX_NKSellCurrency = !ToCharge.DefaultSellCurrencyFromSellAccount.IsEmpty ? ToCharge.DefaultSellCurrencyFromSellAccount : FromCharge.JR_RX_NKSellCurrency;
		}

		void IPopulateAJRJCharges.PopulateCostAmount()
		{
			ToCharge.JR_OSCostAmt = FromCharge.JR_OSSellAmt;
			ToCharge.JR_LocalCostAmt = FromCharge.JR_LocalSellAmt;
		}

		void IPopulateAJRJCharges.PopulateSellAmount()
		{
			if ((AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value &&
						!(ToCharge.JR_OH_SellAccount.IsValid && ToCharge.SellAccount.OH_IsDebtor)) || FromCharge.IsManualJobAccrualCharge)
			{
				ToCharge.JR_OSSellAmt = ZDecimal.Zero;
				ToCharge.JR_LocalSellAmt = ZDecimal.Zero;
			}
			else if (FromCharge.IsMarginCharge)
			{
				ToCharge.JR_OSSellAmt = ToCharge.GetRevenueAmountBasedOnCost();
				ToCharge.JR_LocalSellAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(ToCharge.JR_OSSellAmt, ToCharge.JR_OSSellExRate);
			}
			else
			{
				ToCharge.JR_OSSellAmt = FromCharge.JR_OSSellAmt;
				ToCharge.JR_LocalSellAmt = FromCharge.JR_LocalSellAmt;
			}
		}

		void IPopulateAJRJCharges.PopulateCostExchangeRate()
		{
			if (ToCharge.CostExchangeRate != null)
			{
				ToCharge.CostExchangeRate.SetBaseRate(FromCharge.JR_OSSellExRate);
			}
			else
			{
				ToCharge.JR_OSCostExRate = FromCharge.JR_OSSellExRate;
			}
		}

		void IPopulateAJRJCharges.PopulateSellExchangeRate()
		{
			if (ToCharge.RevenueExchangeRate != null)
			{
				if (ToCharge.RevenueExchangeRate.Rate == 0m)
				{
					ToCharge.RevenueExchangeRate.SetBaseRate(FromCharge.JR_OSSellExRate);
				}
			}
			else
			{
				if (ToCharge.JR_OSSellExRate == 0m)
				{
					ToCharge.JR_OSSellExRate = FromCharge.JR_OSSellExRate;
				}
			}
		}

		void IPopulateAJRJCharges.PopulateRelatedJobNumber(Job job)
		{
			var relatedJobNumber = FromCharge.JR_Calc_RelatedJobNumber;
			if (!relatedJobNumber.IsEmpty && job.IsGatewayBillingJob())
			{
				var relatedShipment = FromCharge.RelatedJob;
				var linkedConsols = relatedShipment.GetAllLinkedConsols().ToHashSet();
				if (linkedConsols.Contains(job.PlugInData))
				{
					ToCharge.JR_Calc_RelatedJobNumber = relatedJobNumber;
				}
			}
		}

		void IPopulateAJRJCharges.PopulateSellRatingOverride()
		{
			ToCharge.JR_SellRatingOverride = FromCharge.JR_SellRatingOverride;
		}

		void IPopulateAJRJCharges.PopulateCostRatingOverride()
		{
			ToCharge.JR_CostRatingOverride = FromCharge.JR_SellRatingOverride;
		}

		readonly ChargeWithCost FromCharge;
		readonly ChargeWithCost ToCharge;
	}
}
