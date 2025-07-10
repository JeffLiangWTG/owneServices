using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AddInfoForm
	{
		protected override void InitializeComponent()
		{
			this.oLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.cancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oKBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.wETQuotingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.valuationAdviceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.manualLineProcessingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tariffQuotaSecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tarrifAdviceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.securityNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dumpingExportPriceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.lCTQuotingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.warehouseReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.warehouseReference3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.secondUnitQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.quarantineTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.preferenceInquiryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dumpingSecificationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.invLinesPriceAdjustmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.secondMinisterialTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.secondTariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tariffQuotaNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.creditSecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.rateNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.quotaItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oLabel45 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel46 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel47 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel48 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel54 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel55 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel56 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel57 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel43 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel40 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel42 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel27 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel22 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel23 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel24 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel25 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel26 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.importCreditNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.warehouseRelatedLineCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.dumpingCountryOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dumpingReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.lCTExemptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.countervailingSecurityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.countervailingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dumpingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dumpingRageOfExchangeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dumpingSecurityAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.interimCountervailingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.interimDumpingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.invoiceSpiritStrengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.luxuryCarTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.otherDutyFactorCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.secondQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.securityConcessionOverrideCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.standardDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.warehouseReference2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.warehouseUnitValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.wineEqualisationTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.preferenceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.wETExemptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.gSTExemptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.originCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.amberLineProcessingBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.effectiveDutyDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.transportInsuranceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 472, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUAddInfo);
			// 
			// oLabel1
			// 
			this.oLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.oLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 432, true);
			this.oLabel1.Name = "oLabel1";
			this.oLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.oLabel1.TabIndex = 1;
			this.oLabel1.Text = "Add Info:";
			// 
			// CancelBoundButton
			// 
			this.cancelBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(647, 432, true);
			this.cancelBoundButton.Name = "CancelBoundButton";
			this.cancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelBoundButton.TabIndex = 52;
			this.cancelBoundButton.Text = "Cancel";
			this.cancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// OKBoundButton
			// 
			this.oKBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKBoundButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 432, true);
			this.oKBoundButton.Name = "OKBoundButton";
			this.oKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.oKBoundButton.TabIndex = 51;
			this.oKBoundButton.Text = "OK";
			this.oKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// AddInfoTextBox
			// 
			this.addInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.addInfoTextBox, "AddInfoLineForAddInfoForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).AddInfoLineForAddInfoForm)));
			this.addInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 432, true);
			this.addInfoTextBox.Name = "AddInfoTextBox";
			this.addInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 20, true);
			this.addInfoTextBox.TabIndex = 50;
			// 
			// WETQuotingTextBox
			// 
			this.BindingSource.SetBindingMember(this.wETQuotingTextBox, "ZA_WETQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WETQ)));
			this.wETQuotingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 344, true);
			this.wETQuotingTextBox.Name = "WETQuotingTextBox";
			this.wETQuotingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.wETQuotingTextBox.TabIndex = 48;
			// 
			// ValuationAdviceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.valuationAdviceNumberTextBox, "ZA_VAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_VAN)));
			this.valuationAdviceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 176, true);
			this.valuationAdviceNumberTextBox.Name = "ValuationAdviceNumberTextBox";
			this.valuationAdviceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.valuationAdviceNumberTextBox.TabIndex = 41;
			// 
			// ManualLineProcessingTextBox
			// 
			this.BindingSource.SetBindingMember(this.manualLineProcessingTextBox, "ZA_MLP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_MLP)));
			this.manualLineProcessingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 128, true);
			this.manualLineProcessingTextBox.Name = "ManualLineProcessingTextBox";
			this.manualLineProcessingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.manualLineProcessingTextBox.TabIndex = 22;
			// 
			// TariffQuotaSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.tariffQuotaSecurityCodeTextBox, "ZA_QSC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_QSC)));
			this.tariffQuotaSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 128, true);
			this.tariffQuotaSecurityCodeTextBox.Name = "TariffQuotaSecurityCodeTextBox";
			this.tariffQuotaSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.tariffQuotaSecurityCodeTextBox.TabIndex = 39;
			// 
			// TarrifAdviceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.tarrifAdviceNumberTextBox, "ZA_TAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_TAN)));
			this.tarrifAdviceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 80, true);
			this.tarrifAdviceNumberTextBox.Name = "TarrifAdviceNumberTextBox";
			this.tarrifAdviceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.tarrifAdviceNumberTextBox.TabIndex = 37;
			// 
			// SecurityNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.securityNumberTextBox, "ZA_SCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_SCN)));
			this.securityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 32, true);
			this.securityNumberTextBox.Name = "SecurityNumberTextBox";
			this.securityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.securityNumberTextBox.TabIndex = 35;
			// 
			// DumpingExportPriceTextBox
			// 
			this.BindingSource.SetBindingMember(this.dumpingExportPriceTextBox, "ZA_DXP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DXP)));
			this.dumpingExportPriceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 224, true);
			this.dumpingExportPriceTextBox.Name = "DumpingExportPriceTextBox";
			this.dumpingExportPriceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dumpingExportPriceTextBox.TabIndex = 9;
			// 
			// LCTQuotingTextBox
			// 
			this.BindingSource.SetBindingMember(this.lCTQuotingTextBox, "ZA_LCTQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_LCTQ)));
			this.lCTQuotingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 80, true);
			this.lCTQuotingTextBox.Name = "LCTQuotingTextBox";
			this.lCTQuotingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.lCTQuotingTextBox.TabIndex = 20;
			// 
			// WarehouseReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.warehouseReferenceTextBox, "ZA_WRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WRN)));
			this.warehouseReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 224, true);
			this.warehouseReferenceTextBox.Name = "WarehouseReferenceTextBox";
			this.warehouseReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.warehouseReferenceTextBox.TabIndex = 43;
			// 
			// WarehouseReference3TextBox
			// 
			this.BindingSource.SetBindingMember(this.warehouseReference3TextBox, "ZA_WRU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WRU)));
			this.warehouseReference3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 272, true);
			this.warehouseReference3TextBox.Name = "WarehouseReference3TextBox";
			this.warehouseReference3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.warehouseReference3TextBox.TabIndex = 45;
			// 
			// SecondUnitQuantityTextBox
			// 
			this.BindingSource.SetBindingMember(this.secondUnitQuantityTextBox, "ZA_UQ2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_UQ2)));
			this.secondUnitQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 392, true);
			this.secondUnitQuantityTextBox.Name = "SecondUnitQuantityTextBox";
			this.secondUnitQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.secondUnitQuantityTextBox.TabIndex = 33;
			// 
			// QuarantineTextBox
			// 
			this.BindingSource.SetBindingMember(this.quarantineTextBox, "ZA_AQIS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_AQIS)));
			this.quarantineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 248, true);
			this.quarantineTextBox.Name = "QuarantineTextBox";
			this.quarantineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.quarantineTextBox.TabIndex = 27;
			// 
			// PreferenceInquiryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.preferenceInquiryNumberTextBox, "ZA_PIQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_PIQ)));
			this.preferenceInquiryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 224, true);
			this.preferenceInquiryNumberTextBox.Name = "PreferenceInquiryNumberTextBox";
			this.preferenceInquiryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.preferenceInquiryNumberTextBox.TabIndex = 26;
			// 
			// DumpingSecificationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.dumpingSecificationNumberTextBox, "ZA_DSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DSN)));
			this.dumpingSecificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 200, true);
			this.dumpingSecificationNumberTextBox.Name = "DumpingSecificationNumberTextBox";
			this.dumpingSecificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dumpingSecificationNumberTextBox.TabIndex = 8;
			// 
			// InvLinesPriceAdjustmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.invLinesPriceAdjustmentTextBox, "ZA_ADJ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ADJ)));
			this.invLinesPriceAdjustmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 8, true);
			this.invLinesPriceAdjustmentTextBox.Name = "InvLinesPriceAdjustmentTextBox";
			this.invLinesPriceAdjustmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.invLinesPriceAdjustmentTextBox.TabIndex = 17;
			// 
			// SecondMinisterialTextBox
			// 
			this.BindingSource.SetBindingMember(this.secondMinisterialTextBox, "ZA_MD2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_MD2)));
			this.secondMinisterialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 320, true);
			this.secondMinisterialTextBox.Name = "SecondMinisterialTextBox";
			this.secondMinisterialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.secondMinisterialTextBox.TabIndex = 30;
			// 
			// SecondTariffTextBox
			// 
			this.BindingSource.SetBindingMember(this.secondTariffTextBox, "ZA_TC2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_TC2)));
			this.secondTariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 344, true);
			this.secondTariffTextBox.Name = "SecondTariffTextBox";
			this.secondTariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.secondTariffTextBox.TabIndex = 31;
			// 
			// TariffQuotaNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.tariffQuotaNumberTextBox, "ZA_TFQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_TFQ)));
			this.tariffQuotaNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 104, true);
			this.tariffQuotaNumberTextBox.Name = "TariffQuotaNumberTextBox";
			this.tariffQuotaNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.tariffQuotaNumberTextBox.TabIndex = 38;
			// 
			// CreditSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.creditSecurityCodeTextBox, "ZA_CSC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_CSC)));
			this.creditSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 344, true);
			this.creditSecurityCodeTextBox.Name = "CreditSecurityCodeTextBox";
			this.creditSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.creditSecurityCodeTextBox.TabIndex = 14;
			// 
			// RateNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.rateNumberTextBox, "ZA_RNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_RNO)));
			this.rateNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 296, true);
			this.rateNumberTextBox.Name = "RateNumberTextBox";
			this.rateNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.rateNumberTextBox.TabIndex = 29;
			// 
			// QuotaItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.quotaItemNumberTextBox, "ZA_QIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_QIN)));
			this.quotaItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 272, true);
			this.quotaItemNumberTextBox.Name = "QuotaItemNumberTextBox";
			this.quotaItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.quotaItemNumberTextBox.TabIndex = 28;
			// 
			// oLabel45
			// 
			this.oLabel45.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 104, true);
			this.oLabel45.Name = "oLabel45";
			this.oLabel45.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.oLabel45.TabIndex = 164;
			this.oLabel45.Text = "LCT Exemptions:";
			// 
			// oLabel46
			// 
			this.oLabel46.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 296, true);
			this.oLabel46.Name = "oLabel46";
			this.oLabel46.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel46.TabIndex = 163;
			this.oLabel46.Text = "GST Exemption:";
			// 
			// oLabel47
			// 
			this.oLabel47.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 224, true);
			this.oLabel47.Name = "oLabel47";
			this.oLabel47.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.oLabel47.TabIndex = 162;
			this.oLabel47.Text = "Warehouse Reference Number:";
			// 
			// oLabel48
			// 
			this.oLabel48.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 296, true);
			this.oLabel48.Name = "oLabel48";
			this.oLabel48.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.oLabel48.TabIndex = 161;
			this.oLabel48.Text = "Warehouse Unit Value:";
			// 
			// oLabel49
			// 
			this.oLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 272, true);
			this.oLabel49.Name = "oLabel49";
			this.oLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.oLabel49.TabIndex = 160;
			this.oLabel49.Text = "Warehouse Reference Unit of Quantity:";
			// 
			// oLabel53
			// 
			this.oLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 344, true);
			this.oLabel53.Name = "oLabel53";
			this.oLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.oLabel53.TabIndex = 158;
			this.oLabel53.Text = "WET Quoting:";
			// 
			// oLabel54
			// 
			this.oLabel54.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 368, true);
			this.oLabel54.Name = "oLabel54";
			this.oLabel54.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.oLabel54.TabIndex = 157;
			this.oLabel54.Text = "WET Exemptions:";
			// 
			// oLabel55
			// 
			this.oLabel55.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 152, true);
			this.oLabel55.Name = "oLabel55";
			this.oLabel55.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.oLabel55.TabIndex = 156;
			this.oLabel55.Text = "Transport and Insurance Line Override:";
			// 
			// oLabel56
			// 
			this.oLabel56.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 80, true);
			this.oLabel56.Name = "oLabel56";
			this.oLabel56.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.oLabel56.TabIndex = 155;
			this.oLabel56.Text = "LCT Quoting:";
			// 
			// oLabel57
			// 
			this.oLabel57.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 56, true);
			this.oLabel57.Name = "oLabel57";
			this.oLabel57.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.oLabel57.TabIndex = 154;
			this.oLabel57.Text = "Luxury Car Tax:";
			// 
			// oLabel43
			// 
			this.oLabel43.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 248, true);
			this.oLabel43.Name = "oLabel43";
			this.oLabel43.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.oLabel43.TabIndex = 153;
			this.oLabel43.Text = "Warehouse Reference Quantity:";
			// 
			// oLabel37
			// 
			this.oLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 248, true);
			this.oLabel37.Name = "oLabel37";
			this.oLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.oLabel37.TabIndex = 150;
			this.oLabel37.Text = "Quarantine:";
			// 
			// oLabel38
			// 
			this.oLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 224, true);
			this.oLabel38.Name = "oLabel38";
			this.oLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.oLabel38.TabIndex = 149;
			this.oLabel38.Text = "Preference Inquiry Number:";
			// 
			// oLabel39
			// 
			this.oLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 176, true);
			this.oLabel39.Name = "oLabel39";
			this.oLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.oLabel39.TabIndex = 148;
			this.oLabel39.Text = "Other Duty Factor:";
			// 
			// oLabel40
			// 
			this.oLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 128, true);
			this.oLabel40.Name = "oLabel40";
			this.oLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel40.TabIndex = 147;
			this.oLabel40.Text = "Manual Line Processing:";
			// 
			// oLabel41
			// 
			this.oLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 32, true);
			this.oLabel41.Name = "oLabel41";
			this.oLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel41.TabIndex = 146;
			this.oLabel41.Text = "Invoice Spirit Strength:";
			// 
			// oLabel42
			// 
			this.oLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 392, true);
			this.oLabel42.Name = "oLabel42";
			this.oLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.oLabel42.TabIndex = 145;
			this.oLabel42.Text = "Interim Dumping Duty:";
			// 
			// oLabel27
			// 
			this.oLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 176, true);
			this.oLabel27.Name = "oLabel27";
			this.oLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.oLabel27.TabIndex = 144;
			this.oLabel27.Text = "Valuation Advice Number:";
			// 
			// oLabel28
			// 
			this.oLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 56, true);
			this.oLabel28.Name = "oLabel28";
			this.oLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel28.TabIndex = 143;
			this.oLabel28.Text = "Standard Duty:";
			// 
			// oLabel29
			// 
			this.oLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 8, true);
			this.oLabel29.Name = "oLabel29";
			this.oLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.oLabel29.TabIndex = 142;
			this.oLabel29.Text = "Security Concession Override:";
			// 
			// oLabel30
			// 
			this.oLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 392, true);
			this.oLabel30.Name = "oLabel30";
			this.oLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel30.TabIndex = 141;
			this.oLabel30.Text = "Second Unit Quantity:";
			// 
			// oLabel31
			// 
			this.oLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 368, true);
			this.oLabel31.Name = "oLabel31";
			this.oLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel31.TabIndex = 140;
			this.oLabel31.Text = "Second Quantity:";
			// 
			// oLabel19
			// 
			this.oLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 368, true);
			this.oLabel19.Name = "oLabel19";
			this.oLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.oLabel19.TabIndex = 136;
			this.oLabel19.Text = "Interim Countervailing Duty:";
			// 
			// oLabel20
			// 
			this.oLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 344, true);
			this.oLabel20.Name = "oLabel20";
			this.oLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.oLabel20.TabIndex = 135;
			this.oLabel20.Text = "Credit Security Code:";
			// 
			// oLabel21
			// 
			this.oLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 248, true);
			this.oLabel21.Name = "oLabel21";
			this.oLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.oLabel21.TabIndex = 134;
			this.oLabel21.Text = "Duty:";
			// 
			// oLabel22
			// 
			this.oLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 128, true);
			this.oLabel22.Name = "oLabel22";
			this.oLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.oLabel22.TabIndex = 133;
			this.oLabel22.Text = "Dumping Rate of Exchange:";
			// 
			// oLabel23
			// 
			this.oLabel23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.oLabel23.Name = "oLabel23";
			this.oLabel23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
			this.oLabel23.TabIndex = 132;
			this.oLabel23.Text = "Dumping Duty:";
			// 
			// oLabel24
			// 
			this.oLabel24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.oLabel24.Name = "oLabel24";
			this.oLabel24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
			this.oLabel24.TabIndex = 131;
			this.oLabel24.Text = "Dumping Country/Region of Export:";
			// 
			// oLabel25
			// 
			this.oLabel25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.oLabel25.Name = "oLabel25";
			this.oLabel25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.oLabel25.TabIndex = 130;
			this.oLabel25.Text = "Countervailing Duty:";
			// 
			// oLabel26
			// 
			this.oLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.oLabel26.Name = "oLabel26";
			this.oLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
			this.oLabel26.TabIndex = 129;
			this.oLabel26.Text = "Countervailing Security Amount:";
			// 
			// oLabel12
			// 
			this.oLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 296, true);
			this.oLabel12.Name = "oLabel12";
			this.oLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.oLabel12.TabIndex = 127;
			this.oLabel12.Text = "Rate Number:";
			// 
			// oLabel13
			// 
			this.oLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 272, true);
			this.oLabel13.Name = "oLabel13";
			this.oLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.oLabel13.TabIndex = 126;
			this.oLabel13.Text = "Quota Item Number:";
			// 
			// oLabel14
			// 
			this.oLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 128, true);
			this.oLabel14.Name = "oLabel14";
			this.oLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.oLabel14.TabIndex = 125;
			this.oLabel14.Text = "Tariff Quota Security Code:";
			// 
			// oLabel15
			// 
			this.oLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 80, true);
			this.oLabel15.Name = "oLabel15";
			this.oLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel15.TabIndex = 124;
			this.oLabel15.Text = "Tariff Advice Number:";
			// 
			// oLabel16
			// 
			this.oLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 32, true);
			this.oLabel16.Name = "oLabel16";
			this.oLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel16.TabIndex = 123;
			this.oLabel16.Text = "Security Number";
			// 
			// oLabel17
			// 
			this.oLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.oLabel17.Name = "oLabel17";
			this.oLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.oLabel17.TabIndex = 122;
			this.oLabel17.Text = "Dumping Export Price:";
			// 
			// oLabel18
			// 
			this.oLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.oLabel18.Name = "oLabel18";
			this.oLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.oLabel18.TabIndex = 121;
			this.oLabel18.Text = "Dumping Security Amount:";
			// 
			// oLabel10
			// 
			this.oLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.oLabel10.Name = "oLabel10";
			this.oLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
			this.oLabel10.TabIndex = 120;
			this.oLabel10.Text = "Dumping Specification Number:";
			// 
			// oLabel9
			// 
			this.oLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 152, true);
			this.oLabel9.Name = "oLabel9";
			this.oLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.oLabel9.TabIndex = 119;
			this.oLabel9.Text = "Dumping Reason Code:";
			// 
			// oLabel7
			// 
			this.oLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 200, true);
			this.oLabel7.Name = "oLabel7";
			this.oLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.oLabel7.TabIndex = 117;
			this.oLabel7.Text = "Preference Indicator:";
			// 
			// oLabel6
			// 
			this.oLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 152, true);
			this.oLabel6.Name = "oLabel6";
			this.oLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.oLabel6.TabIndex = 116;
			this.oLabel6.Text = "Origin:";
			// 
			// oLabel5
			// 
			this.oLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			this.oLabel5.Name = "oLabel5";
			this.oLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.oLabel5.TabIndex = 115;
			this.oLabel5.Text = "Inv. Lines Price Adjustment:";
			// 
			// oLabel4
			// 
			this.oLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 320, true);
			this.oLabel4.Name = "oLabel4";
			this.oLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.oLabel4.TabIndex = 114;
			this.oLabel4.Text = "Second Ministerial Determination:";
			// 
			// oLabel3
			// 
			this.oLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 344, true);
			this.oLabel3.Name = "oLabel3";
			this.oLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.oLabel3.TabIndex = 113;
			this.oLabel3.Text = "Second Tariff Concession Order:";
			// 
			// oLabel2
			// 
			this.oLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 104, true);
			this.oLabel2.Name = "oLabel2";
			this.oLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.oLabel2.TabIndex = 112;
			this.oLabel2.Text = "Tariff Quota Number";
			// 
			// ImportCreditNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.importCreditNumberTextBox, "ZA_ICN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ICN)));
			this.importCreditNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 320, true);
			this.importCreditNumberTextBox.Name = "ImportCreditNumberTextBox";
			this.importCreditNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.importCreditNumberTextBox.TabIndex = 13;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 320, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.zLabel1.TabIndex = 167;
			this.zLabel1.Text = "Import Credit Number:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 320, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.zLabel2.TabIndex = 169;
			this.zLabel2.Text = "Wine Equalisation Tax:";
			// 
			// WarehouseRelatedLineCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.warehouseRelatedLineCalcEdit, "ZA_WRL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WRL)));
			this.warehouseRelatedLineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 200, true);
			this.warehouseRelatedLineCalcEdit.Name = "WarehouseRelatedLineCalcEdit";
			this.warehouseRelatedLineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.warehouseRelatedLineCalcEdit.TabIndex = 42;
			this.warehouseRelatedLineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 200, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.zLabel3.TabIndex = 171;
			this.zLabel3.Text = "Warehouse Related Line:";
			// 
			// DumpingCountryOfExportDropEdit
			// 
			this.BindingSource.SetBindingMember(this.dumpingCountryOfExportDropEdit, "ZA_DCX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DCX)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DCX_List)));
			this.dumpingCountryOfExportDropEdit.BindToList = "ZA_DCX_List";
			this.dumpingCountryOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 80, true);
			this.dumpingCountryOfExportDropEdit.Name = "DumpingCountryOfExportDropEdit";
			this.dumpingCountryOfExportDropEdit.PreBoundMaxLength = 4;
			this.dumpingCountryOfExportDropEdit.ShowDescriptionBox = false;
			this.dumpingCountryOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.dumpingCountryOfExportDropEdit.TabIndex = 3;
			// 
			// DumpingReasonCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.dumpingReasonCodeDropEdit, "ZA_DRC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DRC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DRC_List)));
			this.dumpingReasonCodeDropEdit.BindToList = "ZA_DRC_List";
			this.dumpingReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 152, true);
			this.dumpingReasonCodeDropEdit.Name = "DumpingReasonCodeDropEdit";
			this.dumpingReasonCodeDropEdit.PreBoundMaxLength = 4;
			this.dumpingReasonCodeDropEdit.ShowDescriptionBox = false;
			this.dumpingReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.dumpingReasonCodeDropEdit.TabIndex = 6;
			// 
			// LCTExemptionsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.lCTExemptionsDropEdit, "ZA_LCTE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_LCTE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).Lookups.CMRLCTEList)));
			this.lCTExemptionsDropEdit.BindToList = "Lookups.CMRLCTEList";
			this.lCTExemptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 104, true);
			this.lCTExemptionsDropEdit.Name = "LCTExemptionsDropEdit";
			this.lCTExemptionsDropEdit.PreBoundMaxLength = 4;
			this.lCTExemptionsDropEdit.ShowDescriptionBox = false;
			this.lCTExemptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.lCTExemptionsDropEdit.TabIndex = 21;
			// 
			// CountervailingSecurityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.countervailingSecurityCalcEdit, "ZA_CSA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_CSA)));
			this.countervailingSecurityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 32, true);
			this.countervailingSecurityCalcEdit.Name = "CountervailingSecurityCalcEdit";
			this.countervailingSecurityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.countervailingSecurityCalcEdit.TabIndex = 1;
			this.countervailingSecurityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountervailingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.countervailingDutyCalcEdit, "ZA_CVD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_CVD)));
			this.countervailingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 56, true);
			this.countervailingDutyCalcEdit.Name = "CountervailingDutyCalcEdit";
			this.countervailingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.countervailingDutyCalcEdit.TabIndex = 2;
			this.countervailingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DumpingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.dumpingDutyCalcEdit, "ZA_DMP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DMP)));
			this.dumpingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 104, true);
			this.dumpingDutyCalcEdit.Name = "DumpingDutyCalcEdit";
			this.dumpingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dumpingDutyCalcEdit.TabIndex = 4;
			this.dumpingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DumpingRageOfExchangeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.dumpingRageOfExchangeCalcEdit, "ZA_DRE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DRE)));
			this.dumpingRageOfExchangeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 128, true);
			this.dumpingRageOfExchangeCalcEdit.Name = "DumpingRageOfExchangeCalcEdit";
			this.dumpingRageOfExchangeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dumpingRageOfExchangeCalcEdit.TabIndex = 5;
			this.dumpingRageOfExchangeCalcEdit.Text = "0.0000";
			this.dumpingRageOfExchangeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DumpingSecurityAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.dumpingSecurityAmountCalcEdit, "ZA_DSA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DSA)));
			this.dumpingSecurityAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 176, true);
			this.dumpingSecurityAmountCalcEdit.Name = "DumpingSecurityAmountCalcEdit";
			this.dumpingSecurityAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dumpingSecurityAmountCalcEdit.TabIndex = 7;
			this.dumpingSecurityAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.dutyCalcEdit, "ZA_DTY");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_DTY)));
			this.dutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 248, true);
			this.dutyCalcEdit.Name = "DutyCalcEdit";
			this.dutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.dutyCalcEdit.TabIndex = 10;
			this.dutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InterimCountervailingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.interimCountervailingDutyCalcEdit, "ZA_ICV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ICV)));
			this.interimCountervailingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 368, true);
			this.interimCountervailingDutyCalcEdit.Name = "InterimCountervailingDutyCalcEdit";
			this.interimCountervailingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.interimCountervailingDutyCalcEdit.TabIndex = 15;
			this.interimCountervailingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InterimDumpingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.interimDumpingDutyCalcEdit, "ZA_IDP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_IDP)));
			this.interimDumpingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 392, true);
			this.interimDumpingDutyCalcEdit.Name = "InterimDumpingDutyCalcEdit";
			this.interimDumpingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.interimDumpingDutyCalcEdit.TabIndex = 16;
			this.interimDumpingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceSpiritStrengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.invoiceSpiritStrengthCalcEdit, "ZA_ISS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ISS)));
			this.invoiceSpiritStrengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 32, true);
			this.invoiceSpiritStrengthCalcEdit.Name = "InvoiceSpiritStrengthCalcEdit";
			this.invoiceSpiritStrengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.invoiceSpiritStrengthCalcEdit.TabIndex = 18;
			this.invoiceSpiritStrengthCalcEdit.Text = "0.0";
			this.invoiceSpiritStrengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LuxuryCarTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.luxuryCarTaxCalcEdit, "ZA_LCT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_LCT)));
			this.luxuryCarTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 56, true);
			this.luxuryCarTaxCalcEdit.Name = "LuxuryCarTaxCalcEdit";
			this.luxuryCarTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.luxuryCarTaxCalcEdit.TabIndex = 19;
			this.luxuryCarTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherDutyFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.otherDutyFactorCalcEdit, "ZA_ODF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ODF)));
			this.otherDutyFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 176, true);
			this.otherDutyFactorCalcEdit.Name = "OtherDutyFactorCalcEdit";
			this.otherDutyFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.otherDutyFactorCalcEdit.TabIndex = 24;
			this.otherDutyFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecondQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.secondQuantityCalcEdit, "ZA_QT2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_QT2)));
			this.secondQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 368, true);
			this.secondQuantityCalcEdit.Name = "SecondQuantityCalcEdit";
			this.secondQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.secondQuantityCalcEdit.TabIndex = 32;
			this.secondQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecurityConcessionOverrideCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.securityConcessionOverrideCalcEdit, "ZA_CON");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_CON)));
			this.securityConcessionOverrideCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 8, true);
			this.securityConcessionOverrideCalcEdit.Name = "SecurityConcessionOverrideCalcEdit";
			this.securityConcessionOverrideCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.securityConcessionOverrideCalcEdit.TabIndex = 34;
			this.securityConcessionOverrideCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.standardDutyCalcEdit, "ZA_STD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_STD)));
			this.standardDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 56, true);
			this.standardDutyCalcEdit.Name = "StandardDutyCalcEdit";
			this.standardDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.standardDutyCalcEdit.TabIndex = 36;
			this.standardDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WarehouseReference2CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.warehouseReference2CalcEdit, "ZA_WRQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WRQ)));
			this.warehouseReference2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 248, true);
			this.warehouseReference2CalcEdit.Name = "WarehouseReference2CalcEdit";
			this.warehouseReference2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.warehouseReference2CalcEdit.TabIndex = 44;
			this.warehouseReference2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WarehouseUnitValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.warehouseUnitValueCalcEdit, "ZA_WUV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WUV)));
			this.warehouseUnitValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 296, true);
			this.warehouseUnitValueCalcEdit.Name = "WarehouseUnitValueCalcEdit";
			this.warehouseUnitValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.warehouseUnitValueCalcEdit.TabIndex = 46;
			this.warehouseUnitValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WineEqualisationTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.wineEqualisationTaxCalcEdit, "ZA_WET");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WET)));
			this.wineEqualisationTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 320, true);
			this.wineEqualisationTaxCalcEdit.Name = "WineEqualisationTaxCalcEdit";
			this.wineEqualisationTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.wineEqualisationTaxCalcEdit.TabIndex = 47;
			this.wineEqualisationTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreferenceIndicatorDropEdit
			// 
			this.BindingSource.SetBindingMember(this.preferenceIndicatorDropEdit, "ZA_PRF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_PRF)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_PRFList)));
			this.preferenceIndicatorDropEdit.BindToList = "ZA_PRFList";
			this.preferenceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 200, true);
			this.preferenceIndicatorDropEdit.Name = "PreferenceIndicatorDropEdit";
			this.preferenceIndicatorDropEdit.PreBoundMaxLength = 4;
			this.preferenceIndicatorDropEdit.ShowDescriptionBox = false;
			this.preferenceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.preferenceIndicatorDropEdit.TabIndex = 25;
			// 
			// WETExemptionsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.wETExemptionsDropEdit, "ZA_WETE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WETE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_WETE_List)));
			this.wETExemptionsDropEdit.BindToList = "ZA_WETE_List";
			this.wETExemptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 370, true);
			this.wETExemptionsDropEdit.Name = "WETExemptionsDropEdit";
			this.wETExemptionsDropEdit.PreBoundMaxLength = 4;
			this.wETExemptionsDropEdit.ShowDescriptionBox = false;
			this.wETExemptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.wETExemptionsDropEdit.TabIndex = 49;
			// 
			// GSTExemptionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.gSTExemptionDropEdit, "ZA_GSTE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_GSTE)));
			this.gSTExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 296, true);
			this.gSTExemptionDropEdit.Name = "GSTExemptionDropEdit";
			this.gSTExemptionDropEdit.PreBoundMaxLength = 4;
			this.gSTExemptionDropEdit.ShowDescriptionBox = false;
			this.gSTExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.gSTExemptionDropEdit.TabIndex = 12;
			// 
			// OriginCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.originCodeFindBox, "ZA_ORG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ORG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_ORG_List)));
			this.originCodeFindBox.BindToList = "ZA_ORG_List";
			this.originCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 152, true);
			this.originCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.originCodeFindBox.Name = "OriginCodeFindBox";
			this.originCodeFindBox.PreBoundMaxLength = 4;
			this.originCodeFindBox.ShowDescriptionBox = false;
			this.originCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.originCodeFindBox.TabIndex = 23;
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 8, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.zLabel4.TabIndex = 172;
			this.zLabel4.Text = "Amber Line Processing:";
			// 
			// AmberLineProcessingBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.amberLineProcessingBoundTextBox, "ZA_AMB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_AMB)));
			this.amberLineProcessingBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 8, true);
			this.amberLineProcessingBoundTextBox.Name = "AmberLineProcessingBoundTextBox";
			this.amberLineProcessingBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.amberLineProcessingBoundTextBox.TabIndex = 0;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 272, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.zLabel5.TabIndex = 173;
			this.zLabel5.Text = "Effective Duty Date:";
			// 
			// EffectiveDutyDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.effectiveDutyDateTextBox, "ZA_EFD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_EFD)));
			this.effectiveDutyDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 272, true);
			this.effectiveDutyDateTextBox.Name = "EffectiveDutyDateTextBox";
			this.effectiveDutyDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.effectiveDutyDateTextBox.TabIndex = 11;
			// 
			// TransportInsuranceTextBox
			// 
			this.BindingSource.SetBindingMember(this.transportInsuranceTextBox, "ZA_TILV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUAddInfo)(null)).ZA_TILV)));
			this.transportInsuranceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 152, true);
			this.transportInsuranceTextBox.Name = "TransportInsuranceTextBox";
			this.transportInsuranceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.transportInsuranceTextBox.TabIndex = 40;
			// 
			// AddInfoForm
			// 
			this.AcceptButton = this.oKBoundButton;
			this.CancelButton = this.cancelBoundButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 494, true);
			this.Controls.Add(this.transportInsuranceTextBox);
			this.Controls.Add(this.effectiveDutyDateTextBox);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.amberLineProcessingBoundTextBox);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.originCodeFindBox);
			this.Controls.Add(this.gSTExemptionDropEdit);
			this.Controls.Add(this.wETExemptionsDropEdit);
			this.Controls.Add(this.preferenceIndicatorDropEdit);
			this.Controls.Add(this.wineEqualisationTaxCalcEdit);
			this.Controls.Add(this.warehouseUnitValueCalcEdit);
			this.Controls.Add(this.warehouseReference2CalcEdit);
			this.Controls.Add(this.standardDutyCalcEdit);
			this.Controls.Add(this.securityConcessionOverrideCalcEdit);
			this.Controls.Add(this.secondQuantityCalcEdit);
			this.Controls.Add(this.otherDutyFactorCalcEdit);
			this.Controls.Add(this.luxuryCarTaxCalcEdit);
			this.Controls.Add(this.invoiceSpiritStrengthCalcEdit);
			this.Controls.Add(this.interimDumpingDutyCalcEdit);
			this.Controls.Add(this.interimCountervailingDutyCalcEdit);
			this.Controls.Add(this.dutyCalcEdit);
			this.Controls.Add(this.dumpingSecurityAmountCalcEdit);
			this.Controls.Add(this.dumpingRageOfExchangeCalcEdit);
			this.Controls.Add(this.dumpingDutyCalcEdit);
			this.Controls.Add(this.countervailingDutyCalcEdit);
			this.Controls.Add(this.countervailingSecurityCalcEdit);
			this.Controls.Add(this.lCTExemptionsDropEdit);
			this.Controls.Add(this.dumpingReasonCodeDropEdit);
			this.Controls.Add(this.dumpingCountryOfExportDropEdit);
			this.Controls.Add(this.warehouseRelatedLineCalcEdit);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.importCreditNumberTextBox);
			this.Controls.Add(this.wETQuotingTextBox);
			this.Controls.Add(this.valuationAdviceNumberTextBox);
			this.Controls.Add(this.manualLineProcessingTextBox);
			this.Controls.Add(this.tariffQuotaSecurityCodeTextBox);
			this.Controls.Add(this.tarrifAdviceNumberTextBox);
			this.Controls.Add(this.securityNumberTextBox);
			this.Controls.Add(this.dumpingExportPriceTextBox);
			this.Controls.Add(this.lCTQuotingTextBox);
			this.Controls.Add(this.warehouseReferenceTextBox);
			this.Controls.Add(this.warehouseReference3TextBox);
			this.Controls.Add(this.secondUnitQuantityTextBox);
			this.Controls.Add(this.quarantineTextBox);
			this.Controls.Add(this.preferenceInquiryNumberTextBox);
			this.Controls.Add(this.dumpingSecificationNumberTextBox);
			this.Controls.Add(this.invLinesPriceAdjustmentTextBox);
			this.Controls.Add(this.secondMinisterialTextBox);
			this.Controls.Add(this.secondTariffTextBox);
			this.Controls.Add(this.tariffQuotaNumberTextBox);
			this.Controls.Add(this.creditSecurityCodeTextBox);
			this.Controls.Add(this.rateNumberTextBox);
			this.Controls.Add(this.quotaItemNumberTextBox);
			this.Controls.Add(this.oLabel45);
			this.Controls.Add(this.oLabel46);
			this.Controls.Add(this.oLabel47);
			this.Controls.Add(this.oLabel48);
			this.Controls.Add(this.oLabel49);
			this.Controls.Add(this.oLabel53);
			this.Controls.Add(this.oLabel54);
			this.Controls.Add(this.oLabel55);
			this.Controls.Add(this.oLabel56);
			this.Controls.Add(this.oLabel57);
			this.Controls.Add(this.oLabel43);
			this.Controls.Add(this.oLabel37);
			this.Controls.Add(this.oLabel38);
			this.Controls.Add(this.oLabel39);
			this.Controls.Add(this.oLabel40);
			this.Controls.Add(this.oLabel41);
			this.Controls.Add(this.oLabel42);
			this.Controls.Add(this.oLabel27);
			this.Controls.Add(this.oLabel28);
			this.Controls.Add(this.oLabel29);
			this.Controls.Add(this.oLabel30);
			this.Controls.Add(this.oLabel31);
			this.Controls.Add(this.oLabel19);
			this.Controls.Add(this.oLabel20);
			this.Controls.Add(this.oLabel21);
			this.Controls.Add(this.oLabel22);
			this.Controls.Add(this.oLabel23);
			this.Controls.Add(this.oLabel24);
			this.Controls.Add(this.oLabel25);
			this.Controls.Add(this.oLabel26);
			this.Controls.Add(this.oLabel12);
			this.Controls.Add(this.oLabel13);
			this.Controls.Add(this.oLabel14);
			this.Controls.Add(this.oLabel15);
			this.Controls.Add(this.oLabel16);
			this.Controls.Add(this.oLabel17);
			this.Controls.Add(this.oLabel18);
			this.Controls.Add(this.oLabel10);
			this.Controls.Add(this.oLabel9);
			this.Controls.Add(this.oLabel7);
			this.Controls.Add(this.oLabel6);
			this.Controls.Add(this.oLabel5);
			this.Controls.Add(this.oLabel4);
			this.Controls.Add(this.oLabel3);
			this.Controls.Add(this.oLabel2);
			this.Controls.Add(this.addInfoTextBox);
			this.Controls.Add(this.oLabel1);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.cancelBoundButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUAddInfo);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AUAddInfo";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 528, true);
			this.Name = "AddInfoForm";
			this.Text = "AddInfoForm";
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.oLabel1, 0);
			this.Controls.SetChildIndex(this.addInfoTextBox, 0);
			this.Controls.SetChildIndex(this.oLabel2, 0);
			this.Controls.SetChildIndex(this.oLabel3, 0);
			this.Controls.SetChildIndex(this.oLabel4, 0);
			this.Controls.SetChildIndex(this.oLabel5, 0);
			this.Controls.SetChildIndex(this.oLabel6, 0);
			this.Controls.SetChildIndex(this.oLabel7, 0);
			this.Controls.SetChildIndex(this.oLabel9, 0);
			this.Controls.SetChildIndex(this.oLabel10, 0);
			this.Controls.SetChildIndex(this.oLabel18, 0);
			this.Controls.SetChildIndex(this.oLabel17, 0);
			this.Controls.SetChildIndex(this.oLabel16, 0);
			this.Controls.SetChildIndex(this.oLabel15, 0);
			this.Controls.SetChildIndex(this.oLabel14, 0);
			this.Controls.SetChildIndex(this.oLabel13, 0);
			this.Controls.SetChildIndex(this.oLabel12, 0);
			this.Controls.SetChildIndex(this.oLabel26, 0);
			this.Controls.SetChildIndex(this.oLabel25, 0);
			this.Controls.SetChildIndex(this.oLabel24, 0);
			this.Controls.SetChildIndex(this.oLabel23, 0);
			this.Controls.SetChildIndex(this.oLabel22, 0);
			this.Controls.SetChildIndex(this.oLabel21, 0);
			this.Controls.SetChildIndex(this.oLabel20, 0);
			this.Controls.SetChildIndex(this.oLabel19, 0);
			this.Controls.SetChildIndex(this.oLabel31, 0);
			this.Controls.SetChildIndex(this.oLabel30, 0);
			this.Controls.SetChildIndex(this.oLabel29, 0);
			this.Controls.SetChildIndex(this.oLabel28, 0);
			this.Controls.SetChildIndex(this.oLabel27, 0);
			this.Controls.SetChildIndex(this.oLabel42, 0);
			this.Controls.SetChildIndex(this.oLabel41, 0);
			this.Controls.SetChildIndex(this.oLabel40, 0);
			this.Controls.SetChildIndex(this.oLabel39, 0);
			this.Controls.SetChildIndex(this.oLabel38, 0);
			this.Controls.SetChildIndex(this.oLabel37, 0);
			this.Controls.SetChildIndex(this.oLabel43, 0);
			this.Controls.SetChildIndex(this.oLabel57, 0);
			this.Controls.SetChildIndex(this.oLabel56, 0);
			this.Controls.SetChildIndex(this.oLabel55, 0);
			this.Controls.SetChildIndex(this.oLabel54, 0);
			this.Controls.SetChildIndex(this.oLabel53, 0);
			this.Controls.SetChildIndex(this.oLabel49, 0);
			this.Controls.SetChildIndex(this.oLabel48, 0);
			this.Controls.SetChildIndex(this.oLabel47, 0);
			this.Controls.SetChildIndex(this.oLabel46, 0);
			this.Controls.SetChildIndex(this.oLabel45, 0);
			this.Controls.SetChildIndex(this.quotaItemNumberTextBox, 0);
			this.Controls.SetChildIndex(this.rateNumberTextBox, 0);
			this.Controls.SetChildIndex(this.creditSecurityCodeTextBox, 0);
			this.Controls.SetChildIndex(this.tariffQuotaNumberTextBox, 0);
			this.Controls.SetChildIndex(this.secondTariffTextBox, 0);
			this.Controls.SetChildIndex(this.secondMinisterialTextBox, 0);
			this.Controls.SetChildIndex(this.invLinesPriceAdjustmentTextBox, 0);
			this.Controls.SetChildIndex(this.dumpingSecificationNumberTextBox, 0);
			this.Controls.SetChildIndex(this.preferenceInquiryNumberTextBox, 0);
			this.Controls.SetChildIndex(this.quarantineTextBox, 0);
			this.Controls.SetChildIndex(this.secondUnitQuantityTextBox, 0);
			this.Controls.SetChildIndex(this.warehouseReference3TextBox, 0);
			this.Controls.SetChildIndex(this.warehouseReferenceTextBox, 0);
			this.Controls.SetChildIndex(this.lCTQuotingTextBox, 0);
			this.Controls.SetChildIndex(this.dumpingExportPriceTextBox, 0);
			this.Controls.SetChildIndex(this.securityNumberTextBox, 0);
			this.Controls.SetChildIndex(this.tarrifAdviceNumberTextBox, 0);
			this.Controls.SetChildIndex(this.tariffQuotaSecurityCodeTextBox, 0);
			this.Controls.SetChildIndex(this.manualLineProcessingTextBox, 0);
			this.Controls.SetChildIndex(this.valuationAdviceNumberTextBox, 0);
			this.Controls.SetChildIndex(this.wETQuotingTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.importCreditNumberTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.warehouseRelatedLineCalcEdit, 0);
			this.Controls.SetChildIndex(this.dumpingCountryOfExportDropEdit, 0);
			this.Controls.SetChildIndex(this.dumpingReasonCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.lCTExemptionsDropEdit, 0);
			this.Controls.SetChildIndex(this.countervailingSecurityCalcEdit, 0);
			this.Controls.SetChildIndex(this.countervailingDutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.dumpingDutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.dumpingRageOfExchangeCalcEdit, 0);
			this.Controls.SetChildIndex(this.dumpingSecurityAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.dutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.interimCountervailingDutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.interimDumpingDutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.invoiceSpiritStrengthCalcEdit, 0);
			this.Controls.SetChildIndex(this.luxuryCarTaxCalcEdit, 0);
			this.Controls.SetChildIndex(this.otherDutyFactorCalcEdit, 0);
			this.Controls.SetChildIndex(this.secondQuantityCalcEdit, 0);
			this.Controls.SetChildIndex(this.securityConcessionOverrideCalcEdit, 0);
			this.Controls.SetChildIndex(this.standardDutyCalcEdit, 0);
			this.Controls.SetChildIndex(this.warehouseReference2CalcEdit, 0);
			this.Controls.SetChildIndex(this.warehouseUnitValueCalcEdit, 0);
			this.Controls.SetChildIndex(this.wineEqualisationTaxCalcEdit, 0);
			this.Controls.SetChildIndex(this.preferenceIndicatorDropEdit, 0);
			this.Controls.SetChildIndex(this.wETExemptionsDropEdit, 0);
			this.Controls.SetChildIndex(this.gSTExemptionDropEdit, 0);
			this.Controls.SetChildIndex(this.originCodeFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.amberLineProcessingBoundTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.effectiveDutyDateTextBox, 0);
			this.Controls.SetChildIndex(this.transportInsuranceTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.ZLabel oLabel1;
		private ZButton oKBoundButton;
		private ZArchitecture.ZLabel oLabel45;
		private ZArchitecture.ZLabel oLabel46;
		private ZArchitecture.ZLabel oLabel47;
		private ZArchitecture.ZLabel oLabel48;
		private ZArchitecture.ZLabel oLabel49;
		private ZArchitecture.ZLabel oLabel53;
		private ZArchitecture.ZLabel oLabel54;
		private ZArchitecture.ZLabel oLabel55;
		private ZArchitecture.ZLabel oLabel56;
		private ZArchitecture.ZLabel oLabel57;
		private ZArchitecture.ZLabel oLabel43;
		private ZArchitecture.ZLabel oLabel37;
		private ZArchitecture.ZLabel oLabel38;
		private ZArchitecture.ZLabel oLabel39;
		private ZArchitecture.ZLabel oLabel40;
		private ZArchitecture.ZLabel oLabel41;
		private ZArchitecture.ZLabel oLabel42;
		private ZArchitecture.ZLabel oLabel27;
		private ZArchitecture.ZLabel oLabel28;
		private ZArchitecture.ZLabel oLabel29;
		private ZArchitecture.ZLabel oLabel30;
		private ZArchitecture.ZLabel oLabel31;
		private ZArchitecture.ZLabel oLabel19;
		private ZArchitecture.ZLabel oLabel20;
		private ZArchitecture.ZLabel oLabel21;
		private ZArchitecture.ZLabel oLabel22;
		private ZArchitecture.ZLabel oLabel23;
		private ZArchitecture.ZLabel oLabel24;
		private ZArchitecture.ZLabel oLabel25;
		private ZArchitecture.ZLabel oLabel26;
		private ZArchitecture.ZLabel oLabel12;
		private ZArchitecture.ZLabel oLabel13;
		private ZArchitecture.ZLabel oLabel14;
		private ZArchitecture.ZLabel oLabel15;
		private ZArchitecture.ZLabel oLabel16;
		private ZArchitecture.ZLabel oLabel17;
		private ZArchitecture.ZLabel oLabel18;
		private ZArchitecture.ZLabel oLabel10;
		private ZArchitecture.ZLabel oLabel9;
		private ZArchitecture.ZLabel oLabel7;
		private ZArchitecture.ZLabel oLabel6;
		private ZArchitecture.ZLabel oLabel5;
		private ZArchitecture.ZLabel oLabel4;
		private ZArchitecture.ZLabel oLabel3;
		private ZArchitecture.ZLabel oLabel2;
		private ZArchitecture.ZTextBox invLinesPriceAdjustmentTextBox;
		private ZArchitecture.ZTextBox secondMinisterialTextBox;
		private ZArchitecture.ZTextBox secondTariffTextBox;
		private ZArchitecture.ZTextBox tariffQuotaNumberTextBox;
		private ZButton cancelBoundButton;
		private ZArchitecture.ZTextBox addInfoTextBox;
		private ZArchitecture.ZTextBox wETQuotingTextBox;
		private ZArchitecture.ZTextBox valuationAdviceNumberTextBox;
		private ZArchitecture.ZTextBox manualLineProcessingTextBox;
		private ZArchitecture.ZTextBox tariffQuotaSecurityCodeTextBox;
		private ZArchitecture.ZTextBox tarrifAdviceNumberTextBox;
		private ZArchitecture.ZTextBox securityNumberTextBox;
		private ZArchitecture.ZTextBox dumpingExportPriceTextBox;
		private ZArchitecture.ZTextBox lCTQuotingTextBox;
		private ZArchitecture.ZTextBox warehouseReferenceTextBox;
		private ZArchitecture.ZTextBox warehouseReference3TextBox;
		private ZArchitecture.ZTextBox secondUnitQuantityTextBox;
		private ZArchitecture.ZTextBox quarantineTextBox;
		private ZArchitecture.ZTextBox preferenceInquiryNumberTextBox;
		private ZArchitecture.ZTextBox dumpingSecificationNumberTextBox;
		private ZArchitecture.ZTextBox creditSecurityCodeTextBox;
		private ZArchitecture.ZTextBox rateNumberTextBox;
		private ZArchitecture.ZTextBox quotaItemNumberTextBox;
		private ZArchitecture.ZTextBox importCreditNumberTextBox;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZCalcEdit warehouseRelatedLineCalcEdit;
		private ZArchitecture.ZLabel zLabel3;
		private ZDropEdit dumpingCountryOfExportDropEdit;
		private ZDropEdit dumpingReasonCodeDropEdit;
		private ZDropEdit lCTExemptionsDropEdit;
		private ZArchitecture.ZCalcEdit countervailingSecurityCalcEdit;
		private ZArchitecture.ZCalcEdit countervailingDutyCalcEdit;
		private ZArchitecture.ZCalcEdit dumpingDutyCalcEdit;
		private ZArchitecture.ZCalcEdit dumpingRageOfExchangeCalcEdit;
		private ZArchitecture.ZCalcEdit dumpingSecurityAmountCalcEdit;
		private ZArchitecture.ZCalcEdit dutyCalcEdit;
		private ZArchitecture.ZCalcEdit interimCountervailingDutyCalcEdit;
		private ZArchitecture.ZCalcEdit interimDumpingDutyCalcEdit;
		private ZArchitecture.ZCalcEdit invoiceSpiritStrengthCalcEdit;
		private ZArchitecture.ZCalcEdit luxuryCarTaxCalcEdit;
		private ZArchitecture.ZCalcEdit otherDutyFactorCalcEdit;
		private ZArchitecture.ZCalcEdit secondQuantityCalcEdit;
		private ZArchitecture.ZCalcEdit securityConcessionOverrideCalcEdit;
		private ZArchitecture.ZCalcEdit standardDutyCalcEdit;
		private ZArchitecture.ZCalcEdit warehouseReference2CalcEdit;
		private ZArchitecture.ZCalcEdit warehouseUnitValueCalcEdit;
		private ZArchitecture.ZCalcEdit wineEqualisationTaxCalcEdit;
		private ZDropEdit preferenceIndicatorDropEdit;
		private ZDropEdit wETExemptionsDropEdit;
		private ZDropEdit gSTExemptionDropEdit;
		private ZCodeFindBox originCodeFindBox;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZTextBox amberLineProcessingBoundTextBox;
		private ZArchitecture.ZLabel zLabel5;
		private ZArchitecture.ZTextBox effectiveDutyDateTextBox;
		private ZArchitecture.ZTextBox transportInsuranceTextBox;
	}
}
