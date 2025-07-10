namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderTestForDocumentWrappert : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
}
