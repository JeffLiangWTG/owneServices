using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class REXCertificateSelectionForm : ZChildForm
	{
		public REXCertificateSelectionForm(CertificateReissueHeader header)
			: base(header)
		{
		}

		public override string FormVerb => string.Empty;
		public override string FormCaption => Res.GetString("27126440-1171-48A5-959C-9BC1ECDA305E", "Reissue Quarantine Certificate");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!BusinessEntity.HasMessageErrors() || Globals.Message.Show("There are message errors. Are you sure you wish to continue?", "Continue to send", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
			{
				DialogResult = DialogResult.OK;
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
