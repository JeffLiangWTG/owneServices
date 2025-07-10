using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MRepresentativeProviderTest : DataProviderTestCase<MRepresentativeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("declaration missing", () => new MRepresentativeProvider(null));
		}

		public void TestId()
		{
			AssertEquals("EORI", "IE123456789", Provider.Id);
		}

		public void TestStatus()
		{
			declaration.JE_DeclarantType = ZString.Empty;
			AssertNull("Null Status", GetProvider().Status);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("Status 2 JE_DeclarantType", "2", GetProvider().Status);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("Status 3 JE_DeclarantType", "3", GetProvider().Status);
		}

		public void TestContactPerson()
		{
			var contact = Provider.ContactPerson;
			AssertEquals("Contact : Name", "Joe Bloggs", contact.Name);
			AssertEquals("Contact : Phone", "555 12345", contact.PhoneNumber);
			AssertEquals("Contact : Email", "test@example.com", contact.EmailAddress);
		}

		protected override MRepresentativeProvider GetProvider()
		{
			return new MRepresentativeProvider(declaration);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Test Co";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "A12B3C4";
			address.OA_RN_NKCountryCode = "IE";

			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var contact = orgHeader.Contacts.AddNew();
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "555 12345";
			contact.OC_Email = "test@example.com";
		}
		OrgAddress address;
		JobDeclaration declaration;
	}
}
