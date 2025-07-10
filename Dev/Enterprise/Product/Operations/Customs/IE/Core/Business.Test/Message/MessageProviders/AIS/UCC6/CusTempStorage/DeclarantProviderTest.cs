using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class DeclarantProviderTest : DataProviderTestCase<DeclarantProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", DeclarantProvider.New(null));
				AssertNotNull("Can handle Address Header == null", DeclarantProvider.New(Factory.New<OrgAddress>()));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestContactDetails()
		{
			AssertNull(Provider.ContactDetails);
		}

		public void TestAddress()
		{
			orgAddress.OA_Address1 = "10 Main Street";
			orgAddress.OA_Address2 = "District 12";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12 10XX";
			orgAddress.OA_RN_NKCountryCode = "IE";

			var provider = GetProvider();
			AssertType<FullAddressProvider>(provider.Address);
			AssertEquals("StreetAndNumber", "10 Main Street", provider.Address.StreetAndNumber);
			AssertEquals("StreetAdditionalLine", "District 12", provider.Address.StreetAdditionalLine);
			AssertEquals("City", "Dublin", provider.Address.City);
			AssertEquals("Postcode", "D12 10XX", provider.Address.Postcode);
			AssertEquals("Country", "IE", provider.Address.Country);
			AssertNull("Number", provider.Address.Number);
			AssertNull("PoBox", provider.Address.PoBox);
			AssertNull("SubDivision", provider.Address.SubDivision);
		}

		public void TestCommunication()
		{
			var nonCustomsContact = orgHeader.Contacts.AddNew();
			nonCustomsContact.OC_Phone = "+35319999999";
			var provider = GetProvider();
			AssertNull(provider.Communication.First());

			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Phone = "+35312345678";
			provider = GetProvider();
			AssertEquals("Phone Number", "+35312345678", provider.Communication.First().Id);
			AssertEquals("Phone Type", "TE", provider.Communication.First().Type);

			customsContact.OC_Email = "contact@me.com";
			provider = GetProvider();
			AssertEquals("Email Address", "contact@me.com", provider.Communication.First().Id);
			AssertEquals("Email Type", "EM", provider.Communication.First().Type);
		}

		public void TestCommunication_OrgAddressHeaderNull()
		{
			var provider = DeclarantProvider.New(Factory.New<OrgAddress>());
			AssertNull(provider.Communication.First());
		}

		public void TestName()
		{
			orgHeader.OH_FullName = "Bob the Builder";
			AssertEquals("Bob the Builder", GetProvider().Name);

			orgAddress.OA_CompanyNameOverride = "Thomas the Tank";
			AssertEquals("Thomas the Tank", GetProvider().Name);
		}

		public void TestId()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "B007");
			AssertEquals(string.Empty, GetProvider().Id);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			AssertEquals("IEE007", GetProvider().Id);
		}

		protected override DeclarantProvider GetProvider() => DeclarantProvider.New(orgAddress);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress;
	}
}
