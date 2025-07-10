namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSProvider : EU.EMCS.Business.EMCSProvider
	{
		protected EMCSProvider(string countryCode)
			: base(countryCode)
		{
		}

		protected override EU.EMCS.Business.EMCSAddInfoJobComInvoiceLineValidation GetNewAddInfoValidationCore(EU.EMCS.Business.EMCSAddInfoJobComInvoiceLine invLine) => new EMCSAddInfoJobComInvoiceLineValidation(invLine);

		protected override EU.EMCS.Business.EMCSJobComInvoiceHeaderValidation GetNewInvoiceHeaderValidationCore(EU.EMCS.Business.EMCSJobComInvoiceHeader invoiceHeader) => new EMCSJobComInvoiceHeaderValidation(invoiceHeader);
	}
}
