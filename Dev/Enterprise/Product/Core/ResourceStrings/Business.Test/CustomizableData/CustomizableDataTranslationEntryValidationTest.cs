using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class CustomizableDataTranslationEntryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateParameterConsistency()
		{
			var entry = new CustomizableDataTranslationEntry(null, "FRN", ResString.GetMultilingualString("Test", "Test"));
			AssertNoErrors(entry.TranslationInfo);
			entry.Translation = "Test {0}";
			AssertHasError(entry.TranslationInfo, "The translation does not have the same set of parameters as the English");
			entry.Translation = "Test {test}";
			AssertHasError(entry.TranslationInfo, "Formatting error");
			entry.Translation = "Pass";
			AssertNoErrors(entry.TranslationInfo);
		}

		public void TestTranslation_MoreThanMaxLength()
		{
			var entry = new CustomizableDataTranslationEntry(null, "FRN", ResString.GetMultilingualString("Test", "Test"));
			entry.Translation = new string('a', 513);
			AssertHasWarning(entry.TranslationInfo, "The maximum length of 'Translation' has been exceeded.\r\nThe maximum length of this property is 512 characters, but 513 were entered. It has been truncated automatically.");

			entry.Translation = new string('a', 511);
			AssertNoWarnings(entry.TranslationInfo);
		}

		public void TestTranslationWithEdgeCasesOfSourceMaxLength()
		{
			using (var testHelper = new CustomizableDataTestHelper(-1))
			{
				var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
				var entry = (CustomizableDataTranslationEntry)page.All.First(translation => ((CustomizableDataTranslationEntry)translation).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)translation).English == "four");

				AssertEquals("Pre-condition: Source Max Length should be -1.", -1, testHelper.CustomizableDataResourceStrings.Source.MaxLength);

				entry.Translation = new string('a', 51);
				AssertNoWarnings(entry.TranslationInfo);
			}

			using (var testHelper = new CustomizableDataTestHelper(0))
			{
				var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
				var entry = (CustomizableDataTranslationEntry)page.All.First(translation => ((CustomizableDataTranslationEntry)translation).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)translation).English == "four");

				AssertEquals("Pre-condition: Source Max Length should be 0.", 0, testHelper.CustomizableDataResourceStrings.Source.MaxLength);

				entry.Translation = new string('a', 51);
				AssertHasWarning(entry.TranslationInfo, "The maximum length of 'Translation' has been exceeded.\r\nThe maximum length of this property is 0 characters, but 51 were entered. It has been truncated automatically.");

				AssertEquals("Translation should be truncated.", string.Empty, entry.Translation);
			}
		}
	}
}
