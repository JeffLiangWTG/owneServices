namespace Enterprise.DataTransfer.Native.TestClient
{
	partial class TestClientForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.addressLabel = new System.Windows.Forms.Label();
			this.addressTextBox = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.requestTextBox = new System.Windows.Forms.TextBox();
			this.resultsTextBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.updateButton = new System.Windows.Forms.Button();
			this.retrieveButton = new System.Windows.Forms.Button();
			this.webServiceTabControl = new System.Windows.Forms.TabControl();
			this.button1 = new System.Windows.Forms.Button();
			this.tabPage2.SuspendLayout();
			this.webServiceTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.addressLabel);
			this.tabPage2.Controls.Add(this.addressTextBox);
			this.tabPage2.Controls.Add(this.label9);
			this.tabPage2.Controls.Add(this.requestTextBox);
			this.tabPage2.Controls.Add(this.resultsTextBox);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.updateButton);
			this.tabPage2.Controls.Add(this.retrieveButton);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(948, 615);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Web Service";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// addressLabel
			// 
			this.addressLabel.AutoSize = true;
			this.addressLabel.Location = new System.Drawing.Point(6, 11);
			this.addressLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.addressLabel.Name = "addressLabel";
			this.addressLabel.Size = new System.Drawing.Size(87, 13);
			this.addressLabel.TabIndex = 85;
			this.addressLabel.Text = "Service Address:";
			// 
			// addressTextBox
			// 
			this.addressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.addressTextBox.Location = new System.Drawing.Point(98, 9);
			this.addressTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.addressTextBox.Name = "addressTextBox";
			this.addressTextBox.Size = new System.Drawing.Size(607, 20);
			this.addressTextBox.TabIndex = 84;
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 39);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(50, 13);
			this.label9.TabIndex = 83;
			this.label9.Text = "Request:";
			// 
			// requestTextBox
			// 
			this.requestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.requestTextBox.Location = new System.Drawing.Point(6, 55);
			this.requestTextBox.Multiline = true;
			this.requestTextBox.Name = "requestTextBox";
			this.requestTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.requestTextBox.Size = new System.Drawing.Size(512, 554);
			this.requestTextBox.TabIndex = 82;
			this.requestTextBox.WordWrap = false;
			// 
			// resultsTextBox
			// 
			this.resultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.resultsTextBox.Location = new System.Drawing.Point(524, 55);
			this.resultsTextBox.Multiline = true;
			this.resultsTextBox.Name = "resultsTextBox";
			this.resultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.resultsTextBox.Size = new System.Drawing.Size(418, 554);
			this.resultsTextBox.TabIndex = 80;
			this.resultsTextBox.WordWrap = false;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(519, 39);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(58, 13);
			this.label8.TabIndex = 81;
			this.label8.Text = "Response:";
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.Location = new System.Drawing.Point(829, 6);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(114, 23);
			this.updateButton.TabIndex = 78;
			this.updateButton.Text = "Update";
			this.updateButton.UseVisualStyleBackColor = true;
			this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// retrieveButton
			// 
			this.retrieveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.retrieveButton.Location = new System.Drawing.Point(710, 7);
			this.retrieveButton.Name = "retrieveButton";
			this.retrieveButton.Size = new System.Drawing.Size(114, 23);
			this.retrieveButton.TabIndex = 79;
			this.retrieveButton.Text = "Retrieve";
			this.retrieveButton.UseVisualStyleBackColor = true;
			this.retrieveButton.Click += new System.EventHandler(this.RetrieveSampleButton_Click);
			// 
			// webServiceTabControl
			// 
			this.webServiceTabControl.Controls.Add(this.tabPage2);
			this.webServiceTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webServiceTabControl.Location = new System.Drawing.Point(0, 0);
			this.webServiceTabControl.Name = "webServiceTabControl";
			this.webServiceTabControl.SelectedIndex = 0;
			this.webServiceTabControl.Size = new System.Drawing.Size(956, 641);
			this.webServiceTabControl.TabIndex = 78;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			// 
			// TestClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(956, 641);
			this.Controls.Add(this.webServiceTabControl);
			this.Name = "TestClientForm";
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.webServiceTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox requestTextBox;
		private System.Windows.Forms.TextBox resultsTextBox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button updateButton;
		private System.Windows.Forms.Button retrieveButton;
		private System.Windows.Forms.TabControl webServiceTabControl;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label addressLabel;
		private System.Windows.Forms.TextBox addressTextBox;
	}
}

