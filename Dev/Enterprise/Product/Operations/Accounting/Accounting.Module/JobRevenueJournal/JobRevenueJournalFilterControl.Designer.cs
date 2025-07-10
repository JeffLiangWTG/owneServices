namespace Enterprise.Accounting.Module
{
	public partial class JobRevenueJournalFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			this.BindingSource.SetBindingMember(this.FilteredGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobInvoicing.JobRevenueJournal)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobInvoicing.JobRevenueJournal)(null)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobInvoicing.JobRevenueJournal)(null)).JobLocalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.JobInvoicing.JobRevenueJournal)(null)).AH_IsCancelled)));
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("JobRevenueJournalFilterControl|1a893dba-1cbe-444b-bd07-98dd8eada845", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("JobRevenueJournalFilterControl|3e52e187-1a7b-4dd9-a1fa-71b2d8311ef8", "Date", "Transaction Date", "");
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zTextBoxColumnStyleInfo2.ColumnName = "AH_SystemCreateUser";
			zDateEditColumnStyleInfo3.ColumnName = "AH_SystemCreateTimeUtc";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AH_GE";
			zTextBoxColumnStyleInfo3.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("JobRevenueJournalFilterControl|f5b56889-bd76-4121-ac22-9aef192c7c29", "Job Number");
			zTextBoxColumnStyleInfo4.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("JobRevenueJournalFilterControl|574a7b56-519c-4f8f-9b3a-4fe2e4bf55ef", "Job Local Ref", "Job Local Reference", "");
			zTextBoxColumnStyleInfo5.ColumnName = "JobLocalReference";
			zCheckBoxColumnStyleInfo1.ColumnName = "AH_IsCancelled";
			zTextBoxColumnStyleInfo6.ColumnName = "AH_GS_NKAuditedBy";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("JobRevenueJournalFilterControl|B92D70C1-CCAC-406D-A8D6-B69F7AF9962B", "Audited By");
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.HeaderForeColor = System.Drawing.SystemColors.ControlDark;
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 78, true);
			this.FilteredGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 192, true);
			this.FilteredGrid.TabIndex = 19;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobInvoicing.JobRevenueJournal);
			// 
			// JobRevenueJournalFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "JobRevenueJournalFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
