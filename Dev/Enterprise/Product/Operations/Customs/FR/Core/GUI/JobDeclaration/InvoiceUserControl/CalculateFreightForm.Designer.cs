namespace Enterprise.Customs.FR.GUI
{
	partial class CalculateFreightForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.InUEBorderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DomesticCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountDomesticCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.convertToLocalCurrencyControlInsuranceCurrency = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.zCalcEditInsuranceAmountToEUBorder = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEditInsuranceAmountInEUBorder = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEditInsuranceAmountDomestic = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabelFreight = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelInsurance = new Enterprise.ZArchitecture.ZLabel();
			this.zCheckBoxIsInsuranceIncludedInLines = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.amountConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.convertToLocalCurrencyControlInsuranceCurrency.SuspendLayout();
			this.SuspendLayout();
			// 
			// AmountConvertToLocalCurrencyControl
			// 
			this.amountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 135, true);
			this.amountConvertToLocalCurrencyControl.TabIndex = 4;
			// 
			// amountToEUBorderCalcEdit
			// 
			this.amountToEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 156, true);
			this.amountToEUBorderCalcEdit.TabIndex = 5;
			// 
			// amountAfterEUBorderCalcEdit
			// 
			this.amountAfterEUBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 177, true);
			this.amountAfterEUBorderCalcEdit.TabIndex = 6;
			// 
			// AmountDomesticCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountDomesticCalcEdit, "AmountDomestic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).AmountDomestic)));
			this.AmountDomesticCalcEdit.DecimalPlaces = 2;
			this.AmountDomesticCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 199, true);
			this.AmountDomesticCalcEdit.Name = "AmountDomesticCalcEdit";
			this.AmountDomesticCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.AmountDomesticCalcEdit.TabIndex = 7;
			this.AmountDomesticCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCheckBoxIsFreightIncludedInLines
			// 
			this.zCheckBoxIsFreightIncludedInLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBoxIsFreightIncludedInLines.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 225, true);
			this.zCheckBoxIsFreightIncludedInLines.Name = "zCheckBoxIsFreightIncludedInLines";
			this.zCheckBoxIsFreightIncludedInLines.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 24, true);
			this.zCheckBoxIsFreightIncludedInLines.TabIndex = 15;
			// 
			// oKButton
			// 
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 257, true);
			this.oKButton.TabIndex = 17;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 257, true);
			this.cancelButton.TabIndex = 18;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 286, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj);
			//
			// percentageCalcEdit
			//
			this.percentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 29, true);
			// 
			// InUEBorderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InUEBorderCalcEdit, "PercentageInEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).PercentageInEUBorder)));
			this.InUEBorderCalcEdit.DecimalPlaces = 2;
			this.InUEBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 50, true);
			this.InUEBorderCalcEdit.Name = "InUEBorderCalcEdit";
			this.InUEBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.InUEBorderCalcEdit.TabIndex = 2;
			this.InUEBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DomesticCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DomesticCalcEdit, "PercentageDomestic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).PercentageDomestic)));
			this.DomesticCalcEdit.DecimalPlaces = 2;
			this.DomesticCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 71, true);
			this.DomesticCalcEdit.Name = "PercentageDomestic";
			this.DomesticCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.DomesticCalcEdit.TabIndex = 3;
			this.DomesticCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// convertToLocalCurrencyControlInsuranceCurrency
			// 
			this.convertToLocalCurrencyControlInsuranceCurrency.AllowDrop = true;
			this.convertToLocalCurrencyControlInsuranceCurrency.BindToAmount = "InsuranceAmount";
			this.convertToLocalCurrencyControlInsuranceCurrency.BindToList = "CurrencyList";
			this.convertToLocalCurrencyControlInsuranceCurrency.BindToUnit = "InsuranceCurrency";
			this.convertToLocalCurrencyControlInsuranceCurrency.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.convertToLocalCurrencyControlInsuranceCurrency, false);
			this.convertToLocalCurrencyControlInsuranceCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 135, true);
			this.convertToLocalCurrencyControlInsuranceCurrency.Name = "convertToLocalCurrencyControlInsuranceCurrency";
			this.convertToLocalCurrencyControlInsuranceCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.convertToLocalCurrencyControlInsuranceCurrency.TabIndex = 8;
			// 
			// zCalcEditInsuranceAmountToEUBorder
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditInsuranceAmountToEUBorder, "InsuranceAmountToEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).InsuranceAmountToEUBorder)));
			this.zCalcEditInsuranceAmountToEUBorder.CaptionResourceString = null;
			this.zCalcEditInsuranceAmountToEUBorder.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditInsuranceAmountToEUBorder, false);
			this.zCalcEditInsuranceAmountToEUBorder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 156, true);
			this.zCalcEditInsuranceAmountToEUBorder.Name = "zCalcEditInsuranceAmountToEUBorder";
			this.zCalcEditInsuranceAmountToEUBorder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zCalcEditInsuranceAmountToEUBorder.TabIndex = 9;
			this.zCalcEditInsuranceAmountToEUBorder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEditInsuranceAmountInEUBorder
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditInsuranceAmountInEUBorder, "InsuranceAmountInEUBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).InsuranceAmountInEUBorder)));
			this.zCalcEditInsuranceAmountInEUBorder.CaptionResourceString = null;
			this.zCalcEditInsuranceAmountInEUBorder.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditInsuranceAmountInEUBorder, false);
			this.zCalcEditInsuranceAmountInEUBorder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 177, true);
			this.zCalcEditInsuranceAmountInEUBorder.Name = "zCalcEditInsuranceAmountInEUBorder";
			this.zCalcEditInsuranceAmountInEUBorder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zCalcEditInsuranceAmountInEUBorder.TabIndex = 10;
			this.zCalcEditInsuranceAmountInEUBorder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEditInsuranceAmountDomestic
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditInsuranceAmountDomestic, "InsuranceAmountDomestic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).InsuranceAmountDomestic)));
			this.zCalcEditInsuranceAmountDomestic.CaptionResourceString = null;
			this.zCalcEditInsuranceAmountDomestic.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditInsuranceAmountDomestic, false);
			this.zCalcEditInsuranceAmountDomestic.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 199, true);
			this.zCalcEditInsuranceAmountDomestic.Name = "zCalcEditInsuranceAmountDomestic";
			this.zCalcEditInsuranceAmountDomestic.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zCalcEditInsuranceAmountDomestic.TabIndex = 11;
			this.zCalcEditInsuranceAmountDomestic.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCheckBoxIsInsuranceIncludedInLines
			// 
			this.BindingSource.SetBindingMember(this.zCheckBoxIsInsuranceIncludedInLines, "IsInsuranceIncludedInLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj)(null)).IsInsuranceIncludedInLines)));
			this.zCheckBoxIsInsuranceIncludedInLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBoxIsInsuranceIncludedInLines.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 225, true);
			this.zCheckBoxIsInsuranceIncludedInLines.Name = "zCheckBoxIsInsuranceIncludedInLines";
			this.zCheckBoxIsInsuranceIncludedInLines.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 24, true);
			this.zCheckBoxIsInsuranceIncludedInLines.TabIndex = 16;
			// 
			// zLabelFreight
			// 
			this.zLabelFreight.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelFreight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 109, true);
			this.zLabelFreight.Name = "zLabelFreight";
			this.zLabelFreight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabelFreight.TabIndex = 13;
			this.zLabelFreight.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("A1A3EE94-F475-4D5D-A7B8-C2474BFA51E7", "Freight");
			// 
			// zLabelInsurance
			// 
			this.zLabelInsurance.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelInsurance.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 109, true);
			this.zLabelInsurance.Name = "zLabelInsurance";
			this.zLabelInsurance.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabelInsurance.TabIndex = 14;
			this.zLabelInsurance.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("5C1B3993-9D51-49FC-B1AD-15F7604187A6", "Insurance");
			// 
			// CalculateFreightForm
			// 
			this.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("319BD15A-3725-4713-AC99-2D41E257DD81", "Calculate Freight & Insurance");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 310, true);
			this.Controls.Add(this.zLabelInsurance);
			this.Controls.Add(this.zLabelFreight);
			this.Controls.Add(this.zCalcEditInsuranceAmountDomestic);
			this.Controls.Add(this.zCalcEditInsuranceAmountInEUBorder);
			this.Controls.Add(this.zCalcEditInsuranceAmountToEUBorder);
			this.Controls.Add(this.convertToLocalCurrencyControlInsuranceCurrency);
			this.Controls.Add(this.InUEBorderCalcEdit);
			this.Controls.Add(this.DomesticCalcEdit);
			this.Controls.Add(this.AmountDomesticCalcEdit);
			this.Controls.Add(this.zCheckBoxIsInsuranceIncludedInLines);
			this.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.CalculateFreightBizObj);
			this.Name = "CalculateFreightForm";
			this.Text = "CalculateFreightForm";
			this.Controls.SetChildIndex(this.amountConvertToLocalCurrencyControl, 0);
			this.Controls.SetChildIndex(this.AmountDomesticCalcEdit, 0);
			this.Controls.SetChildIndex(this.DomesticCalcEdit, 0);
			this.Controls.SetChildIndex(this.InUEBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.amountToEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.amountAfterEUBorderCalcEdit, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.convertToLocalCurrencyControlInsuranceCurrency, 0);
			this.Controls.SetChildIndex(this.zCalcEditInsuranceAmountToEUBorder, 0);
			this.Controls.SetChildIndex(this.zCalcEditInsuranceAmountInEUBorder, 0);
			this.Controls.SetChildIndex(this.zCalcEditInsuranceAmountDomestic, 0);
			this.Controls.SetChildIndex(this.zLabelFreight, 0);
			this.Controls.SetChildIndex(this.zLabelInsurance, 0);
			this.Controls.SetChildIndex(this.zCheckBoxIsInsuranceIncludedInLines, 0);
			this.amountConvertToLocalCurrencyControl.ResumeLayout(true);
			this.amountConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.convertToLocalCurrencyControlInsuranceCurrency.ResumeLayout(true);
			this.convertToLocalCurrencyControlInsuranceCurrency.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZCalcEdit InUEBorderCalcEdit;
		ZArchitecture.ZCalcEdit DomesticCalcEdit;
		ZArchitecture.ZCalcEdit AmountDomesticCalcEdit;
		Customs.GUI.ConvertToLocalCurrencyControl convertToLocalCurrencyControlInsuranceCurrency;
		ZArchitecture.ZCalcEdit zCalcEditInsuranceAmountToEUBorder;
		ZArchitecture.ZCalcEdit zCalcEditInsuranceAmountInEUBorder;
		ZArchitecture.ZCalcEdit zCalcEditInsuranceAmountDomestic;
		ZArchitecture.ZLabel zLabelFreight;
		ZArchitecture.ZLabel zLabelInsurance;
		ZArchitecture.GUI.ZCheckBox zCheckBoxIsInsuranceIncludedInLines;
	}
}
