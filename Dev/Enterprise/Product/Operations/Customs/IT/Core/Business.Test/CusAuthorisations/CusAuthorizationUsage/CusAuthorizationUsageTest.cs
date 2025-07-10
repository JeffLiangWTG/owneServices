using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation()
	{
		var authorizationUsage = Factory.New<CusAuthorizationUsage>();
		AssertType<CusAuthorizationUsageValidation>(authorizationUsage.Validation);
	}

	public void TestLookups()
	{
		var authorizationUsage = Factory.New<CusAuthorizationUsage>();
		AssertType<CusAuthorizationUsageLookups>(authorizationUsage.Lookups);
	}

	public void TestIsForAuthorisationHeader()
	{
		var owner = Factory.NewWithValidTestData<OrgHeader>();
		var usage = Factory.New<CusAuthorizationUsage>();
		usage.AGC_Code = "CWP";
		usage.AGC_OH_Owner = owner.PK;
		usage.AGC_Number = "WHP";
		AssertEquals("header is null", false, usage.IsForAuthorisationHeader(null));

		var header = Factory.New<CusAuthorisationHeader>();
		AssertEquals("All 3 header fields are empty", false, usage.IsForAuthorisationHeader(header));

		header.CPH_Type = "CWP";
		header.CPH_OH_PermitHolder = owner.PK;
		header.CPH_Number = "WHP";
		AssertEquals("All 3 fields are equal", true, usage.IsForAuthorisationHeader(header));

		header.CPH_Type = "CW1";
		AssertEquals("Some fields are different", false, usage.IsForAuthorisationHeader(header));
	}
}
