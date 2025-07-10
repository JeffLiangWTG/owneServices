using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class CreateWorkItemsService : ICreateWorkItemsService
	{
		readonly IWinzorDispatcher dispatcher;

		public CreateWorkItemsService(IWinzorDispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public async void ShowMessage(string message)
		{
			await dispatcher.InvokeAsync(() =>
			{
				Globals.Message.Show(message, "Error", MessageBoxButtons.OK, DialogResult.OK);
			});
		}
	}
}

