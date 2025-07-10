using Enterprise.Customs.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ImportInvoiceLineDetailsUserControl
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
			this.ProductCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LotNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.GoodsDescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.BrandCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelTradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IngredientLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.CustomsQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DrawBackQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsUnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PriceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.DutyRateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MinMaxDutyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreferenceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyReductionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InstalmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SpecificUsePermitNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SpecificUseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DomesticTaxCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DomesticTaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DomesticTaxRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DomesticTaxExemptionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DomesticTaxBaseQtyOrPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VATTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VATReductionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EducationTaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AgricultureTaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyReductionRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsSecondQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsFourthQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InstallationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProductCodeFindBox.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.GoodsDescriptionLongTextControl.SuspendLayout();
			this.BrandCodeFindBox.SuspendLayout();
			this.IngredientLongTextControl.SuspendLayout();
			this.CustomsQtyCalcDropEdit.SuspendLayout();
			this.DrawBackQtyCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.InvoiceQtyCalcDropEdit.SuspendLayout();
			this.PriceCalcFindBox.SuspendLayout();
			this.DutyRateTypeDropEdit.SuspendLayout();
			this.MinMaxDutyDropEdit.SuspendLayout();
			this.PreferenceCodeDropEdit.SuspendLayout();
			this.DutyCodeDropEdit.SuspendLayout();
			this.DutyReductionCodeFindBox.SuspendLayout();
			this.InstalmentCodeFindBox.SuspendLayout();
			this.DomesticTaxCodeFindBox.SuspendLayout();
			this.DomesticTaxTypeDropEdit.SuspendLayout();
			this.DomesticTaxExemptionCodeFindBox.SuspendLayout();
			this.VATTypeDropEdit.SuspendLayout();
			this.VATReductionCodeFindBox.SuspendLayout();
			this.EducationTaxTypeDropEdit.SuspendLayout();
			this.AgricultureTaxTypeDropEdit.SuspendLayout();
			this.CustomsSecondQuantityCalcDropEdit.SuspendLayout();
			this.CustomsThirdQuantityCalcDropEdit.SuspendLayout();
			this.CustomsFourthQuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
			// 
			// ProductCodeFindBox
			// 
			this.ProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductCodeFindBox, "JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PartNo)));
			this.ProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 26, true);
			this.ProductCodeFindBox.Name = "ProductCodeFindBox";
			this.ProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProductCodeFindBox.ParentType = null;
			this.ProductCodeFindBox.ShowDescriptionBox = false;
			this.ProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ProductCodeFindBox.TabIndex = 0;
			// 
			// LotNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LotNumberTextBox, "JI_LotNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_LotNumber)));
			this.LotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 56, true);
			this.LotNumberTextBox.Name = "LotNumberTextBox";
			this.LotNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.LotNumberTextBox.TabIndex = 1;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetDataGrouping = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.GetTariffType = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 89, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.TariffFindBox.NeedLoadParentDataGroup = true;
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TariffFindBox.TabIndex = 2;
			this.TariffFindBox.TariffType = null;
			// 
			// GoodsDescriptionLongTextControl
			// 
			this.GoodsDescriptionLongTextControl.AllowDrop = true;
			this.GoodsDescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.GoodsDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 183, true);
			this.GoodsDescriptionLongTextControl.Name = "GoodsDescriptionLongTextControl";
			this.GoodsDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 21, true);
			this.GoodsDescriptionLongTextControl.TabIndex = 3;
			// 
			// BrandCodeFindBox
			// 
			this.BrandCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrandCodeFindBox, "JI_BrandCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_BrandCode)));
			this.BrandCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 121, true);
			this.BrandCodeFindBox.Name = "BrandCodeFindBox";
			this.BrandCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrandCodeFindBox.ParentType = null;
			this.BrandCodeFindBox.ShowDescriptionBox = false;
			this.BrandCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BrandCodeFindBox.TabIndex = 4;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_BrandName)));
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 154, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.BrandNameTextBox.TabIndex = 5;
			// 
			// ModelTradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTradeNameTextBox, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_Model)));
			this.ModelTradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 217, true);
			this.ModelTradeNameTextBox.Name = "ModelTradeNameTextBox";
			this.ModelTradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 20, true);
			this.ModelTradeNameTextBox.TabIndex = 6;
			// 
			// IngredientLongTextControl
			// 
			this.IngredientLongTextControl.AllowDrop = true;
			this.IngredientLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.IngredientLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 249, true);
			this.IngredientLongTextControl.Name = "IngredientLongTextControl";
			this.IngredientLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 21, true);
			this.IngredientLongTextControl.TabIndex = 7;
			// 
			// CustomsQtyCalcDropEdit
			// 
			this.CustomsQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsUnitQty)));
			this.CustomsQtyCalcDropEdit.BindToAmount = "JI_CustomsQuantity";
			this.CustomsQtyCalcDropEdit.BindToUnit = "JI_CustomsUnitQty";
			this.CustomsQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 286, true);
			this.CustomsQtyCalcDropEdit.Name = "CustomsQtyCalcDropEdit";
			this.CustomsQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsQtyCalcDropEdit.TabIndex = 8;
			this.CustomsQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// DrawBackQtyCalcDropEdit
			// 
			this.DrawBackQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DrawBackQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_DrawbackQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_DrawbackUQ)));
			this.DrawBackQtyCalcDropEdit.BindToAmount = "JI_DrawbackQuantity";
			this.DrawBackQtyCalcDropEdit.BindToUnit = "JI_DrawbackUQ";
			this.DrawBackQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 321, true);
			this.DrawBackQtyCalcDropEdit.Name = "DrawBackQtyCalcDropEdit";
			this.DrawBackQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.DrawBackQtyCalcDropEdit.TabIndex = 9;
			this.DrawBackQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "JI_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "JI_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 351, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 10;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_WeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "JI_Weight";
			this.GrossWeightCalcDropEdit.BindToUnit = "JI_WeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 377, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 11;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsUnitPriceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsUnitPriceCalcEdit, "CustomsUnitPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).CustomsUnitPrice)));
			this.CustomsUnitPriceCalcEdit.DecimalPlaces = 6;
			this.CustomsUnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 19, true);
			this.CustomsUnitPriceCalcEdit.Name = "CustomsUnitPriceCalcEdit";
			this.CustomsUnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsUnitPriceCalcEdit.TabIndex = 12;
			this.CustomsUnitPriceCalcEdit.Text = "0.000000";
			this.CustomsUnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CustomsUnitPriceCalcEdit.TrackDisposedAccess = true;
			// 
			// InvoiceQtyCalcDropEdit
			// 
			this.InvoiceQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InvoiceUQ)));
			this.InvoiceQtyCalcDropEdit.BindToAmount = "JI_InvoiceQuantity";
			this.InvoiceQtyCalcDropEdit.BindToUnit = "JI_InvoiceUQ";
			this.InvoiceQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 47, true);
			this.InvoiceQtyCalcDropEdit.Name = "InvoiceQtyCalcDropEdit";
			this.InvoiceQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.InvoiceQtyCalcDropEdit.TabIndex = 13;
			this.InvoiceQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// UnitPriceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UnitPriceCalcEdit, "UnitPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).UnitPrice)));
			this.UnitPriceCalcEdit.DecimalPlaces = 6;
			this.UnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 89, true);
			this.UnitPriceCalcEdit.Name = "UnitPriceCalcEdit";
			this.UnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.UnitPriceCalcEdit.TabIndex = 14;
			this.UnitPriceCalcEdit.Text = "0.000000";
			this.UnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UnitPriceCalcEdit.TrackDisposedAccess = true;
			// 
			// PriceCalcFindBox
			// 
			this.PriceCalcFindBox.AllowDrop = true;
			this.PriceCalcFindBox.BindToAmount = "JI_LinePrice";
			this.PriceCalcFindBox.BindToUnit = "JI_RX_NKLinePriceCurr";
			this.PriceCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PriceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 16, true);
			this.PriceCalcFindBox.Name = "PriceCalcFindBox";
			this.PriceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PriceCalcFindBox.TabIndex = 15;
			// 
			// DutyRateTypeDropEdit
			// 
			this.DutyRateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyRateTypeDropEdit, "DutyRateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyRateCode)));
			this.DutyRateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 161, true);
			this.DutyRateTypeDropEdit.Name = "DutyRateTypeDropEdit";
			this.DutyRateTypeDropEdit.PreBoundMaxLength = 1;
			this.DutyRateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.DutyRateTypeDropEdit.TabIndex = 16;
			// 
			// MinMaxDutyDropEdit
			// 
			this.MinMaxDutyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MinMaxDutyDropEdit, "JI_DutyRateSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_DutyRateSelection)));
			this.MinMaxDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 80, true);
			this.MinMaxDutyDropEdit.Name = "MinMaxDutyDropEdit";
			this.MinMaxDutyDropEdit.PreBoundMaxLength = 3;
			this.MinMaxDutyDropEdit.ShowDescriptionBox = false;
			this.MinMaxDutyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MinMaxDutyDropEdit.TabIndex = 18;
			// 
			// PreferenceCodeDropEdit
			// 
			this.PreferenceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceCodeDropEdit, "JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PrimaryPreference)));
			this.PreferenceCodeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("037D43C5-A727-45ED-90D9-6CDA906EE10B", "Preference Code / Rate");
			this.PreferenceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 114, true);
			this.PreferenceCodeDropEdit.Name = "PreferenceCodeDropEdit";
			this.PreferenceCodeDropEdit.PreBoundMaxLength = 6;
			this.PreferenceCodeDropEdit.ShowDescriptionBox = false;
			this.PreferenceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.PreferenceCodeDropEdit.TabIndex = 19;
			// 
			// DutyCodeDropEdit
			// 
			this.DutyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyCodeDropEdit, "JI_AdditionalDutyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_AdditionalDutyType)));
			this.DutyCodeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6FAA6762-E811-44B5-9079-178BAC509AEE", "Add. Duty / Rate");
			this.DutyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 138, true);
			this.DutyCodeDropEdit.Name = "DutyCodeDropEdit";
			this.DutyCodeDropEdit.PreBoundMaxLength = 6;
			this.DutyCodeDropEdit.ShowDescriptionBox = false;
			this.DutyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.DutyCodeDropEdit.TabIndex = 20;
			// 
			// DutyReductionCodeFindBox
			// 
			this.DutyReductionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyReductionCodeFindBox, "JI_SecondaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SecondaryPreference)));
			this.DutyReductionCodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0B3826B6-6030-4339-9E9C-A83372A141D3", "Duty Reduction Code / Rate");
			this.DutyReductionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 286, true);
			this.DutyReductionCodeFindBox.Name = "DutyReductionCodeFindBox";
			this.DutyReductionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DutyReductionCodeFindBox.ParentType = null;
			this.DutyReductionCodeFindBox.PreBoundMaxLength = 12;
			this.DutyReductionCodeFindBox.ShowDescriptionBox = false;
			this.DutyReductionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.DutyReductionCodeFindBox.TabIndex = 21;
			// 
			// InstalmentCodeFindBox
			// 
			this.InstalmentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstalmentCodeFindBox, "JI_InstallmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InstallmentCode)));
			this.InstalmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 315, true);
			this.InstalmentCodeFindBox.Name = "InstalmentCodeFindBox";
			this.InstalmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InstalmentCodeFindBox.ParentType = null;
			this.InstalmentCodeFindBox.PreBoundMaxLength = 12;
			this.InstalmentCodeFindBox.ShowDescriptionBox = false;
			this.InstalmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.InstalmentCodeFindBox.TabIndex = 22;
			// 
			// SpecificUsePermitNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SpecificUsePermitNoTextBox, "JI_SpecificUseCodeDutyRatePermitNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SpecificUseCodeDutyRatePermitNo)));
			this.SpecificUsePermitNoTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4ACCBDF6-9539-47ED-9A48-554EDBD73A53", "Specific Use Permit No / Specific Use");
			this.SpecificUsePermitNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 343, true);
			this.SpecificUsePermitNoTextBox.Name = "SpecificUsePermitNoTextBox";
			this.SpecificUsePermitNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SpecificUsePermitNoTextBox.TabIndex = 23;
			// 
			// SpecificUseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SpecificUseCheckBox, "JI_IsSpecificUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_IsSpecificUseCode)));
			this.SpecificUseCheckBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("029A458F-A548-4712-B8FA-CB27A28C3281", "/");
			this.SpecificUseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SpecificUseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 371, true);
			this.SpecificUseCheckBox.Name = "SpecificUseCheckBox";
			this.SpecificUseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 24, true);
			this.SpecificUseCheckBox.TabIndex = 24;
			this.SpecificUseCheckBox.UseVisualStyleBackColor = true;
			// 
			// DomesticTaxCodeFindBox
			// 
			this.DomesticTaxCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DomesticTaxCodeFindBox, "JI_DomesticTaxCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_DomesticTaxCode)));
			this.DomesticTaxCodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("F0ECB8F3-450C-4083-BF7B-E3BBFD9F53B2", "Domestic Tax Code / Type");
			this.DomesticTaxCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 32, true);
			this.DomesticTaxCodeFindBox.Name = "DomesticTaxCodeFindBox";
			this.DomesticTaxCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DomesticTaxCodeFindBox.ParentType = null;
			this.DomesticTaxCodeFindBox.PreBoundMaxLength = 8;
			this.DomesticTaxCodeFindBox.ShowDescriptionBox = false;
			this.DomesticTaxCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.DomesticTaxCodeFindBox.TabIndex = 25;
			// 
			// DomesticTaxTypeDropEdit
			// 
			this.DomesticTaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DomesticTaxTypeDropEdit, "DomesticTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DomesticTaxType)));
			this.DomesticTaxTypeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("B5A8ACF6-53C9-457D-8421-753C9E11692E", "/");
			this.DomesticTaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 64, true);
			this.DomesticTaxTypeDropEdit.Name = "DomesticTaxTypeDropEdit";
			this.DomesticTaxTypeDropEdit.PreBoundMaxLength = 2;
			this.DomesticTaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.DomesticTaxTypeDropEdit.TabIndex = 26;
			// 
			// DomesticTaxRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DomesticTaxRateCalcEdit, "DomesticTaxRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DomesticTaxRate)));
			this.DomesticTaxRateCalcEdit.DecimalPlaces = 2;
			this.DomesticTaxRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 99, true);
			this.DomesticTaxRateCalcEdit.Name = "DomesticTaxRateCalcEdit";
			this.DomesticTaxRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.DomesticTaxRateCalcEdit.TabIndex = 27;
			this.DomesticTaxRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DomesticTaxRateCalcEdit.TrackDisposedAccess = true;
			// 
			// DomesticTaxExemptionCodeFindBox
			// 
			this.DomesticTaxExemptionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DomesticTaxExemptionCodeFindBox, "JI_DomesticTaxExemptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_DomesticTaxExemptionCode)));
			this.DomesticTaxExemptionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 123, true);
			this.DomesticTaxExemptionCodeFindBox.Name = "DomesticTaxExemptionCodeFindBox";
			this.DomesticTaxExemptionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DomesticTaxExemptionCodeFindBox.ParentType = null;
			this.DomesticTaxExemptionCodeFindBox.PreBoundMaxLength = 7;
			this.DomesticTaxExemptionCodeFindBox.ShowDescriptionBox = false;
			this.DomesticTaxExemptionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.DomesticTaxExemptionCodeFindBox.TabIndex = 28;
			// 
			// DomesticTaxBaseQtyOrPriceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DomesticTaxBaseQtyOrPriceCalcEdit, "DomesticTaxBaseQtyOrPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DomesticTaxBaseQtyOrPrice)));
			this.DomesticTaxBaseQtyOrPriceCalcEdit.DecimalPlaces = 2;
			this.DomesticTaxBaseQtyOrPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 154, true);
			this.DomesticTaxBaseQtyOrPriceCalcEdit.Name = "DomesticTaxBaseQtyOrPriceCalcEdit";
			this.DomesticTaxBaseQtyOrPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.DomesticTaxBaseQtyOrPriceCalcEdit.TabIndex = 29;
			this.DomesticTaxBaseQtyOrPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DomesticTaxBaseQtyOrPriceCalcEdit.TrackDisposedAccess = true;
			// 
			// VATTypeDropEdit
			// 
			this.VATTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATTypeDropEdit, "JI_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_ZZF_NKTaxType)));
			this.VATTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 187, true);
			this.VATTypeDropEdit.Name = "VATTypeDropEdit";
			this.VATTypeDropEdit.PreBoundMaxLength = 2;
			this.VATTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.VATTypeDropEdit.TabIndex = 30;
			// 
			// VATReductionCodeFindBox
			// 
			this.VATReductionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATReductionCodeFindBox, "JI_VATReductionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_VATReductionCode)));
			this.VATReductionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 221, true);
			this.VATReductionCodeFindBox.Name = "VATReductionCodeFindBox";
			this.VATReductionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VATReductionCodeFindBox.ParentType = null;
			this.VATReductionCodeFindBox.PreBoundMaxLength = 7;
			this.VATReductionCodeFindBox.ShowDescriptionBox = false;
			this.VATReductionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.VATReductionCodeFindBox.TabIndex = 31;
			// 
			// EducationTaxTypeDropEdit
			// 
			this.EducationTaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EducationTaxTypeDropEdit, "EducationTaxExemptIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).EducationTaxExemptIndicator)));
			this.EducationTaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 286, true);
			this.EducationTaxTypeDropEdit.Name = "EducationTaxTypeDropEdit";
			this.EducationTaxTypeDropEdit.PreBoundMaxLength = 1;
			this.EducationTaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.EducationTaxTypeDropEdit.TabIndex = 32;
			// 
			// AgricultureTaxTypeDropEdit
			// 
			this.AgricultureTaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgricultureTaxTypeDropEdit, "AgricultureTaxClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).AgricultureTaxClassification)));
			this.AgricultureTaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 320, true);
			this.AgricultureTaxTypeDropEdit.Name = "AgricultureTaxTypeDropEdit";
			this.AgricultureTaxTypeDropEdit.PreBoundMaxLength = 1;
			this.AgricultureTaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.AgricultureTaxTypeDropEdit.TabIndex = 33;
			// 
			// DutyRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyRateCalcEdit, "DutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyRate)));
			this.DutyRateCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("692D550D-C6DD-4DD3-9211-D0F1FFC74756", "/");
			this.DutyRateCalcEdit.DecimalPlaces = 2;
			this.DutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 349, true);
			this.DutyRateCalcEdit.Name = "DutyRateCalcEdit";
			this.DutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.DutyRateCalcEdit.TabIndex = 34;
			this.DutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// AddDutyRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDutyRateCalcEdit, "JI_AdditionalDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_AdditionalDutyRate)));
			this.AddDutyRateCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("392B75DE-A693-4C11-8318-14FCEDDE16B6", "/");
			this.AddDutyRateCalcEdit.DecimalPlaces = 2;
			this.AddDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 377, true);
			this.AddDutyRateCalcEdit.Name = "AddDutyRateCalcEdit";
			this.AddDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.AddDutyRateCalcEdit.TabIndex = 35;
			this.AddDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AddDutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// DutyReductionRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyReductionRateCalcEdit, "DutyReductionRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyReductionRate)));
			this.DutyReductionRateCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("C47958F5-73E9-45A1-A6F4-0368907BF44B", "/");
			this.DutyReductionRateCalcEdit.DecimalPlaces = 2;
			this.DutyReductionRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 407, true);
			this.DutyReductionRateCalcEdit.Name = "DutyReductionRateCalcEdit";
			this.DutyReductionRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.DutyReductionRateCalcEdit.TabIndex = 36;
			this.DutyReductionRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyReductionRateCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsSecondQuantityCalcDropEdit
			// 
			this.CustomsSecondQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsSecondQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsSecondUnitQty)));
			this.CustomsSecondQuantityCalcDropEdit.BindToAmount = "JI_CustomsSecondQuantity";
			this.CustomsSecondQuantityCalcDropEdit.BindToUnit = "JI_CustomsSecondUnitQty";
			this.CustomsSecondQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 403, true);
			this.CustomsSecondQuantityCalcDropEdit.Name = "CustomsSecondQuantityCalcDropEdit";
			this.CustomsSecondQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsSecondQuantityCalcDropEdit.TabIndex = 37;
			this.CustomsSecondQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsThirdQuantityCalcDropEdit
			// 
			this.CustomsThirdQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsThirdUnitQty)));
			this.CustomsThirdQuantityCalcDropEdit.BindToAmount = "JI_CustomsThirdQuantity";
			this.CustomsThirdQuantityCalcDropEdit.BindToUnit = "JI_CustomsThirdUnitQty";
			this.CustomsThirdQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 429, true);
			this.CustomsThirdQuantityCalcDropEdit.Name = "CustomsThirdQuantityCalcDropEdit";
			this.CustomsThirdQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsThirdQuantityCalcDropEdit.TabIndex = 38;
			this.CustomsThirdQuantityCalcDropEdit.UnitPreBoundMaxLength = 6;
			// 
			// CustomsFourthQuantityCalcDropEdit
			// 
			this.CustomsFourthQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsFourthQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsFourthQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsFourthUnitQty)));
			this.CustomsFourthQuantityCalcDropEdit.BindToAmount = "JI_CustomsFourthQuantity";
			this.CustomsFourthQuantityCalcDropEdit.BindToUnit = "JI_CustomsFourthUnitQty";
			this.CustomsFourthQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 455, true);
			this.CustomsFourthQuantityCalcDropEdit.Name = "CustomsFourthQuantityCalcDropEdit";
			this.CustomsFourthQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsFourthQuantityCalcDropEdit.TabIndex = 39;
			this.CustomsFourthQuantityCalcDropEdit.UnitPreBoundMaxLength = 6;
			// 
			// InstallationCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InstallationCostCalcEdit, "JI_InstallationCost");
			this.InstallationCostCalcEdit.DecimalPlaces = 0;
			this.InstallationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 403, true);
			this.InstallationCostCalcEdit.Name = "InstallationCostCalcEdit";
			this.InstallationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.InstallationCostCalcEdit.TabIndex = 40;
			this.InstallationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InstallationCostCalcEdit.TrackDisposedAccess = true;
			// 
			// ImportInvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InstallationCostCalcEdit);
			this.Controls.Add(this.CustomsFourthQuantityCalcDropEdit);
			this.Controls.Add(this.CustomsThirdQuantityCalcDropEdit);
			this.Controls.Add(this.CustomsSecondQuantityCalcDropEdit);
			this.Controls.Add(this.DutyReductionRateCalcEdit);
			this.Controls.Add(this.AddDutyRateCalcEdit);
			this.Controls.Add(this.DutyRateCalcEdit);
			this.Controls.Add(this.AgricultureTaxTypeDropEdit);
			this.Controls.Add(this.EducationTaxTypeDropEdit);
			this.Controls.Add(this.VATReductionCodeFindBox);
			this.Controls.Add(this.VATTypeDropEdit);
			this.Controls.Add(this.DomesticTaxBaseQtyOrPriceCalcEdit);
			this.Controls.Add(this.DomesticTaxExemptionCodeFindBox);
			this.Controls.Add(this.DomesticTaxRateCalcEdit);
			this.Controls.Add(this.DomesticTaxTypeDropEdit);
			this.Controls.Add(this.DomesticTaxCodeFindBox);
			this.Controls.Add(this.SpecificUseCheckBox);
			this.Controls.Add(this.SpecificUsePermitNoTextBox);
			this.Controls.Add(this.InstalmentCodeFindBox);
			this.Controls.Add(this.DutyReductionCodeFindBox);
			this.Controls.Add(this.DutyCodeDropEdit);
			this.Controls.Add(this.PreferenceCodeDropEdit);
			this.Controls.Add(this.MinMaxDutyDropEdit);
			this.Controls.Add(this.DutyRateTypeDropEdit);
			this.Controls.Add(this.PriceCalcFindBox);
			this.Controls.Add(this.UnitPriceCalcEdit);
			this.Controls.Add(this.InvoiceQtyCalcDropEdit);
			this.Controls.Add(this.CustomsUnitPriceCalcEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.DrawBackQtyCalcDropEdit);
			this.Controls.Add(this.CustomsQtyCalcDropEdit);
			this.Controls.Add(this.IngredientLongTextControl);
			this.Controls.Add(this.ModelTradeNameTextBox);
			this.Controls.Add(this.BrandNameTextBox);
			this.Controls.Add(this.BrandCodeFindBox);
			this.Controls.Add(this.GoodsDescriptionLongTextControl);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.LotNumberTextBox);
			this.Controls.Add(this.ProductCodeFindBox);
			this.Name = "ImportInvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 483, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProductCodeFindBox.ResumeLayout(true);
			this.ProductCodeFindBox.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.GoodsDescriptionLongTextControl.ResumeLayout(true);
			this.GoodsDescriptionLongTextControl.PerformLayout();
			this.BrandCodeFindBox.ResumeLayout(true);
			this.BrandCodeFindBox.PerformLayout();
			this.IngredientLongTextControl.ResumeLayout(true);
			this.IngredientLongTextControl.PerformLayout();
			this.CustomsQtyCalcDropEdit.ResumeLayout(true);
			this.CustomsQtyCalcDropEdit.PerformLayout();
			this.DrawBackQtyCalcDropEdit.ResumeLayout(true);
			this.DrawBackQtyCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.InvoiceQtyCalcDropEdit.ResumeLayout(true);
			this.InvoiceQtyCalcDropEdit.PerformLayout();
			this.PriceCalcFindBox.ResumeLayout(true);
			this.PriceCalcFindBox.PerformLayout();
			this.DutyRateTypeDropEdit.ResumeLayout(true);
			this.DutyRateTypeDropEdit.PerformLayout();
			this.MinMaxDutyDropEdit.ResumeLayout(true);
			this.MinMaxDutyDropEdit.PerformLayout();
			this.PreferenceCodeDropEdit.ResumeLayout(true);
			this.PreferenceCodeDropEdit.PerformLayout();
			this.DutyCodeDropEdit.ResumeLayout(true);
			this.DutyCodeDropEdit.PerformLayout();
			this.DutyReductionCodeFindBox.ResumeLayout(true);
			this.DutyReductionCodeFindBox.PerformLayout();
			this.InstalmentCodeFindBox.ResumeLayout(true);
			this.InstalmentCodeFindBox.PerformLayout();
			this.DomesticTaxCodeFindBox.ResumeLayout(true);
			this.DomesticTaxCodeFindBox.PerformLayout();
			this.DomesticTaxTypeDropEdit.ResumeLayout(true);
			this.DomesticTaxTypeDropEdit.PerformLayout();
			this.DomesticTaxExemptionCodeFindBox.ResumeLayout(true);
			this.DomesticTaxExemptionCodeFindBox.PerformLayout();
			this.VATTypeDropEdit.ResumeLayout(true);
			this.VATTypeDropEdit.PerformLayout();
			this.VATReductionCodeFindBox.ResumeLayout(true);
			this.VATReductionCodeFindBox.PerformLayout();
			this.EducationTaxTypeDropEdit.ResumeLayout(true);
			this.EducationTaxTypeDropEdit.PerformLayout();
			this.AgricultureTaxTypeDropEdit.ResumeLayout(true);
			this.AgricultureTaxTypeDropEdit.PerformLayout();
			this.CustomsSecondQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsSecondQuantityCalcDropEdit.PerformLayout();
			this.CustomsThirdQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityCalcDropEdit.PerformLayout();
			this.CustomsFourthQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsFourthQuantityCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCodeFindBox ProductCodeFindBox;
		public ZArchitecture.ZTextBox LotNumberTextBox;
		public Universal.GUI.TariffFindBox TariffFindBox;
		public LongTextControl GoodsDescriptionLongTextControl;
		public ZArchitecture.GUI.ZCodeFindBox BrandCodeFindBox;
		public ZArchitecture.ZTextBox BrandNameTextBox;
		public ZArchitecture.ZTextBox ModelTradeNameTextBox;
		public LongTextControl IngredientLongTextControl;
		public ZArchitecture.GUI.ZCalcDropEdit CustomsQtyCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit DrawBackQtyCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		public ZArchitecture.ZCalcEdit CustomsUnitPriceCalcEdit;
		public ZArchitecture.GUI.ZCalcDropEdit InvoiceQtyCalcDropEdit;
		public ZArchitecture.ZCalcEdit UnitPriceCalcEdit;
		public ZArchitecture.GUI.ZCalcFindBox PriceCalcFindBox;
		public ZArchitecture.GUI.ZDropEdit DutyRateTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit MinMaxDutyDropEdit;
		public ZArchitecture.GUI.ZDropEdit PreferenceCodeDropEdit;
		public ZArchitecture.GUI.ZDropEdit DutyCodeDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox DutyReductionCodeFindBox;
		public ZArchitecture.GUI.ZCodeFindBox InstalmentCodeFindBox;
		public ZArchitecture.ZTextBox SpecificUsePermitNoTextBox;
		public ZArchitecture.GUI.ZCheckBox SpecificUseCheckBox;
		public ZArchitecture.GUI.ZCodeFindBox DomesticTaxCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit DomesticTaxTypeDropEdit;
		public ZArchitecture.ZCalcEdit DomesticTaxRateCalcEdit;
		public ZArchitecture.GUI.ZCodeFindBox DomesticTaxExemptionCodeFindBox;
		public ZArchitecture.ZCalcEdit DomesticTaxBaseQtyOrPriceCalcEdit;
		public ZArchitecture.GUI.ZDropEdit VATTypeDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox VATReductionCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit EducationTaxTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit AgricultureTaxTypeDropEdit;
		public ZArchitecture.ZCalcEdit DutyRateCalcEdit;
		public ZArchitecture.ZCalcEdit AddDutyRateCalcEdit;
		public ZArchitecture.ZCalcEdit DutyReductionRateCalcEdit;
		public ZArchitecture.GUI.ZCalcDropEdit CustomsSecondQuantityCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit CustomsFourthQuantityCalcDropEdit;
		public ZArchitecture.ZCalcEdit InstallationCostCalcEdit;
	}
}
