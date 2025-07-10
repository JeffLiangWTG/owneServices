using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowProvidersLinkageUserControlTest : BMSTestCaseWithFactory
	{
		public void TestSetReadOnly()
		{
			var control = new WorkflowProvidersLinkageUserControl();
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetReadOnly(true, ZDialogResult.OK);
				AssertEquals(true, control.LinksGrid.ReadOnly);
				AssertEquals(true, control.CreateLinkButton.Enabled);
				AssertEquals(false, control.CancelLinkButton.Enabled);

				control.SetReadOnly(true, ZDialogResult.Cancel);
				AssertEquals(true, control.LinksGrid.ReadOnly);
				AssertEquals(false, control.CreateLinkButton.Enabled);
				AssertEquals(true, control.CancelLinkButton.Enabled);

				// I think this situation is hardly possible (unless defaults are manually edited to enter None), but anyway it is better to test
				control.SetReadOnly(true, ZDialogResult.None);
				AssertEquals(true, control.LinksGrid.ReadOnly);
				AssertEquals(false, control.CreateLinkButton.Enabled);
				AssertEquals(true, control.CancelLinkButton.Enabled);

				control.SetReadOnly(false, ZDialogResult.OK);
				AssertEquals(false, control.LinksGrid.ReadOnly);
				AssertEquals(true, control.CreateLinkButton.Enabled);
				AssertEquals(true, control.CancelLinkButton.Enabled);

				control.SetReadOnly(false, ZDialogResult.Cancel);
				AssertEquals(false, control.LinksGrid.ReadOnly);
				AssertEquals(true, control.CreateLinkButton.Enabled);
				AssertEquals(true, control.CancelLinkButton.Enabled);

				control.SetReadOnly(false, ZDialogResult.None);
				AssertEquals(false, control.LinksGrid.ReadOnly);
				AssertEquals(true, control.CreateLinkButton.Enabled);
				AssertEquals(true, control.CancelLinkButton.Enabled);
			}
		}
	}
}
