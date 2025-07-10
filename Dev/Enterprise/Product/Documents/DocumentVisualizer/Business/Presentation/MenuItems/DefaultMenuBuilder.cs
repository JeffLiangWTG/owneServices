using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DefaultMenuBuilder : IMenuBuilder
	{
		public IReadOnlyCollection<IMenuItemDescriptor> Build(object documentDataSource, IMenuItemBuilder menuItemBuilder, IResourceAccessor res, IMessageInstructions messageInstructions)
		{
			if (menuItemBuilder == null
				|| res == null)
			{
				return null;
			}

			return new IMenuItemDescriptor[]
			{
				menuItemBuilder.CreateDefaultMessagingMenuItem(res, messageInstructions),
				menuItemBuilder.Build(CommandIds.DeliverDocument),
				menuItemBuilder.Build(CommandIds.ResetOverriddenData),
				menuItemBuilder.Build(CommandIds.SaveOverriddenData),
				menuItemBuilder.CreateDefaultToolsMenuItem(res),
				menuItemBuilder.Build(CommandIds.Exit)
			}
			.Where(mi => mi != null)
			.ToArray();
		}
	}
}