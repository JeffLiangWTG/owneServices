using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.BR.Business
{
	public class OtherExpensesICMSFCPFeeCalculator
	{
		public OtherExpensesICMSFCPFeeCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		const string FeeType = Constants.RateTypes.OtherExpensesICMS;

		public void UpdateOtherExpensesICMSFeeOnEntryLine()
		{
			var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

			var totalAmountOnEntryLine = invoiceLines.GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, currency: entryLine.Header.LocalCurrency);

			if (totalAmountOnEntryLine > 0)
			{
				entryLine.Fees.AddOrUpdate(FeeType, totalAmountOnEntryLine);
			}
		}
	}
}
