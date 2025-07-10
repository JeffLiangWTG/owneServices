namespace Enterprise.BufferManagement.GUI
{
	partial class FilterRulesDiagnosisUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FilterRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FilterRulesHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PassesFilterRulesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FilterStripsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FilterStripsControl = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterRulesGroupBox.SuspendLayout();
			this.FilterStripsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			// 
			// FilterRulesGroupBox
			// 
			this.FilterRulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterRulesGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("875a480f-5b30-4fa7-b15b-ca5a663b9a01", "Filter Rules");
			this.FilterRulesGroupBox.Controls.Add(this.FilterRulesHintLabel);
			this.FilterRulesGroupBox.Controls.Add(this.PassesFilterRulesCheckBox);
			this.FilterRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FilterRulesGroupBox.Name = "FilterRulesGroupBox";
			this.FilterRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 83, true);
			this.FilterRulesGroupBox.TabIndex = 11;
			this.FilterRulesGroupBox.TabStop = false;
			// 
			// FilterRulesHintLabel
			// 
			this.FilterRulesHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterRulesHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c7cb63b4-0a05-48c9-9dcc-5effb5f88996", "A workflow must match all filter strips defined below (if any) in order to be considered for transfer to the selected component. The workflow may also be subject to buffer release rules if the 'To Component' is a buffer and is configured to be part of a Release Gate. See the Buffer Release Criteria tab for more information.");
			this.FilterRulesHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.FilterRulesHintLabel.Name = "FilterRulesHintLabel";
			this.FilterRulesHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 40, true);
			this.FilterRulesHintLabel.TabIndex = 0;
			// 
			// PassesFilterRulesCheckBox
			// 
			this.PassesFilterRulesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PassesFilterRulesCheckBox, "TransferDiagnoses.PassedFilterRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).PassedFilterRules)));
			this.PassesFilterRulesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PassesFilterRulesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 61, true);
			this.PassesFilterRulesCheckBox.Name = "PassesFilterRulesCheckBox";
			this.PassesFilterRulesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
			this.PassesFilterRulesCheckBox.TabIndex = 7;
			this.PassesFilterRulesCheckBox.UseVisualStyleBackColor = true;
			// 
			// FilterStripsGroupBox
			// 
			this.FilterStripsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterStripsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("362a7f63-5e48-4a7c-932c-260621f00426", "Filter Strips");
			this.FilterStripsGroupBox.Controls.Add(this.FilterStripsControl);
			this.FilterStripsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 92, true);
			this.FilterStripsGroupBox.Name = "FilterStripsGroupBox";
			this.FilterStripsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 348, true);
			this.FilterStripsGroupBox.TabIndex = 12;
			this.FilterStripsGroupBox.TabStop = false;
			// 
			// FilterStripsControl
			// 
			this.FilterStripsControl.AllowDrop = true;
			this.FilterStripsControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.FilterStripsControl, "TransferDiagnoses.Link.FilterRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).Link.FilterRule)));
			this.FilterStripsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterStripsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.FilterStripsControl.Name = "FilterStripsControl";
			this.FilterStripsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 154, true);
			this.FilterStripsControl.TabIndex = 6;
			// 
			// FilterRulesDiagnosisUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterStripsGroupBox);
			this.Controls.Add(this.FilterRulesGroupBox);
			this.Name = "FilterRulesDiagnosisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 268, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterRulesGroupBox.ResumeLayout(false);
			this.FilterRulesGroupBox.PerformLayout();
			this.FilterStripsGroupBox.ResumeLayout(false);
			this.FilterStripsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FilterRulesGroupBox;
		private ZArchitecture.ZLabel FilterRulesHintLabel;
		private ZArchitecture.GUI.ZCheckBox PassesFilterRulesCheckBox;
		private ZArchitecture.GUI.ZGroupBox FilterStripsGroupBox;
		private Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl FilterStripsControl;
	}
}
