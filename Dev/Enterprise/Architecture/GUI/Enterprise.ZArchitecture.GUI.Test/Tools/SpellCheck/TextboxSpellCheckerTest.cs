using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business.SpellCheck;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	internal class TextboxSpellCheckerTest : SpellCheckerTest<TextboxSpellChecker, ZTextBox>
	{
		public void TestCalculateVisibleIndexRange()
		{
			using (var form = new Form())
			using (var control = GetControlToSpellCheck(true))
			{
				control.Text = "1ff\r\n2ff\r\n3ff\r\n4ff\r\n5ff\r\n";
				control.Font = new Font("Consolas", 14f);
				var spellChecker = InitialiseSpellcheck(control, null, "Whatever");
				form.Height = 85;
				form.Controls.Add(control);
				form.Show();
				var visibleIndex = spellChecker.CalculateVisibleIndexRange_ForTest();
				AssertEquals(0, visibleIndex.Item1);
				AssertEquals(4, visibleIndex.Item2);
			}
		}

		protected override ZTextBox GetControlToSpellCheck(bool enableScroll) => new ZTextBox { Dock = DockStyle.Fill, Multiline = true, CharacterCasing = CharacterCasing.Normal, ScrollBars = enableScroll ? ScrollBars.Vertical : ScrollBars.None };

		protected override TextboxSpellChecker InitialiseSpellcheck(ZTextBox textbox, ISpellChecker spellChecker = null, string name = "DummyTextBox")
			=> new TextboxSpellChecker(spellChecker ?? GetDefaultSpellChecker(), textbox, name, new StringsToExclude(WordDictionaryManager.Instance.CurrentDictionary));

		protected override TextBoxBase GetTextbox(ZTextBox control) => control;
	}
}
