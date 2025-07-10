namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	partial class FeatureSetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FeatureRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DatabaseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DatabaseGrid = new Enterprise.Client.EDI.FeatureControl.GUI.FeatureSetLicenceDatabaseModuleButtonGrid();
			this.FeatureRuleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FeatureRuleGrid)).BeginInit();
			this.FeatureRuleGrid.SuspendLayout();
			this.DatabaseTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseGrid.InnerGrid)).BeginInit();
			this.DatabaseGrid.SuspendLayout();
			this.FeatureRuleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.DatabaseTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 451, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DatabaseTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.CodeTextBox);
			this.MainTabPage.Controls.Add(this.FeatureRuleGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 429, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 429, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 429, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 451, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet);
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "FCS_ProductName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).FCS_ProductName)));
			this.CodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
			this.CodeTextBox.TabIndex = 11;
			// 
			// FeatureRuleGrid
			// 
			this.FeatureRuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeatureRuleGrid, "FeatureRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).FeatureRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).FeatureRules)).SyncRoot)).FCR_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).FeatureRules)).SyncRoot)).FeatureHeaderDescription)));
			this.FeatureRuleGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "FCR_IsActive";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "FeatureHeaderDescription";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FeatureRuleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FeatureRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FeatureRuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeatureRuleGrid.GridId = "7f9e7c5e-094c-46c2-a8a9-631972760b5e";
			this.FeatureRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeatureRuleGrid.LayoutKey = "FeatureRuleGrid";
			this.FeatureRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.FeatureRuleGrid.Name = "FeatureRuleGrid";
			this.FeatureRuleGrid.ReadOnly = true;
			this.FeatureRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 362, true);
			this.FeatureRuleGrid.TabIndex = 0;
			// 
			// DatabaseTabPage
			// 
			this.DatabaseTabPage.Controls.Add(this.DatabaseGrid);
			this.DatabaseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DatabaseTabPage.Name = "DatabaseTabPage";
			this.DatabaseTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DatabaseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 553, true);
			this.DatabaseTabPage.TabIndex = 4;
			this.DatabaseTabPage.Text = "Databases";
			this.DatabaseTabPage.UseVisualStyleBackColor = true;
			// 
			// DatabaseGrid
			// 
			this.DatabaseGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatabaseGrid, "Databases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).Databases)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet)(null)).Lookups.ActiveCargoWiseDatabases)));
			this.DatabaseGrid.BindToFindBoxList = "Lookups+ActiveCargoWiseDatabases";
			zTextBoxColumnStyleInfo1.ColumnName = "LD_DatabaseNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EDIWebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "EDIWebAccessOrg+OH_FullName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "LD_FeatureSetConfigDateUtc";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DatabaseGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.DatabaseGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseGrid.GridId = null;
			// 
			// 
			// 
			this.DatabaseGrid.InnerGrid.AllowNavigation = false;
			this.DatabaseGrid.InnerGrid.CaptionVisible = false;
			this.DatabaseGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseGrid.InnerGrid.GridId = null;
			this.DatabaseGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DatabaseGrid.InnerGrid.LayoutKey = "Grid";
			this.DatabaseGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DatabaseGrid.InnerGrid.Name = "Grid";
			this.DatabaseGrid.InnerGrid.ReadOnly = true;
			this.DatabaseGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 512, true);
			this.DatabaseGrid.InnerGrid.TabIndex = 0;
			this.DatabaseGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DatabaseGrid.Name = "DatabaseGrid";
			this.DatabaseGrid.ReadOnly = true;
			this.DatabaseGrid.ShowEditButton = false;
			this.DatabaseGrid.ShowNewButton = false;
			this.DatabaseGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 548, true);
			this.DatabaseGrid.TabIndex = 51;
			// 
			// FeatureRuleGroupBox
			// 
			this.FeatureRuleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FeatureRuleGroupBox.Controls.Add(this.FeatureRuleGrid);
			this.FeatureRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 48, true);
			this.FeatureRuleGroupBox.Name = "FeatureRuleGroupBox";
			this.FeatureRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 379, true);
			this.FeatureRuleGroupBox.TabIndex = 12;
			this.FeatureRuleGroupBox.TabStop = false;
			this.FeatureRuleGroupBox.Text = "Feature Rules";
			// 
			// FeatureSetForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 507, true);
			this.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlSet);
			this.Name = "FeatureSetForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Feature Set";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FeatureRuleGrid)).EndInit();
			this.FeatureRuleGrid.ResumeLayout(false);
			this.FeatureRuleGrid.PerformLayout();
			this.DatabaseTabPage.ResumeLayout(false);
			this.DatabaseTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseGrid.InnerGrid)).EndInit();
			this.DatabaseGrid.ResumeLayout(true);
			this.DatabaseGrid.PerformLayout();
			this.FeatureRuleGroupBox.ResumeLayout(false);
			this.FeatureRuleGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.GUI.ZGroupBox FeatureRuleGroupBox;
		private ZArchitecture.ZGrid FeatureRuleGrid;
		private ZArchitecture.GUI.ZTabPage DatabaseTabPage;
		private FeatureSetLicenceDatabaseModuleButtonGrid DatabaseGrid;
	}
}
