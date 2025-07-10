namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class BaseIncidentPopupForm
	{

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("BaseIncidentPopupForm|43727e3b-22c0-4629-a32b-deb607e85e6d", "OK");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 216, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Text = "OK";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MessageLabel
			// 
			this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 28, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.Text = "<Instructional Message>";
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("BaseIncidentPopupForm|2f9753c5-4153-4ce1-a2d2-460afa081d42", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 216, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 2;
			this.CancelButtonX.Text = "Cancel";
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// BaseIncidentPopupForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 269, true);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 180, true);
			this.Name = "BaseIncidentPopupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add Incident Log";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		protected Enterprise.ZArchitecture.ZLabel MessageLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;
	}

}
