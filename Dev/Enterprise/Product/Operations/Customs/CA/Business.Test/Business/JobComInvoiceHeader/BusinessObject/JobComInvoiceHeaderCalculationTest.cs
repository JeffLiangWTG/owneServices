namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		#region Implementation

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		#endregion
	}
}
