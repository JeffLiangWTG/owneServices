namespace Enterprise.DocumentScanning.GUI
{
	public partial class GraphicalDisplayControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZToolBar ToolBar;
		internal CargoWise.Windows.UI.KPanel ThumbnailPanel;
		private CargoWise.Windows.UI.KPanel ImagePanel;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox DocumentPreviewPictureBox;
		private System.Windows.Forms.ToolBarButton FullScreen;
		private System.Windows.Forms.ToolBarButton RotateRight;
		private System.Windows.Forms.ToolBarButton ToggleThumbnailToolbarButton;
		private System.Windows.Forms.ImageList ImageList;
		private System.Windows.Forms.ToolBarButton Separator2;
		private System.Windows.Forms.ToolBarButton Separator1;
		private CargoWise.Windows.UI.KPanel Panel;
		private System.Windows.Forms.ToolBarButton RotateLeft;
		internal PageSelectorControl PageSelectorControl;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphicalDisplayControl));
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			this.ToolBar = new Enterprise.ZArchitecture.GUI.ZToolBar();
			this.RotateRight = new System.Windows.Forms.ToolBarButton();
			this.RotateLeft = new System.Windows.Forms.ToolBarButton();
			this.Separator2 = new System.Windows.Forms.ToolBarButton();
			this.ToggleThumbnailToolbarButton = new System.Windows.Forms.ToolBarButton();
			this.FullScreen = new System.Windows.Forms.ToolBarButton();
			this.ToggleRulers = new System.Windows.Forms.ToolBarButton();
			this.Separator1 = new System.Windows.Forms.ToolBarButton();
			this.PageSelectorControl = new Enterprise.DocumentScanning.GUI.PageSelectorControl();
			this.Panel = new CargoWise.Windows.UI.KPanel();
			this.ImagePanel = new CargoWise.Windows.UI.KPanel();
			this.rulerPanel = new Enterprise.DocumentScanning.GUI.RulerPanel();
			this.DocumentPreviewPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ThumbnailPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PageSelectorControl.SuspendLayout();
			this.Panel.SuspendLayout();
			this.ImagePanel.SuspendLayout();
			this.rulerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentPreviewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// ImageList
			//
			ZArchitecture.GUI.ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ImageList, ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream"))));
			this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ImageList.Images.SetKeyName(0, "");
			this.ImageList.Images.SetKeyName(1, "");
			this.ImageList.Images.SetKeyName(2, "");
			this.ImageList.Images.SetKeyName(3, "");
			this.ImageList.Images.SetKeyName(4, "");
			this.ImageList.Images.SetKeyName(5, "");
			// 
			// ToolBar
			// 
			this.ToolBar.AutoSize = false;
			this.ToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.RotateRight,
			this.RotateLeft,
			this.Separator2,
			this.ToggleThumbnailToolbarButton,
			this.FullScreen,
			this.ToggleRulers,
			this.Separator1});
			this.ToolBar.ButtonSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 22, true);
			this.ToolBar.DropDownArrows = true;
			this.ToolBar.Font = new System.Drawing.Font("Tahoma", 8F);
			this.ToolBar.ImageList = this.ImageList;
			this.ToolBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolBar.Name = "ToolBar";
			this.ToolBar.ShowToolTips = true;
			this.ToolBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 28, true);
			this.ToolBar.TabIndex = 15;
			this.ToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.ToolBar_ButtonClick);
			// 
			// RotateRight
			// 
			this.RotateRight.ImageIndex = 1;
			this.RotateRight.Name = "RotateRight";
			this.RotateRight.Tag = "RotateRight";
			// 
			// RotateLeft
			// 
			this.RotateLeft.ImageIndex = 0;
			this.RotateLeft.Name = "RotateLeft";
			this.RotateLeft.Tag = "RotateLeft";
			// 
			// Separator2
			// 
			this.Separator2.Name = "Separator2";
			this.Separator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// ToggleThumbnailToolbarButton
			// 
			this.ToggleThumbnailToolbarButton.ImageIndex = 2;
			this.ToggleThumbnailToolbarButton.Name = "ToggleThumbnailToolbarButton";
			this.ToggleThumbnailToolbarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.ToggleThumbnailToolbarButton.Tag = "ToggleThumbnail";
			// 
			// FullScreen
			// 
			this.FullScreen.ImageIndex = 3;
			this.FullScreen.Name = "FullScreen";
			this.FullScreen.Tag = "FullScreen";
			// 
			// ToggleRulers
			// 
			this.ToggleRulers.ImageIndex = 5;
			this.ToggleRulers.Name = "ToggleRulers";
			this.ToggleRulers.Tag = "ToggleRulers";
			// 
			// Separator1
			// 
			this.Separator1.Name = "Separator1";
			this.Separator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// PageSelectorControl
			// 
			this.PageSelectorControl.AllowDrop = true;
			this.PageSelectorControl.Enabled = false;
			this.PageSelectorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 3, true);
			this.PageSelectorControl.Name = "PageSelectorControl";
			this.PageSelectorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 24, true);
			this.PageSelectorControl.TabIndex = 16;
			this.PageSelectorControl.CurrentPageChanged += OnImagePageChanged;

			// 
			// Panel
			// 
			this.Panel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.Panel.Controls.Add(this.ImagePanel);
			this.Panel.Controls.Add(this.ThumbnailPanel);
			this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.Panel.Name = "Panel";
			this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 452, true);
			this.Panel.TabIndex = 17;
			// 
			// ImagePanel
			// 
			this.ImagePanel.Controls.Add(this.rulerPanel);
			this.ImagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImagePanel.Name = "ImagePanel";
			this.ImagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 452, true);
			this.ImagePanel.TabIndex = 11;
			this.ImagePanel.Visible = false;
			// 
			// rulerPanel
			// 
			this.rulerPanel.Controls.Add(this.DocumentPreviewPictureBox);
			this.rulerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rulerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rulerPanel.Name = "rulerPanel";
			this.rulerPanel.ShowLines = true;
			this.rulerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 452, true);
			this.rulerPanel.TabIndex = 1;
			this.rulerPanel.TabStop = true;
			this.rulerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			// 
			// DocumentPreviewPictureBox
			// 
			this.DocumentPreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DocumentPreviewPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 126, true);
			this.DocumentPreviewPictureBox.Name = "DocumentPreviewPictureBox";
			this.DocumentPreviewPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 44, true);
			this.DocumentPreviewPictureBox.TabIndex = 0;
			this.DocumentPreviewPictureBox.TabStop = false;
			// 
			// ThumbnailPanel
			// 
			this.ThumbnailPanel.AutoScroll = true;
			this.ThumbnailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThumbnailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ThumbnailPanel.Name = "ThumbnailPanel";
			this.ThumbnailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 452, true);
			this.ThumbnailPanel.TabIndex = 12;
			this.ThumbnailPanel.Visible = false;
			// 
			// GraphicalDisplayControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Panel);
			this.Controls.Add(this.PageSelectorControl);
			this.Controls.Add(this.ToolBar);
			this.Name = "GraphicalDisplayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 480, true);
			this.Load += new System.EventHandler(this.GraphicalDisplayControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PageSelectorControl.ResumeLayout(true);
			this.PageSelectorControl.PerformLayout();
			this.Panel.ResumeLayout(false);
			this.Panel.PerformLayout();
			this.ImagePanel.ResumeLayout(false);
			this.ImagePanel.PerformLayout();
			this.rulerPanel.ResumeLayout(false);
			this.rulerPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentPreviewPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private RulerPanel rulerPanel;
		private System.Windows.Forms.ToolBarButton ToggleRulers;
	}
}
