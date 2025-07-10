using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AmendmentReasonForm : Customs.GUI.AmendmentReasonForm
	{
		public AmendmentReasonForm(CMRAmendmentWithdrawalReason reason) : base(reason)
		{
		}

		public AmendmentReasonForm(CMRAmendmentWithdrawalReason reason, ResourceStringData preamble) : base(reason)
		{
			this.preamble = preamble;
			UpdateLayoutForPreamble();
		}

		readonly ResourceStringData preamble;

		void UpdateLayoutForPreamble()
		{
			this.preambleLabel = new ZArchitecture.ZLabel();

			this.SuspendLayout();
			// 
			// PreambleLabel
			// 
			this.preambleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 0, true);
			this.preambleLabel.Name = "PreambleLabel";
			this.preambleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 54, true);
			this.preambleLabel.TabIndex = 1;
			this.preambleLabel.CaptionResourceString = preamble;
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 54, true);
			this.ReasonLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BE84425D-D09A-4489-8D4C-16C25A343F13", "Please enter a reason for the amendment:");
			// 
			// ReasonTextTextBox
			// 
			this.ReasonTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 78, true);
			this.ReasonTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 58, true);

			this.Controls.Add(this.preambleLabel);
			this.StartPosition = FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.preambleLabel, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected ZArchitecture.ZLabel preambleLabel;
	}
}
