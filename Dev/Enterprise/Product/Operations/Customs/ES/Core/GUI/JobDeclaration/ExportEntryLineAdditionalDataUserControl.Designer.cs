namespace Enterprise.Customs.ES.GUI
{
	partial class ExportEntryLineAdditionalDataUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLinePreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalInfosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineAdditionalInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExtendInfoTabControl.SuspendLayout();
			this.ExtendedInfoTabPage.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreviousDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinePreviousDocumentsGrid)).BeginInit();
			this.EntryLinePreviousDocumentsGrid.SuspendLayout();
			this.AdditionalInfosTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineAdditionalInfosGrid)).BeginInit();
			this.EntryLineAdditionalInfosGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.ExtendInfoTabControl.Controls.Add(this.AdditionalInfosTabPage);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.AdditionalInfosTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.PreviousDocumentsTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.ExtendedInfoTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.TaxOrFeeTabPage, 0);
			// 
			// ExtendedInfoTabPage
			// 
			this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
			// 
			// TaxOrFeeTabPage
			// 
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
			// 
			// DutyAndTaxDetails
			// 
			this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
			this.DutyAndTaxDetails.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryLineTaxAndFeeUserControl);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C10BD569-EC04-4457-9316-8668BB23B3FA", "[40] Previous Documents");
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
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLinePreviousDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryLinePreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinePreviousDocumentsGrid.GridId = "152480AB-0331-41A5-8EA9-C5D07DFD1959";
			this.EntryLinePreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLinePreviousDocumentsGrid.LayoutKey = "EntryLinePreviousDocumentsGrid";
			this.EntryLinePreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLinePreviousDocumentsGrid.Name = "EntryLinePreviousDocumentsGrid";
			this.EntryLinePreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
			this.EntryLinePreviousDocumentsGrid.TabIndex = 0;
			// 
			// AdditionalInfosTabPage
			// 
			this.AdditionalInfosTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("6BAA440F-856C-4434-95F4-E546CD304BF0", "Additional Documents");
			this.AdditionalInfosTabPage.Controls.Add(this.EntryLineAdditionalInfosGrid);
			this.AdditionalInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInfosTabPage.Name = "AdditionalInfosTabPage";
			this.AdditionalInfosTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
			this.AdditionalInfosTabPage.TabIndex = 3;
			this.AdditionalInfosTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLineAdditionalInfosGrid
			// 
			this.EntryLineAdditionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineAdditionalInfosGrid, "CustomsEntryHeaders.AllEntryLines.ReadOnlyAdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.ReadOnlyAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlyAdditionalInfos)).SyncRoot)).CSI_ReferenceNumber2)));
			this.EntryLineAdditionalInfosGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "CSI_SubType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.ColumnName = "CSI_Status";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "CSI_RX_NKCurrency";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.EntryLineAdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.EntryLineAdditionalInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineAdditionalInfosGrid.GridId = "792948DD-6AD0-4994-9091-639209E01F3C";
			this.EntryLineAdditionalInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineAdditionalInfosGrid.LayoutKey = "EntryLineAdditionalInfosGrid";
			this.EntryLineAdditionalInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineAdditionalInfosGrid.Name = "EntryLineAdditionalInfosGrid";
			this.EntryLineAdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
			this.EntryLineAdditionalInfosGrid.TabIndex = 0;
			// 
			// ExportEntryLineAdditionalDataUserControl
			// 
			this.Name = "ExportEntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.ExtendedInfoTabPage.ResumeLayout(false);
			this.ExtendedInfoTabPage.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinePreviousDocumentsGrid)).EndInit();
			this.EntryLinePreviousDocumentsGrid.ResumeLayout(false);
			this.EntryLinePreviousDocumentsGrid.PerformLayout();
			this.AdditionalInfosTabPage.ResumeLayout(false);
			this.AdditionalInfosTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineAdditionalInfosGrid)).EndInit();
			this.EntryLineAdditionalInfosGrid.ResumeLayout(false);
			this.EntryLineAdditionalInfosGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		ZArchitecture.ZGrid EntryLinePreviousDocumentsGrid;
		ZArchitecture.GUI.ZTabPage AdditionalInfosTabPage;
		ZArchitecture.ZGrid EntryLineAdditionalInfosGrid;
	}
}
