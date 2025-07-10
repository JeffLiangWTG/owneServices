namespace Enterprise.Customs.CA.GUI
{
	partial class B2LineAsAccountedForUserControl
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
			if (disposing)
			{
				if (CurrentDataItem != null)
				{
					CurrentDataItem.OnApportionmentDirtyChanged -= OnCurrentDataItem_OnApportionmentDirtyChanged;
				}

				var listManager = AsAccountForGrid.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged -= AsAccountListManager_CurrentChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.OriginalTransactionNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OriginalTransactionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SelectingLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SubHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubHeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SubHeaderSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SelectLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ApportionmentPendingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InvoiceLinesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.asAccountSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AsAccountForGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AsAccountForGrid = new Enterprise.ZArchitecture.ZGrid();
			this.asAccountTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AsAccountForDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClassificationTariffFindBox = new Enterprise.Customs.Common.GUI.TariffFindBox();
			this.JI_DescriptionBoundTextBox = new Enterprise.Customs.CA.GUI.JobComInvocieLineBoundLongTextControl();
			this.ValueForDutyCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SpecialAuthorityNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginalB3LineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.asAccountDutyTaxSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AsAccountForDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VauleForDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AsAccountSIMADumpingDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConversionValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Qty2CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Qty3CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AsAccountDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OriginalTransactionNumberPanel.SuspendLayout();
			this.SubHeadersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderGrid)).BeginInit();
			this.SubHeaderGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderSplitContainer)).BeginInit();
			this.SubHeaderSplitContainer.Panel1.SuspendLayout();
			this.SubHeaderSplitContainer.Panel2.SuspendLayout();
			this.SubHeaderSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectLinesSplitContainer)).BeginInit();
			this.SelectLinesSplitContainer.Panel1.SuspendLayout();
			this.SelectLinesSplitContainer.Panel2.SuspendLayout();
			this.SelectLinesSplitContainer.SuspendLayout();
			this.InvoiceLinesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.asAccountSplitContainer)).BeginInit();
			this.asAccountSplitContainer.Panel1.SuspendLayout();
			this.asAccountSplitContainer.Panel2.SuspendLayout();
			this.asAccountSplitContainer.SuspendLayout();
			this.AsAccountForGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AsAccountForGrid)).BeginInit();
			this.AsAccountForGrid.SuspendLayout();
			this.asAccountTableLayoutPanel.SuspendLayout();
			this.AsAccountForDetailsGroupBox.SuspendLayout();
			this.ClassificationTariffFindBox.SuspendLayout();
			this.JI_DescriptionBoundTextBox.SuspendLayout();
			this.ValueForDutyCodeDropEdit.SuspendLayout();
			this.TariffCodeCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.asAccountDutyTaxSplitContainer)).BeginInit();
			this.asAccountDutyTaxSplitContainer.Panel1.SuspendLayout();
			this.asAccountDutyTaxSplitContainer.Panel2.SuspendLayout();
			this.asAccountDutyTaxSplitContainer.SuspendLayout();
			this.AsAccountForDutyAndTaxGroupBox.SuspendLayout();
			this.CustomQuantityCalcDropEdit.SuspendLayout();
			this.Qty2CalcDropEdit.SuspendLayout();
			this.Qty3CalcDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AsAccountDutyAndTaxGrid)).BeginInit();
			this.AsAccountDutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// OriginalTransactionNumberPanel
			// 
			this.OriginalTransactionNumberPanel.Controls.Add(this.OriginalTransactionTextBox);
			this.OriginalTransactionNumberPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.OriginalTransactionNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginalTransactionNumberPanel.Name = "OriginalTransactionNumberPanel";
			this.OriginalTransactionNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 25, true);
			this.OriginalTransactionNumberPanel.TabIndex = 0;
			// 
			// OriginalTransactionTextBox
			// 
			this.OriginalTransactionTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BindingSource.SetBindingMember(this.OriginalTransactionTextBox, "CA_OriginalTransactionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_OriginalTransactionNo)));
			this.OriginalTransactionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("20836f11-1365-4745-bc94-c237ff742cd5", "Original Transaction No.");
			this.OriginalTransactionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 2, true);
			this.OriginalTransactionTextBox.Name = "OriginalTransactionTextBox";
			this.OriginalTransactionTextBox.ReadOnly = true;
			this.OriginalTransactionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalTransactionTextBox.TabIndex = 7;
			// 
			// SelectingLinesButton
			// 
			this.SelectingLinesButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d7fe4e07-1fb6-4729-ac90-ffc742ff46d7", "Select Lines");
			this.SelectingLinesButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.SelectingLinesButton.IsCaptionOverridden = false;
			this.SelectingLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 0, true);
			this.SelectingLinesButton.Name = "SelectingLinesButton";
			this.SelectingLinesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectingLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.SelectingLinesButton.TabIndex = 1;
			this.SelectingLinesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectingLinesButton.ToolTipCaption = null;
			this.SelectingLinesButton.UseVisualStyleBackColor = true;
			this.SelectingLinesButton.Click += new System.EventHandler(this.SelectingLinesButton_Click);
			// 
			// SubHeadersGroupBox
			// 
			this.SubHeadersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7f2b23a8-a2b9-429c-a038-dab5085d6eb9", "Sub Headers");
			this.SubHeadersGroupBox.Controls.Add(this.SubHeaderGrid);
			this.SubHeadersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubHeadersGroupBox.Name = "SubHeadersGroupBox";
			this.SubHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 160, true);
			this.SubHeadersGroupBox.TabIndex = 8;
			this.SubHeadersGroupBox.TabStop = false;
			// 
			// SubHeaderGrid
			// 
			this.SubHeaderGrid.AllowNavigation = false;
			this.SubHeaderGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.SubHeaderGrid, "B2AsAccountedForInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).JZ_InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).JZ_RN_NKDefaultOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).JZ_RW_NKOriginState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_RN_NKExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_USStateOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_TreatmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).JZ_ValuationDateOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).JZ_RX_NKInvoice_Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_TimeLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_TimeLimitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).CA_TradeZone)));
			this.SubHeaderGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d25d56c0-ca18-4c1c-a725-6840abb1df1e", "Sub-header No.", "Sub-header Number", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JZ_InvoiceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JZ_RN_NKDefaultOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JZ_RW_NKOriginState";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CA_RN_NKExport";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CA_USStateOfExport";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CA_TreatmentCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6841c214-3969-40f0-b99e-9b1dbc277be0", "Direct Shipment Date");
			zDateEditColumnStyleInfo1.ColumnName = "JZ_ValuationDateOverride";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JZ_RX_NKInvoice_Currency";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "CA_TimeLimit";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CA_TimeLimitCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("41321098-0bbe-4fed-ba0c-586b99442b4e", "Zone Code");
			zTextBoxColumnStyleInfo2.ColumnName = "CA_TradeZone";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SubHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SubHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SubHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SubHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.SubHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SubHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SubHeaderGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SubHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.SubHeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SubHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.SubHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SubHeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubHeaderGrid.GridId = "b3ce884a-f0b3-4c66-a4da-4f79cf667052";
			this.SubHeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubHeaderGrid.LayoutKey = "SubHeaderGrid";
			this.SubHeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SubHeaderGrid.Name = "SubHeaderGrid";
			this.SubHeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 141, true);
			this.SubHeaderGrid.TabIndex = 2;
			// 
			// SubHeaderSplitContainer
			// 
			this.SubHeaderSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubHeaderSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubHeaderSplitContainer.Name = "SubHeaderSplitContainer";
			this.SubHeaderSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SubHeaderSplitContainer.Panel1
			// 
			this.SubHeaderSplitContainer.Panel1.Controls.Add(this.SelectLinesSplitContainer);
			// 
			// SubHeaderSplitContainer.Panel2
			// 
			this.SubHeaderSplitContainer.Panel2.Controls.Add(this.InvoiceLinesPanel);
			this.SubHeaderSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 700, true);
			this.SubHeaderSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(189);
			this.SubHeaderSplitContainer.TabIndex = 1;
			// 
			// SelectLinesSplitContainer
			// 
			this.SelectLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectLinesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SelectLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectLinesSplitContainer.Name = "SelectLinesSplitContainer";
			this.SelectLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SelectLinesSplitContainer.Panel1
			// 
			this.SelectLinesSplitContainer.Panel1.Controls.Add(this.ApportionmentPendingLabel);
			this.SelectLinesSplitContainer.Panel1.Controls.Add(this.SelectingLinesButton);
			this.SelectLinesSplitContainer.Panel1.Controls.Add(this.OriginalTransactionNumberPanel);
			// 
			// SelectLinesSplitContainer.Panel2
			// 
			this.SelectLinesSplitContainer.Panel2.Controls.Add(this.SubHeadersGroupBox);
			this.SelectLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 189, true);
			this.SelectLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.SelectLinesSplitContainer.TabIndex = 0;
			// 
			// ApportionmentPendingLabel
			// 
			this.ApportionmentPendingLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f47888bd-96eb-4c6f-beca-7f9b361fbb3c", "Apportionment of Charges is pending. Selecting SAVE or alternatively selecting Brokerage -> Perform apportionment will run this feature.");
			this.ApportionmentPendingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionmentPendingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ApportionmentPendingLabel.IsFontBold = true;
			this.ApportionmentPendingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 0, true);
			this.ApportionmentPendingLabel.Name = "ApportionmentPendingLabel";
			this.ApportionmentPendingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 25, true);
			this.ApportionmentPendingLabel.TabIndex = 2;
			this.ApportionmentPendingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// InvoiceLinesPanel
			// 
			this.InvoiceLinesPanel.Controls.Add(this.asAccountSplitContainer);
			this.InvoiceLinesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLinesPanel.Name = "InvoiceLinesPanel";
			this.InvoiceLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 507, true);
			this.InvoiceLinesPanel.TabIndex = 0;
			// 
			// asAccountSplitContainer
			// 
			this.asAccountSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asAccountSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asAccountSplitContainer.Name = "asAccountSplitContainer";
			this.asAccountSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// asAccountSplitContainer.Panel1
			// 
			this.asAccountSplitContainer.Panel1.Controls.Add(this.AsAccountForGroupBox);
			// 
			// asAccountSplitContainer.Panel2
			// 
			this.asAccountSplitContainer.Panel2.Controls.Add(this.asAccountTableLayoutPanel);
			this.asAccountSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 507, true);
			this.asAccountSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(155);
			this.asAccountSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(295);
			this.asAccountSplitContainer.TabIndex = 1;
			// 
			// AsAccountForGroupBox
			// 
			this.AsAccountForGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("62d436ac-fb7c-44ce-933e-e3c28180ca0f", "As Accounted For");
			this.AsAccountForGroupBox.Controls.Add(this.AsAccountForGrid);
			this.AsAccountForGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsAccountForGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsAccountForGroupBox.Name = "AsAccountForGroupBox";
			this.AsAccountForGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 295, true);
			this.AsAccountForGroupBox.TabIndex = 9;
			this.AsAccountForGroupBox.TabStop = false;
			// 
			// AsAccountForGrid
			// 
			this.AsAccountForGrid.AllowNavigation = false;
			this.AsAccountForGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.AsAccountForGrid, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_OriginalLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.TariffPropertyInfo)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_FormattedTariffCodeTariffInfo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_AuthorityNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_99TariffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_CustomsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_ValueForDutyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_CVforCurrConv)));
			this.AsAccountForGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a815328f-2a0d-403a-805a-7e5270290c21", "Original Line No.");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CA_OriginalLineNo";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.BindToTariffPropertyInfo = "JI_FormattedTariffCodeTariffInfo";
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("363462c6-aae2-41dc-b3be-888afe614122", "Class. Tariff #", "Classification Tariff Number", "");
			tariffColumnStyleInfo1.ColumnName = "JI_FormattedTariff";
			tariffColumnStyleInfo1.TariffCode = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JI_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CA_AuthorityNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CA_99TariffCode";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2dc78d56-3008-41d0-9c97-b83b0b35e19c", "VFD", "Value for Duty", "");
			zCalcEditColumnStyleInfo2.ColumnName = "CA_CustomsValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "CA_ValueForDutyCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "JI_CustomsQuantity";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "JI_CustomsUnitQty";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo4.ColumnName = "JI_CustomsSecondQuantity";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "JI_CustomsSecondUnitQty";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo5.ColumnName = "JI_CustomsThirdQuantity";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "JI_CustomsThirdUnitQty";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("77fa9728-c70d-4744-9d30-5685499b94ca", "VFCC", "Value for Currency Conversion", "");
			zCalcEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo6.ColumnName = "CA_CVforCurrConv";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AsAccountForGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AsAccountForGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.AsAccountForGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AsAccountForGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AsAccountForGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.AsAccountForGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AsAccountForGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.AsAccountForGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AsAccountForGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.AsAccountForGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AsAccountForGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.AsAccountForGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.AsAccountForGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.AsAccountForGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.AsAccountForGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsAccountForGrid.GridId = "2ee37e25-79a5-46b5-860e-4ece44bbbd1c";
			this.AsAccountForGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AsAccountForGrid.LayoutKey = "AsAccountForGrid";
			this.AsAccountForGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AsAccountForGrid.Name = "AsAccountForGrid";
			this.AsAccountForGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 276, true);
			this.AsAccountForGrid.TabIndex = 3;
			// 
			// asAccountTableLayoutPanel
			// 
			this.asAccountTableLayoutPanel.ColumnCount = 2;
			this.asAccountTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.asAccountTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.asAccountTableLayoutPanel.Controls.Add(this.AsAccountForDetailsGroupBox, 0, 0);
			this.asAccountTableLayoutPanel.Controls.Add(this.asAccountDutyTaxSplitContainer, 1, 0);
			this.asAccountTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asAccountTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asAccountTableLayoutPanel.Name = "asAccountTableLayoutPanel";
			this.asAccountTableLayoutPanel.RowCount = 1;
			this.asAccountTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.asAccountTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 208, true);
			this.asAccountTableLayoutPanel.TabIndex = 0;
			// 
			// AsAccountForDetailsGroupBox
			// 
			this.AsAccountForDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d53c6a66-ac6a-46f9-9315-b89885581020", "Details");
			this.AsAccountForDetailsGroupBox.Controls.Add(this.ClassificationTariffFindBox);
			this.AsAccountForDetailsGroupBox.Controls.Add(this.JI_DescriptionBoundTextBox);
			this.AsAccountForDetailsGroupBox.Controls.Add(this.ValueForDutyCodeDropEdit);
			this.AsAccountForDetailsGroupBox.Controls.Add(this.TariffCodeCodeFindBox);
			this.AsAccountForDetailsGroupBox.Controls.Add(this.SpecialAuthorityNoTextBox);
			this.AsAccountForDetailsGroupBox.Controls.Add(this.OriginalB3LineNoTextBox);
			this.AsAccountForDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsAccountForDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AsAccountForDetailsGroupBox.Name = "AsAccountForDetailsGroupBox";
			this.AsAccountForDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 202, true);
			this.AsAccountForDetailsGroupBox.TabIndex = 2;
			this.AsAccountForDetailsGroupBox.TabStop = false;
			// 
			// ClassificationTariffFindBox
			// 
			this.ClassificationTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClassificationTariffFindBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			this.ClassificationTariffFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8f7a53b9-f290-40b0-9b5a-cca5be28d148", "Class. Tariff #", "Classification Tariff Number", "");
			this.ClassificationTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 42, true);
			this.ClassificationTariffFindBox.Name = "ClassificationTariffFindBox";
			this.ClassificationTariffFindBox.ShouldResize = true;
			this.ClassificationTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			this.ClassificationTariffFindBox.TabIndex = 2;
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.AllowDrop = true;
			this.JI_DescriptionBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b8bc1318-8418-43aa-82e8-db7e7e99727d", "Goods Description");
			this.JI_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 121, true);
			this.JI_DescriptionBoundTextBox.Name = "JI_DescriptionBoundTextBox";
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 5;
			// 
			// ValueForDutyCodeDropEdit
			// 
			this.ValueForDutyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueForDutyCodeDropEdit, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_ValueForDutyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_ValueForDutyCode)));
			this.ValueForDutyCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e388024d-0548-4cc7-93a0-cbe6e08a949c", "Value for Duty Code");
			this.ValueForDutyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 94, true);
			this.ValueForDutyCodeDropEdit.Name = "ValueForDutyCodeDropEdit";
			this.ValueForDutyCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ValueForDutyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			this.ValueForDutyCodeDropEdit.TabIndex = 4;
			// 
			// TariffCodeCodeFindBox
			// 
			this.TariffCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeCodeFindBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_99TariffCode)));
			this.TariffCodeCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4fa1766-3c4b-4ca9-bc1d-d076d2f65375", "Tariff Code");
			this.TariffCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 68, true);
			this.TariffCodeCodeFindBox.Name = "TariffCodeCodeFindBox";
			this.TariffCodeCodeFindBox.ShouldResize = true;
			this.TariffCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			this.TariffCodeCodeFindBox.TabIndex = 3;
			// 
			// SpecialAuthorityNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SpecialAuthorityNoTextBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_AuthorityNumber)));
			this.SpecialAuthorityNoTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("96c03b26-dbb1-4dd2-bc04-27ffd8b0fd7f", "Auth/Pmt", "Special Auth/Pmt", "Special Authority/Permit", "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.");
			this.SpecialAuthorityNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 16, true);
			this.SpecialAuthorityNoTextBox.Name = "SpecialAuthorityNoTextBox";
			this.SpecialAuthorityNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SpecialAuthorityNoTextBox.TabIndex = 1;
			// 
			// OriginalB3LineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginalB3LineNoTextBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_OriginalLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_OriginalLineNo)));
			this.OriginalB3LineNoTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0c1f7842-6df4-4a12-acd0-0b477c3e7053", "Original Entry Line No.");
			this.OriginalB3LineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 16, true);
			this.OriginalB3LineNoTextBox.Name = "OriginalB3LineNoTextBox";
			this.OriginalB3LineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalB3LineNoTextBox.TabIndex = 0;
			// 
			// asAccountDutyTaxSplitContainer
			// 
			this.asAccountDutyTaxSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asAccountDutyTaxSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.asAccountDutyTaxSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 3, true);
			this.asAccountDutyTaxSplitContainer.Name = "asAccountDutyTaxSplitContainer";
			this.asAccountDutyTaxSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// asAccountDutyTaxSplitContainer.Panel1
			// 
			this.asAccountDutyTaxSplitContainer.Panel1.Controls.Add(this.AsAccountForDutyAndTaxGroupBox);
			// 
			// asAccountDutyTaxSplitContainer.Panel2
			// 
			this.asAccountDutyTaxSplitContainer.Panel2.Controls.Add(this.AsAccountDutyAndTaxGrid);
			this.asAccountDutyTaxSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 202, true);
			this.asAccountDutyTaxSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(66);
			this.asAccountDutyTaxSplitContainer.TabIndex = 3;
			// 
			// AsAccountForDutyAndTaxGroupBox
			// 
			this.AsAccountForDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("45bc3727-06c6-4eca-99d6-3ac4ae9c8b8e", "Line Duties and Taxes");
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.VauleForDutyCalcEdit);
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.AsAccountSIMADumpingDescriptionTextBox);
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.ConversionValueCalcEdit);
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.CustomQuantityCalcDropEdit);
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.Qty2CalcDropEdit);
			this.AsAccountForDutyAndTaxGroupBox.Controls.Add(this.Qty3CalcDropEdit);
			this.AsAccountForDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsAccountForDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsAccountForDutyAndTaxGroupBox.Name = "AsAccountForDutyAndTaxGroupBox";
			this.AsAccountForDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 66, true);
			this.AsAccountForDutyAndTaxGroupBox.TabIndex = 3;
			this.AsAccountForDutyAndTaxGroupBox.TabStop = false;
			// 
			// VauleForDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VauleForDutyCalcEdit, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_CustomsValue)));
			this.VauleForDutyCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1cb661d9-d59a-40c4-9ef2-ea3f30098aa2", "VFD", "Value for Duty", "");
			this.VauleForDutyCalcEdit.DecimalPlaces = 2;
			this.VauleForDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 43, true);
			this.VauleForDutyCalcEdit.Name = "VauleForDutyCalcEdit";
			this.VauleForDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.VauleForDutyCalcEdit.TabIndex = 5;
			this.VauleForDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AsAccountSIMADumpingDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.AsAccountSIMADumpingDescriptionTextBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_SIMADumpingDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_SIMADumpingDesc)));
			this.AsAccountSIMADumpingDescriptionTextBox.CaptionResourceString = null;
			this.AsAccountSIMADumpingDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 42, true);
			this.AsAccountSIMADumpingDescriptionTextBox.Name = "AsAccountSIMADumpingDescriptionTextBox";
			this.AsAccountSIMADumpingDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.AsAccountSIMADumpingDescriptionTextBox.TabIndex = 3;
			// 
			// ConversionValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConversionValueCalcEdit, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.CA_CVforCurrConv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).CA_CVforCurrConv)));
			this.ConversionValueCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("63ce7778-cb49-4152-b35c-fcf77c5e29d3", "VFCC", "Value for Currency Conversion", "");
			this.ConversionValueCalcEdit.DecimalPlaces = 2;
			this.ConversionValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 42, true);
			this.ConversionValueCalcEdit.Name = "ConversionValueCalcEdit";
			this.ConversionValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ConversionValueCalcEdit.TabIndex = 4;
			this.ConversionValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomQuantityCalcDropEdit
			// 
			this.CustomQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
			this.CustomQuantityCalcDropEdit.BindToAmount = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsQuantity";
			this.CustomQuantityCalcDropEdit.BindToUnit = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsUnitQty";
			this.CustomQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b3fe295a-8a5e-40b8-a2ac-167a3a81cec9", "Customs Qty");
			this.CustomQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.CustomQuantityCalcDropEdit.Name = "CustomQuantityCalcDropEdit";
			this.CustomQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CustomQuantityCalcDropEdit.TabIndex = 0;
			// 
			// Qty2CalcDropEdit
			// 
			this.Qty2CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Qty2CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondUnitQty)));
			this.Qty2CalcDropEdit.BindToAmount = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsSecondQuantit" +
    "y";
			this.Qty2CalcDropEdit.BindToUnit = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsSecondUnitQty" +
    "";
			this.Qty2CalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("826493de-80af-48d9-b176-07305c3cf076", "Qty 2", "Customs Qty 2", "");
			this.Qty2CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 16, true);
			this.Qty2CalcDropEdit.Name = "Qty2CalcDropEdit";
			this.Qty2CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.Qty2CalcDropEdit.TabIndex = 1;
			// 
			// Qty3CalcDropEdit
			// 
			this.Qty3CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Qty3CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdUnitQty)));
			this.Qty3CalcDropEdit.BindToAmount = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsThirdQuantity" +
    "";
			this.Qty3CalcDropEdit.BindToUnit = "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_CustomsThirdUnitQty";
			this.Qty3CalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("350cf32e-5c2d-4034-8156-72cd4aabcbcd", "Qty 2", "Customs Qty 3", "");
			this.Qty3CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 16, true);
			this.Qty3CalcDropEdit.Name = "Qty3CalcDropEdit";
			this.Qty3CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.Qty3CalcDropEdit.TabIndex = 2;
			// 
			// AsAccountDutyAndTaxGrid
			// 
			this.AsAccountDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AsAccountDutyAndTaxGrid, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.DutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_TaxType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_ExemptCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Override)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_RateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_PreviousTranLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_PreviousTranNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_NormalValuePerUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_NormalValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).NormalValueCurrencyExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_ForeignRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_ForeignCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).ForeignCurrencyExchangeRate)));
			this.AsAccountDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo9.ColumnName = "C1_TaxType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo6.ColumnName = "C1_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.ColumnName = "Description";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "C1_ExemptCode";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "C1_Override";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.ColumnName = "C1_RateType";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "C1_Rate";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "C1_UnitOfMeasure";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(41);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "C1_Amount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "C1_PreviousTranLine";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "C1_PreviousTranNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "C1_NormalValuePerUnit";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCodeFindBoxColumnStyleInfo5.ColumnName = "C1_NormalValueCurrency";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "NormalValueCurrencyExchangeRate";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "C1_ForeignRate";
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "C1_ForeignCurrency";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "ForeignCurrencyExchangeRate";
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.AsAccountDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.AsAccountDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsAccountDutyAndTaxGrid.GridId = "6fd6af6c-95df-417f-b56a-7e09a48a0055";
			this.AsAccountDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AsAccountDutyAndTaxGrid.LayoutKey = "AsAccountDutyAndTaxGrid";
			this.AsAccountDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsAccountDutyAndTaxGrid.Name = "AsAccountDutyAndTaxGrid";
			this.AsAccountDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 132, true);
			this.AsAccountDutyAndTaxGrid.TabIndex = 4;
			// 
			// B2LineAsAccountedForUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubHeaderSplitContainer);
			this.Name = "B2LineAsAccountedForUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1125, 700, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OriginalTransactionNumberPanel.ResumeLayout(false);
			this.OriginalTransactionNumberPanel.PerformLayout();
			this.SubHeadersGroupBox.ResumeLayout(false);
			this.SubHeadersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderGrid)).EndInit();
			this.SubHeaderGrid.ResumeLayout(false);
			this.SubHeaderGrid.PerformLayout();
			this.SubHeaderSplitContainer.Panel1.ResumeLayout(false);
			this.SubHeaderSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderSplitContainer)).EndInit();
			this.SubHeaderSplitContainer.ResumeLayout(false);
			this.SubHeaderSplitContainer.PerformLayout();
			this.SelectLinesSplitContainer.Panel1.ResumeLayout(false);
			this.SelectLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SelectLinesSplitContainer)).EndInit();
			this.SelectLinesSplitContainer.ResumeLayout(false);
			this.SelectLinesSplitContainer.PerformLayout();
			this.InvoiceLinesPanel.ResumeLayout(false);
			this.InvoiceLinesPanel.PerformLayout();
			this.asAccountSplitContainer.Panel1.ResumeLayout(false);
			this.asAccountSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.asAccountSplitContainer)).EndInit();
			this.asAccountSplitContainer.ResumeLayout(false);
			this.asAccountSplitContainer.PerformLayout();
			this.AsAccountForGroupBox.ResumeLayout(false);
			this.AsAccountForGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AsAccountForGrid)).EndInit();
			this.AsAccountForGrid.ResumeLayout(false);
			this.AsAccountForGrid.PerformLayout();
			this.asAccountTableLayoutPanel.ResumeLayout(false);
			this.asAccountTableLayoutPanel.PerformLayout();
			this.AsAccountForDetailsGroupBox.ResumeLayout(false);
			this.AsAccountForDetailsGroupBox.PerformLayout();
			this.ClassificationTariffFindBox.ResumeLayout(true);
			this.ClassificationTariffFindBox.PerformLayout();
			this.JI_DescriptionBoundTextBox.ResumeLayout(true);
			this.JI_DescriptionBoundTextBox.PerformLayout();
			this.ValueForDutyCodeDropEdit.ResumeLayout(true);
			this.ValueForDutyCodeDropEdit.PerformLayout();
			this.TariffCodeCodeFindBox.ResumeLayout(true);
			this.TariffCodeCodeFindBox.PerformLayout();
			this.asAccountDutyTaxSplitContainer.Panel1.ResumeLayout(false);
			this.asAccountDutyTaxSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.asAccountDutyTaxSplitContainer)).EndInit();
			this.asAccountDutyTaxSplitContainer.ResumeLayout(false);
			this.asAccountDutyTaxSplitContainer.PerformLayout();
			this.AsAccountForDutyAndTaxGroupBox.ResumeLayout(false);
			this.AsAccountForDutyAndTaxGroupBox.PerformLayout();
			this.CustomQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomQuantityCalcDropEdit.PerformLayout();
			this.Qty2CalcDropEdit.ResumeLayout(true);
			this.Qty2CalcDropEdit.PerformLayout();
			this.Qty3CalcDropEdit.ResumeLayout(true);
			this.Qty3CalcDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AsAccountDutyAndTaxGrid)).EndInit();
			this.AsAccountDutyAndTaxGrid.ResumeLayout(false);
			this.AsAccountDutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SubHeaderGrid;
		private ZArchitecture.ZTextBox OriginalTransactionTextBox;
		internal ZArchitecture.GUI.ZButton SelectingLinesButton;
		private ZArchitecture.GUI.ZGroupBox SubHeadersGroupBox;
		private ZArchitecture.GUI.ZPanel OriginalTransactionNumberPanel;
		private CargoWise.Windows.UI.KSplitContainer SubHeaderSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer SelectLinesSplitContainer;
		private ZArchitecture.GUI.ZPanel InvoiceLinesPanel;
		internal ZArchitecture.ZLabel ApportionmentPendingLabel;
		private CargoWise.Windows.UI.KSplitContainer asAccountSplitContainer;
		private ZArchitecture.GUI.ZGroupBox AsAccountForGroupBox;
		private ZArchitecture.ZGrid AsAccountForGrid;
		private CargoWise.Windows.UI.KTableLayoutPanel asAccountTableLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox AsAccountForDetailsGroupBox;
		private Common.GUI.TariffFindBox ClassificationTariffFindBox;
		private JobComInvocieLineBoundLongTextControl JI_DescriptionBoundTextBox;
		private ZArchitecture.GUI.ZDropEdit ValueForDutyCodeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox TariffCodeCodeFindBox;
		private ZArchitecture.ZTextBox SpecialAuthorityNoTextBox;
		private ZArchitecture.ZTextBox OriginalB3LineNoTextBox;
		private CargoWise.Windows.UI.KSplitContainer asAccountDutyTaxSplitContainer;
		private ZArchitecture.GUI.ZGroupBox AsAccountForDutyAndTaxGroupBox;
		private ZArchitecture.ZTextBox AsAccountSIMADumpingDescriptionTextBox;
		private ZArchitecture.ZCalcEdit ConversionValueCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CustomQuantityCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit Qty2CalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit Qty3CalcDropEdit;
		private ZArchitecture.ZGrid AsAccountDutyAndTaxGrid;
		private ZArchitecture.ZCalcEdit VauleForDutyCalcEdit;
	}
}
