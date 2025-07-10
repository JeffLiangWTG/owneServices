namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ItemDetailsFieldsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomEntriesSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CustomEntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.IntrinsicValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.SupplementaryDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.CustomEntriesSeparatorUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomEntriesGrid)).BeginInit();
			this.CustomEntriesGrid.SuspendLayout();
			this.SupplementaryDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.IntrinsicValueConvertToLocalCurrencyControl.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaPackedItem);
			// 
			// QuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "API_CustomsQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(null)).API_CustomsQty)));
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 225, true);
			this.QuantityCalcEdit.Name = "QuantityCalcEdit";
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 18, true);
			this.QuantityCalcEdit.TabIndex = 8;
			// 
			// SupplementaryDropEdit
			//
			this.SupplementaryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryDropEdit, ".");
			this.SupplementaryDropEdit.BindToAmount = "API_CustomsQty2";
			this.SupplementaryDropEdit.BindToUnit = "API_CustomsUQ2";
			this.SupplementaryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 75, true);
			this.SupplementaryDropEdit.Name = "SupplementaryDropEdit";
			this.SupplementaryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.SupplementaryDropEdit.TabIndex = 2;
			this.SupplementaryDropEdit.UnitPreBoundMaxLength = 4;
			//
			// GrossWeightCalcDropEdit
			//
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			this.GrossWeightCalcDropEdit.BindToAmount = "API_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "API_GrossWeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 105, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 3;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 4;
			this.GrossWeightCalcDropEdit.Decimals = 3;
			//
			// NetWeightCalcDropEdit
			//
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			this.NetWeightCalcDropEdit.BindToAmount = "API_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "API_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 135, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 4;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 4;
			this.NetWeightCalcDropEdit.Decimals = 3;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "API_RN_NKGoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(null)).API_RN_NKGoodsOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 165, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 5;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.GoodsOriginCodeFindBox.TabIndex = 5;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "API_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(null)).API_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 195, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 18, true);
			this.GoodsDescriptionTextBox.TabIndex = 6;
			// 
			// CustomEntriesSeparatorUserControl
			// 
			this.CustomEntriesSeparatorUserControl.AllowDrop = true;
			this.CustomEntriesSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("b56b31a6-1cce-429f-8751-b0a78490371f", "Custom Entries");
			this.CustomEntriesSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 3, true);
			this.CustomEntriesSeparatorUserControl.Name = "CustomEntriesSeparatorUserControl";
			this.CustomEntriesSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CustomEntriesSeparatorUserControl.TabIndex = 12;
			// 
			// CustomEntriesGrid
			// 
			this.CustomEntriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomEntriesGrid, "CustomsEntryNumbers");
			this.CustomEntriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49);
			this.CustomEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomEntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomEntriesGrid.GridId = "5fd3280f-b530-4c11-8eb7-121d6c08a13a";
			this.CustomEntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomEntriesGrid.LayoutKey = "customEntriesGrid";
			this.CustomEntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 32, true);
			this.CustomEntriesGrid.Name = "CustomEntriesGrid";
			this.CustomEntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 131, true);
			this.CustomEntriesGrid.TabIndex = 13;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "API_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(null)).API_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 15, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.NeedLoadParentDataGroup = true;
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 8;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShouldResize = false;
			this.TariffFindBox.ShowDescriptionBox = false;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 18, true);
			this.TariffFindBox.TabIndex = 0;
			this.TariffFindBox.TariffType = null;
			// 
			// IntrinsicValueConvertToLocalCurrencyControl
			// 
			this.IntrinsicValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.IntrinsicValueConvertToLocalCurrencyControl.BindToAmount = "API_GoodsValue";
			this.IntrinsicValueConvertToLocalCurrencyControl.BindToUnit = "API_RX_NKGoodsValueCurrency";
			this.IntrinsicValueConvertToLocalCurrencyControl.Decimals = 2;
			this.IntrinsicValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.IntrinsicValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 45, true);
			this.IntrinsicValueConvertToLocalCurrencyControl.Name = "IntrinsicValueConvertToLocalCurrencyControl";
			this.IntrinsicValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.IntrinsicValueConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// EUH7ItemDetailsFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SupplementaryDropEdit);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.CustomEntriesSeparatorUserControl);
			this.Controls.Add(this.CustomEntriesGrid);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.IntrinsicValueConvertToLocalCurrencyControl);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.QuantityCalcEdit);
			this.Name = "EUH7ItemDetailsFieldsUserControl";
			this.TabIndex = 0;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplementaryDropEdit.ResumeLayout(true);
			this.SupplementaryDropEdit.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.CustomEntriesSeparatorUserControl.ResumeLayout(true);
			this.CustomEntriesSeparatorUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomEntriesGrid)).EndInit();
			this.CustomEntriesGrid.ResumeLayout(false);
			this.CustomEntriesGrid.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.IntrinsicValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.IntrinsicValueConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal Universal.GUI.TariffFindBox TariffFindBox;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl IntrinsicValueConvertToLocalCurrencyControl;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit SupplementaryDropEdit;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.GUI.SeparatorUserControl CustomEntriesSeparatorUserControl;
		internal ZArchitecture.ZGrid CustomEntriesGrid;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.ZCalcEdit QuantityCalcEdit;
	}
}
