using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	sealed class DeclarationGoodsShipmentTradeTermsWrapper : IDeclarationGoodsShipmentTradeTerms
	{
		DeclarationGoodsShipmentTradeTermsWrapper(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = invoiceHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;

		internal static DeclarationGoodsShipmentTradeTermsWrapper NewOrNull(JobComInvoiceHeader invoiceHeader)
			=> invoiceHeader == null ? null : new DeclarationGoodsShipmentTradeTermsWrapper(invoiceHeader);

		#region IDeclarationGoodsShipmentTradeTerms

		ICodeType IDeclarationGoodsShipmentTradeTerms.ConditionCode => CodeTypeWrapper.NewOrNull(invoiceHeader.JZ_IncoTerm);

		IIDType IDeclarationGoodsShipmentTradeTerms.LocationID => IDTypeWrapper.NewOrNull(invoiceHeader.JZ_IncoTermPlace);

		#endregion
	}
}
