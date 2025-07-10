namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}
	}
}
