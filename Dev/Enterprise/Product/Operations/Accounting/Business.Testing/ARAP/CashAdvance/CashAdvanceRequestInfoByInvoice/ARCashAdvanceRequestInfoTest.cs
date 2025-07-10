using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public class ARCashAdvanceRequestInfoTest : CashAdvanceRequestInfoByInvoiceTest
	{
		protected override string LedgerType => LedgerTypes.AccountsReceivable;

		protected override ICashAdvanceRequirement GetCashAdvanceRequirement(Charge charge) => charge.ARCashAdvanceRequirement;

		protected override Invoice GetInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice)) as ARInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateARInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.SellCurrency, 1.0M, "DESC01", charge.JR_OSSellAmt);
				charge.WIP.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_ARLine = line.PK;
			}
			return invoice;
		}

		protected override void LinkChargeWithRequestLine(Charge charge, ZGuid linePK)
		{
			charge.JR_CAL_ARLine = linePK;
		}

		protected override void SetCashAdvanceRequirement(Charge charge)
		{
			charge.JR_IsARCashAdvance = true;
		}
	}
}
