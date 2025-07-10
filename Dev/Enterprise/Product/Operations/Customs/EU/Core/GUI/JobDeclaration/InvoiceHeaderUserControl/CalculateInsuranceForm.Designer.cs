namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateInsuranceForm
	{
		new void InitializeComponent()
		{
			this.InvoiceAmountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InsuranceAmountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InsurancePercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutiablePercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncludedInLinesZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 193, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj);
			// 
			// InvoiceAmountTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceAmountTextBox, "InvoiceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).InvoiceAmount)));
			this.InvoiceAmountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 17, true);
			this.InvoiceAmountTextBox.Name = "InvoiceAmountTextBox";
			this.InvoiceAmountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InvoiceAmountTextBox.TabIndex = 0;
			this.InvoiceAmountTextBox.TabStop = false;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).Currency)));
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 17, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CurrencyTextBox.TabIndex = 1;
			this.CurrencyTextBox.TabStop = false;
			// 
			// InsuranceAmountTextBox
			// 
			this.BindingSource.SetBindingMember(this.InsuranceAmountTextBox, "InsuranceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).InsuranceAmount)));
			this.InsuranceAmountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 69, true);
			this.InsuranceAmountTextBox.Name = "InsuranceAmountTextBox";
			this.InsuranceAmountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InsuranceAmountTextBox.TabIndex = 3;
			this.InsuranceAmountTextBox.TabStop = false;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("C8D2B8DA-3489-4858-ABD8-016943E47D00", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 164, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0E6444E1-3029-4241-B85E-F269AB8F990D", "&OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 164, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.okButton.TabIndex = 6;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// InsurancePercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InsurancePercentageCalcEdit, "InsurancePercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).InsurancePercentage)));
			this.InsurancePercentageCalcEdit.DecimalPlaces = 2;
			this.InsurancePercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 43, true);
			this.InsurancePercentageCalcEdit.Name = "InsurancePercentageCalcEdit";
			this.InsurancePercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InsurancePercentageCalcEdit.TabIndex = 2;
			this.InsurancePercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InsurancePercentageCalcEdit.TrackDisposedAccess = true;
			// 
			// DutiablePercentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutiablePercentCalcEdit, "DutiablePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).DutiablePercent)));
			this.DutiablePercentCalcEdit.DecimalPlaces = 2;
			this.DutiablePercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 96, true);
			this.DutiablePercentCalcEdit.Name = "DutiablePercentCalcEdit";
			this.DutiablePercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DutiablePercentCalcEdit.TabIndex = 4;
			this.DutiablePercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutiablePercentCalcEdit.TrackDisposedAccess = true;
			// 
			// IncludedInLinesZCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludedInLinesZCheckBox, "IsInsuranceIncludedInLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj)(null)).IsInsuranceIncludedInLines)));
			this.IncludedInLinesZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 123, true);
			this.IncludedInLinesZCheckBox.Name = "IncludedInLinesZCheckBox";
			this.IncludedInLinesZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 35, true);
			this.IncludedInLinesZCheckBox.TabIndex = 5;
			this.IncludedInLinesZCheckBox.UseVisualStyleBackColor = true;
			// 
			// CalculateInsuranceForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("42136388-59D6-4A88-A9FF-773D977037AC", "Calculate Insurance");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 217, true);
			this.Controls.Add(this.IncludedInLinesZCheckBox);
			this.Controls.Add(this.DutiablePercentCalcEdit);
			this.Controls.Add(this.InsurancePercentageCalcEdit);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.InsuranceAmountTextBox);
			this.Controls.Add(this.CurrencyTextBox);
			this.Controls.Add(this.InvoiceAmountTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CalculateInsuranceBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 200, true);
			this.Name = "CalculateInsuranceForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoiceAmountTextBox, 0);
			this.Controls.SetChildIndex(this.CurrencyTextBox, 0);
			this.Controls.SetChildIndex(this.InsuranceAmountTextBox, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.InsurancePercentageCalcEdit, 0);
			this.Controls.SetChildIndex(this.DutiablePercentCalcEdit, 0);
			this.Controls.SetChildIndex(this.IncludedInLinesZCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.ZTextBox InvoiceAmountTextBox;
		ZArchitecture.ZTextBox CurrencyTextBox;
		ZArchitecture.ZTextBox InsuranceAmountTextBox;
		ZArchitecture.GUI.ZButton cancelButton;
		ZArchitecture.GUI.ZButton okButton;
		ZArchitecture.ZCalcEdit InsurancePercentageCalcEdit;
		private ZArchitecture.ZCalcEdit DutiablePercentCalcEdit;
		private ZArchitecture.GUI.ZCheckBox IncludedInLinesZCheckBox;
	}
}
