namespace Enterprise.ComplianceRisk.GUI
{
	partial class RemovedCommoditiesLogViewerUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RemovedCommoditiesLogsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LogViewerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RemovedCommoditiesLogsGrid)).BeginInit();
			this.RemovedCommoditiesLogsGrid.SuspendLayout();
			this.LogViewerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.RemovedCommoditiesLogCollection);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.RemovedCommoditiesLogsGrid);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1422, 821, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(276);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(90);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(631);
			this.SplitContainer.SplitterWidth = 7;
			this.SplitContainer.TabIndex = 0;
			// 
			// RemovedCommoditiesLogsGrid
			// 
			this.RemovedCommoditiesLogsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RemovedCommoditiesLogsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.RemovedCommoditiesLog)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ComplianceRisk.Business.RemovedCommoditiesLog)(null)).EventDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ComplianceRisk.Business.RemovedCommoditiesLog)(null)).PostedDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.RemovedCommoditiesLog)(null)).User)));
			this.RemovedCommoditiesLogsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "EventDateTime";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.ColumnName = "PostedDateTimeLocal";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "User";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.RemovedCommoditiesLogsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RemovedCommoditiesLogsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.RemovedCommoditiesLogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RemovedCommoditiesLogsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemovedCommoditiesLogsGrid.GridId = "092ff7d0-01bb-40b4-87bf-305a23cac73f";
			this.RemovedCommoditiesLogsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RemovedCommoditiesLogsGrid.LayoutKey = "RemovedCommoditiesLogsGrid";
			this.RemovedCommoditiesLogsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RemovedCommoditiesLogsGrid.Name = "RemovedCommoditiesLogsGrid";
			this.RemovedCommoditiesLogsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1422, 631, true);
			this.RemovedCommoditiesLogsGrid.TabIndex = 2;
			// 
			// LogViewerGroupBox
			// 
			this.LogViewerGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("5c169410-c309-4765-a752-c3d31e29ae86", "Removed Commodities");
			this.LogViewerGroupBox.Controls.Add(this.SplitContainer);
			this.LogViewerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogViewerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogViewerGroupBox.Name = "LogViewerGroupBox";
			this.LogViewerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1426, 842, true);
			this.LogViewerGroupBox.TabIndex = 1;
			this.LogViewerGroupBox.TabStop = false;
			this.LogViewerGroupBox.Text = "Removed Commodities";
			// 
			// RemovedCommoditiesLogViewerUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LogViewerGroupBox);
			this.Name = "RemovedCommoditiesLogViewerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1426, 842, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RemovedCommoditiesLogsGrid)).EndInit();
			this.RemovedCommoditiesLogsGrid.ResumeLayout(false);
			this.RemovedCommoditiesLogsGrid.PerformLayout();
			this.LogViewerGroupBox.ResumeLayout(false);
			this.LogViewerGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LogViewerGroupBox;
		private ZArchitecture.ZGrid RemovedCommoditiesLogsGrid;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
