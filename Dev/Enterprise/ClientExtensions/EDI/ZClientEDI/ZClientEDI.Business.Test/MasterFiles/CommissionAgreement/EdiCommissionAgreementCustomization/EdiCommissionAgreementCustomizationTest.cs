using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCustomization))]
	class EdiCommissionAgreementCustomizationTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestEZN_IsAllDatabases()
		{
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllDatabases = false;
			customization.DatabasePivots.AddNew(licenceDatabase);

			customization.EZN_IsAllDatabases = true;
			AssertEquals(0, customization.DatabasePivots.Count);
			AssertEquals(false, licenceDatabase.IsDeleted);
		}

		public void TestEZN_IsAllCompanies()
		{
			var clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllCompanies = false;
			customization.CompanyPivots.AddNew(clientCompany);
			customization.CompanyAutoAddDatabases.AddNew(ZGuid.Empty);
			customization.CompanyAutoAddCountries.AddNew(ZGuid.Empty, "AU");

			customization.EZN_IsAllCompanies = true;
			AssertEquals(0, customization.CompanyPivots.Count);
			AssertEquals(false, clientCompany.IsDeleted);
			AssertEquals(0, customization.CompanyAutoAddDatabases.Count);
			AssertEquals(0, customization.CompanyAutoAddCountries.Count);
		}

		#endregion

		#region Draft

		public void TestCreateAndMergeDraft()
		{
			var company = Factory.New<ClientCompany>();
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var clientCompany = Factory.New<ClientCompany>();

			var agreement = Factory.New<EdiCommissionAgreement>();
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllCompanies = false;
			customization.EZN_IsAllDatabases = false;

			customization.CompanyPivots.AddNew(company);
			customization.CompanyAutoAddCountries.AddNew(null, "AU");
			customization.CompanyAutoAddDatabases.AddNew(licenceDatabase);
			customization.DatabasePivots.AddNew(licenceDatabase);

			var draft = (EdiCommissionAgreement)agreement.CreateDraft();
			var draftCustomization = draft.Customization;

			AssertEquals(false, draftCustomization.EZN_IsAllCompanies);
			AssertEquals(false, draftCustomization.EZN_IsAllDatabases);
			AssertContainsExactElementsInAnyOrder(new[] { company }, draftCustomization.CompanyPivots.Select(x => x.ClientCompany));
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase }, draftCustomization.DatabasePivots.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create((LicenceDatabase)null, "AU") }, draftCustomization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.LicenceDatabase, x.EPC_RN_NKCountry.ToString())));
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase }, draftCustomization.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));

			var company2 = Factory.New<ClientCompany>();
			var licenceDatabase2 = Factory.New<LicenceDatabase>();

			draftCustomization.CompanyPivots.AddNew(company2);
			draftCustomization.CompanyAutoAddCountries.AddNew(licenceDatabase2, "US");
			draftCustomization.CompanyAutoAddDatabases.AddNew(licenceDatabase2);
			draftCustomization.DatabasePivots.AddNew(licenceDatabase2);

			draft.ApproveDraft();

			AssertEquals(true, draft.IsDeleted);

			AssertEquals(false, customization.EZN_IsAllCompanies);
			AssertEquals(false, customization.EZN_IsAllDatabases);
			AssertContainsExactElementsInAnyOrder(new[] { company, company2 }, customization.CompanyPivots.Select(x => x.ClientCompany));
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase, licenceDatabase2 }, customization.DatabasePivots.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create((LicenceDatabase)null, "AU"), Tuple.Create(licenceDatabase2, "US") }, customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.LicenceDatabase, x.EPC_RN_NKCountry.ToString())));
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase, licenceDatabase2 }, customization.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));
		}

		public void TestCreateAndMergeDraft_ForNewCustomization()
		{
			var agreement = Factory.New<EdiCommissionAgreement>();

			var draft = (EdiCommissionAgreement)agreement.CreateDraft();
			var draftCustomization = draft.GetOrCreateCustomization();
			draftCustomization.EZN_IsAllCompanies = true;
			draftCustomization.EZN_IsAllDatabases = false;

			draft.ApproveDraft();

			AssertEquals(true, draft.IsDeleted);

			var customization = agreement.Customization;
			AssertNotNull(customization);
			AssertEquals(true, customization.EZN_IsAllCompanies);
			AssertEquals(false, customization.EZN_IsAllDatabases);
		}

		#endregion

		#region Customer

		public void TestLicenceEnterprise()
		{
			var orgA = Factory.New<OrgHeader>();
			var orgB = Factory.New<OrgHeader>();
			var orgC = Factory.New<OrgHeader>();

			var licEnterprise1 = Factory.New<LicenceEnterprise>();
			licEnterprise1.LE_OH = orgC.PK;
			var licCompany1 = licEnterprise1.Companies.AddNew();
			licCompany1.LC_LE = licEnterprise1.PK;
			licCompany1.LC_OH = orgA.PK;

			var licEnterprise2 = Factory.New<LicenceEnterprise>();
			licEnterprise2.LE_OH = orgB.PK;

			var licEnterprise3 = Factory.New<LicenceEnterprise>();
			var licCompany3 = licEnterprise3.Companies.AddNew();
			licCompany3.LC_LE = licEnterprise3.PK;
			licCompany3.LC_OH = orgC.PK;

			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = ZGuid.Empty;
			var customization = agreement.GetOrCreateCustomization();
			AssertNull(customization.LicenceEnterprise);

			agreement.CA0_OH_Customer = orgA.PK;
			AssertEquals(licEnterprise1, customization.LicenceEnterprise);

			agreement.CA0_OH_Customer = orgB.PK;
			AssertEquals("Should return LicenceEnterprise org if no LicenceCompany exists", licEnterprise2, customization.LicenceEnterprise);

			agreement.CA0_OH_Customer = orgC.PK;
			AssertEquals("LicenceCompany enterprise should take precendence over LicenceEnterprise org", licEnterprise3, customization.LicenceEnterprise);
		}

		#endregion

		#region Related Business Objects

		public void TestRemoveAllRelatedBusinessObjectsNotBelongingToLicenceEnterprise()
		{
			var licEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licCompany1 = licEnterprise1.Companies.AddNew();
			licCompany1.LC_CompanyCode = "CO1";
			licCompany1.LC_LE = licEnterprise1.PK;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			licCompany1.LC_OH = org1.PK;

			var database1A = licEnterprise1.Databases.AddNew();
			database1A.LD_LE = licEnterprise1.PK;
			database1A.LD_ServerCode = "D1A";
			var clientCompany1Ai = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1Ai.LCC_LD = database1A.PK;
			var clientCompany1Aii = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1Aii.LCC_LD = database1A.PK;

			var database1B = licEnterprise1.Databases.AddNew();
			database1B.LD_LE = licEnterprise1.PK;
			database1B.LD_ServerCode = "D1B";
			var clientCompany1B = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1B.LCC_LD = database1B.PK;

			var licEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licCompany2 = licEnterprise2.Companies.AddNew();
			licCompany2.LC_CompanyCode = "CO2";
			licCompany2.LC_LE = licEnterprise2.PK;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			licCompany2.LC_OH = org2.PK;

			var database2A = licEnterprise2.Databases.AddNew();
			database2A.LD_LE = licEnterprise2.PK;
			database2A.LD_ServerCode = "D2A";
			var clientCompany2A = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2A.LCC_LD = database2A.PK;

			Factory.Save();

			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org1.PK;
			var customization = agreement.GetOrCreateCustomization();

			// add customizations for agreement org's licence enterprise
			customization.CompanyPivots.AddNew(clientCompany1Ai);
			customization.CompanyAutoAddDatabases.AddNew(null);
			customization.CompanyAutoAddDatabases.AddNew(database1A);
			customization.CompanyAutoAddCountries.AddNew(null, "AU");
			customization.CompanyAutoAddCountries.AddNew(database1B, "US");
			customization.DatabasePivots.AddNew(null);
			customization.DatabasePivots.AddNew(database1A);

			// add customizations for a different licence enterprise
			customization.CompanyPivots.AddNew(clientCompany2A);
			customization.CompanyAutoAddDatabases.AddNew(database2A);
			customization.CompanyAutoAddCountries.AddNew(database2A, "GB");
			customization.DatabasePivots.AddNew(database2A);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Should have removed company pivots for non-licEnterprise1 companies", new[] { clientCompany1Ai }, customization.CompanyPivots.Select(x => x.ClientCompany));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add databases for non-licEnterprise1 databases", new[] { null, database1A }, customization.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add countries for non-licEnterprise1 databases", new[] { null, database1B }, customization.CompanyAutoAddCountries.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed database pivots for non-licEnterprise1 databases", new[] { null, database1A }, customization.DatabasePivots.Select(x => x.LicenceDatabase));

			agreement.CA0_OH_Customer = org2.PK;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Should have removed company pivots for non-licEnterprise2 companies", Enumerable.Empty<ClientCompany>(), customization.CompanyPivots.Select(x => x.ClientCompany));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add databases for non-licEnterprise2 databases", new LicenceDatabase[] { null }, customization.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add countries for non-licEnterprise2 databases", new LicenceDatabase[] { null }, customization.CompanyAutoAddCountries.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed database pivots for non-licEnterprise2 databases", new LicenceDatabase[] { null }, customization.DatabasePivots.Select(x => x.LicenceDatabase));

			var orgWithoutLicenceEnterprise = Factory.NewWithValidTestData<OrgHeader>();
			agreement.CA0_OH_Customer = orgWithoutLicenceEnterprise.PK;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Should have removed all company pivots", Enumerable.Empty<ClientCompany>(), customization.CompanyPivots.Select(x => x.ClientCompany));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add databases that aren't for new", new LicenceDatabase[] { null }, customization.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed auto-add countries that aren't for new", new LicenceDatabase[] { null }, customization.CompanyAutoAddCountries.Select(x => x.LicenceDatabase));
			AssertContainsExactElementsInAnyOrder("Should have removed database pivots that aren't for new", new LicenceDatabase[] { null }, customization.DatabasePivots.Select(x => x.LicenceDatabase));
		}

		#endregion

		#region Logs

		public void TestLogs_Properties()
		{
			var customization = Factory.NewWithValidTestData<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllCompanies = true;
			customization.EZN_IsAllDatabases = true;

			Factory.Save();
			var modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save", 0, modifiedLogs.Length);

			customization.EZN_IsAllCompanies = false;
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("All Company Usages: Y > N", modifiedLogs[0].SL_Reference);

			customization.EZN_IsAllDatabases = false;
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("All Database Usages: Y > N", modifiedLogs[1].SL_Reference);
		}

		public void TestLogs_Companies()
		{
			var customization = Factory.NewWithValidTestData<EdiCommissionAgreementCustomization>();

			Factory.Save();
			var modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save", 0, modifiedLogs.Length);

			var company = Factory.NewWithValidTestData<ClientCompany>();
			company.LCC_Code = "MCL";
			company.LCC_Name = "My Cars Ltd";
			var companyPivot = customization.CompanyPivots.AddNew(company);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Company Added: My Cars Ltd (MCL)", modifiedLogs[0].SL_Reference);

			customization.CompanyPivots.Delete(companyPivot);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Company Removed: My Cars Ltd (MCL)", modifiedLogs[1].SL_Reference);

			companyPivot = customization.CompanyPivots.AddNew(company);
			customization.CompanyPivots.Delete(companyPivot);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not add log when add and then remove", 2, modifiedLogs.Length);
		}

		public void TestLogs_Databases()
		{
			var customization = Factory.NewWithValidTestData<EdiCommissionAgreementCustomization>();

			Factory.Save();
			var modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save", 0, modifiedLogs.Length);

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ServerCode = "AAA";
			var databasePivot = customization.DatabasePivots.AddNew(database);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Included AAA Database Usages", modifiedLogs[0].SL_Reference);

			customization.DatabasePivots.Delete(databasePivot);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Excluded AAA Database Usages", modifiedLogs[1].SL_Reference);

			databasePivot = customization.DatabasePivots.AddNew(database);
			customization.DatabasePivots.Delete(databasePivot);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not add log when add and then remove", 2, modifiedLogs.Length);
		}

		public void TestLogs_CompanyAutoAddCountries()
		{
			var customization = Factory.NewWithValidTestData<EdiCommissionAgreementCustomization>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ServerCode = "AAA";

			Factory.Save();
			var modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save", 0, modifiedLogs.Length);

			var autoAdd = customization.CompanyAutoAddCountries.AddNew(database.PK, "AU");
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Enabled Auto-add New Companies in AU Country (AAA Database)", modifiedLogs[0].SL_Reference);

			customization.CompanyAutoAddCountries.AddNew(ZGuid.Empty, "AU");
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Enabled Auto-add New Companies in AU Country (New Databases)", modifiedLogs[1].SL_Reference);

			customization.CompanyAutoAddCountries.Delete(autoAdd);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(3, modifiedLogs.Length);
			AssertEquals("Disabled Auto-add New Companies in AU Country (AAA Database)", modifiedLogs[2].SL_Reference);

			autoAdd = customization.CompanyAutoAddCountries.AddNew(database.PK, "AU");
			customization.CompanyAutoAddCountries.Delete(autoAdd);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not add log when add and then remove", 3, modifiedLogs.Length);
		}

		public void TestLogs_CompanyAutoAddDatabases()
		{
			var customization = Factory.NewWithValidTestData<EdiCommissionAgreementCustomization>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ServerCode = "AAA";

			Factory.Save();
			var modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save", 0, modifiedLogs.Length);

			var autoAdd = customization.CompanyAutoAddDatabases.AddNew(database.PK);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Enabled Auto-add All New Companies in AAA Database", modifiedLogs[0].SL_Reference);

			customization.CompanyAutoAddDatabases.AddNew(ZGuid.Empty);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Enabled Auto-add All New Companies in New Databases", modifiedLogs[1].SL_Reference);

			customization.CompanyAutoAddDatabases.Delete(autoAdd);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(3, modifiedLogs.Length);
			AssertEquals("Disabled Auto-add All New Companies in AAA Database", modifiedLogs[2].SL_Reference);

			autoAdd = customization.CompanyAutoAddDatabases.AddNew(database.PK);
			customization.CompanyAutoAddDatabases.Delete(autoAdd);
			Factory.Save();
			modifiedLogs = customization.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not add log when add and then remove", 3, modifiedLogs.Length);
		}

		public void TestModifiedLogs_AttachDetach()
		{
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();

			Factory.Save();

			var customization = agreement.GetOrCreateCustomization();
			Factory.Save();
			var modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Added Customer Filters", modifiedLogs[0].SL_Reference);

			customization.Delete();
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Deleted Customer Filters", modifiedLogs[1].SL_Reference);
		}

		#endregion
	}
}
