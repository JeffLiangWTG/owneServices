using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class CreateMenuEvent
	{
		public CreateMenuEvent(IEnumerable<IMenuItemDescriptor> menuItems)
		{
			Argument.NotNull(menuItems, nameof(menuItems));

			this.menuItems = menuItems;
		}

		readonly IEnumerable<IMenuItemDescriptor> menuItems;

		public IEnumerable<IMenuItemDescriptor> MenuItems => menuItems;
	}
}