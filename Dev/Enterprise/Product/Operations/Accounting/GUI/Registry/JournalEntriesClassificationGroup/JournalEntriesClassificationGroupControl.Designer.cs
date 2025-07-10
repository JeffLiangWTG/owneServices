namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class JournalEntriesClassificationGroupControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.journalEntriesClassificationGroupGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.journalEntriesClassificationGroupGrid)).BeginInit();
			this.journalEntriesClassificationGroupGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCollection);
			// 
			// journalEntriesClassificationGroupGrid
			// 
			this.journalEntriesClassificationGroupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.journalEntriesClassificationGroupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroup)(null)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroup)(null)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroup)(null)).GroupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroup)(null)).GroupCodeDescription)));
			this.journalEntriesClassificationGroupGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7BFAC912-1E96-424A-944D-618BDBD2A9FD", "Ledger");
			zTextBoxColumnStyleInfo1.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5baa582d-e28b-4563-8c12-8a196c0bf70d", "Transaction Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a7e38538-61ea-4f33-b905-017de39464ce", "Group Code");
			zDropEditColumnStyleInfo1.ColumnName = "GroupCode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b4f0fb96-21f7-4add-9518-58711cd66904", "Code Description");
			zTextBoxColumnStyleInfo3.ColumnName = "GroupCodeDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.journalEntriesClassificationGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.journalEntriesClassificationGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.journalEntriesClassificationGroupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.journalEntriesClassificationGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.journalEntriesClassificationGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.journalEntriesClassificationGroupGrid.GridId = "1c8fd163-5ba1-4e86-b70b-a5385c03b7d2";
			this.journalEntriesClassificationGroupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.journalEntriesClassificationGroupGrid.LayoutKey = "zGrid1";
			this.journalEntriesClassificationGroupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.journalEntriesClassificationGroupGrid.Name = "journalEntriesClassificationGroupGrid";
			this.journalEntriesClassificationGroupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 342, true);
			this.journalEntriesClassificationGroupGrid.TabIndex = 0;
			this.journalEntriesClassificationGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// JournalEntriesClassificationGroupControl
			// 
			this.Controls.Add(this.journalEntriesClassificationGroupGrid);
			this.Name = "JournalEntriesClassificationGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 342, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.journalEntriesClassificationGroupGrid)).EndInit();
			this.journalEntriesClassificationGroupGrid.ResumeLayout(false);
			this.journalEntriesClassificationGroupGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.ZGrid journalEntriesClassificationGroupGrid;
	}
}
