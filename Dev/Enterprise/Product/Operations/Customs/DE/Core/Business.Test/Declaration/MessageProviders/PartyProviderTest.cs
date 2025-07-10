using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class PartyProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyProvider.NewOrNull(null));
			AssertNotNull(PartyProvider.NewOrNull(orgAddress));
		}

		public void TestEoriNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgContact.OC_OH = orgHeader.PK;
			TestHelper.CreateCL010CoutryList(Factory);
			AssertEquals("GREOR1", dataProvider.EoriNumber);
		}

		public void TestEoriBranchSuffix()
		{
			AssertEquals("EBS1", dataProvider.EoriBranchSuffix);
		}

		public void TestType()
		{
			AssertEquals(null, dataProvider.Type);
		}

		public void TestName_FullName()
		{
			AssertEquals("OH_FullName", "Importer", dataProvider.Name);
		}

		public void TestName_CompanyNameOverride()
		{
			orgAddress.OA_CompanyNameOverride = "Override";
			AssertEquals("OA_CompanyNameOverride", "Override", dataProvider.Name);
		}

		public void TestAddress()
		{
			AssertEquals("Address1", dataProvider.Address);
		}

		public void TestCity()
		{
			AssertEquals("City", dataProvider.City);
		}

		public void TestPostcode()
		{
			AssertEquals("2730018", dataProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals("US", dataProvider.Country);
		}

		public void TestAdditionalAddressInfo()
		{
			AssertEquals("Additional", dataProvider.AdditionalAddressInfo);
		}

		public void TestContactPerson_GlbStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Title = "Sachbearbeiter";
			staff.GS_FullName = "Bob Baumeister";
			staff.GS_WorkPhone = "06131474747";
			staff.GS_FaxNum = "12345678";
			staff.GS_EmailAddress = "bob.baumeister@samplefreight.de";
			dataProvider = PartyProvider.NewOrNull(orgAddress, staff);
			CombineAssertions(() =>
			{
				AssertEquals("Position", "Sachbearbeiter", dataProvider.ContactPerson.Position);
				AssertEquals("PersonName", "Bob Baumeister", dataProvider.ContactPerson.PersonName);
				AssertEquals("PhoneNumber", "06131474747", dataProvider.ContactPerson.PhoneNumber);
				AssertEquals("FacsimileNumber", "12345678", dataProvider.ContactPerson.FacsimileNumber);
				AssertEquals("MailAddress", "bob.baumeister@samplefreight.de", dataProvider.ContactPerson.MailAddress);
			});
		}

		public void TestContactPerson_WithContact()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Position", "DEV", dataProvider.ContactPerson.Position);
				AssertEquals("PersonName", "VWG", dataProvider.ContactPerson.PersonName);
				AssertEquals("PhoneNumber", "12345", dataProvider.ContactPerson.PhoneNumber);
				AssertEquals("FacsimileNumber", "789", dataProvider.ContactPerson.FacsimileNumber);
				AssertEquals("MailAddress", "123@abc.com", dataProvider.ContactPerson.MailAddress);
			});
		}

		public void TestContactPerson_WithoutContact()
		{
			dataProvider = PartyProvider.NewOrNull(orgAddress);
			AssertNull("Null", dataProvider.ContactPerson);
		}

		public void TestAddress2()
		{
			orgAddress.Address2 = "Address2";
			AssertEquals("Address2", dataProvider.Address2);
		}

		protected override IEnumerable<Expression<Func<PartyProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ContactPerson;
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			orgAddress.Header.OH_FullName = "Importer";
			orgAddress.Address1 = "Address1";
			orgAddress.City = "City";
			orgAddress.Postcode = "2730018";
			orgAddress.OA_RN_NKCountryCode = "US";
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional";
			orgContact = Factory.New<OrgContact>();
			orgContact.OC_Title = "DEV";
			orgContact.OC_ContactName = "VWG";
			orgContact.OC_Phone = "12345";
			orgContact.OC_Fax = "789";
			orgContact.OC_Email = "123@abc.com";
			dataProvider = PartyProvider.NewOrNull(orgAddress, orgContact);
		}
		IAESParty dataProvider;
		OrgAddress orgAddress;
		OrgContact orgContact;

		protected override PartyProvider GetProvider() => (PartyProvider)dataProvider;

		OrgAddress GetOrgWithEORNumberAndEORIBranch(ZString eoriNumber, ZString ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}
	}
}
