using CargoWise.Types;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Business.Testing
{
	public abstract class GBServiceTaskTestCase<T> : ServiceTaskTestCase<T> where T : ServiceProviderImpl, new()
	{
		public T GetInstance()
		{
			return new T();
		}

		public void TestServiceTaskLogsStartAndFinish()
		{
			var serviceTask = GetInstance();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var log = logger.ToString();
				AssertContains(ZString.Format("Starting service task {0}", ServiceTaskCode), log);
				AssertContains(ZString.Format("Finished service task {0}", ServiceTaskCode), log);
			});
		}

		protected ZString ServiceTaskCode => ServiceTaskCodeCore;

		protected abstract ZString ServiceTaskCodeCore { get; }
	}
}
