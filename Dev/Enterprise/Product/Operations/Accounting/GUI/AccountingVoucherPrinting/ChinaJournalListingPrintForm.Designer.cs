using System;
using System.ComponentModel;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class ChinaJournalListingPrintForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.GenerateButton = new ZButton();
			this.CloseButton = new ZButton();
			this.EndDateEdit = new ZDateEdit();
			this.FromDateEdit = new ZDateEdit();
			this.MainPanel = new ZPanel();
			this.BranchGuidFindBox = new ZGuidFindBox();
			this.VoucherPeriodEdit = new ZPeriodEdit();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EndDateEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChinaJournalListingPrintWrapper);
			// 
			// GenerateButton
			// 
			this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|d188d6a5-fd8f-4513-9c16-375d0a6ec6f8", "Print");
			this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 112, true);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.GenerateButton.TabIndex = 1;
			this.GenerateButton.Click += new EventHandler(this.GenerateButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|34a429d6-a6ea-45be-8e09-3dc857ba09c2", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 112, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaJournalListingPrintWrapper)(null)).EndDate)));
			this.EndDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|fb5190bc-843d-4f5a-bfa8-51280038dbe4", "End Date");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 42, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 2;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaJournalListingPrintWrapper)(null)).FromDate)));
			this.FromDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|a364386e-24ba-43ba-a50a-3720ff2b11ce", "From Date");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 42, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 1;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.BranchGuidFindBox);
			this.MainPanel.Controls.Add(this.EndDateEdit);
			this.MainPanel.Controls.Add(this.VoucherPeriodEdit);
			this.MainPanel.Controls.Add(this.FromDateEdit);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 105, true);
			this.MainPanel.TabIndex = 0;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChinaJournalListingPrintWrapper)(null)).Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|a55c0d88-3e83-426d-8872-8eb9b6a264cd", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 68, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.BranchGuidFindBox.TabIndex = 12;
			// 
			// VoucherPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.VoucherPeriodEdit, "FromPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((ChinaJournalListingPrintWrapper)(null)).FromPeriod)));
			this.VoucherPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|ab952c05-0682-435b-85d3-5cb2483640c3", "Period");
			this.VoucherPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 16, true);
			this.VoucherPeriodEdit.Name = "VoucherPeriodEdit";
			this.VoucherPeriodEdit.TabIndex = 0;
			// 
			// ChinaJournalListingPrintForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChinaJournalListingPrintForm|abb5e604-5e6d-43f6-82e8-9dd1634bfce8", "China Journal Listing Print");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 174, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.GenerateButton);
			this.Controls.Add(this.MainPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ChinaJournalListingPrintWrapper);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.AccountingVoucherPrint.ChinaJournalListingPrintFor" +
	"mer";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 212, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 212, true);
			this.Name = "ChinaJournalListingPrintForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.GenerateButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
