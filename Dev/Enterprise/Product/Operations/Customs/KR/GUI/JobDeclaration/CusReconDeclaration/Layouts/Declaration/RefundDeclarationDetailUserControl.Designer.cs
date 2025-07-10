namespace Enterprise.Customs.KR.GUI
{
	partial class RefundDeclarationDetailUserControl
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
            this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TotalRefundAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.RefundApprovalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.RefundApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ProvisionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ProvisionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RefundTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RefundCauseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RefundReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepartmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TaxOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.BranchCodeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.PayerBankDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PayerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.BankAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RegistrationNumberOneTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.KoreanRegistrationNumberOfCEOTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MessageStatusDropEdit.SuspendLayout();
            this.EntryStatusDropEdit.SuspendLayout();
            this.AcceptedDateEdit.SuspendLayout();
            this.RefundApprovalDateEdit.SuspendLayout();
            this.ProvisionDateEdit.SuspendLayout();
            this.RefundTypeDropEdit.SuspendLayout();
            this.RefundCauseDropEdit.SuspendLayout();
            this.RefundReasonDropEdit.SuspendLayout();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.DepartmentCodeFindBox.SuspendLayout();
            this.TaxOfficeCodeFindBox.SuspendLayout();
            this.BrokerCodeFindBox.SuspendLayout();
            this.BranchCodeGuidFindBox.SuspendLayout();
            this.PayerBankDropEdit.SuspendLayout();
            this.PayerAddressControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
            // 
            // EntryNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "FormattedRefundDeclarationNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).FormattedRefundDeclarationNumber)));
            this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 23, true);
            this.EntryNumberTextBox.Name = "EntryNumberTextBox";
            this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 18, true);
            this.EntryNumberTextBox.TabIndex = 9;
            // 
            // TotalRefundAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalRefundAmountCalcEdit, "TotalRefundAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).TotalRefundAmount)));
            this.TotalRefundAmountCalcEdit.DecimalPlaces = 2;
            this.TotalRefundAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 48, true);
            this.TotalRefundAmountCalcEdit.Name = "TotalRefundAmountCalcEdit";
            this.TotalRefundAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 18, true);
            this.TotalRefundAmountCalcEdit.TabIndex = 10;
            this.TotalRefundAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalRefundAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // MessageStatusDropEdit
            // 
            this.MessageStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CRD_MessageStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_MessageStatus)));
            this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 73, true);
            this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
            this.MessageStatusDropEdit.PreBoundMaxLength = 3;
            this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 18, true);
            this.MessageStatusDropEdit.TabIndex = 11;
            // 
            // EntryStatusDropEdit
            // 
            this.EntryStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CRD_CustomsStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_CustomsStatus)));
            this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 99, true);
            this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
            this.EntryStatusDropEdit.PreBoundMaxLength = 3;
            this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 18, true);
            this.EntryStatusDropEdit.TabIndex = 12;
            // 
            // AcceptedDateEdit
            // 
            this.AcceptedDateEdit.AllowDrop = true;
            this.AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.AcceptedDateEdit, "AcceptedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).AcceptedDate)));
            this.AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 125, true);
            this.AcceptedDateEdit.Name = "AcceptedDateEdit";
            this.AcceptedDateEdit.TabIndex = 13;
            // 
            // RefundApprovalDateEdit
            // 
            this.RefundApprovalDateEdit.AllowDrop = true;
            this.RefundApprovalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.RefundApprovalDateEdit, "RefundApprovalDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).RefundApprovalDate)));
            this.RefundApprovalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 151, true);
            this.RefundApprovalDateEdit.Name = "RefundApprovalDateEdit";
            this.RefundApprovalDateEdit.TabIndex = 14;
            // 
            // RefundApprovalNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.RefundApprovalNumberTextBox, "RefundApprovalNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).RefundApprovalNumber)));
            this.RefundApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 176, true);
            this.RefundApprovalNumberTextBox.Name = "RefundApprovalNumberTextBox";
            this.RefundApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 18, true);
            this.RefundApprovalNumberTextBox.TabIndex = 15;
            // 
            // ProvisionDateEdit
            // 
            this.ProvisionDateEdit.AllowDrop = true;
            this.ProvisionDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ProvisionDateEdit, "ProvisionDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).ProvisionDate)));
            this.ProvisionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 201, true);
            this.ProvisionDateEdit.Name = "ProvisionDateEdit";
            this.ProvisionDateEdit.TabIndex = 16;
            // 
            // ProvisionNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ProvisionNumberTextBox, "ProvisionNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).ProvisionNumber)));
            this.ProvisionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 227, true);
            this.ProvisionNumberTextBox.Name = "ProvisionNumberTextBox";
            this.ProvisionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 18, true);
            this.ProvisionNumberTextBox.TabIndex = 17;
            // 
            // RefundTypeDropEdit
            // 
            this.RefundTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RefundTypeDropEdit, "CRD_DeclarationType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_DeclarationType)));
            this.RefundTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 23, true);
            this.RefundTypeDropEdit.Name = "RefundTypeDropEdit";
            this.RefundTypeDropEdit.PreBoundMaxLength = 1;
            this.RefundTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.RefundTypeDropEdit.TabIndex = 0;
            // 
            // RefundCauseDropEdit
            // 
            this.RefundCauseDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RefundCauseDropEdit, "CRD_RefundCauseCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_RefundCauseCode)));
            this.RefundCauseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 45, true);
            this.RefundCauseDropEdit.Name = "RefundCauseDropEdit";
            this.RefundCauseDropEdit.PreBoundMaxLength = 2;
            this.RefundCauseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.RefundCauseDropEdit.TabIndex = 2;
            // 
            // RefundReasonDropEdit
            // 
            this.RefundReasonDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RefundReasonDropEdit, "CRD_RefundReasonCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_RefundReasonCode)));
            this.RefundReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 67, true);
            this.RefundReasonDropEdit.Name = "RefundReasonDropEdit";
            this.RefundReasonDropEdit.PreBoundMaxLength = 2;
            this.RefundReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.RefundReasonDropEdit.TabIndex = 3;
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CRD_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_CustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 92, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 4;
            // 
            // DepartmentCodeFindBox
            // 
            this.DepartmentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartmentCodeFindBox, "CRD_CustomsDivision");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_CustomsDivision)));
            this.DepartmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 113, true);
            this.DepartmentCodeFindBox.Name = "DepartmentCodeFindBox";
            this.DepartmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartmentCodeFindBox.ParentType = null;
            this.DepartmentCodeFindBox.PreBoundMaxLength = 2;
            this.DepartmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.DepartmentCodeFindBox.TabIndex = 5;
            // 
            // TaxOfficeCodeFindBox
            // 
            this.TaxOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TaxOfficeCodeFindBox, "CRD_TaxOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_TaxOffice)));
            this.TaxOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 135, true);
            this.TaxOfficeCodeFindBox.Name = "TaxOfficeCodeFindBox";
            this.TaxOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TaxOfficeCodeFindBox.ParentType = null;
            this.TaxOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.TaxOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.TaxOfficeCodeFindBox.TabIndex = 6;
            // 
            // BrokerCodeFindBox
            // 
            this.BrokerCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "CRD_GS_NKCustomsAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_GS_NKCustomsAgent)));
            this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 183, true);
            this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
            this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BrokerCodeFindBox.ParentType = null;
            this.BrokerCodeFindBox.PreBoundMaxLength = 3;
            this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.BrokerCodeFindBox.TabIndex = 8;
            // 
            // BranchCodeGuidFindBox
            // 
            this.BranchCodeGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BranchCodeGuidFindBox, "CRD_GB_Branch");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_GB_Branch)));
            this.BranchCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 157, true);
            this.BranchCodeGuidFindBox.Name = "BranchCodeGuidFindBox";
            this.BranchCodeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BranchCodeGuidFindBox.ParentType = null;
            this.BranchCodeGuidFindBox.PreBoundMaxLength = 3;
            this.BranchCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.BranchCodeGuidFindBox.TabIndex = 9;
            // 
            // PayerAddressControl
            // 
            this.PayerAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PayerAddressControl, "CRD_OA_DeclarantAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CRD_OA_DeclarantAddress)));
            this.PayerAddressControl.BindToOrgList = "Lookups.Organisations";
            this.PayerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 225, true);
            this.PayerAddressControl.Name = "PayerAddressControl";
            this.PayerAddressControl.PopupCaption = "";
            this.PayerAddressControl.ShowAddress = false;
            this.PayerAddressControl.ShowOrganisationName = false;
            this.PayerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 21, true);
            this.PayerAddressControl.TabIndex = 0;
            // 
            // BankAccountNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.BankAccountNumberTextBox, "BankAccountNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).BankAccountNumber)));
            this.BankAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 251, true);
            this.BankAccountNumberTextBox.Name = "BankAccountNumberTextBox";
            this.BankAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 21, true);
            this.BankAccountNumberTextBox.TabIndex = 1;
            // 
            // RegistrationNumberOneTextBox
            // 
            this.BindingSource.SetBindingMember(this.RegistrationNumberOneTextBox, "RegistrationNumberOne");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).RegistrationNumberOne)));
            this.RegistrationNumberOneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 276, true);
            this.RegistrationNumberOneTextBox.Name = "RegistrationNumberOneTextBox";
            this.RegistrationNumberOneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 21, true);
            this.RegistrationNumberOneTextBox.TabIndex = 2;
            // 
            // KoreanRegistrationNumberOfCEOTextBox
            // 
            this.BindingSource.SetBindingMember(this.KoreanRegistrationNumberOfCEOTextBox, "KoreanRegistrationNumberOfCEO");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).KoreanRegistrationNumberOfCEO)));
            this.KoreanRegistrationNumberOfCEOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 301, true);
            this.KoreanRegistrationNumberOfCEOTextBox.Name = "KoreanRegistrationNumberOfCEOTextBox";
            this.KoreanRegistrationNumberOfCEOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 21, true);
            this.KoreanRegistrationNumberOfCEOTextBox.TabIndex = 3;
            // 
            // PayerBankDropEdit
            // 
            this.PayerBankDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PayerBankDropEdit, "PayerBank");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).PayerBank)));
            this.PayerBankDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 327, true);
            this.PayerBankDropEdit.Name = "PayerBankDropEdit";
            this.PayerBankDropEdit.PreBoundMaxLength = 3;
            this.PayerBankDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 21, true);
            this.PayerBankDropEdit.TabIndex = 4;
            // 
            // RefundDeclarationDetailUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ProvisionNumberTextBox);
            this.Controls.Add(this.ProvisionDateEdit);
            this.Controls.Add(this.RefundApprovalNumberTextBox);
            this.Controls.Add(this.RefundApprovalDateEdit);
            this.Controls.Add(this.AcceptedDateEdit);
            this.Controls.Add(this.EntryStatusDropEdit);
            this.Controls.Add(this.MessageStatusDropEdit);
            this.Controls.Add(this.TotalRefundAmountCalcEdit);
            this.Controls.Add(this.EntryNumberTextBox);
            this.Controls.Add(this.BranchCodeGuidFindBox);
            this.Controls.Add(this.BrokerCodeFindBox);
            this.Controls.Add(this.TaxOfficeCodeFindBox);
            this.Controls.Add(this.DepartmentCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Controls.Add(this.RefundReasonDropEdit);
            this.Controls.Add(this.RefundCauseDropEdit);
            this.Controls.Add(this.RefundTypeDropEdit);
            this.Controls.Add(this.PayerAddressControl);
            this.Controls.Add(this.BankAccountNumberTextBox);
            this.Controls.Add(this.KoreanRegistrationNumberOfCEOTextBox);
            this.Controls.Add(this.RegistrationNumberOneTextBox);
            this.Controls.Add(this.PayerBankDropEdit);
            this.Name = "RefundDeclarationDetailUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 790, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MessageStatusDropEdit.ResumeLayout(true);
            this.MessageStatusDropEdit.PerformLayout();
            this.EntryStatusDropEdit.ResumeLayout(true);
            this.EntryStatusDropEdit.PerformLayout();
            this.AcceptedDateEdit.ResumeLayout(true);
            this.AcceptedDateEdit.PerformLayout();
            this.RefundApprovalDateEdit.ResumeLayout(true);
            this.RefundApprovalDateEdit.PerformLayout();
            this.ProvisionDateEdit.ResumeLayout(true);
            this.ProvisionDateEdit.PerformLayout();
            this.RefundTypeDropEdit.ResumeLayout(true);
            this.RefundTypeDropEdit.PerformLayout();
            this.RefundCauseDropEdit.ResumeLayout(true);
            this.RefundCauseDropEdit.PerformLayout();
            this.RefundReasonDropEdit.ResumeLayout(true);
            this.RefundReasonDropEdit.PerformLayout();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.DepartmentCodeFindBox.ResumeLayout(true);
            this.DepartmentCodeFindBox.PerformLayout();
            this.TaxOfficeCodeFindBox.ResumeLayout(true);
            this.TaxOfficeCodeFindBox.PerformLayout();
            this.BrokerCodeFindBox.ResumeLayout(true);
            this.BrokerCodeFindBox.PerformLayout();
            this.BranchCodeGuidFindBox.ResumeLayout(true);
            this.BranchCodeGuidFindBox.PerformLayout();
            this.PayerBankDropEdit.ResumeLayout(true);
            this.PayerBankDropEdit.PerformLayout();
            this.PayerAddressControl.ResumeLayout(true);
            this.PayerAddressControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox EntryNumberTextBox;
		internal ZArchitecture.ZCalcEdit TotalRefundAmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		internal ZArchitecture.GUI.ZDateEdit AcceptedDateEdit;
		internal ZArchitecture.GUI.ZDateEdit RefundApprovalDateEdit;
		internal ZArchitecture.ZTextBox RefundApprovalNumberTextBox;
		internal ZArchitecture.GUI.ZDateEdit ProvisionDateEdit;
		internal ZArchitecture.ZTextBox ProvisionNumberTextBox;
        internal ZArchitecture.GUI.ZDropEdit RefundTypeDropEdit;
        internal ZArchitecture.GUI.ZDropEdit RefundCauseDropEdit;
        internal ZArchitecture.GUI.ZDropEdit RefundReasonDropEdit;
        internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
        internal ZArchitecture.GUI.ZCodeFindBox DepartmentCodeFindBox;
        internal ZArchitecture.GUI.ZCodeFindBox TaxOfficeCodeFindBox;
        internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
        internal ZArchitecture.GUI.ZGuidFindBox BranchCodeGuidFindBox;
        internal ZArchitecture.GUI.ZAddressControl PayerAddressControl;
        internal ZArchitecture.ZTextBox BankAccountNumberTextBox;
        internal ZArchitecture.ZTextBox RegistrationNumberOneTextBox;
        internal ZArchitecture.ZTextBox KoreanRegistrationNumberOfCEOTextBox;
        internal ZArchitecture.GUI.ZDropEdit PayerBankDropEdit;
    }
}
