using CargoWise.Windows.UI;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	public partial class ZNotificationsForm
	{ 
		ImageTreeView NotificationsTree;
		KButton RefreshButton;
		KRadioButton AllNotificationsRadio;
		KRadioButton ErrorsRadio;
		KRadioButton WarningsRadio;
		KRadioButton MessageErrorsRadio;
		KLabel label1;
		KButton ExpandButton;
		KButton CollapseButton;
		KLabel label2;
		KButton ToggleOptionsButton;
		KPanel MainPanel;
		KPanel OptionsPanel;
		ImageList IconsImageList;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(ZNotificationsForm));
			this.NotificationsTree = new Enterprise.ZArchitecture.DevTools.ImageTreeView();
			this.IconsImageList = new System.Windows.Forms.ImageList(this.components);
			this.RefreshButton = new CargoWise.Windows.UI.KButton();
			this.AllNotificationsRadio = new CargoWise.Windows.UI.KRadioButton();
			this.ErrorsRadio = new CargoWise.Windows.UI.KRadioButton();
			this.WarningsRadio = new CargoWise.Windows.UI.KRadioButton();
			this.MessageErrorsRadio = new CargoWise.Windows.UI.KRadioButton();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.ExpandButton = new CargoWise.Windows.UI.KButton();
			this.CollapseButton = new CargoWise.Windows.UI.KButton();
			this.ToggleOptionsButton = new CargoWise.Windows.UI.KButton();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.MainPanel = new CargoWise.Windows.UI.KPanel();
			this.OptionsPanel = new CargoWise.Windows.UI.KPanel();
			this.MainPanel.SuspendLayout();
			this.OptionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// NotificationsTree
			// 
			this.NotificationsTree.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.NotificationsTree.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.NotificationsTree.ImageIndex = 0;
			this.NotificationsTree.ImageList = this.IconsImageList;
			this.NotificationsTree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.NotificationsTree.Name = "NotificationsTree";
			this.NotificationsTree.SelectedImageIndex = 0;
			this.NotificationsTree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 548, true);
			this.NotificationsTree.Sorted = true;
			this.NotificationsTree.TabIndex = 0;
			// 
			// IconsImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.IconsImageList, ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("IconsImageList.ImageStream"))));
			this.IconsImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.IconsImageList.Images.SetKeyName(0, "");
			this.IconsImageList.Images.SetKeyName(1, "");
			this.IconsImageList.Images.SetKeyName(2, "");
			this.IconsImageList.Images.SetKeyName(3, "");
			this.IconsImageList.Images.SetKeyName(4, "");
			// 
			// RefreshButton
			// 
			this.RefreshButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.RefreshButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 564, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.Text = "&Refresh";
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// AllNotificationsRadio
			// 
			this.AllNotificationsRadio.Checked = true;
			this.AllNotificationsRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllNotificationsRadio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 44, true);
			this.AllNotificationsRadio.Name = "AllNotificationsRadio";
			this.AllNotificationsRadio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 24, true);
			this.AllNotificationsRadio.TabIndex = 2;
			this.AllNotificationsRadio.TabStop = true;
			this.AllNotificationsRadio.Text = "All &Notifications";
			this.AllNotificationsRadio.CheckedChanged += new System.EventHandler(this.AllNotificationsRadio_CheckedChanged);
			// 
			// ErrorsRadio
			// 
			this.ErrorsRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ErrorsRadio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 68, true);
			this.ErrorsRadio.Name = "ErrorsRadio";
			this.ErrorsRadio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 24, true);
			this.ErrorsRadio.TabIndex = 3;
			this.ErrorsRadio.Text = "&Errors";
			this.ErrorsRadio.CheckedChanged += new System.EventHandler(this.ErrorsRadio_CheckedChanged);
			// 
			// WarningsRadio
			// 
			this.WarningsRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WarningsRadio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 92, true);
			this.WarningsRadio.Name = "WarningsRadio";
			this.WarningsRadio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.WarningsRadio.TabIndex = 4;
			this.WarningsRadio.Text = "&Warnings";
			this.WarningsRadio.CheckedChanged += new System.EventHandler(this.WarningsRadio_CheckedChanged);
			// 
			// MessageErrorsRadio
			// 
			this.MessageErrorsRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MessageErrorsRadio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 116, true);
			this.MessageErrorsRadio.Name = "MessageErrorsRadio";
			this.MessageErrorsRadio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.MessageErrorsRadio.TabIndex = 5;
			this.MessageErrorsRadio.Text = "&Message Errors";
			this.MessageErrorsRadio.CheckedChanged += new System.EventHandler(this.MessageErrorsRadio_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 20, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 14, true);
			this.label1.TabIndex = 7;
			this.label1.Text = "Notifications:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ExpandButton
			// 
			this.ExpandButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExpandButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 196, true);
			this.ExpandButton.Name = "ExpandButton";
			this.ExpandButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.ExpandButton.TabIndex = 9;
			this.ExpandButton.Text = "E&xpand All";
			this.ExpandButton.Click += new System.EventHandler(this.ExpandButton_Click);
			// 
			// CollapseButton
			// 
			this.CollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CollapseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 223, true);
			this.CollapseButton.Name = "CollapseButton";
			this.CollapseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.CollapseButton.TabIndex = 10;
			this.CollapseButton.Text = "&Collapse All";
			this.CollapseButton.Click += new System.EventHandler(this.CollapseButton_Click);
			// 
			// ToggleOptionsButton
			// 
			this.ToggleOptionsButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.ToggleOptionsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ToggleOptionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 564, true);
			this.ToggleOptionsButton.Name = "ToggleOptionsButton";
			this.ToggleOptionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.ToggleOptionsButton.TabIndex = 11;
			this.ToggleOptionsButton.Text = "<";
			this.ToggleOptionsButton.Click += new System.EventHandler(this.ToggleOptionsButton_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 172, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 14, true);
			this.label2.TabIndex = 12;
			this.label2.Text = "Tree View:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.MainPanel.Controls.Add(this.ToggleOptionsButton);
			this.MainPanel.Controls.Add(this.NotificationsTree);
			this.MainPanel.Controls.Add(this.RefreshButton);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 596, true);
			this.MainPanel.TabIndex = 13;
			// 
			// OptionsPanel
			// 
			this.OptionsPanel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.OptionsPanel.Controls.Add(this.WarningsRadio);
			this.OptionsPanel.Controls.Add(this.ErrorsRadio);
			this.OptionsPanel.Controls.Add(this.CollapseButton);
			this.OptionsPanel.Controls.Add(this.MessageErrorsRadio);
			this.OptionsPanel.Controls.Add(this.label2);
			this.OptionsPanel.Controls.Add(this.label1);
			this.OptionsPanel.Controls.Add(this.AllNotificationsRadio);
			this.OptionsPanel.Controls.Add(this.ExpandButton);
			this.OptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 0, true);
			this.OptionsPanel.Name = "OptionsPanel";
			this.OptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 256, true);
			this.OptionsPanel.TabIndex = 14;
			// 
			// ZNotificationsForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 595, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.OptionsPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 316, true);
			this.Name = "ZNotificationsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Notifications..";
			this.MainPanel.ResumeLayout(false);
			this.OptionsPanel.ResumeLayout(false);
			this.OptionsPanel.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
