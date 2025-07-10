namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceHeaderValidation : CAAddInfoValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoJobComInvoiceHeader Parent
		{
			get { return (AddInfoJobComInvoiceHeader)base.Parent; }
		}

		protected AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
