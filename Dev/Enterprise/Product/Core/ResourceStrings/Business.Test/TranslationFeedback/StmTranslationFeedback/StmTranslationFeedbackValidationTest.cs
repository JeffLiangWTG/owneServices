using CargoWise.EntityFramework.Testing;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class StmTranslationFeedbackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateParameterConsistency()
		{
			var feedback = Factory.New<StmTranslationFeedback>();
			feedback.XT_Language = Core.SharedConstants.Languages.French;
			feedback.XT_Source = "Test {0}, {1}";
			feedback.XT_SuggestedTranslation = "Test";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
			feedback.XT_SuggestedTranslation = "Essai {0}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
			feedback.XT_SuggestedTranslation = "{1} Essai";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
			feedback.XT_SuggestedTranslation = "{1} Essai {0}";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_SuggestedTranslation = "{{1} Essai {0}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "Formatting error");
			feedback.XT_Source = "Test {0}";
			feedback.XT_SuggestedTranslation = "{1} Essai {0}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
			feedback.XT_SuggestedTranslation = "{0} Essai";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_SuggestedTranslation = "{0} Essai {!}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "Formatting error");
			feedback.XT_Source = "Test";
			feedback.XT_SuggestedTranslation = "{0} Essai";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
			feedback.XT_SuggestedTranslation = "{Essai}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "Formatting error");
			feedback.XT_SuggestedTranslation = "Essai";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_Source = "{Test}";
			feedback.XT_SuggestedTranslation = "{Essai}";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
		}

		public void TestValidateParameterConsistencyIgnoresCaseFormatting()
		{
			var feedback = Factory.New<StmTranslationFeedback>();
			feedback.XT_Language = Core.SharedConstants.Languages.French;
			feedback.XT_Source = "Test {0}";
			feedback.XT_SuggestedTranslation = "Essai {0:L}";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_SuggestedTranslation = "Essai {0:U}";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_SuggestedTranslation = "Essai {0:T}";
			AssertNoErrors(feedback.XT_SuggestedTranslationInfo);
			feedback.XT_SuggestedTranslation = "Essai {0:X}";
			AssertHasError(feedback.XT_SuggestedTranslationInfo, "The translation does not have the same set of parameters as the English");
		}
	}
}
