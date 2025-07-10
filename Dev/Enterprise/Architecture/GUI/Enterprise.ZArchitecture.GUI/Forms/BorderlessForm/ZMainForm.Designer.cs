using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.SearchBox;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	partial class ZMainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.Main = new CargoWise.Windows.UI.KPanel();
			this.Workspace = new CargoWise.Windows.UI.KPanel();
			this.TitleBar = new CargoWise.Windows.UI.KPanel();
			this.AppTitleText = new CargoWise.Windows.UI.KLabel();
			this.AppIcon = new Enterprise.ZArchitecture.GUI.Forms.ZZoomablePictureBox();
			this.SearchBox = new Enterprise.ZArchitecture.GUI.SearchBox.ZSearchBox();
			this.AppMinimise = new CargoWise.Windows.UI.KButton();
			this.AppMaximise = new CargoWise.Windows.UI.KButton();
			this.AppClose = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Main.SuspendLayout();
			this.TitleBar.SuspendLayout();
			this.AppIcon.SuspendLayout();
			this.SearchBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// Main
			// 
			this.Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this.Main.Controls.Add(this.Workspace);
			this.Main.Controls.Add(this.TitleBar);
			this.Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Main.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Main.Name = "Main";
			this.Main.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 441, true);
			this.Main.TabIndex = 12;
			// 
			// Workspace
			// 
			this.Workspace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.Workspace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Workspace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.Workspace.Name = "Workspace";
			this.Workspace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 417, true);
			this.Workspace.TabIndex = 0;
			// 
			// TitleBar
			// 
			this.TitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.TitleBar.Controls.Add(this.AppTitleText);
			this.TitleBar.Controls.Add(this.AppIcon);
			this.TitleBar.Controls.Add(this.SearchBox);
			this.TitleBar.Controls.Add(this.AppMinimise);
			this.TitleBar.Controls.Add(this.AppMaximise);
			this.TitleBar.Controls.Add(this.AppClose);
			this.TitleBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.TitleBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TitleBar.Name = "TitleBar";
			this.TitleBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			this.TitleBar.TabIndex = 15;
			// 
			// AppTitleText
			// 
			this.AppTitleText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this.AppTitleText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AppTitleText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.AppTitleText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 0, true);
			this.AppTitleText.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
			this.AppTitleText.Name = "AppTitleText";
			this.AppTitleText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 24, true);
			this.AppTitleText.TabIndex = 10;
			this.AppTitleText.Text = "<insert text here>";
#if WINZOR
			this.AppTitleText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
#else
			this.AppTitleText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
