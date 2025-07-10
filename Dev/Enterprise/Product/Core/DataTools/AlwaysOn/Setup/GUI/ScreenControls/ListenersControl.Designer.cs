
namespace Enterprise.AlwaysOn.Setup.GUI
{
	partial class ListenersControl
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListenersControl));
			this.joinLevelImageList = new System.Windows.Forms.ImageList(this.components);
			this.confirmButton = new System.Windows.Forms.Button();
			this.addListenerGroupBox = new System.Windows.Forms.GroupBox();
			this.validateListenerIpsButton = new System.Windows.Forms.Button();
			this.ipAddressesLabel = new System.Windows.Forms.Label();
			this.ipAddressesDataGridView = new System.Windows.Forms.DataGridView();
			this.IpAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.NetworkMask = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.removeListenerButton = new System.Windows.Forms.Button();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.portLabel = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.newListenerNameLabel = new System.Windows.Forms.Label();
			this.addListenerGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ipAddressesDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// joinLevelImageList
			// 
			this.joinLevelImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("joinLevelImageList.ImageStream")));
			this.joinLevelImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.joinLevelImageList.Images.SetKeyName(0, "basic1-137_cross_no.png");
			this.joinLevelImageList.Images.SetKeyName(1, "basic1-062_Battery1of4.png");
			this.joinLevelImageList.Images.SetKeyName(2, "basic1-063_Battery2of4.png");
			this.joinLevelImageList.Images.SetKeyName(3, "basic1-064_Battery3of4.png");
			this.joinLevelImageList.Images.SetKeyName(4, "basic1-065_Battery4of4.png");
			// 
			// confirmButton
			// 
			this.confirmButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.confirmButton.Enabled = false;
			this.confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.confirmButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CheckYes;
			this.confirmButton.Location = new System.Drawing.Point(547, 344);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.Size = new System.Drawing.Size(176, 50);
			this.confirmButton.TabIndex = 20;
			this.confirmButton.Text = "Update Listener";
			this.confirmButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.confirmButton.UseVisualStyleBackColor = false;
			this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
			// 
			// addListenerGroupBox
			// 
			this.addListenerGroupBox.Controls.Add(this.validateListenerIpsButton);
			this.addListenerGroupBox.Controls.Add(this.ipAddressesLabel);
			this.addListenerGroupBox.Controls.Add(this.ipAddressesDataGridView);
			this.addListenerGroupBox.Controls.Add(this.removeListenerButton);
			this.addListenerGroupBox.Controls.Add(this.confirmButton);
			this.addListenerGroupBox.Controls.Add(this.portTextBox);
			this.addListenerGroupBox.Controls.Add(this.portLabel);
			this.addListenerGroupBox.Controls.Add(this.nameTextBox);
			this.addListenerGroupBox.Controls.Add(this.newListenerNameLabel);
			this.addListenerGroupBox.Font = new System.Drawing.Font("Tahoma", 12F);
			this.addListenerGroupBox.Location = new System.Drawing.Point(75, 3);
			this.addListenerGroupBox.Name = "addListenerGroupBox";
			this.addListenerGroupBox.Size = new System.Drawing.Size(739, 465);
			this.addListenerGroupBox.TabIndex = 7;
			this.addListenerGroupBox.TabStop = false;
			// 
			// validateListenerIpsButton
			// 
			this.validateListenerIpsButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.validateListenerIpsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.validateListenerIpsButton.Location = new System.Drawing.Point(406, 279);
			this.validateListenerIpsButton.Name = "validateListenerIpsButton";
			this.validateListenerIpsButton.Size = new System.Drawing.Size(176, 25);
			this.validateListenerIpsButton.TabIndex = 39;
			this.validateListenerIpsButton.Text = "Validate Listener IPs";
			this.validateListenerIpsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.validateListenerIpsButton.UseVisualStyleBackColor = false;
			this.validateListenerIpsButton.Click += new System.EventHandler(this.validateListenerIpsButton_Click);
			// 
			// ipAddressesLabel
			// 
			this.ipAddressesLabel.AutoSize = true;
			this.ipAddressesLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.ipAddressesLabel.Location = new System.Drawing.Point(6, 116);
			this.ipAddressesLabel.Name = "ipAddressesLabel";
			this.ipAddressesLabel.Size = new System.Drawing.Size(107, 19);
			this.ipAddressesLabel.TabIndex = 38;
			this.ipAddressesLabel.Text = "IP Addresses:";
			// 
			// ipAddressesDataGridView
			// 
			this.ipAddressesDataGridView.AllowUserToDeleteRows = false;
			this.ipAddressesDataGridView.AllowUserToResizeRows = false;
			this.ipAddressesDataGridView.BackgroundColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ipAddressesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.ipAddressesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IpAddress,
            this.NetworkMask});
			this.ipAddressesDataGridView.Location = new System.Drawing.Point(6, 138);
			this.ipAddressesDataGridView.Name = "ipAddressesDataGridView";
			this.ipAddressesDataGridView.Size = new System.Drawing.Size(576, 135);
			this.ipAddressesDataGridView.TabIndex = 8;
			this.ipAddressesDataGridView.Enter += new System.EventHandler(this.ipAddressesDataGridView_Enter);
			// 
			// IpAddress
			// 
			this.IpAddress.HeaderText = "IP Address";
			this.IpAddress.Name = "IpAddress";
			this.IpAddress.Width = 250;
			// 
			// NetworkMask
			// 
			this.NetworkMask.HeaderText = "Network Mask";
			this.NetworkMask.Name = "NetworkMask";
			this.NetworkMask.Width = 250;
			// 
			// removeListenerButton
			// 
			this.removeListenerButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.removeListenerButton.Enabled = false;
			this.removeListenerButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.removeListenerButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CrossNo;
			this.removeListenerButton.Location = new System.Drawing.Point(547, 400);
			this.removeListenerButton.Name = "removeListenerButton";
			this.removeListenerButton.Size = new System.Drawing.Size(176, 50);
			this.removeListenerButton.TabIndex = 37;
			this.removeListenerButton.Text = "Remove Listener";
			this.removeListenerButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.removeListenerButton.UseVisualStyleBackColor = false;
			this.removeListenerButton.Click += new System.EventHandler(this.removeListenerButton_Click);
			// 
			// portTextBox
			// 
			this.portTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.portTextBox.Enabled = false;
			this.portTextBox.Font = new System.Drawing.Font("Tahoma", 13F);
			this.portTextBox.Location = new System.Drawing.Point(6, 323);
			this.portTextBox.MaxLength = 5;
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(59, 28);
			this.portTextBox.TabIndex = 19;
			this.portTextBox.Tag = "1433";
			this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.portTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
			this.portTextBox.Leave += new System.EventHandler(this.portTextBox_Leave);
			// 
			// portLabel
			// 
			this.portLabel.AutoSize = true;
			this.portLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.portLabel.Location = new System.Drawing.Point(6, 301);
			this.portLabel.Name = "portLabel";
			this.portLabel.Size = new System.Drawing.Size(44, 19);
			this.portLabel.TabIndex = 28;
			this.portLabel.Text = "Port:";
			// 
			// nameTextBox
			// 
			this.nameTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.nameTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F);
			this.nameTextBox.Location = new System.Drawing.Point(6, 53);
			this.nameTextBox.MaxLength = 15;
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(726, 33);
			this.nameTextBox.TabIndex = 10;
			this.nameTextBox.Tag = "";
			this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
			// 
			// newListenerNameLabel
			// 
			this.newListenerNameLabel.AutoSize = true;
			this.newListenerNameLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.newListenerNameLabel.Location = new System.Drawing.Point(6, 29);
			this.newListenerNameLabel.Name = "newListenerNameLabel";
			this.newListenerNameLabel.Size = new System.Drawing.Size(56, 19);
			this.newListenerNameLabel.TabIndex = 0;
			this.newListenerNameLabel.Text = "Name:";
			// 
			// ListenersControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.addListenerGroupBox);
			this.Name = "ListenersControl";
			this.Size = new System.Drawing.Size(885, 545);
			this.addListenerGroupBox.ResumeLayout(false);
			this.addListenerGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ipAddressesDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button confirmButton;
		private System.Windows.Forms.ImageList joinLevelImageList;
		private System.Windows.Forms.GroupBox addListenerGroupBox;
		private System.Windows.Forms.Label newListenerNameLabel;
		protected System.Windows.Forms.TextBox nameTextBox;
		protected System.Windows.Forms.TextBox portTextBox;
		private System.Windows.Forms.Label portLabel;
		private System.Windows.Forms.Button removeListenerButton;
		protected System.Windows.Forms.DataGridView ipAddressesDataGridView;
		private System.Windows.Forms.Label ipAddressesLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn NetworkMask;
		private System.Windows.Forms.DataGridViewTextBoxColumn IpAddress;
		private System.Windows.Forms.Button validateListenerIpsButton;
	}
}
