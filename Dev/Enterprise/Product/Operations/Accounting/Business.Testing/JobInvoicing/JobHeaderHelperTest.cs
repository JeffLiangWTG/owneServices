using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobHeaderHelperTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDeactivateAllRelatedEmptyJobHeaders()
		{
			//Jobs with related transactions
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "XX";
			job.JH_ParentID = ZGuid.NewZGuid();

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentTableCode = "XX";
			job2.JH_ParentID = ZGuid.NewZGuid();

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentTableCode = "XX";
			job3.JH_ParentID = ZGuid.NewZGuid();

			var job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job4.JH_ParentTableCode = "XX";
			job4.JH_ParentID = ZGuid.NewZGuid();

			var job5 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job5.JH_ParentTableCode = "XX";
			job5.JH_GC = ForeignCompany.PK;
			job5.JH_ParentID = job4.JH_ParentID;

			var parentJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			parentJob.JH_ParentTableCode = "XX";
			parentJob.JH_ParentID = ZGuid.NewZGuid();

			var childJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			childJob.JH_ParentTableCode = "XX";
			childJob.JH_ParentID = ZGuid.NewZGuid();
			childJob.JH_JH_ParentJob = parentJob.PK;

			//empty jobs
			var job6 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job6.JH_ParentTableCode = "XX";
			job6.JH_GC = ForeignCompany.PK;
			job6.JH_ParentID = ZGuid.NewZGuid();

			var job7 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job7.JH_ParentTableCode = "XX";
			job7.JH_ParentID = job6.JH_ParentID;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_JH = job2.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_JH = job3.PK;
			var accHotCheque = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			accHotCheque["AQ_JH"] = job4.PK;
			var accHotCheque2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			accHotCheque2["AQ_JH"] = job5.PK;
			Factory.Save();

			foreach (var jobsToDeactivate in new JobHeader[] { job, job2, job3, job4, job5, parentJob, job6 })
			{
				JobHeaderHelper.DeactivateAllRelatedEmptyJobHeaders(jobsToDeactivate.JH_ParentID, Factory);
			}
			Factory.Save();

			JobHeaderCollection reloadedJobs = new JobHeaderCollection(new BusinessObjectFactory(), new ZQuery(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			reloadedJobs.Load();
			Assert("Collection should contain Job. It wasn't deleted because it has a charge.", reloadedJobs.Contains(job));
			Assert("Collection should contain Job2. It wasn't deleted because it has a line.", reloadedJobs.Contains(job2));
			Assert("Collection should contain Job3. It wasn't deleted because it has a transaction.", reloadedJobs.Contains(job3));
			Assert("Collection should contain Job4. It wasn't deleted because it has a hot cheque.", reloadedJobs.Contains(job4));
			Assert("Collection should contain Job5. It wasn't deleted because it has a hot cheque (foreign company).", reloadedJobs.Contains(job5));
			Assert("Collection should contain parentJob. It wasn't deleted because it is linked to a child job.", reloadedJobs.Contains(parentJob));
			Assert("Collection should not contain Job6. It was deleted because it has no dependencies", !reloadedJobs.Contains(job6));
			Assert("Collection should not contain Job7. It was deleted because it has no dependencies (foreign company)", !reloadedJobs.Contains(job7));
		}

		public void TestDeactivateAllRelatedEmptyJobHeaders_HaveContextWhenRelatedJobNotGetChanged()
		{
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUEEE");
			var companyDE = TestObjectCreator.CreateCompanyAndBranch("DECCC");
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var jobForAU = CreateJobHeaderByCompany(companyAU, shipment);
			var jobForDE = CreateJobHeaderByCompany(companyDE, shipment);

			jobForDE.JH_JobNum += "A";
			CombineAssertions("PreConditions", () => {
				AssertEquals("jobForAU HasChanges", false, jobForAU.HasChanges);
				AssertEquals("jobForDE HasChanges", true, jobForDE.HasChanges);
			});
			JobHeaderHelper.DeactivateAllRelatedEmptyJobHeaders(shipment.PK, Factory);

			AssertEquals("When JobHeader is being deactivated and no other changes, JobHeader should have JobDeactivationForAllCompanies context."
				, true, jobForAU.HasContext(BusinessContext.JobDeactivationForAllCompanies));
			AssertEquals("When JobHeader is being deactivated but having other changes, JobHeader should not have JobDeactivationForAllCompanies context."
				, false, companyDE.HasContext(BusinessContext.JobDeactivationForAllCompanies));
		}

		JobHeader CreateJobHeaderByCompany(GlbCompany company, IJobHeaderParent parent)
		{
			using(Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.ActiveBranches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = new JobHeader.Loader(parent).TryCreateWithMutex();
				Factory.Save();
				return job;
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ForeignCompany = Factory.New<GlbCompany>();
			ForeignCompany.GC_Code = "BLA";
			Factory.Save();
		}

		GlbCompany ForeignCompany;

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
