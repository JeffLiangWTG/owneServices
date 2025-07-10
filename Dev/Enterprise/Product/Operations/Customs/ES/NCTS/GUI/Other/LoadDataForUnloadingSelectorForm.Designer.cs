namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class LoadDataForUnloadingSelectorForm
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
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DepartureRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.CustomsQueryRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ExtraMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 145, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 24, true);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("72FCF6ED-1901-4FFB-A59E-6EAA83285287", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 116, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("57F3B174-9B05-4960-85BF-B8B4AD85C255", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 116, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 8, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 26, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.MessageLabel.UseMnemonic = false;
			// 
			// DepartureRadioButton
			// 
			this.DepartureRadioButton.AutoSize = true;
			this.DepartureRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.DepartureRadioButton.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("666F2AC9-7993-4AA8-8C56-15FD73CD8AB4", "Departure");
			this.DepartureRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 41, true);
			this.DepartureRadioButton.Name = "DepartureRadioButton";
			this.DepartureRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.DepartureRadioButton.TabIndex = 0;
			this.DepartureRadioButton.TabStop = true;
			this.DepartureRadioButton.UseVisualStyleBackColor = false;
			// 
			// CustomsQueryRadioButton
			// 
			this.CustomsQueryRadioButton.AutoCheck = false;
			this.CustomsQueryRadioButton.AutoSize = true;
			this.CustomsQueryRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.CustomsQueryRadioButton.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("CA32C9E5-AD54-487E-899B-12318B22DE5F", "Customs Query");
			this.CustomsQueryRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 64, true);
			this.CustomsQueryRadioButton.Name = "CustomsQueryRadioButton";
			this.CustomsQueryRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.CustomsQueryRadioButton.TabIndex = 1;
			this.CustomsQueryRadioButton.TabStop = true;
			this.CustomsQueryRadioButton.UseVisualStyleBackColor = false;
			// 
			// ExtraMessageLabel
			// 
			this.ExtraMessageLabel.AutoSize = true;
			this.ExtraMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExtraMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 92, true);
			this.ExtraMessageLabel.Name = "ExtraMessageLabel";
			this.ExtraMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 13, true);
			this.ExtraMessageLabel.TabIndex = 0;
			this.ExtraMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ExtraMessageLabel.UseMnemonic = false;
			// 
			// LoadDataForUnloadingSelectorForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("28D28164-5BB3-4DDB-9D64-E50A24DFE56D", "Load Data For Unloading");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 169, true);
			this.Controls.Add(this.ExtraMessageLabel);
			this.Controls.Add(this.CustomsQueryRadioButton);
			this.Controls.Add(this.DepartureRadioButton);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "LoadDataForUnloadingSelectorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.DepartureRadioButton, 0);
			this.Controls.SetChildIndex(this.CustomsQueryRadioButton, 0);
			this.Controls.SetChildIndex(this.ExtraMessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.ZLabel MessageLabel;
		ZArchitecture.GUI.ZRadioButton CustomsQueryRadioButton;
		internal ZArchitecture.GUI.ZRadioButton DepartureRadioButton;
		ZArchitecture.ZLabel ExtraMessageLabel;
		ZArchitecture.GUI.ZButton OKButton;
		ZArchitecture.GUI.ZButton Cancel_Button;
	}
}
