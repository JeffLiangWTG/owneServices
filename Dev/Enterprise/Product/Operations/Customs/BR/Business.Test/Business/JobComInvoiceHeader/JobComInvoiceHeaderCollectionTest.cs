namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCollectionTest
	{
		public void TestGetDefaultSetterForInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var newJobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();

			var invoiceHeaderActiveCollectionExtended = new InvoiceHeaderActiveCollectionExtendedTest(declaration);
			var defaultSetterForInvoiceHeader = invoiceHeaderActiveCollectionExtended.GetDefaultSetterForInvoiceHeaderExposed(newJobComInvoiceHeader, declaration);

			AssertNotNull(defaultSetterForInvoiceHeader);
			AssertType<DefaultSetterForInvoiceHeader>("Type", defaultSetterForInvoiceHeader);
		}

		class InvoiceHeaderActiveCollectionExtendedTest : InvoiceHeaderActiveCollection
		{
			public InvoiceHeaderActiveCollectionExtendedTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeaderExposed(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
			{
				return base.GetDefaultSetterForInvoiceHeader(newElement, declaration);
			}
		}
	}
}
