using Enterprise.MasterFiles.Integration.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	public class DocumentJobTaskForTest : IDocumentJobTaskForTest
	{
		public void RunTask()
		{
			using (var documentJobTask = new DocumentJobTask())
			{
				documentJobTask.ServiceLogger = new TestServiceLogger();
				documentJobTask.RunTask();
			}
		}
	}
}
