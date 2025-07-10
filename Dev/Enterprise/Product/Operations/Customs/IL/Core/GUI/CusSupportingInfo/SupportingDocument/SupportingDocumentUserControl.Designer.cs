namespace Enterprise.Customs.IL.GUI
{
	partial class SupportingDocumentUserControl
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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SupportingDocumentTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.SupportingDocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MetaDatTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MetaDataTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentTabControl.SuspendLayout();
			this.SupportingDocumentTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.Panel2.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.MetaDatTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.SupportingDocument);
			// 
			// SupportingDocumentTabControl
			// 
			this.SupportingDocumentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SupportingDocumentTabControl.Controls.Add(this.SupportingDocumentTabPage);
			this.SupportingDocumentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentTabControl.Name = "SupportingDocumentTabControl";
			this.SupportingDocumentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 122, true);
			this.SupportingDocumentTabControl.TabIndex = 0;
			// 
			// SupportingDocumentTabPage
			// 
			this.SupportingDocumentTabPage.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("1E793167-44EC-4DCD-BC3F-52392E90EFB6", "Supporting Document Details");
			this.SupportingDocumentTabPage.Controls.Add(this.SupportingDocumentLayoutPanel);
			this.SupportingDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentTabPage.Name = "SupportingDocumentTabPage";
			this.SupportingDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 95, true);
			this.SupportingDocumentTabPage.TabIndex = 1;
			// 
			// SupportingDocumentLayoutPanel
			// 
			this.SupportingDocumentLayoutPanel.AllowDrop = true;
			this.SupportingDocumentLayoutPanel.AutoScroll = true;
			this.SupportingDocumentLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentLayoutPanel.Name = "SupportingDocumentLayoutPanel";
			this.SupportingDocumentLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 95, true);
			this.SupportingDocumentLayoutPanel.TabIndex = 2;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 403, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.DetailsSplitContainer);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(131);
			this.SplitContainer.SplitterWidth = 15;
			this.SplitContainer.TabIndex = 21;
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			this.DetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.SupportingDocumentTabControl);
			// 
			// DetailsSplitContainer.Panel2
			// 
			this.DetailsSplitContainer.Panel2.Controls.Add(this.MetaDatTabControl);
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 257, true);
			this.DetailsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(122);
			this.DetailsSplitContainer.SplitterWidth = 15;
			this.DetailsSplitContainer.TabIndex = 21;
			// 
			// MetaDatTabControl
			// 
			this.MetaDatTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MetaDatTabControl.Controls.Add(this.MetaDataTabPage);
			this.MetaDatTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MetaDatTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MetaDatTabControl.Name = "MetaDatTabControl";
			this.MetaDatTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 120, true);
			this.MetaDatTabControl.TabIndex = 0;
			// 
			// MetaDataTabPage
			// 
			this.MetaDataTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MetaDataTabPage.Name = "MetaDataTabPage";
			this.MetaDataTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MetaDataTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 93, true);
			this.MetaDataTabPage.TabIndex = 0;
			this.MetaDataTabPage.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("600B634C-87C9-4C77-A01E-433832E1D24C", "Meta Data");
			
			this.MetaDataTabPage.UseVisualStyleBackColor = true;
			// 
			// SupportingDocumentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "SupportingDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentTabControl.ResumeLayout(false);
			this.SupportingDocumentTabControl.PerformLayout();
			this.SupportingDocumentTabPage.ResumeLayout(false);
			this.SupportingDocumentTabPage.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			this.DetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).EndInit();
			this.DetailsSplitContainer.ResumeLayout(false);
			this.DetailsSplitContainer.PerformLayout();
			this.MetaDatTabControl.ResumeLayout(false);
			this.MetaDatTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion Component Designer generated code

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl SupportingDocumentTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentLayoutPanel;
		private CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		private ZArchitecture.GUI.ZTabControl MetaDatTabControl;
		private ZArchitecture.GUI.ZTabPage MetaDataTabPage;
	}
}
