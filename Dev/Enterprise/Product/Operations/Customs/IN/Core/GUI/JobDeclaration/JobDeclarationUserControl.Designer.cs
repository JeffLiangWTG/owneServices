using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.IN.GUI;

partial class JobDeclarationUserControl
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
		if (disposing)
		{
			components?.Dispose();
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
			this.ImporterDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SupplierDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeclarationOtherDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExportOrientedUnitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExportOrientedUnitsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.FinalDestinationFindBox.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit2.SuspendLayout();
			this.JE_ExportDateBoundDateEdit2.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ShipmentCustomFieldsPage.SuspendLayout();
			this.shipmentCustomFieldsControl1.SuspendLayout();
			this.OrdersPanel.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.OrdersAttachUserControl.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterDocAddress.SuspendLayout();
			this.SupplierDocAddress.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ExportOrientedUnitsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 20, true);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 220, true);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 372, true);
			// 
			// RightTabControl
			// 
			this.RightTabControl.Controls.Add(this.DetailsTabPage);
			this.RightTabControl.Controls.Add(this.ExportOrientedUnitsTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 8, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 549, true);
			this.RightTabControl.Controls.SetChildIndex(this.NumbersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.ShipmentCustomFieldsPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.ExportOrientedUnitsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrdersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrganisationsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.DocsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.DetailsTabPage, 0);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 210, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 522, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 418, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 210, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 514, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 43, true);
			this.DeclarationDetailsGroupBox.Visible = false;
			// 
			// ImporterDocAddress
			// 
			this.ImporterDocAddress.AddressValidationProcessCmdKey = null;
			this.ImporterDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterDocAddress, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ImporterDocAddress.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterDocAddress.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("1DE4504E-094B-477A-A1CB-58555DE9DE67", "Importer");
			this.ImporterDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.ImporterDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.ImporterDocAddress.Name = "ImporterDocAddress";
			this.ImporterDocAddress.ReadOnly = false;
			this.ImporterDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.ImporterDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterDocAddress.TabIndex = 1;
			this.ImporterDocAddress.ValidationJustForced = false;
			// 
			// SupplierDocAddress
			// 
			this.SupplierDocAddress.AddressValidationProcessCmdKey = null;
			this.SupplierDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddress, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddress.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocAddress.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("F9BAE2DA-A6AA-45DC-85A0-EEEB22EF5CCE", "Main Supplier");
			this.SupplierDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.SupplierDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.SupplierDocAddress.Name = "SupplierDocAddress";
			this.SupplierDocAddress.ReadOnly = false;
			this.SupplierDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddress.TabIndex = 0;
			this.SupplierDocAddress.ValidationJustForced = false;
			// 
			// DeclarationOtherDetailsLayoutPanel
			// 
			this.DeclarationOtherDetailsLayoutPanel.AllowDrop = true;
			this.DeclarationOtherDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationOtherDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationOtherDetailsLayoutPanel.Name = "DeclarationOtherDetailsLayoutPanel";
			this.DeclarationOtherDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			this.DeclarationOtherDetailsLayoutPanel.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("7D34205C-97D4-4467-B673-72F8FB8354E7", "Details");
			this.DetailsTabPage.Controls.Add(this.DeclarationOtherDetailsLayoutPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 480, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// ExportOrientedUnitsTabPage
			// 
			this.ExportOrientedUnitsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("a5c6b288-e523-4edf-9593-d54d477a7e3a", "EOU");
			this.ExportOrientedUnitsTabPage.Controls.Add(this.ExportOrientedUnitsLayoutPanel);
			this.ExportOrientedUnitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ExportOrientedUnitsTabPage.Name = "ExportOrientedUnitsTabPage";
			this.ExportOrientedUnitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 526, true);
			this.ExportOrientedUnitsTabPage.TabIndex = 7;
			// 
			// ExportOrientedUnitsLayoutPanel
			// 
			this.ExportOrientedUnitsLayoutPanel.AllowDrop = true;
			this.ExportOrientedUnitsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportOrientedUnitsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportOrientedUnitsLayoutPanel.Name = "ExportOrientedUnitsLayoutPanel";
			this.ExportOrientedUnitsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 526, true);
			this.ExportOrientedUnitsLayoutPanel.TabIndex = 0;
			// 
			// JobDeclarationUserControl
			// 
			this.Controls.Add(this.SupplierDocAddress);
			this.Controls.Add(this.ImporterDocAddress);
			this.Name = "JobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 559, true);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterDocAddress, 0);
			this.Controls.SetChildIndex(this.SupplierDocAddress, 0);
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.JE_MasterBillForAirBoundTextBox.ResumeLayout(true);
			this.JE_MasterBillForAirBoundTextBox.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.JE_ExportDateBoundDateEdit.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit2.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit2.PerformLayout();
			this.JE_ExportDateBoundDateEdit2.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit2.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.OrganisationsTopPanel.ResumeLayout(false);
			this.OrganisationsTopPanel.PerformLayout();
			this.ShippingOrAirLineOrganisationControl.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ShipmentCustomFieldsPage.ResumeLayout(false);
			this.ShipmentCustomFieldsPage.PerformLayout();
			this.shipmentCustomFieldsControl1.ResumeLayout(true);
			this.shipmentCustomFieldsControl1.PerformLayout();
			this.OrdersPanel.ResumeLayout(false);
			this.OrdersPanel.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.OrdersAttachUserControl.ResumeLayout(true);
			this.OrdersAttachUserControl.PerformLayout();
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterDocAddress.ResumeLayout(true);
			this.ImporterDocAddress.PerformLayout();
			this.SupplierDocAddress.ResumeLayout(true);
			this.SupplierDocAddress.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ExportOrientedUnitsTabPage.ResumeLayout(false);
			this.ExportOrientedUnitsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZTabPage DetailsTabPage;
	internal ZArchitecture.GUI.DynamicLayoutPanel DeclarationOtherDetailsLayoutPanel;
	internal ZDocAddressControl ImporterDocAddress;
	internal ZDocAddressControl SupplierDocAddress;
	internal ZArchitecture.GUI.ZTabPage ExportOrientedUnitsTabPage;
	internal ZArchitecture.GUI.DynamicLayoutPanel ExportOrientedUnitsLayoutPanel;
}
