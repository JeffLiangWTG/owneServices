namespace Enterprise.Customs.MX.Business.Testing
{
	class JobComInvoiceHeaderTestForDocumentWrapperTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
