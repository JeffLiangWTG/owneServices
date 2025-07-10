using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressControlRequiresTextBasher]
	[ToolboxItem(false)]
	public class ZImageButton : ZButton
	{
		public ZImageButton()
		{
			BackColor = Color.Transparent;
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
			FlatAppearance.MouseDownBackColor = Color.Transparent;
			FlatAppearance.MouseOverBackColor = Color.Transparent;
		}

		#region Properties

		[DefaultValue(typeof(Color), "Transparent")]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable(true)]
		[Category("Appearance")]
		public Image DownBackgroundImage
		{
			get { return downBackgroundImage; }
			set
			{
				if (downBackgroundImage != value)
				{
					downBackgroundImage = value;
					RefreshBackgroundImage();
				}
			}
		}
		Image downBackgroundImage;

		[Browsable(true)]
		[Category("Appearance")]
		public Image HotBackgroundImage
		{
			get { return hotBackgroundImage; }
			set
			{
				if (hotBackgroundImage != value)
				{
					hotBackgroundImage = value;
					RefreshBackgroundImage();
				}
			}
		}
		Image hotBackgroundImage;

		[Browsable(true)]
		[Category("Appearance")]
		public Image NormalBackgroundImage
		{
			get { return normalBackgroundImage; }
			set
			{
				if (normalBackgroundImage != value)
				{
					normalBackgroundImage = value;
					RefreshBackgroundImage();
				}
			}
		}
		Image normalBackgroundImage;

		[DefaultValue(true)]
		[Category("Appearance")]
		public bool DisplayFocusCues
		{
			get { return displayFocusCues; }
			set { displayFocusCues = value; }
		}
		bool displayFocusCues = true;

		#endregion

		#region Implementation

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			IsMouseOver = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			IsMouseOver = false;
		}

		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			base.OnMouseDown(mevent);
			IsMouseDown = true;
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp(mevent);
			IsMouseDown = false;
		}

		protected void RefreshBackgroundImage()
		{
			BackgroundImage =
				IsDown ? DownBackgroundImage ?? HotBackgroundImage ?? NormalBackgroundImage :
				IsHot ? HotBackgroundImage ?? NormalBackgroundImage :
				NormalBackgroundImage;
		}

		protected virtual bool IsDown
		{
			get { return IsMouseDown && IsMouseOver; }
		}

		protected virtual bool IsHot
		{
			get { return IsMouseOver; }
		}

		bool IsMouseDown
		{
			get { return isMouseDown; }
			set
			{
				isMouseDown = value;
				RefreshBackgroundImage();
			}
		}
		bool isMouseDown;

		bool IsMouseOver
		{
			get { return isMouseOver; }
			set
			{
				isMouseOver = value;
				RefreshBackgroundImage();
			}
		}
		bool isMouseOver;

		protected override bool ShowFocusCues
		{
			get { return DisplayFocusCues && base.ShowFocusCues; }
		}

		#endregion
	}
}
