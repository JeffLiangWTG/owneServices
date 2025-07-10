namespace Enterprise.Customs.IE.Business.Declaration
{
	static class JobComInvoiceHeaderHelper
	{
		public static bool IsSellerOrBuyerDifferentFromDeclaration(this JobComInvoiceHeader invoiceHeader)
		{
			return invoiceHeader.JobDeclaration is JobDeclaration declaration && (invoiceHeader.SellerOrgPK != declaration.SellerOrgPK || invoiceHeader.BuyerOrgPK != declaration.JE_OH_Buyer);
		}
	}
}
