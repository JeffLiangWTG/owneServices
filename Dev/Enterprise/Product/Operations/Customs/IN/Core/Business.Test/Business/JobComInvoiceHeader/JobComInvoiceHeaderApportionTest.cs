namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
{
	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
