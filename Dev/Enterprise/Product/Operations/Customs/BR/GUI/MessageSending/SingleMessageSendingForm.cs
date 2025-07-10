using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class SingleMessageSendingForm : ZChildForm
	{
		public SingleMessageSendingForm(BaseSingleMessageSendingObject sendingObjectParent)
			: base(sendingObjectParent)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void SendButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (MessageSendingEnviromentChecker.CheckIsOKToSend())
				{
					DialogResult = DialogResult.OK;
				}
			}
		}
	}
}
