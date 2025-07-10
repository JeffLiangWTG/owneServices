using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	internal class TranslationFeedabckModuleTest : TranslationFeedbackTestCase
	{
		public void TestModuleWarning()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			using (var module = new TranslationFeedbackModule())
			using (module.ShowPopup())
			{
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishAmerican))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				using (var module = new TranslationFeedbackModule())
				using (module.ShowPopup())
				{
					AssertType(typeof(TranslationFeedbackModuleInfoForm), ZFormModaliser.LastFormShownDialogForTest);
				}

				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "狗");
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				ZFormModaliser.LastFormShownDialogForTest = null;
				using (var module = new TranslationFeedbackModule())
				using (module.ShowPopup())
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestModuleWarningForCustomLanguage()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			using (var module = new TranslationFeedbackModule())
			using (module.ShowPopup())
			{
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}

			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";
			Factory.Save();
			using (Res.TemporarilySwitchLanguage(testLanguage.FullLanguageCode))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				using (var module = new TranslationFeedbackModule())
				using (module.ShowPopup())
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestShouldShowTranslationSearchMenuWhenSwitchToEnglishLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishAmerican))
			using (var module = new TranslationFeedbackModuleForTest())
			{
				var menus = module.GetNewStandardMenuItemsExposed();
				AssertNotNull(menus);

				var translationSearchMenu = menus.FindByText("Translation Search");
				Assert("Should show Translation Search menu when switch to English language", translationSearchMenu != null);
			}
		}
	}
}
