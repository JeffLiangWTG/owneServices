namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceHeaderActiveCollectionForTest : InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollectionForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new bool AllowNew
		{
			get { return base.AllowNew; }
		}
	}
}
