using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business.Testing;

class CustomArrivalCustomerReferenceFormatLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAuthorizationLocationCodeList() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG00001";
		GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = orgHeader.PK;
		var authorisationTestHelper = new CusAuthorisationTestHelper(Factory, defaultPermitHolder: orgHeader);
		authorisationTestHelper.CreateCusAuthorisationHeader("101", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar);
		authorisationTestHelper.CreateCusAuthorisationHeader("102", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec);
		Factory.Save();

		AssertContainsExactElementsInAnyOrder("Codes", new[] { "101" }, lookups.AuthorizationLocationCodeList.GetAllCodes());
		AssertSame("cached", lookups.AuthorizationLocationCodeList, lookups.AuthorizationLocationCodeList);
	});

	public void TestYearOptionList() => CombineAssertions(() =>
	{
		AssertType<CustomArrivalCustomerReferenceFormatYearOptionList>(lookups.YearOptionList);
		AssertSame("cached", lookups.YearOptionList, lookups.YearOptionList);
	});

	protected override void SetUp()
	{
		base.SetUp();
		var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
		lookups = new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory).Lookups;
	}

	CustomArrivalCustomerReferenceFormatLookups lookups;
}
