namespace Enterprise.Customs.DE.GUI
{
	partial class ImportOrganizationUserControl
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
		void InitializeComponent()
		{
			this.AcquirerDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BuyingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.BuyerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AcquirerDocAddress.SuspendLayout();
			this.BuyingAgentAddressControl.SuspendLayout();
			this.BuyerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("bfd5d493-61d7-47bb-bd87-3899e74a93c5", "Seller");
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.SellerAddressControl.TabIndex = 3;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 156, true);
			this.ManufacturerAddressControl.TabIndex = 6;
			this.ManufacturerAddressControl.Visible = false;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("91593cad-7822-4335-b548-b3b9aa638f5b", "[14] Representative");
			this.RepresentativeAddressControl.TabIndex = 2;
			// 
			// DefermentPartyDocAddressControl
			// 
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 134, true);
			this.DefermentPartyDocAddressControl.TabIndex = 7;
			this.DefermentPartyDocAddressControl.Visible = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// AcquirerDocAddress
			// 
			this.AcquirerDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AcquirerDocAddress, "AcquirerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).AcquirerDocAddress)));
			this.AcquirerDocAddress.BindToOrganisations = "Lookups+AcquirerCollection";
			this.AcquirerDocAddress.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9CDB4B15-7585-482D-A428-A6D12239F06F", "Acquirer");
			this.AcquirerDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.AcquirerDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.AcquirerDocAddress.Name = "AcquirerDocAddress";
			this.AcquirerDocAddress.ReadOnly = false;
			this.AcquirerDocAddress.SingleLineNoGroupBoxPanelWidth = 320;
			this.AcquirerDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.AcquirerDocAddress.TabIndex = 1;
			this.AcquirerDocAddress.ValidationJustForced = false;
			// 
			// BuyingAgentAddressControl
			// 
			this.BuyingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyingAgentAddressControl, "JE_OA_BuyingAgentAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).JE_OA_BuyingAgentAddress)));
			this.BuyingAgentAddressControl.BindToOrgList = "Lookups+BuyingAgentAddressList";
			this.BuyingAgentAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("94203d00-9ba5-4cbe-9e70-64589edfe4ee", "Represented Party");
			this.BuyingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.BuyingAgentAddressControl.Name = "BuyingAgentAddressControl";
			this.BuyingAgentAddressControl.PopupCaption = "";
			this.BuyingAgentAddressControl.ReadOnly = false;
			this.BuyingAgentAddressControl.ShowAddress = false;
			this.BuyingAgentAddressControl.ShowOrganisationName = true;
			this.BuyingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.BuyingAgentAddressControl.TabIndex = 5;
			// 
			// BuyerAddressControl
			// 
			this.BuyerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerAddressControl, "JE_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).JE_OA_ConsigneeAddress)));
			this.BuyerAddressControl.BindToOrgList = "Lookups+ConsigneeList";
			this.BuyerAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c4974329-b1e5-4583-b07f-c7e71da8de87", "Buyer");
			this.BuyerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.BuyerAddressControl.Name = "BuyerAddressControl";
			this.BuyerAddressControl.PopupCaption = "";
			this.BuyerAddressControl.ReadOnly = false;
			this.BuyerAddressControl.ShowAddress = false;
			this.BuyerAddressControl.ShowOrganisationName = true;
			this.BuyerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.BuyerAddressControl.TabIndex = 4;
			// 
			// OrganizationImportUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AcquirerDocAddress);
			this.Controls.Add(this.BuyerAddressControl);
			this.Controls.Add(this.BuyingAgentAddressControl);
			this.Name = "OrganizationImportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 198, true);
			this.Controls.SetChildIndex(this.BuyingAgentAddressControl, 0);
			this.Controls.SetChildIndex(this.BuyerAddressControl, 0);
			this.Controls.SetChildIndex(this.AcquirerDocAddress, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.Controls.SetChildIndex(this.DefermentPartyDocAddressControl, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AcquirerDocAddress.ResumeLayout(true);
			this.AcquirerDocAddress.PerformLayout();
			this.BuyingAgentAddressControl.ResumeLayout(true);
			this.BuyingAgentAddressControl.PerformLayout();
			this.BuyerAddressControl.ResumeLayout(true);
			this.BuyerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZDocAddressControl AcquirerDocAddress;
		private ZArchitecture.GUI.ZAddressControl BuyerAddressControl;
		private ZArchitecture.GUI.ZAddressControl BuyingAgentAddressControl;

	}
}
