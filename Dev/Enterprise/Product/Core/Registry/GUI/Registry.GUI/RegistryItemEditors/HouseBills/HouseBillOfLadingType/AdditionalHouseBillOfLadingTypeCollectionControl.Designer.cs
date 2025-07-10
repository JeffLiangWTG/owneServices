namespace Enterprise.Registry.GUI
{
	public partial class AdditionalHouseBillOfLadingTypeCollectionControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid Grid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AdditionalHouseBillOfLadingTypeCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AdditionalHouseBillOfLadingType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AdditionalHouseBillOfLadingType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AdditionalHouseBillOfLadingType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AdditionalHouseBillOfLadingType)(null)).Enable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AdditionalHouseBillOfLadingType)(null)).EnableMessaging)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AdditionalHouseBillOfLadingTypeCollectionControl|d986d0e9-e126-4f07-afae-9f5dc47349b8", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("HouseBillOfLadingTypeCollectionControl|417d7581-a6bd-409e-9147-20ce6e89cb06", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AdditionalHouseBillOfLadingTypeCollectionControl|381038cd-cea9-4981-973f-d34fda8cee16", "Enable");
			zCheckBoxColumnStyleInfo1.ColumnName = "Enable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AdditionalHouseBillOfLadingTypeCollectionControl|0d35308c-acc2-43b0-86be-322c5a0bb025", "Enable Messaging");
			zCheckBoxColumnStyleInfo2.ColumnName = "EnableMessaging";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.Grid.GridId = "2c20d03f-7c59-43ab-bb1a-f90e2f13169b";
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 408, true);
			this.Grid.TabIndex = 0;
			// 
			// HouseBillOfLadingTypeCollectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Grid);
			this.Name = "AdditionalHouseBillOfLadingTypeCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
