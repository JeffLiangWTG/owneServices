using System;
using System.Threading;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business
{
	public interface ICustomsServiceTaskProcess : IDisposable
	{
		LoggingInformation Logger { get; set; }
		void ExecuteBatch(CancellationToken token);
	}
}
