using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ReceiptBatchForm
	{


		#region Windows Form Designer generated code

		private ZGroupBox zGroupBox1;
		private ZGrid ReceiptBatchGrid;
		private ZPostOrCancelButton CancelPostingButton;
		private ZPostOrCancelButton SaveAndCloseButton;
		private ZGuidFindBox BankAccountGuidFindBox;
		private ZDropEdit ReceiptTypeDropEdit;
		private ZGroupBox ReceiptDetailsGroupBox;
		private ZTextBox DescriptionTextBox;
		private ZDateEdit PostDateEdit;
		private ZDateEdit InvoiceDateEdit;
		private ZExchangeRateControl zExchangeRateControl1;
		private ZGroupBox zGroupBox2;
		private ZCheckBox MatchAfterPostingZCheckBox;
		private ZCalcFindBox LocalCurrencyAmountZCalcFindBox;
		private ZCalcFindBox ForeignAmountCalcFindBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel kFlowLayoutPanel1;
		private ZCheckBox CreateDepositSlipZCheckBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			this.zGroupBox1 = new ZGroupBox();
			this.kFlowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.CancelPostingButton = new ZPostOrCancelButton();
			this.SaveAndCloseButton = new ZPostOrCancelButton();
			this.LocalCurrencyAmountZCalcFindBox = new ZCalcFindBox();
			this.ReceiptBatchGrid = new ZGrid();
			this.ForeignAmountCalcFindBox = new ZCalcFindBox();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.ReceiptTypeDropEdit = new ZDropEdit();
			this.zExchangeRateControl1 = new ZExchangeRateControl();
			this.ReceiptDetailsGroupBox = new ZGroupBox();
			this.DescriptionTextBox = new ZTextBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.zGroupBox2 = new ZGroupBox();
			this.MatchAfterPostingZCheckBox = new ZCheckBox();
			this.CreateDepositSlipZCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.kFlowLayoutPanel1.SuspendLayout();
			this.LocalCurrencyAmountZCalcFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptBatchGrid)).BeginInit();
			this.ReceiptBatchGrid.SuspendLayout();
			this.ForeignAmountCalcFindBox.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.ReceiptTypeDropEdit.SuspendLayout();
			this.zExchangeRateControl1.SuspendLayout();
			this.ReceiptDetailsGroupBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 388, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(538);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ARReceiptBatchPoster);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|2204cda3-cc3d-4906-93fc-58b92f80a673", "Receipt Batch");
			this.zGroupBox1.Controls.Add(this.kFlowLayoutPanel1);
			this.zGroupBox1.Controls.Add(this.LocalCurrencyAmountZCalcFindBox);
			this.zGroupBox1.Controls.Add(this.ReceiptBatchGrid);
			this.zGroupBox1.Controls.Add(this.ForeignAmountCalcFindBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 153, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 230, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			// 
			// kFlowLayoutPanel1
			// 
			this.kFlowLayoutPanel1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.kFlowLayoutPanel1.Controls.Add(this.CancelPostingButton);
			this.kFlowLayoutPanel1.Controls.Add(this.SaveAndCloseButton);
			this.kFlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.kFlowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 198, true);
			this.kFlowLayoutPanel1.Name = "kFlowLayoutPanel1";
			this.kFlowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 28, true);
			this.kFlowLayoutPanel1.TabIndex = 9;
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.AutoSize = true;
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|c41327ff-9840-4f80-b5f1-7781e354148f", "Close");
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 2, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPostingButton.TabIndex = 8;
			this.CancelPostingButton.UseVisualStyleBackColor = true;
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndCloseButton.AutoSize = true;
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|cb9273bc-21a6-43d7-bb8c-16f2d75be87b", "Post And Close");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 2, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.SaveAndCloseButton.TabIndex = 7;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			// 
			// LocalCurrencyAmountZCalcFindBox
			// 
			this.LocalCurrencyAmountZCalcFindBox.AllowDrop = true;
			this.LocalCurrencyAmountZCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LocalCurrencyAmountZCalcFindBox.BindToAmount = "LocalCurrencyTotal";
			this.LocalCurrencyAmountZCalcFindBox.BindToDecimalPlaces = "Calc_LocalRXDecimals";
			this.LocalCurrencyAmountZCalcFindBox.BindToUnit = "Calc_LocalRX_NK";
			this.LocalCurrencyAmountZCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|0d024b6a-d847-41dd-9b90-a34aaaf03bc2", "Local Currency Total");
			this.LocalCurrencyAmountZCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalCurrencyAmountZCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 203, true);
			this.LocalCurrencyAmountZCalcFindBox.Name = "LocalCurrencyAmountZCalcFindBox";
			this.LocalCurrencyAmountZCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalCurrencyAmountZCalcFindBox.TabIndex = 6;
			// 
			// ReceiptBatchGrid
			// 
			this.ReceiptBatchGrid.AllowNavigation = false;
			this.ReceiptBatchGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReceiptBatchGrid, "ReceiptBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_Calc_RXDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_Calc_LocalRXDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_ChequeDrawer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_DrawerBank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).AH_DrawerBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ARReceipt)(((System.Collections.IList)(((ARReceiptBatchPoster)(null)).ReceiptBatch)).SyncRoot)).IncludeInDepositBatch)));
			this.ReceiptBatchGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|e9b5b196-41d6-400e-877d-289768b8fcf7", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|cef61642-a833-43e5-8492-72d11ca3499a", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "AH_ReceiptType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_AB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|eb739a83-ec6c-4ae8-96bc-37bfa1e14e28", "Reference Number");
			zTextBoxColumnStyleInfo2.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_ExchangeRate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|da665fd7-731f-4c57-914b-a79d3cd75df0", "Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|80594aa0-1999-4c1c-9902-aade885ab065", "Local Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_LocalExTaxAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|a75a3c8a-1246-4bb3-a6a5-d873b0d0f281", "Drawer");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_ChequeDrawer";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|960a6bae-a335-4053-b46e-f09b9928395f", "Bank");
			zTextBoxColumnStyleInfo4.ColumnName = "AH_DrawerBank";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|1a792c88-391b-42c9-a2bf-fcd9ada156d0", "Branch");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_DrawerBranch";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|380cd1d2-b220-4c3a-aa6f-1e07ca322bf9", "Include In Deposit Batch");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInDepositBatch";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReceiptBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ReceiptBatchGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReceiptBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReceiptBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ReceiptBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ReceiptBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReceiptBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReceiptBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ReceiptBatchGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReceiptBatchGrid.CopySelectedRowsAllowed = true;
			this.ReceiptBatchGrid.GridId = "51fa8d0e-d6cf-48ff-8343-b199a695f933";
			this.ReceiptBatchGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceiptBatchGrid.LayoutKey = "zDisplayGrid1";
			this.ReceiptBatchGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.ReceiptBatchGrid.Name = "ReceiptBatchGrid";
			this.ReceiptBatchGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ReceiptBatchGrid.ShouldSetErrorsOnTabPage = false;
			this.ReceiptBatchGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 173, true);
			this.ReceiptBatchGrid.TabIndex = 0;
			// 
			// ForeignAmountCalcFindBox
			// 
			this.ForeignAmountCalcFindBox.AllowDrop = true;
			this.ForeignAmountCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ForeignAmountCalcFindBox.BindToAmount = "ForeignCurrencyTotal";
			this.ForeignAmountCalcFindBox.BindToDecimalPlaces = "Calc_RXDecimals";
			this.ForeignAmountCalcFindBox.BindToUnit = "RX_NK";
			this.ForeignAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|e949c09b-eff3-44f4-a46a-c9389f91972c", "Foreign Currency Total");
			this.ForeignAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ForeignAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 203, true);
			this.ForeignAmountCalcFindBox.Name = "ForeignAmountCalcFindBox";
			this.ForeignAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ForeignAmountCalcFindBox.TabIndex = 4;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "BankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARReceiptBatchPoster)(null)).BankAccountPK)));
			this.BankAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|08020dd5-cd3b-48d9-954b-98236ba16c18", "Bank", "Bank Account", "");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 40, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.PopupCaption = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 9;
			// 
			// ReceiptTypeDropEdit
			// 
			this.ReceiptTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptTypeDropEdit, "ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ARReceiptBatchPoster)(null)).ReceiptType)));
			this.ReceiptTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|b10f89db-3bea-437f-bbd1-18ebc3738e0f", "Receipt Type");
			this.ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 16, true);
			this.ReceiptTypeDropEdit.Name = "ReceiptTypeDropEdit";
			this.ReceiptTypeDropEdit.PreBoundMaxLength = 5;
			this.ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.ReceiptTypeDropEdit.TabIndex = 7;
			// 
			// zExchangeRateControl1
			// 
			this.zExchangeRateControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zExchangeRateControl1, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((ARReceiptBatchPoster)(null)).ExchangeRate)));
			this.zExchangeRateControl1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|09a9d382-1eec-4c5d-987b-c33366b2c2d7", "Exchange Rate");
			this.zExchangeRateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 64, true);
			this.zExchangeRateControl1.Name = "zExchangeRateControl1";
			this.zExchangeRateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.zExchangeRateControl1.TabIndex = 11;
			// 
			// ReceiptDetailsGroupBox
			// 
			this.ReceiptDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|7406fa5e-7ec1-4cbd-907e-ffe45cc003cd", "Receipt Details");
			this.ReceiptDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.ReceiptTypeDropEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.zExchangeRateControl1);
			this.ReceiptDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 4, true);
			this.ReceiptDetailsGroupBox.Name = "ReceiptDetailsGroupBox";
			this.ReceiptDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 91, true);
			this.ReceiptDetailsGroupBox.TabIndex = 0;
			this.ReceiptDetailsGroupBox.TabStop = false;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARReceiptBatchPoster)(null)).Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|3a88eb9f-f8d0-491b-8740-6c895bf24f9b", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ARReceiptBatchPoster)(null)).PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|94ef3e9c-6236-48e4-b3c3-66c4d0c58f9d", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 3;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ARReceiptBatchPoster)(null)).InvoiceDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|32376ec9-4ea3-4ff8-b987-07d70d941db8", "Receipt Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 1;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|fc177225-f6c6-4aef-9416-80e74c08a9f5", "On Post Options");
			this.zGroupBox2.Controls.Add(this.MatchAfterPostingZCheckBox);
			this.zGroupBox2.Controls.Add(this.CreateDepositSlipZCheckBox);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 101, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 46, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// MatchAfterPostingZCheckBox
			// 
			this.MatchAfterPostingZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MatchAfterPostingZCheckBox, "MatchAfterPosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ARReceiptBatchPoster)(null)).MatchAfterPosting)));
			this.MatchAfterPostingZCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|503a5806-18d0-4677-995c-40e6b033eda2", "Match After Posting");
			this.MatchAfterPostingZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MatchAfterPostingZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.MatchAfterPostingZCheckBox.Name = "MatchAfterPostingZCheckBox";
			this.MatchAfterPostingZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.MatchAfterPostingZCheckBox.TabIndex = 0;
			this.MatchAfterPostingZCheckBox.UseVisualStyleBackColor = true;
			// 
			// CreateDepositSlipZCheckBox
			// 
			this.CreateDepositSlipZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreateDepositSlipZCheckBox, "CreateDepositSlip");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ARReceiptBatchPoster)(null)).CreateDepositSlip)));
			this.CreateDepositSlipZCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|6bb91e43-29ca-429e-9f4a-64584511ea50", "Create Deposit Slip for Cash, Check and Credit Card Receipts");
			this.CreateDepositSlipZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreateDepositSlipZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 19, true);
			this.CreateDepositSlipZCheckBox.Name = "CreateDepositSlipZCheckBox";
			this.CreateDepositSlipZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 17, true);
			this.CreateDepositSlipZCheckBox.TabIndex = 1;
			this.CreateDepositSlipZCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReceiptBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptBatchForm|0e750575-57a8-40ca-91bb-e9a938c0b09c", "AR Receipt Batch Posting");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 412, true);
			this.Controls.Add(this.ReceiptDetailsGroupBox);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.zGroupBox2);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ARReceiptBatchPoster);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.ReceiptPayment.ARReceiptBatchPoster";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 421, true);
			this.Name = "ReceiptBatchForm";
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.ReceiptDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.kFlowLayoutPanel1.ResumeLayout(false);
			this.kFlowLayoutPanel1.PerformLayout();
			this.LocalCurrencyAmountZCalcFindBox.ResumeLayout(true);
			this.LocalCurrencyAmountZCalcFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptBatchGrid)).EndInit();
			this.ReceiptBatchGrid.ResumeLayout(false);
			this.ReceiptBatchGrid.PerformLayout();
			this.ForeignAmountCalcFindBox.ResumeLayout(true);
			this.ForeignAmountCalcFindBox.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.ReceiptTypeDropEdit.ResumeLayout(true);
			this.ReceiptTypeDropEdit.PerformLayout();
			this.zExchangeRateControl1.ResumeLayout(true);
			this.zExchangeRateControl1.PerformLayout();
			this.ReceiptDetailsGroupBox.ResumeLayout(false);
			this.ReceiptDetailsGroupBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}