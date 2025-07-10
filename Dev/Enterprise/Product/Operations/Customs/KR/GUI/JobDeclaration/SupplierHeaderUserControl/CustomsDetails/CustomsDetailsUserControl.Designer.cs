namespace Enterprise.Customs.KR.GUI
{
	partial class CustomsDetailsUserControl
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
            this.BillZGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.CargoManagementNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.COStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ValuationDeclarationStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.BlanketValuationDeclarationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CustomsBrokerCommentMultiTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupplierZOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            this.ShipperZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.EmptyLabel = new Enterprise.ZArchitecture.ZLabel();
            this.OnlineTradeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.OnlineTradeDistributorZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.OnlineTradeSellerZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.OnlineTradeSellingAgentZOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BillZGuidDropEdit.SuspendLayout();
            this.COStatusDropEdit.SuspendLayout();
            this.ValuationDeclarationStatusDropEdit.SuspendLayout();
            this.SupplierZOrganisationFindBox.SuspendLayout();
            this.ShipperZAddressControl.SuspendLayout();
            this.OnlineTradeTypeDropEdit.SuspendLayout();
            this.OnlineTradeDistributorZAddressControl.SuspendLayout();
            this.OnlineTradeSellerZAddressControl.SuspendLayout();
            this.OnlineTradeSellingAgentZOrganisationFindBox.SuspendLayout();
            this.SuspendLayout();
            //
            // BindingSource
            //
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            //
            // BillZGuidDropEdit
            //
            this.BillZGuidDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BillZGuidDropEdit, "Invoices.JZ_CU_RelatedHouseBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_CU_RelatedHouseBill)));
            this.BillZGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 11, true);
            this.BillZGuidDropEdit.Name = "BillZGuidDropEdit";
            this.BillZGuidDropEdit.PreBoundMaxLength = 35;
            this.BillZGuidDropEdit.ShowDescriptionBox = false;
            this.BillZGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 25, true);
            this.BillZGuidDropEdit.TabIndex = 0;
            //
            // CargoManagementNoTextBox
            //
            this.BindingSource.SetBindingMember(this.CargoManagementNoTextBox, "Invoices.JZ_ImportCargoManagementNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ImportCargoManagementNumber)));
            this.CargoManagementNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 33, true);
            this.CargoManagementNoTextBox.Name = "CargoManagementNoTextBox";
            this.CargoManagementNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.CargoManagementNoTextBox.TabIndex = 1;
            //
            // COStatusDropEdit
            //
            this.COStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COStatusDropEdit, "Invoices.JZ_COOStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_COOStatus)));
            this.COStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 54, true);
            this.COStatusDropEdit.Name = "COStatusDropEdit";
            this.COStatusDropEdit.PreBoundMaxLength = 1;
            this.COStatusDropEdit.ShowDescriptionBox = false;
            this.COStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
            this.COStatusDropEdit.TabIndex = 2;
            //
            // ValuationDeclarationStatusDropEdit
            //
            this.ValuationDeclarationStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ValuationDeclarationStatusDropEdit, "Invoices.JZ_ValuationDecAttachCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationDecAttachCode)));
            this.ValuationDeclarationStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 75, true);
            this.ValuationDeclarationStatusDropEdit.Name = "ValuationDeclarationStatusDropEdit";
            this.ValuationDeclarationStatusDropEdit.PreBoundMaxLength = 1;
            this.ValuationDeclarationStatusDropEdit.ShowDescriptionBox = false;
            this.ValuationDeclarationStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
            this.ValuationDeclarationStatusDropEdit.TabIndex = 3;
            //
            // BlanketValuationDeclarationNoTextBox
            //
            this.BindingSource.SetBindingMember(this.BlanketValuationDeclarationNoTextBox, "Invoices.JZ_BlanketValuationDeclarationNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_BlanketValuationDeclarationNumber)));
            this.BlanketValuationDeclarationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 97, true);
            this.BlanketValuationDeclarationNoTextBox.Name = "BlanketValuationDeclarationNoTextBox";
            this.BlanketValuationDeclarationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.BlanketValuationDeclarationNoTextBox.TabIndex = 4;
            //
            // CustomsBrokerCommentMultiTextBox
            //
            this.BindingSource.SetBindingMember(this.CustomsBrokerCommentMultiTextBox, "Invoices.JZ_Remarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Remarks)));
            this.CustomsBrokerCommentMultiTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 118, true);
            this.CustomsBrokerCommentMultiTextBox.Multiline = true;
            this.CustomsBrokerCommentMultiTextBox.Name = "CustomsBrokerCommentMultiTextBox";
            this.CustomsBrokerCommentMultiTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 49, true);
            this.CustomsBrokerCommentMultiTextBox.TabIndex = 5;
            //
            // SupplierZOrganisationFindBox
            //
            this.SupplierZOrganisationFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplierZOrganisationFindBox, "Invoices.JZ_OH_Supplier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Supplier)));
            this.SupplierZOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 47, true);
            this.SupplierZOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.SupplierZOrganisationFindBox.Name = "SupplierZOrganisationFindBox";
            this.SupplierZOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SupplierZOrganisationFindBox.ParentType = null;
            this.SupplierZOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 17, true);
            this.SupplierZOrganisationFindBox.TabIndex = 6;
            //
            // ShipperZAddressControl
            //
            this.ShipperZAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipperZAddressControl, "Invoices.JZ_OA_ShipperAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OA_ShipperAddress)));
            this.ShipperZAddressControl.BindToOrgList = "Lookups.Organisations";
            this.ShipperZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 74, true);
            this.ShipperZAddressControl.Name = "ShipperZAddressControl";
            this.ShipperZAddressControl.PopupCaption = "";
            this.ShipperZAddressControl.ShowAddress = false;
            this.ShipperZAddressControl.ShowOrganisationName = true;
            this.ShipperZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 17, true);
            this.ShipperZAddressControl.TabIndex = 7;
            //
            // EmptyLabel
            //
            this.EmptyLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a6a7f832-b5d1-4049-b8d0-89ce17d97830", " ");
            this.EmptyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.EmptyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 93, true);
            this.EmptyLabel.Name = "EmptyLabel";
            this.EmptyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 15, true);
            this.EmptyLabel.TabIndex = 8;
            this.EmptyLabel.UseMnemonic = false;
            //
            // OnlineTradeTypeDropEdit
            //
            this.OnlineTradeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OnlineTradeTypeDropEdit, "Invoices.JZ_OnlineTradeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OnlineTradeType)));
            this.OnlineTradeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 110, true);
            this.OnlineTradeTypeDropEdit.Name = "OnlineTradeTypeDropEdit";
            this.OnlineTradeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 17, true);
            this.OnlineTradeTypeDropEdit.TabIndex = 9;
            //
            // OnlineTradeDistributorZAddressControl
            //
            this.OnlineTradeDistributorZAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OnlineTradeDistributorZAddressControl, "Invoices.JZ_OA_DistributorAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OA_DistributorAddress)));
            this.OnlineTradeDistributorZAddressControl.BindToOrgList = "Lookups.Organisations";
            this.OnlineTradeDistributorZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 134, true);
            this.OnlineTradeDistributorZAddressControl.Name = "OnlineTradeDistributorZAddressControl";
            this.OnlineTradeDistributorZAddressControl.PopupCaption = "";
            this.OnlineTradeDistributorZAddressControl.ShowAddress = false;
            this.OnlineTradeDistributorZAddressControl.ShowOrganisationName = true;
            this.OnlineTradeDistributorZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 17, true);
            this.OnlineTradeDistributorZAddressControl.TabIndex = 10;
            //
            // OnlineTradeSellerZAddressControl
            //
            this.OnlineTradeSellerZAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OnlineTradeSellerZAddressControl, "Invoices.JZ_OA_SellerAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OA_SellerAddress)));
            this.OnlineTradeSellerZAddressControl.BindToOrgList = "Lookups.Organisations";
            this.OnlineTradeSellerZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 155, true);
            this.OnlineTradeSellerZAddressControl.Name = "OnlineTradeSellerZAddressControl";
            this.OnlineTradeSellerZAddressControl.PopupCaption = "";
            this.OnlineTradeSellerZAddressControl.ShowAddress = false;
            this.OnlineTradeSellerZAddressControl.ShowOrganisationName = true;
            this.OnlineTradeSellerZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 17, true);
            this.OnlineTradeSellerZAddressControl.TabIndex = 11;
            //
            // OnlineTradeSellingAgentZOrganisationFindBox
            //
            this.OnlineTradeSellingAgentZOrganisationFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OnlineTradeSellingAgentZOrganisationFindBox, "Invoices.JZ_OH_SellingAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_SellingAgent)));
            this.OnlineTradeSellingAgentZOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 176, true);
            this.OnlineTradeSellingAgentZOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.OnlineTradeSellingAgentZOrganisationFindBox.Name = "OnlineTradeSellingAgentZOrganisationFindBox";
            this.OnlineTradeSellingAgentZOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OnlineTradeSellingAgentZOrganisationFindBox.ParentType = null;
            this.OnlineTradeSellingAgentZOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 17, true);
            this.OnlineTradeSellingAgentZOrganisationFindBox.TabIndex = 12;
            //
            // CustomsDetailsUserControl
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.OnlineTradeSellingAgentZOrganisationFindBox);
            this.Controls.Add(this.OnlineTradeSellerZAddressControl);
            this.Controls.Add(this.OnlineTradeDistributorZAddressControl);
            this.Controls.Add(this.OnlineTradeTypeDropEdit);
            this.Controls.Add(this.EmptyLabel);
            this.Controls.Add(this.ShipperZAddressControl);
            this.Controls.Add(this.SupplierZOrganisationFindBox);
            this.Controls.Add(this.CustomsBrokerCommentMultiTextBox);
            this.Controls.Add(this.BlanketValuationDeclarationNoTextBox);
            this.Controls.Add(this.ValuationDeclarationStatusDropEdit);
            this.Controls.Add(this.COStatusDropEdit);
            this.Controls.Add(this.CargoManagementNoTextBox);
            this.Controls.Add(this.BillZGuidDropEdit);
            this.Name = "CustomsDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 202, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BillZGuidDropEdit.ResumeLayout(true);
            this.BillZGuidDropEdit.PerformLayout();
            this.COStatusDropEdit.ResumeLayout(true);
            this.COStatusDropEdit.PerformLayout();
            this.ValuationDeclarationStatusDropEdit.ResumeLayout(true);
            this.ValuationDeclarationStatusDropEdit.PerformLayout();
            this.SupplierZOrganisationFindBox.ResumeLayout(true);
            this.SupplierZOrganisationFindBox.PerformLayout();
            this.ShipperZAddressControl.ResumeLayout(true);
            this.ShipperZAddressControl.PerformLayout();
            this.OnlineTradeTypeDropEdit.ResumeLayout(true);
            this.OnlineTradeTypeDropEdit.PerformLayout();
            this.OnlineTradeDistributorZAddressControl.ResumeLayout(true);
            this.OnlineTradeDistributorZAddressControl.PerformLayout();
            this.OnlineTradeSellerZAddressControl.ResumeLayout(true);
            this.OnlineTradeSellerZAddressControl.PerformLayout();
            this.OnlineTradeSellingAgentZOrganisationFindBox.ResumeLayout(true);
            this.OnlineTradeSellingAgentZOrganisationFindBox.PerformLayout();
			this.CaptionRenderingEnabled = true;
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGuidDropEdit BillZGuidDropEdit;
		public ZArchitecture.ZTextBox CargoManagementNoTextBox;
		public ZArchitecture.GUI.ZDropEdit COStatusDropEdit;
		public ZArchitecture.GUI.ZDropEdit ValuationDeclarationStatusDropEdit;
		public ZArchitecture.ZTextBox BlanketValuationDeclarationNoTextBox;
		public ZArchitecture.ZTextBox CustomsBrokerCommentMultiTextBox;
		public Enterprise.MasterFiles.GUI.ZOrganisationFindBox SupplierZOrganisationFindBox;
		public ZArchitecture.GUI.ZAddressControl ShipperZAddressControl;
		public ZArchitecture.ZLabel EmptyLabel;
		public ZArchitecture.GUI.ZDropEdit OnlineTradeTypeDropEdit;
		public ZArchitecture.GUI.ZAddressControl OnlineTradeDistributorZAddressControl;
		public ZArchitecture.GUI.ZAddressControl OnlineTradeSellerZAddressControl;
		public Enterprise.MasterFiles.GUI.ZOrganisationFindBox OnlineTradeSellingAgentZOrganisationFindBox;
	}
}
