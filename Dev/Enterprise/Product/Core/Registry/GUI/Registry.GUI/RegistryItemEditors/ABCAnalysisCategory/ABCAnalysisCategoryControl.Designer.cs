namespace Enterprise.Registry.GUI
{
	public partial class ABCAnalysisCategoryControl : RegistryZUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ABCAnalysisCategoryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ABCAnalysisCategoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.ABCAnalysisCategoryCollection);
			// 
			// ABCAnalysisCategoryGrid
			// 
			this.ABCAnalysisCategoryGrid.AllowNavigation = false;
			this.ABCAnalysisCategoryGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.ABCAnalysisCategoryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.ABCAnalysisCategory)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ABCAnalysisCategory)(null)).CategoryName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ABCAnalysisCategory)(null)).Operator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.ABCAnalysisCategory)(null)).PercentageOfTotal)));
			this.ABCAnalysisCategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f796665b-cf6d-4edb-a275-c02a20abf5c6", "Name", "Category Name", "");
			zTextBoxColumnStyleInfo1.ColumnName = "CategoryName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5c5424b4-b65c-48cf-a4d0-ad979f6efb4b", "Operator");
			zTextBoxColumnStyleInfo2.ColumnName = "Operator";
			zTextBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("33508dcc-daf6-42e6-8b9f-0e2c7fc30286", "Percentage", "Percentage of Total", "");
			zCalcEditColumnStyleInfo1.ColumnName = "PercentageOfTotal";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ABCAnalysisCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ABCAnalysisCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ABCAnalysisCategoryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ABCAnalysisCategoryGrid.GridId = "80af5ce3-8ef6-41b7-8b24-523b14c32dc0";
			this.ABCAnalysisCategoryGrid.CopySelectedRowsAllowed = true;
			this.ABCAnalysisCategoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ABCAnalysisCategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ABCAnalysisCategoryGrid.LayoutKey = "ABCAnalysisCategoryGrid";
			this.ABCAnalysisCategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ABCAnalysisCategoryGrid.Name = "ABCAnalysisCategoryGrid";
			this.ABCAnalysisCategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			this.ABCAnalysisCategoryGrid.TabIndex = 0;
			// 
			// ABCAnalysisCategoryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ABCAnalysisCategoryGrid);
			this.Name = "ABCAnalysisCategoryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ABCAnalysisCategoryGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.ZGrid ABCAnalysisCategoryGrid;
	}
}
