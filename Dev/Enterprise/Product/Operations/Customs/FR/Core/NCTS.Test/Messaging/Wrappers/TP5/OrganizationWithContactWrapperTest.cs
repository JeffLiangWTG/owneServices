using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class OrganizationWithContactWrapperTest :  Customs.Business.Testing.DataProviderTestCase<OrganizationWithContactWrapper>
	{
		public void TestContactPerson()
		{
			var contact = Provider.ContactPerson;
			AssertEquals("ContactPerson should be mapped to organisation first contact of type CUS.", "Contact1, +33 1 01 01 01 01, a@a.com", $"{contact.Name}, {contact.PhoneNumber}, {contact.EmailAddress}");
		}

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
			AssertEquals("Not used in messages yet.", null, Provider.Role);
		}

		protected override OrganizationWithContactWrapper GetProvider()
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

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			contact1.OC_Email = "a@a.com";
			contact1.OC_Phone_Formatted = "+33 1 01 01 01 01";
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";
			contact2.OC_Email = "b@b.com";
			contact2.OC_Phone_Formatted = "+33 2 22 22 22 22";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			return OrganizationWithContactWrapper.New(orgHeader);
		}
	}
}
