namespace Enterprise.Customs.CA.GUI
{
	partial class LVSSubHeaderAllDetailsUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ComInvoiceDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LVSSubHeaderDetailsUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeaderDetailsUserControl();
			this.ChargesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LVSSubHeaderChargesUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeaderChargesUserControl();
			this.CasualImportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LVSSubHeaderCasualImportUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeaderCasualImportUserControl();
			this.OrganizationAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ShipperDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvCustomFieldsDisplayControl = new Enterprise.Customs.GUI.InvoiceHeaderCustomFieldsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.LVSSubHeaderDetailsUserControl.SuspendLayout();
			this.ChargesTabPage.SuspendLayout();
			this.LVSSubHeaderChargesUserControl.SuspendLayout();
			this.CasualImportTabPage.SuspendLayout();
			this.LVSSubHeaderCasualImportUserControl.SuspendLayout();
			this.OrganizationAddressTabPage.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.ShipperDocAddressControl.SuspendLayout();
			this.DeliveryDocAddressControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceHeader);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderAllDetailsUserControl|ee7a65db-76d1-4a29-8c34-ffcd98958dc6", "LVS Shipment Details");
			this.DetailsGroupBox.Controls.Add(this.DetailsTabControl);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 202, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 263, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.ComInvoiceDetailsTabPage);
			this.DetailsTabControl.Controls.Add(this.ChargesTabPage);
			this.DetailsTabControl.Controls.Add(this.CasualImportTabPage);
			this.DetailsTabControl.Controls.Add(this.OrganizationAddressTabPage);
			this.DetailsTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 217, true);
			this.DetailsTabControl.TabIndex = 0;
			// 
			// ComInvoiceDetailsTabPage
			// 
			this.ComInvoiceDetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderAllDetailsUserControl|20384db0-fce8-40b8-8e59-9297c0f3357b", "Details");
			this.ComInvoiceDetailsTabPage.Controls.Add(this.LVSSubHeaderDetailsUserControl);
			this.ComInvoiceDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ComInvoiceDetailsTabPage.Name = "ComInvoiceDetailsTabPage";
			this.ComInvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 129, true);
			this.ComInvoiceDetailsTabPage.TabIndex = 0;
			// 
			// LVSSubHeaderDetailsUserControl
			// 
			this.LVSSubHeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSSubHeaderDetailsUserControl, ".");
			this.LVSSubHeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSubHeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSSubHeaderDetailsUserControl.Name = "LVSSubHeaderDetailsUserControl";
			this.LVSSubHeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 129, true);
			this.LVSSubHeaderDetailsUserControl.TabIndex = 0;
			// 
			// ChargesTabPage
			// 
			this.ChargesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderAllDetailsUserControl|a118258a-4e6a-4e7c-8d52-de103f0213c6", "Charges");
			this.ChargesTabPage.Controls.Add(this.LVSSubHeaderChargesUserControl);
			this.ChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChargesTabPage.Name = "ChargesTabPage";
			this.ChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 129, true);
			this.ChargesTabPage.TabIndex = 1;
			// 
			// LVSSubHeaderChargesUserControl
			// 
			this.LVSSubHeaderChargesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSSubHeaderChargesUserControl, ".");
			this.LVSSubHeaderChargesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSubHeaderChargesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LVSSubHeaderChargesUserControl.Name = "LVSSubHeaderChargesUserControl";
			this.LVSSubHeaderChargesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 123, true);
			this.LVSSubHeaderChargesUserControl.TabIndex = 0;
			// 
			// CasualImportTabPage
			// 
			this.CasualImportTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|07cb29cb-fbde-41e2-b3e2-15558787caa2", "Casual Import");
			this.CasualImportTabPage.Controls.Add(this.LVSSubHeaderCasualImportUserControl);
			this.CasualImportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CasualImportTabPage.Name = "CasualImportTabPage";
			this.CasualImportTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CasualImportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 129, true);
			this.CasualImportTabPage.TabIndex = 2;
			// 
			// LVSSubHeaderCasualImportUserControl
			// 
			this.LVSSubHeaderCasualImportUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSSubHeaderCasualImportUserControl, ".");
			this.LVSSubHeaderCasualImportUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSubHeaderCasualImportUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LVSSubHeaderCasualImportUserControl.Name = "LVSSubHeaderCasualImportUserControl";
			this.LVSSubHeaderCasualImportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 123, true);
			this.LVSSubHeaderCasualImportUserControl.TabIndex = 0;
			// 
			// OrganizationAddressTabPage
			// 
			this.OrganizationAddressTabPage.AutoScroll = true;
			this.OrganizationAddressTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2e086ec-acd4-4c2d-9d55-c0160c4123d8", "Organizations and Addresses");
			this.OrganizationAddressTabPage.Controls.Add(this.DeliveryDocAddressControl);
			this.OrganizationAddressTabPage.Controls.Add(this.SupplierDocAddressControl);
			this.OrganizationAddressTabPage.Controls.Add(this.ShipperDocAddressControl);
			this.OrganizationAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrganizationAddressTabPage.Name = "OrganizationAddressTabPage";
			this.OrganizationAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 190, true);
			this.OrganizationAddressTabPage.TabIndex = 3;
			// 
			// SupplierDocAddressControl
			// 
			this.SupplierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.SupplierDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e26adc28-ffb3-4983-85cc-d7d273a327b2", "Vendor");
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.SupplierDocAddressControl.TabIndex = 1;
			// 
			// DeliveryDocAddressControl
			// 
			this.DeliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "JobDeclaration.ImporterDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JobDeclaration.ImporterDeliveryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "Lookups+ImportersList";
			this.DeliveryDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("53686bae-29af-45c8-87c7-4ac3711bcc2d", "Delivery Address");
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 3, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 2;
			// 
			// ShipperDocAddressControl
			// 
			this.ShipperDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperDocAddressControl, "SupplierPickupDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).SupplierPickupDeliveryAddress)));
			this.ShipperDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b5918627-0082-4f3a-91fa-595b047a4738", "Shipper");
			this.ShipperDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.ShowOverrideAndTabs;
			this.ShipperDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 3, true);
			this.ShipperDocAddressControl.Name = "ShipperDocAddressControl";
			this.ShipperDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ShipperDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 183, true);
			this.ShipperDocAddressControl.TabIndex = 3;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Controls.Add(InvCustomFieldsDisplayControl);
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d417c0b8-abbf-49b8-804a-b7c92186d9e5", "Custom Fields");
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 190, true);
			this.CustomFieldsTabPage.TabIndex = 4;
			// 
			// InvCustomFieldsDisplayControl
			// 
			this.BindingSource.SetBindingMember(this.InvCustomFieldsDisplayControl, ".");
			this.InvCustomFieldsDisplayControl.AllowDrop = true;
			this.InvCustomFieldsDisplayControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvCustomFieldsDisplayControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvCustomFieldsDisplayControl.Name = "InvCustomFieldsDisplayControl";
			this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 341, true);
			this.InvCustomFieldsDisplayControl.TabIndex = 0;
			// 
			// LVSSubHeaderAllDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "LVSSubHeaderAllDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.LVSSubHeaderDetailsUserControl.ResumeLayout(true);
			this.LVSSubHeaderDetailsUserControl.PerformLayout();
			this.ChargesTabPage.ResumeLayout(false);
			this.ChargesTabPage.PerformLayout();
			this.LVSSubHeaderChargesUserControl.ResumeLayout(true);
			this.LVSSubHeaderChargesUserControl.PerformLayout();
			this.CasualImportTabPage.ResumeLayout(false);
			this.CasualImportTabPage.PerformLayout();
			this.LVSSubHeaderCasualImportUserControl.ResumeLayout(true);
			this.LVSSubHeaderCasualImportUserControl.PerformLayout();
			this.OrganizationAddressTabPage.ResumeLayout(false);
			this.OrganizationAddressTabPage.PerformLayout();
			this.DeliveryDocAddressControl.ResumeLayout(true);
			this.DeliveryDocAddressControl.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();
			this.ShipperDocAddressControl.ResumeLayout(true);
			this.ShipperDocAddressControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZTemplateTabControl DetailsTabControl;
		private ZArchitecture.GUI.ZTabPage ComInvoiceDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage ChargesTabPage;
		private ZArchitecture.GUI.ZTabPage CasualImportTabPage;
		private LVSSubHeaderDetailsUserControl LVSSubHeaderDetailsUserControl;
		private LVSSubHeaderChargesUserControl LVSSubHeaderChargesUserControl;
		private LVSSubHeaderCasualImportUserControl LVSSubHeaderCasualImportUserControl;
		private ZArchitecture.GUI.ZTabPage OrganizationAddressTabPage;
		private MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private Enterprise.Customs.GUI.InvoiceHeaderCustomFieldsUserControl InvCustomFieldsDisplayControl;
		private MasterFiles.GUI.ZDocAddressControl SupplierDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl ShipperDocAddressControl;
	}
}
