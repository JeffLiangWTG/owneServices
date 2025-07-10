using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PriceUserControl
	{

		private void InitializeComponent()
		{
            this.PaymentAmountConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.ExchangeRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.IndirectAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PaymentAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BrokerageFeeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PurchaseCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.GoodsCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ContainerPackagingCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.RoyaltyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProductDevCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CommodityUsageCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProductToolCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TransportationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.InsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.UnloadCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.FreightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ExcludingTransportationCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProfitAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalAdditionalAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalDeductionAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DiscountAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.OtherCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TechnicalCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.LocalTransportationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PaymentAmountConvertToLocalCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
            // 
            // PaymentAmountConvertToLocalCurrencyControl
            // 
            this.PaymentAmountConvertToLocalCurrencyControl.AllowDrop = true;
            this.PaymentAmountConvertToLocalCurrencyControl.BindToAmount = "JZ_InvoiceAmount";
            this.PaymentAmountConvertToLocalCurrencyControl.BindToUnit = "JZ_RX_NKInvoice_Currency";
            this.PaymentAmountConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("17d091ca-adc0-42de-ad46-c66df31bbeeb", "Payment Amount");
            this.PaymentAmountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.PaymentAmountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 18, true);
            this.PaymentAmountConvertToLocalCurrencyControl.Name = "PaymentAmountConvertToLocalCurrencyControl";
            this.PaymentAmountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
            this.PaymentAmountConvertToLocalCurrencyControl.TabIndex = 0;
            // 
            // ExchangeRateCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ExchangeRateCalcEdit, "JZ_InvoiceCurrExRate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
            this.ExchangeRateCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("b4a630c7-1edc-4b15-8e0e-1068a7473def", "Exchange Rate");
            this.ExchangeRateCalcEdit.DecimalPlaces = 4;
            this.ExchangeRateCalcEdit.Decimals = 4;
            this.ExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 18, true);
            this.ExchangeRateCalcEdit.Name = "ExchangeRateCalcEdit";
            this.ExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ExchangeRateCalcEdit.TabIndex = 1;
            this.ExchangeRateCalcEdit.Text = "0.0000";
            this.ExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ExchangeRateCalcEdit.TrackDisposedAccess = true;
            // 
            // IndirectAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.IndirectAmountCalcEdit, "IndirectAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).IndirectAmount)));
            this.IndirectAmountCalcEdit.DecimalPlaces = 0;
            this.IndirectAmountCalcEdit.Decimals = 0;
            this.IndirectAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 44, true);
            this.IndirectAmountCalcEdit.Name = "IndirectAmountCalcEdit";
            this.IndirectAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.IndirectAmountCalcEdit.TabIndex = 2;
            this.IndirectAmountCalcEdit.Text = "0";
            this.IndirectAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.IndirectAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // PaymentAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.PaymentAmountCalcEdit, "JZ_InvoiceAmountInLocalCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceAmountInLocalCurrency)));
            this.PaymentAmountCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("286bb5d3-4159-485c-aaba-c1d3e870621e", "Payment Amount (KRW)");
            this.PaymentAmountCalcEdit.DecimalPlaces = 0;
            this.PaymentAmountCalcEdit.Decimals = 0;
            this.PaymentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 44, true);
            this.PaymentAmountCalcEdit.Name = "PaymentAmountCalcEdit";
            this.PaymentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.PaymentAmountCalcEdit.TabIndex = 3;
            this.PaymentAmountCalcEdit.Text = "0";
            this.PaymentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PaymentAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // BrokerageFeeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.BrokerageFeeCalcEdit, "BrokerageFee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).BrokerageFee)));
            this.BrokerageFeeCalcEdit.DecimalPlaces = 0;
            this.BrokerageFeeCalcEdit.Decimals = 0;
            this.BrokerageFeeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 117, true);
            this.BrokerageFeeCalcEdit.Name = "BrokerageFeeCalcEdit";
            this.BrokerageFeeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.BrokerageFeeCalcEdit.TabIndex = 5;
            this.BrokerageFeeCalcEdit.Text = "0";
            this.BrokerageFeeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BrokerageFeeCalcEdit.TrackDisposedAccess = true;
            // 
            // PurchaseCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.PurchaseCostCalcEdit, "PurchaseCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).PurchaseCost)));
            this.PurchaseCostCalcEdit.DecimalPlaces = 0;
            this.PurchaseCostCalcEdit.Decimals = 0;
            this.PurchaseCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 117, true);
            this.PurchaseCostCalcEdit.Name = "PurchaseCostCalcEdit";
            this.PurchaseCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.PurchaseCostCalcEdit.TabIndex = 4;
            this.PurchaseCostCalcEdit.Text = "0";
            this.PurchaseCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PurchaseCostCalcEdit.TrackDisposedAccess = true;
            // 
            // GoodsCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.GoodsCostCalcEdit, "GoodsCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).GoodsCost)));
            this.GoodsCostCalcEdit.DecimalPlaces = 0;
            this.GoodsCostCalcEdit.Decimals = 0;
            this.GoodsCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 143, true);
            this.GoodsCostCalcEdit.Name = "GoodsCostCalcEdit";
            this.GoodsCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.GoodsCostCalcEdit.TabIndex = 7;
            this.GoodsCostCalcEdit.Text = "0";
            this.GoodsCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.GoodsCostCalcEdit.TrackDisposedAccess = true;
            // 
            // ContainerPackagingCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ContainerPackagingCostCalcEdit, "ContainerPackagingCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ContainerPackagingCost)));
            this.ContainerPackagingCostCalcEdit.DecimalPlaces = 0;
            this.ContainerPackagingCostCalcEdit.Decimals = 0;
            this.ContainerPackagingCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 143, true);
            this.ContainerPackagingCostCalcEdit.Name = "ContainerPackagingCostCalcEdit";
            this.ContainerPackagingCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ContainerPackagingCostCalcEdit.TabIndex = 6;
            this.ContainerPackagingCostCalcEdit.Text = "0";
            this.ContainerPackagingCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ContainerPackagingCostCalcEdit.TrackDisposedAccess = true;
            // 
            // RoyaltyCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.RoyaltyCalcEdit, "Royalty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).Royalty)));
            this.RoyaltyCalcEdit.DecimalPlaces = 0;
            this.RoyaltyCalcEdit.Decimals = 0;
            this.RoyaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 195, true);
            this.RoyaltyCalcEdit.Name = "RoyaltyCalcEdit";
            this.RoyaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.RoyaltyCalcEdit.TabIndex = 11;
            this.RoyaltyCalcEdit.Text = "0";
            this.RoyaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.RoyaltyCalcEdit.TrackDisposedAccess = true;
            // 
            // ProductDevCostsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProductDevCostsCalcEdit, "ProductDevCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ProductDevCost)));
            this.ProductDevCostsCalcEdit.DecimalPlaces = 0;
            this.ProductDevCostsCalcEdit.Decimals = 0;
            this.ProductDevCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 195, true);
            this.ProductDevCostsCalcEdit.Name = "ProductDevCostsCalcEdit";
            this.ProductDevCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ProductDevCostsCalcEdit.TabIndex = 10;
            this.ProductDevCostsCalcEdit.Text = "0";
            this.ProductDevCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProductDevCostsCalcEdit.TrackDisposedAccess = true;
            // 
            // CommodityUsageCostsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.CommodityUsageCostsCalcEdit, "CommodityUsageCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).CommodityUsageCost)));
            this.CommodityUsageCostsCalcEdit.DecimalPlaces = 0;
            this.CommodityUsageCostsCalcEdit.Decimals = 0;
            this.CommodityUsageCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 169, true);
            this.CommodityUsageCostsCalcEdit.Name = "CommodityUsageCostsCalcEdit";
            this.CommodityUsageCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.CommodityUsageCostsCalcEdit.TabIndex = 9;
            this.CommodityUsageCostsCalcEdit.Text = "0";
            this.CommodityUsageCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CommodityUsageCostsCalcEdit.TrackDisposedAccess = true;
            // 
            // ProductToolCostsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProductToolCostsCalcEdit, "ProductToolCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ProductToolCost)));
            this.ProductToolCostsCalcEdit.DecimalPlaces = 0;
            this.ProductToolCostsCalcEdit.Decimals = 0;
            this.ProductToolCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 169, true);
            this.ProductToolCostsCalcEdit.Name = "ProductToolCostsCalcEdit";
            this.ProductToolCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ProductToolCostsCalcEdit.TabIndex = 8;
            this.ProductToolCostsCalcEdit.Text = "0";
            this.ProductToolCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProductToolCostsCalcEdit.TrackDisposedAccess = true;
            // 
            // TransportationCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TransportationCostCalcEdit, "TransportationCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TransportationCost)));
            this.TransportationCostCalcEdit.DecimalPlaces = 0;
            this.TransportationCostCalcEdit.Decimals = 0;
            this.TransportationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 273, true);
            this.TransportationCostCalcEdit.Name = "TransportationCostCalcEdit";
            this.TransportationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.TransportationCostCalcEdit.TabIndex = 17;
            this.TransportationCostCalcEdit.Text = "0";
            this.TransportationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TransportationCostCalcEdit.TrackDisposedAccess = true;
            // 
            // InsuranceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.InsuranceCalcEdit, "Insurance");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).Insurance)));
            this.InsuranceCalcEdit.DecimalPlaces = 0;
            this.InsuranceCalcEdit.Decimals = 0;
            this.InsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 273, true);
            this.InsuranceCalcEdit.Name = "InsuranceCalcEdit";
            this.InsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.InsuranceCalcEdit.TabIndex = 16;
            this.InsuranceCalcEdit.Text = "0";
            this.InsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.InsuranceCalcEdit.TrackDisposedAccess = true;
            // 
            // UnloadCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.UnloadCostCalcEdit, "UnloadCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).UnloadCost)));
            this.UnloadCostCalcEdit.DecimalPlaces = 0;
            this.UnloadCostCalcEdit.Decimals = 0;
            this.UnloadCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 247, true);
            this.UnloadCostCalcEdit.Name = "UnloadCostCalcEdit";
            this.UnloadCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.UnloadCostCalcEdit.TabIndex = 15;
            this.UnloadCostCalcEdit.Text = "0";
            this.UnloadCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.UnloadCostCalcEdit.TrackDisposedAccess = true;
            // 
            // FreightCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.FreightCalcEdit, "Freight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).Freight)));
            this.FreightCalcEdit.DecimalPlaces = 0;
            this.FreightCalcEdit.Decimals = 0;
            this.FreightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 247, true);
            this.FreightCalcEdit.Name = "FreightCalcEdit";
            this.FreightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.FreightCalcEdit.TabIndex = 14;
            this.FreightCalcEdit.Text = "0";
            this.FreightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FreightCalcEdit.TrackDisposedAccess = true;
            // 
            // ExcludingTransportationCostsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ExcludingTransportationCostsCalcEdit, "ExcludingTransportationCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ExcludingTransportationCost)));
            this.ExcludingTransportationCostsCalcEdit.DecimalPlaces = 0;
            this.ExcludingTransportationCostsCalcEdit.Decimals = 0;
            this.ExcludingTransportationCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 221, true);
            this.ExcludingTransportationCostsCalcEdit.Name = "ExcludingTransportationCostsCalcEdit";
            this.ExcludingTransportationCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ExcludingTransportationCostsCalcEdit.TabIndex = 13;
            this.ExcludingTransportationCostsCalcEdit.Text = "0";
            this.ExcludingTransportationCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ExcludingTransportationCostsCalcEdit.TrackDisposedAccess = true;
            // 
            // ProfitAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProfitAmountCalcEdit, "ProfitAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ProfitAmount)));
            this.ProfitAmountCalcEdit.DecimalPlaces = 0;
            this.ProfitAmountCalcEdit.Decimals = 0;
            this.ProfitAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 221, true);
            this.ProfitAmountCalcEdit.Name = "ProfitAmountCalcEdit";
            this.ProfitAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.ProfitAmountCalcEdit.TabIndex = 12;
            this.ProfitAmountCalcEdit.Text = "0";
            this.ProfitAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProfitAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalAdditionalAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalAdditionalAmountCalcEdit, "TotalAdditionalAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TotalAdditionalAmount)));
            this.TotalAdditionalAmountCalcEdit.DecimalPlaces = 0;
            this.TotalAdditionalAmountCalcEdit.Decimals = 0;
            this.TotalAdditionalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 299, true);
            this.TotalAdditionalAmountCalcEdit.Name = "TotalAdditionalAmountCalcEdit";
            this.TotalAdditionalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.TotalAdditionalAmountCalcEdit.TabIndex = 18;
            this.TotalAdditionalAmountCalcEdit.Text = "0";
            this.TotalAdditionalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalAdditionalAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalDeductionAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalDeductionAmountCalcEdit, "TotalDeductionAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TotalDeductionAmount)));
            this.TotalDeductionAmountCalcEdit.DecimalPlaces = 0;
            this.TotalDeductionAmountCalcEdit.Decimals = 0;
            this.TotalDeductionAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 410, true);
            this.TotalDeductionAmountCalcEdit.Name = "TotalDeductionAmountCalcEdit";
            this.TotalDeductionAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.TotalDeductionAmountCalcEdit.TabIndex = 23;
            this.TotalDeductionAmountCalcEdit.Text = "0";
            this.TotalDeductionAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalDeductionAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // DiscountAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DiscountAmountCalcEdit, "DiscountAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DiscountAmount)));
            this.DiscountAmountCalcEdit.DecimalPlaces = 0;
            this.DiscountAmountCalcEdit.Decimals = 0;
            this.DiscountAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 384, true);
            this.DiscountAmountCalcEdit.Name = "DiscountAmountCalcEdit";
            this.DiscountAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.DiscountAmountCalcEdit.TabIndex = 22;
            this.DiscountAmountCalcEdit.Text = "0";
            this.DiscountAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DiscountAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // OtherCostsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.OtherCostsCalcEdit, "OtherCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).OtherCost)));
            this.OtherCostsCalcEdit.DecimalPlaces = 0;
            this.OtherCostsCalcEdit.Decimals = 0;
            this.OtherCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 384, true);
            this.OtherCostsCalcEdit.Name = "OtherCostsCalcEdit";
            this.OtherCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.OtherCostsCalcEdit.TabIndex = 21;
            this.OtherCostsCalcEdit.Text = "0";
            this.OtherCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.OtherCostsCalcEdit.TrackDisposedAccess = true;
            // 
            // TechnicalCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TechnicalCostCalcEdit, "TechnicalCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TechnicalCost)));
            this.TechnicalCostCalcEdit.DecimalPlaces = 0;
            this.TechnicalCostCalcEdit.Decimals = 0;
            this.TechnicalCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 358, true);
            this.TechnicalCostCalcEdit.Name = "TechnicalCostCalcEdit";
            this.TechnicalCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.TechnicalCostCalcEdit.TabIndex = 20;
            this.TechnicalCostCalcEdit.Text = "0";
            this.TechnicalCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TechnicalCostCalcEdit.TrackDisposedAccess = true;
            // 
            // LocalTransportationCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.LocalTransportationCostCalcEdit, "LocalTransportationCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).LocalTransportationCost)));
            this.LocalTransportationCostCalcEdit.DecimalPlaces = 0;
            this.LocalTransportationCostCalcEdit.Decimals = 0;
            this.LocalTransportationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 358, true);
            this.LocalTransportationCostCalcEdit.Name = "LocalTransportationCostCalcEdit";
            this.LocalTransportationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
            this.LocalTransportationCostCalcEdit.TabIndex = 19;
            this.LocalTransportationCostCalcEdit.Text = "0";
            this.LocalTransportationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.LocalTransportationCostCalcEdit.TrackDisposedAccess = true;
            // 
            // PriceUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TotalDeductionAmountCalcEdit);
            this.Controls.Add(this.DiscountAmountCalcEdit);
            this.Controls.Add(this.OtherCostsCalcEdit);
            this.Controls.Add(this.TechnicalCostCalcEdit);
            this.Controls.Add(this.LocalTransportationCostCalcEdit);
            this.Controls.Add(this.TotalAdditionalAmountCalcEdit);
            this.Controls.Add(this.TransportationCostCalcEdit);
            this.Controls.Add(this.InsuranceCalcEdit);
            this.Controls.Add(this.UnloadCostCalcEdit);
            this.Controls.Add(this.FreightCalcEdit);
            this.Controls.Add(this.ExcludingTransportationCostsCalcEdit);
            this.Controls.Add(this.ProfitAmountCalcEdit);
            this.Controls.Add(this.RoyaltyCalcEdit);
            this.Controls.Add(this.ProductDevCostsCalcEdit);
            this.Controls.Add(this.CommodityUsageCostsCalcEdit);
            this.Controls.Add(this.ProductToolCostsCalcEdit);
            this.Controls.Add(this.GoodsCostCalcEdit);
            this.Controls.Add(this.ContainerPackagingCostCalcEdit);
            this.Controls.Add(this.BrokerageFeeCalcEdit);
            this.Controls.Add(this.PurchaseCostCalcEdit);
            this.Controls.Add(this.PaymentAmountCalcEdit);
            this.Controls.Add(this.IndirectAmountCalcEdit);
            this.Controls.Add(this.ExchangeRateCalcEdit);
            this.Controls.Add(this.PaymentAmountConvertToLocalCurrencyControl);
            this.Name = "PriceUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 457, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PaymentAmountConvertToLocalCurrencyControl.ResumeLayout(true);
            this.PaymentAmountConvertToLocalCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal Customs.GUI.ConvertToLocalCurrencyControl PaymentAmountConvertToLocalCurrencyControl;
		internal ZArchitecture.ZCalcEdit ExchangeRateCalcEdit;
		internal ZArchitecture.ZCalcEdit IndirectAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PaymentAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit BrokerageFeeCalcEdit;
		internal ZArchitecture.ZCalcEdit PurchaseCostCalcEdit;
		internal ZArchitecture.ZCalcEdit GoodsCostCalcEdit;
		internal ZArchitecture.ZCalcEdit ContainerPackagingCostCalcEdit;
		internal ZArchitecture.ZCalcEdit RoyaltyCalcEdit;
		internal ZArchitecture.ZCalcEdit ProductDevCostsCalcEdit;
		internal ZArchitecture.ZCalcEdit CommodityUsageCostsCalcEdit;
		internal ZArchitecture.ZCalcEdit ProductToolCostsCalcEdit;
		internal ZArchitecture.ZCalcEdit TransportationCostCalcEdit;
		internal ZArchitecture.ZCalcEdit InsuranceCalcEdit;
		internal ZArchitecture.ZCalcEdit UnloadCostCalcEdit;
		internal ZArchitecture.ZCalcEdit FreightCalcEdit;
		internal ZArchitecture.ZCalcEdit ExcludingTransportationCostsCalcEdit;
		internal ZArchitecture.ZCalcEdit ProfitAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalAdditionalAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalDeductionAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit DiscountAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit OtherCostsCalcEdit;
		internal ZArchitecture.ZCalcEdit TechnicalCostCalcEdit;
		internal ZArchitecture.ZCalcEdit LocalTransportationCostCalcEdit;
	}
}
