using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationOrganizationProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationOrganization()
		{
			OrgHeader oSupplier = Factory.New<OrgHeader>();
			oSupplier.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			oSupplier.OH_FullName = "ORGANIZATION BR";
			oSupplier.MainAddress.OA_RN_NKCountryCode = "BR";
			oSupplier.MainAddress.OA_State = "SP";
			oSupplier.MainAddress.OA_Address1 = "PAULISTA AVENUE";
			oSupplier.MainAddress.OA_Address2 = "COMPLEMENTARY";
			var org = new DeclarationOrganizationProvider(oSupplier);
			AssertEquals("Declaration Organization ID should be", "58500398000105", org.ID);
			AssertEquals("Declaration Organization Name should be", oSupplier.OH_FullName, org.Name);
			AssertEquals("Declaration Organization CountryCode should be", oSupplier.MainAddress.OA_RN_NKCountryCode, org.CountryCode);
			AssertEquals("Declaration Organization CountryState should be", oSupplier.MainAddress.OA_State, org.CountryState);
			AssertEquals("Declaration Organization AddressLine should be", oSupplier.MainAddress.Address1 + " " + oSupplier.MainAddress.Address2, org.AddressLine);
		}
	}
}
