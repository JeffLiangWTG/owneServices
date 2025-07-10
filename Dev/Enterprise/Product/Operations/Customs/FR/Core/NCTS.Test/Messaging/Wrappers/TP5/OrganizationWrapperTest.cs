using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class OrganizationWrapperTest : Customs.Business.Testing.DataProviderTestCase<OrganizationWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should match organisation EORI.", "FR12345678900001", Provider.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal organisation OH_FullName.", "BN CORP", Provider.Name);
		}

		public void TestAddress()
		{
			var address = Provider.Address;
			AssertEquals("Address should be mapped to organisation Main Address.", "177 Impasse Jane Poupelet, 24140, MAURENS, FR", $"{address.StreetAndNumber}, {address.PostCode}, {address.City}, {address.Country}");
		}

		public void TestRole()
		{
			AssertEquals("Role should be TestRole", "TestRole", Provider.Role);
		}

		protected override OrganizationWrapper GetProvider()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "BN CORP";

			var address = orgHeader.MainAddress;
			address.OA_City = "MAURENS";
			address.OA_RN_NKCountryCode = "FR";
			address.OA_PostCode = "24140";
			address.OA_Address1 = "177 Impasse Jane Poupelet";
			address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);

			return OrganizationWrapper.New(orgHeader, "TestRole");
		}
	}
}
