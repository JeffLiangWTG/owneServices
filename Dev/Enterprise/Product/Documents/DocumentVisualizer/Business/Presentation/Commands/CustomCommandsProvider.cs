using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class CustomCommandsProvider : ICommandProvider
	{
		public CustomCommandsProvider(IVisualizableDocumentSupporter supporter, string dataContext)
		{
			this.supporter = Argument.NotNull(supporter, nameof(supporter));
			this.dataContext = Argument.NotNullOrEmpty(dataContext, nameof(dataContext));
		}

		readonly IVisualizableDocumentSupporter supporter;
		readonly string dataContext;

		public IEnumerable<ICommand> Commands => CustomCommands;

		ICommand[] CustomCommands => customCommands ?? (customCommands = supporter.GetCustomCommands(dataContext)?.ToArray() ?? Array.Empty<ICommand>());
		ICommand[] customCommands;

		public void OnDocumentInfosCreated(IDocumentInfo documentInfo)
		{
			foreach (var customCommand in CustomCommands.OfType<INotifiableDocumentInfoCreated>())
			{
				customCommand.NotifyDocumentInfoCreated(documentInfo);
			}
		}
	}
}
