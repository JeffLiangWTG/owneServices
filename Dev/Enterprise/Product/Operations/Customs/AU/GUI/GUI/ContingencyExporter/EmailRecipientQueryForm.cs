using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class EmailRecipientQueryForm : ZChildForm
	{
		public EmailRecipientQueryForm(EmailRecipientSelection emailSelector) : base(emailSelector)
		{
			this.emailSelector = emailSelector;
		}

		readonly EmailRecipientSelection emailSelector;

		#region Send Button

		void SendButton_Click(object sender, System.EventArgs e)
		{
			emailSelector.RunPreSaveValidation();
			if (!emailSelector.HasErrors)
			{
				emailSelector.AddNewRecipientIfRequired();
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#endregion
	}
}
