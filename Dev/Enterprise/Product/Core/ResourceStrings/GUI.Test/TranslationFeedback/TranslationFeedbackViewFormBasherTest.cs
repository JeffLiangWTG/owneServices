using System;
using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	[TestedType(typeof(TranslationFeedbackViewForm))]
	sealed class TranslationFeedbackViewFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			return new TranslationFeedbackViewForm(entries);
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			TranslationFeedbackFactory.ClearCaptionCache();
			base.SetUp();
		}

		protected override void TearDown()
		{
			mockSources.Dispose();
			TranslationFeedbackFactory.ClearCaptionCache();
			base.TearDown();
		}

		IDisposable mockSources;
	}
}
