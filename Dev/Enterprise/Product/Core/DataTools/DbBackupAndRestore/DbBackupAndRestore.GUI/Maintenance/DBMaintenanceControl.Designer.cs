using System.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class DBMaintenanceControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();

			this.MaintenancePanel = new CargoWise.Windows.UI.KPanel();
			this.IncludeReferenceFilesDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.IncludeOperationalDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.MaintenanceDatabaseComboBox = new CargoWise.Windows.UI.KComboBox();
			this.RefreshDatabasesButton = new CargoWise.Windows.UI.KButton();
			this.DbDropButton = new CargoWise.Windows.UI.KButton();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.DbMaintenanceServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.label3 = new CargoWise.Windows.UI.KLabel();
			this.IncludeBiDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.MaintenancePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MaintenancePanel
			// 
			this.MaintenancePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MaintenancePanel.Controls.Add(this.IncludeBiDatabasesCheckBox);
			this.MaintenancePanel.Controls.Add(this.IncludeReferenceFilesDatabasesCheckBox);
			this.MaintenancePanel.Controls.Add(this.IncludeOperationalDatabasesCheckBox);
			this.MaintenancePanel.Controls.Add(this.MaintenanceDatabaseComboBox);
			this.MaintenancePanel.Controls.Add(this.RefreshDatabasesButton);
			this.MaintenancePanel.Controls.Add(this.DbDropButton);
			this.MaintenancePanel.Controls.Add(this.label2);
			this.MaintenancePanel.Controls.Add(this.DbMaintenanceServerTextBox);
			this.MaintenancePanel.Controls.Add(this.label3);
			this.MaintenancePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.MaintenancePanel.Name = "MaintenancePanel";
			this.MaintenancePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 246, true);
			this.MaintenancePanel.TabIndex = 14;
			// 
			// IncludeReferenceFilesDatabasesCheckBox
			// 
			this.IncludeReferenceFilesDatabasesCheckBox.AutoSize = true;
			this.IncludeReferenceFilesDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 130, true);
			this.IncludeReferenceFilesDatabasesCheckBox.Name = "IncludeReferenceFilesDatabasesCheckBox";
			this.IncludeReferenceFilesDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 16, true);
			this.IncludeReferenceFilesDatabasesCheckBox.TabIndex = 6;
			this.IncludeReferenceFilesDatabasesCheckBox.Text = "Include Related Reference Databases";
			this.IncludeReferenceFilesDatabasesCheckBox.UseVisualStyleBackColor = true;
			this.IncludeReferenceFilesDatabasesCheckBox.CheckedChanged += new System.EventHandler(this.IncludeReferenceFilesDatabasesCheckBox_CheckedChanged);
			// 
			// IncludeOperationalDatabasesCheckBox
			// 
			this.IncludeOperationalDatabasesCheckBox.AutoSize = true;
			this.IncludeOperationalDatabasesCheckBox.Checked = true;
			this.IncludeOperationalDatabasesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.IncludeOperationalDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 90, true);
			this.IncludeOperationalDatabasesCheckBox.Name = "IncludeOperationalDatabasesCheckBox";
			this.IncludeOperationalDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 16, true);
			this.IncludeOperationalDatabasesCheckBox.TabIndex = 5;
			this.IncludeOperationalDatabasesCheckBox.Text = "Include Related Operational Databases";
			this.IncludeOperationalDatabasesCheckBox.UseVisualStyleBackColor = true;
			this.IncludeOperationalDatabasesCheckBox.CheckedChanged += new System.EventHandler(this.IncludeOperationalDatabasesCheckBox_CheckedChanged);
			// 
			// MaintenanceDatabaseComboBox
			// 
			this.MaintenanceDatabaseComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MaintenanceDatabaseComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.MaintenanceDatabaseComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.MaintenanceDatabaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.MaintenanceDatabaseComboBox.Enabled = false;
			this.MaintenanceDatabaseComboBox.FormattingEnabled = true;
			this.MaintenanceDatabaseComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 37, true);
			this.MaintenanceDatabaseComboBox.Name = "MaintenanceDatabaseComboBox";
			this.MaintenanceDatabaseComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 19, true);
			this.MaintenanceDatabaseComboBox.TabIndex = 2;
			this.MaintenanceDatabaseComboBox.SelectedIndexChanged += new System.EventHandler(this.MaintenanceDatabaseComboBox_SelectedIndexChanged);
			// 
			// RefreshDatabasesButton
			// 
			this.RefreshDatabasesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshDatabasesButton.Enabled = false;
			this.RefreshDatabasesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 11, true);
			this.RefreshDatabasesButton.Name = "RefreshDatabasesButton";
			this.RefreshDatabasesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.RefreshDatabasesButton.TabIndex = 1;
			this.RefreshDatabasesButton.Text = "Refresh DBs";
			this.RefreshDatabasesButton.Click += new System.EventHandler(this.RefreshDatabasesButton_Click);
			// 
			// DbDropButton
			// 
			this.DbDropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DbDropButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.DbDropButton.Enabled = false;
			this.DbDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 167, true);
			this.DbDropButton.Name = "DbDropButton";
			this.DbDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 22, true);
			this.DbDropButton.TabIndex = 7;
			this.DbDropButton.Text = "Remove";
			this.DbDropButton.Click += new System.EventHandler(this.DbDropButton_Click);
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 37, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.label2.TabIndex = 3;
			this.label2.Text = "Database:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DbMaintenanceServerTextBox
			// 
			this.DbMaintenanceServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbMaintenanceServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 12, true);
			this.DbMaintenanceServerTextBox.Name = "DbMaintenanceServerTextBox";
			this.DbMaintenanceServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 17, true);
			this.DbMaintenanceServerTextBox.TabIndex = 0;
			this.DbMaintenanceServerTextBox.TextChanged += new System.EventHandler(this.DbMaintenanceServerTextBox_TextChanged);
			this.DbMaintenanceServerTextBox.Leave += new System.EventHandler(this.DbMaintenanceServerTextBox_Leave);
			// 
			// label3
			// 
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 12, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.label3.TabIndex = 0;
			this.label3.Text = "Server:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// IncludeBiDatabasesCheckBox
			// 
			this.IncludeBiDatabasesCheckBox.AutoSize = true;
			this.IncludeBiDatabasesCheckBox.Checked = true;
			this.IncludeBiDatabasesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.IncludeBiDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 110, true);
			this.IncludeBiDatabasesCheckBox.Name = "IncludeBiDatabasesCheckBox";
			this.IncludeBiDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 16, true);
			this.IncludeBiDatabasesCheckBox.TabIndex = 8;
			this.IncludeBiDatabasesCheckBox.Text = "Include Business Intelligence Databases";
			this.IncludeBiDatabasesCheckBox.UseVisualStyleBackColor = true;
			// 
			// DBMaintenanceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MaintenancePanel);
			this.Name = "DBMaintenanceControl";
			this.Controls.SetChildIndex(this.MaintenancePanel, 0);
			this.MaintenancePanel.ResumeLayout(false);
			this.MaintenancePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KPanel MaintenancePanel;
		private KCheckBox IncludeReferenceFilesDatabasesCheckBox;
		private KCheckBox IncludeOperationalDatabasesCheckBox;
		private KComboBox MaintenanceDatabaseComboBox;
		private KButton RefreshDatabasesButton;
		private KButton DbDropButton;
		private KLabel label2;
		private KTextBox DbMaintenanceServerTextBox;
		private KLabel label3;
		private KCheckBox IncludeBiDatabasesCheckBox;
	}
}
