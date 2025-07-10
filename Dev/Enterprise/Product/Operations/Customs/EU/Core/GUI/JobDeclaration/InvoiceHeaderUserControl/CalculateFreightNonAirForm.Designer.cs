using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateFreightNonAirForm
	{
		protected Enterprise.Customs.GUI.ConvertToLocalCurrencyControl AmountConvertToLocalCurrencyControl;
		protected ZArchitecture.ZCalcEdit AmountToEUBorderCalcEdit;
		protected ZArchitecture.ZCalcEdit AmountToDestinationCountryCalcEdit;
		protected ZArchitecture.ZCalcEdit AmountToFinalDestinationCalcEdit;
		protected ZButton OKButton;
		protected ZButton cancelButton;
		ZArchitecture.ZCalcEdit PercentageToEUBorderCalcEdit;
		ZArchitecture.ZCalcEdit PercentageToDestinationCountryCalcEdit;
		ZArchitecture.ZCalcEdit PercentageToFinalDestinationCalcEdit;

		protected override void InitializeComponent()
		{
			this.AmountConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.PercentageToEUBorderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageToDestinationCountryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageToFinalDestinationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountToEUBorderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountToDestinationCountryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountToFinalDestinationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AmountConvertToLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 24, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj);
			// 
			// AmountConvertToLocalCurrencyControl
			// 
			this.AmountConvertToLocalCurrencyControl.AllowDrop = true;
			this.AmountConvertToLocalCurrencyControl.BindToAmount = "TotalAmount";
			this.AmountConvertToLocalCurrencyControl.BindToList = "CurrencyList";
			this.AmountConvertToLocalCurrencyControl.BindToUnit = "Currency";
			this.AmountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AmountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 8, true);
			this.AmountConvertToLocalCurrencyControl.Name = "AmountConvertToLocalCurrencyControl";
			this.AmountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.AmountConvertToLocalCurrencyControl.TabIndex = 0;
			// 
			// PercentageToEUBorderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageToEUBorderCalcEdit, "PercentageFreightToEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).PercentageFreightToEUBorder)));
			this.PercentageToEUBorderCalcEdit.CaptionResourceString = null;
			this.PercentageToEUBorderCalcEdit.DecimalPlaces = 2;
			this.PercentageToEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 29, true);
			this.PercentageToEUBorderCalcEdit.Name = "PercentageToEUBorderCalcEdit";
			this.PercentageToEUBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.PercentageToEUBorderCalcEdit.TabIndex = 1;
			this.PercentageToEUBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PercentageToDestinationCountryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageToDestinationCountryCalcEdit, "PercentageFreightEUToDestinationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).PercentageFreightEUToDestinationCountry)));
			this.PercentageToDestinationCountryCalcEdit.CaptionResourceString = null;
			this.PercentageToDestinationCountryCalcEdit.DecimalPlaces = 2;
			this.PercentageToDestinationCountryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 50, true);
			this.PercentageToDestinationCountryCalcEdit.Name = "PercentageToDestinationCountryCalcEdit";
			this.PercentageToDestinationCountryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.PercentageToDestinationCountryCalcEdit.TabIndex = 2;
			this.PercentageToDestinationCountryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PercentageToFinalDestinationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageToFinalDestinationCalcEdit, "PercentageFreightToFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).PercentageFreightToFinalDestination)));
			this.PercentageToFinalDestinationCalcEdit.CaptionResourceString = null;
			this.PercentageToFinalDestinationCalcEdit.DecimalPlaces = 0;
			this.PercentageToFinalDestinationCalcEdit.Decimals = 0;
			this.PercentageToFinalDestinationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 71, true);
			this.PercentageToFinalDestinationCalcEdit.Name = "PercentageToFinalDestinationCalcEdit";
			this.PercentageToFinalDestinationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.PercentageToFinalDestinationCalcEdit.TabIndex = 3;
			this.PercentageToFinalDestinationCalcEdit.Text = "0";
			this.PercentageToFinalDestinationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountToEUBorderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountToEUBorderCalcEdit, "AmountToEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).AmountToEUBorder)));
			this.AmountToEUBorderCalcEdit.CaptionResourceString = null;
			this.AmountToEUBorderCalcEdit.DecimalPlaces = 2;
			this.AmountToEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 92, true);
			this.AmountToEUBorderCalcEdit.Name = "AmountToEUBorderCalcEdit";
			this.AmountToEUBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.AmountToEUBorderCalcEdit.TabIndex = 4;
			this.AmountToEUBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountToDestinationCountryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountToDestinationCountryCalcEdit, "AmountToDestinationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).AmountToDestinationCountry)));
			this.AmountToDestinationCountryCalcEdit.CaptionResourceString = null;
			this.AmountToDestinationCountryCalcEdit.DecimalPlaces = 2;
			this.AmountToDestinationCountryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 113, true);
			this.AmountToDestinationCountryCalcEdit.Name = "AmountToDestinationCountryCalcEdit";
			this.AmountToDestinationCountryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.AmountToDestinationCountryCalcEdit.TabIndex = 5;
			this.AmountToDestinationCountryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountToFinalDestinationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountToFinalDestinationCalcEdit, "AmountToFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj)(null)).AmountToFinalDestination)));
			this.AmountToFinalDestinationCalcEdit.CaptionResourceString = null;
			this.AmountToFinalDestinationCalcEdit.DecimalPlaces = 2;
			this.AmountToFinalDestinationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 134, true);
			this.AmountToFinalDestinationCalcEdit.Name = "AmountToFinalDestinationCalcEdit";
			this.AmountToFinalDestinationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.AmountToFinalDestinationCalcEdit.TabIndex = 6;
			this.AmountToFinalDestinationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("cc3e416c-fa29-4447-b925-f291250da195", "&OK");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 160, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.OKButton.TabIndex = 7;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("83f54910-1bb7-4773-a20e-83fa16b1c09d", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 160, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// CalculateFreightNonAirForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("2cdbf3eb-0b84-4b25-932a-fc97cf8b2aa9", "Calculate Freight");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 211, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AmountToFinalDestinationCalcEdit);
			this.Controls.Add(this.AmountToDestinationCountryCalcEdit);
			this.Controls.Add(this.AmountToEUBorderCalcEdit);
			this.Controls.Add(this.PercentageToFinalDestinationCalcEdit);
			this.Controls.Add(this.PercentageToDestinationCountryCalcEdit);
			this.Controls.Add(this.PercentageToEUBorderCalcEdit);
			this.Controls.Add(this.AmountConvertToLocalCurrencyControl);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CalculateFreightNonAirBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CalculateFreightNonAirForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AmountConvertToLocalCurrencyControl, 0);
			this.Controls.SetChildIndex(this.PercentageToEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.PercentageToDestinationCountryCalcEdit, 0);
			this.Controls.SetChildIndex(this.PercentageToFinalDestinationCalcEdit, 0);
			this.Controls.SetChildIndex(this.AmountToEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.AmountToDestinationCountryCalcEdit, 0);
			this.Controls.SetChildIndex(this.AmountToFinalDestinationCalcEdit, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AmountConvertToLocalCurrencyControl.ResumeLayout(true);
			this.AmountConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
