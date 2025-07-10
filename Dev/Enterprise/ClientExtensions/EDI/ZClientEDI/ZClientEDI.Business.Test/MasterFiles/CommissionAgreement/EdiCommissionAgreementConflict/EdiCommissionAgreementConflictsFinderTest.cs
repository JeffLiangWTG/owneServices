using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EdiCommissionAgreementConflictsFinderTest : TestCaseWithFactory
	{
		#region New

		public void TestNew()
		{
			AssertType(typeof(EdiCommissionAgreementConflictsFinder), CommissionAgreementConflictsFinder.New());
		}

		#endregion

		#region GetMoreGenericCommissionAgreementConflicts

		public void TestGetMoreGenericCommissionAgreementConflicts_CompanyAndDatabaseConflicts()
		{
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licCompany = licEnterprise.Companies.AddNew();
			licCompany.LC_CompanyCode = "CO1";
			licCompany.LC_LE = licEnterprise.PK;
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = customer.PK;

			var licenceDatabase1 = licEnterprise.Databases.AddNew();
			licenceDatabase1.LD_LE = licEnterprise.PK;
			licenceDatabase1.LD_ServerCode = "111";
			var licenceDatabase2 = licEnterprise.Databases.AddNew();
			licenceDatabase2.LD_LE = licEnterprise.PK;
			licenceDatabase2.LD_ServerCode = "222";
			var clientCompanyA = Factory.NewWithValidTestData<ClientCompany>();
			clientCompanyA.LCC_LD = licenceDatabase1.PK;
			clientCompanyA.LCC_Code = "AAA";
			var clientCompanyB = Factory.NewWithValidTestData<ClientCompany>();
			clientCompanyB.LCC_LD = licenceDatabase1.PK;
			clientCompanyB.LCC_Code = "BBB";

			var opportunityCompanyAEnt = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementCompanyAEnt = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityCompanyAEnt, customer, "ENT", "ALL", "ALL");
			agreementCompanyAEnt.CA0_Name = "CompanyAEnt";
			var agreementCompanyAEntCustomization = agreementCompanyAEnt.GetOrCreateCustomization();
			agreementCompanyAEntCustomization.EZN_IsAllCompanies = false;
			agreementCompanyAEntCustomization.CompanyPivots.AddNew(clientCompanyA);
			agreementCompanyAEntCustomization.EZN_IsAllDatabases = true;

			var opportunityCompanyABEntOdm = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementCompanyABEntOdm = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityCompanyABEntOdm, customer, "ENT", "ODM", "ALL");
			agreementCompanyABEntOdm.CA0_Name = "CompanyABEntOdm";
			var agreementCompanyABEntOdmCustomization = agreementCompanyABEntOdm.GetOrCreateCustomization();
			agreementCompanyABEntOdmCustomization.EZN_IsAllCompanies = false;
			agreementCompanyABEntOdmCustomization.CompanyPivots.AddNew(clientCompanyA);
			agreementCompanyABEntOdmCustomization.CompanyPivots.AddNew(clientCompanyB);
			agreementCompanyABEntOdmCustomization.EZN_IsAllDatabases = false;

			var opportunityDatabase12Ent = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementDatabase12Ent = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityDatabase12Ent, customer, "ENT", "ALL", "ALL");
			agreementDatabase12Ent.CA0_Name = "Database12Ent";
			var agreementDatabase12EntCustomization = agreementDatabase12Ent.GetOrCreateCustomization();
			agreementDatabase12EntCustomization.EZN_IsAllCompanies = false;
			agreementDatabase12EntCustomization.EZN_IsAllDatabases = false;
			agreementDatabase12EntCustomization.DatabasePivots.AddNew(licenceDatabase1);
			agreementDatabase12EntCustomization.DatabasePivots.AddNew(licenceDatabase2);

			var opportunityGlobalEnt = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalEnt = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalEnt, customer, "ENT", "ALL", "ALL");
			agreementGlobalEnt.CA0_Name = "GlobalEnt";

			var opportunityGlobalEntOdm = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalEntOdm = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalEntOdm, customer, "ENT", "ODM", "ALL");
			agreementGlobalEntOdm.CA0_Name = "GlobalEntOdm";

			var opportunityGlobalEntPav = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalEntPav = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalEntPav, customer, "ENT", "PAV", "ALL");
			agreementGlobalEntPav.CA0_Name = "GlobalEntPav";

			var opportunityGlobalSph = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalSph = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalSph, customer, "SPH", "ALL", "ALL");
			agreementGlobalSph.CA0_Name = "GlobalSph";

			Factory.Save();

			AssertCompanyAndDatabaseConflicts(opportunityCompanyAEnt,
				new[]
				{
					Tuple.Create(clientCompanyA, agreementGlobalEnt),
					Tuple.Create(clientCompanyA, agreementGlobalEntOdm),
					Tuple.Create(clientCompanyA, agreementGlobalEntPav),
				},
				new[]
				{
					Tuple.Create((LicenceDatabase)null, agreementGlobalEnt),
				});

			AssertCompanyAndDatabaseConflicts(opportunityCompanyABEntOdm,
				new[]
				{
					Tuple.Create(clientCompanyA, agreementCompanyAEnt),
					Tuple.Create(clientCompanyB, agreementGlobalEnt),
					Tuple.Create(clientCompanyB, agreementGlobalEntOdm),
				},
				Array.Empty<Tuple<LicenceDatabase, EdiCommissionAgreement>>());

			AssertCompanyAndDatabaseConflicts(opportunityDatabase12Ent,
				Array.Empty<Tuple<ClientCompany, EdiCommissionAgreement>>(),
				new[]
				{
					Tuple.Create(licenceDatabase1, agreementCompanyAEnt),
					Tuple.Create(licenceDatabase1, agreementGlobalEnt),
					Tuple.Create(licenceDatabase1, agreementGlobalEntOdm),
					Tuple.Create(licenceDatabase1, agreementGlobalEntPav),

					Tuple.Create(licenceDatabase2, agreementCompanyAEnt),
					Tuple.Create(licenceDatabase2, agreementGlobalEnt),
					Tuple.Create(licenceDatabase2, agreementGlobalEntOdm),
					Tuple.Create(licenceDatabase2, agreementGlobalEntPav),
				});
		}

		public void TestGetMoreGenericCommissionAgreementConflicts_CompanyAutoAddConflicts()
		{
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licCompany = licEnterprise.Companies.AddNew();
			licCompany.LC_CompanyCode = "CO1";
			licCompany.LC_LE = licEnterprise.PK;
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = customer.PK;

			var licenceDatabase1 = licEnterprise.Databases.AddNew();
			licenceDatabase1.LD_LE = licEnterprise.PK;
			licenceDatabase1.LD_ServerCode = "111";

			var opportunityDb1All = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementDb1All = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityDb1All, customer, "ALL", "ALL", "ALL");
			agreementDb1All.CA0_Name = "Db1All";
			var agreementDb1AllCustomization = agreementDb1All.GetOrCreateCustomization();
			agreementDb1AllCustomization.EZN_IsAllCompanies = false;
			agreementDb1AllCustomization.CompanyAutoAddDatabases.AddNew(licenceDatabase1.PK);

			var opportunityDb1Ent = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementDb1Ent = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityDb1Ent, customer, "ENT", "ALL", "ALL");
			agreementDb1Ent.CA0_Name = "Db1Ent";
			var agreementDb1EntCustomization = agreementDb1Ent.GetOrCreateCustomization();
			agreementDb1EntCustomization.EZN_IsAllCompanies = false;
			agreementDb1EntCustomization.CompanyAutoAddDatabases.AddNew(licenceDatabase1.PK);

			var opportunityDb1AuEnt = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementDb1AuEnt = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityDb1AuEnt, customer, "ENT", "ALL", "ALL");
			agreementDb1AuEnt.CA0_Name = "Db1AuEnt";
			var agreementDb1AuEntCustomization = agreementDb1AuEnt.GetOrCreateCustomization();
			agreementDb1AuEntCustomization.EZN_IsAllCompanies = false;
			agreementDb1AuEntCustomization.CompanyAutoAddCountries.AddNew(licenceDatabase1, "AU");

			var opportunityGlobalEnt = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalEnt = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalEnt, customer, "ENT", "ALL", "ALL");
			agreementGlobalEnt.CA0_Name = "GlobalEnt";

			var opportunityGlobalEntPav = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalEntPav = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalEntPav, customer, "ENT", "PAV", "ALL");
			agreementGlobalEntPav.CA0_Name = "GlobalEntPav";

			var opportunityGlobalSph = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreementGlobalSph = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunityGlobalSph, customer, "SPH", "ALL", "ALL");
			agreementGlobalSph.CA0_Name = "GlobalSph";

			Factory.Save();

			AssertCompanyAutoAddConflicts(opportunityDb1All,
				Array.Empty<Tuple<LicenceDatabase, string, EdiCommissionAgreement>>(),
				new[]
				{
					Tuple.Create(licenceDatabase1, agreementGlobalEnt),
					Tuple.Create(licenceDatabase1, agreementGlobalEntPav),
					Tuple.Create(licenceDatabase1, agreementGlobalSph),
				});

			AssertCompanyAutoAddConflicts(opportunityDb1Ent,
				Array.Empty<Tuple<LicenceDatabase, string, EdiCommissionAgreement>>(),
				new[]
				{
					Tuple.Create(licenceDatabase1, agreementDb1All),
				});

			AssertCompanyAutoAddConflicts(opportunityDb1AuEnt,
				new[]
				{
					Tuple.Create(licenceDatabase1, "AU", agreementDb1Ent),
				},
				Array.Empty<Tuple<LicenceDatabase, EdiCommissionAgreement>>());

			AssertCompanyAutoAddConflicts(opportunityGlobalEnt,
				Array.Empty<Tuple<LicenceDatabase, string, EdiCommissionAgreement>>(),
				Array.Empty<Tuple<LicenceDatabase, EdiCommissionAgreement>>());

			AssertCompanyAutoAddConflicts(opportunityGlobalEntPav,
				Array.Empty<Tuple<LicenceDatabase, string, EdiCommissionAgreement>>(),
				Array.Empty<Tuple<LicenceDatabase, EdiCommissionAgreement>>());

			AssertCompanyAutoAddConflicts(opportunityGlobalSph,
				Array.Empty<Tuple<LicenceDatabase, string, EdiCommissionAgreement>>(),
				Array.Empty<Tuple<LicenceDatabase, EdiCommissionAgreement>>());
		}

		static void AssertCompanyAndDatabaseConflicts(OrgOpportunity opportunity,
				IEnumerable<Tuple<ClientCompany, EdiCommissionAgreement>> expectedCompanyConflicts,
				IEnumerable<Tuple<LicenceDatabase, EdiCommissionAgreement>> expectedDatabaseConflicts)
		{
			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var conflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false);

			AssertContainsExactElementsInAnyOrder("CompanyConflicts",
					x => ((x.Item1 != null ? x.Item1.LCC_Code.ToString() : "null") + " " + x.Item2.CA0_Name),
					expectedCompanyConflicts,
					conflicts.OfType<CommissionAgreementItemAndCompanyConflict>().Select(x => Tuple.Create(x.ClientCompany, (EdiCommissionAgreement)x.LoserCommissionAgreement)));

			AssertContainsExactElementsInAnyOrder("DatabaseConflicts",
				x => ((x.Item1 != null ? x.Item1.LD_ServerCode.ToString() : "null") + " " + x.Item2.CA0_Name),
				expectedDatabaseConflicts,
				conflicts.OfType<CommissionAgreementItemAndDatabaseConflict>().Select(x => Tuple.Create(x.LicenceDatabase, (EdiCommissionAgreement)x.LoserCommissionAgreement)));
		}

		static void AssertCompanyAutoAddConflicts(OrgOpportunity opportunity, IEnumerable<Tuple<LicenceDatabase, string, EdiCommissionAgreement>> expectedCompanyAutoAddCountryConflicts, IEnumerable<Tuple<LicenceDatabase, EdiCommissionAgreement>> expectedCompanyAutoAddDatabaseConflicts)
		{
			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var conflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false);

			AssertContainsExactElementsInAnyOrder("CompanyAutoAddCountryConflicts",
					x => ((x.Item1 != null ? x.Item1.LD_ServerCode.ToString() : "null") + " " + x.Item2 + " " + x.Item3.CA0_Name),
					expectedCompanyAutoAddCountryConflicts,
					conflicts.OfType<CommissionAgreementItemAndCompanyAutoAddCountryConflict>().Select(x => Tuple.Create(x.LicenceDatabase, x.CountryCode.ToString(), (EdiCommissionAgreement)x.LoserCommissionAgreement)));

			AssertContainsExactElementsInAnyOrder("CompanyAutoAddDatabaseConflicts",
					x => ((x.Item1 != null ? x.Item1.LD_ServerCode.ToString() : "null") + " " + x.Item2.CA0_Name),
					expectedCompanyAutoAddDatabaseConflicts,
					conflicts.OfType<CommissionAgreementItemAndCompanyAutoAddDatabaseConflict>().Select(x => Tuple.Create(x.LicenceDatabase, (EdiCommissionAgreement)x.LoserCommissionAgreement)));
		}

		#endregion

		#region Find Duplicates

		public void TestFindDuplicates_CompaniesAndDatabases()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var licDatabase1 = licEnterprise.Databases.AddNew();
			var licDatabase2 = licEnterprise.Databases.AddNew();
			var company = licEnterprise.Companies.AddNew();
			var org = Factory.New<OrgHeader>();
			company.LC_OH = org.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = licDatabase1.PK;
			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licDatabase2.PK;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, org);
			var agreementAllCompaniesAllDatabases = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			var agreementAllCompaniesAllDatabasesENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAllCompaniesAllDatabases, "ENT", "ALL", "ALL");
			var agreementAllCompaniesAllDatabasesALLALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAllCompaniesAllDatabases, "ALL", "ALL", "ALL");

			var agreementCompany1Database1 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			var agreementCompany1Database1Customization = agreementCompany1Database1.GetOrCreateCustomization();
			agreementCompany1Database1Customization.EZN_IsAllCompanies = false;
			agreementCompany1Database1Customization.CompanyPivots.AddNew(clientCompany1);
			agreementCompany1Database1Customization.EZN_IsAllDatabases = false;
			agreementCompany1Database1Customization.DatabasePivots.AddNew(licDatabase1);
			var agreementCompany1Database1ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementCompany1Database1, "ENT", "ALL", "ALL");
			var agreementCompany1Database1ALLALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementCompany1Database1, "ALL", "ALL", "ALL");

			var agreementCompany2Database2 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			var agreementCompany2Database2Customization = agreementCompany2Database2.GetOrCreateCustomization();
			agreementCompany2Database2Customization.EZN_IsAllCompanies = false;
			agreementCompany2Database2Customization.CompanyPivots.AddNew(clientCompany2);
			agreementCompany2Database2Customization.EZN_IsAllDatabases = false;
			agreementCompany2Database2Customization.DatabasePivots.AddNew(licDatabase2);
			var agreementCompany2Database2ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementCompany2Database2, "ENT", "ALL", "ALL");

			var agreementCompany12Database12 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			var agreementCompany12Database12Customization = agreementCompany12Database12.GetOrCreateCustomization();
			agreementCompany12Database12Customization.EZN_IsAllCompanies = false;
			agreementCompany12Database12Customization.CompanyPivots.AddNew(clientCompany1);
			agreementCompany12Database12Customization.CompanyPivots.AddNew(clientCompany2);
			agreementCompany12Database12Customization.EZN_IsAllDatabases = false;
			agreementCompany12Database12Customization.DatabasePivots.AddNew(licDatabase1);
			agreementCompany12Database12Customization.DatabasePivots.AddNew(licDatabase2);
			var agreementCompany12Database12ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementCompany12Database12, "ENT", "ALL", "ALL");

			var agreementNoCompanyNoDatabase = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			var agreementNoCompanyNoDatabaseCustomization = agreementNoCompanyNoDatabase.GetOrCreateCustomization();
			agreementNoCompanyNoDatabaseCustomization.EZN_IsAllCompanies = false;
			agreementNoCompanyNoDatabaseCustomization.EZN_IsAllDatabases = false;
			var agreementNoCompanyNoDatabaseENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementNoCompanyNoDatabase, "ENT", "ALL", "ALL");

			AssertCompanyAndDatabaseDuplications(
				agreementAllCompaniesAllDatabasesENTALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, ClientCompany>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAndDatabaseDuplications(
				agreementAllCompaniesAllDatabasesALLALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, ClientCompany>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAndDatabaseDuplications(
				agreementCompany1Database1ENTALLALL,
				new[] { Tuple.Create(agreementCompany12Database12ENTALLALL, clientCompany1) },
				new[] { Tuple.Create(agreementCompany12Database12ENTALLALL, licDatabase1) });

			AssertCompanyAndDatabaseDuplications(
				agreementCompany1Database1ALLALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, ClientCompany>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAndDatabaseDuplications(
				agreementCompany2Database2ENTALLALL,
				new[] { Tuple.Create(agreementCompany12Database12ENTALLALL, clientCompany2) },
				new[] { Tuple.Create(agreementCompany12Database12ENTALLALL, licDatabase2) });

			AssertCompanyAndDatabaseDuplications(
				agreementCompany12Database12ENTALLALL,
				new[]
				{
					Tuple.Create(agreementCompany1Database1ENTALLALL, clientCompany1),
					Tuple.Create(agreementCompany2Database2ENTALLALL, clientCompany2)
				},
				new[]
				{
					Tuple.Create(agreementCompany1Database1ENTALLALL, licDatabase1),
					Tuple.Create(agreementCompany2Database2ENTALLALL, licDatabase2)
				});

			AssertCompanyAndDatabaseDuplications(
				agreementNoCompanyNoDatabaseENTALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, ClientCompany>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());
		}

		public void TestFindDuplicates_CompanyAutoAdds()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var licDatabase1 = licEnterprise.Databases.AddNew();
			var licDatabase2 = licEnterprise.Databases.AddNew();
			var company = licEnterprise.Companies.AddNew();
			var org = Factory.New<OrgHeader>();
			company.LC_OH = org.PK;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, org);
			var agreementAllCountryAllDb = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			agreementAllCountryAllDb.CA0_Name = "All";
			var agreementAllCountryAllDbENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAllCountryAllDb, "ENT", "ALL", "ALL");
			var agreementAllCountryAllDbALLALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAllCountryAllDb, "ALL", "ALL", "ALL");

			var agreementAuDb1 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			agreementAuDb1.CA0_Name = "AuDb1";
			var agreementAuDb1Customization = agreementAuDb1.GetOrCreateCustomization();
			agreementAuDb1Customization.EZN_IsAllCompanies = false;
			agreementAuDb1Customization.CompanyAutoAddCountries.AddNew(null, "AU");
			agreementAuDb1Customization.CompanyAutoAddDatabases.AddNew(licDatabase1);
			var agreementAuDb1ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAuDb1, "ENT", "ALL", "ALL");
			var agreementAuDb1ALLALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAuDb1, "ALL", "ALL", "ALL");

			var agreementUsDb2 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			agreementUsDb2.CA0_Name = "UsDb2";
			var agreementUsDb2Customization = agreementUsDb2.GetOrCreateCustomization();
			agreementUsDb2Customization.EZN_IsAllCompanies = false;
			agreementUsDb2Customization.CompanyAutoAddCountries.AddNew(null, "US");
			agreementUsDb2Customization.CompanyAutoAddDatabases.AddNew(licDatabase2);
			var agreementUsDb2ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementUsDb2, "ENT", "ALL", "ALL");

			var agreementAuUsDb12 = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			agreementAuUsDb12.CA0_Name = "AuUsDb12";
			var agreementAuUsDb12Customization = agreementAuUsDb12.GetOrCreateCustomization();
			agreementAuUsDb12Customization.EZN_IsAllCompanies = false;
			agreementAuUsDb12Customization.CompanyAutoAddCountries.AddNew(null, "AU");
			agreementAuUsDb12Customization.CompanyAutoAddCountries.AddNew(null, "US");
			agreementAuUsDb12Customization.CompanyAutoAddDatabases.AddNew(licDatabase1);
			agreementAuUsDb12Customization.CompanyAutoAddDatabases.AddNew(licDatabase2);
			var agreementAuUsDb12ENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementAuUsDb12, "ENT", "ALL", "ALL");

			var agreementNoCountryNoDatabase = (EdiCommissionAgreement)OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, org);
			agreementNoCountryNoDatabase.CA0_Name = "None";
			var agreementNoCountryNoDatabaseCustomization = agreementNoCountryNoDatabase.GetOrCreateCustomization();
			agreementNoCountryNoDatabaseCustomization.EZN_IsAllCompanies = false;
			var agreementNoCountryNoDatabaseENTALLALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementNoCountryNoDatabase, "ENT", "ALL", "ALL");

			AssertCompanyAutoAddDuplications(
				agreementAllCountryAllDbENTALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase, string>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAutoAddDuplications(
				agreementAllCountryAllDbALLALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase, string>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAutoAddDuplications(
				agreementAuDb1ENTALLALL,
				new[] { Tuple.Create(agreementAuUsDb12ENTALLALL, (LicenceDatabase)null, "AU") },
				new[] { Tuple.Create(agreementAuUsDb12ENTALLALL, licDatabase1) });

			AssertCompanyAutoAddDuplications(
				agreementAuDb1ALLALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase, string>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());

			AssertCompanyAutoAddDuplications(
				agreementUsDb2ENTALLALL,
				new[] { Tuple.Create(agreementAuUsDb12ENTALLALL, (LicenceDatabase)null, "US") },
				new[] { Tuple.Create(agreementAuUsDb12ENTALLALL, licDatabase2) });

			AssertCompanyAutoAddDuplications(
				agreementAuUsDb12ENTALLALL,
				new[]
				{
					Tuple.Create(agreementAuDb1ENTALLALL, (LicenceDatabase)null, "AU"),
					Tuple.Create(agreementUsDb2ENTALLALL, (LicenceDatabase)null, "US")
				},
				new[]
				{
					Tuple.Create(agreementAuDb1ENTALLALL, licDatabase1),
					Tuple.Create(agreementUsDb2ENTALLALL, licDatabase2)
				});

			AssertCompanyAutoAddDuplications(
				agreementNoCountryNoDatabaseENTALLALL,
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase, string>>(),
				Array.Empty<Tuple<OrgCommissionAgreementItem, LicenceDatabase>>());
		}

		static void AssertCompanyAndDatabaseDuplications(OrgCommissionAgreementItem item, IEnumerable<Tuple<OrgCommissionAgreementItem, ClientCompany>> expectedCompanyDuplications, IEnumerable<Tuple<OrgCommissionAgreementItem, LicenceDatabase>> expectedDatabaseDuplications)
		{
			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var duplications = conflictsFinder.FindDuplicates(item);
			AssertContainsExactElementsInAnyOrder(item.CommissionAgreement.CA0_Name + " CommissionAgreementItemAndCompanyDuplications",
				x => x.Item1.CommissionAgreement.CA0_Name + " " + (x.Item2 != null ? x.Item2.LCC_Code : ZString.Empty),
				expectedCompanyDuplications,
				duplications.OfType<CommissionAgreementItemAndCompanyDuplication>().Select(x => Tuple.Create(x.CommissionAgreementItem, x.ClientCompany)));

			AssertContainsExactElementsInAnyOrder(item.CommissionAgreement.CA0_Name + " CommissionAgreementItemAndDatabaseDuplications",
				x => x.Item1.CommissionAgreement.CA0_Name + " " + (x.Item2 != null ? x.Item2.LD_ServerCode : ZString.Empty),
				expectedDatabaseDuplications,
				duplications.OfType<CommissionAgreementItemAndDatabaseDuplication>().Select(x => Tuple.Create(x.CommissionAgreementItem, x.LicenceDatabase)));
		}

		static void AssertCompanyAutoAddDuplications(OrgCommissionAgreementItem item, IEnumerable<Tuple<OrgCommissionAgreementItem, LicenceDatabase, string>> expectedAutoAddCountryDuplications, IEnumerable<Tuple<OrgCommissionAgreementItem, LicenceDatabase>> expectedAutoAddDatabaseDuplications)
		{
			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var duplications = conflictsFinder.FindDuplicates(item);
			AssertContainsExactElementsInAnyOrder(item.CommissionAgreement.CA0_Name + " CompanyAutoAddCountryDuplications",
				x => x.Item1.CommissionAgreement.CA0_Name + " " + (x.Item2 != null ? x.Item2.LD_ServerCode : ZString.Empty) + " " + x.Item3,
				expectedAutoAddCountryDuplications,
				duplications.OfType<CommissionAgreementItemAndCompanyAutoAddCountryDuplication>().Select(x => Tuple.Create(x.CommissionAgreementItem, x.LicenceDatabase, x.CountryCode.ToString())));

			AssertContainsExactElementsInAnyOrder(item.CommissionAgreement.CA0_Name + " CompanyAutoAddDatabaseDuplications",
				x => x.Item1.CommissionAgreement.CA0_Name + " " + (x.Item2 != null ? x.Item2.LD_ServerCode : ZString.Empty),
				expectedAutoAddDatabaseDuplications,
				duplications.OfType<CommissionAgreementItemAndCompanyAutoAddDatabaseDuplication>().Select(x => Tuple.Create(x.CommissionAgreementItem, x.LicenceDatabase)));
		}

		#endregion

		#region GetMoreSpecificCommissionAgreementConflicts

		public void TestGetMoreSpecificCommissionAgreementConflicts_ALLProduct()
		{
			var customerA = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OpportunityID = "O00001001";
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customerA.PK;
			agreement1.FillWithValidTestData();

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OpportunityID = "O00001002";
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customerA.PK;
			agreement2.FillWithValidTestData();

			Factory.Save();

			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "ENT", "ODM", "ALL");
			var item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "ENT", "ALL", "ALL");

			item1.Validation.ValidateCAI_Code();
			AssertHasWarning(item1.CAI_CodeInfo,
$@"This will not include the following subset of commissions as more specific commission agreements already exist:

ENT > ALL > ALL

{agreement1.AgreementId}
  [All Company Usages]
      ENT > ODM > ALL
  [All Database Usages]
      ENT > ODM > ALL


To allow more specific Product / Service / Module subsets, click ""Add Filters (View Filters)"" and ensure the database and company filters have the same settings as above."
			);
		}

		#endregion
	}
}