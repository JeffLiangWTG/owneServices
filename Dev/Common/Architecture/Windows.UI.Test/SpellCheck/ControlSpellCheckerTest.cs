using System.Windows.Forms;
using CargoWise.Tools.SpellCheck.TestFramework;
using NUnit.Framework;
using WTG.SpellCheck;
using WTG.SpellCheck.Filters;
using WTG.SpellCheck.TestFramework;

namespace CargoWise.Tools.SpellCheck.GUI.Testing
{
	[GuiTest]
	sealed class ControlSpellCheckerTest : TestCase
	{
		public void TestHandledWordsIgnoredAfterManualEdit()
		{
			RunTest("Ths bigg gsod", string.Empty, HandleHandledWordsIgnoredAfterManualEdit, ControlSpellCheckerResult.ErrorsIgnored, "Ths bigg god", string.Empty);
		}

		public void TestFormTextUpdatedAfterManualEdit()
		{
			RunTest("Ths bigg gsod", string.Empty, HandleFormTextUpdatedAfterManualEdit, ControlSpellCheckerResult.ErrorsIgnored, "Ths bigg god", string.Empty);
		}

		public void TestFormIndividualParagraphEditCarriageReturnNewLine()
		{
			RunTest("xThis xis xa giod xtest\r\nandt his is a different line\r\naned agaunt a line", string.Empty, HandleFormIndividualParagraphEditCarriageReturnNewLine, ControlSpellCheckerResult.ErrorsIgnored, "xThis xis xa giod xtest\r\nBrandt his is a different line\r\naned agaunt a line", string.Empty);
		}

		public void TestFormIndividualParagraphEditNewLine()
		{
			RunTest("xThis xis xa giod xtest\nandt his is a different line\naned agaunt a line", string.Empty, HandleFormIndividualParagraphEditNewLine, ControlSpellCheckerResult.ErrorsIgnored, "xThis xis xa giod xtest\nBrandt his is a different line\naned agaunt a line", string.Empty);
		}

		public void TestNoSpellingErrors()
		{
			RunTest("", "", NullHandler, ControlSpellCheckerResult.NoErrors, "", "");
			RunTest("xa xb xc", "", NullHandler, ControlSpellCheckerResult.NoErrors, "xa xb xc", "");
		}

		public void TestNoSuggestionsAndCancel()
		{
			RunTest("bar.", "", HandleNoSuggestions, ControlSpellCheckerResult.Cancel, "bar.", "");
		}

		public void TestMultipleCorrections()
		{
			RunTest("This big good test. xes.", "", HandleMultipleCorrections, ControlSpellCheckerResult.AllErrorsCorrected, "xa xbb xcccc xdddddddd. xes.", "");
		}

		public void TestIgnoreSome()
		{
			RunTest("What is your name?", "bret", HandleIgnoreSome, ControlSpellCheckerResult.ErrorsIgnored, "What xa your xa?", "xbb");
		}

		public void TestChangeAll()
		{
			RunTest("foo zoo foo zoo foo", "xo foo", HandleChangeAll, ControlSpellCheckerResult.AllErrorsCorrected, "xa xcccc xa xcccc xa", "xo xa");
		}

		public void TestIgnoreAll()
		{
			RunTest("foo zoo foo zoo foo", "xo foo", HandleIgnoreAll, ControlSpellCheckerResult.ErrorsIgnored, "foo xa foo xa foo", "xo foo");
		}

		public void TestManualEditingStates()
		{
			RunTest("foo xar bar", "", HandleManualEditingStates, ControlSpellCheckerResult.ErrorsIgnored, "foo xar bar", "");
		}

		public void TestVaryingOptions()
		{
			ISpellChecker checker = new TestingSpellChecker();
			var spellChecker = new TestControlSpellChecker(checker);
			spellChecker.AddControl(form.Controls["textBox1"]);
			spellChecker.AddControl(new FilteringSpellChecker(checker, AllCodesFilter.Instance), form.Controls["textBox2"]);
			RunTest(spellChecker, "FOO foo", "FOO foo", HandleVaryingOptions, ControlSpellCheckerResult.AllErrorsCorrected, "xa xa", "FOO xa");
		}

		#region Implementation

		protected override void SetUp()
		{
			form = CreateTestForm();
			form.Show();
		}

		protected override void TearDown()
		{
			form.Close();
			form.Dispose();
			form = null;
		}

		Form form;

		void RunTest(string text1, string text2, SpellCheckFormTestHandler formActions, ControlSpellCheckerResult expectedResult, string expectedText1, string expectedText2)
		{
			RunTest(
				new TestControlSpellChecker(new TestingSpellChecker(), form),
				text1,
				text2,
				formActions,
				expectedResult,
				expectedText1,
				expectedText2);
		}

		void RunTest(TestControlSpellChecker spellChecker, string text1, string text2, SpellCheckFormTestHandler formActions, ControlSpellCheckerResult expectedResult, string expectedText1, string expectedText2)
		{
			form.Controls["textBox1"].Text = text1;
			form.Controls["textBox2"].Text = text2;
			using (var helper = new SpellCheckFormTestHelper(formActions))
			{
				var result = spellChecker.CheckSpelling();
				AssertEquals(expectedResult, result);
				AssertEquals(expectedText1, form.Controls["textBox1"].Text);
				AssertEquals(expectedText2, form.Controls["textBox2"].Text);
			}
		}

