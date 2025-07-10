namespace Enterprise.Customs.EU.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RouteFRequestedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LCPDepartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LCPInspectDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingInformationUserControl = new Enterprise.Customs.EU.GUI.SupportingInformationControl();
			this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeferralSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ItineraryCountriesUserControl = new Enterprise.Customs.EU.GUI.ItineraryCountriesUserControl();
			this.ItineraryCountriesSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentTypeDropEdit.SuspendLayout();
			this.LCPDepartDateEdit.SuspendLayout();
			this.LCPInspectDateEdit.SuspendLayout();
			this.SupportingInformationUserControl.SuspendLayout();
			this.PaymentMethodDropEdit.SuspendLayout();
			this.DeferralSeparatorUserControl.SuspendLayout();
			this.ItineraryCountriesUserControl.SuspendLayout();
			this.ItineraryCountriesSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "ZG_IsTrainingDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_IsTrainingDeclaration)));
			this.TrainingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 87, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 4;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentTypeDropEdit
			// 
			this.ShipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTypeDropEdit, "ZG_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_ShipmentType)));
			this.ShipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 114, true);
			this.ShipmentTypeDropEdit.Name = "ShipmentTypeDropEdit";
			this.ShipmentTypeDropEdit.PreBoundMaxLength = 5;
			this.ShipmentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ShipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.ShipmentTypeDropEdit.TabIndex = 5;
			// 
			// RouteFRequestedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RouteFRequestedCheckBox, "JE_RouteFRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_RouteFRequested)));
			this.RouteFRequestedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RouteFRequestedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 87, true);
			this.RouteFRequestedCheckBox.Name = "RouteFRequestedCheckBox";
			this.RouteFRequestedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.RouteFRequestedCheckBox.TabIndex = 3;
			this.RouteFRequestedCheckBox.UseVisualStyleBackColor = true;
			// 
			// LCPDepartDateEdit
			// 
			this.LCPDepartDateEdit.AllowDrop = true;
			this.LCPDepartDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCPDepartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCPDepartDateEdit, "ZG_LCPDepart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_LCPDepart)));
			this.LCPDepartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 31, true);
			this.LCPDepartDateEdit.Name = "LCPDepartDateEdit";
			this.LCPDepartDateEdit.TabIndex = 1;
			// 
			// LCPInspectDateEdit
			// 
			this.LCPInspectDateEdit.AllowDrop = true;
			this.LCPInspectDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCPInspectDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCPInspectDateEdit, "ZG_LCPInspect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_LCPInspect)));
			this.LCPInspectDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 3, true);
			this.LCPInspectDateEdit.Name = "LCPInspectDateEdit";
			this.LCPInspectDateEdit.TabIndex = 0;
			// 
			// SupportingInformationUserControl
			// 
			this.SupportingInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
			this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 0, true);
			this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
			this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 400, true);
			this.SupportingInformationUserControl.TabIndex = 7;
			// 
			// PaymentMethodDropEdit
			// 
			this.PaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_PaymentMethod)));
			this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 59, true);
			this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
			this.PaymentMethodDropEdit.PreBoundMaxLength = 1;
			this.PaymentMethodDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.PaymentMethodDropEdit.TabIndex = 2;
			// 
			// DeferralSeparatorUserControl
			// 
			this.DeferralSeparatorUserControl.AllowDrop = true;
			this.DeferralSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EFD6D1B1-AF15-4102-8B3F-042AA8E9D03E", "Deferral");
			this.DeferralSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 142, true);
			this.DeferralSeparatorUserControl.Name = "DeferralSeparatorUserControl";
			this.DeferralSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.DeferralSeparatorUserControl.TabIndex = 6;
			// 
			// ItineraryCountriesUserControl
			// 
			this.ItineraryCountriesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ItineraryCountriesUserControl, ".");
			this.ItineraryCountriesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 211, true);
			this.ItineraryCountriesUserControl.Name = "ItineraryCountriesUserControl";
			this.ItineraryCountriesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 177, true);
			this.ItineraryCountriesUserControl.TabIndex = 8;
			// 
			// ItineraryCountriesSeparatorUserControl
			// 
			this.ItineraryCountriesSeparatorUserControl.AllowDrop = true;
			this.ItineraryCountriesSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("2ba122c8-45cb-496f-bcb4-4857360ae86a", "Itinerary Countries");
			this.ItineraryCountriesSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 182, true);
			this.ItineraryCountriesSeparatorUserControl.Name = "ItineraryCountriesSeparatorUserControl";
			this.ItineraryCountriesSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ItineraryCountriesSeparatorUserControl.TabIndex = 25;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TrainingCheckBox);
			this.Controls.Add(this.ShipmentTypeDropEdit);
			this.Controls.Add(this.RouteFRequestedCheckBox);
			this.Controls.Add(this.LCPDepartDateEdit);
			this.Controls.Add(this.LCPInspectDateEdit);
			this.Controls.Add(this.SupportingInformationUserControl);
			this.Controls.Add(this.DeferralSeparatorUserControl);
			this.Controls.Add(this.PaymentMethodDropEdit);
			this.Controls.Add(this.ItineraryCountriesUserControl);
			this.Controls.Add(this.ItineraryCountriesSeparatorUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 443, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentTypeDropEdit.ResumeLayout(true);
			this.ShipmentTypeDropEdit.PerformLayout();
			this.LCPDepartDateEdit.ResumeLayout(true);
			this.LCPDepartDateEdit.PerformLayout();
			this.LCPInspectDateEdit.ResumeLayout(true);
			this.LCPInspectDateEdit.PerformLayout();
			this.SupportingInformationUserControl.ResumeLayout(true);
			this.SupportingInformationUserControl.PerformLayout();
			this.PaymentMethodDropEdit.ResumeLayout(true);
			this.PaymentMethodDropEdit.PerformLayout();
			this.DeferralSeparatorUserControl.ResumeLayout(true);
			this.DeferralSeparatorUserControl.PerformLayout();
			this.ItineraryCountriesUserControl.ResumeLayout(true);
			this.ItineraryCountriesUserControl.PerformLayout();
			this.ItineraryCountriesSeparatorUserControl.ResumeLayout(true);
			this.ItineraryCountriesSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
		internal ZArchitecture.GUI.ZDropEdit ShipmentTypeDropEdit;
		internal ZArchitecture.GUI.ZCheckBox RouteFRequestedCheckBox;
		internal ZArchitecture.GUI.ZDateEdit LCPDepartDateEdit;
		internal ZArchitecture.GUI.ZDateEdit LCPInspectDateEdit;
		internal SupportingInformationControl SupportingInformationUserControl;
		internal ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
		internal ZArchitecture.GUI.SeparatorUserControl DeferralSeparatorUserControl;
		internal RelatedDeclarationsUserControl RelatedDeclarationsUserControl;
		internal ItineraryCountriesUserControl ItineraryCountriesUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl ItineraryCountriesSeparatorUserControl;
	}
}
