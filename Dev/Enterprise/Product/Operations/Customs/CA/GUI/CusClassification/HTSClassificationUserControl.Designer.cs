using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class HTSClassificationUserControl
	{
		void InitializeComponent()
		{
			this.valueForDutyCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.gSTStatusCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.eTExemptionDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.eTRateCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.tRSNumberTextBox = new ZArchitecture.ZTextBox();
			this.authorityNumberCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.tariff99CodeFindBox = new TariffFindBox();
			this.tariffCodeFindBox = new TariffFindBox();
			this.treatmentCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.cCA_RN_NKOriginzCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.cCA_ProvinceOfOriginDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.cCA_RN_NKOriginzCodeFindBox.SuspendLayout();
			this.cCA_ProvinceOfOriginDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.cCA_ProvinceOfOriginDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.cCA_RN_NKOriginzCodeFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.treatmentCodeDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.tRSNumberTextBox);
			this.BaseClassificationGroupBox.Controls.Add(this.tariff99CodeFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.authorityNumberCodeFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.valueForDutyCodeDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.gSTStatusCodeDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.eTExemptionDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.eTRateCodeDropEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.tariffCodeFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 364, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.valueForDutyCodeDropEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.authorityNumberCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariff99CodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tRSNumberTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.treatmentCodeDropEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.gSTStatusCodeDropEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.eTExemptionDropEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.eTRateCodeDropEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cCA_RN_NKOriginzCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cCA_ProvinceOfOriginDropEdit, 0);
			// 
			// LookupCodeTextBox
			// 
			this.LookupCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 22, true);
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.LookupCodeTextBox.TabIndex = 1;
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 338, true);
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CC_IsActiveCheckBox.TabIndex = 44;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 72, true);
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 32, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// LastAuditDateEdit
			// 
			this.LastAuditDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 336, true);
			this.LastAuditDateEdit.TabIndex = 42;
			// 
			// AuditStaffCodeFindBox
			// 
			this.AuditStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 336, true);
			this.AuditStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.AuditStaffCodeFindBox.TabIndex = 40;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusClassification);
			// 
			// ValueForDutyCodeDropEdit
			// 
			this.valueForDutyCodeDropEdit.AllowDrop = true;
			this.valueForDutyCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.valueForDutyCodeDropEdit, "CCA_ValueForDutyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_ValueForDutyCode);
			this.valueForDutyCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|37157d5c-d02d-4493-93a8-46eb08de7440", "VFD Cd", "VFD Code", "Value for Duty Code", "The basis on which the value for duty was determined.");
			this.valueForDutyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 136, true);
			this.valueForDutyCodeDropEdit.Name = "ValueForDutyCodeDropEdit";
			this.valueForDutyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.valueForDutyCodeDropEdit.TabIndex = 11;
			// 
			// TRSNumberTextBox
			// 
			this.tRSNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.tRSNumberTextBox, "CCA_TRSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_TRSNumber);
			this.tRSNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|1c6b239b-e2f8-4855-b37b-0c3b3e6fb616", "TRS #", "TRS Number", "Technical Reference System ruling number.");
			this.tRSNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 236, true);
			this.tRSNumberTextBox.Name = "TRSNumberTextBox";
			this.tRSNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.tRSNumberTextBox.TabIndex = 21;
			// 
			// AuthorityNumberCodeFindBox
			// 
			this.authorityNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.authorityNumberCodeFindBox, "CCA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_AuthorityNumber);
			this.authorityNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 211, true);
			this.authorityNumberCodeFindBox.Name = "AuthorityNumberCodeFindBox";
			this.authorityNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.authorityNumberCodeFindBox.TabIndex = 19;
			// 
			// Tariff99CodeFindBox
			// 
			this.tariff99CodeFindBox.AllowDrop = true;
			this.tariff99CodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.tariff99CodeFindBox, "CCA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_99TariffCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_99TariffCodeTariffInfo);
			this.tariff99CodeFindBox.BindToTariffPropertyInfo = "CCA_99TariffCodeTariffInfo";
			this.tariff99CodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|c6af2972-5c9d-4103-b392-c6328510270c", "Tariff", "Tariff Code", "Applicable if the conditions specified in the Chapter 99 (special classification provisions) tariff item apply.");
			this.tariff99CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 186, true);
			this.tariff99CodeFindBox.Name = "Tariff99CodeFindBox";
			this.tariff99CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.tariff99CodeFindBox.TabIndex = 17;
			// 
			// TariffCodeFindBox
			// 
			this.tariffCodeFindBox.AllowDrop = true;
			this.tariffCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.tariffCodeFindBox, "CC_FormattedTariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CC_FormattedTariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CC_FormattedTariffNumTariffInfo);
			this.tariffCodeFindBox.BindToTariffPropertyInfo = "CC_FormattedTariffNumTariffInfo";
			this.tariffCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|f581fd07-6428-492f-9788-8b5f43f88561", "HS", "HS Code", "Classification Tariff (HTS) Code", "The classification of goods under the Harmonized System.");
			this.tariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 47, true);
			this.tariffCodeFindBox.Name = "TariffCodeFindBox";
			this.tariffCodeFindBox.PreBoundMaxLength = 10;
			this.tariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.tariffCodeFindBox.TabIndex = 3;
			// 
			// TreatmentCodeDropEdit
			// 
			this.treatmentCodeDropEdit.AllowDrop = true;
			this.treatmentCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.treatmentCodeDropEdit, "CCA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_TreatmentCode);
			this.treatmentCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|1F25855E-5F2C-424E-95E4-1802F0FC2F3D", "TT", "TT Code", "Tariff Treatment Code", "The tariff or trade arrangement under which goods are entered into Canada..");
			this.treatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 161, true);
			this.treatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.treatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.treatmentCodeDropEdit.TabIndex = 15;
			// 
			// GSTStatusCodeDropEdit
			// 
			this.gSTStatusCodeDropEdit.AllowDrop = true;
			this.gSTStatusCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.gSTStatusCodeDropEdit, "CCA_GSTStatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_GSTStatusCode);
			this.gSTStatusCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|D411D375-705C-4A76-B1D3-AB2D82C8EB2B", "GST Code", "GST Status Code.");
			this.gSTStatusCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 261, true);
			this.gSTStatusCodeDropEdit.Name = "GSTStatusCodeDropEdit";
			this.gSTStatusCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.gSTStatusCodeDropEdit.TabIndex = 22;
			// 
			// ETRateCodeDropEdit
			// 
			this.eTRateCodeDropEdit.AllowDrop = true;
			this.eTRateCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.eTRateCodeDropEdit, "CCA_ETRateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_ETRateCode);
			this.eTRateCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|8AD30BED-8E8A-4F8C-8316-00F1CE78E8FE", "Excise Rate Code", "Excise Tax Rate Code.");
			this.eTRateCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 286, true);
			this.eTRateCodeDropEdit.Name = "ETRateCodeDropEdit";
			this.eTRateCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.eTRateCodeDropEdit.TabIndex = 23;
			// 
			// ETExemptionDropEdit
			// 
			this.eTExemptionDropEdit.AllowDrop = true;
			this.eTExemptionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.eTExemptionDropEdit, "CCA_ETExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_ETExemption);
			this.eTExemptionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|657BDA01-F87C-4E52-8D92-1E9830DFB626", "Excise Exemption Code", "Excise Tax Exemption Code.");
			this.eTExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 311, true);
			this.eTExemptionDropEdit.Name = "ETExemptionDropEdit";
			this.eTExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.eTExemptionDropEdit.TabIndex = 24;
			// 
			// CCA_RN_NKOriginzCodeFindBox
			// 
			this.cCA_RN_NKOriginzCodeFindBox.AllowDrop = true;
			this.cCA_RN_NKOriginzCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cCA_RN_NKOriginzCodeFindBox, "CCA_RN_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_RN_NKOrigin);
			this.cCA_RN_NKOriginzCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|268913D6-FF06-4823-B0CD-530FDA250A64", "Org.", "Origin", "Country/Region of Origin", "");
			this.cCA_RN_NKOriginzCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 110, true);
			this.cCA_RN_NKOriginzCodeFindBox.Name = "CCA_RN_NKOriginzCodeFindBox";
			this.cCA_RN_NKOriginzCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.cCA_RN_NKOriginzCodeFindBox.TabIndex = 6;
			// 
			// CCA_ProvinceOfOriginDropEdit
			// 
			this.cCA_ProvinceOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cCA_ProvinceOfOriginDropEdit, "CCA_ProvinceOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CCA_ProvinceOfOrigin);
			this.cCA_ProvinceOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("HTSClassificationUserControl|69ec62b2-11bd-4fa5-a384-99f3b10031ab", "State", "State of Origin", "");
			this.cCA_ProvinceOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 110, true);
			this.cCA_ProvinceOfOriginDropEdit.Name = "CCA_ProvinceOfOriginDropEdit";
			this.cCA_ProvinceOfOriginDropEdit.PreBoundMaxLength = 2;
			this.cCA_ProvinceOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.cCA_ProvinceOfOriginDropEdit.TabIndex = 7;
			// 
			// HTSClassificationUserControl
			// 
			this.Name = "HTSClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 364, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.cCA_RN_NKOriginzCodeFindBox.ResumeLayout(true);
			this.cCA_RN_NKOriginzCodeFindBox.PerformLayout();
			this.cCA_ProvinceOfOriginDropEdit.ResumeLayout(true);
			this.cCA_ProvinceOfOriginDropEdit.PerformLayout();
			this.ResumeLayout(false);
		}

		ZArchitecture.GUI.ZDropEdit valueForDutyCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit gSTStatusCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit eTRateCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit eTExemptionDropEdit;
		ZArchitecture.ZTextBox tRSNumberTextBox;
		TariffFindBox tariff99CodeFindBox;
		ZArchitecture.GUI.ZCodeFindBox authorityNumberCodeFindBox;
		ZArchitecture.GUI.ZDropEdit treatmentCodeDropEdit;
		ZArchitecture.GUI.ZCodeFindBox cCA_RN_NKOriginzCodeFindBox;
		ZArchitecture.GUI.ZDropEdit cCA_ProvinceOfOriginDropEdit;
		TariffFindBox tariffCodeFindBox;
	}
}
