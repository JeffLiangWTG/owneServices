using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Main.Navigation;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup
{
	partial class MainForm
	{
		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.ToolBarImageList = new System.Windows.Forms.ImageList(this.components);
			this.ToolbarIconList = new System.Windows.Forms.ImageList(this.components);
			this.BaseWorkspaceAreaPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightWorkspaceAreaPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ZModulePanel = new CargoWise.Windows.UI.KPanel();
			this.ToolBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToolbarRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainFormToolbarStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ToolbarLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HomeButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HomeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ModuleHeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToolSeparator2 = new System.Windows.Forms.ToolBarButton();
			this.IconImageList = new System.Windows.Forms.ImageList(this.components);
			this.Main.SuspendLayout();
			this.Workspace.SuspendLayout();
			this.TitleBar.SuspendLayout();
			this.AppIcon.SuspendLayout();
			this.SearchBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BaseWorkspaceAreaPanel.SuspendLayout();
			this.RightWorkspaceAreaPanel.SuspendLayout();
			this.ToolBarPanel.SuspendLayout();
			this.ToolbarRightPanel.SuspendLayout();
			this.ToolbarLeftPanel.SuspendLayout();
			this.HomeButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// Main
			// 
			this.Main.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 722, true);
#if !WINZOR
			this.Main.Controls.SetChildIndex(this.TitleBar, 0);
#endif
			this.Main.Controls.SetChildIndex(this.Workspace, 0);
			//
			// Workspace
			//
			this.Workspace.Controls.Add(this.BaseWorkspaceAreaPanel);
			this.Workspace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 676, true);
			//
			// TitleBar
			//
			this.TitleBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 24, true);
			//
			// AppTitleText
			//
#if WINZOR
			this.AppTitleText.Font = new System.Drawing.Font("Segoe UI", 10.15F);
#else
			this.AppTitleText.Font = new System.Drawing.Font("Segoe UI", 10F);
