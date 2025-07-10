using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AssignClaimPopupForm : ZChildForm
	{
		public AssignClaimPopupForm(AccQueryClaimReassignAction action)
			: base(action)
		{
		}

		public override string FormVerb
		{
			get { return String.Empty; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			bool success = ((AccQueryClaimReassignAction)BusinessEntity).Sycnhronise();
			if (success)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}

