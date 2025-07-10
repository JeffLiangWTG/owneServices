namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithThreeGroupsControl : RegistryZUserControl
	{
		#region Component Designer generated code

		protected internal Enterprise.ZArchitecture.ZGrid CodeDescriptionWithThreeGroupsGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionColumnStyleInfo;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CodeDescriptionWithThreeGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithThreeGroupsGrid)).BeginInit();
			this.CodeDescriptionWithThreeGroupsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionWithThreeGroupsCollection);
			// 
			// CodeDescriptionWithThreeGroupsGrid
			// 
			this.CodeDescriptionWithThreeGroupsGrid.AllowNavigation = false;
			this.CodeDescriptionWithThreeGroupsGrid.AllowSorting = false;
			this.CodeDescriptionWithThreeGroupsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeDescriptionWithThreeGroupsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).CodeColumnType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).MainDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Group)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).GroupLookup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Group2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Group2Lookup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).ExtraDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Group3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithThreeGroups)(null)).Group3Lookup)));
			this.CodeDescriptionWithThreeGroupsGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "CodeList";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("98f17f8b-6c5c-4bd4-94d1-f63ec4c803c8", "Code");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "Code";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CodeColumnType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("577fdbe9-6d2a-4910-bf1f-167a62be9ab6", "Main Description");
			zTextBoxColumnStyleInfo1.ColumnName = "MainDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "GroupLookup";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ce154c49-b904-492a-8001-bddb5d7a5d26", "Group");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Group";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Group2Lookup";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f057c0b3-e646-46c8-8d53-e1f83f4652cb", "Group 2");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "Group2";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("530f6b76-1031-48d3-b4a8-c4f4dcdd49d0", "Extra Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ExtraDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo3.BindToList = "Group3Lookup";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8ecedf65-4a10-48b3-b7dd-a4e6d677ef23", "Group 3");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Group3";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CodeDescriptionWithThreeGroupsGrid.GridId = "d7f613f8-4dc4-4127-9173-901fc8d522d3";
			this.CodeDescriptionWithThreeGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionWithThreeGroupsGrid.LayoutKey = "CodeDescriptionWithThreeGroupsGrid";
			this.CodeDescriptionWithThreeGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionWithThreeGroupsGrid.Name = "CodeDescriptionWithThreeGroupsGrid";
			this.CodeDescriptionWithThreeGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1021, 183, true);
			this.CodeDescriptionWithThreeGroupsGrid.TabIndex = 0;
			// 
			// CodeDescriptionWithThreeGroupsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionWithThreeGroupsGrid);
			this.Name = "CodeDescriptionWithThreeGroupsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithThreeGroupsGrid)).EndInit();
			this.CodeDescriptionWithThreeGroupsGrid.ResumeLayout(false);
			this.CodeDescriptionWithThreeGroupsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			#endregion
		}
	}
}
