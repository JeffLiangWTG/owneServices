using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZFilterGridMenuItem : ZMenuItem
	{
		public ZFilterGridMenuItem()
		{
		}

		public ZFilterGridMenuItem(string text)
			: base(text)
		{
		}

		public ZFilterGridMenuItem(string text, EventHandler onClick)
			: base(text, onClick)
		{
		}

		public ZFilterGridMenuItem(MultilingualString text)
			: base(text)
		{
		}

		public ZFilterGridMenuItem(MultilingualString text, EventHandler onClick)
			: base(text, onClick)
		{
		}

		public bool ShowInToolbar
		{
			get { return fShowInToolbar; }
			set { fShowInToolbar = value; }
		}

		bool fShowInToolbar = true;
	}
}
