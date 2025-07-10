using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class Exit : CommandProvider
	{
		readonly Command exit = new Command(CommandIds.Exit, Res.GetString("87f8ff87-b5e1-4578-8a70-3987de799347", "Exit"));

		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield return exit;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
			exit.Invoker = _ =>
			{
				documentInfo.Services.Resolve<IEventBroker>().Publish(new ExitEvent(documentInfo.Document));
			};
		}
	}
}
