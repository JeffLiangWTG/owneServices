namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5TransportAndPackagingTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ContainersAndSealsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContainersAndSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersAndSealsUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5ContainersAndSealsUserControl();
			this.ModeOfTransportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportBorderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportBorderDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransportDepartureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportDepartureDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransportDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportAndPackagingDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransportAndPackingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersAndSealsPanel.SuspendLayout();
			this.ContainersAndSealsGroupBox.SuspendLayout();
			this.ContainersAndSealsUserControl.SuspendLayout();
			this.ModeOfTransportPanel.SuspendLayout();
			this.TransportBorderGroupBox.SuspendLayout();
			this.TransportDepartureGroupBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.TransportAndPackingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// ContainersAndSealsPanel
			// 
			this.ContainersAndSealsPanel.Controls.Add(this.ContainersAndSealsGroupBox);
			this.ContainersAndSealsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.ContainersAndSealsPanel.Name = "ContainersAndSealsPanel";
			this.ContainersAndSealsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 400, true);
			this.ContainersAndSealsPanel.TabIndex = 2;
			// 
			// ContainersAndSealsGroupBox
			// 
			this.ContainersAndSealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("57BBD64E-1B6B-441D-B84B-EC99EAB94B21", "Containers/Equipment and Seals");
			this.ContainersAndSealsGroupBox.Controls.Add(this.ContainersAndSealsUserControl);
			this.ContainersAndSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersAndSealsGroupBox.Name = "ContainersAndSealsGroupBox";
			this.ContainersAndSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 400, true);
			this.ContainersAndSealsGroupBox.TabIndex = 0;
			this.ContainersAndSealsGroupBox.TabStop = false;
			// 
			// ContainersAndSealsUserControl
			// 
			this.ContainersAndSealsUserControl.AllowDrop = true;
			this.ContainersAndSealsUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.ContainersAndSealsUserControl, ".");
			this.ContainersAndSealsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersAndSealsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 205, true);
			this.ContainersAndSealsUserControl.Name = "ContainersAndSealsUserControl";
			this.ContainersAndSealsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 381, true);
			this.ContainersAndSealsUserControl.TabIndex = 1;
			// 
			// ModeOfTransportPanel
			// 
			this.ModeOfTransportPanel.Controls.Add(this.TransportBorderGroupBox);
			this.ModeOfTransportPanel.Controls.Add(this.TransportDepartureGroupBox);
			this.ModeOfTransportPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ModeOfTransportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModeOfTransportPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 140, true);
			this.ModeOfTransportPanel.Name = "ModeOfTransportPanel";
			this.ModeOfTransportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 166, true);
			this.ModeOfTransportPanel.TabIndex = 0;
			// 
			// TransportBorderGroupBox
			// 
			this.TransportBorderGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("252cad25-dd84-410d-af0e-5e7aa7a81c3d", "Transport Border");
			this.TransportBorderGroupBox.Controls.Add(this.TransportBorderDynamicLayoutPanel);
			this.TransportBorderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 0, true);
			this.TransportBorderGroupBox.Name = "TransportBorderGroupBox";
			this.TransportBorderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 163, true);
			this.TransportBorderGroupBox.TabIndex = 1;
			this.TransportBorderGroupBox.TabStop = false;
			// 
			// TransportBorderDynamicLayoutPanel
			// 
			this.TransportBorderDynamicLayoutPanel.AllowDrop = true;
			this.TransportBorderDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportBorderDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportBorderDynamicLayoutPanel.Name = "TransportBorderDynamicLayoutPanel";
			this.TransportBorderDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 144, true);
			this.TransportBorderDynamicLayoutPanel.TabIndex = 0;
			// 
			// TransportDepartureGroupBox
			// 
			this.TransportDepartureGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7e9b6ae3-c8f3-4a83-b3ad-8e1edae4606a", "Transport Departure");
			this.TransportDepartureGroupBox.Controls.Add(this.TransportDepartureDynamicLayoutPanel);
			this.TransportDepartureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDepartureGroupBox.Name = "TransportDepartureGroupBox";
			this.TransportDepartureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 163, true);
			this.TransportDepartureGroupBox.TabIndex = 0;
			this.TransportDepartureGroupBox.TabStop = false;
			// 
			// TransportDepartureDynamicLayoutPanel
			// 
			this.TransportDepartureDynamicLayoutPanel.AllowDrop = true;
			this.TransportDepartureDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDepartureDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportDepartureDynamicLayoutPanel.Name = "TransportDepartureDynamicLayoutPanel";
			this.TransportDepartureDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 144, true);
			this.TransportDepartureDynamicLayoutPanel.TabIndex = 0;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("54bcd129-67e0-4ec2-903d-6810bdf096fe", "Transport Details");
			this.TransportDetailsGroupBox.Controls.Add(this.TransportAndPackagingDynamicLayoutPanel);
			this.TransportDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDetailsGroupBox.Name = "TransportDetailsGroupBox";
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 154, true);
			this.TransportDetailsGroupBox.TabIndex = 0;
			this.TransportDetailsGroupBox.TabStop = false;
			// 
			// TransportAndPackagingDynamicLayoutPanel
			// 
			this.TransportAndPackagingDynamicLayoutPanel.AllowDrop = true;
			this.TransportAndPackagingDynamicLayoutPanel.AutoScroll = true;
			this.TransportAndPackagingDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportAndPackagingDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportAndPackagingDynamicLayoutPanel.Name = "TransportAndPackagingDynamicLayoutPanel";
			this.TransportAndPackagingDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 135, true);
			this.TransportAndPackagingDynamicLayoutPanel.TabIndex = 0;
			// 
			// TransportAndPackingPanel
			// 
			this.TransportAndPackingPanel.Controls.Add(this.TransportDetailsGroupBox);
			this.TransportAndPackingPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TransportAndPackingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
			this.TransportAndPackingPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 100, true);
			this.TransportAndPackingPanel.Name = "TransportAndPackingPanel";
			this.TransportAndPackingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 154, true);
			this.TransportAndPackingPanel.TabIndex = 1;
			// 
			// Phase5TransportAndPackagingTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersAndSealsPanel);
			this.Controls.Add(this.TransportAndPackingPanel);
			this.Controls.Add(this.ModeOfTransportPanel);
			this.Name = "Phase5TransportAndPackagingTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 720, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersAndSealsPanel.ResumeLayout(false);
			this.ContainersAndSealsPanel.PerformLayout();
			this.ContainersAndSealsGroupBox.ResumeLayout(false);
			this.ContainersAndSealsGroupBox.PerformLayout();
			this.ContainersAndSealsUserControl.ResumeLayout(true);
			this.ContainersAndSealsUserControl.PerformLayout();
			this.ModeOfTransportPanel.ResumeLayout(false);
			this.ModeOfTransportPanel.PerformLayout();
			this.TransportBorderGroupBox.ResumeLayout(false);
			this.TransportBorderGroupBox.PerformLayout();
			this.TransportDepartureGroupBox.ResumeLayout(false);
			this.TransportDepartureGroupBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.TransportAndPackingPanel.ResumeLayout(false);
			this.TransportAndPackingPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZPanel ModeOfTransportPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel TransportAndPackingPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel ContainersAndSealsPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportDepartureGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportDepartureDynamicLayoutPanel;
		protected internal ZArchitecture.GUI.ZGroupBox TransportBorderGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportBorderDynamicLayoutPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportAndPackagingDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportDetailsGroupBox;
		internal Phase5ContainersAndSealsUserControl ContainersAndSealsUserControl;
		internal ZArchitecture.GUI.ZGroupBox ContainersAndSealsGroupBox;
	}
}

