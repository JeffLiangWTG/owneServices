using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class LazyContextMenuStrip : KContextMenuStrip
	{
		public LazyContextMenuStrip(Action<KContextMenuStrip, Control> addItemsToStrip, bool clearOnClose)
		{
			addItems = addItemsToStrip;
			Items.Add(new ZToolStripMenuItem("-"));

			Opening += Strip_Opening;

			if (clearOnClose)
			{
				Closing += Strip_Closing;
			}
		}

		readonly Action<KContextMenuStrip, Control> addItems;

		void Strip_Opening(object sender, CancelEventArgs e)
		{
			var strip = GetStrip(sender);
			var sourceControl = GetSourceControl(strip);
			if (sourceControl != null && strip.Items.Count <= 1)
			{
				AddItems(strip, sourceControl);
			}
		}

		void Strip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
		{
			Clear();
			Items.Add(new ZToolStripMenuItem("-"));
		}

		static KContextMenuStrip GetStrip(object sender)
		{
			return sender as KContextMenuStrip;
		}

		void AddItems(KContextMenuStrip strip, Control sourceControl)
		{
			Clear();
			ItemsAdding?.Invoke(strip, EventArgs.Empty);
			addItems?.Invoke(strip, sourceControl);
		}

		public event EventHandler ItemsAdding;

		void Clear()
		{
			Items.Clear();
		}

		static Control GetSourceControl(KContextMenuStrip strip)
		{
			return strip?.SourceControl;
		}

#if DEBUG

		public LazyContextMenuStrip AddItems_ForTest(Control control)
		{
			AddItems(this, control);
			return this;
		}

#endif
	}
}
