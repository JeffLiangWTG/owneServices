using System;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public class ServiceHostMessageDispatcher : IServiceHostMessageDispatcher
	{
		static void Send(string message)
		{
			Console.Out.WriteLine(message);
		}

		public void SendErrorReport(string taskCode)
		{
			Send($"{ServiceManagerHelper.RunnerToHostCommunicationServiceTaskErrorCommandPrefix}:{taskCode}");
		}

		public void SendGrpcPortLockAcquired(int port)
		{
			Send($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:{port}");
		}
	}
}
