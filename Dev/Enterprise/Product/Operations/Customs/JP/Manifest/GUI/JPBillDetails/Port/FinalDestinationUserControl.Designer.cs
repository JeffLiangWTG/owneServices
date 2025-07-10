namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class FinalDestinationUserControl
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
			this.FinalDestinationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FinalDestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinalDestinationPanel.SuspendLayout();
			this.FinalDestinationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaBill);
			// 
			// FinalDestinationPanel
			// 
			this.FinalDestinationPanel.Controls.Add(this.FinalDestinationCodeFindBox);
			this.FinalDestinationPanel.Controls.Add(this.FinalDestinationTextBox);
			this.FinalDestinationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FinalDestinationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationPanel.Name = "FinalDestinationPanel";
			this.FinalDestinationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			this.FinalDestinationPanel.TabIndex = 0;
			// 
			// FinalDestinationCodeFindBox
			// 
			this.FinalDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "ABL_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKFinalDestination)));
			this.FinalDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
			this.FinalDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationCodeFindBox.ParentType = null;
			this.FinalDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.FinalDestinationCodeFindBox.TabIndex = 1;
			// 
			// FinalDestinationTextBox
			//
			this.BindingSource.SetBindingMember(this.FinalDestinationTextBox, "FinalDestinationIATACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).FinalDestinationIATACode)));
			this.FinalDestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.FinalDestinationTextBox.Name = "FinalDestinationTextBox";
			this.FinalDestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.FinalDestinationTextBox.TabIndex = 2;
			// 
			// FinalDestinationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinalDestinationPanel);
			this.Name = "FinalDestinationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinalDestinationPanel.ResumeLayout(false);
			this.FinalDestinationPanel.PerformLayout();
			this.FinalDestinationCodeFindBox.ResumeLayout(true);
			this.FinalDestinationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel FinalDestinationPanel;
		private ZArchitecture.GUI.ZCodeFindBox FinalDestinationCodeFindBox;
		private ZArchitecture.ZTextBox FinalDestinationTextBox;
	}
}
