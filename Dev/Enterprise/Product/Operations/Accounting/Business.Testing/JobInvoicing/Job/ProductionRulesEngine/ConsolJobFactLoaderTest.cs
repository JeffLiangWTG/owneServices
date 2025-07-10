using System;
using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class ConsolJobFactLoaderTest : FactLoaderBaseTest<ConsolJobFact>
	{
		public void TestGetFacts()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var loadPort = Factory.NewWithValidTestData<RefUNLOCO>();
			loadPort.RL_RN_NKCountryCode = country.Code;

			var dischargePort = Factory.NewWithValidTestData<RefUNLOCO>();
			dischargePort.RL_RN_NKCountryCode = country.Code;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_GE_HomeDepartment = department.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GS_NKRepSales = salesRep.GS_Code;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.Origin).Returns(loadPort);
			invoicingSupporterMock.Setup(x => x.Destination).Returns(dischargePort);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			AssertEquals("Total facts", 10, result.Concat(nestedFacts).Count());
			AssertEquals("2 UNLOCOFact", 2, nestedFacts.Count(x => x.GetType() == typeof(UNLOCOFact)));
			AssertEquals("1 OrganisationWithMainAddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(OrganisationWithMainAddressFact)));
			AssertEquals("1 ConsolJobFact", 1, result.Count(x => x.GetType() == typeof(ConsolJobFact)));
			AssertEquals("1 AddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(AddressFact)));
			AssertEquals("1 CountryFact", 1, nestedFacts.Count(x => x.GetType() == typeof(CountryFact)));
			AssertEquals("1 StaffFact", 1, nestedFacts.Count(x => x.GetType() == typeof(StaffFact)));
			AssertEquals("1 DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code != "LOGINDEPT"));
			AssertEquals("1 Login DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code == "LOGINDEPT"));
			AssertEquals("1 Login BranchFact", 1, nestedFacts.OfType<BranchFact>().Count());

			ConsolJobFact consolFact = (ConsolJobFact)result.First(x => x.GetType() == typeof(ConsolJobFact));

			AssertOrganizationFactAndChildren("SendingAgent Fact should be populated", consolFact.SendingAgent.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("ReceivingAgent Fact should be populated", consolFact.ReceivingAgent.Fact, nestedFacts);

			AssertUnlocoFactAndChildren("LoadPort Fact should be populated", consolFact.LoadPort.Fact, nestedFacts);
			AssertUnlocoFactAndChildren("DischargePort Fact should be populated", consolFact.DischargePort.Fact, nestedFacts);
		}

		public void TestGetFacts_SendingAgent_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.SendingAgent).Returns(org),
				(consolFact) => consolFact.SendingAgent.Fact);
		}

		public void TestGetFacts_ReceivingAgent_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(org),
				(consolFact) => consolFact.ReceivingAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfLoginCompany(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ConsolJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns(orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var consolFact = (ConsolJobFact)result.First(x => x.GetType() == typeof(ConsolJobFact));

			var orgWithAddressFact = getFact(consolFact);
			AssertOrganizationFactAndChildren("SendingAgent/ReceivingAgent Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_SendingAgent_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.SendingAgent).Returns(org),
				(consolFact) => consolFact.SendingAgent.Fact);
		}

		public void TestGetFacts_ReceivingAgent_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(org),
				(consolFact) => consolFact.ReceivingAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ConsolJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var consolFact = (ConsolJobFact)result.First(x => x.GetType() == typeof(ConsolJobFact));

			var orgWithAddressFact = getFact(consolFact);
			AssertOrganizationFactAndChildren("SendingAgent/ReceivingAgent Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_SendingAgent_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.SendingAgent).Returns(org),
				(consolFact) => consolFact.SendingAgent.Fact);
		}

		public void TestGetFacts_ReceivingAgent_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(org),
				(consolFact) => consolFact.ReceivingAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfAnotherCompany(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ConsolJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var consolFact = (ConsolJobFact)result.First(x => x.GetType() == typeof(ConsolJobFact));

			var orgWithAddressFact = getFact(consolFact);
			AssertOrganizationFactAndChildren("SendingAgent/ReceivingAgent Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_SendingAgent_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.SendingAgent).Returns(org),
				(consolFact) => consolFact.SendingAgent.Fact);
		}

		public void TestGetFacts_ReceivingAgent_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(org),
				(consolFact) => consolFact.ReceivingAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ConsolJobFact, IOrganisationWithMainAddressFact> getFact)
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
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var consolFact = (ConsolJobFact)result.First(x => x.GetType() == typeof(ConsolJobFact));

			var orgWithAddressFact = getFact(consolFact);
			AssertOrganizationFactAndChildren("SendingAgent/ReceivingAgent Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		protected override Type JobTypeForTaxBranchFact => typeof(ConsolJobForTaxBranchFact);

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new ConsolJobFactLoader(contextType);
		}
	}
}
