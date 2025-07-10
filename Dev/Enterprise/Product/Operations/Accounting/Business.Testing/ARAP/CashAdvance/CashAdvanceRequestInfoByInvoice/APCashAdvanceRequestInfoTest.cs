using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public class APCashAdvanceRequestInfoTest : CashAdvanceRequestInfoByInvoiceTest
	{
		protected override string LedgerType => LedgerTypes.AccountsPayable;

		protected override ICashAdvanceRequirement GetCashAdvanceRequirement(Charge charge) => charge.APCashAdvanceRequirement;

		protected override Invoice GetInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice)) as APInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateAPInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.CostCurrency, 1.0M, "DESC01", charge.JR_OSCostAmt);
				charge.Accrual.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_APLine = line.PK;
			}
			return invoice;
		}

		protected override void LinkChargeWithRequestLine(Charge charge, ZGuid linePK)
		{
			charge.JR_CAL_APLine = linePK;
		}

		protected override void SetCashAdvanceRequirement(Charge charge)
		{
			charge.JR_IsAPCashAdvance = true;
		}
	}
}
