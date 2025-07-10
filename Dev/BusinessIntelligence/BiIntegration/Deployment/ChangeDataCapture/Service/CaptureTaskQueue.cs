using Enterprise.ChangeDataCapture.Common;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.ChangeDataCapture.Service.CaptureTask.ServiceTaskCode,
	Enterprise.ChangeDataCapture.Service.CaptureTask.ServiceTaskName,
	typeof(Enterprise.ChangeDataCapture.Service.CaptureTaskQueue))]

namespace Enterprise.ChangeDataCapture.Service
{
	public class CaptureTaskQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = CdcScanner.GetQueueSizeAndAge();
				return result.Count switch
				{
					-1 => QueueResult.Error,
					0 => QueueResult.Zero,
					_ => new QueueResult(result.Count, result.Age),
				};
			}
		}
	}
}
