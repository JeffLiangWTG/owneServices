namespace Enterprise.FaxRouter.Manager
{
	public partial class FaxManagementUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.TrackingNumberTextBox = new System.Windows.Forms.TextBox();
			this.TrackingNumberLabel = new System.Windows.Forms.Label();
			this.SearchButton = new System.Windows.Forms.Button();
			this.ManagementSearchPanel = new System.Windows.Forms.Panel();
			this.NextButton = new System.Windows.Forms.Button();
			this.PrevButton = new System.Windows.Forms.Button();
			this.ClearButton = new System.Windows.Forms.Button();
			this.ShowTop10Button = new System.Windows.Forms.Button();
			this.MainPanel = new System.Windows.Forms.Panel();
			this.FaxManagementDataGrid = new System.Windows.Forms.DataGrid();
			this.ManagementSearchPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FaxManagementDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// TrackingNumberTextBox
			// 
			this.TrackingNumberTextBox.Location = new System.Drawing.Point(65, 16);
			this.TrackingNumberTextBox.Name = "TrackingNumberTextBox";
			this.TrackingNumberTextBox.Size = new System.Drawing.Size(279, 20);
			this.TrackingNumberTextBox.TabIndex = 0;
			this.TrackingNumberTextBox.Text = "";
			// 
			// TrackingNumberLabel
			// 
			this.TrackingNumberLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.TrackingNumberLabel.Location = new System.Drawing.Point(7, 17);
			this.TrackingNumberLabel.Name = "TrackingNumberLabel";
			this.TrackingNumberLabel.Size = new System.Drawing.Size(55, 17);
			this.TrackingNumberLabel.TabIndex = 1;
			this.TrackingNumberLabel.Text = "Fax Job";
			// 
			// SearchButton
			// 
			this.SearchButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.SearchButton.Location = new System.Drawing.Point(352, 16);
			this.SearchButton.Name = "SearchButton";
			this.SearchButton.Size = new System.Drawing.Size(43, 23);
			this.SearchButton.TabIndex = 0;
			this.SearchButton.Text = "Find";
			this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
			// 
			// ManagementSearchPanel
			// 
			this.ManagementSearchPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ManagementSearchPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																								this.NextButton,
																								this.PrevButton,
																								this.ClearButton,
																								this.ShowTop10Button,
																								this.TrackingNumberTextBox,
																								this.TrackingNumberLabel,
																								this.SearchButton });
			this.ManagementSearchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ManagementSearchPanel.Name = "ManagementSearchPanel";
			this.ManagementSearchPanel.Size = new System.Drawing.Size(624, 48);
			this.ManagementSearchPanel.TabIndex = 3;
			// 
			// NextButton
			// 
			this.NextButton.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.NextButton.Location = new System.Drawing.Point(567, 16);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = new System.Drawing.Size(25, 23);
			this.NextButton.TabIndex = 5;
			this.NextButton.Text = ">>";
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// PrevButton
			// 
			this.PrevButton.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.PrevButton.Location = new System.Drawing.Point(466, 16);
			this.PrevButton.Name = "PrevButton";
			this.PrevButton.Size = new System.Drawing.Size(25, 23);
			this.PrevButton.TabIndex = 4;
			this.PrevButton.Text = "<<";
			this.PrevButton.Click += new System.EventHandler(this.PrevButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ClearButton.Location = new System.Drawing.Point(398, 16);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(45, 23);
			this.ClearButton.TabIndex = 3;
			this.ClearButton.Text = "Clear";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// ShowTop10Button
			// 
			this.ShowTop10Button.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ShowTop10Button.Location = new System.Drawing.Point(493, 16);
			this.ShowTop10Button.Name = "ShowTop10Button";
			this.ShowTop10Button.Size = new System.Drawing.Size(70, 23);
			this.ShowTop10Button.TabIndex = 2;
			this.ShowTop10Button.Text = "Top 100";
			this.ShowTop10Button.Click += new System.EventHandler(this.ShowTop10Button_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.FaxManagementDataGrid });
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = new System.Drawing.Point(0, 48);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = new System.Drawing.Size(624, 184);
			this.MainPanel.TabIndex = 4;
			// 
			// FaxManagementDataGrid
			// 
			this.FaxManagementDataGrid.AllowNavigation = false;
			this.FaxManagementDataGrid.CaptionVisible = false;
			this.FaxManagementDataGrid.DataMember = "";
			this.FaxManagementDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FaxManagementDataGrid.GridLineColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.FaxManagementDataGrid.HeaderBackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.FaxManagementDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FaxManagementDataGrid.Name = "FaxManagementDataGrid";
			this.FaxManagementDataGrid.ParentRowsBackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.FaxManagementDataGrid.ParentRowsVisible = false;
			this.FaxManagementDataGrid.ReadOnly = true;
			this.FaxManagementDataGrid.RowHeadersVisible = false;
			this.FaxManagementDataGrid.Size = new System.Drawing.Size(624, 184);
			this.FaxManagementDataGrid.TabIndex = 0;
			// 
			// FaxManagementUserControl
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.MainPanel,
																		  this.ManagementSearchPanel });
			this.Name = "FaxManagementUserControl";
			this.Size = new System.Drawing.Size(624, 232);
			this.ManagementSearchPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FaxManagementDataGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		System.Windows.Forms.TextBox TrackingNumberTextBox;
		System.Windows.Forms.Label TrackingNumberLabel;
		System.Windows.Forms.Button SearchButton;
		System.Windows.Forms.Panel ManagementSearchPanel;
		System.Windows.Forms.Panel MainPanel;
		System.Windows.Forms.DataGrid FaxManagementDataGrid;
		System.Windows.Forms.Button ShowTop10Button;
		System.Windows.Forms.Button ClearButton;
		System.Windows.Forms.Button PrevButton;
		System.Windows.Forms.Button NextButton;
	}
}
