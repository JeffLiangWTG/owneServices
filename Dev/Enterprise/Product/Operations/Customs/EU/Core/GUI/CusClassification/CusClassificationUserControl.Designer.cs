namespace Enterprise.Customs.EU.GUI
{
	partial class CusClassificationUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.additionalTariffDefailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.additionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cC_EcSupplement2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cC_EcAdditionalSupplementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.supplementLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.supplementLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.supplement1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cPCFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BaseClassificationGroupBox.SuspendLayout();
			this.LastAuditDateEdit.SuspendLayout();
			this.AuditStaffCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.additionalTariffDefailsGroupBox.SuspendLayout();
			this.cPCFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 152, true);
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.MasterFiles.CusClassification);
			// 
			// additionalTariffDefailsGroupBox
			// 
			this.additionalTariffDefailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.additionalTariffDefailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CusClassificationUserControl|cd5b4074-fc95-44d9-8b27-5ad5614647f8", "Additional Tariff Details");
			this.additionalTariffDefailsGroupBox.Controls.Add(this.additionalSupplementaryCodesEditButton);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.cC_EcSupplement2TextBox);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.cC_EcAdditionalSupplementsTextBox);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.supplementLabel1);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.supplementLabel2);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.supplement1TextBox);
			this.additionalTariffDefailsGroupBox.Controls.Add(this.cPCFindBox);
			this.additionalTariffDefailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.additionalTariffDefailsGroupBox.Name = "additionalTariffDefailsGroupBox";
			this.additionalTariffDefailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 62, true);
			this.additionalTariffDefailsGroupBox.TabIndex = 1;
			this.additionalTariffDefailsGroupBox.TabStop = false;
			// 
			// additionalSupplementaryCodesEditButton
			// 
			this.additionalSupplementaryCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("AdditionalSupplementaryCodesEditButton|DACAAF16-DC3A-45FA-961A-DFCE774C64D1", "More...");
			this.additionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 39, true);
			this.additionalSupplementaryCodesEditButton.Name = "additionalSupplementaryCodesEditButton";
			this.additionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.additionalSupplementaryCodesEditButton.TabIndex = 29;
			this.additionalSupplementaryCodesEditButton.ToolTipCaption = null;
			this.additionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
			// 
			// cC_EcSupplement2TextBox
			// 
			this.BindingSource.SetBindingMember(this.cC_EcSupplement2TextBox, "CC_EcSupplement2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.MasterFiles.CusClassification)(null)).CC_EcSupplement2)));
			this.cC_EcSupplement2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cC_EcSupplement2TextBox, false);
			this.cC_EcSupplement2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 41, true);
			this.cC_EcSupplement2TextBox.Name = "cC_EcSupplement2TextBox";
			this.cC_EcSupplement2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.cC_EcSupplement2TextBox.TabIndex = 28;
			// 
			// cC_EcAdditionalSupplementsTextBox
			// 
			this.BindingSource.SetBindingMember(this.cC_EcAdditionalSupplementsTextBox, "CC_EcAdditionalSupplements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.MasterFiles.CusClassification)(null)).CC_EcAdditionalSupplements)));
			this.cC_EcAdditionalSupplementsTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cC_EcAdditionalSupplementsTextBox, false);
			this.cC_EcAdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 41, true);
			this.cC_EcAdditionalSupplementsTextBox.Name = "cC_EcAdditionalSupplementsTextBox";
			this.cC_EcAdditionalSupplementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.cC_EcAdditionalSupplementsTextBox.TabIndex = 30;
			this.cC_EcAdditionalSupplementsTextBox.TabStop = false;
			// 
			// supplementLabel1
			// 
			this.supplementLabel1.AutoSize = true;
			this.supplementLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.supplementLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 41, true);
			this.supplementLabel1.Name = "supplementLabel1";
			this.supplementLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.supplementLabel1.TabIndex = 31;
			this.supplementLabel1.Text = "/";
			// 
			// supplementLabel2
			// 
			this.supplementLabel2.AutoSize = true;
			this.supplementLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.supplementLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 41, true);
			this.supplementLabel2.Name = "supplementLabel2";
			this.supplementLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.supplementLabel2.TabIndex = 32;
			this.supplementLabel2.Text = "/";
			// 
			// supplement1TextBox
			// 
			this.BindingSource.SetBindingMember(this.supplement1TextBox, "CC_EcSupplement1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.MasterFiles.CusClassification)(null)).CC_EcSupplement1)));
			this.supplement1TextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c6f55bc1-26b1-4ef3-b2c9-2e83b75f1c9b", "Supplements", "EC Supplements");
			this.supplement1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 41, true);
			this.supplement1TextBox.Name = "supplement1TextBox";
			this.supplement1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.supplement1TextBox.TabIndex = 27;
			// 
			// cPCFindBox
			// 
			this.cPCFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cPCFindBox, "CC_ProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.MasterFiles.CusClassification)(null)).CC_ProcedureCode)));
			this.cPCFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CusClassificationUserControl|c7d3e2cd-345b-471b-872e-301cae22cf83", "CPC");
			this.cPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 19, true);
			this.cPCFindBox.Name = "cPCFindBox";
			this.cPCFindBox.PreBoundMaxLength = 10;
			this.cPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.cPCFindBox.TabIndex = 2;
			// 
			// CusClassificationUserControl
			// 
			this.Controls.Add(this.additionalTariffDefailsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 217, true);
			this.Name = "CusClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 217, true);
			this.Controls.SetChildIndex(this.additionalTariffDefailsGroupBox, 0);
			this.Controls.SetChildIndex(this.BaseClassificationGroupBox, 0);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			this.LastAuditDateEdit.ResumeLayout(true);
			this.LastAuditDateEdit.PerformLayout();
			this.AuditStaffCodeFindBox.ResumeLayout(true);
			this.AuditStaffCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.additionalTariffDefailsGroupBox.ResumeLayout(false);
			this.additionalTariffDefailsGroupBox.PerformLayout();
			this.cPCFindBox.ResumeLayout(true);
			this.cPCFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox additionalTariffDefailsGroupBox;
		protected ZArchitecture.GUI.ZDropEdit cPCFindBox;
		internal ZArchitecture.ZTextBox cC_EcAdditionalSupplementsTextBox;
		ZArchitecture.ZTextBox cC_EcSupplement2TextBox;
		ZArchitecture.ZTextBox supplement1TextBox;
		internal ZArchitecture.GUI.ZButton additionalSupplementaryCodesEditButton;
		ZArchitecture.ZLabel supplementLabel1;
		ZArchitecture.ZLabel supplementLabel2;
	}
}
