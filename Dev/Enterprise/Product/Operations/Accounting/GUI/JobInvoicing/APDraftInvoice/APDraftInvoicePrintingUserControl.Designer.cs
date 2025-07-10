using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class APDraftInvoicePrintingUserControl
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
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			APDraftInvoiceLinkColumnStyleInfo apDraftInvoiceLinkColumnStyleInfo1 = new APDraftInvoiceLinkColumnStyleInfo();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UploadInvoiceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreditorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TransactionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DraftInvoicesGrid = new APDraftInvoicesGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CreditorFindBox.SuspendLayout();
			this.TransactionTypeDropEdit.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.GridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DraftInvoicesGrid)).BeginInit();
			this.DraftInvoicesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter);
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|28b5b20a-6763-4057-a7fc-1862134dd0e9", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 59, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 3;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|4d4c3bd0-cce5-4856-9c3b-4b1a87bfeb6b", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 59, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindButton.TabIndex = 2;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// UploadInvoiceButton
			// 
			this.UploadInvoiceButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|0d0b4680-1f8d-4917-ad11-d67fde632e51", "Upload Invoice");
			this.UploadInvoiceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 69, true);
			this.UploadInvoiceButton.Name = "UploadInvoiceButton";
			this.UploadInvoiceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.UploadInvoiceButton.TabIndex = 4;
			this.UploadInvoiceButton.ToolTipCaption = null;
			this.UploadInvoiceButton.Click += new System.EventHandler(this.UploadInvoiceButton_Click);
			// 
			// CreditorFindBox
			// 
			this.CreditorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Creditor)));
			this.CreditorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|114175c9-5308-4964-bbeb-1a489169a913", "Creditor", "Creditor", "");
			this.CreditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 11, true);
			this.CreditorFindBox.Name = "CreditorFindBox";
			this.CreditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 17, true);
			this.CreditorFindBox.TabIndex = 0;
			// 
			// TransactionTypeDropEdit
			// 
			this.TransactionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).TransactionType)));
			this.TransactionTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|716f2fb5-0287-4bbc-ae26-6191e33fb39f", "Transaction Type", "Transaction Type", "");
			this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 35, true);
			this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
			this.TransactionTypeDropEdit.PreBoundMaxLength = 3;
			this.TransactionTypeDropEdit.ShowDescriptionBox = false;
			this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.TransactionTypeDropEdit.TabIndex = 1;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.ClearButton);
			this.FilterPanel.Controls.Add(this.TransactionTypeDropEdit);
			this.FilterPanel.Controls.Add(this.FindButton);
			this.FilterPanel.Controls.Add(this.CreditorFindBox);
			this.FilterPanel.Controls.Add(this.UploadInvoiceButton);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 95, true);
			// 
			// GridPanel
			// 
			this.GridPanel.Controls.Add(this.DraftInvoicesGrid);
			this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 350, true);
			// 
			// DraftInvoicesGrid
			// 
			this.DraftInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DraftInvoicesGrid, "Transactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_ExpectedOSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_InternalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_DocumentReceivedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).PostingStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).SystemCreateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).SystemLastEditTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).CreditorName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).AIH_OriginalTransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).OriginalTransactionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccDraftInvoiceHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobDraftInvoicePrintingFilter)(null)).Transactions)).SyncRoot)).OpenInPortal)));
			this.DraftInvoicesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|3EE8BC74-F443-4884-98DE-9C4C9B149F74", "Account");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AIH_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|4EF5F2A7-7FDA-4994-953C-0F756283DAA6", "Transaction Type");
			zTextBoxColumnStyleInfo2.ColumnName = "AIH_TransactionType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|9598A517-47E0-403F-BF9E-7FBFD59E2BEA", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AIH_TransactionNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|3ad53e1e-953d-43a0-93e1-185233c981ef", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "AIH_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|7AD96437-D486-4E7C-A1FA-513BB10F229A", "Invoice Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AIH_ExpectedOSTotalAmount";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|4A784DBB-F1A6-4F40-9356-7694873A716E", "Invoice Date");
			zDateEditColumnStyleInfo1.ColumnName = "AIH_TransactionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|FC16C38C-E176-4A70-AF97-A140E0C36506", "Due Date");
			zDateEditColumnStyleInfo2.ColumnName = "AIH_DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|A460723E-C471-4428-B8B8-57246238ED87", "Internal Reference Number");
			zTextBoxColumnStyleInfo5.ColumnName = "AIH_InternalReference";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|013983CC-23B7-40B6-B4F8-BE4E26AED4E7", "Document Received Date");
			zDateEditColumnStyleInfo3.ColumnName = "AIH_DocumentReceivedDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|13F18151-6212-4230-AA0A-D71F196449D8", "Status");
			zTextBoxColumnStyleInfo6.ColumnName = "AIH_Status";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|B1957ACA-5BA7-4D62-B168-8BD9DD4D4A89", "Posting Status ");
			zTextBoxColumnStyleInfo7.ColumnName = "PostingStatus";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|F45C1C43-A694-4810-B3E9-4FBA860FE355", "Description");
			zTextBoxColumnStyleInfo8.ColumnName = "AIH_Description";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|46B13ED0-81F1-467E-8331-EF1ABEC96E39", "Creating User");
			zTextBoxColumnStyleInfo9.ColumnName = "AIH_SystemCreateUser";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|5755F4FC-CABC-4C77-A91F-24676D29F7B3", "Created Date/Time");
			zDateEditColumnStyleInfo4.ColumnName = "SystemCreateTimeLocal";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|C8BCF36F-7CFA-4A72-938B-89311D90B070", "Last Edit User");
			zTextBoxColumnStyleInfo10.ColumnName = "AIH_SystemLastEditUser";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|5014AC66-114E-48CD-9097-684B4E350087", "Last Edit Date/Time");
			zDateEditColumnStyleInfo5.ColumnName = "SystemLastEditTimeLocal";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|D587B7D6-DA16-4D2A-9443-4C4DC3ACE68B", "Account Name");
			zTextBoxColumnStyleInfo11.ColumnName = "CreditorName";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|EA4EB61E-14D4-41F1-93F0-54F3BF8671B3", "Original Invoice Number");
			zTextBoxColumnStyleInfo12.ColumnName = "AIH_OriginalTransactionNum";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|22C7A56A-C22A-4F1D-9E0D-29EC72C60E0B", "Original Invoice Description");
			zTextBoxColumnStyleInfo13.ColumnName = "OriginalTransactionDescription";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			apDraftInvoiceLinkColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APDraftInvoicePrintingUserControl|0B146B2B-DD55-4FFD-A57C-F3B9856301D8", "Invoice Processing Portal");
			apDraftInvoiceLinkColumnStyleInfo1.ColumnName = "OpenInPortal";
			apDraftInvoiceLinkColumnStyleInfo1.DefaultCollectionIndex = 0;
			apDraftInvoiceLinkColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.DraftInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DraftInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DraftInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DraftInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DraftInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DraftInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DraftInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.DraftInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.DraftInvoicesGrid.ColumnStyles.Add(apDraftInvoiceLinkColumnStyleInfo1);
			this.DraftInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DraftInvoicesGrid.GridId = "8C939DAF-0123-4A86-ADDC-D59843808B96";
			this.DraftInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DraftInvoicesGrid.LayoutKey = "DraftInvoicesGrid";
			this.DraftInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DraftInvoicesGrid.Name = "DraftInvoicesGrid";
			this.DraftInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 350, true);
			// 
			// APDraftInvoicePrintingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridPanel);
			this.Controls.Add(this.FilterPanel);
			this.Name = "APDraftInvoicePrintingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 462, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CreditorFindBox.ResumeLayout(true);
			this.CreditorFindBox.PerformLayout();
			this.TransactionTypeDropEdit.ResumeLayout(true);
			this.TransactionTypeDropEdit.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.GridPanel.ResumeLayout(false);
			this.GridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DraftInvoicesGrid)).EndInit();
			this.DraftInvoicesGrid.ResumeLayout(false);
			this.DraftInvoicesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal APDraftInvoicesGrid DraftInvoicesGrid;
		private Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
		private Enterprise.ZArchitecture.GUI.ZButton UploadInvoiceButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CreditorFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransactionTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel FilterPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel GridPanel;
	}
}
