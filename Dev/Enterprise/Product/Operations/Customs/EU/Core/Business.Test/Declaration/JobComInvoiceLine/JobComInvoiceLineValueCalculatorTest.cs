using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobComInvoiceLineValueCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When parameter is null", () => { new JobComInvoiceLineValueCalculator(null); });
			AssertNoExceptionThrown("When parameter is not null", () => { new JobComInvoiceLineValueCalculator(Factory.New<JobComInvoiceLine>()); });
		}

		public void TestGetTariffDescriptionDefaultLanguage()
		{
			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));

			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("Description can't be retrieved", "", invoiceLineCalculator.GetTariffDescription(""));

			invoiceLine.JI_Tariff = "";
			AssertEquals("Description can't be retrieved", "", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionPreferredLanguage()
		{
			tariffSearchHelper.Language = "IT";
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "IT", "Derrate");
			helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			helper.CreateNomenclatureGroupLanguage(group3.PK, "IT", "Galli e galline della specie Gallus domesticus");
			Factory.Save();

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "ANIMALI VIVI Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone Galli e galline della specie Gallus domesticus Derrate", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionFallbackToUserWorkingLanguage()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "IT", "Derrate");
			helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			helper.CreateNomenclatureGroupLanguage(group3.PK, "IT", "Galli e galline della specie Gallus domesticus");
			Factory.Save();

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "ANIMALI VIVI Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone Galli e galline della specie Gallus domesticus Derrate", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionPreferredLanguageNotAvailable()
		{
			tariffSearchHelper.Language = "IT";

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionUserWorkingLanguageNotAvailable()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionPreferredLanguagePartiallyAvailable()
		{
			tariffSearchHelper.Language = "IT";
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "IT", "Derrate");
			helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			Factory.Save();

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionUserWorkingLanguagePartiallyAvailable()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "IT", "Derrate");
			helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			Factory.Save();

			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));
		}

		public void TestGetTariffDescriptionWithDefinedLanguage()
		{
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "IT", "Derrate");
			helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			helper.CreateNomenclatureGroupLanguage(group3.PK, "IT", "Galli e galline della specie Gallus domesticus");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = Factory.New<jobCominvoicelineForTest>();
			invoiceline.JI_JZ = invoice.PK;
			var invoiceLineCalculator = new JobComInvoiceLineValueCalculator(invoiceline);

			invoiceline.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "LIVE ANIMALS Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls Fowls of the species Gallus domesticus Laying stocks", invoiceLineCalculator.GetTariffDescription(""));

			invoiceline.Language = "IT";
			invoiceline.JI_Tariff = "0105111100";
			AssertEquals("Description should be", "ANIMALI VIVI Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone Galli e galline della specie Gallus domesticus Derrate", invoiceLineCalculator.GetTariffDescription(""));
		}

		protected override void SetUp()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper = new UniversalReferenceTestDataHelper(Factory);
			var importTariffType = helper.CreateNewOrGetExistingTariffType(currentCountryCode, "IMP");
			importTariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "Current Country Description", parent: eunDataGrouping);
			helper.CreateOrGetLanguage("IT", "Italian");
			Factory.Save();

			tariffSearchHelper = new TariffSearchHelper(currentCountryCode, "TST", null, null);

			tariffView = helper.CreateTariff(currentCountryCode, importTariffType.PK, "0105111100", startDate, endDate, description: "Laying stocks", compositeKey: "01.01..05.1.1.10.10");
			group1 = helper.CreateNomenclatureGroup("EUN", "01", startDate, endDate, "LIVE ANIMALS", compositeKey: "01.01", nomenclatureGroupType: "CN");
			group2 = helper.CreateNomenclatureGroup(currentCountryCode, "0105", startDate, endDate, "Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls", compositeKey: "01.01..05", nomenclatureGroupType: "CN");
			group3 = helper.CreateNomenclatureGroup(currentCountryCode, "010511", startDate, endDate, "Fowls of the species Gallus domesticus", compositeKey: "01.01..05.1.1", nomenclatureGroupType: "CN");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLineCalculator = new JobComInvoiceLineValueCalculator(invoiceLine);
		}

		public class jobCominvoicelineForTest : JobComInvoiceLine
		{
			public jobCominvoicelineForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
			protected override ZString LanguageForTariffDescriptionCore() => Language;

			public ZString Language { get; set; }
		}

		UniversalReferenceTestDataHelper helper;
		TariffSearchHelper tariffSearchHelper;
		TariffView tariffView;
		RefCusNomenclatureGroup group1;
		RefCusNomenclatureGroup group2;
		RefCusNomenclatureGroup group3;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLineValueCalculator invoiceLineCalculator;
	}
}
