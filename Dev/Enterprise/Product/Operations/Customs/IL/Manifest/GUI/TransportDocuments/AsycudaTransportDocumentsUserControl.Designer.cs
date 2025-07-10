namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaTransportDocumentsUserControl
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

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TransportDocumentTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TransportDocumentDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportDocumentDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportDocumentTabControl.SuspendLayout();
			this.TransportDocumentDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// TransportDocumentTabControl
			// 
			this.TransportDocumentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransportDocumentTabControl.Controls.Add(this.TransportDocumentDetailsTabPage);
			this.TransportDocumentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDocumentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDocumentTabControl.Name = "TransportDocumentTabControl";
			this.TransportDocumentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 389, true);
			this.TransportDocumentTabControl.TabIndex = 0;
			// 
			// TransportDocumentDetailsTabPage
			// 
			this.TransportDocumentDetailsTabPage.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("B1160299-325C-4C3D-8AF0-500F584ACA20", "Transport Document Details");
			this.TransportDocumentDetailsTabPage.Controls.Add(this.TransportDocumentDetailsLayoutPanel);
			this.TransportDocumentDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.TransportDocumentDetailsTabPage.Name = "TransportDocumentDetailsTabPage";
			this.TransportDocumentDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1282, 366, true);
			this.TransportDocumentDetailsTabPage.TabIndex = 1;
			// 
			// TransportDocumentDetailsLayoutPanel
			// 
			this.TransportDocumentDetailsLayoutPanel.AllowDrop = true;
			this.TransportDocumentDetailsLayoutPanel.AutoScroll = true;
			this.TransportDocumentDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDocumentDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDocumentDetailsLayoutPanel.Name = "TransportDocumentDetailsLayoutPanel";
			this.TransportDocumentDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1282, 366, true);
			this.TransportDocumentDetailsLayoutPanel.TabIndex = 2;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 431, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.TransportDocumentTabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(225);
			this.SplitContainer.SplitterWidth = 15;
			this.SplitContainer.TabIndex = 21;
			// 
			// AsycudaTransportDocumentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "AsycudaTransportDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportDocumentTabControl.ResumeLayout(false);
			this.TransportDocumentTabControl.PerformLayout();
			this.TransportDocumentDetailsTabPage.ResumeLayout(false);
			this.TransportDocumentDetailsTabPage.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion Component Designer generated code

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl TransportDocumentTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TransportDocumentDetailsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel TransportDocumentDetailsLayoutPanel;
	}
}
