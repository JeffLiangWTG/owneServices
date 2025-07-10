using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class BulkDSBJobCloseBatchApprovalForm
	{


		#region Windows Form Designer generated code

		protected Container components = null;

		public ZPostingButtonsUserControl PostingButtonsUserControl;
		protected ZArchitecture.ZTextBox LargestDSBBalanceTextBox;
		protected ZArchitecture.ZTextBox SmallestDSBBalanceTextBox;
		protected ZArchitecture.ZTextBox TotalDSBBalanceTextBox;
		protected ZArchitecture.ZTextBox BatchNumberTextBox;
		protected ZPanel FilterPanel;
		protected ZPanel BottomButtonPanel;
		protected ZGroupBox DisbursementClearingAggregatedBalanceGroupBox;
		protected ZCalcFindBox BalanceCalcFindBox;
		protected ZPanel TopPanel;
		protected ZDateEdit CreateTimeDateEdit;
		protected ZDropEdit StatusDropEdit;
		protected ZCodeFindBox ApprovingUserGuidFindBox;
		protected ZDateEdit ApprovalDateDateEdit;
		protected BulkDSBJobCloseBatchApprovalUserControl bulkDSBJobCloseBatchApprovalUserControl;
		protected ZGroupBox GridGroupBox;

		new void InitializeComponent()
		{
			this.FilterPanel = new ZPanel();
			this.DisbursementClearingAggregatedBalanceGroupBox = new ZGroupBox();
			this.BalanceCalcFindBox = new ZCalcFindBox();
			this.BottomButtonPanel = new ZPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			this.TopPanel = new ZPanel();
			this.StatusDropEdit = new ZDropEdit();
			this.ApprovingUserGuidFindBox = new ZCodeFindBox();
			this.ApprovalDateDateEdit = new ZDateEdit();
			this.LargestDSBBalanceTextBox = new ZArchitecture.ZTextBox();
			this.SmallestDSBBalanceTextBox = new ZArchitecture.ZTextBox();
			this.TotalDSBBalanceTextBox = new ZArchitecture.ZTextBox();
			this.BatchNumberTextBox = new ZArchitecture.ZTextBox();
			this.CreateTimeDateEdit = new ZDateEdit();
			this.GridGroupBox = new ZGroupBox();
			this.bulkDSBJobCloseBatchApprovalUserControl = new BulkDSBJobCloseBatchApprovalUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterPanel.SuspendLayout();
			this.DisbursementClearingAggregatedBalanceGroupBox.SuspendLayout();
			this.BalanceCalcFindBox.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.ApprovingUserGuidFindBox.SuspendLayout();
			this.ApprovalDateDateEdit.SuspendLayout();
			this.CreateTimeDateEdit.SuspendLayout();
			this.GridGroupBox.SuspendLayout();
			this.bulkDSBJobCloseBatchApprovalUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 685, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DsbJobCloseBatch);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.bulkDSBJobCloseBatchApprovalUserControl);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 470, true);
			this.FilterPanel.TabIndex = 0;
			// 
			// DisbursementClearingAggregatedBalanceGroupBox
			// 
			this.DisbursementClearingAggregatedBalanceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|30633b0c-0c41-43bd-9fcc-2a23eb151621", "Disbursement Surplus/Shortfall Aggregated Balance");
			this.DisbursementClearingAggregatedBalanceGroupBox.Controls.Add(this.BalanceCalcFindBox);
			this.DisbursementClearingAggregatedBalanceGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DisbursementClearingAggregatedBalanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DisbursementClearingAggregatedBalanceGroupBox.Name = "DisbursementClearingAggregatedBalanceGroupBox";
			this.DisbursementClearingAggregatedBalanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 45, true);
			this.DisbursementClearingAggregatedBalanceGroupBox.TabIndex = 3;
			this.DisbursementClearingAggregatedBalanceGroupBox.TabStop = false;
			// 
			// BalanceCalcFindBox
			// 
			this.BalanceCalcFindBox.AllowDrop = true;
			this.BalanceCalcFindBox.BindToAmount = "BalanceAmount";
			this.BalanceCalcFindBox.BindToList = "LocalCurrencies";
			this.BalanceCalcFindBox.BindToUnit = "LocalCurrencyPK";
			this.BalanceCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|54c61724-40cf-48c7-bd55-8f4d391f57a2", "Balance");
			this.BalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 18, true);
			this.BalanceCalcFindBox.Name = "BalanceCalcFindBox";
			this.BalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.BalanceCalcFindBox.TabIndex = 12;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomButtonPanel.Controls.Add(this.DisbursementClearingAggregatedBalanceGroupBox);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 603, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 82, true);
			this.BottomButtonPanel.TabIndex = 11;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 25, true);
			this.PostingButtonsUserControl.TabIndex = 10;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.StatusDropEdit);
			this.TopPanel.Controls.Add(this.ApprovingUserGuidFindBox);
			this.TopPanel.Controls.Add(this.ApprovalDateDateEdit);
			this.TopPanel.Controls.Add(this.LargestDSBBalanceTextBox);
			this.TopPanel.Controls.Add(this.SmallestDSBBalanceTextBox);
			this.TopPanel.Controls.Add(this.TotalDSBBalanceTextBox);
			this.TopPanel.Controls.Add(this.BatchNumberTextBox);
			this.TopPanel.Controls.Add(this.CreateTimeDateEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 114, true);
			this.TopPanel.TabIndex = 1;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "JBB_BatchStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DsbJobCloseBatch)(null)).JBB_BatchStatus)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|8c05a953-18c0-4aab-9a1e-49b3ee84a50f", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 75, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.StatusDropEdit.TabIndex = 8;
			// 
			// ApprovingUserGuidFindBox
			// 
			this.ApprovingUserGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApprovingUserGuidFindBox, "JBB_GS_NKApprovingUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DsbJobCloseBatch)(null)).JBB_GS_NKApprovingUser)));
			this.ApprovingUserGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|842da88b-dc8d-41c7-8aaa-4beb82e39188", "Approving User");
			this.ApprovingUserGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 49, true);
			this.ApprovingUserGuidFindBox.Name = "ApprovingUserGuidFindBox";
			this.ApprovingUserGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ApprovingUserGuidFindBox.TabIndex = 6;
			// 
			// ApprovalDateDateEdit
			// 
			this.ApprovalDateDateEdit.AllowDrop = true;
			this.ApprovalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ApprovalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ApprovalDateDateEdit, "JBB_ApprovalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DsbJobCloseBatch)(null)).JBB_ApprovalTime)));
			this.ApprovalDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|5a3e33d0-23df-4388-be32-2d4fb7012b75", "Approval Date");
			this.ApprovalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 49, true);
			this.ApprovalDateDateEdit.Name = "ApprovalDateDateEdit";
			this.ApprovalDateDateEdit.TabIndex = 7;
			// 
			// LargestDSBBalanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.LargestDSBBalanceTextBox, "JBB_LargestAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((DsbJobCloseBatch)(null)).JBB_LargestAmount)));
			this.LargestDSBBalanceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|4c400bdb-9b28-4aa7-977f-b1c7872abbd8", "Largest Job DSB Balance");
			this.LargestDSBBalanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 23, true);
			this.LargestDSBBalanceTextBox.Name = "LargestDSBBalanceTextBox";
			this.LargestDSBBalanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.LargestDSBBalanceTextBox.TabIndex = 4;
			// 
			// SmallestDSBBalanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SmallestDSBBalanceTextBox, "JBB_SmallestAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((DsbJobCloseBatch)(null)).JBB_SmallestAmount)));
			this.SmallestDSBBalanceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|d690dfeb-f825-43f5-a794-2dcd26b83d68", "Smallest Job DSB Balance");
			this.SmallestDSBBalanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 23, true);
			this.SmallestDSBBalanceTextBox.Name = "SmallestDSBBalanceTextBox";
			this.SmallestDSBBalanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.SmallestDSBBalanceTextBox.TabIndex = 3;
			// 
			// TotalDSBBalanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalDSBBalanceTextBox, "JBB_TotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((DsbJobCloseBatch)(null)).JBB_TotalAmount)));
			this.TotalDSBBalanceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|e5d2bb7f-26c5-4084-9057-b304fc4005fc", "Total DSB Balance");
			this.TotalDSBBalanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 23, true);
			this.TotalDSBBalanceTextBox.Name = "TotalDSBBalanceTextBox";
			this.TotalDSBBalanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.TotalDSBBalanceTextBox.TabIndex = 2;
			// 
			// BatchNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "JBB_BatchNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DsbJobCloseBatch)(null)).JBB_BatchNumber)));
			this.BatchNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|a800bef1-c2f8-45b2-8e8d-be633a960b0a", "Batch Number");
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 23, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.BatchNumberTextBox.TabIndex = 1;
			// 
			// CreateTimeDateEdit
			// 
			this.CreateTimeDateEdit.AllowDrop = true;
			this.CreateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CreateTimeDateEdit, "JBB_SystemCreateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DsbJobCloseBatch)(null)).JBB_SystemCreateTime)));
			this.CreateTimeDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|c445cc25-ee80-400c-9068-56e13766ce49", "Create Time");
			this.CreateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 49, true);
			this.CreateTimeDateEdit.Name = "CreateTimeDateEdit";
			this.CreateTimeDateEdit.TabIndex = 5;
			// 
			// GridGroupBox
			// 
			this.GridGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalForm|5cd59238-dc87-49d2-a27c-d4d812f5658b", "Job");
			this.GridGroupBox.Controls.Add(this.FilterPanel);
			this.GridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.GridGroupBox.Name = "GridGroupBox";
			this.GridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 489, true);
			this.GridGroupBox.TabIndex = 13;
			this.GridGroupBox.TabStop = false;
			// 
			// bulkDSBJobCloseBatchApprovalUserControl1
			// 
			this.bulkDSBJobCloseBatchApprovalUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bulkDSBJobCloseBatchApprovalUserControl, ".");
			this.bulkDSBJobCloseBatchApprovalUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bulkDSBJobCloseBatchApprovalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bulkDSBJobCloseBatchApprovalUserControl.Name = "bulkDSBJobCloseBatchApprovalUserControl1";
			this.bulkDSBJobCloseBatchApprovalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 470, true);
			this.bulkDSBJobCloseBatchApprovalUserControl.TabIndex = 0;
			// 
			// BulkDSBJobCloseBatchApprovalForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 709, true);
			this.Controls.Add(this.GridGroupBox);
			this.Controls.Add(this.BottomButtonPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(DsbJobCloseBatch);
			this.Name = "BulkDSBJobCloseBatchApprovalForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.GridGroupBox, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.DisbursementClearingAggregatedBalanceGroupBox.ResumeLayout(false);
			this.DisbursementClearingAggregatedBalanceGroupBox.PerformLayout();
			this.BalanceCalcFindBox.ResumeLayout(true);
			this.BalanceCalcFindBox.PerformLayout();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ApprovingUserGuidFindBox.ResumeLayout(true);
			this.ApprovingUserGuidFindBox.PerformLayout();
			this.ApprovalDateDateEdit.ResumeLayout(true);
			this.ApprovalDateDateEdit.PerformLayout();
			this.CreateTimeDateEdit.ResumeLayout(true);
			this.CreateTimeDateEdit.PerformLayout();
			this.GridGroupBox.ResumeLayout(false);
			this.GridGroupBox.PerformLayout();
			this.bulkDSBJobCloseBatchApprovalUserControl.ResumeLayout(true);
			this.bulkDSBJobCloseBatchApprovalUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}