namespace Enterprise.Customs.EU.GUI
{
	partial class EUCustomsSupplierHeaderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.InvoicePaymentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.invoicePaymentUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.AdditionalInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.additionalInfosUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.previousDocumentsUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ValueIndicatorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ValueIndicatorsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.InvoiceChargesButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceChargesCalculateInsuranceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceChargesCalculateDDPButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceChargesCalculateFreightButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GroupChargesButton = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GroupChargesCalculateFreightButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceChargesAddDeductTotal = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceChargesGroupLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GroupChargesAddDeductTotal = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GroupInvoiceTotal = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GroupChargesGroupLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DetailsGroupBox.SuspendLayout();
			this.InvoiceDetailUserControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.InvoicePaymentTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.ValueIndicatorsTabPage.SuspendLayout();
			this.InvoiceChargesButtonPanel.SuspendLayout();
			this.GroupChargesButton.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 299, true);
			// 
			// InvoiceDetailUserControl
			// 
			this.InvoiceDetailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 280, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 326, true);
			// 
			// IncoTermTextBox
			// 
			this.IncoTermTextBox.Visible = false;
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Controls.Add(this.InvoicePaymentTabPage);
			this.InvoiceTabControl.Controls.Add(this.ValueIndicatorsTabPage);
			this.InvoiceTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.InvoiceTabControl.Controls.Add(this.AdditionalInfoTabPage);
			this.InvoiceTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.InvoiceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 326, true);
			this.InvoiceTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.PreviousDocumentsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.AdditionalInfoTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.ValueIndicatorsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.InvoicePaymentTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.ComInvoiceDetailsTabPage, 0);
			// 
			// ComInvoiceDetailsTabPage
			// 
			this.ComInvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 299, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 155, true);
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 136, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Controls.Add(this.InvoiceChargesButtonPanel);
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 109, true);
			this.InvoiceChargesTabPage.Controls.SetChildIndex(this.InvoiceChargesButtonPanel, 0);
			this.InvoiceChargesTabPage.Controls.SetChildIndex(this.InvoiceChargesGrid, 0);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 109, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Controls.Add(this.GroupChargesButton);
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 138, true);
			this.BaseGroupChargesGroupBox.Controls.SetChildIndex(this.GroupChargesButton, 0);
			this.BaseGroupChargesGroupBox.Controls.SetChildIndex(this.BaseGroupChargesGrid, 0);
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 119, true);
			// 
			// JZ_FOBAmountBoundCurrencyControl
			// 
			this.JZ_FOBAmountBoundCurrencyControl.Visible = false;
			// 
			// JZ_Calc_TNIBoundInvoiceCurrencyControl
			// 
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;
			// 
			// JZ_CIFAmountBoundCurrencyControl
			// 
			this.JZ_CIFAmountBoundCurrencyControl.Visible = false;
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			this.JobComInvoiceHeadersBoundGrid.GridId = null;
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = null;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 62, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			this.JobComInvoiceHeadersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 173, true);
			// 
			// Splitter
			// 
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 540, true);
			this.Splitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(173);
			// 
			// InvDetailLeftPanel
			// 
			this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 299, true);
			// 
			// InvDetailRightPanel
			// 
			this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 299, true);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 299, true);
			// 
			// ChargesGroupsSplitterContainer.Panel1
			// 
			this.ChargesGroupsSplitterContainer.Panel1.Controls.Add(this.ChargesTabControl);
			this.ChargesGroupsSplitterContainer.Panel1.Controls.Add(this.InvoiceChargesAddDeductTotal);
			this.ChargesGroupsSplitterContainer.Panel1.Controls.Add(this.InvoiceChargesGroupLabel);
			// 
			// ChargesGroupsSplitterContainer.Panel2
			// 
			this.ChargesGroupsSplitterContainer.Panel2.Controls.Add(this.BaseGroupChargesGrid);
			this.ChargesGroupsSplitterContainer.Panel2.Controls.Add(this.GroupInvoiceTotal);
			this.ChargesGroupsSplitterContainer.Panel2.Controls.Add(this.GroupChargesAddDeductTotal);
			this.ChargesGroupsSplitterContainer.Panel2.Controls.Add(this.GroupChargesGroupLabel);
			this.ChargesGroupsSplitterContainer.Panel2.Controls.Add(this.GroupChargesButton);
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 299, true);
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(155);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.SupportingDocumentsTabPage.TabIndex = 1;
			this.SupportingDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// SupportingDocumentsUserControl
			// 
			this.SupportingDocumentsUserControl.AllowDrop = true;
			this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
			this.SupportingDocumentsUserControl.TabIndex = 1;
			// 
			// InvoicePaymentTabPage
			// 
			this.InvoicePaymentTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUCustomsSupplierHeaderUserControl|A24389F4-C647-42C0-830F-4A31E6C3F77D", "Payment");
			this.InvoicePaymentTabPage.Controls.Add(this.invoicePaymentUserControl1);
			this.InvoicePaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoicePaymentTabPage.Name = "InvoicePaymentTabPage";
			this.InvoicePaymentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InvoicePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.InvoicePaymentTabPage.TabIndex = 2;
			this.InvoicePaymentTabPage.UseVisualStyleBackColor = true;
			// 
			// invoicePaymentUserControl1
			// 
			this.invoicePaymentUserControl1.AllowDrop = true;
			this.invoicePaymentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.invoicePaymentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.invoicePaymentUserControl1.Name = "invoicePaymentUserControl1";
			this.invoicePaymentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
			this.invoicePaymentUserControl1.TabIndex = 1;
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.Controls.Add(this.additionalInfosUserControl1);
			this.AdditionalInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInfoTabPage.Name = "AdditionalInfoTabPage";
			this.AdditionalInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.AdditionalInfoTabPage.TabIndex = 2;
			this.AdditionalInfoTabPage.UseVisualStyleBackColor = true;
			// 
			// additionalInfosUserControl1
			// 
			this.additionalInfosUserControl1.AllowDrop = true;
			this.additionalInfosUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalInfosUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.additionalInfosUserControl1.Name = "additionalInfosUserControl1";
			this.additionalInfosUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
			this.additionalInfosUserControl1.TabIndex = 1;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.Controls.Add(this.previousDocumentsUserControl1);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 299, true);
			this.PreviousDocumentsTabPage.TabIndex = 3;
			this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// previousDocumentsUserControl1
			// 
			this.previousDocumentsUserControl1.AllowDrop = true;
			this.previousDocumentsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previousDocumentsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.previousDocumentsUserControl1.Name = "previousDocumentsUserControl1";
			this.previousDocumentsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 293, true);
			this.previousDocumentsUserControl1.TabIndex = 1;
			// 
			// ValueIndicatorsTabPage
			// 
			this.ValueIndicatorsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ValueIndicatorsTabPage.Controls.Add(this.ValueIndicatorsUserControl);
			this.ValueIndicatorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ValueIndicatorsTabPage.Name = "ValueIndicatorsTabPage";
			this.ValueIndicatorsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ValueIndicatorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.ValueIndicatorsTabPage.TabIndex = 100;
			// 
			// ValueIndicatorsUserControl
			// 
			this.ValueIndicatorsUserControl.AllowDrop = true;
			this.ValueIndicatorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValueIndicatorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ValueIndicatorsUserControl.Name = "ValueIndicatorsUserControl";
			this.ValueIndicatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
			this.ValueIndicatorsUserControl.TabIndex = 1;
			// 
			// InvoiceChargesButtonPanel
			// 
			this.InvoiceChargesButtonPanel.Controls.Add(this.InvoiceChargesCalculateDDPButton);
			this.InvoiceChargesButtonPanel.Controls.Add(this.InvoiceChargesCalculateInsuranceButton);
			this.InvoiceChargesButtonPanel.Controls.Add(this.InvoiceChargesCalculateFreightButton);
			this.InvoiceChargesButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.InvoiceChargesButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 0, true);
			this.InvoiceChargesButtonPanel.Name = "InvoiceChargesButtonPanel";
			this.InvoiceChargesButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 109, true);
			this.InvoiceChargesButtonPanel.TabIndex = 1;
			// 
			// InvoiceChargesCalculateInsuranceButton
			// 
			this.InvoiceChargesCalculateInsuranceButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("32DB1714-E8F6-4BA2-B816-577D03AE798E", "Calculate Insurance");
			this.InvoiceChargesCalculateInsuranceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 45, true);
			this.InvoiceChargesCalculateInsuranceButton.Name = "InvoiceChargesCalculateInsuranceButton";
			this.InvoiceChargesCalculateInsuranceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 35, true);
			this.InvoiceChargesCalculateInsuranceButton.TabIndex = 1;
			this.InvoiceChargesCalculateInsuranceButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceChargesCalculateInsuranceButton.ToolTipCaption = null;
			this.InvoiceChargesCalculateInsuranceButton.UseVisualStyleBackColor = true;
			this.InvoiceChargesCalculateInsuranceButton.Click += new System.EventHandler(this.InvoiceChargesCalculateInsuranceButton_Click);
			// 
			// InvoiceChargesCalculateFreightButton
			// 
			this.InvoiceChargesCalculateFreightButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("aa612f37-62dd-4916-b8fc-aa7f6c66ecd2", "Calculate Freight");
			this.InvoiceChargesCalculateFreightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.InvoiceChargesCalculateFreightButton.Name = "InvoiceChargesCalculateFreightButton";
			this.InvoiceChargesCalculateFreightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 35, true);
			this.InvoiceChargesCalculateFreightButton.TabIndex = 0;
			this.InvoiceChargesCalculateFreightButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceChargesCalculateFreightButton.ToolTipCaption = null;
			this.InvoiceChargesCalculateFreightButton.UseVisualStyleBackColor = true;
			this.InvoiceChargesCalculateFreightButton.Click += new System.EventHandler(this.InvoiceChargesCalculateFreightButton_Click);
			// 
			// InvoiceChargesCalculateDDPButton
			// 
			this.InvoiceChargesCalculateDDPButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1B450BE0-22D6-46A9-B0F5-062D7EC52C9D", "Calculate DDP");
			this.InvoiceChargesCalculateDDPButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 45, true);
			this.InvoiceChargesCalculateDDPButton.Name = "InvoiceChargesCalculateDDPButton";
			this.InvoiceChargesCalculateDDPButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 35, true);
			this.InvoiceChargesCalculateDDPButton.TabIndex = 2;
			this.InvoiceChargesCalculateDDPButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceChargesCalculateDDPButton.ToolTipCaption = null;
			this.InvoiceChargesCalculateDDPButton.UseVisualStyleBackColor = true;
			this.InvoiceChargesCalculateDDPButton.Click += new System.EventHandler(this.InvoiceChargesCalculateDDPButton_Click);
			// 
			// GroupChargesButton
			// 
			this.GroupChargesButton.Controls.Add(this.GroupChargesCalculateFreightButton);
			this.GroupChargesButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.GroupChargesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 16, true);
			this.GroupChargesButton.Name = "GroupChargesButton";
			this.GroupChargesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 119, true);
			this.GroupChargesButton.TabIndex = 2;
			// 
			// GroupChargesCalculateFreightButton
			// 
			this.GroupChargesCalculateFreightButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5f408f5a-3310-4156-a539-1e58d4ccb2be", "Calculate Freight");
			this.GroupChargesCalculateFreightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.GroupChargesCalculateFreightButton.Name = "GroupChargesCalculateFreightButton";
			this.GroupChargesCalculateFreightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 35, true);
			this.GroupChargesCalculateFreightButton.TabIndex = 0;
			this.GroupChargesCalculateFreightButton.ToolTipCaption = null;
			this.GroupChargesCalculateFreightButton.UseVisualStyleBackColor = true;
			this.GroupChargesCalculateFreightButton.Click += new System.EventHandler(this.GroupChargesCalculateFreightButton_Click);
			// 
			// InvoiceChargesAddDeductTotal
			// 
			this.InvoiceChargesAddDeductTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoiceChargesAddDeductTotal, "Invoices.ChargesAddDeductTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).ChargesAddDeductTotal)));
			this.InvoiceChargesAddDeductTotal.DecimalPlaces = 2;
			this.InvoiceChargesAddDeductTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 2, true);
			this.InvoiceChargesAddDeductTotal.Name = "InvoiceChargesAddDeductTotal";
			this.InvoiceChargesAddDeductTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.InvoiceChargesAddDeductTotal.TabIndex = 16;
			this.InvoiceChargesAddDeductTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InvoiceChargesAddDeductTotal.TrackDisposedAccess = true;
			// 
			// ChargesGroupLabel
			// 
			this.InvoiceChargesGroupLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ab71d544-87d8-4353-ba0c-eb73b9a5bc31", "Invoice Charges");
			this.InvoiceChargesGroupLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.InvoiceChargesGroupLabel.IsFontBold = true;
			this.InvoiceChargesGroupLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.InvoiceChargesGroupLabel.Name = "InvoiceChargesGroupLabel";
			this.InvoiceChargesGroupLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 15, true);
			this.InvoiceChargesGroupLabel.TabIndex = 16;
			this.InvoiceChargesGroupLabel.UseMnemonic = false;
			// 
			// GroupChargesAddDeductTotal
			// 
			this.GroupChargesAddDeductTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GroupChargesAddDeductTotal, "Invoices.GroupHeader.ChargesAddDeductTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).GroupHeader.ChargesAddDeductTotal)));
			this.GroupChargesAddDeductTotal.DecimalPlaces = 2;
			this.GroupChargesAddDeductTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 2, true);
			this.GroupChargesAddDeductTotal.Name = "GroupChargesAddDeductTotal";
			this.GroupChargesAddDeductTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.GroupChargesAddDeductTotal.TabIndex = 16;
			this.GroupChargesAddDeductTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GroupChargesAddDeductTotal.TrackDisposedAccess = true;
			// 
			// GroupInvoiceTotal
			// 
			this.GroupInvoiceTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GroupInvoiceTotal, "Invoices.GroupHeader.TotalInvoiceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).GroupHeader.TotalInvoiceAmount)));
			this.GroupInvoiceTotal.DecimalPlaces = 2;
			this.GroupInvoiceTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 2, true);
			this.GroupInvoiceTotal.Name = "GroupInvoiceTotal";
			this.GroupInvoiceTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.GroupInvoiceTotal.TabIndex = 16;
			this.GroupInvoiceTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GroupInvoiceTotal.TrackDisposedAccess = true;
			// 
			// GroupChargesLabel
			// 
			this.GroupChargesGroupLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("21a7d262-a0c8-47ab-bdc5-a7ad136d1ce1", "Base Group Charges");
			this.GroupChargesGroupLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.GroupChargesGroupLabel.IsFontBold = true;
			this.GroupChargesGroupLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.GroupChargesGroupLabel.Name = "GroupChargesGroupLabel";
			this.GroupChargesGroupLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 15, true);
			this.GroupChargesGroupLabel.TabIndex = 16;
			this.GroupChargesGroupLabel.UseMnemonic = false;
			// 
			// EUCustomsSupplierHeaderUserControl
			// 
			this.Name = "EUCustomsSupplierHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 540, true);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.InvoiceDetailUserControl.ResumeLayout(true);
			this.InvoiceDetailUserControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.InvoicePaymentTabPage.ResumeLayout(false);
			this.InvoicePaymentTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.ValueIndicatorsTabPage.ResumeLayout(false);
			this.ValueIndicatorsTabPage.PerformLayout();
			this.InvoiceChargesButtonPanel.ResumeLayout(false);
			this.InvoiceChargesButtonPanel.PerformLayout();
			this.GroupChargesButton.ResumeLayout(false);
			this.GroupChargesButton.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage InvoicePaymentTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalInfoTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ValueIndicatorsTabPage;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsUserControl;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl invoicePaymentUserControl1;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalInfosUserControl1;
		protected Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl previousDocumentsUserControl1;
		protected Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl ValueIndicatorsUserControl;
		internal protected Enterprise.ZArchitecture.GUI.ZPanel GroupChargesButton;
		protected Enterprise.ZArchitecture.GUI.ZButton GroupChargesCalculateFreightButton;
		private Enterprise.ZArchitecture.GUI.ZPanel InvoiceChargesButtonPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton InvoiceChargesCalculateFreightButton;
		private ZArchitecture.GUI.ZButton InvoiceChargesCalculateInsuranceButton;
		protected Enterprise.ZArchitecture.GUI.ZButton InvoiceChargesCalculateDDPButton;
		internal Enterprise.ZArchitecture.ZCalcEdit InvoiceChargesAddDeductTotal;
		internal ZArchitecture.ZLabel InvoiceChargesGroupLabel;
		internal Enterprise.ZArchitecture.ZCalcEdit GroupChargesAddDeductTotal;
		internal Enterprise.ZArchitecture.ZCalcEdit GroupInvoiceTotal;
		internal ZArchitecture.ZLabel GroupChargesGroupLabel;
	}
}
