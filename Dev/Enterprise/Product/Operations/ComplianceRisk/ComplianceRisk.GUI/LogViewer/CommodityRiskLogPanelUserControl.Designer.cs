namespace Enterprise.ComplianceRisk.GUI
{
	partial class CommodityRiskLogPanelUserControl
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
			this.CommodityAssessmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityAssessmentRiskLogUserControl = new Enterprise.ComplianceRisk.GUI.CommodityRiskStatusLogUserControl();
			this.CommodityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityRiskLogGrid = new Enterprise.ComplianceRisk.GUI.CommodityRiskLogGrid();
			this.CommodityAssessmentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityAssessmentGroupBox.SuspendLayout();
			this.CommodityAssessmentRiskLogUserControl.SuspendLayout();
			this.CommodityDetailsGroupBox.SuspendLayout();
			this.CommodityRiskLogGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityAssessmentSplitContainer)).BeginInit();
			this.CommodityAssessmentSplitContainer.Panel1.SuspendLayout();
			this.CommodityAssessmentSplitContainer.Panel2.SuspendLayout();
			this.CommodityAssessmentSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLogCollection);
			// 
			// CommodityAssessmentGroupBox
			// 
			this.CommodityAssessmentGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("185b093f-97db-41b7-8348-10dcd64fd24c", "Compliance Assessment");
			this.CommodityAssessmentGroupBox.Controls.Add(this.CommodityAssessmentRiskLogUserControl);
			this.CommodityAssessmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityAssessmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityAssessmentGroupBox.Name = "CommodityAssessmentGroupBox";
			this.CommodityAssessmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 141, true);
			this.CommodityAssessmentGroupBox.TabIndex = 5;
			this.CommodityAssessmentGroupBox.TabStop = false;
			// 
			// CommodityAssessmentRiskLogUserControl
			// 
			this.CommodityAssessmentRiskLogUserControl.AllowDrop = true;
			this.CommodityAssessmentRiskLogUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CommodityAssessmentRiskLogUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(null)))));
			this.CommodityAssessmentRiskLogUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityAssessmentRiskLogUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.CommodityAssessmentRiskLogUserControl.Name = "CommodityAssessmentRiskLogUserControl";
			this.CommodityAssessmentRiskLogUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 107, true);
			this.CommodityAssessmentRiskLogUserControl.TabIndex = 0;
			// 
			// CommodityDetailsGroupBox
			// 
			this.CommodityDetailsGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("bcedc491-faf8-4458-9406-e5e6e276ffd2", "Commodity Details");
			this.CommodityDetailsGroupBox.Controls.Add(this.CommodityRiskLogGrid);
			this.CommodityDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityDetailsGroupBox.Name = "CommodityDetailsGroupBox";
			this.CommodityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 141, true);
			this.CommodityDetailsGroupBox.TabIndex = 6;
			this.CommodityDetailsGroupBox.TabStop = false;
			// 
			// CommodityRiskLogGrid
			// 
			this.CommodityRiskLogGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityRiskLogGrid, ".");
			this.CommodityRiskLogGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityRiskLogGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.CommodityRiskLogGrid.Name = "CommodityRiskLogGrid";
			this.CommodityRiskLogGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 107, true);
			this.CommodityRiskLogGrid.TabIndex = 0;
			// 
			// CommodityAssessmentSplitContainer
			// 
			this.CommodityAssessmentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityAssessmentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityAssessmentSplitContainer.Name = "CommodityAssessmentSplitContainer";
			// 
			// CommodityAssessmentSplitContainer.Panel1
			// 
			this.CommodityAssessmentSplitContainer.Panel1.Controls.Add(this.CommodityDetailsGroupBox);
			// 
			// CommodityAssessmentSplitContainer.Panel2
			// 
			this.CommodityAssessmentSplitContainer.Panel2.Controls.Add(this.CommodityAssessmentGroupBox);
			this.CommodityAssessmentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1143, 141, true);
			this.CommodityAssessmentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(811);
			this.CommodityAssessmentSplitContainer.SplitterWidth = 5;
			this.CommodityAssessmentSplitContainer.TabIndex = 4;
			// 
			// CommodityRiskLogPanelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.Controls.Add(this.CommodityAssessmentSplitContainer);
			this.Name = "CommodityRiskLogPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1143, 141, true);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityAssessmentGroupBox.ResumeLayout(false);
			this.CommodityAssessmentGroupBox.PerformLayout();
			this.CommodityAssessmentRiskLogUserControl.ResumeLayout(true);
			this.CommodityAssessmentRiskLogUserControl.PerformLayout();
			this.CommodityDetailsGroupBox.ResumeLayout(false);
			this.CommodityDetailsGroupBox.PerformLayout();
			this.CommodityRiskLogGrid.ResumeLayout(true);
			this.CommodityRiskLogGrid.PerformLayout();
			this.CommodityAssessmentSplitContainer.Panel1.ResumeLayout(false);
			this.CommodityAssessmentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CommodityAssessmentSplitContainer)).EndInit();
			this.CommodityAssessmentSplitContainer.ResumeLayout(false);
			this.CommodityAssessmentSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CommodityAssessmentGroupBox;
		private ZArchitecture.GUI.ZGroupBox CommodityDetailsGroupBox;
		private CargoWise.Windows.UI.KSplitContainer CommodityAssessmentSplitContainer;
		internal CommodityRiskLogGrid CommodityRiskLogGrid;
		private CommodityRiskStatusLogUserControl CommodityAssessmentRiskLogUserControl;
	}
}
