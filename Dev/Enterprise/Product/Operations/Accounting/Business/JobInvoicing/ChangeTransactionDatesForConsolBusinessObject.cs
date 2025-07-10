using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChangeTransactionDatesForConsolBusinessObject : ChangeTransactionDatesBusinessObject
	{
		public ChangeTransactionDatesForConsolBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
			: base(pluginSecurity, factory)
		{
		}

		public ChangeTransactionDatesForConsolBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
			: base(pluginSecurity, factory, codes)
		{
		}

		protected override bool ModifyTransactionDateSecurityIsAllowed
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyTransactionDate).IsAllowed; }
		}

		protected override bool ModifyPostDateSecurityIsAllowed
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ModifyPostDate).IsAllowed; }
		}
	}
}