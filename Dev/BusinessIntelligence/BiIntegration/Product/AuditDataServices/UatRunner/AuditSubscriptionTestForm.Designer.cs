namespace Enterprise.AuditDataServices.UatRunner
{
	partial class AuditSubscriptionForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (notificationTask != null)
				{
					notificationTask.Dispose();
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
			this.RunNotificationOnceButton = new System.Windows.Forms.Button();
			this.LogTextBox = new System.Windows.Forms.TextBox();
			this.StartAuditNotificationButton = new System.Windows.Forms.Button();
			this.StopAuditNotificationButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// RunNotificationOnceButton
			// 
			this.RunNotificationOnceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RunNotificationOnceButton.Location = new System.Drawing.Point(12, 12);
			this.RunNotificationOnceButton.Name = "RunNotificationOnceButton";
			this.RunNotificationOnceButton.Size = new System.Drawing.Size(528, 109);
			this.RunNotificationOnceButton.TabIndex = 0;
			this.RunNotificationOnceButton.Text = "Run Once";
			this.RunNotificationOnceButton.UseVisualStyleBackColor = true;
			this.RunNotificationOnceButton.Click += new System.EventHandler(this.RunNotificationOnceButton_Click);
			// 
			// LogTextBox
			// 
			this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LogTextBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LogTextBox.Location = new System.Drawing.Point(12, 127);
			this.LogTextBox.Multiline = true;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LogTextBox.Size = new System.Drawing.Size(1705, 967);
			this.LogTextBox.TabIndex = 5;
			// 
			// StartAuditNotificationButton
			// 
			this.StartAuditNotificationButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StartAuditNotificationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StartAuditNotificationButton.Location = new System.Drawing.Point(546, 12);
			this.StartAuditNotificationButton.Name = "StartAuditNotificationButton";
			this.StartAuditNotificationButton.Size = new System.Drawing.Size(1171, 51);
			this.StartAuditNotificationButton.TabIndex = 6;
			this.StartAuditNotificationButton.Text = "Start Notification";
			this.StartAuditNotificationButton.UseVisualStyleBackColor = true;
			this.StartAuditNotificationButton.Click += new System.EventHandler(this.StartAuditNotificationButton_Click);
			// 
			// StopAuditNotificationButton
			// 
			this.StopAuditNotificationButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StopAuditNotificationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StopAuditNotificationButton.Location = new System.Drawing.Point(546, 70);
			this.StopAuditNotificationButton.Name = "StopAuditNotificationButton";
			this.StopAuditNotificationButton.Size = new System.Drawing.Size(1171, 51);
			this.StopAuditNotificationButton.TabIndex = 7;
			this.StopAuditNotificationButton.Text = "Stop";
			this.StopAuditNotificationButton.UseVisualStyleBackColor = true;
			this.StopAuditNotificationButton.Click += new System.EventHandler(this.StopAuditNotificationButton_Click);
			// 
			// AuditSubscriptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1729, 1106);
			this.Controls.Add(this.StopAuditNotificationButton);
			this.Controls.Add(this.StartAuditNotificationButton);
			this.Controls.Add(this.LogTextBox);
			this.Controls.Add(this.RunNotificationOnceButton);
			this.Name = "AuditSubscriptionForm";
			this.Text = "Audit Subscription Test";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button RunNotificationOnceButton;
		private System.Windows.Forms.TextBox LogTextBox;
		private System.Windows.Forms.Button StartAuditNotificationButton;
		private System.Windows.Forms.Button StopAuditNotificationButton;
	}
}