#endif
			this.AppTitleText.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 4, true);
			this.AppTitleText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1262, 24, true);
			this.AppTitleText.Text = "Application";
			//
			// AppMinimise
			//
			this.AppMinimise.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(203)))), ((int)(((byte)(234)))));
			this.AppMinimise.FlatAppearance.BorderSize = 0;
			this.AppMinimise.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(188)))), ((int)(((byte)(228)))));
			this.AppMinimise.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(218)))), ((int)(((byte)(240)))));
			this.AppMinimise.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1286, 0, true);
			//
			// AppMaximise
			//
			this.AppMaximise.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(203)))), ((int)(((byte)(234)))));
			this.AppMaximise.FlatAppearance.BorderSize = 0;
			this.AppMaximise.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(188)))), ((int)(((byte)(228)))));
			this.AppMaximise.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(218)))), ((int)(((byte)(240)))));
			this.AppMaximise.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1310, 0, true);
			//
			// AppClose
			//
			this.AppClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.AppClose.FlatAppearance.BorderSize = 0;
			this.AppClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.AppClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(146)))), ((int)(((byte)(146)))));
			this.AppClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1334, 0, true);
			// 
			// AppIcon
			// 
			this.AppIcon.Zoom = 0.8F;
			// 
			// SearchBox
			//
			this.SearchBox.AutoSearch = false;
			this.SearchBox.BackColor = System.Drawing.Color.LightGray;
			this.SearchBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1097, 0, true);
			//
			// TopRightCornerPanel
			//
			this.TopRightCornerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1362, 0, true);
			//
			// BottomLeftCornerPanel
			//
			this.BottomLeftCornerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 726, true);
			//
			// BottomRightCornerPanel
			//
			this.BottomRightCornerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1362, 726, true);
			//
			// TopBorderPanel
			//
			this.TopBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 4, true);
			//
			// BottomBorderPanel
			//
			this.BottomBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 726, true);
			this.BottomBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 4, true);
			//
			// LeftBorderPanel
			//
			this.LeftBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 722, true);
			//
			// RightBorderPanel
			//
			this.RightBorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1362, 4, true);
			this.RightBorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 722, true);
			// 
			// ToolBarImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ToolBarImageList, (System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolBarImageList.ImageStream")));
			this.ToolBarImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ToolBarImageList.Images.SetKeyName(0, "");
			this.ToolBarImageList.Images.SetKeyName(1, "");
			this.ToolBarImageList.Images.SetKeyName(2, "");
			// 
			// ToolbarIconList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ToolbarIconList, (System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolbarIconList.ImageStream")));
			this.ToolbarIconList.TransparentColor = System.Drawing.Color.Transparent;
			this.ToolbarIconList.Images.SetKeyName(0, "");
			this.ToolbarIconList.Images.SetKeyName(1, "");
			this.ToolbarIconList.Images.SetKeyName(2, "");
			this.ToolbarIconList.Images.SetKeyName(3, "");
			this.ToolbarIconList.Images.SetKeyName(4, "");
			this.ToolbarIconList.Images.SetKeyName(5, "");
			// 
			// BaseWorkspaceAreaPanel
			// 
			this.BaseWorkspaceAreaPanel.BackColor = System.Drawing.SystemColors.Control;
			this.BaseWorkspaceAreaPanel.Controls.Add(this.RightWorkspaceAreaPanel);
			this.BaseWorkspaceAreaPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseWorkspaceAreaPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BaseWorkspaceAreaPanel.Name = "BaseWorkspaceAreaPanel";
			this.BaseWorkspaceAreaPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 676, true);
			this.BaseWorkspaceAreaPanel.TabIndex = 1;
			//
			// RightWorkspaceAreaPanel
			//
			this.RightWorkspaceAreaPanel.BackColor = System.Drawing.SystemColors.Control;
			this.RightWorkspaceAreaPanel.Controls.Add(this.ZModulePanel);
			this.RightWorkspaceAreaPanel.Controls.Add(this.ToolBarPanel);
			this.RightWorkspaceAreaPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightWorkspaceAreaPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightWorkspaceAreaPanel.Name = "RightWorkspaceAreaPanel";
			this.RightWorkspaceAreaPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 676, true);
			this.RightWorkspaceAreaPanel.TabIndex = 11;
			//
			// ZModulePanel
			//
			this.ZModulePanel.BackColor = System.Drawing.SystemColors.Control;
			this.ZModulePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZModulePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.ZModulePanel.Name = "ZModulePanel";
			this.ZModulePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 640, true);
			this.ZModulePanel.TabIndex = 17;
			this.ZModulePanel.TrackDisposedAccess = true;
			//
			// ToolBarPanel
			//
			this.ToolBarPanel.Controls.Add(this.ToolbarRightPanel);
			this.ToolBarPanel.Controls.Add(this.ToolbarLeftPanel);
			this.ToolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolBarPanel.Name = "ToolBarPanel";
			this.ToolBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 36, true);
			this.ToolBarPanel.TabIndex = 16;
			this.ToolBarPanel.Visible = false;
			//
			// ToolbarRightPanel
			//
			this.ToolbarRightPanel.Controls.Add(this.MainFormToolbarStrip);
			this.ToolbarRightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToolbarRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 0, true);
			this.ToolbarRightPanel.Name = "ToolbarRightPanel";
			this.ToolbarRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 36, true);
			this.ToolbarRightPanel.TabIndex = 1;
			//
			// MainFormToolbarStrip
			//
			this.MainFormToolbarStrip.BackColor = System.Drawing.Color.Transparent;
			this.MainFormToolbarStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainFormToolbarStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.MainFormToolbarStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainFormToolbarStrip.Name = "MainFormToolbarStrip";
			this.MainFormToolbarStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 36, true);
			this.MainFormToolbarStrip.TabIndex = 0;
			// 
			// ToolbarLeftPanel
			// 
			this.ToolbarLeftPanel.Controls.Add(this.HomeButtonPanel);
			this.ToolbarLeftPanel.Controls.Add(this.ModuleHeadingLabel);
			this.ToolbarLeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToolbarLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolbarLeftPanel.Name = "ToolbarLeftPanel";
			this.ToolbarLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 36, true);
			this.ToolbarLeftPanel.TabIndex = 0;
			// 
			// HomeButtonPanel
			// 
			this.HomeButtonPanel.Controls.Add(this.HomeButton);
			this.HomeButtonPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.HomeButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HomeButtonPanel.Name = "HomeButtonPanel";
			this.HomeButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 36, true);
			this.HomeButtonPanel.TabIndex = 7;
			// 
			// HomeButton
			// 
			this.HomeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(168)))), ((int)(((byte)(225)))));
			this.HomeButton.FlatAppearance.BorderSize = 0;
			this.HomeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(212)))), ((int)(((byte)(0)))));
			this.HomeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.HomeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.HomeButton.ForeColor = System.Drawing.Color.White;
			this.HomeButton.Image = ((System.Drawing.Image)(resources.GetObject("HomeButton.Image")));
			this.HomeButton.IsCaptionOverridden = false;
			this.HomeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HomeButton.Name = "HomeButton";
			this.HomeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.HomeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 28, true);
			this.HomeButton.TabIndex = 2;
			this.HomeButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.HomeButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.HomeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.HomeButton.ToolTipCaption = null;
			this.HomeButton.UseVisualStyleBackColor = false;
			this.HomeButton.Click += new System.EventHandler(this.HomeButton_ButtonClicked);
			// 
			// ModuleHeadingLabel
			// 
			this.ModuleHeadingLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ModuleHeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ModuleHeadingLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ModuleHeadingLabel.IsFontBold = true;
			this.ModuleHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 0, true);
			this.ModuleHeadingLabel.Name = "ModuleHeadingLabel";
			this.ModuleHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 36, true);
			this.ModuleHeadingLabel.TabIndex = 6;
			this.ModuleHeadingLabel.Text = "<module caption>";
			// 
			// ToolSeparator2
			// 
			this.ToolSeparator2.ImageIndex = 0;
			this.ToolSeparator2.Name = "ToolSeparator2";
			this.ToolSeparator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// IconImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.IconImageList, (System.Windows.Forms.ImageListStreamer)(resources.GetObject("IconImageList.ImageStream")));
			this.IconImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.IconImageList.Images.SetKeyName(0, "");
			this.IconImageList.Images.SetKeyName(1, "");
			this.IconImageList.Images.SetKeyName(2, "");
			this.IconImageList.Images.SetKeyName(3, "");
			this.IconImageList.Images.SetKeyName(4, "");
			this.IconImageList.Images.SetKeyName(5, "");
			this.IconImageList.Images.SetKeyName(6, "");
			this.IconImageList.Images.SetKeyName(7, "");
			this.IconImageList.Images.SetKeyName(8, "");
			this.IconImageList.Images.SetKeyName(9, "");
			this.IconImageList.Images.SetKeyName(10, "");
			this.IconImageList.Images.SetKeyName(11, "");
			this.IconImageList.Images.SetKeyName(12, "");
			this.IconImageList.Images.SetKeyName(13, "");
			this.IconImageList.Images.SetKeyName(14, "");
			this.IconImageList.Images.SetKeyName(15, "");
			this.IconImageList.Images.SetKeyName(16, "");
			this.IconImageList.Images.SetKeyName(17, "");
			this.IconImageList.Images.SetKeyName(18, "");
			this.IconImageList.Images.SetKeyName(19, "");
			this.IconImageList.Images.SetKeyName(20, "");
			this.IconImageList.Images.SetKeyName(21, "");
			this.IconImageList.Images.SetKeyName(22, "");
			// 
			// MainForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 730, true);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IconZoom = 0.8F;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 730, true);
			this.Name = "MainForm";
			this.Text = "Application";
			this.Main.ResumeLayout(false);
			this.Main.PerformLayout();
			this.Workspace.ResumeLayout(false);
			this.Workspace.PerformLayout();
			this.TitleBar.ResumeLayout(false);
			this.TitleBar.PerformLayout();
			this.AppIcon.ResumeLayout(true);
			this.AppIcon.PerformLayout();
			this.SearchBox.ResumeLayout(true);
			this.SearchBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BaseWorkspaceAreaPanel.ResumeLayout(false);
			this.BaseWorkspaceAreaPanel.PerformLayout();
			this.RightWorkspaceAreaPanel.ResumeLayout(false);
			this.RightWorkspaceAreaPanel.PerformLayout();
			this.ToolBarPanel.ResumeLayout(false);
			this.ToolBarPanel.PerformLayout();
			this.ToolbarRightPanel.ResumeLayout(false);
			this.ToolbarRightPanel.PerformLayout();
			this.ToolbarLeftPanel.ResumeLayout(false);
			this.ToolbarLeftPanel.PerformLayout();
			this.HomeButtonPanel.ResumeLayout(false);
			this.HomeButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private IContainer components;
		private ImageList ToolBarImageList;
		private ImageList ToolbarIconList;
		internal ZPanel BaseWorkspaceAreaPanel;
		private ToolBarButton ToolSeparator2;
		private ImageList IconImageList;
		internal ModuleNavigation NavigationBar;

#endregion

		internal ZPanel RightWorkspaceAreaPanel;
		internal KPanel ZModulePanel;
		private ZPanel ToolBarPanel;
		private ZPanel ToolbarRightPanel;
		private ZToolStrip MainFormToolbarStrip;
		private ZPanel ToolbarLeftPanel;
		private ZPanel HomeButtonPanel;
		internal ZButton HomeButton;
		private ZLabel ModuleHeadingLabel;
	}
}
