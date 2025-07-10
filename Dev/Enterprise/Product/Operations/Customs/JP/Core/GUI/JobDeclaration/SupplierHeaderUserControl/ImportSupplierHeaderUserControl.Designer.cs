namespace Enterprise.Customs.JP.GUI
{
	partial class ImportSupplierHeaderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo acceptanceNumberTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ValuationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ComprehensiveValuationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ComprehensiveInsuranceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ComprehensiveValuationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InsuranceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdvanceRulingonValuation1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdvanceRulingonValuation2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.InsuranceTypeDropEdit.SuspendLayout();
			this.InvoiceAmountTypeDropEdit.SuspendLayout();
			this.FreightTypeDropEdit.SuspendLayout();
			this.BottomPanel.SuspendLayout();
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
			this.ValuationTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComprehensiveValuationsGrid)).BeginInit();
			this.ComprehensiveValuationsGrid.SuspendLayout();
			this.ComprehensiveValuationGroupBox.SuspendLayout();
			this.AdvanceRulingonValuation1TextBox.SuspendLayout();
			this.AdvanceRulingonValuation2TextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.Controls.Add(this.ComprehensiveInsuranceNumberTextBox);
			this.AdditionalDetailsTabPage.Controls.Add(this.InsuranceTypeDropEdit);
			this.AdditionalDetailsTabPage.Controls.Add(this.FreightTypeDropEdit);
			this.AdditionalDetailsTabPage.Controls.Add(this.ValuationTypeDropEdit);
			this.AdditionalDetailsTabPage.Controls.Add(this.ComprehensiveValuationGroupBox);
			this.AdditionalDetailsTabPage.Controls.Add(this.AdvanceRulingonValuation1TextBox);
			this.AdditionalDetailsTabPage.Controls.Add(this.AdvanceRulingonValuation2TextBox);
			this.AdditionalDetailsTabPage.Controls.SetChildIndex(this.ComprehensiveValuationGroupBox, 0);
			this.AdditionalDetailsTabPage.Controls.SetChildIndex(this.ValuationTypeDropEdit, 0);
			this.AdditionalDetailsTabPage.Controls.SetChildIndex(this.ComprehensiveInsuranceNumberTextBox, 0);
			this.AdditionalDetailsTabPage.Controls.SetChildIndex(this.InsuranceTypeDropEdit, 0);
			this.AdditionalDetailsTabPage.Controls.SetChildIndex(this.FreightTypeDropEdit, 0);
			// 
			// ComprehensiveInsuranceNumberTextBox
			// 
			this.ComprehensiveInsuranceNumberTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.ComprehensiveInsuranceNumberTextBox, "Invoices.JZ_ComprehensiveInsuranceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ComprehensiveInsuranceNumber)));
			this.ComprehensiveInsuranceNumberTextBox.CaptionResourceString = null;
			this.ComprehensiveInsuranceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 143, true);
			this.ComprehensiveInsuranceNumberTextBox.Name = "ComprehensiveInsuranceNumberTextBox";
			this.ComprehensiveInsuranceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.ComprehensiveInsuranceNumberTextBox.TabIndex = 5;
			// 
			// InsuranceTypeDropEdit
			// 
			this.InsuranceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InsuranceTypeDropEdit, "Invoices.JZ_InsuranceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InsuranceType)));
			this.InsuranceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 117, true);
			this.InsuranceTypeDropEdit.Name = "InsuranceTypeDropEdit";
			this.InsuranceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.InsuranceTypeDropEdit.TabIndex = 4;
			// 
			// InvoiceAmountTypeDropEdit
			//
			this.InvoiceAmountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 90, true);
			this.InvoiceAmountTypeDropEdit.TabIndex = 3;
			// 
			// FreightTypeDropEdit
			// 
			this.FreightTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightTypeDropEdit, "Invoices.JZ_FreightType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_FreightType)));
			this.FreightTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 63, true);
			this.FreightTypeDropEdit.Name = "FreightTypeDropEdit";
			this.FreightTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.FreightTypeDropEdit.TabIndex = 2;
			// 
			// JobComInvoiceHeadersBoundGrid
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
			// ValuationTypeDropEdit
			// 
			this.ValuationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationTypeDropEdit, "Invoices.JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationCode)));
			this.ValuationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 169, true);
			this.ValuationTypeDropEdit.Name = "ValuationTypeDropEdit";
			this.ValuationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 18, true);
			this.ValuationTypeDropEdit.TabIndex = 6;
			// 
			// ComprehensiveValuationsGrid
			// 
			this.BindingSource.SetBindingMember(this.ComprehensiveValuationsGrid, "Invoices.ComprehensiveValuations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ComprehensiveValuations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.ComprehensiveValuation)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ComprehensiveValuations)).SyncRoot)).CFR_Reference)));
			this.ComprehensiveValuationsGrid.CaptionVisible = false;
			acceptanceNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("D715DD86-2932-4FF0-9117-D6172EA054C6", "Number");
			acceptanceNumberTextBoxColumnStyleInfo.ColumnName = "CFR_Reference";
			acceptanceNumberTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			acceptanceNumberTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			acceptanceNumberTextBoxColumnStyleInfo.MaxLengthOverride = 0;
			acceptanceNumberTextBoxColumnStyleInfo.IsMandatory = true;
			acceptanceNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ComprehensiveValuationsGrid.ColumnStyles.Add(acceptanceNumberTextBoxColumnStyleInfo);
			this.ComprehensiveValuationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComprehensiveValuationsGrid.GridId = "BDF03551-6CDB-4BB6-8D34-6D8444691518";
			this.ComprehensiveValuationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComprehensiveValuationsGrid.LayoutKey = "ComprehensiveValuationsGrid";
			this.ComprehensiveValuationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ComprehensiveValuationsGrid.Name = "ComprehensiveValuationsGrid";
			this.ComprehensiveValuationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 113, true);
			this.ComprehensiveValuationsGrid.TabIndex = 8;
			// 
			// ComprehensiveValuationGroupBox
			// 
			this.ComprehensiveValuationGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.ComprehensiveValuationGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("06DE5C6D-681A-410A-B64C-7B8BC8C10E8A", "Comprehensive Valuations");
			this.ComprehensiveValuationGroupBox.Controls.Add(this.ComprehensiveValuationsGrid);
			this.ComprehensiveValuationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 195, true);
			this.ComprehensiveValuationGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.ComprehensiveValuationGroupBox.Name = "ComprehensiveValuationGroupBox";
			this.ComprehensiveValuationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 130, true);
			this.ComprehensiveValuationGroupBox.TabIndex = 7;
			this.ComprehensiveValuationGroupBox.TabStop = false;
			// 
			// AdvanceRulingonValuation1TextBox
			// 
			this.BindingSource.SetBindingMember(this.AdvanceRulingonValuation1TextBox, "Invoices.JZ_AdvanceRulingOnValuation1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_AdvanceRulingOnValuation1)));
			this.AdvanceRulingonValuation1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 338, true);
			this.AdvanceRulingonValuation1TextBox.Name = "AdvanceRulingonValuation1TextBox";
			this.AdvanceRulingonValuation1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.AdvanceRulingonValuation1TextBox.TabIndex = 9;
			// 
			// AdvanceRulingonValuation2TextBox
			// 
			this.BindingSource.SetBindingMember(this.AdvanceRulingonValuation2TextBox, "Invoices.JZ_AdvanceRulingOnValuation2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_AdvanceRulingOnValuation2)));
			this.AdvanceRulingonValuation2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 364, true);
			this.AdvanceRulingonValuation2TextBox.Name = "AdvanceRulingonValuation2TextBox";
			this.AdvanceRulingonValuation2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.AdvanceRulingonValuation2TextBox.TabIndex = 10;
			// 
			// ImportSupplierHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ImportSupplierHeaderUserControl";
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.InsuranceTypeDropEdit.ResumeLayout(true);
			this.InsuranceTypeDropEdit.PerformLayout();
			this.InvoiceAmountTypeDropEdit.ResumeLayout(true);
			this.InvoiceAmountTypeDropEdit.PerformLayout();
			this.FreightTypeDropEdit.ResumeLayout(true);
			this.FreightTypeDropEdit.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
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
			this.ValuationTypeDropEdit.ResumeLayout(true);
			this.ValuationTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComprehensiveValuationsGrid)).EndInit();
			this.ComprehensiveValuationsGrid.ResumeLayout(false);
			this.ComprehensiveValuationsGrid.PerformLayout();
			this.ComprehensiveValuationGroupBox.ResumeLayout(false);
			this.ComprehensiveValuationGroupBox.PerformLayout();
			this.AdvanceRulingonValuation1TextBox.ResumeLayout(false);
			this.AdvanceRulingonValuation1TextBox.PerformLayout();
			this.AdvanceRulingonValuation2TextBox.ResumeLayout(false);
			this.AdvanceRulingonValuation2TextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit ValuationTypeDropEdit;
		ZArchitecture.GUI.ZGroupBox ComprehensiveValuationGroupBox;
		ZArchitecture.ZTextBox ComprehensiveInsuranceNumberTextBox;
		ZArchitecture.ZGrid ComprehensiveValuationsGrid;
		ZArchitecture.GUI.ZDropEdit InsuranceTypeDropEdit;
		ZArchitecture.GUI.ZDropEdit FreightTypeDropEdit;
		ZArchitecture.ZTextBox AdvanceRulingonValuation1TextBox;
		ZArchitecture.ZTextBox AdvanceRulingonValuation2TextBox;
	}
}
