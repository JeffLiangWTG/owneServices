using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesForBatchPostingBusinessObject))]
	public class ChangeTransactionDatesForBatchPostingBusinessObjectTest : ChangeTransactionDatesBusinessObjectTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(null);
			return new ChangeTransactionDatesForBatchPostingBusinessObject(JobInvoicingSecurity, Factory, codes);
		}

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.BulkModifyTransactionDate); }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.BulkModifyPostDate); }
		}

		#endregion
	}
}
