using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.Module;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(EDIGlbCompanyCampaignContactFilterBusinessObject))]
	public class EDIGlbCompanyCampaignContactFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters
		#region StatusAndFlags
		public void TestLicenceDatabaseType()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Database Type"];
			AssertNotNull(result);
		}

		public void TestLicenceDatabaseHostedLocation()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Database Hosted Location"];
			AssertNotNull(result);
		}

		public void TestLicenceReleaseRing()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Release Ring"];
			AssertNotNull(result);
		}

		public void TestLicencePurchaseType()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Purchase Type"];
			AssertNotNull(result);
		}

		public void TestLicenceProductType()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Product Type"];
			AssertNotNull(result);
		}

		public void TestLicenceEditionType()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Edition Type"];
			AssertNotNull(result);
		}

		public void TestLicenceModule()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Licence Module"];
			AssertNotNull(result);
		}

		public void TestSQLServerEdition()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["SQL Server Edition"];
			AssertNotNull(result);
		}

		public void TestSQLServerVersion()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["SQL Server Version"];
			AssertNotNull(result);
		}

		public void TestOSName()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["OS Name"];
			AssertNotNull(result);
		}

		public void TestOSVersion()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["OS Version"];
			AssertNotNull(result);
		}

		public void TestSysteManufacturer()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["System Manufacturer"];
			AssertNotNull(result);
		}

		public void TestProcessorType()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Processor Type"];
			AssertNotNull(result);
		}

		#endregion
		#region NumbersAndReferences
		public void TestLicenceModuleUserCount()
		{
			var result = (ModuleNumberRangeFilter)EDICampaignFilter["Licence Module User Count"];
			AssertNotNull(result);
		}

		public void TestTotalMemory()
		{
			var result = (ModuleNumberRangeFilter)EDICampaignFilter["Total Memory"];
			AssertNotNull(result);
		}

		public void TestNoOfProcessors()
		{
			var result = (ModuleNumberRangeFilter)EDICampaignFilter["No of Processors"];
			AssertNotNull(result);
		}

		public void TestProcessorSpeed()
		{
			var result = (ModuleNumberRangeFilter)EDICampaignFilter["Processor Speed"];
			AssertNotNull(result);
		}

		#endregion
		#region TextFilters
		public void TestIsVirtualMachine()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Is Virtual Machine"];
			AssertNotNull(result);
		}

		public void TestOSNameFreeText()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["OS Name Free Text"];
			AssertNotNull(result);
		}

		public void TestOSVersionFreeText()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["OS Version Free Text"];
			AssertNotNull(result);
		}

		public void TestSystemManufacturerFreeText()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["System Manufacturer Free Text"];
			AssertNotNull(result);
		}

		public void TestProcessorTypeFreeText()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Processor Type Free Text"];
			AssertNotNull(result);
		}

		public void TestMasterOrganisationContactsOnly()
		{
			var masterOrgFilter = (ModuleTextFilter)EDICampaignFilter["Is Master Organization Contact"];
			AssertNotNull(masterOrgFilter);
			masterOrgFilter.IsActive = true;
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = "ENT TST Company";
			org.OH_Code = "ENTTST";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User Only - Fred Nerk";
			contact.OC_Email = "fnerk@test.com";
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "TST";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_Product = ProductTypes.Codes.Enterprise;
			var company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = "TST";
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;
			LicenceHeader licence = Factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_FullName = "ENT TST2 Company";
			org2.OH_Code = "ENTTS2";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Test User Only -John Smith";
			contact2.OC_Email = "jsmith@test.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();
			var expectedContact1 = Factory.Load<CampaignContact>(contact.PK);
			var expectedContact2 = Factory.Load<CampaignContact>(contact2.PK);
			masterOrgFilter.Property = IsMasterOrganisationContact.Codes.MasterOrganizationContactsOnly;
			masterOrgFilter.IsActive = true;
			var collection = new GlbCampaignContactCollectionTest(campaign);
			collection.Load(masterOrgFilter.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
			masterOrgFilter.Property = IsMasterOrganisationContact.Codes.NonMasterOrganizationContactsOnly;
			collection = new GlbCampaignContactCollectionTest(campaign);
			collection.Load(masterOrgFilter.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact2, collection);
			masterOrgFilter.Property = IsMasterOrganisationContact.Codes.AllContacts;
			collection = new GlbCampaignContactCollectionTest(campaign);
			collection.Load(masterOrgFilter.Query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
			AssertCollectionContains(expectedContact2, collection);
		}

		public void TestIsACW1ProductionVerifiedContact()
		{
			var result = (ModuleTextFilter)EDICampaignFilter["Is a CW1 Production Verified Contact"];
			AssertNotNull(result);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Test User Only - master yoda 1";
			contact1.OC_Email = "yoda@jedi.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Test User Only - master yoda 2";
			contact2.OC_Email = "yoda@jedi.com";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			licenceDatabase.LD_HostServerName = "LD_HostServerName!";
			licenceDatabase.LD_HostDBInstance = "LD_HostDBInstance!";
			licenceDatabase.LD_HostDBName = "LD_HostDBName!";
			licenceDatabase.LD_HostConnectionServerName = "LD_HostConnectionServerName!";
			licenceDatabase.LD_HostedLocation = "NCW";
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = licenceDatabase.PK;
			userAccount1.EUA_UserID = "Cod";
			userAccount1.EUA_FullName = "Full Name";
			userAccount1.EUA_Email = "a@email.com";
			userAccount1.EUA_IsActive = true;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();
			var orgsForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, new[] { org1.PK });
			var expectedContact1 = Factory.Load<CampaignContact>(contact1.PK);
			var expectedContact2 = Factory.Load<CampaignContact>(contact2.PK);
			var userAccount = Factory.Load<EdiCustomerUserAccount>(userAccount1.PK);
			result.Property = ContactIsCW1ProductionVerified.Codes.IsCw1ProductionVerifiedContactOnly;
			result.IsActive = true;
			var collection = new GlbCampaignContactCollectionTest(campaign);
			var combinedFilter = new ZQuery(result.Query, orgsForTestFilter);
			collection.Load(combinedFilter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
			result.Property = ContactIsCW1ProductionVerified.Codes.NonCw1ProductionVerifiedContact;
			collection = new GlbCampaignContactCollectionTest(campaign);
			combinedFilter = new ZQuery(result.Query, orgsForTestFilter);
			collection.Load(combinedFilter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact2, collection);
			result.Property = ContactIsCW1ProductionVerified.Codes.AllContacts;
			collection = new GlbCampaignContactCollectionTest(campaign);
			combinedFilter = new ZQuery(result.Query, orgsForTestFilter);
			collection.Load(combinedFilter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
			AssertCollectionContains(expectedContact2, collection);
		}

		#endregion
		#region DateFilters
		public void TestBIOSDate()
		{
			var result = (ModuleSingleDateFilter)EDICampaignFilter["BIOS Date"];
			AssertNotNull(result);
		}

		#endregion
		#region RelatedIncidentsFilters
		public void TestRelatedIncidentsFilters()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "Org1";
			var org1IncidentContact = GetNewContact(org1);
			var org1Contact = GetNewContact(org1);
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = org2.OH_FullName = "Org2";
			var org2IncidentContact = GetNewContact(org2);
			var org2Contact = GetNewContact(org2);
			SupportIncident workingIncident = Factory.NewWithValidTestData<SupportIncident>();
			workingIncident.IM_Product = "ENT";
			workingIncident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			workingIncident.IM_Status = "WRK";
			workingIncident.IM_OH_Client = org1.PK;
			workingIncident.IM_OC_Contact = org1IncidentContact.PK;
			SupportIncident closedIncident = Factory.NewWithValidTestData<SupportIncident>();
			closedIncident.IM_Product = "ENT";
			closedIncident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			closedIncident.IM_Status = "CLS";
			closedIncident.IM_OH_Client = org2.PK;
			closedIncident.IM_OC_Contact = org2IncidentContact.PK;
			Factory.Save();
			var contactsfilter = (ContactsIncidentsFilter)EDICampaignFilter["Incidents (by Contact)"];
			contactsfilter.IsActive = true;
			var filterQuery = new ZQuery();
			filterQuery.AddToFilter(contactsfilter.Query);
			var filterResult = Factory.Load<CampaignContact>(filterQuery);
			AssertContainsExactElementsInAnyOrder(new[] { org1IncidentContact.PK, org2IncidentContact.PK }, filterResult.Select(x => x.PK));
			contactsfilter.SelectedFilters.AddTextFilterStrip("Status", "WRK");
			filterQuery = new ZQuery();
			filterQuery.AddToFilter(contactsfilter.Query);
			filterResult = Factory.Load<CampaignContact>(filterQuery);
			AssertContainsExactElementsInAnyOrder(new[] { org1IncidentContact.PK }, filterResult.Select(x => x.PK));
			var orgsfilter = (OrganizationIncidentsFilter)EDICampaignFilter["Incidents (by Organization)"];
			orgsfilter.IsActive = true;
			filterQuery = new ZQuery();
			filterQuery.AddToFilter(orgsfilter.Query);
			filterResult = Factory.Load<CampaignContact>(filterQuery);
			AssertContainsExactElementsInAnyOrder(new[] { org1IncidentContact.PK, org1Contact.PK, org2IncidentContact.PK, org2Contact.PK }, filterResult.Select(x => x.PK));
			orgsfilter.SelectedFilters.AddTextFilterStrip("Status", "CLS");
			filterQuery = new ZQuery();
			filterQuery.AddToFilter(orgsfilter.Query);
			filterResult = Factory.Load<CampaignContact>(filterQuery);
			AssertContainsExactElementsInAnyOrder(new[] { org2IncidentContact.PK, org2Contact.PK }, filterResult.Select(x => x.PK));
		}

		#endregion
		#region WARP filters
		[TestDate(2014, 12, 3, 21, 0, 0)] // 2014-12-04 08:00 UTC+11
		[TestUtcOffset(11, 0, 0)]
		public void TestWARPDateFilter()
		{
			var yesterdayUtc = new ZDateTime(2014, 12, 3, 11, 0, 0); // 2014-12-03 22:00 UTC+11
			ModuleTextFilter textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org4.OH_Code = org4.OH_FullName = "AAAAD";
			org5.OH_Code = org5.OH_FullName = "AAAAE";
			org6.OH_Code = org6.OH_FullName = "AAAAF";
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			var org4Contact = GetNewContact(org4);
			var org5Contact = GetNewContact(org5);
			var org6Contact = GetNewContact(org6);
			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK, yesterdayUtc);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK, yesterdayUtc);
			var validParty = SetupRelatedParty(org5.PK, org6.PK, yesterdayUtc);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();
				AssertEquals("Precondition: OrgRelatedParty.OnSaving did not override", validParty.PR_SystemCreateTimeUtc, yesterdayUtc);
				ModuleDateFilter filter = (ModuleDateFilter)EDICampaignFilter["WARP Date"];
				filter.IsActive = true;
				filter.PropertySearch = "Today";
				var collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 3 contacts", 3, collection.Count);
				AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
				AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
				AssertEquals("Collection does not contain org 3 contact", false, collection.Contains(org3Contact));
				AssertEquals("Collection contains org 4 contact", true, collection.Contains(org4Contact));
				AssertEquals("Collection does not contain org 5 contact", false, collection.Contains(org5Contact));
				AssertEquals("Collection does not contain org 6 contact", false, collection.Contains(org6Contact));
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = yesterdayUtc;
				filter.Property2 = yesterdayUtc;
				collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 5 contacts", 5, collection.Count);
				AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
				AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
				AssertEquals("Collection contains org 3 contact", true, collection.Contains(org3Contact));
				AssertEquals("Collection does not contain org 4 contact", false, collection.Contains(org4Contact));
				AssertEquals("Collection contains org 5 contact", true, collection.Contains(org5Contact));
				AssertEquals("Collection contains org 6 contact", true, collection.Contains(org6Contact));
			}
		}

		public void TestWARPNominatingAgentFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org4.OH_Code = org4.OH_FullName = "AAAAD";
			org5.OH_Code = org5.OH_FullName = "AAAAE";
			org6.OH_Code = org6.OH_FullName = "AAAAF";
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			var org4Contact = GetNewContact(org4);
			var org5Contact = GetNewContact(org5);
			var org6Contact = GetNewContact(org6);
			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();
				ModuleFlagsFilter filter = (ModuleFlagsFilter)EDICampaignFilter["WARP Relationships"];
				filter["Is WARP Nominated Agent"] = true;
				filter.IsActive = true;
				var collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 2 contacts", 2, collection.Count);
				AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
				AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
				filter = (ModuleFlagsFilter)EDICampaignFilter["WARP Relationships"];
				filter["Is WARP Nominated Agent"] = false;
				filter.IsActive = true;
				collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains all 6 contacts", 6, collection.Count);
			}
		}

		public void TestWARPReferringCustomerFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org4.OH_Code = org4.OH_FullName = "AAAAD";
			org5.OH_Code = org5.OH_FullName = "AAAAE";
			org6.OH_Code = org6.OH_FullName = "AAAAF";
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			var org4Contact = GetNewContact(org4);
			var org5Contact = GetNewContact(org5);
			var org6Contact = GetNewContact(org6);
			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();
				ModuleFlagsFilter filter = (ModuleFlagsFilter)EDICampaignFilter["WARP Relationships"];
				filter["Is WARP Referring Customer"] = true;
				filter.IsActive = true;
				var collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 4 contacts", 4, collection.Count);
				AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
				AssertEquals("Collection contains org 3 contact", true, collection.Contains(org3Contact));
				AssertEquals("Collection contains org 4 contact", true, collection.Contains(org4Contact));
				AssertEquals("Collection contains org 5 contact", true, collection.Contains(org5Contact));
				filter = (ModuleFlagsFilter)EDICampaignFilter["WARP Relationships"];
				filter["Is WARP Referring Customer"] = false;
				filter.IsActive = true;
				collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains all 6 contacts", 6, collection.Count);
			}
		}

		public void TestNominatedAgentsCountFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org4.OH_Code = org4.OH_FullName = "AAAAD";
			org5.OH_Code = org5.OH_FullName = "AAAAE";
			org6.OH_Code = org6.OH_FullName = "AAAAF";
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			var org4Contact = GetNewContact(org4);
			var org5Contact1 = GetNewContact(org5);
			var org5Contact2 = GetNewContact(org5);
			var org6Contact = GetNewContact(org6);
			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK);
			SetupRelatedParty(org2.PK, org6.PK);
			SetupRelatedParty(org5.PK, org6.PK);
			SetupRelatedParty(org6.PK, org6.PK);
			Factory.Save();
			var filter = (CampaignContactNumberFilter)EDICampaignFilter["Nominated Agents with Referring Customer Count"];
			AssertEquals("DefaultComparisonOperator", CampaignContactNumberFilter.ComparisonConstants.GreaterThanOrEqualTo, filter.DefaultComparisonOperator);
			filter.Property = 2;
			filter.IsActive = true;
			var collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 2 contacts", 2, collection.Count);
			AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
			AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
			filter.Property = 3;
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
			filter.Property = 1;
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 4 contacts", 4, collection.Count);
			AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
			AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
			AssertEquals("Collection contains org 5 contact 1", true, collection.Contains(org5Contact1));
			AssertEquals("Collection contains org 5 contact 2", true, collection.Contains(org5Contact2));
			filter.Property = 0;
			filter.ComparisonOperator = CampaignContactNumberFilter.ComparisonConstants.Exact;
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 0 contacts", 0, collection.Count);
		}

		public void TestIncludeRankingFilter()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "Org1";
			var org1Contact1 = GetNewContact(org1);
			var doc = org1Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DefaultContact = true;
			var org1Contact2 = GetNewContact(org1);
			doc = org1Contact2.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Marketing.Code;
			doc.OD_DefaultContact = true;
			var org1Contact3 = GetNewContact(org1);
			doc = org1Contact3.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.ExportAirDepot.Code;
			doc.OD_DefaultContact = true;
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = org2.OH_FullName = "Org2";
			var org2Contact1 = GetNewContact(org2);
			doc = org2Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DefaultContact = true;
			var org2Contact2 = GetNewContact(org2);
			doc = org2Contact2.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DefaultContact = true;
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org3.OH_Code = org3.OH_FullName = "Org3";
			var org3Contact1 = GetNewContact(org3);
			doc = org3Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.ExportAirDepot.Code;
			doc.OD_DefaultContact = true;
			var org3Contact2 = GetNewContact(org3);
			doc = org3Contact2.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.ExportAirDepot.Code;
			doc.OD_DefaultContact = true;
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org4.OH_Code = org4.OH_FullName = "Org4";
			var org4Contact1 = GetNewContact(org4);
			doc = org4Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DeliverBy = Core.Constants.ContactNotifyModes.DoNotDeliver;
			doc.OD_DefaultContact = true;
			var org4Contact2 = GetNewContact(org4);
			doc = org4Contact2.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Marketing.Code;
			doc.OD_DefaultContact = true;
			var org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org5.OH_Code = org5.OH_FullName = "Org5";
			var org5Contact1 = GetNewContact(org5);
			doc = org5Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.AirWholesaler.Code;
			doc.OD_DefaultContact = true;
			var org5Contact2 = GetNewContact(org5);
			var org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org6.OH_Code = org6.OH_FullName = "Org6";
			var org6Contact1 = GetNewContact(org6);
			doc = org6Contact1.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DefaultContact = false;
			var org6Contact2 = GetNewContact(org6);
			doc = org6Contact2.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			doc.OD_DefaultContact = true;
			Factory.Save();
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-1)}' where OC_PK = '{org2Contact1.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-2)}' where OC_PK = '{org2Contact2.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-1)}' where OC_PK = '{org3Contact1.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-2)}' where OC_PK = '{org3Contact2.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-1)}' where OC_PK = '{org6Contact1.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.OrgContact set OC_SystemLastEditTimeUtc = '{ZDateTime.UtcNow.AddHours(-2)}' where OC_PK = '{org6Contact2.PK}'");
			var flagFilter = (ModuleFlagsFilter)EDICampaignFilter["Include Rankings"];
			flagFilter["Show Highest Ranked Contact Only"] = false;
			flagFilter.IsActive = true;
			var textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org1";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			var collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 3 contacts", 3, collection.Count);
			AssertEquals("Collection contains org 1 contact1", true, collection.Contains(org1Contact1));
			AssertEquals("Collection contains org 1 contact2", true, collection.Contains(org1Contact2));
			AssertEquals("Collection contains org 1 contact3", true, collection.Contains(org1Contact3));
			flagFilter = (ModuleFlagsFilter)EDICampaignFilter["Include Rankings"];
			flagFilter["Show Highest Ranked Contact Only"] = true;
			flagFilter.IsActive = true;
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 1 contact1", true, collection.Contains(org1Contact1));
			textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org2";
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 2 contact1", true, collection.Contains(org2Contact1));
			textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org3";
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 3 contact1", true, collection.Contains(org3Contact1));
			textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org4";
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 4 contact2", true, collection.Contains(org4Contact2));
			textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org5";
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 5 contact2", true, collection.Contains(org5Contact2));
			textFilter = (ModuleTextFilter)EDICampaignFilter["Organization Name"];
			textFilter.Property = "Org6";
			collection = new GlbCampaignContactCollection(Factory, EDICampaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("Collection contains org 6 contact2", true, collection.Contains(org6Contact2));
		}

		EDIOrgRelatedParty SetupRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid)
		{
			return SetupRelatedParty(parentGuid, relatedPartyGuid, ZDateTime.UtcNow);
		}

		EDIOrgRelatedParty SetupRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid, ZDateTime createdTime)
		{
			EDIOrgRelatedParty party = Factory.NewWithValidTestData<EDIOrgRelatedParty>();
			party.PR_OH_Parent = parentGuid;
			party.PR_OH_RelatedParty = relatedPartyGuid;
			party.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			party.PR_SystemCreateTimeUtc = createdTime;
			return party;
		}

		OrgContact GetNewContact(EDIOrgHeader org)
		{
			var result = org.Contacts.AddNew();
			result.OC_ContactName = org.OH_Code + (org.Contacts.Count + 1);
			return result;
		}

		#endregion WARP filters
		#region Sales Filters
		public void TestSalesFilters()
		{
			using (EDIDataRegistry.Instance.NumberOfEmployeesLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Number Of Employees Caption"))
			using (EDIDataRegistry.Instance.PaidUpCapitalLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Paid Up Capital Caption"))
			{
				var campaignFilter = new EDIGlbCompanyCampaignContactFilterBusinessObject(EDICampaign);
				EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
				EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
				EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
				org1.OH_Code = org1.OH_FullName = "AAAAA";
				org1.MiscServ.OM_CMNoOfEmployees = 3;
				org1.MiscServ.OM_CMPaidUpCapital = 4;
				org2.OH_Code = org2.OH_FullName = "AAAAB";
				org2.MiscServ.OM_CMNoOfEmployees = 6;
				org2.MiscServ.OM_CMPaidUpCapital = 9;
				org3.OH_Code = org3.OH_FullName = "AAAAC";
				org3.MiscServ.OM_CMNoOfEmployees = 9;
				org3.MiscServ.OM_CMPaidUpCapital = 12;
				Factory.Save();
				var org1Contact = GetNewContact(org1);
				var org2Contact = GetNewContact(org2);
				var org3Contact = GetNewContact(org3);
				Factory.Save();
				var numberOfEmployeesFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Number Of Employees"];
				var paidUpCapitalFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Paid Up Capital"];
				numberOfEmployeesFilter.ComparisonOperator = CampaignContactNumberFilter.ComparisonConstants.GreaterThan;
				numberOfEmployeesFilter.Property = 4;
				numberOfEmployeesFilter.IsActive = true;
				var collection = new GlbCampaignContactCollection(Factory, campaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 2 organisations", 2, collection.Count);
				paidUpCapitalFilter.ComparisonOperator = CampaignContactNumberFilter.ComparisonConstants.LessThan;
				paidUpCapitalFilter.Property = 10;
				paidUpCapitalFilter.IsActive = true;
				collection = new GlbCampaignContactCollection(Factory, campaignFilter.Filter);
				collection.Load();
				AssertEquals("Collection contains 1 organisation", 1, collection.Count);
				AssertEquals("Test Number Of Employees Caption", numberOfEmployeesFilter.MultilingualDescription.ToString());
				AssertEquals("Test Paid Up Capital Caption", paidUpCapitalFilter.MultilingualDescription.ToString());
			}
		}

		#endregion
		#region Organisation Multiple Filter
		public void TestOrganisationMultipleFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org1.MiscServ.OM_CMNoOfEmployees = 3;
			org1.MiscServ.OM_CMPaidUpCapital = 4;
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org2.MiscServ.OM_CMNoOfEmployees = 6;
			org2.MiscServ.OM_CMPaidUpCapital = 9;
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org3.MiscServ.OM_CMNoOfEmployees = 9;
			org3.MiscServ.OM_CMPaidUpCapital = 12;
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			Factory.Save();
			var campaignFilter = new EDIGlbCompanyCampaignContactFilterBusinessObject(EDICampaign);
			var organisationFilter = (OrganisationModuleFilter)campaignFilter["Organization (Multiple)"];
			organisationFilter.IsActive = true;
			organisationFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var fullNameFilter = organisationFilter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Name");
			fullNameFilter.IsActive = true;
			fullNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			fullNameFilter.Property = "AAAAC";
			var collection = new GlbCampaignContactCollection(Factory, campaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 1 contact", 1, collection.Count);
			AssertEquals("First member primary key matches", org3Contact.PK, collection.First().PK);
		}

		#endregion
		#region Customised Labels
		public void TestCustomisedLabels()
		{
			using (EDIDataRegistry.Instance.AchievableBusinessLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Achievable Business Caption"))
			using (EDIDataRegistry.Instance.AmountOfBusinessWonLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Amount Of BusinessWon Caption"))
			using (EDIDataRegistry.Instance.TotalClientRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Total Client Revenue Caption"))
			using (EDIDataRegistry.Instance.WarehouseRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Warehouse Revenue Caption"))
			using (EDIDataRegistry.Instance.ConsultingRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Consulting Revenue Caption"))
			{
				var campaignFilter = new EDIGlbCompanyCampaignContactFilterBusinessObject(EDICampaign);
				Factory.Save();
				var achievableBusinessFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Achievable Business"];
				AssertEquals("Test Achievable Business Caption", achievableBusinessFilter.MultilingualDescription.ToString());
				var percentageWonFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Percentage Won"];
				AssertEquals("Test Amount Of BusinessWon Caption", percentageWonFilter.MultilingualDescription.ToString());
				var totalRevenueFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Total Revenue"];
				AssertEquals("Test Total Client Revenue Caption", totalRevenueFilter.MultilingualDescription.ToString());
				var warehouseRevenueFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Warehouse Revenue"];
				AssertEquals("Test Warehouse Revenue Caption", warehouseRevenueFilter.MultilingualDescription.ToString());
				var consultingRevenueFilter = (CampaignContactNumberFilter)campaignFilter["Sales - Consulting Revenue"];
				AssertEquals("Test Consulting Revenue Caption", consultingRevenueFilter.MultilingualDescription.ToString());
			}
		}

		#endregion
		#endregion
		#region Implementation
		EDIGlbCompanyCampaign EDICampaign;
		EDIGlbCompanyCampaignContactFilterBusinessObject EDICampaignFilter;
		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
			GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(EDICampaignFilter);
		}

		void CreateContactsForTest()
		{
			EDICampaign = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
			EDICampaignFilter = new EDIGlbCompanyCampaignContactFilterBusinessObject(EDICampaign);
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIGlbCompanyCampaignContactFilterBusinessObject(Factory.NewWithValidTestData<EDIGlbCompanyCampaign>());
		}
		#endregion
	}
}
