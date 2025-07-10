namespace Enterprise.Customs.EU.GUI
{
	partial class PlaceOfUseOrProcessingForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.PlaceOfUseOrProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicPlaceOfUseOrProcessingPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfUseOrProcessingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 24, true);
			// 
			// PlaceOfUseOrProcessingGroupBox
			// 
			this.PlaceOfUseOrProcessingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PlaceOfUseOrProcessingGroupBox.Controls.Add(this.DynamicPlaceOfUseOrProcessingPanel);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfUseOrProcessingGroupBox, false);
			this.PlaceOfUseOrProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfUseOrProcessingGroupBox.Name = "PlaceOfUseOrProcessingGroupBox";
			this.PlaceOfUseOrProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 240, true);
			this.PlaceOfUseOrProcessingGroupBox.TabIndex = 1;
			this.PlaceOfUseOrProcessingGroupBox.TabStop = false;
			// 
			// DynamicPlaceOfUseOrProcessingPanel
			// 
			this.DynamicPlaceOfUseOrProcessingPanel.AllowDrop = true;
			this.DynamicPlaceOfUseOrProcessingPanel.AutoScroll = true;
			this.DynamicPlaceOfUseOrProcessingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPlaceOfUseOrProcessingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicPlaceOfUseOrProcessingPanel.Name = "DynamicPlaceOfUseOrProcessingPanel";
			this.DynamicPlaceOfUseOrProcessingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 221, true);
			this.DynamicPlaceOfUseOrProcessingPanel.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("B2F9C2F9-B88C-43DA-821D-64BE65D984A8", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 246, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// PlaceOfUseOrProcessingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("881F1949-D8B2-4146-AA32-AFD1FF09B158", "Place of Use or Processing");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 299, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.PlaceOfUseOrProcessingGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 200, true);
			this.Name = "PlaceOfUseOrProcessingForm";
			this.Controls.SetChildIndex(this.PlaceOfUseOrProcessingGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfUseOrProcessingGroupBox.ResumeLayout(false);
			this.PlaceOfUseOrProcessingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox PlaceOfUseOrProcessingGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicPlaceOfUseOrProcessingPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
