namespace Enterprise.Loader
{
	partial class InstanceManagerForm
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.labelInfo = new System.Windows.Forms.Label();
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.serverNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.databaseNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.iCargoWiseOneInstanceEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.itemGroupBox = new System.Windows.Forms.GroupBox();
			this.buttonDeleteItem = new System.Windows.Forms.Button();
			this.buttonSaveItem = new System.Windows.Forms.Button();
			this.textBoxDatabaseName = new System.Windows.Forms.TextBox();
			this.labelDatabaseName = new System.Windows.Forms.Label();
			this.textBoxServerName = new System.Windows.Forms.TextBox();
			this.labelServerName = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.labelName = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iCargoWiseOneInstanceEntryBindingSource)).BeginInit();
			this.itemGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelInfo
			// 
			this.labelInfo.AutoSize = true;
			this.labelInfo.Location = new System.Drawing.Point(12, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(394, 13);
			this.labelInfo.TabIndex = 0;
			this.labelInfo.Text = "Register Product Instances on the current domain for automatic discovery.";
			// 
			// dataGridView
			// 
			this.dataGridView.AllowUserToDeleteRows = false;
			this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView.AutoGenerateColumns = false;
			this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.nameDataGridViewTextBoxColumn,
			this.serverNameDataGridViewTextBoxColumn,
			this.databaseNameDataGridViewTextBoxColumn});
			this.dataGridView.DataSource = this.iCargoWiseOneInstanceEntryBindingSource;
			this.dataGridView.Location = new System.Drawing.Point(12, 26);
			this.dataGridView.MultiSelect = false;
			this.dataGridView.Name = "dataGridView";
			this.dataGridView.ReadOnly = true;
			this.dataGridView.Size = new System.Drawing.Size(664, 202);
			this.dataGridView.TabIndex = 1;
			// 
			// nameDataGridViewTextBoxColumn
			// 
			this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
			this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
			this.nameDataGridViewTextBoxColumn.MinimumWidth = 100;
			this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
			this.nameDataGridViewTextBoxColumn.ReadOnly = true;
			this.nameDataGridViewTextBoxColumn.Width = 200;
			// 
			// serverNameDataGridViewTextBoxColumn
			// 
			this.serverNameDataGridViewTextBoxColumn.DataPropertyName = "ServerName";
			this.serverNameDataGridViewTextBoxColumn.HeaderText = "Server Name";
			this.serverNameDataGridViewTextBoxColumn.MinimumWidth = 100;
			this.serverNameDataGridViewTextBoxColumn.Name = "serverNameDataGridViewTextBoxColumn";
			this.serverNameDataGridViewTextBoxColumn.ReadOnly = true;
			this.serverNameDataGridViewTextBoxColumn.Width = 200;
			// 
			// databaseNameDataGridViewTextBoxColumn
			// 
			this.databaseNameDataGridViewTextBoxColumn.DataPropertyName = "DatabaseName";
			this.databaseNameDataGridViewTextBoxColumn.HeaderText = "Database Name";
			this.databaseNameDataGridViewTextBoxColumn.MinimumWidth = 100;
			this.databaseNameDataGridViewTextBoxColumn.Name = "databaseNameDataGridViewTextBoxColumn";
			this.databaseNameDataGridViewTextBoxColumn.ReadOnly = true;
			this.databaseNameDataGridViewTextBoxColumn.Width = 200;
			// 
			// iCargoWiseOneInstanceEntryBindingSource
			// 
			this.iCargoWiseOneInstanceEntryBindingSource.DataSource = typeof(InstanceBindingEntry);
			// 
			// itemGroupBox
			// 
			this.itemGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.itemGroupBox.Controls.Add(this.buttonDeleteItem);
			this.itemGroupBox.Controls.Add(this.buttonSaveItem);
			this.itemGroupBox.Controls.Add(this.textBoxDatabaseName);
			this.itemGroupBox.Controls.Add(this.labelDatabaseName);
			this.itemGroupBox.Controls.Add(this.textBoxServerName);
			this.itemGroupBox.Controls.Add(this.labelServerName);
			this.itemGroupBox.Controls.Add(this.textBoxName);
			this.itemGroupBox.Controls.Add(this.labelName);
			this.itemGroupBox.Location = new System.Drawing.Point(12, 234);
			this.itemGroupBox.Name = "itemGroupBox";
			this.itemGroupBox.Size = new System.Drawing.Size(665, 135);
			this.itemGroupBox.TabIndex = 2;
			this.itemGroupBox.TabStop = false;
			this.itemGroupBox.Text = "Edit Item";
			// 
			// buttonDeleteItem
			// 
			this.buttonDeleteItem.Location = new System.Drawing.Point(503, 99);
			this.buttonDeleteItem.Name = "buttonDeleteItem";
			this.buttonDeleteItem.Size = new System.Drawing.Size(75, 23);
			this.buttonDeleteItem.TabIndex = 7;
			this.buttonDeleteItem.Text = "Delete Item";
			this.buttonDeleteItem.UseVisualStyleBackColor = true;
			this.buttonDeleteItem.Click += new System.EventHandler(this.buttonDeleteItem_Click);
			// 
			// buttonSaveItem
			// 
			this.buttonSaveItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveItem.Location = new System.Drawing.Point(584, 99);
			this.buttonSaveItem.Name = "buttonSaveItem";
			this.buttonSaveItem.Size = new System.Drawing.Size(75, 23);
			this.buttonSaveItem.TabIndex = 6;
			this.buttonSaveItem.Text = "Save Item";
			this.buttonSaveItem.UseVisualStyleBackColor = true;
			this.buttonSaveItem.Click += new System.EventHandler(this.buttonSaveItem_Click);
			// 
			// textBoxDatabaseName
			// 
			this.textBoxDatabaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDatabaseName.Location = new System.Drawing.Point(100, 73);
			this.textBoxDatabaseName.Name = "textBoxDatabaseName";
			this.textBoxDatabaseName.Size = new System.Drawing.Size(559, 20);
			this.textBoxDatabaseName.TabIndex = 5;
			// 
			// labelDatabaseName
			// 
			this.labelDatabaseName.AutoSize = true;
			this.labelDatabaseName.Location = new System.Drawing.Point(6, 76);
			this.labelDatabaseName.Name = "labelDatabaseName";
			this.labelDatabaseName.Size = new System.Drawing.Size(87, 13);
			this.labelDatabaseName.TabIndex = 4;
			this.labelDatabaseName.Text = "Database Name:";
			// 
			// textBoxServerName
			// 
			this.textBoxServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxServerName.Location = new System.Drawing.Point(100, 46);
			this.textBoxServerName.Name = "textBoxServerName";
			this.textBoxServerName.Size = new System.Drawing.Size(559, 20);
			this.textBoxServerName.TabIndex = 3;
			// 
			// labelServerName
			// 
			this.labelServerName.AutoSize = true;
			this.labelServerName.Location = new System.Drawing.Point(21, 49);
			this.labelServerName.Name = "labelServerName";
			this.labelServerName.Size = new System.Drawing.Size(72, 13);
			this.labelServerName.TabIndex = 2;
			this.labelServerName.Text = "Server Name:";
			// 
			// textBoxName
			// 
			this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxName.Location = new System.Drawing.Point(100, 20);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(559, 20);
			this.textBoxName.TabIndex = 1;
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(55, 23);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(38, 13);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "Name:";
			// 
			// InstanceManagerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(689, 381);
			this.Controls.Add(this.itemGroupBox);
			this.Controls.Add(this.dataGridView);
			this.Controls.Add(this.labelInfo);
			this.MinimumSize = new System.Drawing.Size(688, 388);
			this.Name = "InstanceManagerForm";
			this.Text = "Manage Instances";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iCargoWiseOneInstanceEntryBindingSource)).EndInit();
			this.itemGroupBox.ResumeLayout(false);
			this.itemGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.DataGridView dataGridView;
		private System.Windows.Forms.BindingSource iCargoWiseOneInstanceEntryBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn serverNameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn databaseNameDataGridViewTextBoxColumn;
		private System.Windows.Forms.GroupBox itemGroupBox;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.TextBox textBoxServerName;
		private System.Windows.Forms.Label labelServerName;
		private System.Windows.Forms.TextBox textBoxDatabaseName;
		private System.Windows.Forms.Label labelDatabaseName;
		private System.Windows.Forms.Button buttonSaveItem;
		private System.Windows.Forms.Button buttonDeleteItem;
	}
}