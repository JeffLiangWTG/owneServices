using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(ControlledSupportIncidentDialog))]
	internal class ControlledSupportIncidentDialogTest : ZFormBasherTest
	{
		IncidentManagementLink GetNewLink()
		{
			return Factory.NewWithValidTestData<IncidentManagementLink>();
		}

		protected override Form GetFormToBashCore()
		{
			return new ControlledSupportIncidentDialog(GetNewLink());
		}

		public void TestContentsLabelText()
		{
			var link = GetNewLink();
			using (var form = new TestForm(link))
			{
				AssertContains("It should not be edited whilst control is enabled.", form.ContentsLabel.Text);
			}
		}

		public void TestDialogResult()
		{
			var link = GetNewLink();
			TestForm form;
			using (form = new TestForm(link))
			{
				form.ShowDialog();
				form.ViewButton.PerformClick();
				AssertEquals("Dialog should return View after clicking", ControlledSupportIncidentDialog.Result.View, form.DialogResult);
			}

			using (form = new TestForm(link))
			{
				form.ShowDialog();
				form.EditButton.PerformClick();
				AssertEquals("Dialog should return Edit after clicking", ControlledSupportIncidentDialog.Result.Edit, form.DialogResult);
			}

			using (form = new TestForm(link))
			{
				form.ShowDialog();
				form.GroupButton.PerformClick();
				AssertEquals("Dialog should return OpenGroup after clicking", ControlledSupportIncidentDialog.Result.OpenGroup, form.DialogResult);
			}
		}

		class TestForm : ControlledSupportIncidentDialog
		{
			public TestForm(IncidentManagementLink incidentLink) : base(incidentLink)
			{
			}

			public ZButton ViewButton => base.openAsViewModeButton;

			public ZButton EditButton => base.openAsEditModeButton;

			public ZButton GroupButton => base.openLinkedGroupButton;

			public ZLabel ContentsLabel => base.contentsLabel;
		}
	}
}
