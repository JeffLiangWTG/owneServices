using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class BindingHelperTest : TestCaseWithDummy
	{
		public void TestGetNestedControlDataMember()
		{
			AssertEquals("DocAddress.E2_AddressNumber", BindingHelper.GetNestedControlDataMember("OrganisationPK", "DocAddress.OrganisationPK", "E2_AddressNumber"));
			AssertEquals("DocAddress.OrganisationPK.E2_AddressNumber", BindingHelper.GetNestedControlDataMember("", "DocAddress.OrganisationPK", "E2_AddressNumber"));
			AssertEquals("DocAddress.OrganisationPK.E2_AddressNumber", BindingHelper.GetNestedControlDataMember(".", "DocAddress.OrganisationPK", "E2_AddressNumber"));
		}
	}
}
