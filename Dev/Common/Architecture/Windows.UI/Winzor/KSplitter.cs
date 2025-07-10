using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI.Layout;

namespace CargoWise.Windows.UI
{
	public class KSplitter : Splitter, ISplitterLayoutSaveProvider
	{
		public KSplitter() : base() { }

		protected override bool ApplicationDoEventsOnSplitterMove => false;

		int ISplitterLayoutSaveProvider.SplitterPosition
		{
			get => SplitPosition;
			set => SplitPosition = value;
		}

		int ISplitterLayoutSaveProvider.ContainerSize
		{
			get
			{
				var result = 0;
				if (Parent != null)
				{
					if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
					{
						result = Parent.Height;
					}
					else
					{
						result = Parent.Width;
					}
				}
				return result;
			}
		}

		bool ISplitterLayoutSaveProvider.IsSplitterFixed
		{
			get => DoNotSaveSplitterLayout;
		}

		bool ISplitterLayoutSaveProvider.IsLayoutRestored
		{
			get;
			set;
		}

		public bool DoNotSaveSplitterLayout { get; set; }
	}
}
