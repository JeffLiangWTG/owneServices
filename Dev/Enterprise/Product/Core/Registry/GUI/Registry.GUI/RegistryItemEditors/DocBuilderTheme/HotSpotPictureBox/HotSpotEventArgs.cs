using System;

namespace Enterprise.Registry.GUI.HotSpotPictureBox
{
	public class HotSpotEventArgs : EventArgs
	{
		readonly HotSpot hotSpot;

		public HotSpotEventArgs(HotSpot hotSpot)
		{
			this.hotSpot = hotSpot;
		}

		public HotSpot HotSpot
		{
			get { return hotSpot; }
		}
	}
}
