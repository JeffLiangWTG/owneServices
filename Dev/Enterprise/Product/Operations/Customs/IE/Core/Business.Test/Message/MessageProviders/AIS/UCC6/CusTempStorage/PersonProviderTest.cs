using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class PersonProviderTest : DataProviderTestCase<PersonProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("Arguments == null", PersonProvider.New(null, null, null, null));
				AssertNull("Null address argument", PersonProvider.New(null, "IE235346", "EOR", "+441234232"));
				AssertNotNull("Valid address argument", PersonProvider.New(orgAddress, null, null, null));
				AssertNotNull("Valid arguments", Provider);
			});
		}

		public void TestTypeOfPerson()
		{
			AssertEquals(ZString.Empty, Provider.TypeOfPerson);
			regNoType = "EOR";
			AssertEquals("EOR", GetProvider().TypeOfPerson);
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

			phone = "+44123332122";
			provider = GetProvider();
			AssertEquals("Phone Number", "+44123332122", provider.Communication.First().Id);
			AssertEquals("Phone Type", "TE", provider.Communication.First().Type);
		}

		public void TestCommunication_OrgAddressHeaderNull()
		{
			var provider = PersonProvider.New(Factory.New<OrgAddress>(), null, null, null);
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
			AssertEquals(string.Empty, GetProvider().Id);
			regNo = "E008";
			AssertEquals("E008", GetProvider().Id);
		}

		protected override PersonProvider GetProvider() => PersonProvider.New(orgAddress, regNo, regNoType, phone);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
			regNo = ZString.Empty;
			regNoType = ZString.Empty;
			phone = ZString.Empty;
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress;
		ZString regNo;
		ZString regNoType;
		ZString phone;
	}
}
