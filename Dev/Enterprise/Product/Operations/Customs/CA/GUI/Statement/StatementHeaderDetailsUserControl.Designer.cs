namespace Enterprise.Customs.CA.GUI
{
	partial class StatementHeaderDetailsUserControl
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
			this.StatementAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SIMACalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExciseTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OthersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AccountingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImporterCustomsIDZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryFilerCodeZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GrandTotalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CARMGrandTotalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CARMDNExciseDutiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNGSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNOthersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNDisbursementsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNPaymentsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNExciseTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNInterestsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNSIMACalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CARMDNDutiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AccountingDateDateEdit.SuspendLayout();
			this.PrintDateDateEdit.SuspendLayout();
			this.StatementTypeDropEdit.SuspendLayout();
			this.DueDateDateEdit.SuspendLayout();
			this.ImporterGuidFindBox.SuspendLayout();
			this.GrandTotalGroupBox.SuspendLayout();
			this.CARMGrandTotalGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusStatementHeader);
			// 
			// StatementAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StatementAmountCalcEdit, "B2_StatementAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementAmount)));
			this.StatementAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a4e1651b-a591-4b2c-ad51-68791c11e78e", "Total Due");
			this.StatementAmountCalcEdit.DecimalPlaces = 2;
			this.StatementAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 107, true);
			this.StatementAmountCalcEdit.Name = "StatementAmountCalcEdit";
			this.StatementAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.StatementAmountCalcEdit.TabIndex = 7;
			this.StatementAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutiesCalcEdit, "B2_TotalCustomsDuties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalCustomsDuties)));
			this.DutiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e2f243ca-3124-4b40-b6d5-abb0e7b61b56", "Duties");
			this.DutiesCalcEdit.DecimalPlaces = 2;
			this.DutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.DutiesCalcEdit.Name = "DutiesCalcEdit";
			this.DutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.DutiesCalcEdit.TabIndex = 0;
			this.DutiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SIMACalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SIMACalcEdit, "B2_TotalSIMA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalSIMA)));
			this.SIMACalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bdec9359-1b06-4e4b-8833-5dd66e79bce5", "SIMA");
			this.SIMACalcEdit.DecimalPlaces = 2;
			this.SIMACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 45, true);
			this.SIMACalcEdit.Name = "SIMACalcEdit";
			this.SIMACalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.SIMACalcEdit.TabIndex = 1;
			this.SIMACalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExciseTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExciseTaxCalcEdit, "B2_TotalExciseTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalExciseTax)));
			this.ExciseTaxCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d9a6b2e2-e7ed-41b5-a717-30b5e7ce5edf", "Excise Tax");
			this.ExciseTaxCalcEdit.DecimalPlaces = 2;
			this.ExciseTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 71, true);
			this.ExciseTaxCalcEdit.Name = "ExciseTaxCalcEdit";
			this.ExciseTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.ExciseTaxCalcEdit.TabIndex = 2;
			this.ExciseTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCalcEdit, "B2_TotalGST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalGST)));
			this.GSTCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("92c07353-9846-4304-a455-867d37e7552f", "GST/PST/HST");
			this.GSTCalcEdit.DecimalPlaces = 2;
			this.GSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 95, true);
			this.GSTCalcEdit.Name = "GSTCalcEdit";
			this.GSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.GSTCalcEdit.TabIndex = 3;
			this.GSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OthersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OthersCalcEdit, "B2_TotalOthers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalOthers)));
			this.OthersCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c5975f23-6250-4fec-9507-57e4f2dc6021", "Others");
			this.OthersCalcEdit.DecimalPlaces = 2;
			this.OthersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 121, true);
			this.OthersCalcEdit.Name = "OthersCalcEdit";
			this.OthersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.OthersCalcEdit.TabIndex = 4;
			this.OthersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccountingDateDateEdit
			// 
			this.AccountingDateDateEdit.AllowDrop = true;
			this.AccountingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AccountingDateDateEdit, "B2_ProcessDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_ProcessDate)));
			this.AccountingDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8d4b6e97-77a0-49cc-bbc1-fe92314123ae", "Accounting Date");
			this.AccountingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 158, true);
			this.AccountingDateDateEdit.Name = "AccountingDateDateEdit";
			this.AccountingDateDateEdit.TabIndex = 10;
			// 
			// ImporterCustomsIDZTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterCustomsIDZTextBox, "B2_ImporterCustomsID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_ImporterCustomsID)));
			this.ImporterCustomsIDZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4c95f46-52cd-441b-b241-5f625b7bc0df", "Statement Business Number");
			this.ImporterCustomsIDZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 55, true);
			this.ImporterCustomsIDZTextBox.Name = "ImporterCustomsIDZTextBox";
			this.ImporterCustomsIDZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ImporterCustomsIDZTextBox.TabIndex = 3;
			// 
			// PrintDateDateEdit
			// 
			this.PrintDateDateEdit.AllowDrop = true;
			this.PrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PrintDateDateEdit, "B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PrintDate)));
			this.PrintDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b80a635e-5c68-4584-ac2e-bcfdccca445e", "Statement Date");
			this.PrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 107, true);
			this.PrintDateDateEdit.Name = "PrintDateDateEdit";
			this.PrintDateDateEdit.TabIndex = 6;
			// 
			// EntryFilerCodeZTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryFilerCodeZTextBox, "B2_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_EntryFilerCode)));
			this.EntryFilerCodeZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b4ecc9c2-6c11-483f-934b-c67171edf317", "Account Security Code");
			this.EntryFilerCodeZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 55, true);
			this.EntryFilerCodeZTextBox.Name = "EntryFilerCodeZTextBox";
			this.EntryFilerCodeZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.EntryFilerCodeZTextBox.TabIndex = 4;
			// 
			// StatementNumberZTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementNumberZTextBox, "B2_StatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementNumber)));
			this.StatementNumberZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6fc9b418-f43f-4303-9346-5522efd46d5d", "Statement Number");
			this.StatementNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 29, true);
			this.StatementNumberZTextBox.Name = "StatementNumberZTextBox";
			this.StatementNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 20, true);
			this.StatementNumberZTextBox.TabIndex = 2;
			// 
			// StatementTypeDropEdit
			// 
			this.StatementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementTypeDropEdit, "B2_StatementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).Lookups.StatementTypes)));
			this.StatementTypeDropEdit.BindToList = "Lookups.StatementTypes";
			this.StatementTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24b52959-8a41-499c-b148-eab4809a51c3", "Statement Type");
			this.StatementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 3, true);
			this.StatementTypeDropEdit.Name = "StatementTypeDropEdit";
			this.StatementTypeDropEdit.PreBoundMaxLength = 2;
			this.StatementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.StatementTypeDropEdit.TabIndex = 1;
			// 
			// DueDateDateEdit
			// 
			this.DueDateDateEdit.AllowDrop = true;
			this.DueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DueDateDateEdit, "B2_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_DueDate)));
			this.DueDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81be7346-319a-4017-af1e-34d11e070acb", "Payment Due Date");
			this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 132, true);
			this.DueDateDateEdit.Name = "DueDateDateEdit";
			this.DueDateDateEdit.TabIndex = 8;
			// 
			// MessageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "ARLMessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).ARLMessageType)));
			this.MessageTypeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c029a73-8568-46bf-8fb8-588ecd87e814", "Message Type");
			this.MessageTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 3, true);
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.MessageTypeTextBox.TabIndex = 0;
			// 
			// ImporterGuidFindBox
			// 
			this.ImporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterGuidFindBox, "B2_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_OH_Importer)));
			this.ImporterGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("84cbb931-e439-44a2-9824-bcbf7da13afa", "Importer");
			this.ImporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 81, true);
			this.ImporterGuidFindBox.Name = "ImporterGuidFindBox";
			this.ImporterGuidFindBox.ShouldResize = true;
			this.ImporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 20, true);
			this.ImporterGuidFindBox.TabIndex = 5;
			// 
			// GrandTotalGroupBox
			// 
			this.GrandTotalGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d35a4e12-5876-41ae-baed-379192ade7dd", "Grand Total");
			this.GrandTotalGroupBox.Controls.Add(this.GSTCalcEdit);
			this.GrandTotalGroupBox.Controls.Add(this.OthersCalcEdit);
			this.GrandTotalGroupBox.Controls.Add(this.ExciseTaxCalcEdit);
			this.GrandTotalGroupBox.Controls.Add(this.SIMACalcEdit);
			this.GrandTotalGroupBox.Controls.Add(this.DutiesCalcEdit);
			this.GrandTotalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 3, true);
			this.GrandTotalGroupBox.Name = "GrandTotalGroupBox";
			this.GrandTotalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 173, true);
			this.GrandTotalGroupBox.TabIndex = 12;
			this.GrandTotalGroupBox.TabStop = false;
			// 
			// CARMGrandTotalGroupBox
			// 
			this.CARMGrandTotalGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d35a4e12-5876-41ae-baed-379192ade7dd", "Grand Total");
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNExciseDutiesCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNGSTCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNOthersCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNDisbursementsCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNPaymentsCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNExciseTaxCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNInterestsCalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNSIMACalcEdit);
			this.CARMGrandTotalGroupBox.Controls.Add(this.CARMDNDutiesCalcEdit);
			this.CARMGrandTotalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 4, true);
			this.CARMGrandTotalGroupBox.Name = "CARMGrandTotalGroupBox";
			this.CARMGrandTotalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 148, true);
			this.CARMGrandTotalGroupBox.TabIndex = 13;
			this.CARMGrandTotalGroupBox.TabStop = false;
			// 
			// CARMDNExciseDutiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNExciseDutiesCalcEdit, "B2_TotalExciseDuties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalExciseDuties)));
			this.CARMDNExciseDutiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("25e89dc1-a15f-4d88-820c-3cf5fa426774", "Excise Duties");
			this.CARMDNExciseDutiesCalcEdit.DecimalPlaces = 2;
			this.CARMDNExciseDutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 96, true);
			this.CARMDNExciseDutiesCalcEdit.Name = "CARMDNExciseDutiesCalcEdit";
			this.CARMDNExciseDutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNExciseDutiesCalcEdit.TabIndex = 7;
			this.CARMDNExciseDutiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNGSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNGSTCalcEdit, "B2_TotalGST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalGST)));
			this.CARMDNGSTCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2a8e8f01-2251-455d-a457-c5c5434b0128", "GST/PST/HST");
			this.CARMDNGSTCalcEdit.DecimalPlaces = 2;
			this.CARMDNGSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 122, true);
			this.CARMDNGSTCalcEdit.Name = "CARMDNGSTCalcEdit";
			this.CARMDNGSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNGSTCalcEdit.TabIndex = 9;
			this.CARMDNGSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNOthersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNOthersCalcEdit, "B2_TotalOthers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalOthers)));
			this.CARMDNOthersCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3d1d531b-53c5-4b52-9bc6-e048e6066fa5", "Others");
			this.CARMDNOthersCalcEdit.DecimalPlaces = 2;
			this.CARMDNOthersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 96, true);
			this.CARMDNOthersCalcEdit.Name = "CARMDNOthersCalcEdit";
			this.CARMDNOthersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNOthersCalcEdit.TabIndex = 8;
			this.CARMDNOthersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNDisbursementsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNDisbursementsCalcEdit, "B2_RefundAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_RefundAmount)));
			this.CARMDNDisbursementsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8266f598-3935-4cfd-8189-daa0aa2a027c", "Disbursements");
			this.CARMDNDisbursementsCalcEdit.DecimalPlaces = 2;
			this.CARMDNDisbursementsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 71, true);
			this.CARMDNDisbursementsCalcEdit.Name = "CARMDNDisbursementsCalcEdit";
			this.CARMDNDisbursementsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNDisbursementsCalcEdit.TabIndex = 6;
			this.CARMDNDisbursementsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNPaymentsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNPaymentsCalcEdit, "B2_PaidAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PaidAmount)));
			this.CARMDNPaymentsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a2b6b8c2-420f-4f68-8dec-6ce4c3e63b03", "Payments");
			this.CARMDNPaymentsCalcEdit.DecimalPlaces = 2;
			this.CARMDNPaymentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 45, true);
			this.CARMDNPaymentsCalcEdit.Name = "CARMDNPaymentsCalcEdit";
			this.CARMDNPaymentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNPaymentsCalcEdit.TabIndex = 4;
			this.CARMDNPaymentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNExciseTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNExciseTaxCalcEdit, "B2_TotalExciseTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalExciseTax)));
			this.CARMDNExciseTaxCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3c225603-6380-47ef-9a65-fadd37003023", "Excise Tax");
			this.CARMDNExciseTaxCalcEdit.DecimalPlaces = 2;
			this.CARMDNExciseTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 71, true);
			this.CARMDNExciseTaxCalcEdit.Name = "CARMDNExciseTaxCalcEdit";
			this.CARMDNExciseTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNExciseTaxCalcEdit.TabIndex = 5;
			this.CARMDNExciseTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNInterestsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNInterestsCalcEdit, "B2_TotalInterests");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalInterests)));
			this.CARMDNInterestsCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ddd63269-7663-48be-b9b5-30bf94723731", "Late Payment Interests");
			this.CARMDNInterestsCalcEdit.DecimalPlaces = 2;
			this.CARMDNInterestsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 19, true);
			this.CARMDNInterestsCalcEdit.Name = "CARMDNInterestsCalcEdit";
			this.CARMDNInterestsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNInterestsCalcEdit.TabIndex = 2;
			this.CARMDNInterestsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNSIMACalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNSIMACalcEdit, "B2_TotalSIMA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalSIMA)));
			this.CARMDNSIMACalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2d3c6b9-8596-4f3b-a42b-3f95b69f71fe", "SIMA");
			this.CARMDNSIMACalcEdit.DecimalPlaces = 2;
			this.CARMDNSIMACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 45, true);
			this.CARMDNSIMACalcEdit.Name = "CARMDNSIMACalcEdit";
			this.CARMDNSIMACalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNSIMACalcEdit.TabIndex = 3;
			this.CARMDNSIMACalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CARMDNDutiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CARMDNDutiesCalcEdit, "B2_TotalCustomsDuties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_TotalCustomsDuties)));
			this.CARMDNDutiesCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0af74181-6cd0-4d90-a69a-6a0b71ec9777", "Duties");
			this.CARMDNDutiesCalcEdit.DecimalPlaces = 2;
			this.CARMDNDutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 19, true);
			this.CARMDNDutiesCalcEdit.Name = "CARMDNDutiesCalcEdit";
			this.CARMDNDutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.CARMDNDutiesCalcEdit.TabIndex = 0;
			this.CARMDNDutiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatementHeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CARMGrandTotalGroupBox);
			this.Controls.Add(this.ImporterGuidFindBox);
			this.Controls.Add(this.MessageTypeTextBox);
			this.Controls.Add(this.DueDateDateEdit);
			this.Controls.Add(this.StatementTypeDropEdit);
			this.Controls.Add(this.StatementAmountCalcEdit);
			this.Controls.Add(this.AccountingDateDateEdit);
			this.Controls.Add(this.ImporterCustomsIDZTextBox);
			this.Controls.Add(this.PrintDateDateEdit);
			this.Controls.Add(this.EntryFilerCodeZTextBox);
			this.Controls.Add(this.StatementNumberZTextBox);
			this.Controls.Add(this.GrandTotalGroupBox);
			this.Name = "StatementHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1023, 180, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccountingDateDateEdit.ResumeLayout(true);
			this.AccountingDateDateEdit.PerformLayout();
			this.PrintDateDateEdit.ResumeLayout(true);
			this.PrintDateDateEdit.PerformLayout();
			this.StatementTypeDropEdit.ResumeLayout(true);
			this.StatementTypeDropEdit.PerformLayout();
			this.DueDateDateEdit.ResumeLayout(true);
			this.DueDateDateEdit.PerformLayout();
			this.ImporterGuidFindBox.ResumeLayout(true);
			this.ImporterGuidFindBox.PerformLayout();
			this.GrandTotalGroupBox.ResumeLayout(false);
			this.GrandTotalGroupBox.PerformLayout();
			this.CARMGrandTotalGroupBox.ResumeLayout(false);
			this.CARMGrandTotalGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.ZCalcEdit StatementAmountCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DutiesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit SIMACalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ExciseTaxCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit GSTCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OthersCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit AccountingDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox ImporterCustomsIDZTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PrintDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox EntryFilerCodeZTextBox;
		private Enterprise.ZArchitecture.ZTextBox StatementNumberZTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatementTypeDropEdit;
		private ZArchitecture.GUI.ZDateEdit DueDateDateEdit;
		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox GrandTotalGroupBox;
		private ZArchitecture.GUI.ZGroupBox CARMGrandTotalGroupBox;
		private ZArchitecture.ZCalcEdit CARMDNGSTCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNOthersCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNDisbursementsCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNPaymentsCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNExciseTaxCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNInterestsCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNSIMACalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNDutiesCalcEdit;
		private ZArchitecture.ZCalcEdit CARMDNExciseDutiesCalcEdit;
	}
}
