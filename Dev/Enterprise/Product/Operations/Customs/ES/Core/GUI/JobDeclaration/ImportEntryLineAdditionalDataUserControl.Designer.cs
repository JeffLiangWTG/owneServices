namespace Enterprise.Customs.ES.GUI
{
	partial class ImportEntryLineAdditionalDataUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLinePreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExtendInfoTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreviousDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinePreviousDocumentsGrid)).BeginInit();
			this.EntryLinePreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.PreviousDocumentsTabPage, 0);
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("CF52D69F-E69E-46A7-BE56-7D92A3531394", "[40] Previous Documents");
			this.PreviousDocumentsTabPage.Controls.Add(this.EntryLinePreviousDocumentsGrid);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
			this.PreviousDocumentsTabPage.TabIndex = 2;
			this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLinePreviousDocumentsGrid
			// 
			this.EntryLinePreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLinePreviousDocumentsGrid, "CustomsEntryHeaders.AllEntryLines.ReadOnlyPreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyPreviousDocuments)).SyncRoot)).CSI_PackType)));
			this.EntryLinePreviousDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_SubType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_UnitOfQuantity";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_PackQty";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_PackType";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntryLinePreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinePreviousDocumentsGrid.GridId = "B7B7CF5E-DB00-417D-80A6-63732CABB3F0";
			this.EntryLinePreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLinePreviousDocumentsGrid.LayoutKey = "EntryLinePreviousDocumentsGrid";
			this.EntryLinePreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLinePreviousDocumentsGrid.Name = "EntryLinePreviousDocumentsGrid";
			this.EntryLinePreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
			this.EntryLinePreviousDocumentsGrid.TabIndex = 0;
			// 
			// ImportEntryLineAdditionalDataUserControl
			// 
			this.Name = "ImportEntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinePreviousDocumentsGrid)).EndInit();
			this.EntryLinePreviousDocumentsGrid.ResumeLayout(false);
			this.EntryLinePreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		ZArchitecture.ZGrid EntryLinePreviousDocumentsGrid;

	}
}
