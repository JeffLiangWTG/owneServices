using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class OrganisationalUnitPickerFindBoxTest : BaseFindBoxTest
	{
		public void TestPressingDeleteClearsTextBox()
		{
			using (var form = new ZChildForm())
			using (var findBox = new OrganisationalUnitPickerFindBox())
			{
				form.Controls.Add(FindBox);
				form.Show();

				AssertEquals("Pre-condition: CodeBox is empty", string.Empty, findBox.CodeBox.Text);
				findBox.CodeBox.Text = "some text";
				AssertEquals("some text", findBox.CodeBox.Text);

				KeySender.SendKeyDownToProcessCmdKey(findBox, (int)Keys.Delete);
				Application.DoEvents();

				AssertEquals("CodeBox should be cleared", string.Empty, findBox.CodeBox.Text);
			}
		}

		#region Implementation

		protected override ZFindBoxUserControl NewFindBoxTester => new OrganisationalUnitPickerFindBox();

		#endregion

	}
}
