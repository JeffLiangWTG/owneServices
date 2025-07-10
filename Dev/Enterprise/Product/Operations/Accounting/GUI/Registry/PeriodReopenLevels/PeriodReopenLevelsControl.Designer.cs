namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PeriodReopenLevelsControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PeriodReopenLevelsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PeriodReopenLevelsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.PeriodReopenLevelsCollection);
			// 
			// PeriodReopenLevelsGrid
			// 
			this.PeriodReopenLevelsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PeriodReopenLevelsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.PeriodReopenLevels)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PeriodReopenLevels)(null)).AuthorisationRequirement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.PeriodReopenLevels)(null)).Days)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PeriodReopenLevels)(null)).Range)));
			this.PeriodReopenLevelsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodReopenLevelsControl|1624b9d8-f3d8-44a3-9401-9de8c5537ff4", "Level", "Reopen Level", "Period Reopen Level", "");
			zDropEditColumnStyleInfo1.ColumnName = "AuthorisationRequirement";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodReopenLevelsControl|f8b3e938-9749-4552-8f72-1cfcc3222a10", "Days");
			zCalcEditColumnStyleInfo1.ColumnName = "Days";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodReopenLevelsControl|07e23e7a-54d8-4191-93cd-71eb5ac2061b", "Desc.", "Description", "Description", "");
			zDropEditColumnStyleInfo2.ColumnName = "Range";
			this.PeriodReopenLevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PeriodReopenLevelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PeriodReopenLevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PeriodReopenLevelsGrid.GridId = "669a2cc4-ffa6-47ac-bd9b-a82bd5cdaa45";
			this.PeriodReopenLevelsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PeriodReopenLevelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PeriodReopenLevelsGrid.LayoutKey = "PeriodReopenLevelsGrid";
			this.PeriodReopenLevelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PeriodReopenLevelsGrid.Name = "PeriodReopenLevelsGrid";
			this.PeriodReopenLevelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.PeriodReopenLevelsGrid.TabIndex = 0;
			// 
			// PeriodReopenLevelsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodReopenLevelsGrid);
			this.Name = "PeriodReopenLevelsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PeriodReopenLevelsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal ZArchitecture.ZGrid PeriodReopenLevelsGrid;
	}
}
