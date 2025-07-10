using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgHeader))]
	public class EDIOrgHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region Related Business Objects

		#region Projects

		public void TestProjects()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "A";
			var otherOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			var otherOrgContact = otherOrg.Contacts.AddNew();
			otherOrgContact.OC_ContactName = "B";

			EDIProject proj1 = Factory.NewWithValidTestData<EDIProject>();
			proj1.ChangeClientOrganisation(org);

			EDIProject proj2 = Factory.NewWithValidTestData<EDIProject>();
			proj2.ChangeClientOrganisation(org);

			EDIProject proj3 = Factory.NewWithValidTestData<EDIProject>();
			proj3.ChangeClientOrganisation(otherOrg);

			Factory.Save();

			AssertEquals(2, org.Projects.Count);
			AssertEquals(1, otherOrg.Projects.Count);

			AssertEquals(false, org.Projects.ReadOnly);
		}

		#endregion

		#region Licence Enterprise

		public void TestLicenceEnterpriseDuplicationIssue()
		{
			// if you create two LicenceEnterprise records with the same code at the same time, you get a 
			// unique index error on the LE_Code.
			// EG: 
			//		Create new EDIOrgHeader and LicEnterprise (A)
			//		Create new EDIOrgHeader and LicEnterprise (B) - the LicEnterprise from A is not stored yet, so must be new
			//		Save A
			//		Save B - Error (Unique LE_Code violation)

			BusinessObjectFactory testFactory1 = new BusinessObjectFactory();
			BusinessObjectFactory testFactory2 = new BusinessObjectFactory();

			EDIOrgHeader testHeader1 = CreateHeader(testFactory1, "TGBLOG", "AUBNE");
			EDIOrgHeader testHeader2 = CreateHeader(testFactory2, "TGBSOG", "AUBNE");

			testFactory1.Save();
			testFactory2.Save();
			Assert("TestHeader1 Saved", testHeader1.IsInDatabase);
			Assert("TestHeader2 Saved", testHeader2.IsInDatabase);
			Assert("Headers are in different factories", testHeader1.Factory != testHeader2.Factory);

			AssertNull("Header1's LicEnterprise is null", testHeader1.LicEnterprise);
			testHeader1.CreateAndLoadLicenceForOrg();
			AssertNotNull("Licence Enterprise created", testHeader1.LicEnterprise);
			AssertNotNull("Licence Company created", testHeader1.LicCompany);
			testHeader1.LicenceEnterpriseCode = "TGB";
			AssertEquals("Enterprise code is set properly", "TGB", testHeader1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("Enterprise linked back to header", testHeader1.PK, testHeader1.LicEnterprise.LE_OH);

			AssertNull("Header2's LicEnterprise is null", testHeader2.LicEnterprise);
			testHeader2.CreateAndLoadLicenceForOrg();
			AssertNotNull("Licence Enterprise created", testHeader2.LicEnterprise);
			AssertNotNull("Licence Company created", testHeader2.LicCompany);
			testHeader2.LicenceEnterpriseCode = "TGB";
			AssertEquals("Enterprise code is set properly", "TGB", testHeader2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("Enterprise linked back to header", testHeader2.PK, testHeader2.LicEnterprise.LE_OH);

			Assert("Header1's LicEnterprise is NOT Header2's LicEnterprise", testHeader1.LicEnterprise.PK != testHeader2.LicEnterprise.PK);

			Assert("TestHeader1 should have changes", testHeader1.HasChanges);
			Assert("TestHeader1's LicEnterprise NOT saved", testHeader1.LicEnterprise.HasChanges);
			testFactory1.Save();  // Save Header1 and LicEnterprise1
			Assert("TestHeader1 Saved", testHeader1.IsInDatabase);

			Assert("TestHeader2 should have changes", testHeader2.HasChanges);
			Assert("TestHeader2's LicEnterprise NOT saved", testHeader2.LicEnterprise.HasChanges);
			testFactory2.Save();  // Save Header2 and LicEnterprise2
			Assert("TestHeader2 Saved", testHeader2.IsInDatabase);

			Assert("Header2's LicEnterprise is Header1's LicEnterprise", testHeader1.LicEnterprise.PK == testHeader2.LicEnterprise.PK);
		}

		[ExpectNoExceptions]
		public void TestChangingLicenceEnterpriseCodeToOriginal()
		{
			// Test to stop exception caused when LicenceEnterpriseCode is saved, changed to different value,
			// then changed back to original value and saved.

			LicenceEnterpriseCollection collection = new LicenceEnterpriseCollection(Factory);

			collection.Load(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, "AAA"));
			collection.RemoveAndDeleteAll();

			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			Factory.Save();

			org1.LicenceEnterpriseCode = "BBB";

			org1.LicenceEnterpriseCode = "AAA";
			Factory.Save();
		}

		public void TestEventReferenceWithWildcardsTrigger_ShouldFireForChangeLogEvents()
		{
			var job = Factory.NewWithValidTestData<EDIOrgHeader>();
			job.OH_FullName = "JEAN VALJEAN";

			var trigger = job.WorkflowItems.Triggers.AddNew();

			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.P9_Description = "The miserable trigger";
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "Full Name was*";

			Factory.Save();

			AssertEquals(2, job.GetLogs().DatabaseCount);

			var log = job.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.AddedARecordToTheSystem.Code))[0];
			AssertEquals(ZString.Empty, log.SL_Reference);

			var newFactory = new BusinessObjectFactory();
			var loadedJob = newFactory.Load<EDIOrgHeader>(job.PK);
			loadedJob.OH_FullName = "MONSIEUR MADELEINE";
			newFactory.Save();

			AssertEquals(4, loadedJob.GetLogs().DatabaseCount);
			var x = loadedJob.GetLogs().Find(new ZQuery());
			log = loadedJob.GetLogs().Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"))[0];
			AssertEquals("Full Name was: JEAN VALJEAN", log.SL_Reference);

			AssertEquals("Workflow should fire when SL_Reference for an EDIT event is updated", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestLicenceEnterpriseCodesAreUpdatedCorrectly()
		{
			// "Master Org" is the Organisation associated with a particular Licence Enterprise
			// code in the LicenceEnterprise table. 

			LicenceEnterpriseCollection collection = new LicenceEnterpriseCollection(Factory);

			collection.Load(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, "AAA"));
			collection.RemoveAndDeleteAll();
			collection.Load(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, "BBB"));
			collection.RemoveAndDeleteAll();
			collection.Load(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, "CCC"));
			collection.RemoveAndDeleteAll();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			LicenceEnterpriseCollection newCollection = new LicenceEnterpriseCollection(newFactory);
			newCollection.Load();

			int licenceEnterpriseBeforeTest = newCollection.Count;

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			Factory.Save();

			org2.CreateAndLoadLicenceForOrg();
			org2.LicenceEnterpriseCode = "AAA";
			Factory.Save();

			org3.CreateAndLoadLicenceForOrg();
			org3.LicenceEnterpriseCode = "BBB";
			Factory.Save();

			newCollection = new LicenceEnterpriseCollection(newFactory);
			newCollection.Load();
			AssertEquals("There should be two more LicenceEnterprise records than there were at the start of the test", licenceEnterpriseBeforeTest, newCollection.Count - 2);

			org2.LicenceEnterpriseCode = "CCC";
			Factory.Save();
			newCollection.Load();
			AssertEquals("There should be three more LicenceEnterprise records than there were at the start of the test", licenceEnterpriseBeforeTest, newCollection.Count - 3);
			AssertNotNull("Org 2 is in the LicenceEnterprise table as the master org for code CCC", Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, SQLComparisonOperator.Equal, org2.PK)));

			org2.LicenceEnterpriseCode = "BBB";
			Factory.Save();
			newCollection.Load();
			AssertEquals("Test changing enterprise code on master org: There should be two more LicenceEnterprise records than there were at the start of the test",
				licenceEnterpriseBeforeTest, newCollection.Count - 2);
			AssertNull("Org 2 should no longer be in the LicenceEnterprise table as it is not a master org for any Enterprise", Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, SQLComparisonOperator.Equal, org2.PK)));

			org3.LicenceEnterpriseCode = "AAA";
			Factory.Save();
			newCollection.Load();
			AssertEquals("Test changing enterprise code on master org with a second licence company: There should be two more LicenceEnterprise records than there were at the start of the test",
						licenceEnterpriseBeforeTest, newCollection.Count - 2);
			AssertNull("Org 3 should no longer be in the LicenceEnterprise table as it is not a master org for any Enterprise", Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, SQLComparisonOperator.Equal, org3.PK)));
			AssertNotNull("Org 2 is in the LicenceEnterprise table as the master org for code CCC", Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, SQLComparisonOperator.Equal, org2.PK)));

			org3.LicCompany.LicDatabases.AddNew();
			Factory.Save();
			org3.LicenceEnterpriseCode = "AAA";
			AssertEquals("setting the same code doesn't remove databases", 1, org3.LicCompany.LicDatabases.Count);

			AssertNull("PRE", org4.LicEnterprise);
			org4.LicenceEnterpriseCode = "";
			AssertNotNull(org4.LicEnterprise);
			Factory.Save();
		}

		public void TestLicenceEnterpriseIDsAreUpdatedCorrectly()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			Factory.Save();

			org2.CreateAndLoadLicenceForOrg();
			org2.LicenceEnterpriseCode = "AAA";
			Factory.Save();

			org3.CreateAndLoadLicenceForOrg();
			org3.LicenceEnterpriseCode = "CCC";
			Factory.Save();

			var collection = new LicenceEnterpriseCollection(Factory);
			collection.Load();
			AssertEquals(2, collection.Count);

			org3.LicenceEnterpriseID = org1.LicenceEnterpriseID;
			Factory.Save();

			collection = new LicenceEnterpriseCollection(Factory);
			collection.Load();
			AssertEquals(1, collection.Count);

			AssertEquals(org3.LicEnterprise.PK, org1.LicEnterprise.PK);

			AssertEquals("PRE", 0, org3.LicCompany.LicDatabases.Count);
			org3.LicCompany.LicDatabases.AddNew();
			Factory.Save();
			org3.LicenceEnterpriseID = org3.LicenceEnterpriseID;
			AssertEquals("setting the same ID doesn't remove databases", 1, org3.LicCompany.LicDatabases.Count);
		}

		#endregion

		#region Databases

		public void TestAllowAddingNewDatabases()
		{
			EDIOrgHeader header = HeaderForTest;
			Assert("Should NOT allow adding new databases (LicEnterprise is null)", !header.AllowAddingNewDatabases);

			header.CreateAndLoadLicenceForOrg();
			Assert("Should NOT allow adding new databases (LicEnterprise not in DB)", !header.AllowAddingNewDatabases);

			header.LicEnterprise.LE_EnterpriseCode = "AAA";
			Factory.Save();
			Assert("Should allow adding new databases", header.AllowAddingNewDatabases);
		}

		public void TestRemovalOfDatabaseFromOneCompany()
		{
			EDIOrgHeader testHeader1 = HeaderForTest;
			testHeader1.CreateAndLoadLicenceForOrg();
			testHeader1.LicCompany.LC_CompanyCode = "LOG";
			testHeader1.LicenceEnterpriseCode = "TGB";

			EDIOrgHeader testHeader2 = HeaderForTest;
			testHeader2.OH_Code = "TGBSSS";
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.LicCompany.LC_CompanyCode = "SSS";
			testHeader2.LicenceEnterpriseCode = "TGB";

			LicenceDatabase database = testHeader1.LicCompany.LicDatabases.AddNew();
			testHeader2.LicCompany.LicDatabases.Add(database);

			Assert("TestHeader1 has a database", testHeader1.LicCompany.LicDatabases.Count == 1);
			Assert("TestHeader2 has a database", testHeader2.LicCompany.LicDatabases.Count == 1);

			Factory.Save();
			testHeader1.LicCompany.LicDatabases.Remove(database);
			Assert("TestHeader1 has no database", testHeader1.LicCompany.LicDatabases.Count == 0);
			Assert("TestHeader2 has a database", testHeader2.LicCompany.LicDatabases.Count == 1);
		}

		public void TestChangeOfEnterpriseCode()
		{
			EDIOrgHeader testHeader1 = HeaderForTest;
			testHeader1.CreateAndLoadLicenceForOrg();
			testHeader1.LicenceEnterpriseCode = "TGB";
			testHeader1.LicCompany.LC_CompanyCode = "LOG";

			Factory.Save();

			EDIOrgHeader testHeader2 = HeaderForTest;
			testHeader2.OH_Code = "TGBSSS";
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.LicenceEnterpriseCode = "TGB";
			testHeader2.LicCompany.LC_CompanyCode = "SSS";

			Factory.Save();

			LicenceDatabase database = testHeader1.LicCompany.LicDatabases.AddNew();
			testHeader2.LicCompany.LicDatabases.Add(database);

			LicenceCompanyLicenceDatabaseCollection database1Collection = testHeader1.LicCompany.LicDatabases;

			AssertEquals("TestHeader1 has a database", 1, database1Collection.Count);
			AssertEquals("TestHeader2 has a database", 1, testHeader2.LicCompany.LicDatabases.Count);

			Factory.Save();
			testHeader1.LicenceEnterpriseCode = "AWB";
			AssertEquals("TestHeader1 has no database", 0, testHeader1.LicCompany.LicDatabases.Count);
			AssertEquals("TestHeader2 has a database", 1, testHeader2.LicCompany.LicDatabases.Count);
		}

		public void TestRefreshingNewEnterpriseCode()
		{
			EDIOrgHeader testHeader1 = HeaderForTest;
			testHeader1.CreateAndLoadLicenceForOrg();
			testHeader1.LicenceEnterpriseCode = "TGB";
			testHeader1.LicCompany.LC_CompanyCode = "LOG";
			LicenceDatabase licDB = testHeader1.LicCompany.LicDatabases.AddNew();
			testHeader1.AllDatabasesInLicenceEnterprise.Load(); // Its a FindBoxList
			AssertEquals("Database Collection has 1 element", 1, testHeader1.AllDatabasesInLicenceEnterprise.Count);

			testHeader1.LicenceEnterpriseCode = "XXX";
			testHeader1.AllDatabasesInLicenceEnterprise.Load(); // Its a FindBoxList
			AssertEquals("Database Collection has 0 elements", 0, testHeader1.AllDatabasesInLicenceEnterprise.Count);
		}

		#endregion

		#region Contracting Party

		public void TestContractingParty()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();

			AssertNull(org.ContractingParty);

			EDIOrgHeader relatedOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgRelatedParty contractingParty = Factory.New<EDIOrgRelatedParty>();
			contractingParty.PR_PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;
			contractingParty.PR_OH_RelatedParty = relatedOrg.PK;
			contractingParty.CompanyLevel = CompanyLevelList.Codes.ENT;

			org.AllRelatedParties.Add(contractingParty);

			AssertEquals(relatedOrg, org.ContractingParty);
		}

		#endregion

		#endregion

		#region Has Changes

		public void TestHasChangesWhenViewingOrgAndCreatingNewLicenceEnterprise()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			AssertEquals("Org has no changes", false, org1.HasChanges);
			org1.CreateAndLoadLicenceForOrg();
			AssertEquals("Org still has no changes as this is an automatic function", false, org1.HasChanges);
		}

		#endregion

		#region Validation

		public void TestLicenceEnterpriseCodeValidation()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "XXXX";
			org1.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			org1.CreateAndLoadLicenceForOrg();
			org1.LicEnterprise.LE_EnterpriseCode = "AAA";
			var db1 = org1.LicEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "ABC";
			db1.LD_Product = "CW1";
			Factory.Save();

			org1.LicenceEnterpriseCode = "AAA";
			AssertNoErrors(org1.LicenceEnterpriseCodeInfo);

			org1.LicenceEnterpriseCode = "BBB";
			org1.LicEnterprise.Databases.RemoveAndDeleteAll();
			org1.AllDatabasesInLicenceEnterprise.RemoveAndDeleteAll();
			org1.RunPreSaveValidation();
			AssertHasWarning(org1.LicenceEnterpriseCodeInfo, "Enterprise Code is only necessary for products ENT, CW1, CWN and SPH.");

			org1.LicenceEnterpriseCode = "";
			org1.LicEnterprise.Databases.RemoveAndDeleteAll();
			org1.AllDatabasesInLicenceEnterprise.RemoveAndDeleteAll();
			db1 = org1.LicEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "ABC";
			db1.LD_Product = "CW1";
			org1.LicCompany.ActiveOrAllLicDatabases.Add(db1);
			org1.RunPreSaveValidation();
			AssertHasError(org1.LicenceEnterpriseCodeInfo, "Enterprise Code is mandatory if a database with product CW1 is attached");
		}

		#endregion

		#region Lookups

		public void TestLicenceEnterpriseList()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.LicenceEnterpriseList.Load();
			int licenceEnterprisesAtStartOfList = org1.LicenceEnterpriseList.Count;

			Factory.Save();
			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "XXA";
			Factory.Save();
			org1.LicenceEnterpriseList.Load();
			AssertEquals("Licence Enterprise List has one extra entry", 1 + licenceEnterprisesAtStartOfList, org1.LicenceEnterpriseList.Count);

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			org2.CreateAndLoadLicenceForOrg();
			org2.LicenceEnterpriseCode = "XXB";
			Factory.Save();
			org1.LicenceEnterpriseList.Load();
			AssertEquals("Licence Enterprise List has two extra entries", 2 + licenceEnterprisesAtStartOfList, org1.LicenceEnterpriseList.Count);
		}

		public void TestAllDatabasesInLicenceEnterprise()
		{
			EDIOrgHeader testHeader1 = Factory.New<EDIOrgHeader>();
			EDIOrgHeader testHeader2 = Factory.New<EDIOrgHeader>();
			testHeader1.CreateAndLoadLicenceForOrg();
			testHeader2.CreateAndLoadLicenceForOrg();

			AssertEquals("Precondition: Enterprise Database count is 0", 0, testHeader1.LicCompany.LicEnterprise.Databases.Count);
			LicenceDatabase dB1 = testHeader1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase dB2 = testHeader1.LicCompany.LicDatabases.AddNew();
			AssertEquals("Enterprise Database count is 2", 2, testHeader1.LicCompany.LicEnterprise.Databases.Count);

			AssertEquals("Precondition: Enterprise Database count is 0", 0, testHeader2.LicCompany.LicEnterprise.Databases.Count);
			LicenceDatabase dB3 = testHeader2.LicCompany.LicDatabases.AddNew();
			LicenceDatabase dB4 = testHeader2.LicCompany.LicDatabases.AddNew();
			AssertEquals("Enterprise Database count is 2", 2, testHeader2.LicCompany.LicEnterprise.Databases.Count);

			AssertEquals("Header1 only has 2 items  in collection", 2, testHeader1.LicCompany.LicEnterprise.Databases.Count);
		}

		public void TestFilterPartyTypeList()
		{
			var testHeader = Factory.New<EDIOrgHeader>();

			AssertCollectionContains("EDI Party Types COP",
				EDIOrgRelatedPartyLookups.ContractingPartyCode,
				testHeader.FilterPartyTypeList.GetAllCodes());

			AssertCollectionContains("EDI Party Types WRP",
				EDIOrgRelatedPartyLookups.WARPConstant,
				testHeader.FilterPartyTypeList.GetAllCodes());

			AssertEquals("Description for COP",
				EDIOrgRelatedPartyLookups.ContractingPartyDescription,
				testHeader.FilterPartyTypeList.GetDescriptionFromCode(EDIOrgRelatedPartyLookups.ContractingPartyCode));

			AssertEquals("Description for WRP",
				"WARP Related Parties",
				testHeader.FilterPartyTypeList.GetDescriptionFromCode(EDIOrgRelatedPartyLookups.WARPConstant));

			AssertEquals("Description for ERQ",
				"eRequest Visibility Group",
				testHeader.FilterPartyTypeList.GetDescriptionFromCode(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode));
		}

		#endregion

		#region Licence Code Generation

		[ExpectNoExceptions]
		public void TestExceptionWhenGeneratingLicenceCode()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "XXXXX";
			Factory.Save();
			org1.CreateAndLoadLicenceForOrg();
			Factory.Save();
			org1.LicenceEnterpriseCode = "XX0";
			Factory.Save();
			org1.LicenceEnterpriseCode = "";
			org1.GenerateNewLicenceCode();
			Factory.Save();
		}

		public void TestEnterpriseCode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "TST";
			var company = enterprise.Companies.AddNew();
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			AssertEquals("Licence Enterprise Code is correct", "TST", org.LicenceEnterpriseCode);
		}

		public void TestCompanyCode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var company = enterprise.Companies.AddNew();
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;
			company.LC_CompanyCode = "TST";

			AssertEquals("Company Code is correct", "TST", org.CompanyCode);
		}

		public void TestProductCode()
		{
			var org = Factory.New<EDIOrgHeader>();
			var company = Factory.New<LicenceCompany>();
			company.LC_OH = org.PK;
			var database = company.LicDatabases.AddNew();
			database.LD_Product = "TS1";

			AssertArrayEqualsByElements("Product code is correct", new List<ZString> { "TS1" }.ToArray(), org.ProductId.ToArray());

			var database2 = company.LicDatabases.AddNew();
			database2.LD_Product = "TS2";

			AssertArrayEqualsByElements("Product code is correct with multiple databases", new List<ZString> { "TS1", "TS2" }.ToArray(), org.ProductId.ToArray());
		}

		public void TestEnterpriseId()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseID = "TST1234";
			var company = enterprise.Companies.AddNew();
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			AssertEquals("Licence Enterprise ID", "TST1234", org.LicenceEnterpriseID);
		}

		public void TestGenerateLicenceCode()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "XXXX1";
			Factory.Save();
			org1.CreateAndLoadLicenceForOrg();
			AssertEquals("Org1 has not yet been given a Licence Enterprise Code", ZString.Empty, org1.LicenceEnterpriseCode);
			Factory.Save();
			org1.GenerateNewLicenceCode();
			AssertEquals("Org1 code should be XXX", "XXX", org1.LicenceEnterpriseCode);

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "XXXXX2";
			Factory.Save();
			org2.CreateAndLoadLicenceForOrg();
			AssertEquals("Org2 has not yet been given a Licence Enterprise Code", ZString.Empty, org2.LicenceEnterpriseCode);
			Factory.Save();
			org2.GenerateNewLicenceCode();
			AssertEquals("Org2 code should be XX0", "XX0", org2.LicenceEnterpriseCode);

			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			org3.CreateAndLoadLicenceForOrg();
			AssertEquals("Org3 has not yet been given a Licence Enterprise Code", ZString.Empty, org3.LicenceEnterpriseCode);
			Factory.Save();
			org3.OH_Code = ZString.Empty;
			org3.GenerateNewLicenceCode();
			org3.Validation.ValidateLicenceEnterpriseCode();
			Assert("No OH_Code entered: Enterprise Code cannot be generated", org3.LicenceEnterpriseCodeInfo.HasError("You must enter this Organisation's Code before a Licence Enterprise Code can be generated"));

			//increase the combination rule
			org3.OH_Code = "ABCCC111";
			var ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			ent.LE_EnterpriseCode = "TTT";
			ent.LE_OH = org3.PK;
			ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			ent.LE_EnterpriseCode = "AAA";
			ent.LE_OH = org3.PK;

			for (var idx = 0; idx < 10; idx++)
			{
				ent = Factory.NewWithValidTestData<LicenceEnterprise>();
				ent.LE_EnterpriseCode = "TT" + idx.ToString();
				ent.LE_OH = org3.PK;
			}

			for (var idx = 0; idx < 100; idx++)
			{
				ent = Factory.NewWithValidTestData<LicenceEnterprise>();
				ent.LE_EnterpriseCode = "T" + idx.ToString().PadLeft(2, '0');
				ent.LE_OH = org3.PK;
			}

			Factory.Save();
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org4.OH_Code = "TTTTTT";
			org4.GenerateNewLicenceCode();
			AssertEquals("AAB", org4.LicenceEnterpriseCode);

			Factory.Save();
			var org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org5.OH_Code = "TTTTTT2X";
			org5.GenerateNewLicenceCode();
			AssertEquals("AAC", org5.LicenceEnterpriseCode);
		}

		#endregion

		#region Create / Load Licence

		public void TestCreateAndLoadLicenceForOrg()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			Factory.Save();
			AssertEquals("Test No Errors", 0, testHeader.NotificationsIncludingChildren.GetErrors().Count());
			AssertNull("Licence Enterprise is null before creation", testHeader.LicEnterprise);
			AssertNull("Licence Company is null before creation", testHeader.LicCompany);

			testHeader.CreateAndLoadLicenceForOrg();

			AssertNotNull("Licence Enterprise created", testHeader.LicEnterprise);
			AssertNotNull("Licence Company created", testHeader.LicCompany);
			AssertEquals("Enterprise code is not set", ZString.Empty, testHeader.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("Enterprise linked back to header", testHeader.PK, testHeader.LicEnterprise.LE_OH);
			AssertEquals("Licence Company Country is Aust", "AU", testHeader.LicCompany.LC_CompanyCountry);
		}

		public void TestShouldSyncLicenceCompanyCountry()
		{
			var org = HeaderForTest;
			org.OH_RL_NKClosestPort = "USCHI";
			AssertEquals("Licence hasn't been created", false, org.ShouldSyncLicenceCompanyCountry);

			org.CreateAndLoadLicenceForOrg();
			AssertEquals("Precondition", "US", org.LicCompany.LC_CompanyCountry);
			AssertEquals("Licence has been created and the countries match", false, org.ShouldSyncLicenceCompanyCountry);

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Precondition", "US", org.LicCompany.LC_CompanyCountry);
			AssertEquals("Organisation port has changed and licence country has not", true, org.ShouldSyncLicenceCompanyCountry);
		}

		public void TestSyncLicenceCompanyCountry()
		{
			var org = HeaderForTest;
			org.OH_RL_NKClosestPort = "USCHI";
			org.CreateAndLoadLicenceForOrg();

			org.SyncLicenceCompanyCountry();
			AssertEquals("Synced with org country (well, it actually doesn't try)", "US", org.LicCompany.LC_CompanyCountry);
			AssertEquals("Updated IsGSTRegistered (well, it actually doesn't try)", false, org.LicCompany.LC_IsGSTRegistered);
			AssertEquals("Updated Currency (well, it actually doesn't try)", "USD", org.LicCompany.LC_RX_NKCurrency);

			org.OH_RL_NKClosestPort = "AUSYD";
			org.SyncLicenceCompanyCountry();
			AssertEquals("Synced with org country", "AU", org.LicCompany.LC_CompanyCountry);
			AssertEquals("Updated IsGSTRegistered", true, org.LicCompany.LC_IsGSTRegistered);
			AssertEquals("Updated Currency", "AUD", org.LicCompany.LC_RX_NKCurrency);
		}

		public void TestGetSyncLicenceCompanyCountryMessage()
		{
			var org = HeaderForTest;
			org.OH_RL_NKClosestPort = "USCHI";
			org.CreateAndLoadLicenceForOrg();

			AssertEquals("The licence company country will be changed from 'US' to 'US' to match the country this organization locates. New licence key will be needed to update client system. Would you like to continue?", org.GetSyncLicenceCompanyCountryMessage());

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("The licence company country will be changed from 'US' to 'AU' to match the country this organization locates. New licence key will be needed to update client system. Would you like to continue?", org.GetSyncLicenceCompanyCountryMessage());
		}

		public void TestLoadCreatedButIncompleteLicence()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "";

			AssertNull("Licence Enterprise is null before creation", testHeader.LicEnterprise);
			AssertNull("Licence Company is null before creation", testHeader.LicCompany);

			testHeader.CreateAndLoadLicenceForOrg();
			AssertNotNull("Licence Enterprise created", testHeader.LicEnterprise);
			AssertNotNull("Licence Company created", testHeader.LicCompany);
			AssertEquals("Licence Companies Company Country is empty", ZString.Empty, testHeader.LicCompany.LC_CompanyCountry);

			// Reset the unloco
			testHeader.OH_RL_NKClosestPort = "AUBNE";
			testHeader.CreateAndLoadLicenceForOrg();
			AssertEquals("Licence Companies Company Country is AU", "AU", testHeader.LicCompany.LC_CompanyCountry);
		}

		public void TestCreateLicenceCompanyDefaults()
		{
			EDIOrgHeader header = HeaderForTest;
			header.CreateAndLoadLicenceForOrg();

			AssertEquals(header.PK, header.LicCompany.LC_OH);
			AssertEquals(header.LicEnterprise.PK, header.LicCompany.LC_LE);
			AssertEquals(header.OH_Code.Right(3), header.LicCompany.LC_CompanyCode);
			AssertEquals(header.UNLOCO.RL_RN_NKCountryCode, header.LicCompany.LC_CompanyCountry);
			AssertEquals(header.UNLOCO.Country.RN_RX_NKLocalCurrency, header.LicCompany.LC_RX_NKCurrency);
			AssertEquals("Is WHT Registered", header.CompanyData.OB_ARWHTApplicable, header.LicCompany.LC_IsWHTRegistered);

			AssertEquals("IS GST Registered", true, header.LicCompany.LC_IsGSTRegistered);
			AssertEquals("ReadOnly", true, header.LicCompany.LC_IsGSTRegisteredInfo.ReadOnly);
		}

		#endregion

		#region Licence Business Reg No

		public void TestLicenceBusinessRegistrationNumber()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "AUSYD";

			OrgCusCode zACode = testHeader.CustomsCodes.AddNew();
			zACode.OK_CodeType = "VAT";
			zACode.OK_RN_NKCodeCountry = "ZA"; //South Africa
			zACode.OK_CustomsRegNo = "4123123";

			OrgCusCode aUCode = testHeader.CustomsCodes.AddNew();
			aUCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			aUCode.OK_RN_NKCodeCountry = "AU"; //Australia
			aUCode.OK_CustomsRegNo = "22222222";

			AssertEquals("LicenceBusinessRegistrationNumber", "22222222", testHeader.LicenceTaxationRegNo);

			testHeader.OH_RL_NKClosestPort = "ZAJNB";
			AssertEquals("LicenceBusinessRegistrationNumber", "4123123", testHeader.LicenceTaxationRegNo);
		}

		public void TestLicenceBusinessRegistrationNumberType()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "";

			AssertEquals("No type because no UNLOCO specified", "", testHeader.LicenceBusinessRegNoType);

			testHeader.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Type", GlbCompany.LicenceBusinessRegNoType(testHeader.Country), testHeader.LicenceBusinessRegNoType);

			testHeader.OH_RL_NKClosestPort = "SKBTS";
			AssertEquals("Type", GlbCompany.LicenceBusinessRegNoType(testHeader.Country), testHeader.LicenceBusinessRegNoType);
		}

		#endregion

		#region Logging

		public void TestBusinessObjectsWithRelatedEvents()
		{
			EDIOrgHeader ediOrgHeader = Factory.New<EDIOrgHeader>();
			AssertCollectionContains("Related BizOs should contain", ediOrgHeader.LicEnterprise, ediOrgHeader.BusinessObjectsWithRelatedEvents);

			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_OH = ediOrgHeader.PK;
			AssertCollectionContains("Related BizOs should contain", company, ediOrgHeader.BusinessObjectsWithRelatedEvents);

			LicenceDatabase database = company.LicDatabases.AddNew();
			AssertCollectionContains("Related BizOs should contain", database, ediOrgHeader.BusinessObjectsWithRelatedEvents);

			var licHeader = company.GetHeader(database);
			AssertCollectionContains("Related BizOs should contain", licHeader, ediOrgHeader.BusinessObjectsWithRelatedEvents);
		}

		public void TestLogEventWhenKeyGenerated()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			testHeader.LicenceEnterpriseCode = "XBT";
			Factory.Save();

			testHeader.LicCompany.LC_CompanyCode = "COM";

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_ServerCode = "XXZ";
			db.LD_PublicEmailAddressForUpdate = "";
			db.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			db.LD_PublicEmailAddressForUpdate = "no-reply@edi.com.au";

			// 2nd database
			LicenceDatabase db2 = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader2 = testHeader.LicCompany.GetHeader(db2);
			db2.LD_Product = ProductTypes.Codes.Enterprise;
			db2.LD_ServerCode = "XXY";
			db2.LD_PublicEmailAddressForUpdate = "";
			db2.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			db2.LD_PublicEmailAddressForUpdate = "no-reply@edi.com.au";

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			db.LD_HL_CurrentRunningVersion = build.PK;
			Assert("Can AutoDeploy", testHeader.CanAutoDeployLicenceKey(licHeader));

			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			build2.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			db2.LD_HL_CurrentRunningVersion = build2.PK;
			Assert("Can AutoDeploy", testHeader.CanAutoDeployLicenceKey(licHeader));

			Factory.Save();

			StmALog[] keyGeneratedLogs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXZ Generated"));
			AssertEquals("No Licence Generated Logs", 0, keyGeneratedLogs.Length);
			StmALog[] keyGeneratedLogs2 = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXY Generated"));
			AssertEquals("No Licence Generated Logs", 0, keyGeneratedLogs2.Length);

			string fileName = "", fileName2 = "";

			try
			{
				fileName = testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader, false);
				keyGeneratedLogs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXZ Generated"));
				AssertEquals("No Licence Generated Log created", 0, keyGeneratedLogs.Length);

				fileName = testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader);

				keyGeneratedLogs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXZ Generated"));
				AssertEquals("1 Licence Generated Log created", 1, keyGeneratedLogs.Length);
				Assert(keyGeneratedLogs[0].IsInDatabase);

				testHeader.GenerateAndAutoDeployLicenceKey(licHeader);
				keyGeneratedLogs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXZ Generated"));
				AssertEquals("2 Licence Generated Logs created", 2, keyGeneratedLogs.Length);
				Assert(keyGeneratedLogs[0].IsInDatabase);
				Assert(keyGeneratedLogs[1].IsInDatabase);

				Factory.Save();
				keyGeneratedLogs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXZ Generated"));
				AssertEquals("2 Licence Generated Logs still there", 2, keyGeneratedLogs.Length);

				// 2nd database
				fileName2 = testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader2, false);
				keyGeneratedLogs2 = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXY Generated"));
				AssertEquals("No Licence Generated Log created", 0, keyGeneratedLogs2.Length);

				fileName2 = testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader2);

				keyGeneratedLogs2 = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXY Generated"));
				AssertEquals("1 Licence Generated Log created", 1, keyGeneratedLogs2.Length);
				Assert(keyGeneratedLogs2[0].IsInDatabase);

				testHeader.GenerateAndAutoDeployLicenceKey(licHeader2);
				keyGeneratedLogs2 = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXY Generated"));
				AssertEquals("2 Licence Generated Logs created", 2, keyGeneratedLogs2.Length);
				Assert(keyGeneratedLogs2[0].IsInDatabase);
				Assert(keyGeneratedLogs2[1].IsInDatabase);

				Factory.Save();
				keyGeneratedLogs2 = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Licence For XBTCOMXXY Generated"));
				AssertEquals("2 Licence Generated Logs still there", 2, keyGeneratedLogs2.Length);
			}
			finally
			{
				if (!string.IsNullOrEmpty(fileName))
				{
					File.Delete(fileName);
				}
				if (!string.IsNullOrEmpty(fileName2))
				{
					File.Delete(fileName2);
				}
			}
		}

		#endregion

		#region ReadOnly and Security

		public void TestReadOnlySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				Assert(!testHeader.LicenceEnterpriseCodeInfo.ReadOnly);
				Assert(!testHeader.LicenceEnterpriseIDInfo.ReadOnly);

				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				Assert(testHeader.LicenceEnterpriseCodeInfo.ReadOnly);
				Assert(testHeader.LicenceEnterpriseIDInfo.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		public void TestLicenceEnterpriseCode_ReadOnly()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;

			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				Assert(!testHeader.LicenceEnterpriseCodeInfo.ReadOnly);
				Assert(!testHeader.LicenceEnterpriseIDInfo.ReadOnly);

				LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
				LicenceCompany licCompany = Factory.NewWithValidTestData<LicenceCompany>();
				licCompany.LC_OH = testHeader.PK;
				licCompany.LC_LE = licEnt.PK;
				Assert(!testHeader.LicenceEnterpriseCodeInfo.ReadOnly);
				Assert(!testHeader.LicenceEnterpriseIDInfo.ReadOnly);

				Factory.Save();
				Assert("Enterprise Code is readonly", testHeader.LicenceEnterpriseCodeInfo.ReadOnly);
				Assert(testHeader.LicenceEnterpriseIDInfo.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		#endregion

		#region Name Change

		public void TestNameChange()
		{
			EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "baa@cargowise.com", "foo@cargowise.com" });

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			testHeader.LicenceEnterpriseCode = "XBT";

			GlbStaff.CurrentUser.GS_FullName = "Test User";
			GlbStaff.CurrentUser.GS_EmailAddress = "test.user@cargowise.com";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SLO";
			staff1.GS_EmailAddress = "sales.one@test.com";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ACM";
			staff2.GS_EmailAddress = "account.manager@test.com";

			OrgStaffAssignments assignment1 = testHeader.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			OrgStaffAssignments assignment2 = testHeader.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = StaffAssignmentRoles.Codes.AccountManager;

			Factory.Save();

			string originalCode = testHeader.OH_Code;

			var logs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("new org does not name change not logged", 0, logs.Length);

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			EDIOrgHeader copyOrg = otherFactory.Load<EDIOrgHeader>(testHeader.PK);
			copyOrg.OH_FullName = "New Name";
			otherFactory.Save();

			logs = copyOrg.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("Full Name change is logged", 1, logs.Length);
			AssertEquals("Full Name was: XB Test Organisation", logs[0].SL_Reference);

			AssertEquals(2, copyOrg.BrandsOrRelatedNames.Count);
			AssertEquals("XB Test Organisation", copyOrg.BrandsOrRelatedNames[0].P1_RelatedName);
			AssertEquals(originalCode, copyOrg.BrandsOrRelatedNames[1].P1_RelatedName);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Organization name change \"XB Test Organisation\" -> \"New Name\" by Test User (test.user@cargowise.com)", email.Subject);
			AssertEquals(4, email.Recipients.Count);
			AssertEquals("baa@cargowise.com", email.Recipients[0].Email);
			AssertEquals("foo@cargowise.com", email.Recipients[1].Email);
			AssertEquals("sales.one@test.com", email.Recipients[2].Email);
			AssertEquals("account.manager@test.com", email.Recipients[3].Email);
			AssertContains("XB Test Organisation -> New Name", email.Body);
			AssertContains(originalCode + " -> " + copyOrg.OH_Code, email.Body);
			AssertEquals(SupportIncident.MailFromName, email.FromDisplayName);
			AssertEquals(SupportIncident.MailFromAddress, email.FromAddress);

			// verify old brand names and code are unique
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			EDIOrgHeader org3 = factory3.Load<EDIOrgHeader>(testHeader.PK);
			org3.OH_FullName = "XB Test Organisation";
			factory3.Save();
			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			EDIOrgHeader org4 = factory4.Load<EDIOrgHeader>(testHeader.PK);
			org4.OH_FullName = "New Name";
			factory4.Save();
			AssertEquals(4, org4.BrandsOrRelatedNames.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			BusinessObjectFactory factory5 = new BusinessObjectFactory();
			EDIOrgHeader org5 = factory5.Load<EDIOrgHeader>(testHeader.PK);
			org5.OH_FullName = "Test Organisation Without Licence";
			factory5.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			org5.OH_FullName = "Test Organisation Without Licence NEW";
			org5.LicenceEnterpriseCode = string.Empty;
			factory5.Save();
			AssertEquals("No email should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			logs = org5.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was: Test Organisation Without Licence"));
			AssertEquals("Full Name change should be still logged", 1, logs.Length);
		}

		[ExpectNoExceptions]
		public void TestNameChangeNoReceipient()
		{
			EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>());

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			testHeader.LicenceEnterpriseCode = "XBT";

			GlbStaff.CurrentUser.GS_FullName = "Test User";
			GlbStaff.CurrentUser.GS_EmailAddress = "";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SLO";
			staff1.GS_EmailAddress = "";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ACM";
			staff2.GS_EmailAddress = "";

			OrgStaffAssignments assignment1 = testHeader.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			OrgStaffAssignments assignment2 = testHeader.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = StaffAssignmentRoles.Codes.AccountManager;

			Factory.Save();

			string originalCode = testHeader.OH_Code;

			var logs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("new org does not name change not logged", 0, logs.Length);

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			EDIOrgHeader copyOrg = otherFactory.Load<EDIOrgHeader>(testHeader.PK);
			copyOrg.OH_FullName = "New Name";
			otherFactory.Save();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNameChangeTranslogix()
		{
			OrgHeader transLogixOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			transLogixOrgHeader.OH_Code = "TRACUSSYD1";

			EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "hhh@cargowise.com" });
			EDIDataRegistry.Instance.TranslogixOrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "ccc@cargowise.com", "ggg@cargowise.com" });

			GlbStaff.CurrentUser.GS_FullName = "Test User";
			GlbStaff.CurrentUser.GS_EmailAddress = "test.user@cargowise.com";

			EDIOrgHeader testOrg = HeaderForTest;
			testOrg.OH_FullName = "ABC Test Organisation";
			testOrg.OH_RL_NKClosestPort = "AUSYD";
			testOrg.MainAddress.OA_Address1 = "88 Some St";
			testOrg.MainAddress.OA_City = "Sydney";

			testOrg.AddRelatedParty(transLogixOrgHeader.PK, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null);
			Factory.Save();

			AssertEquals(1, testOrg.AllRelatedParties.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			testOrg.OH_FullName = "New Name";
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var emailToTranslogix = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Translogix Organization name change \"ABC Test Organisation\" -> \"New Name\" by Test User (test.user@cargowise.com)", emailToTranslogix.Subject);
			AssertEquals(2, emailToTranslogix.Recipients.Count);
			AssertEquals("ccc@cargowise.com", emailToTranslogix.Recipients[0].Email);
			AssertEquals("ggg@cargowise.com", emailToTranslogix.Recipients[1].Email);
			AssertContains("ABC Test Organisation -> New Name", emailToTranslogix.Body);

			testOrg.CreateAndLoadLicenceForOrg();
			testOrg.GenerateNewLicenceCode();
			testOrg.LicenceEnterpriseCode = "XBT";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			testOrg.OH_FullName = "Another New Name";
			Factory.Save();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			var emailToCargowise = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Organization name change \"New Name\" -> \"Another New Name\" by Test User (test.user@cargowise.com)", emailToCargowise.Subject);
			AssertEquals(1, emailToCargowise.Recipients.Count);
			AssertEquals("hhh@cargowise.com", emailToCargowise.Recipients[0].Email);
			AssertContains("New Name -> Another New Name", emailToCargowise.Body);

			emailToTranslogix = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Translogix Organization name change \"New Name\" -> \"Another New Name\" by Test User (test.user@cargowise.com)", emailToTranslogix.Subject);
			AssertEquals(2, emailToTranslogix.Recipients.Count);
			AssertEquals("ccc@cargowise.com", emailToTranslogix.Recipients[0].Email);
			AssertEquals("ggg@cargowise.com", emailToTranslogix.Recipients[1].Email);
			AssertContains("New Name -> Another New Name", emailToTranslogix.Body);
		}

		public void TestNameChange_SpaceOrCaseChange()
		{
			EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "baa@cargowise.com", "foo@cargowise.com" });

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			testHeader.LicenceEnterpriseCode = "XBT";

			GlbStaff.CurrentUser.GS_FullName = "Test User";
			GlbStaff.CurrentUser.GS_EmailAddress = "test.user@cargowise.com";

			Factory.Save();
			string originalCode = testHeader.OH_Code;

			var logs = testHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("new org does not name change not logged", 0, logs.Length);

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			EDIOrgHeader copyOrg = otherFactory.Load<EDIOrgHeader>(testHeader.PK);
			copyOrg.OH_FullName = "XB	Test      Organisation		";
			otherFactory.Save();
			logs = copyOrg.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("name change not logged", 0, logs.Length);
			AssertEquals(0, copyOrg.BrandsOrRelatedNames.Count);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			copyOrg.OH_FullName = "xb TEST orGANiSatioN";
			otherFactory.Save();
			logs = copyOrg.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("name change not logged", 0, logs.Length);
			AssertEquals(0, copyOrg.BrandsOrRelatedNames.Count);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			copyOrg.OH_FullName = "XbTesT Organisation";
			otherFactory.Save();
			logs = copyOrg.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Full Name was"));
			AssertEquals("name change is logged", 1, logs.Length);
			AssertEquals("Old code and name", 2, copyOrg.BrandsOrRelatedNames.Count);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestOriginalNamesAndCodes()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "Test Organisation";
			org.OH_Code = "TSTORGSYD2";
			Factory.Save();

			org.OH_FullName = "Test Again Org";
			org.OH_Code = "TSTAGASYD";
			Factory.Save();

			org.OH_FullName = "IBM";
			org.OH_Code = "IBMSYD";
			Factory.Save();

			org.OH_FullName = "Original Organisation";
			org.OH_Code = "ORIORGSYD";
			Factory.Save();

			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			EDIOrgHeader loadedOrg = cleanFactory.Load<EDIOrgHeader>(org.PK);

			AssertEquals("Test Organisation, Test Again Org, IBM", loadedOrg.OriginalNames);
			AssertEquals("TSTORGSYD2, TSTAGASYD, IBMSYD", loadedOrg.OriginalCodes);
		}

		#endregion

		public void TestRelationshipManager()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "Test Organisation";
			org.OH_Code = "TSTORGSYD2";
			org.StaffAssignments.CompanySpecific = false;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();

			AssertEquals(ZString.Empty, org.RelationshipManager);

			org.StaffAssignments.SetStaffAssignment("MAN", staff1.GS_Code, "FIA");
			AssertEquals(ZString.Empty, org.RelationshipManager);

			org.StaffAssignments.SetStaffAssignment("RM1", staff2.GS_Code, OrgStaffAssignmentsLookups.AllServices);
			AssertEquals(staff2.GS_Code, org.RelationshipManager);

			using (branch1.SetAsTemporaryContext())
			{
				AssertEquals(staff2.GS_Code, org.RelationshipManager);
			}
		}

		public void TestSaveOrgHeaderForEmptyLicenceEnterprise()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_FullName = "Test Organisation1";
			org1.OH_Code = "TSTORGSYD1";

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_FullName = "Test Organisation2";
			org2.OH_Code = "TSTORGSYD2";

			org1.CreateAndLoadLicenceForOrg();
			AssertEquals(ZString.Empty, org1.LicenceEnterpriseCode);
			org1.LicEnterprise.LE_EnterpriseCode = "AAA";

			org2.CreateAndLoadLicenceForOrg();
			AssertEquals(ZString.Empty, org2.LicenceEnterpriseCode);
			org2.LicEnterprise.LE_EnterpriseCode = "BBB";

			Factory.Save();
		}

		public void TestLicenceEnterpriseAlreadySaved()
		{
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "AAA";
			Factory.Save();

			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_FullName = "Test Organisation1";
			org1.OH_Code = "TSTORGSYD1";
			org1.CreateAndLoadLicenceForOrg();
			org1.LicEnterprise.LE_EnterpriseCode = "AAA";
			Factory.Save();
			AssertEquals(org1.LicCompany.LicEnterprise, licEnterprise);
		}

		public void TestHasCurrentSupportContractOrNoActiveLicence()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";

			AssertNull("Org has no licence", org.LicCompany);
			Factory.Save();
			AssertEquals("Org has no licence", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has no licence", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			org.CreateAndLoadLicenceForOrg();
			var db1 = org.LicCompany.LicDatabases.AddNew();
			var licHeader = org.LicCompany.GetHeader(db1);
			db1.LD_ServerCode = "PRD";
			db1.LD_PublicEmailAddressForUpdate = "test@test.com";
			Factory.Save();
			AssertEquals("Only one database and no support expiry date", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and no support expiry date", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertEquals("Only one database and support expiry date is in the future", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and support expiry date is in the future", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertEquals("Only one database and support expiry date is in the past", false, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and support expiry date is in the past", false, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			var db2 = org.LicCompany.LicDatabases.AddNew();
			var licHeader2 = org.LicCompany.GetHeader(db2);
			db2.LD_ServerCode = "TST";
			db2.LD_PublicEmailAddressForUpdate = "test@test.com";
			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(5);
			Factory.Save();
			AssertEquals("At least one database has support expiry date in the future", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("At least one database has support expiry date in the future", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();
			AssertEquals("All databases have support expiry date in the past", false, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("All databases have support expiry date in the past", false, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			licHeader.LA_IsActive = false;
			db2.LD_IsActive = false;
			Factory.Save();
			AssertEquals("Org has no active databases", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has no active databases", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));

			licHeader.LA_IsActive = true;
			db2.LD_IsActive = true;
			licHeader2.LA_ContractExpiryDate = ZDateTime.Empty;
			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();
			AssertEquals("Org has at least one empty expiry date", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has at least one empty expiry date", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));
		}

		public void TestGetOrgsWithCurrentSupportContractOrNoActiveLicenceMultipleOrgs()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "AAAAGASYD";

			AssertNull("Precondition: Org has no licence", org.LicCompany);
			AssertNull("Precondition: Org has no licence", org2.LicCompany);
			Factory.Save();
			AssertEquals("Org has no licence", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has no licence", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			var hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Org has no licence", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Org has no licence", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			org.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			var db1 = org.LicCompany.LicDatabases.AddNew();
			var db21 = org2.LicCompany.LicDatabases.AddNew();
			var licHeader = org.LicCompany.GetHeader(db1);
			var licHeader21 = org2.LicCompany.GetHeader(db21);
			db1.LD_ServerCode = "PRD";
			db21.LD_ServerCode = "PRD";
			db1.LD_PublicEmailAddressForUpdate = "test@test.com";
			db21.LD_PublicEmailAddressForUpdate = "test@test.com";
			Factory.Save();
			AssertEquals("Only one database and no support expiry date", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and no support expiry date", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Only one database and no support expiry date", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Only one database and no support expiry date", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(1);
			licHeader21.LA_ContractExpiryDate = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertEquals("Only one database and support expiry date is in the future", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and support expiry date is in the future", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Only one database and support expiry date is in the future", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Only one database and support expiry date is in the future", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			licHeader21.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertEquals("Only one database and support expiry date is in the future", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Only one database and support expiry date is in the past", false, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Only one database and support expiry date is in the future", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Only one database and support expiry date is in the past", false, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			var db2 = org.LicCompany.LicDatabases.AddNew();
			var db22 = org2.LicCompany.LicDatabases.AddNew();
			var licHeader2 = org.LicCompany.GetHeader(db2);
			var licHeader22 = org2.LicCompany.GetHeader(db22);
			db2.LD_ServerCode = "TST";
			db22.LD_ServerCode = "TST";
			db2.LD_PublicEmailAddressForUpdate = "test@test.com";
			db22.LD_PublicEmailAddressForUpdate = "test@test.com";
			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(5);
			licHeader22.LA_ContractExpiryDate = ZDateTime.Now.AddDays(5);
			Factory.Save();
			AssertEquals("At least one database has support expiry date in the future", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("At least one database has support expiry date in the future", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("At least one database has support expiry date in the future", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("At least one database has support expiry date in the future", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			licHeader22.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();
			AssertEquals("All databases have support expiry date in the past", false, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("All databases have support expiry date in the past", false, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("All databases have support expiry date in the past", false, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("All databases have support expiry date in the past", false, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			licHeader.LA_IsActive = false;
			licHeader21.LA_IsActive = false;
			licHeader2.LA_IsActive = false;
			licHeader22.LA_IsActive = false;
			db2.LD_IsActive = false;
			db22.LD_IsActive = false;
			Factory.Save();
			AssertEquals("Org has no active databases", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has no active databases", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Org has no active databases", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Org has no active databases", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));

			licHeader.LA_IsActive = true;
			licHeader21.LA_IsActive = true;
			licHeader2.LA_IsActive = true;
			licHeader22.LA_IsActive = true;
			db2.LD_IsActive = true;
			db22.LD_IsActive = true;
			licHeader2.LA_ContractExpiryDate = ZDateTime.Empty;
			licHeader22.LA_ContractExpiryDate = ZDateTime.Empty;
			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			licHeader21.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();
			AssertEquals("Org has at least one empty expiry date", true, org.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has at least one empty expiry date", true, org2.HasCurrentSupportContractOrNoActiveLicence);
			AssertEquals("Org has at least one empty expiry date", true, EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()));
			hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Org has at least one empty expiry date", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Org has at least one empty expiry date", true, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));
		}

		#region Licence change marks org as changed

		public void TestMarkForSavingNoChanges()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			org.MarkForSaving();
			Factory.Save();

			var reloadedOrg = new BusinessObjectFactory { RefreshEnabled = false }.Load<EDIOrgHeader>(org.PK);

			AssertEquals(EDIOrgHeader.OrgForcedSaveCode, reloadedOrg.OH_SystemLastEditUser);
		}

		public void TestMarkForSavingWithChanges()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			org.MarkForSaving();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var reloadedOrg = new BusinessObjectFactory { RefreshEnabled = false }.Load<EDIOrgHeader>(org.PK);

			AssertNotEquals(EDIOrgHeader.OrgForcedSaveCode, reloadedOrg.OH_SystemLastEditUser);

			AssertEquals("Still should be saved", ScreeningStatusesList.Codes.Clear, reloadedOrg.OH_ScreeningStatus);
		}

		public void TestChangeLicenceCompanyUpdatesOrg()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			Assert(!org.IsMarkedForSaving);

			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_OH = org.PK;
			licCompany.LC_CompanyNumber = 100;

			Assert("Org should be marked for saving after new LicenceCompany is created for it.", org.IsMarkedForSaving);

			Factory.Save();

			Assert(!org.IsMarkedForSaving);

			licCompany.LC_CompanyNumber = 200;

			Assert("Org should be marked for saving after LicenceCompany is modified.", org.IsMarkedForSaving);
		}

		public void TestLinkLicenceUpdatesOrg()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_OH = org.PK;
			licCompany.LC_CompanyNumber = 100;
			Factory.Save();

			Assert(!org.IsMarkedForSaving);

			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_DatabaseNumber = 1003;
			var licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;

			Assert("Org should be marked for saving after new licence is linked to it.", org.IsMarkedForSaving);
		}

		public void TestChangeLicenceDatabaseUpdatesOrg()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_OH = org.PK;
			licCompany.LC_CompanyNumber = 100;
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_DatabaseNumber = 1003;
			var licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;

			Factory.Save();

			Assert(!org.IsMarkedForSaving);

			licDatabase.LD_DatabaseNumber = 2003;

			Assert("Org should be marked for saving after LicenceDatabase is modified.", org.IsMarkedForSaving);
		}

		public void TestUnlinkLicenceUpdatesOrg()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_OH = org.PK;
			licCompany.LC_CompanyNumber = 100;
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_DatabaseNumber = 1003;
			var licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;

			Factory.Save();

			Assert(!org.IsMarkedForSaving);

			licHeader.Delete();

			Assert("Org should be marked for saving after licence is unlinked.", org.IsMarkedForSaving);
		}

		public void TestMoveLicenceUpdatesOldAndNewOrgs()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany1 = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany1.LC_OH = org1.PK;
			licCompany1.LC_CompanyNumber = 100;
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_DatabaseNumber = 1003;
			var licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_LC = licCompany1.PK;
			licHeader.LA_LD = licDatabase.PK;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany2 = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany2.LC_OH = org2.PK;
			licCompany2.LC_CompanyNumber = 200;

			Factory.Save();

			Assert(!org1.IsMarkedForSaving);
			Assert(!org2.IsMarkedForSaving);

			licHeader.LA_LC = licCompany2.PK;

			Assert("Org should be marked for saving after licence is unlinked.", org1.IsMarkedForSaving);
			Assert("Org should be marked for saving after new licence is linked to it.", org2.IsMarkedForSaving);
		}

		#endregion

		public void TestLicenceDatabaseCount()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "AAA", "COM");
			licCompany.LC_OH = org.PK;

			Factory.Save();

			AssertNotNull(org.LicCompany);
			AssertNotNull(org.LicCompany.LicDatabases);

			AssertEquals(0, org.LicenceDatabaseCount);

			org.LicCompany.LicDatabases.AddNew();
			org.LicCompany.LicDatabases.AddNew();

			AssertEquals(2, org.LicenceDatabaseCount);
		}

		public void TestEnableLightValidationIfAvailable()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			AssertEquals(true, org.LightValidationEnabled);
			AssertEquals(false, org.EnableLightValidationIfAvailableOverriden.HasValue);

			org.EnableLightValidationIfAvailableOverriden = false;
			AssertEquals(false, org.LightValidationEnabled);
			org.EnableLightValidationIfAvailableOverriden = true;
			AssertEquals(true, org.LightValidationEnabled);
		}

		public void TestModifyOrgContactShouldNotQueryDocumentForContactsNotModified()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var orgContact1 = org.Contacts.AddNew();
			orgContact1.OC_ContactName = "orgContact1";
			orgContact1.OC_IsActive = true;
			var orgContact2 = org.Contacts.AddNew();
			orgContact2.OC_ContactName = "orgContact2";
			orgContact2.OC_IsActive = true;

			Factory.Save();

			var testFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgHeader1 = testFactory1.LoadTop1<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, org.PK));
			var contact1 = orgHeader1.Contacts.FirstOrDefault(c => c.PK == orgContact1.PK);
			((EDIOrgContact)contact1).OC_Title = "Title 1";
			var contact2 = orgHeader1.Contacts.FirstOrDefault(c => c.PK == orgContact2.PK);
			((EDIOrgContact)contact2).OC_Title = "Title 2";
			testFactory1.Save();
			AssertEquals(2, testFactory1.GetTableHitCount(OrgDocumentSchema.Constants.TableName));

			var testFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgHeader2 = testFactory2.LoadTop1<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, org.PK));
			var contact11 = orgHeader2.Contacts.FirstOrDefault(c => c.PK == orgContact1.PK);
			((EDIOrgContact)contact11).OC_Title = "Title 11";
			testFactory2.Save();
			AssertEquals(1, testFactory2.GetTableHitCount(OrgDocumentSchema.Constants.TableName));
		}

		public void TestDeleteEDIOrgHeader()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			AssertNull(org.LicCompany);
			AssertNull(org.LicEnterprise);
			org.CreateAndLoadLicenceForOrg();
			Factory.Save();

			AssertNotNull(org.LicEnterprise);
			var reloadedOrg = new BusinessObjectFactory { RefreshEnabled = false }.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, org.PK));
			AssertEquals(1, reloadedOrg.Length);
			org.Delete();
			Factory.Save();
			var reloadedEnterpriseAfterDelete = new BusinessObjectFactory { RefreshEnabled = false }.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, org.PK));
			var reloadedCompanyAfterDelete = new BusinessObjectFactory { RefreshEnabled = false }.Load<LicenceCompany>(new ZQuery(LicenceCompanySchema.LC_OH, org.PK));
			AssertEquals(0, reloadedCompanyAfterDelete.Length);
			AssertEquals(0, reloadedEnterpriseAfterDelete.Length);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint = EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed;
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = false;
		}

		bool OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint;

		protected override void TearDown()
		{
			base.TearDown();
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.OH_RL_NKClosestPort = "AUSYD";
			return org;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.OH_RL_NKClosestPort = "AUSYD";
			return org;
		}

		protected virtual EDIOrgHeader GetHeader(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<EDIOrgHeader>();
		}

		EDIOrgHeader CreateHeader(BusinessObjectFactory inputFactory, string code, string uNLOCO)
		{
			EDIOrgHeader fHeaderForTest = GetHeader(inputFactory);
			fHeaderForTest.MainAddress.OA_Address1 = "TestAddress";
			fHeaderForTest.OH_Code = code;
			fHeaderForTest.OH_RL_NKClosestPort = uNLOCO;
			fHeaderForTest.MainAddress.OA_Phone = "+61426829924";

			return fHeaderForTest;
		}

		public EDIOrgHeader HeaderForTest
		{
			get { return CreateHeader(Factory, "TGBLOG", "AUBNE"); }
		}

		#endregion
	}
}
