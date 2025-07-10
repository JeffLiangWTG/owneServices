using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class OverrideComplianceRiskConfirmationForm : ZChildForm
	{
		public OverrideComplianceRiskConfirmationForm(string jobUniqueRef, OverrideComplianceRiskConfirmationModel confirmationModel)
			: base(confirmationModel)
		{
			InitializeComponent();

			this.confirmationModel = confirmationModel;
			this.ActiveControl = this.OverrideButton;

			this.WarningJobReferenceLabel.Text = ResString.GetMultilingualString("2B6DAC42-E563-4C6E-A87B-559A4471F800", "Override the compliance risk of job {0}", jobUniqueRef);
			this.WarningLabel1.Text = ResString.GetMultilingualString("70A30805-F2CB-4B5A-98AB-66F6ADF0D690", "This action overrides compliance restrictions set by your organization.");
			this.WarningLabel2.Text = ResString.GetMultilingualString("E85C53B3-8DE3-4C18-9327-87834C3C58EE", "Only authorized compliance personnel that understand the impact to your organization should proceed.");
			this.WarningLabel3.Text = ResString.GetMultilingualString("B83ED74D-E8C0-4B30-93E3-EBCB983EC88C", "Note, that by completing this action your details will be recorded for audit purposes.");

			this.ConfirmationLabel1.Text = ResString.GetMultilingualString("EEA4376B-9798-4118-B26A-26DE80509C89", "Provide an override reason:");
			this.ConfirmationLabel2.Text = ResString.GetMultilingualString("DB309E79-0000-472C-8A1D-37C6162FF670", "To continue, enter your name '{0}'.", EnvProxy.Instance.CurrentUser.FullName);
			this.ConfirmationLabel3.Text = ResString.GetMultilingualString("E73A6B09-8360-4A47-960A-59B4820C9EB5", "I,");
			this.ConfirmationJobReferenceLabel.Text = ResString.GetMultilingualString("D4F97F6E-3168-4546-8C1D-4BB7E334819A", ", understand the consequences of the override action and I am authorized to clear the job {0}", jobUniqueRef);

			this.UserConfirmationTextBox.PlaceHolderText = EnvProxy.Instance.CurrentUser.FullName;
		}

		readonly OverrideComplianceRiskConfirmationModel confirmationModel;

		public override string FormVerb => string.Empty;

		void UserConfirmationTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(UserConfirmationTextBox.Text))
			{
				if (UserConfirmationTextBox.Text != EnvProxy.Instance.CurrentUser.FullName)
				{
					UserConfirmationTextBox.ColorChanger.ForceBackColor(Color.LightCoral);
				}
				else
				{
					UserConfirmationTextBox.ColorChanger.ForceBackColor(Color.LightGreen);
				}
			}
			OverrideButton.Enabled = UserConfirmationTextBox.Text == EnvProxy.Instance.CurrentUser.FullName;
		}

		void UserConfirmationTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (UserConfirmationTextBox.SelectionStart < UserConfirmationTextBox.PlaceHolderText.Length)
			{
				var expectedChar = UserConfirmationTextBox.PlaceHolderText[UserConfirmationTextBox.SelectionStart];
				var startPosition = UserConfirmationTextBox.SelectionStart;
				if (char.IsLetter(e.KeyChar) && char.IsLetter(expectedChar))
				{
					if (UserConfirmationTextBox.SelectionLength > 0)
					{
						UserConfirmationTextBox.Text = UserConfirmationTextBox.Text.Remove(UserConfirmationTextBox.SelectionStart, UserConfirmationTextBox.SelectionLength);
					}

					var charToInsert = char.IsUpper(expectedChar) ? char.ToUpper(e.KeyChar, CultureInfo.InvariantCulture) : char.ToLower(e.KeyChar, CultureInfo.InvariantCulture);
					UserConfirmationTextBox.Text = UserConfirmationTextBox.Text.Insert(startPosition, charToInsert.ToString());
					UserConfirmationTextBox.SelectionStart = startPosition + 1;
					e.Handled = true;
				}
			}
		}

		void OverrideButton_Click(object sender, EventArgs e)
		{
			confirmationModel.RunPreSaveValidation();

			if (confirmationModel.HasErrors)
			{
				Globals.Message.ShowError(confirmationModel.GetErrors().ToMessageListString());
			}
			else
			{
				SetDialogResult(DialogResult.OK);
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			SetDialogResult(DialogResult.Cancel);
		}

		void SetDialogResult(DialogResult result)
		{
			DialogResult = result;
			Close();
		}

		protected override bool ProcessTabKeyCore(bool forward)
		{
			confirmationModel.Reason = OverrideClearReasonTextBox.Text;
			return base.ProcessTabKeyCore(forward);
		}
	}
}
