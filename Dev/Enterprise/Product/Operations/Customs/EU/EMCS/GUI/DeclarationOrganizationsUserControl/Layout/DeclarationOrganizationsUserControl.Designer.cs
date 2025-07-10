namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class DeclarationOrganizationsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OwnerDocAddressUserControl = new Enterprise.Customs.EU.EMCS.GUI.OwnerDocAddressUserControl();
			this.ConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OwnerDocAddressUserControl.SuspendLayout();
			this.ConsignorDocAddressControl.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// OwnerDocAddressUserControl
			// 
			this.OwnerDocAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerDocAddressUserControl, ".");
			this.OwnerDocAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 380, true);
			this.OwnerDocAddressUserControl.Name = "OwnerDocAddressUserControl";
			this.OwnerDocAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.OwnerDocAddressUserControl.TabIndex = 2;
			// 
			// ConsignorDocAddressControl
			// 
			this.ConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.ConsignorDocAddressControl.BindToOrganisations = "Lookups.SuppliersList";
			this.ConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("1c8994c8-5aa7-40e0-8909-1fed19b06cc1", "Consignor");
			this.ConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ConsignorDocAddressControl.Name = "ConsignorDocAddressControl";
			this.ConsignorDocAddressControl.ReadOnly = false;
			this.ConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsignorDocAddressControl.TabIndex = 0;
			this.ConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.ConsigneesList";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("e4dc92bd-6676-40b7-ac9f-c30bf163cadb", "Consignee");
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 192, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 1;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// DeclarationOrganizationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OwnerDocAddressUserControl);
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Controls.Add(this.ConsignorDocAddressControl);
			this.Name = "DeclarationOrganizationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 569, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OwnerDocAddressUserControl.ResumeLayout(true);
			this.OwnerDocAddressUserControl.PerformLayout();
			this.ConsignorDocAddressControl.ResumeLayout(true);
			this.ConsignorDocAddressControl.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal OwnerDocAddressUserControl OwnerDocAddressUserControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsignorDocAddressControl;
	}
}
