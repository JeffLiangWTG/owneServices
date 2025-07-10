using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class ToolStripAdapter : IMenuItemCollection
	{
		public ToolStripAdapter(ToolStrip toolStrip)
		{
			Argument.NotNull(toolStrip, nameof(toolStrip));

			this.toolStrip = toolStrip;
		}

		readonly ToolStrip toolStrip;
		readonly List<Action> handlers = new List<Action>();

		void IMenuItemCollection.Add(IMenuItemDescriptor menuItemDescriptor)
		{
			toolStrip.Items.Add(menuItemDescriptor.CreateToolStripItem(handlers));
		}

		void IMenuItemCollection.Refresh()
		{
			handlers.ForEach(handler => handler());
		}

		void IMenuItemCollection.Clear()
		{
			toolStrip.Items.Clear();
			handlers.Clear();
		}
	}
}