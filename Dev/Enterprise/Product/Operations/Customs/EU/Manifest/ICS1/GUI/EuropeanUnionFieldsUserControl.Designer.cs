namespace Enterprise.Customs.EU.Manifest.GUI
{
	partial class EuropeanUnionFieldsUserControl
	{
		private void InitializeComponent()
		{
			this.SpecificCircumstanceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ATAatFirstCustomsOfficeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpecialMentionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ETAatFirstCustomsOfficeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EUCustomsOfficesUserControl = new Enterprise.Customs.EU.Manifest.GUI.EUCountryCustomsOfficesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SpecificCircumstanceIndicatorDropEdit.SuspendLayout();
			this.ATAatFirstCustomsOfficeDateEdit.SuspendLayout();
			this.MethodOfPaymentDropEdit.SuspendLayout();
			this.SpecialMentionsDropEdit.SuspendLayout();
			this.ETAatFirstCustomsOfficeDateEdit.SuspendLayout();
			this.EUCustomsOfficesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// SpecificCircumstanceIndicatorDropEdit
			// 
			this.SpecificCircumstanceIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceIndicatorDropEdit, "SpecificCircumstanceIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).SpecificCircumstanceIndicator)));
			this.SpecificCircumstanceIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.GUI.Res.GetData("9e55ecd9-6147-4fe3-81d7-041e07f7e3e3", "Specific Circumstance Indicator");
			this.SpecificCircumstanceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 4, true);
			this.SpecificCircumstanceIndicatorDropEdit.Name = "SpecificCircumstanceIndicatorDropEdit";
			this.SpecificCircumstanceIndicatorDropEdit.PreBoundMaxLength = 1;
			this.SpecificCircumstanceIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.SpecificCircumstanceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.SpecificCircumstanceIndicatorDropEdit.TabIndex = 0;
			// 
			// ATAatFirstCustomsOfficeDateEdit
			// 
			this.ATAatFirstCustomsOfficeDateEdit.AllowDrop = true;
			this.ATAatFirstCustomsOfficeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ATAatFirstCustomsOfficeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ATAatFirstCustomsOfficeDateEdit, "ATAatFirstCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ATAatFirstCustomsOffice)));
			this.ATAatFirstCustomsOfficeDateEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.GUI.Res.GetData("76ab2f5f-ec31-4d9b-8fad-af500f71ae4a", "ATA at First Customs Office");
			this.ATAatFirstCustomsOfficeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 26, true);
			this.ATAatFirstCustomsOfficeDateEdit.Name = "ATAatFirstCustomsOfficeDateEdit";
			this.ATAatFirstCustomsOfficeDateEdit.TabIndex = 4;
			// 
			// MethodOfPaymentDropEdit
			// 
			this.MethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPaymentDropEdit, "MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).MethodOfPayment)));
			this.MethodOfPaymentDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.GUI.Res.GetData("2d19c69a-67e6-4a6e-804c-94080ed714d7", "Method of Payment");
			this.MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 4, true);
			this.MethodOfPaymentDropEdit.Name = "MethodOfPaymentDropEdit";
			this.MethodOfPaymentDropEdit.PreBoundMaxLength = 1;
			this.MethodOfPaymentDropEdit.ShouldResizeByMaxLength = true;
			this.MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.MethodOfPaymentDropEdit.TabIndex = 1;
			// 
			// SpecialMentionsDropEdit
			// 
			this.SpecialMentionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialMentionsDropEdit, "SpecialMentions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).SpecialMentions)));
			this.SpecialMentionsDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.GUI.Res.GetData("460f84b0-e210-4471-a587-f15fbb29684f", "Special Mentions");
			this.SpecialMentionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 4, true);
			this.SpecialMentionsDropEdit.Name = "SpecialMentionsDropEdit";
			this.SpecialMentionsDropEdit.PreBoundMaxLength = 5;
			this.SpecialMentionsDropEdit.ShouldResizeByMaxLength = true;
			this.SpecialMentionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.SpecialMentionsDropEdit.TabIndex = 2;
			// 
			// ETAatFirstCustomsOfficeDateEdit
			// 
			this.ETAatFirstCustomsOfficeDateEdit.AllowDrop = true;
			this.ETAatFirstCustomsOfficeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETAatFirstCustomsOfficeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETAatFirstCustomsOfficeDateEdit, "ETAatFirstCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ETAatFirstCustomsOffice)));
			this.ETAatFirstCustomsOfficeDateEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.GUI.Res.GetData("561e1d6a-97ee-4448-9770-b2744a48cf78", "ETA at First Customs Office");
			this.ETAatFirstCustomsOfficeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 26, true);
			this.ETAatFirstCustomsOfficeDateEdit.Name = "ETAatFirstCustomsOfficeDateEdit";
			this.ETAatFirstCustomsOfficeDateEdit.TabIndex = 3;
			// 
			// EUCustomsOfficesUserControl
			// 
			this.EUCustomsOfficesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EUCustomsOfficesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)))));
			this.EUCustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.EUCustomsOfficesUserControl.Name = "EUCustomsOfficesUserControl";
			this.EUCustomsOfficesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 153, true);
			this.EUCustomsOfficesUserControl.TabIndex = 3;
			this.EUCustomsOfficesUserControl.Visible = false;
			// 
			// EuropeanUnionFieldsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ETAatFirstCustomsOfficeDateEdit);
			this.Controls.Add(this.SpecialMentionsDropEdit);
			this.Controls.Add(this.MethodOfPaymentDropEdit);
			this.Controls.Add(this.SpecificCircumstanceIndicatorDropEdit);
			this.Controls.Add(this.ATAatFirstCustomsOfficeDateEdit);
			this.Controls.Add(this.EUCustomsOfficesUserControl);
			this.Name = "EuropeanUnionFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 241, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SpecificCircumstanceIndicatorDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceIndicatorDropEdit.PerformLayout();
			this.ATAatFirstCustomsOfficeDateEdit.ResumeLayout(true);
			this.ATAatFirstCustomsOfficeDateEdit.PerformLayout();
			this.MethodOfPaymentDropEdit.ResumeLayout(true);
			this.MethodOfPaymentDropEdit.PerformLayout();
			this.SpecialMentionsDropEdit.ResumeLayout(true);
			this.SpecialMentionsDropEdit.PerformLayout();
			this.ETAatFirstCustomsOfficeDateEdit.ResumeLayout(true);
			this.ETAatFirstCustomsOfficeDateEdit.PerformLayout();
			this.EUCustomsOfficesUserControl.ResumeLayout(true);
			this.EUCustomsOfficesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit SpecificCircumstanceIndicatorDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MethodOfPaymentDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SpecialMentionsDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ETAatFirstCustomsOfficeDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ATAatFirstCustomsOfficeDateEdit;
		internal EUCountryCustomsOfficesUserControl EUCustomsOfficesUserControl;
	}
}
