namespace Enterprise.Customs.EU.GUI
{
	public partial class OrganisationsUserControl
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
			this.DefermentPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ExporterDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContractualPartnerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CarrierEUBorderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DutyPayerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			this.ExporterDocAddressControl.SuspendLayout();
			this.ContractualPartnerDocAddressControl.SuspendLayout();
			this.CarrierEUBorderDocAddressControl.SuspendLayout();
			this.DutyPayerGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
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
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 3, true);
			this.DefermentPartyDocAddressControl.Name = "DefermentPartyDocAddressControl";
			this.DefermentPartyDocAddressControl.ReadOnly = false;
			this.DefermentPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 348;
			this.DefermentPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DefermentPartyDocAddressControl.TabIndex = 0;
			this.DefermentPartyDocAddressControl.ValidationJustForced = false;
			// 
			// ExporterDocAddressControl
			// 
			this.ExporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterDocAddressControl, "ExporterDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ExporterDocAddress)));
			this.ExporterDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ExporterDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3ABB0424-4926-45E0-823A-84C956721FA7", "Exporter");
			this.ExporterDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ExporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 29, true);
			this.ExporterDocAddressControl.Name = "ExporterDocAddressControl";
			this.ExporterDocAddressControl.ReadOnly = false;
			this.ExporterDocAddressControl.SingleLineNoGroupBoxPanelWidth = 348;
			this.ExporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ExporterDocAddressControl.TabIndex = 1;
			this.ExporterDocAddressControl.ValidationJustForced = false;
			// 
			// ContractualPartnerDocAddressControl
			// 
			this.ContractualPartnerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContractualPartnerDocAddressControl, "ContractualPartnerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ContractualPartnerDocAddress)));
			this.ContractualPartnerDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ContractualPartnerDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("BE7F2D94-6A88-4449-8B4F-AC69C27FEF98", "Contractual Partner");
			this.ContractualPartnerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ContractualPartnerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 55, true);
			this.ContractualPartnerDocAddressControl.Name = "ContractualPartnerDocAddressControl";
			this.ContractualPartnerDocAddressControl.ReadOnly = false;
			this.ContractualPartnerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 348;
			this.ContractualPartnerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ContractualPartnerDocAddressControl.TabIndex = 2;
			this.ContractualPartnerDocAddressControl.ValidationJustForced = false;
			// 
			// CarrierEUBorderDocAddressControl
			// 
			this.CarrierEUBorderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierEUBorderDocAddressControl, "CarrierEUBorderDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CarrierEUBorderDocAddress)));
			this.CarrierEUBorderDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.CarrierEUBorderDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("FA04462E-EAB5-4BAA-A666-00C44E828EF9", "Carrier EU Border");
			this.CarrierEUBorderDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierEUBorderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 81, true);
			this.CarrierEUBorderDocAddressControl.Name = "CarrierEUBorderDocAddressControl";
			this.CarrierEUBorderDocAddressControl.ReadOnly = false;
			this.CarrierEUBorderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 348;
			this.CarrierEUBorderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.CarrierEUBorderDocAddressControl.TabIndex = 3;
			this.CarrierEUBorderDocAddressControl.ValidationJustForced = false;
			// 
			// DutyPayerGuidFindBox
			// 
			this.DutyPayerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyPayerGuidFindBox, "JE_OH_DutyPayer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OH_DutyPayer)));
			this.DutyPayerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 115, true);
			this.DutyPayerGuidFindBox.Name = "DutyPayerGuidFindBox";
			this.DutyPayerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DutyPayerGuidFindBox.ParentType = null;
			this.DutyPayerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 17, true);
			this.DutyPayerGuidFindBox.TabIndex = 4;
			// 
			// OrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DutyPayerGuidFindBox);
			this.Controls.Add(this.DefermentPartyDocAddressControl);
			this.Controls.Add(this.ExporterDocAddressControl);
			this.Controls.Add(this.ContractualPartnerDocAddressControl);
			this.Controls.Add(this.CarrierEUBorderDocAddressControl);
			this.Name = "OrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 385, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			this.ExporterDocAddressControl.ResumeLayout(true);
			this.ExporterDocAddressControl.PerformLayout();
			this.ContractualPartnerDocAddressControl.ResumeLayout(true);
			this.ContractualPartnerDocAddressControl.PerformLayout();
			this.CarrierEUBorderDocAddressControl.ResumeLayout(true);
			this.CarrierEUBorderDocAddressControl.PerformLayout();
			this.DutyPayerGuidFindBox.ResumeLayout(true);
			this.DutyPayerGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterFiles.GUI.ZDocAddressControl DefermentPartyDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ExporterDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ContractualPartnerDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl CarrierEUBorderDocAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox DutyPayerGuidFindBox;
	}
}
