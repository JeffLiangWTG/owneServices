namespace Enterprise.Accounting.Registry.GUI
{
	public partial class GLJournalApprovalThresholdControl
	{

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AccountTypeGrid = new ZArchitecture.ZGrid();
			this.ApprovalThresholdSetupGrid = new ZArchitecture.ZGrid();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AccountTypeGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalThresholdSetupGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.GLJournalApprovalThresholdCollection);
			// 
			// AccountTypeGrid
			// 
			this.AccountTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AccountTypeGrid, ".");
			this.AccountTypeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|01582eb4-9c34-4a9b-aecf-75cd2df7b33b", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|fe9578ee-fabc-42d5-bb32-8ebd4ed6baf7", "GL Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GLAccount";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|e3ac81f3-2472-419a-8f78-fccf3ee12801", "Report Section");
			zDropEditColumnStyleInfo2.ColumnName = "ReportSection";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|9f0fec3f-1074-4fac-bc15-ca4c9b0598d4", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			this.AccountTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AccountTypeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AccountTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AccountTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AccountTypeGrid.CopySelectedRowsAllowed = true;
			this.AccountTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountTypeGrid.GridId = "e55db716-ce5c-435f-a9b6-365d46de0fa9";
			this.AccountTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccountTypeGrid.LayoutKey = "AccountTypeGrid";
			this.AccountTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccountTypeGrid.Name = "AccountTypeGrid";
			this.AccountTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 161, true);
			this.AccountTypeGrid.TabIndex = 0;
			// 
			// ApprovalThresholdSetupGrid
			// 
			this.ApprovalThresholdSetupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApprovalThresholdSetupGrid, "AuthorisationSettings");
			this.ApprovalThresholdSetupGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|7d019ee9-3974-4f3f-b754-a6b67d02f803", "Range");
			zDropEditColumnStyleInfo3.ColumnName = "RangeLocalized";
			zDropEditColumnStyleInfo3.Width = 55;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|95a86bb4-9dea-4063-b686-4766b7f230a3", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalApprovalThresholdControl|29cd8d49-e4c8-4a44-b828-81bea82cdb5f", "Authorization Requirement");
			zDropEditColumnStyleInfo4.ColumnName = "AuthorisationRequirementLocalized";
			zDropEditColumnStyleInfo4.Width = 160;
			this.ApprovalThresholdSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ApprovalThresholdSetupGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ApprovalThresholdSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ApprovalThresholdSetupGrid.CopySelectedRowsAllowed = true;
			this.ApprovalThresholdSetupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovalThresholdSetupGrid.GridId = "e0aaf5f0-fc67-4f8d-86d1-8a253a1d1250";
			this.ApprovalThresholdSetupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApprovalThresholdSetupGrid.LayoutKey = "ApprovalThresholdSetupGrid";
			this.ApprovalThresholdSetupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApprovalThresholdSetupGrid.Name = "ApprovalThresholdSetupGrid";
			this.ApprovalThresholdSetupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 157, true);
			this.ApprovalThresholdSetupGrid.TabIndex = 0;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.AccountTypeGrid);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.ApprovalThresholdSetupGrid);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.splitContainer.TabIndex = 0;
			// 
			// GLJournalApprovalThresholdControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "GLJournalApprovalThresholdControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AccountTypeGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalThresholdSetupGrid)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		protected ZArchitecture.ZGrid ApprovalThresholdSetupGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer;
		ZArchitecture.ZGrid AccountTypeGrid;
	}
}
