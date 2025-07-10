namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	partial class AmbiguousCommissionResolverForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnresolvedCommissionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ResolveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FormCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PossibleOverallItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnresolvedCommissionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PossibleAgreementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PossibleAgreementsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.IsResolvingUnresolvedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IsResolvingFilteredRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.InvoiceNumberToResolveTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResolveTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoicePkToResolveGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CompanyPkGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AmbiguousCommissionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnresolvedCommissionsGrid)).BeginInit();
			this.UnresolvedCommissionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PossibleOverallItemsGrid)).BeginInit();
			this.PossibleOverallItemsGrid.SuspendLayout();
			this.UnresolvedCommissionsGroupBox.SuspendLayout();
			this.PossibleAgreementsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).BeginInit();
			this.BottomSplitContainer.Panel1.SuspendLayout();
			this.BottomSplitContainer.Panel2.SuspendLayout();
			this.BottomSplitContainer.SuspendLayout();
			this.ResolveTypeGroupBox.SuspendLayout();
			this.InvoicePkToResolveGuidFindBox.SuspendLayout();
			this.CompanyPkGuidFindBox.SuspendLayout();
			this.AmbiguousCommissionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 541, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver);
			// 
			// UnresolvedCommissionsGrid
			// 
			this.UnresolvedCommissionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnresolvedCommissionsGrid, "ResolveItemCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).SelectedAgreementPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).AmbiguousCommission.AC0_CommissionStream)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.Header.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).Invoice.AH_Desc)));
			this.UnresolvedCommissionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SelectedAgreementPk";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ebb7b2df-4571-4957-a3c3-c8d6dcdfe36c", "Stream", "Commission Stream", "");
			zTextBoxColumnStyleInfo1.ColumnName = "AmbiguousCommission+AC0_CommissionStream";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Invoice+AH_GC";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("fd4f2d60-19ac-4920-aed0-3bd327f84661", "Creditor/Debtor");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Invoice+AH_OH";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Invoice+Header+OH_FullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "Invoice+AH_TransactionType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo4.ColumnName = "Invoice+AH_Ledger";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo5.ColumnName = "Invoice+AH_TransactionNum";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Invoice+AH_Desc";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UnresolvedCommissionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UnresolvedCommissionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnresolvedCommissionsGrid.GridId = "bcb00b20-fa62-4cd3-bdc8-81437ddaa396";
			this.UnresolvedCommissionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnresolvedCommissionsGrid.LayoutKey = "UnresolvedCommissionsGrid";
			this.UnresolvedCommissionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.UnresolvedCommissionsGrid.Name = "UnresolvedCommissionsGrid";
			this.UnresolvedCommissionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 315, true);
			this.UnresolvedCommissionsGrid.TabIndex = 0;
			this.UnresolvedCommissionsGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UnresolvedCommissionsGrid_MouseDown);
			// 
			// ResolveButton
			// 
			this.ResolveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResolveButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7875a4ae-f35f-4c90-841f-5495ca0fc218", "Resolve");
			this.ResolveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(748, 391, true);
			this.ResolveButton.Name = "ResolveButton";
			this.ResolveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.ResolveButton.TabIndex = 2;
			this.ResolveButton.Click += new System.EventHandler(this.ResolveButton_Click);
			// 
			// FormCancelButton
			// 
			this.FormCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormCancelButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8b7f8890-714f-4d08-b632-6694e21267ea", "Cancel");
			this.FormCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.FormCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(811, 391, true);
			this.FormCancelButton.Name = "FormCancelButton";
			this.FormCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.FormCancelButton.TabIndex = 3;
			this.FormCancelButton.Click += new System.EventHandler(this.FormCancelButton_Click);
			// 
			// PossibleOverallItemsGrid
			// 
			this.PossibleOverallItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PossibleOverallItemsGrid, "ResolveItemCollection.MatchingOverallItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).CommissionAgreement.Opportunity.P8_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).CommissionAgreement.CA0_OH_Customer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).CommissionAgreement.Opportunity.P8_GS_NKPrimarySalesPerson)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).CommissionAgreement.AgreementId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).CommissionAgreement.EffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).VCI_ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).VCI_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewCommissionAgreementOverallItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolveItem)(((System.Collections.IList)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).ResolveItemCollection)).SyncRoot)).MatchingOverallItems)).SyncRoot)).VCI_SubModuleCode)));
			this.PossibleOverallItemsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "CommissionAgreement+Opportunity+P8_OH";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "CommissionAgreement+CA0_OH_Customer";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "CommissionAgreement+Opportunity+P8_GS_NKPrimarySalesPerson";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "CommissionAgreement+AgreementId";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo1.ColumnName = "CommissionAgreement+EffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "VCI_ProductCode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo10.ColumnName = "VCI_ServiceCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo11.ColumnName = "VCI_SubModuleCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PossibleOverallItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.PossibleOverallItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PossibleOverallItemsGrid.GridId = "b6a622e8-fbc1-42fa-a769-002b83c5f8fa";
			this.PossibleOverallItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PossibleOverallItemsGrid.LayoutKey = "MatchingAgreementsGrid";
			this.PossibleOverallItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 38, true);
			this.PossibleOverallItemsGrid.Name = "PossibleOverallItemsGrid";
			this.PossibleOverallItemsGrid.ReadOnly = true;
			this.PossibleOverallItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 291, true);
			this.PossibleOverallItemsGrid.TabIndex = 1;
			this.PossibleOverallItemsGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PossibleOverallItemsGrid_MouseDown);
			// 
			// UnresolvedCommissionsGroupBox
			// 
			this.UnresolvedCommissionsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a6c2c058-0100-4b7b-9176-4994ade83785", "Invoices");
			this.UnresolvedCommissionsGroupBox.Controls.Add(this.UnresolvedCommissionsGrid);
			this.UnresolvedCommissionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnresolvedCommissionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnresolvedCommissionsGroupBox.Name = "UnresolvedCommissionsGroupBox";
			this.UnresolvedCommissionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 330, true);
			this.UnresolvedCommissionsGroupBox.TabIndex = 0;
			this.UnresolvedCommissionsGroupBox.TabStop = false;
			// 
			// PossibleAgreementsGroupBox
			// 
			this.PossibleAgreementsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3e227811-1bfd-4905-89f6-4f971a6bcf7e", "Possible Agreements Matched");
			this.PossibleAgreementsGroupBox.Controls.Add(this.PossibleOverallItemsGrid);
			this.PossibleAgreementsGroupBox.Controls.Add(this.PossibleAgreementsLabel);
			this.PossibleAgreementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PossibleAgreementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PossibleAgreementsGroupBox.Name = "PossibleAgreementsGroupBox";
			this.PossibleAgreementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 330, true);
			this.PossibleAgreementsGroupBox.TabIndex = 0;
			this.PossibleAgreementsGroupBox.TabStop = false;
			// 
			// PossibleAgreementsLabel
			// 
			this.PossibleAgreementsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PossibleAgreementsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.PossibleAgreementsLabel.Name = "PossibleAgreementsLabel";
			this.PossibleAgreementsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 24, true);
			this.PossibleAgreementsLabel.TabIndex = 0;
			this.PossibleAgreementsLabel.Text = "Double-click an agreement to assign it to the invoice.";
			// 
			// BottomSplitContainer
			// 
			this.BottomSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 58, true);
			this.BottomSplitContainer.Name = "BottomSplitContainer";
			// 
			// BottomSplitContainer.Panel1
			// 
			this.BottomSplitContainer.Panel1.Controls.Add(this.UnresolvedCommissionsGroupBox);
			// 
			// BottomSplitContainer.Panel2
			// 
			this.BottomSplitContainer.Panel2.Controls.Add(this.PossibleAgreementsGroupBox);
			this.BottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 330, true);
			this.BottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(403);
			this.BottomSplitContainer.TabIndex = 1;
			// 
			// IsResolvingUnresolvedRadioButton
			// 
			this.IsResolvingUnresolvedRadioButton.AutoCheck = false;
			this.IsResolvingUnresolvedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsResolvingUnresolvedRadioButton, "FilterBizObj.IsResolvingUnresolved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).FilterBizObj.IsResolvingUnresolved)));
			this.IsResolvingUnresolvedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsResolvingUnresolvedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 17, true);
			this.IsResolvingUnresolvedRadioButton.Name = "IsResolvingUnresolvedRadioButton";
			this.IsResolvingUnresolvedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 16, true);
			this.IsResolvingUnresolvedRadioButton.TabIndex = 0;
			this.IsResolvingUnresolvedRadioButton.TabStop = true;
			this.IsResolvingUnresolvedRadioButton.UseVisualStyleBackColor = true;
			// 
			// IsResolvingFilteredRadioButton
			// 
			this.IsResolvingFilteredRadioButton.AutoCheck = false;
			this.IsResolvingFilteredRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsResolvingFilteredRadioButton, "FilterBizObj.IsResolvingFiltered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).FilterBizObj.IsResolvingFiltered)));
			this.IsResolvingFilteredRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsResolvingFilteredRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 47, true);
			this.IsResolvingFilteredRadioButton.Name = "IsResolvingFilteredRadioButton";
			this.IsResolvingFilteredRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 16, true);
			this.IsResolvingFilteredRadioButton.TabIndex = 1;
			this.IsResolvingFilteredRadioButton.TabStop = true;
			this.IsResolvingFilteredRadioButton.UseVisualStyleBackColor = true;
			// 
			// InvoiceNumberToResolveTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberToResolveTextBox, "FilterBizObj.InvoiceNumberToResolve");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).FilterBizObj.InvoiceNumberToResolve)));
			this.InvoiceNumberToResolveTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 70, true);
			this.InvoiceNumberToResolveTextBox.Name = "InvoiceNumberToResolveTextBox";
			this.InvoiceNumberToResolveTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.InvoiceNumberToResolveTextBox.TabIndex = 3;
			// 
			// ResolveTypeGroupBox
			// 
			this.ResolveTypeGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c942c4e4-a72f-4577-a5a8-e5e51fd09ea1", "Resolve Kind");
			this.ResolveTypeGroupBox.Controls.Add(this.InvoicePkToResolveGuidFindBox);
			this.ResolveTypeGroupBox.Controls.Add(this.CompanyPkGuidFindBox);
			this.ResolveTypeGroupBox.Controls.Add(this.FindButton);
			this.ResolveTypeGroupBox.Controls.Add(this.IsResolvingUnresolvedRadioButton);
			this.ResolveTypeGroupBox.Controls.Add(this.InvoiceNumberToResolveTextBox);
			this.ResolveTypeGroupBox.Controls.Add(this.IsResolvingFilteredRadioButton);
			this.ResolveTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.ResolveTypeGroupBox.Name = "ResolveTypeGroupBox";
			this.ResolveTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 112, true);
			this.ResolveTypeGroupBox.TabIndex = 0;
			this.ResolveTypeGroupBox.TabStop = false;
			// 
			// InvoicePkToResolveGuidFindBox
			// 
			this.InvoicePkToResolveGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoicePkToResolveGuidFindBox, "FilterBizObj.InvoicePkToResolve");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).FilterBizObj.InvoicePkToResolve)));
			this.InvoicePkToResolveGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 70, true);
			this.InvoicePkToResolveGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ARTransaction;
			this.InvoicePkToResolveGuidFindBox.Name = "InvoicePkToResolveGuidFindBox";
			this.InvoicePkToResolveGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.InvoicePkToResolveGuidFindBox.TabIndex = 3;
			// 
			// CompanyPkGuidFindBox
			// 
			this.CompanyPkGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyPkGuidFindBox, "FilterBizObj.CompanyPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver)(null)).FilterBizObj.CompanyPk)));
			this.CompanyPkGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 70, true);
			this.CompanyPkGuidFindBox.Name = "CompanyPkGuidFindBox";
			this.CompanyPkGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 17, true);
			this.CompanyPkGuidFindBox.TabIndex = 2;
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6f98e9ed-e906-4465-b6bb-b2cc033c44b5", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 86, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.FindButton.TabIndex = 4;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f2eca93e-ee57-4430-8976-d7a821210420", "Assign commission agreements to invoices with ambiguous commissions by entering the agreement ID or double-clicking on one of the matching agreements. Previously resolved commissions can be reassigned by removing or updating the selected agreement.");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.InstructionsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 36, true);
			this.InstructionsLabel.TabIndex = 0;
			this.InstructionsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AmbiguousCommissionsGroupBox
			// 
			this.AmbiguousCommissionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AmbiguousCommissionsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2cc4ebe8-ddd2-41f3-b4e3-faa0b881d6e5", "Ambiguous Commissions");
			this.AmbiguousCommissionsGroupBox.Controls.Add(this.InstructionsLabel);
			this.AmbiguousCommissionsGroupBox.Controls.Add(this.BottomSplitContainer);
			this.AmbiguousCommissionsGroupBox.Controls.Add(this.ResolveButton);
			this.AmbiguousCommissionsGroupBox.Controls.Add(this.FormCancelButton);
			this.AmbiguousCommissionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 121, true);
			this.AmbiguousCommissionsGroupBox.Name = "AmbiguousCommissionsGroupBox";
			this.AmbiguousCommissionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 417, true);
			this.AmbiguousCommissionsGroupBox.TabIndex = 1;
			this.AmbiguousCommissionsGroupBox.TabStop = false;
			// 
			// AmbiguousCommissionResolverForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.FormCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("913ef9c8-b82c-4aed-abe6-9f599ec77387", "Commission Ambiguity Resolver");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 565, true);
			this.Controls.Add(this.AmbiguousCommissionsGroupBox);
			this.Controls.Add(this.ResolveTypeGroupBox);
			this.DataSourceType = typeof(Enterprise.Client.EDI.CommissionManagement.Business.AmbiguousCommissionResolver);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 600, true);
			this.Name = "AmbiguousCommissionResolverForm";
			this.Controls.SetChildIndex(this.ResolveTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AmbiguousCommissionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UnresolvedCommissionsGrid)).EndInit();
			this.UnresolvedCommissionsGrid.ResumeLayout(false);
			this.UnresolvedCommissionsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PossibleOverallItemsGrid)).EndInit();
			this.PossibleOverallItemsGrid.ResumeLayout(false);
			this.PossibleOverallItemsGrid.PerformLayout();
			this.UnresolvedCommissionsGroupBox.ResumeLayout(false);
			this.UnresolvedCommissionsGroupBox.PerformLayout();
			this.PossibleAgreementsGroupBox.ResumeLayout(false);
			this.PossibleAgreementsGroupBox.PerformLayout();
			this.BottomSplitContainer.Panel1.ResumeLayout(false);
			this.BottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).EndInit();
			this.BottomSplitContainer.ResumeLayout(false);
			this.BottomSplitContainer.PerformLayout();
			this.ResolveTypeGroupBox.ResumeLayout(false);
			this.ResolveTypeGroupBox.PerformLayout();
			this.InvoicePkToResolveGuidFindBox.ResumeLayout(true);
			this.InvoicePkToResolveGuidFindBox.PerformLayout();
			this.CompanyPkGuidFindBox.ResumeLayout(true);
			this.CompanyPkGuidFindBox.PerformLayout();
			this.AmbiguousCommissionsGroupBox.ResumeLayout(false);
			this.AmbiguousCommissionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid UnresolvedCommissionsGrid;
		protected ZArchitecture.GUI.ZButton ResolveButton;
		private ZArchitecture.GUI.ZButton FormCancelButton;
		protected ZArchitecture.ZGrid PossibleOverallItemsGrid;
		private ZArchitecture.GUI.ZGroupBox UnresolvedCommissionsGroupBox;
		private ZArchitecture.GUI.ZGroupBox PossibleAgreementsGroupBox;
		private ZArchitecture.ZLabel PossibleAgreementsLabel;
		private CargoWise.Windows.UI.KSplitContainer BottomSplitContainer;
		private ZArchitecture.GUI.ZRadioButton IsResolvingUnresolvedRadioButton;
		private ZArchitecture.GUI.ZRadioButton IsResolvingFilteredRadioButton;
		protected ZArchitecture.ZTextBox InvoiceNumberToResolveTextBox;
		private ZArchitecture.GUI.ZGroupBox ResolveTypeGroupBox;
		protected ZArchitecture.GUI.ZButton FindButton;
		protected ZArchitecture.GUI.ZGuidFindBox InvoicePkToResolveGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CompanyPkGuidFindBox;
		private ZArchitecture.ZLabel InstructionsLabel;
		private ZArchitecture.GUI.ZGroupBox AmbiguousCommissionsGroupBox;
	}
}