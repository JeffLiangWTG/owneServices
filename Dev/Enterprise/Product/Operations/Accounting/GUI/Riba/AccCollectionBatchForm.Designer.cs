namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionBatchForm
	{


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new MasterFiles.GUI.ZWorkflowTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.PostingControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.PostingControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 357, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 23, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.AccCollectionBatch);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 360, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccCollectionBatchForm|dcc4e54e-b81f-489e-8b39-82f4aa25e203", "Collection Batch");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 339, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).IncludeInOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).CollectionAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).CollectionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).JobInvoicingNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).AOL_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).OSInvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).OSCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).LocalInvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).LocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionOrderLines)).SyncRoot)).LocalCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).IncludeInBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_OH_Debtor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_CollectionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_DepositedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).ACO_CancelledReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestBankBsb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestAccountName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestBankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestBankSwift)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestBankCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestIBANNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).CollectionOrders)).SyncRoot)).CollectionRequestAccountCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).ACB_AB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).ACB_RX_NKCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).ACB_BatchNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Riba.AccCollectionBatch)(null)).ACB_CollectionFileFormat)));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 339, true);
			this.WorkflowTabPage.TabIndex = 2;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 339, true);
			this.zLogsTabPage1.TabIndex = 1;
			// 
			// PostingControl
			// 
			this.PostingControl.AllowDrop = true;
			this.PostingControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 5, true);
			this.PostingControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingControl.Name = "PostingControl";
			this.PostingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 27, true);
			this.PostingControl.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 380, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// AccCollectionBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("59220cef-b195-4258-8961-dfece450668d", "Collection Batch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 415, true);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.AccCollectionBatch);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Riba.AccCollectionBatch";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 290, true);
			this.Name = "AccCollectionBatchForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.PostingControl.ResumeLayout(true);
			this.PostingControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CollectionOrdersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrderLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OrdersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TotalAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.BankAccountBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BatchNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CollectionFileFormatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BatchTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabPage.SuspendLayout();
			this.CollectionOrdersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).BeginInit();
			this.OrderLinesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid)).BeginInit();
			this.OrdersGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.TotalAmountCalcFindBox.SuspendLayout();
			this.BankAccountBoundGuidFindBox.SuspendLayout();
			this.MainCurrencyCodeFindBox.SuspendLayout();
			this.CollectionFileFormatDropEdit.SuspendLayout();
			this.BatchTypeDropEdit.SuspendLayout();
			this.MainTabPage.Controls.Add(this.CollectionOrdersGroupBox);
			this.MainTabPage.Controls.Add(this.TopPanel);
			// 
			// CollectionOrdersGroupBox
			// 
			this.CollectionOrdersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ad3dce71-298a-4272-af71-cc88358ceaa5", "Orders");
			this.CollectionOrdersGroupBox.Controls.Add(this.OrderLinesGrid);
			this.CollectionOrdersGroupBox.Controls.Add(this.OrdersGrid);
			this.CollectionOrdersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CollectionOrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.CollectionOrdersGroupBox.Name = "CollectionOrdersGroupBox";
			this.CollectionOrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 276, true);
			this.CollectionOrdersGroupBox.TabIndex = 1;
			this.CollectionOrdersGroupBox.TabStop = false;
			// 
			// OrderLinesGrid
			// 
			this.OrderLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrderLinesGrid, "CollectionOrders.CollectionOrderLines");
			this.OrderLinesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("66afaeaf-d371-4657-867e-0e84e2a594d7", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInOrder";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bf53add9-7cd9-412e-8e76-8db189b27e2f", "Transaction No.");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e83db575-278e-4f84-b280-b0f2d9229899", "Transaction Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ed5179e-7748-4acb-a35c-daccfcd600d5", "Collection Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CollectionAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e5eac8fe-1b6f-4241-8b25-46ef6de07a9c", "Collection Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "CollectionCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("00e3f09a-ebd7-47ed-8ddb-e334662fa5e1", "Job Invoicing No.");
			zTextBoxColumnStyleInfo4.ColumnName = "JobInvoicingNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4b06bc8-3933-4c4b-895e-342f440b8673", "Invoice Date");
			zDateEditColumnStyleInfo1.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6d30ab98-0866-49bb-8a49-c0814b15d02a", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "PostDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a62dc198-0923-4eb6-ac16-2e49cba093e9", "Due Date");
			zDateEditColumnStyleInfo3.ColumnName = "DueDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("baccdca4-8c53-4a6b-a3db-623c0bdbc26f", "Is Rejected");
			zCheckBoxColumnStyleInfo2.ColumnName = "AOL_IsCancelled";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5c86323-2305-46c2-82f7-6611905a4649", "OS Invoice Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "OSInvoiceAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("91c0590c-c882-4638-b00a-141a0053e7ce", "OS Outstanding Amt");
			zCalcEditColumnStyleInfo3.ColumnName = "OSOutstandingAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("658d5ffe-191e-4a6d-a1b3-0bbb7834819e", "Invoice Currency");
			zTextBoxColumnStyleInfo5.ColumnName = "OSCurrency";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e83c0b9a-24ea-4aa0-8483-0ea545fddd14", "Local Invoice Amt");
			zCalcEditColumnStyleInfo4.ColumnName = "LocalInvoiceAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7d6a0d5e-9bd2-481c-9657-89558a9d081b", "Local Outstanding Amt");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalOutstandingAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ec9077e-3d2f-4de4-a89e-4ac4d9c1d3bd", "Local Currency");
			zTextBoxColumnStyleInfo6.ColumnName = "LocalCurrency";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrderLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrderLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OrderLinesGrid.CopySelectedRowsAllowed = true;
			this.OrderLinesGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OrderLinesGrid.GridId = "192F619C-3BB9-40B4-9C6F-36E39312D8BC";
			this.OrderLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderLinesGrid.LayoutKey = "TransactionsGrid";
			this.OrderLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 105, true);
			this.OrderLinesGrid.Name = "OrderLinesGrid";
			this.OrderLinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OrderLinesGrid.ShouldSetErrorsOnTabPage = false;
			this.OrderLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 169, true);
			this.OrderLinesGrid.TabIndex = 1;
			// 
			// OrdersGrid
			// 
			this.OrdersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrdersGrid, "CollectionOrders");
			this.OrdersGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("860eb0d2-d637-4ba8-90dd-85c623a6d379", "Include");
			zCheckBoxColumnStyleInfo3.ColumnName = "IncludeInBatch";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ACO_OrderNumber";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ACO_OH_Debtor";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "ACO_Amount";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "ACO_CollectionDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.ColumnName = "ACO_DepositedDate";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo9.ColumnName = "ACO_RX_NKCurrency";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("70c43013-409f-4b5c-a582-f90b7660e48e", "Is Rejected");
			zCheckBoxColumnStyleInfo4.ColumnName = "ACO_IsCancelled";
			zCheckBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4646dce1-d8d9-4a25-a176-f77cb64f448a", "Rejected Reason");
			zTextBoxColumnStyleInfo10.ColumnName = "ACO_CancelledReason";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4330210b-b71e-4386-8faf-336a254dc3bd", "Bank/Branch Code");
			zTextBoxColumnStyleInfo11.ColumnName = "CollectionRequestBankBsb";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8d1193f6-220b-4629-bc3e-5d4ee8ba551b", "Account Name");
			zTextBoxColumnStyleInfo12.ColumnName = "CollectionRequestAccountName";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1022d088-73b1-4f8d-bd00-029fd439436e", "Account Number");
			zTextBoxColumnStyleInfo13.ColumnName = "CollectionRequestAccountNumber";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5b6101c2-0a95-4749-bd80-48a596fe6038", "Bank Name");
			zTextBoxColumnStyleInfo14.ColumnName = "CollectionRequestBankName";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5311cd23-eafd-43e3-a0f1-9136781258ce", "Bank Swift");
			zTextBoxColumnStyleInfo15.ColumnName = "CollectionRequestBankSwift";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("eaa0cbf6-b861-4d0b-ac73-f6e5eebf0597", "Bank Country/Region");
			zTextBoxColumnStyleInfo16.ColumnName = "CollectionRequestBankCountry";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c2aa183e-f96d-49da-852e-7e3f07aa7ffa", "IBAN Number");
			zTextBoxColumnStyleInfo17.ColumnName = "CollectionRequestIBANNumber";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cafcdcbf-ef2d-4bcc-8e6a-8b11d745bed8", "Account Currency");
			zTextBoxColumnStyleInfo18.ColumnName = "CollectionRequestAccountCurrency";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrdersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OrdersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OrdersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.OrdersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OrdersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.OrdersGrid.CopySelectedRowsAllowed = true;
			this.OrdersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGrid.GridId = "47879719-cb1e-4664-8cb3-8426eaefeaed";
			this.OrdersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdersGrid.LayoutKey = "TransactionsGrid";
			this.OrdersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.OrdersGrid.Name = "OrdersGrid";
			this.OrdersGrid.ShouldSetErrorsOnTabPage = false;
			this.OrdersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 259, true);
			this.OrdersGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.CollectionFileFormatDropEdit);
			this.TopPanel.Controls.Add(this.BatchTypeDropEdit);
			this.TopPanel.Controls.Add(this.TotalAmountCalcFindBox);
			this.TopPanel.Controls.Add(this.BankAccountBoundGuidFindBox);
			this.TopPanel.Controls.Add(this.MainCurrencyCodeFindBox);
			this.TopPanel.Controls.Add(this.BatchNumberTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 63, true);
			this.TopPanel.TabIndex = 0;
			// 
			// TotalAmountCalcFindBox
			// 
			this.TotalAmountCalcFindBox.AllowDrop = true;
			this.TotalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalAmountCalcFindBox.BindToAmount = "ACB_TotalAmount";
			this.TotalAmountCalcFindBox.BindToDecimalPlaces = "ACB_Calc_RXDecimals";
			this.TotalAmountCalcFindBox.BindToUnit = "ACB_RX_NKCurrency";
			this.TotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 35, true);
			this.TotalAmountCalcFindBox.Name = "TotalAmountCalcFindBox";
			this.TotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.TotalAmountCalcFindBox.TabIndex = 8;
			// 
			// BankAccountBoundGuidFindBox
			// 
			this.BankAccountBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountBoundGuidFindBox, "ACB_AB");
			this.BankAccountBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 12, true);
			this.BankAccountBoundGuidFindBox.Name = "BankAccountBoundGuidFindBox";
			this.BankAccountBoundGuidFindBox.PopupCaption = null;
			this.BankAccountBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 18, true);
			this.BankAccountBoundGuidFindBox.TabIndex = 7;
			// 
			// MainCurrencyCodeFindBox
			// 
			this.MainCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MainCurrencyCodeFindBox, "ACB_RX_NKCurrency");
			this.MainCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 34, true);
			this.MainCurrencyCodeFindBox.Name = "MainCurrencyCodeFindBox";
			this.MainCurrencyCodeFindBox.PopupCaption = null;
			this.MainCurrencyCodeFindBox.PreBoundMaxLength = 4;
			this.MainCurrencyCodeFindBox.ShowDescriptionBox = false;
			this.MainCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 18, true);
			this.MainCurrencyCodeFindBox.TabIndex = 3;
			// 
			// BatchNumberTextBox
			// 
			this.BatchNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "ACB_BatchNumber");
			this.BatchNumberTextBox.EnableValidStateColor = false;
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(687, 12, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.ReadOnly = true;
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.BatchNumberTextBox.TabIndex = 0;
			// 
			// CollectionFileFormatDropEdit
			// 
			this.CollectionFileFormatDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CollectionFileFormatDropEdit, "ACB_CollectionFileFormat");
			this.CollectionFileFormatDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b4c26219-74a6-4684-9953-b22918d24b23", "File Format");
			this.CollectionFileFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 35, true);
			this.CollectionFileFormatDropEdit.Name = "CollectionFileFormatDropEdit";
			this.CollectionFileFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 18, true);
			this.CollectionFileFormatDropEdit.TabIndex = 4;
			// 
			// BatchTypeDropEdit
			// 
			this.BatchTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BatchTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BatchTypeDropEdit, "ACB_Type");
			this.BatchTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 12, true);
			this.BatchTypeDropEdit.Name = "BatchTypeDropEdit";
			this.BatchTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 18, true);
			this.BatchTypeDropEdit.TabIndex = 5;

			this.MainTabPage.PerformLayout();
			this.CollectionOrdersGroupBox.ResumeLayout(false);
			this.CollectionOrdersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).EndInit();
			this.OrderLinesGrid.ResumeLayout(false);
			this.OrderLinesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid)).EndInit();
			this.OrdersGrid.ResumeLayout(false);
			this.OrdersGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.TotalAmountCalcFindBox.ResumeLayout(true);
			this.TotalAmountCalcFindBox.PerformLayout();
			this.BankAccountBoundGuidFindBox.ResumeLayout(true);
			this.BankAccountBoundGuidFindBox.PerformLayout();
			this.MainCurrencyCodeFindBox.ResumeLayout(true);
			this.MainCurrencyCodeFindBox.PerformLayout();
			this.CollectionFileFormatDropEdit.ResumeLayout(true);
			this.CollectionFileFormatDropEdit.PerformLayout();
			this.BatchTypeDropEdit.ResumeLayout(true);
			this.BatchTypeDropEdit.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox CollectionOrdersGroupBox;
		internal Enterprise.ZArchitecture.ZGrid OrdersGrid;
		System.ComponentModel.IContainer components;
		Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		Enterprise.ZArchitecture.ZTextBox BatchNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		Enterprise.ZArchitecture.ZGrid OrderLinesGrid;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox MainCurrencyCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox BankAccountBoundGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox TotalAmountCalcFindBox;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingControl;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

	}
}
