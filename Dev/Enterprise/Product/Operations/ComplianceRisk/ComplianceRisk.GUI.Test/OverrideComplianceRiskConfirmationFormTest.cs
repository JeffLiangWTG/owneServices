using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(OverrideComplianceRiskConfirmationForm))]
	public class OverrideComplianceRiskConfirmationFormTest : ZFormBasherTest
	{
		public void TestDefaultLabelAndTextDescription()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var warningJobReferenceLabel = form.Controls.Find("WarningJobReferenceLabel", true)[0] as ZLabel;
				AssertEquals("Override the compliance risk of job DummyUniqueRef", warningJobReferenceLabel.Text);

				var warningLabel1 = form.Controls.Find("WarningLabel1", true)[0] as ZLabel;
				AssertEquals("This action overrides compliance restrictions set by your organization.", warningLabel1.Text);
				Assert(warningLabel1.IsFontBold);

				var warningLabel2 = form.Controls.Find("WarningLabel2", true)[0] as ZLabel;
				AssertEquals("Only authorized compliance personnel that understand the impact to your organization should proceed.", warningLabel2.Text);

				var warningLabel3 = form.Controls.Find("WarningLabel3", true).SingleOrDefault() as ZLabel;
				AssertEquals("Note, that by completing this action your details will be recorded for audit purposes.", warningLabel3.Text);

				var confirmationLabel1 = form.Controls.Find("ConfirmationLabel1", true)[0] as ZLabel;
				AssertEquals("Provide an override reason:", confirmationLabel1.Text);

				var confirmationLabel2 = form.Controls.Find("ConfirmationLabel2", true)[0] as ZLabel;
				AssertEquals("To continue, enter your name 'CargoWise Support'.", confirmationLabel2.Text);

				var confirmationLabel3 = form.Controls.Find("ConfirmationLabel3", true)[0] as ZLabel;
				AssertEquals("I,", confirmationLabel3.Text);

				var confirmationJobReferenceLabel = form.Controls.Find("ConfirmationJobReferenceLabel", true)[0] as ZLabel;
				AssertEquals(", understand the consequences of the override action and I am authorized to clear the job DummyUniqueRef", confirmationJobReferenceLabel.Text);
			}
		}

		public void TestOverrideButton_NoError()
		{
			var confirmationModel = new OverrideComplianceRiskConfirmationModel();

			using (var form = new OverrideComplianceRiskConfirmationForm("DummyUniqueRef", confirmationModel))
			{
				form.Show();
				var overrideButton = form.Controls.Find("OverrideButton", true)[0] as ZButton;
				AssertEquals(false, overrideButton.Enabled);

				var userConfirmationTextBox = form.Controls.Find("UserConfirmationTextBox", true)[0] as ZTextBox;
				userConfirmationTextBox.Text = EnvProxy.Instance.CurrentUser.FullName;

				AssertEquals(true, overrideButton.Enabled);
				AssertNotEquals(DialogResult.OK, form.DialogResult);

				var overrideClearReasonTextBox = form.Controls.Find("OverrideClearReasonTextBox", true)[0] as ZTextBox;
				confirmationModel.Reason = "ABCD ABCDEF";

				overrideButton.PerformClick();

				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals("ABCD ABCDEF", confirmationModel.Reason);
			}
		}

		public void TestOverrideButton_HasErrors()
		{
			var confirmationModel = new OverrideComplianceRiskConfirmationModel();

			using (var form = new OverrideComplianceRiskConfirmationForm("DummyUniqueRef", confirmationModel))
			{
				form.Show();
				var overrideButton = form.Controls.Find("OverrideButton", true)[0] as ZButton;
				AssertEquals(false, overrideButton.Enabled);

				var userConfirmationTextBox = form.Controls.Find("UserConfirmationTextBox", true)[0] as ZTextBox;
				userConfirmationTextBox.Text = EnvProxy.Instance.CurrentUser.FullName;

				AssertEquals(true, overrideButton.Enabled);
				AssertNotEquals(DialogResult.OK, form.DialogResult);

				overrideButton.PerformClick();

				AssertNotEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(confirmationModel.GetErrors().ToMessageListString(), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestUserConfirmationTextBoxColorValidation()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var userConfirmationTextBox = form.Controls.Find("UserConfirmationTextBox", true)[0] as ZTextBox;
				userConfirmationTextBox.Text = EnvProxy.Instance.CurrentUser.FullName.Substring(0, 3);
				AssertEquals(Color.LightCoral, userConfirmationTextBox.BackColor);

				userConfirmationTextBox.Text = EnvProxy.Instance.CurrentUser.FullName;
				AssertEquals(Color.LightGreen, userConfirmationTextBox.BackColor);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "UserConfirmationTextBox")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			var form = new OverrideComplianceRiskConfirmationForm("DummyUniqueRef", new OverrideComplianceRiskConfirmationModel());
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("OverrideClearReasonTextBox", true)[0]);
			return form;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
