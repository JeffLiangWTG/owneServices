using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.Module
{
	public interface IPrintJobView
	{
		StmPrintJob[] GetSelectedJobs();
		bool AskUserConfirmation(string message);
	}

	public class PrintJobDeleteCommand
	{
		readonly IPrintJobView view;
		readonly IPrintJobManager manager;

		public PrintJobDeleteCommand(IPrintJobView view, IPrintJobManager manager)
		{
			this.view = view;
			this.manager = manager;
		}

		public void Execute()
		{
			StmPrintJob[] selected = view.GetSelectedJobs();
			if (selected.Length == 0)
			{
				return;
			}

			bool confirmed = view.AskUserConfirmation(string.Format(GetConfirmationMessage(selected.Length)));
			if (!confirmed)
			{
				return;
			}

			manager.Delete(selected);
		}

		public static string GetConfirmationMessage(int numberOfJobs)
		{
			return Res.GetString("00d91eca-f597-4f37-a22e-a996a307192e", "Continue to delete {0} print job(s)?", numberOfJobs);
		}
	}
}
