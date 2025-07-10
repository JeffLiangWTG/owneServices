using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideTransactionAgreedPaymentMethodForm))]
	public class OverrideTransactionAgreedPaymentMethodFormBasherTest : OverrideInvoiceDetailsFormTest
	{
		#region Implementation

		BusinessObject bizObj;

		protected override Form GetFormToBashCore()
		{
			return new OverrideTransactionAgreedPaymentMethodForm(new OverrideTransactionAgreedPaymentMethodHelper(Factory, bizObj?.PK ?? ZGuid.Empty));
		}

		public void TestUpperPanelAndRegistryMessageForPayables()
		{
			var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed = true;
			security.PayablesModifyDueDateForPosted.IsAllowed = true;
			var formType = typeof(OverrideInvoiceDetailsForm);

			bizObj = Factory.New<APInvoice>();

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);

				AssertEquals("Upper Panel is hidden", false, upperPanel.Visible);
			}

			security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed = false;
			security.PayablesModifyDueDateForPosted.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Agreed Payment Method Registry",
					"To override Agreed Payment Method, the following security right is required: Manage > Payables > Payables Transactions > Modify Agreed Payment Method",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}

			security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed = true;
			security.PayablesModifyDueDateForPosted.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Due Date",
					"To override Due Date, the following security right is required: Manage > Payables > Payables Transactions > Modify Due Date",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}
		}

		public void TestUpperPanelAndRegistryMessageForReceivables()
		{
			var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed = true;
			security.ReceivablesModifyDueDateForPosted.IsAllowed = true;
			var formType = typeof(OverrideInvoiceDetailsForm);

			bizObj = Factory.New<ARInvoice>();

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);

				AssertEquals("Upper Panel is hidden", false, upperPanel.Visible);
			}

			security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed = false;
			security.ReceivablesModifyDueDateForPosted.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Agreed Payment Method Registry",
					"To override Agreed Payment Method, the following security right is required: Manage > Receivables > Receivables Transactions > Modify Agreed Payment Method",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}

			security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed = true;
			security.ReceivablesModifyDueDateForPosted.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Due Date",
					"To override Due Date, the following security right is required: Manage > Receivables > Receivables Transactions > Modify Due Date",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}
		}

		#endregion
	}
}
