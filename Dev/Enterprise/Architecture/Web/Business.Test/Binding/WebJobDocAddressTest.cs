using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[TestedType(typeof(WebJobDocAddress))]
	[HttpContextEnabledTest]
	sealed class WebJobDocAddressTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNewAddressDoesNotResetCompanyIfMiscOrganisationIsSelected()
		{
			var jda = Factory.New<JobDocAddress>();

			var orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, "TESTING NEW COMPANY");
			AssertEquals(0, Factory.Load<OrgHeader>(orgQuery).Length);

			ZQuery addressQuery = new ZQuery();
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, "TESTING NEW ADDRESS");
			AssertEquals(0, Factory.Load<OrgAddress>(addressQuery).Length);

			jda.E2_CompanyName = "TESTING NEW COMPANY";
			jda.E2_AddressOverride = true;
			AssertNotNull(jda.Organisation);
			AssertEquals(false, jda.HasRealOrganisation);

			var wjda = new WebJobDocAddress(jda);
			AssertEquals("TESTING NEW COMPANY", wjda.CompanyName);

			wjda.AddressPK = jda.Organisation.MainAddress.PK;
			wjda.Address1 = "TESTING NEW ADDRESS";
			AssertEquals("TESTING NEW COMPANY", wjda.CompanyName);
			AssertEquals("TESTING NEW ADDRESS", wjda.Address1);
		}

		public void TestAllTemporaryNewAddressRelatedBusinessObjectsAreMarkedAsDeleted()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, "TESTING NEW COMPANY");
			AssertEquals(0, Factory.Load<OrgHeader>(orgQuery).Length);

			ZQuery addressQuery = new ZQuery();
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, "TESTING NEW ADDRESS");
			AssertEquals(0, Factory.Load<OrgAddress>(addressQuery).Length);

			Assert(!jda.E2_AddressOverride);
			AssertNull(wjda.Organisation);
			AssertNull(wjda.Address);
			AssertNull(wjda.Contact);

			wjda.CompanyName = "TESTING NEW COMPANY";
			wjda.Address1 = "TESTING NEW ADDRESS";
			wjda.Address2 = "SOME INFO";
			wjda.PostCode = "60084";
			wjda.State = "ILLINOIS";
			wjda.CountryCode = "US";
			wjda.ContactName = "VASYA PUPKIN";
			wjda.Phone = "1112223344";
			wjda.Email = "pishite@pisma.mne";
			wjda.SaveAsNew = true;

			Assert(!jda.E2_AddressOverride);
			AssertNotNull(wjda.Organisation);
			AssertNotNull(wjda.Address);
			AssertNotNull(wjda.Contact);
			Assert(!wjda.Organisation.IsDeleted);
			Assert(!wjda.Address.IsDeleted);
			Assert(!wjda.Contact.IsDeleted);

			OrgHeader tempOrg = wjda.Organisation;
			OrgAddress tempAddress = wjda.Address;
			OrgContact tempContact = wjda.Contact;

			wjda.SaveAsNew = false;
			Assert(jda.E2_AddressOverride);
			AssertNull(wjda.Organisation);
			AssertNull(wjda.Address);
			AssertNull(wjda.Contact);
			Assert(tempOrg.IsDeleted);
			Assert(tempAddress.IsDeleted);
			Assert(tempContact.IsDeleted);
		}

		public void TestNewWebJobDocAddressWithOverride()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);

			AssertEquals(ZString.Empty, wjda.CompanyName);
			AssertEquals(ZString.Empty, wjda.Address1);
			AssertEquals(ZString.Empty, wjda.Address2);
			AssertEquals(ZString.Empty, wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals(ZString.Empty, wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(false, jda.E2_AddressOverride);

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, "TEST ORG 123");
			AssertEquals(0, Factory.Load<OrgHeader>(orgQuery).Length);

			ZQuery addressQuery = new ZQuery();
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, "TEST ADDR 123");
			AssertEquals(0, Factory.Load<OrgAddress>(addressQuery).Length);

			wjda.CompanyName = "TEST ORG 123";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals(ZString.Empty, wjda.Address1);
			AssertEquals(ZString.Empty, wjda.Address2);
			AssertEquals(ZString.Empty, wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals(ZString.Empty, wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.Address1 = "TEST ADDR 123";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals(ZString.Empty, wjda.Address2);
			AssertEquals(ZString.Empty, wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals(ZString.Empty, wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.Address2 = "TEST ADDR 2";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals("TEST ADDR 2", wjda.Address2);
			AssertEquals(ZString.Empty, wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals(ZString.Empty, wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.City = "TEST CITY";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals("TEST ADDR 2", wjda.Address2);
			AssertEquals("TEST CITY", wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals(ZString.Empty, wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.State = "TEST STATE";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals("TEST ADDR 2", wjda.Address2);
			AssertEquals("TEST CITY", wjda.City);
			AssertEquals(ZString.Empty, wjda.PostCode);
			AssertEquals("TEST STATE", wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.PostCode = "60084";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals("TEST ADDR 2", wjda.Address2);
			AssertEquals("TEST CITY", wjda.City);
			AssertEquals("60084", wjda.PostCode);
			AssertEquals("TEST STATE", wjda.State);
			AssertEquals(ZString.Empty, wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);

			wjda.CountryCode = "US";
			AssertEquals("TEST ORG 123", wjda.CompanyName);
			AssertEquals("TEST ADDR 123", wjda.Address1);
			AssertEquals("TEST ADDR 2", wjda.Address2);
			AssertEquals("TEST CITY", wjda.City);
			AssertEquals("60084", wjda.PostCode);
			AssertEquals("TEST STATE", wjda.State);
			AssertEquals("US", wjda.CountryCode);
			AssertEquals(true, jda.E2_AddressOverride);
		}

		public void TestControlFillingWithSaveEnabledFirst()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);
			AssertNull("Internals Are null originally", wjda.Organisation);
			AssertNull(wjda.Address);
			AssertNull(wjda.Contact);

			wjda.SaveAsNew = ZBool.True;
			Assert("override disabled because save is enabled", !jda.E2_AddressOverride);
			AssertNotNull("Internals created with save enabling", wjda.Organisation);
			AssertNotNull(wjda.Address);
			AssertNull("Contact won't be created until ContactName is filled with something different then default", wjda.Contact);

			wjda.CompanyName = "new ROGA & COPITA";
			wjda.Address1 = "new Wasyuki";
			wjda.Address2 = "ulico stroiteley d7";
			wjda.PostCode = "123321";
			wjda.City = "Chicago";
			wjda.State = "IL";
			wjda.CountryCode = "US";
			wjda.Phone = "123123123";

			Assert("Contact still does not have value", jda.ContactPK.IsEmpty || jda.ContactPK.IsDefault);

			AssertEquals(wjda.Organisation.OH_FullName, "new ROGA & COPITA");
			AssertEquals(wjda.Address.OA_Address1, "new Wasyuki");
			AssertEquals(wjda.Address.OA_State, "IL");

			AssertEquals(jda.E2_CompanyName, "new ROGA & COPITA");
			AssertEquals(jda.E2_Address1, "new Wasyuki");
			AssertEquals(jda.E2_State, "IL");

			AssertEquals("Address phone will be updated because contact does not exist", wjda.Address.OA_Phone, "123123123");
		}

		public void TestControlFillingWithNonExistingData()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);

			Assert(jda.E2_CompanyName.IsEmpty);
			Assert(jda.OrganisationPK.IsEmpty || jda.OrganisationPK.IsDefault);
			Assert(jda.E2_OA_Address.IsEmpty || jda.E2_OA_Address.IsDefault);
			Assert(jda.ContactPK.IsEmpty || jda.ContactPK.IsDefault);

			wjda.CompanyName = "new ROGA & COPITA";
			Assert(jda.E2_AddressOverride);
			wjda.Address2 = "ulico stroiteley d7";

			Assert("everything goes as override", jda.E2_AddressOverride);
			AssertEquals(jda.E2_CompanyName, "new ROGA & COPITA");
			AssertEquals(jda.E2_Address2, "ulico stroiteley d7");

			wjda.Address1 = "new Wasyuki";
			wjda.PostCode = "123321";
			wjda.City = "Chicago";
			wjda.State = "IL";
			wjda.CountryCode = "US";

			Assert("nothing above caused override change", jda.E2_AddressOverride);
			wjda.SaveAsNew = ZBool.True;
			Assert("Override was turned off", !jda.E2_AddressOverride);

			Assert("New data was created", !jda.E2_CompanyName.IsEmpty);
			Assert(!jda.OrganisationPK.IsEmpty);
			Assert(!jda.E2_OA_Address.IsEmpty);
			AssertNotNull(wjda.Organisation);
			AssertNotNull(wjda.Address);
			Assert("Contact still does not have value", jda.ContactPK.IsEmpty || jda.ContactPK.IsDefault);

			AssertEquals(wjda.Organisation.OH_FullName, "new ROGA & COPITA");
			AssertEquals(wjda.Address.OA_Address1, "new Wasyuki");
			AssertEquals(wjda.Address.OA_State, "IL");

			wjda.Phone = "123123123";
			AssertEquals("Address phone will be updated because contact does not exist", wjda.Address.OA_Phone, "123123123");

			wjda.ContactName = "Ostap Bender";
			AssertNotNull(wjda.Contact);
			wjda.Email = "WhatSUUUp@cargowise.com";
			Assert("Name different from default shoudld trigger contact creation on SaveAsNew enabled", wjda.Contact != null);
			AssertEquals("Email should not go to Address because Contact exist", wjda.Contact.OC_Email, "WhatSUUUp@cargowise.com");

			AssertEquals(jda.E2_CompanyName, "new ROGA & COPITA");
			AssertEquals(jda.E2_Address1, "new Wasyuki");
			AssertEquals(jda.E2_State, "IL");
			AssertEquals(jda.E2_Contact, "Ostap Bender");
		}

		public void TestControlFillingWithExistingData()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();

			JobDocAddress jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.OrganisationPK = org.PK;
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.ContactPK = orgContact.PK;

			WebJobDocAddress webJobDocAddress = new WebJobDocAddress(jobDocAddress);

			AssertNotNull("Organisation", webJobDocAddress.Organisation);
			AssertNotNull("Address", webJobDocAddress.Address);
			AssertNotNull("Contact", webJobDocAddress.Contact);

			JobDocAddress anotherJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			anotherJobDocAddress.E2_AddressOverride = false;
			WebJobDocAddress anotherWebJobDocAddress = new WebJobDocAddress(anotherJobDocAddress);
			AssertNull("Organisation", anotherWebJobDocAddress.Organisation);
			AssertNull("Address", anotherWebJobDocAddress.Address);
			AssertNull("Contact", anotherWebJobDocAddress.Contact);
		}

		public void TestRemoveContactAndPhoneFaxEmailShouldBeEmptyWhenAddressIsNull()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Phone = "123456";
			orgAddress.OA_Fax = "FaxTest";
			orgAddress.OA_Email = "test@test.com";

			JobDocAddress jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_Phone = "123456Override";
			jobDocAddress.E2_Fax = "FaxTestOverride";
			jobDocAddress.E2_Email = "test@test.comOverride";

			WebJobDocAddress webJobDocAddress = new WebJobDocAddress(jobDocAddress);
			webJobDocAddress.Address = orgAddress;

			webJobDocAddress.ContactPK = ZGuid.Empty;
			AssertEquals("Phone should not be empty", "123456", webJobDocAddress.Phone);
			AssertEquals("Fax should not be empty", "FaxTest", webJobDocAddress.Fax);
			AssertEquals("Email should not be empty", "test@test.com", webJobDocAddress.Email);

			webJobDocAddress.Address = null;
			webJobDocAddress.ContactPK = ZGuid.Empty;

			AssertEquals("Phone should be empty", "", webJobDocAddress.Phone);
			AssertEquals("Fax should be empty", "", webJobDocAddress.Fax);
			AssertEquals("Email should be empty", "", webJobDocAddress.Email);
		}

		public void TestUpdateJobDocAddressPropertiesIgnoredWhenDeleted()
		{
			var jda = Factory.NewWithValidTestData<JobDocAddress>();
			var wjda = new WebJobDocAddress(jda);

			wjda.JobDocAddressPropertiesHaveBeenUpdated = false;
			wjda.UpdateJobDocAddressProperties();
			Assert(wjda.JobDocAddressPropertiesHaveBeenUpdated);

			wjda = new WebJobDocAddress(jda);
			wjda.JobDocAddressPropertiesHaveBeenUpdated = false;
			wjda.Delete();
			wjda.UpdateJobDocAddressProperties();
			Assert(!wjda.JobDocAddressPropertiesHaveBeenUpdated);
		}

		public void TestUpdateJobDocAddressPropertiesProducesNoExceptionsWhenDeleted()
		{
			var jda = Factory.NewWithValidTestData<JobDocAddress>();
			var wjda = new WebJobDocAddress(jda);

			wjda.IsOptional = true;
			AssertNoExceptionThrown(wjda.UpdateJobDocAddressProperties);

			wjda = new WebJobDocAddress(jda);
			wjda.IsOptional = true;
			wjda.Delete();
			AssertNoExceptionThrown(wjda.UpdateJobDocAddressProperties);
		}

		#region Override Test Cases

		public void TestOverrideAndSaveAsNewRelation()
		{
			TestWebAddress.SaveAsNew = ZBool.False;
			TestJobDocAddress.E2_AddressOverride = ZBool.False;

			AssertOverride(false);
			AssertSaveAsNew(false);

			TestWebAddress.SaveAsNew = ZBool.True;
			AssertOverride(false);
			AssertSaveAsNew(true);

			TestWebAddress.SaveAsNew = ZBool.False;
			AssertOverride(true);
			AssertSaveAsNew(false);

			TestWebAddress.CompanyName = "abc";
			TestWebAddress.SaveAsNew = ZBool.False;
			AssertOverride(true);
			AssertSaveAsNew(false);

			TestWebAddress.SaveAsNew = ZBool.True;
			AssertOverride(false);
			AssertSaveAsNew(true);
		}

		public void TestOverrideForCompanyNameChanges()
		{
			TestWebAddress.SaveAsNew = ZBool.False;
			TestJobDocAddress.E2_AddressOverride = ZBool.False;
			AssertOverride(false);
			TestWebAddress.CompanyName = "TESTING";
			AssertOverride(true);
			AssertEquals("TESTING", TestWebAddress.CompanyName);
			AssertEquals("TESTING", TestJobDocAddress.E2_CompanyName);
			AssertSaveAsNew(false);

			TestWebAddress.CompanyName = "TESTING2";
			AssertOverride(true);
			AssertEquals("TESTING2", TestWebAddress.CompanyName);
			AssertEquals("TESTING2", TestJobDocAddress.E2_CompanyName);
			AssertOverride(true);
			AssertSaveAsNew(false);

			TestWebAddress.SaveAsNew = ZBool.True;
			AssertOverride(false);
			AssertSaveAsNew(true);

			CreateOrgHeaderAndAddressAndContact(Factory);
			TestWebAddress.OrganisationPK = OrgHeader.PK;
			AssertOverride(false);
			AssertEquals(OrgHeader.OH_FullName, TestWebAddress.CompanyName);
			//AssertSaveAsNew(false);

			Factory.Save();
			TestWebAddress.SaveAsNew = false;
			TestWebAddress.OrganisationPK = OrgHeader.PK;
			AssertOverride(false);
			AssertEquals(OrgHeader.OH_FullName, TestWebAddress.CompanyName);
			AssertSaveAsNew(false);
		}

		#endregion

		#region SocialSecurityNumber

		public void TestSocialSecurityNumber()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);

			wjda.GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			wjda.SocialSecurityNumber = "111";
			AssertEquals(string.Empty, wjda.SocialSecurityNumber);

			wjda.GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			wjda.SocialSecurityNumber = "111";
			AssertEquals(string.Empty, wjda.SocialSecurityNumber);

			//jda = Factory.New<ISFDocAddress>();
			//wjda = new WebJobDocAddress(jda);

			//wjda.GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			//wjda.SocialSecurityNumber = "111";
			//AssertEquals(string.Empty, wjda.SocialSecurityNumber);

			//wjda.GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			//wjda.SocialSecurityNumber = "111";
			//AssertEquals("111", wjda.SocialSecurityNumber);
		}

		#endregion

		#region SocialSecurityNumberDateOfBirth

		public void TestSocialSecurityNumberDateOfBirth()
		{
			JobDocAddress jda = Factory.New<JobDocAddress>();
			WebJobDocAddress wjda = new WebJobDocAddress(jda);

			wjda.GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			ZDateTime testDate = new ZDateTime(2010, 12, 15);
			wjda.SocialSecurityNumberDateOfBirth = testDate;
			AssertEquals(ZDateTime.Empty, wjda.SocialSecurityNumberDateOfBirth);

			wjda.GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			wjda.SocialSecurityNumberDateOfBirth = testDate;
			AssertEquals(ZDateTime.Empty, wjda.SocialSecurityNumberDateOfBirth);

			//jda = Factory.New<ISFDocAddress>();
			//wjda = new WebJobDocAddress(jda);

			//wjda.GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			//wjda.SocialSecurityNumberDateOfBirth = testDate;
			//AssertEquals(ZDateTime.Empty, wjda.SocialSecurityNumberDateOfBirth);

			//wjda.GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			//wjda.SocialSecurityNumberDateOfBirth = testDate;
			//AssertEquals(testDate, wjda.SocialSecurityNumberDateOfBirth);
		}

		#endregion

		#region TestSaveOverriddenJobDocAddressWithNewInformation

		public void TestSaveOverriddenJobDocAddressWithNewInformation()
		{
			SetCompany("Cargowise Importers");
			SetAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			SetContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
			TestWebAddress.SaveAsNew = false;

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertCompany("Cargowise Importers");
			AssertAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			AssertContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
		}

		#endregion

		#region TestNewOrganisationWithControllingBanch

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestNewOrganisationWithControllingBanch()
		{
			IUserContext initialUserContext = EnvProxy.Instance.CurrentUserContext;

			GlbCompany globalCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch globalBranch = globalCompany.Branches.AddNew();
			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			globalBranch.GB_Code = "TST";
			globalBranch.GB_RL_NKHomePort = "USORD";
			company.OH_Code = "TSTCO";
			company.OH_RL_NKClosestPort = "USORD";
			if (company.CompanyDataCollection.Count == 0)
			{
				company.CompanyDataCollection.AddNew();
			}
			company.CompanyDataCollection[0].OB_GB_ControllingBranch = globalBranch.PK;

			OrgContact contact = company.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Email = "test@test.com";
			contact.SetHashedPassword("abrakadabra");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			OrgContactWebUser webSiteUser = new OrgContactWebUser();
			webSiteUser.Login(company.OH_Code, "test@test.com", "abrakadabra");
#if NETFRAMEWORK
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(webSiteUser);
#elif NET
			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject("SiteUser", (WebUser)webSiteUser);
#endif

			Assert(webSiteUser.IsLoggedIn);
			AssertEquals("Test Contact", webSiteUser.LoggedInUserName);
#if NETFRAMEWORK
			AssertEquals(company.PK, ((OrgContactWebUser)(WebEnv.AppInstance.SiteUser)).LoggedInOrganisation.PK);
#elif NET
			var siteUser = WebEnv.SiteUser;
			var relatedOrgs = ((OrgContactWebUser)siteUser).AllUserRelatedOrgs;
			AssertEquals(company.PK, ((OrgContactWebUser)(siteUser)).LoggedInOrganisation.PK);
#endif
			SetCompany("Cargowise1");
			SetAddress("Unit 1", "1 Main St", "1000", "Sydney", "NSW", "AU");
			TestWebAddress.SaveAsNew = true;

			AssertNotNull(TestWebAddress.Organisation);
			AssertNotNull(TestWebAddress.Organisation.CompanyData);
			AssertNull(TestWebAddress.Organisation.CompanyData.ControllingBranch);

			Factory.Save();

			AssertNotNull(TestWebAddress.Organisation.CompanyData);
			AssertNotNull(TestWebAddress.Organisation.CompanyData.ControllingBranch);
			AssertEquals(globalBranch, TestWebAddress.Organisation.CompanyData.ControllingBranch);

			TestJobDocAddress.Delete();
			company.CompanyDataCollection[0].OB_GB_ControllingBranch = ZGuid.Empty;
			Factory.Save();

			TestJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress);
			SetCompany("Cargowise2");
			SetAddress("Unit 2", "2 Main St", "2000", "Chicago", "IL", "US");
			TestWebAddress.SaveAsNew = true;
			Factory.Save();

			AssertNotNull(TestWebAddress.Organisation.CompanyData);
			AssertNull(TestWebAddress.Organisation.CompanyData.ControllingBranch);

			GlbBranch globalBranch2 = globalCompany.Branches.AddNew();
			globalBranch2.GB_Code = "TS2";
			globalBranch2.GB_RL_NKHomePort = "USJFK";
			company.CompanyDataCollection[0].OB_GB_ControllingBranch = globalBranch.PK;
			Factory.Save();

			TestJobDocAddress.Organisation.CompanyData.OB_GB_ControllingBranch = globalBranch2.PK;
			TestJobDocAddress.Organisation.OH_FullName = "Cargowise3";
			Factory.Save();
			AssertNotNull(TestWebAddress.Organisation.CompanyData);
			AssertNotNull(TestWebAddress.Organisation.CompanyData.ControllingBranch);
			AssertEquals(globalBranch2, TestWebAddress.Organisation.CompanyData.ControllingBranch);

			TestJobDocAddress.Delete();
			Factory.Save();

			TestJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress);
			SetCompany("Cargowise4");
			SetAddress("Unit 4", "4 Main St", "4000", "New York", "NY", "US");
			TestWebAddress.SaveAsNew = true;
			TestWebAddress.Organisation.CompanyData.OB_GB_ControllingBranch = globalBranch2.PK;
			Factory.Save();

			AssertNotNull(TestWebAddress.Organisation.CompanyData);
			AssertNotNull(TestWebAddress.Organisation.CompanyData.ControllingBranch);
			AssertEquals(globalBranch2, TestWebAddress.Organisation.CompanyData.ControllingBranch);

			webSiteUser.Logout();
			EnvProxy.Instance.SetUserContext(initialUserContext);
		}

