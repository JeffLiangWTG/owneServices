using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class CommandMenuItemBuilder : IMenuItemBuilder
	{
		public CommandMenuItemBuilder(IResourceAccessor res, IReadOnlyCollection<ICommand> commands)
		{
			Argument.NotNull(res, nameof(res));
			Argument.NotNull(commands, nameof(commands));

			this.res = res;
			this.commands = commands;
		}

		readonly IResourceAccessor res;
		readonly IReadOnlyCollection<ICommand> commands;

		IReadOnlyDictionary<string, ICommand> CommandMap => commandMap ?? (commandMap = CreateCommandMap());
		IReadOnlyDictionary<string, ICommand> commandMap;

		public IMenuItemDescriptor Build(string commandID)
		{
			if (CommandMap.TryGetValue(commandID, out ICommand command))
			{
				return new MenuItemDescriptorFromCommand(res, command);
			}

			return null;
		}

		public IMenuItemDescriptor Build(string caption, object image, IMenuItemDescriptor[] subMenuItems)
		{
			return new MenuItemDescriptor(
				caption,
				image,
				subMenuItems);
		}

		IReadOnlyDictionary<string, ICommand> CreateCommandMap()
		{
			return commands
				.ToDictionary(c => c.Id, c => c);
		}
	}
}
