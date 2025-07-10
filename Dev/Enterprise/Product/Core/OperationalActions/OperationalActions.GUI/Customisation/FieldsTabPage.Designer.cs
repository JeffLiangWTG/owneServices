namespace Enterprise.Services.OperationalActions.GUI
{
	partial class FieldsTabPage
	{
		private void InitializeComponent()
		{
			CargoWise.Windows.UI.KSplitContainer fieldSplitContainer;
			Enterprise.ZArchitecture.ZGrid fieldsGrid;
			Enterprise.Services.OperationalActions.GUI.FieldFindBoxColumnStyleInfo fieldFindBoxColumnStyleInfo1 = new Enterprise.Services.OperationalActions.GUI.FieldFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.Services.OperationalActions.GUI.FieldFilterControl fieldFilter;
			fieldSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			fieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			fieldFilter = new Enterprise.Services.OperationalActions.GUI.FieldFilterControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			fieldSplitContainer.Panel1.SuspendLayout();
			fieldSplitContainer.Panel2.SuspendLayout();
			fieldSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(fieldsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalAction);
			// 
			// fieldSplitContainer
			// 
			fieldSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			fieldSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			fieldSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			fieldSplitContainer.Name = "fieldSplitContainer";
			// 
			// fieldSplitContainer.Panel1
			// 
			fieldSplitContainer.Panel1.Controls.Add(fieldsGrid);
			fieldSplitContainer.Panel1MinSize = 120;
			// 
			// fieldSplitContainer.Panel2
			// 
			fieldSplitContainer.Panel2.Controls.Add(fieldFilter);
			fieldSplitContainer.Panel2MinSize = 120;
			fieldSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			fieldSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(477);
			fieldSplitContainer.TabIndex = 2;
			// 
			// fieldsGrid
			// 
			fieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(fieldsGrid, "FieldDescriptors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).FieldCaption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).Filter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).EmptyBehaviour)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).Lookups.EmptyBehaviour_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).DefaultingStrategy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).DefaultValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)).DefaultValueFieldType)));
			fieldsGrid.CaptionVisible = false;
			fieldFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			fieldFindBoxColumnStyleInfo1.ColumnName = "FieldName";
			fieldFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo1.ColumnName = "FieldCaption";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo1.ColumnName = "Filter";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+EmptyBehaviour_List";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "EmptyBehaviour";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "DefaultingStrategy";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Services.OperationalActions.GUI.Res.GetData("FieldsTabPage|4cdefe58-f1f7-4f18-85e9-ba31a870e687", "Defaulting");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zMultiControlColumnStyleInfo1.ColumnName = "DefaultValue";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "DefaultValueFieldType";
			zMultiControlColumnStyleInfo1.GroupName = Enterprise.Services.OperationalActions.GUI.Res.GetData("FieldsTabPage|4cdefe58-f1f7-4f18-85e9-ba31a870e687", "Defaulting");
			fieldsGrid.ColumnStyles.Add(fieldFindBoxColumnStyleInfo1);
			fieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			fieldsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			fieldsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			fieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			fieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			fieldsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			fieldsGrid.GridId = "37c962e9-2701-4197-b0be-d7644cb6f269";
			fieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			fieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			fieldsGrid.LayoutKey = "zGrid3";
			fieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			fieldsGrid.Name = "fieldsGrid";
			fieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 170, true);
			fieldsGrid.TabIndex = 0;
			// 
			// fieldFilter
			// 
			this.BindingSource.SetBindingMember(fieldFilter, "FieldDescriptors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).FieldDescriptors)).SyncRoot)))));
			fieldFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			fieldFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			fieldFilter.Name = "fieldFilter";
			fieldFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 170, true);
			fieldFilter.TabIndex = 0;
			// 
			// FieldsTabPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(fieldSplitContainer);
			this.Name = "FieldsTabPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			fieldSplitContainer.Panel1.ResumeLayout(false);
			fieldSplitContainer.Panel2.ResumeLayout(false);
			fieldSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(fieldsGrid)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
