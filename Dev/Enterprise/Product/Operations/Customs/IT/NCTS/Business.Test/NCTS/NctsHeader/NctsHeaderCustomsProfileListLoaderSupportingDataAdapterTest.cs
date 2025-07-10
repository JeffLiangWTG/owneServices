using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderCustomsProfileListLoaderSupportingDataAdapterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderCustomsProfileListLoaderSupportingDataAdapter(null));
	}

	public void TestCompanyPK()
	{
		AssertEquals("CompanyPK", nctsHeader.Company.PK, adapter.CompanyPK);
	}

	public void TestGetEligibleOrganizations_Phase5()
	{
		nctsHeader.BH_ApplicationCode = "NC5";

		nctsHeader.Principal.OrganisationPK = ZGuid.Empty;
		nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
		nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
		nctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
		AssertEquals("When Principal, Consignor, Consignee and Representative are not set, EligibleOrganizations Count", 0, adapter.GetEligibleOrganizations().Count);

		nctsHeader.Principal.OrganisationPK = Factory.New<OrgHeader>().PK;
		nctsHeader.Consignor.OrganisationPK = Factory.New<OrgHeader>().PK;
		nctsHeader.Consignee.OrganisationPK = Factory.New<OrgHeader>().PK;
		nctsHeader.MovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;
		CombineAssertions("When Principal, Consignor, Consignee and Representative are set", () =>
		{
			var eligibleOrganizations = adapter.GetEligibleOrganizations();
			AssertEquals("EligibleOrganizations Count", 4, eligibleOrganizations.Count);
			AssertEquals("EligibleOrganizations contains Principal?", true, eligibleOrganizations.Contains(nctsHeader.Principal.Organisation));
			AssertEquals("EligibleOrganizations contains Consignor?", true, eligibleOrganizations.Contains(nctsHeader.Consignor.Organisation));
			AssertEquals("EligibleOrganizations contains Consignee?", true, eligibleOrganizations.Contains(nctsHeader.Consignee.Organisation));
			AssertEquals("EligibleOrganizations contains Representative?", true, eligibleOrganizations.Contains(nctsHeader.MovementHeader.Representative.Organisation));
		});
	}

	public void TestGetEligibleOrganizations_Phase4()
	{
		nctsHeader.BH_ApplicationCode = "NCT";

		nctsHeader.DeclarantAddressPK = ZGuid.Empty;
		nctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
		AssertEquals("When Declarant and Representative are not set, EligibleOrganizations Count", 0, adapter.GetEligibleOrganizations().Count);

		nctsHeader.DeclarantAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
		nctsHeader.MovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;
		CombineAssertions("When Declarant and Representative are set", () =>
		{
			var eligibleOrganizations = adapter.GetEligibleOrganizations();
			AssertEquals("EligibleOrganizations Count", 2, eligibleOrganizations.Count);
			AssertEquals("EligibleOrganizations contains Declarant?", true, eligibleOrganizations.Contains(nctsHeader.DeclarantAddress.Header));
			AssertEquals("EligibleOrganizations contains Representative?", true, eligibleOrganizations.Contains(nctsHeader.MovementHeader.Representative.Organisation));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		adapter = new NctsHeaderCustomsProfileListLoaderSupportingDataAdapter(nctsHeader);
	}

	NctsHeader nctsHeader;
	ICustomsProfileListProviderSupportingData adapter;
}
