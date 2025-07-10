using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ConfirmSendForm : ZChildForm
	{
		public ConfirmSendForm(bool isHasMRN = false)
		{
			InitializeComponent();
			if (isHasMRN)
			{
				WarningMessageLabel.Text = ResubmissionWarningMessage;
			}
			else
			{
				WarningMessageLabel.Text = WarningMessage;
			}
		}

		public string Reason;

		void SendButton_Click(object sender, EventArgs e)
		{
			if (CheckIsOkToConfirm())
			{
				DialogResult = DialogResult.OK;
				Reason = ReasonTextBox.Text;
				Close();
			}
		}

		bool CheckIsOkToConfirm()
		{
			var result = false;
			if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("5840CB5F-A7A6-466E-846A-05B96502C0C0", "Please enter reasons"));
			}
			else
			{
				result = true;
			}

			return result;
		}

		string WarningMessage => Res.GetString("5763E909-14FB-4782-BD02-A4AA098D21FA",
			@"Please note that you have not received a response for the message that you have submitted previously.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ");

		string ResubmissionWarningMessage => Res.GetString("8CF2DB7F-F17C-4F52-AE9C-41E8FF0EE7D1",
			@"The entry already has a MRN number, submitting the original message might overwrite the MRN number by creating a duplicate entry in Irish customs system.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ");
	}
}
