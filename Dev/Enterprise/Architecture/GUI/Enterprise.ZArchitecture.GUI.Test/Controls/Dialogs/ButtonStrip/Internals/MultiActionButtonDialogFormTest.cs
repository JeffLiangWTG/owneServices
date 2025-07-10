using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(MultiActionButtonDialogForm))]
	sealed class MultiActionButtonDialogFormTest : ZFormBasherTest
	{
		public void TestDisplayDialog_WithBigMessage_ShouldSizeDialogToFit()
		{
			using (var form = new MultiActionButtonDialogForm("Abs", "In. Ouuuut.", new Control()))
			{
				form.Show();

				ControlTestHelper.AssertControlSize(136, 177, form, AllowedPixelVariance);
				ControlTestHelper.AssertControlSize(64, 25, form.MessageTextBox, AllowedPixelVariance);
			}

			var tallerMessage =
@"One.
Two.
One,
Two,
Threeee!!";

			using (var form = new MultiActionButtonDialogForm("Plank", tallerMessage, new Control()))
			{
				form.Show();

				ControlTestHelper.AssertControlSize("Form height should be extended to fit the taller message", 136, 229, form, AllowedPixelVariance);
				ControlTestHelper.AssertControlSize("Textbox height should be extended to fit the taller message", 55, 77, form.MessageTextBox, AllowedPixelVariance);
			}

			var widerMessage = "Shift and hold. Shift and hold one. Shift and hold two. Shift and hold three. Shift and hold four. Shift and hold back one. Shift and hold three. Shift and hold two. Shift and hold four.";

			using (var form = new MultiActionButtonDialogForm("Stop", widerMessage, new Control()))
			{
				form.Show();

				ControlTestHelper.AssertControlSize("Form width should be extended to fit the wider message", 597, 190, form, AllowedPixelVariance);
				ControlTestHelper.AssertControlSize("Textbox height should be extended to fit the wider message", 585, 38, form.MessageTextBox, AllowedPixelVariance);
			}
		}

		public void TestSelectTextBox_BackgroundColorShouldNotChange()
		{
			using (var form = new MultiActionButtonDialogForm("Wop", "Krikkit", new Control()))
			{
				form.Show();

				var textBox = form.FindSingle<KTextBox>();

				AssertEquals(Color.White, textBox.BackColor);

				textBox.Focus();

				AssertEquals(Color.White, textBox.BackColor);
				AssertEquals(true, textBox.ReadOnly);
			}
		}

		#region Implementation

		const int AllowedPixelVariance = 5;

		protected override Form GetFormToBashCore()
		{
			return new MultiActionButtonDialogForm("", @"One
Two
One
Two
Threeee!!", new ZPanel());
		}

		#endregion
	}
}
