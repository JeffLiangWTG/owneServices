namespace Enterprise.Customs.JP.GUI
{
	public partial class BaseCustomsSupplierHeaderUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceAmountTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ElectronicInvoiceReceiptNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.InvoiceAmountTypeDropEdit.SuspendLayout();
			this.InvoiceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.InvoiceTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.AdditionalDetailsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.ComInvoiceDetailsTabPage, 0);
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutUU5vFvxYnT4ITmuJkyIQgg==";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 109, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			// 
			// InvCustomFieldsDisplayControl
			// 
			this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			// 
			// Splitter
			// 
			// 
			// ChargesGroupsSplitterContainer
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("BaseCustomsSupplierHeaderUserControl|AdditionalDetailsTabPage", "Additional Details");
			this.AdditionalDetailsTabPage.Controls.Add(this.InvoiceAmountTypeDropEdit);
			this.AdditionalDetailsTabPage.Controls.Add(this.InvoiceTypeDropEdit);
			this.AdditionalDetailsTabPage.Controls.Add(this.ElectronicInvoiceReceiptNumberTextBox);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.AdditionalDetailsTabPage.TabIndex = 3;
			// 
			// InvoiceAmountTypeDropEdit
			// 
			this.InvoiceAmountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceAmountTypeDropEdit, "Invoices.JZ_InvoiceAmountType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceAmountType)));
			this.InvoiceAmountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 63, true);
			this.InvoiceAmountTypeDropEdit.Name = "InvoiceAmountTypeDropEdit";
			this.InvoiceAmountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.InvoiceAmountTypeDropEdit.TabIndex = 2;
			// 
			// ElectronicInvoiceReceiptNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ElectronicInvoiceReceiptNumberTextBox, "Invoices.JZ_ElectronicInvoiceReceiptNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ElectronicInvoiceReceiptNumber)));
			this.ElectronicInvoiceReceiptNumberTextBox.CaptionResourceString = null;
			this.ElectronicInvoiceReceiptNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 37, true);
			this.ElectronicInvoiceReceiptNumberTextBox.Name = "ElectronicInvoiceReceiptNumberTextBox";
			this.ElectronicInvoiceReceiptNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.ElectronicInvoiceReceiptNumberTextBox.TabIndex = 1;
			// 
			// InvoiceTypeDropEdit
			// 
			this.InvoiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTypeDropEdit, "Invoices.JZ_InvoiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceType)));
			this.InvoiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 10, true);
			this.InvoiceTypeDropEdit.Name = "InvoiceTypeDropEdit";
			this.InvoiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.InvoiceTypeDropEdit.TabIndex = 0;
			// 
			// BaseCustomsSupplierHeaderUserControl
			// 
			this.Name = "BaseCustomsSupplierHeaderUserControl";
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.InvoiceAmountTypeDropEdit.ResumeLayout(true);
			this.InvoiceAmountTypeDropEdit.PerformLayout();
			this.InvoiceTypeDropEdit.ResumeLayout(true);
			this.InvoiceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDetailsTabPage;
		Enterprise.ZArchitecture.ZTextBox ElectronicInvoiceReceiptNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceTypeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit InvoiceAmountTypeDropEdit;
	}
}
