namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : CAAddInfoValidationTest<AddInfoJobComInvoiceHeader>
	{
		protected override AddInfoJobComInvoiceHeader GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceHeader(Factory.New<JobDeclaration>().Invoices.AddNew().JZ_AddInfoInfo);
		}
	}
}
