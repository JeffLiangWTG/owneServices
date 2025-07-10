using System;

namespace Enterprise.DocumentEngine.GUI
{
	partial class XLSPreviewForm
	{
		#region Windows Form Designer generated code

		CargoWise.Windows.UI.KPanel Separator3Panel;
		Enterprise.ZArchitecture.ZLabel PagesLabel;
		CargoWise.Windows.UI.KPanel SeparatorPanel;
		CargoWise.Windows.UI.KPanel DocumentPanel;
		CargoWise.Windows.UI.KPanel Separator2Panel;
		Enterprise.ZArchitecture.ZLabel DocumentLabel;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom10MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom25MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom50MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom75MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom100MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom125MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom200MenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem Zoom400MenuItem;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal CargoWise.Windows.UI.KPanel ThumbsPanel;
		private Enterprise.DocumentEngine.GUI.FlexCelPreviewWithCulture PreviewThumbs;
		private CargoWise.Windows.UI.KPanel SheetsPanel;
		private CargoWise.Windows.UI.KSplitter SheetsSplitter;
		internal CargoWise.Windows.UI.KListBox SheetsListBox;
		private CargoWise.Windows.UI.KPanel SheetCaptionPanel;
		internal Enterprise.DocumentEngine.GUI.FlexCelPreviewWithCulture PreviewMain;
#if WINZOR
internal FlexCel.Render.FlexCelSVGExport
#else
		internal FlexCel.Render.FlexCelImgExport
#endif
			flexCelImgProducer;
		private Enterprise.ZArchitecture.ZLabel ZoomLabel;
		private CargoWise.Windows.UI.KTextBox CurrentPageTextBox;
		internal CargoWise.Windows.UI.KButton GoToLastPageButton;
		internal CargoWise.Windows.UI.KButton GoToNextPageButton;
		internal CargoWise.Windows.UI.KButton GoToPrevPageButton;
		internal CargoWise.Windows.UI.KButton GoToFirstPageButton;
		private CargoWise.Windows.UI.KPanel PreviewPanel;
		internal CargoWise.Windows.UI.KNumericUpDown ZoomUpDown;
		internal Enterprise.ZArchitecture.GUI.ZButton OpenInExcelButton;
		protected Enterprise.ZArchitecture.GUI.ZButton SaveAsButton;
		private System.ComponentModel.Container components = null;
		private CargoWise.Windows.UI.KPanel ToolbarPanel;
		private CargoWise.Windows.UI.KPanel buttonPanel;
		private CargoWise.Windows.UI.KPanel PreviewMainPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton DeliverButton;
		private System.Windows.Forms.ContextMenu DeliverMenu;
		internal CargoWise.Windows.UI.KButton ZoomPresetButton;
		private System.Windows.Forms.ContextMenu ZoomMenu;
		internal CargoWise.Windows.UI.KButton HidThumbsButton;
		internal CargoWise.Windows.UI.KSplitter ThumbSplitter;
		CargoWise.Windows.UI.KPanel PagesPanel;

		private new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XLSPreviewForm));
			this.ZoomLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentPageTextBox = new CargoWise.Windows.UI.KTextBox();
			this.GoToLastPageButton = new CargoWise.Windows.UI.KButton();
			this.GoToNextPageButton = new CargoWise.Windows.UI.KButton();
			this.GoToPrevPageButton = new CargoWise.Windows.UI.KButton();
			this.GoToFirstPageButton = new CargoWise.Windows.UI.KButton();
			this.ToolbarPanel = new CargoWise.Windows.UI.KPanel();
			this.ZoomPresetButton = new CargoWise.Windows.UI.KButton();
			this.buttonPanel = new CargoWise.Windows.UI.KPanel();
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenInExcelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveAsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ZoomUpDown = new CargoWise.Windows.UI.KNumericUpDown();
			this.PreviewPanel = new CargoWise.Windows.UI.KPanel();
			this.PreviewMainPanel = new CargoWise.Windows.UI.KPanel();
			this.PreviewMain = new Enterprise.DocumentEngine.GUI.FlexCelPreviewWithCulture();
			this.flexCelImgProducer = new
#if WINZOR
			FlexCel.Render.FlexCelSVGExport();
#else
			FlexCel.Render.FlexCelImgExport();
