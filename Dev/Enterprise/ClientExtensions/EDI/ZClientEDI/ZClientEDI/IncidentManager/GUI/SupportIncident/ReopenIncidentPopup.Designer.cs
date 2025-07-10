namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class ReopenIncidentPopup
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.AssignToSelfButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AssignToCapabilityButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// AssignToSelfButton
			// 
			this.AssignToSelfButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AssignToSelfButton.CaptionResourceString = ZClientEDI.Res.GetData("C4AEB380-71CD-4F12-96E6-C18D58D1B906", "Re-open and assign to self");
			this.AssignToSelfButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 70, true);
			this.AssignToSelfButton.Name = "AssignToSelfButton";
			this.AssignToSelfButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.AssignToSelfButton.TabIndex = 1;
			this.AssignToSelfButton.Text = "Re-open and assign to self";
			this.AssignToSelfButton.UseVisualStyleBackColor = true;
			this.AssignToSelfButton.Click += new System.EventHandler(this.AssignToSelfButton_Click);
			// 
			// AssignToCapabilityButton
			// 
			this.AssignToCapabilityButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AssignToCapabilityButton.CaptionResourceString = ZClientEDI.Res.GetData("39F95474-8EDE-4ABE-B775-E4B4FACC47D2", "Re-open and assign to capability");
			this.AssignToCapabilityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 70, true);
			this.AssignToCapabilityButton.Name = "AssignToCapabilityButton";
			this.AssignToCapabilityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.AssignToCapabilityButton.TabIndex = 2;
			this.AssignToCapabilityButton.Text = "Re-open and assign to capability";
			this.AssignToCapabilityButton.UseVisualStyleBackColor = true;
			this.AssignToCapabilityButton.Click += new System.EventHandler(this.AssignToCapabilityButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = ZClientEDI.Res.GetData("A4D00F34-0A0B-42CC-BC17-9BF7F69A2487", "Cancel");
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 70, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 3;
			this.CancelButtonX.Text = "Cancel";
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// MessageLabel
			// 
			this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 28, true);
			this.MessageLabel.TabIndex = 4;
			this.MessageLabel.Text = "The form will be saved automatically as part of re-opening the incident. Please choose one of the following options to proceed.";
			// 
			// ReopenIncidentPopup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 160, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 160, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 160, true);
			this.AcceptButton = this.AssignToCapabilityButton;
			this.CancelButton = this.CancelButtonX;
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.AssignToCapabilityButton);
			this.Controls.Add(this.AssignToSelfButton);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.Name = "ReopenIncidentPopup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Re-Open Confirmation";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AssignToCapabilityButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.ResumeLayout(false);
		}

		#endregion

		protected Enterprise.ZArchitecture.ZLabel MessageLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton AssignToSelfButton;
		protected Enterprise.ZArchitecture.GUI.ZButton AssignToCapabilityButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;
	}
}
