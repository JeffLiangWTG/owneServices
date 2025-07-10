using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(WhsInventoryDutyAndTax))]
	sealed class WhsInventoryDutyAndTaxTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<WhsInventoryDutyAndTax('DTY', '<Inventory.WarehouseCountry>', '<Inventory.PK>', '<Inventory.Tariff>', '<Inventory.CountryOfOrigin>', '<Inventory.CustomsValue>', '<Inventory.CustomsQty1>', '<Inventory.CustomsUQ1>', '<Inventory.CustomsQty2>', '<Inventory.CustomsUQ2>', '<Inventory.CustomsQty3>', '<Inventory.CustomsUQ3>', '<Inventory.Ratio>', '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>')>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<WhsInventoryDutyAndTax('DTY', '<Inventory.WarehouseCountry>', '<Inventory.PK>', '<Inventory.Tariff>', '<Inventory.CountryOfOrigin>', '<Inventory.CustomsValue>', '<Inventory.CustomsQty1>', '<Inventory.CustomsUQ1>', '<Inventory.CustomsQty2>', '<Inventory.CustomsUQ2>', '<Inventory.CustomsQty3>', '<Inventory.CustomsUQ3>', '<Inventory.Ratio>', '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>')>", Passes.SecondPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<   WhsInventoryDutyAndTax   (   'DTY'   ,   '<Inventory.WarehouseCountry>'   ,   '<Inventory.PK>'   ,   '<Inventory.Tariff>'   ,   '<Inventory.CountryOfOrigin>'   ,   '<Inventory.CustomsValue>'   ,   '<Inventory.CustomsQty1>'   ,   '<Inventory.CustomsUQ1>'   ,   '<Inventory.CustomsQty2>'   ,   '<Inventory.CustomsUQ2>'   ,   '<Inventory.CustomsQty3>'   ,   '<Inventory.CustomsUQ3>'   ,   '<Inventory.Ratio>'   ,   '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>'   )   >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var helper = PrepareData(Report);

			CombineAssertions(() =>
			{
				Report.ErrorManager.ClearErrors();
				AssertEquals(ZDecimal.Zero, ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax()>", Report));
				AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);

				Report.ErrorManager.ClearErrors();
				AssertEquals(ZDecimal.Zero, ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '48cd5fff-fe9b', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: InventoryPK: '48cd5fff-fe9b', Error: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx). (in ConvertFrom, value = '48cd5fff-fe9b')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals("NO RATE AVAILABLE", ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '18cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000AS', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: CustomsValue: '1000AS', Error: Input string was not in a correct format. (in ConvertFrom, value = '1000AS')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals("NO RATE AVAILABLE", ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '28cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1AS00', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: CustomsQty1: '1AS00', Error: Input string was not in a correct format. (in ConvertFrom, value = '1AS00')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals("NO RATE AVAILABLE", ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '38cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '20SD0', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: CustomsQty2: '20SD0', Error: Input string was not in a correct format. (in ConvertFrom, value = '20SD0')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals("NO RATE AVAILABLE", ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '48cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '30ASD0', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: CustomsQty3: '30ASD0', Error: Input string was not in a correct format. (in ConvertFrom, value = '30ASD0')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals(ZDecimal.Zero, ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '58cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '6A', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: Ratio: '6A', Error: Input string was not in a correct format. (in ConvertFrom, value = '6A')]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals("NO RATE AVAILABLE", ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'ZA', '68cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-sdfs')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: ArrivalDate: '18-sdfs', Error: Cannot initialise a CargoWise.Types.ZDateTime with <18-sdfs> (System.String).]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals(ZDecimal.Zero, ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('DTY', 'Z#', '78cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: DutyAndTaxCalculatorProvider: 'Z#', Error: WhsInventoryDutyAndTaxCalculator is not currently implemented for 'Z#'.]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				AssertEquals(ZDecimal.Zero, ValueProviderToTest.GetReplacement("<WhsInventoryDutyAndTax('SD#', 'ZA', '88cd5fff-fe9b-4789-b895-604da84ab32f', '11111111', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '18-MAR-19')>", Report));
				Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
				AssertContains("[Error in WhsInventoryDutyAndTax Macro: DutyAndTaxCalculator: 'ZA', Error: 'SD#' is not a valid.]", Report.ErrorManager.ToString());

				Report.ErrorManager.ClearErrors();
				var macro = $"<WhsInventoryDutyAndTax('{{0}}', 'ZA', '98cd5fff-fe9b-4789-b895-604da84ab32f', '{helper.TariffCode}', 'CN', '1000.0049', '1000', 'LI', '2000', 'KG', '3000', 'NO', '0.6', '{helper.StartDate.AddMonths(1).ToShortDateString()}')>";
				AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
				AssertEquals(new ZDecimal(7940.10), ValueProviderToTest.GetReplacement(string.Format(macro, "DTY"), Report));
				AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
				AssertEquals(new ZDecimal(1209.52), ValueProviderToTest.GetReplacement(string.Format(macro, "VAT"), Report));
				AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
				AssertEquals(new ZDecimal(39.07m), ValueProviderToTest.GetReplacement(string.Format(macro, "12B"), Report));
			});
		}

		public void TestReplaceWithNestedMacros()
		{
			var content = $@"{{A}}-[#Config]
{{A}}-[#SectionBody]
{{B}}-[<WhsInventoryDutyAndTax('<If(<ABC>==1, ""DTY"", ""VAT"")>', '<Inventory.WarehouseCountry>', '<Inventory.PK>', '<Inventory.Tariff>', '<Inventory.CountryOfOrigin>', '<Inventory.CustomsValue>', '<Inventory.CustomsQty1>', '<Inventory.CustomsUQ1>', '<Inventory.CustomsQty2>', '<Inventory.CustomsUQ2>', '<Inventory.CustomsQty3>', '<Inventory.CustomsUQ3>', '<Inventory.Ratio>', '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>')>]
{{A}}-[#EndOfReport]";
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", content);
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				PrepareData(report);
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ABC", "1"));
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();
						AssertEquals("{B}-[7940.1]", workSheet.ToString());
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new WhsInventoryDutyAndTax();
		}

		Enterprise.Integration.Customs.IZAWhsInventoryDutyAndTaxCalculatorTestHelper PrepareData(Report report)
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.IZAWhsInventoryDutyAndTaxCalculatorTestHelper>();
			helper.SetupTestData(Factory);
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.WarehouseCountry", Core.Constants.CountryCodes.SouthAfrica));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.PK", ZGuid.NewZGuid()));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.Tariff", helper.TariffCode));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CountryOfOrigin", "CN"));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsValue", 1000.0049m));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsQty1", 1000m));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsUQ1", "LI"));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsQty2", 2000m));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsUQ2", "KG"));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsQty3", 3000m));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.CustomsUQ3", "NO"));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.Ratio", 0.6m));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inventory.ArrivalDate", helper.StartDate.AddDays(30)));
			return helper;
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareData(Report);
		}
	}
}
