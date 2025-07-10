namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm));
			this.SaveAndSendRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SaveWithoutSendingRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.CancelRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AmendmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OutturnNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SavingOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AgreeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SavingOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 219, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.DeferredCusOutturnHeaderSavingOptions);
			// 
			// SaveAndSendRadioButton
			// 
			this.SaveAndSendRadioButton.AutoCheck = false;
			this.SaveAndSendRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SaveAndSendRadioButton, "ShouldSaveAndSendMessages");
			this.SaveAndSendRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveAndSendRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SaveAndSendRadioButton.Name = "SaveAndSendRadioButton";
			this.SaveAndSendRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 17, true);
			this.SaveAndSendRadioButton.TabIndex = 1;
			this.SaveAndSendRadioButton.TabStop = true;
			this.SaveAndSendRadioButton.Text = "Save and Send messages now.";
			this.SaveAndSendRadioButton.UseVisualStyleBackColor = true;
			// 
			// SaveWithoutSendingRadioButton
			// 
			this.SaveWithoutSendingRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SaveWithoutSendingRadioButton, "ShouldSaveWithoutSendingMessages");
			this.SaveWithoutSendingRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveWithoutSendingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.SaveWithoutSendingRadioButton.Name = "SaveWithoutSendingRadioButton";
			this.SaveWithoutSendingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 36, true);
			this.SaveWithoutSendingRadioButton.TabIndex = 2;
			this.SaveWithoutSendingRadioButton.TabStop = true;
			this.SaveWithoutSendingRadioButton.Text = resources.GetString("SaveWithoutSendingRadioButton.Text");
			this.SaveWithoutSendingRadioButton.UseVisualStyleBackColor = true;
			// 
			// CancelRadioButton
			// 
			this.CancelRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.CancelRadioButton, "IsCancel");
			this.CancelRadioButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("96704464-e130-4a4d-bc45-68ce64f89faf", "", "Do not save now, and return to editing this Sea Cargo Outturn if you are editing a Sea Cargo Outturn, or abandon scanning results if you are scanning outturns.");
			this.CancelRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 84, true);
			this.CancelRadioButton.Name = "CancelRadioButton";
			this.CancelRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 36, true);
			this.CancelRadioButton.TabIndex = 3;
			this.CancelRadioButton.TabStop = true;
			this.CancelRadioButton.Text = "Do not save now, and return to editing this Sea Cargo Outturn if you are editing " +
    "a Sea Cargo Outturn, or abandon scanning results if you are scanning outturns.";
			this.CancelRadioButton.UseVisualStyleBackColor = true;
			// 
			// AmendmentLabel
			// 
			this.AmendmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.AmendmentLabel.Name = "AmendmentLabel";
			this.AmendmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 23, true);
			this.AmendmentLabel.TabIndex = 4;
			this.AmendmentLabel.Text = "The following amendments need to be sent:";
			// 
			// OutturnNameLabel
			// 
			this.OutturnNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 32, true);
			this.OutturnNameLabel.Name = "OutturnNameLabel";
			this.OutturnNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 23, true);
			this.OutturnNameLabel.TabIndex = 5;
			// 
			// SavingOptionsGroupBox
			// 
			this.SavingOptionsGroupBox.Controls.Add(this.SaveAndSendRadioButton);
			this.SavingOptionsGroupBox.Controls.Add(this.SaveWithoutSendingRadioButton);
			this.SavingOptionsGroupBox.Controls.Add(this.CancelRadioButton);
			this.SavingOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.SavingOptionsGroupBox.Name = "SavingOptionsGroupBox";
			this.SavingOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 126, true);
			this.SavingOptionsGroupBox.TabIndex = 6;
			this.SavingOptionsGroupBox.TabStop = false;
			this.SavingOptionsGroupBox.Text = "Saving Options";
			// 
			// AgreeButton
			// 
			this.AgreeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 190, true);
			this.AgreeButton.Name = "AgreeButton";
			this.AgreeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AgreeButton.TabIndex = 7;
			this.AgreeButton.Text = "OK";
			this.AgreeButton.UseVisualStyleBackColor = true;
			this.AgreeButton.Click += new System.EventHandler(this.AgreeButton_Click);
			// 
			// SeaCargoDeportOutturnBackdoorForSavingOnAmendmentForm
			// 
			this.AcceptButton = this.AgreeButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 243, true);
			this.Controls.Add(this.AgreeButton);
			this.Controls.Add(this.SavingOptionsGroupBox);
			this.Controls.Add(this.OutturnNameLabel);
			this.Controls.Add(this.AmendmentLabel);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.DeferredCusOutturnHeaderSavingOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SeaCargoDeportOutturnBackdoorForSavingOnAmendmentForm";
			this.Text = "SeaCargoDeportOutturnBackdoorForSavingOnAmendmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AmendmentLabel, 0);
			this.Controls.SetChildIndex(this.OutturnNameLabel, 0);
			this.Controls.SetChildIndex(this.SavingOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.AgreeButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SavingOptionsGroupBox.ResumeLayout(false);
			this.SavingOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZRadioButton SaveAndSendRadioButton;
		private ZArchitecture.GUI.ZRadioButton SaveWithoutSendingRadioButton;
		private ZArchitecture.GUI.ZRadioButton CancelRadioButton;
		private ZArchitecture.ZLabel AmendmentLabel;
		private ZArchitecture.ZLabel OutturnNameLabel;
		private ZArchitecture.GUI.ZGroupBox SavingOptionsGroupBox;
		private ZArchitecture.GUI.ZButton AgreeButton;
	}
}
