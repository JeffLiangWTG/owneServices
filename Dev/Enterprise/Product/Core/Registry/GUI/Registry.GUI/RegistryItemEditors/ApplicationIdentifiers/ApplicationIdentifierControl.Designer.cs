namespace Enterprise.Registry.GUI
{
	public partial class ApplicationIdentifierControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid ApplicationIdentifierGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ApplicationIdentifierGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationIdentifierGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.ApplicationIdentifierCollection);
			// 
			// ApplicationIdentifierGrid
			// 
			this.ApplicationIdentifierGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApplicationIdentifierGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).ApplicationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).FullTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).DataType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).DataTypesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).MinFieldLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).MaxFieldLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.ApplicationIdentifier)(null)).DataTitle)));
			this.ApplicationIdentifierGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|32c43fec-b663-41e4-ac8e-ada98de1571a", "AI");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplicationID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|4db799fb-f82b-472e-8f17-76ab7381f679", "Full Title");
			zTextBoxColumnStyleInfo2.ColumnName = "FullTitle";
			zDropEditColumnStyleInfo1.BindToList = "DataTypesList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|9ea742c4-2a4a-4c4a-b1c3-764eacdaa549", "Data");
			zDropEditColumnStyleInfo1.ColumnName = "DataType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|63e54d3e-ab5e-45ed-9286-e68e454c112e", "Min Length");
			zCalcEditColumnStyleInfo1.ColumnName = "MinFieldLength";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|8f5b284e-3510-4097-823b-c11b85b24c9b", "Max Length");
			zCalcEditColumnStyleInfo2.ColumnName = "MaxFieldLength";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ApplicationIdentifierControl|82983f74-1b8e-4930-ad82-930b48305f54", "Data Title");
			zTextBoxColumnStyleInfo3.ColumnName = "DataTitle";
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ApplicationIdentifierGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ApplicationIdentifierGrid.GridId = "80af5ce3-8ef6-41b7-8b24-523b14c32dc0";
			this.ApplicationIdentifierGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApplicationIdentifierGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicationIdentifierGrid.LayoutKey = "ApplicationIdentifierGrid";
			this.ApplicationIdentifierGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicationIdentifierGrid.Name = "ApplicationIdentifierGrid";
			this.ApplicationIdentifierGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			this.ApplicationIdentifierGrid.TabIndex = 0;
			// 
			// ApplicationIdentifierControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ApplicationIdentifierGrid);
			this.Name = "ApplicationIdentifierControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationIdentifierGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
