using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobAutoPopulationForm
	{


		#region Windows Form Designer generated code

		private ZPanel BottomPanel;
		private ZGroupBox TotalsGroupBox;
		private ZTextBox TotalAmtOnInvoiceForJobCurrencyTextBox;
		private ZCalcEdit TotalInvoiceAmountIncGSTCalcEdit;
		private ZTextBox GSTTaxCurrencyTextBox;
		private ZCalcEdit GSTCostTaxAmountCalcEdit;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZDateEdit zDateEdit1;
		private ZGrid ChargesGrid;
		private ZDateEdit zDateEdit2;
		private ZGroupBox InvoiceSummaryGroupBox;
		private ZGuidFindBox CreditorsGuidFindBox;
		private ZTextBox InvoiceNumBoundTextEdit;
		private System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			this.zDateEdit1 = new ZDateEdit();
			this.zDateEdit2 = new ZDateEdit();
			this.InvoiceSummaryGroupBox = new ZGroupBox();
			this.DocReceivedDateEdit = new ZDateEdit();
			this.InvoiceNumBoundTextEdit = new ZTextBox();
			this.CreditorsGuidFindBox = new ZGuidFindBox();
			this.BottomPanel = new ZPanel();
			this.TotalsGroupBox = new ZGroupBox();
			this.TotalAmtOnInvoiceForJobCurrencyTextBox = new ZTextBox();
			this.TotalInvoiceAmountIncGSTCalcEdit = new ZCalcEdit();
			this.GSTTaxCurrencyTextBox = new ZTextBox();
			this.GSTCostTaxAmountCalcEdit = new ZCalcEdit();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.ChargesGrid = new ZGrid();
			this.SupplierCostReferenceTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDateEdit1.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.InvoiceSummaryGroupBox.SuspendLayout();
			this.DocReceivedDateEdit.SuspendLayout();
			this.CreditorsGuidFindBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TotalsGroupBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 438, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Job);
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).InvoiceDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|27ca9a26-07a5-4858-9d88-3fd31b6f9a89", "Invoice Date");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 71, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 7;
			// 
			// DocReceivedDateEdit
			// 
			this.DocReceivedDateEdit.AllowDrop = true;
			this.DocReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DocReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DocReceivedDateEdit, "DocumentReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).DocumentReceivedDate)));
			this.DocReceivedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|9360B57C-248F-4E20-824B-25C1DB204DB2", "Document Received Date");
			this.DocReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 71, true);
			this.DocReceivedDateEdit.Name = "DocReceivedDateEdit";
			this.DocReceivedDateEdit.TabIndex = 8;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "InvoiceDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).InvoiceDueDate)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|9fca2496-b6de-41f8-8216-5ba876c100b8", "Due Date");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 71, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 9;
			// 
			// InvoiceSummaryGroupBox
			// 
			this.InvoiceSummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|040684bd-81c6-41da-9036-608ceaf9066c", "Invoice Summary");
			this.InvoiceSummaryGroupBox.Controls.Add(this.DocReceivedDateEdit);
			this.InvoiceSummaryGroupBox.Controls.Add(this.SupplierCostReferenceTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.InvoiceNumBoundTextEdit);
			this.InvoiceSummaryGroupBox.Controls.Add(this.CreditorsGuidFindBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.zDateEdit1);
			this.InvoiceSummaryGroupBox.Controls.Add(this.zDateEdit2);
			this.InvoiceSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceSummaryGroupBox.Name = "InvoiceSummaryGroupBox";
			this.InvoiceSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 101, true);
			this.InvoiceSummaryGroupBox.TabIndex = 0;
			this.InvoiceSummaryGroupBox.TabStop = false;
			// 
			// InvoiceNumBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumBoundTextEdit, "InvoiceNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Job)(null)).InvoiceNum)));
			this.InvoiceNumBoundTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|56696843-761b-4bea-8b71-861f3e4ecdbd", "Invoice Number");
			this.InvoiceNumBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 45, true);
			this.InvoiceNumBoundTextEdit.Name = "InvoiceNumBoundTextEdit";
			this.InvoiceNumBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.InvoiceNumBoundTextEdit.TabIndex = 5;
			this.InvoiceNumBoundTextEdit.Tag = "";
			// 
			// CreditorsGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.CreditorsGuidFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Job)(null)).Creditor)));
			this.CreditorsGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|92a1222d-f447-40f5-96a9-fab7a3889f9f", "Creditor");
			this.CreditorsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.CreditorsGuidFindBox.Name = "CreditorsGuidFindBox";
			this.CreditorsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CreditorsGuidFindBox.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.TotalsGroupBox);
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 315, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 123, true);
			this.BottomPanel.TabIndex = 4;
			// 
			// TotalsGroupBox
			// 
			this.TotalsGroupBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|b66aafe7-f745-44ea-8858-4419c0eea1af", "Totals");
			this.TotalsGroupBox.Controls.Add(this.TotalAmtOnInvoiceForJobCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.TotalInvoiceAmountIncGSTCalcEdit);
			this.TotalsGroupBox.Controls.Add(this.GSTTaxCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.GSTCostTaxAmountCalcEdit);
			this.TotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 5, true);
			this.TotalsGroupBox.Name = "TotalsGroupBox";
			this.TotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 83, true);
			this.TotalsGroupBox.TabIndex = 4;
			this.TotalsGroupBox.TabStop = false;
			// 
			// TotalAmtOnInvoiceForJobCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalAmtOnInvoiceForJobCurrencyTextBox, "ChargesForAutoPopulate.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|5924adb1-a1a0-4965-ae7c-b5a2aa604d7b", "Cost Currency", "Displays the currency of the tax amount.");
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 45, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Name = "TotalAmtOnInvoiceForJobCurrencyTextBox";
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.TabIndex = 5;
			// 
			// TotalInvoiceAmountIncGSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountIncGSTCalcEdit, "ChargesForAutoPopulate.TotalAmountOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).TotalAmountOnInvForJob)));
			this.TotalInvoiceAmountIncGSTCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|26289302-486e-4d7c-8138-b0f5485bcf97", "Invoice Amount");
			this.TotalInvoiceAmountIncGSTCalcEdit.DecimalPlaces = 2;
			this.TotalInvoiceAmountIncGSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.Name = "TotalInvoiceAmountIncGSTCalcEdit";
			this.TotalInvoiceAmountIncGSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.TabIndex = 4;
			this.TotalInvoiceAmountIncGSTCalcEdit.Text = "0.00";
			this.TotalInvoiceAmountIncGSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSTTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.GSTTaxCurrencyTextBox, "ChargesForAutoPopulate.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.GSTTaxCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|1f973126-b56e-4314-b724-5cde5ddcfc71", "Cost Currency", "Displays the currency of the tax amount.");
			this.GSTTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 19, true);
			this.GSTTaxCurrencyTextBox.Name = "GSTTaxCurrencyTextBox";
			this.GSTTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.GSTTaxCurrencyTextBox.TabIndex = 2;
			// 
			// GSTCostTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCostTaxAmountCalcEdit, "ChargesForAutoPopulate.TotalTaxOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).TotalTaxOnInvForJob)));
			this.GSTCostTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|2bd9cd14-c746-4b6d-8755-bd02cf4581c9", "Total Tax", "Total Tax on Invoice for Job", "Total Tax Amount relating to this AP Invoice or Credit Note number relating to this Job Number.");
			this.GSTCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.GSTCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.GSTCostTaxAmountCalcEdit.Name = "GSTCostTaxAmountCalcEdit";
			this.GSTCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.GSTCostTaxAmountCalcEdit.TabIndex = 1;
			this.GSTCostTaxAmountCalcEdit.Text = "0.00";
			this.GSTCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 94, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 23, true);
			this.PostingButtonsUserControl.TabIndex = 5;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargesGrid, "ChargesForAutoPopulate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).IsSelectedForAutoPopulation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_IsApportioned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_OH_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_APInvoiceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_AT_CostGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_OSCostGSTAmt_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_OSCostAmtWithGSTAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_Cost_LocalGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).ChargesForAutoPopulate)).SyncRoot)).JR_Calc_LocalCostAmtWithGST)));
			this.ChargesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|d49e37b4-e033-49c5-b07d-7e431cb0292d", "Select");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelectedForAutoPopulation";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|81594932-770e-4625-ad93-0f37899b8b5f", "Apt", "Apportioned", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "JR_IsApportioned";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JR_OH_CostAccount";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|fd0ddf86-4de8-48c6-b480-91a964af4539", "Invoice Num.", "Invoice Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JR_APInvoiceNum";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zTextBoxColumnStyleInfo2.ColumnName = "JR_Desc";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JR_GE";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|fb2b21f6-02fa-4f43-8a31-6399c9893a2d", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "JR_OSCostAmt";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JR_RX_NKCostCurrency";
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|15f80a31-8bc9-4472-af89-1291a2e50fea", "Tax ID");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JR_AT_CostGSTRate";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|b2f926e3-a75a-428a-b1b0-66af8a8fc01a", "Tax");
			zCalcEditColumnStyleInfo2.ColumnName = "JR_OSCostGSTAmt_Calc";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|05ae2ae7-d63c-4dec-91a5-d6aad18302f5", "Total OS Amount", "Total OS Amount (Including Tax).");
			zCalcEditColumnStyleInfo3.ColumnName = "JR_OSCostAmtWithGSTAmt";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|5dc86b00-237d-44c8-9a6b-34f34f1872cb", "Local Tax");
			zCalcEditColumnStyleInfo5.ColumnName = "JR_Cost_LocalGSTAmount";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|0b82d28e-5b15-41b4-b5ac-ceee418aae99", "Total Local Amount", "Total Local Amount (Including Tax).");
			zCalcEditColumnStyleInfo6.ColumnName = "JR_Calc_LocalCostAmtWithGST";
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ChargesGrid.GridId = "c899972e-1842-4602-98e3-f0c16af65269";
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 214, true);
			this.ChargesGrid.TabIndex = 1;
			// 
			// SupplierCostReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierCostReferenceTextBox, "SupplierCostReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Job)(null)).SupplierCostReference)));
			this.SupplierCostReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("45fbc17c-ecaf-4192-b8b2-7016fd498d9a", "Sup. Cost Ref.", "Supplier Cost Reference", "");
			this.SupplierCostReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(631, 71, true);
			this.SupplierCostReferenceTextBox.Name = "SupplierCostReferenceTextBox";
			this.SupplierCostReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.SupplierCostReferenceTextBox.TabIndex = 10;
			this.SupplierCostReferenceTextBox.Tag = "";
			// 
			// JobAutoPopulationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 464, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobAutoPopulationForm|c363fd65-c502-4962-a919-281eeabca1e7", "Auto Population");
			this.Controls.Add(this.ChargesGrid);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.InvoiceSummaryGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Job);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.Job";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "JobAutoPopulationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoiceSummaryGroupBox, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ChargesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.InvoiceSummaryGroupBox.ResumeLayout(false);
			this.InvoiceSummaryGroupBox.PerformLayout();
			this.DocReceivedDateEdit.ResumeLayout(true);
			this.DocReceivedDateEdit.PerformLayout();
			this.CreditorsGuidFindBox.ResumeLayout(true);
			this.CreditorsGuidFindBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TotalsGroupBox.ResumeLayout(false);
			this.TotalsGroupBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}