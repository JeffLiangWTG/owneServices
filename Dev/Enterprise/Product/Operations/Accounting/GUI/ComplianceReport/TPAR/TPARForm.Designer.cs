namespace Enterprise.Accounting.GUI.ComplianceReport.TPAR
{
	partial class TPARForm : ZArchitecture.GUI.ZChildForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.generateFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.markAsSubmittedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.CreditorLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CreditorLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.periodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dateToZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateFromZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.regNoText = new Enterprise.ZArchitecture.ZLabel();
			this.abnRegNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyState = new Enterprise.ZArchitecture.ZLabel();
			this.companyPostCode = new Enterprise.ZArchitecture.ZLabel();
			this.companyCity = new Enterprise.ZArchitecture.ZLabel();
			this.companyCountry = new Enterprise.ZArchitecture.ZLabel();
			this.companyAddressLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.companyAddressLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.companyNameLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.CreditorLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CreditorLinesGrid)).BeginInit();
			this.CreditorLinesGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.statusDropEdit.SuspendLayout();
			this.dateToZDateEdit.SuspendLayout();
			this.dateFromZDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 546, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport);
			// 
			// generateFileButton
			// 
			this.generateFileButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("33873d43-cd49-4ad1-b7f4-e99de6b0396f", "Generate TPAR File");
			this.generateFileButton.IsCaptionOverridden = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.generateFileButton, false);
			this.generateFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 9, true);
			this.generateFileButton.Name = "generateFileButton";
			this.generateFileButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.generateFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 22, true);
			this.generateFileButton.TabIndex = 92;
			this.generateFileButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.generateFileButton.ToolTipCaption = null;
			this.generateFileButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomPanel.Controls.Add(this.markAsSubmittedButton);
			this.BottomPanel.Controls.Add(this.PostingButtons);
			this.BottomPanel.Controls.Add(this.generateFileButton);
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 505, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 37, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// markAsSubmittedButton
			// 
			this.markAsSubmittedButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("12d2fafd-1811-43b4-8e0d-3cecede66f7f", "Mark as Submitted");
			this.markAsSubmittedButton.IsCaptionOverridden = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.markAsSubmittedButton, false);
			this.markAsSubmittedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 9, true);
			this.markAsSubmittedButton.Name = "markAsSubmittedButton";
			this.markAsSubmittedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.markAsSubmittedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 22, true);
			this.markAsSubmittedButton.TabIndex = 93;
			this.markAsSubmittedButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.markAsSubmittedButton.ToolTipCaption = null;
			this.markAsSubmittedButton.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(689, 8, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.PostingButtons.TabIndex = 5;
			this.PostingButtons.TabStop = true;
			// 
			// CreditorLinesGroupBox
			// 
			this.CreditorLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CreditorLinesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("97a31647-74eb-4c74-972f-a8cb895812dc", "Creditor Lines");
			this.CreditorLinesGroupBox.Controls.Add(this.CreditorLinesGrid);
			this.CreditorLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.CreditorLinesGroupBox.Name = "CreditorLinesGroupBox";
			this.CreditorLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(937, 359, true);
			this.CreditorLinesGroupBox.TabIndex = 16;
			this.CreditorLinesGroupBox.TabStop = false;
			// 
			// CreditorLinesGrid
			// 
			this.CreditorLinesGrid.AllowNavigation = false;
			this.CreditorLinesGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.CreditorLinesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OH_Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OrgName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OrgRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_TotalAmountIncludingTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OverriddenTotalAmountIncludingTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OverriddenGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_PaymentsBasisWithholdingTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_OverriddenPaymentsBasisWithholdingTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).ARL_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).PhoneNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReportCreditorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Lines)).SyncRoot)).Email)));
			this.CreditorLinesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ARL_OH_Organisation";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ARL_OrgName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zTextBoxColumnStyleInfo2.ColumnName = "ARL_OrgRegNo";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "ARL_Address1";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "ARL_Address2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "ARL_City";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "ARL_State";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ARL_PostCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "ARL_RN_NKCountryCode";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ARL_TotalAmountIncludingTax";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ARL_OverriddenTotalAmountIncludingTax";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ARL_GSTAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ARL_OverriddenGSTAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "ARL_PaymentsBasisWithholdingTaxAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "ARL_OverriddenPaymentsBasisWithholdingTaxAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "ARL_Comment";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.ColumnName = "ARL_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "ARL_SystemLastEditUser";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "PhoneNumber";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "Email";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CreditorLinesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CreditorLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CreditorLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CreditorLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CreditorLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditorLinesGrid.GridId = "C0C6CB79-71A0-4070-85E5-747CEA021A9E";
			this.CreditorLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditorLinesGrid.LayoutKey = "TaxRecordsGrid";
			this.CreditorLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CreditorLinesGrid.Name = "CreditorLinesGrid";
			this.CreditorLinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CreditorLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 342, true);
			this.CreditorLinesGrid.TabIndex = 2;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6b1cc881-03f7-4a01-a2ad-89d4f151b761", "Company and Report details");
			this.zGroupBox1.Controls.Add(this.descriptionTextBox);
			this.zGroupBox1.Controls.Add(this.periodCalcEdit);
			this.zGroupBox1.Controls.Add(this.statusDropEdit);
			this.zGroupBox1.Controls.Add(this.dateToZDateEdit);
			this.zGroupBox1.Controls.Add(this.dateFromZDateEdit);
			this.zGroupBox1.Controls.Add(this.regNoText);
			this.zGroupBox1.Controls.Add(this.abnRegNoLabel);
			this.zGroupBox1.Controls.Add(this.companyState);
			this.zGroupBox1.Controls.Add(this.companyPostCode);
			this.zGroupBox1.Controls.Add(this.companyCity);
			this.zGroupBox1.Controls.Add(this.companyCountry);
			this.zGroupBox1.Controls.Add(this.companyAddressLabel2);
			this.zGroupBox1.Controls.Add(this.companyAddressLabel1);
			this.zGroupBox1.Controls.Add(this.companyNameLabel);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 136, true);
			this.zGroupBox1.TabIndex = 17;
			this.zGroupBox1.TabStop = false;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "ATR_Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_Comment)));
			this.descriptionTextBox.CaptionResourceString = null;
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 111, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 17, true);
			this.descriptionTextBox.TabIndex = 31;
			// 
			// periodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.periodCalcEdit, "ATR_Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_Version)));
			this.periodCalcEdit.CaptionResourceString = null;
			this.periodCalcEdit.DecimalPlaces = 0;
			this.periodCalcEdit.Decimals = 0;
			this.periodCalcEdit.IsCalculatorEnabled = false;
			this.periodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 68, true);
			this.periodCalcEdit.Name = "periodCalcEdit";
			this.periodCalcEdit.ShowGroupSeparators = false;
			this.periodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.periodCalcEdit.TabIndex = 30;
			this.periodCalcEdit.Text = "0";
			this.periodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "ATR_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_Status)));
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 89, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.ShouldResizeByMaxLength = true;
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 17, true);
			this.statusDropEdit.TabIndex = 29;
			// 
			// dateToZDateEdit
			// 
			this.dateToZDateEdit.AllowDrop = true;
			this.dateToZDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateToZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateToZDateEdit, "ComplianceReport.ACR_DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ComplianceReport.ACR_DateTo)));
			this.dateToZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("323f6b50-1713-4321-a2f2-013beedf0329", "To");
			this.dateToZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(839, 47, true);
			this.dateToZDateEdit.Name = "dateToZDateEdit";
			this.dateToZDateEdit.TabIndex = 28;
			// 
			// dateFromZDateEdit
			// 
			this.dateFromZDateEdit.AllowDrop = true;
			this.dateFromZDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateFromZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateFromZDateEdit, "ComplianceReport.ACR_DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ComplianceReport.ACR_DateFrom)));
			this.dateFromZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("563234ee-19a1-4c37-a03a-3347741d0a56", "Date From");
			this.dateFromZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 47, true);
			this.dateFromZDateEdit.Name = "dateFromZDateEdit";
			this.dateFromZDateEdit.TabIndex = 26;
			// 
			// regNoText
			// 
			this.regNoText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.regNoText.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.regNoText, false);
			this.regNoText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 21, true);
			this.regNoText.Name = "regNoText";
			this.regNoText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 17, true);
			this.regNoText.TabIndex = 23;
			this.regNoText.Text = "Registration Number:";
			this.regNoText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// abnRegNoLabel
			// 
			this.BindingSource.SetBindingMember(this.abnRegNoLabel, "ATR_VATRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_VATRegNo)));
			this.abnRegNoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.abnRegNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 21, true);
			this.abnRegNoLabel.Name = "abnRegNoLabel";
			this.abnRegNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.abnRegNoLabel.TabIndex = 24;
			this.abnRegNoLabel.Text = "<ABN>";
			// 
			// companyState
			// 
			this.BindingSource.SetBindingMember(this.companyState, "ATR_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_State)));
			this.companyState.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyState, false);
			this.companyState.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 89, true);
			this.companyState.Name = "companyState";
			this.companyState.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.companyState.TabIndex = 22;
			this.companyState.Text = "<Company State>";
			// 
			// companyPostCode
			// 
			this.BindingSource.SetBindingMember(this.companyPostCode, "ATR_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_PostCode)));
			this.companyPostCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyPostCode, false);
			this.companyPostCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 111, true);
			this.companyPostCode.Name = "companyPostCode";
			this.companyPostCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.companyPostCode.TabIndex = 21;
			this.companyPostCode.Text = "0000";
			// 
			// companyCity
			// 
			this.BindingSource.SetBindingMember(this.companyCity, "ATR_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_City)));
			this.companyCity.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyCity, false);
			this.companyCity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 89, true);
			this.companyCity.Name = "companyCity";
			this.companyCity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.companyCity.TabIndex = 20;
			this.companyCity.Text = "<Company City>";
			// 
			// companyCountry
			// 
			this.BindingSource.SetBindingMember(this.companyCountry, "Country.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).Country.Description)));
			this.companyCountry.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyCountry, false);
			this.companyCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 111, true);
			this.companyCountry.Name = "companyCountry";
			this.companyCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.companyCountry.TabIndex = 19;
			this.companyCountry.Text = "<Country>";
			// 
			// companyAddressLabel2
			// 
			this.BindingSource.SetBindingMember(this.companyAddressLabel2, "ATR_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_Address2)));
			this.companyAddressLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyAddressLabel2, false);
			this.companyAddressLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 68, true);
			this.companyAddressLabel2.Name = "companyAddressLabel2";
			this.companyAddressLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.companyAddressLabel2.TabIndex = 18;
			this.companyAddressLabel2.Text = "<Company Address>";
			// 
			// companyAddressLabel1
			// 
			this.BindingSource.SetBindingMember(this.companyAddressLabel1, "ATR_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_Address1)));
			this.companyAddressLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyAddressLabel1, false);
			this.companyAddressLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 47, true);
			this.companyAddressLabel1.Name = "companyAddressLabel1";
			this.companyAddressLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.companyAddressLabel1.TabIndex = 17;
			this.companyAddressLabel1.Text = "<Company Address>";
			// 
			// companyNameLabel
			// 
			this.BindingSource.SetBindingMember(this.companyNameLabel, "ATR_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport)(null)).ATR_CompanyName)));
			this.companyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyNameLabel, false);
			this.companyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 21, true);
			this.companyNameLabel.Name = "companyNameLabel";
			this.companyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 17, true);
			this.companyNameLabel.TabIndex = 16;
			this.companyNameLabel.Text = "<Company Name>";
			// 
			// TPARForm
			// 
			this.AcceptButton = this.generateFileButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4fc851f9-e80e-40a2-be9d-86d7ddeb9f8d", "TPAR Summary by Creditors");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 568, true);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CreditorLinesGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.TPAR.TparReport";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 605, true);
			this.Name = "TPARForm";
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CreditorLinesGroupBox, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.CreditorLinesGroupBox.ResumeLayout(false);
			this.CreditorLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CreditorLinesGrid)).EndInit();
			this.CreditorLinesGrid.ResumeLayout(false);
			this.CreditorLinesGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.dateToZDateEdit.ResumeLayout(true);
			this.dateToZDateEdit.PerformLayout();
			this.dateFromZDateEdit.ResumeLayout(true);
			this.dateFromZDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZArchitecture.GUI.ZButton generateFileButton;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private ZArchitecture.GUI.ZGroupBox CreditorLinesGroupBox;
		private ZArchitecture.ZGrid CreditorLinesGrid;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZDateEdit dateToZDateEdit;
		private ZArchitecture.GUI.ZDateEdit dateFromZDateEdit;
		private ZArchitecture.ZLabel regNoText;
		private ZArchitecture.ZLabel abnRegNoLabel;
		private ZArchitecture.ZLabel companyState;
		private ZArchitecture.ZLabel companyPostCode;
		private ZArchitecture.ZLabel companyCity;
		private ZArchitecture.ZLabel companyAddressLabel2;
		private ZArchitecture.ZLabel companyAddressLabel1;
		private ZArchitecture.ZLabel companyNameLabel;
		private ZArchitecture.ZLabel companyCountry;
		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.ZCalcEdit periodCalcEdit;
		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.GUI.ZButton markAsSubmittedButton;
	}
}
