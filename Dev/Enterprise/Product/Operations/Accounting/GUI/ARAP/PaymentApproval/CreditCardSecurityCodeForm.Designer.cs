namespace Enterprise.Accounting.GUI.ARAP.PaymentApproval
{
	partial class CreditCardSecurityCodeForm
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
		new void InitializeComponent()
		{
			this.CardSecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentCreditCardSecurityCode);
			// 
			// CardSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CardSecurityCodeTextBox, "CardSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentCreditCardSecurityCode)(null)).CardSecurityCode)));
			this.CardSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.CardSecurityCodeTextBox.Name = "CardSecurityCodeTextBox";
			this.CardSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.CardSecurityCodeTextBox.TabIndex = 1;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageLabel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardSecurityCodeForm|e2dc8de4-64bd-4326-ac39-825d07053e33", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 23, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardSecurityCodeForm|1a2c1762-6365-4113-b4cf-9384b3eaf99f", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 23, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 4;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// CreditCardSecurityCodeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 87, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardSecurityCodeForm|2ba36da6-105e-464c-8de3-6a91594137ca", "Credit Card Security Code");
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.CardSecurityCodeTextBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentCreditCardSecurityCode);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 123, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 123, true);
			this.Name = "CreditCardSecurityCodeForm";
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CardSecurityCodeTextBox, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox CardSecurityCodeTextBox;
		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
	}
}
