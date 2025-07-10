namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
