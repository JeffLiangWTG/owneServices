namespace Enterprise.StlAnalysis.Load
{
	partial class EditFeatureControl
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
			this.txbRulesTextBox = new System.Windows.Forms.TextBox();
			this.txbSummaryQueryTextBox = new System.Windows.Forms.TextBox();
			this.ltvFeatureListView = new System.Windows.Forms.ListView();
			this.ModuleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.StlBasis = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.StlEntity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UnitWeight = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lblRulesLable = new System.Windows.Forms.Label();
			this.lblSummaryQueryLabel = new System.Windows.Forms.Label();
			this.txbStatusTextBox = new System.Windows.Forms.TextBox();
			this.btnSaveChangesButton = new System.Windows.Forms.Button();
			this.ckbIsSystemLevelCheckBox = new System.Windows.Forms.CheckBox();
			this.ckbIsCompanyLevelCheckBox = new System.Windows.Forms.CheckBox();
			this.lblDataSourceLabel = new System.Windows.Forms.Label();
			this.cbxDataSourceComboBox = new System.Windows.Forms.ComboBox();
			this.lblDataGranularityLabel = new System.Windows.Forms.Label();
			this.lbDetailSqlExpressionLabel = new System.Windows.Forms.Label();
			this.txbDetailSqlExpressionTextBox = new System.Windows.Forms.TextBox();
			this.txbDetailCountExpressionTextBox = new System.Windows.Forms.TextBox();
			this.lblDetailCountExpressionLabel = new System.Windows.Forms.Label();
			this.FeatureCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// txbRulesTextBox
			// 
			this.txbRulesTextBox.AcceptsReturn = true;
			this.txbRulesTextBox.AcceptsTab = true;
			this.txbRulesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txbRulesTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbRulesTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbRulesTextBox.Location = new System.Drawing.Point(343, 319);
			this.txbRulesTextBox.Multiline = true;
			this.txbRulesTextBox.Name = "txbRulesTextBox";
			this.txbRulesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbRulesTextBox.Size = new System.Drawing.Size(205, 59);
			this.txbRulesTextBox.TabIndex = 1;
			// 
			// txbSummaryQueryTextBox
			// 
			this.txbSummaryQueryTextBox.AcceptsReturn = true;
			this.txbSummaryQueryTextBox.AcceptsTab = true;
			this.txbSummaryQueryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbSummaryQueryTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbSummaryQueryTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbSummaryQueryTextBox.Location = new System.Drawing.Point(3, 319);
			this.txbSummaryQueryTextBox.Multiline = true;
			this.txbSummaryQueryTextBox.Name = "txbSummaryQueryTextBox";
			this.txbSummaryQueryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbSummaryQueryTextBox.Size = new System.Drawing.Size(334, 269);
			this.txbSummaryQueryTextBox.TabIndex = 2;
			// 
			// ltvFeatureListView
			// 
			this.ltvFeatureListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ltvFeatureListView.BackColor = System.Drawing.Color.Snow;
			this.ltvFeatureListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FeatureCode,
            this.ModuleName,
            this.StlBasis,
            this.StlEntity,
            this.UnitWeight});
			this.ltvFeatureListView.ForeColor = System.Drawing.Color.Black;
			this.ltvFeatureListView.FullRowSelect = true;
			this.ltvFeatureListView.GridLines = true;
			this.ltvFeatureListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ltvFeatureListView.HideSelection = false;
			this.ltvFeatureListView.Location = new System.Drawing.Point(3, 3);
			this.ltvFeatureListView.MultiSelect = false;
			this.ltvFeatureListView.Name = "ltvFeatureListView";
			this.ltvFeatureListView.ShowItemToolTips = true;
			this.ltvFeatureListView.Size = new System.Drawing.Size(821, 297);
			this.ltvFeatureListView.TabIndex = 10;
			this.ltvFeatureListView.UseCompatibleStateImageBehavior = false;
			this.ltvFeatureListView.View = System.Windows.Forms.View.Details;
			this.ltvFeatureListView.SelectedIndexChanged += new System.EventHandler(this.ltvFeatureListView_SelectedIndexChanged);
			// 
			// ModuleName
			// 
			this.ModuleName.Text = "Module";
			this.ModuleName.Width = 400;
			// 
			// StlBasis
			// 
			this.StlBasis.Text = "STL Basis";
			this.StlBasis.Width = 150;
			// 
			// StlEntity
			// 
			this.StlEntity.Text = "STL Entity";
			this.StlEntity.Width = 175;
			// 
			// UnitWeight
			// 
			this.UnitWeight.Text = "Unit Factor";
			this.UnitWeight.Width = 70;
			// 
			// lblRulesLable
			// 
			this.lblRulesLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRulesLable.AutoSize = true;
			this.lblRulesLable.ForeColor = System.Drawing.Color.Black;
			this.lblRulesLable.Location = new System.Drawing.Point(340, 303);
			this.lblRulesLable.Name = "lblRulesLable";
			this.lblRulesLable.Size = new System.Drawing.Size(37, 13);
			this.lblRulesLable.TabIndex = 11;
			this.lblRulesLable.Text = "Rules:";
			// 
			// lblSummaryQueryLabel
			// 
			this.lblSummaryQueryLabel.AutoSize = true;
			this.lblSummaryQueryLabel.ForeColor = System.Drawing.Color.Black;
			this.lblSummaryQueryLabel.Location = new System.Drawing.Point(3, 303);
			this.lblSummaryQueryLabel.Name = "lblSummaryQueryLabel";
			this.lblSummaryQueryLabel.Size = new System.Drawing.Size(84, 13);
			this.lblSummaryQueryLabel.TabIndex = 12;
			this.lblSummaryQueryLabel.Text = "Summary Query:";
			// 
			// txbStatusTextBox
			// 
			this.txbStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbStatusTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbStatusTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.txbStatusTextBox.Location = new System.Drawing.Point(343, 594);
			this.txbStatusTextBox.Multiline = true;
			this.txbStatusTextBox.Name = "txbStatusTextBox";
			this.txbStatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbStatusTextBox.Size = new System.Drawing.Size(481, 41);
			this.txbStatusTextBox.TabIndex = 13;
			this.txbStatusTextBox.Visible = false;
			// 
			// btnSaveChangesButton
			// 
			this.btnSaveChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSaveChangesButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnSaveChangesButton.FlatAppearance.BorderSize = 0;
			this.btnSaveChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSaveChangesButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSaveChangesButton.ForeColor = System.Drawing.Color.Black;
			this.btnSaveChangesButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.basic1_038;
			this.btnSaveChangesButton.Location = new System.Drawing.Point(3, 594);
			this.btnSaveChangesButton.Name = "btnSaveChangesButton";
			this.btnSaveChangesButton.Size = new System.Drawing.Size(109, 41);
			this.btnSaveChangesButton.TabIndex = 5;
			this.btnSaveChangesButton.Text = "Save";
			this.btnSaveChangesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnSaveChangesButton.UseVisualStyleBackColor = false;
			this.btnSaveChangesButton.Click += new System.EventHandler(this.btnSaveChangesButton_Click);
			// 
			// ckbIsSystemLevelCheckBox
			// 
			this.ckbIsSystemLevelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ckbIsSystemLevelCheckBox.AutoSize = true;
			this.ckbIsSystemLevelCheckBox.Location = new System.Drawing.Point(630, 322);
			this.ckbIsSystemLevelCheckBox.Name = "ckbIsSystemLevelCheckBox";
			this.ckbIsSystemLevelCheckBox.Size = new System.Drawing.Size(89, 17);
			this.ckbIsSystemLevelCheckBox.TabIndex = 14;
			this.ckbIsSystemLevelCheckBox.Text = "System Level";
			this.ckbIsSystemLevelCheckBox.UseVisualStyleBackColor = true;
			// 
			// ckbIsCompanyLevelCheckBox
			// 
			this.ckbIsCompanyLevelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ckbIsCompanyLevelCheckBox.AutoSize = true;
			this.ckbIsCompanyLevelCheckBox.Location = new System.Drawing.Point(725, 322);
			this.ckbIsCompanyLevelCheckBox.Name = "ckbIsCompanyLevelCheckBox";
			this.ckbIsCompanyLevelCheckBox.Size = new System.Drawing.Size(99, 17);
			this.ckbIsCompanyLevelCheckBox.TabIndex = 15;
			this.ckbIsCompanyLevelCheckBox.Text = "Company Level";
			this.ckbIsCompanyLevelCheckBox.UseVisualStyleBackColor = true;
			// 
			// lblDataSourceLabel
			// 
			this.lblDataSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDataSourceLabel.AutoSize = true;
			this.lblDataSourceLabel.ForeColor = System.Drawing.Color.Black;
			this.lblDataSourceLabel.Location = new System.Drawing.Point(554, 357);
			this.lblDataSourceLabel.Name = "lblDataSourceLabel";
			this.lblDataSourceLabel.Size = new System.Drawing.Size(70, 13);
			this.lblDataSourceLabel.TabIndex = 17;
			this.lblDataSourceLabel.Text = "Data Source:";
			// 
			// cbxDataSourceComboBox
			// 
			this.cbxDataSourceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbxDataSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxDataSourceComboBox.FormattingEnabled = true;
			this.cbxDataSourceComboBox.Items.AddRange(new object[] {
            "CLIENT",
            "EDIPROD",
            "EDIFAX",
            "DENIEDPARTY",
            "EHUB"});
			this.cbxDataSourceComboBox.Location = new System.Drawing.Point(630, 354);
			this.cbxDataSourceComboBox.Name = "cbxDataSourceComboBox";
			this.cbxDataSourceComboBox.Size = new System.Drawing.Size(194, 21);
			this.cbxDataSourceComboBox.TabIndex = 18;
			// 
			// lblDataGranularityLabel
			// 
			this.lblDataGranularityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDataGranularityLabel.AutoSize = true;
			this.lblDataGranularityLabel.ForeColor = System.Drawing.Color.Black;
			this.lblDataGranularityLabel.Location = new System.Drawing.Point(554, 323);
			this.lblDataGranularityLabel.Name = "lblDataGranularityLabel";
			this.lblDataGranularityLabel.Size = new System.Drawing.Size(60, 13);
			this.lblDataGranularityLabel.TabIndex = 19;
			this.lblDataGranularityLabel.Text = "Granularity:";
			// 
			// lbDetailSqlExpressionLabel
			// 
			this.lbDetailSqlExpressionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lbDetailSqlExpressionLabel.AutoSize = true;
			this.lbDetailSqlExpressionLabel.ForeColor = System.Drawing.Color.Black;
			this.lbDetailSqlExpressionLabel.Location = new System.Drawing.Point(340, 381);
			this.lbDetailSqlExpressionLabel.Name = "lbDetailSqlExpressionLabel";
			this.lbDetailSqlExpressionLabel.Size = new System.Drawing.Size(144, 13);
			this.lbDetailSqlExpressionLabel.TabIndex = 21;
			this.lbDetailSqlExpressionLabel.Text = "Detail Reference Expression:";
			// 
			// txbDetailSqlExpressionTextBox
			// 
			this.txbDetailSqlExpressionTextBox.AcceptsReturn = true;
			this.txbDetailSqlExpressionTextBox.AcceptsTab = true;
			this.txbDetailSqlExpressionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbDetailSqlExpressionTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbDetailSqlExpressionTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbDetailSqlExpressionTextBox.Location = new System.Drawing.Point(343, 397);
			this.txbDetailSqlExpressionTextBox.Multiline = true;
			this.txbDetailSqlExpressionTextBox.Name = "txbDetailSqlExpressionTextBox";
			this.txbDetailSqlExpressionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbDetailSqlExpressionTextBox.Size = new System.Drawing.Size(481, 99);
			this.txbDetailSqlExpressionTextBox.TabIndex = 22;
			// 
			// txbDetailCountExpressionTextBox
			// 
			this.txbDetailCountExpressionTextBox.AcceptsReturn = true;
			this.txbDetailCountExpressionTextBox.AcceptsTab = true;
			this.txbDetailCountExpressionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.txbDetailCountExpressionTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbDetailCountExpressionTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbDetailCountExpressionTextBox.Location = new System.Drawing.Point(343, 515);
			this.txbDetailCountExpressionTextBox.Multiline = true;
			this.txbDetailCountExpressionTextBox.Name = "txbDetailCountExpressionTextBox";
			this.txbDetailCountExpressionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbDetailCountExpressionTextBox.Size = new System.Drawing.Size(481, 73);
			this.txbDetailCountExpressionTextBox.TabIndex = 23;
			// 
			// lblDetailCountExpressionLabel
			// 
			this.lblDetailCountExpressionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDetailCountExpressionLabel.AutoSize = true;
			this.lblDetailCountExpressionLabel.ForeColor = System.Drawing.Color.Black;
			this.lblDetailCountExpressionLabel.Location = new System.Drawing.Point(340, 499);
			this.lblDetailCountExpressionLabel.Name = "lblDetailCountExpressionLabel";
			this.lblDetailCountExpressionLabel.Size = new System.Drawing.Size(122, 13);
			this.lblDetailCountExpressionLabel.TabIndex = 24;
			this.lblDetailCountExpressionLabel.Text = "Detail Count Expression:";
			// 
			// FeatureCode
			// 
			this.FeatureCode.Text = "Code";
			this.FeatureCode.Width = 40;
			// 
			// EditFeatureControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.Controls.Add(this.lblDetailCountExpressionLabel);
			this.Controls.Add(this.txbDetailCountExpressionTextBox);
			this.Controls.Add(this.txbDetailSqlExpressionTextBox);
			this.Controls.Add(this.lbDetailSqlExpressionLabel);
			this.Controls.Add(this.lblDataGranularityLabel);
			this.Controls.Add(this.cbxDataSourceComboBox);
			this.Controls.Add(this.lblDataSourceLabel);
			this.Controls.Add(this.ckbIsCompanyLevelCheckBox);
			this.Controls.Add(this.ckbIsSystemLevelCheckBox);
			this.Controls.Add(this.lblSummaryQueryLabel);
			this.Controls.Add(this.lblRulesLable);
			this.Controls.Add(this.btnSaveChangesButton);
			this.Controls.Add(this.txbRulesTextBox);
			this.Controls.Add(this.ltvFeatureListView);
			this.Controls.Add(this.txbStatusTextBox);
			this.Controls.Add(this.txbSummaryQueryTextBox);
			this.Name = "EditFeatureControl";
			this.Size = new System.Drawing.Size(827, 638);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txbRulesTextBox;
		private System.Windows.Forms.TextBox txbSummaryQueryTextBox;
		private System.Windows.Forms.Button btnSaveChangesButton;
		private System.Windows.Forms.ListView ltvFeatureListView;
		private System.Windows.Forms.ColumnHeader ModuleName;
		private System.Windows.Forms.ColumnHeader StlBasis;
		private System.Windows.Forms.ColumnHeader StlEntity;
		private System.Windows.Forms.ColumnHeader UnitWeight;
		private System.Windows.Forms.Label lblRulesLable;
		private System.Windows.Forms.Label lblSummaryQueryLabel;
		private System.Windows.Forms.TextBox txbStatusTextBox;
		private System.Windows.Forms.CheckBox ckbIsSystemLevelCheckBox;
		private System.Windows.Forms.CheckBox ckbIsCompanyLevelCheckBox;
		private System.Windows.Forms.Label lblDataSourceLabel;
		private System.Windows.Forms.ComboBox cbxDataSourceComboBox;
		private System.Windows.Forms.Label lblDataGranularityLabel;
		private System.Windows.Forms.Label lbDetailSqlExpressionLabel;
		private System.Windows.Forms.TextBox txbDetailSqlExpressionTextBox;
		private System.Windows.Forms.TextBox txbDetailCountExpressionTextBox;
		private System.Windows.Forms.Label lblDetailCountExpressionLabel;
		private System.Windows.Forms.ColumnHeader FeatureCode;
	}
}
