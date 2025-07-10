using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.Upgrades;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Loader
{
	sealed class NoCurrentVersionDialog : Form
	{
		private PictureBox pictureBox1;
		private Label label1;
		private IContainer components;
		private DataGridView dataGridViewPackages;
		private BindingSource upgradeInfoExtendedCollectionBindingSource;
		private DataGridViewTextBoxColumn versionDateDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn versionDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
		private Button buttonRun;

		public NoCurrentVersionDialog()
		{
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
			this.pictureBox1.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();
		}

		public NoCurrentVersionDialog(UpgradeInfoExtendedCollection upgradePackages)
			: this()
		{
			upgradeInfoExtendedCollectionBindingSource.DataSource = upgradePackages;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.components = new Container();
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			this.label1 = new Label();
			this.pictureBox1 = new PictureBox();
			this.dataGridViewPackages = new DataGridView();
			this.versionDateDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			this.versionDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			this.statusDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			this.upgradeInfoExtendedCollectionBindingSource = new BindingSource(this.components);
			this.buttonRun = new Button();
			((ISupportInitialize)(this.pictureBox1)).BeginInit();
			((ISupportInitialize)(this.dataGridViewPackages)).BeginInit();
			((ISupportInitialize)(this.upgradeInfoExtendedCollectionBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(64, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(340, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "The current software version is not indicated in the database. Please select the " +
				"version you would like to run or import an upgrade package file.";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(8, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// dataGridViewPackages
			// 
			this.dataGridViewPackages.AllowUserToAddRows = false;
			this.dataGridViewPackages.AllowUserToDeleteRows = false;
			this.dataGridViewPackages.AutoGenerateColumns = false;
			dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			this.dataGridViewPackages.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridViewPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewPackages.Columns.AddRange(new DataGridViewColumn[] {
			this.versionDateDataGridViewTextBoxColumn,
			this.versionDataGridViewTextBoxColumn,
			this.statusDataGridViewTextBoxColumn });
			this.dataGridViewPackages.DataSource = this.upgradeInfoExtendedCollectionBindingSource;
			dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
			this.dataGridViewPackages.DefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridViewPackages.Location = new System.Drawing.Point(8, 62);
			this.dataGridViewPackages.MultiSelect = false;
			this.dataGridViewPackages.Name = "dataGridViewPackages";
			this.dataGridViewPackages.ReadOnly = true;
			dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
			this.dataGridViewPackages.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.dataGridViewPackages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			this.dataGridViewPackages.Size = new System.Drawing.Size(396, 150);
			this.dataGridViewPackages.TabIndex = 1;
			// 
			// versionDateDataGridViewTextBoxColumn
			// 
			this.versionDateDataGridViewTextBoxColumn.DataPropertyName = "VersionDate";
			dataGridViewCellStyle2.Format = "d-MMM-yy H:mm";
			this.versionDateDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
			this.versionDateDataGridViewTextBoxColumn.HeaderText = "VersionDate";
			this.versionDateDataGridViewTextBoxColumn.Name = "versionDateDataGridViewTextBoxColumn";
			this.versionDateDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// versionDataGridViewTextBoxColumn
			// 
			this.versionDataGridViewTextBoxColumn.DataPropertyName = "Version";
			this.versionDataGridViewTextBoxColumn.HeaderText = "Version";
			this.versionDataGridViewTextBoxColumn.Name = "versionDataGridViewTextBoxColumn";
			this.versionDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// statusDataGridViewTextBoxColumn
			// 
			this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
			this.statusDataGridViewTextBoxColumn.HeaderText = "Status";
			this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
			this.statusDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// upgradeInfoExtendedCollectionBindingSource
			// 
			this.upgradeInfoExtendedCollectionBindingSource.DataSource = typeof(UpgradeInfoExtendedCollection);
			// 
			// buttonRun
			// 
			this.buttonRun.Location = new System.Drawing.Point(323, 219);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(80, 23);
			this.buttonRun.TabIndex = 2;
			this.buttonRun.Text = "Run Selected";
			this.buttonRun.UseVisualStyleBackColor = true;
			this.buttonRun.Click += new EventHandler(this.buttonRun_Click);
			// 
			// NoCurrentVersionDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 253);
			this.Controls.Add(this.buttonRun);
			this.Controls.Add(this.dataGridViewPackages);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NoCurrentVersionDialog";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Select Software Version";
			((ISupportInitialize)(this.pictureBox1)).EndInit();
			((ISupportInitialize)(this.dataGridViewPackages)).EndInit();
			((ISupportInitialize)(this.upgradeInfoExtendedCollectionBindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		private void buttonRun_Click(object sender, EventArgs e)
		{
			if (dataGridViewPackages.CurrentRow == null)
			{
				MessageBox.Show("You must select a version", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				UpgradeToRun = ((UpgradeInfoExtendedCollection)upgradeInfoExtendedCollectionBindingSource.DataSource)[dataGridViewPackages.CurrentRow.Index];
				Close();
			}
		}

		public UpgradeInfoExtended UpgradeToRun { get; private set; }
	}
}
