using System;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class ZScrollablePictureBox : ZUserControl
	{
		public ZScrollablePictureBox()
		{
			InitializeComponent();
		}

		public Image Image
		{
			get { return pictureBox.Image; }
			set
			{
				if (value != pictureBox.Image)
				{
					pictureBox.Image = value;

					if (pictureBox.Image != null)
					{
						UpdatePictureBoxSize();
					}
					else
					{
						ControlDpiScalingHelper.SetWidth(ref pictureBox, 0, true);
						ControlDpiScalingHelper.SetHeight(ref pictureBox, 0, true);
					}
				}
			}
		}

		int zoom = 100;
		public int Zoom
		{
			get { return zoom; }
			set
			{
				if (value != zoom)
				{
					zoom = value;
					UpdatePictureBoxSize();
				}
			}
		}

		void UpdatePictureBoxSize()
		{
			if (pictureBox.Image != null)
			{
				ControlDpiScalingHelper.SetWidth(ref pictureBox, pictureBox.Image.Width * Zoom / Convert.ToInt32(pictureBox.Image.HorizontalResolution), false);
				ControlDpiScalingHelper.SetHeight(ref pictureBox, pictureBox.Image.Height * Zoom / Convert.ToInt32(pictureBox.Image.VerticalResolution), false);
			}
		}

		public int ZoomedImageWidth
		{
			get { return pictureBox.Width; }
		}

		public int ZoomedImageHeight
		{
			get { return pictureBox.Height; }
		}
	}
}
