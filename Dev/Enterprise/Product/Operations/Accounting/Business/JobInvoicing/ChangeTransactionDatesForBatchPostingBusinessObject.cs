using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChangeTransactionDatesForBatchPostingBusinessObject : ChangeTransactionDatesBusinessObject
	{
		public ChangeTransactionDatesForBatchPostingBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
			: base(pluginSecurity, factory)
		{
		}

		public ChangeTransactionDatesForBatchPostingBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
			: base(pluginSecurity, factory, codes)
		{
		}

		protected override bool ModifyTransactionDateSecurityIsAllowed
		{
			get { return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.BulkModifyTransactionDate); }
		}

		protected override bool ModifyPostDateSecurityIsAllowed
		{
			get { return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.BulkModifyPostDate); }
		}
	}
}
