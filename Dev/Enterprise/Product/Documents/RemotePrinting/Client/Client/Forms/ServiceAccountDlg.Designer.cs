namespace Enterprise.RemotePrinting.Client.Forms
{
	partial class ServiceAccountDlg
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
			this.loginTextBox = new System.Windows.Forms.TextBox();
			this.webPrintServicePWD = new System.Windows.Forms.TextBox();
			this.webPrintServicePWD_Confirmation = new System.Windows.Forms.TextBox();
			this.DomainUserLabel = new System.Windows.Forms.Label();
			this.Passwordlabel = new System.Windows.Forms.Label();
			this.ServiceConfirmationPWDLabel = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// loginTextBox
			// 
			this.loginTextBox.Location = new System.Drawing.Point(60, 60);
			this.loginTextBox.Name = "loginTextBox";
			this.loginTextBox.Size = new System.Drawing.Size(200, 20);
			this.loginTextBox.TabIndex = 0;
			// 
			// webPrintServicePWD
			// 
			this.webPrintServicePWD.Location = new System.Drawing.Point(60, 110);
			this.webPrintServicePWD.Name = "webPrintServicePWD";
			this.webPrintServicePWD.Size = new System.Drawing.Size(200, 20);
			this.webPrintServicePWD.TabIndex = 1;
			this.webPrintServicePWD.UseSystemPasswordChar = true;
			// 
			// webPrintServicePWD_Confirmation
			// 
			this.webPrintServicePWD_Confirmation.Location = new System.Drawing.Point(60, 160);
			this.webPrintServicePWD_Confirmation.Name = "webPrintServicePWD_Confirmation";
			this.webPrintServicePWD_Confirmation.Size = new System.Drawing.Size(200, 20);
			this.webPrintServicePWD_Confirmation.TabIndex = 2;
			this.webPrintServicePWD_Confirmation.UseSystemPasswordChar = true;
			// 
			// DomainUserLabel
			// 
			this.DomainUserLabel.AutoSize = true;
			this.DomainUserLabel.Location = new System.Drawing.Point(60, 40);
			this.DomainUserLabel.Name = "DomainUserLabel";
			this.DomainUserLabel.Size = new System.Drawing.Size(83, 15);
			this.DomainUserLabel.TabIndex = 3;
			this.DomainUserLabel.Text = "Domain User:";
			// 
			// Passwordlabel
			// 
			this.Passwordlabel.AutoSize = true;
			this.Passwordlabel.Location = new System.Drawing.Point(60, 90);
			this.Passwordlabel.Name = "Passwordlabel";
			this.Passwordlabel.Size = new System.Drawing.Size(64, 15);
			this.Passwordlabel.TabIndex = 4;
			this.Passwordlabel.Text = "Password:";
			// 
			// ServiceConfirmationPWDLabel
			// 
			this.ServiceConfirmationPWDLabel.AutoSize = true;
			this.ServiceConfirmationPWDLabel.Location = new System.Drawing.Point(60, 140);
			this.ServiceConfirmationPWDLabel.Name = "ServiceConfirmationPWDLabel";
			this.ServiceConfirmationPWDLabel.Size = new System.Drawing.Size(109, 15);
			this.ServiceConfirmationPWDLabel.TabIndex = 5;
			this.ServiceConfirmationPWDLabel.Text = "Confirm password:";
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(170, 202);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(75, 202);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 7;
			this.buttonOK.Text = "Confirm";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.loginTextBox);
			this.panel1.Controls.Add(this.webPrintServicePWD);
			this.panel1.Controls.Add(this.webPrintServicePWD_Confirmation);
			this.panel1.Controls.Add(this.ServiceConfirmationPWDLabel);
			this.panel1.Controls.Add(this.DomainUserLabel);
			this.panel1.Controls.Add(this.Passwordlabel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(323, 250);
			this.panel1.TabIndex = 8;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(288, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter following account information to install service";
			// 
			// ServiceAccountDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(323, 250);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ServiceAccountDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "WebPrint Service Installation";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox loginTextBox;
		private System.Windows.Forms.TextBox webPrintServicePWD;
		private System.Windows.Forms.TextBox webPrintServicePWD_Confirmation;
		private System.Windows.Forms.Label DomainUserLabel;
		private System.Windows.Forms.Label Passwordlabel;
		private System.Windows.Forms.Label ServiceConfirmationPWDLabel;
		public System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
	}
}
