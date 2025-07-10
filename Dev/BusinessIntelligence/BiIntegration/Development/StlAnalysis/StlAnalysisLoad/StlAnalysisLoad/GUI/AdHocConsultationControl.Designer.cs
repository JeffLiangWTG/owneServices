namespace Enterprise.StlAnalysis.Load
{
	partial class AdHocConsultationControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdHocConsultationControl));
			this.cbxHostedClientComboBox = new System.Windows.Forms.ComboBox();
			this.txbYearTextBox = new System.Windows.Forms.TextBox();
			this.txbMonthTextBox = new System.Windows.Forms.TextBox();
			this.ltvModuleTransactionsListView = new System.Windows.Forms.ListView();
			this.ModuleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.StlBasis = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.StlEntity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UnitCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnCollectDataButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cbxHostedClientComboBox
			// 
			this.cbxHostedClientComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbxHostedClientComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.cbxHostedClientComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.cbxHostedClientComboBox.BackColor = System.Drawing.Color.Snow;
			this.cbxHostedClientComboBox.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbxHostedClientComboBox.FormattingEnabled = true;
			this.cbxHostedClientComboBox.Location = new System.Drawing.Point(3, 3);
			this.cbxHostedClientComboBox.Name = "cbxHostedClientComboBox";
			this.cbxHostedClientComboBox.Size = new System.Drawing.Size(594, 31);
			this.cbxHostedClientComboBox.TabIndex = 10;
			this.cbxHostedClientComboBox.Tag = "(client)";
			this.cbxHostedClientComboBox.Text = "(client)";
			this.cbxHostedClientComboBox.SelectedIndexChanged += new System.EventHandler(this.cbxHostedClientComboBox_SelectedIndexChanged);
			// 
			// txbYearTextBox
			// 
			this.txbYearTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txbYearTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbYearTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbYearTextBox.Location = new System.Drawing.Point(603, 2);
			this.txbYearTextBox.MaxLength = 4;
			this.txbYearTextBox.Name = "txbYearTextBox";
			this.txbYearTextBox.Size = new System.Drawing.Size(95, 33);
			this.txbYearTextBox.TabIndex = 11;
			this.txbYearTextBox.Tag = "(year)";
			this.txbYearTextBox.Text = "(year)";
			this.txbYearTextBox.TextChanged += new System.EventHandler(this.txbYearTextBox_TextChanged);
			this.txbYearTextBox.Enter += new System.EventHandler(this.txbYearTextBox_Enter);
			this.txbYearTextBox.Leave += new System.EventHandler(this.txbYearTextBox_Leave);
			// 
			// txbMonthTextBox
			// 
			this.txbMonthTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txbMonthTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbMonthTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbMonthTextBox.Location = new System.Drawing.Point(704, 2);
			this.txbMonthTextBox.MaxLength = 2;
			this.txbMonthTextBox.Name = "txbMonthTextBox";
			this.txbMonthTextBox.Size = new System.Drawing.Size(84, 33);
			this.txbMonthTextBox.TabIndex = 12;
			this.txbMonthTextBox.Tag = "(month)";
			this.txbMonthTextBox.Text = "(month)";
			this.txbMonthTextBox.TextChanged += new System.EventHandler(this.txbMonthTextBox_TextChanged);
			this.txbMonthTextBox.Enter += new System.EventHandler(this.txbMonthTextBox_Enter);
			this.txbMonthTextBox.Leave += new System.EventHandler(this.txbMonthTextBox_Leave);
			// 
			// ltvModuleTransactionsListView
			// 
			this.ltvModuleTransactionsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ltvModuleTransactionsListView.BackColor = System.Drawing.Color.Snow;
			this.ltvModuleTransactionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModuleName,
            this.StlBasis,
            this.StlEntity,
            this.UnitCount});
			this.ltvModuleTransactionsListView.FullRowSelect = true;
			this.ltvModuleTransactionsListView.GridLines = true;
			this.ltvModuleTransactionsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ltvModuleTransactionsListView.HideSelection = false;
			this.ltvModuleTransactionsListView.Location = new System.Drawing.Point(3, 40);
			this.ltvModuleTransactionsListView.MultiSelect = false;
			this.ltvModuleTransactionsListView.Name = "ltvModuleTransactionsListView";
			this.ltvModuleTransactionsListView.ShowItemToolTips = true;
			this.ltvModuleTransactionsListView.Size = new System.Drawing.Size(824, 623);
			this.ltvModuleTransactionsListView.TabIndex = 14;
			this.ltvModuleTransactionsListView.UseCompatibleStateImageBehavior = false;
			this.ltvModuleTransactionsListView.View = System.Windows.Forms.View.Details;
			this.ltvModuleTransactionsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ltvModuleTransactionsListView_MouseClick);
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
			this.StlEntity.Width = 170;
			// 
			// UnitCount
			// 
			this.UnitCount.Text = "Units";
			this.UnitCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UnitCount.Width = 80;
			// 
			// btnCollectDataButton
			// 
			this.btnCollectDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCollectDataButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnCollectDataButton.FlatAppearance.BorderSize = 0;
			this.btnCollectDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCollectDataButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCollectDataButton.Image = ((System.Drawing.Image)(resources.GetObject("btnCollectDataButton.Image")));
			this.btnCollectDataButton.Location = new System.Drawing.Point(794, 3);
			this.btnCollectDataButton.Name = "btnCollectDataButton";
			this.btnCollectDataButton.Size = new System.Drawing.Size(33, 31);
			this.btnCollectDataButton.TabIndex = 13;
			this.btnCollectDataButton.UseVisualStyleBackColor = false;
			this.btnCollectDataButton.Click += new System.EventHandler(this.btnCollectDataButton_Click);
			// 
			// AdHocConsultationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.Controls.Add(this.cbxHostedClientComboBox);
			this.Controls.Add(this.txbYearTextBox);
			this.Controls.Add(this.txbMonthTextBox);
			this.Controls.Add(this.btnCollectDataButton);
			this.Controls.Add(this.ltvModuleTransactionsListView);
			this.Name = "AdHocConsultationControl";
			this.Size = new System.Drawing.Size(830, 666);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cbxHostedClientComboBox;
		private System.Windows.Forms.TextBox txbYearTextBox;
		private System.Windows.Forms.TextBox txbMonthTextBox;
		private System.Windows.Forms.Button btnCollectDataButton;
		private System.Windows.Forms.ListView ltvModuleTransactionsListView;
		private System.Windows.Forms.ColumnHeader ModuleName;
		private System.Windows.Forms.ColumnHeader StlBasis;
		private System.Windows.Forms.ColumnHeader StlEntity;
		private System.Windows.Forms.ColumnHeader UnitCount;
	}
}
