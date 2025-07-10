using System.Windows.Forms;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI
{
	public partial class EmailDetailsForm : ZChildForm
	{
		public EmailDetailsForm()
		{
			InitializeComponent();
		}

		public EmailDetailsForm(EmailExportInstructions bizObj) : base(bizObj)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		EmailExportInstructions BusinessObject
		{
			get { return (EmailExportInstructions)BusinessEntity; }
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		protected virtual void HandleSend()
		{
			BusinessObject.RunPreSaveValidation();
			if (!BusinessObject.HasErrors)
			{
				BusinessObject.SplitUserEnteredRecipientsIntoSeparateAddresses();
				DialogResult = DialogResult.OK;
			}
		}

		void SendButton_Click(object sender, System.EventArgs e)
		{
			HandleSend();
		}
	}
}
