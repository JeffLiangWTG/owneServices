namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderTestForDocumentWrapperTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
