
namespace Enterprise.Customs.GB.GUI
{
	partial class ValueBuildUpUserControl
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
            this.ValueBuildupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Box68Calculator = new Enterprise.ZArchitecture.GUI.ZButton();
            this.DiscountPercent = new Enterprise.ZArchitecture.ZCalcEdit();
            this.FARPCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.OSAirTransportValueTextBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.AdjustmentForVATValueCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.OtherChargesDeductionsCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.InsuranceCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.AllowableDiscountCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.FreightChargesCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.ApportionByWeightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ManualValueBuildupCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ValueBuildupGroupBox.SuspendLayout();
            this.FARPCodeFindBox.SuspendLayout();
            this.OSAirTransportValueTextBox.SuspendLayout();
            this.AdjustmentForVATValueCurrencyControl.SuspendLayout();
            this.OtherChargesDeductionsCurrencyControl.SuspendLayout();
            this.InsuranceCurrencyControl.SuspendLayout();
            this.AllowableDiscountCurrencyControl.SuspendLayout();
            this.FreightChargesCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.JobDeclaration);
            // 
            // ValueBuildupGroupBox
            // 
            this.ValueBuildupGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|d0f89def-cf9f-4cb6-b0ba-5dae7bf18da7", "Value Build Up");
            this.ValueBuildupGroupBox.Controls.Add(this.Box68Calculator);
            this.ValueBuildupGroupBox.Controls.Add(this.DiscountPercent);
            this.ValueBuildupGroupBox.Controls.Add(this.FARPCodeFindBox);
            this.ValueBuildupGroupBox.Controls.Add(this.OSAirTransportValueTextBox);
            this.ValueBuildupGroupBox.Controls.Add(this.AdjustmentForVATValueCurrencyControl);
            this.ValueBuildupGroupBox.Controls.Add(this.OtherChargesDeductionsCurrencyControl);
            this.ValueBuildupGroupBox.Controls.Add(this.InsuranceCurrencyControl);
            this.ValueBuildupGroupBox.Controls.Add(this.AllowableDiscountCurrencyControl);
            this.ValueBuildupGroupBox.Controls.Add(this.FreightChargesCurrencyControl);
            this.ValueBuildupGroupBox.Controls.Add(this.ApportionByWeightCheckBox);
            this.ValueBuildupGroupBox.Controls.Add(this.ManualValueBuildupCheckBox);
            this.ValueBuildupGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ValueBuildupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ValueBuildupGroupBox.Name = "ValueBuildupGroupBox";
            this.ValueBuildupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 234, true);
            this.ValueBuildupGroupBox.TabIndex = 0;
            this.ValueBuildupGroupBox.TabStop = false;
            // 
            // Box68Calculator
            // 
            this.Box68Calculator.AutoSize = true;
			this.Box68Calculator.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fbaac9ca-af62-423b-b1e1-305f41726903", "Calculate Box 68");
			this.Box68Calculator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 212, true);
            this.Box68Calculator.Name = "Box68Calculator";
            this.Box68Calculator.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.Box68Calculator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
            this.Box68Calculator.TabIndex = 10;
            this.Box68Calculator.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.Box68Calculator.ToolTipCaption = null;
            this.Box68Calculator.UseVisualStyleBackColor = true;
            this.Box68Calculator.Click += new System.EventHandler(this.Box68Calculator_Click);
            // 
            // DiscountPercent
            // 
            this.BindingSource.SetBindingMember(this.DiscountPercent, "ZG_DiscPerc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).ZG_DiscPerc)));
            this.DiscountPercent.CaptionResourceString = null;
            this.DiscountPercent.DecimalPlaces = 2;
            this.DiscountPercent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 114, true);
            this.DiscountPercent.Name = "DiscountPercent";
            this.DiscountPercent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
            this.DiscountPercent.TabIndex = 6;
            this.DiscountPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // FARPCodeFindBox
            // 
            this.FARPCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FARPCodeFindBox, "JE_IATALoadPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_IATALoadPort)));
            this.FARPCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 39, true);
            this.FARPCodeFindBox.Name = "FARPCodeFindBox";
            this.FARPCodeFindBox.ShouldResize = true;
            this.FARPCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.FARPCodeFindBox.TabIndex = 2;
            // 
            // OSAirTransportValueTextBox
            // 
            this.OSAirTransportValueTextBox.AllowDrop = true;
            this.OSAirTransportValueTextBox.BindToAmount = "ZG_OSAirTransportAmount";
            this.OSAirTransportValueTextBox.BindToList = "Lookups.CurrencyList";
            this.OSAirTransportValueTextBox.BindToUnit = "ZG_RX_NKFrtChg";
            this.OSAirTransportValueTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|cf4d4b5c-c3e0-40ab-a49c-66029afade31", "[62] OS Air Transport Costs");
            this.OSAirTransportValueTextBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.OSAirTransportValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 64, true);
            this.OSAirTransportValueTextBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.OSAirTransportValueTextBox.Name = "OSAirTransportValueTextBox";
            this.OSAirTransportValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
            this.OSAirTransportValueTextBox.TabIndex = 3;
            // 
            // AdjustmentForVATValueCurrencyControl
            // 
            this.AdjustmentForVATValueCurrencyControl.AllowDrop = true;
            this.AdjustmentForVATValueCurrencyControl.BindToAmount = "ZG_VATAdjAmt";
            this.AdjustmentForVATValueCurrencyControl.BindToList = "Lookups.CurrencyList";
            this.AdjustmentForVATValueCurrencyControl.BindToUnit = "ZG_RX_NKVATAdj";
            this.AdjustmentForVATValueCurrencyControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|ab42d359-13f9-4ee0-b5fe-dd3b15c5e411", "[68] Adjustment for VAT Value");
            this.AdjustmentForVATValueCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.AdjustmentForVATValueCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 189, true);
            this.AdjustmentForVATValueCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.AdjustmentForVATValueCurrencyControl.Name = "AdjustmentForVATValueCurrencyControl";
            this.AdjustmentForVATValueCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
            this.AdjustmentForVATValueCurrencyControl.TabIndex = 9;
            // 
            // OtherChargesDeductionsCurrencyControl
            // 
            this.OtherChargesDeductionsCurrencyControl.AllowDrop = true;
            this.OtherChargesDeductionsCurrencyControl.BindToAmount = "ZG_OthChgAmt";
            this.OtherChargesDeductionsCurrencyControl.BindToList = "Lookups.CurrencyList";
            this.OtherChargesDeductionsCurrencyControl.BindToUnit = "ZG_RX_NKOthChg";
            this.OtherChargesDeductionsCurrencyControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|615de5b7-ce34-465a-b23c-d394baef4f74", "[67] Other Charges/Deductions");
            this.OtherChargesDeductionsCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.OtherChargesDeductionsCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 164, true);
            this.OtherChargesDeductionsCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.OtherChargesDeductionsCurrencyControl.Name = "OtherChargesDeductionsCurrencyControl";
            this.OtherChargesDeductionsCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
            this.OtherChargesDeductionsCurrencyControl.TabIndex = 8;
            // 
            // InsuranceCurrencyControl
            // 
            this.InsuranceCurrencyControl.AllowDrop = true;
            this.InsuranceCurrencyControl.BindToAmount = "ZG_InsAmt";
            this.InsuranceCurrencyControl.BindToList = "Lookups.CurrencyList";
            this.InsuranceCurrencyControl.BindToUnit = "ZG_RX_NKIns";
            this.InsuranceCurrencyControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|b67e34d9-28a8-4cb8-b47c-b2c9cd5d8f03", "[66] Insurance");
            this.InsuranceCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.InsuranceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 139, true);
            this.InsuranceCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.InsuranceCurrencyControl.Name = "InsuranceCurrencyControl";
            this.InsuranceCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
            this.InsuranceCurrencyControl.TabIndex = 7;
            // 
            // AllowableDiscountCurrencyControl
            // 
            this.AllowableDiscountCurrencyControl.AllowDrop = true;
            this.AllowableDiscountCurrencyControl.BindToAmount = "ZG_DiscAmt";
            this.AllowableDiscountCurrencyControl.BindToList = "Lookups.CurrencyList";
            this.AllowableDiscountCurrencyControl.BindToUnit = "ZG_RX_NKDisc";
            this.AllowableDiscountCurrencyControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|f692d012-6f3a-4f25-ab72-ac20d0381eeb", "[65] Allowable Discount");
            this.AllowableDiscountCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.AllowableDiscountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 114, true);
            this.AllowableDiscountCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.AllowableDiscountCurrencyControl.Name = "AllowableDiscountCurrencyControl";
            this.AllowableDiscountCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
            this.AllowableDiscountCurrencyControl.TabIndex = 5;
            // 
            // FreightChargesCurrencyControl
            // 
            this.FreightChargesCurrencyControl.AllowDrop = true;
            this.FreightChargesCurrencyControl.BindToAmount = "ZG_FrtChgAmt";
            this.FreightChargesCurrencyControl.BindToList = "Lookups.CurrencyList";
            this.FreightChargesCurrencyControl.BindToUnit = "ZG_RX_NKFrtChg";
            this.FreightChargesCurrencyControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|57822a90-58f1-4a59-99ed-e3f4eefbca0e", "[63] AWB/Freight Charges");
            this.FreightChargesCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.FreightChargesCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 89, true);
            this.FreightChargesCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.FreightChargesCurrencyControl.Name = "FreightChargesCurrencyControl";
            this.FreightChargesCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
            this.FreightChargesCurrencyControl.TabIndex = 4;
            // 
            // ApportionByWeightCheckBox
            // 
            this.ApportionByWeightCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.ApportionByWeightCheckBox, "ZG_ApportionByWeight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).ZG_ApportionByWeight)));
            this.ApportionByWeightCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|f42cfd0c-70ae-4b85-b84a-12067d0518c5", "[64] Apportion By Weight");
            this.ApportionByWeightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ApportionByWeightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 19, true);
            this.ApportionByWeightCheckBox.Name = "ApportionByWeightCheckBox";
            this.ApportionByWeightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.ApportionByWeightCheckBox.TabIndex = 1;
            this.ApportionByWeightCheckBox.UseVisualStyleBackColor = true;
            // 
            // ManualValueBuildupCheckBox
            // 
            this.ManualValueBuildupCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.ManualValueBuildupCheckBox, "ZG_ManualCalc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).ZG_ManualCalc)));
            this.ManualValueBuildupCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ValueBuildUpUserControl|75a20504-ed4b-489f-adc1-0e6673cc727f", "Manual Value Build Up");
            this.ManualValueBuildupCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ManualValueBuildupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 19, true);
            this.ManualValueBuildupCheckBox.Name = "ManualValueBuildupCheckBox";
            this.ManualValueBuildupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.ManualValueBuildupCheckBox.TabIndex = 0;
            this.ManualValueBuildupCheckBox.UseVisualStyleBackColor = true;
            // 
            // ValueBuildUpUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ValueBuildupGroupBox);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 234, true);
            this.Name = "ValueBuildUpUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 234, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ValueBuildupGroupBox.ResumeLayout(false);
            this.ValueBuildupGroupBox.PerformLayout();
            this.FARPCodeFindBox.ResumeLayout(true);
            this.FARPCodeFindBox.PerformLayout();
            this.OSAirTransportValueTextBox.ResumeLayout(true);
            this.OSAirTransportValueTextBox.PerformLayout();
            this.AdjustmentForVATValueCurrencyControl.ResumeLayout(true);
            this.AdjustmentForVATValueCurrencyControl.PerformLayout();
            this.OtherChargesDeductionsCurrencyControl.ResumeLayout(true);
            this.OtherChargesDeductionsCurrencyControl.PerformLayout();
            this.InsuranceCurrencyControl.ResumeLayout(true);
            this.InsuranceCurrencyControl.PerformLayout();
            this.AllowableDiscountCurrencyControl.ResumeLayout(true);
            this.AllowableDiscountCurrencyControl.PerformLayout();
            this.FreightChargesCurrencyControl.ResumeLayout(true);
            this.FreightChargesCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox FARPCodeFindBox;
		private ZArchitecture.ZCalcEdit DiscountPercent;
		private ZArchitecture.GUI.ZButton Box68Calculator;
		private ZArchitecture.GUI.ZGroupBox ValueBuildupGroupBox;
		private Customs.GUI.ConvertToLocalCurrencyControl OSAirTransportValueTextBox;
		private Customs.GUI.ConvertToLocalCurrencyControl AdjustmentForVATValueCurrencyControl;
		private Customs.GUI.ConvertToLocalCurrencyControl OtherChargesDeductionsCurrencyControl;
		private Customs.GUI.ConvertToLocalCurrencyControl InsuranceCurrencyControl;
		private Customs.GUI.ConvertToLocalCurrencyControl AllowableDiscountCurrencyControl;
		private Customs.GUI.ConvertToLocalCurrencyControl FreightChargesCurrencyControl;
		private ZArchitecture.GUI.ZCheckBox ApportionByWeightCheckBox;
		private ZArchitecture.GUI.ZCheckBox ManualValueBuildupCheckBox;
	}
}
