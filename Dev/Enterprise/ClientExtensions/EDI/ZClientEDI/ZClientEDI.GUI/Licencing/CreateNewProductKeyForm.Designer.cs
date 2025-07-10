namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class CreateNewProductKeyForm
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
		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ServerCodeCriteriaLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreferredServerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("167e2069-4ca1-4712-8cf9-388e770bbff6", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 113, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OkButton
			// 
			this.OkButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("312b6300-11c3-47e8-af46-b0bc1b80b9eb", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 113, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 4;
			this.OkButton.Text = "OK";
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// ServerCodeCriteriaLabel
			// 
			this.ServerCodeCriteriaLabel.AutoSize = true;
			this.ServerCodeCriteriaLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9214521d-598c-42e8-98aa-63bd3a414c12", "Criteria: Code must be three characters beginning with a letter and containing only letter and numbers.");
			this.ServerCodeCriteriaLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServerCodeCriteriaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 18, true);
			this.ServerCodeCriteriaLabel.Name = "ServerCodeCriteriaLabel";
			this.ServerCodeCriteriaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 13, true);
			this.ServerCodeCriteriaLabel.TabIndex = 1;
			this.ServerCodeCriteriaLabel.Text = "Criteria: Code must be three characters beginning with a letter and containing on" +
    "ly letter and numbers.";
			// 
			// EnterpriseCodeTextBox
			// 
			this.EnterpriseCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bed320af-3f5c-4499-a65c-650542004a61", "Enterprise Code:");
			this.EnterpriseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 50, true);
			this.EnterpriseCodeTextBox.Name = "EnterpriseCodeTextBox";
			this.EnterpriseCodeTextBox.ReadOnly = true;
			this.EnterpriseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.EnterpriseCodeTextBox.TabIndex = 2;
			this.EnterpriseCodeTextBox.Text = "WTL";
			this.EnterpriseCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// PreferredServerCodeTextBox
			// 
			this.PreferredServerCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("59ae6f09-b9cc-4eee-a608-0363c9daf429", "Preferred Server Code:");
			this.PreferredServerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 76, true);
			this.PreferredServerCodeTextBox.Name = "PreferredServerCodeTextBox";
			this.PreferredServerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.PreferredServerCodeTextBox.TabIndex = 3;
			this.PreferredServerCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.PreferredServerCodeTextBox.EnterPressedLeavingControl += new System.EventHandler(this.PreferredServerCodeTextBox_EnterPressedLeavingControl);
			// 
			// CreateNewProductKeyForm
			// 
			this.AcceptButton = this.OkButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8c6158f7-724c-43f6-8512-1369758b4229", "Create New Key");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 155, true);
			this.Controls.Add(this.PreferredServerCodeTextBox);
			this.Controls.Add(this.EnterpriseCodeTextBox);
			this.Controls.Add(this.ServerCodeCriteriaLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OkButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "CreateNewProductKeyForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ServerCodeCriteriaLabel, 0);
			this.Controls.SetChildIndex(this.EnterpriseCodeTextBox, 0);
			this.Controls.SetChildIndex(this.PreferredServerCodeTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZButton OkButton;
		private ZArchitecture.ZTextBox EnterpriseCodeTextBox;
		protected ZArchitecture.ZTextBox PreferredServerCodeTextBox;
		private ZArchitecture.ZLabel ServerCodeCriteriaLabel;
	}
}