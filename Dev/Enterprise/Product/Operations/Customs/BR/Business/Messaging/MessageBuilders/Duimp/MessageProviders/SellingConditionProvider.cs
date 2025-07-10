using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class SellingConditionProvider : ISellingCondition
	{
		SellingConditionProvider(JobComInvoiceHeader invoiceHeader, CusEntryLine entryLine)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
		}
		readonly JobComInvoiceHeader invoiceHeader;
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine[] invoiceLines;

		public static SellingConditionProvider New(JobComInvoiceHeader invoiceHeader, CusEntryLine entryLine) =>
			invoiceHeader == null || entryLine == null || entryLine.Declaration == null ? null : new SellingConditionProvider(invoiceHeader, entryLine);

		public int ValuationMethodCode => ZInt.ParseSafe(invoiceHeader.JZ_ValuationCode, ZInt.Zero);

		public string IncoTermCode => invoiceHeader.JZ_IncoTerm;

		public string IncoTermComplement => invoiceHeader.JZ_AdditionalTerms;

		public double? FreightInLocalCurrency => entryLine.Declaration.JE_DispatchModality.IsEmpty ? null : (double)entryLine.OverseasFreightInLocalCurrency.Amount;

		public double? InsuranceInLocalCurrency => entryLine.Declaration.JE_DispatchModality.IsEmpty ? null : (double)entryLine.OverseasInsuranceInLocalCurrency.Amount;

		public IEnumerable<IAdditionDeduction> AdditionsDeductions => fAdditionsDeductions ??= invoiceLines.SelectMany(
			x => x.GetAllCharges(x => ImportChargesProvider.IsAdditionsOrDeductions(x.J7_ChargeType))).GroupBy(x => (x.J7_ChargeType, x.J7_RX_NKCurrency))
						.Select(x => (x.Key.J7_ChargeType, x.Key.J7_RX_NKCurrency, Amount: x.Sum(c => c.J7_Amount)))
						.Select(x => AdditionDeductionProvider.New(x.J7_ChargeType, x.J7_RX_NKCurrency, x.Amount)).ToArray();
		IAdditionDeduction[] fAdditionsDeductions;
	}
}
