using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class ResendReasonSendForm : ZChildForm
{
	public ResendReasonSendForm()
	{
		InitializeComponent();
		WarningMessageLabel.Text = WarningMessage;
	}

	public string Reason { get; set; }

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
			Globals.Message.ShowError(Res.GetString("F4D5BD31-0532-4C14-B162-91F35CDB38BC", "Please enter reasons"));
		}
		else
		{
			result = true;
		}

		return result;
	}

	string WarningMessage => Res.GetString("6675C50E-5EBF-4CAA-916C-8336F549E34A",
		@"Please note that you have not received a response for the message that you have submitted previously.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ");
}
