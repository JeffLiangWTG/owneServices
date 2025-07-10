namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	partial class CusTempStorageDecUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Disposing
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.HeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.GuaranteeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SJH_CPH_GuaranteeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.SJH_IsSameConditionExpectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.SJH_IsExaminationExpectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SJH_TempStorageEndDateUtcDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CreatedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.LinesBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ClassificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TSL_CustomsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.UnionStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DestinationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.PackageQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
            this.LinesGrid.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            this.SJH_CPH_GuaranteeGuidFindBox.SuspendLayout();
            this.SJH_TempStorageEndDateUtcDateEdit.SuspendLayout();
            this.CreatedDateEdit.SuspendLayout();
            this.DetailsPanel.SuspendLayout();
            this.DetailsTabControl.SuspendLayout();
            this.LinesTabPage.SuspendLayout();
            this.LinesBottomPanel.SuspendLayout();
            this.ClassificationGroupBox.SuspendLayout();
            this.OwnerReferenceTypeDropEdit.SuspendLayout();
            this.UnionStatusDropEdit.SuspendLayout();
            this.ItemDetailsGroupBox.SuspendLayout();
            this.GoodsTypeDropEdit.SuspendLayout();
            this.GrossWeightCalcDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader);
            // 
            // LinesGrid
            // 
            this.LinesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageDec.CusTempStorageLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GrossWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_PackageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_UnionStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_DestinationPlace)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumberLine)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumberType)));
            this.LinesGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
            zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsCustomColumn = false;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "TSL_GoodsDescription";
			zTextBoxColumnStyleInfo3.IsCustomColumn = false;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "TSL_GrossWeight";
            zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.IsCustomColumn = false;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "TSL_PackageQty";
            zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsCustomColumn = false;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("33EFE669-CF0E-4C83-9AB5-715DEF315DB3", "Package Type");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "TSL_PackageType";
			zDropEditColumnStyleInfo2.IsCustomColumn = false;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2923EF2C-F6A7-4F44-9099-EABB5BF6797A", "Union Status");
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "TSL_UnionStatus";
			zDropEditColumnStyleInfo3.IsCustomColumn = false;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo4.ColumnName = "TSL_GoodsType";
			zDropEditColumnStyleInfo4.IsCustomColumn = false;
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo4.ColumnName = "TSL_LocationOfGoods";
			zTextBoxColumnStyleInfo4.IsCustomColumn = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2B8B1053-2EF0-4AC4-BBC3-7C5FCFA06346", "Destination Place");
            zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo5.ColumnName = "TSL_DestinationPlace";
			zTextBoxColumnStyleInfo5.IsCustomColumn = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo6.ColumnName = "TSL_ReferenceNumber";
			zTextBoxColumnStyleInfo6.IsCustomColumn = false;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "TSL_ReferenceNumberLine";
            zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.IsCustomColumn = false;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("DF74E4CB-6204-4502-AF7F-F0E57E9A5899", "Customs Reference");
            zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo7.ColumnName = "TSL_ReferenceNumberType";
			zTextBoxColumnStyleInfo7.IsCustomColumn = false;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinesGrid.GridId = "D03269E4-76ED-41ED-A914-C904237ABB53";
            this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LinesGrid.LayoutKey = "LinesGrid";
            this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LinesGrid.Name = "LinesGrid";
            this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 408, true);
            this.LinesGrid.TabIndex = 0;
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.Controls.Add(this.GuaranteeDescriptionTextBox);
            this.HeaderPanel.Controls.Add(this.SJH_CPH_GuaranteeGuidFindBox);
            this.HeaderPanel.Controls.Add(this.SJH_IsSameConditionExpectedCheckBox);
            this.HeaderPanel.Controls.Add(this.SJH_IsExaminationExpectedCheckBox);
            this.HeaderPanel.Controls.Add(this.StatusTextBox);
            this.HeaderPanel.Controls.Add(this.SJH_TempStorageEndDateUtcDateEdit);
            this.HeaderPanel.Controls.Add(this.CreatedDateEdit);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 89, true);
            this.HeaderPanel.TabIndex = 0;
            // 
            // GuaranteeDescriptionTextBox
            // 
            this.GuaranteeDescriptionTextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GuaranteeDescriptionTextBox, "SJH_GuaranteeDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_GuaranteeDescription)));
            this.GuaranteeDescriptionTextBox.CaptionResourceString = null;
            this.GuaranteeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 63, true);
            this.GuaranteeDescriptionTextBox.Name = "GuaranteeDescriptionTextBox";
            this.GuaranteeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 15, true);
            this.GuaranteeDescriptionTextBox.TabIndex = 6;
            // 
            // SJH_CPH_GuaranteeGuidFindBox
            // 
            this.SJH_CPH_GuaranteeGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SJH_CPH_GuaranteeGuidFindBox, "SJH_CPH_Guarantee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_CPH_Guarantee)));
            this.SJH_CPH_GuaranteeGuidFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("EF61896E-96B2-4196-9E68-3ADA416F7B9C", "Guarantee");
            this.SJH_CPH_GuaranteeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 63, true);
            this.SJH_CPH_GuaranteeGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
            this.SJH_CPH_GuaranteeGuidFindBox.Name = "SJH_CPH_GuaranteeGuidFindBox";
            this.SJH_CPH_GuaranteeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SJH_CPH_GuaranteeGuidFindBox.PreBoundMaxLength = 35;
            this.SJH_CPH_GuaranteeGuidFindBox.ShowDescriptionBox = false;
            this.SJH_CPH_GuaranteeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 15, true);
            this.SJH_CPH_GuaranteeGuidFindBox.TabIndex = 5;
            // 
            // SJH_IsSameConditionExpectedCheckBox
            // 
            this.SJH_IsSameConditionExpectedCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.SJH_IsSameConditionExpectedCheckBox, "SJH_IsSameConditionExpected");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_IsSameConditionExpected)));
            this.SJH_IsSameConditionExpectedCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("6BE152F7-49B5-4DA6-9FF3-D87DC21E2AA4", "Same Condition Expected");
            this.SJH_IsSameConditionExpectedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.SJH_IsSameConditionExpectedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SJH_IsSameConditionExpectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 35, true);
            this.SJH_IsSameConditionExpectedCheckBox.Name = "SJH_IsSameConditionExpectedCheckBox";
            this.SJH_IsSameConditionExpectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
            this.SJH_IsSameConditionExpectedCheckBox.TabIndex = 4;
            this.SJH_IsSameConditionExpectedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SJH_IsSameConditionExpectedCheckBox.UseVisualStyleBackColor = true;
            // 
            // SJH_IsExaminationExpectedCheckBox
            // 
            this.SJH_IsExaminationExpectedCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.SJH_IsExaminationExpectedCheckBox, "SJH_IsExaminationExpected");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_IsExaminationExpected)));
            this.SJH_IsExaminationExpectedCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("4DCA551A-B415-4FD3-9562-90DC078D4D67", "Examination and/or Sampling");
            this.SJH_IsExaminationExpectedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.SJH_IsExaminationExpectedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SJH_IsExaminationExpectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 9, true);
            this.SJH_IsExaminationExpectedCheckBox.Name = "SJH_IsExaminationExpectedCheckBox";
            this.SJH_IsExaminationExpectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
            this.SJH_IsExaminationExpectedCheckBox.TabIndex = 1;
            this.SJH_IsExaminationExpectedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SJH_IsExaminationExpectedCheckBox.UseVisualStyleBackColor = true;
            // 
            // StatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.StatusTextBox, "CusTempStorageDec.STH_MessageStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.STH_MessageStatus)));
            this.StatusTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("AFCF6D23-9DD0-42B1-B796-110A9663C725", "Message Status");
            this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 7, true);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
            this.StatusTextBox.TabIndex = 0;
            this.StatusTextBox.TabStop = false;
            // 
            // SJH_TempStorageEndDateUtcDateEdit
            // 
            this.SJH_TempStorageEndDateUtcDateEdit.AllowDrop = true;
            this.SJH_TempStorageEndDateUtcDateEdit.AutoCompleteMonthThreshold = 1;
            this.SJH_TempStorageEndDateUtcDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.SJH_TempStorageEndDateUtcDateEdit, "SJH_TempStorageEndDateUtc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TempStorageEndDateUtc)));
            this.SJH_TempStorageEndDateUtcDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            this.SJH_TempStorageEndDateUtcDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 35, true);
            this.SJH_TempStorageEndDateUtcDateEdit.Name = "SJH_TempStorageEndDateUtcDateEdit";
            this.SJH_TempStorageEndDateUtcDateEdit.TabIndex = 3;
            this.SJH_TempStorageEndDateUtcDateEdit.TabStop = false;
            // 
            // CreatedDateEdit
            // 
            this.CreatedDateEdit.AllowDrop = true;
            this.CreatedDateEdit.AutoCompleteMonthThreshold = 1;
            this.CreatedDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.CreatedDateEdit, "CusTempStorageDec.STH_SystemCreateTimeUtc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.STH_SystemCreateTimeUtc)));
            this.CreatedDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("61DA59DD-05E5-4150-B923-0E186E96343E", "Created");
            this.CreatedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            this.CreatedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 35, true);
            this.CreatedDateEdit.Name = "CreatedDateEdit";
            this.CreatedDateEdit.TabIndex = 2;
            this.CreatedDateEdit.TabStop = false;
            // 
            // DetailsPanel
            // 
            this.DetailsPanel.Controls.Add(this.DetailsTabControl);
            this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
            this.DetailsPanel.Name = "DetailsPanel";
            this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 560, true);
            this.DetailsPanel.TabIndex = 1;
            // 
            // DetailsTabControl
            // 
            this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.DetailsTabControl.Controls.Add(this.LinesTabPage);
            this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsTabControl.Name = "DetailsTabControl";
            this.DetailsTabControl.SelectedIndex = 0;
            this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 560, true);
            this.DetailsTabControl.TabIndex = 0;
            // 
            // LinesTabPage
            // 
            this.LinesTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("9D9E2E07-C789-46DD-B426-69946B7B879F", "Line Details");
            this.LinesTabPage.Controls.Add(this.LinesGrid);
            this.LinesTabPage.Controls.Add(this.LinesBottomPanel);
            this.LinesTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.LinesTabPage.Name = "LinesTabPage";
            this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 536, true);
            this.LinesTabPage.TabIndex = 0;
            // 
            // LinesBottomPanel
            // 
            this.LinesBottomPanel.Controls.Add(this.ClassificationGroupBox);
            this.LinesBottomPanel.Controls.Add(this.ItemDetailsGroupBox);
            this.LinesBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LinesBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 408, true);
            this.LinesBottomPanel.Name = "LinesBottomPanel";
            this.LinesBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 128, true);
            this.LinesBottomPanel.TabIndex = 2;
            // 
            // ClassificationGroupBox
            // 
            this.ClassificationGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("205731EA-96D2-4E4B-B10B-D7CD622F57F1", "Classification");
            this.ClassificationGroupBox.Controls.Add(this.TSL_CustomsStatusTextBox);
            this.ClassificationGroupBox.Controls.Add(this.OwnerReferenceNumberTextBox);
            this.ClassificationGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
            this.ClassificationGroupBox.Controls.Add(this.UnionStatusDropEdit);
            this.ClassificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ClassificationGroupBox.Name = "ClassificationGroupBox";
            this.ClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 124, true);
            this.ClassificationGroupBox.TabIndex = 0;
            this.ClassificationGroupBox.TabStop = false;
            // 
            // TSL_CustomsStatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.TSL_CustomsStatusTextBox, "CusTempStorageDec.CusTempStorageLines.TSL_CustomsStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
            this.TSL_CustomsStatusTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("174ed421-4532-4f2c-8f2c-f7dacd24756c", "Customs Status");
            this.TSL_CustomsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 96, true);
            this.TSL_CustomsStatusTextBox.Name = "TSL_CustomsStatusTextBox";
			this.TSL_CustomsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
            this.TSL_CustomsStatusTextBox.TabIndex = 3;
            // 
            // OwnerReferenceNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "CusTempStorageDec.CusTempStorageLines.TSL_OwnerReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
            this.OwnerReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("9F879A78-0383-4B59-8976-A4C0769465C0", "Ref. Num.", "Owner Ref. Num.", "Owner Reference Number", "");
            this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 44, true);
            this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
			this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
            this.OwnerReferenceNumberTextBox.TabIndex = 1;
            // 
            // OwnerReferenceTypeDropEdit
            // 
            this.OwnerReferenceTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CusTempStorageDec.CusTempStorageLines.TSL_OwnerReferenceType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
            this.OwnerReferenceTypeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E86A8836-6BD4-47A1-8948-DCE9CBD5CBA8", "Ref. Type", "Owner Ref. Type", "Owner Reference Type", "");
            this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 18, true);
            this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
            this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
            this.OwnerReferenceTypeDropEdit.TabIndex = 0;
            // 
            // UnionStatusDropEdit
            // 
            this.UnionStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnionStatusDropEdit, "CusTempStorageDec.CusTempStorageLines.TSL_UnionStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_UnionStatus)));
            this.UnionStatusDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2923EF2C-F6A7-4F44-9099-EABB5BF6797A", "Union Status");
            this.UnionStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 70, true);
            this.UnionStatusDropEdit.Name = "UnionStatusDropEdit";
            this.UnionStatusDropEdit.ShouldResizeByMaxLength = true;
			this.UnionStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
            this.UnionStatusDropEdit.TabIndex = 2;
            // 
            // ItemDetailsGroupBox
            // 
            this.ItemDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("86199E03-B109-4085-92D6-4DE128B280B5", "Item Details");
            this.ItemDetailsGroupBox.Controls.Add(this.DestinationPlaceTextBox);
            this.ItemDetailsGroupBox.Controls.Add(this.GoodsLocationTextBox);
            this.ItemDetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
            this.ItemDetailsGroupBox.Controls.Add(this.GrossWeightCalcDropEdit);
            this.ItemDetailsGroupBox.Controls.Add(this.PackageQuantityCalcEdit);
            this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 0, true);
            this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
            this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 124, true);
            this.ItemDetailsGroupBox.TabIndex = 1;
            this.ItemDetailsGroupBox.TabStop = false;
            // 
            // DestinationPlaceTextBox
            // 
            this.BindingSource.SetBindingMember(this.DestinationPlaceTextBox, "CusTempStorageDec.CusTempStorageLines.TSL_DestinationPlace");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_DestinationPlace)));
            this.DestinationPlaceTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2B8B1053-2EF0-4AC4-BBC3-7C5FCFA06346", "Destination Place");
            this.DestinationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 70, true);
            this.DestinationPlaceTextBox.Name = "DestinationPlaceTextBox";
			this.DestinationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
            this.DestinationPlaceTextBox.TabIndex = 2;
            // 
            // GoodsLocationTextBox
            // 
            this.GoodsLocationTextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsLocationTextBox, "CusTempStorageDec.CusTempStorageLines.TSL_LocationOfGoods");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
            this.GoodsLocationTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("155B816F-FF04-4AF6-86A0-1A99C4471012", "Goods Location");
            this.GoodsLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 44, true);
            this.GoodsLocationTextBox.Name = "GoodsLocationTextBox";
			this.GoodsLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
            this.GoodsLocationTextBox.TabIndex = 1;
            // 
            // GoodsTypeDropEdit
            // 
            this.GoodsTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "CusTempStorageDec.CusTempStorageLines.TSL_GoodsType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsType)));
            this.GoodsTypeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("6A20A16E-795A-44E6-8378-E4A28EB78549", "Goods Type");
            this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 18, true);
            this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
            this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
            this.GoodsTypeDropEdit.TabIndex = 0;
            // 
            // GrossWeightCalcDropEdit
            // 
            this.GrossWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GrossWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GrossWeightUQ)));
            this.GrossWeightCalcDropEdit.BindToAmount = "CusTempStorageDec.CusTempStorageLines.TSL_GrossWeight";
            this.GrossWeightCalcDropEdit.BindToUnit = "CusTempStorageDec.CusTempStorageLines.TSL_GrossWeightUQ";
            this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("17423243-9C8D-4855-98E0-5EA26C548DC1", "Gross Weight");
            this.GrossWeightCalcDropEdit.Decimals = 3;
            this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 96, true);
            this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
            this.GrossWeightCalcDropEdit.TabIndex = 3;
            this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
            // 
            // PackageQuantityCalcEdit
            // 
            this.PackageQuantityCalcEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PackageQuantityCalcEdit, "CusTempStorageDec.CusTempStorageLines.TSL_PackageQty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
            this.PackageQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1FD8823C-E993-4141-9671-415FBBAA6DFC", "Package Qty");
            this.PackageQuantityCalcEdit.DecimalPlaces = 0;
            this.PackageQuantityCalcEdit.Decimals = 0;
            this.PackageQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 96, true);
            this.PackageQuantityCalcEdit.Name = "PackageQuantityCalcEdit";
			this.PackageQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
            this.PackageQuantityCalcEdit.TabIndex = 4;
            this.PackageQuantityCalcEdit.Text = "0";
            this.PackageQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CusTempStorageDecUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DetailsPanel);
            this.Controls.Add(this.HeaderPanel);
            this.Name = "CusTempStorageDecUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
            this.LinesGrid.ResumeLayout(false);
            this.LinesGrid.PerformLayout();
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.SJH_CPH_GuaranteeGuidFindBox.ResumeLayout(true);
            this.SJH_CPH_GuaranteeGuidFindBox.PerformLayout();
            this.SJH_TempStorageEndDateUtcDateEdit.ResumeLayout(true);
            this.SJH_TempStorageEndDateUtcDateEdit.PerformLayout();
            this.CreatedDateEdit.ResumeLayout(true);
            this.CreatedDateEdit.PerformLayout();
            this.DetailsPanel.ResumeLayout(false);
            this.DetailsPanel.PerformLayout();
            this.DetailsTabControl.ResumeLayout(false);
            this.DetailsTabControl.PerformLayout();
            this.LinesTabPage.ResumeLayout(false);
            this.LinesTabPage.PerformLayout();
            this.LinesBottomPanel.ResumeLayout(false);
            this.LinesBottomPanel.PerformLayout();
            this.ClassificationGroupBox.ResumeLayout(false);
            this.ClassificationGroupBox.PerformLayout();
            this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
            this.OwnerReferenceTypeDropEdit.PerformLayout();
            this.UnionStatusDropEdit.ResumeLayout(true);
            this.UnionStatusDropEdit.PerformLayout();
            this.ItemDetailsGroupBox.ResumeLayout(false);
            this.ItemDetailsGroupBox.PerformLayout();
            this.GoodsTypeDropEdit.ResumeLayout(true);
            this.GoodsTypeDropEdit.PerformLayout();
            this.GrossWeightCalcDropEdit.ResumeLayout(true);
            this.GrossWeightCalcDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZPanel HeaderPanel;
		private ZArchitecture.GUI.ZPanel DetailsPanel;
		protected ZArchitecture.GUI.ZTabControl DetailsTabControl;
		protected ZArchitecture.GUI.ZTabPage LinesTabPage;
		protected ZArchitecture.ZGrid LinesGrid;
		protected ZArchitecture.GUI.ZPanel LinesBottomPanel;
		protected ZArchitecture.GUI.ZGroupBox ClassificationGroupBox;
		private ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth OwnerReferenceTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit UnionStatusDropEdit;
		protected ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		private ZArchitecture.ZTextBox DestinationPlaceTextBox;
		protected ZArchitecture.ZTextBox GoodsLocationTextBox;
		private ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PackageQuantityCalcEdit;
		private ZArchitecture.ZTextBox TSL_CustomsStatusTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.GUI.ZDateEdit CreatedDateEdit;
		private ZArchitecture.GUI.ZDateEdit SJH_TempStorageEndDateUtcDateEdit;
		private ZArchitecture.GUI.ZCheckBox SJH_IsExaminationExpectedCheckBox;
		private ZArchitecture.GUI.ZCheckBox SJH_IsSameConditionExpectedCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox SJH_CPH_GuaranteeGuidFindBox;
		private ZArchitecture.ZTextBox GuaranteeDescriptionTextBox;
	}
}

