using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ConsigneeDataProvider))]
sealed class ConsigneeDataProviderTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("JobDocAddress==null", ConsigneeDataProvider.New(null));

		var address = Factory.New<JobDocAddress>();
		AssertNull("JobDocAddress is empty", ConsigneeDataProvider.New(address));
		address.E2_AddressOverride = true;
		AssertNotNull("E2_AddressOverride = true", ConsigneeDataProvider.New(address));
		address.E2_OA_Address = Factory.New<OrgAddress>().PK;
		AssertNotNull("E2_AddressOverride = true", ConsigneeDataProvider.New(address));
	});

	public void TestReferenceNumber()
	{
		var address = Factory.New<JobDocAddress>();
		address.E2_AddressOverride = true;
		AssertNull("ReferenceNumber not available", ConsigneeDataProvider.New(address).ReferenceNumber);
	}
}
