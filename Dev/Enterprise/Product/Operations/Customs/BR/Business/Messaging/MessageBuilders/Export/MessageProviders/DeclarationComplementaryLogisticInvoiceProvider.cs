using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationComplementaryLogisticInvoiceProvider : IDeclarationNFeInvoice
	{
		public DeclarationComplementaryLogisticInvoiceProvider(ComplementaryLogisticInvoice complementaryLogisticInvoice)
		{
			this.complementaryLogisticInvoice = Argument.NotNull(complementaryLogisticInvoice, nameof(complementaryLogisticInvoice));
		}

		readonly ComplementaryLogisticInvoice complementaryLogisticInvoice;

		public int NFEItemSequence => 0;
		public string NFEKey => complementaryLogisticInvoice.CSI_ReferenceNumber;
		public short NFEItemNumber => (short)complementaryLogisticInvoice.CSI_LineNo;
		public decimal CustomsQuantityRelated => decimal.Zero;
		public string Type => complementaryLogisticInvoice.CSI_Type;
	}
}
