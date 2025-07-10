using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CLREGContactInfoProvider))]
	sealed class CLREGContactInfoProviderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CLREGContactInfoProvider>
	{
		public void TestContactPhonesFromContactDetails()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var contact = header.Contacts.AddNew();
			contact.OC_Email = "test1@test.com";
			contact.OC_ContactName = "Test Contact 1";
			contact.OC_JobCategory = "ACNT";
			contact.OC_Mobile = "04123456";
			contact.OC_OA_OrgAddress = header.MainAddress.PK;
			contact.OC_HomePhone = "028864564";
			contact.OC_Phone = "02789654123";
			contact.OC_Fax = "02886411";

			var wrapper = new OrgHeaderWrapper(header);
			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_OC_Contact = contact.PK;

			AssertEquals("Contact Name", "Test Contact 1", contactDataProvider.ZA_ContName);
			AssertEquals("Contact Purpose", "ACNT", contactDataProvider.ZA_ContPurpose);
			AssertEquals("Contact Mobile", "04123456", contactDataProvider.ZA_ContMob);
			AssertEquals("Contact Email", "test1@test.com", contactDataProvider.ZA_ContEmail);
			AssertEquals("Contact Phone", "7-8965-4123", contactDataProvider.ZA_ContPh);
			AssertEquals("Contact Phone Prefix", "2", contactDataProvider.ZA_ContPhPref);
			AssertEquals("Contact AH", "2886-4564", contactDataProvider.ZA_ContAH);
			AssertEquals("Contact AH Prefix", "0", contactDataProvider.ZA_ContAHPref);
			AssertEquals("Contact Fax", "88-6411", contactDataProvider.ZA_ContFax);
			AssertEquals("Contact Fax Prefix", "2", contactDataProvider.ZA_ContFaxPref);
		}

		public void TestContactAddressDetails()
		{
			var wrapper = new OrgHeaderWrapper(OrganisationWithAddressDetails);
			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_OA_ContactAddress = OrganisationWithAddressDetails.MainAddress.PK;

			AssertEquals("address 1", "Test Address 1", contactDataProvider.ZA_Cont1);
			AssertEquals("address 2", "Test address 2", contactDataProvider.ZA_Cont2);
			AssertEquals("City", "London", contactDataProvider.ZA_ContCity);
			AssertEquals("Post Code", "2000", contactDataProvider.ZA_ContPostCode);
			AssertEquals("State", "NSW", contactDataProvider.ZA_ContState);
			AssertEquals("Port", "AUSYD", contactDataProvider.ZA_ContPort);
		}

		public void TestContactPostalAddressDetails()
		{
			var wrapper = new OrgHeaderWrapper(OrganisationWithAddressDetails);
			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_OA_ContactPostalAddress = OrganisationWithAddressDetails.MainAddress.PK;

			AssertEquals("address 1", "Test Address 1", contactDataProvider.ZA_ContPost1);
			AssertEquals("address 2", "Test address 2", contactDataProvider.ZA_ContPost2);
			AssertEquals("City", "London", contactDataProvider.ZA_ContPostCity);
			AssertEquals("Post Code", "2000", contactDataProvider.ZA_ContPostPostCode);
			AssertEquals("State", "NSW", contactDataProvider.ZA_ContPostState);
			AssertEquals("Port", "AUSYD", contactDataProvider.ZA_ContPostPort);
		}

		OrgHeader OrganisationWithAddressDetails => CreateHeader(Factory);

		OrgHeader CreateHeader(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			header.MainAddress.OA_Address1 = "Test Address 1";
			header.MainAddress.OA_Address2 = "Test address 2";
			header.MainAddress.OA_City = "London";
			header.MainAddress.OA_PostCode = "2000";
			header.MainAddress.OA_State = "NSW";
			return header;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CLREGContactInfoProvider>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var wrapper = new OrgHeaderWrapper(CreateHeader(factory));
			var parent = wrapper.CLREGInfoProvider;
			var result = factory.New<CLREGContactInfoProvider>();
			result.B7_ParentID = parent.PK;
			result.B7_ParentTableCode = parent.TablePrefix;
			return result;
		}
	}
}
