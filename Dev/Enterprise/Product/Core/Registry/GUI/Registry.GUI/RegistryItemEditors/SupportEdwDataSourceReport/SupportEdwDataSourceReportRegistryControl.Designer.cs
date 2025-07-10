namespace Enterprise.Registry.GUI
{
	partial class SupportEdwDataSourceReportRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReportListGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportListGrid)).BeginInit();
			this.ReportListGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SupportEdwDataSourceReportCollection);
			// 
			// CountryListGrid
			// 
			this.ReportListGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportListGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ReportListGrid.CaptionVisible = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SupportEdwDataSourceReport)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SupportEdwDataSourceReport)(null)).ReportName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SupportEdwDataSourceReport)(null)).BusinessContext)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReportListControl|95ce9d9c-d204-4e8f-9f09-f4dabf9fef4c", "Report Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ReportName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReportListControl|0e1f39f3-3b75-4fc8-a1d9-3071e6d26193", "Report Business Context");
			zTextBoxColumnStyleInfo2.ColumnName = "BusinessContext";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);

			this.ReportListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReportListGrid.GridId = "735d4509-fa70-45f9-8856-6b09ee7146c0";
			this.ReportListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportListGrid.LayoutKey = "ReportListGrid";
			this.ReportListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportListGrid.Name = "ReportListGrid";
			this.ReportListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 152, true);
			this.ReportListGrid.TabIndex = 0;
			// 
			// ReportListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportListGrid);
			this.Name = "ReportListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportListGrid)).EndInit();
			this.ReportListGrid.ResumeLayout(false);
			this.ReportListGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ReportListGrid;
	}
}
