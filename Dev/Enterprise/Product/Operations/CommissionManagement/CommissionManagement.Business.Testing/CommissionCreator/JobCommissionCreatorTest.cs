using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class JobCommissionCreatorTest : CommissionCreatorTestCase
	{
		#region Create Commissions

		[TestDate(2002, 2, 2)]
		public void TestCreateCommissions()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var invoice1999 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1999.AH_JH = job.PK;
			invoice1999.AH_PostDate = new ZDateTime(1999, 1, 1);

			var invoice2000 = Factory.NewWithValidTestData<ARCreditNote>();
			invoice2000.AH_JH = job.PK;
			invoice2000.AH_PostDate = new ZDateTime(2000, 1, 1);

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2002, 1, 1);
			var closeLog2002 = job.Logs.AddNew(Events.JobClose);
			Factory.Save();

			var invoice2003 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2003.AH_JH = job.PK;
			invoice2003.AH_PostDate = new ZDateTime(2003, 1, 1);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_JH = job.PK;
			creditNote.AH_PostDate = new ZDateTime(2004, 1, 1);

			var adjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote.AH_JH = job.PK;
			adjustmentNote.AH_PostDate = new ZDateTime(2004, 2, 1);

			var jobRevJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			jobRevJournal.AH_JH = job.PK;
			jobRevJournal.AH_PostDate = new ZDateTime(2004, 3, 1);

			var overpayment = Factory.NewWithValidTestData<AROverpayment>();
			overpayment.AH_JH = job.PK;
			overpayment.AH_PostDate = new ZDateTime(2004, 4, 1);

			var invoice2005 = Factory.NewWithValidTestData<ARCreditNote>();
			invoice2005.AH_JH = job.PK;
			invoice2005.AH_PostDate = new ZDateTime(2005, 1, 1);

			var jrj = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), TestObjectCreator.CC1, job.PK, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2005, 2, 1);

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 1, 1);
			var closeLog2006 = job.Logs.AddNew(Events.JobClose);
			Factory.Save();

			using (Factory.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				var jobCommissionCreator = new JobCommissionCreatorForTest(job);
				jobCommissionCreator.CreateCommissions(new CreateCommissionContext() { FromDate = new ZDateTime(2000, 1, 1) });

				AssertArrayEqualsByElements(
					new EnterpriseBusinessObject[]
					{
					invoice2000,
					closeLog2002,
					invoice2003,
					creditNote,
					adjustmentNote,
					invoice2005,
					jrj,
					closeLog2006
					},
					jobCommissionCreator.CreatedCommissionFor.ToArray());

				foreach (var bizObj in jobCommissionCreator.CreatedCommissionFor)
				{
					AssertEquals("BizO is loaded in the job Factory", job.Factory, bizObj.Factory);
				}
			}
		}

		#endregion
	}

	class JobCommissionCreatorForTest : JobCommissionCreator
	{
		public JobCommissionCreatorForTest(JobHeader job)
			: base(job)
		{
		}

		public List<EnterpriseBusinessObject> CreatedCommissionFor = new List<EnterpriseBusinessObject>();

		protected override void CreateJobClosedCommissions(JobHeader job, StmALog jobCloseLog, CreateCommissionContext context)
		{
			CreatedCommissionFor.Add(jobCloseLog);
		}

		protected override void CreateJobRelatedTransactionCommissions(ICommissionableTransaction jobTransaction, CreateCommissionContext context)
		{
			CreatedCommissionFor.Add(jobTransaction as EnterpriseBusinessObject);
		}
	}
}
