using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TranslateDBField))]
	sealed class TranslateDBFieldTest : ValueProviderWithLoadControlFactoryTest<TranslateDBField>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <TranslateDBField blah blah(GL_Description)>", !ValueProviderToTest.IsResponsibleForReplacing("<TranslateDBField blah blah(GL_Description)>", Passes.FirstPass));
			Assert("should not match <   TranslateDBField  \t       blah   >", !ValueProviderToTest.IsResponsibleForReplacing("<   TranslateDBField  \t       blah   >", Passes.FirstPass));
			Assert("should not match <   TranslateDBField  \t       (, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateDBField  \t       (, test only)   >", Passes.SecondPass));
			Assert("should not match <   TranslateDBField  \t       (, )   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateDBField  \t       (, )   >", Passes.SecondPass));
			Assert("should match <   TranslateDBField  \t       (GL_Description, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateDBField  \t       (GL_Description, test only)   >", Passes.SecondPass));
			Assert("should match <   TranslateDBField  \t       (AccGLHeader.GL_Description, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateDBField  \t       (AccGLHeader.GL_Description, test only)   >", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			using (var mockFrn = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			using (var documentPack = DocumentPack.EmptyPack)
			{
				var key = CustomizableDataResourceStrings.GetCustomizableDataKey("AG_Description", "Description");
				mockFrn.Put(key, new ResourceStringData(key, "Noitpircsed"));

				documentPack.Language = Core.SharedConstants.Languages.French;
				var report = Report.NewForTesting(documentPack);
				AssertEquals("Noitpircsed", ValueProviderToTest.GetReplacement("<TranslateDBField(AG_Description, Description)>", report));

				documentPack.Language = Core.SharedConstants.Languages.EnglishAmerican;
				var filter = new CollectionOfIFilter();
				filter.Add(new TextField(Factory) { DisplayName = "Translation Language", Value = Core.SharedConstants.Languages.French });
				report.FilterCollection = filter;
				AssertEquals("Noitpircsed", ValueProviderToTest.GetReplacement("<TranslateDBField(AG_Description, Description)>", report));

				AssertEquals("Description", ValueProviderToTest.GetReplacement("<TranslateDBField(InvalidBOName.AG_Description, Description)>", report));
				AssertEquals("Noitpircsed", ValueProviderToTest.GetReplacement("<TranslateDBField(AccGLHeader.AG_Description, Description)>", report));
			}
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			using (var mockFrn = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			using (var documentPack = DocumentPack.EmptyPack)
			{
				var key = CustomizableDataResourceStrings.GetCustomizableDataKey("AG_Description", "Description");
				mockFrn.Put(key, new ResourceStringData(key, "Noitpircsed"));
				documentPack.Language = Core.SharedConstants.Languages.French;
				Report = Report.NewForTesting(documentPack);
				AssertEquals(expectedResult, Report.MacroTranslator.GetValue(example, PassToReplaceExample));
			}
		}
	}
}
