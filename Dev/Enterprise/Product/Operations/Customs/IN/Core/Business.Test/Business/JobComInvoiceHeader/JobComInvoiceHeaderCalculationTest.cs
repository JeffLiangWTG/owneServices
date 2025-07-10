namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
