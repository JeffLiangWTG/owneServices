using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class CARMSOAStatementForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.StatementHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatementHeaderDetailsUserControl = new Enterprise.Customs.CA.GUI.CARMSOAStatementHeaderDetailsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatementMessagesUserControl = new Enterprise.Customs.CA.GUI.StatementMessagesUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.TransactionsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.BillingPeriodTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviousStatementBalanceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CorrectionsLastBalanceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PaymentsAfterLastSOACalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DisbursementsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InterestSumCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DebitsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CreditsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeBreakdownGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DutiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExciseCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExciseDutiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SIMACalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InterestCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PaymentsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OthersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ByDayTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatementHeaderGroupBox.SuspendLayout();
			this.StatementHeaderDetailsUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.StatementMessagesUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.TransactionsTabControl.SuspendLayout();
			this.BillingPeriodTabPage.SuspendLayout();
			this.ChargeGroupBox.SuspendLayout();
			this.ChargeBreakdownGroupBox.SuspendLayout();
			this.ByDayTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 597, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f9d2b469-1093-43e5-8f59-3e2521af43a0", "Details");
			this.MainTabPage.Controls.Add(this.TransactionsTabControl);
			this.MainTabPage.Controls.Add(this.StatementHeaderGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 570, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 551, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 570, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 597, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusStatementHeader);
			// 
			// StatementHeaderGroupBox
			// 
			this.StatementHeaderGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("28d73223-291c-48a3-a909-f99896a4529e", "Details");
			this.StatementHeaderGroupBox.Controls.Add(this.StatementHeaderDetailsUserControl);
			this.StatementHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.StatementHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatementHeaderGroupBox.Name = "StatementHeaderGroupBox";
			this.StatementHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 210, true);
			this.StatementHeaderGroupBox.TabIndex = 1;
			this.StatementHeaderGroupBox.TabStop = false;
			// 
			// StatementHeaderDetailsUserControl
			// 
			this.StatementHeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementHeaderDetailsUserControl, ".");
			this.StatementHeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementHeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StatementHeaderDetailsUserControl.Name = "StatementHeaderDetailsUserControl";
			this.StatementHeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 191, true);
			this.StatementHeaderDetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("855d1ce6-0efc-4672-b16d-b5b8dad24a6b", "Messages");
			this.MessagesTabPage.Controls.Add(this.StatementMessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 551, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// StatementMessagesUserControl
			// 
			this.StatementMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementMessagesUserControl, ".");
			this.StatementMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatementMessagesUserControl.Name = "StatementMessagesUserControl";
			this.StatementMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 545, true);
			this.StatementMessagesUserControl.TabIndex = 7;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 2;
			// 
			// TransactionsTabControl
			// 
			this.TransactionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransactionsTabControl.Controls.Add(this.BillingPeriodTabPage);
			this.TransactionsTabControl.Controls.Add(this.ByDayTabPage);
			this.TransactionsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
			this.TransactionsTabControl.Name = "TransactionsTabControl";
			this.TransactionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 360, true);
			this.TransactionsTabControl.TabIndex = 2;
			this.TransactionsTabControl.TabStop = false;
			// 
			// BillingPeriodTabPage
			// 
			this.BillingPeriodTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("43C68B43-AD31-46B5-9EA3-DE8C60EE137A", "Summary - Billing Period");
			this.BillingPeriodTabPage.Controls.Add(this.ChargeGroupBox);
			this.BillingPeriodTabPage.Controls.Add(this.ChargeBreakdownGroupBox);
			this.BillingPeriodTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillingPeriodTabPage.Name = "BillingPeriodTabPage";
			this.BillingPeriodTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 333, true);
			this.BillingPeriodTabPage.TabIndex = 0;
			// 
			// ChargeGroupBox
			// 
			this.ChargeGroupBox.Controls.Add(this.PreviousStatementBalanceCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.CorrectionsLastBalanceCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.PaymentsAfterLastSOACalcEdit);
			this.ChargeGroupBox.Controls.Add(this.DisbursementsCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.InterestSumCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.DebitsCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.CreditsCalcEdit);
			this.ChargeGroupBox.Controls.Add(this.TotalCalcEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeGroupBox, false);
			this.ChargeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ChargeGroupBox.Name = "ChargeGroupBox";
			this.ChargeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 328, true);
			this.ChargeGroupBox.TabIndex = 0;
			this.ChargeGroupBox.TabStop = false;
			// 
			// PreviousStatementBalanceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousStatementBalanceCalcEdit, "PreviousStatementBalance");
			this.PreviousStatementBalanceCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7dd09621-2c72-4e9b-8e65-48bc4d869f04", "Previous Statement Balance");
			this.PreviousStatementBalanceCalcEdit.DecimalPlaces = 2;
			this.PreviousStatementBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 16, true);
			this.PreviousStatementBalanceCalcEdit.Name = "PreviousStatementBalanceCalcEdit";
			this.PreviousStatementBalanceCalcEdit.ReadOnly = true;
			this.PreviousStatementBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.PreviousStatementBalanceCalcEdit.TabIndex = 0;
			this.PreviousStatementBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PreviousStatementBalanceCalcEdit.TrackDisposedAccess = true;
			// 
			// CorrectionsLastBalanceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CorrectionsLastBalanceCalcEdit, "CorrectionsLastBalance");
			this.CorrectionsLastBalanceCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e7b753cf-1f78-4a91-b98e-df596bb064e0", "CorrectionsLastBalance");
			this.CorrectionsLastBalanceCalcEdit.DecimalPlaces = 2;
			this.CorrectionsLastBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 42, true);
			this.CorrectionsLastBalanceCalcEdit.Name = "CorrectionsLastBalanceCalcEdit";
			this.CorrectionsLastBalanceCalcEdit.ReadOnly = true;
			this.CorrectionsLastBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.CorrectionsLastBalanceCalcEdit.TabIndex = 1;
			this.CorrectionsLastBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CorrectionsLastBalanceCalcEdit.TrackDisposedAccess = true;
			// 
			// PaymentsAfterLastSOACalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentsAfterLastSOACalcEdit, "PaymentsAfterLastSOA");
			this.PaymentsAfterLastSOACalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5fec8d7c-648a-4564-91f5-da764f012a67", "Payments After Last SOA");
			this.PaymentsAfterLastSOACalcEdit.DecimalPlaces = 2;
			this.PaymentsAfterLastSOACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 68, true);
			this.PaymentsAfterLastSOACalcEdit.Name = "PaymentsAfterLastSOACalcEdit";
			this.PaymentsAfterLastSOACalcEdit.ReadOnly = true;
			this.PaymentsAfterLastSOACalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.PaymentsAfterLastSOACalcEdit.TabIndex = 2;
			this.PaymentsAfterLastSOACalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaymentsAfterLastSOACalcEdit.TrackDisposedAccess = true;
			// 
			// DisbursementsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DisbursementsCalcEdit, "Disbursements");
			this.DisbursementsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("39c4fff7-ca8f-4de3-9c5e-85c381eced6f", "Disbursements");
			this.DisbursementsCalcEdit.DecimalPlaces = 2;
			this.DisbursementsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 94, true);
			this.DisbursementsCalcEdit.Name = "DisbursementsCalcEdit";
			this.DisbursementsCalcEdit.ReadOnly = true;
			this.DisbursementsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.DisbursementsCalcEdit.TabIndex = 3;
			this.DisbursementsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DisbursementsCalcEdit.TrackDisposedAccess = true;
			// 
			// InterestSumCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InterestSumCalcEdit, "InterestSum");
			this.InterestSumCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("adbc6950-02c1-46ea-b42f-9902fda80c5b", "Interest");
			this.InterestSumCalcEdit.DecimalPlaces = 2;
			this.InterestSumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 120, true);
			this.InterestSumCalcEdit.Name = "InterestSumCalcEdit";
			this.InterestSumCalcEdit.ReadOnly = true;
			this.InterestSumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.InterestSumCalcEdit.TabIndex = 4;
			this.InterestSumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InterestSumCalcEdit.TrackDisposedAccess = true;
			// 
			// DebitsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DebitsCalcEdit, "CurrentPeriodCharges");
			this.DebitsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9955f4c6-4c3a-4cce-a419-4a651368bca4", "Debits");
			this.DebitsCalcEdit.DecimalPlaces = 2;
			this.DebitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 146, true);
			this.DebitsCalcEdit.Name = "DebitsCalcEdit";
			this.DebitsCalcEdit.ReadOnly = true;
			this.DebitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.DebitsCalcEdit.TabIndex = 5;
			this.DebitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DebitsCalcEdit.TrackDisposedAccess = true;
			// 
			// CreditsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CreditsCalcEdit, "CurrentPeriodCredits");
			this.CreditsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("52a5fc47-af3f-486c-8c09-70cfdf79456d", "Credits");
			this.CreditsCalcEdit.DecimalPlaces = 2;
			this.CreditsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 172, true);
			this.CreditsCalcEdit.Name = "CreditsCalcEdit";
			this.CreditsCalcEdit.ReadOnly = true;
			this.CreditsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.CreditsCalcEdit.TabIndex = 6;
			this.CreditsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CreditsCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCalcEdit, "Total");
			this.TotalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7b84cc87-c9b0-4f59-a662-9a923cdb453b", "Total");
			this.TotalCalcEdit.DecimalPlaces = 2;
			this.TotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 198, true);
			this.TotalCalcEdit.Name = "TotalCalcEdit";
			this.TotalCalcEdit.ReadOnly = true;
			this.TotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.TotalCalcEdit.TabIndex = 7;
			this.TotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCalcEdit.TrackDisposedAccess = true;
			// 
			// ChargeBreakdownGroupBox
			// 
			this.ChargeBreakdownGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5b8a73f2-0674-4a49-87b6-f90d6e2d675f", "Charge Breakdown");
			this.ChargeBreakdownGroupBox.Controls.Add(this.DutiesCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.ExciseCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.ExciseDutiesCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.SIMACalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.GSTCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.HSTCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.PSTCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.InterestCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.PenaltiesCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.PaymentsCalcEdit);
			this.ChargeBreakdownGroupBox.Controls.Add(this.OthersCalcEdit);
			this.ChargeBreakdownGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 4, true);
			this.ChargeBreakdownGroupBox.Name = "ChargeBreakdownGroupBox";
			this.ChargeBreakdownGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 328, true);
			this.ChargeBreakdownGroupBox.TabIndex = 0;
			this.ChargeBreakdownGroupBox.TabStop = false;
			// 
			// DutiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutiesCalcEdit, "Duties");
			this.DutiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ba8be798-d2ed-4637-8dcf-f12cf9458110", "Duties");
			this.DutiesCalcEdit.DecimalPlaces = 2;
			this.DutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.DutiesCalcEdit.Name = "DutiesCalcEdit";
			this.DutiesCalcEdit.ReadOnly = true;
			this.DutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.DutiesCalcEdit.TabIndex = 8;
			this.DutiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutiesCalcEdit.TrackDisposedAccess = true;
			// 
			// ExciseCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExciseCalcEdit, "Excise");
			this.ExciseCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7bbc9d96-a5dd-4649-87e6-84f3a8d43818", "Excise");
			this.ExciseCalcEdit.DecimalPlaces = 2;
			this.ExciseCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 42, true);
			this.ExciseCalcEdit.Name = "ExciseCalcEdit";
			this.ExciseCalcEdit.ReadOnly = true;
			this.ExciseCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.ExciseCalcEdit.TabIndex = 9;
			this.ExciseCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExciseCalcEdit.TrackDisposedAccess = true;
			// 
			// ExciseDutiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExciseDutiesCalcEdit, "ExciseDuties");
			this.ExciseDutiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9401593f-f8e2-4ed4-a2ff-f853a2bd7cc9", "Excise Duties");
			this.ExciseDutiesCalcEdit.DecimalPlaces = 2;
			this.ExciseDutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 68, true);
			this.ExciseDutiesCalcEdit.Name = "ExciseDutiesCalcEdit";
			this.ExciseDutiesCalcEdit.ReadOnly = true;
			this.ExciseDutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.ExciseDutiesCalcEdit.TabIndex = 10;
			this.ExciseDutiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExciseDutiesCalcEdit.TrackDisposedAccess = true;
			// 
			// SIMACalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SIMACalcEdit, "SIMA");
			this.SIMACalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2f8199ee-167f-4c14-a96a-ca6305cf86b4", "SIMA");
			this.SIMACalcEdit.DecimalPlaces = 2;
			this.SIMACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 94, true);
			this.SIMACalcEdit.Name = "SIMACalcEdit";
			this.SIMACalcEdit.ReadOnly = true;
			this.SIMACalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.SIMACalcEdit.TabIndex = 11;
			this.SIMACalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SIMACalcEdit.TrackDisposedAccess = true;
			// 
			// GSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCalcEdit, "GST");
			this.GSTCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a4743430-407c-4bfa-80c2-9aeec0d69bd8", "GST");
			this.GSTCalcEdit.DecimalPlaces = 2;
			this.GSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 120, true);
			this.GSTCalcEdit.Name = "GSTCalcEdit";
			this.GSTCalcEdit.ReadOnly = true;
			this.GSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.GSTCalcEdit.TabIndex = 12;
			this.GSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GSTCalcEdit.TrackDisposedAccess = true;
			// 
			// HSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HSTCalcEdit, "HST");
			this.HSTCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cdea4617-cdbd-461f-9e7c-c4302be30cf1", "HST");
			this.HSTCalcEdit.DecimalPlaces = 2;
			this.HSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 146, true);
			this.HSTCalcEdit.Name = "HSTCalcEdit";
			this.HSTCalcEdit.ReadOnly = true;
			this.HSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.HSTCalcEdit.TabIndex = 13;
			this.HSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.HSTCalcEdit.TrackDisposedAccess = true;
			// 
			// PSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PSTCalcEdit, "PST");
			this.PSTCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("41f85f94-bb0f-41f7-864b-70b705a71fa4", "PST");
			this.PSTCalcEdit.DecimalPlaces = 2;
			this.PSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 172, true);
			this.PSTCalcEdit.Name = "PSTCalcEdit";
			this.PSTCalcEdit.ReadOnly = true;
			this.PSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.PSTCalcEdit.TabIndex = 14;
			this.PSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PSTCalcEdit.TrackDisposedAccess = true;
			// 
			// InterestCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InterestCalcEdit, "Interest");
			this.InterestCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("701d4cd5-30d9-444e-b7f0-94a121074952", "Interest");
			this.InterestCalcEdit.DecimalPlaces = 2;
			this.InterestCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 198, true);
			this.InterestCalcEdit.Name = "InterestCalcEdit";
			this.InterestCalcEdit.ReadOnly = true;
			this.InterestCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.InterestCalcEdit.TabIndex = 15;
			this.InterestCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InterestCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltiesCalcEdit, "Penalties");
			this.PenaltiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4e1e7661-5b05-416a-9e8c-b9237b3d30e3", "Penalties");
			this.PenaltiesCalcEdit.DecimalPlaces = 2;
			this.PenaltiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 224, true);
			this.PenaltiesCalcEdit.Name = "PenaltiesCalcEdit";
			this.PenaltiesCalcEdit.ReadOnly = true;
			this.PenaltiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.PenaltiesCalcEdit.TabIndex = 16;
			this.PenaltiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltiesCalcEdit.TrackDisposedAccess = true;
			// 
			// PaymentsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentsCalcEdit, "Payments");
			this.PaymentsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("88c04e1b-62f8-4255-b1bd-6e03dbe27fca", "Payments");
			this.PaymentsCalcEdit.DecimalPlaces = 2;
			this.PaymentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 250, true);
			this.PaymentsCalcEdit.Name = "PaymentsCalcEdit";
			this.PaymentsCalcEdit.ReadOnly = true;
			this.PaymentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.PaymentsCalcEdit.TabIndex = 17;
			this.PaymentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaymentsCalcEdit.TrackDisposedAccess = true;
			// 
			// OthersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OthersCalcEdit, "Others");
			this.OthersCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("748f3552-56bf-4d50-a85a-8155f13f932c", "Others");
			this.OthersCalcEdit.DecimalPlaces = 2;
			this.OthersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 276, true);
			this.OthersCalcEdit.Name = "OthersCalcEdit";
			this.OthersCalcEdit.ReadOnly = true;
			this.OthersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.OthersCalcEdit.TabIndex = 18;
			this.OthersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.OthersCalcEdit.TrackDisposedAccess = true;
			// 
			// ByDayTabPage
			// 
			this.ByDayTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9216CD0B-7535-402B-BB8A-D271513F27DD", "Summary - By Day");
			this.ByDayTabPage.Controls.Add(this.LinesGrid);
			this.ByDayTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ByDayTabPage.Name = "ByDayTabPage";
			this.ByDayTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 314, true);
			this.ByDayTabPage.TabIndex = 0;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "StatementLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_ImporterCustomsID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_ScheduledProcessDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B2_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_Duties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_ExciseTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_ExciseDuties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_SIMA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_GST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_HST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_PST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_Interests)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_Penalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_Payments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B4_CARMDNChargeAmount_Others)));
			this.LinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a320395f-cef3-4972-8113-74db97c358d6", "Account");
			zTextBoxColumnStyleInfo1.ColumnName = "B3_ImporterCustomsID";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a3c13445-e084-4468-9788-71b524163bba", "Release Date");
			zDateEditColumnStyleInfo1.ColumnName = "B3_DueDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("efcc592b-a117-401a-922a-746432879f1f", "Accounting Date");
			zDateEditColumnStyleInfo2.ColumnName = "B3_ScheduledProcessDate";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1afdb256-7288-49d0-904f-ddd793c4b7b4", "Payment Due Date");
			zDateEditColumnStyleInfo3.ColumnName = "B2_DueDate";
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2867417-a8b8-4fe7-ac9c-f40a081e7b4b", "Duties");
			zCalcEditColumnStyleInfo1.ColumnName = "B4_CARMDNChargeAmount_Duties";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("598a7927-75f4-468e-8bf4-eee48eb6cce5", "Excise");
			zCalcEditColumnStyleInfo2.ColumnName = "B4_CARMDNChargeAmount_ExciseTax";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f3e02d0b-3b60-44e7-9d73-231497b7ba74", "Excise Duties");
			zCalcEditColumnStyleInfo3.ColumnName = "B4_CARMDNChargeAmount_ExciseDuties";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81de99cd-d74c-4cfe-a8a1-bd41b8242c03", "SIMA");
			zCalcEditColumnStyleInfo4.ColumnName = "B4_CARMDNChargeAmount_SIMA";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cbc987b4-cc13-4614-8510-36a1cdffe7a9", "GST");
			zCalcEditColumnStyleInfo5.ColumnName = "B4_CARMDNChargeAmount_GST";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("83060001-8a86-4f95-9e51-666f5da46e1e", "HST");
			zCalcEditColumnStyleInfo6.ColumnName = "B4_CARMDNChargeAmount_HST";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a040b836-5914-45c8-825d-396c92e82f35", "PST");
			zCalcEditColumnStyleInfo7.ColumnName = "B4_CARMDNChargeAmount_PST";
			zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0904d16f-edc7-4db3-8c40-b0f8d1f7c1a3", "Interest");
			zCalcEditColumnStyleInfo8.ColumnName = "B4_CARMDNChargeAmount_Interests";
			zCalcEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("79ac5087-001d-4ee7-aac6-4ddc63f1b65b", "Penalties");
			zCalcEditColumnStyleInfo9.ColumnName = "B4_CARMDNChargeAmount_Penalties";
			zCalcEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("522e1f9f-4a36-4eca-904d-76925b1440e9", "Payments");
			zCalcEditColumnStyleInfo10.ColumnName = "B4_CARMDNChargeAmount_Payments";
			zCalcEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("517d0b0e-3101-4987-91c7-a96e6b57bb87", "Others");
			zCalcEditColumnStyleInfo11.ColumnName = "B4_CARMDNChargeAmount_Others";
			zCalcEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "686bf4ad-cc4b-49cf-bd16-c2e900f08a22";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 314, true);
			this.LinesGrid.TabIndex = 0;
			// 
			// CARMSOAStatementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ed2a1b21-ce37-4306-8c51-351db4ce5fc6", "Daily Statement Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 653, true);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusStatementHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1044, 692, true);
			this.Name = "CARMSOAStatementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatementHeaderGroupBox.ResumeLayout(false);
			this.StatementHeaderGroupBox.PerformLayout();
			this.StatementHeaderDetailsUserControl.ResumeLayout(true);
			this.StatementHeaderDetailsUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.StatementMessagesUserControl.ResumeLayout(true);
			this.StatementMessagesUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.TransactionsTabControl.ResumeLayout(false);
			this.TransactionsTabControl.PerformLayout();
			this.BillingPeriodTabPage.ResumeLayout(false);
			this.BillingPeriodTabPage.PerformLayout();
			this.ChargeGroupBox.ResumeLayout(false);
			this.ChargeGroupBox.PerformLayout();
			this.ChargeBreakdownGroupBox.ResumeLayout(false);
			this.ChargeBreakdownGroupBox.PerformLayout();
			this.ByDayTabPage.ResumeLayout(false);
			this.ByDayTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox StatementHeaderGroupBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private StatementMessagesUserControl StatementMessagesUserControl;
		private ZWorkflowTabPage WorkflowTabPage;
		private CARMSOAStatementHeaderDetailsUserControl StatementHeaderDetailsUserControl;
		private ZTabControl TransactionsTabControl;
		private ZTabPage BillingPeriodTabPage;
		private ZTabPage ByDayTabPage;
		private Enterprise.ZArchitecture.ZGrid LinesGrid;

		private Enterprise.ZArchitecture.GUI.ZGroupBox ChargeGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit PreviousStatementBalanceCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit CorrectionsLastBalanceCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PaymentsAfterLastSOACalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DisbursementsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit InterestSumCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DebitsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit CreditsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalCalcEdit;

		private Enterprise.ZArchitecture.GUI.ZGroupBox ChargeBreakdownGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit DutiesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ExciseCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ExciseDutiesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit SIMACalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit GSTCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit HSTCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PSTCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit InterestCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PenaltiesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PaymentsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OthersCalcEdit;
	}
}
