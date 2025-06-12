using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using eServices.ApplicationEvent.EnrichmentService.Kafka;
using KafkaFlow.Retry;
using Microsoft.Extensions.Logging.Testing;

namespace eServices.ApplicationEvent.Tests.Kafka;

public class FaultRetryHandlerTests
{
	private static readonly List<(Exception Exception, bool ShouldHandle)> FaultRetryHandler_Handle_Cases = [
		(new InvalidOperationException(), false),
		(new TaskCanceledException(), false),
		(new IOException() { Source = "Confluent.Kafka" }, true)
	];

	[TestCaseSource(nameof(FaultRetryHandler_Handle_Cases))]
	public void FaultRetryHandler_Handle((Exception Exception, bool ShouldHandle) test)
	{
		var metrics = new Mock<IMetrics>();

		var handler = new FaultRetryHandler(metrics.Object, new FakeLogger<FaultRetryHandler>());
		var handled = handler.Handle(new RetryContext(test.Exception));

		Assert.That(handled, Is.EqualTo(test.ShouldHandle));

		metrics.Verify(x => x.AddFaults(1), test.ShouldHandle ? Times.Once : Times.Never);
	}
}
