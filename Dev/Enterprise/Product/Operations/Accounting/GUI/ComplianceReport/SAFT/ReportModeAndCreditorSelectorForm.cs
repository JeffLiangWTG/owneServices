using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.SAFT
{
	public partial class ReportModeAndCreditorSelectorForm : ZChildForm
	{
		public ReportModeAndCreditorSelectorForm(ReportModeAndCreditorSelector data) : base(data)
		{
			formData = data;
		}

		readonly ReportModeAndCreditorSelector formData;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation
		void ContinueButton_Click(object sender, EventArgs e)
		{
			formData.RunPreSaveValidation();
			if (!formData.HasErrors)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		#endregion
	}
}
