using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateFreightForm
	{
		protected Customs.GUI.ConvertToLocalCurrencyControl amountConvertToLocalCurrencyControl;
		protected ZArchitecture.ZCalcEdit amountToEUBorderCalcEdit;
		protected ZArchitecture.ZCalcEdit amountAfterEUBorderCalcEdit;
		protected ZButton oKButton;
		protected ZButton cancelButton;
		protected ZCheckBox zCheckBoxIsFreightIncludedInLines;
		protected ZArchitecture.ZCalcEdit percentageCalcEdit;

		protected override void InitializeComponent()
		{
			this.amountConvertToLocalCurrencyControl = new Customs.GUI.ConvertToLocalCurrencyControl();
			this.percentageCalcEdit = new ZArchitecture.ZCalcEdit();
			this.amountToEUBorderCalcEdit = new ZArchitecture.ZCalcEdit();
			this.amountAfterEUBorderCalcEdit = new ZArchitecture.ZCalcEdit();
			this.oKButton = new ZButton();
			this.cancelButton = new ZButton();
			this.zCheckBoxIsFreightIncludedInLines = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.amountConvertToLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CalculateFreightBizObj);
			// 
			// AmountConvertToLocalCurrencyControl
			// 
			this.amountConvertToLocalCurrencyControl.AllowDrop = true;
			this.amountConvertToLocalCurrencyControl.BindToAmount = "Amount";
			this.amountConvertToLocalCurrencyControl.BindToList = "CurrencyList";
			this.amountConvertToLocalCurrencyControl.BindToUnit = "Currency";
			this.amountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.amountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 8, true);
			this.amountConvertToLocalCurrencyControl.Name = "AmountConvertToLocalCurrencyControl";
			this.amountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.amountConvertToLocalCurrencyControl.TabIndex = 0;
			// 
			// PercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.percentageCalcEdit, "Percentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CalculateFreightBizObj)(null)).Percentage);
			this.percentageCalcEdit.CaptionResourceString = null;
			this.percentageCalcEdit.DecimalPlaces = 2;
			this.percentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 29, true);
			this.percentageCalcEdit.Name = "PercentageCalcEdit";
			this.percentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.percentageCalcEdit.TabIndex = 1;
			this.percentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountToEUBorderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.amountToEUBorderCalcEdit, "AmountToEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CalculateFreightBizObj)(null)).AmountToEUBorder);
			this.amountToEUBorderCalcEdit.CaptionResourceString = null;
			this.amountToEUBorderCalcEdit.DecimalPlaces = 2;
			this.amountToEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 51, true);
			this.amountToEUBorderCalcEdit.Name = "AmountToEUBorderCalcEdit";
			this.amountToEUBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.amountToEUBorderCalcEdit.TabIndex = 2;
			this.amountToEUBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountAfterEUBorderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.amountAfterEUBorderCalcEdit, "AmountAfterEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CalculateFreightBizObj)(null)).AmountAfterEUBorder);
			this.amountAfterEUBorderCalcEdit.CaptionResourceString = null;
			this.amountAfterEUBorderCalcEdit.DecimalPlaces = 2;
			this.amountAfterEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 72, true);
			this.amountAfterEUBorderCalcEdit.Name = "AmountAfterEUBorderCalcEdit";
			this.amountAfterEUBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.amountAfterEUBorderCalcEdit.TabIndex = 3;
			this.amountAfterEUBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OKButton
			// 
			this.oKButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.oKButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("73abcd10-d350-45ca-bff8-b71927a15cbc", "&OK");
			this.oKButton.IsCaptionOverridden = false;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 129, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.oKButton.TabIndex = 5;
			this.oKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.oKButton.ToolTipCaption = null;
			this.oKButton.UseVisualStyleBackColor = true;
			this.oKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("364cf022-d5b5-4305-b2c2-58c9c18c6e64", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 129, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
			// 
			// zCheckBoxIsFreightIncludedInLines
			// 
			this.BindingSource.SetBindingMember(this.zCheckBoxIsFreightIncludedInLines, "IsFreightIncludedInLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CalculateFreightBizObj)(null)).IsFreightIncludedInLines);
			this.zCheckBoxIsFreightIncludedInLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBoxIsFreightIncludedInLines.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 93, true);
			this.zCheckBoxIsFreightIncludedInLines.Name = "zCheckBoxIsFreightIncludedInLines";
			this.zCheckBoxIsFreightIncludedInLines.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 24, true);
			this.zCheckBoxIsFreightIncludedInLines.TabIndex = 4;
			// 
			// CalculateFreightForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("bf9f57ea-9fc5-4b74-a178-0e0a23c3e2a9", "Calculate Freight");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 179, true);
			this.Controls.Add(this.zCheckBoxIsFreightIncludedInLines);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.amountAfterEUBorderCalcEdit);
			this.Controls.Add(this.amountToEUBorderCalcEdit);
			this.Controls.Add(this.percentageCalcEdit);
			this.Controls.Add(this.amountConvertToLocalCurrencyControl);
			this.DataSourceType = typeof(CalculateFreightBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CalculateFreightForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.amountConvertToLocalCurrencyControl, 0);
			this.Controls.SetChildIndex(this.percentageCalcEdit, 0);
			this.Controls.SetChildIndex(this.amountToEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.amountAfterEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.zCheckBoxIsFreightIncludedInLines, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.amountConvertToLocalCurrencyControl.ResumeLayout(true);
			this.amountConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
