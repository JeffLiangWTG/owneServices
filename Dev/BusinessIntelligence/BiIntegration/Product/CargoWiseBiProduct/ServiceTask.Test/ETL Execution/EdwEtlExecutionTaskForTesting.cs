using Enterprise.Integration;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	class EdwEtlExecutionTaskForTesting : EdwEtlExecutionTask
	{
		public EdwEtlExecutionTaskForTesting()
		{ }

		public EdwEtlExecutionTaskForTesting(ILogger logger)
		{
			ServiceLogger = logger;
		}
	}
}
