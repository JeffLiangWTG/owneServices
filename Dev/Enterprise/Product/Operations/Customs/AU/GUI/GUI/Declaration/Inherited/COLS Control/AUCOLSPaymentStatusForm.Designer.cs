namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSPaymentStatusForm
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
		private new void InitializeComponent()
		{
			this.ClientAccountNumberextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaymentStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentStatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 24, true);
			this.MainStatusBar.Text = "Invoice or client account number has max 12 characters";
			// 
			// ClientAccountNumberextBox
			// 
			this.ClientAccountNumberextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientAccountNumberextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 20, true);
			this.ClientAccountNumberextBox.Name = "ClientAccountNumberextBox";
			this.ClientAccountNumberextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 25, true);
			this.ClientAccountNumberextBox.TabIndex = 1;
			this.ClientAccountNumberextBox.TextChanged += new System.EventHandler(this.ClientAccountNumberextBox_TextChanged);
			// 
			// PaymentStatusGroupBox
			// 
			this.PaymentStatusGroupBox.Controls.Add(this.ClientAccountNumberextBox);
			this.PaymentStatusGroupBox.Controls.Add(this.SendButton);
			this.PaymentStatusGroupBox.Controls.Add(this.cancelButton);
			this.PaymentStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.PaymentStatusGroupBox.Name = "PaymentStatusGroupBox";
			this.PaymentStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 78, true);
			this.PaymentStatusGroupBox.TabIndex = 4;
			this.PaymentStatusGroupBox.TabStop = false;
			this.PaymentStatusGroupBox.Text = "Please enter a valid invoice or client account number:";
			// 
			// SendButton
			// 
			this.SendButton.Enabled = false;
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 49, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "Send";
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 49, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// AUCOLSPaymentStatusForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 113, true);
			this.Controls.Add(this.PaymentStatusGroupBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 150, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 150, true);
			this.Name = "AUCOLSPaymentStatusForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "COLS Payment Status";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PaymentStatusGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentStatusGroupBox.ResumeLayout(false);
			this.PaymentStatusGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZTextBox ClientAccountNumberextBox;
		private ZArchitecture.GUI.ZGroupBox PaymentStatusGroupBox;
		internal ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
