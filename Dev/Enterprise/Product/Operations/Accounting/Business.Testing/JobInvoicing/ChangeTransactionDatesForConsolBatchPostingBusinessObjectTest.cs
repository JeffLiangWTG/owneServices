using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesForConsolBatchPostingBusinessObject))]
	public class ChangeTransactionDatesForConsolBatchPostingBusinessObjectTest : ChangeTransactionDatesBusinessObjectTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(null);
			return new ChangeTransactionDatesForConsolBatchPostingBusinessObject(JobInvoicingSecurity, Factory, codes);
		}

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.ConsolBulkModifyTransactionDate; }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.ConsolBulkModifyPostDate; }
		}

		#endregion
	}
}