#endif
			this.PreviewThumbs = new Enterprise.DocumentEngine.GUI.FlexCelPreviewWithCulture();
			this.ThumbSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ThumbsPanel = new CargoWise.Windows.UI.KPanel();
			this.Separator3Panel = new CargoWise.Windows.UI.KPanel();
			this.PagesPanel = new CargoWise.Windows.UI.KPanel();
			this.PagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SeparatorPanel = new CargoWise.Windows.UI.KPanel();
			this.SheetsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.SheetsPanel = new CargoWise.Windows.UI.KPanel();
			this.DocumentPanel = new CargoWise.Windows.UI.KPanel();
			this.Separator2Panel = new CargoWise.Windows.UI.KPanel();
			this.SheetsListBox = new CargoWise.Windows.UI.KListBox();
			this.SheetCaptionPanel = new CargoWise.Windows.UI.KPanel();
			this.HidThumbsButton = new CargoWise.Windows.UI.KButton();
			this.DocumentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliverMenu = new System.Windows.Forms.ContextMenu();
			this.ZoomMenu = new System.Windows.Forms.ContextMenu();
			this.Zoom10MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom25MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom50MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom75MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom100MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom125MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom200MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.Zoom400MenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToolbarPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoomUpDown)).BeginInit();
			this.PreviewPanel.SuspendLayout();
			this.PreviewMainPanel.SuspendLayout();
			this.ThumbsPanel.SuspendLayout();
			this.PagesPanel.SuspendLayout();
			this.SheetsPanel.SuspendLayout();
			this.SheetCaptionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 619, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(415);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// ZoomLabel
			// 
			this.ZoomLabel.BackColor = System.Drawing.SystemColors.Control;
			this.ZoomLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ZoomLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.ZoomLabel.Name = "ZoomLabel";
			this.ZoomLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 19, true);
			this.ZoomLabel.TabIndex = 7;
			this.ZoomLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|3af7b80d-1d28-4f7b-b48a-9e08c632d0a0", "Zoom");
			this.ZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CurrentPageTextBox
			// 
			this.CurrentPageTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CurrentPageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 4, true);
			this.CurrentPageTextBox.Name = "CurrentPageTextBox";
			this.CurrentPageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 21, true);
			this.CurrentPageTextBox.TabIndex = 2;
			this.CurrentPageTextBox.Text = "1 of 1 (document 100 of 100)";
			this.CurrentPageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.CurrentPageTextBox.Leave += new System.EventHandler(this.CurrentPageTextBox_Leave);
			this.CurrentPageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CurrentPageTextBox_KeyPress);
			this.CurrentPageTextBox.Enter += new System.EventHandler(this.CurrentPageTextBox_Enter);
			// 
			// GoToLastPageButton
			// 
			this.GoToLastPageButton.BackColor = System.Drawing.SystemColors.Control;
			this.GoToLastPageButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GoToLastPageButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GoToLastPageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 4, true);
			this.GoToLastPageButton.Name = "GoToLastPageButton";
			this.GoToLastPageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 19, true);
			this.GoToLastPageButton.TabIndex = 5;
			this.GoToLastPageButton.TabStop = false;
			this.GoToLastPageButton.Text = ">>";
			this.GoToLastPageButton.UseVisualStyleBackColor = false;
			this.GoToLastPageButton.Click += new System.EventHandler(this.GoToLastPageButton_Click);
			// 
			// GoToNextPageButton
			// 
			this.GoToNextPageButton.BackColor = System.Drawing.SystemColors.Control;
			this.GoToNextPageButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GoToNextPageButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GoToNextPageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 4, true);
			this.GoToNextPageButton.Name = "GoToNextPageButton";
			this.GoToNextPageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 19, true);
			this.GoToNextPageButton.TabIndex = 4;
			this.GoToNextPageButton.TabStop = false;
			this.GoToNextPageButton.Text = ">";
			this.GoToNextPageButton.UseVisualStyleBackColor = false;
			this.GoToNextPageButton.Click += new System.EventHandler(this.GoToNextPageButton_Click);
			// 
			// GoToPrevPageButton
			// 
			this.GoToPrevPageButton.BackColor = System.Drawing.SystemColors.Control;
			this.GoToPrevPageButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GoToPrevPageButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GoToPrevPageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 4, true);
			this.GoToPrevPageButton.Name = "GoToPrevPageButton";
			this.GoToPrevPageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 19, true);
			this.GoToPrevPageButton.TabIndex = 3;
			this.GoToPrevPageButton.TabStop = false;
			this.GoToPrevPageButton.Text = "<";
			this.GoToPrevPageButton.UseVisualStyleBackColor = false;
			this.GoToPrevPageButton.Click += new System.EventHandler(this.GoToPrevPageButton_Click);
			// 
			// GoToFirstPageButton
			// 
			this.GoToFirstPageButton.BackColor = System.Drawing.SystemColors.Control;
			this.GoToFirstPageButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GoToFirstPageButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GoToFirstPageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 4, true);
			this.GoToFirstPageButton.Name = "GoToFirstPageButton";
			this.GoToFirstPageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 19, true);
			this.GoToFirstPageButton.TabIndex = 2;
			this.GoToFirstPageButton.TabStop = false;
			this.GoToFirstPageButton.Text = "<<";
			this.GoToFirstPageButton.UseVisualStyleBackColor = false;
			this.GoToFirstPageButton.Click += new System.EventHandler(this.GoToFirstPageButton_Click);
			// 
			// ToolbarPanel
			// 
			this.ToolbarPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ToolbarPanel.Controls.Add(this.ZoomPresetButton);
			this.ToolbarPanel.Controls.Add(this.buttonPanel);
			this.ToolbarPanel.Controls.Add(this.ZoomUpDown);
			this.ToolbarPanel.Controls.Add(this.ZoomLabel);
			this.ToolbarPanel.Controls.Add(this.CurrentPageTextBox);
			this.ToolbarPanel.Controls.Add(this.GoToLastPageButton);
			this.ToolbarPanel.Controls.Add(this.GoToNextPageButton);
			this.ToolbarPanel.Controls.Add(this.GoToPrevPageButton);
			this.ToolbarPanel.Controls.Add(this.GoToFirstPageButton);
			this.ToolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolbarPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ToolbarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolbarPanel.Name = "ToolbarPanel";
			this.ToolbarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 30, true);
			this.ToolbarPanel.TabIndex = 2;
			// 
			// ZoomPresetButton
			// 
			this.ZoomPresetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 5, true);
			this.ZoomPresetButton.Name = "ZoomPresetButton";
			this.ZoomPresetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 16, true);
			this.ZoomPresetButton.TabIndex = 13;
			this.ZoomPresetButton.Click += new System.EventHandler(this.ZoomPresetButton_Click);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.buttonPanel.Controls.Add(this.DeliverButton);
			this.buttonPanel.Controls.Add(this.OpenInExcelButton);
			this.buttonPanel.Controls.Add(this.SaveAsButton);
			this.buttonPanel.Controls.Add(this.CloseButton);
			this.buttonPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 4, true);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 21, true);
			this.buttonPanel.TabIndex = 2;
			// 
			// DeliverButton
			// 
			this.DeliverButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 0, true);
			this.DeliverButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.DeliverButton.TabIndex = 9;
			this.DeliverButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|4377e86b-7bd1-47d4-8d44-fae75861d638", "Deliver");
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// OpenInExcelButton
			// 
			this.OpenInExcelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenInExcelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 0, true);
			this.OpenInExcelButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.OpenInExcelButton.Name = "OpenInExcelButton";
			this.OpenInExcelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.OpenInExcelButton.TabIndex = 11;
			this.OpenInExcelButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|bfda939a-9b55-41a8-a788-e8cd98e470f6", "Open in Excel");
			this.OpenInExcelButton.Click += new System.EventHandler(this.OpenInExcelButton_Click);
			// 
			// SaveAsButton
			// 
			this.SaveAsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 0, true);
			this.SaveAsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveAsButton.Name = "SaveAsButton";
			this.SaveAsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.SaveAsButton.TabIndex = 11;
			this.SaveAsButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|13E2C3CF-5152-41CE-A07B-CD797E5E387A", "&Save As...");
			this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 0, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|5e49fd03-16aa-4a33-a2a7-e12ec38ec715", "Close");
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ZoomUpDown
			// 
			this.ZoomUpDown.Increment = new decimal(new int[] {
			5,
			0,
			0,
			0});
			this.ZoomUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 4, true);
			this.ZoomUpDown.Maximum = new decimal(new int[] {
			400,
			0,
			0,
			0});
			this.ZoomUpDown.Minimum = new decimal(new int[] {
			10,
			0,
			0,
			0});
			this.ZoomUpDown.Name = "ZoomUpDown";
			this.ZoomUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.ZoomUpDown.TabIndex = 0;
			this.ZoomUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.ZoomUpDown.Value = new decimal(new int[] {
			75,
			0,
			0,
			0});
			this.ZoomUpDown.ValueChanged += new System.EventHandler(this.ZoomUpDown_ValueChanged);
			// 
			// PreviewPanel
			// 
			this.PreviewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewPanel.Controls.Add(this.PreviewMainPanel);
			this.PreviewPanel.Controls.Add(this.ThumbSplitter);
			this.PreviewPanel.Controls.Add(this.ThumbsPanel);
			this.PreviewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.PreviewPanel.Name = "PreviewPanel";
			this.PreviewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 617, true);
			this.PreviewPanel.TabIndex = 4;
			// 
			// PreviewMainPanel
			// 
			this.PreviewMainPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PreviewMainPanel.Controls.Add(this.PreviewMain);
			this.PreviewMainPanel.Controls.Add(this.ToolbarPanel);
			this.PreviewMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviewMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 0, true);
			this.PreviewMainPanel.Name = "PreviewMainPanel";
			this.PreviewMainPanel.TabIndex = 4;
			// 
			// PreviewMain
			// 
			this.PreviewMain.AutoScroll = true;
			this.PreviewMain.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 10, true);
			this.PreviewMain.BackColor = System.Drawing.Color.Gray;
			this.PreviewMain.CacheSize = 64;
			this.PreviewMain.Cursor = System.Windows.Forms.Cursors.Default;
			this.PreviewMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviewMain.Document = this.flexCelImgProducer;
			this.PreviewMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			this.PreviewMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.PreviewMain.Name = "PreviewMain";
			this.PreviewMain.PageXSeparation = 10;
			this.PreviewMain.PageYSeparation = 10;
			this.PreviewMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 585, true);
			this.PreviewMain.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
			this.PreviewMain.StartPage = 1;
			this.PreviewMain.TabIndex = 0;
			this.PreviewMain.ThumbnailLarge = null;
			this.PreviewMain.ThumbnailSmall = this.PreviewThumbs;
			this.PreviewMain.Zoom = 0.75F;
			this.PreviewMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PreviewMain_MouseMove);
			this.PreviewMain.StartPageChanged += new System.EventHandler(this.PreviewMain_StartPageChanged);
			this.PreviewMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PreviewMain_MouseDown);
			this.PreviewMain.ZoomChanged += new System.EventHandler(this.PreviewMain_ZoomChanged);
			this.PreviewMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PreviewMain_MouseUp);
			// 
			// flexCelImgProducer
			//
