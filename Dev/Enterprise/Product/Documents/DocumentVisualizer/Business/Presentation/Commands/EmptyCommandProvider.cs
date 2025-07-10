using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class EmptyCommandProvider : CommandProvider
	{
		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield break;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
		}
	}
}