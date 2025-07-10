using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	[TestedType(typeof(TranslationFeedbackCreateForm))]
	sealed class TranslationFeedbackCreateFormBasherTest : ZFormBasherTest
	{
		public void TestSaveButton()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k1", new ResourceStringData("k1", "Essai"));
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			var feedback = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k1", TranslationFeedbackMatchTypes.Codes.Exact);
			entries.Add(feedback);
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				feedback.XT_SuggestedTranslation = "�preuve";
				form.FireSaveButton();
			}
			var checkedOutString = ResourceStringsFactory.Lookup(Core.SharedConstants.Languages.French, "k1", true);
			AssertNotNull(checkedOutString);
			AssertEquals("�preuve", checkedOutString.HD_Caption);
		}

		public void TestSourceControlCaptionIsUpdated()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("DummyBizo|Z0_Description", new ResourceStringData("DummyBizo|Z0_Description", "Description"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("DummyBizo|Z0_Description", new ResourceStringData("DummyBizo|Z0_Description", "Old Desc"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("TTT", new ResourceStringData("TTT", "Source"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("TTT", new ResourceStringData("TTT", "Old Caption"));
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Core.SharedConstants.Languages.French;
			try
			{
				using (var sourceForm = new ZDummyForm())
				{
					sourceForm.CaptionRenderingEnabled = true;
					sourceForm.CalcEdit.Left = 100;
					sourceForm.TextBox.Left = 100;
					sourceForm.CalcEdit.CaptionResourceString = Res.GetData("TTT", "Old Caption");
					sourceForm.Show();

					var entries = new TopLevelTranslationFeedbackCollection(Factory);
					entries.Add(StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "DummyBizo|Z0_Description", TranslationFeedbackMatchTypes.Codes.Exact));
					using (var feedbackForm = new TranslationFeedbackCreateForm(entries, sourceForm.TextBox))
					{
						feedbackForm.Show();
						entries[0].XT_SuggestedTranslation = "New Desc";
						feedbackForm.FireSaveButton();
					}
					AssertEquals("New Desc", sourceForm.TextBox.GetExtension<ILabelCaptionRenderer>().Caption);

					entries = new TopLevelTranslationFeedbackCollection(Factory);
					entries.Add(StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "TTT", TranslationFeedbackMatchTypes.Codes.Exact));
					using (var feedbackForm = new TranslationFeedbackCreateForm(entries, sourceForm.CalcEdit))
					{
						feedbackForm.Show();
						entries[0].XT_SuggestedTranslation = "New Caption";
						feedbackForm.FireSaveButton();
					}
					AssertEquals("New Caption", sourceForm.CalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
			finally
			{
				ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Res.DefaultLanguage;
			}
		}

		protected override Form GetFormToBashCore()
		{
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			return new TranslationFeedbackCreateForm(entries);
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
