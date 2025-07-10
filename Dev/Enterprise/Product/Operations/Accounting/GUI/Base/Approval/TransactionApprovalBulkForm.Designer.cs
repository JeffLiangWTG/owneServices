
namespace Enterprise.Accounting.GUI
{
	partial class TransactionApprovalBulkForm<TransactionType, RequestType, DetailsType>
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TopGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TopGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MiddlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetailsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopSingleRequestPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApprovalStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CreatedUserCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ApprovingUserCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CreatedTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ApprovalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RequestingBranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TopGridPanel.SuspendLayout();
			this.TopGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).BeginInit();
			this.TopGrid.SuspendLayout();
			this.MiddlePanel.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			this.DetailsGrid.SuspendLayout();
			this.TopSingleRequestPanel.SuspendLayout();
			this.ApprovalStatusDropEdit.SuspendLayout();
			this.CreatedUserCodeFindBox.SuspendLayout();
			this.ApprovingUserCodeFindBox.SuspendLayout();
			this.CreatedTimeDateEdit.SuspendLayout();
			this.ApprovalDateEdit.SuspendLayout();
			this.RequestingBranchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 381, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 345, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 36, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 6, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// TopGridPanel
			// 
			this.TopGridPanel.Controls.Add(this.TopGridGroupBox);
			this.TopGridPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopGridPanel.Name = "TopGridPanel";
			this.TopGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 112, true);
			this.TopGridPanel.TabIndex = 0;
			// 
			// TopGridGroupBox
			// 
			this.TopGridGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARCreditNoteApprovalBulkForm|b768c639-fb6f-4543-8ad8-50a20894c7c9", "Approval Requests");
			this.TopGridGroupBox.Controls.Add(this.TopGrid);
			this.TopGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopGridGroupBox.Name = "TopGridGroupBox";
			this.TopGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 112, true);
			this.TopGridGroupBox.TabIndex = 0;
			this.TopGridGroupBox.TabStop = false;
			// 
			// TopGrid
			// 
			this.TopGrid.AllowNavigation = false;
			this.TopGrid.CaptionVisible = false;
			this.TopGrid.CopySelectedRowsAllowed = true;
			this.TopGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopGrid.GridId = "ECE50775-0706-45E4-B597-DAE52E70BEC7";
			this.TopGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TopGrid.LayoutKey = "TopGrid";
			this.TopGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TopGrid.Name = "TopGrid";
			this.TopGrid.ReadOnly = true;
			this.TopGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 93, true);
			this.TopGrid.TabIndex = 0;
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Controls.Add(this.DetailsGroupBox);
			this.MiddlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.MiddlePanel.Name = "MiddlePanel";
			this.MiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 128, true);
			this.MiddlePanel.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARCreditNoteApprovalBulkForm|8c6ae982-b9ee-46e6-9d28-db61083a9005", "Details");
			this.DetailsGroupBox.Controls.Add(this.DetailsGrid);
			this.DetailsGroupBox.Controls.Add(this.DetailsTopPanel);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 128, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DetailsGrid
			// 
			this.DetailsGrid.AllowNavigation = false;
			this.DetailsGrid.CaptionVisible = false;
			this.DetailsGrid.CopySelectedRowsAllowed = true;
			this.DetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGrid.GridId = "103021BE-7C24-4767-9D2C-7E9875DE8A2A";
			this.DetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DetailsGrid.LayoutKey = "TopGrid";
			this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 42, true);
			this.DetailsGrid.Name = "DetailsGrid";
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 83, true);
			this.DetailsGrid.TabIndex = 1;
			// 
			// DetailsTopPanel
			// 
			this.DetailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DetailsTopPanel.Name = "DetailsTopPanel";
			this.DetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 26, true);
			this.DetailsTopPanel.TabIndex = 0;
			// 
			// TopSingleRequestPanel
			// 
			this.TopSingleRequestPanel.Controls.Add(this.ApprovalStatusDropEdit);
			this.TopSingleRequestPanel.Controls.Add(this.CreatedUserCodeFindBox);
			this.TopSingleRequestPanel.Controls.Add(this.ApprovingUserCodeFindBox);
			this.TopSingleRequestPanel.Controls.Add(this.CreatedTimeDateEdit);
			this.TopSingleRequestPanel.Controls.Add(this.ApprovalDateEdit);
			this.TopSingleRequestPanel.Controls.Add(this.RequestingBranchGuidFindBox);
			this.TopSingleRequestPanel.Controls.Add(this.JobNumberTextBox);
			this.TopSingleRequestPanel.Controls.Add(this.ReasonDescriptionTextBox);
			this.TopSingleRequestPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopSingleRequestPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.TopSingleRequestPanel.Name = "TopSingleRequestPanel";
			this.TopSingleRequestPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 105, true);
			this.TopSingleRequestPanel.TabIndex = 1;
			// 
			// ApprovalStatusDropEdit
			// 
			this.ApprovalStatusDropEdit.AllowDrop = true;
			this.ApprovalStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 54, true);
			this.ApprovalStatusDropEdit.Name = "ApprovalStatusDropEdit";
			this.ApprovalStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ApprovalStatusDropEdit.TabIndex = 2;
			// 
			// CreatedUserCodeFindBox
			// 
			this.CreatedUserCodeFindBox.AllowDrop = true;
			this.CreatedUserCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 6, true);
			this.CreatedUserCodeFindBox.Name = "CreatedUserCodeFindBox";
			this.CreatedUserCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CreatedUserCodeFindBox.TabIndex = 4;
			// 
			// ApprovingUserCodeFindBox
			// 
			this.ApprovingUserCodeFindBox.AllowDrop = true;
			this.ApprovingUserCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARCreditNoteApprovalBulkForm|6e199b49-6ccc-45a4-84fd-5f402d8e1bab", "Approving User");
			this.ApprovingUserCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 30, true);
			this.ApprovingUserCodeFindBox.Name = "ApprovingUserCodeFindBox";
			this.ApprovingUserCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ApprovingUserCodeFindBox.TabIndex = 6;
			// 
			// CreatedTimeDateEdit
			// 
			this.CreatedTimeDateEdit.AllowDrop = true;
			this.CreatedTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreatedTimeDateEdit.AutoCompleteYear = true;
			this.CreatedTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 6, true);
			this.CreatedTimeDateEdit.Name = "CreatedTimeDateEdit";
			this.CreatedTimeDateEdit.TabIndex = 5;
			// 
			// ApprovalDateEdit
			// 
			this.ApprovalDateEdit.AllowDrop = true;
			this.ApprovalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ApprovalDateEdit.AutoCompleteYear = true;
			this.ApprovalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 30, true);
			this.ApprovalDateEdit.Name = "ApprovalDateEdit";
			this.ApprovalDateEdit.TabIndex = 7;
			// 
			// RequestingBranchGuidFindBox
			// 
			this.RequestingBranchGuidFindBox.AllowDrop = true;
			this.RequestingBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 30, true);
			this.RequestingBranchGuidFindBox.Name = "RequestingBranchGuidFindBox";
			this.RequestingBranchGuidFindBox.ShowDescriptionBox = false;
			this.RequestingBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.RequestingBranchGuidFindBox.TabIndex = 1;
			// 
			// JobNumberTextBox
			// 
			this.JobNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 6, true);
			this.JobNumberTextBox.Name = "JobNumberTextBox";
			this.JobNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JobNumberTextBox.TabIndex = 0;
			// 
			// ReasonDescriptionTextBox
			// 
			this.ReasonDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 78, true);
			this.ReasonDescriptionTextBox.Name = "ReasonDescriptionTextBox";
			this.ReasonDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 20, true);
			this.ReasonDescriptionTextBox.TabIndex = 3;
			// 
			// TransactionApprovalBulkForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseApprovalBulkForm|56575cb6-2c1b-449f-a178-c10f897ad536", "Approval Request");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 405, true);
			this.Controls.Add(this.MiddlePanel);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopSingleRequestPanel);
			this.Controls.Add(this.TopGridPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 400, true);
			this.Name = "TransactionApprovalBulkForm";
			this.Text = "InvoicingBaseApprovalBulkForm";
			this.Controls.SetChildIndex(this.TopGridPanel, 0);
			this.Controls.SetChildIndex(this.TopSingleRequestPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MiddlePanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TopGridPanel.ResumeLayout(false);
			this.TopGridPanel.PerformLayout();
			this.TopGridGroupBox.ResumeLayout(false);
			this.TopGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).EndInit();
			this.TopGrid.ResumeLayout(false);
			this.TopGrid.PerformLayout();
			this.MiddlePanel.ResumeLayout(false);
			this.MiddlePanel.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			this.DetailsGrid.ResumeLayout(false);
			this.DetailsGrid.PerformLayout();
			this.TopSingleRequestPanel.ResumeLayout(false);
			this.TopSingleRequestPanel.PerformLayout();
			this.ApprovalStatusDropEdit.ResumeLayout(true);
			this.ApprovalStatusDropEdit.PerformLayout();
			this.CreatedUserCodeFindBox.ResumeLayout(true);
			this.CreatedUserCodeFindBox.PerformLayout();
			this.ApprovingUserCodeFindBox.ResumeLayout(true);
			this.ApprovingUserCodeFindBox.PerformLayout();
			this.CreatedTimeDateEdit.ResumeLayout(true);
			this.CreatedTimeDateEdit.PerformLayout();
			this.ApprovalDateEdit.ResumeLayout(true);
			this.ApprovalDateEdit.PerformLayout();
			this.RequestingBranchGuidFindBox.ResumeLayout(true);
			this.RequestingBranchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel BottomPanel;
		protected Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		protected ZArchitecture.GUI.ZPanel TopGridPanel;
		protected ZArchitecture.GUI.ZPanel MiddlePanel;
		protected ZArchitecture.GUI.ZGroupBox TopGridGroupBox;
		protected ZArchitecture.ZGrid TopGrid;
		protected ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected ZArchitecture.ZGrid DetailsGrid;
		protected ZArchitecture.GUI.ZPanel TopSingleRequestPanel;
		protected ZArchitecture.GUI.ZPanel DetailsTopPanel;
		protected ZArchitecture.GUI.ZCodeFindBox CreatedUserCodeFindBox;
		protected ZArchitecture.GUI.ZCodeFindBox ApprovingUserCodeFindBox;
		protected ZArchitecture.GUI.ZDateEdit CreatedTimeDateEdit;
		protected ZArchitecture.GUI.ZDateEdit ApprovalDateEdit;
		protected ZArchitecture.GUI.ZGuidFindBox RequestingBranchGuidFindBox;
		protected ZArchitecture.ZTextBox JobNumberTextBox;
		protected ZArchitecture.ZTextBox ReasonDescriptionTextBox;
		protected ZArchitecture.GUI.ZDropEdit ApprovalStatusDropEdit;
	}
}