#endif
			this.AppTitleText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AppTitleText_MouseDown);
			this.AppTitleText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AppTitleText_MouseUp);
			// 
			// AppIcon
			// 
			this.AppIcon.AllowDrop = true;
			this.AppIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.AppIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.AppIcon.DisplayedImage = null;
			this.AppIcon.Dock = System.Windows.Forms.DockStyle.Left;
			this.AppIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AppIcon.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AppIcon.Name = "AppIcon";
			this.AppIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.AppIcon.TabIndex = 16;
			this.AppIcon.Zoom = 1F;
			this.AppIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AppIcon_MouseUp);
			// 
			// SearchBox
			// 
			this.SearchBox.AutoSearch = true;
			this.SearchBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.SearchBox.DefaultBackColor = System.Drawing.Color.LightGray;
			this.SearchBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.SearchBox.Font = new System.Drawing.Font("Segoe UI", 7F);
			this.SearchBox.ForeColor = System.Drawing.Color.Black;
			this.SearchBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 0, true);
			this.SearchBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 64, true);
			this.SearchBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
			this.SearchBox.Name = "SearchBox";
			this.SearchBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.SearchBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 24, true);
			this.SearchBox.TabIndex = 14;
			// 
			// AppMinimise
			// 
			this.AppMinimise.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.AppMinimise.Dock = System.Windows.Forms.DockStyle.Right;
			this.AppMinimise.FlatAppearance.BorderSize = 0;
			this.AppMinimise.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AppMinimise.Font = new System.Drawing.Font("Marlett", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppMinimise.IsCaptionOverridden = true;
			this.AppMinimise.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 0, true);
			this.AppMinimise.Name = "AppMinimise";
			this.AppMinimise.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.AppMinimise.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.AppMinimise.TabIndex = 7;
			this.AppMinimise.Text = "0";
			this.AppMinimise.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AppMinimise.ToolTipCaption = null;
			this.AppMinimise.UseCompatibleTextRendering = true;
			this.AppMinimise.UseVisualStyleBackColor = false;
			this.AppMinimise.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AppMinimise_MouseClick);
			// 
			// AppMaximise
			// 
			this.AppMaximise.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.AppMaximise.Dock = System.Windows.Forms.DockStyle.Right;
			this.AppMaximise.FlatAppearance.BorderSize = 0;
			this.AppMaximise.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AppMaximise.Font = new System.Drawing.Font("Marlett", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppMaximise.IsCaptionOverridden = true;
			this.AppMaximise.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 0, true);
			this.AppMaximise.Name = "AppMaximise";
			this.AppMaximise.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.AppMaximise.TabIndex = 8;
			this.AppMaximise.Text = "1";
			this.AppMaximise.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AppMaximise.ToolTipCaption = null;
			this.AppMaximise.UseCompatibleTextRendering = true;
			this.AppMaximise.UseVisualStyleBackColor = false;
			this.AppMaximise.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AppMaximise_MouseClick);
			// 
			// AppClose
			// 
			this.AppClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.AppClose.Dock = System.Windows.Forms.DockStyle.Right;
			this.AppClose.FlatAppearance.BorderSize = 0;
			this.AppClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AppClose.Font = new System.Drawing.Font("Marlett", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppClose.IsCaptionOverridden = true;
			this.AppClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 0, true);
			this.AppClose.Name = "AppClose";
			this.AppClose.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.AppClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.AppClose.TabIndex = 9;
			this.AppClose.Text = "r";
			this.AppClose.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AppClose.ToolTipCaption = null;
			this.AppClose.UseCompatibleTextRendering = true;
			this.AppClose.UseVisualStyleBackColor = false;
			this.AppClose.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AppClose_MouseClick);
			// 
			// ZMainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 441, true);
			this.Controls.Add(this.Main);
			this.Name = "ZMainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.IconChanged += new System.EventHandler<Enterprise.ZArchitecture.GUI.Forms.Native.IconType>(this.ZMainForm_IconChanged);
			this.Activated += new System.EventHandler(this.ZMainForm_Activated);
			this.Deactivate += new System.EventHandler(this.ZMainForm_Deactivate);
			this.BackColorChanged += new System.EventHandler(this.ZMainForm_BackColorChanged);
			this.SizeChanged += new System.EventHandler(this.ZMainForm_SizeChanged);
			this.TextChanged += new System.EventHandler(this.ZMainForm_TextChanged);
			this.Controls.SetChildIndex(this.TopLeftCornerPanel, 0);
			this.Controls.SetChildIndex(this.BottomRightCornerPanel, 0);
			this.Controls.SetChildIndex(this.BottomLeftCornerPanel, 0);
			this.Controls.SetChildIndex(this.TopRightCornerPanel, 0);
			this.Controls.SetChildIndex(this.TopBorderPanel, 0);
			this.Controls.SetChildIndex(this.BottomBorderPanel, 0);
			this.Controls.SetChildIndex(this.LeftBorderPanel, 0);
			this.Controls.SetChildIndex(this.RightBorderPanel, 0);
			this.Controls.SetChildIndex(this.Main, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Main.ResumeLayout(false);
			this.Main.PerformLayout();
			this.TitleBar.ResumeLayout(false);
			this.TitleBar.PerformLayout();
			this.AppIcon.ResumeLayout(true);
			this.AppIcon.PerformLayout();
			this.SearchBox.ResumeLayout(true);
			this.SearchBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

		protected internal KPanel Main;
		protected internal KPanel Workspace;
		protected internal KPanel TitleBar;
		protected internal KLabel AppTitleText;
		protected internal KButton AppMinimise;
		protected internal KButton AppMaximise;
		protected internal KButton AppClose;
		protected internal ZZoomablePictureBox AppIcon;
		protected internal ZSearchBox SearchBox;
	}
}
