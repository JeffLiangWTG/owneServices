
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class ConsentGrantingAndOAuth2TokenUserControl
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

			cts?.Cancel();
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnGrant = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.lblMessage = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// btnGrant
			// 
			this.btnGrant.IsCaptionOverridden = true;
			this.btnGrant.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.btnGrant.Name = "btnGrant";
			this.btnGrant.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnGrant.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 33, true);
			this.btnGrant.TabIndex = 0;
			this.btnGrant.Text = Res.GetString("DDFF6143-2F2D-4E00-BA1E-60F235D70488", "Grant Permissions");
			this.btnGrant.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnGrant.ToolTipCaption = null;
			this.btnGrant.UseVisualStyleBackColor = true;
			this.btnGrant.Click += new System.EventHandler(this.btnGrant_Click);
			// 
			// btnClear
			// 
			this.btnClear.IsCaptionOverridden = true;
			this.btnClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 13, true);
			this.btnClear.Name = "btnClear";
			this.btnClear.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 33, true);
			this.btnClear.TabIndex = 1;
			this.btnClear.Text = Res.GetString("7C590ABC-4404-4690-8D65-1BFA17D96EAE", "Clear");
			this.btnClear.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnClear.ToolTipCaption = null;
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// lblMessage
			// 
			this.lblMessage.AutoSize = true;
			this.lblMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 61, true);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 12, true);
			this.lblMessage.TabIndex = 2;
			this.lblMessage.Text = "";
			// 
			// ConsentGrantingAndOAuth2TokenUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnGrant);
			this.Name = "ConsentGrantingAndOAuth2TokenUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton btnGrant;
		private ZButton btnClear;
		internal ZLabel lblMessage;
	}
}
