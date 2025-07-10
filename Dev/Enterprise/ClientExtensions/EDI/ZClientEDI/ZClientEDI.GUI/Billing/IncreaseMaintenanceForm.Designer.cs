namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class IncreaseMaintenanceForm
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
		protected new void InitializeComponent()
		{
			this.okButton = new CargoWise.Windows.UI.KButton();
			this.cancelButton = new CargoWise.Windows.UI.KButton();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.percentBox = new CargoWise.Windows.UI.KNumericUpDown();
			this.oldCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.newCheckBox = new CargoWise.Windows.UI.KCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).BeginInit();
			this.percentBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 24, true);
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 124, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 124, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 28, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 14, true);
			this.label1.TabIndex = 2;
			this.label1.Text = "%";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 28, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 14, true);
			this.label2.TabIndex = 0;
			this.label2.Text = "Increase By:";
			// 
			// percentBox
			// 
			this.percentBox.DecimalPlaces = 2;
			this.percentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 25, true);
			this.percentBox.Name = "percentBox";
			this.percentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.percentBox.TabIndex = 1;
			// 
			// oldCheckBox
			// 
			this.oldCheckBox.AutoSize = true;
			this.oldCheckBox.Checked = true;
			this.oldCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.oldCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 60, true);
			this.oldCheckBox.Name = "oldCheckBox";
			this.oldCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 16, true);
			this.oldCheckBox.TabIndex = 3;
			this.oldCheckBox.Text = "Increase for existing licences";
			this.oldCheckBox.UseVisualStyleBackColor = true;
			// 
			// newCheckBox
			// 
			this.newCheckBox.AutoSize = true;
			this.newCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 83, true);
			this.newCheckBox.Name = "newCheckBox";
			this.newCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 16, true);
			this.newCheckBox.TabIndex = 4;
			this.newCheckBox.Text = "Increase for amendments";
			this.newCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncreaseMaintenanceForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 162, true);
			this.Controls.Add(this.newCheckBox);
			this.Controls.Add(this.oldCheckBox);
			this.Controls.Add(this.percentBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "IncreaseMaintenanceForm";
			this.Text = "Increase Maintenance";
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.percentBox, 0);
			this.Controls.SetChildIndex(this.oldCheckBox, 0);
			this.Controls.SetChildIndex(this.newCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).EndInit();
			this.percentBox.ResumeLayout(false);
			this.percentBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KButton okButton;
		private CargoWise.Windows.UI.KButton cancelButton;
		private CargoWise.Windows.UI.KLabel label1;
		private CargoWise.Windows.UI.KLabel label2;
		private CargoWise.Windows.UI.KNumericUpDown percentBox;
		private CargoWise.Windows.UI.KCheckBox oldCheckBox;
		private CargoWise.Windows.UI.KCheckBox newCheckBox;
	}
}
