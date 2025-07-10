namespace Enterprise.Registry.GUI
{
	public partial class LegTypesControl : RegistryZUserControl
	{
		protected internal Enterprise.ZArchitecture.ZGrid LegTypesGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.LegTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LegTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.LegType);
			// 
			// LegTypesGrid
			// 
			this.LegTypesGrid.AllowNavigation = false;
			this.LegTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LegTypesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).PickupFromOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).OrgType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).WaitPointOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).OrgType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).DeliverToOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).OrgType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).MovementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).MovementType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).Containerised)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).Containerised_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LegType)(null)).EquipmentGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LegType)(null)).EquipmentGroup_List)));
			this.LegTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|464d8de3-fb28-44b9-8bf1-c080d13090ec", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|3d7f2d65-e28a-4b41-a4e4-7776d4015d74", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.BindToList = "OrgType_List";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|e05f8834-7f4e-4802-a272-c7e38cb28734", "Pickup");
			zDropEditColumnStyleInfo1.ColumnName = "PickupFromOrg";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.BindToList = "OrgType_List";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|3b2b8aac-98a9-4693-a2a8-e339dec1b962", "Wait");
			zDropEditColumnStyleInfo2.ColumnName = "WaitPointOrg";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zDropEditColumnStyleInfo3.BindToList = "OrgType_List";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|bf8c8cfb-259a-4ec5-822b-bc412e26bac2", "Deliver");
			zDropEditColumnStyleInfo3.ColumnName = "DeliverToOrg";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.BindToList = "MovementType_List";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|13057c27-56d6-4ef8-a262-aea3566f3bd4", "Mov.");
			zDropEditColumnStyleInfo4.ColumnName = "MovementType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zDropEditColumnStyleInfo5.BindToList = "Containerised_List";
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|8dc65463-cda4-42cf-ac20-d596b3d843c0", "Mode");
			zDropEditColumnStyleInfo5.ColumnName = "Containerised";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zDropEditColumnStyleInfo6.BindToList = "EquipmentGroup_List";
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LegTypesControl|1d443078-3f77-48d3-8574-59ffb49bcbd0", "Equip");
			zDropEditColumnStyleInfo6.ColumnName = "EquipmentGroup";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			this.LegTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LegTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LegTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.LegTypesGrid.GridId = "2b7349da-9848-47c4-ba36-12e9fb6ab959";
			this.LegTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LegTypesGrid.LayoutKey = "LegTypesGrid";
			this.LegTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegTypesGrid.Name = "LegTypesGrid";
			this.LegTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 280, true);
			this.LegTypesGrid.TabIndex = 0;
			// 
			// LegTypesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LegTypesGrid);
			this.Name = "LegTypesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 280, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LegTypesGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
