using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChangeTransactionDatesForConsolBatchPostingBusinessObject : ChangeTransactionDatesBusinessObject
	{
		public ChangeTransactionDatesForConsolBatchPostingBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
			: base(pluginSecurity, factory)
		{
		}

		public ChangeTransactionDatesForConsolBatchPostingBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
			: base(pluginSecurity, factory, codes)
		{
		}

		protected override bool ModifyTransactionDateSecurityIsAllowed
		{
			get { return Env.Security.ConsolBulkModifyTransactionDate.IsAllowed; }
		}

		protected override bool ModifyPostDateSecurityIsAllowed
		{
			get { return Env.Security.ConsolBulkModifyPostDate.IsAllowed; }
		}
	}
}
