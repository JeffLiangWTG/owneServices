using System;
using System.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class OpeningReceiptForm
	{


		#region Windows Form Designer generated code

		private ZTemplateTabControl zTabControl;
		private ZLogsTabPage zEventTabPage1;
		private ZTabPage OpeningReceiptTabPage;
		private ZGroupBox ReceiptDetailsGroupBox;
		private ZGroupBox BankDetailsGroupBox;
		private ZDateEdit InvoiceDateEdit;
		private ZDateEdit PostDateEdit;
		private ZGuidFindBox OrganisationGuidFindBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZDropEdit ReceiptTypeDropEdit;
		private ZGuidFindBox BankAccountGuidFindBox;
		private ZArchitecture.ZTextBox ChequeNumberTextBox;
		private ZGroupBox ReceiptAmountGroupBox;
		private ZExchangeRateControl ExchangeRateControl;
		private ZCalcFindBox OSAmountCalcFindBox;
		private ZCalcFindBox LocalAmountCalcFindBox;
		private ZGroupBox ChequeDetailsGroupBox;
		private ZArchitecture.ZTextBox ChequeDrawerTextBox;
		private ZArchitecture.ZTextBox DrawerBankTextBox;
		private ZArchitecture.ZTextBox DrawerBranchTextBox;
		private ZArchitecture.ZTextBox TransactionNumTextBox;
		private ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.zTabControl = new ZTemplateTabControl();
			this.OpeningReceiptTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.oPostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 479, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OpeningReceipt);
			// 
			// zTabControl
			// 
			this.zTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl.Controls.Add(this.OpeningReceiptTabPage);
			this.zTabControl.Controls.Add(this.zEventTabPage1);
			this.zTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl.Name = "zTabControl";
			this.zTabControl.SelectedIndex = 0;
			this.zTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 444, true);
			this.zTabControl.TabIndex = 1;
			// 
			// OpeningReceiptTabPage
			// 
			this.OpeningReceiptTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|a39d3820-9181-4942-b612-bc851a9a25dc", "Opening Receipt");
			this.OpeningReceiptTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OpeningReceiptTabPage.Name = "OpeningReceiptTabPage";
			this.OpeningReceiptTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 417, true);
			this.OpeningReceiptTabPage.TabIndex = 0;
			this.OpeningReceiptTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.OpeningReceiptTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_DrawerBranch)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_DrawerBank)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_ChequeDrawer)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((OpeningReceipt)(null)).ExchangeRate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_ChequeOrReference)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((OpeningReceipt)(null)).AH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningReceipt)(null)).Lookups.BankAccounts)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningReceipt)(null)).AH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningReceipt)(null)).ReceiptMethods)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_TransactionNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningReceipt)(null)).AH_Desc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((OpeningReceipt)(null)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningReceipt)(null)).Lookups.Headers)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningReceipt)(null)).AH_PostDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningReceipt)(null)).AH_InvoiceDate)));
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 417, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.oPostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 444, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 6, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.oPostingButtonsUserControl.TabIndex = 3;
			// 
			// OpeningReceiptForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 503, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|878f993c-3a2d-4137-94fe-c073b74dee48", "Opening Receipt");
			this.Controls.Add(this.zTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(OpeningReceipt);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.OpeningReceipt.OpeningReceipt";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 528, true);
			this.Name = "OpeningReceiptForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.zTabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}