using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZMessageBoxWithCheckbox : ZMessageBox
	{
#if DEBUG
		internal
#endif
		ZCheckBox DontAskMeAgainCheckBox;
		ZLinkLabel LinkLabel;

		public ZMessageBoxWithCheckbox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string link, IRegistryItem registryItem)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeComponent();
			((Button)AcceptButton).Click += new EventHandler(AcceptButton_Click);
			this.link = link;
			this.registryItem = registryItem;
		}

		public ZMessageBoxWithCheckbox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string link, IRegistryItem registryItem)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeComponent();
			((Button)AcceptButton).Click += new EventHandler(AcceptButton_Click);
			this.link = link;
			this.registryItem = registryItem;
		}

		void AcceptButton_Click(object sender, EventArgs e)
		{
			if (DontAskMeAgainCheckBox.Checked && registryItem != null)
			{
				registryItem.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, false);
			}
		}

		public ZMessageBoxWithCheckbox(string message, string caption, MessageBoxButtons buttons, Image customImage, MessageBoxDefaultButton defaultButton, string link, IRegistryItem registryItem)
			: this(message, caption, buttons, MessageBoxIcon.None, defaultButton, link, registryItem)
		{
			Image = customImage;
		}

		public ZMessageBoxWithCheckbox(MultilingualString message, string caption, MessageBoxButtons buttons, Image customImage, MessageBoxDefaultButton defaultButton, string link, IRegistryItem registryItem)
			: this(message, caption, buttons, MessageBoxIcon.None, defaultButton, link, registryItem)
		{
			Image = customImage;
		}

		public void SetDontAskMeAgainCheckBoxVisibility(bool isVisible)
		{
			DontAskMeAgainCheckBox.Visible = isVisible;
		}

		public string Link { get { return link; } }
		readonly string link;
		readonly IRegistryItem registryItem;

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		void InitializeComponent()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		{
			this.DontAskMeAgainCheckBox = new ZCheckBox();
			this.LinkLabel = new ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DontAskMeAgainCheckBox
			// 
			this.DontAskMeAgainCheckBox.CaptionResourceString = Res.GetData("886711AB-54A9-413F-A849-9EBE8AA75FD8", "Do not show this message again");
			this.DontAskMeAgainCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DontAskMeAgainCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 32, true);
			this.DontAskMeAgainCheckBox.Name = "DontAskMeAgainCheckBox";
			this.DontAskMeAgainCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			this.DontAskMeAgainCheckBox.TabIndex = 6;
			this.DontAskMeAgainCheckBox.UseVisualStyleBackColor = true;
			// 
			// LinkLabel
			// 
			this.LinkLabel.AutoSize = true;
			this.LinkLabel.CaptionResourceString = null;
			this.LinkLabel.IsFontBold = false;
			this.LinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 63, true);
			this.LinkLabel.Name = "LinkLabel";
			this.LinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.LinkLabel.TabIndex = 7;
			this.LinkLabel.TabStop = false;
			// 
			// ZMessageBoxWithCheckbox
			// 
			this.Controls.Add(this.LinkLabel);
			this.Controls.Add(this.DontAskMeAgainCheckBox);
			this.Name = "ZMessageBoxWithCheckbox";
			this.Controls.SetChildIndex(this.DontAskMeAgainCheckBox, 0);
			this.Controls.SetChildIndex(this.LinkLabel, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateHeightWidthSettings();
		}

		protected void UpdateHeightWidthSettings()
		{
			if (!string.IsNullOrEmpty(Link))
			{
				LinkLabel.Text = Link;
				LinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_LinkClicked);
				LinkLabel.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, TextBox.Location.Y + TextBox.Height, false);
			}

			var minWidthWhenTextBoxIsNotScrolling = Math.Max(Math.Max(TextBox.Width, LinkLabel.Width), DontAskMeAgainCheckBox.Width) + PictureBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			var minWidthWhenTextBoxIsScrolling = TextBox.Right + Width - ClientRectangle.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(6);
			var width = Math.Max(minWidthWhenTextBoxIsNotScrolling, minWidthWhenTextBoxIsScrolling);
			if (Width < width)
			{
				var increasedWidth = width - Width;
				ControlDpiScalingHelper.SetWidth(this, width, false);
				Button1.Location = ControlDpiScalingHelper.NewScaledPoint(Button1.Location.X + (increasedWidth / 2), Button1.Location.Y, false);
				Button2.Location = ControlDpiScalingHelper.NewScaledPoint(Button2.Location.X + (increasedWidth / 2), Button2.Location.Y, false);
				Button3.Location = ControlDpiScalingHelper.NewScaledPoint(Button3.Location.X + (increasedWidth / 2), Button3.Location.Y, false);
			}

			if (!string.IsNullOrEmpty(Link))
			{
				DontAskMeAgainCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, LinkLabel.Location.Y + LinkLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
			}
			else
			{
				DontAskMeAgainCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, TextBox.Location.Y + TextBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			}

			ControlDpiScalingHelper.SetHeight(this, DontAskMeAgainCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(85), false);
			Button1.Location = ControlDpiScalingHelper.NewScaledPoint(Button1.Location.X, DontAskMeAgainCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(25), false);
			Button2.Location = ControlDpiScalingHelper.NewScaledPoint(Button2.Location.X, DontAskMeAgainCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(25), false);
			Button3.Location = ControlDpiScalingHelper.NewScaledPoint(Button3.Location.X, DontAskMeAgainCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(25), false);
		}

		void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				WebUrlLauncher.Launch(Link);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}
}
