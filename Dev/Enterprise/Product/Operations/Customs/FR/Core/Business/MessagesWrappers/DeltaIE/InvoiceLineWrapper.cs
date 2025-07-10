using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class InvoiceLineWrapper : IInvoiceLine
	{
		InvoiceLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		public static InvoiceLineWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new InvoiceLineWrapper(entryLine);

		public double ItemAmountInvoiced => itemAmountInvoiced.Equals(0d) ? GetItemAmountInvoiced() : itemAmountInvoiced;

		double GetItemAmountInvoiced() => itemAmountInvoiced = entryLine.Header.IsMultiInvoiceCurrency ? (double)entryLine.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency : (double)entryLine.CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency;

		double itemAmountInvoiced;

		readonly CusEntryLine entryLine;
	}
}
