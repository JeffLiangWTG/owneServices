using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5UnloadingRemarksTabUserControl
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
			this.components = new System.ComponentModel.Container();
			this.UnloadingRemarksTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.UnloadingDifferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicUnloadingDifferencesTabUserControl = new ZDynamicControlCreationUserControl();
			this.HouseConsignmentDifferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentDifferencesTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentDifferencesTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingRemarksTabControl.SuspendLayout();
			this.UnloadingDifferencesTabPage.SuspendLayout();
			this.DynamicUnloadingDifferencesTabUserControl.SuspendLayout();
			this.HouseConsignmentDifferencesTabPage.SuspendLayout();
			this.HouseConsignmentDifferencesTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// UnloadingRemarksTabControl
			// 
			this.UnloadingRemarksTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.UnloadingRemarksTabControl.Controls.Add(this.UnloadingDifferencesTabPage);
			this.UnloadingRemarksTabControl.Controls.Add(this.HouseConsignmentDifferencesTabPage);
			this.UnloadingRemarksTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingRemarksTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingRemarksTabControl.Name = "UnloadingRemarksTabControl";
			this.UnloadingRemarksTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			this.UnloadingRemarksTabControl.TabIndex = 0;
			// 
			// UnloadingDifferencesTabPage
			// 
			this.UnloadingDifferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("642303e7-42d9-4dfb-8b79-2c055a0f67ae", "Unloading Differences");
			this.UnloadingDifferencesTabPage.Controls.Add(this.DynamicUnloadingDifferencesTabUserControl);
			this.UnloadingDifferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.UnloadingDifferencesTabPage.Name = "UnloadingDifferencesTabPage";
			this.UnloadingDifferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingDifferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 709, true);
			this.UnloadingDifferencesTabPage.TabIndex = 4;
			this.UnloadingDifferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// UnloadingDifferencesTabUserControl
			// 
			this.DynamicUnloadingDifferencesTabUserControl.AllowDrop = true;
			this.DynamicUnloadingDifferencesTabUserControl.AutoSize = true;
			this.DynamicUnloadingDifferencesTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.DynamicUnloadingDifferencesTabUserControl, ".");
			this.DynamicUnloadingDifferencesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicUnloadingDifferencesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicUnloadingDifferencesTabUserControl.Name = "DynamicUnloadingDifferencesTabUserControl";
			this.DynamicUnloadingDifferencesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 703, true);
			this.DynamicUnloadingDifferencesTabUserControl.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesTabPage
			// 
			this.HouseConsignmentDifferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ff2b23ee-ecc4-4041-8d7a-36423cecf138", "House Consignment Differences");
			this.HouseConsignmentDifferencesTabPage.Controls.Add(this.HouseConsignmentDifferencesTabUserControl);
			this.HouseConsignmentDifferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.HouseConsignmentDifferencesTabPage.Name = "HouseConsignmentDifferencesTabPage";
			this.HouseConsignmentDifferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseConsignmentDifferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 709, true);
			this.HouseConsignmentDifferencesTabPage.TabIndex = 4;
			this.HouseConsignmentDifferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentDifferencesTabUserControl
			// 
			this.HouseConsignmentDifferencesTabUserControl.AllowDrop = true;
			this.HouseConsignmentDifferencesTabUserControl.AutoSize = true;
			this.HouseConsignmentDifferencesTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.HouseConsignmentDifferencesTabUserControl, "Bills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsBillCollection<Enterprise.Customs.EU.NCTS.Business.NctsBill>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Bills)));
			this.HouseConsignmentDifferencesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentDifferencesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentDifferencesTabUserControl.Name = "HouseConsignmentDifferencesTabUserControl";
			this.HouseConsignmentDifferencesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 703, true);
			this.HouseConsignmentDifferencesTabUserControl.TabIndex = 0;
			// 
			// Phase5UnloadingRemarksTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingRemarksTabControl);
			this.Name = "Phase5UnloadingRemarksTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingRemarksTabControl.ResumeLayout(false);
			this.UnloadingRemarksTabControl.PerformLayout();
			this.UnloadingDifferencesTabPage.ResumeLayout(false);
			this.UnloadingDifferencesTabPage.PerformLayout();
			this.DynamicUnloadingDifferencesTabUserControl.ResumeLayout(true);
			this.DynamicUnloadingDifferencesTabUserControl.PerformLayout();
			this.HouseConsignmentDifferencesTabPage.ResumeLayout(false);
			this.HouseConsignmentDifferencesTabPage.PerformLayout();
			this.HouseConsignmentDifferencesTabUserControl.ResumeLayout(true);
			this.HouseConsignmentDifferencesTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal ZTabControl UnloadingRemarksTabControl;
		internal ZTabPage UnloadingDifferencesTabPage;
		internal ZDynamicControlCreationUserControl DynamicUnloadingDifferencesTabUserControl;
		internal ZTabPage HouseConsignmentDifferencesTabPage;
		internal HouseConsignmentDifferencesTabUserControl HouseConsignmentDifferencesTabUserControl;
	}
}
