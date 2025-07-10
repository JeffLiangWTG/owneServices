using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms
{
	[TestedType(typeof(ProgressWithDetailesForm))]
	public class ProgressWithDetailesFormTest : ProgressFormBasherTest
	{
		public void TestLogging()
		{
			using (var form = GetNewForm())
			{
				form.AddLog("Test Log1");
				form.AddLog("Test Log2");
				string expectedText =
@"
Test Log1
Test Log2";
				AssertEquals("logTextBox.Text", expectedText, form.Log);

				form.ResetLog();
				AssertEquals("logTextBox.Text", "", form.Log);
			}
		}

		public void TestActivateClose()
		{
			using (var form = GetNewForm())
			{
				form.Show();
				Application.DoEvents();

				var cancelProgressButton = (Button)GetControl(form, "CancelProgressButton");
				AssertEquals("Precondition: cancelProgressButton.Text", "Cancel", cancelProgressButton.Text);
				AssertEquals("Precondition: form.Visible", true, form.Visible);

				form.ActivateCloseButton();
				AssertEquals("cancelProgressButton.Text", "Close", cancelProgressButton.Text);

				cancelProgressButton.PerformClick();
				Application.DoEvents();
				AssertEquals("The form should be closed.", false, form.Visible);
			}
		}

		public void TestCancelButtonBehaviour()
		{
			using (var form = GetNewForm())
			{
				form.Show();
				Application.DoEvents();

				var cancelProgressButton = (Button)GetControl(form, "CancelProgressButton");
				AssertEquals("Precondition: cancelProgressButton.Text", "Cancel", cancelProgressButton.Text);
				AssertEquals("Precondition: form.Visible", true, form.Visible);

				cancelProgressButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Cancel button should change name.", "Canceling", cancelProgressButton.Text);

				form.ActivateCloseButton();
				AssertEquals("Cancel button should change name.", "Close", cancelProgressButton.Text);

				cancelProgressButton.PerformClick();
				Application.DoEvents();
				AssertEquals("The form should be closed after click on Close button.", false, form.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ProgressWithDetailesForm();
		}

		ProgressWithDetailesForm GetNewForm()
		{
			return GetFormToBashCore() as ProgressWithDetailesForm;
		}

		Control GetControl(Form form, string controlName)
		{
			return form.Controls.Find(controlName, true)[0];
		}

		#endregion
	}
}
