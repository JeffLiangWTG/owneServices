using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	abstract class TNTServiceTaskTest<T> : ServiceTaskTestCase<T> where T : TNTServiceTask
	{
		public void TestRunServiceTask()
		{
			TestRunServiceTaskCore();
		}

		protected abstract void TestRunServiceTaskCore();
	}
}
