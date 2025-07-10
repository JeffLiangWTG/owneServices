namespace Enterprise.Customs.FR.GUI.NCTS
{
	partial class DeclarationDetailsTabUserControl
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
			this.AuthorisedLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ControlResultDateLimitDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.FRSpecificGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.IsPrelodgedMovementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IsQueriedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IsQueryAvailableOnPaperCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.QueryInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.IsTC11DeliveredByCustomsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TC11DateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ChargePaymentOrDestinationIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DetailedStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DeclarantDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
            this.SealsNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AuthorisedLocationCodeFindBox.SuspendLayout();
			this.TraderDetailsGroupBox.SuspendLayout();
            this.RepresentativeDocAddressControl.SuspendLayout();
            this.CustomsOfficesGroupBox.SuspendLayout();
            this.GuaranteesGroupBox.SuspendLayout();
            this.ContainersGroupBox.SuspendLayout();
            this.TransportDetailsGroupBox.SuspendLayout();
            this.GoodsLocationNormalGroupBox.SuspendLayout();
            this.GoodsLocationSimplifiedGroupBox.SuspendLayout();
            this.DeclarationDetailsGroupBox.SuspendLayout();
            this.BrokerFindBox.SuspendLayout();
            this.CountryOfDestinationDropEdit.SuspendLayout();
            this.MainDataPanel.SuspendLayout();
            this.LeftDataPanel.SuspendLayout();
            this.DeclarationTypeDropEdit.SuspendLayout();
            this.CountryOfDispatchDropEdit.SuspendLayout();
            this.PortOfDispatchFindBox.SuspendLayout();
            this.StatusDropEdit.SuspendLayout();
            this.MessageStatusDropEdit.SuspendLayout();
            this.MeansOfTransportCrossingBorderNationalityDropEdit.SuspendLayout();
            this.TransportModeAtBorderDropEdit.SuspendLayout();
            this.MeansOfTransportAtDepartureNationalityDropEdit.SuspendLayout();
            this.InlandTransportModeDropEdit.SuspendLayout();
            this.PlaceOfLoadingNormalCodeFindBox.SuspendLayout();
            this.ControlResultDateLimitDateEdit.SuspendLayout();
            this.PlaceOfLoadingSimplifiedCodeFindBox.SuspendLayout();
            this.AgreedLocationOfGoodsCodePanel.SuspendLayout();
            this.GoodsLocationNormalMainPanel.SuspendLayout();
            this.TirCarnetExpiryDateEdit.SuspendLayout();
            this.ValuationDateDateEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ControlResultDateLimitDateEdit2.SuspendLayout();
            this.FRSpecificGroupBox.SuspendLayout();
            this.TC11DateDateEdit.SuspendLayout();
            this.ChargePaymentOrDestinationIDDropEdit.SuspendLayout();
            this.DetailedStatusDropEdit.SuspendLayout();
            this.DeclarantDocAddressControl.SuspendLayout();
            this.SealsNatureDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // TraderDetailsGroupBox
            // 
            this.TraderDetailsGroupBox.Controls.Add(this.DeclarantDocAddressControl);
            this.TraderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 775, true);
            this.TraderDetailsGroupBox.Controls.SetChildIndex(this.DeclarantDocAddressControl, 0);
            // 
			// RepresentativeDocAddressControl
			// 
			this.RepresentativeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 245, true);
            // 
            // ContainersGroupBox
            // 
            this.ContainersGroupBox.Controls.Add(this.SealsNatureDropEdit);
            this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 235, true);
            this.ContainersGroupBox.Controls.SetChildIndex(this.ContainersAndSealsZDynamicUserControl, 0);
            this.ContainersGroupBox.Controls.SetChildIndex(this.SealsNatureDropEdit, 0);
            // 
            // TransportDetailsGroupBox
            // 
            this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 315, true);
            // 
            // GoodsLocationNormalGroupBox
            // 
            this.GoodsLocationNormalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 471, true);
            this.GoodsLocationNormalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 148, true);
			// 
			// GoodsLocationSimplifiedGroupBox
			//
			this.GoodsLocationSimplifiedGroupBox.Controls.Add(this.AuthorisedLocationCodeFindBox);
			this.GoodsLocationSimplifiedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 619, true);
			this.GoodsLocationSimplifiedGroupBox.Controls.SetChildIndex(this.AuthorisedLocationCodeFindBox, 0);
			// 
			// AuthorisedLocationCodeTextBox
			// 
			this.AuthorisedLocationCodeTextBox.Visible = false;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.DetailedStatusDropEdit);
            this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 315, true);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ValuationDateDateEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.PortOfDispatchFindBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.RepresentativeDocAddressControl, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.BrokerFindBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DetailedStatusDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.TirCarnetExpiryDateEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.TirCarnetNumberTextBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.LrnTextBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.MrnTextBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DeclarationTypeDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.SafetyAndSecurityCheckBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDispatchDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDestinationDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.SimplifiedNctsProcedureCheckBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.MessageStatusDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.OverrideFreightDefaults, 0);
            // 
            // BrokerFindBox
            // 
            this.BrokerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 267, true);
            // 
            // CountryOfDestinationDropEdit
            // 
            this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 223, true);
            // 
            // MainDataPanel
            // 
            this.MainDataPanel.Controls.Add(this.FRSpecificGroupBox);
            this.MainDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 775, true);
            this.MainDataPanel.Controls.SetChildIndex(this.FRSpecificGroupBox, 0);
            this.MainDataPanel.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
            this.MainDataPanel.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
            this.MainDataPanel.Controls.SetChildIndex(this.GoodsLocationNormalGroupBox, 0);
            this.MainDataPanel.Controls.SetChildIndex(this.GoodsLocationSimplifiedGroupBox, 0);
            // 
            // LeftDataPanel
            // 
            this.LeftDataPanel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LeftDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 775, true);
            // 
            // SafetyAndSecurityCheckBox
            // 
            this.SafetyAndSecurityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 135, true);
            // 
            // DeclarationTypeDropEdit
            // 
            this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 157, true);
            // 
            // CountryOfDispatchDropEdit
            // 
            this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 201, true);
            // 
            // PortOfDispatchFindBox
            // 
            this.PortOfDispatchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 201, true);
            // 
            // SimplifiedNctsProcedureCheckBox
            // 
            this.SimplifiedNctsProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 135, true);
            // 
            // MessageStatusDropEdit
            // 
            this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 107, true);
            // 
            // AgreedLocationOfGoodsNormalTextBox
            // 
            this.AgreedLocationOfGoodsNormalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 3, true);
            // 
            // GoodsLocationNormalMainPanel
            // 
            this.GoodsLocationNormalMainPanel.Controls.Add(this.ControlResultDateLimitDateEdit2);
            this.GoodsLocationNormalMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 92, true);
            this.GoodsLocationNormalMainPanel.Controls.SetChildIndex(this.ControlResultDateLimitDateEdit2, 0);
            this.GoodsLocationNormalMainPanel.Controls.SetChildIndex(this.AgreedLocationOfGoodsNormalTextBox, 0);
            this.GoodsLocationNormalMainPanel.Controls.SetChildIndex(this.CustomsSubPlaceNormalTextBox, 0);
            this.GoodsLocationNormalMainPanel.Controls.SetChildIndex(this.PlaceOfLoadingNormalCodeFindBox, 0);
            // 
            // TirCarnetNumberTextBox
            // 
            this.TirCarnetNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 179, true);
            // 
            // TirCarnetExpiryDateEdit
            // 
            this.TirCarnetExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 179, true);
            // 
            // ContainersAndSealsZDynamicUserControl
            // 
            this.ContainersAndSealsZDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ContainersAndSealsZDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 182, true);
            // 
            // ValuationDateDateEdit
            // 
            this.ValuationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 289, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.NCTS.NctsHeader);
            // 
            // ControlResultDateLimitDateEdit2
            // 
            this.ControlResultDateLimitDateEdit2.AllowDrop = true;
            this.ControlResultDateLimitDateEdit2.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ControlResultDateLimitDateEdit2, "MovementHeader.BM_ExportDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).MovementHeader.BM_ExportDate)));
            this.ControlResultDateLimitDateEdit2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("5878E909-84F0-41B8-A36D-EC09FFCB1EAD", "Date Limit");
            this.ControlResultDateLimitDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 68, true);
            this.ControlResultDateLimitDateEdit2.Name = "ControlResultDateLimitDateEdit2";
            this.ControlResultDateLimitDateEdit2.TabIndex = 5;
            // 
            // FRSpecificGroupBox
            // 
            this.FRSpecificGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("7401B5C3-FD90-4E10-A31E-470950C74491", "Miscellaneous");
            this.FRSpecificGroupBox.Controls.Add(this.IsPrelodgedMovementCheckBox);
            this.FRSpecificGroupBox.Controls.Add(this.IsQueriedCheckBox);
            this.FRSpecificGroupBox.Controls.Add(this.IsQueryAvailableOnPaperCheckBox);
            this.FRSpecificGroupBox.Controls.Add(this.QueryInformationTextBox);
            this.FRSpecificGroupBox.Controls.Add(this.IsTC11DeliveredByCustomsCheckBox);
            this.FRSpecificGroupBox.Controls.Add(this.TC11DateDateEdit);
            this.FRSpecificGroupBox.Controls.Add(this.ChargePaymentOrDestinationIDDropEdit);
            this.FRSpecificGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 587, true);
            this.FRSpecificGroupBox.Name = "FRSpecificGroupBox";
            this.FRSpecificGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 119, true);
            this.FRSpecificGroupBox.TabIndex = 4;
            this.FRSpecificGroupBox.TabStop = false;
            // 
            // IsPrelodgedMovementCheckBox
            // 
            this.IsPrelodgedMovementCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsPrelodgedMovementCheckBox, "IsPrelodgedMovement");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).IsPrelodgedMovement)));
            this.IsPrelodgedMovementCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("9e2c5b97-4b4a-4e00-8002-8bc0da7c5bf4", "Pre-lodged Movement");
            this.IsPrelodgedMovementCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.IsPrelodgedMovementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 19, true);
            this.IsPrelodgedMovementCheckBox.Name = "IsPrelodgedMovementCheckBox";
            this.IsPrelodgedMovementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.IsPrelodgedMovementCheckBox.TabIndex = 1;
            this.IsPrelodgedMovementCheckBox.UseVisualStyleBackColor = true;
            // 
            // IsQueriedCheckBox
            // 
            this.IsQueriedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsQueriedCheckBox, "IsQueried");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).IsQueried)));
            this.IsQueriedCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("cecfec96-d1ee-448d-a926-e0a199208a1a", "Queried");
            this.IsQueriedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.IsQueriedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 19, true);
            this.IsQueriedCheckBox.Name = "IsQueriedCheckBox";
            this.IsQueriedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.IsQueriedCheckBox.TabIndex = 2;
            this.IsQueriedCheckBox.UseVisualStyleBackColor = true;
            // 
            // IsQueryAvailableOnPaperCheckBox
            // 
            this.IsQueryAvailableOnPaperCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsQueryAvailableOnPaperCheckBox, "IsQueryAvailableOnPaper");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).IsQueryAvailableOnPaper)));
            this.IsQueryAvailableOnPaperCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1dd1b8a6-a839-4cab-9f73-94167be90acf", "Query Available On Paper");
            this.IsQueryAvailableOnPaperCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.IsQueryAvailableOnPaperCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 19, true);
            this.IsQueryAvailableOnPaperCheckBox.Name = "IsQueryAvailableOnPaperCheckBox";
            this.IsQueryAvailableOnPaperCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.IsQueryAvailableOnPaperCheckBox.TabIndex = 3;
            this.IsQueryAvailableOnPaperCheckBox.UseVisualStyleBackColor = true;
            // 
            // QueryInformationTextBox
            // 
            this.BindingSource.SetBindingMember(this.QueryInformationTextBox, "QueryInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).QueryInformation)));
            this.QueryInformationTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FR NCTS Departure form|9617B6C2-110D-4F5D-999E-F3124BF31B9B", "Query Information");
            this.QueryInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 37, true);
            this.QueryInformationTextBox.Name = "QueryInformationTextBox";
            this.QueryInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
            this.QueryInformationTextBox.TabIndex = 4;
            this.QueryInformationTextBox.TabStop = false;
            // 
			// AuthorisedLocationCodeFindBox
			// 
			this.AuthorisedLocationCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorisedLocationCodeFindBox, "MovementHeader.BM_LocationOfGoodsCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).MovementHeader.BM_LocationOfGoodsCode)));
            this.AuthorisedLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1156777E-E631-46F8-BAC7-7F5A7AA72D3E", "[30] Location Code");
			this.AuthorisedLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 16, true);
			this.AuthorisedLocationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
            this.AuthorisedLocationCodeFindBox.Name = "AuthorisedLocationCodeFindBox";
            this.AuthorisedLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.AuthorisedLocationCodeFindBox.ParentType = null;
            this.AuthorisedLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.AuthorisedLocationCodeFindBox.TabIndex = 0;
			// 
			// IsTC11DeliveredByCustomsCheckBox
			// 
			this.IsTC11DeliveredByCustomsCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsTC11DeliveredByCustomsCheckBox, "IsTC11DeliveredByCustoms");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).IsTC11DeliveredByCustoms)));
            this.IsTC11DeliveredByCustomsCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("d3b9cfbc-e613-468b-9a92-8d221919d358", "TC11 Delivered By Customs");
            this.IsTC11DeliveredByCustomsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.IsTC11DeliveredByCustomsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 65, true);
            this.IsTC11DeliveredByCustomsCheckBox.Name = "IsTC11DeliveredByCustomsCheckBox";
            this.IsTC11DeliveredByCustomsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.IsTC11DeliveredByCustomsCheckBox.TabIndex = 6;
            this.IsTC11DeliveredByCustomsCheckBox.UseVisualStyleBackColor = true;
            // 
            // TC11DateDateEdit
            // 
            this.TC11DateDateEdit.AllowDrop = true;
            this.TC11DateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.TC11DateDateEdit, "TC11Date");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).TC11Date)));
            this.TC11DateDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FR NCTS Departure form|A52636BF-C29A-4BA1-AEE9-DFDE541B1679", "TC11 Date");
            this.TC11DateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 61, true);
            this.TC11DateDateEdit.Name = "TC11DateDateEdit";
            this.TC11DateDateEdit.TabIndex = 5;
            // 
            // ChargePaymentOrDestinationIDDropEdit
            // 
            this.ChargePaymentOrDestinationIDDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ChargePaymentOrDestinationIDDropEdit, "MovementHeader.ChargePaymentOrDestinationID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).MovementHeader.ChargePaymentOrDestinationID)));
            this.ChargePaymentOrDestinationIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 85, true);
            this.ChargePaymentOrDestinationIDDropEdit.Name = "ChargePaymentOrDestinationIDDropEdit";
            this.ChargePaymentOrDestinationIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.ChargePaymentOrDestinationIDDropEdit.TabIndex = 7;
            // 
            // DetailedStatusDropEdit
            // 
            this.DetailedStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DetailedStatusDropEdit, "DetailedDepartureStatusCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).DetailedDepartureStatusCode)));
            this.DetailedStatusDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FR NCTS Departure form|D98ABF40-264F-40A7-B9F8-1B7F9A6A4230", "Detailed status");
            this.DetailedStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 85, true);
            this.DetailedStatusDropEdit.Name = "DetailedStatusDropEdit";
            this.DetailedStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
            this.DetailedStatusDropEdit.TabIndex = 13;
            // 
            // DeclarantDocAddressControl
            // 
            this.DeclarantDocAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeclarantDocAddressControl, "Declarant");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).Declarant)));
            this.DeclarantDocAddressControl.BindToOrganisations = "Lookups.Organisations";
            this.DeclarantDocAddressControl.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FR NCTS Departure form|018FAF38-EC5F-48AA-BA38-F29BFE97D136", "Declarant");
            this.DeclarantDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 583, true);
            this.DeclarantDocAddressControl.Name = "DeclarantDocAddressControl";
            this.DeclarantDocAddressControl.ReadOnly = false;
            this.DeclarantDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
            this.DeclarantDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
            this.DeclarantDocAddressControl.TabIndex = 3;
            this.DeclarantDocAddressControl.ValidationJustForced = false;
            // 
            // SealsNatureDropEdit
            // 
            this.SealsNatureDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SealsNatureDropEdit, "FRNctsHeader.CFN_NatureOfSeals");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.NCTS.NctsHeader)(null)).FRNctsHeader.CFN_NatureOfSeals)));
            this.SealsNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 204, true);
            this.SealsNatureDropEdit.Name = "SealsNatureDropEdit";
            this.SealsNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.SealsNatureDropEdit.TabIndex = 4;
            // 
            // DeclarationDetailsTabUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = false;
			this.Name = "DeclarationDetailsTabUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 775, true);
            this.TraderDetailsGroupBox.ResumeLayout(false);
            this.TraderDetailsGroupBox.PerformLayout();
            this.RepresentativeDocAddressControl.ResumeLayout(true);
            this.RepresentativeDocAddressControl.PerformLayout();
            this.CustomsOfficesGroupBox.ResumeLayout(false);
            this.CustomsOfficesGroupBox.PerformLayout();
            this.GuaranteesGroupBox.ResumeLayout(false);
            this.GuaranteesGroupBox.PerformLayout();
            this.ContainersGroupBox.ResumeLayout(false);
            this.ContainersGroupBox.PerformLayout();
            this.TransportDetailsGroupBox.ResumeLayout(false);
            this.TransportDetailsGroupBox.PerformLayout();
            this.GoodsLocationNormalGroupBox.ResumeLayout(false);
            this.GoodsLocationNormalGroupBox.PerformLayout();
            this.GoodsLocationSimplifiedGroupBox.ResumeLayout(false);
            this.GoodsLocationSimplifiedGroupBox.PerformLayout();
            this.DeclarationDetailsGroupBox.ResumeLayout(false);
            this.DeclarationDetailsGroupBox.PerformLayout();
            this.BrokerFindBox.ResumeLayout(true);
            this.BrokerFindBox.PerformLayout();
            this.CountryOfDestinationDropEdit.ResumeLayout(true);
            this.CountryOfDestinationDropEdit.PerformLayout();
            this.MainDataPanel.ResumeLayout(false);
            this.MainDataPanel.PerformLayout();
            this.LeftDataPanel.ResumeLayout(false);
            this.LeftDataPanel.PerformLayout();
            this.DeclarationTypeDropEdit.ResumeLayout(true);
            this.DeclarationTypeDropEdit.PerformLayout();
            this.CountryOfDispatchDropEdit.ResumeLayout(true);
            this.CountryOfDispatchDropEdit.PerformLayout();
            this.PortOfDispatchFindBox.ResumeLayout(true);
            this.PortOfDispatchFindBox.PerformLayout();
            this.StatusDropEdit.ResumeLayout(true);
            this.StatusDropEdit.PerformLayout();
            this.MessageStatusDropEdit.ResumeLayout(true);
            this.MessageStatusDropEdit.PerformLayout();
            this.MeansOfTransportCrossingBorderNationalityDropEdit.ResumeLayout(true);
            this.MeansOfTransportCrossingBorderNationalityDropEdit.PerformLayout();
            this.TransportModeAtBorderDropEdit.ResumeLayout(true);
            this.TransportModeAtBorderDropEdit.PerformLayout();
            this.MeansOfTransportAtDepartureNationalityDropEdit.ResumeLayout(true);
            this.MeansOfTransportAtDepartureNationalityDropEdit.PerformLayout();
            this.InlandTransportModeDropEdit.ResumeLayout(true);
            this.InlandTransportModeDropEdit.PerformLayout();
            this.PlaceOfLoadingNormalCodeFindBox.ResumeLayout(true);
            this.PlaceOfLoadingNormalCodeFindBox.PerformLayout();
            this.ControlResultDateLimitDateEdit.ResumeLayout(true);
            this.ControlResultDateLimitDateEdit.PerformLayout();
            this.PlaceOfLoadingSimplifiedCodeFindBox.ResumeLayout(true);
            this.PlaceOfLoadingSimplifiedCodeFindBox.PerformLayout();
            this.AgreedLocationOfGoodsCodePanel.ResumeLayout(false);
            this.AgreedLocationOfGoodsCodePanel.PerformLayout();
            this.GoodsLocationNormalMainPanel.ResumeLayout(false);
            this.GoodsLocationNormalMainPanel.PerformLayout();
            this.TirCarnetExpiryDateEdit.ResumeLayout(true);
            this.TirCarnetExpiryDateEdit.PerformLayout();
            this.ValuationDateDateEdit.ResumeLayout(true);
            this.ValuationDateDateEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ControlResultDateLimitDateEdit2.ResumeLayout(true);
            this.ControlResultDateLimitDateEdit2.PerformLayout();
            this.FRSpecificGroupBox.ResumeLayout(false);
            this.FRSpecificGroupBox.PerformLayout();
            this.TC11DateDateEdit.ResumeLayout(true);
            this.TC11DateDateEdit.PerformLayout();
            this.ChargePaymentOrDestinationIDDropEdit.ResumeLayout(true);
            this.ChargePaymentOrDestinationIDDropEdit.PerformLayout();
            this.DetailedStatusDropEdit.ResumeLayout(true);
            this.DetailedStatusDropEdit.PerformLayout();
            this.DeclarantDocAddressControl.ResumeLayout(true);
            this.DeclarantDocAddressControl.PerformLayout();
            this.SealsNatureDropEdit.ResumeLayout(true);
            this.SealsNatureDropEdit.PerformLayout();
            this.AuthorisedLocationCodeFindBox.ResumeLayout(true);
            this.AuthorisedLocationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}
		private ZArchitecture.GUI.ZDropEdit DetailedStatusDropEdit;
		private ZArchitecture.GUI.ZGroupBox FRSpecificGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsPrelodgedMovementCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsQueriedCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsQueryAvailableOnPaperCheckBox;
		private ZArchitecture.ZTextBox QueryInformationTextBox;
		private ZArchitecture.GUI.ZCheckBox IsTC11DeliveredByCustomsCheckBox;
		private ZArchitecture.GUI.ZDateEdit TC11DateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox AuthorisedLocationCodeFindBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl DeclarantDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ControlResultDateLimitDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargePaymentOrDestinationIDDropEdit;
		#endregion

		private ZArchitecture.GUI.ZDropEdit SealsNatureDropEdit;
	}
}
