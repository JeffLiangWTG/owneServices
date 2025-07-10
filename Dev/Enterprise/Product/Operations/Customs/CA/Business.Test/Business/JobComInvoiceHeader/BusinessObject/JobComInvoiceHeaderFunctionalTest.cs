namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		#endregion
	}
}
