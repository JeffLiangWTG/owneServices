namespace Enterprise.Accounting
{
	public partial class AccReportingBookForm
	{

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 300, true);
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 234, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 211, true);
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccReportingBookForm|d076cc00-d4ef-45d3-8fab-dc888c56b871", "Details");
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 211, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 234, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccReportingBook);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_AAC_AlternateChart)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_GC_CompanyOfPeriod)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_IncludePresentationJournals)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_IsGlobal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_IncludeChildPresentation)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccReportingBook)(null)).ARB_RX_NKCurrency)));
			// 
			// AccReportingBookForm
			// 
			this.AutoAddPreviousNextButtons = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccReportingBookForm|a4960973-b449-42e7-ae69-b7c9e7a08dc6", "Reporting Books");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 250, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccReportingBook);
			this.Name = "AccReportingBookForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ARB_Code = new Enterprise.ZArchitecture.ZTextBox();
			this.ARB_AAC_AlternateChart = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ARB_GC_CompanyOfPeriod = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ARB_IsActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARB_IncludePresentationJournals = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ARB_IsGlobal = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARB_IncludeChildPresentation = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARB_Description = new Enterprise.ZArchitecture.ZTextBox();
			this.ARB_RX_NKCurrency = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabPage.SuspendLayout();
			this.ARB_AAC_AlternateChart.SuspendLayout();
			this.ARB_GC_CompanyOfPeriod.SuspendLayout();
			this.ARB_IncludePresentationJournals.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ARB_RX_NKCurrency.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// ARB_Code
			// 
			this.BindingSource.SetBindingMember(this.ARB_Code, "ARB_Code");
			this.ARB_Code.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.ARB_Code.Name = "ARB_Code";
			this.ARB_Code.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.ARB_Code.TabIndex = 0;
			this.ARB_Code.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReportingBookCodeTextBox_KeyPress);
			// 
			// ARB_IsGlobal
			// 
			this.BindingSource.SetBindingMember(this.ARB_IsGlobal, "ARB_IsGlobal");
			this.ARB_IsGlobal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 20, true);
			this.ARB_IsGlobal.Name = "ARB_IsGlobal";
			this.ARB_IsGlobal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.ARB_IsGlobal.TabIndex = 1;
			// 
			// ARB_IsActive
			// 
			this.BindingSource.SetBindingMember(this.ARB_IsActive, "ARB_IsActive");
			this.ARB_IsActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 20, true);
			this.ARB_IsActive.Name = "ARB_IsActive";
			this.ARB_IsActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.ARB_IsActive.TabIndex = 2;
			// 
			// ARB_Description
			// 
			this.BindingSource.SetBindingMember(this.ARB_Description, "ARB_Description");
			this.ARB_Description.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
			this.ARB_Description.Name = "ARB_Description";
			this.ARB_Description.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 20, true);
			this.ARB_Description.TabIndex = 3;
			// 
			// ARB_AAC_AlternateChart
			// 
			this.ARB_AAC_AlternateChart.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARB_AAC_AlternateChart, "ARB_AAC_AlternateChart");
			this.ARB_AAC_AlternateChart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 66, true);
			this.ARB_AAC_AlternateChart.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AlternateChartofAccounts;
			this.ARB_AAC_AlternateChart.Name = "ARB_AAC_AlternateChart";
			this.ARB_AAC_AlternateChart.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ARB_AAC_AlternateChart.ParentType = null;
			this.ARB_AAC_AlternateChart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 20, true);
			this.ARB_AAC_AlternateChart.TabIndex = 4;
			// 
			// ARB_IncludePresentationJournals
			// 
			this.ARB_IncludePresentationJournals.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARB_IncludePresentationJournals, "ARB_IncludePresentationJournals");
			this.ARB_IncludePresentationJournals.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 89, true);
			this.ARB_IncludePresentationJournals.Name = "ARB_IncludePresentationJournals";
			this.ARB_IncludePresentationJournals.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.ARB_IncludePresentationJournals.TabIndex = 5;
			//
			// ARB_IncludeChildPresentation
			// 
			this.BindingSource.SetBindingMember(this.ARB_IncludeChildPresentation, "ARB_IncludeChildPresentation");
			this.ARB_IncludeChildPresentation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 89, true);
			this.ARB_IncludeChildPresentation.Name = "ARB_IncludeChildPresentation";
			this.ARB_IncludeChildPresentation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ARB_IncludeChildPresentation.TabIndex = 6;
			// 
			// ARB_GC_CompanyOfPeriod
			// 
			this.ARB_GC_CompanyOfPeriod.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARB_GC_CompanyOfPeriod, "ARB_GC_CompanyOfPeriod");
			this.ARB_GC_CompanyOfPeriod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 112, true);
			this.ARB_GC_CompanyOfPeriod.Name = "ARB_GC_CompanyOfPeriod";
			this.ARB_GC_CompanyOfPeriod.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ARB_GC_CompanyOfPeriod.ParentType = null;
			this.ARB_GC_CompanyOfPeriod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 20, true);
			this.ARB_GC_CompanyOfPeriod.TabIndex = 7;
			// 
			// ARB_RX_NKCurrency
			// 
			this.ARB_RX_NKCurrency.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARB_RX_NKCurrency, "ARB_RX_NKCurrency");
			this.ARB_RX_NKCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 135, true);
			this.ARB_RX_NKCurrency.Name = "ARB_RX_NKCurrency";
			this.ARB_RX_NKCurrency.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ARB_RX_NKCurrency.ParentType = null;
			this.ARB_RX_NKCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 20, true);
			this.ARB_RX_NKCurrency.Visible = false;
			this.ARB_RX_NKCurrency.TabIndex = 8;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.ARB_Code);
			this.DetailsGroupBox.Controls.Add(this.ARB_IsGlobal);
			this.DetailsGroupBox.Controls.Add(this.ARB_IsActive);
			this.DetailsGroupBox.Controls.Add(this.ARB_Description);
			this.DetailsGroupBox.Controls.Add(this.ARB_AAC_AlternateChart);
			this.DetailsGroupBox.Controls.Add(this.ARB_IncludePresentationJournals);
			this.DetailsGroupBox.Controls.Add(this.ARB_IncludeChildPresentation);
			this.DetailsGroupBox.Controls.Add(this.ARB_GC_CompanyOfPeriod);
			this.DetailsGroupBox.Controls.Add(this.ARB_RX_NKCurrency);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsGroupBox, false);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 175, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabPage.PerformLayout();
			this.ARB_AAC_AlternateChart.ResumeLayout(true);
			this.ARB_AAC_AlternateChart.PerformLayout();
			this.ARB_GC_CompanyOfPeriod.ResumeLayout(true);
			this.ARB_GC_CompanyOfPeriod.PerformLayout();
			this.ARB_IncludePresentationJournals.ResumeLayout(true);
			this.ARB_IncludePresentationJournals.PerformLayout();
			this.ARB_RX_NKCurrency.ResumeLayout(false);
			this.ARB_RX_NKCurrency.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		Enterprise.ZArchitecture.ZTextBox ARB_Code;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARB_IsGlobal;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARB_IncludeChildPresentation;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARB_IsActive;
		Enterprise.ZArchitecture.ZTextBox ARB_Description;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ARB_AAC_AlternateChart;
		Enterprise.ZArchitecture.GUI.ZDropEdit ARB_IncludePresentationJournals;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ARB_GC_CompanyOfPeriod;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ARB_RX_NKCurrency;

	}
}
