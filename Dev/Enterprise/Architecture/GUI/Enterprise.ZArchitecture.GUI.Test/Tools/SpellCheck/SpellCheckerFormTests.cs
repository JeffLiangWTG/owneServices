using System.Linq;
using System.Windows.Forms;
using CargoWise.Tools.SpellCheck.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck.Testing
{
	[TestedType(typeof(SpellCheckerForm))]
	sealed class SpellCheckerFormTests : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new SpellCheckerForm();

		// Not dynamically resized, the borders/padding sizes are being changed on show though.
		protected override bool AllowFormSizeFixed => true;

		public void TestAddToDictionaryButton()
		{
			var someLongText = "Here is xyzxyz mistake thats xyzxyz there twice";
			using (var form = new SpellCheckerForm())
			{
				var spellingError = SpellCheckerTestHelpers.CreateFormSpellingError(someLongText, "xyzxyz", "extreme-ironing");
				form.LoadSpellingError(spellingError);

				form.Show();
				Application.DoEvents();

				Assert("PRE: The word should not be in the users dictionary", !WordDictionaryManager.Instance.CurrentDictionary.Contains("xyzxyz"));
				AssertEquals("PRE: Word should be the current error", "xyzxyz", form.CurrentError.SpellingError.Word);

				form.ItemSpellCheckCompleted += (error, result, value, errorParagraph, errorParagraphIndex) =>
				{
					AssertEquals("PRE: Has the correct spelling error", "xyzxyz", error.SpellingError.Word);
					AssertEquals("When we Add To Dictionary, we should ignore future instances of the error", SpellCheckerFormResult.IgnoreAll, result);
					return null;
				};

				var addToDictionaryButton = (Button)form.Controls.Find("buttonAddToDictionary", searchAllChildren: true)[0];
				addToDictionaryButton.PerformClick();
				Application.DoEvents();

				Assert("The word should be added to the users dictionary", WordDictionaryManager.Instance.CurrentDictionary.Contains("xyzxyz"));
				AssertNull("We should ignore other instances of the word", form.CurrentError?.SpellingError.Word);
			}
		}

		public void TestSpellcheckerScrollsTextboxToMisspelledWord()
		{
			var someLongText = "That thing is simply dummy text of the printing and typesetting industry. The thing has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus ZXZXZX including versions of the thing.";
			using (var form = new SpellCheckerForm())
			{
				var spellingError = SpellCheckerTestHelpers.CreateFormSpellingError(someLongText, "ZXZXZX", "extreme-ironing");
				form.LoadSpellingError(spellingError);

				form.Show();

				var textBox = (SpellCheckerRichTextBox)form.Controls.Find("richTextBoxContext", true).Single();
				AssertCaratInWord(textBox, spellingError.SpellingError); // PRE Assert
				var wordPosition = textBox.GetPositionFromCharIndex(spellingError.SpellingError.WordIndex);
				Assert("The textbox should be scrolled to the correct position", textBox.ClientRectangle.Contains(wordPosition));
			}
		}

		void AssertCaratInWord(SpellCheckerRichTextBox textbox, ISpellingError error)
		{
			var wordStart = error.WordIndex;
			var wordEnd = error.WordIndex + error.Word.Length + 1;
			Assert($"Carat should be between the word boundaries: {wordStart} <= {textbox.SelectionStart} <= {wordEnd}", wordStart <= textbox.SelectionStart && textbox.SelectionStart <= wordEnd);
		}
	}
}
