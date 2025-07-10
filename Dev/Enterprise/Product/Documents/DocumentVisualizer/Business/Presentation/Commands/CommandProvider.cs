using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	abstract class CommandProvider : ICommandProvider
	{
		public IEnumerable<ICommand> Commands => commands ?? (commands = CreateCommandsCore().ToArray());
		ICommand[] commands;

		protected abstract IEnumerable<ICommand> CreateCommandsCore();

		public void OnDocumentInfosCreated(IDocumentInfo documentInfo)
		{
			OnDocumentInfoCreatedCore(documentInfo);
		}

		protected abstract void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo);
	}
}