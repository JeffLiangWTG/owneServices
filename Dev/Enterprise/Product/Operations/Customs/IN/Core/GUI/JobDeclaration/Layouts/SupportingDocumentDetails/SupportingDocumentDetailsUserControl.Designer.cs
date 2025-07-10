namespace Enterprise.Customs.IN.GUI
{
	partial class SupportingDocumentDetailsUserControl
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
			this.IssuingPartyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrganizationDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ImageReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IssuingPartyGroupBox.SuspendLayout();
			this.OrganizationDocAddressControl.SuspendLayout();
			this.CodeDropEdit.SuspendLayout();
			this.DocumentTypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.SupportingDocumentCollection);
			// 
			// IssuingPartyGroupBox
			// 
			this.IssuingPartyGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("63bb9e24-4e41-4c04-89bc-6b22a2bfc9ee", "Issuing Party");
			this.IssuingPartyGroupBox.Controls.Add(this.OrganizationDocAddressControl);
			this.IssuingPartyGroupBox.Controls.Add(this.CodeDropEdit);
			this.IssuingPartyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 73, true);
			this.IssuingPartyGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 71, true);
			this.IssuingPartyGroupBox.Name = "IssuingPartyGroupBox";
			this.IssuingPartyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 71, true);
			this.IssuingPartyGroupBox.TabIndex = 3;
			this.IssuingPartyGroupBox.TabStop = false;
			// 
			// OrgnizationDocAddressControl
			// 
			this.OrganizationDocAddressControl.AddressValidationProcessCmdKey = null;
			this.OrganizationDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationDocAddressControl, "OrganizationAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IN.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SupportingDocument)(null)))).SyncRoot)).OrganizationAddress)));
			this.OrganizationDocAddressControl.BindToOrganisations = "Lookups.OrganizationList";
			this.OrganizationDocAddressControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("671cad82-f009-47cf-a282-3bdecdb205f7", "Org.", "Organization", "Organization for Supporting Documents");
			this.OrganizationDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.OrganizationDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 18, true);
			this.OrganizationDocAddressControl.Name = "OrgnizationDocAddressControl";
			this.OrganizationDocAddressControl.ReadOnly = false;
			this.OrganizationDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.OrganizationDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.OrganizationDocAddressControl.TabIndex = 0;
			this.OrganizationDocAddressControl.ValidationJustForced = false;
			// 
			// CodeDropEdit
			// 
			this.CodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CodeDropEdit, "CSI_IssuerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SupportingDocument)(null)))).SyncRoot)).CSI_IssuerType)));
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.SupportingDocument)(null)).CSI_IssuerType)));
			this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 43, true);
			this.CodeDropEdit.Name = "CodeDropEdit";
			this.CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.CodeDropEdit.TabIndex = 1;
			// 
			// DocumentTypeCodeFindBox
			// 
			this.DocumentTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentTypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SupportingDocument)(null)))).SyncRoot)).CSI_Code)));
			this.DocumentTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 47, true);
			this.DocumentTypeCodeFindBox.Name = "DocumentTypeCodeFindBox";
			this.DocumentTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DocumentTypeCodeFindBox.ParentType = null;
			this.DocumentTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.DocumentTypeCodeFindBox.TabIndex = 2;
			// 
			// ImageReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImageReferenceNumberTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SupportingDocument)(null)))).SyncRoot)).CSI_ReferenceNumber2)));
			this.ImageReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 18, true);
			this.ImageReferenceNumberTextBox.Name = "ImageReferenceNumberTextBox";
			this.ImageReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.ImageReferenceNumberTextBox.TabIndex = 1;
			// 
			// SupportingDocumentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImageReferenceNumberTextBox);
			this.Controls.Add(this.DocumentTypeCodeFindBox);
			this.Controls.Add(this.IssuingPartyGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 7, 8, 7, true);
			this.Name = "SupportingDocumentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 272, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IssuingPartyGroupBox.ResumeLayout(false);
			this.IssuingPartyGroupBox.PerformLayout();
			this.OrganizationDocAddressControl.ResumeLayout(true);
			this.OrganizationDocAddressControl.PerformLayout();
			this.CodeDropEdit.ResumeLayout(true);
			this.CodeDropEdit.PerformLayout();
			this.DocumentTypeCodeFindBox.ResumeLayout(true);
			this.DocumentTypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox IssuingPartyGroupBox;
		internal ZArchitecture.GUI.ZCodeFindBox DocumentTypeCodeFindBox;
		internal ZArchitecture.ZTextBox ImageReferenceNumberTextBox;
		private MasterFiles.GUI.ZDocAddressControl OrganizationDocAddressControl;
		private ZArchitecture.GUI.ZDropEdit CodeDropEdit;
	}
}
