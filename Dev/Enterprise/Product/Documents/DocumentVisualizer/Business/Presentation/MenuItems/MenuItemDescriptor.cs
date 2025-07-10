using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	[DebuggerDisplay("{" + nameof(Caption) + "}")]
	sealed class MenuItemDescriptor : IMenuItemDescriptor
	{
		public MenuItemDescriptor(string caption, object image, Action action, Func<bool> isEnabledRule = null, Func<bool> isVisibleRule = null)
		{
			Caption = caption;
			Image = image;
			this.action = action;
			this.isEnabledRule = isEnabledRule;
			this.isVisibleRule = isVisibleRule;
			this.menuItems = Array.Empty<IMenuItemDescriptor>();
		}

		public MenuItemDescriptor(string caption, object image, IMenuItemDescriptor[] menuItems)
		{
			Caption = caption;
			Image = image;
			this.isEnabledRule = () => this.menuItems.Any(mi => mi.IsEnabled());
			this.isVisibleRule = () => this.menuItems.Any(mi => mi.IsVisible());
			this.menuItems = menuItems ?? Array.Empty<IMenuItemDescriptor>();
		}

		readonly Action action;
		readonly Func<bool> isEnabledRule;
		readonly Func<bool> isVisibleRule;
		readonly IMenuItemDescriptor[] menuItems;

		public string Caption { get; }
		public object Image { get; }

		public void Invoke() => action();
		public bool IsEnabled() => isEnabledRule();
		public bool IsVisible() => isVisibleRule();

		public IEnumerable<IMenuItemDescriptor> MenuItems => menuItems;
	}
}
