namespace Enterprise.Accounting.GUI.Base
{
	partial class MultipleReversingForHeaderForm
	{
		System.ComponentModel.IContainer components = null;

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "TransactionsAlreadyReversed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).Organization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).SupportingDocumentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).UnmatchDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).OverseasTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).OriginalTransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).OriginalTransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).ReversalStatusCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.IReversingImplicitlyImplementedWrapperForBinding)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader)(null)).TransactionsAlreadyReversed)).SyncRoot)).AmendStatusCode)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|a712425a-d047-451c-b9d9-9ec3212994cc", "Transaction Type");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|c9ad7353-e403-4819-8fd6-3a83d59d3534", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Organization";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|85db4bdc-0ba8-4f4e-b396-5f7e4ad5fffa", "Transaction Date");
			zDateEditColumnStyleInfo1.ColumnName = "TransactionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|7585a992-c732-47ae-b97f-9288f467c6e5", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8D263661-2E13-4B08-8D69-246D8C662782", "Supporting Document Number");
			zTextBoxColumnStyleInfo2.ColumnName = "SupportingDocumentNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|ed50998c-7bb4-44f8-a69a-bf0e06b6e4cb", "Unmatch Date");
			zDateEditColumnStyleInfo3.ColumnName = "UnmatchDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|941ffd24-6c38-49fc-ba5b-3dc33ff7498d", "Transaction Num.");
			zTextBoxColumnStyleInfo3.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|cbdd0568-edb1-4f6e-95aa-668dd9eb8666", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|04712f5f-07e0-482b-9b0e-cea6ee8b9dfa", "OS Total Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "OverseasTotalAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|78d90393-d4e0-4aa6-b200-8bc630c8e5bf", "Ledger");
			zTextBoxColumnStyleInfo5.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a643bb1f-79ee-4088-91fe-c086c8811887", "Original Transaction Num.");
			zTextBoxColumnStyleInfo6.ColumnName = "OriginalTransactionNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("28c70536-9b91-46fc-a13d-e10f534c5a77", "Original Transaction Type");
			zTextBoxColumnStyleInfo7.ColumnName = "OriginalTransactionType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|68744B49-7786-41AA-8156-0DCAD0CAB1A3", "Reversal Code");
			zDropEditColumnStyleInfo1.ColumnName = "ReversalStatusCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4d92e4c-2db1-46f4-9b9f-c20fb1d3091f", "Amend Status Code");
			zDropEditColumnStyleInfo2.ColumnName = "AmendStatusCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "0f366aa8-c393-4cb6-8d49-6b49186c4525";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 217, true);
			this.Grid.TabIndex = 0;
			// 
			// MultipleReversingForHeaderForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 275, true);
			this.Controls.Add(this.Grid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderForHeader);
			this.Name = "MultipleReversingForHeaderForm";
			this.Text = "MultipleReversingForHeaderForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Grid, 0);
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.ZGrid Grid;
	}
}
