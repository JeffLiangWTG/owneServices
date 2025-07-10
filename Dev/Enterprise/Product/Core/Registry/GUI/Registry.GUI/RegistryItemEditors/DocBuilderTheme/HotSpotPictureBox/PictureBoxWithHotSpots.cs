using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.Registry.GUI.HotSpotPictureBox
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class PictureBoxWithHotSpots : PictureBox
	{
		Bitmap mapBitmap;
		public event HotSpotEventHandler HotSpotClick;

		public PictureBoxWithHotSpots()
		{
			MouseClick += new MouseEventHandler(PictureBoxWithHotSpots_MouseClick);
		}

		void PictureBoxWithHotSpots_MouseClick(object sender, MouseEventArgs e)
		{
			var hotSpot = GetHotSpot(e.X, e.Y);
			OnHotSpotClick(new HotSpotEventArgs(hotSpot));
		}

		protected virtual void OnHotSpotClick(HotSpotEventArgs e)
		{
			if (HotSpotClick != null)
			{
				HotSpotClick(this, e);
			}
		}

		public Bitmap MapBitmap
		{
			get { return mapBitmap; }
			set { mapBitmap = value; }
		}

		HotSpot GetHotSpot(int x, int y)
		{
			if (MapBitmap == null || x >= MapBitmap.Width || y >= MapBitmap.Height)
			{
				return null;
			}
			else
			{
				return GetHotSpot(MapBitmap.GetPixel(x, y));
			}
		}

		HotSpot GetHotSpot(Color color)
		{
			return Array.Find<HotSpot>(HotSpots.ToArray(), hotSpot => hotSpot.Color.ToArgb().Equals(color.ToArgb()));
		}

		readonly List<HotSpot> hotSpots = new List<HotSpot>();
		public List<HotSpot> HotSpots
		{
			get { return hotSpots; }
		}

		protected override void Dispose(bool disposing)
		{
			MouseClick -= new MouseEventHandler(PictureBoxWithHotSpots_MouseClick);
			base.Dispose(disposing);
		}

#if DEBUG
		internal void PerformMouseClickForTesting(MouseEventArgs e)
		{
			PictureBoxWithHotSpots_MouseClick(this, e);
		}
#endif
	}
}
