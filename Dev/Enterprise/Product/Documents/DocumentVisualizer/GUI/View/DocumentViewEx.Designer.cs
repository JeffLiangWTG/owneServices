using System.Drawing;
using System.Windows.Forms;
using CargoWise.ResourceStrings.Cache;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class DocumentViewEx
	{
		void InitializeComponent()
		{
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.toolStrip = new CargoWise.Windows.UI.KToolStrip();
			this.zoomControl = new Enterprise.ZArchitecture.ZCalcEdit();
			this.notificationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.notificationsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.statusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.statusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.notificationPanel.SuspendLayout();
			this.statusPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentVisualizer.Presentation.DocumentSettings);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.toolStrip);
			this.topPanel.Controls.Add(this.zoomControl);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 35, true);
			this.topPanel.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 6, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.toolStrip.TabIndex = 11;
			this.toolStrip.Text = "toolStrip";
			// 
			// zoomControl
			// 
			this.BindingSource.SetBindingMember(this.zoomControl, "Zoom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentVisualizer.Presentation.DocumentSettings)(null)).Zoom)));
			this.zoomControl.DecimalPlaces = 0;
			this.zoomControl.Decimals = 0;
			this.zoomControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 6, true);
			this.zoomControl.Name = "zoomControl";
			this.zoomControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.zoomControl.TabIndex = 4;
			this.zoomControl.TabStop = false;
			this.zoomControl.Text = "0";
			this.zoomControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mainPanel
			// 
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 495, true);
			this.mainPanel.TabIndex = 1;
			// 
			// notificationPanel
			// 
			this.notificationPanel.Controls.Add(this.notificationsLinkLabel);
			this.notificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.notificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.notificationPanel.Name = "notificationPanel";
			this.notificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 31, true);
			this.notificationPanel.TabIndex = 10;
			// 
			// notificationsLinkLabel
			// 
			this.notificationsLinkLabel.AutoSize = true;
			this.notificationsLinkLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("1cd33bc1-7fb0-47eb-b05b-e9c15a7bff81", "This document has pending notifications. Click here to view them.");
			this.notificationsLinkLabel.IsFontBold = false;
			this.notificationsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.notificationsLinkLabel.Name = "notificationsLinkLabel";
			this.notificationsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 14, true);
			this.notificationsLinkLabel.TabIndex = 1;
			// 
			// statusPanel
			// 
			this.statusPanel.Controls.Add(this.statusLabel);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.statusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 30, true);
			this.statusPanel.TabIndex = 11;
			this.statusPanel.Visible = false;
			// 
			// statusLabel
			// 
			this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusLabel.IsFontBold = true;
			this.statusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 30, true);
			this.statusLabel.TabIndex = 0;
			// 
			// DocumentVisualizerView
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.notificationPanel);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.statusPanel);
			this.Controls.Add(this.topPanel);
			this.Name = "DocumentVisualizerView";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 560, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.notificationPanel.ResumeLayout(false);
			this.notificationPanel.PerformLayout();
			this.statusPanel.ResumeLayout(false);
			this.statusPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel mainPanel;
		private ZArchitecture.ZCalcEdit zoomControl;
		private ZArchitecture.GUI.ZPanel notificationPanel;
		private ZArchitecture.GUI.ZLinkLabel notificationsLinkLabel;
		private CargoWise.Windows.UI.KToolStrip toolStrip;
		private ZArchitecture.GUI.ZPanel statusPanel;
		private ZArchitecture.ZLabel statusLabel;
	}
}
