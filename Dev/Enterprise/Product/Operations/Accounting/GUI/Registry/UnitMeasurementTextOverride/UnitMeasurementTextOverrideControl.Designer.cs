namespace Enterprise.Accounting.Registry.GUI
{
	partial class UnitMeasurementTextOverrideControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnitMeasurementTextOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnitMeasurementTextOverrideGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.UnitMeasurementTextOverrideCollection);
			// 
			// UnitMeasurementTextOverrideGrid
			//
			this.UnitMeasurementTextOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnitMeasurementTextOverrideGrid, ".");
			this.UnitMeasurementTextOverrideGrid.CaptionVisible = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			zDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2B51D813-8458-4F8A-8E7E-30126B7981CB", "Unit Measurement");
			zDropEditColumnStyleInfo.ColumnName = "UnitMeasurement";
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1D22EE08-0DC0-43A1-BCF7-3647C6952752", "Text Override");
			zTextBoxColumnStyleInfo.ColumnName = "TextOverride";
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.UnitMeasurementTextOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			this.UnitMeasurementTextOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			this.UnitMeasurementTextOverrideGrid.CopySelectedRowsAllowed = true;
			this.UnitMeasurementTextOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnitMeasurementTextOverrideGrid.GridId = "84fdf4ec-f065-4c8b-ad5a-00d5d786e13b";
			this.UnitMeasurementTextOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnitMeasurementTextOverrideGrid.LayoutKey = "UnitMeasurementTextOverrideGrid";
			this.UnitMeasurementTextOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnitMeasurementTextOverrideGrid.Name = "UnitMeasurementTextOverrideGrid";
			this.UnitMeasurementTextOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.UnitMeasurementTextOverrideGrid.TabIndex = 0;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnitMeasurementTextOverrideGrid);
			this.Name = "UnitMeasurementTextOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UnitMeasurementTextOverrideGrid)).EndInit();
			this.UnitMeasurementTextOverrideGrid.ResumeLayout(false);
			this.UnitMeasurementTextOverrideGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
