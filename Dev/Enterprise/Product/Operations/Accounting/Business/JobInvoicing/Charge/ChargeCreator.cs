using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IChargeCreator
	{
		Charge CreateChargeFromJobRelatedRevenueLine(InvoicingLineBase line, InvoicingBase header, bool isTaxRecoveryLine);
	}

	class ChargeCreator : IChargeCreator
	{
		Charge IChargeCreator.CreateChargeFromJobRelatedRevenueLine(InvoicingLineBase line, InvoicingBase header, bool isTaxRecoveryLine)
		{
			Charge charge = null;
			if (line.AL_JH.IsValid)
			{
				charge = line.Factory.New<Charge>();
				using (charge.SetTempContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
				using (charge.TaxAmountAdjustmentSuspender.GetSuspender())
				using (charge.GetValidationSuspender())
				{
					charge.JR_JH = line.AL_JH;
					charge.JR_AC = line.AL_AC;
					charge.JR_Desc = line.AL_Desc;
					charge.JR_GB = line.AL_GB;
					charge.JR_GE = line.AL_GE;
					charge.JR_OH_SellAccount = header.AH_OH;
					charge.JR_RX_NKSellCurrency = line.AL_RX_NKTransactionCurrency;
					charge.JR_OSSellAmt = line.AL_OSExTaxAmount_DBSigned; // must come before setting SellExRate
																				   //charge.JR_OSSellExRate = line.AL_ExchangeRate;
																				   //charge.JR_LocalSellAmt = line.AL_LineAmount;
					charge.JR_GB_SellTaxBranch = line.AL_GB_TaxBranch;
					charge.JR_AT_SellGSTRate = line.AL_AT;
					charge.SetSellTaxDateSafe(line.AL_TaxDate);
					charge.JR_A9_SellVATClass = line.AL_A9_VATClass;
					charge.JR_AW_SellWHTRate = line.AL_AW;
					charge.JR_AL_ARLine = line.PK;
					charge.JR_LineCFX = 0m;
					charge.JR_OSSellExRate = line.AL_ExchangeRate;
					charge.JR_LocalSellAmt = line.AL_LineAmount;
					charge.JR_OA_SellInvoiceAddress = header.AH_OA_InvoiceAddressOverride;
					charge.JR_OC_SellInvoiceContact = header.AH_OC_InvoiceContactOverride;
					charge.JR_SellGovtChargeCode = line.AL_GovtChargeCode;
					charge.JR_SellSupplyType = line.AL_SupplyType;

					if (line.AL_OSAmount < ZDecimal.Zero && AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.Value)
					{
						charge.JR_OSCostAmt = ZDecimal.Zero;
						charge.JR_LocalCostAmt = ZDecimal.Zero;
					}

					if (charge.JR_OSCostExRate <= 0m
						&& charge.JR_RX_NKCostCurrency == line.AL_RX_NKTransactionCurrency)
					{
						charge.CostExchangeRate.SetBaseRate(line.AL_ExchangeRate);
					}

					if (isTaxRecoveryLine)
					{
						charge.JR_InvoiceType = header.TransactionCategory;
					}
				}
			}

			return charge;
		}
	}
}
