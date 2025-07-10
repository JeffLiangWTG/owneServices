using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class RepresentativeProviderTest : DataProviderTestCase<RepresentativeProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("TemporaryStorageHeader == null", RepresentativeProvider.New(null));
				var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
				AssertNull("Representative Address == null", RepresentativeProvider.New(temporaryStorageHeader));
				var orgAddress = Factory.New<OrgAddress>();
				temporaryStorageHeader.AMA_OA_Representative = orgAddress.PK;
				AssertNotNull("Can handle Representative Address Header == null", RepresentativeProvider.New(temporaryStorageHeader));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestStatus()
		{
			AssertEquals("Different declarant", "2", Provider.Status);

			tempStorageHeader.AMA_OA_Declarant = orgAddress.PK;
			AssertEquals("Same declarant", "3", GetProvider().Status);
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
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var orgAddress = Factory.New<OrgAddress>();
			temporaryStorageHeader.AMA_OA_Representative = orgAddress.PK;
			var provider = RepresentativeProvider.New(temporaryStorageHeader);
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

		protected override RepresentativeProvider GetProvider() => RepresentativeProvider.New(tempStorageHeader);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
			tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			tempStorageHeader.AMA_OA_Representative = orgAddress.PK;
		}

		TemporaryStorageHeader tempStorageHeader;
		OrgHeader orgHeader;
		OrgAddress orgAddress;
	}
}
