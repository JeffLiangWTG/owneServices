using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	class TranslationFeedbackFilterControlTest : TranslationFeedbackTestCase
	{
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
			using (var form = new TestForm(entries))
			{
				form.Show();
				var grid = form.filterControl.Grid;
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
			using (var form = new TestForm(entries))
			{
				form.Show();
				var grid = form.filterControl.Grid;
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
			using (var form = new TestForm(entries))
			{
				form.Show();
				var grid = form.filterControl.Grid;
				AssertEquals(0, grid.HitTest(x, line1).Row);
				AssertEquals(1, grid.HitTest(x, line2).Row);
				AssertEquals(1, grid.HitTest(x, line3).Row);
				AssertEquals(1, grid.HitTest(x, line4).Row);
				AssertEquals(-1, grid.HitTest(x, line5).Row);
			}
		}

		[ExpectNoExceptions]
		public void TestDiff()
		{
			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Single line test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries[0].XT_SuggestedTranslation = "Essai trait simple";
			using (var form = new TestForm(entries))
			{
				form.Show();
				form.Width = 500;
				form.Update();
			}

			// MeasureCharacterRanges throws OverflowException with more than 32 character ranges
			entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty twentyone twentytwo twentythree twentyfour twentyfive twentysix twentyseven twentyeight twentynine thrity thrityone thritytwo thirtythree" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.French, HD_Caption = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty twentyone twentytwo twentythree twentyfour twentyfive twentysix twentyseven twentyeight twentynine thrity thrityone thritytwo thirtythree" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries[0].XT_SuggestedTranslation = "un two tois four cinq six sept eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty twentyone twentytwo twentythree twentyfour twentyfive twentysix twentyseven twentyeight twentynine thrity thrityone thritytwo thirtythree";
			using (var form = new TestForm(entries))
			{
				form.Show();
				form.Width = 500;
				form.Update();
			}
		}

		[ExpectNoExceptions]
		public void TestDiffRefresh()
		{
			AddMockTranslation("X", "Single line test", "Essai ligne simple");

			var entries = new TopLevelTranslationFeedbackCollection(Factory);
			entries.Add(StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Code = "x", HD_Language = Res.DefaultLanguage, HD_Caption = "Single line test" },
				new HelpDataString() { HD_Code = "x", HD_Language = Core.SharedConstants.Languages.ChineseTraditional, HD_Caption = "Essai ligne simple" },
				TranslationFeedbackMatchTypes.Codes.Exact));
			entries[0].XT_SuggestedTranslation = "Essai trait unique";
			Factory.Save();

			using (var form = new TestForm(entries))
			{
				form.Show();
				form.Width = 1000;
				Application.DoEvents();

				var anotherFactory = new BusinessObjectFactory();
				var entry = anotherFactory.Load<StmTranslationFeedback>(entries[0].PK);
				entry.XT_SuggestedTranslation = "Essai unique";
				anotherFactory.Save();

				Application.DoEvents();
			}
		}

		class TestForm : ZChildForm
		{
			public TestForm(TopLevelTranslationFeedbackCollection entries)
			{
				InitializeComponent();
				filterControl = new TranslationFeedbackFilterControl(entries, new TranslationFeedbackFilterBusinessObject());
				filterControl.Dock = System.Windows.Forms.DockStyle.Fill;
				Controls.Add(filterControl);
			}

			public TranslationFeedbackFilterControl filterControl;
		}
	}
}
