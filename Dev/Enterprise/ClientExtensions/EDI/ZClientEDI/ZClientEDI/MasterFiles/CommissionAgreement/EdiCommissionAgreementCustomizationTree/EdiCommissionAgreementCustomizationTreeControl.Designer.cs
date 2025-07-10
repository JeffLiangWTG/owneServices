namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EdiCommissionAgreementCustomizationTreeControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.treeView = new Enterprise.ZArchitecture.GUI.ZTreeViewAdv();
			this.codeTreeColumn = new Aga.Controls.Tree.TreeColumn();
			this.descriptionTreeColumn = new Aga.Controls.Tree.TreeColumn();
			this.autoAddTreeColumn = new Aga.Controls.Tree.TreeColumn();
			this.includeDatabaseUsageTreeColumn = new Aga.Controls.Tree.TreeColumn();
			this.selectedCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this.codeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.descriptionTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.autoAddCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this.includeDatabaseUsageCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this.bottomToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.addCountriesDropDownButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.addCountryForNewDatabasesMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.addCountryForExistingDatabasesMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.addCountryForAllExistingDatabasesMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.addCountryForExistingDatabasesStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.resetToDefaultButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EZN_IsAllDatabasesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EZN_IsAllCompaniesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.treeView.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization);
			// 
			// treeView
			// 
			this.treeView.AutoRowHeight = true;
			this.treeView.BackColor = System.Drawing.SystemColors.Window;
			this.treeView.Columns.Add(this.codeTreeColumn);
			this.treeView.Columns.Add(this.descriptionTreeColumn);
			this.treeView.Columns.Add(this.autoAddTreeColumn);
			this.treeView.Columns.Add(this.includeDatabaseUsageTreeColumn);
			this.treeView.DefaultToolTipProvider = null;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeView.ElementType = null;
			this.treeView.Indent = 32;
			this.treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this.treeView.LineDashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			this.treeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.treeView.Name = "treeView";
			this.treeView.NodeControls.Add(this.selectedCheckBox);
			this.treeView.NodeControls.Add(this.codeTextBox);
			this.treeView.NodeControls.Add(this.descriptionTextBox);
			this.treeView.NodeControls.Add(this.autoAddCheckBox);
			this.treeView.NodeControls.Add(this.includeDatabaseUsageCheckBox);
			this.treeView.SelectedNode = null;
			this.treeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 309, true);
			this.treeView.TabIndex = 1;
			this.treeView.Text = "treeView";
			this.treeView.UseColumns = true;
			// 
			// codeTreeColumn
			// 
			this.codeTreeColumn.Header = "";
			this.codeTreeColumn.Sortable = true;
			this.codeTreeColumn.SortOrder = System.Windows.Forms.SortOrder.Ascending;
			this.codeTreeColumn.TooltipText = null;
			this.codeTreeColumn.Width = 180;
			// 
			// descriptionTreeColumn
			// 
			this.descriptionTreeColumn.Header = "Description";
			this.descriptionTreeColumn.Sortable = true;
			this.descriptionTreeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.descriptionTreeColumn.TooltipText = null;
			this.descriptionTreeColumn.Width = 260;
			// 
			// autoAddTreeColumn
			// 
			this.autoAddTreeColumn.Header = "Auto-add newly created companies";
			this.autoAddTreeColumn.Sortable = true;
			this.autoAddTreeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.autoAddTreeColumn.TooltipText = null;
			this.autoAddTreeColumn.Width = 140;
			// 
			// includeDatabaseUsageTreeColumn
			// 
			this.includeDatabaseUsageTreeColumn.Header = "Include Database Usages";
			this.includeDatabaseUsageTreeColumn.Sortable = true;
			this.includeDatabaseUsageTreeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.includeDatabaseUsageTreeColumn.TooltipText = null;
			this.includeDatabaseUsageTreeColumn.Width = 140;
			// 
			// selectedCheckBox
			// 
			this.selectedCheckBox.DataPropertyName = "Selected";
			this.selectedCheckBox.EditEnabled = true;
			this.selectedCheckBox.LeftMargin = 0;
			this.selectedCheckBox.ParentColumn = this.codeTreeColumn;
			// 
			// codeTextBox
			// 
			this.codeTextBox.DataPropertyName = "Code";
			this.codeTextBox.IncrementalSearchEnabled = true;
			this.codeTextBox.LeftMargin = 3;
			this.codeTextBox.ParentColumn = this.codeTreeColumn;
			this.codeTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.DataPropertyName = "Description";
			this.descriptionTextBox.IncrementalSearchEnabled = true;
			this.descriptionTextBox.LeftMargin = 3;
			this.descriptionTextBox.ParentColumn = this.descriptionTreeColumn;
			this.descriptionTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// autoAddCheckBox
			// 
			this.autoAddCheckBox.DataPropertyName = "ShouldAutoAdd";
			this.autoAddCheckBox.EditEnabled = true;
			this.autoAddCheckBox.LeftMargin = 0;
			this.autoAddCheckBox.ParentColumn = this.autoAddTreeColumn;
			// 
			// includeDatabaseUsageCheckBox
			// 
			this.includeDatabaseUsageCheckBox.DataPropertyName = "IncludeDatabaseUsage";
			this.includeDatabaseUsageCheckBox.EditEnabled = true;
			this.includeDatabaseUsageCheckBox.LeftMargin = 0;
			this.includeDatabaseUsageCheckBox.ParentColumn = this.includeDatabaseUsageTreeColumn;
			// 
			// bottomToolStrip
			// 
			this.bottomToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.bottomToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.bottomToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCountriesDropDownButton});
			this.bottomToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 331, true);
			this.bottomToolStrip.Name = "bottomToolStrip";
			this.bottomToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 21, true);
			this.bottomToolStrip.TabIndex = 2;
			this.bottomToolStrip.Text = "zToolStrip1";
			// 
			// addCountriesDropDownButton
			// 
			this.addCountriesDropDownButton.CaptionResourceString = ZClientEDI.Res.GetData("a68e23ce-e185-486f-b572-5d5b135626c5", "Add Countries/Regions");
			this.addCountriesDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCountryForNewDatabasesMenuItem,
            this.addCountryForExistingDatabasesMenuItem});
			this.addCountriesDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.addCountriesDropDownButton.Name = "addCountriesDropDownButton";
			// 
			// addCountryForNewDatabasesMenuItem
			// 
			this.addCountryForNewDatabasesMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("74296a07-456c-4660-a6d2-ccb534cc5ce8", "New Databases");
			this.addCountryForNewDatabasesMenuItem.Name = "addCountryForNewDatabasesMenuItem";
			this.addCountryForNewDatabasesMenuItem.Click += new System.EventHandler(this.AddCountryForNewDatabasesMenuItem_Click);
			// 
			// addCountryForExistingDatabasesMenuItem
			// 
			this.addCountryForExistingDatabasesMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("6d837e68-99b6-4a41-8dbf-1061c09845e5", "Existing Databases");
			this.addCountryForExistingDatabasesMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCountryForAllExistingDatabasesMenuItem,
            this.addCountryForExistingDatabasesStripSeparator});
			this.addCountryForExistingDatabasesMenuItem.Name = "addCountryForExistingDatabasesMenuItem";
			// 
			// addCountryForAllExistingDatabasesMenuItem
			// 
			this.addCountryForAllExistingDatabasesMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("88a22f95-01a7-4cca-a493-37a85350641b", "All");
			this.addCountryForAllExistingDatabasesMenuItem.Name = "addCountryForAllExistingDatabasesMenuItem";
			this.addCountryForAllExistingDatabasesMenuItem.Click += new System.EventHandler(this.AddCountryForAllExistingDatabasesMenuItem_Click);
			// 
			// addCountryForExistingDatabasesStripSeparator
			// 
			this.addCountryForExistingDatabasesStripSeparator.Name = "addCountryForExistingDatabasesStripSeparator";
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.resetToDefaultButton);
			this.topPanel.Controls.Add(this.EZN_IsAllDatabasesCheckBox);
			this.topPanel.Controls.Add(this.EZN_IsAllCompaniesCheckBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 22, true);
			this.topPanel.TabIndex = 0;
			// 
			// resetToDefaultButton
			// 
			this.resetToDefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.resetToDefaultButton.CaptionResourceString = ZClientEDI.Res.GetData("2973272f-bfb2-4320-8e5f-c3fbf6046362", "Reset to Default");
			this.resetToDefaultButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 0, true);
			this.resetToDefaultButton.Name = "resetToDefaultButton";
			this.resetToDefaultButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.resetToDefaultButton.TabIndex = 2;
			this.resetToDefaultButton.Click += new System.EventHandler(this.ResetToDefaultButton_Click);
			// 
			// EZN_IsAllDatabasesCheckBox
			// 
			this.EZN_IsAllDatabasesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EZN_IsAllDatabasesCheckBox, "EZN_IsAllDatabases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization)(null)).EZN_IsAllDatabases)));
			this.EZN_IsAllDatabasesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EZN_IsAllDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 4, true);
			this.EZN_IsAllDatabasesCheckBox.Name = "EZN_IsAllDatabasesCheckBox";
			this.EZN_IsAllDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 16, true);
			this.EZN_IsAllDatabasesCheckBox.TabIndex = 1;
			this.EZN_IsAllDatabasesCheckBox.UseVisualStyleBackColor = true;
			// 
			// EZN_IsAllCompaniesCheckBox
			// 
			this.EZN_IsAllCompaniesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EZN_IsAllCompaniesCheckBox, "EZN_IsAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization)(null)).EZN_IsAllCompanies)));
			this.EZN_IsAllCompaniesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EZN_IsAllCompaniesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.EZN_IsAllCompaniesCheckBox.Name = "EZN_IsAllCompaniesCheckBox";
			this.EZN_IsAllCompaniesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 16, true);
			this.EZN_IsAllCompaniesCheckBox.TabIndex = 0;
			this.EZN_IsAllCompaniesCheckBox.UseVisualStyleBackColor = true;
			// 
			// EdiCommissionAgreementCustomizationTreeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.treeView);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomToolStrip);
			this.Name = "EdiCommissionAgreementCustomizationTreeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 353, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.treeView.ResumeLayout(false);
			this.treeView.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTreeViewAdv treeView;
		private Aga.Controls.Tree.TreeColumn codeTreeColumn;
		private Aga.Controls.Tree.TreeColumn descriptionTreeColumn;
		private Aga.Controls.Tree.TreeColumn autoAddTreeColumn;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox selectedCheckBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox codeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox descriptionTextBox;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox autoAddCheckBox;
		private ZArchitecture.GUI.ZToolStrip bottomToolStrip;
		private ZArchitecture.GUI.ZToolStripDropDownButton addCountriesDropDownButton;
		private ZArchitecture.GUI.ZToolStripMenuItem addCountryForNewDatabasesMenuItem;
		private ZArchitecture.GUI.ZToolStripMenuItem addCountryForExistingDatabasesMenuItem;
		private ZArchitecture.GUI.ZToolStripMenuItem addCountryForAllExistingDatabasesMenuItem;
		private System.Windows.Forms.ToolStripSeparator addCountryForExistingDatabasesStripSeparator;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZCheckBox EZN_IsAllCompaniesCheckBox;
		private ZArchitecture.GUI.ZCheckBox EZN_IsAllDatabasesCheckBox;
		private Aga.Controls.Tree.TreeColumn includeDatabaseUsageTreeColumn;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox includeDatabaseUsageCheckBox;
		private ZArchitecture.GUI.ZButton resetToDefaultButton;
	}
}
