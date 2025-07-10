namespace Enterprise.BufferManagement.GUI
{
	partial class LastSuccessfulReleaseUserControl
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
			this.SuccessfulReleaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SuccessfulReleaseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuccessfulReleaseGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			// 
			// SuccessfulReleaseGroupBox
			// 
			this.SuccessfulReleaseGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bb3a2c6a-d246-4e07-8d02-9a4a5f0e3c8b", "Last Successful Release");
			this.SuccessfulReleaseGroupBox.Controls.Add(this.HintLabel);
			this.SuccessfulReleaseGroupBox.Controls.Add(this.SuccessfulReleaseTextBox);
			this.SuccessfulReleaseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuccessfulReleaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SuccessfulReleaseGroupBox.Name = "SuccessfulReleaseGroupBox";
			this.SuccessfulReleaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 455, true);
			this.SuccessfulReleaseGroupBox.TabIndex = 0;
			this.SuccessfulReleaseGroupBox.TabStop = false;
			// 
			// HintLabel
			// 
			this.HintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e5f36ea4-8b59-41af-9f9c-29eb0ffa7453", "Listed below are capacity details valid at the time this workflow was last released to a Buffer. This will be cleared when the workflow is closed. Note that resource available capacity listed below could be different from available capacity displayed on Visual Boards. This is due to capacity being reserved by other items earlier in the release sequence queue.");
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 27, true);
			this.HintLabel.TabIndex = 1;
			this.HintLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SuccessfulReleaseTextBox
			// 
			this.SuccessfulReleaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SuccessfulReleaseTextBox, "LastSuccessfulRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).LastSuccessfulRelease)));
			this.SuccessfulReleaseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SuccessfulReleaseTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SuccessfulReleaseTextBox, false);
			this.SuccessfulReleaseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
			this.SuccessfulReleaseTextBox.Multiline = true;
			this.SuccessfulReleaseTextBox.Name = "SuccessfulReleaseTextBox";
			this.SuccessfulReleaseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SuccessfulReleaseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 403, true);
			this.SuccessfulReleaseTextBox.TabIndex = 0;
			// 
			// LastSuccessfulReleaseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SuccessfulReleaseGroupBox);
			this.Name = "LastSuccessfulReleaseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 455, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SuccessfulReleaseGroupBox.ResumeLayout(false);
			this.SuccessfulReleaseGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SuccessfulReleaseGroupBox;
		private ZArchitecture.ZLabel HintLabel;
		private ZArchitecture.ZTextBox SuccessfulReleaseTextBox;
	}
}
