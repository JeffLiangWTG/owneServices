namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class SingleAlternateGLAccountControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.singleAlternateGLAccountPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.accountNumberWithSeparatorTextBox = new ZArchitecture.ZTextBox();
			this.reportSectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.totalReferenceGuidFindBox = new AlternateGLAccountZGuidFindBox();
			this.alternateNumGuidFindBox = new AlternateGLAccountZGuidFindBox();
			this.debitCreditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.consolidateGuidFindBox = new AlternateGLAccountZGuidFindBox();
			this.percentNumberGuidFindBox = new AlternateGLAccountZGuidFindBox();
			this.printSequenceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.totalLevelCalcEdit = new ZArchitecture.ZCalcEdit();
			this.prefixedACNumTextBox = new ZArchitecture.ZTextBox();
			this.accountNameTextBox = new ZArchitecture.ZTextBox();
			this.accountNumberTextBox = new ZArchitecture.ZTextBox();
			this.existAlternateAccountLabel = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.singleAlternateGLAccountPanel.SuspendLayout();
			this.reportSectionDropEdit.SuspendLayout();
			this.totalReferenceGuidFindBox.SuspendLayout();
			this.alternateNumGuidFindBox.SuspendLayout();
			this.debitCreditDropEdit.SuspendLayout();
			this.consolidateGuidFindBox.SuspendLayout();
			this.percentNumberGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.AlternateGLAccounts);
			// 
			// singleAlternateGLAccountPanel
			// 
			this.singleAlternateGLAccountPanel.Controls.Add(this.accountNumberWithSeparatorTextBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.reportSectionDropEdit);
			this.singleAlternateGLAccountPanel.Controls.Add(this.totalReferenceGuidFindBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.alternateNumGuidFindBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.debitCreditDropEdit);
			this.singleAlternateGLAccountPanel.Controls.Add(this.consolidateGuidFindBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.percentNumberGuidFindBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.printSequenceCalcEdit);
			this.singleAlternateGLAccountPanel.Controls.Add(this.totalLevelCalcEdit);
			this.singleAlternateGLAccountPanel.Controls.Add(this.prefixedACNumTextBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.accountNameTextBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.accountNumberTextBox);
			this.singleAlternateGLAccountPanel.Controls.Add(this.existAlternateAccountLabel);
			this.singleAlternateGLAccountPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.singleAlternateGLAccountPanel.Name = "singleAlternateGLAccountPanel";
			this.singleAlternateGLAccountPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 215, true);
			this.singleAlternateGLAccountPanel.TabIndex = 9;
			// 
			// AccountNumberWithSeparatorTextBox
			// 
			this.BindingSource.SetBindingMember(this.accountNumberWithSeparatorTextBox, "FirstAlternateGLAccountWithAttributeSet+AlternateGLAccount+AccountNumWithSeparator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AccountNumWithSeparator)));
			this.accountNumberWithSeparatorTextBox.CaptionResourceString = null;
			this.accountNumberWithSeparatorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 4, true);
			this.accountNumberWithSeparatorTextBox.Name = "accountNumberWithSeparatorTextBox";
			this.accountNumberWithSeparatorTextBox.ReadOnly = true;
			this.accountNumberWithSeparatorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.accountNumberWithSeparatorTextBox.TabStop = false;
			Enterprise.ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(accountNumberWithSeparatorTextBox);
			//
			// existAlternateAccountLabel
			// 
			this.existAlternateAccountLabel.AutoSize = true;
			this.existAlternateAccountLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9CE7EC18-18C4-4362-B2AF-1FE66209D57E", "Existing");
			this.existAlternateAccountLabel.FontType = (ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.existAlternateAccountLabel.ForeColor = System.Drawing.Color.Blue;
			this.existAlternateAccountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 4, true);
			this.existAlternateAccountLabel.Name = "existAlternateAccountLabel";
			this.existAlternateAccountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.accountNumberWithSeparatorTextBox.TabStop = false;
			this.existAlternateAccountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.existAlternateAccountLabel.Visible = false;
			// 
			// reportSectionDropEdit
			// 
			this.reportSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reportSectionDropEdit, "FirstAlternateGLAccountWithAttributeSet+ReportSection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection)));
			this.reportSectionDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9a690cf5-d937-40de-8edb-a959f58f76ec", "Report Section");
			this.reportSectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 51, true);
			this.reportSectionDropEdit.Name = "reportSectionDropEdit";
			this.reportSectionDropEdit.PreBoundMaxLength = 2;
			this.reportSectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 15, true);
			this.reportSectionDropEdit.TabIndex = 10;
			// 
			// totalReferenceGuidFindBox
			// 
			this.totalReferenceGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.totalReferenceGuidFindBox, "FirstAlternateGLAccountWithAttributeSet+HeaderDependsOnTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal)));
			this.totalReferenceGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca658c4f-98b3-48f4-a54f-a9530ab29589", "Total Reference");
			this.totalReferenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 154, true);
			this.totalReferenceGuidFindBox.Name = "totalReferenceGuidFindBox";
			this.totalReferenceGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.totalReferenceGuidFindBox.ParentType = null;
			this.totalReferenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.totalReferenceGuidFindBox.TabIndex = 14;
			// 
			// alternateNumGuidFindBox
			// 
			this.alternateNumGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.alternateNumGuidFindBox, "FirstAlternateGLAccountWithAttributeSet+AlternateNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_AlternateNum)));
			this.alternateNumGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6e25c748-4e0b-4177-b300-ff40305e3afd", "Alternate Number");
			this.alternateNumGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 126, true);
			this.alternateNumGuidFindBox.Name = "alternateNumGuidFindBox";
			this.alternateNumGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.alternateNumGuidFindBox.ParentType = null;
			this.alternateNumGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.alternateNumGuidFindBox.TabIndex = 13;
			// 
			// debitCreditDropEdit
			// 
			this.debitCreditDropEdit.AllowDrop = true;
			this.debitCreditDropEdit.DescriptionBox.Visible = false;
			this.BindingSource.SetBindingMember(this.debitCreditDropEdit, "FirstAlternateGLAccountWithAttributeSet+DebitCredit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit)));
			this.debitCreditDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a9649f59-f540-4f32-a228-f53cf8f876ee", "Debit/Credit");
			this.debitCreditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 4, true);
			this.debitCreditDropEdit.Name = "debitCreditDropEdit";
			this.debitCreditDropEdit.PreBoundMaxLength = 2;
			this.debitCreditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.debitCreditDropEdit.TabIndex = 8;
			// 
			// consolidateGuidFindBox
			// 
			this.consolidateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consolidateGuidFindBox, "FirstAlternateGLAccountWithAttributeSet+ConsolidationNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_ConsolidationNum)));
			this.consolidateGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0e64f31d-70bb-4f27-bada-31cc31e2d0ae", "Consolidate");
			this.consolidateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 100, true);
			this.consolidateGuidFindBox.Name = "consolidateGuidFindBox";
			this.consolidateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.consolidateGuidFindBox.ParentType = null;
			this.consolidateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.consolidateGuidFindBox.TabIndex = 12;
			// 
			// percentNumberGuidFindBox
			// 
			this.percentNumberGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.percentNumberGuidFindBox, "FirstAlternateGLAccountWithAttributeSet+PercentNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_PercentNum)));
			this.percentNumberGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ad881601-473d-49aa-91b5-017622c6ab94", "Percent Number");
			this.percentNumberGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 74, true);
			this.percentNumberGuidFindBox.Name = "percentNumberGuidFindBox";
			this.percentNumberGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.percentNumberGuidFindBox.ParentType = null;
			this.percentNumberGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.percentNumberGuidFindBox.TabIndex = 11;
			// 
			// printSequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.printSequenceCalcEdit, "FirstAlternateGLAccountWithAttributeSet+PrintSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_PrintSequence)));
			this.printSequenceCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d0450055-cf32-41ed-be16-9a1b69fab31b", "Print Sequence");
			this.printSequenceCalcEdit.DecimalPlaces = 2;
			this.printSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 184, true);
			this.printSequenceCalcEdit.Name = "printSequenceCalcEdit";
			this.printSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.printSequenceCalcEdit.TabIndex = 16;
			this.printSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.printSequenceCalcEdit.TrackDisposedAccess = true;
			// 
			// totalLevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalLevelCalcEdit, "FirstAlternateGLAccountWithAttributeSet+TotalLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_TotalLevel)));
			this.totalLevelCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7036925c-29bc-4aff-a98e-242e2e6ded30", "Total Level");
			this.totalLevelCalcEdit.DecimalPlaces = 2;
			this.totalLevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 184, true);
			this.totalLevelCalcEdit.Name = "totalLevelCalcEdit";
			this.totalLevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.totalLevelCalcEdit.TabIndex = 15;
			this.totalLevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.totalLevelCalcEdit.TrackDisposedAccess = true;
			// 
			// prefixedACNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.prefixedACNumTextBox, "FirstAlternateGLAccountWithAttributeSet+AlternateGLAccount+PrefixedAccountNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PrefixedAccountNum)));
			this.prefixedACNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("64b5836c-62cc-4f97-a1ef-e024c54640f3", "Account for Total");
			this.prefixedACNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 51, true);
			this.prefixedACNumTextBox.Name = "prefixedACNumTextBox";
			this.prefixedACNumTextBox.ReadOnly = true;
			this.prefixedACNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 15, true);
			this.prefixedACNumTextBox.TabStop = false;
			// 
			// accountNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.accountNameTextBox, "FirstAlternateGLAccountWithAttributeSet+Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description)));
			this.accountNameTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("375dfd54-82d2-4f9c-ae91-45c99039f4ea", "Account Name");
			this.accountNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 28, true);
			this.accountNameTextBox.Name = "accountNameTextBox";
			this.accountNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.accountNameTextBox.TabIndex = 9;
			// 
			// accountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.accountNumberTextBox, "FirstAlternateGLAccountWithAttributeSet+AlternateGLAccountNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum)));
			this.accountNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3b4db96b-c0e8-4a7d-a7e8-aee66da61c07", "Account Number");
			this.accountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 4, true);
			this.accountNumberTextBox.Name = "accountNumberTextBox";
			this.accountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 15, true);
			this.accountNumberTextBox.TabIndex = 7;
			// 
			// SingleAlternateGLAccountControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.singleAlternateGLAccountPanel);
			this.Name = "SingleAlternateGLAccountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 223, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.singleAlternateGLAccountPanel.ResumeLayout(false);
			this.singleAlternateGLAccountPanel.PerformLayout();
			this.reportSectionDropEdit.ResumeLayout(true);
			this.reportSectionDropEdit.PerformLayout();
			this.totalReferenceGuidFindBox.ResumeLayout(true);
			this.totalReferenceGuidFindBox.PerformLayout();
			this.alternateNumGuidFindBox.ResumeLayout(true);
			this.alternateNumGuidFindBox.PerformLayout();
			this.debitCreditDropEdit.ResumeLayout(true);
			this.debitCreditDropEdit.PerformLayout();
			this.consolidateGuidFindBox.ResumeLayout(true);
			this.consolidateGuidFindBox.PerformLayout();
			this.percentNumberGuidFindBox.ResumeLayout(true);
			this.percentNumberGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel singleAlternateGLAccountPanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit reportSectionDropEdit;
		AlternateGLAccountZGuidFindBox totalReferenceGuidFindBox;
		AlternateGLAccountZGuidFindBox alternateNumGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit debitCreditDropEdit;
		AlternateGLAccountZGuidFindBox consolidateGuidFindBox;
		AlternateGLAccountZGuidFindBox percentNumberGuidFindBox;
		ZArchitecture.ZCalcEdit printSequenceCalcEdit;
		ZArchitecture.ZCalcEdit totalLevelCalcEdit;
		ZArchitecture.ZTextBox prefixedACNumTextBox;
		ZArchitecture.ZTextBox accountNameTextBox;
		ZArchitecture.ZTextBox accountNumberWithSeparatorTextBox;
		ZArchitecture.ZTextBox accountNumberTextBox;
		ZArchitecture.ZLabel existAlternateAccountLabel;
	}
}
