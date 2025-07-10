namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPTemporaryLandingUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OtherLawsandRegulationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherLawsandRegulationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TemporaryLandingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JPTemporaryLandingDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherLawsandRegulationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherLawsandRegulationsGrid)).BeginInit();
			this.OtherLawsandRegulationsGrid.SuspendLayout();
			this.TemporaryLandingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaBill);
			// 
			// OtherLawsandRegulationsGroupBox
			// 
			this.OtherLawsandRegulationsGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("e4990dc8-d1fd-410d-a8f5-54f44a765cb6", "Other Laws and Regulations");
			this.OtherLawsandRegulationsGroupBox.Controls.Add(this.OtherLawsandRegulationsGrid);
			this.OtherLawsandRegulationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherLawsandRegulationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 145, true);
			this.OtherLawsandRegulationsGroupBox.Name = "OtherLawsandRegulationsGroupBox";
			this.OtherLawsandRegulationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 218, true);
			this.OtherLawsandRegulationsGroupBox.TabIndex = 6;
			this.OtherLawsandRegulationsGroupBox.TabStop = false;
			// 
			// OtherLawsandRegulationsGrid
			// 
			this.OtherLawsandRegulationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherLawsandRegulationsGrid, "OtherLawsandRegulations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).OtherLawsandRegulations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.CusOtherLawReference)(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).OtherLawsandRegulations)).SyncRoot)).CFR_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.CusOtherLawReference)(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).OtherLawsandRegulations)).SyncRoot)).CodeDescription)));
			this.OtherLawsandRegulationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "CFR_Reference";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CodeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OtherLawsandRegulationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OtherLawsandRegulationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OtherLawsandRegulationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherLawsandRegulationsGrid.GridId = "9720B1A4-9D3D-49CE-8BDB-608C5BB00A18";
			this.OtherLawsandRegulationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherLawsandRegulationsGrid.LayoutKey = "OtherLawsandGrid";
			this.OtherLawsandRegulationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OtherLawsandRegulationsGrid.Name = "OtherLawsandRegulationsGrid";
			this.OtherLawsandRegulationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 199, true);
			this.OtherLawsandRegulationsGrid.TabIndex = 1;
			// 
			// TemporaryLandingPanel
			// 
			this.TemporaryLandingPanel.Controls.Add(this.JPTemporaryLandingDynamicLayoutPanel);
			this.TemporaryLandingPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TemporaryLandingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemporaryLandingPanel.Name = "TemporaryLandingPanel";
			this.TemporaryLandingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 145, true);
			this.TemporaryLandingPanel.TabIndex = 1;
			// 
			// JPTemporaryLandingDynamicLayoutPanel
			// 
			this.JPTemporaryLandingDynamicLayoutPanel.AllowDrop = true;
			this.JPTemporaryLandingDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JPTemporaryLandingDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JPTemporaryLandingDynamicLayoutPanel.Name = "JPTemporaryLandingDynamicLayoutPanel";
			this.JPTemporaryLandingDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 145, true);
			this.JPTemporaryLandingDynamicLayoutPanel.TabIndex = 0;
			// 
			// JPTemporaryLandingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OtherLawsandRegulationsGroupBox);
			this.Controls.Add(this.TemporaryLandingPanel);
			this.Name = "JPTemporaryLandingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 363, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherLawsandRegulationsGroupBox.ResumeLayout(false);
			this.OtherLawsandRegulationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherLawsandRegulationsGrid)).EndInit();
			this.OtherLawsandRegulationsGrid.ResumeLayout(false);
			this.OtherLawsandRegulationsGrid.PerformLayout();
			this.TemporaryLandingPanel.ResumeLayout(false);
			this.TemporaryLandingPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox OtherLawsandRegulationsGroupBox;
		ZArchitecture.ZGrid OtherLawsandRegulationsGrid;
		ZArchitecture.GUI.ZPanel TemporaryLandingPanel;
		ZArchitecture.GUI.DynamicLayoutPanel JPTemporaryLandingDynamicLayoutPanel;
	}
}
