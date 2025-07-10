using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class HeaderDetailsUserControl
	{
		void InitializeComponent()
		{
			this.HeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicHeaderDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.HeaderDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bottomPanel = new CargoWise.Windows.UI.KPanel();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderDetailsPanel.SuspendLayout();
			this.HeaderDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusReconDeclaration);
			// 
			// HeaderDetailsPanel
			// 
			this.HeaderDetailsPanel.Controls.Add(this.HeaderDetailsGroupBox);
			this.HeaderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsPanel.Name = "OrderLinesGridAndAdditionalDataPanel";
			this.HeaderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 230, true);
			this.HeaderDetailsPanel.TabIndex = 0;
			// 
			// HeaderDetailsGroupBox
			// 
			this.HeaderDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5D2873E8-9532-4C63-8D03-EDEB39B0FCB3", "Header Details");
			this.HeaderDetailsGroupBox.Controls.Add(this.DynamicHeaderDetailsPanel);
			this.HeaderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsGroupBox.Name = "HeaderDetailsGroupBox";
			this.HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 230, true);
			this.HeaderDetailsGroupBox.TabIndex = 0;
			this.HeaderDetailsGroupBox.TabStop = false;
			// 
			// DynamicHeaderDetailsPanel
			// 
			this.DynamicHeaderDetailsPanel.AllowDrop = true;
			this.DynamicHeaderDetailsPanel.AutoScroll = true;
			this.DynamicHeaderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicHeaderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicHeaderDetailsPanel.Name = "DynamicHeaderDetailsPanel";
			this.DynamicHeaderDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicHeaderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1251, 211, true);
			this.DynamicHeaderDetailsPanel.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 229, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 6, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// bottomPanel
			//
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 358, true);
			this.bottomPanel.TabIndex = 0;
			// 
			// HeaderDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HeaderDetailsPanel);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.bottomPanel);
			this.Name = "HeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 626, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderDetailsPanel.ResumeLayout(false);
			this.HeaderDetailsPanel.PerformLayout();
			this.HeaderDetailsGroupBox.ResumeLayout(false);
			this.HeaderDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZGroupBox HeaderDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicHeaderDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZUserControl FSimplifiedDeclarationFilterUserControl;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal CargoWise.Windows.UI.KPanel bottomPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel HeaderDetailsPanel;
		internal CargoWise.Windows.UI.KSplitter splitter1;
	}
}
