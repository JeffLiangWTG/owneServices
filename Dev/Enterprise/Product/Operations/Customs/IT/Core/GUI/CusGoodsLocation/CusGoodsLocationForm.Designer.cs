namespace Enterprise.Customs.IT.GUI
{
	partial class CusGoodsLocationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ClearFieldsButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("47c77e20-09e4-428a-81c0-e74098108e9b", "Clear Fields");
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 246, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ClearFieldsButton.TabIndex = 2;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.Click += new System.EventHandler(this.ClearFieldsButton_Click);
			// 
			// CusGoodsLocationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 299, true);
			this.Controls.Add(this.ClearFieldsButton);
			this.Name = "CusGoodsLocationForm";
			this.SecurityToken = "Enterprise.Customs.IT.GUI.CusGoodsLocationForm";
			this.Controls.SetChildIndex(this.ClearFieldsButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton ClearFieldsButton;
	}
}
