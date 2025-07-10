using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	class MainFormTask<T> : IPostLoginTask where T : IPostLoginTask, new()
	{
		internal MainFormTask()
		{
			innerTask = new T();
		}

		public string TaskDescription
		{
			get { return innerTask.TaskDescription; }
		}

		public bool ShouldExecute()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			return mainForm != null && innerTask.ShouldExecute();
		}

		public void Execute()
		{
			innerTask.Execute();
		}

		readonly T innerTask;
	}
}
