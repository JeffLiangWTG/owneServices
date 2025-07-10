using System;
using System.Windows.Forms;

using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class SendToForm : ZChildForm
	{
		public SendToForm()
			: base(new SendToAddress())
		{ }

		public SendToAddress SendToAddress
		{
			get { return (SendToAddress)BusinessEntity; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (ValidateSendToAddress())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#region Validation

		bool ValidateSendToAddress()
		{
			var validationFailed = false;

			SendToAddress.RunPreSaveValidation();
			if (SendToAddress.HasErrors)
			{
				validationFailed = true;
				ShowErrorsDialog();
			}

			return !validationFailed;
		}

		#endregion
	}
}

