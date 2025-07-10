namespace Enterprise.Customs.CH.Business.Testing;

class JobComInvoiceHeaderTestForDocumentWrapperTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
