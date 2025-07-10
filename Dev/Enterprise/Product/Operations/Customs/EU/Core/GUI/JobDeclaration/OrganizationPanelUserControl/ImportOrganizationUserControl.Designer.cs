namespace Enterprise.Customs.EU.GUI
{
	partial class ImportOrganizationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.SellerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DefermentPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.RepresentativeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeclarantOfficeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SellerAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerAddressControl, "JE_OA_SellerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OA_SellerAddress)));
			this.SellerAddressControl.BindToOrgList = "Lookups+SellerList";
			this.SellerAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|SellerAddressControl", "Seller");
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.SellerAddressControl.Name = "SellerAddressControl";
			this.SellerAddressControl.PopupCaption = "";
			this.SellerAddressControl.ShowAddress = false;
			this.SellerAddressControl.ShowOrganisationName = true;
			this.SellerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.SellerAddressControl.TabIndex = 2;
			// 
			// DefermentPartyDocAddressControl
			// 
			this.DefermentPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefermentPartyDocAddressControl, "DefermentPartyDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DefermentPartyDocAddress)));
			this.DefermentPartyDocAddressControl.BindToOrganisations = "Lookups+DefermentPartyCollection";
			this.DefermentPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("448c62bd-a3c2-48b9-95a9-28201ab87bd4", "Deferment Party");
			this.DefermentPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.DefermentPartyDocAddressControl.Name = "DefermentPartyDocAddressControl";
			this.DefermentPartyDocAddressControl.ReadOnly = false;
			this.DefermentPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DefermentPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DefermentPartyDocAddressControl.TabIndex = 4;
			this.DefermentPartyDocAddressControl.ValidationJustForced = false;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "JE_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|ManufacturerAddressControl", "Manufacturer");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.ShowOrganisationName = true;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ManufacturerAddressControl.TabIndex = 3;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "JE_OA_Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OA_Representative)));
			this.RepresentativeAddressControl.BindToOrgList = "Lookups+RepresentativeList";
			this.RepresentativeAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|RepresentativeAddressControl", "Representative");
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 24, true);
			this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
			this.RepresentativeAddressControl.PopupCaption = "";
			this.RepresentativeAddressControl.ShowAddress = false;
			this.RepresentativeAddressControl.ShowOrganisationName = true;
			this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.RepresentativeAddressControl.TabIndex = 1;
			// 
			// DeclarantOfficeAddressControl
			// 
			this.DeclarantOfficeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantOfficeAddressControl, "JE_OA_DeclarantAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OA_DeclarantAddress)));
			this.DeclarantOfficeAddressControl.BindToOrgList = "Lookups+DeclarantOfficeList";
			this.DeclarantOfficeAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|DeclarantOfficeAddressControl", "[14] Declarant", "[14] Declarant", "[14] Declarant", "[14] Declarant. Name of the declarant controlling this declaration.");
			this.DeclarantOfficeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.DeclarantOfficeAddressControl.Name = "DeclarantOfficeAddressControl";
			this.DeclarantOfficeAddressControl.PopupCaption = "";
			this.DeclarantOfficeAddressControl.ShowAddress = false;
			this.DeclarantOfficeAddressControl.ShowOrganisationName = true;
			this.DeclarantOfficeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DeclarantOfficeAddressControl.TabIndex = 0;
			// 
			// ImportOrganizationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclarantOfficeAddressControl);
			this.Controls.Add(this.RepresentativeAddressControl);
			this.Controls.Add(this.SellerAddressControl);
			this.Controls.Add(this.ManufacturerAddressControl);
			this.Controls.Add(this.DefermentPartyDocAddressControl);
			this.Name = "ImportOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 136, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZAddressControl SellerAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl DeclarantOfficeAddressControl;
		protected MasterFiles.GUI.ZDocAddressControl DefermentPartyDocAddressControl;

		#endregion
	}
}
