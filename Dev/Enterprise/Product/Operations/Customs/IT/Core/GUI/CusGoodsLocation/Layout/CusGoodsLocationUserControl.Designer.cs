namespace Enterprise.Customs.IT.GUI
{
	partial class CusGoodsLocationUserControl
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
			this.AdditionalIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrganizationAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalIdentifierDropEdit.SuspendLayout();
			this.OrganizationAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.CusGoodsLocation);
			// 
			// AdditionalIdentifierDropEdit
			// 
			this.AdditionalIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalIdentifierDropEdit, "CGL_AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusGoodsLocation)(null)).CGL_AdditionalIdentifier)));
			this.AdditionalIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 3, true);
			this.AdditionalIdentifierDropEdit.Name = "AdditionalIdentifierDropEdit";
			this.AdditionalIdentifierDropEdit.ShowDescriptionBox = false;
			this.AdditionalIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AdditionalIdentifierDropEdit.TabIndex = 0;
			// 
			// OrganizationAddressControl
			// 
			this.OrganizationAddressControl.AddressValidationProcessCmdKey = null;
			this.OrganizationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationAddressControl, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IT.Business.Declaration.CusGoodsLocation)(null)).Address)));
			this.OrganizationAddressControl.BindToOrganisations = "Lookups.OrganisationList";
			this.OrganizationAddressControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("2b2600ae-08eb-4292-a325-ecca8532b123", "Organization");
			this.OrganizationAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.OrganizationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 38, true);
			this.OrganizationAddressControl.Name = "OrganizationAddressControl";
			this.OrganizationAddressControl.ReadOnly = false;
			this.OrganizationAddressControl.ShowCompanyName = true;
			this.OrganizationAddressControl.SingleLineNoGroupBoxPanelWidth = 330;
			this.OrganizationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.OrganizationAddressControl.TabIndex = 0;
			this.OrganizationAddressControl.ValidationJustForced = false;
			// 
			// OverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideCheckBox, "Address.E2_AddressOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.CusGoodsLocation)(null)).Address.E2_AddressOverride)));
			this.OverrideCheckBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("36f80b84-e16c-4269-9924-fef46a0a1949", "Override");
			this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 64, true);
			this.OverrideCheckBox.Name = "OverrideCheckBox";
			this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.OverrideCheckBox.TabIndex = 1;
			// 
			// CusGoodsLocationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrganizationAddressControl);
			this.Controls.Add(this.AdditionalIdentifierDropEdit);
			this.Controls.Add(this.OverrideCheckBox);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 138, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalIdentifierDropEdit.ResumeLayout(true);
			this.AdditionalIdentifierDropEdit.PerformLayout();
			this.OrganizationAddressControl.ResumeLayout(true);
			this.OrganizationAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit AdditionalIdentifierDropEdit;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl OrganizationAddressControl;
		internal ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
	}
}
