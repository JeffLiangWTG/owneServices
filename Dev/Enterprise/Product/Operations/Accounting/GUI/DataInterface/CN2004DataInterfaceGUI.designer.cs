using System;
using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT19581_2004
{
	public partial class CN2004DataInterfaceGUI
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.InterfaceFileGroupBox = new ZGroupBox();
			this.DeliveryToTextBox = new ZTextBox();
			this.DirectoryPopUpButton = new ZButton();
			this.DirectoryTextBox = new ZTextBox();
			this.FromPeriodEdit = new ZPeriodEdit();
			this.CloseButton = new ZButton();
			this.GenerateButton = new ZButton();
			this.LogTextBox = new ZTextBox();
			this.DateGroupBox = new ZGroupBox();
			this.ExportTXTRadioButton = new ZRadioButton();
			this.ExportXMLRadioButton = new ZRadioButton();
			this.zGroupBox1 = new ZGroupBox();
			this.CashFlowStatementCheckBox = new ZCheckBox();
			this.EquityMovementCheckBox = new ZCheckBox();
			this.PNLAppropriationCheckBox = new ZCheckBox();
			this.AssetProvisionCheckBox = new ZCheckBox();
			this.VATDetailedCheckBox = new ZCheckBox();
			this.ChartOfAccountsCheckBox = new ZCheckBox();
			this.ProfitAndLossCheckBox = new ZCheckBox();
			this.AccountingVouchersCheckBox = new ZCheckBox();
			this.TrialBalanceCheckBox = new ZCheckBox();
			this.BalanceSheetCheckBox = new ZCheckBox();
			this.SupplementaryAccountsCheckBox = new ZCheckBox();
			this.AccountBookCheckBox = new ZCheckBox();
			this.zGroupBox2 = new ZGroupBox();
			this.zGroupBox3 = new ZGroupBox();
			this.BranchGuidFindBox = new ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InterfaceFileGroupBox.SuspendLayout();
			this.DateGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 534, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChinaStandard2004DataInterfaceWrapper);
			// 
			// InterfaceFileGroupBox
			// 
			this.InterfaceFileGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("62aca295-84ac-44a5-8964-4e1f33bbfd99", "Export To");
			this.InterfaceFileGroupBox.Controls.Add(this.DeliveryToTextBox);
			this.InterfaceFileGroupBox.Controls.Add(this.DirectoryPopUpButton);
			this.InterfaceFileGroupBox.Controls.Add(this.DirectoryTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InterfaceFileGroupBox, false);
			this.InterfaceFileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 254, true);
			this.InterfaceFileGroupBox.Name = "InterfaceFileGroupBox";
			this.InterfaceFileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 56, true);
			this.InterfaceFileGroupBox.TabIndex = 4;
			this.InterfaceFileGroupBox.TabStop = false;
			// 
			// DeliveryToTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeliveryToTextBox, "DeliveryTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChinaStandard2004DataInterfaceWrapper)(null)).DeliveryTo)));
			this.DeliveryToTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5c054864-69ab-458b-8dc2-5ec850ab0bed", "Email To");
			this.DeliveryToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 21, true);
			this.DeliveryToTextBox.Name = "DeliveryToTextBox";
			this.DeliveryToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 20, true);
			this.DeliveryToTextBox.TabIndex = 16;
			// 
			// DirectoryPopUpButton
			// 
			this.DirectoryPopUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 19, true);
			this.DirectoryPopUpButton.Name = "DirectoryPopUpButton";
			this.DirectoryPopUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 21, true);
			this.DirectoryPopUpButton.TabIndex = 14;
			this.DirectoryPopUpButton.Text = "...";
			this.DirectoryPopUpButton.Click += new EventHandler(this.DirectoryPopUpButton_Click);
			// 
			// DirectoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirectoryTextBox, "ExportDirectory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChinaStandard2004DataInterfaceWrapper)(null)).ExportDirectory)));
			this.DirectoryTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7a34d44c-bdd1-4ae9-8691-aee4a12679db", "Path");
			this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 21, true);
			this.DirectoryTextBox.Name = "DirectoryTextBox";
			this.DirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 20, true);
			this.DirectoryTextBox.TabIndex = 15;
			// 
			// FromPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.FromPeriodEdit, "Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((ChinaStandard2004DataInterfaceWrapper)(null)).Period)));
			this.FromPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4b12b74b-3f8e-46fc-a375-5f956af3b592", "Period");
			this.FromPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 19, true);
			this.FromPeriodEdit.Name = "FromPeriodEdit";
			this.FromPeriodEdit.TabIndex = 12;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|b8703c06-15ec-44cb-a18c-cd012585e2c2", "&Close", "Close.");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 500, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 18;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// GenerateButton
			// 
			this.GenerateButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|a00b6050-de9b-4664-9dab-c798160848ae", "&Export");
			this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 500, true);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.GenerateButton.TabIndex = 17;
			this.GenerateButton.Click += new EventHandler(this.GenerateButton_Click);
			// 
			// LogTextBox
			// 
			this.LogTextBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LogTextBox, "Log");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChinaStandard2004DataInterfaceWrapper)(null)).Log)));
			this.LogTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78c61239-8020-4e31-825a-e21a7e3f88a3", " ");
			this.LogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 320, true);
			this.LogTextBox.Multiline = true;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ReadOnly = true;
			this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 169, true);
			this.LogTextBox.TabIndex = 16;
			this.LogTextBox.TabStop = false;
			// 
			// DateGroupBox
			// 
			this.DateGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("16257467-bf06-41e6-a486-a8fd5f9e6162", "Period");
			this.DateGroupBox.Controls.Add(this.FromPeriodEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DateGroupBox, false);
			this.DateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 198, true);
			this.DateGroupBox.Name = "DateGroupBox";
			this.DateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 51, true);
			this.DateGroupBox.TabIndex = 2;
			this.DateGroupBox.TabStop = false;
			// 
			// ExportTXTRadioButton
			// 
			this.ExportTXTRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ExportTXTRadioButton, "ExportTXT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).ExportTXT)));
			this.ExportTXTRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("26cca124-49f9-4386-9fa6-727fe4901bfe", "TXT", "Export TXT Files");
			this.ExportTXTRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportTXTRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 19, true);
			this.ExportTXTRadioButton.Name = "ExportTXTRadioButton";
			this.ExportTXTRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.ExportTXTRadioButton.TabIndex = 13;
			this.ExportTXTRadioButton.CheckedChanged += new EventHandler(this.ExportTXTRadioButton_CheckedChanged);
			// 
			// ExportXMLRadioButton
			// 
			this.ExportXMLRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ExportXMLRadioButton, "ExportXML");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).ExportXML)));
			this.ExportXMLRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("83593f63-9a6a-48bb-a167-0ae521a23a8b", "XML", "Export XML Files");
			this.ExportXMLRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportXMLRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 19, true);
			this.ExportXMLRadioButton.Name = "ExportXMLRadioButton";
			this.ExportXMLRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ExportXMLRadioButton.TabIndex = 14;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b1e52a1a-f1e9-40eb-b647-50e595de545c", "Data Types");
			this.zGroupBox1.Controls.Add(this.CashFlowStatementCheckBox);
			this.zGroupBox1.Controls.Add(this.EquityMovementCheckBox);
			this.zGroupBox1.Controls.Add(this.PNLAppropriationCheckBox);
			this.zGroupBox1.Controls.Add(this.AssetProvisionCheckBox);
			this.zGroupBox1.Controls.Add(this.VATDetailedCheckBox);
			this.zGroupBox1.Controls.Add(this.ChartOfAccountsCheckBox);
			this.zGroupBox1.Controls.Add(this.ProfitAndLossCheckBox);
			this.zGroupBox1.Controls.Add(this.AccountingVouchersCheckBox);
			this.zGroupBox1.Controls.Add(this.TrialBalanceCheckBox);
			this.zGroupBox1.Controls.Add(this.BalanceSheetCheckBox);
			this.zGroupBox1.Controls.Add(this.SupplementaryAccountsCheckBox);
			this.zGroupBox1.Controls.Add(this.AccountBookCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 131, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Data Types";
			// 
			// CashFlowStatementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CashFlowStatementCheckBox, "CashFlowStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).CashFlowStatement)));
			this.CashFlowStatementCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("289ac2be-9ad2-4990-ae07-98742c4cf50d", "Cash Flow Statement");
			this.CashFlowStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CashFlowStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 98, true);
			this.CashFlowStatementCheckBox.Name = "CashFlowStatementCheckBox";
			this.CashFlowStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.CashFlowStatementCheckBox.TabIndex = 11;
			// 
			// EquityMovementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EquityMovementCheckBox, "EquityMovement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).EquityMovement)));
			this.EquityMovementCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cfe1e8ce-0126-40da-9762-4036cbb0922c", "Shareholder Equity", "Statement of Shareholder Equity");
			this.EquityMovementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EquityMovementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 98, true);
			this.EquityMovementCheckBox.Name = "EquityMovementCheckBox";
			this.EquityMovementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.EquityMovementCheckBox.TabIndex = 10;
			// 
			// PNLAppropriationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PNLAppropriationCheckBox, "PNLAppropriation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).PNLAppropriation)));
			this.PNLAppropriationCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9ea9d85a-e382-4d12-bdb4-26462641a5b6", "P&&L Appropriation");
			this.PNLAppropriationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PNLAppropriationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 98, true);
			this.PNLAppropriationCheckBox.Name = "PNLAppropriationCheckBox";
			this.PNLAppropriationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.PNLAppropriationCheckBox.TabIndex = 9;
			// 
			// AssetProvisionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AssetProvisionCheckBox, "AssetProvision");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).AssetProvision)));
			this.AssetProvisionCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b0254142-66ec-46bb-800f-cf6cfeaf1d6a", "Asset Provision", "China Statement of Provision for Impairment of Assets");
			this.AssetProvisionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AssetProvisionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 72, true);
			this.AssetProvisionCheckBox.Name = "AssetProvisionCheckBox";
			this.AssetProvisionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.AssetProvisionCheckBox.TabIndex = 8;
			// 
			// VATDetailedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.VATDetailedCheckBox, "VATDetailed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).VATDetailed)));
			this.VATDetailedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d9583bc8-6c1d-4677-afb6-c9edde8fdb15", "VAT Detailed");
			this.VATDetailedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.VATDetailedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 72, true);
			this.VATDetailedCheckBox.Name = "VATDetailedCheckBox";
			this.VATDetailedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.VATDetailedCheckBox.TabIndex = 7;
			// 
			// ChartOfAccountsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ChartOfAccountsCheckBox, "ChartOfAccounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).ChartOfAccounts)));
			this.ChartOfAccountsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("00a3cf58-1c4f-4d55-8ad0-9dc213aab8d7", "Chart of Accounts ");
			this.ChartOfAccountsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChartOfAccountsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 19, true);
			this.ChartOfAccountsCheckBox.Name = "ChartOfAccountsCheckBox";
			this.ChartOfAccountsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.ChartOfAccountsCheckBox.TabIndex = 1;
			// 
			// ProfitAndLossCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ProfitAndLossCheckBox, "ProfitAndLoss");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).ProfitAndLoss)));
			this.ProfitAndLossCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a66b19c0-97e4-41fb-80a9-3bdab316d78a", "Profit and Loss");
			this.ProfitAndLossCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProfitAndLossCheckBox, false);
			this.ProfitAndLossCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 46, true);
			this.ProfitAndLossCheckBox.Name = "ProfitAndLossCheckBox";
			this.ProfitAndLossCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.ProfitAndLossCheckBox.TabIndex = 5;
			// 
			// AccountingVouchersCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AccountingVouchersCheckBox, "AccountingVouchers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).AccountingVouchers)));
			this.AccountingVouchersCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f00b5f6e-b6c1-45ca-90bf-38128fde0b73", "Accounting Vouchers");
			this.AccountingVouchersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AccountingVouchersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 46, true);
			this.AccountingVouchersCheckBox.Name = "AccountingVouchersCheckBox";
			this.AccountingVouchersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.AccountingVouchersCheckBox.TabIndex = 3;
			// 
			// TrialBalanceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrialBalanceCheckBox, "TrialBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).TrialBalance)));
			this.TrialBalanceCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("45f510c1-ffca-47e5-b0e0-22758c9e9dce", "Trial Balances", "Export GL Account Balances And Movements");
			this.TrialBalanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrialBalanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 46, true);
			this.TrialBalanceCheckBox.Name = "TrialBalanceCheckBox";
			this.TrialBalanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.TrialBalanceCheckBox.TabIndex = 4;
			// 
			// BalanceSheetCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BalanceSheetCheckBox, "BalanceSheet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).BalanceSheet)));
			this.BalanceSheetCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f31b77f7-ee4f-4cdd-9a01-2c59a7320990", "Balance Sheet");
			this.BalanceSheetCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BalanceSheetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 72, true);
			this.BalanceSheetCheckBox.Name = "BalanceSheetCheckBox";
			this.BalanceSheetCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.BalanceSheetCheckBox.TabIndex = 6;
			// 
			// SupplementaryAccountsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SupplementaryAccountsCheckBox, "SupplementaryAccounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).SupplementaryAccounts)));
			this.SupplementaryAccountsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("53dac8c9-e75c-4d2d-8780-0997ddf0b96e", "Supplementary Accounts");
			this.SupplementaryAccountsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SupplementaryAccountsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 19, true);
			this.SupplementaryAccountsCheckBox.Name = "SupplementaryAccountsCheckBox";
			this.SupplementaryAccountsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.SupplementaryAccountsCheckBox.TabIndex = 2;
			// 
			// AccountBookCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AccountBookCheckBox, "AccountBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChinaStandard2004DataInterfaceWrapper)(null)).AccountBook)));
			this.AccountBookCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8fe06c28-dbc0-4d72-888c-5ec9b75b2e77", "Account Book");
			this.AccountBookCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AccountBookCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 19, true);
			this.AccountBookCheckBox.Name = "AccountBookCheckBox";
			this.AccountBookCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.AccountBookCheckBox.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32dff566-1402-4575-b44c-6ddbc420744d", "Export File Format");
			this.zGroupBox2.Controls.Add(this.ExportTXTRadioButton);
			this.zGroupBox2.Controls.Add(this.ExportXMLRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 198, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 51, true);
			this.zGroupBox2.TabIndex = 3;
			this.zGroupBox2.TabStop = false;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0c90acbe-208d-4988-a55a-d63070cb9c0c", "Branch");
			this.zGroupBox3.Controls.Add(this.BranchGuidFindBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox3, false);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 138, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 56, true);
			this.zGroupBox3.TabIndex = 1;
			this.zGroupBox3.TabStop = false;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChinaStandard2004DataInterfaceWrapper)(null)).Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e429c581-db88-44b3-9bab-1085eec80676", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 19, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.BranchGuidFindBox.TabIndex = 11;
			// 
			// CN2004DataInterfaceGUI
			// 
			this.AcceptButton = this.GenerateButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2AD27B48-0C8D-4EA0-90EE-9403ACFCFDB5", "China Standard GB-T 19851-2004", "China Standard GB-T 19851-2004 Data interface");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 558, true);
			this.Controls.Add(this.zGroupBox3);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.DateGroupBox);
			this.Controls.Add(this.InterfaceFileGroupBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.LogTextBox);
			this.Controls.Add(this.GenerateButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.DataTransfer";
			this.DataSourceType = typeof(ChinaStandard2004DataInterfaceWrapper);
			this.DataSourceTypeName = "Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004.Chin" +
	"aStandard2004DataInterfaceWrapper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CN2004DataInterfaceGUI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "China Standard GB-T 19851-2004";
			this.Controls.SetChildIndex(this.GenerateButton, 0);
			this.Controls.SetChildIndex(this.LogTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.InterfaceFileGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DateGroupBox, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.zGroupBox3, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InterfaceFileGroupBox.ResumeLayout(false);
			this.InterfaceFileGroupBox.PerformLayout();
			this.DateGroupBox.ResumeLayout(false);
			this.DateGroupBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox3.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		#region Controls

		protected ZGroupBox InterfaceFileGroupBox;
#if DEBUG
		public
#endif
 ZButton DirectoryPopUpButton;
		private ZTextBox DirectoryTextBox;
		private ZPeriodEdit FromPeriodEdit;
#if DEBUG
		public
#endif
 ZButton CloseButton;
#if DEBUG
		public
#endif
 ZButton GenerateButton;
		private ZTextBox LogTextBox;
		protected ZGroupBox DateGroupBox;
		private ZRadioButton ExportXMLRadioButton;
		protected ZGroupBox zGroupBox1;
		private ZCheckBox ProfitAndLossCheckBox;
		private ZCheckBox AccountingVouchersCheckBox;
		private ZCheckBox TrialBalanceCheckBox;
		private ZCheckBox BalanceSheetCheckBox;
		private ZCheckBox SupplementaryAccountsCheckBox;
		private ZCheckBox AccountBookCheckBox;
		private ZRadioButton ExportTXTRadioButton;
		private ZCheckBox ChartOfAccountsCheckBox;
		protected ZGroupBox zGroupBox2;
		private ZCheckBox VATDetailedCheckBox;
		private ZCheckBox AssetProvisionCheckBox;
		private ZCheckBox PNLAppropriationCheckBox;
		private ZCheckBox EquityMovementCheckBox;
		protected ZGroupBox zGroupBox3;
		protected ZGuidFindBox BranchGuidFindBox;
		private ZCheckBox CashFlowStatementCheckBox;
		private ZTextBox DeliveryToTextBox;

		private readonly System.ComponentModel.Container components = null;

		#endregion

	}
}
