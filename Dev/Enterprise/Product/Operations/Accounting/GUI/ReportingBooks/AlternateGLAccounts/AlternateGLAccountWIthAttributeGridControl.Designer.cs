namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class AlternateGLAccountWithAttributeGridControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo oRGTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo oCGTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo tICTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo lFETextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo lFOTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo sPRTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo alternateGLAccountNumTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo accountNumWithSeparatorTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo debitCreditDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo accountNameTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo reportSectionDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo accountforTotalTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			AlternateGLAccountFindBoxColumnStyleInfo percentNumAlternateGLAccountFindBoxColumnStyleInfo = new AlternateGLAccountFindBoxColumnStyleInfo();
			AlternateGLAccountFindBoxColumnStyleInfo consolidationNumAlternateGLAccountFindBoxColumnStyleInfo = new AlternateGLAccountFindBoxColumnStyleInfo();
			AlternateGLAccountFindBoxColumnStyleInfo alternateNumAlternateGLAccountFindBoxColumnStyleInfo = new AlternateGLAccountFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo printSequenceCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.attributeGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.attributeGrid)).BeginInit();
			this.attributeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.AlternateGLAccounts);
			// 
			// attributeGrid
			// 
			this.attributeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributeGrid, "AlternateGLAccountsWithAttributeSet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).OCGDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).TICDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).LFEDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).LFODescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).SPRDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AccountNumWithSeparator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_ReportSection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.PrefixedAccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_AGA_PercentNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_AGA_ConsolidationNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_AGA_AlternateNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.AlternateGLAccountWithAttributeSet)(((System.Collections.IList)(((Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)).AlternateGLAccount.AGA_PrintSequence)));
			this.attributeGrid.CaptionVisible = false;
			oRGTextBoxColumnStyleInfo.Caption = "ORG";
			oRGTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("22185dd7-27a6-4d37-ad50-b86a497ebc73", "ORG Attribute");
			oRGTextBoxColumnStyleInfo.ColumnName = "OrganizationCode";
			oRGTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			oRGTextBoxColumnStyleInfo.IsReadOnly = true;
			oRGTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			oCGTextBoxColumnStyleInfo.Caption = "OCG";
			oCGTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("753a6ce9-402e-44d1-b25a-dee183b4b30a", "OCG Attribute");
			oCGTextBoxColumnStyleInfo.ColumnName = "OCGDescription";
			oCGTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			oCGTextBoxColumnStyleInfo.IsReadOnly = true;
			oCGTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tICTextBoxColumnStyleInfo.Caption = "TIC";
			tICTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f487a9c5-9e13-440a-a372-021999648cdd", "TIC Attribute");
			tICTextBoxColumnStyleInfo.ColumnName = "TICDescription";
			tICTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			tICTextBoxColumnStyleInfo.IsReadOnly = true;
			tICTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			lFETextBoxColumnStyleInfo.Caption = "LFE";
			lFETextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3d858d3e-c74e-4840-aacf-2a652031931b", "LFE Attribute");
			lFETextBoxColumnStyleInfo.ColumnName = "LFEDescription";
			lFETextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			lFETextBoxColumnStyleInfo.IsReadOnly = true;
			lFETextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			lFOTextBoxColumnStyleInfo.Caption = "LFO";
			lFOTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9fa36a77-a5fb-4d93-bdce-bc3c71fe4401", "LFO Attribute");
			lFOTextBoxColumnStyleInfo.ColumnName = "LFODescription";
			lFOTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			lFOTextBoxColumnStyleInfo.IsReadOnly = true;
			lFOTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			sPRTextBoxColumnStyleInfo.Caption = "SPR";
			sPRTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b335a691-4991-4bb8-a6a5-8d8009e2d0b5", "SPR Attribute");
			sPRTextBoxColumnStyleInfo.ColumnName = "SPRDescription";
			sPRTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			sPRTextBoxColumnStyleInfo.IsReadOnly = true;
			sPRTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			alternateGLAccountNumTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9f08d80f-372a-4caf-83a9-1c4f31db8656", "Account Number");
			alternateGLAccountNumTextBoxColumnStyleInfo.ColumnName = "AlternateGLAccountNum";
			alternateGLAccountNumTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			alternateGLAccountNumTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			accountNumWithSeparatorTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f9ebb606-fac1-4a26-aba5-2e26a757fd64", "Account Number (With Separator)");
			accountNumWithSeparatorTextBoxColumnStyleInfo.ColumnName = "AlternateGLAccount+AccountNumWithSeparator";
			accountNumWithSeparatorTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			accountNumWithSeparatorTextBoxColumnStyleInfo.IsReadOnly = true;
			accountNumWithSeparatorTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			debitCreditDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c1d9be03-a9d0-40c8-9139-523461929e94", "Debit/Credit");
			debitCreditDropEditColumnStyleInfo.ColumnName = "DebitCredit";
			debitCreditDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			debitCreditDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			accountNameTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d6017239-a722-42c9-8315-671c0ffbdb79", "Account Name");
			accountNameTextBoxColumnStyleInfo.ColumnName = "Description";
			accountNameTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			accountNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			reportSectionDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4b3dd087-7b49-468a-9262-cb0eb292de8e", "Report Section");
			reportSectionDropEditColumnStyleInfo.ColumnName = "ReportSection";
			reportSectionDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			reportSectionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			accountforTotalTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5c200aa-f989-463e-a900-9dd1a39eda4a", "Account for Total");
			accountforTotalTextBoxColumnStyleInfo.ColumnName = "AlternateGLAccount+PrefixedAccountNum";
			accountforTotalTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			accountforTotalTextBoxColumnStyleInfo.IsReadOnly = true;
			accountforTotalTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			percentNumAlternateGLAccountFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("64a3235a-87d5-4849-b36c-df594aa27f14", "Percent Number");
			percentNumAlternateGLAccountFindBoxColumnStyleInfo.ColumnName = "PercentNum";
			percentNumAlternateGLAccountFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			percentNumAlternateGLAccountFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			consolidationNumAlternateGLAccountFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9bc24d5a-96d8-46dd-80c9-3dc5f1f32501", "Consolidate");
			consolidationNumAlternateGLAccountFindBoxColumnStyleInfo.ColumnName = "ConsolidationNum";
			consolidationNumAlternateGLAccountFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			consolidationNumAlternateGLAccountFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			alternateNumAlternateGLAccountFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4e2b851d-9bd2-448c-8966-7c26cd96032a", "Alternate Number");
			alternateNumAlternateGLAccountFindBoxColumnStyleInfo.ColumnName = "AlternateNum";
			alternateNumAlternateGLAccountFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			alternateNumAlternateGLAccountFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			printSequenceCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			printSequenceCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e9933ff4-85b3-4700-af08-2036fa712958", "Print Sequence");
			printSequenceCalcEditColumnStyleInfo.ColumnName = "PrintSequence";
			printSequenceCalcEditColumnStyleInfo.DefaultCollectionIndex = 0;
			printSequenceCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.attributeGrid.ColumnStyles.Add(oRGTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(oCGTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(tICTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(lFETextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(lFOTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(sPRTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(alternateGLAccountNumTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(accountNumWithSeparatorTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(debitCreditDropEditColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(accountNameTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(reportSectionDropEditColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(accountforTotalTextBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(percentNumAlternateGLAccountFindBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(consolidationNumAlternateGLAccountFindBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(alternateNumAlternateGLAccountFindBoxColumnStyleInfo);
			this.attributeGrid.ColumnStyles.Add(printSequenceCalcEditColumnStyleInfo);
			this.attributeGrid.GridId = "27095883-99a2-469d-95fb-b8186365687f";
			this.attributeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributeGrid.LayoutKey = "attributeGrid";
			this.attributeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.attributeGrid.MaximumRows = 1;
			this.attributeGrid.Name = "attributeGrid";
			this.attributeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 214, true);
			this.attributeGrid.TabIndex = 20;
			this.attributeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// AlternateGLAccountWithAttributeGridControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.attributeGrid);
			this.Name = "AlternateGLAccountWithAttributeGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 218, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.attributeGrid)).EndInit();
			this.attributeGrid.ResumeLayout(false);
			this.attributeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZGrid attributeGrid;
	}
}