#endregion

		#region TestSaveNewOrganisationWithAddressAndContact

		public void TestSaveNewOrganisationWithAddressAndContact()
		{
			SetCompany("Cargowise Importers");
			SetAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			SetContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
			TestWebAddress.SaveAsNew = true;

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertCompany("Cargowise Importers");
			AssertAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			AssertContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
		}

		#endregion

		#region TestCheckIfEnteredDataIsValidToCreateNewOrganization

		public void TestCheckIfEnteredDataIsValidToCreateNewOrganization()
		{
			SetCompany("Cargowise Importers");
			SetAddress("Unit 35", "1200 Long St", "2000", "Sydney2008", "", "AU"); //Incorrect city
			SetContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
			TestWebAddress.SaveAsNew = true;

			TestWebAddress.RunPreSaveValidation();

			Assert("Error expected", TestWebAddress.SaveAsNewInfo.GetErrors().Count() == 1);
			AssertEquals("Error has correct warning", "Unable to locate closest port for organization Cargowise Importers.  Please check City, State and Country/Region or uncheck the 'Save' checkbox.", TestWebAddress.SaveAsNewInfo.GetErrors().GetFirst().Message);

			SetAddress("", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			TestWebAddress.RunPreSaveValidation();

			Assert("Error expected", TestWebAddress.SaveAsNewInfo.GetErrors().Count() == 1);
			AssertEquals("Error has correct warning", "Organization Cargowise Importers must have an address.  Please input an address on line 1 or uncheck the 'Save' checkbox.", TestWebAddress.SaveAsNewInfo.GetErrors().GetFirst().Message);

			SetAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			TestWebAddress.RunPreSaveValidation();

			Assert("No error expected", TestWebAddress.SaveAsNewInfo.GetErrors().Count() == 0);
		}

		public void TestCheckIfChangingCountryResetsStateValidationErrors()
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var cn = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			Assert("Expected Australia to require states to be entered", au.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered);
			Assert("Expected China to not require states to be entered", cn.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.NoValidationRule);

			SetCompany("Test");
			SetAddress("X", "", "12345", "X", "", "AU");
			TestWebAddress.SaveAsNew = false;
			TestWebAddress.RunPreSaveValidation();

			Assert("Expected state validation error", TestWebAddress.GetErrors().Count() == 1);

			TestWebAddress.CountryCode = "CN";
			Assert("Expected state validation to be reset", TestWebAddress.GetErrors().Count() == 0);
		}

		#endregion

		#region TestAddressOverride

		public void TestAddressOverride()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CreateOrgHeaderAndAddressAndContact(newFactory);
			newFactory.Save();

			TestWebAddress.Delete();

			TestJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress, true);

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);

			TestWebAddress.SaveAsNew = false;

			Factory.Save();
			LoadBusinessObjectsInNewFactory();
			Assert("Empty Contact should not be a reason to override", TestJobDocAddress.E2_AddressOverride);
		}

		#endregion

		#region TestResidentialAddress

		public void TestResidentialAddress()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CreateOrgHeaderAndAddressAndContact(newFactory);
			newFactory.Save();

			TestWebAddress.Delete();

			TestJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress, true);

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);
			SetContact(OrgContact);

			TestWebAddress.IsResidentialAddress = true;
			TestWebAddress.SaveAsNew = true;

			Assert("Precondition: Address is not residential", !OrgAddress.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Residential).Enabled);

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			Assert("Address is residential", OrgAddress.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Residential).Enabled);
		}

		#endregion

		#region TestOrganizationPKChanged

		public void TestOrganizationPKChanged()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			TestJobDocAddress.DefaultAddressType = AddressType.OFC;

			JobDocAddress etalon = Factory.New<JobDocAddress>();
			etalon.DefaultAddressType = TestJobDocAddress.DefaultAddressType;
			etalon.OrganisationPK = OrgHeader.PK;

			TestWebAddress.OrganisationPK = OrgHeader.PK;

			AssertCompany(etalon.Organisation);
			AssertAddress(etalon.Address);
			AssertContact(etalon.Contact);
		}

		#endregion

		#region TestAddressPKChanged

		public void TestAddressPKChanged()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			TestWebAddress.AddressPK = OrgAddress.PK;

			AssertAddress(OrgAddress);
		}

		#endregion

		#region TestContactPKChanged

		public void TestContactPKChanged()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			TestWebAddress.ContactPK = OrgContact.PK;

			AssertContact(OrgContact);
		}

		#endregion

		#region TestDefaultAddress

		public void TestDefaultAddress()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			TestJobDocAddress.DefaultAddressType = AddressType.OFC;
			TestWebAddress.OrganisationPK = OrgHeader.PK;

			AssertAddress(OrgAddress);

			OrgHeader otherHeader = Factory.New<OrgHeader>();
			otherHeader.OH_FullName = "other organisation";

			otherHeader.MainAddress.OA_Address1 = "MainAddress";

			OrgAddress pickupAddress = otherHeader.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Another St";
			pickupAddress.OA_Address2 = "Another sub street";
			pickupAddress.OA_PostCode = "2222";
			pickupAddress.OA_City = "Sydney";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "AU", "Sydney", "NSW");
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			TestJobDocAddress.DefaultAddressType = AddressType.PIC;
			TestWebAddress.OrganisationPK = otherHeader.PK;

			AssertAddress(pickupAddress);
		}

		#endregion

		#region TestUpdateContact

		public void TestUpdateContact()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);
			SetContact(OrgContact);

			TestWebAddress.Phone = "911";
			TestWebAddress.Fax = "912";
			TestWebAddress.Email = "email@somewhere.com";

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertContact(OrgContact.OC_ContactName, "911", "912", "email@somewhere.com");
		}

		#endregion

		#region TestUpdateAddress

		public void TestUpdateAddress()
		{
			CreateOrgHeaderAndAddressAndContact(Factory);

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);
			SetContact(OrgContact);

			TestWebAddress.City = "Kiev";
			TestWebAddress.PostCode = "00001";
			TestWebAddress.State = "";
			TestWebAddress.CountryCode = "UK";

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertAddress(OrgAddress.OA_Address1, OrgAddress.OA_Address2, "00001", "Kiev", "", "UK");
		}

		public void TestNoDefaultCountry()
		{
			var address = Factory.New<JobDocAddress>();
			var webAddress = new WebJobDocAddress(address);

			webAddress.CompanyName = "Test";
			AssertNullOrEmpty(nameof(webAddress.CountryCode), webAddress.CountryCode);
		}

		#endregion

		#region TestCreateNewOrganisation

		public void TestCreateNewOrganisation()
		{
			SetCompany("Cargowise Importers");
			SetAddress("Unit 35", "1200 Long St", "2000", "Sydney", "NSW", "AU");
			SetContact("John Smith", "+61 (2) 1234 5678", "+61 (2) 8765 4321", "mykola@example.com");
			TestWebAddress.SaveAsNew = true;

			Factory.Save();

			OrgHeader newOrgHeader = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Cargowise Importers"));
			AssertNotNull("Organisation", newOrgHeader);

			OrgAddress[] addresses = (OrgAddress[])newOrgHeader.Addresses.Find(new ZQuery(OrgAddressSchema.OA_Address1, "Unit 35"));
			AssertNotNull("Address", addresses[0]);
			AssertEquals("Address2", "1200 Long St", addresses[0].OA_Address2);
			AssertEquals("PostCode", "2000", addresses[0].OA_PostCode);
			AssertEquals("City", "Sydney", addresses[0].OA_City);
			AssertEquals("State", "NSW", addresses[0].OA_State);
			AssertEquals("Country", "AU", addresses[0].OA_RL_NKRelatedPortCode.Substring(0, 2));

			AssertNotNull("Contact", newOrgHeader.Contacts[0]);
			AssertEquals("Contact Name", "John Smith", newOrgHeader.Contacts[0].OC_ContactName);
			AssertEquals("Phone", "+61 (2) 1234 5678", newOrgHeader.Contacts[0].OC_Phone);
			AssertEquals("Fax", "+61 (2) 8765 4321", newOrgHeader.Contacts[0].OC_Fax);
			AssertEquals("Email", "mykola@example.com", newOrgHeader.Contacts[0].OC_Email);
		}

		#endregion

		#region TestCreateNewContactForExistingOrganisation

		public void TestCreateNewContactForExistingOrganisation()
		{
			BusinessObjectFactory dataCreationFactory = new BusinessObjectFactory();
			CreateOrgHeaderAndAddressAndContact(dataCreationFactory);
			dataCreationFactory.Save();

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);
			SetContact(OrgContact);
			SetContact("John Bon Jovi", "911", "912", "johnbonjovi@somewhere.com"); // Different Contact - should create a new OrgContact				
			TestWebAddress.SaveAsNew = true;

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertContact("John Bon Jovi", "911", "912", "johnbonjovi@somewhere.com");

			OrgContact[] newContacts = (OrgContact[])OrgHeader.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, "John Bon Jovi"));
			Assert("New Contact", newContacts.Length == 1);

			OrgContact[] existingContacts = (OrgContact[])OrgHeader.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, OrgContact.OC_ContactName));
			Assert("Existing Contact", existingContacts.Length == 1);
		}

		#endregion

		#region TestCreateNewAddressForExistingOrganisation

		public void TestCreateNewAddressForExistingOrganisation()
		{
			BusinessObjectFactory dataCreationFactory = new BusinessObjectFactory();
			CreateOrgHeaderAndAddressAndContact(dataCreationFactory);
			dataCreationFactory.Save();

			SetCompany(OrgHeader);
			SetAddress(OrgAddress);
			SetContact(OrgContact);
			SetAddress("25 Short St", "", "1111", "Sydney", "NSW", "AU"); // Different Address - should create a new OrgAddress
			TestWebAddress.SaveAsNew = true;

			Factory.Save();
			LoadBusinessObjectsInNewFactory();

			AssertAddress("25 Short St", "", "1111", "Sydney", "NSW", "AU");

			OrgAddress[] newAddresses = (OrgAddress[])OrgHeader.Addresses.Find(new ZQuery(OrgAddressSchema.OA_Address1, "25 Short St"));
			Assert("New Addresses", newAddresses.Length == 1);

			OrgAddress[] existingAddresses = (OrgAddress[])OrgHeader.Addresses.Find(new ZQuery(OrgAddressSchema.OA_Address1, "20 Short St"));
			Assert("Existing Addresses", existingAddresses.Length == 1);
		}

		#endregion

		#region TestGetPortCode

		//public void TestGetPortCode()
		//{
		//    CreateOrgHeaderAndAddressAndContact(Factory);

		//    TestWebAddress.OrganisationPK = OrgHeader.PK;
		//    AssertEquals("Expected existing address related port code", OrgAddress.OA_RL_NKRelatedPortCode, TestWebAddress.GetPortCode());

		//    TestWebAddress.AddressPK = ZGuid.Empty;
		//    AssertEquals("Expected organisation closest port code", OrgHeader.OH_RL_NKClosestPort, TestWebAddress.GetPortCode());

		//    TestWebAddress.OrganisationPK = ZGuid.Empty;
		//    SetAddress("", "", "", "Melbourne", "VIC", "AU");
		//    AssertEquals("Expected port code defined by country, state, city", "AUMEL", TestWebAddress.GetPortCode());
		//}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			Assert("Precondition: JobDocAddress is not deleted", !TestJobDocAddress.IsDeleted);
			TestWebAddress.Delete();
			Assert("If WebJobDocAddress is deleted then JobDocAddress should be deleted too", TestJobDocAddress.IsDeleted);
		}

		#endregion

		#region TestJobDocAddressWillBeOverriden

		//public void TestJobDocAddressWillBeOverriden()
		//{
		//    CreateOrgHeaderAndAddressAndContact(Factory);
		//    Factory.Save();

		//    TestWebAddress.CompanyName = OrgHeader.OH_FullName;
		//    TestWebAddress.SaveAsNew = false;
		//    Assert("OrgHeaderExists = true, SaveAsNew = false", !TestWebAddress.JobDocAddressWillBeOverriden);

		//    SetCompany("New Company");
		//    SetAddress("", "", "", "Sydney", "NSW", "AU");
		//    TestWebAddress.SaveAsNew = true;
		//    Assert("OrgHeaderExists = false, SaveAsNew = true, CanCreateNewOrgHeader = true", !TestWebAddress.JobDocAddressWillBeOverriden);

		//    TestWebAddress.SaveAsNew = false;
		//    Assert("OrgHeaderExists = false, SaveAsNew = false, CanCreateNewOrgHeader = true", TestWebAddress.JobDocAddressWillBeOverriden);

		//    TestWebAddress.SaveAsNew = true;
		//    SetAddress("", "", "", "", "", "");
		//    Assert("OrgHeaderExists = false, SaveAsNew = true, CanCreateNewOrgHeader = false", TestWebAddress.JobDocAddressWillBeOverriden);
		//}

		#endregion

		#region TestInactive

		public void TestInactive()
		{
			TestWebAddress.Inactive = true;

			Factory.Save();

			Assert("WebJobDocAddress should be deleted before saving if it is inactive", TestJobDocAddress.IsDeleted);
		}

		#endregion

		#region TestIsOptional

		public void TestIsOptional()
		{
			TestWebAddress.InitBeforeBinding(new OrgHeaderAutoCompleteHelper(Factory)
			{
				IsConsignor = true,
				IsConsignee = false,
				NewOrgRelationType = NewOrgRelationTypes.Supplier
			});

			SetCompany("Company");
			SetAddress("", "", "", "", "", "");
			SetContact("", "", "", "");
			TestWebAddress.SaveAsNew = false;

			TestWebAddress.RunPreSaveValidation();
			Assert(TestJobDocAddress.E2_AddressOverride);

			SetCompany("");

			TestWebAddress.RunPreSaveValidation();
			Assert(TestJobDocAddress.E2_AddressOverride);

			TestWebAddress.IsOptional = true;

			TestWebAddress.RunPreSaveValidation();
			Assert("JobDocAddress should not be overriden if WebJobDocAddress is empty and optional", !TestJobDocAddress.E2_AddressOverride);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestCreateIdenticalOrganisationsForConsigneeAndGoodsBilledTo()
		{
			JobDocAddress consigneeJobDocAddress = Factory.New<JobDocAddress>();
			WebJobDocAddress consigneeWebJobDocAddress = new WebJobDocAddress(consigneeJobDocAddress);
			consigneeWebJobDocAddress.CompanyName = "Company #1";
			consigneeWebJobDocAddress.Address1 = "Company #1 consignee address";
			consigneeWebJobDocAddress.SaveAsNew = true;

			JobDocAddress goodsBilledToJobDocAddress = Factory.New<JobDocAddress>();
			WebJobDocAddress goodsBilledToWebJobDocAddress = new WebJobDocAddress(goodsBilledToJobDocAddress);
			goodsBilledToWebJobDocAddress.CompanyName = "Company #1";
			goodsBilledToWebJobDocAddress.Address1 = "Company #1 goods billed to address";
			goodsBilledToWebJobDocAddress.SaveAsNew = true;

			consigneeWebJobDocAddress.SaveAsNew = false;

			var mainAddress = goodsBilledToWebJobDocAddress.Organisation.MainAddress;

			//SaveAsNew>UpdateInternalsForSaving will delete organization, address
			AssertEquals("Organization must still contain 2 addresses because consigneeWebJobDocAddress is main address and cannot be deleted.", 2, goodsBilledToWebJobDocAddress.Organisation.Addresses.Count);
			AssertEquals("consigneeWebJobDocAddress is a main-address and cannot be deleted, its address description must be AddressNotOnFile.", OrgAddress.AddressNotOnFile, mainAddress.AddressDescription);

			goodsBilledToWebJobDocAddress.SaveAsNew = false;
		}

		#region Implementation

		void AssertSaveAsNew(bool expectedValue)
		{
			AssertEquals(expectedValue, TestWebAddress.SaveAsNew);
			if (expectedValue)
			{
				AssertNotNull(TestJobDocAddress.Organisation);
			}
		}

		void AssertOverride(bool expectedValue)
		{
			AssertEquals(expectedValue, TestJobDocAddress.E2_AddressOverride);
			AssertEquals(TestJobDocAddress.OrganisationPK, TestWebAddress.OrganisationPK);
			if (expectedValue)
			{
				AssertEquals(ZGuid.Empty, TestWebAddress.OrganisationPK);
				AssertEquals(ZGuid.Empty, TestJobDocAddress.OrganisationPK);
			}
			else
			{
				if (TestJobDocAddress.Organisation != null)
				{
					AssertNotEquals(ZGuid.Empty, TestWebAddress.OrganisationPK);
					AssertNotEquals(ZGuid.Empty, TestJobDocAddress.OrganisationPK);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebJobDocAddress(Factory.New<JobDocAddress>());
		}

		void CreateOrgHeaderAndAddressAndContact(BusinessObjectFactory factory)
		{
			OrgHeader = factory.New<OrgHeader>();
			OrgHeader.OH_FullName = "Cargowise Importers";
			OrgHeader.OH_Code = "OMGCODE";

			if (WebEnv.CurrentUser != null)
			{
				OrgSupplierBuyerLink link = factory.New<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = OrgHeader.PK;
				link.OL_OH_Supplier = ((OrgContact)WebEnv.CurrentUser).OC_OH;
			}

			OrgAddress = OrgHeader.Addresses[0];
			OrgAddress.OA_Address1 = "20 Short St";
			OrgAddress.OA_Address2 = "";
			OrgAddress.OA_PostCode = "1111";
			OrgAddress.OA_City = "Sydney";
			OrgAddress.OA_State = "NSW";
			OrgAddress.OA_RL_NKRelatedPortCode = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(factory, "AU", "Sydney", "NSW");

			OrgContact = OrgHeader.Contacts.AddNew();
			OrgContact.OC_ContactName = "John Smith";
			OrgContact.OC_Email = "john.smith@example.com";
			OrgContact.OC_Phone = "+61 (2) 1234 5678";
			OrgContact.OC_Fax = "+61 (2) 8765 4321";
		}

		void LoadBusinessObjectsInNewFactory()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestJobDocAddress = newFactory.Load<JobDocAddress>(TestJobDocAddress.PK);
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress);

			if (OrgHeader != null)
			{
				OrgHeader = newFactory.Load<OrgHeader>(OrgHeader.PK);
			}

			if (OrgAddress != null)
			{
				OrgAddress = newFactory.Load<OrgAddress>(OrgAddress.PK);
			}

			if (OrgContact != null)
			{
				OrgContact = newFactory.Load<OrgContact>(OrgContact.PK);
			}
		}

		void SetCompany(OrgHeader header)
		{
			TestWebAddress.CompanyName = header.OH_FullName;
		}

		void SetCompany(string companyName)
		{
			TestWebAddress.CompanyName = companyName;
		}

		void SetAddress(OrgAddress address)
		{
			SetAddress(address.OA_Address1, address.OA_Address2, address.OA_PostCode, address.OA_City,
				address.OA_State, address.OA_RL_NKRelatedPortCode.Substring(0, 2));
		}

		void SetAddress(string address1, string address2, string postCode, string city, string state, string countryCode)
		{
			TestWebAddress.Address1 = address1;
			TestWebAddress.Address2 = address2;
			TestWebAddress.PostCode = postCode;
			TestWebAddress.City = city;
			TestWebAddress.State = state;
			TestWebAddress.CountryCode = countryCode;
		}

		void SetContact(OrgContact contact)
		{
			SetContact(contact.OC_ContactName, contact.OC_Phone, contact.OC_Fax, contact.OC_Email);
		}

		void SetContact(string contactName, string phone, string fax, string email)
		{
			TestWebAddress.ContactName = contactName;
			TestWebAddress.Phone = phone;
			TestWebAddress.Fax = fax;
			TestWebAddress.Email = email;
		}

		void AssertCompany(OrgHeader orgHeader)
		{
			AssertCompany(orgHeader.OH_FullName);
		}

		void AssertCompany(string companyName)
		{
			AssertEquals("CompanyName", companyName, TestWebAddress.CompanyName);
		}

		void AssertAddress(OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				AssertAddress("", "", "", "", "", "");
			}
			else
			{
				AssertAddress(orgAddress.OA_Address1, orgAddress.OA_Address2, orgAddress.OA_PostCode, orgAddress.OA_City, orgAddress.OA_State, orgAddress.OA_RL_NKRelatedPortCode.Substring(0, 2));
			}
		}

		void AssertAddress(string address1, string address2, string postCode, string city, string state, string countryCode)
		{
			AssertEquals("Address1", address1, TestWebAddress.Address1);
			AssertEquals("Address2", address2, TestWebAddress.Address2);
			AssertEquals("PostCode", postCode, TestWebAddress.PostCode);
			AssertEquals("City", city, TestWebAddress.City);
			AssertEquals("State", state, TestWebAddress.State);
			AssertEquals("CountryCode", countryCode, TestWebAddress.CountryCode);
		}

		void AssertContact(OrgContact orgContact)
		{
			if (orgContact == null)
			{
				AssertContact("", "", "", "");
			}
			else
			{
				AssertContact(orgContact.OC_ContactName, orgContact.OC_Phone, orgContact.OC_Fax, orgContact.OC_Email);
			}
		}

		void AssertContact(string contactName, string phone, string fax, string email)
		{
			AssertEquals("ContactName ", contactName, TestWebAddress.ContactName);
			AssertEquals("Phone ", phone, TestWebAddress.Phone);
			AssertEquals("Fax ", fax, TestWebAddress.Fax);
			AssertEquals("Email ", email, TestWebAddress.Email);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestWebAddress = new WebJobDocAddress(TestJobDocAddress);
		}

		JobDocAddress TestJobDocAddress;
		WebJobDocAddress TestWebAddress;

		OrgHeader OrgHeader;
		OrgAddress OrgAddress;
		OrgContact OrgContact;

#endregion
	}
}
