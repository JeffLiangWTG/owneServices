using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class K84UserControl
	{
		ZGroupBox k84GroupBox;
		ZArchitecture.ZTextBox releaseOfficetextBox;
		CargoWise.Windows.UI.KSplitter horizontalSplitter;
		ZTemplateTabControl noticesTabControl;
		public ZTabPage CCNTabPage;
		ZGroupBox cCNGroupBox;
		public CargoControlNumbersUserControl CCNUserControl;
		ZTabPage rNSStatusHistoryTabPage;
		ZTabPage dailyNoticeDocTrasacTabPage;
		ZTabPage noticeTabPage;
		ZTabPage b2sTabPage;
		ZDateEdit cA_K84StatementDateBoundDateEdit;
		ZArchitecture.ZCalcEdit totalDutyAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalExciseTaxAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalSIMAAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalDutyAndTaxAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalGSTAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalGSTDirectAmountCalcEdit;
		ZArchitecture.ZCalcEdit totalIncludingPenaltyCalcEdit;
		ZArchitecture.ZCalcEdit k84LateFilingPenaltyCalcEdit;
		ZArchitecture.ZCalcEdit aRLOthersCalcEdit;
		ZDateEdit cA_K84AccountingDateBoundDateEdit;
		ZGroupBox declarationDetailsGroupBox;
		ZArchitecture.ZGrid rNSStatusHistoryBoundGrid;
		ZArchitecture.ZGrid dNHistoryBoundGrid;
		ZArchitecture.ZGrid noticesBoundGrid;
		ZArchitecture.ZGrid b2sBoundGrid;
		K84DeclarationDetailsUserControl k84DeclarationDetailsUserControl;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.k84GroupBox = new ZGroupBox();
			this.aRLOthersCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalIncludingPenaltyCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalDutyAndTaxAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalGSTAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.k84LateFilingPenaltyCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalExciseTaxAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalGSTDirectAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalSIMAAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalDutyAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.releaseOfficetextBox = new ZArchitecture.ZTextBox();
			this.cA_K84StatementDateBoundDateEdit = new ZDateEdit();
			this.cA_K84AccountingDateBoundDateEdit = new ZDateEdit();
			this.horizontalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.noticesTabControl = new ZTemplateTabControl();
			this.CCNTabPage = new ZTabPage();
			this.cCNGroupBox = new ZGroupBox();
			this.CCNUserControl = new CargoControlNumbersUserControl();
			this.rNSStatusHistoryTabPage = new ZTabPage();
			this.rNSStatusHistoryBoundGrid = new ZArchitecture.ZGrid();
			this.dailyNoticeDocTrasacTabPage = new ZTabPage();
			this.dNHistoryBoundGrid = new ZArchitecture.ZGrid();
			this.noticeTabPage = new ZTabPage();
			this.noticesBoundGrid = new ZArchitecture.ZGrid();
			this.b2sTabPage = new ZTabPage();
			this.b2sBoundGrid = new ZArchitecture.ZGrid();
			this.declarationDetailsGroupBox = new ZGroupBox();
			this.k84DeclarationDetailsUserControl = new K84DeclarationDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.k84GroupBox.SuspendLayout();
			this.cA_K84StatementDateBoundDateEdit.SuspendLayout();
			this.cA_K84AccountingDateBoundDateEdit.SuspendLayout();
			this.noticesTabControl.SuspendLayout();
			this.CCNTabPage.SuspendLayout();
			this.cCNGroupBox.SuspendLayout();
			this.CCNUserControl.SuspendLayout();
			this.rNSStatusHistoryTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rNSStatusHistoryBoundGrid)).BeginInit();
			this.rNSStatusHistoryBoundGrid.SuspendLayout();
			this.dailyNoticeDocTrasacTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dNHistoryBoundGrid)).BeginInit();
			this.dNHistoryBoundGrid.SuspendLayout();
			this.noticeTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.noticesBoundGrid)).BeginInit();
			this.noticesBoundGrid.SuspendLayout();
			this.b2sTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.b2sBoundGrid)).BeginInit();
			this.b2sBoundGrid.SuspendLayout();
			this.declarationDetailsGroupBox.SuspendLayout();
			this.k84DeclarationDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// K84GroupBox
			// 
			this.k84GroupBox.Controls.Add(this.aRLOthersCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalIncludingPenaltyCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalDutyAndTaxAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalGSTAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.k84LateFilingPenaltyCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalExciseTaxAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalGSTDirectAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalSIMAAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.totalDutyAmountCalcEdit);
			this.k84GroupBox.Controls.Add(this.releaseOfficetextBox);
			this.k84GroupBox.Controls.Add(this.cA_K84StatementDateBoundDateEdit);
			this.k84GroupBox.Controls.Add(this.cA_K84AccountingDateBoundDateEdit);
			this.k84GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.k84GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.k84GroupBox.Name = "K84GroupBox";
			this.k84GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 136, true);
			this.k84GroupBox.TabIndex = 0;
			this.k84GroupBox.TabStop = false;
			this.k84GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|62CCBBDF-4865-49E0-A8D2-3D647A23BB45", "Daily Notice");
			// 
			// ARLOthersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.aRLOthersCalcEdit, "CA_ARLOthersAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_ARLOthersAmount);
			this.aRLOthersCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.aRLOthersCalcEdit.DecimalPlaces = 2;
			this.aRLOthersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 100, true);
			this.aRLOthersCalcEdit.Name = "ARLOthersCalcEdit";
			this.aRLOthersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.aRLOthersCalcEdit.TabIndex = 10;
			this.aRLOthersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalIncludingPenaltyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalIncludingPenaltyCalcEdit, "CA_TotalIncludingPenalty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalIncludingPenalty);
			this.totalIncludingPenaltyCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalIncludingPenaltyCalcEdit.DecimalPlaces = 2;
			this.totalIncludingPenaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 100, true);
			this.totalIncludingPenaltyCalcEdit.Name = "TotalIncludingPenaltyCalcEdit";
			this.totalIncludingPenaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalIncludingPenaltyCalcEdit.TabIndex = 11;
			this.totalIncludingPenaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalDutyAndTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalDutyAndTaxAmountCalcEdit, "CA_TotalDutyAndTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalDutyAndTaxAmount);
			this.totalDutyAndTaxAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalDutyAndTaxAmountCalcEdit.DecimalPlaces = 2;
			this.totalDutyAndTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 74, true);
			this.totalDutyAndTaxAmountCalcEdit.Name = "TotalDutyAndTaxAmountCalcEdit";
			this.totalDutyAndTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalDutyAndTaxAmountCalcEdit.TabIndex = 8;
			this.totalDutyAndTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalGSTAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalGSTAmountCalcEdit, "CA_TotalGSTAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalGSTAmount);
			this.totalGSTAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalGSTAmountCalcEdit.DecimalPlaces = 2;
			this.totalGSTAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 74, true);
			this.totalGSTAmountCalcEdit.Name = "TotalGSTAmountCalcEdit";
			this.totalGSTAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalGSTAmountCalcEdit.TabIndex = 7;
			this.totalGSTAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// K84LateFilingPenaltyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.k84LateFilingPenaltyCalcEdit, "CA_K84LateFilingPenalty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_K84LateFilingPenalty);
			this.k84LateFilingPenaltyCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.k84LateFilingPenaltyCalcEdit.DecimalPlaces = 2;
			this.k84LateFilingPenaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 100, true);
			this.k84LateFilingPenaltyCalcEdit.Name = "K84LateFilingPenaltyCalcEdit";
			this.k84LateFilingPenaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.k84LateFilingPenaltyCalcEdit.TabIndex = 9;
			this.k84LateFilingPenaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalExciseTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalExciseTaxAmountCalcEdit, "CA_TotalExciseTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalExciseTaxAmount);
			this.totalExciseTaxAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalExciseTaxAmountCalcEdit.DecimalPlaces = 2;
			this.totalExciseTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 48, true);
			this.totalExciseTaxAmountCalcEdit.Name = "TotalExciseTaxAmountCalcEdit";
			this.totalExciseTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalExciseTaxAmountCalcEdit.TabIndex = 5;
			this.totalExciseTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalGSTDirectAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalGSTDirectAmountCalcEdit, "CA_TotalGSTDirectAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalGSTDirectAmount);
			this.totalGSTDirectAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalGSTDirectAmountCalcEdit.DecimalPlaces = 2;
			this.totalGSTDirectAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 74, true);
			this.totalGSTDirectAmountCalcEdit.Name = "TotalGSTDirectAmountCalcEdit";
			this.totalGSTDirectAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalGSTDirectAmountCalcEdit.TabIndex = 6;
			this.totalGSTDirectAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalSIMAAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalSIMAAmountCalcEdit, "CA_TotalSIMAAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalSIMAAmount);
			this.totalSIMAAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalSIMAAmountCalcEdit.DecimalPlaces = 2;
			this.totalSIMAAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 48, true);
			this.totalSIMAAmountCalcEdit.Name = "TotalSIMAAmountCalcEdit";
			this.totalSIMAAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalSIMAAmountCalcEdit.TabIndex = 4;
			this.totalSIMAAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalDutyAmountCalcEdit, "CA_TotalDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_TotalDutyAmount);
			this.totalDutyAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalDutyAmountCalcEdit.DecimalPlaces = 2;
			this.totalDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 48, true);
			this.totalDutyAmountCalcEdit.Name = "TotalDutyAmountCalcEdit";
			this.totalDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.totalDutyAmountCalcEdit.TabIndex = 3;
			this.totalDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReleaseOfficetextBox
			// 
			this.BindingSource.SetBindingMember(this.releaseOfficetextBox, "CA_ReleaseOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_ReleaseOffice);
			this.releaseOfficetextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 22, true);
			this.releaseOfficetextBox.Name = "ReleaseOfficetextBox";
			this.releaseOfficetextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.releaseOfficetextBox.TabIndex = 2;
			// 
			// CA_K84StatementDateBoundDateEdit
			// 
			this.cA_K84StatementDateBoundDateEdit.AllowDrop = true;
			this.cA_K84StatementDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.cA_K84StatementDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.cA_K84StatementDateBoundDateEdit, "CA_K84StatementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_K84StatementDate);
			this.cA_K84StatementDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 22, true);
			this.cA_K84StatementDateBoundDateEdit.Name = "CA_K84StatementDateBoundDateEdit";
			this.cA_K84StatementDateBoundDateEdit.TabIndex = 1;
			// 
			// CA_K84AccountingDateBoundDateEdit
			// 
			this.cA_K84AccountingDateBoundDateEdit.AllowDrop = true;
			this.cA_K84AccountingDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.cA_K84AccountingDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.cA_K84AccountingDateBoundDateEdit, "CA_K84AccountingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_K84AccountingDate);
			this.cA_K84AccountingDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 22, true);
			this.cA_K84AccountingDateBoundDateEdit.Name = "CA_K84AccountingDateBoundDateEdit";
			this.cA_K84AccountingDateBoundDateEdit.TabIndex = 0;
			// 
			// HorizontalSplitter
			// 
			this.horizontalSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.horizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.horizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 357, true);
			this.horizontalSplitter.Name = "HorizontalSplitter";
			this.horizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 3, true);
			this.horizontalSplitter.TabIndex = 1;
			this.horizontalSplitter.TabStop = false;
			// 
			// NoticesTabControl
			// 
			this.noticesTabControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.noticesTabControl.Controls.Add(this.CCNTabPage);
			this.noticesTabControl.Controls.Add(this.rNSStatusHistoryTabPage);
			this.noticesTabControl.Controls.Add(this.dailyNoticeDocTrasacTabPage);
			this.noticesTabControl.Controls.Add(this.noticeTabPage);
			this.noticesTabControl.Controls.Add(this.b2sTabPage);
			this.noticesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.noticesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
			this.noticesTabControl.Name = "NoticesTabControl";
			this.noticesTabControl.SelectedIndex = 0;
			this.noticesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 236, true);
			this.noticesTabControl.TabIndex = 0;
			// 
			// CCNTabPage
			// 
			this.CCNTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|A73D219C-3A7B-448B-B2BB-791650F7F73B", "Cargo Control Numbers");
			this.CCNTabPage.Controls.Add(this.cCNGroupBox);
			this.CCNTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CCNTabPage.Name = "CCNTabPage";
			this.CCNTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CCNTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 232, true);
			this.CCNTabPage.TabIndex = 0;
			// 
			// CCNGroupBox
			// 
			this.cCNGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|3A55E0C9-7E8F-4491-8D67-FB5DE12B5C82", "Cargo Control Numbers");
			this.cCNGroupBox.Controls.Add(this.CCNUserControl);
			this.cCNGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cCNGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cCNGroupBox.Name = "CCNGroupBox";
			this.cCNGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 252, true);
			this.cCNGroupBox.TabIndex = 0;
			this.cCNGroupBox.TabStop = false;
			// 
			// CCNUserControl
			// 
			this.CCNUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CCNUserControl, ".");
			this.CCNUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CCNUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CCNUserControl.Name = "CCNUserControl";
			this.CCNUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 233, true);
			this.CCNUserControl.TabIndex = 0;
			this.CCNUserControl.ReleaseStatusesGrid.ReadOnly = true;
			// 
			// RNSStatusHistoryTabPage
			// 
			this.rNSStatusHistoryTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|6EFA13CC-26E8-4886-8BB6-4DD4ACE36352", "RNS Status History");
			this.rNSStatusHistoryTabPage.Controls.Add(this.rNSStatusHistoryBoundGrid);
			this.rNSStatusHistoryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.rNSStatusHistoryTabPage.Name = "RNSStatusHistoryTabPage";
			this.rNSStatusHistoryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.rNSStatusHistoryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 232, true);
			this.rNSStatusHistoryTabPage.TabIndex = 1;
			// 
			// RNSStatusHistoryBoundGrid
			// 
			this.rNSStatusHistoryBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.rNSStatusHistoryBoundGrid, "RnsHistoryMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).RnsHistoryMessages);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).EM_MessageSubType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).ProcessingIndicator);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).EM_MessageDateTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).RNSProcessingDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).RNSReleaseDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).ServiceOption);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).DocumentReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).ReleaseOffice);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).WarehouseCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).CargoControlNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EDIMessage)(((System.Collections.IList)(((JobDeclaration)(null)).RnsHistoryMessages)).SyncRoot)).ContainerNumbers);
			this.rNSStatusHistoryBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|7FB9B7BB-ECAC-46ED-AC31-5FD7D42D2038", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|84735BC4-1099-4E1E-8AFE-E2A8A4F22E4D", "Processing Indicator");
			zTextBoxColumnStyleInfo2.ColumnName = "ProcessingIndicator";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|D07A77FE-CD18-49FA-B855-0534B508774D", "Message Date");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|F4112B1B-E7F9-4EFF-9780-DF2396809007", "Processing Date");
			zDateEditColumnStyleInfo2.ColumnName = "RNSProcessingDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|6934C54E-B358-41D6-A776-CE5D7E1E51D0", "Release Date");
			zDateEditColumnStyleInfo3.ColumnName = "RNSReleaseDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|EB75EEC2-A812-4BA0-BE08-58E4B95B54EB", "Service Option");
			zTextBoxColumnStyleInfo3.ColumnName = "ServiceOption";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|4F3967EE-0FCA-411D-BCC4-94C8B9EDDEE2", "Document Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "DocumentReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|6E6DA90D-B7C4-4132-B839-C1CCC25D199E", "Release Office");
			zTextBoxColumnStyleInfo5.ColumnName = "ReleaseOffice";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|F12AABAC-34B3-4EB9-A088-41629F502009", "Warehouse Code");
			zTextBoxColumnStyleInfo6.ColumnName = "WarehouseCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|8389661B-9C6C-47F9-8163-7D2B41D1E446", "CCN");
			zTextBoxColumnStyleInfo7.ColumnName = "CargoControlNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|459B70A3-28DA-4C85-B46C-42BCF43D24EB", "Container Numbers");
			zTextBoxColumnStyleInfo8.ColumnName = "ContainerNumbers";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.rNSStatusHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.rNSStatusHistoryBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rNSStatusHistoryBoundGrid.GridId = "fa17b6d7-672b-4a12-835b-3a013b41c3f9";
			this.rNSStatusHistoryBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rNSStatusHistoryBoundGrid.LayoutKey = "RNSStatusHistoryBoundGrid";
			this.rNSStatusHistoryBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.rNSStatusHistoryBoundGrid.Name = "RNSStatusHistoryBoundGrid";
			this.rNSStatusHistoryBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 252, true);
			this.rNSStatusHistoryBoundGrid.TabIndex = 0;
			// 
			// DailyNoticeDocTrasacTabPage
			// 
			this.dailyNoticeDocTrasacTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|4CD9755D-C37E-4690-B214-DFB132A5FD9B", "Daily Notice Document Transactions");
			this.dailyNoticeDocTrasacTabPage.Controls.Add(this.dNHistoryBoundGrid);
			this.dailyNoticeDocTrasacTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.dailyNoticeDocTrasacTabPage.Name = "DailyNoticeDocTrasacTabPage";
			this.dailyNoticeDocTrasacTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dailyNoticeDocTrasacTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 209, true);
			this.dailyNoticeDocTrasacTabPage.TabIndex = 2;
			// 
			// DNHistoryBoundGrid
			// 
			this.dNHistoryBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.dNHistoryBoundGrid, "DNHistoryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).DNHistoryLines);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_ProcessDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_EntryFilerCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_Status);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_PaymentType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B3_EntryProcessPort);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B3_EntryNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B3_BrokerReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountDTY);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountGST);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountGSD);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountSIM);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountEXS);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountKPM);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B4_ChargeAmountOTH);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).CustomsFeesTotal);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_AccountNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).MessageType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B3_EntryType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_StatementType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementLine)(((System.Collections.IList)(((JobDeclaration)(null)).DNHistoryLines)).SyncRoot)).B2_RMNumber);
			this.dNHistoryBoundGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|40D62DD7-6FE6-4FBA-B047-B88F1384FED2", "Accounting Date");
			zDateEditColumnStyleInfo4.ColumnName = "B2_ProcessDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|0E0D5E5D-E093-461B-B185-2788B2221378", "ASCN");
			zTextBoxColumnStyleInfo9.ColumnName = "B2_EntryFilerCode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|B5789EF9-FD11-4F80-B517-7ABE55E2AE4E", "Doc.Type");
			zTextBoxColumnStyleInfo10.ColumnName = "B2_Status";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|65348BAC-CFD1-4486-8E1E-26CBC32CCA7E", "Payment Party");
			zTextBoxColumnStyleInfo11.ColumnName = "B2_PaymentType";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|381049BF-5446-4FDF-8BD2-0AE7AC03339E", "Customs Port");
			zTextBoxColumnStyleInfo12.ColumnName = "B3_EntryProcessPort";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|3AA9A347-CFDD-42A6-B1D4-9BA730914D0B", "Document Number");
			zTextBoxColumnStyleInfo13.ColumnName = "B3_EntryNum";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|A4F2DB16-9428-44A9-968F-84CD33351650", "Reference");
			zTextBoxColumnStyleInfo14.ColumnName = "B3_BrokerReference";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|821C0FC6-1737-4834-8633-DD37DDB15C4D", "Duty");
			zTextBoxColumnStyleInfo15.ColumnName = "B4_ChargeAmountDTY";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|4A268015-F40A-4DFB-BA9C-87CF5049DD6E", "GST");
			zTextBoxColumnStyleInfo16.ColumnName = "B4_ChargeAmountGST";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|318D449E-079A-4731-B86B-E7DB953A6DB0", "GST Direct");
			zTextBoxColumnStyleInfo17.ColumnName = "B4_ChargeAmountGSD";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|7F34B3A5-919F-494D-9D28-B0743873DE12", "SIMA");
			zTextBoxColumnStyleInfo18.ColumnName = "B4_ChargeAmountSIM";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|C0326B5E-5CAD-463B-8D3E-1F1B19468FC8", "Excise Tax");
			zTextBoxColumnStyleInfo19.ColumnName = "B4_ChargeAmountEXS";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|236DD0ED-CEE0-49E7-8530-7D57E8939A18", "Late Penalty");
			zTextBoxColumnStyleInfo20.ColumnName = "B4_ChargeAmountKPM";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|CF718CB5-6D58-442B-8598-AAB552613AE6", "Other/Penalty");
			zTextBoxColumnStyleInfo21.ColumnName = "B4_ChargeAmountOTH";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|9889F031-C300-4612-8B90-11FEC8AFC477", "Total");
			zTextBoxColumnStyleInfo22.ColumnName = "CustomsFeesTotal";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|FB093732-492C-4F91-8C2A-58D3E8640F87", "Statement BN9");
			zTextBoxColumnStyleInfo23.ColumnName = "B2_AccountNo";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|21FB125D-48A6-427E-817C-31830552A204", "Message Type");
			zTextBoxColumnStyleInfo24.ColumnName = "MessageType";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|8EC30740-2F5F-4EB4-8AC9-1B37A9CDDDED", "Tran. Type");
			zTextBoxColumnStyleInfo26.ColumnName = "B3_EntryType";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo27.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|59D23914-BE54-472A-A20D-FBA1E5DCB269", "Statement Type");
			zTextBoxColumnStyleInfo27.ColumnName = "B2_StatementType";
			zTextBoxColumnStyleInfo27.IsVisible = false;
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo28.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|1F2D5D7F-AEA1-4954-974A-338847156E98", "RM Account Number");
			zTextBoxColumnStyleInfo28.ColumnName = "B2_RMNumber";
			zTextBoxColumnStyleInfo28.IsVisible = false;
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.dNHistoryBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.dNHistoryBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dNHistoryBoundGrid.GridId = "58AE7587-D546-42BA-85A8-70D537C91A5A";
			this.dNHistoryBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dNHistoryBoundGrid.LayoutKey = "DNHistoryBoundGrid";
			this.dNHistoryBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.dNHistoryBoundGrid.Name = "DNHistoryBoundGrid";
			this.dNHistoryBoundGrid.ReadOnly = true;
			this.dNHistoryBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 203, true);
			this.dNHistoryBoundGrid.TabIndex = 0;
			// 
			// NoticeTabPage
			// 
			this.noticeTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|8EC4F19C-3399-415B-8486-0ADDA0AFC0C8", "Notices");
			this.noticeTabPage.Controls.Add(this.noticesBoundGrid);
			this.noticeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.noticeTabPage.Name = "NoticeTabPage";
			this.noticeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.noticeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 232, true);
			this.noticeTabPage.TabIndex = 3;
			// 
			// NoticesBoundGrid
			// 
			this.noticesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.noticesBoundGrid, "NoticesMessagesForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).NoticesMessagesForDisplay);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NoticesMessage)(((System.Collections.IList)(((JobDeclaration)(null)).NoticesMessagesForDisplay)).SyncRoot)).EM_MessageSubType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NoticesMessage)(((System.Collections.IList)(((JobDeclaration)(null)).NoticesMessagesForDisplay)).SyncRoot)).EM_MessageDateTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NoticesMessage)(((System.Collections.IList)(((JobDeclaration)(null)).NoticesMessagesForDisplay)).SyncRoot)).RNSProcessingDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NoticesMessage)(((System.Collections.IList)(((JobDeclaration)(null)).NoticesMessagesForDisplay)).SyncRoot)).ReferenceNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NoticesMessage)(((System.Collections.IList)(((JobDeclaration)(null)).NoticesMessagesForDisplay)).SyncRoot)).StatusDescription);
			this.noticesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo29.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|0F731B94-5E9A-4683-A2B1-BCE59B013521", "Message Sub-Type");
			zTextBoxColumnStyleInfo29.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|5F9D4648-A620-4A18-BDBA-4B1BA84B5099", "Message Date");
			zDateEditColumnStyleInfo5.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|6E10E249-E725-48C5-8C9B-9705952DF9EE", "Processing Date");
			zDateEditColumnStyleInfo6.ColumnName = "RNSProcessingDate";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo30.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|FBC3793C-1862-4C9B-8550-3C3E2A874C6C", "Reference Number");
			zTextBoxColumnStyleInfo30.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo31.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|FFA7A828-32F2-441C-88C2-4D8AA354CCCD", "Status Description");
			zTextBoxColumnStyleInfo31.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.noticesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.noticesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.noticesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.noticesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.noticesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.noticesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.noticesBoundGrid.GridId = "F2DED682-5661-4810-AEF5-E56F6C530921";
			this.noticesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.noticesBoundGrid.LayoutKey = "NoticesBoundGrid";
			this.noticesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.noticesBoundGrid.Name = "NoticesBoundGrid";
			this.noticesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 226, true);
			this.noticesBoundGrid.TabIndex = 0;
			this.noticesBoundGrid.DoubleClick += new EventHandler(this.NoticesBoundGrid_Click);
			// 
			// B2sTabPage
			// 
			this.b2sTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|0A993BFD-B53E-4F34-93E2-71AD9690369F", "B2s");
			this.b2sTabPage.Controls.Add(this.b2sBoundGrid);
			this.b2sTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.b2sTabPage.Name = "B2sTabPage";
			this.b2sTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.b2sTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 232, true);
			this.b2sTabPage.TabIndex = 4;
			// 
			// B2sBoundGrid
			// 
			this.b2sBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.b2sBoundGrid, "B2s");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).B2s);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).JE_MessageType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).JE_DeclarationReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).FormattedTransactionNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).Importer.OH_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).ImporterName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).B3EntrySubmittedDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).CA_B2AcceptedDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).CA_IsOurFault);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).CA_InitiatedBy);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((System.Collections.IList)(((JobDeclaration)(null)).B2s)).SyncRoot)).CA_B2Total);
			this.b2sBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo32.ColumnName = "JE_MessageType";
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo33.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|A03E0893-E677-488E-8625-9D93B0AD28B4", "DeclarationNumber");
			zTextBoxColumnStyleInfo33.ColumnName = "JE_DeclarationReference";
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo34.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|4D72E59F-DDC6-488E-9B21-AB8B5CEBF6C4", "Transaction No.");
			zTextBoxColumnStyleInfo34.ColumnName = "FormattedTransactionNumber";
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo35.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|155424E8-7DC2-4A33-8439-EFADA7115C3C", "Importer Code");
			zTextBoxColumnStyleInfo35.ColumnName = "Importer+OH_Code";
			zTextBoxColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo36.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|00EB515F-2CDF-4609-9F2B-5C4815CD9022", "Importer Name");
			zTextBoxColumnStyleInfo36.ColumnName = "ImporterName";
			zTextBoxColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|5ACF96BD-CC93-461A-96AA-6E7F57390701", "Date Submitted");
			zDateEditColumnStyleInfo7.ColumnName = "B3EntrySubmittedDate";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|67F27CAA-9381-4084-B02E-8F2FFB54B251", "Date Accepted");
			zDateEditColumnStyleInfo8.ColumnName = "CA_B2AcceptedDate";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|34CCE7CA-61D9-477F-9754-E24CC761C168", "Broker Issue");
			zCheckBoxColumnStyleInfo1.ColumnName = "CA_IsOurFault";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo37.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|9552765F-7A24-4508-BD80-FC4C850F10AB", "Who\'s initiative");
			zTextBoxColumnStyleInfo37.ColumnName = "CA_InitiatedBy";
			zTextBoxColumnStyleInfo37.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|0970F4C9-DD59-4B66-8A9E-80BEF866A841", "B2 Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CA_B2Total";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);
			this.b2sBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.b2sBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.b2sBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.b2sBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);
			this.b2sBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.b2sBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.b2sBoundGrid.GridId = "0CE03B8F-B654-4D04-871F-B0025E33D953";
			this.b2sBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.b2sBoundGrid.LayoutKey = "B2sBoundGrid";
			this.b2sBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.b2sBoundGrid.Name = "B2sBoundGrid";
			this.b2sBoundGrid.ReadOnly = true;
			this.b2sBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 226, true);
			this.b2sBoundGrid.TabIndex = 0;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.declarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("FrontPageUserControl|1FC38EA0-B22F-47A2-BB3E-F63E5F030465", "Declaration Details");
			this.declarationDetailsGroupBox.Controls.Add(this.k84DeclarationDetailsUserControl);
			this.declarationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.declarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.declarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.declarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 221, true);
			this.declarationDetailsGroupBox.TabIndex = 0;
			this.declarationDetailsGroupBox.TabStop = false;
			// 
			// K84DeclarationDetailsUserControl
			// 
			this.k84DeclarationDetailsUserControl.AllowDrop = true;
			this.k84DeclarationDetailsUserControl.CaptionRenderingEnabled = true;
			this.BindingSource.SetBindingMember(this.k84DeclarationDetailsUserControl, ".");
			this.k84DeclarationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.k84DeclarationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.k84DeclarationDetailsUserControl.Name = "K84DeclarationDetailsUserControl";
			this.k84DeclarationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 202, true);
			this.k84DeclarationDetailsUserControl.TabIndex = 0;
			// 
			// K84UserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.noticesTabControl);
			this.Controls.Add(this.horizontalSplitter);
			this.Controls.Add(this.k84GroupBox);
			this.Controls.Add(this.declarationDetailsGroupBox);
			this.Name = "K84UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 596, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.k84GroupBox.ResumeLayout(false);
			this.k84GroupBox.PerformLayout();
			this.cA_K84StatementDateBoundDateEdit.ResumeLayout(true);
			this.cA_K84StatementDateBoundDateEdit.PerformLayout();
			this.cA_K84AccountingDateBoundDateEdit.ResumeLayout(true);
			this.cA_K84AccountingDateBoundDateEdit.PerformLayout();
			this.noticesTabControl.ResumeLayout(false);
			this.noticesTabControl.PerformLayout();
			this.CCNTabPage.ResumeLayout(false);
			this.CCNTabPage.PerformLayout();
			this.cCNGroupBox.ResumeLayout(false);
			this.cCNGroupBox.PerformLayout();
			this.CCNUserControl.ResumeLayout(true);
			this.CCNUserControl.PerformLayout();
			this.rNSStatusHistoryTabPage.ResumeLayout(false);
			this.rNSStatusHistoryTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.rNSStatusHistoryBoundGrid)).EndInit();
			this.rNSStatusHistoryBoundGrid.ResumeLayout(false);
			this.rNSStatusHistoryBoundGrid.PerformLayout();
			this.dailyNoticeDocTrasacTabPage.ResumeLayout(false);
			this.dailyNoticeDocTrasacTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dNHistoryBoundGrid)).EndInit();
			this.dNHistoryBoundGrid.ResumeLayout(false);
			this.dNHistoryBoundGrid.PerformLayout();
			this.noticeTabPage.ResumeLayout(false);
			this.noticeTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.noticesBoundGrid)).EndInit();
			this.noticesBoundGrid.ResumeLayout(false);
			this.noticesBoundGrid.PerformLayout();
			this.b2sTabPage.ResumeLayout(false);
			this.b2sTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.b2sBoundGrid)).EndInit();
			this.b2sBoundGrid.ResumeLayout(false);
			this.b2sBoundGrid.PerformLayout();
			this.declarationDetailsGroupBox.ResumeLayout(false);
			this.declarationDetailsGroupBox.PerformLayout();
			this.k84DeclarationDetailsUserControl.ResumeLayout(true);
			this.k84DeclarationDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
