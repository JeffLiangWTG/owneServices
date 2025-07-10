namespace Enterprise.Registry.GUI
{
	partial class PerformanceReportingControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.MetricGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CategoryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MetricGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.PerformanceReportingMetricCollection);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.MetricGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.CategoryGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 449, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.splitContainer1.TabIndex = 0;
			// 
			// MetricGrid
			// 
			this.MetricGrid.AllowNavigation = false;
			this.MetricGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MetricGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricName)));
			this.MetricGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3783af6e-dda2-4f07-8538-d159c7a8317d", "Metric Name");
			zTextBoxColumnStyleInfo1.ColumnName = "MetricName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			this.MetricGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MetricGrid.CopySelectedRowsAllowed = true;
			this.MetricGrid.GridId = "6d3bbfcc-951a-4b44-971e-024ff6912ce9";
			this.MetricGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MetricGrid.LayoutKey = "MetricGrid";
			this.MetricGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MetricGrid.Name = "MetricGrid";
			this.MetricGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 141, true);
			this.MetricGrid.TabIndex = 0;
			// 
			// CategoryGrid
			// 
			this.CategoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CategoryGrid, "MetricCategories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricCategories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetricCategory)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricCategories)).SyncRoot)).CategoryName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetricCategory)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricCategories)).SyncRoot)).Operator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetricCategory)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricCategories)).SyncRoot)).Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetricCategory)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PerformanceReportingMetric)(null)).MetricCategories)).SyncRoot)).IsTarget)));
			this.CategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b8ac75da-0200-4991-b251-54bfa9576d80", "Category Name");
			zTextBoxColumnStyleInfo2.ColumnName = "CategoryName";
			zTextBoxColumnStyleInfo2.IsSortable = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ceb223c3-947c-4ee0-a516-d50030007599", "Operator");
			zTextBoxColumnStyleInfo3.ColumnName = "Operator";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsSortable = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("63491399-f86c-4dc5-9b00-25ffca8ffb68", "Value");
			zCalcEditColumnStyleInfo1.ColumnName = "Value";
			zCalcEditColumnStyleInfo1.IsSortable = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4aa33233-0fa8-407b-bbec-f5668200fe59", "Is Target");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsTarget";
			zCheckBoxColumnStyleInfo1.IsSortable = false;
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CategoryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CategoryGrid.CopySelectedRowsAllowed = true;
			this.CategoryGrid.GridId = "eaff4805-f6e2-4d18-97e9-c2f13eb121be";
			this.CategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CategoryGrid.LayoutKey = "CategoryGrid";
			this.CategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CategoryGrid.Name = "CategoryGrid";
			this.CategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 301, true);
			this.CategoryGrid.TabIndex = 0;
			// 
			// PerformanceReportingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "PerformanceReportingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MetricGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;

		#if DEBUG
		internal
		#endif
		ZArchitecture.ZGrid CategoryGrid;

		#if DEBUG
		internal
		#endif
		ZArchitecture.ZGrid MetricGrid;
	}
}
