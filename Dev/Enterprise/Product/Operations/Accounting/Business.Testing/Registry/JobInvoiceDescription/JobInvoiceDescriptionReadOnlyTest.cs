using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobInvoiceDescriptionReadOnlyTest : JobConfigurationSelectorReadOnlyTest
	{
		public void TestInvoiceDescription()
		{
			var bizO = BizObj as JobInvoiceDescription;
			AssertNotNull(bizO);

			bizO.JobType = ZString.Empty;
			AssertEquals("Read only without Job Type", true, bizO.InvoiceDescriptionInfo.ReadOnly);

			bizO.JobType = "@#$";
			AssertEquals("Read only with invalid Job Type", true, bizO.InvoiceDescriptionInfo.ReadOnly);

			bizO.JobType = "ALL";
			AssertEquals("Editable with Correct Job Type", false, bizO.InvoiceDescriptionInfo.ReadOnly);
		}

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new JobInvoiceDescription();
			}
		}
	}
}