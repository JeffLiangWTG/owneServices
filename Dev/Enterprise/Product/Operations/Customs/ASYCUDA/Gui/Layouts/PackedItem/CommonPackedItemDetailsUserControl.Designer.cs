namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class CommonPackedItemDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.CustomsQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsQty2CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsQty3CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GoodsValueLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.CustomEntriesSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CustomEntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffFindBox.SuspendLayout();
			this.CustomsQtyCalcDropEdit.SuspendLayout();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.CustomsQty2CalcDropEdit.SuspendLayout();
			this.CustomsQty3CalcDropEdit.SuspendLayout();
			this.GoodsValueLocalCurrencyControl.SuspendLayout();
			this.CustomEntriesSeparatorUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomEntriesGrid)).BeginInit();
			this.CustomEntriesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPack);
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "PackedItem.API_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 3, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 8;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShouldResize = false;
			this.TariffFindBox.ShowDescriptionBox = false;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.TariffFindBox.TabIndex = 0;
			this.TariffFindBox.TariffType = null;
			// 
			// CustomsQtyCalcDropEdit
			// 
			this.CustomsQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsUQ)));
			this.CustomsQtyCalcDropEdit.BindToAmount = "PackedItem.API_CustomsQty";
			this.CustomsQtyCalcDropEdit.BindToUnit = "PackedItem.API_CustomsUQ";
			this.CustomsQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 23, true);
			this.CustomsQtyCalcDropEdit.Name = "CustomsQtyCalcDropEdit";
			this.CustomsQtyCalcDropEdit.ShowDescriptionBox = true;
			this.CustomsQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.CustomsQtyCalcDropEdit.TabIndex = 1;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "PackedItem.API_RN_NKGoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_RN_NKGoodsOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 83, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.GoodsOriginCodeFindBox.TabIndex = 4;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "PackedItem.API_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = null;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 103, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 3;
			// 
			// CustomsValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsValueCalcEdit, "PackedItem.API_CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsValue)));
			this.CustomsValueCalcEdit.CaptionResourceString = null;
			this.CustomsValueCalcEdit.DecimalPlaces = 2;
			this.CustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 43, true);
			this.CustomsValueCalcEdit.Name = "CustomsValueCalcEdit";
			this.CustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.CustomsValueCalcEdit.TabIndex = 8;
			this.CustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxAmountCalcEdit, "PackedItem.API_TaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_TaxAmount)));
			this.TaxAmountCalcEdit.CaptionResourceString = null;
			this.TaxAmountCalcEdit.DecimalPlaces = 2;
			this.TaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 123, true);
			this.TaxAmountCalcEdit.Name = "TaxAmountCalcEdit";
			this.TaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.TaxAmountCalcEdit.TabIndex = 9;
			this.TaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyAmountCalcEdit, "PackedItem.API_DutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_DutyAmount)));
			this.DutyAmountCalcEdit.CaptionResourceString = null;
			this.DutyAmountCalcEdit.DecimalPlaces = 2;
			this.DutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 143, true);
			this.DutyAmountCalcEdit.Name = "DutyAmountCalcEdit";
			this.DutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.DutyAmountCalcEdit.TabIndex = 10;
			this.DutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "PackedItem.API_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_MessageStatus)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 163, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.MessageStatusTextBox.TabIndex = 5;
			// 
			// PackStatusTextBox
			// 
			this.PackStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackStatusTextBox, "PackedItem.API_PackStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_PackStatus)));
			this.PackStatusTextBox.CaptionResourceString = null;
			this.PackStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 263, true);
			this.PackStatusTextBox.Name = "PackStatusTextBox";
			this.PackStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.PackStatusTextBox.TabIndex = 7;
			// 
			// BrandTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandTextBox, "PackedItem.API_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_Brand)));
			this.BrandTextBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("453f2a9c-0cae-4e32-8442-a71d047b4db1", "Brand");
			this.BrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 223, true);
			this.BrandTextBox.Name = "BrandTextBox";
			this.BrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.BrandTextBox.TabIndex = 4;
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "PackedItem.API_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_Model)));
			this.ModelTextBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("9165C317-2569-4DEF-A55A-0F226F7B489A", "Model");
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 243, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ModelTextBox.TabIndex = 4;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_GrossWeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "PackedItem.API_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "PackedItem.API_GrossWeightUQ";
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("7345ee14-dc13-4569-9b4b-ecdbd9bea057", "Gross Weight");
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 183, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.ShowDescriptionBox = true;
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 10;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "PackedItem.API_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "PackedItem.API_NetWeightUQ";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("3223c8e0-b9c5-4dbf-b641-b9bec6385265", "Net Weight");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 203, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.ShowDescriptionBox = true;
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 10;
			// 
			// CustomsQty2CalcDropEdit
			// 
			this.CustomsQty2CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQty2CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsUQ2)));
			this.CustomsQty2CalcDropEdit.BindToAmount = "PackedItem.API_CustomsQty2";
			this.CustomsQty2CalcDropEdit.BindToUnit = "PackedItem.API_CustomsUQ2";
			this.CustomsQty2CalcDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1140da2a-bde8-4fda-9757-465e5e179a2b", "Supp. Qty. 1", "Supplementary Quantity 1", "");
			this.CustomsQty2CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 283, true);
			this.CustomsQty2CalcDropEdit.Name = "CustomsQty2CalcDropEdit";
			this.CustomsQty2CalcDropEdit.ShowDescriptionBox = true;
			this.CustomsQty2CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.CustomsQty2CalcDropEdit.TabIndex = 7;
			// 
			// CustomsQty3CalcDropEdit
			// 
			this.CustomsQty3CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQty3CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsQty3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.API_CustomsUQ3)));
			this.CustomsQty3CalcDropEdit.BindToAmount = "PackedItem.API_CustomsQty3";
			this.CustomsQty3CalcDropEdit.BindToUnit = "PackedItem.API_CustomsUQ3";
			this.CustomsQty3CalcDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("aa20408a-5945-4492-b945-6f79acea7927", "Supp. Qty. 2", "Supplementary Quantity 2", "");
			this.CustomsQty3CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 303, true);
			this.CustomsQty3CalcDropEdit.Name = "CustomsQty3CalcDropEdit";
			this.CustomsQty3CalcDropEdit.ShowDescriptionBox = true;
			this.CustomsQty3CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.CustomsQty3CalcDropEdit.TabIndex = 7;
			// 
			// GoodsValueLocalCurrencyControl
			// 
			this.GoodsValueLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueLocalCurrencyControl.BindToAmount = "PackedItem.API_GoodsValue";
			this.GoodsValueLocalCurrencyControl.BindToUnit = "PackedItem.API_RX_NKGoodsValueCurrency";
			this.GoodsValueLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("7598d99a-29e5-44f5-9151-b3b3a031d1f5", "Goods Value");
			this.GoodsValueLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 63, true);
			this.GoodsValueLocalCurrencyControl.Name = "GoodsValueLocalCurrencyControl";
			this.GoodsValueLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GoodsValueLocalCurrencyControl.TabIndex = 19;
			// 
			// CustomEntriesSeparatorUserControl
			// 
			this.CustomEntriesSeparatorUserControl.AllowDrop = true;
			this.CustomEntriesSeparatorUserControl.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("79BF5D90-D3A1-4948-A691-DE63D548E8B6", "Custom Entries");
			this.CustomEntriesSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 3, true);
			this.CustomEntriesSeparatorUserControl.Name = "CustomEntriesSeparatorUserControl";
			this.CustomEntriesSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CustomEntriesSeparatorUserControl.TabIndex = 12;
			// 
			// CustomEntriesGrid
			// 
			this.CustomEntriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomEntriesGrid, "PackedItem.CustomsEntryNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.CustomsEntryNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItemEntryNum)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.CustomsEntryNumbers)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItemEntryNum)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItem.CustomsEntryNumbers)).SyncRoot)).CE_EntryType)));
			this.CustomEntriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49);
			this.CustomEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomEntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomEntriesGrid.GridId = "F3AA8AC3-FC61-46EC-BF3B-16730FC1148C";
			this.CustomEntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomEntriesGrid.LayoutKey = "customEntriesGrid";
			this.CustomEntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 32, true);
			this.CustomEntriesGrid.Name = "CustomEntriesGrid";
			this.CustomEntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 131, true);
			this.CustomEntriesGrid.TabIndex = 1;
			// 
			// CommonPackedItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.CustomsQtyCalcDropEdit);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.CustomsValueCalcEdit);
			this.Controls.Add(this.TaxAmountCalcEdit);
			this.Controls.Add(this.DutyAmountCalcEdit);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Controls.Add(this.PackStatusTextBox);
			this.Controls.Add(this.CustomEntriesSeparatorUserControl);
			this.Controls.Add(this.CustomEntriesGrid);
			this.Controls.Add(this.BrandTextBox);
			this.Controls.Add(this.ModelTextBox);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.CustomsQty2CalcDropEdit);
			this.Controls.Add(this.CustomsQty3CalcDropEdit);
			this.Controls.Add(this.GoodsValueLocalCurrencyControl);
			this.Name = "CommonPackedItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 345, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.CustomsQtyCalcDropEdit.ResumeLayout(true);
			this.CustomsQtyCalcDropEdit.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.CustomsQty2CalcDropEdit.ResumeLayout(true);
			this.CustomsQty2CalcDropEdit.PerformLayout();
			this.CustomsQty3CalcDropEdit.ResumeLayout(true);
			this.CustomsQty3CalcDropEdit.PerformLayout();
			this.GoodsValueLocalCurrencyControl.ResumeLayout(true);
			this.GoodsValueLocalCurrencyControl.PerformLayout();
			this.CustomEntriesSeparatorUserControl.ResumeLayout(true);
			this.CustomEntriesSeparatorUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomEntriesGrid)).EndInit();
			this.CustomEntriesGrid.ResumeLayout(false);
			this.CustomEntriesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Universal.GUI.TariffFindBox TariffFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsQtyCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.ZCalcEdit CustomsValueCalcEdit;
		internal ZArchitecture.ZCalcEdit TaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit DutyAmountCalcEdit;
		internal ZArchitecture.ZTextBox MessageStatusTextBox;
		internal ZArchitecture.ZTextBox PackStatusTextBox;
		internal ZArchitecture.GUI.SeparatorUserControl CustomEntriesSeparatorUserControl;
		internal ZArchitecture.ZGrid CustomEntriesGrid;
		internal Enterprise.ZArchitecture.ZTextBox BrandTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ModelTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsQty2CalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsQty3CalcDropEdit;
		internal Customs.GUI.ConvertToLocalCurrencyControl GoodsValueLocalCurrencyControl;
	}
}
