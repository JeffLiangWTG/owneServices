using System;

namespace Enterprise.ServiceManager.Host
{
	public class ServiceTaskCodeWithRunnerProcessId : Tuple<string, int>
	{
		public ServiceTaskCodeWithRunnerProcessId(string item1, int item2) : base(item1, item2)
		{
		}
	}
}
