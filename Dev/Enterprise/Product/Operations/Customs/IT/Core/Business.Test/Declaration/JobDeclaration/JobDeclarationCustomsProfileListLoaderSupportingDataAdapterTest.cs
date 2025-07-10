using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationCustomsProfileListLoaderSupportingDataAdapterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationCustomsProfileListLoaderSupportingDataAdapter(declaration: null));
	}

	public void TestCompanyPK_WhenDeclarationIsNotUCC6()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("CompanyPK", declarationCompany.PK, adapter.CompanyPK);
		}
	}

	public void TestCompanyPK_WhenDeclarationIsUCC6()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("CompanyPK", GlbCompany.CurrentCompany.PK, adapter.CompanyPK);
		}
	}

	public void TestGetEligibleOrganizations()
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertEquals("When Declarant and Representative are not set, EligibleOrganizations Count", 0, adapter.GetEligibleOrganizations().Count);

		declaration.JE_OA_DeclarantAddress = Factory.New<OrgHeader>().MainAddress.PK;
		declaration.JE_OA_Representative = Factory.New<OrgHeader>().MainAddress.PK;
		CombineAssertions("When Declarant and Representative are set", () =>
		{
			var eligibleOrganizations = adapter.GetEligibleOrganizations();
			AssertEquals("EligibleOrganizations Count", 2, eligibleOrganizations.Count);
			AssertEquals("EligibleOrganizations contains Declarant?", true, eligibleOrganizations.Contains(declaration.DeclarantAddress.Header));
			AssertEquals("EligibleOrganizations contains Representative?", true, eligibleOrganizations.Contains(declaration.RepresentativeOrgAddress.Header));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declarationCompany = Factory.NewWithValidTestData<GlbCompany>();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_GC = declarationCompany.PK;
		adapter = new JobDeclarationCustomsProfileListLoaderSupportingDataAdapter(declaration);
	}

	GlbCompany declarationCompany;
	JobDeclaration declaration;
	ICustomsProfileListProviderSupportingData adapter;
}
