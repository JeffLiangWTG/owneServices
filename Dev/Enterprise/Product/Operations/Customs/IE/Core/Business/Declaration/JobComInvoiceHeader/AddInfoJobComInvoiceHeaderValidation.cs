namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		//this is to ensure that the add info properties are not accessed - because they may not have values - Look at ZG_CountryOfDestination on InvoiceLine as an example as to why
		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent.Parent;
	}
}
