using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetBusinessObjectNumericValueWithZeroAsEmpty))]
	sealed class GetBusinessObjectNumericValueWithZeroAsEmptyTest : GetBusinessObjectValueAbstractTest<GetBusinessObjectNumericValueWithZeroAsEmpty>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <Two args are missing>", !ValueProviderToTest.IsResponsibleForReplacing("<GetBusinessObjectNumericValueWithZeroAsEmpty(AField)>", Passes.FirstPass));
			Assert("should not match <One arg is missing>", !ValueProviderToTest.IsResponsibleForReplacing("<GetBusinessObjectNumericValueWithZeroAsEmpty(AField, BField)>", Passes.FirstPass));
			Assert("should not match <Macro name is wrong>", !ValueProviderToTest.IsResponsibleForReplacing("<GetBusinessObjectNumericValueWithZeroAsEmpt>", Passes.FirstPass));
			Assert("should not match <Macro name is wrong>", !ValueProviderToTest.IsResponsibleForReplacing("<   GetBusinessObjectNumericValueWithZeroAsEmpt somefield   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   GetBusinessObjectNumericValueWithZeroAsEmpty  \t  (     fld , dd, cc )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   GetBusinessObjectNumericValueWithZeroAsEmpty(AField, other, last)   >", Passes.FirstPass));
		}

		public override void TestReplacement()
		{
			ValueProvider = new GetBusinessObjectNumericValueWithZeroAsEmpty();

			var bizObj = (BusinessObject)ValueProviderFactory.New<Enterprise.Integration.Customs.ICusEntryHeader>();

			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				using (var testReport = new Report(new DocumentPack(), excelTemplate))
				{
					var value = ValueProvider.GetReplacement("<GetBusinessObjectNumericValueWithZeroAsEmpty(CusEntryHeader, " + bizObj.PK + ", CustomsValue)>", testReport);
					AssertEquals(ZString.Empty, value);
				}
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			ValueProvider = new GetBusinessObjectNumericValueWithZeroAsEmpty();
			var bizObj = (BusinessObject)ValueProviderFactory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("EntryHEader.CH_PK", bizObj.PK));
		}
	}
}
