using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class MoneyProvider : IMoney
	{
		public MoneyProvider(CusEntryInstruction entryInstruction, JobComInvoiceHeader invoiceHeader)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			CurrencyCode = invoiceHeader?.JZ_RX_NKInvoice_Currency;
		}

		public decimal Value => CachedValueHelper.GetValue(ref value, () => entryInstruction.InvoiceLines.Sum(l => l.JI_LinePrice));
		CachedValue<decimal> value;

		public string CurrencyCode { get; }

		readonly CusEntryInstruction entryInstruction;
	}
}
