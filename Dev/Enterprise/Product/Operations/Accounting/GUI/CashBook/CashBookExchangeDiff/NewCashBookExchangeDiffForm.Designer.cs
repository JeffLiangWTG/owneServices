using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class NewCashBookExchangeDiffForm
	{


		#region Windows Form Designer generated code

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		///
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			components = new Container();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.PostCancelPanel = new ZPanel();
			this.DeselectAllButton = new ZButton();
			this.SelectAllButton = new ZButton();
			this.zPanel1 = new ZPanel();
			this.AdjustmentHeaderPanel = new ZPanel();
			this.AH_PostDateBoundDateEdit = new ZDateEdit();
			this.AH_InvoiceDateBoundDateEdit = new ZDateEdit();
			this.AH_DescBoundTextBox = new ZTextBox();
			this.BankAccountFilterGroupBox = new ZGroupBox();
			this.FilterPanel = new ZPanel();
			this.NotificationGridContainer = new ZPanel();
			this.NewCashBookExchangeDiffGrid = new ZGrid();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.PostCancelPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.AdjustmentHeaderPanel.SuspendLayout();
			this.AH_PostDateBoundDateEdit.SuspendLayout();
			this.AH_InvoiceDateBoundDateEdit.SuspendLayout();
			this.BankAccountFilterGroupBox.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.NotificationGridContainer.SuspendLayout();
			((ISupportInitialize)(this.NewCashBookExchangeDiffGrid)).BeginInit();
			this.NewCashBookExchangeDiffGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 455, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NewCashbookExchangeDiffHeader);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 0, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 29, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// PostCancelPanel
			// 
			this.PostCancelPanel.Controls.Add(this.DeselectAllButton);
			this.PostCancelPanel.Controls.Add(this.SelectAllButton);
			this.PostCancelPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PostCancelPanel.Name = "PostCancelPanel";
			this.PostCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 29, true);
			this.PostCancelPanel.TabIndex = 3;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cb33e2a3-16fc-4d2e-8136-b36350a1d50d", "Deselect All");
			this.DeselectAllButton.IsCaptionOverridden = false;
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 3, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.DeselectAllButton.TabIndex = 1;
			this.DeselectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DeselectAllButton.ToolTipCaption = null;
			this.DeselectAllButton.UseVisualStyleBackColor = true;
			this.DeselectAllButton.Click += new EventHandler(this.DeselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("de8c9f83-ecfb-4910-b073-fde9ffc8a426", "Select All");
			this.SelectAllButton.IsCaptionOverridden = false;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new EventHandler(this.SelectAllButton_Click);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.PostCancelPanel);
			this.zPanel1.Controls.Add(this.PostingButtonsUserControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 29, true);
			this.zPanel1.TabIndex = 7;
			// 
			// AdjustmentHeaderPanel
			// 
			this.AdjustmentHeaderPanel.Controls.Add(this.AH_PostDateBoundDateEdit);
			this.AdjustmentHeaderPanel.Controls.Add(this.AH_InvoiceDateBoundDateEdit);
			this.AdjustmentHeaderPanel.Controls.Add(this.AH_DescBoundTextBox);
			this.AdjustmentHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdjustmentHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdjustmentHeaderPanel.Name = "AdjustmentHeaderPanel";
			this.AdjustmentHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 73, true);
			this.AdjustmentHeaderPanel.TabIndex = 8;
			// 
			// AH_PostDateBoundDateEdit
			// 
			this.AH_PostDateBoundDateEdit.AllowDrop = true;
			this.AH_PostDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateBoundDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewCashbookExchangeDiffHeader)(null)).AH_PostDate)));
			this.AH_PostDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("857cdc41-c98d-4396-929e-8167bd3e9d07", "Post Date");
			this.AH_PostDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 3, true);
			this.AH_PostDateBoundDateEdit.Name = "AH_PostDateBoundDateEdit";
			this.AH_PostDateBoundDateEdit.TabIndex = 2;
			// 
			// AH_InvoiceDateBoundDateEdit
			// 
			this.AH_InvoiceDateBoundDateEdit.AllowDrop = true;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateBoundDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewCashbookExchangeDiffHeader)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("070fb5ac-931e-40d7-8706-698e90d0b29d", "Invoice Date");
			this.AH_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 3, true);
			this.AH_InvoiceDateBoundDateEdit.Name = "AH_InvoiceDateBoundDateEdit";
			this.AH_InvoiceDateBoundDateEdit.TabIndex = 0;
			// 
			// AH_DescBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_DescBoundTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((NewCashbookExchangeDiffHeader)(null)).AH_Desc)));
			this.AH_DescBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("45aabe1b-023c-4b1d-9638-fe6456db071d", "Description");
			this.AH_DescBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 29, true);
			this.AH_DescBoundTextBox.Multiline = true;
			this.AH_DescBoundTextBox.Name = "AH_DescBoundTextBox";
			this.AH_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 38, true);
			this.AH_DescBoundTextBox.TabIndex = 4;
			// 
			// BankAccountFilterGroupBox
			// 
			this.BankAccountFilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a014ccf3-04bb-4fa9-9d27-d23c35a66b94", "Bank Account");
			this.BankAccountFilterGroupBox.Controls.Add(this.FilterPanel);
			this.BankAccountFilterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BankAccountFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 73, true);
			this.BankAccountFilterGroupBox.Name = "BankAccountFilterGroupBox";
			this.BankAccountFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 353, true);
			this.BankAccountFilterGroupBox.TabIndex = 5;
			this.BankAccountFilterGroupBox.TabStop = false;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.NotificationGridContainer);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 274, true);
			this.FilterPanel.TabIndex = 0;
			// 
			// NotificationGridContainer
			// 
			this.NotificationGridContainer.Controls.Add(this.NewCashBookExchangeDiffGrid);
			this.NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NotificationGridContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 163, true);
			this.NotificationGridContainer.Name = "NotificationGridContainer";
			this.NotificationGridContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 171, true);
			this.NotificationGridContainer.TabIndex = 0;
			// 
			// CashBookExchangeDiffGrid
			// 
			this.NewCashBookExchangeDiffGrid.DisableImportDataMenuItem = true;
			this.NewCashBookExchangeDiffGrid.AllowNavigation = false;
			this.NewCashBookExchangeDiffGrid.RemoveAction = RemoveAction.NoRemovePossible;
			this.BindingSource.SetBindingMember(this.NewCashBookExchangeDiffGrid, "NewCashbookExchangeDiffCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).AH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).BankCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).BankCurrencyBalance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).CurrentExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).LocalAmountBeforeAdjustment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).LocalAmountAfterAdjustment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).ForeignCurrencyGainLoss)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).BankAccountDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).AH_NumberOfSupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).ExchangeGainLossAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((NewCashbookExchangeDiff)(((System.Collections.IList)(((NewCashbookExchangeDiffHeader)(null)).NewCashbookExchangeDiffCollection)).SyncRoot)).ExchangeGainLossAccountDesc)));
			this.NewCashBookExchangeDiffGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2de2e4ea-d901-4da7-8916-f49dfff5a1e3", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("007ca52e-ac85-4b35-b6ae-ff5fcd5b0f4a", "Adjustment No.");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("848401f0-22ba-433e-b872-cdfa44d5f85c", "Bank Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_AB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21DB11A3-9DEC-487F-91DE-7DDC63775773", "GL Account");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ExchangeGainLossAccount";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("497d48c3-e247-425f-8afd-b706523a608a", "Currency");
			zTextBoxColumnStyleInfo2.ColumnName = "BankCurrency";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4e5e3b57-1c80-4fe9-a859-4e4dff9f5e5c", "OS Balance");
			zCalcEditColumnStyleInfo1.ColumnName = "BankCurrencyBalance";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6595e1ba-3154-4274-88ce-7ab6c5be828e", "Current Exchange Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentExchangeRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("65a67a42-5c9b-43cd-bc84-d7ccad587016", "Local Balance");
			zCalcEditColumnStyleInfo3.ColumnName = "LocalAmountBeforeAdjustment";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8ee8239f-eef3-4dc6-ae31-a01cf5f4b723", "New Exchange Rate");
			zCalcEditColumnStyleInfo4.ColumnName = "AH_ExchangeRate";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cae9f8d8-6c85-4c4b-a73c-6adaf500dfc5", "Adjusted Local Balance");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalAmountAfterAdjustment";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1bd257f2-469d-447b-9187-57d1b7db2902", "Exchange Gain/(Loss)");
			zCalcEditColumnStyleInfo6.ColumnName = "ForeignCurrencyGainLoss";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("db7e3fb2-1ca7-42ec-840c-7af37c5fb9a1", "Bank Account Description");
			zTextBoxColumnStyleInfo3.ColumnName = "BankAccountDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("D9EDF856-C09E-42E3-A4D3-0754E05370D0", "GL Account Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ExchangeGainLossAccountDesc";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bb725f9a-a24b-474d-9fd8-dd3d4fbaa053", "No. Of Attachment");
			zCalcEditColumnStyleInfo7.ColumnName = "AH_NumberOfSupportingDocuments";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.NewCashBookExchangeDiffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.NewCashBookExchangeDiffGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewCashBookExchangeDiffGrid.GridId = "F23D3F21-6F54-4032-A7E6-2C296A55DD08";
			this.NewCashBookExchangeDiffGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewCashBookExchangeDiffGrid.LayoutKey = "NewCashBookExchangeDiffGrid";
			this.NewCashBookExchangeDiffGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NewCashBookExchangeDiffGrid.Name = "NewCashBookExchangeDiffGrid";
			this.NewCashBookExchangeDiffGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 171, true);
			this.NewCashBookExchangeDiffGrid.TabIndex = 6;
			// 
			// CashBookExchangeDiffNewForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 477, true);
			this.Controls.Add(this.BankAccountFilterGroupBox);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.AdjustmentHeaderPanel);
			this.DataSourceType = typeof(NewCashbookExchangeDiffHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 516, true);
			this.Name = "NewCashBookExchangeDiffForm";
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|20992818-3345-4f9e-ad8e-6680725fb74b", "Bank Currency Adjustment");
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.AdjustmentHeaderPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.BankAccountFilterGroupBox, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.PostCancelPanel.ResumeLayout(false);
			this.PostCancelPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.AdjustmentHeaderPanel.ResumeLayout(false);
			this.AdjustmentHeaderPanel.PerformLayout();
			this.AH_PostDateBoundDateEdit.ResumeLayout(true);
			this.AH_PostDateBoundDateEdit.PerformLayout();
			this.AH_InvoiceDateBoundDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateBoundDateEdit.PerformLayout();
			this.BankAccountFilterGroupBox.ResumeLayout(false);
			this.BankAccountFilterGroupBox.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.NotificationGridContainer.ResumeLayout(false);
			this.NotificationGridContainer.PerformLayout();
			((ISupportInitialize)(this.NewCashBookExchangeDiffGrid)).EndInit();
			this.NewCashBookExchangeDiffGrid.ResumeLayout(false);
			this.NewCashBookExchangeDiffGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZFilterStripControl FilterControl;
		private ZPanel PostCancelPanel;
		private ZButton DeselectAllButton;
		private ZButton SelectAllButton;
		private ZPanel zPanel1;
		private ZPanel AdjustmentHeaderPanel;
		private ZDateEdit AH_PostDateBoundDateEdit;
		private ZDateEdit AH_InvoiceDateBoundDateEdit;
		private ZTextBox AH_DescBoundTextBox;
		private ZGroupBox BankAccountFilterGroupBox;
		private ZPanel FilterPanel;
		private ZPanel NotificationGridContainer;
		private ZGrid NewCashBookExchangeDiffGrid;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private IContainer components;

		#endregion

	}
}