#if !WINZOR
			this.flexCelImgProducer.AllVisibleSheets = false;
			this.flexCelImgProducer.PageSize = null;
			this.flexCelImgProducer.ResetPageNumberOnEachSheet = false;
			this.flexCelImgProducer.Resolution = 96F;
			this.flexCelImgProducer.Workbook = null;
			this.flexCelImgProducer.AfterPaint += new FlexCel.Render.PaintEventHandler(this.flexCelImgProducer_AfterPaint);
#else
			this.PreviewMain.WaterMark = GetWatermark();
			this.PreviewThumbs.WaterMark = GetWatermark();
#endif
			// 
			// PreviewThumbs
			// 
			this.PreviewThumbs.AutoScroll = true;
			this.PreviewThumbs.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 10, true);
			this.PreviewThumbs.BackColor = System.Drawing.Color.Gray;
			this.PreviewThumbs.CacheSize = 64;
			this.PreviewThumbs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviewThumbs.Document = this.flexCelImgProducer;
			this.PreviewThumbs.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			this.PreviewThumbs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 91, true);
			this.PreviewThumbs.Name = "PreviewThumbs";
			this.PreviewThumbs.PageXSeparation = 10;
			this.PreviewThumbs.PageYSeparation = 10;
			this.PreviewThumbs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 524, true);
			this.PreviewThumbs.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			this.PreviewThumbs.StartPage = 1;
			this.PreviewThumbs.TabIndex = 2;
			this.PreviewThumbs.ThumbnailLarge = this.PreviewMain;
			this.PreviewThumbs.ThumbnailSmall = null;
			this.PreviewThumbs.Zoom = 0.1F;
			// 
			// ThumbSplitter
			// 
			this.ThumbSplitter.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ThumbSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 0, true);
			this.ThumbSplitter.MinExtra = 16;
			this.ThumbSplitter.MinSize = 16;
			this.ThumbSplitter.Name = "ThumbSplitter";
			this.ThumbSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 617, true);
			this.ThumbSplitter.TabIndex = 0;
			this.ThumbSplitter.TabStop = false;
			this.ThumbSplitter.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.ThumbSplitter_SplitterMoving);
			// 
			// ThumbsPanel
			// 
			this.ThumbsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ThumbsPanel.Controls.Add(this.PreviewThumbs);
			this.ThumbsPanel.Controls.Add(this.Separator3Panel);
			this.ThumbsPanel.Controls.Add(this.PagesPanel);
			this.ThumbsPanel.Controls.Add(this.SeparatorPanel);
			this.ThumbsPanel.Controls.Add(this.SheetsSplitter);
			this.ThumbsPanel.Controls.Add(this.SheetsPanel);
			this.ThumbsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ThumbsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ThumbsPanel.Name = "ThumbsPanel";
			this.ThumbsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 617, true);
			this.ThumbsPanel.TabIndex = 3;
			this.ThumbsPanel.Resize += new System.EventHandler(this.ThumbsPanel_Resize);
			// 
			// Separator3Panel
			// 
			this.Separator3Panel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.Separator3Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.Separator3Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.Separator3Panel.Name = "Separator3Panel";
			this.Separator3Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 1, true);
			this.Separator3Panel.TabIndex = 11;
			// 
			// PagesPanel
			// 
			this.PagesPanel.BackColor = System.Drawing.SystemColors.Control;
			this.PagesPanel.Controls.Add(this.PagesLabel);
			this.PagesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PagesPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.PagesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 73, true);
			this.PagesPanel.Name = "PagesPanel";
			this.PagesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.PagesPanel.TabIndex = 9;
			// 
			// PagesLabel
			// 
			this.PagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 1, true);
			this.PagesLabel.Name = "PagesLabel";
			this.PagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.PagesLabel.TabIndex = 1;
			this.PagesLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|f4fecca5-7c85-4ed0-a1de-4cd48bb1a775", "Pages");
			// 
			// SeparatorPanel
			// 
			this.SeparatorPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.SeparatorPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SeparatorPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.SeparatorPanel.Name = "SeparatorPanel";
			this.SeparatorPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 1, true);
			this.SeparatorPanel.TabIndex = 10;
			// 
			// SheetsSplitter
			// 
			this.SheetsSplitter.BackColor = System.Drawing.SystemColors.ControlLight;
			this.SheetsSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.SheetsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
			this.SheetsSplitter.Name = "SheetsSplitter";
			this.SheetsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 5, true);
			this.SheetsSplitter.TabIndex = 6;
			this.SheetsSplitter.TabStop = false;
			// 
			// SheetsPanel
			// 
			this.SheetsPanel.Controls.Add(this.DocumentPanel);
			this.SheetsPanel.Controls.Add(this.Separator2Panel);
			this.SheetsPanel.Controls.Add(this.SheetsListBox);
			this.SheetsPanel.Controls.Add(this.SheetCaptionPanel);
			this.SheetsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SheetsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SheetsPanel.Name = "SheetsPanel";
			this.SheetsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 67, true);
			this.SheetsPanel.TabIndex = 7;
			// 
			// DocumentPanel
			// 
			this.DocumentPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.DocumentPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DocumentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
			this.DocumentPanel.Name = "DocumentPanel";
			this.DocumentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 1, true);
			this.DocumentPanel.TabIndex = 12;
			// 
			// Separator2Panel
			// 
			this.Separator2Panel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.Separator2Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.Separator2Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 17, true);
			this.Separator2Panel.Name = "Separator2Panel";
			this.Separator2Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 1, true);
			this.Separator2Panel.TabIndex = 11;
			// 
			// SheetsListBox
			// 
			this.SheetsListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.SheetsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SheetsListBox.IntegralHeight = false;
			this.SheetsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 17, true);
			this.SheetsListBox.Name = "SheetsListBox";
			this.SheetsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 50, true);
			this.SheetsListBox.TabIndex = 6;
			this.SheetsListBox.SelectedIndexChanged += new System.EventHandler(this.SheetsListBox_SelectedIndexChanged);
			// 
			// SheetCaptionPanel
			// 
			this.SheetCaptionPanel.BackColor = System.Drawing.SystemColors.Control;
			this.SheetCaptionPanel.Controls.Add(this.HidThumbsButton);
			this.SheetCaptionPanel.Controls.Add(this.DocumentLabel);
			this.SheetCaptionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SheetCaptionPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SheetCaptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SheetCaptionPanel.Name = "SheetCaptionPanel";
			this.SheetCaptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.SheetCaptionPanel.TabIndex = 7;
			// 
			// HidThumbsButton
			// 
			this.HidThumbsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HidThumbsButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.HidThumbsButton.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HidThumbsButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.HidThumbsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.HidThumbsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 0, true);
			this.HidThumbsButton.Name = "HidThumbsButton";
			this.HidThumbsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 17, true);
			this.HidThumbsButton.TabIndex = 14;
			this.HidThumbsButton.TabStop = false;
			this.HidThumbsButton.Text = "<";
			this.HidThumbsButton.Click += new System.EventHandler(this.HidThumbsButton_Click);
			// 
			// DocumentLabel
			// 
			this.DocumentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 1, true);
			this.DocumentLabel.Name = "DocumentLabel";
			this.DocumentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.DocumentLabel.TabIndex = 1;
			this.DocumentLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|0778f0dc-1e4c-4dc1-9e6d-6e5fd2de67b6", "Document");
			// 
			// ZoomMenu
			// 
			this.ZoomMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.Zoom10MenuItem,
			this.Zoom25MenuItem,
			this.Zoom50MenuItem,
			this.Zoom75MenuItem,
			this.Zoom100MenuItem,
			this.Zoom125MenuItem,
			this.Zoom200MenuItem,
			this.Zoom400MenuItem});
			// 
			// Zoom10MenuItem
			// 
			this.Zoom10MenuItem.Index = 0;
			this.Zoom10MenuItem.Text = "10%";
			this.Zoom10MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom25MenuItem
			// 
			this.Zoom25MenuItem.Index = 1;
			this.Zoom25MenuItem.Text = "25%";
			this.Zoom25MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom50MenuItem
			// 
			this.Zoom50MenuItem.Index = 2;
			this.Zoom50MenuItem.Text = "50%";
			this.Zoom50MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom75MenuItem
			// 
			this.Zoom75MenuItem.Index = 3;
			this.Zoom75MenuItem.Text = "75%";
			this.Zoom75MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom100MenuItem
			// 
			this.Zoom100MenuItem.Index = 4;
			this.Zoom100MenuItem.Text = "100%";
			this.Zoom100MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom125MenuItem
			// 
			this.Zoom125MenuItem.Index = 5;
			this.Zoom125MenuItem.Text = "125%";
			this.Zoom125MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom200MenuItem
			// 
			this.Zoom200MenuItem.Index = 6;
			this.Zoom200MenuItem.Text = "200%";
			this.Zoom200MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// Zoom400MenuItem
			// 
			this.Zoom400MenuItem.Index = 7;
			this.Zoom400MenuItem.Text = "400%";
			this.Zoom400MenuItem.Click += new System.EventHandler(this.PresetMenuItem_Click);
			// 
			// XLSPreviewForm
			// 
			this.CaptionRenderingEnabled = true;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 643, true);
			this.Controls.Add(this.PreviewPanel);
			this.Menu = null;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 590, true);
			this.Name = "XLSPreviewForm";
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("XLSPreviewForm|042b0beb-a8d6-4c57-832c-cc72ecb9af19", "Preview");
			this.Load += new System.EventHandler(this.XLSPreviewForm_Load);
			this.Closed += new System.EventHandler(this.XLSPreviewForm_Closed);
			this.Shown += new System.EventHandler(this.XLSPreviewForm_Shown);
			this.SizeChanged += new System.EventHandler(this.XLSPreviewForm_SizeChanged);
			this.Controls.SetChildIndex(this.PreviewPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToolbarPanel.ResumeLayout(false);
			this.ToolbarPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoomUpDown)).EndInit();
			this.PreviewPanel.ResumeLayout(false);
			this.PreviewMainPanel.ResumeLayout(false);
			this.ThumbsPanel.ResumeLayout(false);
			this.PagesPanel.ResumeLayout(false);
			this.SheetsPanel.ResumeLayout(false);
			this.SheetCaptionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
