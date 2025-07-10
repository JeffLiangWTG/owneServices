using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	partial class StatementHeaderUserControl
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
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.StatementNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.BranchDesignationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.StatementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.PeriodEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.EntryFillerCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ImporterCustomsIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CheckNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.ChargesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.EntriesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.StatusDropEdit.SuspendLayout();
            this.BranchDesignationDropEdit.SuspendLayout();
            this.ImporterFindBox.SuspendLayout();
            this.StatementTypeDropEdit.SuspendLayout();
            this.PeriodStartDateEdit.SuspendLayout();
            this.PeriodEndDateEdit.SuspendLayout();
            this.PaymentTypeDropEdit.SuspendLayout();
            this.DetailsGroupBox.SuspendLayout();
            this.TabControl.SuspendLayout();
            this.ChargesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
            this.ChargesGrid.SuspendLayout();
            this.EntriesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
            this.EntriesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader);
            // 
            // StatementNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.StatementNumberTextBox, "B2_StatementNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_StatementNumber)));
            this.StatementNumberTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("e0644aeb-6230-4e44-991a-0de1d9752c7f", "Statement Number");
            this.StatementNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 15, true);
            this.StatementNumberTextBox.Name = "StatementNumberTextBox";
            this.StatementNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.StatementNumberTextBox.TabIndex = 0;

			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CorrelationID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).CorrelationID)));
            this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("41CB223A-0D3F-43CF-A397-77917465A6AB", "Reference Number");
            this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 42, true);
            this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
            this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.ReferenceNumberTextBox.TabIndex = 1;

			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "EntryNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).EntryNumber)));
            this.EntryNumberTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("98CF1D0F-6BE7-45A6-990A-844DD7308E63", "Entry Number");
            this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 67, true);
            this.EntryNumberTextBox.Name = "EntryNumberTextBox";
            this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.EntryNumberTextBox.TabIndex = 2;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatusDropEdit, "B2_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_Status)));
            this.StatusDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("449387ff-a3ae-44e3-bdf1-0d5ddce74a6d", "Status");
            this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 93, true);
            this.StatusDropEdit.Name = "StatusDropEdit";
            this.StatusDropEdit.ShouldResizeByMaxLength = true;
            this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.StatusDropEdit.TabIndex = 3;
            // 
            // BranchDesignationDropEdit
            // 
            this.BranchDesignationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BranchDesignationDropEdit, "B2_BranchDesignation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_BranchDesignation)));
            this.BranchDesignationDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("fef047e8-4e22-426e-b15c-10e78ccda135", "Direction");
            this.BranchDesignationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 121, true);
            this.BranchDesignationDropEdit.Name = "BranchDesignationDropEdit";
            this.BranchDesignationDropEdit.ShouldResizeByMaxLength = true;
            this.BranchDesignationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.BranchDesignationDropEdit.TabIndex = 4;
            // 
            // ImporterFindBox
            // 
            this.ImporterFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImporterFindBox, "B2_OH_Importer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_OH_Importer)));
            this.ImporterFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("99fdbec0-c142-432d-b116-f91e06be13fc", "Delta G Party");
            this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 149, true);
            this.ImporterFindBox.Name = "ImporterFindBox";
            this.ImporterFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ImporterFindBox.ParentType = null;
            this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.ImporterFindBox.TabIndex = 5;
            // 
            // StatementTypeDropEdit
            // 
            this.StatementTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatementTypeDropEdit, "B2_StatementType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_StatementType)));
            this.StatementTypeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1ed151c1-8cb5-4ca3-ad91-aa9e8c384eda", "Reporting Frequency");
            this.StatementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 177, true);
            this.StatementTypeDropEdit.Name = "StatementTypeDropEdit";
            this.StatementTypeDropEdit.ShouldResizeByMaxLength = true;
            this.StatementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.StatementTypeDropEdit.TabIndex = 6;
            // 
            // PeriodStartDateEdit
            // 
            this.PeriodStartDateEdit.AllowDrop = true;
            this.PeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
            this.PeriodStartDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.PeriodStartDateEdit, "B2_PeriodStartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_PeriodStartDate)));
            this.PeriodStartDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("03436f95-7b5a-4944-9271-fb6495617974", "Period Start Date");
            this.PeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 205, true);
            this.PeriodStartDateEdit.Name = "PeriodStartDateEdit";
            this.PeriodStartDateEdit.TabIndex = 7;
            // 
            // PeriodEndDateEdit
            // 
            this.PeriodEndDateEdit.AllowDrop = true;
            this.PeriodEndDateEdit.AutoCompleteMonthThreshold = 1;
            this.PeriodEndDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.PeriodEndDateEdit, "B2_PeriodEndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_PeriodEndDate)));
            this.PeriodEndDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1833d836-2365-4c84-aa19-dc4634a7c0f0", "Period End Date");
            this.PeriodEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 233, true);
            this.PeriodEndDateEdit.Name = "PeriodEndDateEdit";
            this.PeriodEndDateEdit.TabIndex = 8;
            // 
            // EntryFillerCodeDropEdit
            // 
            this.BindingSource.SetBindingMember(this.EntryFillerCodeDropEdit, "B2_EntryFilerCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_EntryFilerCode)));
            this.EntryFillerCodeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("fbdeeff5-0db4-459b-8dd0-56b30c218b7e", "Delta Agreement Number");
            this.EntryFillerCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 260, true);
            this.EntryFillerCodeDropEdit.Name = "EntryFillerCodeDropEdit";
            this.EntryFillerCodeDropEdit.ShouldResizeByMaxLength = true;
            this.EntryFillerCodeDropEdit.ShowDescriptionBox = false;
            this.EntryFillerCodeDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.EntryFillerCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.EntryFillerCodeDropEdit.TabIndex = 9;
            // 
            // PaymentTypeDropEdit
            // 
            this.PaymentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "B2_PaymentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_PaymentType)));
            this.PaymentTypeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("11108631-a0d4-4513-9c2c-f446dafca969", "Method of Payment");
            this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 288, true);
            this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
            this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
            this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.PaymentTypeDropEdit.TabIndex = 10;
            // 
            // ImporterCustomsIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.ImporterCustomsIDTextBox, "B2_ImporterCustomsID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_ImporterCustomsID)));
            this.ImporterCustomsIDTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("f7e1ebf8-8f6d-4e49-8e3f-2462b3f0f820", "Operational Representative");
            this.ImporterCustomsIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 315, true);
            this.ImporterCustomsIDTextBox.Name = "ImporterCustomsIDTextBox";
            this.ImporterCustomsIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.ImporterCustomsIDTextBox.TabIndex = 11;
            // 
            // CheckNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.CheckNoTextBox, "B2_CheckNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).B2_CheckNo)));
            this.CheckNoTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("01f0b86c-7aac-48c1-99db-99670b2fb39c", "Deferral Account No.");
            this.CheckNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 342, true);
            this.CheckNoTextBox.Name = "CheckNoTextBox";
            this.CheckNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 31, true);
            this.CheckNoTextBox.TabIndex = 12;
            // 
            // DetailsGroupBox
            // 
            this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("f6770b60-3aae-4b6f-9bcf-cdaabfc797fd", "Liquidation Details");
            this.DetailsGroupBox.Controls.Add(this.CheckNoTextBox);
            this.DetailsGroupBox.Controls.Add(this.ImporterCustomsIDTextBox);
            this.DetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
            this.DetailsGroupBox.Controls.Add(this.EntryFillerCodeDropEdit);
            this.DetailsGroupBox.Controls.Add(this.PeriodEndDateEdit);
            this.DetailsGroupBox.Controls.Add(this.PeriodStartDateEdit);
            this.DetailsGroupBox.Controls.Add(this.StatementTypeDropEdit);
            this.DetailsGroupBox.Controls.Add(this.ImporterFindBox);
            this.DetailsGroupBox.Controls.Add(this.BranchDesignationDropEdit);
            this.DetailsGroupBox.Controls.Add(this.StatusDropEdit);
            this.DetailsGroupBox.Controls.Add(this.StatementNumberTextBox);
            this.DetailsGroupBox.Controls.Add(this.ReferenceNumberTextBox);
            this.DetailsGroupBox.Controls.Add(this.EntryNumberTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsGroupBox.Name = "DetailsGroupBox";
            this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 368, true);
            this.DetailsGroupBox.TabIndex = 0;
            this.DetailsGroupBox.TabStop = false;
            // 
            // TabControl
            // 
            this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.TabControl.Controls.Add(this.ChargesTabPage);
            this.TabControl.Controls.Add(this.EntriesTabPage);
            this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 272, true);
            this.TabControl.TabIndex = 12;
            // 
            // ChargesTabPage
            // 
            this.ChargesTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("476a8103-70c0-48f8-9d03-2977b68beb88", "Charges");
            this.ChargesTabPage.Controls.Add(this.ChargesGrid);
            this.ChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.ChargesTabPage.Name = "ChargesTabPage";
            this.ChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.ChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 202, true);
            this.ChargesTabPage.TabIndex = 0;
            this.ChargesTabPage.UseVisualStyleBackColor = true;
            // 
            // ChargesGrid
            // 
            this.ChargesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ChargesGrid, "ChargesDetail.Charges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)).SyncRoot)).B4_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)).SyncRoot)).B4_ChargeAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)).SyncRoot)).B4_MethodOfPayment)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)).SyncRoot)).MethodOfPaymentDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).ChargesDetail.Charges)).SyncRoot)).B4_ChargeGroup)));
            this.ChargesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "B4_ChargeType";
            zTextBoxColumnStyleInfo1.IsCustomColumn = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "B4_ChargeAmount";
            zCalcEditColumnStyleInfo1.IsCustomColumn = false;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.ColumnName = "B4_MethodOfPayment";
            zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("cdcbcb59-3221-400b-9a12-7313a40c7205", "Method of Payment");
            zDropEditColumnStyleInfo1.IsCustomColumn = false;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo2.ColumnName = "MethodOfPaymentDescription";
            zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("cdcbcb59-3221-400b-9a12-7313a40c7205", "Method of Payment");
            zTextBoxColumnStyleInfo2.IsCustomColumn = false;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.ColumnName = "B4_ChargeGroup";
            zTextBoxColumnStyleInfo3.IsCustomColumn = false;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChargesGrid.GridId = "a63c454d-c4c3-4158-8f9d-85bc18bf0ce9";
            this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ChargesGrid.LayoutKey = "ChargesGrid";
            this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.ChargesGrid.Name = "ChargesGrid";
            this.ChargesGrid.ReadOnly = true;
            this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 196, true);
            this.ChargesGrid.TabIndex = 2;
            // 
            // EntriesTabPage
            // 
            this.EntriesTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("69055286-2df1-4b92-a7a8-fecd64c9c5cf", "Entries");
            this.EntriesTabPage.Controls.Add(this.EntriesGrid);
            this.EntriesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.EntriesTabPage.Name = "EntriesTabPage";
            this.EntriesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.EntriesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 234, true);
            this.EntriesTabPage.TabIndex = 1;
            this.EntriesTabPage.UseVisualStyleBackColor = true;
            // 
            // EntriesGrid
            // 
            this.EntriesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.EntriesGrid, "Entries");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Entries)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementEntry)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Entries)).SyncRoot)).B3_EntryNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementEntry)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Entries)).SyncRoot)).B3_BrokerReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementEntry)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Entries)).SyncRoot)).B3_EntryType)));
            this.EntriesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo4.ColumnName = "B3_EntryNum";
            zTextBoxColumnStyleInfo4.IsCustomColumn = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "B3_BrokerReference";
            zTextBoxColumnStyleInfo5.IsCustomColumn = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo2.ColumnName = "B3_EntryType";
            zDropEditColumnStyleInfo2.IsCustomColumn = false;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.EntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.EntriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EntriesGrid.GridId = "a63c454d-c4c3-4158-8f9d-85bc18bf0ce9";
            this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.EntriesGrid.LayoutKey = "EntriesGrid";
            this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.EntriesGrid.Name = "EntriesGrid";
            this.EntriesGrid.ReadOnly = true;
            this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 228, true);
            this.EntriesGrid.TabIndex = 3;
            // 
            // StatementHeaderUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.DetailsGroupBox);
            this.Name = "StatementHeaderUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 600, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.StatusDropEdit.ResumeLayout(true);
            this.StatusDropEdit.PerformLayout();
            this.BranchDesignationDropEdit.ResumeLayout(true);
            this.BranchDesignationDropEdit.PerformLayout();
            this.ImporterFindBox.ResumeLayout(true);
            this.ImporterFindBox.PerformLayout();
            this.StatementTypeDropEdit.ResumeLayout(true);
            this.StatementTypeDropEdit.PerformLayout();
            this.PeriodStartDateEdit.ResumeLayout(true);
            this.PeriodStartDateEdit.PerformLayout();
            this.PeriodEndDateEdit.ResumeLayout(true);
            this.PeriodEndDateEdit.PerformLayout();
            this.PaymentTypeDropEdit.ResumeLayout(true);
            this.PaymentTypeDropEdit.PerformLayout();
            this.DetailsGroupBox.ResumeLayout(false);
            this.DetailsGroupBox.PerformLayout();
            this.TabControl.ResumeLayout(false);
            this.TabControl.PerformLayout();
            this.ChargesTabPage.ResumeLayout(false);
            this.ChargesTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
            this.ChargesGrid.ResumeLayout(false);
            this.ChargesGrid.PerformLayout();
            this.EntriesTabPage.ResumeLayout(false);
            this.EntriesTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
            this.EntriesGrid.ResumeLayout(false);
            this.EntriesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox StatementNumberTextBox;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		private ZArchitecture.ZTextBox EntryNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit BranchDesignationDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ImporterFindBox;
		private ZArchitecture.GUI.ZDropEdit StatementTypeDropEdit;
		private ZArchitecture.GUI.ZDateEdit PeriodStartDateEdit;
		private ZArchitecture.GUI.ZDateEdit PeriodEndDateEdit;
		private ZArchitecture.GUI.ZDropEdit EntryFillerCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		private ZArchitecture.ZTextBox ImporterCustomsIDTextBox;
		private ZArchitecture.ZTextBox CheckNoTextBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZTemplateTabControl TabControl;
		private ZArchitecture.GUI.ZTabPage ChargesTabPage;
		private ZArchitecture.ZGrid ChargesGrid;
		private ZArchitecture.GUI.ZTabPage EntriesTabPage;
		private ZArchitecture.ZGrid EntriesGrid;
	}
}
