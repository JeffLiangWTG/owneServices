using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public abstract class FactLoaderBaseTest<TFact> : TestCaseWithFactory
	{
		public void TestJobRelatedFacts()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;
			job.JH_GS_NKRepSales = salesRep.GS_Code;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<TFact>(lastFact);
			var jobFact = lastFact as JobFact;

			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", jobFact.LocalClient.Fact, nestedFacts);
			AssertStaffFactAndChildren("SalesRep Fact should be populated", jobFact.SalesRep.Fact, nestedFacts);
		}

		public void TestGetFacts_JobForTaxBranchFact()
		{
			var loader = GetFactLoader(RulesContextType.JobBillingTaxBranchDefaulting);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);

			if (JobTypeForTaxBranchFact != null)
			{
				AssertEquals("Total facts", 5, result.Concat(nestedFacts).Count());
				AssertEquals("1 JobForTaxBranchFact", 1, result.Count(x => x.GetType() == JobTypeForTaxBranchFact));
				AssertEquals("1 DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code != "LOGINDEPT"));
				AssertEquals("1 Login DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code == "LOGINDEPT"));
				AssertEquals("1 JobBranchFact", 1, nestedFacts.Count(x => x is BranchFact brn && brn.Code != "LOGINBRANCH"));
				AssertEquals("1 Login BranchFact", 1, nestedFacts.Count(x => x is BranchFact brn && brn.Code == "LOGINBRANCH"));

				var jobForTaxBranchFact = (IJobBranchDepartmentFact)result.First(x => x.GetType() == JobTypeForTaxBranchFact);

				AssertJobBranchFact("JobBranch Fact should be populated", jobForTaxBranchFact.JobBranch.Fact, nestedFacts);
				AssertJobDepartmentFact("JobDepartment Fact should be populated", jobForTaxBranchFact.JobDepartment.Fact, nestedFacts);
			}
			else
			{
				AssertEquals("Total facts", 3, result.Concat(nestedFacts).Count());
				AssertType<TFact>(result.Last());
				AssertEquals("1 Login DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code == "LOGINDEPT"));
				AssertEquals("1 Login BranchFact", 1, nestedFacts.Count(x => x is BranchFact brn && brn.Code == "LOGINBRANCH"));
			}
		}

		public void TestOrganizationFact_IsOrgProxyOfLoginCompany()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			var result = loader.GetFacts(jobPluginMock.Object, company, branchMock.Object, departmentMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<TFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			var result = loader.GetFacts(jobPluginMock.Object, company, branchMock.Object, departmentMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<TFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfAnotherCompany()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<TFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = orgHeader.PK;
			branch.GB_GC = company.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<TFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		#region  GetFacts

		protected IEnumerable<IInputFact> GetFacts(IFactLoader loader, IJobInvoicingSupporter invoicingSupporterMock)
		{
			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock);

			return GetFacts(loader, jobPluginMock.Object);
		}

		protected IEnumerable<IInputFact> GetFacts(IFactLoader loader, IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			var countryMock = new Mock<ICountry>();
			var companyMock = new Mock<ICompany>();
			companyMock.Setup(x => x.Country).Returns(countryMock.Object);
			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			return loader.GetFacts(jobInvoicingPlugIn, companyMock.Object, branchMock.Object, departmentMock.Object);
		}

		#endregion

		protected abstract Type JobTypeForTaxBranchFact { get; }

		protected abstract IFactLoader GetFactLoader(RulesContextType contextType);

		protected IEnumerable<IInputFact> GetNestedFacts(IEnumerable<IInputFact> topLevelFacts)
		{
			var finder = new NestedFactsFinder();
			var queue = new Queue<IInputFact>(topLevelFacts);
			var result = new HashSet<IInputFact>();
			while (queue.Count > 0)
			{
				var fact = queue.Dequeue();
				var nestedFacts = finder.GetDirectNestedInputFacts(fact);
				foreach (var nestedFact in nestedFacts)
				{
					if (result.Add(nestedFact.Fact))
					{
						queue.Enqueue(nestedFact.Fact);
					}
				}
			}

			return result;
		}

		protected void AssertFactJoin<T>(string message, IInputFact fact, IEnumerable<IInputFact> facts)
		{
			AssertNotNull(message, fact);
			AssertType<T>(fact);
			AssertCollectionContains(fact, facts);
		}

		protected void AssertOrganizationFactAndChildren(string message, IOrganisationWithMainAddressFact orgFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<OrganisationWithMainAddressFact>(message, orgFact, facts);
			AssertFactJoin<AddressFact>("MainAddress should be populated", orgFact.MainAddress.Fact, facts);
			AssertFactJoin<CountryFact>("MainAddress.Country should be populated", orgFact.MainAddress.Fact.Country.Fact, facts);
		}

		protected void AssertUnlocoFactAndChildren(string message, IUNLOCOFact unlocoFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<UNLOCOFact>(message, unlocoFact, facts);
			AssertFactJoin<CountryFact>("Country Fact shoule be populated", unlocoFact.Country.Fact, facts);
		}

		protected void AssertStaffFactAndChildren(string message, IStaffFact staffFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<StaffFact>(message, staffFact, facts);
			AssertFactJoin<DepartmentFact>("HomeDepartment should be populated", staffFact.HomeDepartment.Fact, facts);
		}

		void AssertJobBranchFact(string message, IBranchFact jobBranchFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<BranchFact>(message, jobBranchFact, facts);
		}

		void AssertJobDepartmentFact(string message, IDepartmentFact jobDepartmentFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<DepartmentFact>(message, jobDepartmentFact, facts);
		}
	}
}
