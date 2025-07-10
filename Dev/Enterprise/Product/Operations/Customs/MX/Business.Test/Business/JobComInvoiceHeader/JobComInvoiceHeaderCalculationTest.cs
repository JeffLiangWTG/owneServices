namespace Enterprise.Customs.MX.Business.Testing
{
	class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
