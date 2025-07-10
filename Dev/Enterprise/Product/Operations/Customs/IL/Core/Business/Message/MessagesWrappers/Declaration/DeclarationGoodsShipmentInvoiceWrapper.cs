using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	sealed class DeclarationGoodsShipmentInvoiceWrapper : IDeclarationGoodsShipmentInvoice
	{
		DeclarationGoodsShipmentInvoiceWrapper(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = invoiceHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;

		internal static DeclarationGoodsShipmentInvoiceWrapper NewOrNull(JobComInvoiceHeader invoiceHeader)
			=> invoiceHeader == null
			? null
			: new DeclarationGoodsShipmentInvoiceWrapper(invoiceHeader);

		#region IDeclarationGoodsShipmentInvoice

		IDeclarationGoodsShipmentInvoiceDmExtensions IDeclarationGoodsShipmentInvoice.DmExtensions => DeclarationGoodsShipmentInvoiceDmExtWrapper.NewOrNull(invoiceHeader);

		IIDType IDeclarationGoodsShipmentInvoice.ID => IDTypeWrapper.NewOrNull(invoiceHeader.JZ_InvoiceNumber);

		string IDeclarationGoodsShipmentInvoice.IssueDateTime => invoiceHeader.JZ_InvoiceDate.IsEmpty ? null : invoiceHeader.JZ_InvoiceDate.ToCustomsDateTimeString();

		ICodeType IDeclarationGoodsShipmentInvoice.TypeCode => CodeTypeWrapper.NewOrNull(invoiceHeader.JZ_InvoiceType);

		#endregion
	}
}
