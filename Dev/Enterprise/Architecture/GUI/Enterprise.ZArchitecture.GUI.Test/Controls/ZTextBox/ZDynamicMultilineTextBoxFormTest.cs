using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDynamicMultilineTextBoxFormTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "Testing SafeClipboard")]
		public void TestShowDynamicMultiline_ShouldRespectAndTruncate_MaxLength()
		{
			var exceedsMaxLengthText = "ExceedsMaxLength";

			using (var textBox = new ZTextBox { IsDynamicMultiline = true, MaxLength = 3, CharacterCasing = CharacterCasing.Normal })
			using (var dynamicMultiline = new ZDynamicMultilineTextBoxForm(textBox, new Size(5, 5)))
			{
				CombineAssertions("Should take max length from textbox parent passed in", () =>
				{
					AssertEquals(3, textBox.MaxLength);
					AssertEquals(textBox.MaxLength, dynamicMultiline.MultilineTextBox.MaxLength);
				});

				dynamicMultiline.Show();
				Application.DoEvents();

				Clipboard.SetText(exceedsMaxLengthText);
				dynamicMultiline.MultilineTextBox.Paste();

				CombineAssertions("Should truncate text if copy paste too long", () =>
				{
					AssertEquals(textBox.MaxLength, dynamicMultiline.MultilineTextBox.TextLength);
					AssertEquals("Exc", dynamicMultiline.MultilineTextBox.Text);
				});

				foreach (var character in exceedsMaxLengthText)
				{
					KeySender.SendKeyPress(dynamicMultiline.MultilineTextBox, character);
				}

				CombineAssertions("Should truncate text if entered text too long", () =>
				{
					AssertEquals(textBox.MaxLength, dynamicMultiline.MultilineTextBox.TextLength);
					AssertEquals("Exc", dynamicMultiline.MultilineTextBox.Text);
				});
			}
		}
	}
}
