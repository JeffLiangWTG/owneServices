using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class MessageInstructionForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MessageInstructionForm()
		{
			InitializeComponent();
		}

		readonly MessageInstruction instruction;
		public MessageInstructionForm(MessageInstruction instruction)
			: base(instruction)
		{
			InitializeComponent();
			this.SetDataBinding(instruction, "");
			this.instruction = instruction;
		}

		void MessageInstructionForm_Load(object sender, EventArgs e)
		{
			this.IsContinueWithAWaitingForResponseCheckBox.Visible = instruction.IsWaitingForResponse;

			this.IsContinueWithValidationErrorsCheckBox.Visible = instruction.ContainsValidationErrors;
			var isSecurityOK = instruction.IsAllowedToSendWithMessageErrors;
			this.IsContinueWithValidationErrorsCheckBox.Enabled = isSecurityOK;
			this.SecurityNotAllowedLabel.Visible = !isSecurityOK;

			this.IsContinueWithAdditionalWarningsCheckBox.Visible = instruction.ContainsAdditionalWarnings;
			this.AdditionalWarningsTextBox.Visible = instruction.ContainsAdditionalWarnings;
			this.BillingJobReadyForPostingCheckBox.Visible = instruction.ShowJobReadyForPosting;

			if (!instruction.ContainsAdditionalWarnings)
			{
				this.MessageTextBoxSplitContainer.Panel2Collapsed = true;
			}
			else if (!instruction.ContainsValidationErrors)
			{
				this.MessageTextBoxSplitContainer.Panel1Collapsed = true;
			}

			UpdateSendButtonEnabled();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			Close();
		}

		void zCancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		void UserConfirmationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateSendButtonEnabled();
		}

		void UpdateSendButtonEnabled()
		{
			this.SendButton.Enabled = ((!this.IsContinueWithAWaitingForResponseCheckBox.Visible || this.IsContinueWithAWaitingForResponseCheckBox.Checked)
				&& (!this.IsContinueWithValidationErrorsCheckBox.Visible || this.IsContinueWithValidationErrorsCheckBox.Checked)
				&& (!this.IsContinueWithAdditionalWarningsCheckBox.Visible || this.IsContinueWithAdditionalWarningsCheckBox.Checked))
				|| (!this.IsContinueWithAWaitingForResponseCheckBox.Visible && !this.IsContinueWithValidationErrorsCheckBox.Visible
				&& !this.IsContinueWithAdditionalWarningsCheckBox.Visible && this.BillingJobReadyForPostingCheckBox.Visible);
		}
	}
}
