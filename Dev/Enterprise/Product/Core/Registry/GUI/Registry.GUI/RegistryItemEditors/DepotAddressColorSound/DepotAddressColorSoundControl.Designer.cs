namespace Enterprise.Registry.GUI
{
	public partial class DepotAddressColorSoundControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid depotAddressColorSoundGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo colorSelectorColumnStyleInfo1 = new Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo();
			Enterprise.Registry.GUI.MP3FileSelectorColumnStyleInfo mP3FileSelectorColumnStyleInfo1 = new Enterprise.Registry.GUI.MP3FileSelectorColumnStyleInfo();
			this.depotAddressColorSoundGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.depotAddressColorSoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DepotAddressColorSoundCollection);
			// 
			// depotAddressColorSoundGrid
			// 
			this.depotAddressColorSoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.depotAddressColorSoundGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DepotAddressColorSound)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.DepotAddressColorSound)(null)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.DepotAddressColorSound)(null)).Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DepotAddressColorSound)(null)).Color)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DepotAddressColorSound)(null)).MP3FileName)));
			this.depotAddressColorSoundGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BDB79E08-1B28-4EF0-B8BF-06F1673B21AF", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Organisation";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0DD07C22-184A-4F2E-B554-F597DFAD930A", "Address");
			zGuidDropEditColumnStyleInfo1.ColumnName = "Address";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			colorSelectorColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("14EF4041-D65F-467E-9D4E-87A0C807810B", "Color (R,G,B)");
			colorSelectorColumnStyleInfo1.ColumnName = "Color";
			colorSelectorColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			colorSelectorColumnStyleInfo1.IsMandatory = true;
			colorSelectorColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			mP3FileSelectorColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A7A08A1C-B6EE-427C-9DFB-516409B5486A", "MP3 File");
			mP3FileSelectorColumnStyleInfo1.ColumnName = "MP3FileName";
			mP3FileSelectorColumnStyleInfo1.IsMandatory = true;
			mP3FileSelectorColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			mP3FileSelectorColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.depotAddressColorSoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.depotAddressColorSoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.depotAddressColorSoundGrid.ColumnStyles.Add(colorSelectorColumnStyleInfo1);
			this.depotAddressColorSoundGrid.ColumnStyles.Add(mP3FileSelectorColumnStyleInfo1);
			this.depotAddressColorSoundGrid.CopySelectedRowsAllowed = true;
			this.depotAddressColorSoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.depotAddressColorSoundGrid.GridId = "67BF7631-69E1-4846-9EAE-B0AEB76497B1";
			this.depotAddressColorSoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.depotAddressColorSoundGrid.LayoutKey = "DepotAddressColorSoundGrid";
			this.depotAddressColorSoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.depotAddressColorSoundGrid.Name = "depotAddressColorSoundGrid";
			this.depotAddressColorSoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.depotAddressColorSoundGrid.TabIndex = 0;
			this.depotAddressColorSoundGrid.ShowMassUpdateMenuItem = false;
			// 
			// DepotAddressColorSoundControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.depotAddressColorSoundGrid);
			this.Name = "DepotAddressColorSoundControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.depotAddressColorSoundGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
