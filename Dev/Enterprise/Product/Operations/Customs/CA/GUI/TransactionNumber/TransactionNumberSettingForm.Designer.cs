namespace Enterprise.Customs.CA.GUI
{
	partial class TransactionNumberSettingForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TransactionNumberGroupsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ActiveTransactionNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RangeParametersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.MinNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentMinNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentMaxNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NextNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentNextNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AccountSecurityNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransactionNumberSettingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InactiveNumberCombinationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.ArrowsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MoveToAvailableButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveToExistingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionNumberGroupsSplitContainer)).BeginInit();
			this.TransactionNumberGroupsSplitContainer.Panel1.SuspendLayout();
			this.TransactionNumberGroupsSplitContainer.Panel2.SuspendLayout();
			this.TransactionNumberGroupsSplitContainer.SuspendLayout();
			this.ActiveTransactionNumbersGroupBox.SuspendLayout();
			this.RangeParametersPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionNumberSettingGrid)).BeginInit();
			this.TransactionNumberSettingGrid.SuspendLayout();
			this.InactiveNumberCombinationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.ArrowsPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 635, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.TransactionNumberSettingBO);
			// 
			// TransactionNumberGroupsSplitContainer
			// 
			this.TransactionNumberGroupsSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionNumberGroupsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TransactionNumberGroupsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionNumberGroupsSplitContainer.Name = "TransactionNumberGroupsSplitContainer";
			// 
			// TransactionNumberGroupsSplitContainer.Panel1
			// 
			this.TransactionNumberGroupsSplitContainer.Panel1.Controls.Add(this.ActiveTransactionNumbersGroupBox);
			// 
			// TransactionNumberGroupsSplitContainer.Panel2
			// 
			this.TransactionNumberGroupsSplitContainer.Panel2.Controls.Add(this.InactiveNumberCombinationsGroupBox);
			this.TransactionNumberGroupsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 603, true);
			this.TransactionNumberGroupsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(933);
			this.TransactionNumberGroupsSplitContainer.TabIndex = 5;
			// 
			// ActiveTransactionNumbersGroupBox
			// 
			this.ActiveTransactionNumbersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("95cf4c03-5476-4df5-8c3e-02b366df1f35", "Active Transaction Numbers");
			this.ActiveTransactionNumbersGroupBox.Controls.Add(this.RangeParametersPanel);
			this.ActiveTransactionNumbersGroupBox.Controls.Add(this.TransactionNumberSettingGrid);
			this.ActiveTransactionNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ActiveTransactionNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ActiveTransactionNumbersGroupBox.Name = "ActiveTransactionNumbersGroupBox";
			this.ActiveTransactionNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 603, true);
			this.ActiveTransactionNumbersGroupBox.TabIndex = 4;
			this.ActiveTransactionNumbersGroupBox.TabStop = false;
			// 
			// RangeParametersPanel
			// 
			this.RangeParametersPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RangeParametersPanel.Controls.Add(this.label2);
			this.RangeParametersPanel.Controls.Add(this.label1);
			this.RangeParametersPanel.Controls.Add(this.MinNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.CurrentMinNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.CurrentMaxNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.NextNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.MaxNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.CurrentNextNumberCalcEdit);
			this.RangeParametersPanel.Controls.Add(this.AccountSecurityNumberTextBox);
			this.RangeParametersPanel.Controls.Add(this.BranchCodeTextBox);
			this.RangeParametersPanel.Controls.Add(this.DeclarationTypeTextBox);
			this.RangeParametersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 471, true);
			this.RangeParametersPanel.Name = "RangeParametersPanel";
			this.RangeParametersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 128, true);
			this.RangeParametersPanel.TabIndex = 7;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.label2.IsFontBold = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 38, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.label2.TabIndex = 15;
			this.label2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("57c001d1-2f44-4686-93a4-6ad5985ac837", "New");
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.label1.IsFontBold = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 38, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.label1.TabIndex = 14;
			this.label1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2ab6b9f-7941-43af-9760-b28e923394e4", "Current");
			// 
			// MinNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinNumberCalcEdit, "ExistingTransactionNumberSettingCollection.MinNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).MinNumber)));
			this.MinNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b479f740-f784-4cfb-b547-03baba090e2d", "Min. Number");
			this.MinNumberCalcEdit.DecimalPlaces = 0;
			this.MinNumberCalcEdit.Decimals = 0;
			this.MinNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 53, true);
			this.MinNumberCalcEdit.Name = "MinNumberCalcEdit";
			this.MinNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.MinNumberCalcEdit.TabIndex = 11;
			this.MinNumberCalcEdit.Text = "0";
			this.MinNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentMinNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CurrentMinNumberCalcEdit, "ExistingTransactionNumberSettingCollection.CurrentMinNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentMinNumber)));
			this.CurrentMinNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("877e6c16-45da-4a65-a0dc-069c1d251e6f", "Min. Number");
			this.CurrentMinNumberCalcEdit.DecimalPlaces = 0;
			this.CurrentMinNumberCalcEdit.Decimals = 0;
			this.CurrentMinNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 53, true);
			this.CurrentMinNumberCalcEdit.Name = "CurrentMinNumberCalcEdit";
			this.CurrentMinNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CurrentMinNumberCalcEdit.TabIndex = 8;
			this.CurrentMinNumberCalcEdit.Text = "0";
			this.CurrentMinNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentMaxNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CurrentMaxNumberCalcEdit, "ExistingTransactionNumberSettingCollection.CurrentMaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentMaxNumber)));
			this.CurrentMaxNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b5dc335a-0da8-4fbc-82f2-50064b950db2", "Max. Number");
			this.CurrentMaxNumberCalcEdit.DecimalPlaces = 0;
			this.CurrentMaxNumberCalcEdit.Decimals = 0;
			this.CurrentMaxNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 96, true);
			this.CurrentMaxNumberCalcEdit.Name = "CurrentMaxNumberCalcEdit";
			this.CurrentMaxNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CurrentMaxNumberCalcEdit.TabIndex = 10;
			this.CurrentMaxNumberCalcEdit.Text = "0";
			this.CurrentMaxNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NextNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NextNumberCalcEdit, "ExistingTransactionNumberSettingCollection.NextNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).NextNumber)));
			this.NextNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7e821ad2-5d00-4dbd-837f-2f64d2f82485", "Next Number");
			this.NextNumberCalcEdit.DecimalPlaces = 0;
			this.NextNumberCalcEdit.Decimals = 0;
			this.NextNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 75, true);
			this.NextNumberCalcEdit.Name = "NextNumberCalcEdit";
			this.NextNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.NextNumberCalcEdit.TabIndex = 12;
			this.NextNumberCalcEdit.Text = "0";
			this.NextNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxNumberCalcEdit, "ExistingTransactionNumberSettingCollection.MaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).MaxNumber)));
			this.MaxNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5e31bd9d-e746-45c7-a8a0-f57981997573", "Max. Number");
			this.MaxNumberCalcEdit.DecimalPlaces = 0;
			this.MaxNumberCalcEdit.Decimals = 0;
			this.MaxNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 96, true);
			this.MaxNumberCalcEdit.Name = "MaxNumberCalcEdit";
			this.MaxNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.MaxNumberCalcEdit.TabIndex = 13;
			this.MaxNumberCalcEdit.Text = "0";
			this.MaxNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentNextNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CurrentNextNumberCalcEdit, "ExistingTransactionNumberSettingCollection.CurrentNextNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentNextNumber)));
			this.CurrentNextNumberCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("adc3c1d1-e083-46f9-abc9-10b9f54c0c5c", "Next Number");
			this.CurrentNextNumberCalcEdit.DecimalPlaces = 0;
			this.CurrentNextNumberCalcEdit.Decimals = 0;
			this.CurrentNextNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 75, true);
			this.CurrentNextNumberCalcEdit.Name = "CurrentNextNumberCalcEdit";
			this.CurrentNextNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CurrentNextNumberCalcEdit.TabIndex = 9;
			this.CurrentNextNumberCalcEdit.Text = "0";
			this.CurrentNextNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccountSecurityNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountSecurityNumberTextBox, "ExistingTransactionNumberSettingCollection.AccountSecurityCodeForUI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).AccountSecurityCodeForUI)));
			this.AccountSecurityNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bfc3aa6f-3c2d-4dea-9910-0f937b94dddd", "ASEC Number");
			this.AccountSecurityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
			this.AccountSecurityNumberTextBox.Name = "AccountSecurityNumberTextBox";
			this.AccountSecurityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.AccountSecurityNumberTextBox.TabIndex = 0;
			this.AccountSecurityNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BranchCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BranchCodeTextBox, "ExistingTransactionNumberSettingCollection.BranchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).BranchCode)));
			this.BranchCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("71CAE703-CB12-4763-B690-6C6BCEA3082A", "Branch Code");
			this.BranchCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 12, true);
			this.BranchCodeTextBox.Name = "BranchCodeTextBox";
			this.BranchCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.BranchCodeTextBox.TabIndex = 3;
			this.BranchCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DeclarationTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationTypeTextBox, "ExistingTransactionNumberSettingCollection.DeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).DeclarationType)));
			this.DeclarationTypeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("32453BAF-0F8D-48C7-B888-381DF4D6725C", "Dec. Type");
			this.DeclarationTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 12, true);
			this.DeclarationTypeTextBox.Name = "DeclarationTypeTextBox";
			this.DeclarationTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DeclarationTypeTextBox.TabIndex = 2;
			this.DeclarationTypeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TransactionNumberSettingGrid
			// 
			this.TransactionNumberSettingGrid.AllowNavigation = false;
			this.TransactionNumberSettingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionNumberSettingGrid, "ExistingTransactionNumberSettingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).AccountSecurityCodeForUI)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).DeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).BranchCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentMinNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentNextNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).CurrentMaxNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).MinNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).NextNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).ExistingTransactionNumberSettingCollection)).SyncRoot)).MaxNumber)));
			this.TransactionNumberSettingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("be672126-2077-48ef-ab65-c4479111de89", "Owner", "Owner", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Owner";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ec32b61b-882b-43a4-ac2b-668e2b18b2f8", "ASEC No.", "ASEC Number", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "AccountSecurityCodeForUI";
			zTextBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("252BA064-F9E9-4819-BD29-32E1486D380C", "Type", "Declaration Type", "");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "DeclarationType";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("79CAF301-7126-4F18-BE7E-B1DCF817C4A9", "Branch", "Branch Code", "");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "BranchCode";
			zTextBoxColumnStyleInfo4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("80b3e8a8-f877-4902-b342-e63490b7a42b", "Curr Min No.", "Current Min Number", "");
			zCalcEditColumnStyleInfo1.ColumnName = "CurrentMinNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3835f0cd-55ab-4fd7-993b-0051616f9ffe", "Curr Next No.", "Current Next Number", "");
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentNextNumber";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1c2d5eac-4f9c-4854-880d-ffeff6614048", "Curr Max No.", "Current Max Number", "");
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "CurrentMaxNumber";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8b287e60-27ed-43d3-a7ce-3278ad87cf8f", "Min No.", "Minimum Number", "");
			zCalcEditColumnStyleInfo4.ColumnName = "MinNumber";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b6afa669-ed1a-4508-834b-aec8ebce0c28", "Next No.", "Next Number", "");
			zCalcEditColumnStyleInfo5.ColumnName = "NextNumber";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2fa6ffc-31e8-41a7-9f7e-53c7d8e892e6", "Max No.", "Maximum Number", "");
			zCalcEditColumnStyleInfo6.ColumnName = "MaxNumber";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.TransactionNumberSettingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.TransactionNumberSettingGrid.GridId = "73810a09-1c49-487c-88da-1c42ed5a0a38";
			this.TransactionNumberSettingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionNumberSettingGrid.LayoutKey = "TransactionNumberSettingGrid";
			this.TransactionNumberSettingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.TransactionNumberSettingGrid.Name = "TransactionNumberSettingGrid";
			this.TransactionNumberSettingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 451, true);
			this.TransactionNumberSettingGrid.TabIndex = 4;
			// 
			// InactiveNumberCombinationsGroupBox
			// 
			this.InactiveNumberCombinationsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ded6414e-e20f-4fd6-b538-dd4fbfd7fcad", "Inactive Number Combinations");
			this.InactiveNumberCombinationsGroupBox.Controls.Add(this.zGrid1);
			this.InactiveNumberCombinationsGroupBox.Controls.Add(this.ArrowsPanel);
			this.InactiveNumberCombinationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InactiveNumberCombinationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InactiveNumberCombinationsGroupBox.Name = "InactiveNumberCombinationsGroupBox";
			this.InactiveNumberCombinationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 603, true);
			this.InactiveNumberCombinationsGroupBox.TabIndex = 6;
			this.InactiveNumberCombinationsGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "AvailableTransactionNumberSettingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).AvailableTransactionNumberSettingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).AvailableTransactionNumberSettingCollection)).SyncRoot)).Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).AvailableTransactionNumberSettingCollection)).SyncRoot)).AccountSecurityCodeForUI)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).AvailableTransactionNumberSettingCollection)).SyncRoot)).DeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TransactionNumberSetting)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TransactionNumberSettingBO)(null)).AvailableTransactionNumberSettingCollection)).SyncRoot)).BranchCode)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("be672126-2077-48ef-ab65-c4479111de89", "Owner", "Owner", "");
			zTextBoxColumnStyleInfo5.ColumnName = "Owner";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ec32b61b-882b-43a4-ac2b-668e2b18b2f8", "ASEC No.", "ASEC Number", "");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "AccountSecurityCodeForUI";
			zTextBoxColumnStyleInfo6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("252BA064-F9E9-4819-BD29-32E1486D380C", "Type", "Declaration Type", "");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "DeclarationType";
			zTextBoxColumnStyleInfo7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("79CAF301-7126-4F18-BE7E-B1DCF817C4A9", "Branch", "Branch Code", "");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "BranchCode";
			zTextBoxColumnStyleInfo8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zGrid1.GridId = "73810a09-1c49-487c-88da-1c42ed5a0a38";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "TransactionNumberSettingGrid";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 17, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 581, true);
			this.zGrid1.TabIndex = 7;
			// 
			// ArrowsPanel
			// 
			this.ArrowsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ArrowsPanel.Controls.Add(this.MoveToAvailableButton);
			this.ArrowsPanel.Controls.Add(this.MoveToExistingButton);
			this.ArrowsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.ArrowsPanel.Name = "ArrowsPanel";
			this.ArrowsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 581, true);
			this.ArrowsPanel.TabIndex = 6;
			// 
			// MoveToAvailableButton
			// 
			this.MoveToAvailableButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 257, true);
			this.MoveToAvailableButton.Name = "MoveToAvailableButton";
			this.MoveToAvailableButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveToAvailableButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.MoveToAvailableButton.TabIndex = 10;
			this.MoveToAvailableButton.Text = ">>";
			this.MoveToAvailableButton.ToolTipCaption = null;
			this.MoveToAvailableButton.UseVisualStyleBackColor = true;
			this.MoveToAvailableButton.Click += new System.EventHandler(this.MoveToAvailableButton_Click);
			// 
			// MoveToExistingButton
			// 
			this.MoveToExistingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 231, true);
			this.MoveToExistingButton.Name = "MoveToExistingButton";
			this.MoveToExistingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveToExistingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.MoveToExistingButton.TabIndex = 9;
			this.MoveToExistingButton.Text = "<<";
			this.MoveToExistingButton.ToolTipCaption = null;
			this.MoveToExistingButton.UseVisualStyleBackColor = true;
			this.MoveToExistingButton.Click += new System.EventHandler(this.MoveToExistingButton_Click);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(846, 607, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 25, true);
			this.PostingButtons.TabIndex = 0;
			// 
			// TransactionNumberSettingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 659, true);
			this.Controls.Add(this.PostingButtons);
			this.Controls.Add(this.TransactionNumberGroupsSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.TransactionNumberSettingBO);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 697, true);
			this.Name = "TransactionNumberSettingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cf4c3f0e-d3d6-4c1d-aa4b-9862714f23bf", "Transaction Number Setting Form");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TransactionNumberGroupsSplitContainer, 0);
			this.Controls.SetChildIndex(this.PostingButtons, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionNumberGroupsSplitContainer.Panel1.ResumeLayout(false);
			this.TransactionNumberGroupsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransactionNumberGroupsSplitContainer)).EndInit();
			this.TransactionNumberGroupsSplitContainer.ResumeLayout(false);
			this.TransactionNumberGroupsSplitContainer.PerformLayout();
			this.ActiveTransactionNumbersGroupBox.ResumeLayout(false);
			this.ActiveTransactionNumbersGroupBox.PerformLayout();
			this.RangeParametersPanel.ResumeLayout(false);
			this.RangeParametersPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionNumberSettingGrid)).EndInit();
			this.TransactionNumberSettingGrid.ResumeLayout(false);
			this.TransactionNumberSettingGrid.PerformLayout();
			this.InactiveNumberCombinationsGroupBox.ResumeLayout(false);
			this.InactiveNumberCombinationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ArrowsPanel.ResumeLayout(false);
			this.ArrowsPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZCalcEdit CurrentMaxNumberCalcEdit;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtons;
		protected internal ZArchitecture.ZCalcEdit NextNumberCalcEdit;
		protected internal ZArchitecture.ZCalcEdit MaxNumberCalcEdit;
		protected ZArchitecture.ZCalcEdit CurrentNextNumberCalcEdit;
		protected ZArchitecture.ZTextBox AccountSecurityNumberTextBox;
		protected ZArchitecture.ZTextBox BranchCodeTextBox;
		protected ZArchitecture.ZTextBox DeclarationTypeTextBox;
		private CargoWise.Windows.UI.KSplitContainer TransactionNumberGroupsSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ActiveTransactionNumbersGroupBox;
		protected ZArchitecture.ZGrid TransactionNumberSettingGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InactiveNumberCombinationsGroupBox;
		protected ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.GUI.ZPanel ArrowsPanel;
		protected internal ZArchitecture.GUI.ZButton MoveToAvailableButton;
		protected internal ZArchitecture.GUI.ZButton MoveToExistingButton;
		protected internal ZArchitecture.ZCalcEdit MinNumberCalcEdit;
		protected ZArchitecture.ZCalcEdit CurrentMinNumberCalcEdit;
		private ZArchitecture.GUI.ZPanel RangeParametersPanel;
		private Enterprise.ZArchitecture.ZLabel label2;
		private Enterprise.ZArchitecture.ZLabel label1;
	}
}
