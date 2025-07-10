
namespace Enterprise.Customs.KR.GUI
{
	partial class StatementUserControl
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
			this.FormattedNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedProcessDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RelatedFormattedAccountNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ProcessPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PeriodDateUserControl = new Enterprise.Customs.KR.GUI.PeriodDateUserControl();
			this.ProcessAndDueDateUserControl = new Enterprise.Customs.KR.GUI.ProcessAndDueDateUserControl();
			this.PayerFromCustomsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ProcessDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PaymentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormattedEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsAccountIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalAmountAfterDueDateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusDropEdit.SuspendLayout();
			this.StatementTypeDropEdit.SuspendLayout();
			this.RelatedProcessDateEdit.SuspendLayout();
			this.RelatedFormattedAccountNumberCodeFindBox.SuspendLayout();
			this.ProcessPortCodeFindBox.SuspendLayout();
			this.PeriodDateUserControl.SuspendLayout();
			this.ProcessAndDueDateUserControl.SuspendLayout();
			this.ImporterGuidFindBox.SuspendLayout();
			this.ProcessDateEdit.SuspendLayout();
			this.PaymentDateEdit.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.BillTypeDropEdit.SuspendLayout();
			this.DueDateEdit.SuspendLayout();
			this.IssueDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// FormattedNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedNumberTextBox, "FormattedStatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FormattedStatementNumber)));
			this.FormattedNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FormattedNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 11, true);
			this.FormattedNumberTextBox.Name = "FormattedNumberTextBox";
			this.FormattedNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FormattedNumberTextBox.TabIndex = 0;
			// 
			// StatementAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StatementAmountCalcEdit, "TotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).TotalAmount)));
			this.StatementAmountCalcEdit.DecimalPlaces = 2;
			this.StatementAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 77, true);
			this.StatementAmountCalcEdit.Name = "StatementAmountCalcEdit";
			this.StatementAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.StatementAmountCalcEdit.TabIndex = 4;
			this.StatementAmountCalcEdit.TabStop = false;
			this.StatementAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.StatementAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "B2_PaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PaymentStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 198, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StatusDropEdit.TabIndex = 6;
			// 
			// StatementTypeDropEdit
			// 
			this.StatementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementTypeDropEdit, "B2_StatementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_StatementType)));
			this.StatementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 221, true);
			this.StatementTypeDropEdit.Name = "StatementTypeDropEdit";
			this.StatementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StatementTypeDropEdit.TabIndex = 7;
			// 
			// RelatedProcessDateEdit
			// 
			this.RelatedProcessDateEdit.AllowDrop = true;
			this.RelatedProcessDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RelatedProcessDateEdit, "RelatedProcessDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).RelatedProcessDate)));
			this.RelatedProcessDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 290, true);
			this.RelatedProcessDateEdit.Name = "RelatedProcessDateEdit";
			this.RelatedProcessDateEdit.TabIndex = 11;
			// 
			// RelatedFormattedAccountNumberCodeFindBox
			// 
			this.RelatedFormattedAccountNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedFormattedAccountNumberCodeFindBox, "B2_AccountNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_AccountNo)));
			this.RelatedFormattedAccountNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 267, true);
			this.RelatedFormattedAccountNumberCodeFindBox.Name = "RelatedFormattedAccountNumberCodeFindBox";
			this.RelatedFormattedAccountNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RelatedFormattedAccountNumberCodeFindBox.ParentType = null;
			this.RelatedFormattedAccountNumberCodeFindBox.ShowDescriptionBox = false;
			this.RelatedFormattedAccountNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.RelatedFormattedAccountNumberCodeFindBox.TabIndex = 10;
			// 
			// ProcessPortCodeFindBox
			// 
			this.ProcessPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessPortCodeFindBox, "B2_ProcessPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_ProcessPort)));
			this.ProcessPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 33, true);
			this.ProcessPortCodeFindBox.Name = "ProcessPortCodeFindBox";
			this.ProcessPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProcessPortCodeFindBox.ParentType = null;
			this.ProcessPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ProcessPortCodeFindBox.TabIndex = 1;
			// 
			// PeriodDateUserControl
			// 
			this.PeriodDateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodDateUserControl, ".");
			this.PeriodDateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 243, true);
			this.PeriodDateUserControl.Name = "PeriodDateUserControl";
			this.PeriodDateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PeriodDateUserControl.TabIndex = 8;
			// 
			// ProcessAndDueDateUserControl
			// 
			this.ProcessAndDueDateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessAndDueDateUserControl, ".");
			this.ProcessAndDueDateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 55, true);
			this.ProcessAndDueDateUserControl.Name = "ProcessAndDueDateUserControl";
			this.ProcessAndDueDateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ProcessAndDueDateUserControl.TabIndex = 2;
			// 
			// PayerFromCustomsTextBox
			// 
			this.BindingSource.SetBindingMember(this.PayerFromCustomsTextBox, "PayerFromCustoms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).PayerFromCustoms)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PayerFromCustomsTextBox, false);
			this.PayerFromCustomsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 122, true);
			this.PayerFromCustomsTextBox.Multiline = true;
			this.PayerFromCustomsTextBox.Name = "PayerFromCustomsTextBox";
			this.PayerFromCustomsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 72, true);
			this.PayerFromCustomsTextBox.TabIndex = 6;
			// 
			// ImporterGuidFindBox
			// 
			this.ImporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterGuidFindBox, "B2_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_OH_Importer)));
			this.ImporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 99, true);
			this.ImporterGuidFindBox.Name = "ImporterGuidFindBox";
			this.ImporterGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ImporterGuidFindBox.ParentType = null;
			this.ImporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ImporterGuidFindBox.TabIndex = 5;
			// 
			// ProcessDateEdit
			// 
			this.ProcessDateEdit.AllowDrop = true;
			this.ProcessDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ProcessDateEdit, "B2_ProcessDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_ProcessDate)));
			this.ProcessDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 313, true);
			this.ProcessDateEdit.Name = "ProcessDateEdit";
			this.ProcessDateEdit.TabIndex = 12;
			// 
			// PaymentDateEdit
			// 
			this.PaymentDateEdit.AllowDrop = true;
			this.PaymentDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PaymentDateEdit, "B2_PaymentAuthorizationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PaymentAuthorizationDate)));
			this.PaymentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 335, true);
			this.PaymentDateEdit.Name = "PaymentDateEdit";
			this.PaymentDateEdit.TabIndex = 13;
			// 
			// TotalVATAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVATAmountCalcEdit, "TotalVATBaseAmountForVATReport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).TotalVATBaseAmountForVATReport)));
			this.TotalVATAmountCalcEdit.DecimalPlaces = 2;
			this.TotalVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 358, true);
			this.TotalVATAmountCalcEdit.Name = "TotalVATAmountCalcEdit";
			this.TotalVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.TotalVATAmountCalcEdit.TabIndex = 14;
			this.TotalVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalVATAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "B2_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PaymentType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 381, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 15;
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "B2_PaymentParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PaymentParty)));
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 403, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 16;
			// 
			// FormattedEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedEntryNumberTextBox, "FirstLine+FormattedNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.FormattedNumber)));
			this.FormattedEntryNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FormattedEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 429, true);
			this.FormattedEntryNumberTextBox.Name = "FormattedEntryNumberTextBox";
			this.FormattedEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FormattedEntryNumberTextBox.TabIndex = 17;
			// 
			// BillTypeDropEdit
			// 
			this.BillTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillTypeDropEdit, "B2_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_Status)));
			this.BillTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 455, true);
			this.BillTypeDropEdit.Name = "BillTypeDropEdit";
			this.BillTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BillTypeDropEdit.TabIndex = 18;
			// 
			// CustomsAccountIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsAccountIDTextBox, "BankAccountID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).BankAccountID)));
			this.CustomsAccountIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomsAccountIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 481, true);
			this.CustomsAccountIDTextBox.Name = "CustomsAccountIDTextBox";
			this.CustomsAccountIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CustomsAccountIDTextBox.TabIndex = 19;
			// 
			// DueDateEdit
			// 
			this.DueDateEdit.AllowDrop = true;
			this.DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DueDateEdit, "B2_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_DueDate)));
			this.DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 507, true);
			this.DueDateEdit.Name = "DueDateEdit";
			this.DueDateEdit.TabIndex = 20;
			// 
			// IssueDateEdit
			// 
			this.IssueDateEdit.AllowDrop = true;
			this.IssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.IssueDateEdit, "B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PrintDate)));
			this.IssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 533, true);
			this.IssueDateEdit.Name = "IssueDateEdit";
			this.IssueDateEdit.TabIndex = 21;
			// 
			// TotalAmountAfterDueDateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalAmountAfterDueDateCalcEdit, "OverdueAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).OverdueAmount)));
			this.TotalAmountAfterDueDateCalcEdit.DecimalPlaces = 2;
			this.TotalAmountAfterDueDateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 559, true);
			this.TotalAmountAfterDueDateCalcEdit.Name = "TotalAmountAfterDueDateCalcEdit";
			this.TotalAmountAfterDueDateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.TotalAmountAfterDueDateCalcEdit.TabIndex = 23;
			this.TotalAmountAfterDueDateCalcEdit.TabStop = false;
			this.TotalAmountAfterDueDateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalAmountAfterDueDateCalcEdit.TrackDisposedAccess = true;
			// 
			// StatementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TotalAmountAfterDueDateCalcEdit);
			this.Controls.Add(this.IssueDateEdit);
			this.Controls.Add(this.DueDateEdit);
			this.Controls.Add(this.CustomsAccountIDTextBox);
			this.Controls.Add(this.BillTypeDropEdit);
			this.Controls.Add(this.FormattedEntryNumberTextBox);
			this.Controls.Add(this.PaymentPartyDropEdit);
			this.Controls.Add(this.PaymentTypeDropEdit);
			this.Controls.Add(this.TotalVATAmountCalcEdit);
			this.Controls.Add(this.PaymentDateEdit);
			this.Controls.Add(this.ProcessDateEdit);
			this.Controls.Add(this.ImporterGuidFindBox);
			this.Controls.Add(this.PayerFromCustomsTextBox);
			this.Controls.Add(this.ProcessAndDueDateUserControl);
			this.Controls.Add(this.PeriodDateUserControl);
			this.Controls.Add(this.ProcessPortCodeFindBox);
			this.Controls.Add(this.RelatedFormattedAccountNumberCodeFindBox);
			this.Controls.Add(this.RelatedProcessDateEdit);
			this.Controls.Add(this.StatementTypeDropEdit);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.StatementAmountCalcEdit);
			this.Controls.Add(this.FormattedNumberTextBox);
			this.Name = "StatementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 643, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.StatementTypeDropEdit.ResumeLayout(true);
			this.StatementTypeDropEdit.PerformLayout();
			this.RelatedProcessDateEdit.ResumeLayout(true);
			this.RelatedProcessDateEdit.PerformLayout();
			this.RelatedFormattedAccountNumberCodeFindBox.ResumeLayout(true);
			this.RelatedFormattedAccountNumberCodeFindBox.PerformLayout();
			this.ProcessPortCodeFindBox.ResumeLayout(true);
			this.ProcessPortCodeFindBox.PerformLayout();
			this.PeriodDateUserControl.ResumeLayout(true);
			this.PeriodDateUserControl.PerformLayout();
			this.ProcessAndDueDateUserControl.ResumeLayout(true);
			this.ProcessAndDueDateUserControl.PerformLayout();
			this.ImporterGuidFindBox.ResumeLayout(true);
			this.ImporterGuidFindBox.PerformLayout();
			this.ProcessDateEdit.ResumeLayout(true);
			this.ProcessDateEdit.PerformLayout();
			this.PaymentDateEdit.ResumeLayout(true);
			this.PaymentDateEdit.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.BillTypeDropEdit.ResumeLayout(true);
			this.BillTypeDropEdit.PerformLayout();
			this.DueDateEdit.ResumeLayout(true);
			this.DueDateEdit.PerformLayout();
			this.IssueDateEdit.ResumeLayout(true);
			this.IssueDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZCalcEdit StatementAmountCalcEdit;
		internal ZArchitecture.ZTextBox FormattedNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit StatementTypeDropEdit;
		internal ZArchitecture.GUI.ZDateEdit RelatedProcessDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox RelatedFormattedAccountNumberCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox ProcessPortCodeFindBox;
		internal PeriodDateUserControl PeriodDateUserControl;
		internal ProcessAndDueDateUserControl ProcessAndDueDateUserControl;
		internal ZArchitecture.ZTextBox PayerFromCustomsTextBox;
		internal ZArchitecture.GUI.ZGuidFindBox ImporterGuidFindBox;
		internal ZArchitecture.GUI.ZDateEdit ProcessDateEdit;
		internal ZArchitecture.GUI.ZDateEdit PaymentDateEdit;
		internal ZArchitecture.ZCalcEdit TotalVATAmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		internal ZArchitecture.ZTextBox FormattedEntryNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit BillTypeDropEdit;
		internal ZArchitecture.ZTextBox CustomsAccountIDTextBox;
		internal ZArchitecture.GUI.ZDateEdit DueDateEdit;
		internal ZArchitecture.GUI.ZDateEdit IssueDateEdit;
		internal ZArchitecture.ZCalcEdit TotalAmountAfterDueDateCalcEdit;
	}
}
