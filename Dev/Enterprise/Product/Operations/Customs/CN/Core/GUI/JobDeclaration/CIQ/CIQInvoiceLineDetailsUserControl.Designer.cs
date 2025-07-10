namespace Enterprise.Customs.CN.GUI
{
	partial class CIQInvoiceLineDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.DangerousGoodsGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BatchesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufactureDatesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.JI_CIQQualityGuaranteePeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_CIQExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CIQIngredientTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.JI_CIQEndUseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_CIQCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CargoAttributesTextBox = new CodeDescriptionSelectionUserControl();
			this.JI_BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_NDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_OA_ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JI_PackageTypeOfUNDGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_NonDangerousChemicalFlagCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JI_OrigContainerFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SyncCIQDetailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DangerousGoodsGuidFindBox.SuspendLayout();
			this.BatchesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BatchNumbersGrid)).BeginInit();
			this.BatchNumbersGrid.SuspendLayout();
			this.JI_CIQExpiryDateEdit.SuspendLayout();
			this.CIQIngredientTextBox.SuspendLayout();
			this.JI_CIQEndUseDropEdit.SuspendLayout();
			this.JI_CIQCodeFindBox.SuspendLayout();
			this.JI_OA_ManufacturerAddressControl.SuspendLayout();
			this.JI_PackageTypeOfUNDGDropEdit.SuspendLayout();
			this.JI_OrigContainerFlagDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobComInvoiceLine);
			// 
			// DangerousGoodsGuidFindBox
			// 
			this.DangerousGoodsGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DangerousGoodsGuidFindBox, "DangerousGoodsDGSubs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).DangerousGoodsDGSubs)));
			this.DangerousGoodsGuidFindBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("f90b4760-2c33-4a7b-a802-4c6edab08613", "UNDG");
			this.DangerousGoodsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 196, true);
			this.DangerousGoodsGuidFindBox.Name = "DangerousGoodsGuidFindBox";
			this.DangerousGoodsGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DangerousGoodsGuidFindBox.ParentType = null;
			this.DangerousGoodsGuidFindBox.PreBoundMaxLength = 5;
			this.DangerousGoodsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.DangerousGoodsGuidFindBox.TabIndex = 11;
			// 
			// BatchesGroupBox
			// 
			this.BatchesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BatchesGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("5f161a1b-9699-4bf1-8625-edb474faa290", "Production Batches");
			this.BatchesGroupBox.Controls.Add(this.ManufactureDatesTextBox);
			this.BatchesGroupBox.Controls.Add(this.BatchNumbersTextBox);
			this.BatchesGroupBox.Controls.Add(this.BatchNumbersGrid);
			this.BatchesGroupBox.Controls.Add(this.JI_CIQQualityGuaranteePeriodCalcEdit);
			this.BatchesGroupBox.Controls.Add(this.JI_CIQExpiryDateEdit);
			this.BatchesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 5, true);
			this.BatchesGroupBox.Name = "BatchesGroupBox";
			this.BatchesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 240, true);
			this.BatchesGroupBox.TabIndex = 14;
			this.BatchesGroupBox.TabStop = false;
			// 
			// ManufactureDatesTextBox
			// 
			this.ManufactureDatesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ManufactureDatesTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ManufactureDatesTextBox, "ManufactureDatesAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).ManufactureDatesAsString)));
			this.ManufactureDatesTextBox.CaptionResourceString = null;
			this.ManufactureDatesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ManufactureDatesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 191, true);
			this.ManufactureDatesTextBox.Name = "ManufactureDatesTextBox";
			this.ManufactureDatesTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ManufactureDatesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ManufactureDatesTextBox.TabIndex = 2;
			// 
			// BatchNumbersTextBox
			// 
			this.BatchNumbersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BatchNumbersTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BatchNumbersTextBox, "BatchNumbersAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).BatchNumbersAsString)));
			this.BatchNumbersTextBox.CaptionResourceString = null;
			this.BatchNumbersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BatchNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 168, true);
			this.BatchNumbersTextBox.Name = "BatchNumbersTextBox";
			this.BatchNumbersTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.BatchNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.BatchNumbersTextBox.TabIndex = 1;
			// 
			// BatchNumbersGrid
			// 
			this.BatchNumbersGrid.AllowNavigation = false;
			this.BatchNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BatchNumbersGrid, "ProductionBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).ProductionBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.ProductionBatch)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).ProductionBatch)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CN.Business.ProductionBatch)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).ProductionBatch)).SyncRoot)).CY_Date)));
			this.BatchNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("E6329017-BCEB-48DC-B84D-E5E406F5B9D7", "Batch Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("BF49D38F-62CC-4B06-9A11-AE07B2930DD2", "Manufacture Date");
			zDateEditColumnStyleInfo1.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.BatchNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BatchNumbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BatchNumbersGrid.GridId = "68adc6af-468f-42e5-b495-fb263033f638";
			this.BatchNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BatchNumbersGrid.LayoutKey = "BatchNumbersGrid";
			this.BatchNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.BatchNumbersGrid.Name = "BatchNumbersGrid";
			this.BatchNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 143, true);
			this.BatchNumbersGrid.TabIndex = 0;
			// 
			// JI_CIQQualityGuaranteePeriodCalcEdit
			// 
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JI_CIQQualityGuaranteePeriodCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_CIQQualityGuaranteePeriodCalcEdit, "JI_CIQQualityGuaranteePeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_CIQQualityGuaranteePeriod)));
			this.JI_CIQQualityGuaranteePeriodCalcEdit.CaptionResourceString = null;
			this.JI_CIQQualityGuaranteePeriodCalcEdit.DecimalPlaces = 0;
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Decimals = 0;
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 214, true);
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Name = "JI_CIQQualityGuaranteePeriodCalcEdit";
			this.JI_CIQQualityGuaranteePeriodCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.JI_CIQQualityGuaranteePeriodCalcEdit.TabIndex = 4;
			this.JI_CIQQualityGuaranteePeriodCalcEdit.Text = "0";
			this.JI_CIQQualityGuaranteePeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_CIQExpiryDateEdit
			// 
			this.JI_CIQExpiryDateEdit.AllowDrop = true;
			this.JI_CIQExpiryDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JI_CIQExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_CIQExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_CIQExpiryDateEdit, "JI_CIQExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_CIQExpiryDate)));
			this.JI_CIQExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 214, true);
			this.JI_CIQExpiryDateEdit.Name = "JI_CIQExpiryDateEdit";
			this.JI_CIQExpiryDateEdit.TabIndex = 3;
			// 
			// CIQIngredientTextBox
			// 
			this.CIQIngredientTextBox.AllowDrop = true;
			this.CIQIngredientTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.CIQIngredientTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CIQIngredientTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 79, true);
			this.CIQIngredientTextBox.Name = "CIQIngredientTextBox";
			this.CIQIngredientTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 20, true);
			this.CIQIngredientTextBox.TabIndex = 6;
			// 
			// JI_CIQEndUseDropEdit
			// 
			this.JI_CIQEndUseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CIQEndUseDropEdit, "JI_CIQEndUse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_CIQEndUse)));
			this.JI_CIQEndUseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 32, true);
			this.JI_CIQEndUseDropEdit.Name = "JI_CIQEndUseDropEdit";
			this.JI_CIQEndUseDropEdit.PreBoundMaxLength = 3;
			this.JI_CIQEndUseDropEdit.ShouldResizeByMaxLength = true;
			this.JI_CIQEndUseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.JI_CIQEndUseDropEdit.TabIndex = 2;
			// 
			// JI_CIQCodeFindBox
			// 
			this.JI_CIQCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CIQCodeFindBox, "JI_CIQTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_CIQTariff)));
			this.JI_CIQCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 9, true);
			this.JI_CIQCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			this.JI_CIQCodeFindBox.Name = "JI_CIQCodeFindBox";
			this.JI_CIQCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_CIQCodeFindBox.ParentType = null;
			this.JI_CIQCodeFindBox.PreBoundMaxLength = 3;
			this.JI_CIQCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.JI_CIQCodeFindBox.TabIndex = 0;
			// 
			// CargoAttributesTextBox
			// 
			this.CargoAttributesTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.CargoAttributesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 55, true);
			this.CargoAttributesTextBox.Name = "CargoAttributesTextBox";
			this.CargoAttributesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 20, true);
			this.CargoAttributesTextBox.TabIndex = 4;
			// 
			// JI_BrandNameTextBox
			// 
			this.JI_BrandNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_BrandNameTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_BrandName)));
			this.JI_BrandNameTextBox.CaptionResourceString = null;
			this.JI_BrandNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JI_BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 149, true);
			this.JI_BrandNameTextBox.Name = "JI_BrandNameTextBox";
			this.JI_BrandNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JI_BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 20, true);
			this.JI_BrandNameTextBox.TabIndex = 9;
			// 
			// JI_ModelTextBox
			// 
			this.JI_ModelTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_ModelTextBox, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_Model)));
			this.JI_ModelTextBox.CaptionResourceString = null;
			this.JI_ModelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JI_ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 173, true);
			this.JI_ModelTextBox.Name = "JI_ModelTextBox";
			this.JI_ModelTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JI_ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 20, true);
			this.JI_ModelTextBox.TabIndex = 10;
			// 
			// JI_NDescriptionTextBox
			// 
			this.JI_NDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_NDescriptionTextBox, "JI_NDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_NDescription)));
			this.JI_NDescriptionTextBox.CaptionResourceString = null;
			this.JI_NDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JI_NDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 126, true);
			this.JI_NDescriptionTextBox.Name = "JI_NDescriptionTextBox";
			this.JI_NDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JI_NDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 20, true);
			this.JI_NDescriptionTextBox.TabIndex = 8;
			// 
			// JI_OA_ManufacturerAddressControl
			// 
			this.JI_OA_ManufacturerAddressControl.AllowDrop = true;
			this.JI_OA_ManufacturerAddressControl.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_OA_ManufacturerAddressControl, "JI_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_OA_ManufacturerAddress)));
			this.JI_OA_ManufacturerAddressControl.BindToOrgList = "Lookups.Manufacturers";
			this.JI_OA_ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 103, true);
			this.JI_OA_ManufacturerAddressControl.Name = "JI_OA_ManufacturerAddressControl";
			this.JI_OA_ManufacturerAddressControl.PopupCaption = "";
			this.JI_OA_ManufacturerAddressControl.ReadOnly = false;
			this.JI_OA_ManufacturerAddressControl.ShowAddress = false;
			this.JI_OA_ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.JI_OA_ManufacturerAddressControl.TabIndex = 7;
			// 
			// JI_PackageTypeOfUNDGDropEdit
			// 
			this.JI_PackageTypeOfUNDGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_PackageTypeOfUNDGDropEdit, "JI_PackageTypeOfUNDG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_PackageTypeOfUNDG)));
			this.JI_PackageTypeOfUNDGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 219, true);
			this.JI_PackageTypeOfUNDGDropEdit.Name = "JI_PackageTypeOfUNDGDropEdit";
			this.JI_PackageTypeOfUNDGDropEdit.PreBoundMaxLength = 3;
			this.JI_PackageTypeOfUNDGDropEdit.ShouldResizeByMaxLength = true;
			this.JI_PackageTypeOfUNDGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.JI_PackageTypeOfUNDGDropEdit.TabIndex = 13;
			// 
			// JI_NonDangerousChemicalFlagCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JI_NonDangerousChemicalFlagCheckBox, "JI_NonDangerousChemicalFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_NonDangerousChemicalFlag)));
			this.JI_NonDangerousChemicalFlagCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JI_NonDangerousChemicalFlagCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 196, true);
			this.JI_NonDangerousChemicalFlagCheckBox.Name = "JI_NonDangerousChemicalFlagCheckBox";
			this.JI_NonDangerousChemicalFlagCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.JI_NonDangerousChemicalFlagCheckBox.TabIndex = 12;
			// 
			// JI_OrigContainerFlagDropEdit
			// 
			this.JI_OrigContainerFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_OrigContainerFlagDropEdit, "JI_OrigContainerFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).JI_OrigContainerFlag)));
			this.JI_OrigContainerFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 32, true);
			this.JI_OrigContainerFlagDropEdit.Name = "JI_OrigContainerFlagDropEdit";
			this.JI_OrigContainerFlagDropEdit.PreBoundMaxLength = 3;
			this.JI_OrigContainerFlagDropEdit.ShouldResizeByMaxLength = true;
			this.JI_OrigContainerFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.JI_OrigContainerFlagDropEdit.TabIndex = 3;
			// 
			// SyncCIQDetailsButton
			// 
			this.SyncCIQDetailsButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("1cdf0db5-9d97-4157-a005-6323ac4b944c", "Sync with Add. Info.", "Synchronize CIQ Ingredient, Specification, Brand, Model, Manufacture Dates with Specification & Model (Additional Information)");
			this.SyncCIQDetailsButton.IsCaptionOverridden = false;
			this.SyncCIQDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 7, true);
			this.SyncCIQDetailsButton.Name = "SyncCIQDetailsButton";
			this.SyncCIQDetailsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SyncCIQDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.SyncCIQDetailsButton.TabIndex = 1;
			this.SyncCIQDetailsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SyncCIQDetailsButton.ToolTipCaption = null;
			this.SyncCIQDetailsButton.UseVisualStyleBackColor = true;
			this.SyncCIQDetailsButton.Click += new System.EventHandler(this.SyncCIQDetailsButton_Click);
			// 
			// CIQInvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SyncCIQDetailsButton);
			this.Controls.Add(this.DangerousGoodsGuidFindBox);
			this.Controls.Add(this.BatchesGroupBox);
			this.Controls.Add(this.CIQIngredientTextBox);
			this.Controls.Add(this.JI_PackageTypeOfUNDGDropEdit);
			this.Controls.Add(this.JI_OrigContainerFlagDropEdit);
			this.Controls.Add(this.JI_CIQEndUseDropEdit);
			this.Controls.Add(this.JI_CIQCodeFindBox);
			this.Controls.Add(this.CargoAttributesTextBox);
			this.Controls.Add(this.JI_NonDangerousChemicalFlagCheckBox);
			this.Controls.Add(this.JI_BrandNameTextBox);
			this.Controls.Add(this.JI_ModelTextBox);
			this.Controls.Add(this.JI_NDescriptionTextBox);
			this.Controls.Add(this.JI_OA_ManufacturerAddressControl);
			this.Name = "CIQInvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DangerousGoodsGuidFindBox.ResumeLayout(true);
			this.DangerousGoodsGuidFindBox.PerformLayout();
			this.BatchesGroupBox.ResumeLayout(false);
			this.BatchesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BatchNumbersGrid)).EndInit();
			this.BatchNumbersGrid.ResumeLayout(false);
			this.BatchNumbersGrid.PerformLayout();
			this.JI_CIQExpiryDateEdit.ResumeLayout(true);
			this.JI_CIQExpiryDateEdit.PerformLayout();
			this.CIQIngredientTextBox.ResumeLayout(true);
			this.CIQIngredientTextBox.PerformLayout();
			this.JI_CIQEndUseDropEdit.ResumeLayout(true);
			this.JI_CIQEndUseDropEdit.PerformLayout();
			this.JI_CIQCodeFindBox.ResumeLayout(true);
			this.JI_CIQCodeFindBox.PerformLayout();
			this.JI_OA_ManufacturerAddressControl.ResumeLayout(true);
			this.JI_OA_ManufacturerAddressControl.PerformLayout();
			this.JI_PackageTypeOfUNDGDropEdit.ResumeLayout(true);
			this.JI_PackageTypeOfUNDGDropEdit.PerformLayout();
			this.JI_OrigContainerFlagDropEdit.ResumeLayout(true);
			this.JI_OrigContainerFlagDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit JI_CIQExpiryDateEdit;
		private CodeDescriptionSelectionUserControl CargoAttributesTextBox;
		private ZArchitecture.GUI.ZDropEdit JI_CIQEndUseDropEdit;
		private ZArchitecture.ZTextBox JI_BrandNameTextBox;
		private ZArchitecture.ZTextBox JI_ModelTextBox;
		private ZArchitecture.ZTextBox JI_NDescriptionTextBox;
		private ZArchitecture.ZCalcEdit JI_CIQQualityGuaranteePeriodCalcEdit;
		private Customs.GUI.LongTextControl CIQIngredientTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JI_CIQCodeFindBox;
		private ZArchitecture.GUI.ZAddressControl JI_OA_ManufacturerAddressControl;
		private ZArchitecture.GUI.ZGroupBox BatchesGroupBox;
		private ZArchitecture.ZGrid BatchNumbersGrid;
		private ZArchitecture.GUI.ZGuidFindBox DangerousGoodsGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit JI_PackageTypeOfUNDGDropEdit;
		private ZArchitecture.GUI.ZCheckBox JI_NonDangerousChemicalFlagCheckBox;
		private ZArchitecture.GUI.ZDropEdit JI_OrigContainerFlagDropEdit;
		internal ZArchitecture.GUI.ZButton SyncCIQDetailsButton;
		private ZArchitecture.ZTextBox ManufactureDatesTextBox;
		private ZArchitecture.ZTextBox BatchNumbersTextBox;
	}
}
