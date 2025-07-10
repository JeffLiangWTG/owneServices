namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class JournalEntriesNumberCustomisationSettingControl
	{
		private void InitializeComponent()
		{
			this.accountingTransactionsNumberSequenceCustomisationControl = new Enterprise.Accounting.Registry.GUI.AccountingTransactionsNumberSequenceCustomisationControl();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.numberRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.allocationOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.sequenceResetOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.accountingTransactionsNumberSequenceCustomisationControl.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.numberRuleDropEdit.SuspendLayout();
			this.allocationOptionDropEdit.SuspendLayout();
			this.sequenceResetOptionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisationSetting);
			// 
			// accountingTransactionsNumberSequenceCustomisationControl
			// 
			this.accountingTransactionsNumberSequenceCustomisationControl.AllowDrop = true;
			this.accountingTransactionsNumberSequenceCustomisationControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.accountingTransactionsNumberSequenceCustomisationControl, "NumberSequenceCustomisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisationCollection)(((Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisationSetting)(null)).NumberSequenceCustomisations)));
			this.accountingTransactionsNumberSequenceCustomisationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accountingTransactionsNumberSequenceCustomisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.accountingTransactionsNumberSequenceCustomisationControl.Name = "accountingTransactionsNumberSequenceCustomisationControl";
			this.accountingTransactionsNumberSequenceCustomisationControl.ReadOnly = false;
			this.accountingTransactionsNumberSequenceCustomisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 184, true);
			this.accountingTransactionsNumberSequenceCustomisationControl.TabIndex = 1;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.allocationOptionDropEdit);
			this.topPanel.Controls.Add(this.numberRuleDropEdit);
			this.topPanel.Controls.Add(this.sequenceResetOptionDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 120, true);
			this.topPanel.TabIndex = 3;
			// 
			// NumberRuleDropEdit
			// 
			this.numberRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.numberRuleDropEdit, "NumberRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisationSetting)(null)).NumberRule)));
			this.numberRuleDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f9b37240-4f61-411a-a9e8-a746473c6773", "Number Rule");
			this.numberRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 15, true);
			this.numberRuleDropEdit.Name = "numberRuleDropEdit";
			this.numberRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.numberRuleDropEdit.TabIndex = 0;
			// 
			// allocationOptionDropEdit
			// 
			this.allocationOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.allocationOptionDropEdit, "AllocationOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisationSetting)(null)).AllocationOption)));
			this.allocationOptionDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c3e24a98-af76-4ee9-bb7a-723c94ba92cd", "Allocation Option");
			this.allocationOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 47, true);
			this.allocationOptionDropEdit.Name = "allocationOptionDropEdit";
			this.allocationOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.allocationOptionDropEdit.TabIndex = 1;
			// 
			// sequenceResetOptionDropEdit
			// 
			this.sequenceResetOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sequenceResetOptionDropEdit, "SequenceResetOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisationSetting)(null)).SequenceResetOption)));
			this.sequenceResetOptionDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("E54BC735-A9BF-4D28-9721-92EAAC0609F4", "Sequence Reset Option");
			this.sequenceResetOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 79, true);
			this.sequenceResetOptionDropEdit.Name = "sequenceResetOptionDropEdit";
			this.sequenceResetOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.sequenceResetOptionDropEdit.TabIndex = 2;
			// 
			// JournalEntriesNumberCustomisationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.accountingTransactionsNumberSequenceCustomisationControl);
			this.Controls.Add(this.topPanel);
			this.Name = "JournalEntriesNumberCustomisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.accountingTransactionsNumberSequenceCustomisationControl.ResumeLayout(true);
			this.accountingTransactionsNumberSequenceCustomisationControl.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.numberRuleDropEdit.ResumeLayout(true);
			this.numberRuleDropEdit.PerformLayout();
			this.allocationOptionDropEdit.ResumeLayout(true);
			this.allocationOptionDropEdit.PerformLayout();
			this.sequenceResetOptionDropEdit.ResumeLayout(true);
			this.sequenceResetOptionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private AccountingTransactionsNumberSequenceCustomisationControl accountingTransactionsNumberSequenceCustomisationControl;
		private ZArchitecture.GUI.ZDropEdit allocationOptionDropEdit;
		private ZArchitecture.GUI.ZDropEdit numberRuleDropEdit;
		private ZArchitecture.GUI.ZDropEdit sequenceResetOptionDropEdit;
	}
}
