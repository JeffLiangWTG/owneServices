using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NZCustomsAPInvoiceJobWrapperTest : TestCaseWithFactory
	{
		public void TestValidChargeLines()
		{
			var charge1 = Factory.New<JobCharge>();
			var line1 = Factory.New<APInvoiceLine>();
			charge1.JR_AL_APLine = line1.PK;
			charge1.JR_JH = job.PK;

			var charge2 = Factory.New<JobCharge>();
			var line2 = Factory.New<Accrual>();
			charge2.JR_AL_APLine = line2.PK;
			charge2.JR_JH = job.PK;

			var charge3 = Factory.New<JobCharge>();
			var line3 = Factory.New<Accrual>();
			charge3.JR_AL_APLine = line3.PK;
			charge3.JR_JH = job.PK;

			AssertEquals("Valid charge lines", 2, wrapper.ValidChargeLines.Length);
		}

		public void TestJobWrapperProperties()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();

			job.JH_GB = branch1.PK;
			job.JH_GE = department1.PK;
			job.JH_JobNum = "S00000001";

			wrapper = new NZCustomsAPInvoiceJobWrapper(job);
			AssertEquals("Job Branch", branch1.GB_Code, wrapper.BranchCode);
			AssertEquals("Job Department", department1.GE_Code, wrapper.DepartmentCode);
			AssertEquals("Job Number", "S00000001", wrapper.JobNumber);
		}

		NZCustomsAPInvoiceJobWrapper wrapper;
		JobHeader job;
		protected override void SetUp()
		{
			job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			wrapper = new NZCustomsAPInvoiceJobWrapper(job);

			base.SetUp();
		}
	}
}
