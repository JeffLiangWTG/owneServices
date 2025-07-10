using System;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class EdiAddUserAgreementAcceptanceForm : ZChildForm
	{
		public EdiAddUserAgreementAcceptanceForm(EdiUserAgreementAcceptanceLog agreementAcceptanceLog) : base(agreementAcceptanceLog)
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;
			this.agreementAcceptanceLog = agreementAcceptanceLog;
		}

		readonly EdiUserAgreementAcceptanceLog agreementAcceptanceLog;

		public override string FormCaption => ResString.GetMultilingualString("04b0ab03-4cd5-481d-a34e-dcf829ee5717", "Add acceptance");

		void AddAcceptanceButton_Click(object sender, EventArgs e)
		{
			agreementAcceptanceLog.Validation.ValidateAll();
			agreementAcceptanceLog.RefreshBinding();
			if (!agreementAcceptanceLog.HasErrors)
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
