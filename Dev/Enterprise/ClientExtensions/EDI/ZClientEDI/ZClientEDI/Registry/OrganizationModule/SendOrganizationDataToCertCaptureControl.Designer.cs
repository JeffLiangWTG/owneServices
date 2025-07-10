namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class SendOrganizationDataToCertCaptureControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ActionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendAllOrganizationsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.noRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.yesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ActionGroupBox.SuspendLayout();
			this.optionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.SendOrganizationDataToCertCapture);
			// 
			// ActionGroupBox
			// 
			this.ActionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("03c10136-2831-43d2-951f-1108c4599386", "Action");
			this.ActionGroupBox.Controls.Add(this.ActionLabel);
			this.ActionGroupBox.Controls.Add(this.SendAllOrganizationsButton);
			this.ActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
			this.ActionGroupBox.Name = "ActionGroupBox";
			this.ActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 250, true);
			this.ActionGroupBox.TabIndex = 2;
			this.ActionGroupBox.TabStop = false;
			// 
			// ActionLabel
			// 
			this.ActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionLabel.CaptionResourceString = ZClientEDI.Res.GetData("5a2f5787-ebf0-4d80-a1c2-d7a5a1a104ad", "Click this button to send all US receivables organizations to Cert Capture, this operation is charged per organization and may take some time.");
			this.ActionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 14, true);
			this.ActionLabel.Name = "ActionLabel";
			this.ActionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 206, true);
			this.ActionLabel.TabIndex = 2;
			this.ActionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SendAllOrganizationsButton
			// 
			this.SendAllOrganizationsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendAllOrganizationsButton.CaptionResourceString = ZClientEDI.Res.GetData("7389ab2a-e07e-438e-a1d7-4b41019c0291", "Send all organizations");
			this.SendAllOrganizationsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 222, true);
			this.SendAllOrganizationsButton.Name = "SendAllOrganizationsButton";
			this.SendAllOrganizationsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 23, true);
			this.SendAllOrganizationsButton.TabIndex = 4;
			this.SendAllOrganizationsButton.ToolTipCaption = null;
			this.SendAllOrganizationsButton.Click += new System.EventHandler(this.SendAllOrganizationsButton_Click);
			// 
			// optionGroupBox
			// 
			this.optionGroupBox.Controls.Add(this.noRadioButton);
			this.optionGroupBox.Controls.Add(this.yesRadioButton);
			this.optionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.optionGroupBox.Name = "optionGroupBox";
			this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
			this.optionGroupBox.TabIndex = 3;
			this.optionGroupBox.TabStop = false;
			// 
			// noRadioButton
			// 
			this.noRadioButton.AutoCheck = false;
			this.noRadioButton.AutoSize = true;
			this.noRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.noRadioButton.CaptionResourceString = ZClientEDI.Res.GetData("1f506f36-9e16-4aaf-8704-3d515746bc44", "No");
			this.noRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 24, true);
			this.noRadioButton.Name = "noRadioButton";
			this.noRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.noRadioButton.TabIndex = 3;
			this.noRadioButton.TabStop = true;
			this.noRadioButton.UseVisualStyleBackColor = false;
			// 
			// yesRadioButton
			// 
			this.yesRadioButton.AutoCheck = false;
			this.yesRadioButton.AutoSize = true;
			this.yesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.yesRadioButton, "EnableSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.SendOrganizationDataToCertCapture)(null)).EnableSend)));
			this.yesRadioButton.CaptionResourceString = ZClientEDI.Res.GetData("8cae2c23-f8ce-4642-b9f3-43c1edf2328f", "Yes");
			this.yesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 24, true);
			this.yesRadioButton.Name = "yesRadioButton";
			this.yesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.yesRadioButton.TabIndex = 2;
			this.yesRadioButton.TabStop = true;
			this.yesRadioButton.UseVisualStyleBackColor = false;
			// 
			// SendOrganizationDataToCertCaptureControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.optionGroupBox);
			this.Controls.Add(this.ActionGroupBox);
			this.Name = "SendOrganizationDataToCertCaptureControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActionGroupBox.ResumeLayout(false);
			this.ActionGroupBox.PerformLayout();
			this.optionGroupBox.ResumeLayout(false);
			this.optionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ActionGroupBox;
		internal ZArchitecture.GUI.ZButton SendAllOrganizationsButton;
		protected CargoWise.Windows.UI.KGroupBox optionGroupBox;
		internal ZArchitecture.GUI.ZRadioButton noRadioButton;
		internal ZArchitecture.GUI.ZRadioButton yesRadioButton;
		private ZArchitecture.ZLabel ActionLabel;
	}
}
