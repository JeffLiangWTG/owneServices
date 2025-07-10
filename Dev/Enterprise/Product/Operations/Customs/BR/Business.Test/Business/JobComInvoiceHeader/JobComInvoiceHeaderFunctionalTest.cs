using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		#region Implementation

		protected override BaseJobDeclaration GetNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			return declaration;
		}

		#endregion
	}
}
