namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRVesselInformationUserControl
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

			JPH_VesselNameFindBox.PopupSelected -= JPH_VesselNameFindBox_PopupSelected;
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.VesselInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JPH_VesselNameFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RN_NKCountryOfRegFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RadioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_VesselDetailsChangedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarrierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JPH_CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JPH_RL_NKLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_LoadingPortSuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JPH_RelaxedAppIdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JPH_ETDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JPH_RL_NKDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_ETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselInformationGroupBox.SuspendLayout();
			this.JPH_VesselNameFindBox.SuspendLayout();
			this.JPH_RN_NKCountryOfRegFindBox.SuspendLayout();
			this.CarrierDocAddressControl.SuspendLayout();
			this.JPH_RL_NKLoadingCodeFindBox.SuspendLayout();
			this.MasterInformationGroupBox.SuspendLayout();
			this.JPH_ETDDateEdit.SuspendLayout();
			this.JPH_RL_NKDischargeCodeFindBox.SuspendLayout();
			this.JPH_ETADateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// VesselInformationGroupBox
			// 
			this.VesselInformationGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("2de7d2f9-b6b9-4366-b1e0-07e3cd18e3e9", "Vessel Information");
			this.VesselInformationGroupBox.Controls.Add(this.JPH_VesselNameFindBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_RN_NKCountryOfRegFindBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_RadioCallSignTextBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_VesselDetailsChangedCheckBox);
			this.VesselInformationGroupBox.Controls.Add(this.CarrierDocAddressControl);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_CarrierCodeTextBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_VoyageTextBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_RL_NKLoadingCodeFindBox);
			this.VesselInformationGroupBox.Controls.Add(this.JPH_LoadingPortSuffixTextBox);
			this.VesselInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.VesselInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselInformationGroupBox.Name = "VesselInformationGroupBox";
			this.VesselInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 182, true);
			this.VesselInformationGroupBox.TabIndex = 0;
			this.VesselInformationGroupBox.TabStop = false;
			// 
			// JPH_VesselNameFindBox
			// 
			this.JPH_VesselNameFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_VesselNameFindBox, "JPH_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_VesselName)));
			this.JPH_VesselNameFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 76, true);
			this.JPH_VesselNameFindBox.Name = "JPH_VesselNameFindBox";
			this.JPH_VesselNameFindBox.PreBoundMaxLength = 23;
			this.JPH_VesselNameFindBox.ShouldResize = true;
			this.JPH_VesselNameFindBox.ShowDescriptionBox = false;
			this.JPH_VesselNameFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.JPH_VesselNameFindBox.TabIndex = 2;
			// 
			// JPH_RN_NKCountryOfRegFindBox
			// 
			this.JPH_RN_NKCountryOfRegFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RN_NKCountryOfRegFindBox, "JPH_RN_NKCountryOfReg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RN_NKCountryOfReg)));
			this.JPH_RN_NKCountryOfRegFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("01604239-ceeb-4dab-81e5-17267712be26", "Nationality");
			this.JPH_RN_NKCountryOfRegFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 102, true);
			this.JPH_RN_NKCountryOfRegFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.JPH_RN_NKCountryOfRegFindBox.Name = "JPH_RN_NKCountryOfRegFindBox";
			this.JPH_RN_NKCountryOfRegFindBox.PreBoundMaxLength = 3;
			this.JPH_RN_NKCountryOfRegFindBox.ShouldResize = true;
			this.JPH_RN_NKCountryOfRegFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.JPH_RN_NKCountryOfRegFindBox.TabIndex = 4;
			// 
			// JPH_RadioCallSignTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_RadioCallSignTextBox, "JPH_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RadioCallSign)));
			this.JPH_RadioCallSignTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("90ab6556-87a4-4a0b-b78b-25042eea893b", "Call Sign");
			this.JPH_RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 102, true);
			this.JPH_RadioCallSignTextBox.Name = "JPH_RadioCallSignTextBox";
			this.JPH_RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_RadioCallSignTextBox.TabIndex = 3;
			// 
			// JPH_VesselDetailsChangedCheckBox
			// 
			this.JPH_VesselDetailsChangedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JPH_VesselDetailsChangedCheckBox, "JPH_VesselDetailsChanged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_VesselDetailsChanged)));
			this.JPH_VesselDetailsChangedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JPH_VesselDetailsChangedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JPH_VesselDetailsChangedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 134, true);
			this.JPH_VesselDetailsChangedCheckBox.Name = "JPH_VesselDetailsChangedCheckBox";
			this.JPH_VesselDetailsChangedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JPH_VesselDetailsChangedCheckBox.TabIndex = 6;
			this.JPH_VesselDetailsChangedCheckBox.UseVisualStyleBackColor = true;
			// 
			// CarrierDocAddressControl
			// 
			this.CarrierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierDocAddressControl, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).Carrier)));
			this.CarrierDocAddressControl.BindToOrganisations = "Header+Lookups+Carriers";
			this.CarrierDocAddressControl.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("bb43f3b5-aa97-4696-9b4e-3d9a050fc9b8", "Carrier");
			this.CarrierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 24, true);
			this.CarrierDocAddressControl.Name = "CarrierDocAddressControl";
			this.CarrierDocAddressControl.ReadOnly = false;
			this.CarrierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.CarrierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.CarrierDocAddressControl.TabIndex = 0;
			this.CarrierDocAddressControl.ValidationJustForced = false;
			// 
			// JPH_CarrierCodeTextBox
			// 
			this.JPH_CarrierCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_CarrierCodeTextBox, "JPH_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_CarrierCode)));
			this.JPH_CarrierCodeTextBox.CaptionResourceString = null;
			this.JPH_CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 50, true);
			this.JPH_CarrierCodeTextBox.Name = "JPH_CarrierCodeTextBox";
			this.JPH_CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_CarrierCodeTextBox.TabIndex = 1;
			// 
			// JPH_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_VoyageTextBox, "JPH_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_Voyage)));
			this.JPH_VoyageTextBox.CaptionResourceString = null;
			this.JPH_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 128, true);
			this.JPH_VoyageTextBox.Name = "JPH_VoyageTextBox";
			this.JPH_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_VoyageTextBox.TabIndex = 5;
			// 
			// JPH_RL_NKLoadingCodeFindBox
			// 
			this.JPH_RL_NKLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RL_NKLoadingCodeFindBox, "JPH_RL_NKLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RL_NKLoading)));
			this.JPH_RL_NKLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 154, true);
			this.JPH_RL_NKLoadingCodeFindBox.Name = "JPH_RL_NKLoadingCodeFindBox";
			this.JPH_RL_NKLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.JPH_RL_NKLoadingCodeFindBox.ShouldResize = true;
			this.JPH_RL_NKLoadingCodeFindBox.ShowDescriptionBox = false;
			this.JPH_RL_NKLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JPH_RL_NKLoadingCodeFindBox.TabIndex = 7;
			// 
			// JPH_LoadingPortSuffixTextBox
			// 
			this.JPH_LoadingPortSuffixTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_LoadingPortSuffixTextBox, "JPH_LoadingPortSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_LoadingPortSuffix)));
			this.JPH_LoadingPortSuffixTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("a7a01964-f630-43d4-95c2-69917030cc24", "Suffix");
			this.JPH_LoadingPortSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 152, true);
			this.JPH_LoadingPortSuffixTextBox.Name = "JPH_LoadingPortSuffixTextBox";
			this.JPH_LoadingPortSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.JPH_LoadingPortSuffixTextBox.TabIndex = 8;
			// 
			// MasterInformationGroupBox
			// 
			this.MasterInformationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MasterInformationGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("fb122aa5-b7fe-44d9-9ff6-4e9e2fc2d5e5", "Master Information");
			this.MasterInformationGroupBox.Controls.Add(this.JPH_RelaxedAppIdCheckBox);
			this.MasterInformationGroupBox.Controls.Add(this.JPH_ETDDateEdit);
			this.MasterInformationGroupBox.Controls.Add(this.JPH_RL_NKDischargeCodeFindBox);
			this.MasterInformationGroupBox.Controls.Add(this.JPH_ETADateEdit);
			this.MasterInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 0, true);
			this.MasterInformationGroupBox.Name = "MasterInformationGroupBox";
			this.MasterInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 180, true);
			this.MasterInformationGroupBox.TabIndex = 1;
			this.MasterInformationGroupBox.TabStop = false;
			// 
			// JPH_RelaxedAppIdCheckBox
			// 
			this.JPH_RelaxedAppIdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JPH_RelaxedAppIdCheckBox, "JPH_RelaxedAppId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RelaxedAppId)));
			this.JPH_RelaxedAppIdCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JPH_RelaxedAppIdCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JPH_RelaxedAppIdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 19, true);
			this.JPH_RelaxedAppIdCheckBox.Name = "JPH_RelaxedAppIdCheckBox";
			this.JPH_RelaxedAppIdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JPH_RelaxedAppIdCheckBox.TabIndex = 9;
			this.JPH_RelaxedAppIdCheckBox.UseVisualStyleBackColor = true;
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
			this.JPH_ETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 65, true);
			this.JPH_ETDDateEdit.Name = "JPH_ETDDateEdit";
			this.JPH_ETDDateEdit.TabIndex = 11;
			// 
			// JPH_RL_NKDischargeCodeFindBox
			// 
			this.JPH_RL_NKDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RL_NKDischargeCodeFindBox, "JPH_RL_NKDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_RL_NKDischarge)));
			this.JPH_RL_NKDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 39, true);
			this.JPH_RL_NKDischargeCodeFindBox.Name = "JPH_RL_NKDischargeCodeFindBox";
			this.JPH_RL_NKDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.JPH_RL_NKDischargeCodeFindBox.ShouldResize = true;
			this.JPH_RL_NKDischargeCodeFindBox.ShowDescriptionBox = false;
			this.JPH_RL_NKDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JPH_RL_NKDischargeCodeFindBox.TabIndex = 10;
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
			this.JPH_ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 91, true);
			this.JPH_ETADateEdit.Name = "JPH_ETADateEdit";
			this.JPH_ETADateEdit.TabIndex = 12;
			// 
			// JPAFRVesselInformationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VesselInformationGroupBox);
			this.Controls.Add(this.MasterInformationGroupBox);
			this.Name = "JPAFRVesselInformationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VesselInformationGroupBox.ResumeLayout(false);
			this.VesselInformationGroupBox.PerformLayout();
			this.JPH_VesselNameFindBox.ResumeLayout(true);
			this.JPH_VesselNameFindBox.PerformLayout();
			this.JPH_RN_NKCountryOfRegFindBox.ResumeLayout(true);
			this.JPH_RN_NKCountryOfRegFindBox.PerformLayout();
			this.CarrierDocAddressControl.ResumeLayout(true);
			this.CarrierDocAddressControl.PerformLayout();
			this.JPH_RL_NKLoadingCodeFindBox.ResumeLayout(true);
			this.JPH_RL_NKLoadingCodeFindBox.PerformLayout();
			this.MasterInformationGroupBox.ResumeLayout(false);
			this.MasterInformationGroupBox.PerformLayout();
			this.JPH_ETDDateEdit.ResumeLayout(true);
			this.JPH_ETDDateEdit.PerformLayout();
			this.JPH_RL_NKDischargeCodeFindBox.ResumeLayout(true);
			this.JPH_RL_NKDischargeCodeFindBox.PerformLayout();
			this.JPH_ETADateEdit.ResumeLayout(true);
			this.JPH_ETADateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox VesselInformationGroupBox;
		private MasterFiles.GUI.ZDocAddressControl CarrierDocAddressControl;
		private ZArchitecture.ZTextBox JPH_CarrierCodeTextBox;
		private ZArchitecture.ZTextBox JPH_VoyageTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RL_NKLoadingCodeFindBox;
		private ZArchitecture.ZTextBox JPH_LoadingPortSuffixTextBox;
		private ZArchitecture.GUI.ZGroupBox MasterInformationGroupBox;
		private ZArchitecture.GUI.ZCheckBox JPH_RelaxedAppIdCheckBox;
		private ZArchitecture.GUI.ZDateEdit JPH_ETDDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RL_NKDischargeCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit JPH_ETADateEdit;
		private ZArchitecture.GUI.ZCheckBox JPH_VesselDetailsChangedCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_VesselNameFindBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RN_NKCountryOfRegFindBox;
		private ZArchitecture.ZTextBox JPH_RadioCallSignTextBox;
	}
}
