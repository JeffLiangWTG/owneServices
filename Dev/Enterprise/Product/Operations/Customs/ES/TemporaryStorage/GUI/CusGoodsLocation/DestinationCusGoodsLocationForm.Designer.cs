namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class DestinationCusGoodsLocationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.GoodsLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicGoodsLocationPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsLocationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 24, true);
			// 
			// GoodsLocationGroupBox
			// 
			this.GoodsLocationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GoodsLocationGroupBox.Controls.Add(this.DynamicGoodsLocationPanel);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsLocationGroupBox, false);
			this.GoodsLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsLocationGroupBox.Name = "GoodsLocationGroupBox";
			this.GoodsLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 240, true);
			this.GoodsLocationGroupBox.TabIndex = 1;
			this.GoodsLocationGroupBox.TabStop = false;
			// 
			// DynamicGoodsLocationPanel
			// 
			this.DynamicGoodsLocationPanel.AllowDrop = true;
			this.DynamicGoodsLocationPanel.AutoScroll = true;
			this.DynamicGoodsLocationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicGoodsLocationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicGoodsLocationPanel.Name = "DynamicGoodsLocationPanel";
			this.DynamicGoodsLocationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 221, true);
			this.DynamicGoodsLocationPanel.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("9B318584-FA01-4304-BF55-9CB1F91FDC9D", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 246, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// DestinationCusGoodsLocationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("CD69188A-9E41-4F09-A525-9BBBD3F709FC", "Location of Goods");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 299, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.GoodsLocationGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 200, true);
			this.Name = "DestinationCusGoodsLocationForm";
			this.Controls.SetChildIndex(this.GoodsLocationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsLocationGroupBox.ResumeLayout(false);
			this.GoodsLocationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox GoodsLocationGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicGoodsLocationPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
