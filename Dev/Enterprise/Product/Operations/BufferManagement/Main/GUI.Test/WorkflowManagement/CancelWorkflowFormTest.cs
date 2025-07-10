using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(CancelWorkflowForm))]
	class CancelWorkflowFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CancelWorkflowForm();
		}

		public void TestCancelTextAccessible()
		{
			using (var formToTest = new CancelWorkflowForm())
			{
				var textBox = formToTest.Controls.Find("cancellationReasonTextBox", true).FirstOrDefault();

				AssertNotNull(textBox);

				var cancelText = "this is a cancellation reason";

				textBox.Text = cancelText;

				AssertEquals(cancelText, formToTest.CancellationReasonText);
			}
		}
	}
}
