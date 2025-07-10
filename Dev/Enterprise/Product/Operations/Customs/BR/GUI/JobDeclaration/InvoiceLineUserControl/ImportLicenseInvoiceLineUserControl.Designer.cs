
namespace Enterprise.Customs.BR.GUI
{
	partial class ImportLicenseInvoiceLineUserControl
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
            this.BRTariffDetailsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ImportTariffDetailsUserControl = new Enterprise.Customs.BR.GUI.ImportTariffDetailsUserControl();
            this.AdditionalDetailsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.AdditionalDetailsUserControl = new Enterprise.Customs.BR.GUI.AdditionalDetailsUserControl();
            this.ManufacturerDetailsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ManufacturerDetailsUserControl = new Enterprise.Customs.BR.GUI.ManufacturerDetailsUserControl();
            this.JI_CEIGuidDropEdit.SuspendLayout();
            this.JI_Calc_InvAmountControl.SuspendLayout();
            this.InvoiceLinesSummaryGroupBox.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.LineDetailTabControl.SuspendLayout();
            this.InvoiceDetailsGroupBox.SuspendLayout();
            this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
            this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
            this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
            this.ClassificationDetailsGroupBox.SuspendLayout();
            this.LineChargesTabPage.SuspendLayout();
            this.CurrentInvoicePanel.SuspendLayout();
            this.LineSummaryPanel.SuspendLayout();
            this.ContainersTabPage.SuspendLayout();
            this.ContainersGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
            this.CusContainerInvoiceLineGrid.SuspendLayout();
            this.LineDetailsTabPage.SuspendLayout();
            this.NewLineDetailsTabPage.SuspendLayout();
            this.InvoiceLineDetailsUserControl.SuspendLayout();
            this.ClassificationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
            this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
            this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
            this.CustomsQuantityCalcDropEdit.SuspendLayout();
            this.VolumeCalcDropEdit.SuspendLayout();
            this.JI_WeightCalcDropEdit.SuspendLayout();
            this.InvoiceQuantityCalcDropEdit.SuspendLayout();
            this.JI_DescriptionBoundTextBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BRTariffDetailsTab.SuspendLayout();
            this.ImportTariffDetailsUserControl.SuspendLayout();
            this.AdditionalDetailsTab.SuspendLayout();
            this.AdditionalDetailsUserControl.SuspendLayout();
            this.ManufacturerDetailsTab.SuspendLayout();
            this.ManufacturerDetailsUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // JI_CEIGuidDropEdit
            // 
            this.JI_CEIGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 10, true);
            // 
            // JI_Calc_InvAmountControl
            // 
            this.JI_Calc_InvAmountControl.TabIndex = 6;
            // 
            // InvoiceLinesSummaryGroupBox
            // 
            this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(823, 0, true);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 394, true);
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 320, true);
            // 
            // TopPanel
            // 
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 663, true);
            // 
            // LineDetailTabControl
            // 
            this.LineDetailTabControl.Controls.Add(this.ManufacturerDetailsTab);
            this.LineDetailTabControl.Controls.Add(this.AdditionalDetailsTab);
            this.LineDetailTabControl.Controls.Add(this.BRTariffDetailsTab);
            this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 320, true);
            this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.BRTariffDetailsTab, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.AdditionalDetailsTab, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.ManufacturerDetailsTab, 0);
            // 
            // ClassificationDetailsGroupBox
            // 
            this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 46, true);
            // 
            // CantCreateInvoiceLinesLabel
            // 
            this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
            this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 663, true);
            // 
            // ClassificationPanel
            // 
            this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 46, true);
            // 
            // CustomsInvoiceLinesBoundGrid
            // 
            this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 663, true);
            // 
            // JI_Calc_CIFConvertToLocalCurrencyControl
            // 
            this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 3;
            // 
            // JI_Calc_InsuranceConvertToLocalCurrencyControl
            // 
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 2;
            // 
            // JI_Calc_FreightConvertToLocalCurrencyControl
            // 
            this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 1;
            // 
            // JI_Calc_FOBConvertToLocalCurrencyControl
            // 
            this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 0;
            // 
            // JI_Calc_GSTConvertToLocalCurrencyControl
            // 
            this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 5;
            // 
            // JI_Calc_DutyConvertToLocalCurrencyControl
            // 
            this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 4;
            // 
            // CustomsQuantityCalcDropEdit
            // 
            this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 10, true);
            // 
            // Splitter
            // 
            this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 384, true);
            this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 10, true);
            // 
            // BRTariffDetailsTab
            // 
            this.BRTariffDetailsTab.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("8FE37A78-9455-4CDD-9025-9B163889460A", "Tariff Details");
            this.BRTariffDetailsTab.Controls.Add(this.ImportTariffDetailsUserControl);
            this.BRTariffDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.BRTariffDetailsTab.Name = "BRTariffDetailsTab";
            this.BRTariffDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.BRTariffDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 293, true);
            this.BRTariffDetailsTab.TabIndex = 5;
            this.BRTariffDetailsTab.UseVisualStyleBackColor = true;
            // 
            // ImportTariffDetailsUserControl
            // 
            this.ImportTariffDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImportTariffDetailsUserControl, "FilteredInvoiceLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)))));
            this.ImportTariffDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportTariffDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.ImportTariffDetailsUserControl.Name = "ImportTariffDetailsUserControl";
            this.ImportTariffDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 287, true);
            this.ImportTariffDetailsUserControl.TabIndex = 0;
            // 
            // AdditionalDetailsTab
            // 
            this.AdditionalDetailsTab.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("A25492CF-8620-4C2F-AAEB-DDBD807E801B", "Additional Details");
            this.AdditionalDetailsTab.Controls.Add(this.AdditionalDetailsUserControl);
            this.AdditionalDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.AdditionalDetailsTab.Name = "AdditionalDetailsTab";
            this.AdditionalDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.AdditionalDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
            this.AdditionalDetailsTab.TabIndex = 6;
            this.AdditionalDetailsTab.UseVisualStyleBackColor = true;
            // 
            // AdditionalDetailsUserControl
            // 
            this.AdditionalDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AdditionalDetailsUserControl, "FilteredInvoiceLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)))));
            this.AdditionalDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AdditionalDetailsUserControl.Name = "AdditionalDetailsUserControl";
            this.AdditionalDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 287, true);
            this.AdditionalDetailsUserControl.TabIndex = 0;
            // 
            // ManufacturerDetailsTab
            // 
            this.ManufacturerDetailsTab.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("893F7857-BEE0-4AA8-BBA5-1A7B74AD1FF6", "Manufacturer Details");
            this.ManufacturerDetailsTab.Controls.Add(this.ManufacturerDetailsUserControl);
            this.ManufacturerDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ManufacturerDetailsTab.Name = "ManufacturerDetailsTab";
            this.ManufacturerDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.ManufacturerDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 293, true);
            this.ManufacturerDetailsTab.TabIndex = 7;
            this.ManufacturerDetailsTab.UseVisualStyleBackColor = true;
            // 
            // ManufacturerDetailsUserControl
            // 
            this.ManufacturerDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ManufacturerDetailsUserControl, "FilteredInvoiceLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)))));
            this.ManufacturerDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ManufacturerDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.ManufacturerDetailsUserControl.Name = "ManufacturerDetailsUserControl";
            this.ManufacturerDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 287, true);
            this.ManufacturerDetailsUserControl.TabIndex = 0;
            // 
            // ImportLicenseInvoiceLineUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "ImportLicenseInvoiceLineUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 714, true);
            this.JI_CEIGuidDropEdit.ResumeLayout(true);
            this.JI_CEIGuidDropEdit.PerformLayout();
            this.JI_Calc_InvAmountControl.ResumeLayout(true);
            this.JI_Calc_InvAmountControl.PerformLayout();
            this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
            this.InvoiceLinesSummaryGroupBox.PerformLayout();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.LineDetailTabControl.ResumeLayout(false);
            this.LineDetailTabControl.PerformLayout();
            this.InvoiceDetailsGroupBox.ResumeLayout(false);
            this.InvoiceDetailsGroupBox.PerformLayout();
            this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
            this.JI_CountryOfOriginBoundFindBox.PerformLayout();
            this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
            this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
            this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
            this.JI_LinePriceBoundCurrencyControl.PerformLayout();
            this.ClassificationDetailsGroupBox.ResumeLayout(false);
            this.ClassificationDetailsGroupBox.PerformLayout();
            this.LineChargesTabPage.ResumeLayout(false);
            this.LineChargesTabPage.PerformLayout();
            this.CurrentInvoicePanel.ResumeLayout(false);
            this.CurrentInvoicePanel.PerformLayout();
            this.LineSummaryPanel.ResumeLayout(false);
            this.LineSummaryPanel.PerformLayout();
            this.ContainersTabPage.ResumeLayout(false);
            this.ContainersTabPage.PerformLayout();
            this.ContainersGroupBox.ResumeLayout(false);
            this.ContainersGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
            this.CusContainerInvoiceLineGrid.ResumeLayout(false);
            this.CusContainerInvoiceLineGrid.PerformLayout();
            this.LineDetailsTabPage.ResumeLayout(false);
            this.LineDetailsTabPage.PerformLayout();
            this.NewLineDetailsTabPage.ResumeLayout(false);
            this.NewLineDetailsTabPage.PerformLayout();
            this.InvoiceLineDetailsUserControl.ResumeLayout(true);
            this.InvoiceLineDetailsUserControl.PerformLayout();
            this.ClassificationPanel.ResumeLayout(false);
            this.ClassificationPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
            this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
            this.CustomsInvoiceLinesBoundGrid.PerformLayout();
            this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
            this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
            this.CustomsQuantityCalcDropEdit.PerformLayout();
            this.VolumeCalcDropEdit.ResumeLayout(true);
            this.VolumeCalcDropEdit.PerformLayout();
            this.JI_WeightCalcDropEdit.ResumeLayout(true);
            this.JI_WeightCalcDropEdit.PerformLayout();
            this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
            this.InvoiceQuantityCalcDropEdit.PerformLayout();
            this.JI_DescriptionBoundTextBox.ResumeLayout(true);
            this.JI_DescriptionBoundTextBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BRTariffDetailsTab.ResumeLayout(false);
            this.BRTariffDetailsTab.PerformLayout();
            this.ImportTariffDetailsUserControl.ResumeLayout(true);
            this.ImportTariffDetailsUserControl.PerformLayout();
            this.AdditionalDetailsTab.ResumeLayout(false);
            this.AdditionalDetailsTab.PerformLayout();
            this.AdditionalDetailsUserControl.ResumeLayout(true);
            this.AdditionalDetailsUserControl.PerformLayout();
            this.ManufacturerDetailsTab.ResumeLayout(false);
            this.ManufacturerDetailsTab.PerformLayout();
            this.ManufacturerDetailsUserControl.ResumeLayout(true);
            this.ManufacturerDetailsUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabPage BRTariffDetailsTab;
		internal ImportTariffDetailsUserControl ImportTariffDetailsUserControl;
		internal ZArchitecture.GUI.ZTabPage AdditionalDetailsTab;
		internal AdditionalDetailsUserControl AdditionalDetailsUserControl;
		internal ZArchitecture.GUI.ZTabPage ManufacturerDetailsTab;
		internal ManufacturerDetailsUserControl ManufacturerDetailsUserControl;
	}
}
