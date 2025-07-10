namespace Enterprise.ZArchitecture.GUI
{
	partial class FileSaveToOpenForm
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
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.emailButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// messageLabel
			// 
			this.messageLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FileSaveToOpenForm|371b6ce0-6d3e-4a2b-ae84-9ac83faa09df", "The file cannot be opened directly.");
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 59, true);
			this.messageLabel.TabIndex = 0;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// emailButton
			// 
			this.emailButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FileSaveToOpenForm|b889a0ce-34ff-4dbb-b35d-30d11ea81cd1", "Send via email");
			this.emailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 75, true);
			this.emailButton.Name = "emailButton";
			this.emailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.emailButton.TabIndex = 1;
			this.emailButton.UseVisualStyleBackColor = true;
			this.emailButton.Click += new System.EventHandler(this.emailButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FileSaveToOpenForm|43f0fd84-c3ed-482d-822e-cc309d145174", "Save File As...");
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 75, true);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 23, true);
			this.saveButton.TabIndex = 2;
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FileSaveToOpenForm|3a3d110d-eb5f-45e5-a672-683118aab43f", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 75, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// FileSaveToOpenForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 103, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FileSaveToOpenForm|76d9877a-152e-4739-8b2c-350fdbdaf5db", "Save File To Open");
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.emailButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "FileSaveToOpenForm";
			this.Controls.SetChildIndex(this.emailButton, 0);
			this.Controls.SetChildIndex(this.saveButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZLabel messageLabel;
		internal ZButton emailButton;
		internal ZButton saveButton;
		internal ZButton cancelButton;
	}
}
