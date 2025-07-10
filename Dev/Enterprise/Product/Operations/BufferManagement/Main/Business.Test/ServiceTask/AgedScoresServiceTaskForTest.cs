using Enterprise.Environment;
using Enterprise.PAVE.MENT.Business.ServiceTasks;

namespace Enterprise.BufferManagement.Business.Test
{
	public class AgedScoresServiceTaskForTest : AgedScoresServiceTask
	{
		public void Run()
		{
			ServiceLogger = new BufferManagementLogger();

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				RunTaskCore();
			}
		}
	}
}
