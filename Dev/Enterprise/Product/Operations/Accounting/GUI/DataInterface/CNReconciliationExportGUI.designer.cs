using System;
using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.DataInterface
{
	public partial class CNReconciliationExportGUI
	{

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.NextButton = new ZButton();
			this.BranchGuidFindBox = new ZGuidFindBox();
			this.PostDateFromEdit = new ZDateEdit();
			this.PostDateToEdit = new ZDateEdit();
			this.ComplianceSubTypeEdit = new ZDropEdit();
			this.ExportStatusEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BranchGuidFindBox.SuspendLayout();
			this.PostDateFromEdit.SuspendLayout();
			this.PostDateToEdit.SuspendLayout();
			this.ComplianceSubTypeEdit.SuspendLayout();
			this.ExportStatusEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 201, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChinaReconciliationExportWrapper);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("900c319d-c422-4dd9-8f2c-798fd1206bf9", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 161, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new EventHandler(this.CancelButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1edf4538-4d03-4533-a590-c97b260ef061", "&Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 161, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.NextButton.TabIndex = 6;
			this.NextButton.Click += new EventHandler(this.NextButton_Click);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChinaReconciliationExportWrapper)(null)).Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bd01bf64-b560-4278-898d-6873d6a532b4", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 56, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.BranchGuidFindBox.TabIndex = 3;
			// 
			// PostDateFromEdit
			// 
			this.PostDateFromEdit.AllowDrop = true;
			this.PostDateFromEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateFromEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateFromEdit, "PostDateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaReconciliationExportWrapper)(null)).PostDateFrom)));
			this.PostDateFromEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0c4f8926-d270-4fdd-81e2-50eeccbb22fa", "Post Date From");
			this.PostDateFromEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 21, true);
			this.PostDateFromEdit.Name = "PostDateFromEdit";
			this.PostDateFromEdit.TabIndex = 1;
			// 
			// PostDateToEdit
			// 
			this.PostDateToEdit.AllowDrop = true;
			this.PostDateToEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateToEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateToEdit, "PostDateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaReconciliationExportWrapper)(null)).PostDateTo)));
			this.PostDateToEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("144bd73d-8b2b-4752-9bfc-02dd0ea0b166", "To");
			this.PostDateToEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 21, true);
			this.PostDateToEdit.Name = "PostDateToEdit";
			this.PostDateToEdit.TabIndex = 2;
			// 
			// ComplianceSubTypeEdit
			// 
			this.ComplianceSubTypeEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceSubTypeEdit, "ComplianceSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaReconciliationExportWrapper)(null)).ComplianceSubType)));
			this.ComplianceSubTypeEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fbd2751c-f5ce-491f-8bd5-729b64568521", "Compliance Sub Type");
			this.ComplianceSubTypeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 93, true);
			this.ComplianceSubTypeEdit.Name = "ComplianceSubTypeEdit";
			this.ComplianceSubTypeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ComplianceSubTypeEdit.TabIndex = 4;
			// 
			// ExportStatusEdit
			// 
			this.ExportStatusEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportStatusEdit, "ExportStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ChinaReconciliationExportWrapper)(null)).ExportStatus)));
			this.ExportStatusEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1eb7bd62-3716-47d6-a3c0-0fda4d6c9d19", "Export Status");
			this.ExportStatusEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 132, true);
			this.ExportStatusEdit.Name = "ExportStatusEdit";
			this.ExportStatusEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ExportStatusEdit.TabIndex = 5;
			// 
			// CNReconciliationExportGUI
			// 
			this.AcceptButton = this.NextButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9dd35b57-3320-44da-9a48-ccfadfebc865", "Invoices Reconciliation Export");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 225, true);
			this.Controls.Add(this.ExportStatusEdit);
			this.Controls.Add(this.ComplianceSubTypeEdit);
			this.Controls.Add(this.PostDateToEdit);
			this.Controls.Add(this.PostDateFromEdit);
			this.Controls.Add(this.BranchGuidFindBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.NextButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.DataTransfer";
			this.DataSourceType = typeof(ChinaReconciliationExportWrapper);
			this.DataSourceTypeName = "Enterprise.Accounting.DataTransfer.DataInterface.ChinaReconciliationExportWrapper" +
	"";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CNReconciliationExportGUI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.Controls.SetChildIndex(this.PostDateFromEdit, 0);
			this.Controls.SetChildIndex(this.PostDateToEdit, 0);
			this.Controls.SetChildIndex(this.ComplianceSubTypeEdit, 0);
			this.Controls.SetChildIndex(this.ExportStatusEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.PostDateFromEdit.ResumeLayout(true);
			this.PostDateFromEdit.PerformLayout();
			this.PostDateToEdit.ResumeLayout(true);
			this.PostDateToEdit.PerformLayout();
			this.ComplianceSubTypeEdit.ResumeLayout(true);
			this.ComplianceSubTypeEdit.PerformLayout();
			this.ExportStatusEdit.ResumeLayout(true);
			this.ExportStatusEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		#region Controls

#if DEBUG
		public
#endif
 ZButton NextButton;
#if DEBUG
		public
#endif
 ZButton CloseButton;
		protected ZGuidFindBox BranchGuidFindBox;
		private ZDateEdit PostDateFromEdit;
		private ZDateEdit PostDateToEdit;
		private ZDropEdit ComplianceSubTypeEdit;
		private ZDropEdit ExportStatusEdit;

		private readonly System.ComponentModel.Container components = null;

		#endregion
	}
}
