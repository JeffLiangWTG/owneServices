namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class BaseInvoiceLineUserControl
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
			this.JI_PreviousEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_PreviousEntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsUserControl = new Enterprise.Customs.AsycudaCustoms.GUI.SupportingDocumentsUserControl();
			this.JI_Calc_FOBInLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_CIF_InLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_TariffFindBox.SuspendLayout();
			this.JI_ProcedureFindBox.SuspendLayout();
			this.ProductCodeFindBox.SuspendLayout();
			this.CustomsSecondQuantityCalcDropEdit.SuspendLayout();
			this.CustomsThirdQuantityCalcDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
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
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsUserControl.SuspendLayout();
			this.JI_Calc_FOBInLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_CIF_InLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// JI_TariffFindBox
			// 
			this.JI_TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 42, true);
			this.JI_TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			// 
			// JI_ProcedureFindBox
			// 
			this.JI_ProcedureFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 19, true);
			// 
			// ProductCodeFindBox
			// 
			this.ProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 42, true);
			this.ProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			// 
			// CustomsSecondQuantityCalcDropEdit
			// 
			this.CustomsSecondQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 91, true);
			// 
			// CustomsThirdQuantityCalcDropEdit
			// 
			this.CustomsThirdQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 114, true);
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 137, true);
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 183, true);
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(944, 0, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.AutoScroll = true;
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 320, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 533, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 320, true);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_PreviousEntryNumberTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_PreviousEntryLineNumberCalcEdit);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_ProcedureFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.ProductCodeFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TaxOrFeeDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.PreferenceDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsSecondQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsThirdQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_PreviousEntryLineNumberCalcEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_PreviousEntryNumberTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 180, true);
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 203, true);
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 134, true);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_CIF_InLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_FOBInLocalCurrencyControl);
			this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBInLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIF_InLocalCurrencyControl, 0);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 274, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 533, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.AutoScroll = true;
			this.NewLineDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 245, true);
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 533, true);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("1D22FC50-0A92-49D8-9BAD-4F82F6591C88", "VAT/GST Value");
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("00FC4292-806F-46A0-85D1-A04771308F0C", "Customs Value");
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 176, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 17;
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 129, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 13;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 111, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 160, true);
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 157, true);
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 88, true);
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 65, true);
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 20, true);
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 10, true);
			// 
			// JI_PreviousEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_PreviousEntryNumberTextBox, "FilteredInvoiceLines.JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryNumber)));
			this.JI_PreviousEntryNumberTextBox.CaptionResourceString = null;
			this.JI_PreviousEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 226, true);
			this.JI_PreviousEntryNumberTextBox.Name = "JI_PreviousEntryNumberTextBox";
			this.JI_PreviousEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_PreviousEntryNumberTextBox.TabIndex = 16;
			// 
			// JI_PreviousEntryLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_PreviousEntryLineNumberCalcEdit, "FilteredInvoiceLines.JI_PreviousEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryLineNumber)));
			this.JI_PreviousEntryLineNumberCalcEdit.CaptionResourceString = null;
			this.JI_PreviousEntryLineNumberCalcEdit.DecimalPlaces = 2;
			this.JI_PreviousEntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 226, true);
			this.JI_PreviousEntryLineNumberCalcEdit.Name = "JI_PreviousEntryLineNumberCalcEdit";
			this.JI_PreviousEntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_PreviousEntryLineNumberCalcEdit.TabIndex = 17;
			this.JI_PreviousEntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("81f1ba44-3c5c-4b99-9780-aa1ef916e9a8", "Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 293, true);
			this.SupportingDocumentsTabPage.TabIndex = 3;
			// 
			// SupportingDocumentsUserControl
			// 
			this.SupportingDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 293, true);
			this.SupportingDocumentsUserControl.TabIndex = 0;
			// 
			// JI_Calc_FOBInLocalCurrencyControl
			// 
			this.JI_Calc_FOBInLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_FOBInLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_FOB_InLocalCurrency";
			this.JI_Calc_FOBInLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_FOBInLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_FOBInLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("306f45e8-25e3-4bd8-98c1-204a9b2beb77", "Customs Value", "Customs Value for current line item.");
			this.JI_Calc_FOBInLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 107, true);
			this.JI_Calc_FOBInLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_FOBInLocalCurrencyControl.Name = "JI_Calc_FOBInLocalCurrencyControl";
			this.JI_Calc_FOBInLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_FOBInLocalCurrencyControl.TabIndex = 11;
			// 
			// JI_Calc_CIF_InLocalCurrencyControl
			// 
			this.JI_Calc_CIF_InLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_CIF_InLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_CIF_InLocalCurrency";
			this.JI_Calc_CIF_InLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_CIF_InLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_CIF_InLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("3edaa520-6e58-4c38-9e04-d6ea412a0a1b", "VAT/GST Value", "VAT/GST Value for current line item.");
			this.JI_Calc_CIF_InLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 152, true);
			this.JI_Calc_CIF_InLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_CIF_InLocalCurrencyControl.Name = "JI_Calc_CIF_InLocalCurrencyControl";
			this.JI_Calc_CIF_InLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_CIF_InLocalCurrencyControl.TabIndex = 15;
			// 
			// BaseInvoiceLineUserControl
			// 
			this.Name = "BaseInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 584, true);
			this.JI_TariffFindBox.ResumeLayout(true);
			this.JI_TariffFindBox.PerformLayout();
			this.JI_ProcedureFindBox.ResumeLayout(true);
			this.JI_ProcedureFindBox.PerformLayout();
			this.ProductCodeFindBox.ResumeLayout(true);
			this.ProductCodeFindBox.PerformLayout();
			this.CustomsSecondQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsSecondQuantityCalcDropEdit.PerformLayout();
			this.CustomsThirdQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityCalcDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
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
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.SupportingDocumentsUserControl.ResumeLayout(true);
			this.SupportingDocumentsUserControl.PerformLayout();
			this.JI_Calc_FOBInLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FOBInLocalCurrencyControl.PerformLayout();
			this.JI_Calc_CIF_InLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_CIF_InLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZTextBox JI_PreviousEntryNumberTextBox;
		protected Enterprise.ZArchitecture.ZCalcEdit JI_PreviousEntryLineNumberCalcEdit;
		protected ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected SupportingDocumentsUserControl SupportingDocumentsUserControl;
		private Customs.GUI.ConvertToLocalCurrencyControl JI_Calc_FOBInLocalCurrencyControl;
		private Customs.GUI.ConvertToLocalCurrencyControl JI_Calc_CIF_InLocalCurrencyControl;
	}
}
