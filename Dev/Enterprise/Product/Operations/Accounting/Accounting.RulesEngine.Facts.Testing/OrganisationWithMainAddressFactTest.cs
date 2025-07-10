using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class OrganisationWithMainAddressFactTest : TestCase
	{
		public void TestNullOrgHeader_ThrowsException()
		{
			var addressFactMock = new Mock<IAddressFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new OrganisationWithMainAddressFact(null, addressFactMock.Object));
		}

		public void TestNullAddressFact_ThrowsException()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertExceptionThrown<ArgumentNullException>(() => new OrganisationWithMainAddressFact(orgHeader, null));
		}

		public void TestPK()
		{
			var pk = Guid.NewGuid();
			var orgHeader = Factory.NewWithPrimaryKey<OrgHeader>(pk);
			var addressFactMock = new Mock<IAddressFact>();

			var organisationFact = new OrganisationWithMainAddressFact(orgHeader, addressFactMock.Object);
			AssertEquals(pk, organisationFact.PK);
		}

		public void TestCode()
		{
			var orgCode = "ABIGAS";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			var addressFactMock = new Mock<IAddressFact>();

			var organisationFact = new OrganisationWithMainAddressFact(orgHeader, addressFactMock.Object);
			AssertEquals(orgCode, organisationFact.Code);
		}

		public void TestMainAddress_NotNull()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var addressFactMock = new Mock<IAddressFact>();

			var organisationFact = new OrganisationWithMainAddressFact(orgHeader, addressFactMock.Object);
			AssertNotNull(organisationFact.MainAddress);
		}

		public void TestIsProxyOrgOfCurrentCompany()
		{
			var addressFactMock = new Mock<IAddressFact>();
			var org = Factory.New<OrgHeader>();
			var organisationFact_Org = new OrganisationWithMainAddressFact(org, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), false, organisationFact_Org.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_Org.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfCurrentCompany);

			var orgProxy = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var organisationFact_OrgProxy = new OrganisationWithMainAddressFact(orgProxy, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact_OrgProxy.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, organisationFact_OrgProxy.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfCurrentCompany_BranchProxy()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;
			var addressFactMock = new Mock<IAddressFact>();

			var organisationFact1 = new OrganisationWithMainAddressFact(orgHeader, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact1.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact1).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, organisationFact1.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, ((IOrganisationFact)organisationFact1).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfAnyCompany()
		{
			var addressFactMock = new Mock<IAddressFact>();
			var org = Factory.New<OrgHeader>();
			var organisationFact_Org = new OrganisationWithMainAddressFact(org, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), false, organisationFact_Org.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_Org.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfCurrentCompany);

			var orgProxy = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = orgProxy.PK;
			var organisationFact_OrgProxy = new OrganisationWithMainAddressFact(orgProxy, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact_OrgProxy.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_OrgProxy.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfAnyCompany_BranchProxy()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = orgHeader.PK;
			var addressFactMock = new Mock<IAddressFact>();

			var organisationFact = new OrganisationWithMainAddressFact(orgHeader, addressFactMock.Object);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact).IsProxyOrgOfCurrentCompany);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
