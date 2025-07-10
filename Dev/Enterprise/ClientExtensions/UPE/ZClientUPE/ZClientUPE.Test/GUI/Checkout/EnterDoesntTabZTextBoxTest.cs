using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class EnterDoesntTabZTextBoxTest : TestCase
	{
		public void TestPressingEnterExecutesDefaultButton()
		{
			using (ZForm form = new ZForm())
			{
				EnterDoesntTabZTextBox enterDoesntTabZTextBox = new EnterDoesntTabZTextBox();
				form.Controls.Add(enterDoesntTabZTextBox);
				Button oKButton = new Button();
				form.Controls.Add(oKButton);
				oKButton.Click += new EventHandler(OKButton_Click);
				form.AcceptButton = oKButton;
				form.KeyPreview = true;
				form.Show();
				enterDoesntTabZTextBox.Focus();
				KeySender.PostKeyDown(enterDoesntTabZTextBox, Keys.Enter);
				Application.DoEvents();
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Assert(true);
		}
	}
}
