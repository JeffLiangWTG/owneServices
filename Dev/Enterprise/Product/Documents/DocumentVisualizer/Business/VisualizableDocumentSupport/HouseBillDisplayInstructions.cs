using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class HouseBillDisplayInstructions : IDisplayInstructions
	{
		public HouseBillDisplayInstructions(IServiceContainer services, IMessageInstructions messageInstructions, IReadOnlyCollection<ICommand> commands)
		{
			Argument.NotNull(services, nameof(services));
			Argument.NotNull(commands, nameof(commands));

			this.services = services;
			this.messageInstructions = messageInstructions;
			this.commands = commands;
		}

		readonly IServiceContainer services;
		readonly IMessageInstructions messageInstructions;
		readonly IReadOnlyCollection<ICommand> commands;

		public bool ShowEvents => messageInstructions.AllowSendMessage;
		public bool ShowLastEventDetails => messageInstructions.AllowSendMessage;
		public IEnumerable<IMenuItemDescriptor> MenuItems => menuItems ?? (menuItems = CreateMenuItems());
		IReadOnlyCollection<IMenuItemDescriptor> menuItems;

		IReadOnlyCollection<IMenuItemDescriptor> CreateMenuItems()
		{
			var res = services.Resolve<IResourceAccessor>();
			var menuItemBuilder = new CommandMenuItemBuilder(res, commands);
			var menuBuilder = new DefaultMenuBuilder();
			return menuBuilder.Build(null, menuItemBuilder, res, messageInstructions);
		}
	}
}
