using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	sealed class PrintJobDeleteCommandTest : TestCase
	{
		BusinessObjectFactory factory;

		Mock<IPrintJobView> view;
		Mock<IPrintJobManager> manager;

		protected override void SetUp()
		{
			base.SetUp();

			factory = new BusinessObjectFactory();

			view = new Mock<IPrintJobView>(MockBehavior.Strict);
			manager = new Mock<IPrintJobManager>(MockBehavior.Strict);
		}

		public void TestExecute_DeleteJobsIfUserConfirmsDeletion()
		{
			var job = StmPrintJob.New(factory);
			view.Setup(m => m.GetSelectedJobs()).Returns(new StmPrintJob[] { job });
			view.Setup(m => m.AskUserConfirmation(string.Format(PrintJobDeleteCommand.GetConfirmationMessage(1)))).Returns(true);
			manager.Setup(m => m.Delete(new StmPrintJob[] { job }));

			var cmd = new PrintJobDeleteCommand(view.Object, manager.Object);
			cmd.Execute();
			view.VerifyAll();
			AssertNoExceptionThrown(() => manager.VerifyAll());
		}

		public void TestExecute_DoesNotDeleteJobsIfUserNotConfirmedDeletion()
		{
			var job = StmPrintJob.New(factory);
			view.Setup(m => m.GetSelectedJobs()).Returns(new StmPrintJob[] { job });
			view.Setup(m => m.AskUserConfirmation(string.Format(PrintJobDeleteCommand.GetConfirmationMessage(1)))).Returns(false);
			var cmd = new PrintJobDeleteCommand(view.Object, manager.Object);
			cmd.Execute();
			view.VerifyAll();
			AssertNoExceptionThrown(() => view.VerifyAll());
		}

		public void TestExecute_DoNothingIfNoJobsWereSelected()
		{
			view.Setup(m => m.GetSelectedJobs()).Returns(System.Array.Empty<StmPrintJob>());
			var cmd = new PrintJobDeleteCommand(view.Object, manager.Object);
			cmd.Execute();
			view.VerifyAll();
			AssertNoExceptionThrown(() => view.VerifyAll());
		}
	}
}
