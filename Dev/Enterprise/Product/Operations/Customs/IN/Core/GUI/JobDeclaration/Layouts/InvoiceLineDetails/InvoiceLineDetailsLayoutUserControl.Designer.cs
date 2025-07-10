namespace Enterprise.Customs.IN.GUI;

partial class InvoiceLineDetailsLayoutUserControl
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
			this.TransitCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnitPriceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.UnitQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalPMVCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.PMVFieldsUserControl = new Enterprise.Customs.IN.GUI.PMVFieldsUserControl();
			this.AccessoryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EndUseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RewardItemDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IGSTPaymentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PaymentOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AccessoryDescriptionLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransitCountryDropEdit.SuspendLayout();
			this.UnitPriceCalcFindBox.SuspendLayout();
			this.UnitQuantityCalcDropEdit.SuspendLayout();
			this.TotalPMVCalcFindBox.SuspendLayout();
			this.PMVFieldsUserControl.SuspendLayout();
			this.AccessoryStatusDropEdit.SuspendLayout();
			this.EndUseCodeFindBox.SuspendLayout();
			this.RewardItemDropEdit.SuspendLayout();
			this.IGSTPaymentGroupBox.SuspendLayout();
			this.AccessoryDescriptionLongTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobComInvoiceLine);
			// 
			// TransitCountryDropEdit
			// 
			this.TransitCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitCountryDropEdit, "JI_RN_NKCountryOfTransit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_RN_NKCountryOfTransit)));
			this.TransitCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 267, true);
			this.TransitCountryDropEdit.Name = "TransitCountryDropEdit";
			this.TransitCountryDropEdit.PreBoundMaxLength = 2;
			this.TransitCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TransitCountryDropEdit.TabIndex = 2;
			// 
			// UnitPriceCalcFindBox
			// 
			this.UnitPriceCalcFindBox.AllowDrop = true;
			this.UnitPriceCalcFindBox.BindToAmount = "JI_UnitPrice";
			this.UnitPriceCalcFindBox.BindToList = "Lookups+CurrencyList";
			this.UnitPriceCalcFindBox.BindToUnit = "JI_RX_NKLinePriceCurr";
			this.UnitPriceCalcFindBox.Decimals = 5;
			this.UnitPriceCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.UnitPriceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.UnitPriceCalcFindBox.MaxValue = new decimal(new int[] {
            1874919423,
            2328306,
            0,
            327680});
			this.UnitPriceCalcFindBox.Name = "UnitPriceCalcFindBox";
			this.UnitPriceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.UnitPriceCalcFindBox.TabIndex = 0;
			// 
			// UnitQuantityCalcDropEdit
			// 
			this.UnitQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_UnitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_UnitUQ)));
			this.UnitQuantityCalcDropEdit.BindToAmount = "JI_UnitQuantity";
			this.UnitQuantityCalcDropEdit.BindToUnit = "JI_UnitUQ";
			this.UnitQuantityCalcDropEdit.Decimals = 0;
			this.UnitQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 44, true);
			this.UnitQuantityCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.UnitQuantityCalcDropEdit.Name = "UnitQuantityCalcDropEdit";
			this.UnitQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.UnitQuantityCalcDropEdit.TabIndex = 1;
			this.UnitQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TotalPMVCalcFindBox
			// 
			this.TotalPMVCalcFindBox.AllowDrop = true;
			this.TotalPMVCalcFindBox.BindToAmount = "JI_TotalPMV";
			this.TotalPMVCalcFindBox.BindToList = "Lookups+CurrencyList";
			this.TotalPMVCalcFindBox.BindToUnit = "JI_RX_LocalCurrency";
			this.TotalPMVCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 189, true);
			this.TotalPMVCalcFindBox.Name = "TotalPMVCalcFindBox";
			this.TotalPMVCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TotalPMVCalcFindBox.TabIndex = 3;
			// 
			// PMVFieldsUserControl
			// 
			this.PMVFieldsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PMVFieldsUserControl, ".");
			this.PMVFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 151, true);
			this.PMVFieldsUserControl.Name = "PMVFieldsUserControl";
			this.PMVFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 22, true);
			this.PMVFieldsUserControl.TabIndex = 2;
			// 
			// AccessoryStatusDropEdit
			// 
			this.AccessoryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccessoryStatusDropEdit, "JI_AccessoryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_AccessoryStatus)));
			this.AccessoryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 302, true);
			this.AccessoryStatusDropEdit.Name = "AccessoryStatusDropEdit";
			this.AccessoryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.AccessoryStatusDropEdit.TabIndex = 2;
			// 
			// EndUseCodeFindBox
			// 
			this.EndUseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EndUseCodeFindBox, "JI_EndUse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_EndUse)));
			this.EndUseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 110, true);
			this.EndUseCodeFindBox.Name = "EndUseCodeFindBox";
			this.EndUseCodeFindBox.ParentType = null;
			this.EndUseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.EndUseCodeFindBox.TabIndex = 4;
			// 
			// RewardItemDropEdit
			// 
			this.RewardItemDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RewardItemDropEdit, "JI_RewardItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_RewardItem)));
			this.RewardItemDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 78, true);
			this.RewardItemDropEdit.Name = "RewardItemDropEdit";
			this.RewardItemDropEdit.PreBoundMaxLength = 3;
			this.RewardItemDropEdit.ShowDescriptionBox = false;
			this.RewardItemDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RewardItemDropEdit.TabIndex = 3;
			// 
			// IGSTPaymentGroupBox
			// 
			this.IGSTPaymentGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("7E1891C0-4E1C-4D2C-A94E-7418B6154599", "IGST Payment Information");
			this.IGSTPaymentGroupBox.Controls.Add(this.PaymentOverrideCheckBox);
			this.IGSTPaymentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 386, true);
			this.IGSTPaymentGroupBox.Name = "IGSTPaymentGroupBox";
			this.IGSTPaymentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 49, true);
			this.IGSTPaymentGroupBox.TabIndex = 2;
			this.IGSTPaymentGroupBox.TabStop = false;
			// 
			// PaymentOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentOverrideCheckBox, "JI_GSTPayNotApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_GSTPayNotApplicable)));
			this.PaymentOverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PaymentOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.PaymentOverrideCheckBox.Name = "PaymentOverrideCheckBox";
			this.PaymentOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.PaymentOverrideCheckBox.TabIndex = 0;
			this.PaymentOverrideCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.PaymentOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// AccessoryDescriptionLongTextBox
			// 
			this.AccessoryDescriptionLongTextBox.AllowDrop = true;
			this.AccessoryDescriptionLongTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessoryDescriptionLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 335, true);
			this.AccessoryDescriptionLongTextBox.Name = "AccessoryDescriptionLongTextBox";
			this.AccessoryDescriptionLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 22, true);
			this.AccessoryDescriptionLongTextBox.TabIndex = 5;
			// 
			// InvoiceLineDetailsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccessoryDescriptionLongTextBox);
			this.Controls.Add(this.TransitCountryDropEdit);
			this.Controls.Add(this.RewardItemDropEdit);
			this.Controls.Add(this.EndUseCodeFindBox);
			this.Controls.Add(this.AccessoryStatusDropEdit);
			this.Controls.Add(this.PMVFieldsUserControl);
			this.Controls.Add(this.UnitPriceCalcFindBox);
			this.Controls.Add(this.UnitQuantityCalcDropEdit);
			this.Controls.Add(this.TotalPMVCalcFindBox);
			this.Controls.Add(this.IGSTPaymentGroupBox);
			this.Name = "InvoiceLineDetailsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 526, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransitCountryDropEdit.ResumeLayout(true);
			this.TransitCountryDropEdit.PerformLayout();
			this.UnitPriceCalcFindBox.ResumeLayout(true);
			this.UnitPriceCalcFindBox.PerformLayout();
			this.UnitQuantityCalcDropEdit.ResumeLayout(true);
			this.UnitQuantityCalcDropEdit.PerformLayout();
			this.TotalPMVCalcFindBox.ResumeLayout(true);
			this.TotalPMVCalcFindBox.PerformLayout();
			this.PMVFieldsUserControl.ResumeLayout(true);
			this.PMVFieldsUserControl.PerformLayout();
			this.AccessoryStatusDropEdit.ResumeLayout(true);
			this.AccessoryStatusDropEdit.PerformLayout();
			this.EndUseCodeFindBox.ResumeLayout(true);
			this.EndUseCodeFindBox.PerformLayout();
			this.RewardItemDropEdit.ResumeLayout(true);
			this.RewardItemDropEdit.PerformLayout();
			this.IGSTPaymentGroupBox.ResumeLayout(false);
			this.IGSTPaymentGroupBox.PerformLayout();
			this.AccessoryDescriptionLongTextBox.ResumeLayout(true);
			this.AccessoryDescriptionLongTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal Enterprise.Customs.IN.GUI.PMVFieldsUserControl PMVFieldsUserControl;
	internal Enterprise.ZArchitecture.GUI.ZCalcFindBox TotalPMVCalcFindBox;
	internal Enterprise.ZArchitecture.GUI.ZCalcFindBox UnitPriceCalcFindBox;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit UnitQuantityCalcDropEdit;
	internal ZArchitecture.GUI.ZDropEdit AccessoryStatusDropEdit;
	internal ZArchitecture.GUI.ZCodeFindBox EndUseCodeFindBox;
	internal ZArchitecture.GUI.ZDropEdit RewardItemDropEdit;
	internal ZArchitecture.GUI.ZGroupBox IGSTPaymentGroupBox;
	internal ZArchitecture.GUI.ZCheckBox PaymentOverrideCheckBox;
	internal ZArchitecture.GUI.ZDropEdit TransitCountryDropEdit;
	internal Enterprise.Customs.GUI.LongTextControl AccessoryDescriptionLongTextBox;
}
