namespace Enterprise.CustomerService.GUI
{
	partial class ImportantNoticeForm
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

		private Enterprise.ZArchitecture.ZLabel HeadingLabel;
		private Enterprise.ZArchitecture.ZLabel ContentTextLabel;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton CancelButtonX;

		new void InitializeComponent()
		{
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 10, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// HeadingLabel
			// 
			this.HeadingLabel.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("5459c93e-6f41-48d1-a868-e47bb41f9741", "FASTEST RESOLUTION WILL BE RECEIVED IF THE REQUEST IS LOGGED FROM THE CORRECT FORM/SCREEN");
			this.HeadingLabel.ForeColor = System.Drawing.Color.Red;
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 26, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 72, true);
			this.HeadingLabel.TabIndex = 1;
			this.HeadingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// ContentTextLabel
			// 
			this.ContentTextLabel.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("9e912c86-d66c-47d5-b7b3-b7ddaa1e81ef", "Wherever possible you must lodge eRequests related to software functionality from the related form with appropriate data visible. This will ensure that we get the correct module/product team allocation and a suitable screen shot. The more accurate the information provided the faster and more complete our response will be. ");
			this.ContentTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 105, true);
			this.ContentTextLabel.Name = "ContentTextLabel";
			this.ContentTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 59, true);
			this.ContentTextLabel.TabIndex = 2;
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("0354ccb4-b615-4fe0-a53f-5123866580b6", "Continue");
			this.SendButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 189, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("ee0cd1ae-f111-43df-96df-3fee25fcc607", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 189, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.CancelButtonX.TabIndex = 4;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ImportantNoticeForm
			// 
			this.AcceptButton = this.SendButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("c70094c6-3c27-4bc0-9029-26dbf423edb6", "Important");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 226, true);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.ContentTextLabel);
			this.Controls.Add(this.HeadingLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ImportantNoticeForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HeadingLabel, 0);
			this.Controls.SetChildIndex(this.ContentTextLabel, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
