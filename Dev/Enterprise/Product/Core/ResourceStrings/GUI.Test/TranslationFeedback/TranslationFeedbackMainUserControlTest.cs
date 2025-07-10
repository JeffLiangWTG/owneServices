using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	sealed class TranslationFeedbackMainUserControlTest : TranslationFeedbackTestCase
	{
		public void TestContextLinks()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k1", new ResourceStringData("k1", "Essai"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k2", new ResourceStringData("k2", "Essai"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", new ResourceStringData("k3", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k3", new ResourceStringData("k3", "Essai"));
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			var feedback = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k1", TranslationFeedbackMatchTypes.Codes.Exact);
			entries.Add(feedback);
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(false, feedback.AllContexts[1].Update);
				AssertEquals(false, feedback.AllContexts[2].Update);
				form.translationFeedbackMainUserControl.selectAllContextsLink_LinkClicked(form, null);
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(true, feedback.AllContexts[1].Update);
				AssertEquals(true, feedback.AllContexts[2].Update);
				form.translationFeedbackMainUserControl.selectOneContextLink_LinkClicked(form, null);
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(false, feedback.AllContexts[1].Update);
				AssertEquals(false, feedback.AllContexts[2].Update);
				feedback.AllContexts[0].Update = false;
				feedback.AllContexts[1].Update = true;
				form.translationFeedbackMainUserControl.selectOneContextLink_LinkClicked(form, null);
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(false, feedback.AllContexts[1].Update);
				AssertEquals(false, feedback.AllContexts[2].Update);
				feedback.AllContexts[0].Update = false;
				feedback.AllContexts[1].Update = true;
				form.translationFeedbackMainUserControl.selectAllContextsLink_LinkClicked(form, null);
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(true, feedback.AllContexts[1].Update);
				AssertEquals(true, feedback.AllContexts[2].Update);

				feedback.XT_SuggestedTranslation = "Examiner";
				form.FireSaveButton();
			}

			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(true, feedback.AllContexts[1].Update);
				AssertEquals(true, feedback.AllContexts[2].Update);
				form.translationFeedbackMainUserControl.selectOneContextLink_LinkClicked(form, null);
				AssertEquals(true, feedback.AllContexts[0].Update);
				AssertEquals(true, feedback.AllContexts[1].Update);
				AssertEquals(true, feedback.AllContexts[2].Update);
			}
		}

		public void TestOtherTranslationsControlGrid()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k1", new ResourceStringData("k1", "Essai"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k2", new ResourceStringData("k2", "Contr�le"));
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			var feedback = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k1", TranslationFeedbackMatchTypes.Codes.Exact);
			entries.Add(feedback);
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				object formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender; });
				ZForm.FormCreated += formCreatedHandler;
				try
				{
					form.translationFeedbackMainUserControl.otherTranslationsControl.grid.Select(0);
					form.translationFeedbackMainUserControl.OtherTranslationsControlGrid_DoubleClick(null, EventArgs.Empty);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
					var otherEntries = (TopLevelTranslationFeedbackCollection)((TranslationFeedbackCreateForm)formCreated).BusinessEntity;
					AssertEquals(1, otherEntries.Count);
					AssertEquals("Contr�le", otherEntries[0].XT_OriginalTranslation);
					otherEntries[0].XT_SuggestedTranslation = "�preuve";
					((TranslationFeedbackCreateForm)formCreated).FireSaveButton();
					((IDisposable)formCreated).Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}

			AssertEquals("Contr�le", entries[0].OtherTranslations[0].XT_OriginalTranslation);
			AssertEquals("�preuve", entries[0].OtherTranslations[0].XT_SuggestedTranslation);

			var allFeedback = new BusinessObjectFactory().Load<StmTranslationFeedback>(new ZQuery());
			AssertEquals(1, allFeedback.Length);
			AssertFeedback("", "Test", "Contr�le", "�preuve", "", TranslationFeedbackMatchTypes.Codes.None, allFeedback[0]);
			AssertEquals(1, allFeedback[0].SavedContexts.Count);
			AssertEquals("k2", allFeedback[0].SavedContexts[0].XQ_ResourceStringKey);
		}

		public void TestNoExceptionThrown_OtherFactory_Saved()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k1", new ResourceStringData("k1", "Essai"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k2", new ResourceStringData("k2", "Contr�le"));
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			var feedback = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k1", TranslationFeedbackMatchTypes.Codes.Exact);
			entries.Add(feedback);
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				object formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender; });
				ZForm.FormCreated += formCreatedHandler;
				try
				{
					form.translationFeedbackMainUserControl.otherTranslationsControl.grid.Select(0);
					form.translationFeedbackMainUserControl.OtherTranslationsControlGrid_DoubleClick(null, EventArgs.Empty);
					var otherEntries = (TopLevelTranslationFeedbackCollection)((TranslationFeedbackCreateForm)formCreated).BusinessEntity;
					otherEntries[0].XT_SuggestedTranslation = "�preuve";
					((IDisposable)form).Dispose();
					AssertNoExceptionThrown(() => ((TranslationFeedbackCreateForm)formCreated).FireSaveButton());
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		public void TestDataOfTranslationContextGridAndOtherTranslationGridAreSynchronizedWithMainGrid()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "Test"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k1", new ResourceStringData("k1", "Essai"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "Test2"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k2", new ResourceStringData("k2", "Contr�le"));
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			var feedback1 = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k1", TranslationFeedbackMatchTypes.Codes.Exact);
			var feedback2 = StmTranslationFeedback.New(Factory, Core.SharedConstants.Languages.French, "k2", TranslationFeedbackMatchTypes.Codes.Exact);
			entries.Add(feedback1);
			entries.Add(feedback2);
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid.PerformMouseDownForTest(0, 1);
				AssertEquals(feedback1.AllContexts[0], form.translationFeedbackMainUserControl.translationFeedbackContextsControl.grid.GetCurrent());
				AssertEquals(feedback1.OtherTranslations[0], form.translationFeedbackMainUserControl.otherTranslationsControl.grid.GetCurrent());

				form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid.PerformMouseDownForTest(1, 1);
				AssertEquals(feedback2.AllContexts[0], form.translationFeedbackMainUserControl.translationFeedbackContextsControl.grid.GetCurrent());
				AssertEquals(feedback2.OtherTranslations[0], form.translationFeedbackMainUserControl.otherTranslationsControl.grid.GetCurrent());
			}
		}
	}
}
