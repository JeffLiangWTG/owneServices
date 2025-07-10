namespace Enterprise.Customs.CA.GUI
{
	partial class LVXUserControl
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
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LVSSubHeaderAllDetailsUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeaderAllDetailsUserControl();
			this.OtherDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CAMergeByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PeriodMonthEdit = new Enterprise.Customs.Module.MonthEdit();
			this.PeriodYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.K84GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.K84StatementDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.K84AccountingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransactionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransactionNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckDigitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequentialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LVSLinesUserControl = new Enterprise.Customs.CA.GUI.LVSLinesUserControl();
			this.JE_TransportModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormattedTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.LVSSubHeaderAllDetailsUserControl.SuspendLayout();
			this.OtherDetailsGroupBox.SuspendLayout();
			this.CAMergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.K84GroupBox.SuspendLayout();
			this.K84StatementDateDateEdit.SuspendLayout();
			this.K84AccountingDateDateEdit.SuspendLayout();
			this.TransactionDetailsGroupBox.SuspendLayout();
			this.TransactionNumberPanel.SuspendLayout();
			this.LVSLinesUserControl.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.LVSSubHeaderAllDetailsUserControl);
			this.SplitContainer.Panel1.Controls.Add(this.OtherDetailsGroupBox);
			this.SplitContainer.Panel1.Controls.Add(this.K84GroupBox);
			this.SplitContainer.Panel1.Controls.Add(this.TransactionDetailsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1290, 631, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(221);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LVSLinesUserControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(320);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(280);
			this.SplitContainer.SplitterWidth = 2;
			this.SplitContainer.TabIndex = 21;
			// 
			// LVSSubHeaderAllDetailsUserControl
			// 
			this.LVSSubHeaderAllDetailsUserControl.AllowDrop = true;
			this.LVSSubHeaderAllDetailsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LVSSubHeaderAllDetailsUserControl, "LVXInvoiceHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).LVXInvoiceHeader)));
			this.LVSSubHeaderAllDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 42, true);
			this.LVSSubHeaderAllDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 232, true);
			this.LVSSubHeaderAllDetailsUserControl.Name = "LVSSubHeaderAllDetailsUserControl";
			this.LVSSubHeaderAllDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 236, true);
			this.LVSSubHeaderAllDetailsUserControl.TabIndex = 3;
			// 
			// OtherDetailsGroupBox
			// 
			this.OtherDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.OtherDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|9e6db28d-d951-463d-a8f5-030ba7cf103b", "Header Details");
			this.OtherDetailsGroupBox.Controls.Add(this.JE_TransportModeBoundDropDownEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.CAMergeByDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodMonthEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodYearEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodLabel);
			this.OtherDetailsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.OtherDetailsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.OtherDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 42, true);
			this.OtherDetailsGroupBox.Name = "OtherDetailsGroupBox";
			this.OtherDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 236, true);
			this.OtherDetailsGroupBox.TabIndex = 2;
			this.OtherDetailsGroupBox.TabStop = false;
			// 
			// CAMergeByDropEdit
			// 
			this.CAMergeByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CAMergeByDropEdit, "CA_MergeBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_MergeBy)));
			this.CAMergeByDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|5e2903ec-c6b5-42d2-90ef-ce0127443ba6", "Entry Merge By");
			this.CAMergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 111, true);
			this.CAMergeByDropEdit.Name = "CAMergeByDropEdit";
			this.CAMergeByDropEdit.PreBoundMaxLength = 3;
			this.CAMergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.CAMergeByDropEdit.TabIndex = 4;
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodMonthEdit, "JE_PeriodMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PeriodMonth)));
			this.PeriodMonthEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|cb539892-58a4-4cdd-b398-6704d78192da", "Period");
			this.PeriodMonthEdit.DecimalPlaces = 2;
			this.PeriodMonthEdit.IsCalculatorEnabled = false;
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 33, true);
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.ShowGroupSeparators = false;
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PeriodMonthEdit.TabIndex = 0;
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodYearEdit, "JE_PeriodYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PeriodYear)));
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodYearEdit, false);
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 33, true);
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PeriodYearEdit.TabIndex = 1;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.AutoSize = true;
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 36, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.PeriodLabel.TabIndex = 3;
			this.PeriodLabel.Text = "/";
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BrokerCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 85, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.BrokerCodeFindBox.TabIndex = 3;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BranchGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GB)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 59, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// K84GroupBox
			// 
			this.K84GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.K84GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|40f82117-281a-4ed5-9d7d-3e5a42110d5a", "K84 Dates");
			this.K84GroupBox.Controls.Add(this.K84StatementDateDateEdit);
			this.K84GroupBox.Controls.Add(this.K84AccountingDateDateEdit);
			this.K84GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(841, 1, true);
			this.K84GroupBox.Name = "K84GroupBox";
			this.K84GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 41, true);
			this.K84GroupBox.TabIndex = 1;
			this.K84GroupBox.TabStop = false;
			// 
			// K84StatementDateDateEdit
			// 
			this.K84StatementDateDateEdit.AllowDrop = true;
			this.K84StatementDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.K84StatementDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.K84StatementDateDateEdit, "CA_K84StatementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_K84StatementDate)));
			this.K84StatementDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 16, true);
			this.K84StatementDateDateEdit.Name = "K84StatementDateDateEdit";
			this.K84StatementDateDateEdit.TabIndex = 1;
			// 
			// K84AccountingDateDateEdit
			// 
			this.K84AccountingDateDateEdit.AllowDrop = true;
			this.K84AccountingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.K84AccountingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.K84AccountingDateDateEdit, "CA_K84AccountingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_K84AccountingDate)));
			this.K84AccountingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 16, true);
			this.K84AccountingDateDateEdit.Name = "K84AccountingDateDateEdit";
			this.K84AccountingDateDateEdit.TabIndex = 0;
			// 
			// TransactionDetailsGroupBox
			// 
			this.TransactionDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|d97218f2-24ab-4192-9021-1602a729dc82", "Transaction Details");
			this.TransactionDetailsGroupBox.Controls.Add(this.TransactionNumberPanel);
			this.TransactionDetailsGroupBox.Controls.Add(this.EditButton);
			this.TransactionDetailsGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.TransactionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.TransactionDetailsGroupBox.Name = "TransactionDetailsGroupBox";
			this.TransactionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 41, true);
			this.TransactionDetailsGroupBox.TabIndex = 0;
			this.TransactionDetailsGroupBox.TabStop = false;
			// 
			// TransactionNumberPanel
			// 
			this.TransactionNumberPanel.Controls.Add(this.FormattedTransactionNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.CheckDigitTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SequentialNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SecurityCodeTextBox);
			this.TransactionNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 13, true);
			this.TransactionNumberPanel.Name = "TransactionNumberPanel";
			this.TransactionNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 24, true);
			this.TransactionNumberPanel.TabIndex = 0;
			// 
			// FormattedTransactionNumberTextBox
			// 
			this.FormattedTransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FormattedTransactionNumberTextBox, "TransactionNumber.FormattedTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.FormattedTransactionNumber)));
			this.FormattedTransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.FormattedTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 4, true);
			this.FormattedTransactionNumberTextBox.Name = "FormattedTransactionNumberTextBox";
			this.FormattedTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FormattedTransactionNumberTextBox.TabIndex = 0;
			// 
			// CheckDigitTextBox
			// 
			this.CheckDigitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CheckDigitTextBox, "TransactionNumber.CheckDigit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.CheckDigit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CheckDigitTextBox, false);
			this.CheckDigitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 4, true);
			this.CheckDigitTextBox.Name = "CheckDigitTextBox";
			this.CheckDigitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.CheckDigitTextBox.TabIndex = 2;
			this.CheckDigitTextBox.Text = "1";
			this.CheckDigitTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SequentialNumberTextBox
			// 
			this.SequentialNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequentialNumberTextBox, "TransactionNumber.SequentialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.SequentialNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SequentialNumberTextBox, false);
			this.SequentialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 4, true);
			this.SequentialNumberTextBox.Name = "SequentialNumberTextBox";
			this.SequentialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.SequentialNumberTextBox.TabIndex = 1;
			this.SequentialNumberTextBox.Text = "12345678";
			this.SequentialNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecurityCodeTextBox
			// 
			this.SecurityCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityCodeTextBox, "TransactionNumber.AccountSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.AccountSecurityCode)));
			this.SecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 4, true);
			this.SecurityCodeTextBox.Name = "SecurityCodeTextBox";
			this.SecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.SecurityCodeTextBox.TabIndex = 0;
			this.SecurityCodeTextBox.Text = "12345";
			this.SecurityCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EditButton
			// 
			this.EditButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c127e5f2-3775-4e8f-9381-7cda96a1b686", "Edit");
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 14, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.EditButton.TabIndex = 1;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "JE_MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_MessageStatusDescription)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVXUserControl|d925325d-59d8-4911-9085-f70ad6538313", "Last Message");
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 16, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 20, true);
			this.MessageStatusTextBox.TabIndex = 2;
			// 
			// LVSLinesUserControl
			// 
			this.LVSLinesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSLinesUserControl, "LVXInvoiceHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).LVXInvoiceHeader)));
			this.LVSLinesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSLinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSLinesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 320, true);
			this.LVSLinesUserControl.Name = "LVSLinesUserControl";
			this.LVSLinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1290, 349, true);
			this.LVSLinesUserControl.TabIndex = 0;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_TransportModeBoundDropDownEdit, "JE_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Lookups.TransportTypeList)));
			this.JE_TransportModeBoundDropDownEdit.BindToList = "Lookups.TransportTypeList";
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 137, true);
			this.JE_TransportModeBoundDropDownEdit.Name = "JE_TransportModeBoundDropDownEdit";
			this.JE_TransportModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 5;
			// 
			// LVXUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1290, 631, true);
			this.Name = "LVXUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1290, 631, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.LVSSubHeaderAllDetailsUserControl.ResumeLayout(true);
			this.LVSSubHeaderAllDetailsUserControl.PerformLayout();
			this.OtherDetailsGroupBox.ResumeLayout(false);
			this.OtherDetailsGroupBox.PerformLayout();
			this.CAMergeByDropEdit.ResumeLayout(true);
			this.CAMergeByDropEdit.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.K84GroupBox.ResumeLayout(false);
			this.K84GroupBox.PerformLayout();
			this.K84StatementDateDateEdit.ResumeLayout(true);
			this.K84StatementDateDateEdit.PerformLayout();
			this.K84AccountingDateDateEdit.ResumeLayout(true);
			this.K84AccountingDateDateEdit.PerformLayout();
			this.TransactionDetailsGroupBox.ResumeLayout(false);
			this.TransactionDetailsGroupBox.PerformLayout();
			this.TransactionNumberPanel.ResumeLayout(false);
			this.TransactionNumberPanel.PerformLayout();
			this.LVSLinesUserControl.ResumeLayout(true);
			this.LVSLinesUserControl.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private LVSLinesUserControl LVSLinesUserControl;
		private LVSSubHeaderAllDetailsUserControl LVSSubHeaderAllDetailsUserControl;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox OtherDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private Enterprise.Customs.Module.MonthEdit PeriodMonthEdit;
		private ZArchitecture.ZLabel PeriodLabel;
		private ZArchitecture.GUI.ZYearEdit PeriodYearEdit;
		private ZArchitecture.GUI.ZGroupBox K84GroupBox;
		private ZArchitecture.GUI.ZDateEdit K84StatementDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit K84AccountingDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox TransactionDetailsGroupBox;
		private ZArchitecture.GUI.ZPanel TransactionNumberPanel;
		private ZArchitecture.ZTextBox CheckDigitTextBox;
		private ZArchitecture.ZTextBox SequentialNumberTextBox;
		private ZArchitecture.ZTextBox SecurityCodeTextBox;
		public ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.GUI.ZDropEdit CAMergeByDropEdit;
		private ZArchitecture.GUI.ZButton EditButton;
		private ZArchitecture.GUI.ZDropEdit JE_TransportModeBoundDropDownEdit;
		private Enterprise.ZArchitecture.ZTextBox FormattedTransactionNumberTextBox;
	}
}
