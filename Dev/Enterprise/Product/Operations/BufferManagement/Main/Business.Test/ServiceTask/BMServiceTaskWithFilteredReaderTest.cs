using System.Linq;
using Enterprise.ServiceManager.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class BMServiceTaskWithFilteredReaderTestCase<T> : BMServiceTaskTestCase<T> where T : BMSServiceTaskBase
	{
		public void TestRun_ExecutedCommands_QueriesUseTableValuedParameters()
		{
			var task = GetNewServiceTask();
			InitializeProcessHeaders();
			InitialiseTaskSchedule(task);

			using (TestConnection.TrackExecutedCommands())
			{
				RunTaskSchedule(task);

				var executedCommand = TestConnection.ExecutedCommands.Where(c => (c.Contains("FROM dbo.ProcessHeader\r\n\tWHERE (FH_PK in ") && c.Contains("Top 200")));

				// 'SELECT Value FROM' will only be present in queryies that use TVPs
				foreach (string query in executedCommand)
				{
					AssertContains("WHERE (FH_PK in (SELECT Value FROM", query);
				}
			}
		}

		public void TestBMServiceTasks_ShouldBeActiveByDefault()
		{
			var task = GetNewServiceTask();
			InitialiseTaskSchedule(task, out var schedule);

			Assert(@"Our Service Task Options bizo should have set ActiveByDefault to true, because their impact is negligible performance-wise, and they only start doing anything when BM is enabled.
Please include ActiveByDefault = true in your task's HostedServiceAttribute.", ((StmServiceTask)schedule).SST_Active);
		}

		protected abstract T GetNewServiceTask();

		protected virtual void InitializeProcessHeaders()
		{
		}
	}
}
