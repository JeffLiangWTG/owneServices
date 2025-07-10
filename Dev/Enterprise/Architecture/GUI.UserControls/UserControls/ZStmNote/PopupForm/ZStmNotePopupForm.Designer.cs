namespace Enterprise.ZArchitecture.GUI
{
	partial class ZStmNotePopupForm
	{
		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		readonly System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotePopupForm|bd415b60-e0b2-4fe4-ae53-3e4f25aeaa0c", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 346, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ZStmNotePopupForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 377, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotePopupForm|26d70b57-9828-4fc7-bc47-df93efa20927", "Note");
			this.Controls.Add(this.OKButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 230, true);
			this.Name = "ZStmNotePopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
