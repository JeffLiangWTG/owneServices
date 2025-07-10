using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA
{
	public partial class VATAdjustmentReasonForm : ZChildForm
	{
		public VATAdjustmentReasonForm(LIQSubmissionDataRow submissionDataRow) : base(submissionDataRow)
		{
		}

		LIQSubmissionDataRow SubmissionDataRow => BusinessEntity as LIQSubmissionDataRow;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			okButton.Enabled = !SubmissionDataRow.ReadOnly;
		}

		#region Implementation
		void OKButton_Click(object sender, EventArgs e)
		{
			SubmissionDataRow.RunPreSaveValidation();
			if (!SubmissionDataRow.ReasonHolder.HasErrors)
			{
				using (new CursorSwitcher(Cursors.WaitCursor))
				{
					DialogResult = DialogResult.OK;
					SubmissionDataRow.AdjustmentAproved();
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("fdfc802c-4fd0-4492-8c5b-97d9f2db2d45", "Please resolve all errors before saving."));
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		#endregion

		#region Debuging Gets
#if DEBUG
		public ZArchitecture.ZCalcEdit CW1Box => cw1Box;
		public ZArchitecture.ZCalcEdit AdjustmentsBox => adjustmentsBox;
		public ZArchitecture.ZCalcEdit TotalBox => totalBox;
#endif
		#endregion
	}
}
