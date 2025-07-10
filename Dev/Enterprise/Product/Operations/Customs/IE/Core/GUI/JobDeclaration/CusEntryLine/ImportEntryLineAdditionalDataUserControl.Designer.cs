namespace Enterprise.Customs.IE.GUI
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.RefundsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.RefundsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ExtendInfoTabControl.SuspendLayout();
            this.ExtendedInfoTabPage.SuspendLayout();
            this.TaxOrFeeTabPage.SuspendLayout();
            this.SupportingDocumentsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
            this.EntryLineSupportingDocumentsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RefundsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RefundsGrid)).BeginInit();
            this.RefundsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExtendInfoTabControl
            // 
            this.ExtendInfoTabControl.Controls.Add(this.RefundsTabPage);
            this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 483, true);
            this.ExtendInfoTabControl.Controls.SetChildIndex(this.RefundsTabPage, 0);
            this.ExtendInfoTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
            this.ExtendInfoTabControl.Controls.SetChildIndex(this.ExtendedInfoTabPage, 0);
            this.ExtendInfoTabControl.Controls.SetChildIndex(this.TaxOrFeeTabPage, 0);
            // 
            // ExtendedInfoTabPage
            // 
            this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
            // 
            // TaxOrFeeTabPage
            // 
            this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
            // 
            // SupportingDocumentsTabPage
            // 
            this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 456, true);
            // 
            // DutyAndTaxDetails
            // 
            this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
            this.DutyAndTaxDetails.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryLineTaxAndConfirmedFeeUserControl);
            // 
            // EntryLineSupportingDocumentsGrid
            // 
            this.EntryLineSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 450, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobDeclaration);
            // 
            // RefundsTabPage
            // 
            this.RefundsTabPage.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("7A15EBA3-97B2-4470-89D6-A4BE4E0EB7F1", "Refunds");
            this.RefundsTabPage.Controls.Add(this.RefundsGrid);
            this.RefundsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.RefundsTabPage.Name = "RefundsTabPage";
            this.RefundsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 456, true);
            this.RefundsTabPage.TabIndex = 3;
            this.RefundsTabPage.Text = "Refunds";
            // 
            // RefundsGrid
            // 
            this.RefundsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.RefundsGrid, "CustomsEntryHeaders.AllEntryLines.RefundDuties");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).TaxType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).Lookups.TaxTypes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).TaxAmountConfirmedRelease)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).TaxAmountConfirmedAmendment)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).TaxAmountDifferenceForRefunds)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.Declaration.RefundDuty)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).RefundDuties)).SyncRoot)).TaxAmountOfDutyToBeRepaid)));
            this.RefundsGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.BindToList = "Lookups.TaxTypes";
            zDropEditColumnStyleInfo1.ColumnName = "TaxType";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "TaxAmountConfirmedRelease";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "TaxAmountConfirmedAmendment";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "TaxAmountDifferenceForRefunds";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "TaxAmountOfDutyToBeRepaid";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.RefundsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.RefundsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.RefundsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.RefundsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.RefundsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.RefundsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RefundsGrid.GridId = "c38f25f5-1bad-4438-b7de-e9fbe07dbc1d";
            this.RefundsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.RefundsGrid.LayoutKey = "RefundsGrid";
            this.RefundsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RefundsGrid.Name = "RefundsGrid";
            this.RefundsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 456, true);
            this.RefundsGrid.TabIndex = 0;
            // 
            // ImportEntryLineAdditionalDataUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "ImportEntryLineAdditionalDataUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 483, true);
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
            this.RefundsTabPage.ResumeLayout(false);
            this.RefundsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RefundsGrid)).EndInit();
            this.RefundsGrid.ResumeLayout(false);
            this.RefundsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal ZArchitecture.GUI.ZTabPage RefundsTabPage;
        internal ZArchitecture.ZGrid RefundsGrid;

    }
}
