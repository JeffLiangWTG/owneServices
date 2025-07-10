using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUSupplierHeaderUserControl
	{
		private void InitializeComponent()
		{
			this.JZ_AddInfoBoundAddInfoControl = new Enterprise.Customs.AU.Declaration.GUI.AddInfoControlOptionalCMR();
			this.ITOTIncoTermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.GroupInvoiceDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
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
			this.JZ_AddInfoBoundAddInfoControl.SuspendLayout();
			this.InvoiceOriginCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 104, true);
			// 
			// JZ_InvoiceNumberBoundTextBox
			// 
			this.JZ_InvoiceNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 6, true);
			this.JZ_InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			// 
			// JZ_InvoiceAmountBoundCurrencyControl
			// 
			this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.ITOTIncoTermTextBox);
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 32, true);
			this.LeftBottomPanel.TabIndex = 1;
			this.LeftBottomPanel.Controls.SetChildIndex(this.LineTotalBoundConvertToLocalCurrencyControl, 0);
			this.LeftBottomPanel.Controls.SetChildIndex(this.IncoTermTextBox, 0);
			this.LeftBottomPanel.Controls.SetChildIndex(this.ITOTIncoTermTextBox, 0);
			// 
			// IncoTermTextBox
			// 
			this.IncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			// 
			// ApportionmentPendingLabel
			// 
			this.ApportionmentPendingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 32, true);
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 0, true);
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 32, true);
			this.RightBottomPanel.TabIndex = 2;
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 306, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 127, true);
			this.ChargesGroupBox.TabIndex = 20;
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 108, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 81, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 81, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 110, true);
			this.BaseGroupChargesGroupBox.TabIndex = 21;
			this.BaseGroupChargesGroupBox.Text = "Group Charges(All Invoices)";
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 91, true);
			// 
			// JZ_FOBAmountBoundCurrencyControl
			// 
			this.JZ_FOBAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 6, true);
			this.JZ_FOBAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			// 
			// JZ_Calc_TNIBoundInvoiceCurrencyControl
			// 
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 6, true);
			// 
			// JZ_CIFAmountBoundCurrencyControl
			// 
			this.JZ_CIFAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 6, true);
			this.JZ_CIFAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			// 
			// LineTotalBoundConvertToLocalCurrencyControl
			// 
			this.LineTotalBoundConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 8, true);
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
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 125, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			this.JobComInvoiceHeadersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 163, true);
			// 
			// InvCustomFieldsDisplayControl
			// 
			this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 279, true);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 279, true);
			// 
			// Splitter
			// 
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 510, true);
			this.Splitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(163);
			// 
			// InvDetailLeftPanel
			// 
			this.InvDetailLeftPanel.Controls.Add(this.JZ_AddInfoBoundAddInfoControl);
			this.InvDetailLeftPanel.Controls.Add(this.InvoiceOriginCodeFindBox);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermBoundDropDownEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceAmountBoundCurrencyControl, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GroupInvoiceDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.InvoiceOriginCodeFindBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_AddInfoBoundAddInfoControl, 0);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(127);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 306, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.IInvoicesProvider);
			// 
			// JZ_AddInfoBoundAddInfoControl
			// 
			this.JZ_AddInfoBoundAddInfoControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_AddInfoBoundAddInfoControl, "Invoices.AddInfo+AddInfoLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.AddInfoLine)));
			this.JZ_AddInfoBoundAddInfoControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUSupplierHeaderUserControl|0dd2b835-cce7-41f2-af7e-efbfba978c2a", "Add Info | MISC");
			this.JZ_AddInfoBoundAddInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 249, true);
			this.JZ_AddInfoBoundAddInfoControl.Name = "JZ_AddInfoBoundAddInfoControl";
			this.JZ_AddInfoBoundAddInfoControl.ShowCMRAddInfo = false;
			this.JZ_AddInfoBoundAddInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.JZ_AddInfoBoundAddInfoControl.TabIndex = 19;
			// 
			// ITOTIncoTermTextBox
			// 
			this.BindingSource.SetBindingMember(this.ITOTIncoTermTextBox, "Invoices.JZ_ITOTIncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.IInvoicesProvider)(null)).Invoices)).SyncRoot)).JZ_ITOTIncoTerm)));
			this.ITOTIncoTermTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ITOTIncoTermTextBox, false);
			this.ITOTIncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 311, true);
			this.ITOTIncoTermTextBox.Name = "ITOTIncoTermTextBox";
			this.ITOTIncoTermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ITOTIncoTermTextBox.TabIndex = 94;
			// 
			// InvoiceOriginCodeFindBox
			// 
			this.InvoiceOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceOriginCodeFindBox, "Invoices.AddInfo+ZA_ORG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_ORG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_ORG_List)));
			this.InvoiceOriginCodeFindBox.BindToList = "Invoices.AddInfo+ZA_ORG_List";
			this.InvoiceOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 177, true);
			this.InvoiceOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.InvoiceOriginCodeFindBox.Name = "InvoiceOriginCodeFindBox";
			this.InvoiceOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceOriginCodeFindBox.ParentType = null;
			this.InvoiceOriginCodeFindBox.PreBoundMaxLength = 2;
			this.InvoiceOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.InvoiceOriginCodeFindBox.TabIndex = 16;
			// 
			// AUSupplierHeaderUserControl
			// 
			this.Name = "AUSupplierHeaderUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 510, true);
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.GroupInvoiceDropEdit.ResumeLayout(true);
			this.GroupInvoiceDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
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
			this.JZ_AddInfoBoundAddInfoControl.ResumeLayout(true);
			this.JZ_AddInfoBoundAddInfoControl.PerformLayout();
			this.InvoiceOriginCodeFindBox.ResumeLayout(true);
			this.InvoiceOriginCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal AddInfoControlOptionalCMR JZ_AddInfoBoundAddInfoControl;
		protected internal ZTextBox ITOTIncoTermTextBox;
		protected internal ZCodeFindBox InvoiceOriginCodeFindBox;
	}
}
