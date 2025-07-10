using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.GUI.RichEdit;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	internal class RichTextSpellCheckerTest : SpellCheckerTest<RichTextSpellChecker, ZRichTextBox>
	{
		protected override RichTextSpellChecker InitialiseSpellcheck(ZRichTextBox textbox, ISpellChecker spellChecker = null, string name = "DummyTextBox")
			=> new RichTextSpellChecker(spellChecker ?? GetDefaultSpellChecker(), textbox, name, new StringsToExclude(WordDictionaryManager.Instance.CurrentDictionary));

		protected override ZRichTextBox GetControlToSpellCheck(bool enableScroll = true) => new ZRichTextBox { Dock = DockStyle.Fill, AutoScroll = enableScroll };
		protected override TextBoxBase GetTextbox(ZRichTextBox control) => control.GetRichTextBoxForTest();
		public void TestPopoutFormGetsSpellcheckedToo()
		{
			using (var control = GetControlToSpellCheck())
			using (var dummyForm = new ZChildForm())
			{
				dummyForm.Controls.Add(control);
				InitialiseSpellcheck(control);
				control.ShowPopupEditor();
				var popup = ZApplication.GetOpenForms().OfType<ZRichTextBoxPopupForm>().Single();
				var richText = popup.Controls.OfType<ZRichTextBox>().Single().Controls.OfType<RichTextBox>().Single();
				AssertNotNull("Spellcheck should be enabled for the popout form (we know it's enabled if the menu item is there)", richText.ContextMenuStrip.Items[SpellChecker.MenuItemKeys.SpellCheckSeperator]);
			}
		}
	}
}
