using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesForConsolBusinessObject))]
	public class ChangeTransactionDatesForConsolBusinessObjectTest : ChangeTransactionDatesBusinessObjectTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(null);
			return new ChangeTransactionDatesForConsolBusinessObject(JobInvoicingSecurity, Factory, codes);
		}

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyTransactionDate); }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyPostDate); }
		}

		#endregion
	}
}
