using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Business
{
	sealed class InvoiceProvider : IInvoice
	{
		public InvoiceProvider(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			this.entryHeader = entryHeader;
			this.invoiceHeader = entryHeader.InvoiceHeaders().FirstOrDefault();
		}

		readonly CusEntryHeader entryHeader;
		readonly JobComInvoiceHeader invoiceHeader;

		public string Type => invoiceHeader?.JZ_InvoiceType;

		public string ElectronicReceiptNumber => invoiceHeader?.JZ_ElectronicInvoiceReceiptNumber;

		public string Number => invoiceHeader?.JZ_InvoiceNumber;

		public string PriceTypeCode => invoiceHeader?.JZ_InvoiceAmountType;

		public string Incoterm => invoiceHeader?.JZ_IncoTerm;

		public IMoney Price => TryGetMoneyProvider(entryHeader);

		MoneyProvider TryGetMoneyProvider(CusEntryHeader entryHeader) => entryHeader != null && entryHeader.IsInDatabase ? new MoneyProvider(entryHeader, nameof(CusEntryHeaderMessageProvider.Invoice)) : null;
	}
}
