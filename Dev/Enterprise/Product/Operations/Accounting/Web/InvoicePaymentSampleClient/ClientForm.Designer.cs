namespace InvoicePaymentSampleClient
{
	partial class ClientForm
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
			this.RequestGroupBox = new System.Windows.Forms.GroupBox();
			this.PaymentReferenceTextBox = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.InternalReferenceTextBox = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.PaymentDateSetCheckBox = new System.Windows.Forms.CheckBox();
			this.PaymentDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.TransactionNumberTextBox = new System.Windows.Forms.TextBox();
			this.JobTransactionNumberTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.PaymentAmontTextBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.TransactionTypeComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.RequestXMLButton = new System.Windows.Forms.Button();
			this.LedgerTypeComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.CompanyCodeTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.ClientCodeTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ResponseTextBox = new System.Windows.Forms.TextBox();
			this.LoginDetailsGroupBox = new System.Windows.Forms.GroupBox();
			this.PasswordTextBox = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.UserNameTextBox = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textBoxUrl = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.RequestGroupBox.SuspendLayout();
			this.LoginDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// RequestGroupBox
			// 
			this.RequestGroupBox.Controls.Add(this.PaymentReferenceTextBox);
			this.RequestGroupBox.Controls.Add(this.label12);
			this.RequestGroupBox.Controls.Add(this.InternalReferenceTextBox);
			this.RequestGroupBox.Controls.Add(this.label11);
			this.RequestGroupBox.Controls.Add(this.PaymentDateSetCheckBox);
			this.RequestGroupBox.Controls.Add(this.PaymentDateTimePicker);
			this.RequestGroupBox.Controls.Add(this.TransactionNumberTextBox);
			this.RequestGroupBox.Controls.Add(this.JobTransactionNumberTextBox);
			this.RequestGroupBox.Controls.Add(this.label4);
			this.RequestGroupBox.Controls.Add(this.label5);
			this.RequestGroupBox.Controls.Add(this.PaymentAmontTextBox);
			this.RequestGroupBox.Controls.Add(this.label8);
			this.RequestGroupBox.Controls.Add(this.label7);
			this.RequestGroupBox.Controls.Add(this.TransactionTypeComboBox);
			this.RequestGroupBox.Controls.Add(this.label6);
			this.RequestGroupBox.Controls.Add(this.RequestXMLButton);
			this.RequestGroupBox.Controls.Add(this.LedgerTypeComboBox);
			this.RequestGroupBox.Controls.Add(this.label3);
			this.RequestGroupBox.Controls.Add(this.CompanyCodeTextBox);
			this.RequestGroupBox.Controls.Add(this.label2);
			this.RequestGroupBox.Controls.Add(this.ClientCodeTextBox);
			this.RequestGroupBox.Controls.Add(this.label1);
			this.RequestGroupBox.Location = new System.Drawing.Point(1, 53);
			this.RequestGroupBox.Name = "RequestGroupBox";
			this.RequestGroupBox.Size = new System.Drawing.Size(705, 197);
			this.RequestGroupBox.TabIndex = 8;
			this.RequestGroupBox.TabStop = false;
			this.RequestGroupBox.Text = "Request Parameters";
			// 
			// PaymentReferenceTextBox
			// 
			this.PaymentReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.PaymentReferenceTextBox.Location = new System.Drawing.Point(159, 100);
			this.PaymentReferenceTextBox.Name = "PaymentReferenceTextBox";
			this.PaymentReferenceTextBox.Size = new System.Drawing.Size(170, 20);
			this.PaymentReferenceTextBox.TabIndex = 14;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(47, 103);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(104, 13);
			this.label12.TabIndex = 13;
			this.label12.Text = "Payment Reference:";
			// 
			// InternalReferenceTextBox
			// 
			this.InternalReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.InternalReferenceTextBox.Enabled = false;
			this.InternalReferenceTextBox.Location = new System.Drawing.Point(479, 100);
			this.InternalReferenceTextBox.Name = "InternalReferenceTextBox";
			this.InternalReferenceTextBox.Size = new System.Drawing.Size(170, 20);
			this.InternalReferenceTextBox.TabIndex = 16;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(375, 103);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(98, 13);
			this.label11.TabIndex = 15;
			this.label11.Text = "Internal Reference:";
			// 
			// PaymentDateSetCheckBox
			// 
			this.PaymentDateSetCheckBox.AutoSize = true;
			this.PaymentDateSetCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PaymentDateSetCheckBox.Location = new System.Drawing.Point(376, 131);
			this.PaymentDateSetCheckBox.Name = "PaymentDateSetCheckBox";
			this.PaymentDateSetCheckBox.Size = new System.Drawing.Size(97, 17);
			this.PaymentDateSetCheckBox.TabIndex = 19;
			this.PaymentDateSetCheckBox.Text = "Fully Paid Date";
			this.PaymentDateSetCheckBox.UseVisualStyleBackColor = true;
			this.PaymentDateSetCheckBox.CheckStateChanged += new System.EventHandler(this.PaymentDateSetCheckBox_CheckStateChanged);
			// 
			// PaymentDateTimePicker
			// 
			this.PaymentDateTimePicker.Enabled = false;
			this.PaymentDateTimePicker.Location = new System.Drawing.Point(480, 128);
			this.PaymentDateTimePicker.Name = "PaymentDateTimePicker";
			this.PaymentDateTimePicker.Size = new System.Drawing.Size(200, 20);
			this.PaymentDateTimePicker.TabIndex = 20;
			// 
			// TransactionNumberTextBox
			// 
			this.TransactionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TransactionNumberTextBox.Location = new System.Drawing.Point(159, 73);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = new System.Drawing.Size(170, 20);
			this.TransactionNumberTextBox.TabIndex = 10;
			// 
			// JobTransactionNumberTextBox
			// 
			this.JobTransactionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.JobTransactionNumberTextBox.Location = new System.Drawing.Point(479, 73);
			this.JobTransactionNumberTextBox.Name = "JobTransactionNumberTextBox";
			this.JobTransactionNumberTextBox.Size = new System.Drawing.Size(170, 20);
			this.JobTransactionNumberTextBox.TabIndex = 12;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(63, 131);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(90, 13);
			this.label4.TabIndex = 17;
			this.label4.Text = "Payment Amount:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 178);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 22;
			this.label5.Text = "Response";
			// 
			// PaymentAmontTextBox
			// 
			this.PaymentAmontTextBox.Location = new System.Drawing.Point(159, 128);
			this.PaymentAmontTextBox.Name = "PaymentAmontTextBox";
			this.PaymentAmontTextBox.Size = new System.Drawing.Size(100, 20);
			this.PaymentAmontTextBox.TabIndex = 18;
			this.PaymentAmontTextBox.TabStop = false;
			this.PaymentAmontTextBox.Text = "0.00";
			this.PaymentAmontTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(347, 77);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(126, 13);
			this.label8.TabIndex = 11;
			this.label8.Text = "Job Transaction Number:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(47, 77);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(106, 13);
			this.label7.TabIndex = 9;
			this.label7.Text = "Transaction Number:";
			// 
			// TransactionTypeComboBox
			// 
			this.TransactionTypeComboBox.FormattingEnabled = true;
			this.TransactionTypeComboBox.ItemHeight = 13;
			this.TransactionTypeComboBox.Items.AddRange(new object[] {
            "INV",
            "CRD"});
			this.TransactionTypeComboBox.Location = new System.Drawing.Point(479, 46);
			this.TransactionTypeComboBox.Name = "TransactionTypeComboBox";
			this.TransactionTypeComboBox.Size = new System.Drawing.Size(49, 21);
			this.TransactionTypeComboBox.TabIndex = 19;
			this.TransactionTypeComboBox.Text = "INV";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(383, 49);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(90, 13);
			this.label6.TabIndex = 7;
			this.label6.Text = "TransactionType:";
			// 
			// RequestXMLButton
			// 
			this.RequestXMLButton.Location = new System.Drawing.Point(548, 168);
			this.RequestXMLButton.Name = "RequestXMLButton";
			this.RequestXMLButton.Size = new System.Drawing.Size(101, 23);
			this.RequestXMLButton.TabIndex = 21;
			this.RequestXMLButton.Text = "Request XML";
			this.RequestXMLButton.UseVisualStyleBackColor = true;
			this.RequestXMLButton.Click += new System.EventHandler(this.RequestXMLButton_Click);
			// 
			// LedgerTypeComboBox
			// 
			this.LedgerTypeComboBox.FormattingEnabled = true;
			this.LedgerTypeComboBox.ItemHeight = 13;
			this.LedgerTypeComboBox.Items.AddRange(new object[] {
            "AR",
            "AP"});
			this.LedgerTypeComboBox.Location = new System.Drawing.Point(159, 45);
			this.LedgerTypeComboBox.Name = "LedgerTypeComboBox";
			this.LedgerTypeComboBox.Size = new System.Drawing.Size(47, 21);
			this.LedgerTypeComboBox.TabIndex = 13;
			this.LedgerTypeComboBox.Text = "AR";
			this.LedgerTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.LedgerTypeComboBox_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(83, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Ledger Type:";
			// 
			// CompanyCodeTextBox
			// 
			this.CompanyCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.CompanyCodeTextBox.Location = new System.Drawing.Point(159, 19);
			this.CompanyCodeTextBox.MaxLength = 3;
			this.CompanyCodeTextBox.Name = "CompanyCodeTextBox";
			this.CompanyCodeTextBox.Size = new System.Drawing.Size(47, 20);
			this.CompanyCodeTextBox.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.ControlLight;
			this.label2.Location = new System.Drawing.Point(71, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(82, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Company Code;";
			// 
			// ClientCodeTextBox
			// 
			this.ClientCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ClientCodeTextBox.Location = new System.Drawing.Point(480, 19);
			this.ClientCodeTextBox.MaxLength = 35;
			this.ClientCodeTextBox.Name = "ClientCodeTextBox";
			this.ClientCodeTextBox.Size = new System.Drawing.Size(170, 20);
			this.ClientCodeTextBox.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(409, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Client Code:";
			// 
			// ResponseTextBox
			// 
			this.ResponseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResponseTextBox.Location = new System.Drawing.Point(1, 251);
			this.ResponseTextBox.Multiline = true;
			this.ResponseTextBox.Name = "ResponseTextBox";
			this.ResponseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ResponseTextBox.Size = new System.Drawing.Size(705, 230);
			this.ResponseTextBox.TabIndex = 23;
			// 
			// LoginDetailsGroupBox
			// 
			this.LoginDetailsGroupBox.Controls.Add(this.textBoxUrl);
			this.LoginDetailsGroupBox.Controls.Add(this.label13);
			this.LoginDetailsGroupBox.Controls.Add(this.PasswordTextBox);
			this.LoginDetailsGroupBox.Controls.Add(this.label10);
			this.LoginDetailsGroupBox.Controls.Add(this.UserNameTextBox);
			this.LoginDetailsGroupBox.Controls.Add(this.label9);
			this.LoginDetailsGroupBox.Location = new System.Drawing.Point(1, 3);
			this.LoginDetailsGroupBox.Name = "LoginDetailsGroupBox";
			this.LoginDetailsGroupBox.Size = new System.Drawing.Size(705, 44);
			this.LoginDetailsGroupBox.TabIndex = 11;
			this.LoginDetailsGroupBox.TabStop = false;
			this.LoginDetailsGroupBox.Text = "Login details";
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Location = new System.Drawing.Point(242, 15);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = new System.Drawing.Size(90, 20);
			this.PasswordTextBox.TabIndex = 3;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(180, 18);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Password:";
			// 
			// UserNameTextBox
			// 
			this.UserNameTextBox.Location = new System.Drawing.Point(76, 15);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = new System.Drawing.Size(90, 20);
			this.UserNameTextBox.TabIndex = 1;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(10, 18);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(63, 13);
			this.label9.TabIndex = 0;
			this.label9.Text = "User Name:";
			// 
			// textBoxUrl
			// 
			this.textBoxUrl.Location = new System.Drawing.Point(388, 15);
			this.textBoxUrl.Name = "textBoxUrl";
			this.textBoxUrl.Size = new System.Drawing.Size(292, 20);
			this.textBoxUrl.TabIndex = 5;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(350, 18);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(32, 13);
			this.label13.TabIndex = 4;
			this.label13.Text = "URL:";
			// 
			// ClientForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(708, 482);
			this.Controls.Add(this.LoginDetailsGroupBox);
			this.Controls.Add(this.ResponseTextBox);
			this.Controls.Add(this.RequestGroupBox);
			this.Name = "ClientForm";
			this.Text = "Invoice Payment Test Client Form";
			this.RequestGroupBox.ResumeLayout(false);
			this.RequestGroupBox.PerformLayout();
			this.LoginDetailsGroupBox.ResumeLayout(false);
			this.LoginDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox RequestGroupBox;
		private System.Windows.Forms.ComboBox LedgerTypeComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox CompanyCodeTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox ClientCodeTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox ResponseTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button RequestXMLButton;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox TransactionTypeComboBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.GroupBox LoginDetailsGroupBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox PasswordTextBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox UserNameTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox PaymentAmontTextBox;
		private System.Windows.Forms.TextBox JobTransactionNumberTextBox;
		private System.Windows.Forms.TextBox TransactionNumberTextBox;
		private System.Windows.Forms.CheckBox PaymentDateSetCheckBox;
		private System.Windows.Forms.DateTimePicker PaymentDateTimePicker;
		private System.Windows.Forms.TextBox InternalReferenceTextBox;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox PaymentReferenceTextBox;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox textBoxUrl;
		private System.Windows.Forms.Label label13;
	}
}

