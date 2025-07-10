namespace Enterprise.Registry.GUI
{
	partial class PickGroupControl : RegistryZUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PickGroupGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PickGroupGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.PickGroupCollection);
			// 
			// PickGroupGrid
			// 
			this.PickGroupGrid.AllowNavigation = false;
			this.PickGroupGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.PickGroupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.PickGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickGroup)(null)).PickSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.PickGroup)(null)).EnglishDescription)));
			this.PickGroupGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("fd4add7b-a899-490e-8a26-361c0c37d856", "Pick Seq.", "Pick Sequence", "");
			zCalcEditColumnStyleInfo1.ColumnName = "PickSequence";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("165157e2-5946-4252-95d2-2b0318353ddf", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.PickGroupGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PickGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PickGroupGrid.CopySelectedRowsAllowed = true;
			this.PickGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickGroupGrid.GridId = "80af5ce3-8ef6-41b7-8b24-523b14c32dc0";
			this.PickGroupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PickGroupGrid.LayoutKey = "PickGroupGrid";
			this.PickGroupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickGroupGrid.Name = "PickGroupGrid";
			this.PickGroupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			this.PickGroupGrid.TabIndex = 0;
			// 
			// PickGroupControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickGroupGrid);
			this.Name = "PickGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PickGroupGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.ZGrid PickGroupGrid;
	}
}
