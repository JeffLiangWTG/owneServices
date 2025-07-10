namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class CompanyConfigurationUserControl
	{
		private void InitializeComponent()
		{
			this.CustomsConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValuationDateImportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ValuationDateExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsValueCodeForExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VATValueCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsValueCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsReciprocalHelpTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsReciprocalExchangeRateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CautionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsConfigurationGroupBox.SuspendLayout();
			this.ValuationDateImportDropEdit.SuspendLayout();
			this.ValuationDateExportDropEdit.SuspendLayout();
			this.CustomsValueCodeForExportDropEdit.SuspendLayout();
			this.VATValueCodeDropEdit.SuspendLayout();
			this.CustomsValueCodeDropEdit.SuspendLayout();
			this.IsReciprocalExchangeRateDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper);
			// 
			// CustomsConfigurationGroupBox
			// 
			this.CustomsConfigurationGroupBox.Controls.Add(this.ValuationDateImportDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.ValuationDateExportDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.CustomsValueCodeForExportDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.VATValueCodeDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.CustomsValueCodeDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.NoteLabel);
			this.CustomsConfigurationGroupBox.Controls.Add(this.IsReciprocalHelpTextLabel);
			this.CustomsConfigurationGroupBox.Controls.Add(this.IsReciprocalExchangeRateDropEdit);
			this.CustomsConfigurationGroupBox.Controls.Add(this.CautionLabel);
			this.CustomsConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 2, true);
			this.CustomsConfigurationGroupBox.Name = "CustomsConfigurationGroupBox";
			this.CustomsConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 406, true);
			this.CustomsConfigurationGroupBox.TabIndex = 1;
			this.CustomsConfigurationGroupBox.TabStop = false;
			// 
			// ValuationDateImportDropEdit
			// 
			this.ValuationDateImportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationDateImportDropEdit, "Configuration.ZZC_DefaultImportValuationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_DefaultImportValuationDate)));
			this.ValuationDateImportDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("1f3f3dc5-2574-4737-a3ce-e766b6e35127", "Valuation Date Import");
			this.ValuationDateImportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 335, true);
			this.ValuationDateImportDropEdit.Name = "ValuationDateImportDropEdit";
			this.ValuationDateImportDropEdit.ShouldResizeByMaxLength = true;
			this.ValuationDateImportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ValuationDateImportDropEdit.TabIndex = 8;
			// 
			// ValuationDateExportDropEdit
			// 
			this.ValuationDateExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationDateExportDropEdit, "Configuration.ZZC_DefaultExportValuationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_DefaultExportValuationDate)));
			this.ValuationDateExportDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("8ef26b60-6feb-4ab8-b1f1-6695f4b86a69", "Valuation Date Export");
			this.ValuationDateExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 370, true);
			this.ValuationDateExportDropEdit.Name = "ValuationDateExportDropEdit";
			this.ValuationDateExportDropEdit.ShouldResizeByMaxLength = true;
			this.ValuationDateExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ValuationDateExportDropEdit.TabIndex = 9;
			// 
			// CustomsValueCodeForExportDropEdit
			// 
			this.CustomsValueCodeForExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCodeForExportDropEdit, "Configuration.ZZC_CustomsValueCodeForExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_CustomsValueCodeForExport)));
			this.CustomsValueCodeForExportDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("88b0aea3-adb0-4427-9bb1-935ef70c55fd", "Customs Value Export");
			this.CustomsValueCodeForExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 300, true);
			this.CustomsValueCodeForExportDropEdit.Name = "CustomsValueCodeForExportDropEdit";
			this.CustomsValueCodeForExportDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsValueCodeForExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsValueCodeForExportDropEdit.TabIndex = 7;
			// 
			// VATValueCodeDropEdit
			// 
			this.VATValueCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATValueCodeDropEdit, "Configuration.ZZC_VATValueCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_VATValueCode)));
			this.VATValueCodeDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("dc6b6cfe-8d77-4903-8ece-d9e058bbd46d", "VAT Value Calculation");
			this.VATValueCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 265, true);
			this.VATValueCodeDropEdit.Name = "VATValueCodeDropEdit";
			this.VATValueCodeDropEdit.ShouldResizeByMaxLength = true;
			this.VATValueCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.VATValueCodeDropEdit.TabIndex = 6;
			// 
			// CustomsValueCodeDropEdit
			// 
			this.CustomsValueCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCodeDropEdit, "Configuration.ZZC_CustomsValueCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_CustomsValueCode)));
			this.CustomsValueCodeDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("2a3e7dcd-5da4-4590-b2d4-003e80f09ddc", "Customs Value Calculation");
			this.CustomsValueCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 230, true);
			this.CustomsValueCodeDropEdit.Name = "CustomsValueCodeDropEdit";
			this.CustomsValueCodeDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsValueCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsValueCodeDropEdit.TabIndex = 5;
			// 
			// NoteLabel
			// 
			this.NoteLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 180, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 28, true);
			this.NoteLabel.TabIndex = 4;
			// 
			// IsReciprocalHelpTextLabel
			// 
			this.IsReciprocalHelpTextLabel.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("6a6a23de-4bbc-44c0-bbbf-6bbce774b75c", "Changing the ‘Exchange Rates Is Reciprocal Override’ value where Customs Exchange Rates already exist for this country should only be done where, either the Customs exchange rates are also changed to a reciprocal / non - reciprocal format OR the override of this setting is being actioned in order to correct the conflict between Accounting Exchange rate format and Customs Exchange rate format.");
			this.IsReciprocalHelpTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.IsReciprocalHelpTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 99, true);
			this.IsReciprocalHelpTextLabel.Name = "IsReciprocalHelpTextLabel";
			this.IsReciprocalHelpTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 79, true);
			this.IsReciprocalHelpTextLabel.TabIndex = 3;
			// 
			// IsReciprocalExchangeRateDropEdit
			// 
			this.IsReciprocalExchangeRateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IsReciprocalExchangeRateDropEdit, "Configuration.ZZC_IsReciprocalExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.ZZC_IsReciprocalExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.GlbCompanyWrapper)(null)).Configuration.IsReciprocalExchangeRateDescription)));
			this.IsReciprocalExchangeRateDropEdit.BindToForDescription = "Configuration.IsReciprocalExchangeRateDescription";
			this.IsReciprocalExchangeRateDropEdit.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("d72b0d12-d06a-4d2d-aa9d-832f52dfdaff", "Exchange Rate Is Reciprocal Override");
			this.IsReciprocalExchangeRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 76, true);
			this.IsReciprocalExchangeRateDropEdit.Name = "IsReciprocalExchangeRateDropEdit";
			this.IsReciprocalExchangeRateDropEdit.ShouldResizeByMaxLength = true;
			this.IsReciprocalExchangeRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.IsReciprocalExchangeRateDropEdit.TabIndex = 2;
			// 
			// CautionLabel
			// 
			this.CautionLabel.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("D3E98F58-9FB8-40FD-9CC8-540E84570D3B", "These settings should be changed exercising caution. Normally these configurations would be set up prior to using the customs system for this company. Changing any of these settings will cause the customs system for this company to behave differently.");
			this.CautionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CautionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CautionLabel.Name = "CautionLabel";
			this.CautionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 44, true);
			this.CautionLabel.TabIndex = 1;
			// 
			// CompanyConfigurationUserControl
			// 
			this.Controls.Add(this.CustomsConfigurationGroupBox);
			this.Name = "CompanyConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 406, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsConfigurationGroupBox.ResumeLayout(false);
			this.CustomsConfigurationGroupBox.PerformLayout();
			this.ValuationDateImportDropEdit.ResumeLayout(true);
			this.ValuationDateImportDropEdit.PerformLayout();
			this.ValuationDateExportDropEdit.ResumeLayout(true);
			this.ValuationDateExportDropEdit.PerformLayout();
			this.CustomsValueCodeForExportDropEdit.ResumeLayout(true);
			this.CustomsValueCodeForExportDropEdit.PerformLayout();
			this.VATValueCodeDropEdit.ResumeLayout(true);
			this.VATValueCodeDropEdit.PerformLayout();
			this.CustomsValueCodeDropEdit.ResumeLayout(true);
			this.CustomsValueCodeDropEdit.PerformLayout();
			this.IsReciprocalExchangeRateDropEdit.ResumeLayout(true);
			this.IsReciprocalExchangeRateDropEdit.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZGroupBox CustomsConfigurationGroupBox;
		internal ZArchitecture.ZLabel CautionLabel;
		internal ZArchitecture.ZLabel NoteLabel;
		internal ZArchitecture.ZLabel IsReciprocalHelpTextLabel;
		internal ZArchitecture.GUI.ZDropEdit IsReciprocalExchangeRateDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CustomsValueCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit VATValueCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CustomsValueCodeForExportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ValuationDateImportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ValuationDateExportDropEdit;
	}
}
