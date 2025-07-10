namespace Enterprise.Customs.ES.NCTS.GUI
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
			UnhookEvents(currentHeader);
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
            this.NationalSimplificatorIndDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AgreedLocationOfGoodsCodeNormalDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DeclEmailAddrTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ClearanceInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TADPrintTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ArrivalLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ClearanceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ClearanceProcedureTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ClearanceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
            this.NationalSimplificatorIndDropEdit.SuspendLayout();
            this.AgreedLocationOfGoodsCodeNormalDropEdit.SuspendLayout();
            this.ClearanceInfoGroupBox.SuspendLayout();
            this.ArrivalLimitDateEdit.SuspendLayout();
            this.ClearanceDateDateEdit.SuspendLayout();
            this.CertificateDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // TraderDetailsGroupBox
            // 
            this.TraderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 722, true);
            // 
            // RepresentativeDocAddressControl
            // 
            this.RepresentativeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 265, true);
            // 
            // CustomsOfficesGroupBox
            // 
            this.CustomsOfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 184, true);
            // 
            // GuaranteesGroupBox
            // 
            this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 184, true);
            // 
            // ContainersGroupBox
            // 
            this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 194, true);
            // 
            // TransportDetailsGroupBox
            // 
            this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 379, true);
            // 
            // GoodsLocationNormalGroupBox
            // 
            this.GoodsLocationNormalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 535, true);
            // 
            // GoodsLocationSimplifiedGroupBox
            // 
            this.GoodsLocationSimplifiedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 660, true);
            // 
            // DeclarationDetailsGroupBox
            // 
            this.DeclarationDetailsGroupBox.Controls.Add(this.DeclEmailAddrTextBox);
            this.DeclarationDetailsGroupBox.Controls.Add(this.NationalSimplificatorIndDropEdit);
            this.DeclarationDetailsGroupBox.Controls.Add(this.CertificateDropEdit);
            this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 379, true);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.PortOfDispatchFindBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ValuationDateDateEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.CertificateDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.RepresentativeDocAddressControl, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.BrokerFindBox, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.OverrideFreightDefaults, 0);
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
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.NationalSimplificatorIndDropEdit, 0);
            this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DeclEmailAddrTextBox, 0);
            // 
            // BrokerFindBox
            // 
            this.BrokerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 309, true);
            // 
            // CountryOfDestinationDropEdit
            // 
            this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 243, true);
            // 
            // MainDataPanel
            // 
            this.MainDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 722, true);
            // 
            // LeftDataPanel
            // 
            this.LeftDataPanel.Controls.Add(this.ClearanceInfoGroupBox);
            this.LeftDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 722, true);
            this.LeftDataPanel.Controls.SetChildIndex(this.CustomsOfficesGroupBox, 0);
            this.LeftDataPanel.Controls.SetChildIndex(this.GuaranteesGroupBox, 0);
            this.LeftDataPanel.Controls.SetChildIndex(this.ContainersGroupBox, 0);
            this.LeftDataPanel.Controls.SetChildIndex(this.ClearanceInfoGroupBox, 0);
            // 
            // DeclarationTypeDropEdit
            // 
            this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 155, true);
            // 
            // CountryOfDispatchDropEdit
            // 
            this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 221, true);
            // 
            // PortOfDispatchFindBox
            // 
            this.PortOfDispatchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 221, true);
            // 
            // AgreedLocationOfGoodsCodeNormalTextBox
            // 
            this.AgreedLocationOfGoodsCodeNormalTextBox.Visible = false;
            // 
            // AgreedLocationOfGoodsCodePanel
            // 
            this.AgreedLocationOfGoodsCodePanel.Controls.Add(this.AgreedLocationOfGoodsCodeNormalDropEdit);
            this.AgreedLocationOfGoodsCodePanel.Controls.SetChildIndex(this.AgreedLocationOfGoodsCodeNormalDropEdit, 0);
            this.AgreedLocationOfGoodsCodePanel.Controls.SetChildIndex(this.AgreedLocationOfGoodsCodeNormalTextBox, 0);
            this.AgreedLocationOfGoodsCodePanel.Controls.SetChildIndex(this.PreLodgedForAgreedLocationOfGoodsCodeTickBox, 0);
            // 
            // TirCarnetNumberTextBox
            // 
            this.TirCarnetNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 177, true);
            // 
            // TirCarnetExpiryDateEdit
            // 
            this.TirCarnetExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 177, true);
            // 
            // ContainersAndSealsZDynamicUserControl
            // 
            this.ContainersAndSealsZDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 175, true);
            // 
            // ValuationDateDateEdit
            // 
            this.ValuationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 353, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
            // 
            // NationalSimplificatorIndDropEdit
            // 
            this.NationalSimplificatorIndDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NationalSimplificatorIndDropEdit, "NationalSimplificatorInd");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).NationalSimplificatorInd)));
            this.NationalSimplificatorIndDropEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("ACCDE432-B774-4619-B961-EB342972FF7D", "National Simplification Ind.");
            this.NationalSimplificatorIndDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 133, true);
            this.NationalSimplificatorIndDropEdit.Name = "NationalSimplificatorIndDropEdit";
            this.NationalSimplificatorIndDropEdit.PreBoundMaxLength = 3;
            this.NationalSimplificatorIndDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
            this.NationalSimplificatorIndDropEdit.TabIndex = 7;
            // 
            // AgreedLocationOfGoodsCodeNormalDropEdit
            // 
            this.AgreedLocationOfGoodsCodeNormalDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AgreedLocationOfGoodsCodeNormalDropEdit, "MovementHeader.BM_LocationOfGoodsCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_LocationOfGoodsCode)));
            this.AgreedLocationOfGoodsCodeNormalDropEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("B2CB353D-85B7-4EB4-BC37-5F658B6B805D", "[30] Agreed Location Code");
            this.AgreedLocationOfGoodsCodeNormalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 7, true);
            this.AgreedLocationOfGoodsCodeNormalDropEdit.Name = "AgreedLocationOfGoodsCodeNormalDropEdit";
            this.AgreedLocationOfGoodsCodeNormalDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
            this.AgreedLocationOfGoodsCodeNormalDropEdit.TabIndex = 2;
            // 
            // DeclEmailAddrTextBox
            // 
            this.BindingSource.SetBindingMember(this.DeclEmailAddrTextBox, "DeclEmailAddr");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).DeclEmailAddr)));
            this.DeclEmailAddrTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("195262EB-C98A-4721-A10B-92150E0F2AEF", "Declaration Email");
            this.DeclEmailAddrTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 287, true);
            this.DeclEmailAddrTextBox.Name = "DeclEmailAddrTextBox";
            this.DeclEmailAddrTextBox.ReadOnly = true;
            this.DeclEmailAddrTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
            this.DeclEmailAddrTextBox.TabIndex = 13;
            // 
            // ClearanceInfoGroupBox
            // 
            this.ClearanceInfoGroupBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("f9f0bf14-2305-4fd1-b47f-58481497d7fe", "Clearance Info");
            this.ClearanceInfoGroupBox.Controls.Add(this.TADPrintTextBox);
            this.ClearanceInfoGroupBox.Controls.Add(this.ArrivalLimitDateEdit);
            this.ClearanceInfoGroupBox.Controls.Add(this.ClearanceDateDateEdit);
            this.ClearanceInfoGroupBox.Controls.Add(this.ClearanceProcedureTextBox);
            this.ClearanceInfoGroupBox.Controls.Add(this.ClearanceNumberTextBox);
            this.ClearanceInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 568, true);
            this.ClearanceInfoGroupBox.Name = "ClearanceInfoGroupBox";
            this.ClearanceInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 130, true);
            this.ClearanceInfoGroupBox.TabIndex = 5;
            this.ClearanceInfoGroupBox.TabStop = false;
            // 
            // TADPrintTextBox
            // 
            this.BindingSource.SetBindingMember(this.TADPrintTextBox, "TADPrintProcedureDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).TADPrintProcedureDescription)));
            this.TADPrintTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("24442e0e-8fee-4b86-b797-9c07c8a103d1", "TAD Print");
            this.TADPrintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 84, true);
            this.TADPrintTextBox.Name = "TADPrintTextBox";
            this.TADPrintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
            this.TADPrintTextBox.TabIndex = 19;
            // 
            // ArrivalLimitDateEdit
            // 
            this.ArrivalLimitDateEdit.AllowDrop = true;
            this.ArrivalLimitDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ArrivalLimitDateEdit, "ArrivalLimit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalLimit)));
            this.ArrivalLimitDateEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("6f167fe0-7dc7-4ca8-acfc-5bdcbdfa74d0", "Arrival Limit");
            this.ArrivalLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 58, true);
            this.ArrivalLimitDateEdit.Name = "ArrivalLimitDateEdit";
            this.ArrivalLimitDateEdit.TabIndex = 18;
            // 
            // ClearanceDateDateEdit
            // 
            this.ClearanceDateDateEdit.AllowDrop = true;
            this.ClearanceDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ClearanceDateDateEdit, "ClearanceDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ClearanceDate)));
            this.ClearanceDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("ed197766-7f37-4445-9601-c74f8ab9442d", "Clearance Date");
            this.ClearanceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 32, true);
            this.ClearanceDateDateEdit.Name = "ClearanceDateDateEdit";
            this.ClearanceDateDateEdit.TabIndex = 17;
            // 
            // ClearanceProcedureTextBox
            // 
            this.BindingSource.SetBindingMember(this.ClearanceProcedureTextBox, "ClearanceCriteriaDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ClearanceCriteriaDescription)));
            this.ClearanceProcedureTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("c0f156ca-1b44-4fcb-b065-f1136cf3fa51", "Clearance Procedure");
            this.ClearanceProcedureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 58, true);
            this.ClearanceProcedureTextBox.Name = "ClearanceProcedureTextBox";
            this.ClearanceProcedureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
            this.ClearanceProcedureTextBox.TabIndex = 16;
            // 
            // ClearanceNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ClearanceNumberTextBox, "ClearanceReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ClearanceReferenceNumber)));
            this.ClearanceNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("99cf2f8b-a81d-45ae-b3d0-1d43548b0c96", "Clearance Number");
            this.ClearanceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 32, true);
            this.ClearanceNumberTextBox.Name = "ClearanceNumberTextBox";
            this.ClearanceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
            this.ClearanceNumberTextBox.TabIndex = 14;
            // 
            // CertificateDropEdit
            // 
            this.CertificateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CertificateDropEdit, "BH_CustomsProfile");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).BH_CustomsProfile)));
            this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 331, true);
            this.CertificateDropEdit.Name = "CertificateDropEdit";
            this.CertificateDropEdit.PreBoundMaxLength = 50;
            this.CertificateDropEdit.ShowDescriptionBox = false;
            this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
            this.CertificateDropEdit.TabIndex = 15;
            // 
            // DeclarationDetailsTabUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "DeclarationDetailsTabUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1286, 722, true);
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
            this.NationalSimplificatorIndDropEdit.ResumeLayout(true);
            this.NationalSimplificatorIndDropEdit.PerformLayout();
            this.AgreedLocationOfGoodsCodeNormalDropEdit.ResumeLayout(true);
            this.AgreedLocationOfGoodsCodeNormalDropEdit.PerformLayout();
            this.ClearanceInfoGroupBox.ResumeLayout(false);
            this.ClearanceInfoGroupBox.PerformLayout();
            this.ArrivalLimitDateEdit.ResumeLayout(true);
            this.ArrivalLimitDateEdit.PerformLayout();
            this.ClearanceDateDateEdit.ResumeLayout(true);
            this.ClearanceDateDateEdit.PerformLayout();
            this.CertificateDropEdit.ResumeLayout(true);
            this.CertificateDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit AgreedLocationOfGoodsCodeNormalDropEdit;
		private ZArchitecture.GUI.ZDropEdit NationalSimplificatorIndDropEdit;
		private Enterprise.ZArchitecture.ZTextBox DeclEmailAddrTextBox;
		private ZArchitecture.ZTextBox ClearanceNumberTextBox;
		private ZArchitecture.ZTextBox ClearanceProcedureTextBox;
		private ZArchitecture.GUI.ZDateEdit ArrivalLimitDateEdit;
		private ZArchitecture.GUI.ZDateEdit ClearanceDateDateEdit;
		private ZArchitecture.ZTextBox TADPrintTextBox;
		protected ZArchitecture.GUI.ZGroupBox ClearanceInfoGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
	}
}
