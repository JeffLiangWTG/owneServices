using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	[ToolboxItem(false)] // required to shut up an erroneous reflection test
	public partial class ZZoomablePictureBox : KUserControl // not used for data binding
	{
		public ZZoomablePictureBox()
		{
			InitializeComponent();
		}

		#region Events

		void pictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			OnMouseDown(e);
		}

		void pictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			OnMouseUp(e);
		}

		void ZoomablePictureBox_Resize(object sender, EventArgs e)
		{
			UpdateImage();
		}

		void ZZoomablePictureBox_PaddingChanged(object sender, EventArgs e)
		{
			UpdateImage();
		}

		#endregion

		[Category("Custom Properties")]
		[Description("The scale factor to apply to the image.")]
		public float Zoom
		{
			get => zoom;
			set
			{
				zoom = value;
				UpdateImage();
			}
		}
		float zoom = 1f;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Already scaled")]
		public void UpdateImage()
		{
			pictureBox.Height = Math.Min(Width - Padding.Horizontal, Height - Padding.Vertical);
			pictureBox.Width = pictureBox.Height;
			pictureBox.Scale(new SizeF(Zoom, Zoom));
			pictureBox.Location = new Point(
				(Width - pictureBox.Width + Padding.Left - Padding.Right) / 2,
				(Height - pictureBox.Height + Padding.Top - Padding.Bottom) / 2);
		}

		public Image DisplayedImage
		{
			get => pictureBox.BackgroundImage;
			set => pictureBox.BackgroundImage = value;
		}
	}
}
