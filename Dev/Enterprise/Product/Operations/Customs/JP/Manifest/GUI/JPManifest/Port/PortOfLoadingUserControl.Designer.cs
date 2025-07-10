namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class PortOfLoadingUserControl
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
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
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
			this.PortOfLoadingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfLoadingPanel.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader);
			// 
			// PortOfLoadingPanel
			// 
			this.PortOfLoadingPanel.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.PortOfLoadingPanel.Controls.Add(this.PortOfLoadingTextBox);
			this.PortOfLoadingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortOfLoadingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfLoadingPanel.Name = "PortOfLoadingPanel";
			this.PortOfLoadingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			this.PortOfLoadingPanel.TabIndex = 0;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "AMA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfLoading)));
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingCodeFindBox.ParentType = null;
			this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 1;
			// 
			// PortOfLoadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfLoadingTextBox, "PortOfLoadingIATACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).PortOfLoadingIATACode)));
			this.PortOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.PortOfLoadingTextBox.Name = "PortOfLoadingTextBox";
			this.PortOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PortOfLoadingTextBox.TabIndex = 2;
			// 
			// PortOfLoadingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PortOfLoadingPanel);
			this.Name = "PortOfLoadingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfLoadingPanel.ResumeLayout(false);
			this.PortOfLoadingPanel.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel PortOfLoadingPanel;
		private ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox PortOfLoadingTextBox;
	}
}
