using System.Collections;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalProcedureCodesLookupGetsCorrectValues()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "53", "00", "D20", "this description", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure("CDS", "A", "40", "00", "C24", "is spread", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "A", "53", "00", "002", "across many", "IMP", group: "IFD");
			var procedure4 = helper.CreateRefCusProcedure("CDS", "A", "40", "00", "IDP", "different cus procedures", "IMP", group: "IFD");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = "IMP";
			declaration.JE_DeclarationType = "IFD";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "5300D20";

			var additionalProcedureCode = invoiceLine1.AdditionalProcedureCodes.AddNew();

			AssertEquals(0, invoiceLine1.MaxNumberOfAdditionalProcedureCode);
			Assert("CPC List", additionalProcedureCode.Lookups.CY_CodeList.ContainsCode(procedure3.FullCodeCurrentPlusPreviousPlusConcession));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure1));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure2));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure4));

			declaration.JE_ApplicationCode = "CDS";
			invoiceLine1.JI_Procedure = "4000C24";
			declaration.CusEntryInstruction.CEI_Style = "IFD";
			AssertEquals(98, invoiceLine1.MaxNumberOfAdditionalProcedureCode);
			Assert("CPC List", additionalProcedureCode.Lookups.CY_CodeList.ContainsCode(procedure4.FullCodeCurrentPlusPreviousPlusConcession));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure1));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure2));
			Assert("CPC List should not contain", !additionalProcedureCode.Lookups.CY_CodeList.Contains(procedure3));
		}

		public void TestPrimaryPreferenceList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gbId = helper.CreateNewOrGetExistingDataGrouping("GB");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, parent: gbId);

			var cusPref1 = helper.CreatePreferenceForCountry("140", "Exemption for End-Use Resulting from the CCT", Env.CurrentCompany.Country.Code);
			var cusPref2 = helper.CreatePreferenceForCountry("200", "GSP Rate Without Conditions Or Limits (Including Ceilings)", Env.CurrentCompany.Country.Code);
			var cusPref3 = helper.CreatePreferenceForCountry("500", "500Description", "CDS");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = header.PK;
			declaration.JE_ApplicationCode = "CHF";
			var preferencesListGB = invoiceLine.Lookups.PrimaryPreferenceList;
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref1, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref2, preferencesListGB);
			AssertCollectionNotContainsCusRefPreferenceViewPk(cusPref3, preferencesListGB);

			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;
			var tradeGroup = helper.CreateTradeGroup(Env.CurrentCompany.Country.Code, "TEST1", date1, date2);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Env.CurrentCompany.Country.Code, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Env.CurrentCompany.Country.Code, Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			Factory.Save();
			var cusTariff = helper.CreateTariff(Env.CurrentCompany.Country.Code, hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: cusPref1.PK);
			helper.CreateCusApplicability(testRate1, tradeGroup, date1, date2, "add11", "ord11");
			var testRate2 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: cusPref2.PK);
			helper.CreateCusApplicability(testRate2, tradeGroup, date1, date2, "add21", "ord21");
			var testRate3 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: cusPref3.PK);
			helper.CreateCusApplicability(testRate3, tradeGroup, date1, date2, "add31", "ord31");
			Factory.Save();

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CDS";

			invoiceLine.JI_Tariff = "123456789";
			preferencesListGB = invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals("No CountryOfOrigin and get all preferences", 3, preferencesListGB.Count);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref3, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref2, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref1, preferencesListGB);

			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			preferencesListGB = invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals("No JI_Tariff and get all preferences", 3, preferencesListGB.Count);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref3, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref2, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref1, preferencesListGB);

			invoiceLine.JI_Tariff = "999999999";
			preferencesListGB = invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals("No UniveralTariff and get empty preference", 0, preferencesListGB.Count);

			invoiceLine.JI_Tariff = "123456789";
			preferencesListGB = invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals("had UniveralTariff and get related preferences", 3, preferencesListGB.Count);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref3, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref2, preferencesListGB);
			AssertCollectionContainsCusRefPreferenceViewPk(cusPref1, preferencesListGB);
		}

		public void TestValuationCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = new JobComInvoiceLineLookups(invoiceLine);

			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.ValuationCodeList, lookups.ValuationCodeList);
				AssertEquals("CodesAsString", "1, 2, 3, 4, 5, 6, 7", lookups.ValuationCodeList.CodesAsString);
			});
		}

		void AssertCollectionContainsCusRefPreferenceViewPk(CusRefPreferenceView preference, IEnumerable collection)
		{
			AssertCollectionContains(preference, collection);
		}

		void AssertCollectionNotContainsCusRefPreferenceViewPk(CusRefPreferenceView preference, IEnumerable collection)
		{
			AssertCollectionNotContains(preference, collection);
		}

		public void TestCountriesOfDestinationCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cl010 = UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010;
			helper.CreateCusCodeList("CDS", cl010, "GB", "United Kingdom, Great Britain, Northern Ireland", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("CDS", cl010, "FR", "France", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("GB", cl010, "GB", "Great Britain", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			Factory.Save();

			var cdsDeclaration = Factory.New<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceLineWithCDSDeclaration = cdsDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = invoiceLineWithCDSDeclaration.AddInfoLookups;
			var countriesOfDestination = (ZZRefCusCodeListCombinedCollection)lookups.CountriesOfDestination;
			countriesOfDestination.Load();

			CombineAssertions(() =>
			{
				AssertEquals("Count of CDS countries", 2, countriesOfDestination.Count);
				AssertSame("Cached list of CDS countries", countriesOfDestination, lookups.CountriesOfDestination);
				AssertEquals("Country GB exists in CDS list", "United Kingdom, Great Britain, Northern Ireland", countriesOfDestination.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "GB").ZZD_Description);
			});
		}

		public void TestCountryOfExportList()
		{
			SetUpImportCountryRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.JE_EntryStyle = "IM";
			AssertEquals("When EntryStyle is IM, CountryOfExportList CodesAsString", "ZZ", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);

			declaration.JE_EntryStyle = "CO";
			AssertEquals("When EntryStyle is CO, CountryOfExportList CodesAsString", "CC", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);

			declaration.JE_EntryStyle = "EU";
			AssertEquals("When EntryStyle is EU, CountryOfExportList CodesAsString", "EE", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);
		}

		void SetUpImportCountryRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("CDS", parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "Origin country/territory for entry style IM");
			helper.CreateCusCodeList("CDS", UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "ZZ", "Test ZZ", ZDateTime.Now.AddMonths(-2), ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "Origin country/territory for entry style CO");
			helper.CreateCusCodeListWithAttribute("CDS", UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "CC", "Test CC", ZDateTime.Now.AddMonths(-2), ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "Origin country/territory for entry style EU");
			helper.CreateCusCodeListWithAttribute("CDS", UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "EE", "Test EE", ZDateTime.Now.AddMonths(-2), ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			Factory.Save();
		}
	}
}
