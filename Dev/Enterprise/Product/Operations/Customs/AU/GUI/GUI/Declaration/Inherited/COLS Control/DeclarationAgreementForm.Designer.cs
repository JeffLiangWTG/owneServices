namespace Enterprise.Customs.AU.GUI
{
	partial class DeclarationAgreementForm
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
			this.DeclarationAgreementControl = new Enterprise.Customs.AU.GUI.DeclarationAgreementUserControl();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationAgreementControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 513, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance);
			// 
			// DeclarationAgreementControl
			// 
			this.DeclarationAgreementControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationAgreementControl, ".");
			this.DeclarationAgreementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.DeclarationAgreementControl.Name = "DeclarationAgreementControl";
			this.DeclarationAgreementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 473, true);
			this.DeclarationAgreementControl.TabIndex = 1;
			// 
			// SendButton
			// 
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 482, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "Send";
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 482, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// DeclarationAgreementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 537, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.DeclarationAgreementControl);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 572, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 572, true);
			this.Name = "DeclarationAgreementForm";
			this.Controls.SetChildIndex(this.DeclarationAgreementControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationAgreementControl.ResumeLayout(true);
			this.DeclarationAgreementControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DeclarationAgreementUserControl DeclarationAgreementControl;
		internal ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
