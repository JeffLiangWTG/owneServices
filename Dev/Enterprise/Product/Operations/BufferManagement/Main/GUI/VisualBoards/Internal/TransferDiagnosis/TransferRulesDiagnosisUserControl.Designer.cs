namespace Enterprise.BufferManagement.GUI
{
	partial class TransferRulesDiagnosisUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BufferReleaseCriteriaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BufferReleaseCriteriaHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FailureReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BufferReleaseCriteriaGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			// 
			// BufferReleaseCriteriaGroupBox
			// 
			this.BufferReleaseCriteriaGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ab801ed0-43b8-494e-b62c-db597cbb0ff2", "Buffer Release Criteria");
			this.BufferReleaseCriteriaGroupBox.Controls.Add(this.BufferReleaseCriteriaHintLabel);
			this.BufferReleaseCriteriaGroupBox.Controls.Add(this.FailureReasonTextBox);
			this.BufferReleaseCriteriaGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BufferReleaseCriteriaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BufferReleaseCriteriaGroupBox.Name = "BufferReleaseCriteriaGroupBox";
			this.BufferReleaseCriteriaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 170, true);
			this.BufferReleaseCriteriaGroupBox.TabIndex = 11;
			this.BufferReleaseCriteriaGroupBox.TabStop = false;
			// 
			// BufferReleaseCriteriaHintLabel
			// 
			this.BufferReleaseCriteriaHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BufferReleaseCriteriaHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("7c627cbf-47ea-4e33-bd9b-0e5e8221539b", "In order to be released to a buffer, all resources having tasks assigned to them in that workflow must have adequate capacity. Additionally, if there are tasks that require a capability and do not have a resource assigned, all resources who possess that capability will need to have adequate capacity for those tasks. The estimated duration of these capability tasks is divided amongst the resources who possess the capability.");
			this.BufferReleaseCriteriaHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.BufferReleaseCriteriaHintLabel.Name = "BufferReleaseCriteriaHintLabel";
			this.BufferReleaseCriteriaHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 56, true);
			this.BufferReleaseCriteriaHintLabel.TabIndex = 1;
			// 
			// FailureReasonTextBox
			// 
			this.FailureReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FailureReasonTextBox, "TransferDiagnoses.FailureReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).FailureReason)));
			this.FailureReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FailureReasonTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FailureReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 94, true);
			this.FailureReasonTextBox.Multiline = true;
			this.FailureReasonTextBox.Name = "FailureReasonTextBox";
			this.FailureReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FailureReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 70, true);
			this.FailureReasonTextBox.TabIndex = 1;
			// 
			// TransferRulesDiagnosisUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BufferReleaseCriteriaGroupBox);
			this.Name = "TransferRulesDiagnosisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 367, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BufferReleaseCriteriaGroupBox.ResumeLayout(false);
			this.BufferReleaseCriteriaGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox BufferReleaseCriteriaGroupBox;
		private ZArchitecture.ZLabel BufferReleaseCriteriaHintLabel;
		private ZArchitecture.ZTextBox FailureReasonTextBox;
	}
}
