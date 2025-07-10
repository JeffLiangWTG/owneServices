namespace Enterprise.Registry.GUI
{
	partial class IndexDurationRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RetentionTimeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RetentionTimeGrid)).BeginInit();
			this.RetentionTimeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefaultCollection);
			// 
			// Grid
			// 
			this.RetentionTimeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RetentionTimeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IndexDuration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IndexDuration)(null)).Table)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.IndexDuration)(null)).DurationInMonths)));
			this.RetentionTimeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("table-column-name", "Table");
			zDropEditColumnStyleInfo1.ColumnName = "Table";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "LowWatermarkTables";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("retention-in-months-column-name", "Maximum record age (Months)");
			zTextBoxColumnStyleInfo1.ColumnName = "DurationInMonths";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.RetentionTimeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RetentionTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RetentionTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RetentionTimeGrid.GridId = "03c6cd20-4b8a-4e0e-9303-bff04e3f98bf";
			this.RetentionTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RetentionTimeGrid.LayoutKey = "Grid";
			this.RetentionTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RetentionTimeGrid.Name = "Grid";
			this.RetentionTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.RetentionTimeGrid.TabIndex = 0;
			// 
			// IndexDurationRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RetentionTimeGrid);
			this.Name = "IndexDurationRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RetentionTimeGrid)).EndInit();
			this.RetentionTimeGrid.ResumeLayout(false);
			this.RetentionTimeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid RetentionTimeGrid;
	}
}
