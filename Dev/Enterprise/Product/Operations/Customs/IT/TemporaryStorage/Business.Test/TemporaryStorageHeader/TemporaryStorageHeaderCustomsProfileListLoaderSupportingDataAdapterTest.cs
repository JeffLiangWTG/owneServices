using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapter(null));
	}

	public void TestCompanyPK()
	{
		AssertEquals("CompanyPK", GlbCompany.CurrentCompany.PK, adapter.CompanyPK);
	}

	public void TestGetEligibleOrganizations()
	{
		header.AMA_OA_Declarant = ZGuid.Empty;
		header.AMA_OA_Representative = ZGuid.Empty;
		AssertEquals("When Declarant and Representative are not set, EligibleOrganizations Count", 0, adapter.GetEligibleOrganizations().Count);

		header.AMA_OA_Declarant = Factory.NewWithValidTestData<OrgAddress>().PK;
		header.AMA_OA_Representative = Factory.NewWithValidTestData<OrgAddress>().PK;
		CombineAssertions(() =>
		{
			var eligibleOrganizations = adapter.GetEligibleOrganizations();
			AssertEquals("EligibleOrganizations Count", 2, eligibleOrganizations.Count);
			AssertEquals("EligibleOrganizations contains Declarant?", true, eligibleOrganizations.Contains(header.Declarant.Header));
			AssertEquals("EligibleOrganizations contains Representative?", true, eligibleOrganizations.Contains(header.Representative.Header));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		adapter = new TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapter(header);
	}

	TemporaryStorageHeader header;
	ICustomsProfileListProviderSupportingData adapter;
}
