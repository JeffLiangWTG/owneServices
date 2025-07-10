using System.IO;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BoolToYN))]
	sealed class BoolToYNTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<BoolToYN(<Is Active>)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<BoolToYN(<IsActive>)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<BoolToYN(True)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<   BoolToYN  (  False  )  >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals("Y", ValueProviderToTest.GetReplacement("<BoolToYN(true)>", Report));
			AssertEquals("Y", ValueProviderToTest.GetReplacement("<BoolToYN(True)>", Report));
			AssertEquals("Y", ValueProviderToTest.GetReplacement("<BoolToYN(TruE)>", Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement("<BoolToYN(false)>", Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement("<BoolToYN(False)>", Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement("<BoolToYN(FaLse)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<BoolToYN()>", Report));

			Assert("Report.ErrorManager.HasErrors", !Report.ErrorManager.HasErrors);
			AssertEquals("", ValueProviderToTest.GetReplacement("<BoolToYN(0)>", Report));
			Assert("Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors);
			AssertContains("Value: '0', Error: String was not recognized as a valid Boolean.", Report.ErrorManager.ToString());
		}

		public void TestReplaceWithNestedMacros()
		{
			var content = $@"{{A}}-[#Config]
{{A}}-[#SectionBody]
{{B}}-[<BoolToYN(<EvaluateInnerContent(""<If(<ABC>==1, ""true"", ""false"")>"")>)>]
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
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ABC", "1"));
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();
						AssertEquals("{B}-[Y]", workSheet.ToString());
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BoolToYN();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ChargeCode.IsActive", true));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Is Active", false));
		}
	}
}
