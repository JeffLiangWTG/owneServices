using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPCertificatesUserControl
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

		private void InitializeComponent()
		{
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductDescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_MeatInspectionDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CopyMeatInspectionDescriptionFromLineButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QL_AdditionalProductDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_CommercialProductDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_HealthCertificateDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_SendHCDescCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QL_HCNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxQL_ExtraCertificate = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_ImportAuthorityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_HCFormatAllocatedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_HCFormatRequestedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateGroupBox.SuspendLayout();
			this.ProductDescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// CertificateGroupBox
			// 
			this.CertificateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CertificateGroupBox.Controls.Add(this.QL_SendHCDescCheckBox);
			this.CertificateGroupBox.Controls.Add(this.QL_HCNumberTextBox);
			this.CertificateGroupBox.Controls.Add(this.TextBoxQL_ExtraCertificate);
			this.CertificateGroupBox.Controls.Add(this.QL_ImportAuthorityCodeTextBox);
			this.CertificateGroupBox.Controls.Add(this.QL_HCFormatAllocatedTextBox);
			this.CertificateGroupBox.Controls.Add(this.QL_HCFormatRequestedTextBox);
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 3, true);
			this.CertificateGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 215, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 215, true);
			this.CertificateGroupBox.TabIndex = 1;
			this.CertificateGroupBox.TabStop = false;
			this.CertificateGroupBox.Text = "Certificate";
			// 
			// ProductDescriptionGroupBox
			// 
			this.ProductDescriptionGroupBox.Controls.Add(this.QL_MeatInspectionDescriptionTextBox);
			this.ProductDescriptionGroupBox.Controls.Add(this.CopyMeatInspectionDescriptionFromLineButton);
			this.ProductDescriptionGroupBox.Controls.Add(this.QL_AdditionalProductDescriptionTextBox);
			this.ProductDescriptionGroupBox.Controls.Add(this.QL_CommercialProductDescriptionTextBox);
			this.ProductDescriptionGroupBox.Controls.Add(this.QL_HealthCertificateDescriptionTextBox);
			this.ProductDescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProductDescriptionGroupBox.Name = "ProductDescriptionGroupBox";
			this.ProductDescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 215, true);
			this.ProductDescriptionGroupBox.TabIndex = 0;
			this.ProductDescriptionGroupBox.TabStop = false;
			this.ProductDescriptionGroupBox.Text = "Product Description";
			// 
			// QL_MeatInspectionDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_MeatInspectionDescriptionTextBox, "QuarantineExDocLine+QL_MeatInspectionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_MeatInspectionDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_MeatInspectionDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.QL_MeatInspectionDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 32, true);
			this.QL_MeatInspectionDescriptionTextBox.Name = "QL_MeatInspectionDescriptionTextBox";
			this.QL_MeatInspectionDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 20, true);
			this.QL_MeatInspectionDescriptionTextBox.TabIndex = 1;
			// 
			// CopyMeatInspectionDescriptionFromLineButton
			// 
			this.CopyMeatInspectionDescriptionFromLineButton.IsCaptionOverridden = true;
			this.CopyMeatInspectionDescriptionFromLineButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 55, true);
			this.CopyMeatInspectionDescriptionFromLineButton.Name = "CopyMeatInspectionDescriptionFromLineButton";
			this.CopyMeatInspectionDescriptionFromLineButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.CopyMeatInspectionDescriptionFromLineButton.TabIndex = 2;
			this.CopyMeatInspectionDescriptionFromLineButton.Text = "Use Invoice Line Description";
			this.CopyMeatInspectionDescriptionFromLineButton.ToolTipCaption = null;
			this.CopyMeatInspectionDescriptionFromLineButton.Click += new System.EventHandler(this.CopyMeatInspectionDescriptionFromLineButton_Click);
			// 
			// QL_AdditionalProductDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_AdditionalProductDescriptionTextBox, "QuarantineExDocLine+QL_AddtionalProductDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_AddtionalProductDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_AdditionalProductDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.QL_AdditionalProductDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 71, true);
			this.QL_AdditionalProductDescriptionTextBox.Multiline = true;
			this.QL_AdditionalProductDescriptionTextBox.Name = "QL_AdditionalProductDescriptionTextBox";
			this.QL_AdditionalProductDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 32, true);
			this.QL_AdditionalProductDescriptionTextBox.TabIndex = 3;
			// 
			// QL_CommercialProductDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_CommercialProductDescriptionTextBox, "QuarantineExDocLine+QL_CommercialProductDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_CommercialProductDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_CommercialProductDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.QL_CommercialProductDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 122, true);
			this.QL_CommercialProductDescriptionTextBox.Multiline = true;
			this.QL_CommercialProductDescriptionTextBox.Name = "QL_CommercialProductDescriptionTextBox";
			this.QL_CommercialProductDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 32, true);
			this.QL_CommercialProductDescriptionTextBox.TabIndex = 5;
			// 
			// QL_HealthCertificateDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_HealthCertificateDescriptionTextBox, "QuarantineExDocLine+QL_EffectiveHealthCertificateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_EffectiveHealthCertificateDescription)));
			this.QL_HealthCertificateDescriptionTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|8ac993eb-cdda-4e08-b9c2-c0634d33f6d1", "Health Certificate");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_HealthCertificateDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.QL_HealthCertificateDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 173, true);
			this.QL_HealthCertificateDescriptionTextBox.Multiline = true;
			this.QL_HealthCertificateDescriptionTextBox.Name = "QL_HealthCertificateDescriptionTextBox";
			this.QL_HealthCertificateDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 32, true);
			this.QL_HealthCertificateDescriptionTextBox.TabIndex = 7;
			// 
			// QL_SendHCDescCheckBox
			// 
			this.QL_SendHCDescCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QL_SendHCDescCheckBox, "QuarantineExDocLine+QL_SendHCDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_SendHCDesc)));
			this.QL_SendHCDescCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|59b38b0f-5cbc-43e0-b9d9-e30a0d25455d", "Ovr. HC Desc.", "Send Override HC Desc.", "Send Override HC Description", "If this box is checked then the description entered on the Line details tab will " +
		"be sent to Quarantine as the override HC description.");
			this.QL_SendHCDescCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 126, true);
			this.QL_SendHCDescCheckBox.Name = "QL_SendHCDescCheckBox";
			this.QL_SendHCDescCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 16, true);
			this.QL_SendHCDescCheckBox.TabIndex = 28;
			this.QL_SendHCDescCheckBox.UseVisualStyleBackColor = true;
			// 
			// QL_HCNumberTextBox
			// 
			this.QL_HCNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_HCNumberTextBox, "QuarantineExDocLine+QL_HCNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_HCNumber)));
			this.QL_HCNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 100, true);
			this.QL_HCNumberTextBox.Name = "QL_HCNumberTextBox";
			this.QL_HCNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.QL_HCNumberTextBox.TabIndex = 7;
			// 
			// TextBoxQL_ExtraCertificate
			// 
			this.TextBoxQL_ExtraCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxQL_ExtraCertificate, "QuarantineExDocLine+QL_ExtraCertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ExtraCertificate)));
			this.TextBoxQL_ExtraCertificate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 44, true);
			this.TextBoxQL_ExtraCertificate.Name = "TextBoxQL_ExtraCertificate";
			this.TextBoxQL_ExtraCertificate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TextBoxQL_ExtraCertificate.TabIndex = 3;
			// 
			// QL_ImportAuthorityCodeTextBox
			// 
			this.QL_ImportAuthorityCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_ImportAuthorityCodeTextBox, "QuarantineExDocLine+QL_ImportAuthorityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ImportAuthorityCode)));
			this.QL_ImportAuthorityCodeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|fc355796-b2b3-4b4d-be4f-07cb9de40e87", "Import Authority Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_ImportAuthorityCodeTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.QL_ImportAuthorityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 160, true);
			this.QL_ImportAuthorityCodeTextBox.Multiline = true;
			this.QL_ImportAuthorityCodeTextBox.Name = "QL_ImportAuthorityCodeTextBox";
			this.QL_ImportAuthorityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 50, true);
			this.QL_ImportAuthorityCodeTextBox.TabIndex = 9;
			// 
			// QL_HCFormatAllocatedTextBox
			// 
			this.QL_HCFormatAllocatedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_HCFormatAllocatedTextBox, "QuarantineExDocLine+QL_HCFormatAllocated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_HCFormatAllocated)));
			this.QL_HCFormatAllocatedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 72, true);
			this.QL_HCFormatAllocatedTextBox.Name = "QL_HCFormatAllocatedTextBox";
			this.QL_HCFormatAllocatedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.QL_HCFormatAllocatedTextBox.TabIndex = 5;
			// 
			// QL_HCFormatRequestedTextBox
			// 
			this.QL_HCFormatRequestedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_HCFormatRequestedTextBox, "QuarantineExDocLine+QL_HCFormatRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_HCFormatRequested)));
			this.QL_HCFormatRequestedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 16, true);
			this.QL_HCFormatRequestedTextBox.Name = "QL_HCFormatRequestedTextBox";
			this.QL_HCFormatRequestedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.QL_HCFormatRequestedTextBox.TabIndex = 1;
			// 
			// RFPCertificatesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CertificateGroupBox);
			this.Controls.Add(this.ProductDescriptionGroupBox);
			this.Name = "RFPCertificatesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.ProductDescriptionGroupBox.ResumeLayout(false);
			this.ProductDescriptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal ZGroupBox CertificateGroupBox;
		protected internal ZGroupBox ProductDescriptionGroupBox;
		protected internal ZCheckBox QL_SendHCDescCheckBox;
		protected internal ZTextBox QL_HCNumberTextBox;
		protected internal ZTextBox TextBoxQL_ExtraCertificate;
		protected internal ZTextBox QL_ImportAuthorityCodeTextBox;
		protected internal ZTextBox QL_HCFormatAllocatedTextBox;
		protected internal ZTextBox QL_HCFormatRequestedTextBox;
		protected internal ZTextBox QL_MeatInspectionDescriptionTextBox;
		protected internal ZButton CopyMeatInspectionDescriptionFromLineButton;
		protected internal ZTextBox QL_HealthCertificateDescriptionTextBox;
		protected internal ZTextBox QL_AdditionalProductDescriptionTextBox;
		protected internal ZTextBox QL_CommercialProductDescriptionTextBox;
	}
}
