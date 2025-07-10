using System.Diagnostics;

namespace Enterprise.ServiceManager.Runner
{
	class CurrentProcessInfoProvider : ICurrentProcessInfoProvider
	{
		public TimeSpan TotalProcessorTime
		{
			get
			{
				using (var currentProcess = Process.GetCurrentProcess())
				{
					return currentProcess.TotalProcessorTime;
				}
			}
		}
	}
}
