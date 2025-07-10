namespace Enterprise.Customs.CH.Business.Testing;

class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
