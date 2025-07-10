using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque
{
	public partial class AccHotChequeForm
	{


		#region Windows Form Designer generated code

		private ZGuidFindBox AQ_OHBoundFindBox;
		private ZTextBox AQ_Calc_ChequeStatusBoundTextBox;
		private ZGuidFindBox AQ_JHBoundFindBox;
		private ZTextBox AQ_ChequePayeeBoundTextBox;
		private ZTextBox AQ_MasterBillBoundTextBox;
		internal ZRadioButton AQ_Calc_ActualAmountIndicatorBoundRadioButton;
		internal ZRadioButton AQ_Calc_MaximumAmountIndicatorBoundRadioButton;
		private ZDateEdit AQ_ChequeDateBoundDateEdit;
		private ZCodeFindBox AQ_GS_NKResponsibleStaffBoundCodeFindBox;
		private ZTextBox AQ_DescriptionBoundTextBox;
		private ZTextBox AQ_HouseBillBoundTextBox;
		private ZGuidFindBox AQ_AKBoundFindBox;
		private ZCalcFindBox AQ_AmountBoundCalcFindBox;
		private ZTextBox AQ_ChequeNumberBoundTextBox;
		private ZTemplateTabControl MainTabControl;
		private ZTabPage MainTabPage;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZLabel AutoAllocateZLabel;
		private ZLabel AutoPrintZLabel;
		private ZLogsTabPage EventTabPage;
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.AQ_ChequeDateBoundDateEdit = new ZDateEdit();
			this.AQ_OHBoundFindBox = new ZGuidFindBox();
			this.AQ_AKBoundFindBox = new ZGuidFindBox();
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox = new ZCodeFindBox();
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton = new ZRadioButton();
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton = new ZRadioButton();
			this.AQ_JHBoundFindBox = new ZGuidFindBox();
			this.AQ_DescriptionBoundTextBox = new ZTextBox();
			this.AQ_Calc_ChequeStatusBoundTextBox = new ZTextBox();
			this.AQ_ChequePayeeBoundTextBox = new ZTextBox();
			this.AQ_MasterBillBoundTextBox = new ZTextBox();
			this.AQ_HouseBillBoundTextBox = new ZTextBox();
			this.AQ_AmountBoundCalcFindBox = new ZCalcFindBox();
			this.AQ_ChequeNumberBoundTextBox = new ZTextBox();
			this.MainTabControl = new ZTemplateTabControl();
			this.MainTabPage = new ZTabPage();
			this.AutoAllocateZLabel = new ZLabel();
			this.AutoPrintZLabel = new ZLabel();
			this.EventTabPage = new ZLogsTabPage();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(296);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccHotCheque);
			// 
			// AQ_ChequeDateBoundDateEdit
			// 
			this.AQ_ChequeDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AQ_ChequeDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AQ_ChequeDateBoundDateEdit, "AQ_ChequeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccHotCheque)(null)).AQ_ChequeDate)));
			this.AQ_ChequeDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 11, true);
			this.AQ_ChequeDateBoundDateEdit.Name = "AQ_ChequeDateBoundDateEdit";
			this.AQ_ChequeDateBoundDateEdit.TabIndex = 1;
			// 
			// AQ_OHBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AQ_OHBoundFindBox, "AQ_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccHotCheque)(null)).AQ_OH)));
			this.AQ_OHBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 33, true);
			this.AQ_OHBoundFindBox.Name = "AQ_OHBoundFindBox";
			this.AQ_OHBoundFindBox.PopupCaption = null;
			this.AQ_OHBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_OHBoundFindBox.TabIndex = 5;
			// 
			// AQ_AKBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AQ_AKBoundFindBox, "AQ_AK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccHotCheque)(null)).AQ_AK)));
			this.AQ_AKBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 55, true);
			this.AQ_AKBoundFindBox.Name = "AQ_AKBoundFindBox";
			this.AQ_AKBoundFindBox.PopupCaption = null;
			this.AQ_AKBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 21, true);
			this.AQ_AKBoundFindBox.TabIndex = 7;
			// 
			// AQ_GS_NKResponsibleStaffBoundCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.AQ_GS_NKResponsibleStaffBoundCodeFindBox, "AQ_GS_NKResponsibleStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_GS_NKResponsibleStaff)));
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 241, true);
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox.Name = "AQ_GS_NKResponsibleStaffBoundCodeFindBox";
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox.PopupCaption = null;
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_GS_NKResponsibleStaffBoundCodeFindBox.TabIndex = 25;
			// 
			// AQ_Calc_MaximumAmountIndicatorBoundRadioButton
			// 
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton, "AQ_Calc_MaximumAmountIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((AccHotCheque)(null)).AQ_Calc_MaximumAmountIndicator)));
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|c12726a7-62a0-4389-ba10-989ad3d85dbe", "Maximum Amount");
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 126, true);
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Name = "AQ_Calc_MaximumAmountIndicatorBoundRadioButton";
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton.TabIndex = 14;
			// 
			// AQ_Calc_ActualAmountIndicatorBoundRadioButton
			// 
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.AQ_Calc_ActualAmountIndicatorBoundRadioButton, "AQ_Calc_ActualAmountIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((AccHotCheque)(null)).AQ_Calc_ActualAmountIndicator)));
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Checked = true;
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|4251410b-b943-4475-8666-ce20348a2a9f", "Actual Amount");
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 126, true);
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Name = "AQ_Calc_ActualAmountIndicatorBoundRadioButton";
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.TabIndex = 13;
			this.AQ_Calc_ActualAmountIndicatorBoundRadioButton.TabStop = true;
			// 
			// AQ_JHBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AQ_JHBoundFindBox, "AQ_JH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccHotCheque)(null)).AQ_JH)));
			this.AQ_JHBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 152, true);
			this.AQ_JHBoundFindBox.Name = "AQ_JHBoundFindBox";
			this.AQ_JHBoundFindBox.PopupCaption = null;
			this.AQ_JHBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_JHBoundFindBox.TabIndex = 17;
			// 
			// AQ_DescriptionBoundTextBox
			// 
			this.AQ_DescriptionBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AQ_DescriptionBoundTextBox, "AQ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_Description)));
			this.AQ_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AQ_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 219, true);
			this.AQ_DescriptionBoundTextBox.Name = "AQ_DescriptionBoundTextBox";
			this.AQ_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_DescriptionBoundTextBox.TabIndex = 23;
			// 
			// AQ_Calc_ChequeStatusBoundTextBox
			// 
			this.AQ_Calc_ChequeStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AQ_Calc_ChequeStatusBoundTextBox, "AQ_Calc_ChequeStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_Calc_ChequeStatus)));
			this.AQ_Calc_ChequeStatusBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|0031b6b6-e56f-4ac5-95ba-6996122936a3", "Check Status");
			this.AQ_Calc_ChequeStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 11, true);
			this.AQ_Calc_ChequeStatusBoundTextBox.Name = "AQ_Calc_ChequeStatusBoundTextBox";
			this.AQ_Calc_ChequeStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.AQ_Calc_ChequeStatusBoundTextBox.TabIndex = 2;
			// 
			// AQ_ChequePayeeBoundTextBox
			// 
			this.AQ_ChequePayeeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AQ_ChequePayeeBoundTextBox, "AQ_ChequePayee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_ChequePayee)));
			this.AQ_ChequePayeeBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AQ_ChequePayeeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 99, true);
			this.AQ_ChequePayeeBoundTextBox.Name = "AQ_ChequePayeeBoundTextBox";
			this.AQ_ChequePayeeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_ChequePayeeBoundTextBox.TabIndex = 11;
			// 
			// AQ_MasterBillBoundTextBox
			// 
			this.AQ_MasterBillBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AQ_MasterBillBoundTextBox, "AQ_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_MasterBill)));
			this.AQ_MasterBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 175, true);
			this.AQ_MasterBillBoundTextBox.Name = "AQ_MasterBillBoundTextBox";
			this.AQ_MasterBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_MasterBillBoundTextBox.TabIndex = 19;
			// 
			// AQ_HouseBillBoundTextBox
			// 
			this.AQ_HouseBillBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AQ_HouseBillBoundTextBox, "AQ_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_HouseBill)));
			this.AQ_HouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 197, true);
			this.AQ_HouseBillBoundTextBox.Name = "AQ_HouseBillBoundTextBox";
			this.AQ_HouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.AQ_HouseBillBoundTextBox.TabIndex = 21;
			// 
			// AQ_AmountBoundCalcFindBox
			// 
			this.AQ_AmountBoundCalcFindBox.BindToAmount = "AQ_Amount";
			this.AQ_AmountBoundCalcFindBox.BindToUnit = "AQ_Calc_RX_NK";
			this.AQ_AmountBoundCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AQ_AmountBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 126, true);
			this.AQ_AmountBoundCalcFindBox.Name = "AQ_AmountBoundCalcFindBox";
			this.AQ_AmountBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.AQ_AmountBoundCalcFindBox.TabIndex = 15;
			// 
			// AQ_ChequeNumberBoundTextBox
			// 
			this.AQ_ChequeNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AQ_ChequeNumberBoundTextBox, "AQ_ChequeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).AQ_ChequeNumber)));
			this.AQ_ChequeNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 77, true);
			this.AQ_ChequeNumberBoundTextBox.Name = "AQ_ChequeNumberBoundTextBox";
			this.AQ_ChequeNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.AQ_ChequeNumberBoundTextBox.TabIndex = 9;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.EventTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 338, true);
			this.MainTabControl.TabIndex = 1;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|29ebb0b1-6de2-4172-b333-662810b7de4f", "Hot Check");
			this.MainTabPage.Controls.Add(this.AutoAllocateZLabel);
			this.MainTabPage.Controls.Add(this.AutoPrintZLabel);
			this.MainTabPage.Controls.Add(this.AQ_ChequePayeeBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_Calc_ChequeStatusBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_DescriptionBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_OHBoundFindBox);
			this.MainTabPage.Controls.Add(this.AQ_AKBoundFindBox);
			this.MainTabPage.Controls.Add(this.AQ_AmountBoundCalcFindBox);
			this.MainTabPage.Controls.Add(this.AQ_ChequeNumberBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_GS_NKResponsibleStaffBoundCodeFindBox);
			this.MainTabPage.Controls.Add(this.AQ_Calc_ActualAmountIndicatorBoundRadioButton);
			this.MainTabPage.Controls.Add(this.AQ_Calc_MaximumAmountIndicatorBoundRadioButton);
			this.MainTabPage.Controls.Add(this.AQ_MasterBillBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_HouseBillBoundTextBox);
			this.MainTabPage.Controls.Add(this.AQ_JHBoundFindBox);
			this.MainTabPage.Controls.Add(this.AQ_ChequeDateBoundDateEdit);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 311, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|339015a4-433c-4203-b0a1-0602e468b1e7", "Auto Allocate");
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 80, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.AutoAllocateZLabel.TabIndex = 27;
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccHotCheque)(null)).Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|7381ff8d-8047-477c-8888-ccdd121b6471", "Auto Print");
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 58, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.AutoPrintZLabel.TabIndex = 26;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 311, true);
			this.EventTabPage.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 344, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// AccHotChequeForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 393, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccHotChequeForm|cf20bbca-6b28-45db-83d2-bfe4a0bdca8f", "Hot Check");
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(AccHotCheque);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.HotCheque.AccHotCheque";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "AccHotChequeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}