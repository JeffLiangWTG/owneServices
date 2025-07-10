namespace Enterprise.Accounting.GUI.Base
{
	partial class MultipleReversingForLineForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.WIPAccrualGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WIPAccrualGrid)).BeginInit();
			this.WIPAccrualGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine);
			// 
			// WIPAccrualGrid
			// 
			this.WIPAccrualGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WIPAccrualGrid, "TransactionLinesAlreadyReversed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).BranchPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).DepartmentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).GLHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).OrganizationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).CreatingUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).CreatedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionLineForReversing)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine)(null)).TransactionLinesAlreadyReversed)).SyncRoot)).ReverseDate)));
			this.WIPAccrualGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ac9bf564-bb73-49c6-930b-dd15e65d6bc8", "Transaction Type");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0788c02c-0869-473b-a2cc-76ad33f76478", "Job Number");
			zTextBoxColumnStyleInfo2.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("535a0235-59b6-4db7-9fed-bc3591ae05d3", "Branch");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BranchPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7de2bebf-da63-4982-8d56-59b66a48ea9d", "Department");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DepartmentPK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("14aa18c8-1277-4d69-903d-062534a47218", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "PostDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e004df67-5dbf-46b1-85cd-9eebce38f9c1", "Charge Code");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7d15c137-d9d8-4864-87b4-b6f1561a46a6", "GL Account");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "GLHeaderPK";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("485a815f-28c9-4bf9-8708-3db0e98ee36c", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a28068d7-e72a-45e6-9a17-79605a444712", "Organization");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OrganizationPK";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("33e5cca3-70e8-4d09-9483-9ef0c5fdd7bc", "Creating User");
			zTextBoxColumnStyleInfo3.ColumnName = "CreatingUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ad8988b-7fff-442b-ab6a-031a5f94f00a", "Created Date");
			zDateEditColumnStyleInfo2.ColumnName = "CreatedDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1fa9bb23-b396-47a7-9f22-c526fa99b877", "Reverse Date");
			zDateEditColumnStyleInfo3.ColumnName = "ReverseDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.WIPAccrualGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WIPAccrualGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.WIPAccrualGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.WIPAccrualGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.WIPAccrualGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.WIPAccrualGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.WIPAccrualGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.WIPAccrualGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.WIPAccrualGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.WIPAccrualGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.WIPAccrualGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.WIPAccrualGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.WIPAccrualGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WIPAccrualGrid.GridId = "1d027669-c5b6-464a-ac15-45b1ab7ead3e";
			this.WIPAccrualGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WIPAccrualGrid.LayoutKey = "zGrid1";
			this.WIPAccrualGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WIPAccrualGrid.Name = "WIPAccrualGrid";
			this.WIPAccrualGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 253, true);
			this.WIPAccrualGrid.TabIndex = 3;
			// 
			// MultipleReversingForLineForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 312, true);
			this.Controls.Add(this.WIPAccrualGrid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForLine);
			this.Name = "MultipleReversingForLineForm";
			this.Text = "MultipleReversingForLineForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.WIPAccrualGrid, 0);
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WIPAccrualGrid)).EndInit();
			this.WIPAccrualGrid.ResumeLayout(false);
			this.WIPAccrualGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid WIPAccrualGrid;
	}
}