using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZKeyStatusForm
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
		void InitializeComponent()
		{
			this.zButtonCaps = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCtrl = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonAlt = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonShift = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonReset = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zButtonCaps
			// 
			this.zButtonCaps.IsCaptionOverridden = true;
			this.zButtonCaps.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 29, true);
			this.zButtonCaps.Name = "zButtonCaps";
			this.zButtonCaps.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonCaps.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 41, true);
			this.zButtonCaps.TabIndex = 0;
			this.zButtonCaps.Text = Enterprise.ZArchitecture.GUI.Res.GetString("32F8A4D1-D7D7-455A-8353-CFF9A449C89E", "Caps");
			this.zButtonCaps.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonCaps.ToolTipCaption = null;
			this.zButtonCaps.UseVisualStyleBackColor = true;
			this.zButtonCaps.ReadOnly = true;
			// 
			// zButtonCtrl
			// 
			this.zButtonCtrl.IsCaptionOverridden = true;
			this.zButtonCtrl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 29, true);
			this.zButtonCtrl.Name = "zButtonCtrl";
			this.zButtonCtrl.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonCtrl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 41, true);
			this.zButtonCtrl.TabIndex = 1;
			this.zButtonCtrl.Text = Enterprise.ZArchitecture.GUI.Res.GetString("10EA2419-3F71-49AA-83DA-156C8F99AAE2", "Ctrl");
			this.zButtonCtrl.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonCtrl.ToolTipCaption = null;
			this.zButtonCtrl.UseVisualStyleBackColor = true;
			this.zButtonCtrl.ReadOnly = true;
			// 
			// zButtonAlt
			// 
			this.zButtonAlt.IsCaptionOverridden = true;
			this.zButtonAlt.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 29, true);
			this.zButtonAlt.Name = "zButtonAlt";
			this.zButtonAlt.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonAlt.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 41, true);
			this.zButtonAlt.TabIndex = 2;
			this.zButtonAlt.Text = Enterprise.ZArchitecture.GUI.Res.GetString("414A7A2A-2DBC-4CE3-BF8D-D451EDE54C87", "Alt");
			this.zButtonAlt.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonAlt.ToolTipCaption = null;
			this.zButtonAlt.UseVisualStyleBackColor = true;
			this.zButtonAlt.ReadOnly = true;
			// 
			// zButtonShift
			// 
			this.zButtonShift.IsCaptionOverridden = true;
			this.zButtonShift.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 29, true);
			this.zButtonShift.Name = "zButtonShift";
			this.zButtonShift.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonShift.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 41, true);
			this.zButtonShift.TabIndex = 3;
			this.zButtonShift.Text = Enterprise.ZArchitecture.GUI.Res.GetString("8FD029F5-50D6-42F3-9390-FB2BE92A3FA9", "Shift");
			this.zButtonShift.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonShift.ToolTipCaption = null;
			this.zButtonShift.UseVisualStyleBackColor = true;
			this.zButtonShift.ReadOnly = true;
			// 
			// zButtonReset
			// 
			this.zButtonReset.IsCaptionOverridden = true;
			this.zButtonReset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 79, true);
			this.zButtonReset.Name = "zButtonReset";
			this.zButtonReset.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonReset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 41, true);
			this.zButtonReset.TabIndex = 4;
			this.zButtonReset.Text = Enterprise.ZArchitecture.GUI.Res.GetString("F4EED07C-1E81-4D86-A838-B262B11BDE3B", "Reset All");
			this.zButtonReset.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonReset.ToolTipCaption = null;
			this.zButtonReset.UseVisualStyleBackColor = true;
			// 
			// ZKeyStatusForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 158, true);
			this.Controls.Add(this.zButtonReset);
			this.Controls.Add(this.zButtonShift);
			this.Controls.Add(this.zButtonAlt);
			this.Controls.Add(this.zButtonCtrl);
			this.Controls.Add(this.zButtonCaps);
			this.Name = "ZKeyStatusForm";
			this.Text = Res.GetString("4E911BE3-62ED-4826-B1C1-D1C7B36AD6C7", "Key Status Form");
			this.Controls.SetChildIndex(this.zButtonCaps, 0);
			this.Controls.SetChildIndex(this.zButtonCtrl, 0);
			this.Controls.SetChildIndex(this.zButtonAlt, 0);
			this.Controls.SetChildIndex(this.zButtonShift, 0);
			this.Controls.SetChildIndex(this.zButtonReset, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton zButtonCaps;
		private ZButton zButtonCtrl;
		private ZButton zButtonAlt;
		private ZButton zButtonShift;
		private ZButton zButtonReset;
	}
}