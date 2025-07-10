using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	sealed class TranslationFeedbackEntriesListControlTestCase : TestCaseWithFactory
	{
		public void TestLanguageLabels()
		{
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai" },
				TranslationFeedbackMatchTypes.Codes.Exact));

			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				AssertEquals("English", form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid.Columns[StmTranslationFeedback.Schema.XT_Source].ColumnStyle.HeaderText);
				AssertEquals("French", form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle.HeaderText);
			}
		}

		public void TestColumnWidths()
		{
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai" },
				TranslationFeedbackMatchTypes.Codes.Exact));

			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				for (int w = form.MinimumSize.Width; w <= 1280; w += 5)
				{
					form.Width = w;
					Assert("Grid should stretch with the form", form.Width - grid.Width < 50);
					var hitTest = grid.HitTest(grid.Size.Width - 22, 5);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, StmTranslationFeedback.Schema.XT_SuggestedTranslation, grid.Columns[hitTest.Column].ColumnName);
					hitTest = grid.HitTest(grid.TableStyles[0].RowHeaderWidth + 50, 5);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, StmTranslationFeedback.Schema.XT_Source, grid.Columns[hitTest.Column].ColumnName);
					AssertEquals("Source and translation columns should be the same width " + w, grid.Columns[StmTranslationFeedback.Schema.XT_Source].ColumnStyle.Width, grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle.Width);
					AssertEquals("grid.IsHorizontalScrollBarVisible " + w, false, grid.IsHorizontalScrollBarVisible);
				}
			}

			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.ShowOriginalTranslation = true;
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				for (int w = form.MinimumSize.Width; w <= 1280; w += 5)
				{
					form.Width = w;
					Assert("Grid should stretch with the form", form.Width - grid.Width < 50);
					var hitTest = grid.HitTest(grid.Size.Width - 30, 5);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, StmTranslationFeedback.Schema.XT_SuggestedTranslation, grid.Columns[hitTest.Column].ColumnName);
					hitTest = grid.HitTest(grid.TableStyles[0].RowHeaderWidth + 50, 5);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, StmTranslationFeedback.Schema.XT_Source, grid.Columns[hitTest.Column].ColumnName);
					AssertEquals("Source and suggested translation columns should be the same width " + w, grid.Columns[StmTranslationFeedback.Schema.XT_Source].ColumnStyle.Width, grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle.Width);
					AssertEquals("Source and original translation columns should be the same width " + w, grid.Columns[StmTranslationFeedback.Schema.XT_Source].ColumnStyle.Width, grid.Columns[StmTranslationFeedback.Schema.XT_OriginalTranslation].ColumnStyle.Width);
					AssertEquals("grid.IsHorizontalScrollBarVisible " + w, false, grid.IsHorizontalScrollBarVisible);
				}
			}
		}

		public void TestRowHeights()
		{
			const int x = 100;
			const int line1 = 30;
			const int line2 = 46;
			const int line3 = 62;
			const int line4 = 78;
			const int line5 = 94;

			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Single line test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test Line 1\r\nTest Line 2" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(1, grid.HitTest(x, line3).Row);
				AssertEquals(-1, grid.HitTest(x, line4).Row);
			}

			entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test Line 1" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne 1\r\nEssai ligne 2" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(0, grid.HitTest(x, line2).Row);
				AssertEquals(-1, grid.HitTest(x, line3).Row);
			}

			entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Single line test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Test Line 1\r\nTest Line 2\r\nTest Line 3" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne 1\r\nEssai ligne 2\r\nEssai ligne 3" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(1, grid.HitTest(x, line3).Row);
				AssertEquals(1, grid.HitTest(x, line4).Row);
				AssertEquals(-1, grid.HitTest(x, line5).Row);
			}

			entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Single line test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_FullDescription = "Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test Test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_FullDescription = "Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai Essai" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			using (var form = new TranslationFeedbackCreateForm(entries))
			{
				form.Show();
				var grid = form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.grid;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(1, grid.HitTest(x, line3).Row);
				AssertEquals(1, grid.HitTest(x, line4).Row);
				AssertEquals(-1, grid.HitTest(x, line5).Row);

				form.Width = 1600;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(-1, grid.HitTest(x, line3).Row);

				form.Width = 800;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(1, grid.HitTest(x, line3).Row);
				AssertEquals(-1, grid.HitTest(x, line4).Row);
			}
		}
	}
}
