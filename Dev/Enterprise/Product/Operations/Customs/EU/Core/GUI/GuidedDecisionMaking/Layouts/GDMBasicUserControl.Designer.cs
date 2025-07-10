namespace Enterprise.Customs.EU.GUI
{
	partial class GDMBasicUserControl
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
			this.EffectiveDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CountryOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuotaOrderNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsFirstQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsThirdQuantityCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsSecondQuantityCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EffectiveDateDateEdit.SuspendLayout();
			this.CountryOfOriginDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.QuotaOrderNumberDropEdit.SuspendLayout();
			this.CustomsFirstQuantityCalcEdit.SuspendLayout();
			this.CustomsSecondQuantityCalcDropEdit.SuspendLayout();
			this.CustomsThirdQuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = this.ExpectedDataSourceType;
			// 
			// EffectiveDateDateEdit
			// 
			this.EffectiveDateDateEdit.AllowDrop = true;
			this.EffectiveDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EffectiveDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EffectiveDateDateEdit, "EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).EffectiveDate)));
			this.EffectiveDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 5, true);
			this.EffectiveDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.EffectiveDateDateEdit.Name = "EffectiveDateDateEdit";
			this.EffectiveDateDateEdit.TabIndex = 1;
			this.EffectiveDateDateEdit.ReadOnly = true;
			// 
			// CountryOfOriginDropEdit
			// 
			this.CountryOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginDropEdit, "CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CountryOfOrigin)));
			this.CountryOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 50, true);
			this.CountryOfOriginDropEdit.Name = "CountryOfOriginDropEdit";
			this.CountryOfOriginDropEdit.PreBoundMaxLength = 2;
			this.CountryOfOriginDropEdit.ShouldResizeByMaxLength = true;
			this.CountryOfOriginDropEdit.ShowDescriptionBox = true;
			this.CountryOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CountryOfOriginDropEdit.TabIndex = 3;
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "CountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CountryOfDestination)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 50, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.PreBoundMaxLength = 2;
			this.CountryOfDestinationDropEdit.ShouldResizeByMaxLength = true;
			this.CountryOfDestinationDropEdit.ShowDescriptionBox = true;
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CountryOfDestinationDropEdit.TabIndex = 3;
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "Preference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).Preference)));
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 80, true);
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.PreBoundMaxLength = 3;
			this.PreferenceDropEdit.ShouldResizeByMaxLength = false;
			this.PreferenceDropEdit.ShowDescriptionBox = true;
			this.PreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.PreferenceDropEdit.TabIndex = 4;
			// 
			// QuotaOrderNumberDropEdit
			// 
			this.QuotaOrderNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotaOrderNumberDropEdit, "QuotaOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).QuotaOrderNumber)));
			this.QuotaOrderNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 110, true);
			this.QuotaOrderNumberDropEdit.Name = "QuotaOrderNumberDropEdit";
			this.QuotaOrderNumberDropEdit.PreBoundMaxLength = 6;
			this.QuotaOrderNumberDropEdit.ShouldResizeByMaxLength = true;
			this.QuotaOrderNumberDropEdit.ShowDescriptionBox = false;
			this.QuotaOrderNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.QuotaOrderNumberDropEdit.TabIndex = 5;
			// 
			// CustomsFirstQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsFirstQuantityCalcEdit, "CustomsFirstQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CustomsFirstQuantity)));
			this.CustomsFirstQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 140, true);
			this.CustomsFirstQuantityCalcEdit.Name = "CustomsFirstQuantityCalcEdit";
			this.CustomsFirstQuantityCalcEdit.DecimalPlaces = 2;
			this.CustomsFirstQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CustomsFirstQuantityCalcEdit.TabIndex = 6;
			// 
			// CustomsSecondQuantityCalcDropEdit
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CustomsSecondQuantity)));
			this.CustomsSecondQuantityCalcDropEdit.BindToAmount = "CustomsSecondQuantity";
			this.CustomsSecondQuantityCalcDropEdit.BindToList = "Lookups+CustomsSecondUQList";
			this.CustomsSecondQuantityCalcDropEdit.BindToUnit = "CustomsSecondUnitQty";
			this.CustomsSecondQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 170, true);
			this.CustomsSecondQuantityCalcDropEdit.Name = "CustomsSecondQuantityCalcDropEdit";
			this.CustomsSecondQuantityCalcDropEdit.UnitPreBoundMaxLength = 4;
			this.CustomsSecondQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CustomsSecondQuantityCalcDropEdit.TabIndex = 7;
			// 
			// CustomsThirdQuantityCalcDropEdit
			//
			this.CustomsThirdQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityCalcDropEdit, "");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).CustomsThirdUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).Lookups.CustomsUQList)));
			this.CustomsThirdQuantityCalcDropEdit.BindToAmount = "CustomsThirdQuantity";
			this.CustomsThirdQuantityCalcDropEdit.BindToList = "Lookups+CustomsUQList";
			this.CustomsThirdQuantityCalcDropEdit.BindToUnit = "CustomsThirdUnitQty";
			this.CustomsThirdQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 200, true);
			this.CustomsThirdQuantityCalcDropEdit.Name = "CustomsThirdQuantityCalcDropEdit";
			this.CustomsThirdQuantityCalcDropEdit.UnitPreBoundMaxLength = 4;
			this.CustomsThirdQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CustomsThirdQuantityCalcDropEdit.TabIndex = 8;
			// 
			// GDMBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EffectiveDateDateEdit);
			this.Controls.Add(this.CountryOfOriginDropEdit);
			this.Controls.Add(this.CountryOfDestinationDropEdit);
			this.Controls.Add(this.PreferenceDropEdit);
			this.Controls.Add(this.QuotaOrderNumberDropEdit);
			this.Controls.Add(this.CustomsFirstQuantityCalcEdit);
			this.Controls.Add(this.CustomsSecondQuantityCalcDropEdit);
			this.Controls.Add(this.CustomsThirdQuantityCalcDropEdit);
			this.Name = "GDMBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EffectiveDateDateEdit.ResumeLayout(true);
			this.EffectiveDateDateEdit.PerformLayout();
			this.CountryOfOriginDropEdit.ResumeLayout(true);
			this.CountryOfOriginDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.QuotaOrderNumberDropEdit.ResumeLayout(true);
			this.QuotaOrderNumberDropEdit.PerformLayout();
			this.CustomsFirstQuantityCalcEdit.ResumeLayout(true);
			this.CustomsFirstQuantityCalcEdit.PerformLayout();
			this.CustomsSecondQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsSecondQuantityCalcDropEdit.PerformLayout();
			this.CustomsThirdQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZDateEdit EffectiveDateDateEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfOriginDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PreferenceDropEdit;
		internal ZArchitecture.GUI.ZDropEdit QuotaOrderNumberDropEdit;
		internal ZArchitecture.ZCalcEdit CustomsFirstQuantityCalcEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsSecondQuantityCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityCalcDropEdit;
	}
}
