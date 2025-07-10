namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class PortOfDischargeUserControl
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
			this.PortOfDischargePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfDischargeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfDischargePanel.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader);
			// 
			// PortOfDischargePanel
			// 
			this.PortOfDischargePanel.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.PortOfDischargePanel.Controls.Add(this.PortOfDischargeTextBox);
			this.PortOfDischargePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortOfDischargePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfDischargePanel.Name = "PortOfDischargePanel";
			this.PortOfDischargePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			this.PortOfDischargePanel.TabIndex = 0;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "AMA_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfDischarge)));
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeCodeFindBox.ParentType = null;
			this.PortOfDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 1;
			// 
			// PortOfDischargeTextBox
			//
			this.BindingSource.SetBindingMember(this.PortOfDischargeTextBox, "PortOfDischargeIATACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).PortOfDischargeIATACode)));
			this.PortOfDischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.PortOfDischargeTextBox.Name = "PortOfDischargeTextBox";
			this.PortOfDischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PortOfDischargeTextBox.TabIndex = 2;
			// 
			// PortOfDischargeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PortOfDischargePanel);
			this.Name = "PortOfDischargeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfDischargePanel.ResumeLayout(false);
			this.PortOfDischargePanel.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel PortOfDischargePanel;
		private ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox;
		private ZArchitecture.ZTextBox PortOfDischargeTextBox;
	}
}
