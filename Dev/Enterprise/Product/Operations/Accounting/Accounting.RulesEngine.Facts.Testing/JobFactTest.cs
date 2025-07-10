using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public abstract class JobFactTest : TestCase
	{
		public void TestNullJob_ThrowsException()
		{
			var environmentMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new JobFact(null, environmentMock.Object, null, null));
		}

		public void TestNullEnvironment_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();

			AssertExceptionThrown<ArgumentNullException>(() => new JobFact(job, null, null, null));
		}

		public void TestPK()
		{
			var pk = Guid.NewGuid();
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(pk);
			var environmentMock = new Mock<IEnvironmentFact>();

			var jobFact = new JobFact(job, environmentMock.Object, null, null);
			AssertEquals(pk, jobFact.PK);
		}

		public void TestSalesRep()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			Assert("Precondition", job.RepSales == null);

			var environmentMock = new Mock<IEnvironmentFact>();
			var staffFactMock = new Mock<IStaffFact>();

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			job.JH_GS_NKRepSales = salesRep.GS_Code;

			var jobFact = new JobFact(job, environmentMock.Object, null, salesRepFact: staffFactMock.Object);
			AssertNotNull(jobFact.SalesRep);
			AssertType<FactLeftJoin<IStaffFact>>(jobFact.SalesRep);
			AssertNotNull(jobFact.SalesRep.Fact);
		}

		public void TestLocalClient()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			Assert("Precondition", job.LocalCharges == null);

			var environmentMock = new Mock<IEnvironmentFact>();
			var localClientFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var localClient = Factory.New<OrgHeader>();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var jobFact = new JobFact(job, environmentMock.Object, localClientFact: localClientFactMock.Object, null);
			AssertNotNull(jobFact.LocalClient);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(jobFact.LocalClient);
			AssertNotNull(jobFact.LocalClient.Fact);
		}

		public void TestCurrentBranch()
		{
			var pk = Guid.NewGuid();
			var branchCode = "SYD";

			var branchFactMock = new Mock<IBranchFact>();
			branchFactMock.Setup(x => x.PK).Returns(pk);
			branchFactMock.Setup(x => x.Code).Returns(branchCode);

			var environmentMock = new Mock<IEnvironmentFact>();
			environmentMock.Setup(x => x.CurrentBranch).Returns(new FactJoin<IBranchFact>(branchFactMock.Object));

			var job = Factory.NewJobForTesting<JobHeader>();

			var jobFact = new JobFact(job, environmentMock.Object, null, null);
			AssertNotNull(jobFact.CurrentBranch);
			AssertType<FactJoin<IBranchFact>>(jobFact.CurrentBranch);
			AssertNotNull(jobFact.CurrentBranch.Fact);
			AssertEquals(pk, jobFact.CurrentBranch.Fact.PK);
			AssertEquals(branchCode, jobFact.CurrentBranch.Fact.Code);
		}

		public void TestCurrentDepartment()
		{
			var pk = Guid.NewGuid();
			var departmentCode = "FIA";

			var departmentFactMock = new Mock<IDepartmentFact>();
			departmentFactMock.Setup(x => x.PK).Returns(pk);
			departmentFactMock.Setup(x => x.Code).Returns(departmentCode);

			var environmentMock = new Mock<IEnvironmentFact>();
			environmentMock.Setup(x => x.CurrentDepartment).Returns(new FactJoin<IDepartmentFact>(departmentFactMock.Object));

			var job = Factory.NewJobForTesting<JobHeader>();

			var jobFact = new JobFact(job, environmentMock.Object, null, null);
			AssertNotNull(jobFact.CurrentDepartment);
			AssertType<FactJoin<IDepartmentFact>>(jobFact.CurrentDepartment);
			AssertNotNull(jobFact.CurrentDepartment.Fact);
			AssertEquals(pk, jobFact.CurrentDepartment.Fact.PK);
			AssertEquals(departmentCode, jobFact.CurrentDepartment.Fact.Code);
		}

		public void TestCurrentCompanyCountry()
		{
			var currentCompanyCountry = "AU";
			var environmentMock = new Mock<IEnvironmentFact>();
			environmentMock.Setup(x => x.CurrentCompanyCountry).Returns(currentCompanyCountry);

			var job = Factory.NewJobForTesting<JobHeader>();

			var jobFact = new JobFact(job, environmentMock.Object, null, null);
			AssertEquals(currentCompanyCountry, jobFact.CurrentCompanyCountry);
		}

		public virtual void TestJobBranchDepartment()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var branchFact = new BranchFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var departmentFact = new DepartmentFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var jobBranchDepartmentFact = new JobBranchDepartmentFact(branchFact, departmentFact);

			var jobForTaxBranchFact = GetJobBranchDepartmentFact(plugInMock.Object, environmentFactMock.Object, jobBranchDepartmentFact);

			if (jobForTaxBranchFact == null)
			{
				Assert(true);
			}
			else
			{
				AssertNotNull(jobForTaxBranchFact.JobBranch);
				AssertNotNull(jobForTaxBranchFact.JobDepartment);
				AssertType<FactLeftJoin<IBranchFact>>(jobForTaxBranchFact.JobBranch);
				AssertType<FactLeftJoin<IDepartmentFact>>(jobForTaxBranchFact.JobDepartment);
				AssertNotNull(jobForTaxBranchFact.JobBranch.Fact);
				AssertNotNull(jobForTaxBranchFact.JobDepartment.Fact);
			}
		}

		protected virtual IJobBranchDepartmentFact GetJobBranchDepartmentFact(IJobInvoicingPlugIn jobInvoicingPlugIn, IEnvironmentFact environmentFact, JobBranchDepartmentFact jobBranchDepartmentFact) => null;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
