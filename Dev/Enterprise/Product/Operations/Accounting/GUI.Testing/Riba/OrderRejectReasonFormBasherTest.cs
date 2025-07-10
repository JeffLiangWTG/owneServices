using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Riba.Testing
{
	[TestedType(typeof(OrderRejectReasonForm))]
	public class OrderRejectReasonFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.ReversingCode = "IDE";
			invoice.ReversingReason = "some reason goes here";

			RejectOrderReasonHolder rejectholder = new RejectOrderReasonHolder();
			rejectholder.HasChanges = false;

			return new OrderRejectReasonForm(rejectholder, "Please enter some reason", Res.GetString("26107194-7cd0-4a2a-bc55-b3f72d3af0de", "Test form"));
		}

		public void TestOrderRejectReasonFormReasonCodeAndDescription()
		{
			using (var form = new OrderRejectReasonForm(new RejectOrderReasonHolder(), "reject", "reject"))
			{
				form.Show();

				var rejectReasonCodeDropEdit = (ZDropEdit)form.Controls.Find("RejectReasonCodeDropEdit", true).Single();
				var reasonTextBox = (ZTextBox)form.Controls.Find("ReasonTextBox", true).Single();
				var oKReasonButton = (ZButton)form.Controls.Find("OKReasonButton", true).Single();

				rejectReasonCodeDropEdit.CodeBox.Text = string.Empty;
				reasonTextBox.Text = "any description";
				UnitTestUserNotification.Instance.ClearMessages();
				oKReasonButton.PerformClick();
				AssertEquals("Enter a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				rejectReasonCodeDropEdit.CodeBox.Text = "ABC";
				AssertEquals("selected reason code is not in the drop down list values", -1, rejectReasonCodeDropEdit.SelectedIndex);
				reasonTextBox.Text = "any description";
				UnitTestUserNotification.Instance.ClearMessages();
				oKReasonButton.PerformClick();
				AssertEquals("Enter a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				rejectReasonCodeDropEdit.CodeBox.Text = "TXT";
				reasonTextBox.Text = string.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				oKReasonButton.PerformClick();
				AssertEquals("Please enter a non-blank reason text!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOrderRejectReasonForm_Closed()
		{
			var rejectOrderReasonHolder = new RejectOrderReasonHolder();
			using (var form = new OrderRejectReasonForm(rejectOrderReasonHolder, "reject", "reject"))
			{
				form.Show();

				var rejectReasonCodeDropEdit = (ZDropEdit)form.Controls.Find("RejectReasonCodeDropEdit", true).Single();
				var reasonTextBox = (ZTextBox)form.Controls.Find("ReasonTextBox", true).Single();

				rejectReasonCodeDropEdit.CodeBox.Text = "ABC";
				reasonTextBox.Text = "any description";
				UnitTestUserNotification.Instance.ClearMessages();
				form.Close();
				Assert("form is closed", !form.Visible);
				AssertNull("No validation popup", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("reason value is reset", string.Empty, rejectOrderReasonHolder.Reason);
			}
		}

		#endregion
	}
}
