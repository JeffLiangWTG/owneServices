namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRMainUserControl
	{

		private void InitializeComponent()
		{
			this.MailDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JPH_VesselNameFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RN_NKCountryOfRegFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RadioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_OperationalCarrierVoyageNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_VesselDetailsChangedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JPH_DischargePortSuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_OverrideFreightDefaultsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JPH_GB_BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CarrierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JPH_CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_RL_NKLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_LoadingPortSuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_RelaxedAppIdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JPH_RL_NKDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_ETDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JPH_ETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JPH_MasterBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JPH_BillMessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_BillMessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_BillReleaseStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_BillReleaseStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_BillRegistrationStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MailDetailsGroupBox.SuspendLayout();
			this.JPH_VesselNameFindBox.SuspendLayout();
			this.JPH_RN_NKCountryOfRegFindBox.SuspendLayout();
			this.JPH_GB_BranchGuidFindBox.SuspendLayout();
			this.CarrierDocAddressControl.SuspendLayout();
			this.JPH_RL_NKLoadingCodeFindBox.SuspendLayout();
			this.JPH_RL_NKDischargeCodeFindBox.SuspendLayout();
			this.JPH_ETDDateEdit.SuspendLayout();
			this.JPH_ETADateEdit.SuspendLayout();
			this.StatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// MailDetailsGroupBox
			// 
			this.MailDetailsGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("93551a0a-1e1e-47d9-838b-e02a2edc1afa", "Manifest Details");
			this.MailDetailsGroupBox.Controls.Add(this.JPH_VesselNameFindBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_RN_NKCountryOfRegFindBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_RadioCallSignTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_OperationalCarrierVoyageNoTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_VesselDetailsChangedCheckBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_DischargePortSuffixTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_OverrideFreightDefaultsCheckBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_GB_BranchGuidFindBox);
			this.MailDetailsGroupBox.Controls.Add(this.CarrierDocAddressControl);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_CarrierCodeTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_VoyageTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_RL_NKLoadingCodeFindBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_LoadingPortSuffixTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_RelaxedAppIdCheckBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_RL_NKDischargeCodeFindBox);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_ETDDateEdit);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_ETADateEdit);
			this.MailDetailsGroupBox.Controls.Add(this.JPH_MasterBillNumberTextBox);
			this.MailDetailsGroupBox.Controls.Add(this.StatusGroupBox);
			this.MailDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MailDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MailDetailsGroupBox.Name = "MailDetailsGroupBox";
			this.MailDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 438, true);
			this.MailDetailsGroupBox.TabIndex = 0;
			this.MailDetailsGroupBox.TabStop = false;
			// 
			// JPH_VesselNameFindBox
			// 
			this.JPH_VesselNameFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_VesselNameFindBox, "JPH_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_VesselName)));
			this.JPH_VesselNameFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("688ed773-43ec-4214-9849-61761d7a10bf", "Vessel Name");
			this.JPH_VesselNameFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 97, true);
			this.JPH_VesselNameFindBox.Name = "JPH_VesselNameFindBox";
			this.JPH_VesselNameFindBox.PreBoundMaxLength = 23;
			this.JPH_VesselNameFindBox.ShouldResize = true;
			this.JPH_VesselNameFindBox.ShowDescriptionBox = false;
			this.JPH_VesselNameFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.JPH_VesselNameFindBox.TabIndex = 4;
			// 
			// JPH_RN_NKCountryOfRegFindBox
			// 
			this.JPH_RN_NKCountryOfRegFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RN_NKCountryOfRegFindBox, "JPH_RN_NKCountryOfReg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RN_NKCountryOfReg)));
			this.JPH_RN_NKCountryOfRegFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("2b7efff1-1c5e-4ae8-a074-5443615f2726", "Nationality");
			this.JPH_RN_NKCountryOfRegFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 123, true);
			this.JPH_RN_NKCountryOfRegFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.JPH_RN_NKCountryOfRegFindBox.Name = "JPH_RN_NKCountryOfRegFindBox";
			this.JPH_RN_NKCountryOfRegFindBox.PreBoundMaxLength = 3;
			this.JPH_RN_NKCountryOfRegFindBox.ShouldResize = true;
			this.JPH_RN_NKCountryOfRegFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.JPH_RN_NKCountryOfRegFindBox.TabIndex = 6;
			// 
			// JPH_RadioCallSignTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_RadioCallSignTextBox, "JPH_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RadioCallSign)));
			this.JPH_RadioCallSignTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("13ecb200-73c5-4c29-8647-86c3e20acc79", "Call Sign");
			this.JPH_RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 123, true);
			this.JPH_RadioCallSignTextBox.Name = "JPH_RadioCallSignTextBox";
			this.JPH_RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_RadioCallSignTextBox.TabIndex = 5;
			// 
			// JPH_OperationalCarrierVoyageNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_OperationalCarrierVoyageNoTextBox, "JPH_OperationalCarrierVoyageNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_OperationalCarrierVoyageNo)));
			this.JPH_OperationalCarrierVoyageNoTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c9ab6392-a8c5-4e4f-bde6-7c56e7b729c6", "Operator Voyage", "Operator Voyage No.", "Operator Carrier Voyage No.", "Operating Carrier Voyage Number");
			this.JPH_OperationalCarrierVoyageNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 175, true);
			this.JPH_OperationalCarrierVoyageNoTextBox.Name = "JPH_OperationalCarrierVoyageNoTextBox";
			this.JPH_OperationalCarrierVoyageNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_OperationalCarrierVoyageNoTextBox.TabIndex = 9;
			// 
			// JPH_VesselDetailsChangedCheckBox
			// 
			this.JPH_VesselDetailsChangedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JPH_VesselDetailsChangedCheckBox, "JPH_VesselDetailsChanged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_VesselDetailsChanged)));
			this.JPH_VesselDetailsChangedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JPH_VesselDetailsChangedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JPH_VesselDetailsChangedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 157, true);
			this.JPH_VesselDetailsChangedCheckBox.Name = "JPH_VesselDetailsChangedCheckBox";
			this.JPH_VesselDetailsChangedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JPH_VesselDetailsChangedCheckBox.TabIndex = 8;
			this.JPH_VesselDetailsChangedCheckBox.UseVisualStyleBackColor = true;
			// 
			// JPH_DischargePortSuffixTextBox
			// 
			this.JPH_DischargePortSuffixTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_DischargePortSuffixTextBox, "JPH_DischargePortSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_DischargePortSuffix)));
			this.JPH_DischargePortSuffixTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4ff511e6-2093-45bb-9108-8a8291aebfdf", "Suffix");
			this.JPH_DischargePortSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 226, true);
			this.JPH_DischargePortSuffixTextBox.Name = "JPH_DischargePortSuffixTextBox";
			this.JPH_DischargePortSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.JPH_DischargePortSuffixTextBox.TabIndex = 14;
			this.JPH_DischargePortSuffixTextBox.Visible = false;
			// 
			// JPH_OverrideFreightDefaultsCheckBox
			// 
			this.JPH_OverrideFreightDefaultsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JPH_OverrideFreightDefaultsCheckBox, "JPH_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_OverrideFreightDefaults)));
			this.JPH_OverrideFreightDefaultsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JPH_OverrideFreightDefaultsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JPH_OverrideFreightDefaultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, -1, true);
			this.JPH_OverrideFreightDefaultsCheckBox.Name = "JPH_OverrideFreightDefaultsCheckBox";
			this.JPH_OverrideFreightDefaultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JPH_OverrideFreightDefaultsCheckBox.TabIndex = 0;
			this.JPH_OverrideFreightDefaultsCheckBox.UseVisualStyleBackColor = true;
			// 
			// JPH_GB_BranchGuidFindBox
			// 
			this.JPH_GB_BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_GB_BranchGuidFindBox, "JPH_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_GB_Branch)));
			this.JPH_GB_BranchGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.JPH_GB_BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 19, true);
			this.JPH_GB_BranchGuidFindBox.Name = "JPH_GB_BranchGuidFindBox";
			this.JPH_GB_BranchGuidFindBox.ShouldResize = true;
			this.JPH_GB_BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.JPH_GB_BranchGuidFindBox.TabIndex = 1;
			// 
			// CarrierDocAddressControl
			// 
			this.CarrierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierDocAddressControl, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).Carrier)));
			this.CarrierDocAddressControl.BindToOrganisations = "Lookups+Carriers";
			this.CarrierDocAddressControl.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("3da29dfd-8c77-4575-b9f2-6331bb3089f7", "Carrier");
			this.CarrierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 45, true);
			this.CarrierDocAddressControl.Name = "CarrierDocAddressControl";
			this.CarrierDocAddressControl.ReadOnly = false;
			this.CarrierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.CarrierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.CarrierDocAddressControl.TabIndex = 2;
			this.CarrierDocAddressControl.ValidationJustForced = false;
			// 
			// JPH_CarrierCodeTextBox
			// 
			this.JPH_CarrierCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_CarrierCodeTextBox, "JPH_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_CarrierCode)));
			this.JPH_CarrierCodeTextBox.CaptionResourceString = null;
			this.JPH_CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 71, true);
			this.JPH_CarrierCodeTextBox.Name = "JPH_CarrierCodeTextBox";
			this.JPH_CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_CarrierCodeTextBox.TabIndex = 3;
			// 
			// JPH_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_VoyageTextBox, "JPH_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_Voyage)));
			this.JPH_VoyageTextBox.CaptionResourceString = null;
			this.JPH_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 151, true);
			this.JPH_VoyageTextBox.Name = "JPH_VoyageTextBox";
			this.JPH_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_VoyageTextBox.TabIndex = 7;
			// 
			// JPH_RL_NKLoadingCodeFindBox
			// 
			this.JPH_RL_NKLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RL_NKLoadingCodeFindBox, "JPH_RL_NKLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RL_NKLoading)));
			this.JPH_RL_NKLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 200, true);
			this.JPH_RL_NKLoadingCodeFindBox.Name = "JPH_RL_NKLoadingCodeFindBox";
			this.JPH_RL_NKLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.JPH_RL_NKLoadingCodeFindBox.ShouldResize = true;
			this.JPH_RL_NKLoadingCodeFindBox.ShowDescriptionBox = false;
			this.JPH_RL_NKLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JPH_RL_NKLoadingCodeFindBox.TabIndex = 10;
			// 
			// JPH_LoadingPortSuffixTextBox
			// 
			this.JPH_LoadingPortSuffixTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_LoadingPortSuffixTextBox, "JPH_LoadingPortSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_LoadingPortSuffix)));
			this.JPH_LoadingPortSuffixTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("9215ad6b-ad50-4d7f-a2b3-7c1650f479df", "Suffix");
			this.JPH_LoadingPortSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 200, true);
			this.JPH_LoadingPortSuffixTextBox.Name = "JPH_LoadingPortSuffixTextBox";
			this.JPH_LoadingPortSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.JPH_LoadingPortSuffixTextBox.TabIndex = 11;
			// 
			// JPH_RelaxedAppIdCheckBox
			// 
			this.JPH_RelaxedAppIdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JPH_RelaxedAppIdCheckBox, "JPH_RelaxedAppId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RelaxedAppId)));
			this.JPH_RelaxedAppIdCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("440c97bb-a7d1-4e6c-a9be-3f5e1626ee1b", "Relaxed Area? ");
			this.JPH_RelaxedAppIdCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JPH_RelaxedAppIdCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JPH_RelaxedAppIdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 203, true);
			this.JPH_RelaxedAppIdCheckBox.Name = "JPH_RelaxedAppIdCheckBox";
			this.JPH_RelaxedAppIdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JPH_RelaxedAppIdCheckBox.TabIndex = 12;
			this.JPH_RelaxedAppIdCheckBox.UseVisualStyleBackColor = true;
			// 
			// JPH_RL_NKDischargeCodeFindBox
			// 
			this.JPH_RL_NKDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RL_NKDischargeCodeFindBox, "JPH_RL_NKDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RL_NKDischarge)));
			this.JPH_RL_NKDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 226, true);
			this.JPH_RL_NKDischargeCodeFindBox.Name = "JPH_RL_NKDischargeCodeFindBox";
			this.JPH_RL_NKDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.JPH_RL_NKDischargeCodeFindBox.ShouldResize = true;
			this.JPH_RL_NKDischargeCodeFindBox.ShowDescriptionBox = false;
			this.JPH_RL_NKDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JPH_RL_NKDischargeCodeFindBox.TabIndex = 13;
			// 
			// JPH_ETDDateEdit
			// 
			this.JPH_ETDDateEdit.AllowDrop = true;
			this.JPH_ETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.JPH_ETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JPH_ETDDateEdit, "JPH_ETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_ETD)));
			this.JPH_ETDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JPH_ETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 252, true);
			this.JPH_ETDDateEdit.Name = "JPH_ETDDateEdit";
			this.JPH_ETDDateEdit.TabIndex = 15;
			// 
			// JPH_ETADateEdit
			// 
			this.JPH_ETADateEdit.AllowDrop = true;
			this.JPH_ETADateEdit.AutoCompleteMonthThreshold = 1;
			this.JPH_ETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JPH_ETADateEdit, "JPH_ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_ETA)));
			this.JPH_ETADateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JPH_ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 278, true);
			this.JPH_ETADateEdit.Name = "JPH_ETADateEdit";
			this.JPH_ETADateEdit.TabIndex = 16;
			// 
			// JPH_MasterBillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_MasterBillNumberTextBox, "JPH_MasterBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_MasterBillNumber)));
			this.JPH_MasterBillNumberTextBox.CaptionResourceString = null;
			this.JPH_MasterBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 304, true);
			this.JPH_MasterBillNumberTextBox.Name = "JPH_MasterBillNumberTextBox";
			this.JPH_MasterBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.JPH_MasterBillNumberTextBox.TabIndex = 17;
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("e7e12818-8eda-4804-9069-83b06fdc4a16", "Statuses");
			this.StatusGroupBox.Controls.Add(this.JPH_BillMessageStatusDescriptionTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_BillMessageStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_BillReleaseStatusDescriptionTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_BillReleaseStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_MessageStatusDescriptionTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_MessageStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.JPH_BillRegistrationStatusTextBox);
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 19, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 135, true);
			this.StatusGroupBox.TabIndex = 18;
			this.StatusGroupBox.TabStop = false;
			// 
			// JPH_BillMessageStatusDescriptionTextBox
			// 
			this.JPH_BillMessageStatusDescriptionTextBox.AllowDrop = true;
			this.JPH_BillMessageStatusDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JPH_BillMessageStatusDescriptionTextBox, "JPH_BillMessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_BillMessageStatusDescription)));
			this.JPH_BillMessageStatusDescriptionTextBox.CaptionResourceString = null;
			this.JPH_BillMessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 104, true);
			this.JPH_BillMessageStatusDescriptionTextBox.Name = "JPH_BillMessageStatusDescriptionTextBox";
			this.JPH_BillMessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.JPH_BillMessageStatusDescriptionTextBox.TabIndex = 6;
			// 
			// JPH_BillMessageStatusTextBox
			// 
			this.JPH_BillMessageStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_BillMessageStatusTextBox, "JPH_BillMessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_BillMessageStatus)));
			this.JPH_BillMessageStatusTextBox.CaptionResourceString = null;
			this.JPH_BillMessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 104, true);
			this.JPH_BillMessageStatusTextBox.Name = "JPH_BillMessageStatusTextBox";
			this.JPH_BillMessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.JPH_BillMessageStatusTextBox.TabIndex = 5;
			// 
			// JPH_BillReleaseStatusDescriptionTextBox
			// 
			this.JPH_BillReleaseStatusDescriptionTextBox.AllowDrop = true;
			this.JPH_BillReleaseStatusDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JPH_BillReleaseStatusDescriptionTextBox, "JPH_BillReleaseStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_BillReleaseStatusDescription)));
			this.JPH_BillReleaseStatusDescriptionTextBox.CaptionResourceString = null;
			this.JPH_BillReleaseStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 78, true);
			this.JPH_BillReleaseStatusDescriptionTextBox.Name = "JPH_BillReleaseStatusDescriptionTextBox";
			this.JPH_BillReleaseStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.JPH_BillReleaseStatusDescriptionTextBox.TabIndex = 4;
			// 
			// JPH_BillReleaseStatusTextBox
			// 
			this.JPH_BillReleaseStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_BillReleaseStatusTextBox, "JPH_BillReleaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_BillReleaseStatus)));
			this.JPH_BillReleaseStatusTextBox.CaptionResourceString = null;
			this.JPH_BillReleaseStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 78, true);
			this.JPH_BillReleaseStatusTextBox.Name = "JPH_BillReleaseStatusTextBox";
			this.JPH_BillReleaseStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.JPH_BillReleaseStatusTextBox.TabIndex = 3;
			// 
			// JPH_MessageStatusDescriptionTextBox
			// 
			this.JPH_MessageStatusDescriptionTextBox.AllowDrop = true;
			this.JPH_MessageStatusDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JPH_MessageStatusDescriptionTextBox, "JPH_MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_MessageStatusDescription)));
			this.JPH_MessageStatusDescriptionTextBox.CaptionResourceString = null;
			this.JPH_MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 52, true);
			this.JPH_MessageStatusDescriptionTextBox.Name = "JPH_MessageStatusDescriptionTextBox";
			this.JPH_MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.JPH_MessageStatusDescriptionTextBox.TabIndex = 2;
			// 
			// JPH_MessageStatusTextBox
			// 
			this.JPH_MessageStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_MessageStatusTextBox, "JPH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_MessageStatus)));
			this.JPH_MessageStatusTextBox.CaptionResourceString = null;
			this.JPH_MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 52, true);
			this.JPH_MessageStatusTextBox.Name = "JPH_MessageStatusTextBox";
			this.JPH_MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.JPH_MessageStatusTextBox.TabIndex = 1;
			// 
			// JPH_BillRegistrationStatusTextBox
			// 
			this.JPH_BillRegistrationStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_BillRegistrationStatusTextBox, "JPH_BillRegistrationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_BillRegistrationStatus)));
			this.JPH_BillRegistrationStatusTextBox.CaptionResourceString = null;
			this.JPH_BillRegistrationStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 26, true);
			this.JPH_BillRegistrationStatusTextBox.Name = "JPH_BillRegistrationStatusTextBox";
			this.JPH_BillRegistrationStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.JPH_BillRegistrationStatusTextBox.TabIndex = 0;
			// 
			// JPAFRMainUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MailDetailsGroupBox);
			this.Name = "JPAFRMainUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 438, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MailDetailsGroupBox.ResumeLayout(false);
			this.MailDetailsGroupBox.PerformLayout();
			this.JPH_VesselNameFindBox.ResumeLayout(true);
			this.JPH_VesselNameFindBox.PerformLayout();
			this.JPH_RN_NKCountryOfRegFindBox.ResumeLayout(true);
			this.JPH_RN_NKCountryOfRegFindBox.PerformLayout();
			this.JPH_GB_BranchGuidFindBox.ResumeLayout(true);
			this.JPH_GB_BranchGuidFindBox.PerformLayout();
			this.CarrierDocAddressControl.ResumeLayout(true);
			this.CarrierDocAddressControl.PerformLayout();
			this.JPH_RL_NKLoadingCodeFindBox.ResumeLayout(true);
			this.JPH_RL_NKLoadingCodeFindBox.PerformLayout();
			this.JPH_RL_NKDischargeCodeFindBox.ResumeLayout(true);
			this.JPH_RL_NKDischargeCodeFindBox.PerformLayout();
			this.JPH_ETDDateEdit.ResumeLayout(true);
			this.JPH_ETDDateEdit.PerformLayout();
			this.JPH_ETADateEdit.ResumeLayout(true);
			this.JPH_ETADateEdit.PerformLayout();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZGroupBox MailDetailsGroupBox;
		private ZArchitecture.ZTextBox JPH_VoyageTextBox;
		private ZArchitecture.GUI.ZGuidFindBox JPH_GB_BranchGuidFindBox;
		private ZArchitecture.ZTextBox JPH_CarrierCodeTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RL_NKDischargeCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit JPH_ETADateEdit;
		private ZArchitecture.GUI.ZCheckBox JPH_RelaxedAppIdCheckBox;
		private ZArchitecture.GUI.ZDateEdit JPH_ETDDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RL_NKLoadingCodeFindBox;
		private ZArchitecture.ZTextBox JPH_LoadingPortSuffixTextBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl CarrierDocAddressControl;
		internal ZArchitecture.GUI.ZCheckBox JPH_OverrideFreightDefaultsCheckBox;
		private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private ZArchitecture.ZTextBox JPH_BillMessageStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox JPH_BillMessageStatusTextBox;
		private ZArchitecture.ZTextBox JPH_BillReleaseStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox JPH_BillReleaseStatusTextBox;
		private ZArchitecture.ZTextBox JPH_MessageStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox JPH_MessageStatusTextBox;
		private ZArchitecture.ZTextBox JPH_BillRegistrationStatusTextBox;
		internal ZArchitecture.ZTextBox JPH_MasterBillNumberTextBox;
		internal ZArchitecture.ZTextBox JPH_DischargePortSuffixTextBox;
		private ZArchitecture.GUI.ZCheckBox JPH_VesselDetailsChangedCheckBox;
		private ZArchitecture.ZTextBox JPH_OperationalCarrierVoyageNoTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_VesselNameFindBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RN_NKCountryOfRegFindBox;
		private ZArchitecture.ZTextBox JPH_RadioCallSignTextBox;
	}
}
