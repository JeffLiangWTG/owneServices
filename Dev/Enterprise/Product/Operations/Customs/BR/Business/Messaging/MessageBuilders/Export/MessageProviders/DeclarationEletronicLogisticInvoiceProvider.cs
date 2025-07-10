using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationEletronicLogisticInvoiceProvider : IDeclarationNFeInvoice
	{
		public DeclarationEletronicLogisticInvoiceProvider(ElectronicLogisticInvoice eletronicLogisticInvoice)
		{
			this.eletronicLogisticInvoice = Argument.NotNull(eletronicLogisticInvoice, nameof(eletronicLogisticInvoice));
		}

		readonly ElectronicLogisticInvoice eletronicLogisticInvoice;

		public int NFEItemSequence => 0;
		public string NFEKey => eletronicLogisticInvoice.CSI_ReferenceNumber;
		public short NFEItemNumber => (short)eletronicLogisticInvoice.CSI_LineNo;
		public decimal CustomsQuantityRelated => eletronicLogisticInvoice.CSI_Quantity;
		public string Type => eletronicLogisticInvoice.CSI_Type;
	}
}
