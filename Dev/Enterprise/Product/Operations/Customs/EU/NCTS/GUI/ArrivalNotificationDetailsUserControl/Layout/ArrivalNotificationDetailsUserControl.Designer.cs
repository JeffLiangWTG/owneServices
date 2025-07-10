using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ArrivalNotificationDetailsUserControl
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
			this.LocationOfGoodsUserControl = new LocationOfGoodsUserControl();
			this.CarnetTotalPagesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DischargeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DestinationCustomsOfficeCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationTraderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.AuthorizationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncidentFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OwnerZGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ArrivalDateDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.OverrideFreightDetailsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransportMeansLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommunicationLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportAtArrivalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportAtArrivalIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StateOfSealsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NationalInfoSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.GoodsLocationFromAuthorizationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.CarnetTotalPagesDropEdit.SuspendLayout();
			this.DischargeTypeDropEdit.SuspendLayout();
			this.DestinationCustomsOfficeCodeCodeFindBox.SuspendLayout();
			this.DestinationTraderDocAddressControl.SuspendLayout();
			this.AuthorizationCodeDropEdit.SuspendLayout();
			this.IncidentFlagDropEdit.SuspendLayout();
			this.NumberCodeFindBox.SuspendLayout();
			this.OwnerZGuidFindBox.SuspendLayout();
			this.ArrivalDateDateTimeOffsetEdit.SuspendLayout();
			this.CommunicationLanguageDropEdit.SuspendLayout();
			this.TransportAtArrivalTypeDropEdit.SuspendLayout();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.StateOfSealsDropEdit.SuspendLayout();
			this.NationalInfoSeparatorUserControl.SuspendLayout();
			this.GoodsLocationFromAuthorizationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			//
			// LocationOfGoodsUserControl
			//
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, "ArrivalMovementHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader)));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 276, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.LocationOfGoodsUserControl.TabIndex = 0;
			//
			// CarnetTotalPagesDropEdit
			//
			this.CarnetTotalPagesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarnetTotalPagesDropEdit, "ArrivalMovementHeader.BM_CarnetTotalPages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_CarnetTotalPages)));
			this.CarnetTotalPagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 17, true);
			this.CarnetTotalPagesDropEdit.Name = "CarnetTotalPagesDropEdit";
			this.CarnetTotalPagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CarnetTotalPagesDropEdit.TabIndex = 8;
			//
			// DischargeTypeDropEdit
			//
			this.DischargeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeTypeDropEdit, "ArrivalMovementHeader.BM_DischargeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_DischargeType)));
			this.DischargeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 146, true);
			this.DischargeTypeDropEdit.Name = "DischargeTypeDropEdit";
			this.DischargeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DischargeTypeDropEdit.TabIndex = 7;
			// 
			// NationalInfoSeparatorUserControl
			// 
			this.NationalInfoSeparatorUserControl.AllowDrop = true;
			this.NationalInfoSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("30ABA0C5-C9C6-4947-8BDB-3EBC08587A43", "National Info");
			this.NationalInfoSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 503, true);
			this.NationalInfoSeparatorUserControl.Name = "NationalInfoSeparatorUserControl";
			this.NationalInfoSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 15, true);
			this.NationalInfoSeparatorUserControl.TabIndex = 22;
			//
			// DestinationCustomsOfficeCodeCodeFindBox
			//
			this.DestinationCustomsOfficeCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCustomsOfficeCodeCodeFindBox, "ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival)));
			this.DestinationCustomsOfficeCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 172, true);
			this.DestinationCustomsOfficeCodeCodeFindBox.Name = "DestinationCustomsOfficeCodeCodeFindBox";
			this.DestinationCustomsOfficeCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DestinationCustomsOfficeCodeCodeFindBox.ParentType = null;
			this.DestinationCustomsOfficeCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DestinationCustomsOfficeCodeCodeFindBox.TabIndex = 4;
			//
			// MrnTextBox
			//
			this.BindingSource.SetBindingMember(this.MrnTextBox, "ArrivalMrnFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMrnFromUser)));
			this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 71, true);
			this.MrnTextBox.Name = "MrnTextBox";
			this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.MrnTextBox.TabIndex = 3;
			//
			// LocalReferenceNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.LocalReferenceNumberTextBox, "ArrivalMovementHeader.BM_PaperlessInbondNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_PaperlessInbondNum)));
			this.LocalReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 44, true);
			this.LocalReferenceNumberTextBox.Name = "LocalReferenceNumberTextBox";
			this.LocalReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.LocalReferenceNumberTextBox.TabIndex = 4;
			//
			// DestinationTraderDocAddressControl
			//
			this.DestinationTraderDocAddressControl.AddressValidationProcessCmdKey = null;
			this.DestinationTraderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationTraderDocAddressControl, "DestinationTrader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DestinationTrader)));
			this.DestinationTraderDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.DestinationTraderDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("98FAFFD2-D88B-426C-A85B-B296C9F463D9", "Destination Trader");
			this.DestinationTraderDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DestinationTraderDocAddressControl, false);
			this.DestinationTraderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 0, true);
			this.DestinationTraderDocAddressControl.Name = "DestinationTraderDocAddressControl";
			this.DestinationTraderDocAddressControl.ReadOnly = false;
			this.DestinationTraderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DestinationTraderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DestinationTraderDocAddressControl.TabIndex = 9;
			this.DestinationTraderDocAddressControl.ValidationJustForced = false;
			//
			// AuthorizationCodeDropEdit
			//
			this.AuthorizationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationCodeDropEdit, "ArrivalMovementHeader.AuthorizationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AuthorizationCode)));
			this.AuthorizationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 198, true);
			this.AuthorizationCodeDropEdit.Name = "AuthorizationCodeDropEdit";
			this.AuthorizationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.AuthorizationCodeDropEdit.TabIndex = 10;
			//
			// IncidentFlagDropEdit
			//
			this.IncidentFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncidentFlagDropEdit, "BH_ExportFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_ExportFlag)));
			this.IncidentFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 302, true);
			this.IncidentFlagDropEdit.Name = "IncidentFlagDropEdit";
			this.IncidentFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.IncidentFlagDropEdit.TabIndex = 0;
			//
			// NumberCodeFindBox
			//
			this.NumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NumberCodeFindBox, "ArrivalMovementHeader.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AuthorizationNumber)));
			this.NumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 224, true);
			this.NumberCodeFindBox.Name = "NumberCodeFindBox";
			this.NumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			this.NumberCodeFindBox.ParentType = null;
			this.NumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.NumberCodeFindBox.TabIndex = 11;
			//
			// OwnerZGuidFindBox
			//
			this.OwnerZGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerZGuidFindBox, "ArrivalMovementHeader.AuthorizationOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AuthorizationOwner)));
			this.OwnerZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 251, true);
			this.OwnerZGuidFindBox.Name = "OwnerZGuidFindBox";
			this.OwnerZGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OwnerZGuidFindBox.ParentType = null;
			this.OwnerZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.OwnerZGuidFindBox.TabIndex = 12;
			//
			// ArrivalDateDateTimeOffsetEdit
			//
			this.ArrivalDateDateTimeOffsetEdit.AllowDrop = true;
			this.ArrivalDateDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDateDateTimeOffsetEdit, "ArrivalMovementHeader.BM_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_ArrivalDate)));
			this.ArrivalDateDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArrivalDateDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 328, true);
			this.ArrivalDateDateTimeOffsetEdit.Name = "ArrivalDateDateTimeOffsetEdit";
			this.ArrivalDateDateTimeOffsetEdit.TabIndex = 13;
			//
			// OverrideFreightDetailsCheckBox
			//
			this.BindingSource.SetBindingMember(this.OverrideFreightDetailsCheckBox, "BH_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_OverrideFreightDefaults)));
			this.OverrideFreightDetailsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 354, true);
			this.OverrideFreightDetailsCheckBox.Name = "OverrideFreightDetailsCheckBox";
			this.OverrideFreightDetailsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 24, true);
			this.OverrideFreightDetailsCheckBox.TabIndex = 15;
			this.OverrideFreightDetailsCheckBox.UseVisualStyleBackColor = true;
			//
			// TransportMeansLabel
			//
			this.TransportMeansLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4FB10F95-C55E-4484-B483-BBBE6E566097", "Transport Means");
			this.TransportMeansLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TransportMeansLabel.IsFontBold = true;
			this.TransportMeansLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 184, true);
			this.TransportMeansLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.TransportMeansLabel.Name = "TransportMeansLabel";
			this.TransportMeansLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TransportMeansLabel.TabIndex = 16;
			//
			// CommunicationLanguageDropEdit
			//
			this.CommunicationLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommunicationLanguageDropEdit, "BH_CommunicationLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_CommunicationLanguage)));
			this.CommunicationLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 206, true);
			this.CommunicationLanguageDropEdit.Name = "CommunicationLanguageDropEdit";
			this.CommunicationLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.CommunicationLanguageDropEdit.TabIndex = 17;
			//
			// TransportAtArrivalTypeDropEdit
			//
			this.TransportAtArrivalTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportAtArrivalTypeDropEdit, "ArrivalMovementHeader.BM_TransportAtArrivalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_TransportAtArrivalType)));
			this.TransportAtArrivalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 233, true);
			this.TransportAtArrivalTypeDropEdit.Name = "TransportAtArrivalTypeDropEdit";
			this.TransportAtArrivalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.TransportAtArrivalTypeDropEdit.TabIndex = 18;
			//
			// TransportAtArrivalIDTextBox
			//
			this.BindingSource.SetBindingMember(this.TransportAtArrivalIDTextBox, "ArrivalMovementHeader.BM_TransportAtArrivalID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_TransportAtArrivalID)));
			this.TransportAtArrivalIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 257, true);
			this.TransportAtArrivalIDTextBox.Name = "TransportAtArrivalIDTextBox";
			this.TransportAtArrivalIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.TransportAtArrivalIDTextBox.TabIndex = 19;
			//
			// TransportNationalityCodeFindBox
			//
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality)));
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 278, true);
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 20;
			//
			// StateOfSealsDropEdit
			//
			this.StateOfSealsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateOfSealsDropEdit, "ArrivalMovementHeader.BM_StateOfSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_StateOfSeals)));
			this.StateOfSealsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 94, true);
			this.StateOfSealsDropEdit.Name = "StateOfSealsDropEdit";
			this.StateOfSealsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.StateOfSealsDropEdit.TabIndex = 4;
			//
			// AdditionalTextTextBox
			//
			this.BindingSource.SetBindingMember(this.AdditionalTextTextBox, "ArrivalMovementHeader.BM_AdditionalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_AdditionalText)));
			this.AdditionalTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 120, true);
			this.AdditionalTextTextBox.Name = "AdditionalTextTextBox";
			this.AdditionalTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.AdditionalTextTextBox.TabIndex = 5;
			//
			// GoodsLocationFromAuthorizationCodeFindBox
			// 
			this.GoodsLocationFromAuthorizationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationFromAuthorizationCodeFindBox, "ArrivalMovementHeader.AuthorizationLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AuthorizationLocation)));
			this.GoodsLocationFromAuthorizationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 28, true);
			this.GoodsLocationFromAuthorizationCodeFindBox.Name = "GoodsLocationFromAuthorizationCodeFindBox";
			this.GoodsLocationFromAuthorizationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			this.GoodsLocationFromAuthorizationCodeFindBox.ParentType = null;
			this.GoodsLocationFromAuthorizationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 15, true);
			//
			// ArrivalNotificationDetailsUserControl
			//
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OverrideFreightDetailsCheckBox);
			this.Controls.Add(this.ArrivalDateDateTimeOffsetEdit);
			this.Controls.Add(this.OwnerZGuidFindBox);
			this.Controls.Add(this.NumberCodeFindBox);
			this.Controls.Add(this.AuthorizationCodeDropEdit);
			this.Controls.Add(this.DestinationTraderDocAddressControl);
			this.Controls.Add(this.MrnTextBox);
			this.Controls.Add(this.LocalReferenceNumberTextBox);
			this.Controls.Add(this.DestinationCustomsOfficeCodeCodeFindBox);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.DischargeTypeDropEdit);
			this.Controls.Add(this.CarnetTotalPagesDropEdit);
			this.Controls.Add(this.NationalInfoSeparatorUserControl);
			this.Controls.Add(this.IncidentFlagDropEdit);
			this.Controls.Add(this.TransportMeansLabel);
			this.Controls.Add(this.CommunicationLanguageDropEdit);
			this.Controls.Add(this.TransportAtArrivalTypeDropEdit);
			this.Controls.Add(this.TransportAtArrivalIDTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Controls.Add(this.StateOfSealsDropEdit);
			this.Controls.Add(this.AdditionalTextTextBox);
			this.Controls.Add(this.GoodsLocationFromAuthorizationCodeFindBox);
			this.Name = "ArrivalNotificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 391, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.CarnetTotalPagesDropEdit.ResumeLayout(true);
			this.CarnetTotalPagesDropEdit.PerformLayout();
			this.DischargeTypeDropEdit.ResumeLayout(true);
			this.DischargeTypeDropEdit.PerformLayout();
			this.NationalInfoSeparatorUserControl.ResumeLayout(true);
			this.NationalInfoSeparatorUserControl.PerformLayout();
			this.DestinationCustomsOfficeCodeCodeFindBox.ResumeLayout(true);
			this.DestinationCustomsOfficeCodeCodeFindBox.PerformLayout();
			this.DestinationTraderDocAddressControl.ResumeLayout(true);
			this.DestinationTraderDocAddressControl.PerformLayout();
			this.AuthorizationCodeDropEdit.ResumeLayout(true);
			this.AuthorizationCodeDropEdit.PerformLayout();
			this.IncidentFlagDropEdit.ResumeLayout(true);
			this.IncidentFlagDropEdit.PerformLayout();
			this.NumberCodeFindBox.ResumeLayout(true);
			this.NumberCodeFindBox.PerformLayout();
			this.OwnerZGuidFindBox.ResumeLayout(true);
			this.OwnerZGuidFindBox.PerformLayout();
			this.ArrivalDateDateTimeOffsetEdit.ResumeLayout(true);
			this.ArrivalDateDateTimeOffsetEdit.PerformLayout();
			this.CommunicationLanguageDropEdit.ResumeLayout(true);
			this.CommunicationLanguageDropEdit.PerformLayout();
			this.TransportAtArrivalTypeDropEdit.ResumeLayout(true);
			this.TransportAtArrivalTypeDropEdit.PerformLayout();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.StateOfSealsDropEdit.ResumeLayout(true);
			this.StateOfSealsDropEdit.PerformLayout();
			this.GoodsLocationFromAuthorizationCodeFindBox.ResumeLayout(true);
			this.GoodsLocationFromAuthorizationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox MrnTextBox;
		public ZArchitecture.ZTextBox LocalReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit IncidentFlagDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CarnetTotalPagesDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DischargeTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationCustomsOfficeCodeCodeFindBox;
		internal MasterFiles.GUI.ZDocAddressControl DestinationTraderDocAddressControl;
		internal ZArchitecture.GUI.ZDropEdit AuthorizationCodeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox NumberCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox OwnerZGuidFindBox;
		internal LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal ZArchitecture.GUI.ZDateTimeOffsetEdit ArrivalDateDateTimeOffsetEdit;
		internal ZArchitecture.GUI.ZCheckBox OverrideFreightDetailsCheckBox;
		internal Enterprise.ZArchitecture.ZLabel TransportMeansLabel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CommunicationLanguageDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportAtArrivalTypeDropEdit;
		internal ZArchitecture.ZTextBox TransportAtArrivalIDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit StateOfSealsDropEdit;
		internal ZArchitecture.ZTextBox AdditionalTextTextBox;
		internal ZArchitecture.GUI.SeparatorUserControl NationalInfoSeparatorUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox GoodsLocationFromAuthorizationCodeFindBox;
	}
}
