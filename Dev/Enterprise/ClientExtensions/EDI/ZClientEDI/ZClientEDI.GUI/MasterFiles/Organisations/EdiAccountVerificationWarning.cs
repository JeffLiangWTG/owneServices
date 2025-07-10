using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EdiAccountVerificationWarning : ZChildForm
	{
		public EdiAccountVerificationWarning()
		{
			InitializeComponent();
		}

		public EdiAccountVerificationWarning(EDIOrgContact ediOrgContact)
			: base(ediOrgContact)
		{
			InitializeComponent();
		}

		void CancelProceedButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		void ProceedButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
