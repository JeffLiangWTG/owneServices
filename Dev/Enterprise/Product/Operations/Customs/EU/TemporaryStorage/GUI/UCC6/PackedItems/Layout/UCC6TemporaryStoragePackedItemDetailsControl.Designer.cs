namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePackedItemDetailsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SeqCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CusCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CountryOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SupplementaryUnitsCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsSecondQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.AdditionalSupplementaryCodesUserControl();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DutiesAndTaxesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DutiesAndTaxesLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.CusCodeFindBox.SuspendLayout();
			this.CountryOfOriginDropEdit.SuspendLayout();
			this.CustomsValueCalcDropEdit.SuspendLayout();
			this.SupplementaryUnitsCalcDropEdit.SuspendLayout();
			this.CustomsSecondQuantityDropEdit.SuspendLayout();
			this.CustomsThirdQuantityDropEdit.SuspendLayout();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutiesAndTaxesGrid)).BeginInit();
			this.DutiesAndTaxesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// SeqCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SeqCalcEdit, "Bills.PackedItems.API_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_LineNo)));
			this.SeqCalcEdit.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.GUI.Res.GetData("97973735-7C0A-4ACE-8018-37474F5F4F7B", "Seq.");
			this.SeqCalcEdit.DecimalPlaces = 2;
			this.SeqCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 100, true);
			this.SeqCalcEdit.Name = "SeqCalcEdit";
			this.SeqCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.SeqCalcEdit.TabIndex = 1;
			this.SeqCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "Bills.PackedItems.API_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 140, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_GrossWeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "Bills.PackedItems.API_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "Bills.PackedItems.API_GrossWeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 182, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 15, true);
			this.GrossWeightCalcDropEdit.TabIndex = 3;
			// 
			// CusCodeFindBox
			// 
			this.CusCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusCodeFindBox, "Bills.PackedItems.API_ChemicalSubstanceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_ChemicalSubstanceCode)));
			this.CusCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 262, true);
			this.CusCodeFindBox.Name = "CusCodeFindBox";
			this.CusCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusCodeFindBox.ParentType = null;
			this.CusCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CusCodeFindBox.TabIndex = 5;
			// 
			// CountryOfOriginDropEdit
			// 
			this.CountryOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginDropEdit, "Bills.PackedItems.API_RN_NKGoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_RN_NKGoodsOrigin)));
			this.CountryOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 374, true);
			this.CountryOfOriginDropEdit.Name = "CountryOfOriginDropEdit";
			this.CountryOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfOriginDropEdit.TabIndex = 13;
			// 
			// CustomsValueCalcDropEdit
			// 
			this.CustomsValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_RX_NKGoodsValueCurrency)));
			this.CustomsValueCalcDropEdit.BindToAmount = "Bills.PackedItems.API_GoodsValue";
			this.CustomsValueCalcDropEdit.BindToUnit = "Bills.PackedItems.API_RX_NKGoodsValueCurrency";
			this.CustomsValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 139, true);
			this.CustomsValueCalcDropEdit.Name = "CustomsValueCalcDropEdit";
			this.CustomsValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsValueCalcDropEdit.TabIndex = 18;
			this.CustomsValueCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// SupplementaryUnitsCalcDropEdit
			// 
			this.SupplementaryUnitsCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryUnitsCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsUQ)));
			this.SupplementaryUnitsCalcDropEdit.BindToAmount = "Bills.PackedItems.API_CustomsQty";
			this.SupplementaryUnitsCalcDropEdit.BindToUnit = "Bills.PackedItems.API_CustomsUQ";
			this.SupplementaryUnitsCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 139, true);
			this.SupplementaryUnitsCalcDropEdit.Name = "SupplementaryUnitsCalcDropEdit";
			this.SupplementaryUnitsCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SupplementaryUnitsCalcDropEdit.TabIndex = 4;
			this.SupplementaryUnitsCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsSecondQuantityDropEdit
			// 
			this.CustomsSecondQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsSecondQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsUQ2)));
			this.CustomsSecondQuantityDropEdit.BindToAmount = "Bills.PackedItems.API_CustomsQty2";
			this.CustomsSecondQuantityDropEdit.BindToUnit = "Bills.PackedItems.API_CustomsUQ2";
			this.CustomsSecondQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 139, true);
			this.CustomsSecondQuantityDropEdit.Name = "CustomsSecondQuantityDropEdit";
			this.CustomsSecondQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CustomsSecondQuantityDropEdit.TabIndex = 4;
			this.CustomsSecondQuantityDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsThirdQuantityDropEdit
			// 
			this.CustomsThirdQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsQty3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_CustomsUQ3)));
			this.CustomsThirdQuantityDropEdit.BindToAmount = "Bills.PackedItems.API_CustomsQty3";
			this.CustomsThirdQuantityDropEdit.BindToUnit = "Bills.PackedItems.API_CustomsUQ3";
			this.CustomsThirdQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 139, true);
			this.CustomsThirdQuantityDropEdit.Name = "CustomsThirdQuantityDropEdit";
			this.CustomsThirdQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CustomsThirdQuantityDropEdit.TabIndex = 4;
			this.CustomsThirdQuantityDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, "Bills.PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)))));
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 191, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 25;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).API_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "Bills.PackedItems.API_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "Bills.PackedItems.API_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 302, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 17, true);
			this.NetWeightCalcDropEdit.TabIndex = 26;
			// 
			// DutiesAndTaxesGrid
			// 
			this.DutiesAndTaxesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DutiesAndTaxesGrid, "Bills.PackedItems.DutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageDutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).AET_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageDutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).AET_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageDutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).AET_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageDutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).AET_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageDutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).AET_ChargeAmount)));
			this.DutiesAndTaxesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AET_ChargeType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo2.ColumnName = "AET_MethodOfCalculation";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AET_BaseValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AET_Rate";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AET_ChargeAmount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.DutiesAndTaxesGrid.GridId = "c3efafdd-cb67-4ea5-850c-0c10696b2d31";
			this.DutiesAndTaxesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DutiesAndTaxesGrid.LayoutKey = "DutiesAndTaxesGrid";
			this.DutiesAndTaxesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 281, true);
			this.DutiesAndTaxesGrid.Name = "DutiesAndTaxesGrid";
			this.DutiesAndTaxesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 114, true);
			this.DutiesAndTaxesGrid.TabIndex = 27;
			this.DutiesAndTaxesGrid.TabStop = false;
			// 
			// DutiesAndTaxesLabel
			// 
			this.DutiesAndTaxesLabel.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.GUI.Res.GetData("951d0934-95da-4871-bfe5-99896c4b0e9e", "Calculated Duty and Tax");
			this.DutiesAndTaxesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DutiesAndTaxesLabel.IsFontBold = true;
			this.DutiesAndTaxesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 262, true);
			this.DutiesAndTaxesLabel.Name = "DutiesAndTaxesLabel";
			this.DutiesAndTaxesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.DutiesAndTaxesLabel.TabIndex = 28;
			this.DutiesAndTaxesLabel.UseMnemonic = false;
			// 
			// UCC6TemporaryStoragePackedItemDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CusCodeFindBox);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.SeqCalcEdit);
			this.Controls.Add(this.CountryOfOriginDropEdit);
			this.Controls.Add(this.CustomsValueCalcDropEdit);
			this.Controls.Add(this.SupplementaryUnitsCalcDropEdit);
			this.Controls.Add(this.CustomsSecondQuantityDropEdit);
			this.Controls.Add(this.CustomsThirdQuantityDropEdit);
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.DutiesAndTaxesGrid);
			this.Controls.Add(this.DutiesAndTaxesLabel);
			this.Name = "UCC6TemporaryStoragePackedItemDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 381, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.CusCodeFindBox.ResumeLayout(true);
			this.CusCodeFindBox.PerformLayout();
			this.CountryOfOriginDropEdit.ResumeLayout(true);
			this.CountryOfOriginDropEdit.PerformLayout();
			this.CustomsValueCalcDropEdit.ResumeLayout(true);
			this.CustomsValueCalcDropEdit.PerformLayout();
			this.SupplementaryUnitsCalcDropEdit.ResumeLayout(true);
			this.SupplementaryUnitsCalcDropEdit.PerformLayout();
			this.CustomsSecondQuantityDropEdit.ResumeLayout(true);
			this.CustomsSecondQuantityDropEdit.PerformLayout();
			this.CustomsThirdQuantityDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityDropEdit.PerformLayout();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutiesAndTaxesGrid)).EndInit();
			this.DutiesAndTaxesGrid.ResumeLayout(false);
			this.DutiesAndTaxesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit SeqCalcEdit;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CusCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit CountryOfOriginDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsValueCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit SupplementaryUnitsCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsSecondQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityDropEdit;
		internal AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.ZGrid DutiesAndTaxesGrid;
		internal ZArchitecture.ZLabel DutiesAndTaxesLabel;
	}
}
