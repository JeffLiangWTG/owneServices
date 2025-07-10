using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(ServiceRequestDataAdapterForTest))]
	public class ServiceRequestDataAdapterTest : ValueObjectDataAdapterTest<SupportIncident, Xsd.CustomerServiceRequest>
	{
		public void TestImportFromValueObjectCore()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "1 2 3 4 5";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Zubin Appoo";
			request.Product = "";
			request.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			request.ActiveModuleId = ModuleIDs.ArchivedRecords.ID.ToString();
			request.Criticality = "CR3";

			Xsd.OrgContact staffItem = request.Staff.AddNew();
			staffItem.Name = "Zubin Appoo";
			staffItem.HomePhone = "** View DENIED";
			staffItem.Mobile = "** view denied";
			staffItem.Phone = "** VIEW DENIED";
			staffItem.EmailAddress = "** VIEW DEnIED";

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("CL111111", incident.IM_ClientIncidentReference);
			AssertEquals("My Incident", incident.IM_Description);
			AssertEquals("1 2 3 4 5", incident.DetailNoteText);
			AssertEquals("ENT", incident.IM_Product);
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager, incident.IM_Module);
			AssertEquals(ModuleIDs.ArchivedRecords.ID.ToString(), incident.IM_SourceModuleId);
			AssertEquals("CR3", incident.IM_Priority);

			AssertEquals("There is 1 contact must be added", 1, Org.Contacts.Count);
			AssertEquals("Zubin Appoo", Org.Contacts[0].OC_ContactName);
			AssertEquals("Must be empty field", ZString.Empty, Org.Contacts[0].OC_HomePhone);
			AssertEquals("Must be empty field", ZString.Empty, Org.Contacts[0].OC_Phone);
			AssertEquals("Must be empty field", ZString.Empty, Org.Contacts[0].OC_Mobile);
			AssertEquals("Must be empty field", ZString.Empty, Org.Contacts[0].OC_Email);

			request.Product = "SPH";
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("SPH", incident.IM_Product);
		}

		public void TestImportFromValueObjectCore_StaffNameLongerThanContactName()
		{
			var nameTooLong1 = new ZString('A', OrgContactSchema.OC_ContactName.MaxLength + 1);
			var nameTooLong2 = new ZString('B', OrgContactSchema.OC_ContactName.MaxLength + 1);

			Org.Contacts.AddNew().OC_ContactName = nameTooLong2.Substring(0, OrgContactSchema.OC_ContactName.MaxLength);

			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "1 2 3 4 5";
			request.ReportingStaffMemberName = nameTooLong1;
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = nameTooLong1;
			request.Product = "";
			request.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			request.ActiveModuleId = ModuleIDs.ArchivedRecords.ID.ToString();
			request.Criticality = "CR3";

			Xsd.OrgContact staffItem1 = request.Staff.AddNew();
			staffItem1.Name = nameTooLong1;

			Xsd.OrgContact staffItem2 = request.Staff.AddNew();
			staffItem2.Name = nameTooLong2;

			SupportIncident incident = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			Factory.Save();

			AssertEquals("contacts added", 2, Org.Contacts.Count);
			AssertEquals("name is truncated", OrgContactSchema.OC_ContactName.MaxLength, Org.Contacts[0].OC_ContactName.Length);
			AssertEquals("name is truncated", OrgContactSchema.OC_ContactName.MaxLength, Org.Contacts[1].OC_ContactName.Length);

			request.Product = "SPH";
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
		}

		public void TestImportFromValueObjectCore_WithoutActiveModuleID()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			request.Criticality = "CR4";
			request.Module = "COR";
			request.ActiveModuleId = "";

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("COR", incident.IM_SourceModuleId);
		}

		public void TestImportFromValueObjectCore_WithNonMenuSectionModuleType()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			request.Criticality = "CR9";
			request.Module = "CEC";
			request.ActiveModuleId = ModuleIDs.ArchivedRecords.ID.ToString();

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(IncidentApproval.NotAvailableActiveModuleID, incident.IM_SourceModuleId);
		}

		public void TestImportFromValueObjectCore_EmailOverName()
		{
			OrgContact existingContact = Org.Contacts.AddNew();
			existingContact.OC_ContactName = "Zubin";
			existingContact.OC_Title = "Jr. MEH";
			existingContact.OC_Email = "someone@example.com";

			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Zubin Appoo";

			Xsd.OrgContact staffItem = request.Staff.AddNew();
			staffItem.Name = "Zubin Appoo";
			staffItem.JobTitle = "MEH";
			staffItem.EmailAddress = "someone@example.com";

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("Correct contact on incident", existingContact.PK, incident.Contact.PK);
			AssertEquals("Still only 1 contact on the org", 1, Org.Contacts.Count);
			AssertEquals("Still only 1 contact on the org - Zubin", "Zubin", Org.Contacts[0].OC_ContactName);
			AssertEquals("Should be updated", "MEH", Org.Contacts[0].OC_Title);
		}

		public void TestImportFromValueObject_IncidentContactFromClientLicenceOrg()
		{
			TestImportFromValueObjectCore_MatchToTheCorrectContact(Org);
		}

		public void TestImportFromValueObject_IncidentContactFromEnterpriseOrg()
		{
			Licence.Company.LicEnterprise.LE_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestImportFromValueObjectCore_MatchToTheCorrectContact(Licence.Company.LicEnterprise.Header);
		}

		public void TestImportFromValueObject_IncidentContactFromOtherClientLicenceOrg()
		{
			LicenceCompany newCompany = Licence.Company.LicEnterprise.Companies.AddNew();
			newCompany.FillWithValidTestData();
			newCompany.LC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestImportFromValueObjectCore_MatchToTheCorrectContact(newCompany.Header);
		}

		void TestImportFromValueObjectCore_MatchToTheCorrectContact(OrgHeader organisation)
		{
			OrgContact contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Jessica";
			contact1.OC_Email = "jessica.biel@cargowise.com";
			contact1.IsCustomerServiceContact = true;
			OrgContact contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "Jessica O";
			contact2.OC_Email = "jessica.one@cargowise.com";

			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.ApprovingUser = "jessica.biel@cargowise.com";

			AssertNull("Precondition", incident.Contact);
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("contact 1", contact1.PK, incident.Contact.PK);

			request.ApprovingUser = "Jessica";
			incident.IM_OC_Contact = ZGuid.Empty;
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("contact 1", contact1.PK, incident.Contact.PK);

			request.ApprovingUser = "jessica.one@cargowise.com";
			incident.IM_OC_Contact = ZGuid.Empty;
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("contact 2", contact2.PK, incident.Contact.PK);
		}

		public void TestImportFromValueObject_IncidentContactByNameAndEmail()
		{
			OrgContact contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "Jessica";
			contact1.OC_Email = "jessica.biel@cargowise.com";

			OrgContact contact2 = Org.Contacts.AddNew();
			contact2.OC_ContactName = "Jessica O";
			contact2.OC_Email = "jessica.one@cargowise.com";

			OrgContact contact3 = Org.Contacts.AddNew();
			contact3.OC_ContactName = "Jessica (1)";
			contact3.OC_Email = "jessica.two@cargowise.com";

			var request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;

			var jessica1 = request.Staff.AddNew();
			jessica1.Name = "Jessica";
			jessica1.EmailAddress = "jessica.one@cargowise.com";

			var jessica2 = request.Staff.AddNew();
			jessica2.Name = "Jessica";
			jessica2.EmailAddress = "jessica.two@cargowise.com";

			var jessica3 = request.Staff.AddNew();
			jessica3.Name = "Jessica";
			jessica3.EmailAddress = "jessica.biel@cargowise.com";

			SupportIncident incident = Factory.New<SupportIncident>();
			AssertNull("Precondition", incident.Contact);

			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();

			request.ApprovingUser = "jessica.biel@cargowise.com";
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Only contains email", contact1.PK, incident.Contact.PK);

			incident.IM_OC_Contact = ZGuid.Empty;
			request.ApprovingUser = "Jessica";
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Only contains name", contact2.PK, incident.Contact.PK);

			incident.IM_OC_Contact = ZGuid.Empty;
			request.ApprovingUserEmail = "jessica.two@cargowise.com";
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Contains name and email", contact3.PK, incident.Contact.PK);
		}

		public void TestImportFromValueObject_IncidentContactShouldBeActive()
		{
			var existingContact = Org.Contacts.AddNew();
			existingContact.OC_ContactName = "Contact Name";
			existingContact.OC_Email = "contact.existing@test.com";
			existingContact.IsCustomerServiceContact = true;
			existingContact.OC_SystemCreateUser = User.ServiceUserCode;

			var request1 = new Xsd.CustomerServiceRequest();
			request1.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request1.Action = "ADD";
			request1.ApprovingUser = "Contact Name";
			request1.ApprovingUserEmail = "contact.new.email@test.com";
			var staff1 = request1.Staff.AddNew();
			staff1.Name = "Contact Name";
			staff1.EmailAddress = "contact.new.email@test.com";

			var incident = Factory.New<SupportIncident>();
			var adapter = new ServiceRequestDataAdapterForTest();
			adapter.ImportFromValueObjectCoreForTest(incident, request1, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			Assert(incident.Contact.OC_IsActive);
			AssertNotEquals(existingContact.PK, incident.Contact.PK);
			AssertEquals("contact.new.email@test.com", incident.Contact.OC_Email);

			existingContact.OC_IsActive = false;

			var request2 = new Xsd.CustomerServiceRequest();
			request2.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request2.Action = "ADD";
			request2.ApprovingUser = "Contact Name";
			request2.ApprovingUserEmail = "contact.existing@test.com";
			var staff2 = request2.Staff.AddNew();
			staff2.Name = "Contact Name";
			staff2.EmailAddress = "contact.existing@test.com";

			incident = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident, request2, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertNotNull(incident.Contact);
			Assert(incident.Contact.OC_IsActive);
			AssertEquals("contact.existing@test.com", incident.Contact.OC_Email);
			Assert(existingContact.OC_IsActive);
		}

		public void TestImportFromValueObject_DoNotImportOtherContacts()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.ApprovingUser = "jessica.biel@cargowise.com";

			Xsd.OrgContact otherContact1 = request.Staff.AddNew();
			otherContact1.Name = "Koala 1";
			otherContact1.EmailAddress = "scrumMaster@koala.cargowise.com";
			Xsd.OrgContact otherContact2 = request.Staff.AddNew();
			otherContact2.Name = "Koala 2";
			otherContact2.EmailAddress = "productOwner@koala.cargowise.com";
			Xsd.OrgContact otherContact3 = request.Staff.AddNew();
			otherContact3.Name = "Koala 3";
			otherContact3.EmailAddress = "pizzaOrderer@koala.cargowise.com";
			Xsd.OrgContact approvingContact = request.Staff.AddNew();
			approvingContact.Name = "Koala 4";
			approvingContact.EmailAddress = "jessica.biel@cargowise.com";

			AssertNull("Precondition", incident.Contact);
			AssertEquals("Precondition", 0, Org.Contacts.Count);

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			Org.Contacts.Reload(false);
			AssertEquals(1, Org.Contacts.Count);
			AssertEquals("Koala 4", Org.Contacts[0].OC_ContactName);
			AssertEquals("jessica.biel@cargowise.com", Org.Contacts[0].OC_Email);
			AssertEquals(Org.Contacts[0], incident.Contact);
		}

		public void TestImportFromValueObject_DoNotDeactivateSystemGeneratedContactsOnNonProductionLicenceDatabase()
		{
			OrgContact contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "Koala 1";
			contact1.OC_Email = "koala1@cargowise.com";
			contact1.OC_IsActive = true;
			contact1.OC_SystemCreateUser = User.ServiceUserCode;

			OrgContact contact2 = Org.Contacts.AddNew();
			contact2.OC_ContactName = "Koala 2";
			contact2.OC_Email = "koala2@cargowise.com";
			contact2.OC_IsActive = true;
			contact2.OC_SystemCreateUser = User.ServiceUserCode;

			Licence.Database.LD_LicenceType = DatabaseTypes.Codes.Training;
			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.ApprovingUser = "samuel.wang@cargowise.com";

			Xsd.OrgContact approvingContact = request.Staff.AddNew();
			approvingContact.Name = "Koala 3";
			approvingContact.EmailAddress = "samuel.wang@cargowise.com";

			AssertNull("Precondition", incident.Contact);
			AssertEquals("Precondition", 2, Org.Contacts.Count);

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			Org.Contacts.Reload(false);
			IEnumerable<OrgContact> contactList = Org.Contacts.Cast<OrgContact>();
			AssertEquals(3, contactList.Count());
			AssertEquals(1, contactList.Count(c => c.OC_Email == "koala1@cargowise.com" && c.OC_IsActive));
			AssertEquals(1, contactList.Count(c => c.OC_Email == "koala2@cargowise.com" && c.OC_IsActive));
			AssertEquals(1, contactList.Count(c => c.OC_Email == "samuel.wang@cargowise.com" && c.OC_ContactName == "Koala 3"));
		}

		public void TestImportFromValueObject_InvalidValues()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.ApprovingUser = "zed@cargowise.com";
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			Xsd.OrgContact approvingContact = request.Staff.AddNew();
			approvingContact.Name = "Zed 3";
			approvingContact.EmailAddress = "zed@cargowise.com";
			approvingContact.Phone = "123.456";
			approvingContact.Fax = "ABC";

			AssertNull("Precondition", incident.Contact);
			AssertEquals("Precondition", 0, Org.Contacts.Count);

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			OrgContact contact = Org.Contacts[0];
			AssertEquals("", contact.OC_Phone);
			AssertEquals("", contact.OC_Fax);
		}

		public void TestImportFromValueObject_FromClientCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDAAAMEL";
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_ServerCode = "PRD";
			database1.LD_LE = enterprise.PK;
			database1.LD_DatabaseNumber = 1024;

			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LicenceType = DatabaseTypes.Codes.Production;
			database2.LD_ServerCode = "DB2";
			database2.LD_LE = enterprise.PK;
			database2.LD_DatabaseNumber = 1200;

			var company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_CompanyCode = "AAA";
			company.LC_CompanyCountry = "AU";
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			var licence = Factory.New<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database1.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_OH = org.PK;
			clientCompany1.LCC_LD = database1.PK;
			clientCompany1.LCC_Code = "TLA";
			clientCompany1.LCC_DeactivateTimeUtc = ZDateTime.Today.AddMonths(-6);

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = database1.PK;
			clientCompany2.LCC_Code = "AAA";
			clientCompany2.LCC_RN_NKCountryCode = "AU";

			Factory.Save();

			var adapter = new ServiceRequestDataAdapterForTest();

			#region Request 1

			var request1 = new Xsd.CustomerServiceRequest();
			request1.Action = SupportIncidentLookups.LegacyActions.Add;
			request1.LicenceCode = "DDDSYDPRD";
			request1.CompanyCode = "SYD";
			request1.DatabaseNumber = 1024;
			request1.IncidentSummary = "My Incident";
			request1.IncidentDetails = "1 2 3 4 5";
			request1.ReportingStaffMemberName = "Sam";
			request1.ClientReferenceNumber = "SR00002001";
			request1.Product = "ENT";
			request1.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			request1.ActiveModuleId = ModuleIDs.GlbCompanyCampaign.ID.ToString();
			request1.Criticality = "CR3";
			request1.ApprovingUser = "Sam";
			request1.ApprovingUserEmail = "Sam@Test.com";

			var staffItem1 = request1.Staff.AddNew();
			staffItem1.Name = "Sam";
			staffItem1.EmailAddress = "sam@test.com";
			var staffItem2 = request1.Staff.AddNew();
			staffItem2.Name = "User One";
			staffItem2.EmailAddress = "user.one@test.com";

			var incident1 = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident1, request1, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("SR00002001", incident1.IM_ClientIncidentReference);
			AssertEquals("My Incident", incident1.IM_Description);
			AssertEquals("1 2 3 4 5", incident1.DetailNoteText);
			AssertEquals("ENT", incident1.IM_Product);
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing, incident1.IM_Module);
			AssertEquals(ModuleIDs.GlbCompanyCampaign.ID.ToString(), incident1.IM_SourceModuleId);
			AssertEquals("CR3", incident1.IM_Priority);
			AssertEquals(org.PK, incident1.IM_OH_Client);
			AssertEquals("Sam", incident1.Contact.OC_ContactName);
			AssertEquals(database1.PK, incident1.IM_LD);

			AssertEquals("Contact is imported", 1, org.Contacts.Count);
			AssertEquals("Sam", org.Contacts[0].OC_ContactName);
			AssertEquals("sam@test.com", org.Contacts[0].OC_Email);

			#endregion

			#region Request 2

			var request2 = new Xsd.CustomerServiceRequest();
			request2.Action = SupportIncidentLookups.LegacyActions.Add;
			request2.LicenceCode = "DDDCOMPRD";
			request2.DatabaseNumber = 1200;
			request2.CompanyCode = "COM";
			request2.IncidentSummary = "Another Incident";
			request2.IncidentDetails = "1 2 3 4 5";
			request2.ReportingStaffMemberName = "Tester";
			request2.ClientReferenceNumber = "SR00001001";
			request2.Product = "ENT";
			request2.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.System;
			request2.ActiveModuleId = ModuleIDs.Organisation.ID.ToString();
			request2.Criticality = "CR5";
			request2.ApprovingUser = "Tester";
			request2.ApprovingUserEmail = "Sam@Test.com";

			var staffItem3 = request2.Staff.AddNew();
			staffItem3.Name = "Tester";
			staffItem3.EmailAddress = "sam@test.com";
			var staffItem4 = request2.Staff.AddNew();
			staffItem4.Name = "User Two";
			staffItem4.EmailAddress = "user.two@test.com";

			var incident2 = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident2, request2, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(database2.PK, incident2.IM_LD);
			AssertEquals("Other contacts are not imported", 1, org.Contacts.Count);

			#endregion

			#region Request 3

			var request3 = new Xsd.CustomerServiceRequest();
			request3.Action = SupportIncidentLookups.LegacyActions.Add;
			request3.LicenceCode = "DDDAAAPRD";
			request3.DatabaseNumber = 1024;
			request3.CompanyCode = "AAA";
			request3.IncidentSummary = "Third Incident";
			request3.IncidentDetails = "1 2 3 4 5";
			request3.ReportingStaffMemberName = "Tester";
			request3.ClientReferenceNumber = "SR00001003";
			request3.Product = "ENT";
			request3.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.System;
			request3.ActiveModuleId = ModuleIDs.Organisation.ID.ToString();
			request3.Criticality = "CR5";
			request3.ApprovingUser = "Tester";
			request3.ApprovingUserEmail = "Sam@Test.com";

			var incident3 = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident3, request3, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("database", database1.PK, incident3.IM_LD);
			AssertEquals("clientCompany", clientCompany2.PK, incident3.IM_LCC);

			#endregion
		}

		public void TestImportFromValueObjectCore_Language()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Mei";
			contact.OC_Email = "mei@overwatch.com";
			contact.OC_Language = Core.SharedConstants.Languages.English;

			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;

			request.ReportingStaffMemberName = "Juergen Klinsmann M.E.H";
			request.ReportingStaffEmail = "juergen.klinsmann@cargowise.com";

			request.ApprovingUser = "Mei-Ling Zhou Hero";
			request.ApprovingUserEmail = "mei@overwatch.com";
			request.Language = Core.SharedConstants.Languages.ChineseSimplified;

			var reportingContact = request.Staff.AddNew();
			reportingContact.Name = "Juergen Klinsmann M.E.H";
			reportingContact.EmailAddress = "juergen.klinsmann@cargowise.com";

			var approvingContact = request.Staff.AddNew();
			approvingContact.Name = "Mei-Ling Zhou Hero";
			approvingContact.EmailAddress = "mei@overwatch.com";

			var incident = Factory.New<SupportIncident>();
			AssertNull("Precondition", incident.Contact);

			var adapter = new ServiceRequestDataAdapterForTest();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Incident Language", Core.SharedConstants.Languages.ChineseSimplified, incident.IM_Language);
			AssertEquals("Org Contact Name", "Mei", incident.Contact.OC_ContactName);
			AssertEquals("Org Contact Email", "mei@overwatch.com", incident.Contact.OC_Email);
			AssertEquals("Org Contact Language", Core.SharedConstants.Languages.English, incident.Contact.OC_Language);
		}

		public void TestImportFromValueObjectCore_OldLanguage()
		{
			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;

			request.ReportingStaffMemberName = "Juergen Klinsmann M.E.H";
			request.ReportingStaffEmail = "juergen.klinsmann@cargowise.com";

			request.ApprovingUser = "Mei-Ling Zhou Hero";
			request.ApprovingUserEmail = "mei@overwatch.com";
			request.Language = "ENG";

			var incident = Factory.New<SupportIncident>();
			AssertNull("Precondition", incident.Contact);

			var adapter = new ServiceRequestDataAdapterForTest();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Incident Language", Core.SharedConstants.Languages.English, incident.IM_Language);
		}

		[ExpectNoExceptions]
		public void TestAttachmentWithInvalidCharacters()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			Xsd.CustomerServiceRequestAttachment attachment = request.Attachments.AddNew();
			attachment.FileName = "P&L > Foo Report";
			attachment.Data = new byte[] { 1, 2, 3 };

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
		}

		public void TestAttachmentName()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			var image = new Bitmap(1, 1);
			image.SetPixel(0, 0, Color.White);
			var stream = new MemoryStream();
			image.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
			byte[] imageBytes = stream.ToArray();

			Xsd.CustomerServiceRequestAttachment attachment = request.Attachments.AddNew();
			attachment.FileName = "image1.tif";
			attachment.Data = imageBytes;

			Xsd.CustomerServiceRequestAttachment attachment2 = request.Attachments.AddNew();
			attachment2.FileName = "foo.txt";
			attachment2.Data = Encoding.ASCII.GetBytes("foo");

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			var imageDoc = incident.DocManagerInfo.Documents[0];
			AssertEquals("image1.tif", imageDoc.FileName);

			var fileDoc = incident.DocManagerInfo.Files[0];
			AssertEquals("foo.txt", fileDoc.FileName);
		}

		public void TestImportFromValueObject_DoNotImportContactsPhoneNumberWithoutCountryCode()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "AAA";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUserEmail = "AAA@cargowise.com";

			OrgContact invalidExistingContact = Org.Contacts.AddNew();
			invalidExistingContact.OC_Email = "test2@user.cargowise.com";
			invalidExistingContact.OC_Phone = "01234567";

			Xsd.OrgContact validContact1 = request.Staff.AddNew();
			validContact1.Name = "Test 1";
			validContact1.EmailAddress = "test1@user.cargowise.com";
			validContact1.Phone = "61 (2) 11112222";
			validContact1.HomePhone = "+34 986332264";
			validContact1.Mobile = "+31(0)629093366";
			validContact1.Pager = "1 (333) 1234567";
			validContact1.OtherPhone = "+243 1236-1434";
			validContact1.Fax = "+45 237-1434";
			validContact1.PhoneExtension = "111";

			Xsd.OrgContact invalidContact1 = request.Staff.AddNew();
			invalidContact1.Name = "Test 2";
			invalidContact1.EmailAddress = "test2@user.cargowise.com";
			invalidContact1.Phone = "01234567";
			invalidContact1.HomePhone = "01234567";
			invalidContact1.Mobile = "01234567";
			invalidContact1.Pager = "01234567";
			invalidContact1.OtherPhone = "01234567";
			invalidContact1.Fax = "01234567";
			invalidContact1.PhoneExtension = "111";

			Factory.Save();
			AssertNotEquals(ZString.Empty, request.Staff[1].Phone);

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			Factory.Save();

			AssertEquals("61 (2) 11112222", request.Staff[0].Phone);
			AssertEquals("+34 986332264", request.Staff[0].HomePhone);
			AssertEquals("+31(0)629093366", request.Staff[0].Mobile);
			AssertEquals("1 (333) 1234567", request.Staff[0].Pager);
			AssertEquals("+243 1236-1434", request.Staff[0].OtherPhone);
			AssertEquals("+45 237-1434", request.Staff[0].Fax);
			AssertEquals("111", request.Staff[0].PhoneExtension);

			AssertEquals(ZString.Empty, request.Staff[1].Phone);
			AssertEquals(ZString.Empty, request.Staff[1].HomePhone);
			AssertEquals(ZString.Empty, request.Staff[1].Mobile);
			AssertEquals(ZString.Empty, request.Staff[1].Pager);
			AssertEquals(ZString.Empty, request.Staff[1].OtherPhone);
			AssertEquals(ZString.Empty, request.Staff[1].Fax);
			AssertEquals(ZString.Empty, request.Staff[1].PhoneExtension);

			OrgContact loadedInvalidContact = new BusinessObjectFactory().Load<OrgContact>(invalidExistingContact.PK);
			AssertEquals("01234567", loadedInvalidContact.OC_Phone);
		}

		public void TestImportFromValueObject_Validation()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "AAA";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUserEmail = "AAA@cargowise.com";

			OrgContact invalidExistingContact = Org.Contacts.AddNew();
			invalidExistingContact.OC_Email = "test2@user.cargowise.com";
			invalidExistingContact.OC_Phone = "01234567";

			Xsd.OrgContact validContact1 = request.Staff.AddNew();
			validContact1.Name = "Test 1";
			validContact1.EmailAddress = "test1@user.cargowise.com";
			validContact1.Phone = "61 (2) 11112222";
			validContact1.HomePhone = "+34 986332264";
			validContact1.Mobile = "+31(0)629093366";
			validContact1.OtherPhone = "+243 1236-1434";
			validContact1.Fax = "+45 237-1434";

			Xsd.OrgContact invalidContact1 = request.Staff.AddNew();
			invalidContact1.Name = "Test 2";
			invalidContact1.EmailAddress = "test2@user.cargowise.com";
			invalidContact1.Phone = "61 (2) 11s112F222";
			invalidContact1.HomePhone = "+34 9863322h64";
			invalidContact1.Mobile = "+31(0)6290d93366";
			invalidContact1.OtherPhone = "+243 123d6-1434";
			invalidContact1.Fax = "+45 2s37-1434";

			Factory.Save();

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			Factory.Save();

			AssertEquals("61 (2) 11112222", request.Staff[0].Phone);
			AssertEquals("+34 986332264", request.Staff[0].HomePhone);
			AssertEquals("+31(0)629093366", request.Staff[0].Mobile);
			AssertEquals("+243 1236-1434", request.Staff[0].OtherPhone);
			AssertEquals("+45 237-1434", request.Staff[0].Fax);

			AssertEquals(ZString.Empty, request.Staff[1].Phone);
			AssertEquals(ZString.Empty, request.Staff[1].HomePhone);
			AssertEquals(ZString.Empty, request.Staff[1].Mobile);
			AssertEquals(ZString.Empty, request.Staff[1].OtherPhone);
			AssertEquals(ZString.Empty, request.Staff[1].Fax);
		}

		public void TestImportFromValueObject_OtherProduct()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "TLX", false);
			var licEnt = BillingTestHelper.CreateAnotherDatabase(lic, "CW1", true);
			lic.Database.LD_Product = "TLX";
			Factory.Save();

			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = "ENTCOMTLX";
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Joe User";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Joe User";
			request.Product = "TLX";

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Joe User";
			contact1.EmailAddress = "Joe.User@test.com";

			var adapter = new ServiceRequestDataAdapterForTest();
			var incident = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(lic.Database.PK, incident.IM_LD);
			AssertEquals(lic.Company.LC_OH, incident.IM_OH_Client);
			AssertEquals(ZGuid.Empty, incident.IM_LCC);
			AssertNull("ClientCompany not created for other products", lic.ClientCompany);
		}

		public void TestImportFromValueObject_ProductivityWise()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRW", false);
			lic.Database.LD_Product = "PRW";
			Factory.Save();

			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = "ENTCOMPRW";
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Joe User";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Joe User";
			request.Product = "PRW";
			request.CompanyCode = "C01";

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Joe User";
			contact1.EmailAddress = "Joe.User@test.com";

			var adapter = new ServiceRequestDataAdapterForTest();
			var incident = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(lic.Database.PK, incident.IM_LD);
			AssertEquals(lic.Company.LC_OH, incident.IM_OH_Client);
			AssertNotNull("ClientCompany created for PRW", incident.ClientCompany);
			AssertEquals("C01", incident.ClientCompany.LCC_Code);
		}

		public void TestImportFromValueObject_DuplicateEmail()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD", true);
			Factory.Save();

			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = "ENTCOMPRD";
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Joe User";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Joe User";
			request.Product = "";

			var contact1 = request.Staff.AddNew();
			contact1.Name = "Joe User";
			contact1.EmailAddress = "Joe.User@test.com";
			contact1.WebAccessEnable = true;

			var contact2BlankEmail = request.Staff.AddNew();
			contact2BlankEmail.Name = "User 2";
			contact2BlankEmail.WebAccessEnable = true;

			var contact3BlankEmail = request.Staff.AddNew();
			contact3BlankEmail.Name = "User 3";
			contact3BlankEmail.WebAccessEnable = true;

			var contact4DupeEmail = request.Staff.AddNew();
			contact4DupeEmail.Name = "User 4";
			contact4DupeEmail.EmailAddress = "dupe@test.com";
			contact4DupeEmail.WebAccessEnable = true;

			var contact5DupeEmail = request.Staff.AddNew();
			contact5DupeEmail.Name = "User 5";
			contact5DupeEmail.EmailAddress = "dupe@test.com";
			contact5DupeEmail.WebAccessEnable = true;

			var adapter = new ServiceRequestDataAdapterForTest();
			var incident = Factory.New<SupportIncident>();
			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("webaccess granted for approving user", true, incident.Client.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "Joe User").OC_WebAccessEnabled);
		}

		public void TestImportFromValueObject_Attachments()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();

			Xsd.CustomerServiceRequestAttachment attachment = request.Attachments.AddNew();
			attachment.FileName = "f.txt";
			attachment.Data = Encoding.UTF8.GetBytes("Hi there");
			attachment.DocType = Core.Constants.RefDocTypes.MiscellaneousDocument; //Only this DocType can have a custom description!
			attachment.Desc = "The description";

			SupportIncident incident = Factory.New<SupportIncident>();
			ServiceRequestDataAdapterForTest adapter = new ServiceRequestDataAdapterForTest();

			adapter.ImportFromValueObjectCoreForTest(incident, request, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			var doc = incident.DocManagerInfo.AllEDocs[0];
			AssertEquals("f.txt", doc.FileName);
			AssertEquals("The description", doc.Description);
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, doc.DocType);
		}

		#region Implementation

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		#region Overrides for base test

		readonly string BaseTestFilePath = @"Enterprise\Product\Operations\CustomerService\Testing\";

		protected override ValueObjectDataAdapter<SupportIncident, Xsd.CustomerServiceRequest> GetNewBizObjXmlDataAdapter()
		{
			return new ServiceRequestDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			SupportIncident bizObj = Factory.New<SupportIncident>();
			return new BusinessObjectAndExpectedOutputFileName(bizObj, BaseTestFilePath + "EmptySupportIncident.xml", ValidationKind.None, "Empty business object");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			SupportIncident bizObj = Factory.New<SupportIncident>();
			return new BusinessObjectAndExpectedOutputFileName(bizObj, BaseTestFilePath + "PopulatedSupportIncident.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "Populated business object");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override void PopulateValueObjectWithLongStrings(IValueObject value, ValueObjectPropertyNavigator navigator)
		{
			base.PopulateValueObjectWithLongStrings(value, navigator);
			if (value is Xsd.CustomerServiceRequestAttachment attachment)
			{
				attachment.DocType = "DOC";
			}
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return null; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "CustomerServiceRequest"; }
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		#endregion

		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.New<OrgHeader>();
					fOrg.OH_Code = "MYCLIENT";
				}
				return fOrg;
			}
		}

		OrgHeader fOrg;

		LicenceHeader Licence
		{
			get
			{
				if (fLicence == null)
				{
					LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
					enterprise.LE_EnterpriseCode = "ZUB";
					enterprise.LE_OH = Org.PK;

					LicenceCompany company = Factory.New<LicenceCompany>();
					company.LC_CompanyCode = "RAK";
					company.LC_OH = Org.PK;
					company.LC_LE = enterprise.PK;

					LicenceDatabase database = Factory.New<LicenceDatabase>();
					database.LD_LicenceType = DatabaseTypes.Codes.Production;
					database.LD_ServerCode = "LOL";
					database.LD_LE = enterprise.PK;
					database.LD_PublicEmailAddressForUpdate = "someone@somewhere.com";
					database.LD_PublicEmailAddressForUpdate = "test@test.com";

					fLicence = Factory.New<LicenceHeader>();
					fLicence.LA_LC = company.PK;
					fLicence.LA_LD = database.PK;

					Factory.Save();
				}

				return fLicence;
			}
		}

		LicenceHeader fLicence;

		public class ServiceRequestDataAdapterForTest : ServiceRequestDataAdapter
		{
			public void ImportFromValueObjectCoreForTest(SupportIncident bizObj, Xsd.CustomerServiceRequest value, ValueObjectImportContext context)
			{
				base.ImportFromValueObjectCore(bizObj, value, context);
			}
		}

		#endregion
	}
}
