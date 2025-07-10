namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class InvoicingBaseBulkConsolCostImportForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ConsolsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConsolCostsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolCostsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectedTotalCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.CancelPostingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.ConsolsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolsGrid)).BeginInit();
			this.ConsolsGrid.SuspendLayout();
			this.ConsolCostsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostsGrid)).BeginInit();
			this.ConsolCostsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SelectedTotalCalcEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ConsolsGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ConsolCostsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 272, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(168);
			this.SplitContainer.TabIndex = 2;
			// 
			// ConsolsGroupBox
			// 
			this.ConsolsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|0288e30c-8e5a-428a-bf5e-b0ceb3d25f77", "Consols");
			this.ConsolsGroupBox.Controls.Add(this.ConsolsGrid);
			this.ConsolsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolsGroupBox.Name = "ConsolsGroupBox";
			this.ConsolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 168, true);
			this.ConsolsGroupBox.TabIndex = 0;
			this.ConsolsGroupBox.TabStop = false;
			// 
			// ConsolsGrid
			// 
			this.ConsolsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsolsGrid, "ConsolsFilteredByViewingPermission");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).IsSelectedForImport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_SecondaryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).VX_CoLoadMasterBill)));
			this.ConsolsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|beddfa7b-00e1-4d87-8fc1-7946a4875b95", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelectedForImport";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|84e32eeb-45dc-4c1b-9057-48c143122daf", "Consol ID", "Consol ID", "");
			zTextBoxColumnStyleInfo1.ColumnName = "VX_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|b2c0dea3-99df-4d17-b51f-3ee6c31c3e34", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "VX_ETD";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|da347776-9ad5-415f-b9de-b9a835ea1efe", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "VX_ETA";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "VX_RL_NKLoadPort";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "VX_RL_NKDischargePort";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|1b121eea-a0ca-4aa8-bcaf-b1783838ac3a", "Master Bill", "Master Bill", "");
			zTextBoxColumnStyleInfo4.ColumnName = "VX_SecondaryCode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "VX_CoLoadMasterBill";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConsolsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsolsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ConsolsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ConsolsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolsGrid.GridId = "bf024fd6-7f6b-4acb-bccf-da2a44ec57d9";
			this.ConsolsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolsGrid.LayoutKey = "ConsolsGrid";
			this.ConsolsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ConsolsGrid.Name = "ConsolsGrid";
			this.ConsolsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 152, true);
			this.ConsolsGrid.TabIndex = 0;
			// 
			// ConsolCostsGroupBox
			// 
			this.ConsolCostsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|4a6c2983-8322-46c2-9d5e-ac99ad08173b", "Consol Costs");
			this.ConsolCostsGroupBox.Controls.Add(this.ConsolCostsGrid);
			this.ConsolCostsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolCostsGroupBox.Name = "ConsolCostsGroupBox";
			this.ConsolCostsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 102, true);
			this.ConsolCostsGroupBox.TabIndex = 0;
			this.ConsolCostsGroupBox.TabStop = false;
			// 
			// ConsolCostsGrid
			// 
			this.ConsolCostsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsolCostsGrid, "ConsolsFilteredByViewingPermission.ConsolCostsFilteredByViewingPermission");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).IsSelectedForImport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_LocalCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_OSCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_CostReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_GS_NKConsolCostOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).E6_GB_CostTaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter)(null)).ConsolsFilteredByViewingPermission)).SyncRoot)).ConsolCostsFilteredByViewingPermission)).SyncRoot)).CostTaxBranchName)));
			this.ConsolCostsGrid.CaptionBackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.ConsolCostsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|e3ae5ee2-520b-4a9c-bafa-f9eaad9bca4e", "Include");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSelectedForImport";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "E6_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E6_LocalCostAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "E6_RX_NKCurrency";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "E6_OSCostAmount";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "E6_ExchangeRate";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceBulkConsolCostImportForm|df8ce85f-8897-4587-9a1e-e0a4a53f52b9", "Company");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "E6_GC";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "E6_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f66fbe3e-6554-4c44-9c07-ef633c083091", "Sup. Cost Ref.", "Supplier Cost Reference", "");
			zTextBoxColumnStyleInfo7.ColumnName = "E6_CostReference";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "E6_PlaceOfSupply";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceBulkConsolCostImportForm|BDA439CF-1E29-4624-9CC4-E2B569938C3D", "Cost Owner");
			zTextBoxColumnStyleInfo8.ColumnName = "E6_GS_NKConsolCostOwner";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceBulkConsolCostImportForm|0819C256-D216-4F99-B191-55159F16C6EC", "Supply Type");
			zDropEditColumnStyleInfo2.ColumnName = "E6_SupplyType";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceBulkConsolCostImportForm|59cba516-a24c-405a-ae7e-c6db0494bc02", "Tax Branch");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "E6_GB_CostTaxBranch";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceBulkConsolCostImportForm|119202c9-c3ea-456f-bfb9-675c862f7997", "Tax Branch Name");
			zTextBoxColumnStyleInfo9.ColumnName = "CostTaxBranchName";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ConsolCostsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ConsolCostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ConsolCostsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConsolCostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ConsolCostsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConsolCostsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ConsolCostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ConsolCostsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ConsolCostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ConsolCostsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConsolCostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ConsolCostsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ConsolCostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ConsolCostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ConsolCostsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostsGrid.GridId = "154c4fdb-db4c-4d49-85a9-f3e82c2c45c4";
			this.ConsolCostsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolCostsGrid.LayoutKey = "zGrid1";
			this.ConsolCostsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ConsolCostsGrid.Name = "ConsolCostsGrid";
			this.ConsolCostsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 86, true);
			this.ConsolCostsGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.DeselectAllButton);
			this.BottomPanel.Controls.Add(this.SelectAllButton);
			this.BottomPanel.Controls.Add(this.SelectedTotalCalcEdit);
			this.BottomPanel.Controls.Add(this.CancelPostingButton);
			this.BottomPanel.Controls.Add(this.PostButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 396, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 64, true);
			this.BottomPanel.TabIndex = 0;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|251e5184-86f3-404b-9074-e04df99bfd51", "&Deselect All");
			this.DeselectAllButton.IsCaptionOverridden = false;
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 6, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.DeselectAllButton.TabIndex = 1;
			this.DeselectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DeselectAllButton.ToolTipCaption = null;
			this.DeselectAllButton.UseVisualStyleBackColor = true;
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|600a1198-f8f9-44c2-9e0b-55fad2c81f80", "&Select All");
			this.SelectAllButton.IsCaptionOverridden = false;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// SelectedTotalCalcEdit
			// 
			this.SelectedTotalCalcEdit.AllowDrop = true;
			this.SelectedTotalCalcEdit.BindToAmount = "SelectedTotal";
			this.SelectedTotalCalcEdit.BindToDecimalPlaces = "Parent.ParentAPInvoice.AH_Calc_RXDecimals";
			this.SelectedTotalCalcEdit.BindToUnit = "Parent.ParentAPInvoice.AH_Readonly_RXCode";
			this.SelectedTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|5c42cb7f-9813-4a00-92e3-54cc9f84b5e4", "Selected Total");
			this.SelectedTotalCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.SelectedTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 38, true);
			this.SelectedTotalCalcEdit.Name = "SelectedTotalCalcEdit";
			this.SelectedTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.SelectedTotalCalcEdit.TabIndex = 2;
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|e87e1cab-b0d2-499d-8328-f40a33518d81", "&Cancel");
			this.CancelPostingButton.IsCaptionOverridden = false;
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 34, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.CancelPostingButton.TabIndex = 4;
			this.CancelPostingButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelPostingButton.ToolTipCaption = null;
			this.CancelPostingButton.UseVisualStyleBackColor = true;
			this.CancelPostingButton.Click += new System.EventHandler(this.CancelPostingButton_Click);
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|79b18408-ac70-40fd-966a-3ccf234efbc1", "&Import");
			this.PostButton.IsCaptionOverridden = false;
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 34, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.PostButton.TabIndex = 3;
			this.PostButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PostButton.ToolTipCaption = null;
			this.PostButton.UseVisualStyleBackColor = true;
			this.PostButton.Click += new System.EventHandler(this.PostButton_Click);
			// 
			// InvoicingBaseBulkConsolCostImportForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseBulkConsolCostImportForm|a377df88-18a4-45cd-8c7b-72f9ad160880", "Bulk Consol Cost Import");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 484, true);
			this.Controls.Add(this.SplitContainer);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 520, true);
			this.Name = "InvoicingBaseBulkConsolCostImportForm";
			this.Text = "APInvoiceBulkConsolCostImportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ConsolsGroupBox.ResumeLayout(false);
			this.ConsolsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolsGrid)).EndInit();
			this.ConsolsGrid.ResumeLayout(false);
			this.ConsolsGrid.PerformLayout();
			this.ConsolCostsGroupBox.ResumeLayout(false);
			this.ConsolCostsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostsGrid)).EndInit();
			this.ConsolCostsGrid.ResumeLayout(false);
			this.ConsolCostsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SelectedTotalCalcEdit.ResumeLayout(true);
			this.SelectedTotalCalcEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolCostsGroupBox;
		private Enterprise.ZArchitecture.ZGrid ConsolsGrid;
		private Enterprise.ZArchitecture.ZGrid ConsolCostsGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZButton CancelPostingButton;
		private Enterprise.ZArchitecture.GUI.ZButton PostButton;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox SelectedTotalCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZButton DeselectAllButton;
		private Enterprise.ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
