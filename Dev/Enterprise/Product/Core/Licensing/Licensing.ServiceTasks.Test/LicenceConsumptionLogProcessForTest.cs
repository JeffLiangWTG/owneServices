using CargoWise.Types;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	sealed class LicenceConsumptionLogProcessForTest : ILicenceConsumptionLogProcess
	{
		public string Execute(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive)
		{
			++ExecuteCallCount;
			return "";
		}

		public int ExecuteCallCount;
	}
}