		SpellCheckFormAction NullHandler(ISpellCheckerForm form)
		{
			return null;
		}

		SpellCheckFormAction HandleNoSuggestions(ISpellCheckerForm iform)
		{
			var form = (SpellCheckerForm)iform;
			AssertEquals(false, GetControl(form, "buttonChange").Enabled);
			AssertEquals(false, GetControl(form, "buttonChangeAll").Enabled);
			AssertEquals(0, ((ListBox)GetControl(form, "listBoxSuggestions")).Items.Count);
			return new SpellCheckFormAction(SpellCheckerFormResult.Cancel, NullHandler);
		}

		SpellCheckFormAction HandleMultipleCorrections(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, 0, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, 1, delegate
				{
					return new SpellCheckFormAction(SpellCheckerFormResult.Change, 2, delegate
					{
						return new SpellCheckFormAction(SpellCheckerFormResult.Change, 3, NullHandler);
					});
				});
			});
		}

		SpellCheckFormAction HandleIgnoreSome(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, delegate
				{
					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
					{
						return new SpellCheckFormAction(SpellCheckerFormResult.Change, delegate
						{
							return new SpellCheckFormAction(SpellCheckerFormResult.Change, 1, NullHandler);
						});
					});
				});
			});
		}

		SpellCheckFormAction HandleHandledWordsIgnoredAfterManualEdit(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, "Ths bigg god", delegate
				{
					AssertEquals("Spellchecker should recheck the whole text & skip any previously ignored words", "bigg", form.CurrentError.SpellingError.Word);

					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, NullHandler);
				});
			});
		}

		SpellCheckFormAction HandleFormTextUpdatedAfterManualEdit(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, "Ths bigg god", delegate
				{
					AssertEquals(form.CurrentError.Text, this.form.Controls["textBox1"].Text);

					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, NullHandler);
				});
			});
		}

		SpellCheckFormAction HandleFormIndividualParagraphEditCarriageReturnNewLine(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, "Brandt his is a different line", delegate
				{
					AssertEquals("The text afterwards should have ONLY the current error's line changed after a manual edit", "xThis xis xa giod xtest\r\nBrandt his is a different line\r\naned agaunt a line", this.form.Controls["textBox1"].Text);

					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, NullHandler);
				});
			});
		}

		SpellCheckFormAction HandleFormIndividualParagraphEditNewLine(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, "Brandt his is a different line", delegate
				{
					AssertEquals("The text afterwards should have ONLY the current error's line changed after a manual edit", "xThis xis xa giod xtest\nBrandt his is a different line\naned agaunt a line", this.form.Controls["textBox1"].Text);

					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, NullHandler);
				});
			});
		}

		SpellCheckFormAction HandleChangeAll(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, 2, NullHandler);
			});
		}

		SpellCheckFormAction HandleIgnoreAll(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.IgnoreAll, delegate
			{
				return new SpellCheckFormAction(SpellCheckerFormResult.Change, delegate
				{
					return new SpellCheckFormAction(SpellCheckerFormResult.Change, NullHandler);
				});
			});
		}

		SpellCheckFormAction HandleManualEditingStates(ISpellCheckerForm iform)
		{
			var form = (SpellCheckerForm)iform;

			var box = (RichTextBox)GetControl(form, "richTextBoxContext");

			box.Text = "o xar bar";
			AssertEquals(SpellCheckerForm.Captions.UndoEdit, GetControl(form, "buttonIgnoreUndo").Text);
			AssertEquals(false, GetControl(form, "buttonIgnoreAll").Enabled);
			AssertEquals(true, GetControl(form, "buttonChange").Enabled);
			AssertEquals(false, GetControl(form, "buttonChangeAll").Enabled);
			AssertEquals(false, GetControl(form, "listBoxSuggestions").Enabled);
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
			{
				AssertEquals(SpellCheckerForm.Captions.IgnoreOnce, GetControl(form, "buttonIgnoreUndo").Text);
				AssertEquals(true, GetControl(form, "buttonIgnoreAll").Enabled);
				AssertEquals(true, GetControl(form, "buttonChange").Enabled);
				AssertEquals(true, GetControl(form, "buttonChangeAll").Enabled);
				AssertEquals(true, GetControl(form, "listBoxSuggestions").Enabled);
				return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, delegate
				{
					return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, NullHandler);
				});
			});
		}

		// "FOO foo", "FOO foo"
		// "xa xa", "FOO xa"
		SpellCheckFormAction HandleVaryingOptions(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, HandleVaryingOptions);
		}

		static Control GetControl(Form form, string name)
		{
			return SpellCheckFormTestHelper.GetControl(form, name);
		}

		Form CreateTestForm()
		{
			var form = new Form();
			var textBox1 = new TextBox();
			textBox1.Name = "textBox1";
			form.Controls.Add(textBox1);
			var textBox2 = new TextBox();
			textBox2.Name = "textBox2";
			textBox2.Top = textBox1.Height + 8;
			form.Controls.Add(textBox2);
			return form;
		}

		#endregion
	}
}
