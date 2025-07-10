using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.Builder.Generator
{
	partial class GeneratorForm : Form
	{
		Button DataUpgrateSaveXmlButton;
		IContainer components;
		Button BuildButton;
		TextBox ProgressTextBox;
		Panel BuildPanel;
		Button CheckInChangesButton;
		MenuItem FileMenuItem;
		MenuItem FileExitMenuItem;
		MenuItem ViewMenuItem;
		MenuItem SaveToClipboardMenuItem;
		MenuItem ClearMenuItem;
		MenuItem UnitTestMenuItem;
		Button Cancel_Button;
		StatusBar StatusBar;
		StatusBarPanel StatusBarPanel;
		Button UndoCheckoutButton;
		ListBox ProgressListBox;
		CheckBox GenerateBusinessObjectsCheckBox;
		TabControl GeneratorTabControl;
		TabPage BuilderTabPage;
		TabPage SystemDataUpdateTabPage;
		CheckedListBox UpgradeTasksCheckedListBox;
		Button DataUpgradeUndoCheckOutButton;
		Button DataUpgradeCheckOutButton;
		CheckBox GenerateStmEventConstantsCheckBox;
		private Button button1;
		MainMenu FormMainMenu;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Baseline")]
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneratorForm));
			this.BuildButton = new System.Windows.Forms.Button();
			this.FormMainMenu = new System.Windows.Forms.MainMenu(this.components);
			this.FileMenuItem = new System.Windows.Forms.MenuItem();
			this.FileExitMenuItem = new System.Windows.Forms.MenuItem();
			this.ViewMenuItem = new System.Windows.Forms.MenuItem();
			this.SaveToClipboardMenuItem = new System.Windows.Forms.MenuItem();
			this.ClearMenuItem = new System.Windows.Forms.MenuItem();
			this.UnitTestMenuItem = new System.Windows.Forms.MenuItem();
			this.ProgressTextBox = new System.Windows.Forms.TextBox();
			this.BuildPanel = new System.Windows.Forms.Panel();
			this.GeneratorTabControl = new System.Windows.Forms.TabControl();
			this.BuilderTabPage = new System.Windows.Forms.TabPage();
			this.GenerateStmEventConstantsCheckBox = new System.Windows.Forms.CheckBox();
			this.CheckInChangesButton = new System.Windows.Forms.Button();
			this.UndoCheckoutButton = new System.Windows.Forms.Button();
			this.GenerateBusinessObjectsCheckBox = new System.Windows.Forms.CheckBox();
			this.Cancel_Button = new System.Windows.Forms.Button();
			this.SystemDataUpdateTabPage = new System.Windows.Forms.TabPage();
			this.button1 = new System.Windows.Forms.Button();
			this.DataUpgrateSaveXmlButton = new System.Windows.Forms.Button();
			this.DataUpgradeCheckOutButton = new System.Windows.Forms.Button();
			this.DataUpgradeUndoCheckOutButton = new System.Windows.Forms.Button();
			this.UpgradeTasksCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.StatusBar = new System.Windows.Forms.StatusBar();
			this.StatusBarPanel = new System.Windows.Forms.StatusBarPanel();
			this.ProgressListBox = new System.Windows.Forms.ListBox();
			this.BuildPanel.SuspendLayout();
			this.GeneratorTabControl.SuspendLayout();
			this.BuilderTabPage.SuspendLayout();
			this.SystemDataUpdateTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// BuildButton
			// 
			this.BuildButton.BackColor = System.Drawing.Color.ForestGreen;
			this.BuildButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BuildButton.ForeColor = System.Drawing.Color.White;
			this.BuildButton.Location = new System.Drawing.Point(295, 10);
			this.BuildButton.Name = "BuildButton";
			this.BuildButton.Size = new System.Drawing.Size(104, 23);
			this.BuildButton.TabIndex = 11;
			this.BuildButton.Text = "Build";
			this.BuildButton.UseVisualStyleBackColor = false;
			this.BuildButton.Click += new System.EventHandler(this.BuildButton_Click);
			// 
			// FormMainMenu
			// 
			this.FormMainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileMenuItem,
            this.ViewMenuItem,
            this.UnitTestMenuItem});
			// 
			// FileMenuItem
			// 
			this.FileMenuItem.Index = 0;
			this.FileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileExitMenuItem});
			this.FileMenuItem.Text = "File";
			// 
			// FileExitMenuItem
			// 
			this.FileExitMenuItem.Index = 0;
			this.FileExitMenuItem.Text = "Exit";
			this.FileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
			// 
			// ViewMenuItem
			// 
			this.ViewMenuItem.Index = 1;
			this.ViewMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.SaveToClipboardMenuItem,
            this.ClearMenuItem});
			this.ViewMenuItem.Text = "View";
			// 
			// SaveToClipboardMenuItem
			// 
			this.SaveToClipboardMenuItem.Index = 0;
			this.SaveToClipboardMenuItem.Text = "Save To Clipboard";
			this.SaveToClipboardMenuItem.Click += new System.EventHandler(this.SaveToClipboardMenuItem_Click);
			// 
			// ClearMenuItem
			// 
			this.ClearMenuItem.Index = 1;
			this.ClearMenuItem.Text = "Clear";
			this.ClearMenuItem.Click += new System.EventHandler(this.ClearMenuItem_Click);
			// 
			// UnitTestMenuItem
			// 
			this.UnitTestMenuItem.Index = 2;
			this.UnitTestMenuItem.Text = "Unit Test";
			this.UnitTestMenuItem.Click += new System.EventHandler(this.UnitTestMenuItem_Click);
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Location = new System.Drawing.Point(0, 0);
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.Size = new System.Drawing.Size(888, 26);
			this.ProgressTextBox.TabIndex = 0;
			this.ProgressTextBox.Text = "textBox1";
			// 
			// BuildPanel
			// 
			this.BuildPanel.Controls.Add(this.GeneratorTabControl);
			this.BuildPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BuildPanel.Location = new System.Drawing.Point(0, 0);
			this.BuildPanel.Name = "BuildPanel";
			this.BuildPanel.Size = new System.Drawing.Size(840, 311);
			this.BuildPanel.TabIndex = 0;
			// 
			// GeneratorTabControl
			// 
			this.GeneratorTabControl.Controls.Add(this.BuilderTabPage);
			this.GeneratorTabControl.Controls.Add(this.SystemDataUpdateTabPage);
			this.GeneratorTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GeneratorTabControl.Location = new System.Drawing.Point(0, 0);
			this.GeneratorTabControl.Name = "GeneratorTabControl";
			this.GeneratorTabControl.SelectedIndex = 0;
			this.GeneratorTabControl.Size = new System.Drawing.Size(840, 311);
			this.GeneratorTabControl.TabIndex = 15;
			this.GeneratorTabControl.SelectedIndexChanged += new System.EventHandler(this.GeneratorTabControl_SelectedIndexChanged);
			// 
			// BuilderTabPage
			// 
			this.BuilderTabPage.AllowDrop = true;
			this.BuilderTabPage.Controls.Add(this.GenerateStmEventConstantsCheckBox);
			this.BuilderTabPage.Controls.Add(this.CheckInChangesButton);
			this.BuilderTabPage.Controls.Add(this.UndoCheckoutButton);
			this.BuilderTabPage.Controls.Add(this.BuildButton);
			this.BuilderTabPage.Controls.Add(this.GenerateBusinessObjectsCheckBox);
			this.BuilderTabPage.Controls.Add(this.Cancel_Button);
			this.BuilderTabPage.Location = new System.Drawing.Point(4, 22);
			this.BuilderTabPage.Name = "BuilderTabPage";
			this.BuilderTabPage.Size = new System.Drawing.Size(832, 285);
			this.BuilderTabPage.TabIndex = 0;
			this.BuilderTabPage.Text = "Builder";
			// 
			// GenerateStmEventConstantsCheckBox
			// 
			this.GenerateStmEventConstantsCheckBox.AutoSize = true;
			this.GenerateStmEventConstantsCheckBox.Location = new System.Drawing.Point(8, 37);
			this.GenerateStmEventConstantsCheckBox.Name = "GenerateStmEventConstantsCheckBox";
			this.GenerateStmEventConstantsCheckBox.Size = new System.Drawing.Size(152, 17);
			this.GenerateStmEventConstantsCheckBox.TabIndex = 17;
			this.GenerateStmEventConstantsCheckBox.Text = "Generate StmEvent Types";
			// 
			// CheckInChangesButton
			// 
			this.CheckInChangesButton.Location = new System.Drawing.Point(295, 38);
			this.CheckInChangesButton.Name = "CheckInChangesButton";
			this.CheckInChangesButton.Size = new System.Drawing.Size(104, 23);
			this.CheckInChangesButton.TabIndex = 12;
			this.CheckInChangesButton.Text = "Check In Changes";
			this.CheckInChangesButton.Click += new System.EventHandler(this.CheckInChangesButton_Click);
			// 
			// UndoCheckoutButton
			// 
			this.UndoCheckoutButton.Location = new System.Drawing.Point(411, 38);
			this.UndoCheckoutButton.Name = "UndoCheckoutButton";
			this.UndoCheckoutButton.Size = new System.Drawing.Size(103, 23);
			this.UndoCheckoutButton.TabIndex = 13;
			this.UndoCheckoutButton.Text = "Undo Checkout";
			this.UndoCheckoutButton.Click += new System.EventHandler(this.UndoCheckoutButton_Click);
			// 
			// GenerateBusinessObjectsCheckBox
			// 
			this.GenerateBusinessObjectsCheckBox.AutoSize = true;
			this.GenerateBusinessObjectsCheckBox.Location = new System.Drawing.Point(8, 14);
			this.GenerateBusinessObjectsCheckBox.Name = "GenerateBusinessObjectsCheckBox";
			this.GenerateBusinessObjectsCheckBox.Size = new System.Drawing.Size(155, 17);
			this.GenerateBusinessObjectsCheckBox.TabIndex = 7;
			this.GenerateBusinessObjectsCheckBox.Text = "Generate Business Objects";
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.BackColor = System.Drawing.Color.OrangeRed;
			this.Cancel_Button.Enabled = false;
			this.Cancel_Button.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel_Button.ForeColor = System.Drawing.Color.White;
			this.Cancel_Button.Location = new System.Drawing.Point(411, 10);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = new System.Drawing.Size(103, 23);
			this.Cancel_Button.TabIndex = 15;
			this.Cancel_Button.Text = "&Cancel";
			this.Cancel_Button.UseVisualStyleBackColor = false;
			this.Cancel_Button.Visible = false;
			this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SystemDataUpdateTabPage
			// 
			this.SystemDataUpdateTabPage.Controls.Add(this.button1);
			this.SystemDataUpdateTabPage.Controls.Add(this.DataUpgrateSaveXmlButton);
			this.SystemDataUpdateTabPage.Controls.Add(this.DataUpgradeCheckOutButton);
			this.SystemDataUpdateTabPage.Controls.Add(this.DataUpgradeUndoCheckOutButton);
			this.SystemDataUpdateTabPage.Controls.Add(this.UpgradeTasksCheckedListBox);
			this.SystemDataUpdateTabPage.Location = new System.Drawing.Point(4, 29);
			this.SystemDataUpdateTabPage.Name = "SystemDataUpdateTabPage";
			this.SystemDataUpdateTabPage.Size = new System.Drawing.Size(832, 278);
			this.SystemDataUpdateTabPage.TabIndex = 2;
			this.SystemDataUpdateTabPage.Text = "System Data Update";
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Goldenrod;
			this.button1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.Location = new System.Drawing.Point(10, 236);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(224, 32);
			this.button1.TabIndex = 6;
			this.button1.Text = "Select All";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// DataUpgrateSaveXmlButton
			// 
			this.DataUpgrateSaveXmlButton.BackColor = System.Drawing.Color.DarkGoldenrod;
			this.DataUpgrateSaveXmlButton.Enabled = false;
			this.DataUpgrateSaveXmlButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DataUpgrateSaveXmlButton.ForeColor = System.Drawing.Color.White;
			this.DataUpgrateSaveXmlButton.Location = new System.Drawing.Point(8, 46);
			this.DataUpgrateSaveXmlButton.Name = "DataUpgrateSaveXmlButton";
			this.DataUpgrateSaveXmlButton.Size = new System.Drawing.Size(224, 32);
			this.DataUpgrateSaveXmlButton.TabIndex = 5;
			this.DataUpgrateSaveXmlButton.Text = "Save DB data to XML files ";
			this.DataUpgrateSaveXmlButton.UseVisualStyleBackColor = false;
			this.DataUpgrateSaveXmlButton.Click += new System.EventHandler(this.DataUpgrateSaveXmlButton_Click);
			// 
			// DataUpgradeCheckOutButton
			// 
			this.DataUpgradeCheckOutButton.BackColor = System.Drawing.Color.DarkGoldenrod;
			this.DataUpgradeCheckOutButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DataUpgradeCheckOutButton.ForeColor = System.Drawing.Color.White;
			this.DataUpgradeCheckOutButton.Location = new System.Drawing.Point(8, 8);
			this.DataUpgradeCheckOutButton.Name = "DataUpgradeCheckOutButton";
			this.DataUpgradeCheckOutButton.Size = new System.Drawing.Size(224, 32);
			this.DataUpgradeCheckOutButton.TabIndex = 4;
			this.DataUpgradeCheckOutButton.Text = "CHECK-OUT XML / Apply data to DB";
			this.DataUpgradeCheckOutButton.UseVisualStyleBackColor = false;
			this.DataUpgradeCheckOutButton.Click += new System.EventHandler(this.DataUpgradeCheckOutButton_Click);
			// 
			// DataUpgradeUndoCheckOutButton
			// 
			this.DataUpgradeUndoCheckOutButton.BackColor = System.Drawing.Color.Goldenrod;
			this.DataUpgradeUndoCheckOutButton.Enabled = false;
			this.DataUpgradeUndoCheckOutButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DataUpgradeUndoCheckOutButton.ForeColor = System.Drawing.Color.White;
			this.DataUpgradeUndoCheckOutButton.Location = new System.Drawing.Point(8, 84);
			this.DataUpgradeUndoCheckOutButton.Name = "DataUpgradeUndoCheckOutButton";
			this.DataUpgradeUndoCheckOutButton.Size = new System.Drawing.Size(224, 32);
			this.DataUpgradeUndoCheckOutButton.TabIndex = 1;
			this.DataUpgradeUndoCheckOutButton.Text = "UNDO CHECK-OUT XML Files";
			this.DataUpgradeUndoCheckOutButton.UseVisualStyleBackColor = false;
			this.DataUpgradeUndoCheckOutButton.Click += new System.EventHandler(this.DataUpgradeUndoCheckOutButton_Click);
			// 
			// UpgradeTasksCheckedListBox
			// 
			this.UpgradeTasksCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.UpgradeTasksCheckedListBox.Location = new System.Drawing.Point(240, 8);
			this.UpgradeTasksCheckedListBox.Name = "UpgradeTasksCheckedListBox";
			this.UpgradeTasksCheckedListBox.Size = new System.Drawing.Size(582, 260);
			this.UpgradeTasksCheckedListBox.TabIndex = 0;
			// 
			// StatusBar
			// 
			this.StatusBar.Location = new System.Drawing.Point(0, 507);
			this.StatusBar.Name = "StatusBar";
			this.StatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.StatusBarPanel});
			this.StatusBar.ShowPanels = true;
			this.StatusBar.Size = new System.Drawing.Size(840, 22);
			this.StatusBar.TabIndex = 27;
			// 
			// StatusBarPanel
			// 
			this.StatusBarPanel.Name = "StatusBarPanel";
			this.StatusBarPanel.Width = 3000;
			// 
			// ProgressListBox
			// 
			this.ProgressListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressListBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProgressListBox.HorizontalScrollbar = true;
			this.ProgressListBox.Location = new System.Drawing.Point(0, 317);
			this.ProgressListBox.Name = "ProgressListBox";
			this.ProgressListBox.ScrollAlwaysVisible = true;
			this.ProgressListBox.Size = new System.Drawing.Size(840, 186);
			this.ProgressListBox.TabIndex = 9;
			// 
			// GeneratorForm
			// 
			this.ClientSize = new System.Drawing.Size(840, 529);
			this.Controls.Add(this.StatusBar);
			this.Controls.Add(this.ProgressListBox);
			this.Controls.Add(this.BuildPanel);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.FormMainMenu;
			this.MinimumSize = new System.Drawing.Size(750, 350);
			this.Name = "GeneratorForm";
			this.Text = "CargoWise Builder";
			this.Load += new System.EventHandler(this.GeneratorForm_Load);
			this.BuildPanel.ResumeLayout(false);
			this.GeneratorTabControl.ResumeLayout(false);
			this.BuilderTabPage.ResumeLayout(false);
			this.BuilderTabPage.PerformLayout();
			this.SystemDataUpdateTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.StatusBarPanel)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
