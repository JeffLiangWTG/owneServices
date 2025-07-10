using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	interface ICommandProvider
	{
		IEnumerable<ICommand> Commands { get; }
		void OnDocumentInfosCreated(IDocumentInfo documentInfo);
	}
}