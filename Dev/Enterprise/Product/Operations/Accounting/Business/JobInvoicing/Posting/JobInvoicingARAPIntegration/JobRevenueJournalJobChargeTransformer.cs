using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalJobChargeTransformer : BaseIntegrationTransformer
	{
		public JobRevenueJournalJobChargeTransformer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void TransformCore(TransactionHeaderWithLines transaction)
		{
			JobRevenueJournal journal = transaction as JobRevenueJournal;
			if (journal == null)
			{
				return;
			}

			foreach (JobRevenueJournalLine line in journal.Lines)
			{
				Job associatedJob = Factory.Load<Job>(line.AL_JH);
				if (associatedJob != null)
				{
					#region AddCurrencyAndExchangeRateToJob

					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(line.Company, ExchangeRateValidLedgerEnum.AR, line.TransactionHeader.AH_RX_NKTransactionCurrency);
					var rate = ((IExchangeRateProvider)associatedJob).GetExchangeRate(line.TransactionCurrency.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType: invoiceCurrencyType);
					if (rate == null)
					{
						associatedJob.AddCurrency(line.TransactionCurrency, line.AL_ExchangeRate, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					}
					else if (rate.Rate == 0m)
					{
						rate.SetBaseRate(line.AL_ExchangeRate);
					}

					#endregion

					#region CreateNewCharge

					Charge newCharge = associatedJob.Charges.AddNew();

					newCharge.InvoicingJob.InitializeParentFromGenericJobWithSettingDefaults();

					newCharge.JR_AC = line.AL_AC;
					newCharge.JR_Desc = line.AL_Desc;
					newCharge.JR_GB = line.AL_GB;
					newCharge.JR_GE = line.AL_GE;
					if (line.CostRevenueType == TransactionLineTypes.Revenue)
					{
						// DR REV should be negative sell
						// CR REV should be positive sell
						newCharge.JR_OH_SellAccount = line.AL_OH;
						newCharge.JR_RX_NKSellCurrency = line.AL_RX_NKTransactionCurrency;
						newCharge.JR_AL_ARLine = line.PK;
						newCharge.JR_AT_SellGSTRate = line.AL_AT;
						newCharge.SetSellTaxDateSafe(line.AL_TaxDate);
						newCharge.JR_AW_SellWHTRate = line.AL_AW;

						newCharge.JR_OSSellAmt = line.AL_OSExTaxAmount;
						newCharge.JR_LineCFX = 0; //For Job Revenue Journal it is always zero.
						newCharge.JR_OSSellExRate = line.AL_ExchangeRate;

						using (newCharge.Calculations.SuspendCalculations())
						{
							newCharge.JR_LocalSellAmt = line.Company.GetExchangeRate().ForeignToLocal(newCharge.JR_OSSellAmt, line.AL_ExchangeRate);
						}

						newCharge.ClearCostAmount();
						newCharge.SetEstimatedCost(0);
						newCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code; //to force clearing of any auto-created job ex rate which possibly equals 0;
					}
					else
					{
						newCharge.JR_OH_CostAccount = line.AL_OH;
						newCharge.JR_RX_NKCostCurrency = line.AL_RX_NKTransactionCurrency;
						newCharge.JR_AT_CostGSTRate = line.AL_AT;
						newCharge.SetCostTaxDateSafe(line.AL_TaxDate);
						newCharge.JR_AW_CostWHTRate = line.AL_AW;

						// DR CST should be positive cost - currently DR values are shown as negative
						// CR CST should be negative cost - currently CR values are shown as positive
						//So we need to negate the values.
						newCharge.JR_OSCostAmt = -line.AL_OSExTaxAmount;
						newCharge.JR_OSCostExRate = line.AL_ExchangeRate;
						using (newCharge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
						{
							newCharge.JR_IsCostTaxAmountOverridden = true;
							newCharge.JR_OSCostGSTAmt_Calc = -line.AL_OSTaxAmount;
						}

						using (newCharge.Calculations.SuspendCalculations())
						{
							newCharge.JR_LocalCostAmt = line.Company.GetExchangeRate().ForeignToLocal(newCharge.JR_OSCostAmt, line.AL_ExchangeRate);
						}

						newCharge.JR_AL_APLine = line.PK;
						newCharge.ClearRevenueAmount();
						newCharge.JR_EstimatedRevenue = ZDecimal.Zero;
						newCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code; //to force clearing of any auto-created job ex rate which possibly equals 0;
					}

					#endregion
				}
			}
		}
	}
}
