namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmNotePopupBase
	{
		private void InitializeComponent()
		{
			this.popupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// popupButton
			// 
			this.popupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.popupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 0, true);
			this.popupButton.Name = "popupButton";
			this.popupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 22, true);
			this.popupButton.TabIndex = 1;
			this.popupButton.TabStop = false;
			this.popupButton.UseVisualStyleBackColor = true;
			this.popupButton.Click += new System.EventHandler(this.PopupButton_Click);
			// 
			// ZStmNotePopupBase
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.popupButton);
			this.Name = "ZStmNotePopupBase";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		protected ZButton popupButton;
	}
}
