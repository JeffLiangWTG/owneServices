using System;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationDrawbackInvoiceProvider : IDeclarationDrawbackInvoice
	{
		public DeclarationDrawbackInvoiceProvider(SuspensionDrawbackInvoice drawbackInvoice)
		{
			this.drawbackInvoice = Argument.NotNull(drawbackInvoice, nameof(drawbackInvoice));
		}
		readonly SuspensionDrawbackInvoice drawbackInvoice;

		public string ID => drawbackInvoice.CSI_ReferenceNumber;
		public DateTime? IssueDate => drawbackInvoice.CSI_DateOfIssue.ToNullableDateTime();
		public decimal TradingCurrencyValue => drawbackInvoice.CSI_Value;
		public decimal Quantity => drawbackInvoice.CSI_Quantity;
	}
}

