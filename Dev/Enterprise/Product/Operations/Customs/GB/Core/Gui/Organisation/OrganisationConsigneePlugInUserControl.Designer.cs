using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	partial class OrganisationConsigneePlugInUserControl : ZUserControl//, IOrganisationConPlUserCont
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
			this.GBPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DropEditDucrAttribute = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CheckBoxUseClientEori = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DropEditDeclarantRefAttribute = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEditVATDeferralType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GBPanel.SuspendLayout();
			this.DropEditDucrAttribute.SuspendLayout();
			this.DropEditDeclarantRefAttribute.SuspendLayout();
			this.zDropEditVATDeferralType.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.GBOrgImpAddInfo);
			// 
			// GBPanel
			// 
			this.GBPanel.Controls.Add(this.DropEditDucrAttribute);
			this.GBPanel.Controls.Add(this.CheckBoxUseClientEori);
			this.GBPanel.Controls.Add(this.DropEditDeclarantRefAttribute);
			this.GBPanel.Controls.Add(this.zDropEditVATDeferralType);
			this.GBPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GBPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GBPanel.Name = "GBPanel";
			this.GBPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GBPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			this.GBPanel.TabIndex = 2;
			this.GBPanel.Text = "GB";
			// 
			// DropEditDucrAttribute
			// 
			this.DropEditDucrAttribute.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditDucrAttribute, "ZO_Box44ClientsDucrSourceAttributeField");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.GBOrgImpAddInfo)(null)).ZO_Box44ClientsDucrSourceAttributeField)));
			this.DropEditDucrAttribute.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("Box44ClientsDucrSourceAttributeField", "DUCR Ref.", "DUCR reference field?", "Custom field for client DUCR reference", "Which custom attribute field will be used to set the client\'s DUCR reference field?");
			this.DropEditDucrAttribute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 42, true);
			this.DropEditDucrAttribute.Name = "DropEditDucrAttribute";
			this.DropEditDucrAttribute.PreBoundMaxLength = 1;
			this.DropEditDucrAttribute.ShouldResizeByMaxLength = true;
			this.DropEditDucrAttribute.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DropEditDucrAttribute.TabIndex = 4;
			// 
			// CheckBoxUseClientEori
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxUseClientEori, "ZO_Box44UseClientsEoriForDucrs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GBOrgImpAddInfo)(null)).ZO_Box44UseClientsEoriForDucrs)));
			this.CheckBoxUseClientEori.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("Box44UseClientsEoriForDucrs", "Use EORI?", "Use EORI for DUCR?", "Use client\'s EORI for DUCR?", "Use client\'s own EORI when constructing DUCRs?  If not, the default of the declarant\'s (defaulting to your OrgProxy\'s) will be used.");
			this.CheckBoxUseClientEori.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CheckBoxUseClientEori.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 69, true);
			this.CheckBoxUseClientEori.Name = "CheckBoxUseClientEori";
			this.CheckBoxUseClientEori.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 18, true);
			this.CheckBoxUseClientEori.TabIndex = 3;
			this.CheckBoxUseClientEori.Text = "Use client\'s EORI for DUCRs?";
			this.CheckBoxUseClientEori.UseVisualStyleBackColor = true;
			// 
			// DropEditDeclarantRefAttribute
			// 
			this.DropEditDeclarantRefAttribute.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditDeclarantRefAttribute, "ZO_Box7DeclarantsReferenceSourceAttributeField");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.GBOrgImpAddInfo)(null)).ZO_Box7DeclarantsReferenceSourceAttributeField)));
			this.DropEditDeclarantRefAttribute.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("Box7DeclarantsReferenceSourceAttributeField", "Declarant\'s Ref.", "[7] Declarant\'s Ref.", "Custom field for declarant\'s reference", "Which custom attribute field will be used to set the declarant\'s reference field?");
			this.DropEditDeclarantRefAttribute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 94, true);
			this.DropEditDeclarantRefAttribute.Name = "DropEditDeclarantRefAttribute";
			this.DropEditDeclarantRefAttribute.PreBoundMaxLength = 1;
			this.DropEditDeclarantRefAttribute.ShouldResizeByMaxLength = true;
			this.DropEditDeclarantRefAttribute.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DropEditDeclarantRefAttribute.TabIndex = 5;
			// 
			// zDropEditVATDeferralType
			// 
			this.zDropEditVATDeferralType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditVATDeferralType, "ZO_VATDeferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.GBOrgImpAddInfo)(null)).ZO_VATDeferType)));
			this.zDropEditVATDeferralType.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6668F6B5-4E87-428C-BDC1-32862D8F2BE1", "[48] VAT Deferment Type", "The standard deferment method for VAT.");
			this.zDropEditVATDeferralType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 15, true);
			this.zDropEditVATDeferralType.Name = "zDropEditVATDeferralType";
			this.zDropEditVATDeferralType.PreBoundMaxLength = 1;
			this.zDropEditVATDeferralType.ShouldResizeByMaxLength = true;
			this.zDropEditVATDeferralType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEditVATDeferralType.TabIndex = 1;
			// 
			// OrganisationConsigneePlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GBPanel);
			this.Name = "OrganisationConsigneePlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GBPanel.ResumeLayout(false);
			this.GBPanel.PerformLayout();
			this.DropEditDucrAttribute.ResumeLayout(true);
			this.DropEditDucrAttribute.PerformLayout();
			this.DropEditDeclarantRefAttribute.ResumeLayout(true);
			this.DropEditDeclarantRefAttribute.PerformLayout();
			this.zDropEditVATDeferralType.ResumeLayout(true);
			this.zDropEditVATDeferralType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel GBPanel;
		private ZDropEdit zDropEditVATDeferralType;
		private ZCheckBox CheckBoxUseClientEori;
		private ZDropEdit DropEditDucrAttribute;
		ZDropEdit DropEditDeclarantRefAttribute;
	}
}
