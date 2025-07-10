using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TemporaryStorageUserControl
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
			this.components = new System.ComponentModel.Container();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarantAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.RepresentativeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CarrierAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PersonPresentingTheGoodsAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SupervisingCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PresentationCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AuthorizationOwnerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AuthorizationNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsENSReuseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HasHouseConsignmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HasNoMasterBillCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ArrivalTransportMeansCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoodsPresentationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstimatedDateOfArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PlaceOfUnloadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PlaceOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
			this.GuaranteeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicGuaranteePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CusAgentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PreviousDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.PreviousDocumentUserControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.CustomsStatusDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryCodeFindBox.SuspendLayout();
			this.MessageTypeDropEdit.SuspendLayout();
			this.DeclarantAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.CarrierAddressControl.SuspendLayout();
			this.PersonPresentingTheGoodsAddressControl.SuspendLayout();
			this.SupervisingCustomsOfficeCodeFindBox.SuspendLayout();
			this.PresentationCustomsOfficeCodeFindBox.SuspendLayout();
			this.AuthorizationOwnerGuidFindBox.SuspendLayout();
			this.AuthorizationNumberCodeFindBox.SuspendLayout();
			this.TransportTypeDropEdit.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.AuthorizationTypeDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.DeclarationDateDateEdit.SuspendLayout();
			this.GoodsPresentationDateEdit.SuspendLayout();
			this.EstimatedDateOfArrivalDateEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.PlaceOfUnloadingCodeFindBox.SuspendLayout();
			this.PlaceOfLoadingCodeFindBox.SuspendLayout();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.DynamicGuaranteePanel.SuspendLayout();
			this.GuaranteeGroupBox.SuspendLayout();
			this.CusAgentCodeFindBox.SuspendLayout();
			this.PreviousDocumentUserControlTabPage.SuspendLayout();
			this.DocumentsTabControl.SuspendLayout();
			this.CustomsStatusDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "AMA_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_RN_NKCountry)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 21, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.PreBoundMaxLength = 1;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.CountryCodeFindBox.TabIndex = 1;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "AMA_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_MessageType)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 73, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.MessageTypeDropEdit.TabIndex = 3;
			// 
			// LRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNTextBox, "LRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).LRN)));
			this.LRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 37, true);
			this.LRNTextBox.Name = "LRNTextBox";
			this.LRNTextBox.ReadOnly = true;
			this.LRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.LRNTextBox.TabIndex = 17;
			// 
			// DeclarantAddressControl
			// 
			this.DeclarantAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantAddressControl, "AMA_OA_Declarant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_OA_Declarant)));
			this.DeclarantAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 197, true);
			this.DeclarantAddressControl.Name = "DeclarantAddressControl";
			this.DeclarantAddressControl.PopupCaption = "";
			this.DeclarantAddressControl.ShowAddress = false;
			this.DeclarantAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.DeclarantAddressControl.TabIndex = 18;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "AMA_OA_Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_OA_Representative)));
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 223, true);
			this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
			this.RepresentativeAddressControl.PopupCaption = "";
			this.RepresentativeAddressControl.ShowAddress = false;
			this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.RepresentativeAddressControl.TabIndex = 19;
			// 
			// CarrierAddressControl
			// 
			this.CarrierAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAddressControl, "AMA_OA_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_OA_Carrier)));
			this.CarrierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 355, true);
			this.CarrierAddressControl.Name = "CarrierAddressControl";
			this.CarrierAddressControl.PopupCaption = "";
			this.CarrierAddressControl.ShowAddress = false;
			this.CarrierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.CarrierAddressControl.TabIndex = 20;
			// 
			// PersonPresentingTheGoodsAddressControl
			// 
			this.PersonPresentingTheGoodsAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PersonPresentingTheGoodsAddressControl, "AMA_OA_Presenter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_OA_Presenter)));
			this.PersonPresentingTheGoodsAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 328, true);
			this.PersonPresentingTheGoodsAddressControl.Name = "PersonPresentingTheGoodsAddressControl";
			this.PersonPresentingTheGoodsAddressControl.PopupCaption = "";
			this.PersonPresentingTheGoodsAddressControl.ShowAddress = false;
			this.PersonPresentingTheGoodsAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.PersonPresentingTheGoodsAddressControl.TabIndex = 21;
			// 
			// SupervisingCustomsOfficeCodeFindBox
			// 
			this.SupervisingCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupervisingCustomsOfficeCodeFindBox, "AMA_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_CustomsOffice)));
			this.SupervisingCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 251, true);
			this.SupervisingCustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.SupervisingCustomsOfficeCodeFindBox.Name = "SupervisingCustomsOfficeCodeFindBox";
			this.SupervisingCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SupervisingCustomsOfficeCodeFindBox.ParentType = null;
			this.SupervisingCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.SupervisingCustomsOfficeCodeFindBox.TabIndex = 22;
			// 
			// PresentationCustomsOfficeCodeFindBox
			// 
			this.PresentationCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresentationCustomsOfficeCodeFindBox, "PresentationCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).PresentationCustomsOffice)));
			this.PresentationCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 276, true);
			this.PresentationCustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PresentationCustomsOfficeCodeFindBox.Name = "PresentationCustomsOfficeCodeFindBox";
			this.PresentationCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PresentationCustomsOfficeCodeFindBox.ParentType = null;
			this.PresentationCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.PresentationCustomsOfficeCodeFindBox.TabIndex = 23;
			// 
			// AuthorizationOwnerGuidFindBox
			// 
			this.AuthorizationOwnerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationOwnerGuidFindBox, "AuthorizationOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AuthorizationOwner)));
			this.AuthorizationOwnerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 463, true);
			this.AuthorizationOwnerGuidFindBox.Name = "AuthorizationOwnerGuidFindBox";
			this.AuthorizationOwnerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationOwnerGuidFindBox.ParentType = null;
			this.AuthorizationOwnerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.AuthorizationOwnerGuidFindBox.TabIndex = 44;
			// 
			// AuthorizationNumberCodeFindBox
			// 
			this.AuthorizationNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberCodeFindBox, "AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AuthorizationNumber)));
			this.AuthorizationNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 494, true);
			this.AuthorizationNumberCodeFindBox.Name = "AuthorizationNumberCodeFindBox";
			this.AuthorizationNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationNumberCodeFindBox.ParentType = null;
			this.AuthorizationNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.AuthorizationNumberCodeFindBox.TabIndex = 45;
			// 
			// HasHouseConsignmentCheckBox
			// 
			this.HasHouseConsignmentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasHouseConsignmentCheckBox, "AMA_Calc_HasHouseConsignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).IsENSReuse)));
			this.HasHouseConsignmentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HasHouseConsignmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 100, true);
			this.HasHouseConsignmentCheckBox.Name = "HasHouseConsignmentCheckBox";
			this.HasHouseConsignmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.HasHouseConsignmentCheckBox.TabIndex = 31;
			this.HasHouseConsignmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// HasNoMasterBillCheckBox
			// 
			this.HasNoMasterBillCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasNoMasterBillCheckBox, "HasNoMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).IsENSReuse)));
			this.HasNoMasterBillCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HasNoMasterBillCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 100, true);
			this.HasNoMasterBillCheckBox.Name = "HasNoMasterBillCheckBox";
			this.HasNoMasterBillCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.HasNoMasterBillCheckBox.TabIndex = 31;
			this.HasNoMasterBillCheckBox.UseVisualStyleBackColor = true;
			// 
			// MRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MRNTextBox, "MRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).MRN)));
			this.MRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 162, true);
			this.MRNTextBox.Name = "MRNTextBox";
			this.MRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.MRNTextBox.TabIndex = 24;
			// 
			// CRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.CRNTextBox, "CRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).CRN)));
			this.CRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 203, true);
			this.CRNTextBox.Name = "CRNTextBox";
			this.CRNTextBox.ReadOnly = true;
			this.CRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.CRNTextBox.TabIndex = 25;
			// 
			// FRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.FRNTextBox, "FRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).FRN)));
			this.FRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 238, true);
			this.FRNTextBox.Name = "FRNTextBox";
			this.FRNTextBox.ReadOnly = true;
			this.FRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.FRNTextBox.TabIndex = 26;
			// 
			// IsENSReuseCheckBox
			// 
			this.IsENSReuseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsENSReuseCheckBox, "IsENSReuse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).IsENSReuse)));
			this.IsENSReuseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsENSReuseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 100, true);
			this.IsENSReuseCheckBox.Name = "IsENSReuseCheckBox";
			this.IsENSReuseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.IsENSReuseCheckBox.TabIndex = 31;
			this.IsENSReuseCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransportTypeDropEdit
			// 
			this.TransportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportTypeDropEdit, "TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).TransportType)));
			this.TransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 147, true);
			this.TransportTypeDropEdit.Name = "TransportTypeDropEdit";
			this.TransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.TransportTypeDropEdit.TabIndex = 33;
			// 
			// ArrivalTransportMeansCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalTransportMeansCodeTextBox, "ArrivalTransportMeansCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).ArrivalTransportMeansCode)));
			this.ArrivalTransportMeansCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 171, true);
			this.ArrivalTransportMeansCodeTextBox.Name = "ArrivalTransportMeansCodeTextBox";
			this.ArrivalTransportMeansCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 18, true);
			this.ArrivalTransportMeansCodeTextBox.TabIndex = 34;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).CustomsStatus)));
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 96, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.CustomsStatusDropEdit.TabIndex = 35;
			// 
			// AuthorizationTypeDropEdit
			// 
			this.AuthorizationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationTypeDropEdit, "AuthorizationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AuthorizationType)));
			this.AuthorizationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 441, true);
			this.AuthorizationTypeDropEdit.Name = "AuthorizationTypeDropEdit";
			this.AuthorizationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.AuthorizationTypeDropEdit.TabIndex = 43;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "AMA_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 68, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.MessageStatusDropEdit.TabIndex = 36;
			// 
			// DeclarationDateDateEdit
			// 
			this.DeclarationDateDateEdit.AllowDrop = true;
			this.DeclarationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeclarationDateDateEdit, "DeclarationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).DeclarationDate)));
			this.DeclarationDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeclarationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 47, true);
			this.DeclarationDateDateEdit.Name = "DeclarationDateDateEdit";
			this.DeclarationDateDateEdit.TabIndex = 39;
			// 
			// GoodsPresentationDateEdit
			// 
			this.GoodsPresentationDateEdit.AllowDrop = true;
			this.GoodsPresentationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.GoodsPresentationDateEdit, "AMA_DateAtCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_DateAtCustomsOffice)));
			this.GoodsPresentationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.GoodsPresentationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 302, true);
			this.GoodsPresentationDateEdit.Name = "GoodsPresentationDateEdit";
			this.GoodsPresentationDateEdit.TabIndex = 40;
			// 
			// EstimatedDateOfArrivalDateEdit
			// 
			this.EstimatedDateOfArrivalDateEdit.AllowDrop = true;
			this.EstimatedDateOfArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EstimatedDateOfArrivalDateEdit, "EstimatedDateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).EstimatedDateOfArrival)));
			this.EstimatedDateOfArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstimatedDateOfArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 519, true);
			this.EstimatedDateOfArrivalDateEdit.Name = "EstimatedDateOfArrivalDateEdit";
			this.EstimatedDateOfArrivalDateEdit.TabIndex = 40;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "AMA_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 119, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 18, true);
			this.TransportModeDropEdit.TabIndex = 41;
			// 
			// PlaceOfUnloadingCodeFindBox
			// 
			this.PlaceOfUnloadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeFindBox, "PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).PlaceOfUnloading)));
			this.PlaceOfUnloadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 388, true);
			this.PlaceOfUnloadingCodeFindBox.Name = "PlaceOfUnloadingCodeFindBox";
			this.PlaceOfUnloadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfUnloadingCodeFindBox.ParentType = null;
			this.PlaceOfUnloadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.PlaceOfUnloadingCodeFindBox.TabIndex = 42;
			// 
			// PlaceOfLoadingCodeFindBox
			// 
			this.PlaceOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfLoadingCodeFindBox, "PlaceOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).PlaceOfLoading)));
			this.PlaceOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 544, true);
			this.PlaceOfLoadingCodeFindBox.Name = "PlaceOfLoadingCodeFindBox";
			this.PlaceOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfLoadingCodeFindBox.ParentType = null;
			this.PlaceOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.PlaceOfLoadingCodeFindBox.TabIndex = 42;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)))));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 411, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 23, true);
			this.LocationOfGoodsUserControl.TabIndex = 46;
			// 
			// CusAgentCodeFindBox
			// 
			this.CusAgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusAgentCodeFindBox, "AMA_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_GS_NKCustomsAgent)));
			this.CusAgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 266, true);
			this.CusAgentCodeFindBox.Name = "CusAgentCodeFindBox";
			this.CusAgentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusAgentCodeFindBox.ParentType = null;
			this.CusAgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 18, true);
			this.CusAgentCodeFindBox.TabIndex = 47;
			// 
			// GuaranteeGroupBox
			// 
			this.GuaranteeGroupBox.CaptionResourceString = Res.GetData("95EF1E50-2A7A-4AA7-B00F-DCB7DAF7EA7F", "Guarantee");
			this.GuaranteeGroupBox.Controls.Add(this.DynamicGuaranteePanel);
			this.GuaranteeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 16, true);
			this.GuaranteeGroupBox.Name = "GuaranteeGroupBox";
			this.GuaranteeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 82, true);
			this.GuaranteeGroupBox.TabIndex = 2;
			this.GuaranteeGroupBox.TabStop = false;
			// 
			// DynamicGuaranteePanel
			//
			this.BindingSource.SetBindingMember(this.DynamicGuaranteePanel, "Guarantee");
			this.DynamicGuaranteePanel.AllowDrop = true;
			this.DynamicGuaranteePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicGuaranteePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicGuaranteePanel.Name = "DynamicGuaranteePanel";
			this.DynamicGuaranteePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
			this.DynamicGuaranteePanel.TabIndex = 3;
			// 
			// PreviousDocumentsLayoutPanel
			// 
			this.PreviousDocumentsLayoutPanel.AllowDrop = true;
			this.PreviousDocumentsLayoutPanel.AutoScroll = true;
			this.PreviousDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsLayoutPanel.Name = "PreviousDocumentsLayoutPanel";
			this.PreviousDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 228, true);
			this.PreviousDocumentsLayoutPanel.TabIndex = 0;
			// 
			// PreviousDocumentUserControlTabPage
			// 
			this.PreviousDocumentUserControlTabPage.CaptionResourceString = Res.GetData("7564A342-1B37-49BD-AB2A-20CA7167A281", "Previous Docs");
			this.PreviousDocumentUserControlTabPage.Controls.Add(this.PreviousDocumentsLayoutPanel);
			this.PreviousDocumentUserControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PreviousDocumentUserControlTabPage.Name = "PreviousDocumentUserControlTabPage";
			this.PreviousDocumentUserControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 229, true);
			this.PreviousDocumentUserControlTabPage.TabIndex = 0;
			// 
			// DocumentsTabControl
			// 
			this.DocumentsTabControl.Controls.Add(this.PreviousDocumentUserControlTabPage);
			this.DocumentsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 251, true);
			this.DocumentsTabControl.Name = "DocumentsTabControl";
			this.DocumentsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 251, true);
			this.DocumentsTabControl.TabIndex = 49;
			// 
			// CustomsStatusDateEdit
			// 
			this.CustomsStatusDateEdit.AllowDrop = true;
			this.CustomsStatusDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CustomsStatusDateEdit, "CustomsStatusDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).CustomsStatusDate)));
			this.CustomsStatusDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CustomsStatusDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 128, true);
			this.CustomsStatusDateEdit.Name = "CustomsStatusDateEdit";
			this.CustomsStatusDateEdit.TabIndex = 50;
			// 
			// UCC6TemporaryStorageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocumentsTabControl);
			this.Controls.Add(this.CustomsStatusDateEdit);
			this.Controls.Add(this.CusAgentCodeFindBox);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.GoodsPresentationDateEdit);
			this.Controls.Add(this.EstimatedDateOfArrivalDateEdit);
			this.Controls.Add(this.DeclarationDateDateEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.CustomsStatusDropEdit);
			this.Controls.Add(this.AuthorizationOwnerGuidFindBox);
			this.Controls.Add(this.AuthorizationNumberCodeFindBox);
			this.Controls.Add(this.AuthorizationTypeDropEdit);
			this.Controls.Add(this.PlaceOfUnloadingCodeFindBox);
			this.Controls.Add(this.PlaceOfLoadingCodeFindBox);
			this.Controls.Add(this.ArrivalTransportMeansCodeTextBox);
			this.Controls.Add(this.TransportTypeDropEdit);
			this.Controls.Add(this.IsENSReuseCheckBox);
			this.Controls.Add(this.HasHouseConsignmentCheckBox);
			this.Controls.Add(this.HasNoMasterBillCheckBox);
			this.Controls.Add(this.FRNTextBox);
			this.Controls.Add(this.CRNTextBox);
			this.Controls.Add(this.MRNTextBox);
			this.Controls.Add(this.PresentationCustomsOfficeCodeFindBox);
			this.Controls.Add(this.SupervisingCustomsOfficeCodeFindBox);
			this.Controls.Add(this.PersonPresentingTheGoodsAddressControl);
			this.Controls.Add(this.CarrierAddressControl);
			this.Controls.Add(this.RepresentativeAddressControl);
			this.Controls.Add(this.DeclarantAddressControl);
			this.Controls.Add(this.LRNTextBox);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.CountryCodeFindBox);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.GuaranteeGroupBox);
			this.Name = "UCC6TemporaryStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 567, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.DeclarantAddressControl.ResumeLayout(true);
			this.DeclarantAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.CarrierAddressControl.ResumeLayout(true);
			this.CarrierAddressControl.PerformLayout();
			this.PersonPresentingTheGoodsAddressControl.ResumeLayout(true);
			this.PersonPresentingTheGoodsAddressControl.PerformLayout();
			this.SupervisingCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.SupervisingCustomsOfficeCodeFindBox.PerformLayout();
			this.PresentationCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.PresentationCustomsOfficeCodeFindBox.PerformLayout();
			this.AuthorizationOwnerGuidFindBox.ResumeLayout(true);
			this.AuthorizationOwnerGuidFindBox.PerformLayout();
			this.AuthorizationNumberCodeFindBox.ResumeLayout(true);
			this.AuthorizationNumberCodeFindBox.PerformLayout();
			this.AuthorizationNumberCodeFindBox.ResumeLayout(true);
			this.AuthorizationNumberCodeFindBox.PerformLayout();
			this.HasHouseConsignmentCheckBox.ResumeLayout(true);
			this.HasHouseConsignmentCheckBox.PerformLayout();
			this.HasNoMasterBillCheckBox.ResumeLayout(true);
			this.HasNoMasterBillCheckBox.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.AuthorizationTypeDropEdit.ResumeLayout(true);
			this.AuthorizationTypeDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.DeclarationDateDateEdit.ResumeLayout(true);
			this.DeclarationDateDateEdit.PerformLayout();
			this.GoodsPresentationDateEdit.ResumeLayout(true);
			this.GoodsPresentationDateEdit.PerformLayout();
			this.EstimatedDateOfArrivalDateEdit.ResumeLayout(true);
			this.EstimatedDateOfArrivalDateEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.TransportTypeDropEdit.ResumeLayout(true);
			this.TransportTypeDropEdit.PerformLayout();
			this.PlaceOfUnloadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingCodeFindBox.PerformLayout();
			this.PlaceOfLoadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfLoadingCodeFindBox.PerformLayout();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.DynamicGuaranteePanel.ResumeLayout(false);
			this.DynamicGuaranteePanel.PerformLayout();
			this.GuaranteeGroupBox.ResumeLayout(false);
			this.GuaranteeGroupBox.PerformLayout();
			this.CusAgentCodeFindBox.ResumeLayout(true);
			this.CusAgentCodeFindBox.PerformLayout();
			this.PreviousDocumentUserControlTabPage.ResumeLayout(false);
			this.PreviousDocumentUserControlTabPage.PerformLayout();
			this.DocumentsTabControl.ResumeLayout(false);
			this.DocumentsTabControl.PerformLayout();
			this.CustomsStatusDateEdit.ResumeLayout(true);
			this.CustomsStatusDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		internal ZArchitecture.ZTextBox LRNTextBox;
		internal ZArchitecture.GUI.ZAddressControl DeclarantAddressControl;
		internal ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		internal ZArchitecture.GUI.ZAddressControl CarrierAddressControl;
		internal ZArchitecture.GUI.ZAddressControl PersonPresentingTheGoodsAddressControl;
		internal ZArchitecture.GUI.ZCodeFindBox SupervisingCustomsOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox PresentationCustomsOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox AuthorizationOwnerGuidFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox AuthorizationNumberCodeFindBox;
		internal ZArchitecture.ZTextBox MRNTextBox;
		internal ZArchitecture.ZTextBox CRNTextBox;
		internal ZArchitecture.ZTextBox FRNTextBox;
		internal ZArchitecture.GUI.ZCheckBox IsENSReuseCheckBox;
		internal ZArchitecture.ZTextBox ArrivalTransportMeansCodeTextBox;
		internal ZArchitecture.GUI.ZDropEdit TransportTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit AuthorizationTypeDropEdit;
		internal ZArchitecture.GUI.ZDateEdit DeclarationDateDateEdit;
		internal ZArchitecture.GUI.ZDateEdit GoodsPresentationDateEdit;
		internal ZArchitecture.GUI.ZDateEdit EstimatedDateOfArrivalDateEdit;
		internal ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfLoadingCodeFindBox;
		internal LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicGuaranteePanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox GuaranteeGroupBox;
		internal ZArchitecture.GUI.ZCodeFindBox CusAgentCodeFindBox;
		private ZArchitecture.GUI.DynamicLayoutPanel PreviousDocumentsLayoutPanel;
		internal ZTabPage PreviousDocumentUserControlTabPage;
		internal ZTemplateTabControl DocumentsTabControl;
		internal ZArchitecture.GUI.ZDateEdit CustomsStatusDateEdit;
		internal ZArchitecture.GUI.ZCheckBox HasHouseConsignmentCheckBox;
		internal ZArchitecture.GUI.ZCheckBox HasNoMasterBillCheckBox;
	}
}
