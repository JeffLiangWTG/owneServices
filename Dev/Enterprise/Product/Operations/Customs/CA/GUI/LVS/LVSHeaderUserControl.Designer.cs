using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.CA.GUI
{
	partial class LVSHeaderUserControl
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
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_OH_ImporterInfo.ValueChanged -= JE_OH_ImporterInfo_ValueChanged;
				JobDeclaration.CA_DeclarationExceptionInfo.ValueChanged -= CA_DeclarationExceptionInfo_ValueChanged;
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
			this.LVSLinesUserControl = new Enterprise.Customs.CA.GUI.LVSLinesUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LVSSubHeadersUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeadersUserControl();
			this.OtherDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PeriodYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.PeriodMonthEdit = new Enterprise.Customs.Module.MonthEdit();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LVSTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LVSCloseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfClearanceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_TransportModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProvinceofClearanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AllowOICCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExceptionDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.K84GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.K84StatementDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.K84AccountingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransactionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransactionNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FormattedTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CheckDigitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequentialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CADStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CADSubmittedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ScheduledB3DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LVSLinesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.LVSSubHeadersUserControl.SuspendLayout();
			this.OtherDetailsGroupBox.SuspendLayout();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.LVSTypeDropEdit.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.LVSCloseDateEdit.SuspendLayout();
			this.PortOfClearanceCodeFindBox.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.ProvinceofClearanceDropEdit.SuspendLayout();
			this.K84GroupBox.SuspendLayout();
			this.K84StatementDateDateEdit.SuspendLayout();
			this.K84AccountingDateDateEdit.SuspendLayout();
			this.TransactionDetailsGroupBox.SuspendLayout();
			this.TransactionNumberPanel.SuspendLayout();
			this.CADSubmittedDateEdit.SuspendLayout();
			this.ScheduledB3DateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// LVSLinesUserControl
			// 
			this.LVSLinesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSLinesUserControl, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)))));
			this.LVSLinesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSLinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSLinesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 320, true);
			this.LVSLinesUserControl.Name = "LVSLinesUserControl";
			this.LVSLinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 332, true);
			this.LVSLinesUserControl.TabIndex = 0;
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
			this.SplitContainer.Panel1.Controls.Add(this.LVSSubHeadersUserControl);
			this.SplitContainer.Panel1.Controls.Add(this.OtherDetailsGroupBox);
			this.SplitContainer.Panel1.Controls.Add(this.K84GroupBox);
			this.SplitContainer.Panel1.Controls.Add(this.TransactionDetailsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 710, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(374);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LVSLinesUserControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(334);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(374);
			this.SplitContainer.SplitterWidth = 2;
			this.SplitContainer.TabIndex = 21;
			// 
			// LVSSubHeadersUserControl
			// 
			this.LVSSubHeadersUserControl.AllowDrop = true;
			this.LVSSubHeadersUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LVSSubHeadersUserControl, ".");
			this.LVSSubHeadersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 38, true);
			this.LVSSubHeadersUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 230, true);
			this.LVSSubHeadersUserControl.Name = "LVSSubHeadersUserControl";
			this.LVSSubHeadersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 332, true);
			this.LVSSubHeadersUserControl.TabIndex = 3;
			// 
			// OtherDetailsGroupBox
			// 
			this.OtherDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.OtherDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|a3fd7615-8f44-4327-af87-a725842d1cfe", "Header Details");
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodLabel);
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodYearEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PeriodMonthEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PaymentPartyDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.LVSTypeDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.ImporterOrganisationControl);
			this.OtherDetailsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.OtherDetailsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.OtherDetailsGroupBox.Controls.Add(this.LVSCloseDateEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.PortOfClearanceCodeFindBox);
			this.OtherDetailsGroupBox.Controls.Add(this.JE_TransportModeBoundDropDownEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.ProvinceofClearanceDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.AllowOICCheckBox);
			this.OtherDetailsGroupBox.Controls.Add(this.ExceptionDescriptionLabel);
			this.OtherDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 38, true);
			this.OtherDetailsGroupBox.Name = "OtherDetailsGroupBox";
			this.OtherDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 332, true);
			this.OtherDetailsGroupBox.TabIndex = 2;
			this.OtherDetailsGroupBox.TabStop = false;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.AutoSize = true;
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 82, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.PeriodLabel.TabIndex = 3;
			this.PeriodLabel.Text = "/";
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodYearEdit, "JE_PeriodYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PeriodYear)));
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodYearEdit, false);
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 79, true);
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PeriodYearEdit.TabIndex = 4;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodMonthEdit, "JE_PeriodMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PeriodMonth)));
			this.PeriodMonthEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|9bbe62e7-884c-4580-bdfc-097058b32eb5", "Period");
			this.PeriodMonthEdit.DecimalPlaces = 2;
			this.PeriodMonthEdit.IsCalculatorEnabled = false;
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 79, true);
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.ShowGroupSeparators = false;
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PeriodMonthEdit.TabIndex = 2;
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.AllowDrop = true;
			this.PaymentPartyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PaymentMethod)));
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 154, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 3;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 7;
			// 
			// LVSTypeDropEdit
			// 
			this.LVSTypeDropEdit.AllowDrop = true;
			this.LVSTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LVSTypeDropEdit, "JE_MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_MessageSubType)));
			this.LVSTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|ab284046-1e2a-4190-a05f-9ac8e709b1f4", "LVS Type");
			this.LVSTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 54, true);
			this.LVSTypeDropEdit.Name = "LVSTypeDropEdit";
			this.LVSTypeDropEdit.PreBoundMaxLength = 3;
			this.LVSTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.LVSTypeDropEdit.TabIndex = 1;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.ImporterOrganisationControl.Captions = new string[] {
		"Importer"};
			this.ImporterOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 11, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.ImporterOrganisationControl.TabIndex = 0;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BrokerCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 129, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BrokerCodeFindBox.TabIndex = 6;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BranchGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GB)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 104, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BranchGuidFindBox.TabIndex = 5;
			// 
			// LVSCloseDateEdit
			// 
			this.LVSCloseDateEdit.AllowDrop = true;
			this.LVSCloseDateEdit.AutoCompleteMonthThreshold = 1;
			this.LVSCloseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LVSCloseDateEdit, "CA_LVSCloseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_LVSCloseDate)));
			this.LVSCloseDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|abab4410-34e5-4db5-9d79-cd79afe9712f", "LVS Close Date");
			this.LVSCloseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 204, true);
			this.LVSCloseDateEdit.Name = "LVSCloseDateEdit";
			this.LVSCloseDateEdit.TabIndex = 9;
			// 
			// PortOfClearanceCodeFindBox
			// 
			this.PortOfClearanceCodeFindBox.AllowDrop = true;
			this.PortOfClearanceCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PortOfClearanceCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.PortOfClearanceCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|3c20b3b6-1c59-416c-8a38-48059789582f", "Reporting Port");
			this.PortOfClearanceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 179, true);
			this.PortOfClearanceCodeFindBox.Name = "PortOfClearanceCodeFindBox";
			this.PortOfClearanceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.PortOfClearanceCodeFindBox.TabIndex = 8;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.AllowDrop = true;
			this.JE_TransportModeBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JE_TransportModeBoundDropDownEdit, "JE_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Lookups.TransportTypeList)));
			this.JE_TransportModeBoundDropDownEdit.BindToList = "Lookups.TransportTypeList";
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 257, true);
			this.JE_TransportModeBoundDropDownEdit.Name = "JE_TransportModeBoundDropDownEdit";
			this.JE_TransportModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 11;
			// 
			// ProvinceofClearanceDropEdit
			// 
			this.ProvinceofClearanceDropEdit.AllowDrop = true;
			this.ProvinceofClearanceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProvinceofClearanceDropEdit, "CA_ProvinceOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ProvinceOfClearance)));
			this.ProvinceofClearanceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|a31b1e36-f498-4803-8d87-d548f8ff4bc0", "Province of Clearance");
			this.ProvinceofClearanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 229, true);
			this.ProvinceofClearanceDropEdit.Name = "ProvinceofClearanceDropEdit";
			this.ProvinceofClearanceDropEdit.PreBoundMaxLength = 2;
			this.ProvinceofClearanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.ProvinceofClearanceDropEdit.TabIndex = 10;
			// 
			// AllowOICCheckBox
			// 
			this.AllowOICCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllowOICCheckBox, "CA_AllowOIC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_AllowOIC)));
			this.AllowOICCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllowOICCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 283, true);
			this.AllowOICCheckBox.Name = "AllowOICCheckBox";
			this.AllowOICCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.AllowOICCheckBox.TabIndex = 11;
			this.AllowOICCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|361692dc-6693-487a-bc9a-dc2cd2172865", "Allow OIC");
			this.AllowOICCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExceptionDescriptionLabel
			// 
			this.ExceptionDescriptionLabel.BackColor = System.Drawing.Color.LightSalmon;
			this.BindingSource.SetBindingMember(this.ExceptionDescriptionLabel, "CA_DeclarationExceptionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_DeclarationExceptionDescription)));
			this.ExceptionDescriptionLabel.IsFontBold = true;
			this.ExceptionDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 304, true);
			this.ExceptionDescriptionLabel.Name = "ExceptionDescriptionLabel";
			this.ExceptionDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 22, true);
			this.ExceptionDescriptionLabel.TabIndex = 12;
			// 
			// K84GroupBox
			// 
			this.K84GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.K84GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|60fce6e9-e8fb-42a0-bc69-f304d6e1ec73", "K84 Dates");
			this.K84GroupBox.Controls.Add(this.K84StatementDateDateEdit);
			this.K84GroupBox.Controls.Add(this.K84AccountingDateDateEdit);
			this.K84GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(837, 1, true);
			this.K84GroupBox.Name = "K84GroupBox";
			this.K84GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 37, true);
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
			this.K84StatementDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 12, true);
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
			this.K84AccountingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 12, true);
			this.K84AccountingDateDateEdit.Name = "K84AccountingDateDateEdit";
			this.K84AccountingDateDateEdit.TabIndex = 0;
			// 
			// TransactionDetailsGroupBox
			// 
			this.TransactionDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSHeaderUserControl|3825b579-9079-46ab-b67e-b0d5485d3d9a", "Transaction Details");
			this.TransactionDetailsGroupBox.Controls.Add(this.TransactionNumberPanel);
			this.TransactionDetailsGroupBox.Controls.Add(this.CADStatusTextBox);
			this.TransactionDetailsGroupBox.Controls.Add(this.CADSubmittedDateEdit);
			this.TransactionDetailsGroupBox.Controls.Add(this.ScheduledB3DateEdit);
			this.TransactionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.TransactionDetailsGroupBox.Name = "TransactionDetailsGroupBox";
			this.TransactionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 37, true);
			this.TransactionDetailsGroupBox.TabIndex = 0;
			this.TransactionDetailsGroupBox.TabStop = false;
			// 
			// TransactionNumberPanel
			// 
			this.TransactionNumberPanel.Controls.Add(this.FormattedTransactionNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.CheckDigitTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SequentialNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SecurityCodeTextBox);
			this.TransactionNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 11, true);
			this.TransactionNumberPanel.Name = "TransactionNumberPanel";
			this.TransactionNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 24, true);
			this.TransactionNumberPanel.TabIndex = 0;
			// 
			// FormattedTransactionNumberTextBox
			// 
			this.FormattedTransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FormattedTransactionNumberTextBox, "TransactionNumber.FormattedTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.FormattedTransactionNumber)));
			this.FormattedTransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.FormattedTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 2, true);
			this.FormattedTransactionNumberTextBox.Name = "FormattedTransactionNumberTextBox";
			this.FormattedTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.FormattedTransactionNumberTextBox.TabIndex = 0;
			// 
			// CheckDigitTextBox
			// 
			this.CheckDigitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CheckDigitTextBox, "TransactionNumber.CheckDigit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.CheckDigit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CheckDigitTextBox, false);
			this.CheckDigitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 2, true);
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
			this.SequentialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 2, true);
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
			this.SecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 2, true);
			this.SecurityCodeTextBox.Name = "SecurityCodeTextBox";
			this.SecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.SecurityCodeTextBox.TabIndex = 0;
			this.SecurityCodeTextBox.Text = "12345";
			this.SecurityCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CADStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CADStatusTextBox, "B3EntryHeader.EntryHeaderStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.EntryHeaderStatusDescription)));
			this.CADStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("51cdc2dc9-a0ec-4a2a-9fb2-3b0b3a053508", "CAD Status");
			this.CADStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CADStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 12, true);
			this.CADStatusTextBox.Name = "CADStatusTextBox";
			this.CADStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CADStatusTextBox.TabIndex = 2;
			// 
			// CADSubmittedDateEdit
			// 
			this.CADSubmittedDateEdit.AllowDrop = true;
			this.CADSubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CADSubmittedDateEdit, "B3EntryHeader.CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.CH_EntrySubmittedDate)));
			this.CADSubmittedDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("906e9094-4f09-47b2-af93-476808b60386", "CAD Submitted Time");
			this.CADSubmittedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CADSubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 12, true);
			this.CADSubmittedDateEdit.Name = "CADSubmittedDateEdit";
			this.CADSubmittedDateEdit.TabIndex = 1;
			// 
			// ScheduledB3DateEdit
			// 
			this.ScheduledB3DateEdit.AllowDrop = true;
			this.ScheduledB3DateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ScheduledB3DateEdit, "ScheduledB3SendingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ScheduledB3SendingDate)));
			this.ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("554C907B-BC8E-4834-A112-AA6AC9D6153E", "Entry Scheduled Time", "Scheduled Sending Time For Entry Message", "");
			this.ScheduledB3DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledB3DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(734, 12, true);
			this.ScheduledB3DateEdit.Name = "ScheduledB3DateEdit";
			this.ScheduledB3DateEdit.TabIndex = 3;
			// 
			// LVSHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 710, true);
			this.Name = "LVSHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 710, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LVSLinesUserControl.ResumeLayout(true);
			this.LVSLinesUserControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.LVSSubHeadersUserControl.ResumeLayout(true);
			this.LVSSubHeadersUserControl.PerformLayout();
			this.OtherDetailsGroupBox.ResumeLayout(false);
			this.OtherDetailsGroupBox.PerformLayout();
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.LVSTypeDropEdit.ResumeLayout(true);
			this.LVSTypeDropEdit.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.LVSCloseDateEdit.ResumeLayout(true);
			this.LVSCloseDateEdit.PerformLayout();
			this.PortOfClearanceCodeFindBox.ResumeLayout(true);
			this.PortOfClearanceCodeFindBox.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.ProvinceofClearanceDropEdit.ResumeLayout(true);
			this.ProvinceofClearanceDropEdit.PerformLayout();
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
			this.CADSubmittedDateEdit.ResumeLayout(true);
			this.CADSubmittedDateEdit.PerformLayout();
			this.ScheduledB3DateEdit.ResumeLayout(true);
			this.ScheduledB3DateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		#endregion

		private LVSLinesUserControl LVSLinesUserControl;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private LVSSubHeadersUserControl LVSSubHeadersUserControl;
		private ZArchitecture.GUI.ZGroupBox OtherDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		private ZArchitecture.GUI.ZDropEdit LVSTypeDropEdit;
		private Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControl;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private ZCodeFindBox PortOfClearanceCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit LVSCloseDateEdit;
		private ZArchitecture.GUI.ZGroupBox K84GroupBox;
		public ZArchitecture.GUI.ZDateEdit K84StatementDateDateEdit;
		public ZArchitecture.GUI.ZDateEdit K84AccountingDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox TransactionDetailsGroupBox;
		internal ZArchitecture.GUI.ZPanel TransactionNumberPanel;
		private ZArchitecture.ZTextBox CheckDigitTextBox;
		private ZArchitecture.ZTextBox SequentialNumberTextBox;
		private ZArchitecture.ZTextBox SecurityCodeTextBox;
		public ZArchitecture.ZTextBox CADStatusTextBox;
		private Enterprise.Customs.Module.MonthEdit PeriodMonthEdit;
		private ZArchitecture.ZLabel PeriodLabel;
		private ZArchitecture.GUI.ZYearEdit PeriodYearEdit;
		private ZDateEdit ScheduledB3DateEdit;
		private ZArchitecture.GUI.ZDropEdit ProvinceofClearanceDropEdit;
		private ZCheckBox AllowOICCheckBox;
		private Enterprise.ZArchitecture.ZLabel ExceptionDescriptionLabel;
		private ZDropEdit JE_TransportModeBoundDropDownEdit;
		private Enterprise.ZArchitecture.ZTextBox FormattedTransactionNumberTextBox;
		private ZDateEdit CADSubmittedDateEdit;
	}
}
