using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC
{
	public partial class MTDDeclarationForm : ZChildForm
	{
		public MTDDeclarationForm(MTDSubmissionDataColumns data) : base(data)
		{
			formData = data;
		}

		readonly MTDSubmissionDataColumns formData;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation
		void ConfirmButton_Click(object sender, EventArgs e)
		{
			if (formData.Declaration)
			{
				using (new CursorSwitcher(Cursors.WaitCursor))
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
			else
			{
				errorLabel.Visible = true;
				DialogResult = DialogResult.None;
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			formData.Declaration = false;
			Close();
		}
		#endregion
	}
}
