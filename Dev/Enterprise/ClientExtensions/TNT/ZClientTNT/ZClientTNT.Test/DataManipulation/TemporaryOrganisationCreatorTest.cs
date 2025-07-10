using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.Testing
{
	public class TemporaryOrganisationCreatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNotNull("Creator should not be null", Creator);
		}

		#region TestCreate
		public void TestCreate_ConsignorCodeForNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			TNTOrganisation tNTOrg = new TNTOrganisation();
			tNTOrg.CompanyName = "TEST COMP";
			tNTOrg.LegacyCode = "12345678";
			Buffer.Clear();
			OrgHeader newOrg = Creator.Create(tNTOrg, Buffer);
			AssertNotNull(newOrg);
			AssertNotEquals(newOrg.OH_Code, "12345678");
		}

		public void TestCreate_MissingFields()
		{
			int orgHeaderCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Buffer.Clear();
			OrgHeader newOrg = Creator.Create(new TNTOrganisation(), Buffer);
			AssertNull("No new OrgHeader created for Empty TNTOrg", newOrg);
			TNTOrg.LegacyCode = "";
			TNTOrg.Address1 = "";
			TNTOrg.Address2 = "";
			TNTOrg.Country = "";
			newOrg = Creator.Create(TNTOrg, Buffer);
			Factory.Save();
			AssertEquals("1 new OrgHeader was created", orgHeaderCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader)));
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertNotNull("NewOrg should not be null", newOrg);
			AssertEquals("Company Name", TNTOrg.CompanyName, newOrg.OH_FullName);
			AssertEquals("NewOrg is a Temp Account", true, newOrg.OH_IsTempAccount);
			AssertEquals("Closest Port", TemporaryOrganisationCreator.DefaultEmptyPortCode, newOrg.OH_RL_NKClosestPort);
			AssertEquals("Main Address 1", TemporaryOrganisationCreator.DefaultEmptyAddress, newOrg.MainAddress.OA_Address1);
			AssertEquals("Main Address 2", "", newOrg.MainAddress.OA_Address2);
			AssertEquals("Main City", TNTOrg.City, newOrg.MainAddress.OA_City);
			AssertEquals("Main State", TNTOrg.State, newOrg.MainAddress.OA_State);
			AssertEquals("Main PostCode", TNTOrg.PostCode, newOrg.MainAddress.OA_PostCode);
			AssertEquals("Main Phone", TNTOrg.Phone, newOrg.MainAddress.OA_Phone);
			AssertEquals("Main Fax", TNTOrg.Fax, newOrg.MainAddress.OA_Fax);
			AssertEquals("Main Email", TNTOrg.Email, newOrg.MainAddress.OA_Email);
			AssertEquals("Code", "TESNAM", newOrg.OH_Code);
			ZQuery legacyFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			legacyFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, TNTOrg.LegacyCode);
			AssertEquals("New Temporary Org should not have a legacy code", 0, newOrg.CustomsCodes.Find(legacyFilter).Length);
		}

		public void TestCreate()
		{
			int orgHeaderCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Buffer.Clear();
			OrgHeader newOrg = Creator.Create(new TNTOrganisation(), Buffer);
			AssertNull("No new OrgHeader created for Empty TNTOrg", newOrg);
			newOrg = Creator.Create(TNTOrg, Buffer);
			Factory.Save();
			AssertEquals("1 new OrgHeader was created", orgHeaderCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader)));
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertOrganisatioHasTNTOrgTempData(newOrg);
		}

		#endregion
		#region TestAddContact
		public void TestAddContact()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "BOB SMITH COMPANY";
			Buffer.Clear();
			OrgContact newContact = Creator.AddContact(null, "BOB SMITH", "02 23212 123", Buffer);
			AssertNull("NewContact should be null as Organisation was null", newContact);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			newContact = Creator.AddContact(organisation, "", "02 23212 123", Buffer);
			AssertNull("NewContact should be null as Contact Name was empty", newContact);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("PreCondition: Organisation has no contact", 0, organisation.Contacts.Count);
			newContact = Creator.AddContact(organisation, "BOB SMITH", "02 23212 123", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertNotNull("NewContact should not be null", newContact);
			AssertEquals("Organisation should have 1 contact", 1, organisation.Contacts.Count);
			AssertEquals("NewContact should belong to Organisation", newContact.PK, organisation.Contacts[0].PK);
			AssertContact(newContact, "BOB SMITH", "02 23212 123", Core.Constants.ContactNotifyModes.Print);
			// Check Contact Phone is updated for existing contact
			newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			newContact = Creator.AddContact(organisation, "BOB SMITH", "02 5456 4565", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertNotNull("NewContact should not be null", newContact);
			AssertEquals("Organisation should still have 1 contact", 1, organisation.Contacts.Count);
			AssertEquals("NewContact should belong to Organisation", newContact.PK, organisation.Contacts[0].PK);
			AssertContact(newContact, "BOB SMITH", "02 5456 4565", Core.Constants.ContactNotifyModes.Email);
			// Check Notify Mode not changed for an update
			newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			newContact = Creator.AddContact(organisation, "BOB SMITH", "", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertNotNull("NewContact should not be null", newContact);
			AssertEquals("Organisation should still have 1 contact", 1, organisation.Contacts.Count);
			AssertEquals("NewContact should belong to Organisation", newContact.PK, organisation.Contacts[0].PK);
			AssertContact(newContact, "BOB SMITH", "02 5456 4565", Core.Constants.ContactNotifyModes.Email);
			// Check New Contact added for differnt Contact Name
			newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			newContact = Creator.AddContact(organisation, "JANE SMITH", "02 5456 4565", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertNotNull("NewContact should not be null", newContact);
			AssertEquals("Organisation should still have 2 contact", 2, organisation.Contacts.Count);
			AssertNotNull("NewContact should belong to Organisation", organisation.Contacts.FindByPK(newContact.PK));
			AssertContact(newContact, "JANE SMITH", "02 5456 4565", Core.Constants.ContactNotifyModes.Print);
		}

		#endregion
		#region TestCreateConsignor
		public void TestCreateConsignor()
		{
			ZQuery consignorFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
			int consignorCount = Factory.GetDatabaseCount(typeof(OrgHeader), consignorFilter);
			Buffer.Clear();
			OrgHeader newOrg = Creator.CreateConsignor(new TNTOrganisation(), Buffer);
			AssertNull("No new Consignor OrgHeader created for Empty TNTOrg", newOrg);
			newOrg = Creator.CreateConsignor(TNTOrg, Buffer);
			Factory.Save();
			AssertEquals("1 new Consignor was created", consignorCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), consignorFilter));
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertOrganisatioHasTNTOrgTempData(newOrg);
			AssertEquals("NewOrg is a Consignor", true, newOrg.OH_IsConsignor);
			AssertEquals("NewOrg should have 1 contact", 1, newOrg.Contacts.Count);
			OrgContact contact = newOrg.Contacts[0];
			AssertContact(contact, TNTOrg.ContactName, TNTOrg.ContactPhone, Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignor.Code, 1);
		}

		#endregion
		#region TestAddConsignorContact
		public void TestAddConsignorContact()
		{
			Buffer.Clear();
			OrgHeader newOrg = Factory.New<OrgHeader>();
			Creator.AddConsignorContact(newOrg, "BOB SMITH", "02 9342 3423", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("NewOrg should have 1 contact", 1, newOrg.Contacts.Count);
			OrgContact contact = newOrg.Contacts[0];
			AssertContact(contact, "BOB SMITH", "02 9342 3423", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignor.Code, 1);
			Creator.AddConsignorContact(newOrg, "BOB SMITH", "02 9452 5843", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("Existing Contact should have been matched", 1, newOrg.Contacts.Count);
			contact = newOrg.Contacts[0];
			AssertContact(contact, "BOB SMITH", "02 9452 5843", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignor.Code, 1);
			Creator.AddConsignorContact(newOrg, "JAMES SMITH", "02 9452 5844", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("New Contact should have been created", 2, newOrg.Contacts.Count);
			OrgContact[] contactMatched = (OrgContact[])newOrg.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, "JAMES SMITH"));
			AssertEquals("There should be only one contact with the name 'JAMES SMITH'", 1, contactMatched.Length);
			contact = contactMatched[0];
			AssertContact(contact, "JAMES SMITH", "02 9452 5844", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignor.Code, 1);
		}

		#endregion
		#region TestCreateConsignee
		public void TestCreateConsignee()
		{
			ZQuery consigneeFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
			int consigneeCount = Factory.GetDatabaseCount(typeof(OrgHeader), consigneeFilter);
			Buffer.Clear();
			OrgHeader newOrg = Creator.CreateConsignee(new TNTOrganisation(), Buffer);
			AssertNull("No new Consignee OrgHeader created for Empty TNTOrg", newOrg);
			newOrg = Creator.CreateConsignee(TNTOrg, Buffer);
			Factory.Save();
			AssertEquals("1 new Consignee was created", consigneeCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), consigneeFilter));
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertOrganisatioHasTNTOrgTempData(newOrg);
			AssertEquals("NewOrg is a Consignee", true, newOrg.OH_IsConsignee);
			AssertEquals("NewOrg should have 1 contact", 1, newOrg.Contacts.Count);
			OrgContact contact = newOrg.Contacts[0];
			AssertContact(contact, TNTOrg.ContactName, TNTOrg.ContactPhone, Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignee.Code, 1);
		}

		#endregion
		#region TestAddConsigneeContact
		public void TestAddConsigneeContact()
		{
			Buffer.Clear();
			OrgHeader newOrg = Factory.New<OrgHeader>();
			Creator.AddConsigneeContact(newOrg, "BOB SMITH", "02 9342 3423", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("NewOrg should have 1 contact", 1, newOrg.Contacts.Count);
			OrgContact contact = newOrg.Contacts[0];
			AssertContact(contact, "BOB SMITH", "02 9342 3423", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignee.Code, 1);
			Creator.AddConsigneeContact(newOrg, "BOB SMITH", "02 9452 5843", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("Existing Contact should have been matched", 1, newOrg.Contacts.Count);
			contact = newOrg.Contacts[0];
			AssertContact(contact, "BOB SMITH", "02 9452 5843", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignee.Code, 1);
			Creator.AddConsigneeContact(newOrg, "JAMES SMITH", "02 9452 5844", Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("New Contact should have been created", 2, newOrg.Contacts.Count);
			OrgContact[] contactMatched = (OrgContact[])newOrg.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, "JAMES SMITH"));
			AssertEquals("There should be only one contact with the name 'JAMES SMITH'", 1, contactMatched.Length);
			contact = contactMatched[0];
			AssertContact(contact, "JAMES SMITH", "02 9452 5844", Core.Constants.ContactNotifyModes.Print);
			AssertContactHasDocumentGroup(contact, ContactType.Consignee.Code, 1);
		}

		#endregion
		#region TestUpdateOrgAddress
		public void TestUpdateOrgAddress()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			Buffer.Clear();
			Creator.UpdateOrgAddress(address, TNTOrg, Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("CompanyName", TNTOrg.CompanyName, address.OA_CompanyNameOverride);
			AssertEquals("Address1", TNTOrg.Address1, address.OA_Address1);
			AssertEquals("Address2", TNTOrg.Address2, address.OA_Address2);
			AssertEquals("City", TNTOrg.City, address.OA_City);
			AssertEquals("PostCode", TNTOrg.PostCode, address.OA_PostCode);
			AssertEquals("Related PortCode", AUSYDLocode.Code, address.OA_RL_NKRelatedPortCode);
			AssertEquals("State", TNTOrg.State, address.OA_State);
			AssertEquals("Phone", TNTOrg.Phone, address.OA_Phone);
			AssertEquals("Fax", TNTOrg.Fax, address.OA_Fax);
			AssertEquals("Email", TNTOrg.Email, address.OA_Email);
			AssertEquals("Language", TNTOrg.Language, address.OA_Language);
		}

		public void TestUpdateOrgAddress_EmptyTNTOrg()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			Buffer.Clear();
			Creator.UpdateOrgAddress(address, new TNTOrganisation(), Buffer);
			AssertEquals("Buffer should have no errors; Buffer contain:" + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals("CompanyName", "", address.OA_CompanyNameOverride);
			AssertEquals("Address1", "", address.OA_Address1);
			AssertEquals("Address2", "", address.OA_Address2);
			AssertEquals("City", "", address.OA_City);
			AssertEquals("PostCode", "", address.OA_PostCode);
			AssertEquals("Related PortCode", "", address.OA_RL_NKRelatedPortCode);
			AssertEquals("State", "", address.OA_State);
			AssertEquals("Phone", "", address.OA_Phone);
			AssertEquals("Fax", "", address.OA_Fax);
			AssertEquals("Email", "", address.OA_Email);
			AssertEquals("Language", Core.Constants.Languages.English, address.OA_Language);
		}

		#endregion
		#region TestGetPortCode
		public void TestGetPortCode()
		{
			ZString portCode = Creator.GetPortCode(AUSYDLocode.RL_RN_NKCountryCode + "   ", AUSYDLocode.CountryStates.RW_Code + "    ", AUSYDLocode.RL_PortName + "   ");
			AssertEquals("Port should have been matched", AUSYDLocode.Code, portCode);
			portCode = Creator.GetPortCode(AUSYDLocode.RL_RN_NKCountryCode, AUSYDLocode.CountryStates.RW_Code, "BLAH PORTNAME");
			AssertEquals("Port should have been matched base on counry and state", AUSYDLocode.Code, portCode);
			portCode = Creator.GetPortCode(AUSYDLocode.RL_RN_NKCountryCode, "BLAH STATE", AUSYDLocode.RL_PortName);
			AssertEquals("Port should have been matched base on counry and port name", AUSYDLocode.Code, portCode);
			portCode = Creator.GetPortCode("BLAH COUNTRY", "BLAH STATE", "BLAH PORTNAME");
			AssertEquals("No Port Matched, use Country first 2 character", "BLZZZ", portCode);
			portCode = Creator.GetPortCode(AUSYDLocode.RL_RN_NKCountryCode, "BLAH STATE", "BLAH PORTNAME");
			AssertEquals("No Port Matched, use Country first 2 character", AUSYDLocode.RL_RN_NKCountryCode + "ZZZ", portCode);
			portCode = Creator.GetPortCode("", "BLAH STATE", "BLAH PORTNAME");
			AssertEquals("No Port Matched use default '" + TemporaryOrganisationCreator.DefaultEmptyPortCode + "'", TemporaryOrganisationCreator.DefaultEmptyPortCode, portCode);
		}

		#endregion
		#region TestGetPortFromNameAndCountryCode
		public void TestGetPortFromNameAndCountryCode()
		{
			ZString portCode = Creator.GetPortFromNameAndCountryCode(AUSYDLocode.RL_PortName, AUSYDLocode.RL_RN_NKCountryCode);
			AssertEquals("Port code", AUSYDLocode.Code, portCode);
			portCode = Creator.GetPortFromNameAndCountryCode("BLAH PORTNAME", "BLAH COUNTRY");
			AssertEquals("Port code", TemporaryOrganisationCreator.DefaultEmptyPortCode, portCode);
		}

		#endregion
		#region Implementation
		#region Creator
		TemporaryOrganisationCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new TemporaryOrganisationCreator(Factory);
				}

				return fCreator;
			}
		}

		TemporaryOrganisationCreator fCreator;
		#endregion
		#region Buffer
		NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}

				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;
		#endregion
		#region AUSYDLocode
		RefUNLOCO AUSYDLocode
		{
			get
			{
				if (fAUSYDLocode == null)
				{
					fAUSYDLocode = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				}

				return fAUSYDLocode;
			}
		}

		RefUNLOCO fAUSYDLocode;
		#endregion
		void AssertOrganisatioHasTNTOrgTempData(OrgHeader newOrg)
		{
			AssertNotNull("NewOrg should not be null", newOrg);
			AssertEquals("Company Name", TNTOrg.CompanyName, newOrg.OH_FullName);
			AssertEquals("NewOrg is a Temp Account", true, newOrg.OH_IsTempAccount);
			AssertEquals("Closest Port", AUSYDLocode.Code, newOrg.OH_RL_NKClosestPort);
			AssertEquals("Main Address 1", TNTOrg.Address1, newOrg.MainAddress.OA_Address1);
			AssertEquals("Main Address 2", TNTOrg.Address2, newOrg.MainAddress.OA_Address2);
			AssertEquals("Main City", TNTOrg.City, newOrg.MainAddress.OA_City);
			AssertEquals("Main State", TNTOrg.State, newOrg.MainAddress.OA_State);
			AssertEquals("Main PostCode", TNTOrg.PostCode, newOrg.MainAddress.OA_PostCode);
			AssertEquals("Main Phone", TNTOrg.Phone, newOrg.MainAddress.OA_Phone);
			AssertEquals("Main Fax", TNTOrg.Fax, newOrg.MainAddress.OA_Fax);
			AssertEquals("Main Email", TNTOrg.Email, newOrg.MainAddress.OA_Email);
			AssertEquals("Code", "TESNAMSYD", newOrg.OH_Code);
			ZQuery legacyFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			legacyFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, TNTOrg.LegacyCode);
			AssertEquals("New Temporary Org should not have a legacy code", 0, newOrg.CustomsCodes.Find(legacyFilter).Length);
		}

		void AssertContact(OrgContact contact, ZString name, ZString phone, ZString notifyMode)
		{
			AssertNotNull("Contact should not be null", contact);
			AssertEquals("Contact Name", name, contact.OC_ContactName);
			AssertEquals("Contact Phone", phone, contact.OC_Phone);
			AssertEquals("Contact Notify Mode", notifyMode, contact.OC_NotifyMode);
		}

		void AssertContactHasDocumentGroup(OrgContact contact, ZString documentGroup, int expectedNoOfMatched)
		{
			OrgDocument[] matchedOrgDoc = (OrgDocument[])contact.Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, documentGroup));
			AssertEquals(string.Format("Contact should have {0} number of Document Group '{1}'", expectedNoOfMatched, documentGroup), expectedNoOfMatched, matchedOrgDoc.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TNTOrg = CreateTNTOrg();
		}

		TNTOrganisation CreateTNTOrg()
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "LEGACYCODE";
			result.CompanyName = "TEST COMPANY NAME";
			result.Address1 = "TEST ADDRESS 1";
			result.Address2 = "TEST ADDRESS 2";
			result.City = AUSYDLocode.RL_PortName;
			result.PostCode = "2000";
			result.State = AUSYDLocode.CountryStates.RW_Code;
			result.Country = AUSYDLocode.RL_RN_NKCountryCode;
			result.Phone = "02 9000 2222";
			result.Fax = "02 9000 2223";
			result.Email = "email@email.com";
			result.Language = Core.Constants.Languages.English;
			result.ContactName = "BOB SMITH";
			result.ContactPhone = "02 9000 2224";
			return result;
		}

		TNTOrganisation TNTOrg;
		#endregion
	}
}
