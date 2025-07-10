
namespace Enterprise.Customs.DE.GUI
{
	partial class ExportOrganizationUserControl
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
			this.ExporterDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContractualPartnerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CarrierEUBorderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExporterDocAddressControl.SuspendLayout();
			this.ContractualPartnerDocAddressControl.SuspendLayout();
			this.CarrierEUBorderDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("fca3ebab-9e7e-4612-8fbc-ac93fe172edd", "[2] Subcontractor");
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Visible = false;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4b4d478e-c99b-4934-acb1-a31eb5555404", "[14] Representative");
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// ExporterDocAddressControl
			// 
			this.ExporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterDocAddressControl, "ExporterDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ExporterDocAddress)));
			this.ExporterDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ExporterDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("C7D1950F-372F-4464-8A36-33DD3BD275B8", "Exporter");
			this.ExporterDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ExporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.ExporterDocAddressControl.Name = "ExporterDocAddressControl";
			this.ExporterDocAddressControl.ReadOnly = false;
			this.ExporterDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ExporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ExporterDocAddressControl.TabIndex = 4;
			this.ExporterDocAddressControl.ValidationJustForced = false;
			// 
			// ContractualPartnerDocAddressControl
			// 
			this.ContractualPartnerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContractualPartnerDocAddressControl, "ContractualPartnerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ContractualPartnerDocAddress)));
			this.ContractualPartnerDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ContractualPartnerDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ba02004f-f418-4b49-ad78-e33086b241ce", "Contractual Partner");
			this.ContractualPartnerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ContractualPartnerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.ContractualPartnerDocAddressControl.Name = "ContractualPartnerDocAddressControl";
			this.ContractualPartnerDocAddressControl.ReadOnly = false;
			this.ContractualPartnerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContractualPartnerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContractualPartnerDocAddressControl.TabIndex = 5;
			this.ContractualPartnerDocAddressControl.ValidationJustForced = false;
			// 
			// CarrierEUBorderDocAddressControl
			// 
			this.CarrierEUBorderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierEUBorderDocAddressControl, "CarrierEUBorderDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CarrierEUBorderDocAddress)));
			this.CarrierEUBorderDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.CarrierEUBorderDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("26A0A3A9-C25B-45FD-B49E-2398AF2DE0C0", "Carrier EU Border");
			this.CarrierEUBorderDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierEUBorderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.CarrierEUBorderDocAddressControl.Name = "CarrierEUBorderDocAddressControl";
			this.CarrierEUBorderDocAddressControl.ReadOnly = false;
			this.CarrierEUBorderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.CarrierEUBorderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.CarrierEUBorderDocAddressControl.TabIndex = 6;
			this.CarrierEUBorderDocAddressControl.ValidationJustForced = false;
			// 
			// ExportOrganizationUserControl
			// 
			this.Controls.Add(this.ContractualPartnerDocAddressControl);
			this.Controls.Add(this.ExporterDocAddressControl);
			this.Controls.Add(this.CarrierEUBorderDocAddressControl);
			this.Name = "ExportOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 150, true);
			this.Controls.SetChildIndex(this.CarrierEUBorderDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ExporterDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.Controls.SetChildIndex(this.ContractualPartnerDocAddressControl, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExporterDocAddressControl.ResumeLayout(true);
			this.ExporterDocAddressControl.PerformLayout();
			this.ContractualPartnerDocAddressControl.ResumeLayout(true);
			this.ContractualPartnerDocAddressControl.PerformLayout();
			this.CarrierEUBorderDocAddressControl.ResumeLayout(true);
			this.CarrierEUBorderDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		MasterFiles.GUI.ZDocAddressControl ExporterDocAddressControl;
		MasterFiles.GUI.ZDocAddressControl ContractualPartnerDocAddressControl;
		MasterFiles.GUI.ZDocAddressControl CarrierEUBorderDocAddressControl;
	}
}
