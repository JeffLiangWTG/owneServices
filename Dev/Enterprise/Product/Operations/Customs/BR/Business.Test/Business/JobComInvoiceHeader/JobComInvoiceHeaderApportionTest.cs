using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
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
