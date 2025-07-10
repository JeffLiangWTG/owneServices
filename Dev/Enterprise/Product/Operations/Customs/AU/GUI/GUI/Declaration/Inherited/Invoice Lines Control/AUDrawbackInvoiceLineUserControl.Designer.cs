namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUDrawbackInvoiceLineUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.dutyPercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.dutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.drawbackMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importDeclarationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.importDeclarationLineCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dutyRateControl = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dutyAmountControl = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LineAmberReasonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.selectEntryLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.customsValueControl = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TreatmentCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_AddInfoBoundAddInfoControl = new Enterprise.Customs.AU.Declaration.GUI.AddInfoControlOptionalCMR();
			this.eDNControl = new Enterprise.Customs.AU.Declaration.GUI.EDNFindBox();
			this.tariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffImportFindBox();
			this.tariffFindBoxAUCClass = new Enterprise.Customs.AU.Declaration.GUI.AUCClassFindBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 328, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 328, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 560, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 328, true);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceDetailsGroupBox.Controls.Add(this.zCodeFindBox1);
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceDetailsGroupBox, false);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 111, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.zCodeFindBox1, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 79, true);
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 21, true);
			this.JI_CountryOfOriginBoundFindBox.Visible = false;
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 68, true);
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 68, true);
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 7;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.AutoSize = true;
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox1);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AddInfoBoundAddInfoControl);
			this.ClassificationDetailsGroupBox.Controls.Add(this.TreatmentCodeBoundDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.OverrideCheckBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.customsValueControl);
			this.ClassificationDetailsGroupBox.Controls.Add(this.selectEntryLinesButton);
			this.ClassificationDetailsGroupBox.Controls.Add(this.LineAmberReasonTypeDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.eDNControl);
			this.ClassificationDetailsGroupBox.Controls.Add(this.dutyAmountControl);
			this.ClassificationDetailsGroupBox.Controls.Add(this.dutyRateControl);
			this.ClassificationDetailsGroupBox.Controls.Add(this.importDeclarationLineCalcEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.importDeclarationCodeFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.drawbackMethodDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.tariffFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.tariffFindBoxAUCClass);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 200, true);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.tariffFindBoxAUCClass, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.tariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.drawbackMethodDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.importDeclarationCodeFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.importDeclarationLineCalcEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.dutyRateControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.dutyAmountControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.eDNControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.LineAmberReasonTypeDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.selectEntryLinesButton, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.customsValueControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.OverrideCheckBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TreatmentCodeBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AddInfoBoundAddInfoControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox1, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 301, true);
			// 
			// CurrentInvoicePanel
			// 
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 84, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.dutyRateCalcEdit);
			this.LineSummaryPanel.Controls.Add(this.dutyPercentLabel);
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 225, true);
			this.LineSummaryPanel.Controls.SetChildIndex(this.dutyPercentLabel, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.dutyRateCalcEdit, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 301, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 301, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 282, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 560, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 301, true);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ClassificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 200, true);
			this.ClassificationPanel.TabIndex = 1;
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			zCheckBoxColumnStyleInfo1.Caption = "BOM";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsBOMParentLine";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.Caption = "Parent";
			zTextBoxColumnStyleInfo1.ColumnName = "BOMParentLineNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = "Merged Ln #";
			zTextBoxColumnStyleInfo2.ColumnName = "MergedLineNumber";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.ToolTip = "Merged Line No";
			zDropEditColumnStyleInfo1.BindToList = "AddInfo.Lookups.ZA_DAM_List";
			zDropEditColumnStyleInfo1.Caption = "Method";
			zDropEditColumnStyleInfo1.ColumnName = "AddInfo+ZA_DAM_Hidden";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 4;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.BindToList = "AddInfo.Lookups+GlobalEntryLineKeys";
			zCodeFindBoxColumnStyleInfo1.Caption = "Import Dec Number";
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AddInfo+ZA_DDN_Hidden";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|3fd531e9-3fdb-4b9b-92ea-328ec7ad3676", "Import Declaration");
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.EntryLine;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Line";
			zCalcEditColumnStyleInfo1.ColumnName = "AddInfo+ZA_DDL_Hidden";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|3fd531e9-3fdb-4b9b-92ea-328ec7ad3676", "Import Declaration");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Duty %";
			zCalcEditColumnStyleInfo2.ColumnName = "AddInfo+ZA_DTR_Hidden";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|2b00c93c-63a9-45f4-8cf6-bee86122a7be", "Duty");
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Claim Amount";
			zCalcEditColumnStyleInfo3.ColumnName = "AddInfo+ZA_DDT_Hidden";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|2b00c93c-63a9-45f4-8cf6-bee86122a7be", "Duty");
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo2.BindToList = "AddInfo+Lookups+DrawbackAmberCodeList";
			zDropEditColumnStyleInfo2.Caption = "Amber Reason";
			zDropEditColumnStyleInfo2.ColumnName = "AddInfo+ZA_DARC_Hidden";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Caption = "EDN";
			zTextBoxColumnStyleInfo3.ColumnName = "AddInfo+ZA_EDN_Hidden";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Customs Value";
			zCalcEditColumnStyleInfo4.ColumnName = "AddInfo+ZA_DCV_Hidden";
			zCalcEditColumnStyleInfo4.IsMandatory = true;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 560, true);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 94, true);
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 70, true);
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 46, true);
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 22, true);
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 142, true);
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 118, true);
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 23, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 1;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 79, true);
			this.VolumeCalcDropEdit.TabIndex = 9;
			this.VolumeCalcDropEdit.Visible = false;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 79, true);
			this.JI_WeightCalcDropEdit.TabIndex = 2;
			this.JI_WeightCalcDropEdit.Visible = false;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 19, true);
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 42, true);
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 20, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// DutyPercentLabel
			// 
			this.dutyPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 165, true);
			this.dutyPercentLabel.Name = "DutyPercentLabel";
			this.dutyPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.dutyPercentLabel.TabIndex = 14;
			this.dutyPercentLabel.Text = "Duty Rate(%):";
			// 
			// DutyRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.dutyRateCalcEdit, "FilteredInvoiceLines.AdValoremDutyPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdValoremDutyPercent)));
			this.dutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 166, true);
			this.dutyRateCalcEdit.Name = "DutyRateCalcEdit";
			this.dutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.dutyRateCalcEdit.TabIndex = 15;
			this.dutyRateCalcEdit.Text = "0.00";
			this.dutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DrawbackMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(this.drawbackMethodDropEdit, "FilteredInvoiceLines.AddInfo+ZA_DAM_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DAM_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.Lookups.ZA_DAM_List)));
			this.drawbackMethodDropEdit.BindToList = "FilteredInvoiceLines.AddInfo+Lookups+ZA_DAM_List";
			this.drawbackMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 174, true);
			this.drawbackMethodDropEdit.MaxItemsToShowInDropDown = 4;
			this.drawbackMethodDropEdit.Name = "DrawbackMethodDropEdit";
			this.drawbackMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 20, true);
			this.drawbackMethodDropEdit.TabIndex = 26;
			// 
			// ImportDeclarationCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.importDeclarationCodeFindBox, "FilteredInvoiceLines.AddInfo+ZA_DDN_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DDN_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.Lookups.GlobalEntryLineKeys)));
			this.importDeclarationCodeFindBox.BindToList = "FilteredInvoiceLines.AddInfo+Lookups+GlobalEntryLineKeys";
			this.importDeclarationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 48, true);
			this.importDeclarationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.EntryLine;
			this.importDeclarationCodeFindBox.Name = "ImportDeclarationCodeFindBox";
			this.importDeclarationCodeFindBox.ShowDescriptionBox = false;
			this.importDeclarationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.importDeclarationCodeFindBox.TabIndex = 5;
			// 
			// ImportDeclarationLineCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.importDeclarationLineCalcEdit, "FilteredInvoiceLines.AddInfo+ZA_DDL_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DDL_Hidden)));
			this.importDeclarationLineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 48, true);
			this.importDeclarationLineCalcEdit.Name = "ImportDeclarationLineCalcEdit";
			this.importDeclarationLineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.importDeclarationLineCalcEdit.TabIndex = 7;
			this.importDeclarationLineCalcEdit.Text = "0";
			this.importDeclarationLineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyRateControl
			// 
			this.BindingSource.SetBindingMember(this.dutyRateControl, "FilteredInvoiceLines.AddInfo+ZA_DTR_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DTR_Hidden)));
			this.dutyRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 122, true);
			this.dutyRateControl.Name = "DutyRateControl";
			this.dutyRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.dutyRateControl.TabIndex = 17;
			this.dutyRateControl.Text = "0.0000";
			this.dutyRateControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyAmountControl
			// 
			this.BindingSource.SetBindingMember(this.dutyAmountControl, "FilteredInvoiceLines.AddInfo+ZA_DDT_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DDT_Hidden)));
			this.dutyAmountControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 122, true);
			this.dutyAmountControl.Name = "DutyAmountControl";
			this.dutyAmountControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.dutyAmountControl.TabIndex = 19;
			this.dutyAmountControl.Text = "0.00";
			this.dutyAmountControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineAmberReasonTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.LineAmberReasonTypeDropEdit, "FilteredInvoiceLines.AddInfo+ZA_DARC_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DARC_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.Lookups.DrawbackAmberCodeList)));
			this.LineAmberReasonTypeDropEdit.BindToList = "FilteredInvoiceLines.AddInfo+Lookups+DrawbackAmberCodeList";
			this.LineAmberReasonTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|50d34fc8-6b71-4a99-a119-95aa41426085", "Amber Reason");
			this.LineAmberReasonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 148, true);
			this.LineAmberReasonTypeDropEdit.Name = "LineAmberReasonTypeDropEdit";
			this.LineAmberReasonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 20, true);
			this.LineAmberReasonTypeDropEdit.TabIndex = 24;
			// 
			// SelectEntryLinesButton
			// 
			this.selectEntryLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 20, true);
			this.selectEntryLinesButton.Name = "SelectEntryLinesButton";
			this.selectEntryLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 43, true);
			this.selectEntryLinesButton.TabIndex = 2;
			this.selectEntryLinesButton.Text = "Select Entry Lines for Method B Average Calculation";
			this.selectEntryLinesButton.UseVisualStyleBackColor = true;
			this.selectEntryLinesButton.Click += new System.EventHandler(this.SelectEntryLinesButton_Click);
			// 
			// zCodeFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "FilteredInvoiceLines.JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.PartsList)));
			this.zCodeFindBox1.BindToList = "FilteredInvoiceLines.Lookups+PartsList";
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 18, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierPart;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 35;
			this.zCodeFindBox1.ShowDescriptionBox = false;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.zCodeFindBox1.TabIndex = 1;
			// 
			// CustomsValueControl
			// 
			this.BindingSource.SetBindingMember(this.customsValueControl, "FilteredInvoiceLines.AddInfo+ZA_DCV_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_DCV_Hidden)));
			this.customsValueControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 122, true);
			this.customsValueControl.Name = "CustomsValueControl";
			this.customsValueControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.customsValueControl.TabIndex = 15;
			this.customsValueControl.Text = "0.00";
			this.customsValueControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideCheckBox, "FilteredInvoiceLines.IsDrawbackLineValueOverriden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).IsDrawbackLineValueOverriden)));
			this.OverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 122, true);
			this.OverrideCheckBox.Name = "OverrideCheckBox";
			this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.OverrideCheckBox.TabIndex = 20;
			this.OverrideCheckBox.Text = "Override:";
			// 
			// TreatmentCodeBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TreatmentCodeBoundDropEdit, "FilteredInvoiceLines.AddInfo+ZA_TreatmentCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_TreatmentCode_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.TreatmentCodeList)));
			this.TreatmentCodeBoundDropEdit.BindToList = "FilteredInvoiceLines.AddInfo+TreatmentCodeList";
			this.TreatmentCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 74, true);
			this.TreatmentCodeBoundDropEdit.Name = "TreatmentCodeBoundDropEdit";
			this.TreatmentCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.TreatmentCodeBoundDropEdit.ShowDescriptionBox = false;
			this.TreatmentCodeBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TreatmentCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TreatmentCodeBoundDropEdit.TabIndex = 11;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "FilteredInvoiceLines.DrawbackCalculationMethodComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).DrawbackCalculationMethodComment)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 15, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 50, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// JI_AddInfoBoundAddInfoControl
			// 
			this.BindingSource.SetBindingMember(this.JI_AddInfoBoundAddInfoControl, "FilteredInvoiceLines.AddInfo+AddInfoLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.AddInfoLine)));
			this.JI_AddInfoBoundAddInfoControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDrawbackInvoiceLineUserControl|3325db41-779d-412b-a7f2-db9a53147519", "Add. Info", "Additional Info", "");
			this.JI_AddInfoBoundAddInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 97, true);
			this.JI_AddInfoBoundAddInfoControl.Name = "JI_AddInfoBoundAddInfoControl";
			this.JI_AddInfoBoundAddInfoControl.ShowCMRAddInfo = true;
			this.JI_AddInfoBoundAddInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 20, true);
			this.JI_AddInfoBoundAddInfoControl.TabIndex = 13;
			// 
			// EDNControl
			// 
			this.eDNControl.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.eDNControl, "FilteredInvoiceLines.AddInfo+ZA_EDN_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_EDN_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Declaration.Lookups.ExportDeclarations)));
			this.eDNControl.BindToList = "FilteredInvoiceLines.Declaration+Lookups+ExportDeclarations";
			this.eDNControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 148, true);
			this.eDNControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.JobDeclaration;
			this.eDNControl.Name = "EDNControl";
			this.eDNControl.ShowDescriptionBox = false;
			this.eDNControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.eDNControl.TabIndex = 22;
			// 
			// tariffFindBox
			// 
			this.BindingSource.SetBindingMember(this.tariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 73, true);
			this.tariffFindBox.Name = "tariffFindBox";
			this.tariffFindBox.PreBoundMaxLength = 13;
			this.tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 21, true);
			this.tariffFindBox.TabIndex = 9;
			this.tariffFindBox.Visible = false;
			// 
			// tariffFindBoxAUCClass
			// 
			this.BindingSource.SetBindingMember(this.tariffFindBoxAUCClass, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.tariffFindBoxAUCClass.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 73, true);
			this.tariffFindBoxAUCClass.Name = "tariffFindBoxAUCClass";
			this.tariffFindBoxAUCClass.PreBoundMaxLength = 13;
			this.tariffFindBoxAUCClass.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 21, true);
			this.tariffFindBoxAUCClass.TabIndex = 9;
			this.tariffFindBoxAUCClass.Visible = false;
			// 
			// AUDrawbackInvoiceLineUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "AUDrawbackInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 560, true);
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			this.ClassificationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		protected internal Enterprise.Customs.AU.Declaration.GUI.UniversalTariffImportFindBox tariffFindBox;
		protected internal AUCClassFindBox tariffFindBoxAUCClass;
		protected Enterprise.ZArchitecture.ZLabel dutyPercentLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit dutyRateCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit drawbackMethodDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox importDeclarationCodeFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit importDeclarationLineCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit dutyRateControl;
		private Enterprise.ZArchitecture.ZCalcEdit dutyAmountControl;
		private EDNFindBox eDNControl;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit LineAmberReasonTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton selectEntryLinesButton;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private Enterprise.ZArchitecture.ZCalcEdit customsValueControl;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
		public Enterprise.ZArchitecture.GUI.ZDropEdit TreatmentCodeBoundDropEdit;
		public AddInfoControlOptionalCMR JI_AddInfoBoundAddInfoControl;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
	}
}
