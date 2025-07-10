using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EdiCommissionAgreementCustomizationAutoAdderTest : TestCaseWithFactory
	{
		public void TestExecute_LicenceDatabase()
		{
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licDatabase1 = licEnterprise.Databases.AddNew();
			licDatabase1.LD_LE = licEnterprise.PK;
			licDatabase1.LD_ServerCode = "111";
			licDatabase1.FillWithValidTestData();

			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var licCompanyA = licEnterprise.Companies.AddNew();
			licCompanyA.LC_LE = licEnterprise.PK;
			licCompanyA.LC_OH = orgA.PK;
			licCompanyA.FillWithValidTestData();

			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			licEnterprise.LE_OH = orgB.PK;

			var agreementAautoAddDb = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAautoAddDb.CA0_OH_Customer = orgA.PK;
			var customizationAAutoAddDb = agreementAautoAddDb.GetOrCreateCustomization();
			customizationAAutoAddDb.EZN_IsAllCompanies = false;
			customizationAAutoAddDb.EZN_IsAllDatabases = false;
			customizationAAutoAddDb.DatabasePivots.AddNew(null);

			var agreementAcompanyAutoAddDb = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAcompanyAutoAddDb.CA0_OH_Customer = orgA.PK;
			var customizationAcompanyAutoAddDb = agreementAcompanyAutoAddDb.GetOrCreateCustomization();
			customizationAcompanyAutoAddDb.EZN_IsAllCompanies = false;
			customizationAcompanyAutoAddDb.EZN_IsAllDatabases = false;
			customizationAcompanyAutoAddDb.CompanyAutoAddDatabases.AddNew(ZGuid.Empty);

			var agreementAcompanyAutoAddCountry = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAcompanyAutoAddCountry.CA0_OH_Customer = orgA.PK;
			var customizationAcompanyAutoAddCountry = agreementAcompanyAutoAddCountry.GetOrCreateCustomization();
			customizationAcompanyAutoAddCountry.EZN_IsAllCompanies = false;
			customizationAcompanyAutoAddCountry.EZN_IsAllDatabases = false;
			customizationAcompanyAutoAddCountry.CompanyAutoAddCountries.AddNew(ZGuid.Empty, "AU");

			var agreementBcompanyAutoAddDb = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementBcompanyAutoAddDb.CA0_OH_Customer = orgB.PK;
			var customizationBcompanyAutoAdd = agreementBcompanyAutoAddDb.GetOrCreateCustomization();
			customizationBcompanyAutoAdd.EZN_IsAllCompanies = false;
			customizationBcompanyAutoAdd.EZN_IsAllDatabases = false;
			customizationBcompanyAutoAdd.CompanyAutoAddDatabases.AddNew(ZGuid.Empty);

			var agreementBnoAutoAdd = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementBnoAutoAdd.CA0_OH_Customer = orgB.PK;
			var customizationBnoAutoAdd = agreementBnoAutoAdd.GetOrCreateCustomization();
			customizationBnoAutoAdd.EZN_IsAllCompanies = false;
			customizationBnoAutoAdd.EZN_IsAllDatabases = false;

			var agreementOtherEnteprise = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementOtherEnteprise.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var customizationOtherEnteprise = agreementOtherEnteprise.GetOrCreateCustomization();
			customizationOtherEnteprise.EZN_IsAllCompanies = false;
			customizationOtherEnteprise.EZN_IsAllDatabases = false;
			customizationOtherEnteprise.CompanyAutoAddDatabases.AddNew(ZGuid.Empty);

			Factory.Save();

			var newLicenceDatabase = licEnterprise.Databases.AddNew();
			newLicenceDatabase.LD_ServerCode = "222";
			newLicenceDatabase.LD_LE = licEnterprise.PK;

			var autoAdder = new EdiCommissionAgreementCustomizationAutoAdder(Factory);
			autoAdder.Execute(newLicenceDatabase);

			AssertCustomizationInfo(customizationAAutoAddDb,
				new[] { null, newLicenceDatabase },
				Array.Empty<LicenceDatabase>(),
				Array.Empty<Tuple<LicenceDatabase, ZString>>());

			AssertCustomizationInfo(customizationAcompanyAutoAddDb,
				Array.Empty<LicenceDatabase>(),
				new[]
				{
					null,
					newLicenceDatabase
				},
				Array.Empty<Tuple<LicenceDatabase, ZString>>());

			AssertCustomizationInfo(customizationAcompanyAutoAddCountry,
				Array.Empty<LicenceDatabase>(),
				Array.Empty<LicenceDatabase>(),
				new[]
				{
					Tuple.Create((LicenceDatabase)null, (ZString)"AU"),
					Tuple.Create(newLicenceDatabase, (ZString)"AU")
				});

			AssertCustomizationInfo(customizationBcompanyAutoAdd,
				Array.Empty<LicenceDatabase>(),
				new[]
				{
					null,
					newLicenceDatabase
				},
				Array.Empty<Tuple<LicenceDatabase, ZString>>());

			AssertCustomizationInfo(customizationBnoAutoAdd,
				Array.Empty<LicenceDatabase>(),
				Array.Empty<LicenceDatabase>(),
				Array.Empty<Tuple<LicenceDatabase, ZString>>());

			AssertCustomizationInfo(customizationOtherEnteprise,
				Array.Empty<LicenceDatabase>(),
				new LicenceDatabase[] { null },
				Array.Empty<Tuple<LicenceDatabase, ZString>>());
		}

		public void TestExecute_ClientCompany()
		{
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licDatabase1 = licEnterprise.Databases.AddNew();
			licDatabase1.LD_LE = licEnterprise.PK;
			licDatabase1.LD_ServerCode = "111";
			licDatabase1.FillWithValidTestData();
			var licDatabase2 = licEnterprise.Databases.AddNew();
			licDatabase2.LD_LE = licEnterprise.PK;
			licDatabase2.LD_ServerCode = "222";
			licDatabase2.FillWithValidTestData();

			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var licCompanyA = licEnterprise.Companies.AddNew();
			licCompanyA.LC_LE = licEnterprise.PK;
			licCompanyA.LC_OH = orgA.PK;
			licCompanyA.FillWithValidTestData();

			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			licEnterprise.LE_OH = orgB.PK;

			var agreementAautoAddDb1Au = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAautoAddDb1Au.CA0_OH_Customer = orgA.PK;
			agreementAautoAddDb1Au.Approve();
			var customizationAAutoAddDb1Au = agreementAautoAddDb1Au.GetOrCreateCustomization();
			customizationAAutoAddDb1Au.EZN_IsAllCompanies = false;
			customizationAAutoAddDb1Au.EZN_IsAllDatabases = false;
			customizationAAutoAddDb1Au.CompanyAutoAddCountries.AddNew(licDatabase1, "AU");

			var agreementAautoAddDb2Au = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAautoAddDb2Au.CA0_OH_Customer = orgA.PK;
			var customizationAAutoAddDb2Au = agreementAautoAddDb2Au.GetOrCreateCustomization();
			customizationAAutoAddDb2Au.EZN_IsAllCompanies = false;
			customizationAAutoAddDb2Au.EZN_IsAllDatabases = false;
			customizationAAutoAddDb2Au.CompanyAutoAddCountries.AddNew(licDatabase2, "AU");

			var agreementAautoAddDb1Us = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAautoAddDb1Us.CA0_OH_Customer = orgA.PK;
			var customizationAAutoAddDb1Us = agreementAautoAddDb1Us.GetOrCreateCustomization();
			customizationAAutoAddDb1Us.EZN_IsAllCompanies = false;
			customizationAAutoAddDb1Us.EZN_IsAllDatabases = false;
			customizationAAutoAddDb1Us.CompanyAutoAddCountries.AddNew(licDatabase1, "US");

			var agreementAautoAddDb1 = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementAautoAddDb1.CA0_OH_Customer = orgA.PK;
			var customizationAAutoAddDb1 = agreementAautoAddDb1.GetOrCreateCustomization();
			customizationAAutoAddDb1.EZN_IsAllCompanies = false;
			customizationAAutoAddDb1.EZN_IsAllDatabases = false;
			customizationAAutoAddDb1.CompanyAutoAddDatabases.AddNew(licDatabase1);

			var agreementBautoAddDb1 = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementBautoAddDb1.CA0_OH_Customer = orgB.PK;
			agreementBautoAddDb1.Approve();
			var customizationBAutoAddDb1 = agreementBautoAddDb1.GetOrCreateCustomization();
			customizationBAutoAddDb1.EZN_IsAllCompanies = false;
			customizationBAutoAddDb1.EZN_IsAllDatabases = false;
			customizationBAutoAddDb1.CompanyAutoAddDatabases.AddNew(licDatabase1);

			var agreementBnoAutoAdd = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementBnoAutoAdd.CA0_OH_Customer = orgB.PK;
			var customizationBnoAutoAdd = agreementBnoAutoAdd.GetOrCreateCustomization();
			customizationBnoAutoAdd.EZN_IsAllCompanies = false;
			customizationBnoAutoAdd.EZN_IsAllDatabases = false;

			var agreementOtherEnteprise = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreementOtherEnteprise.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var customizationOtherEnteprise = agreementOtherEnteprise.GetOrCreateCustomization();
			customizationOtherEnteprise.EZN_IsAllCompanies = false;
			customizationOtherEnteprise.EZN_IsAllDatabases = false;
			customizationOtherEnteprise.CompanyAutoAddCountries.AddNew(licDatabase1, "AU");

			Factory.Save();

			var newClientCompany = Factory.New<ClientCompany>();
			newClientCompany.LCC_RN_NKCountryCode = "AU";
			newClientCompany.LCC_LD = licDatabase1.PK;

			var autoAdder = new EdiCommissionAgreementCustomizationAutoAdder(Factory);
			autoAdder.Execute(newClientCompany);

			AssertCustomizationCompniesInfo("Should add because same db and country", customizationAAutoAddDb1Au, new[] { newClientCompany });
			AssertCustomizationCompniesInfo("Should not add because different database", customizationAAutoAddDb2Au, Array.Empty<ClientCompany>());
			AssertCustomizationCompniesInfo("Should not add because different country", customizationAAutoAddDb1Us, Array.Empty<ClientCompany>());
			AssertCustomizationCompniesInfo("Should add because same db and country is all", customizationAAutoAddDb1, new[] { newClientCompany });
			AssertCustomizationCompniesInfo("Should add because same db and country is all", customizationBAutoAddDb1, new[] { newClientCompany });
			AssertCustomizationCompniesInfo("Should not add because no auto-add settings", customizationBnoAutoAdd, Array.Empty<ClientCompany>());
			AssertCustomizationCompniesInfo("Should not add because different enterprise", customizationOtherEnteprise, Array.Empty<ClientCompany>());

			AssertEquals(false, agreementAautoAddDb1Au.IsDraft);
			AssertEquals(true, agreementAautoAddDb1.IsDraft);
			AssertEquals(false, agreementBautoAddDb1.IsDraft);
		}

		#region Implementation

		static void AssertCustomizationInfo(EdiCommissionAgreementCustomization customzation,
			IEnumerable<LicenceDatabase> expectedDatabases,
			IEnumerable<LicenceDatabase> expectedCompanyAutoAddDatabases,
			IEnumerable<Tuple<LicenceDatabase, ZString>> expectedCompanyAutoAddCompanies)
		{
			AssertContainsExactElementsInAnyOrder("DatabasePivots",
				x => x != null ? x.LD_ServerCode.ToString() : "null",
				expectedDatabases,
				customzation.DatabasePivots.Select(x => x.LicenceDatabase));

			AssertContainsExactElementsInAnyOrder("CompanyAutoAddDatabases",
				x => x != null ? x.LD_ServerCode.ToString() : "null",
				expectedCompanyAutoAddDatabases,
				customzation.CompanyAutoAddDatabases.Select(x => x.LicenceDatabase));

			AssertContainsExactElementsInAnyOrder("CompanyAutoAddCountries",
				x => string.Format(CultureInfo.CurrentCulture, "Database:[{0}] Country:[{1}]", x.Item1 != null ? x.Item1.LD_ServerCode.ToString() : "null", x.Item2),
				expectedCompanyAutoAddCompanies,
				customzation.CompanyAutoAddCountries.Select(x => Tuple.Create(x.LicenceDatabase, x.EPC_RN_NKCountry)));
		}

		static void AssertCustomizationCompniesInfo(string message, EdiCommissionAgreementCustomization customzation,
			IEnumerable<ClientCompany> expectedCompanies)
		{
			AssertContainsExactElementsInAnyOrder(message,
				x => x != null ? x.LCC_Name.ToString() : "null",
				expectedCompanies,
				customzation.CompanyPivots.Select(x => x.ClientCompany));
		}

		#endregion
	}
}