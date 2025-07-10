using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	sealed class DeclarationGoodsShipmentInvoiceDmExtWrapper : IDeclarationGoodsShipmentInvoiceDmExtensions
	{
		DeclarationGoodsShipmentInvoiceDmExtWrapper(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = invoiceHeader;
		}

		internal static DeclarationGoodsShipmentInvoiceDmExtWrapper NewOrNull(JobComInvoiceHeader invoiceHeader)
			=> invoiceHeader == null ? null : new DeclarationGoodsShipmentInvoiceDmExtWrapper(invoiceHeader);

		#region IDeclarationGoodsShipmentInvoiceDmExtensions

		IAmountType IDeclarationGoodsShipmentInvoiceDmExtensions.ActualPayedAmount => null;

		IAmountType IDeclarationGoodsShipmentInvoiceDmExtensions.InvoiceAmount
			=> AmountTypeWrapper.NewOrNull(
				invoiceHeader.JZ_InvoiceAmount.Round(JobComInvoiceHeader.Schema.JZ_InvoiceAmount_DecimalPlaces),
				invoiceHeader.JZ_RX_NKInvoice_Currency);

		IIsPrefarenceDocumentIndType IDeclarationGoodsShipmentInvoiceDmExtensions.IsPrefarenceDocumentInd => IsPrefarenceDocumentIndTypeWrapper.New(!invoiceHeader.JZ_PreferenceDocumentType.IsEmpty);

		ICodeType IDeclarationGoodsShipmentInvoiceDmExtensions.PaymentType => CodeTypeWrapper.NewOrNull(invoiceHeader.JZ_PaymentTerms);

		ICodeType IDeclarationGoodsShipmentInvoiceDmExtensions.PrefarenceDocumentType => CodeTypeWrapper.NewOrNull(invoiceHeader.JZ_PreferenceDocumentType);

		decimal? IDeclarationGoodsShipmentInvoiceDmExtensions.RateNumericValue => null;

		#endregion

		readonly JobComInvoiceHeader invoiceHeader;
	}
}
