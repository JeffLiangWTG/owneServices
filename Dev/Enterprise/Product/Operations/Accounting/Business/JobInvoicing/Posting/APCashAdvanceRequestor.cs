using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class APCashAdvanceRequestor
	{
		public APCashAdvanceRequestor(ChargeWithCost charge)
		{
			this.CostCharge = charge;
		}

		BusinessObjectFactory Factory => CostCharge.Factory;

		public CashAdvanceRequestHeader GenerateRequest()
		{
			CostCharge.LoadRelevantChargesForAPCashAdvance();

			var cah = Factory.New<CashAdvanceRequestHeader>();
			cah.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			cah.CAH_JH_Job = CostCharge.JR_JH;
			cah.CAH_Ledger = LedgerTypes.AccountsPayable;
			cah.CAH_RX_NKTransactionCurrency = CostCharge.JR_RX_NKCostCurrency;
			cah.CAH_OH_Organization = CostCharge.JR_OH_CostAccount;
			cah.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			cah.CAH_LocalAmount = CostCharge.RelevantChargesForAPCashAdvance.OfType<Charge>().Sum(c => c.JR_Calc_LocalCostAmtWithGST);
			if (cah.CAH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				cah.CAH_OSAmount = CostCharge.RelevantChargesForAPCashAdvance.OfType<Charge>().Sum(c => c.JR_OSCostAmtWithGSTAmt);
			}
			else
			{
				cah.CAH_OSAmount = cah.CAH_LocalAmount;
			}

			foreach (Charge charge in CostCharge.RelevantChargesForAPCashAdvance)
			{
				var cal = CreateCashAdvanceRequestLine(cah, charge);
				charge.JR_CAL_APLine = cal.PK;
				charge.JR_IsAPCashAdvance = true;
			}
			return cah;
		}

		AccCashAdvanceRequestLine CreateCashAdvanceRequestLine(CashAdvanceRequestHeader header, Charge charge)
		{
			var cal = header.Lines.AddNew();
			cal.CAL_CAH_RequestHeader = header.PK;
			cal.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			cal.CAL_LocalAmount = charge.JR_Calc_LocalCostAmtWithGST;
			if (header.CAH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				cal.CAL_OSAmount = charge.JR_OSCostAmtWithGSTAmt;
			}
			else
			{
				cal.CAL_OSAmount = cal.CAL_LocalAmount;
			}
			cal.CAL_LocalPaidAmount = 0;
			cal.CAL_OSPaidAmount = 0;
			return cal;
		}

		protected internal ChargeWithCost CostCharge;
	}
}
