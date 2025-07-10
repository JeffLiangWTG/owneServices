using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class PersonalIdentityWrapperTest : TestCaseWithFactory
	{
		public void Test01ExactNameAndPassportMatch()
		{
			var redHerring = MakeExistingPerson("Red Herring", "ABCDE");
			var blueHerring = MakeExistingPerson("Daniel Clarke", "XYZ");
			var danielPerson = MakeExistingPerson("Daniel Clarke", "ABCDE");
			var staffDaniel = MakeExistingStaff("Daniel Clarke", "ABCDE");
			var contactDaniel = MakeExistingContact("Daniel Clarke", "ABCDE");
			RunStaffTest(staffDaniel, danielPerson);
			RunContactTest(contactDaniel, danielPerson);
		}

		public void Test02ExactNameMatchNoSourcePassport()
		{
			var redHerring = MakeExistingPerson("Red Herring", "ABCDE");
			var danielPerson = MakeExistingPerson("Daniel Clarke", "ABCDE");
			var staffDaniel = MakeExistingStaff("Daniel Clarke", "");
			var contactDaniel = MakeExistingContact("Daniel Clarke", "");
			var personFound = RunStaffTest(staffDaniel, danielPerson);
			AssertEquals("Existing person's passport is unchanged", "ABCDE", personFound.PER_Passport);
			personFound = RunContactTest(contactDaniel, danielPerson);
			AssertEquals("Existing person's passport is unchanged", "ABCDE", personFound.PER_Passport);
		}

		public void Test03ExactNameMatchHasSourcePassportNoPersonPassport()
		{
			var redHerring = MakeExistingPerson("Red Herring", "ABCDE");
			var staffDaniel = MakeExistingStaff("Daniel Clarke", "UseMeLikeCurrency");
			var danielPerson = MakeExistingPerson("Daniel Clarke", "");
			var contactDaniel = MakeExistingContact("Contact Clarke", "MyVoiceIsMyPassport");
			var personFound = RunStaffTest(staffDaniel, danielPerson);
			AssertEquals("Existing person's passport is updated", "UseMeLikeCurrency", personFound.PER_Passport);
			danielPerson.PER_Passport = "";
			personFound = RunContactTest(contactDaniel, contactDaniel.Person);
			AssertEquals("Existing person's passport is updated", "MyVoiceIsMyPassport", personFound.PER_Passport);
		}

		public void Test04PassportAndNameSoundexMatch()
		{
			var personBrendAn = MakeExistingPerson("Brendan Paine", "123456");
			var personBrendOn = MakeExistingPerson("Brendon Paine", "123456");
			var personBrenTon = MakeExistingPerson("Brenton Payne", "123456");
			var personBrenda = MakeExistingPerson("Brenda Paine", "123456");
			var staff = MakeExistingStaff("Brendon Paine", "123456");
			var contact = MakeExistingContact("Red Herring", "123456");
			RunStaffTest(staff, personBrendOn);
			RunContactTest(contact, contact.Person);
			personBrendOn.Delete();
			RunStaffTest(staff, personBrendAn);
			RunContactTest(contact, contact.Person);
			personBrendAn.Delete();
			RunStaffTest(staff, personBrenTon);
			RunContactTest(contact, contact.Person);
		}

		public void Test05PassportAndNamesInitials()
		{
			var personNitWit = MakeExistingPerson("Nit Wit", "123456");
			var staff = MakeExistingStaff("Ninad Wagle", "123456");
			var contact = MakeExistingContact("Contact Wagle", "123456");
			RunStaffTest(staff, personNitWit);
			RunContactTest(contact, contact.Person);
		}

		public void Test06NoMatch()
		{
			var personDaniel = MakeExistingPerson("Daniel Clarke", "ABCDE");
			var staff = MakeExistingStaff("Ninad Wagle", "123456");
			var contact = MakeExistingContact("Contact Wagle", "123456");
			AssertNull(new PersonalIdentityWrapper(staff).TryReallyHardToGetExistingGlbPerson());
			AssertNotEquals(new PersonalIdentityWrapper(contact).TryReallyHardToGetExistingGlbPerson(), personDaniel.PK);
		}

		public void Test07CloseNameMatchWithPassportButWrongIssuingCountry()
		{
			var personNitWitZA = MakeExistingPerson("Nit Wit", "123456", "ZA");
			var staff = MakeExistingStaff("Ninad Wagle", "123456");
			var contact = MakeExistingContact("Contact Wagle", "123456");
			AssertEquals(null, new PersonalIdentityWrapper(staff).TryReallyHardToGetExistingGlbPerson());
			AssertNotNull(new PersonalIdentityWrapper(contact).TryReallyHardToGetExistingGlbPerson());
			var personNitWitGB = MakeExistingPerson("Nit Wit", "123456", "GB");
			AssertEquals(personNitWitGB, new PersonalIdentityWrapper(staff).TryReallyHardToGetExistingGlbPerson());
			AssertEquals(contact.Person, new PersonalIdentityWrapper(contact).TryReallyHardToGetExistingGlbPerson());
		}

		public void Test08RunMethodsOnManifestHeader()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var daniel = MakeExistingStaff("Daniel Clarke", "ABCDE");
			var liana = MakeExistingStaff("Liana Clarke", "123456");
			var johnLocke = MakeExistingContact("John Locke", "9878");
			var igor = MakeExistingContact("Igor Shearer", "xyz");
			manifest.MakeGlbPersonsFromStaffOrContacts(new[] { daniel, liana });
			var cusPersonDaniel = manifest.Persons[0];
			var cusPersonLiana = manifest.Persons[1];
			manifest.MakeGlbPersonsFromStaffOrContacts(new[] { johnLocke, igor });
			var cusPersonLocke = manifest.Persons[2];
			var cusPersonIgor = manifest.Persons[3];
			AssertEquals("Daniel Clarke", cusPersonDaniel.PersonFullName);
			AssertEquals("Liana Clarke", cusPersonLiana.PersonFullName);
			AssertEquals("John Locke", cusPersonLocke.PersonFullName);
			AssertEquals("Igor Shearer", cusPersonIgor.PersonFullName);
		}

		GlbPerson RunStaffTest(GlbStaff existingStaff, GlbPerson expectedExistingPerson)
		{
			var wrapper = new PersonalIdentityWrapper(existingStaff);
			var foundHit = wrapper.TryReallyHardToGetExistingGlbPerson();
			AssertEquals(expectedExistingPerson.PK, foundHit.PK);
			return foundHit;
		}

		GlbPerson RunContactTest(OrgContact existingContact, GlbPerson expectedExistingPerson)
		{
			var wrapper = new PersonalIdentityWrapper(existingContact);
			var foundHit = wrapper.TryReallyHardToGetExistingGlbPerson();
			AssertEquals(expectedExistingPerson.PK, foundHit.PK);
			return foundHit;
		}

		GlbPerson MakeExistingPerson(ZString name, ZString passportNumber, string issuingCountry = "GB")
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = name;
			person.PER_Passport = passportNumber;
			person.PER_PassportPlaceOfIssue = issuingCountry;
			return person;
		}

		GlbStaff MakeExistingStaff(ZString name, ZString passportNumber)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = name;
			var pp = staff.Certificates.AddNew();
			pp.XZ_Type = "PAS";
			pp.XZ_RefNumber = passportNumber;
			pp.XZ_RN_NKCountryOfIssuance = "GB";
			return staff;
		}

		OrgContact MakeExistingContact(ZString name, ZString passportNumber)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = name;
			contact.Person.UpdateFromContact(contact);
			var pp = contact.Certificates.AddNew();
			pp.XZ_Type = "PAS";
			pp.XZ_RefNumber = passportNumber;
			pp.XZ_RN_NKCountryOfIssuance = "GB";
			return contact;
		}
	}
}
