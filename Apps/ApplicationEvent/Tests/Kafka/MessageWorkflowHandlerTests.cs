using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using eServices.ApplicationEvent.EnrichmentService.Kafka;
using KafkaFlow;
using Microsoft.Extensions.Hosting;

namespace eServices.ApplicationEvent.Tests.Kafka;

public class MessageWorkflowHandlerTests
{
	[Test]
	public async Task MessageWorkflowHandler_Success()
	{
		var errorReporting = new Mock<IErrorReporting>();
		var metrics = new Mock<IMetrics>();
		var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();
		var message = new Mock<IMessageContext>();
		bool nextCalled = false;
		var next = new MiddlewareDelegate(async _ => { nextCalled = true; await Task.CompletedTask; });

		var handler = new MessageWorkflowHandler(errorReporting.Object, metrics.Object, hostApplicationLifetime.Object);
		await handler.Invoke(message.Object, next);

		Assert.DoesNotThrowAsync(async () => await handler.Invoke(message.Object, next));
		metrics.Verify(x => x.AddMsgsRcvd(1));
		Assert.That(nextCalled, Is.True);
	}

	[Test]
	public void MessageWorkflowHandler_Fail()
	{
		var errorReporting = new Mock<IErrorReporting>();
		var metrics = new Mock<IMetrics>();
		var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();
		hostApplicationLifetime.SetupGet(x => x.ApplicationStopping).Returns(CancellationToken.None);
		var message = new Mock<IMessageContext>();
		bool nextCalled = false;
		var next = new MiddlewareDelegate(async _ => { nextCalled = true; await Task.FromException(new TaskCanceledException()); });

		var handler = new MessageWorkflowHandler(errorReporting.Object, metrics.Object, hostApplicationLifetime.Object);
		Assert.ThrowsAsync<TaskCanceledException>(() => handler.Invoke(message.Object, next));

		metrics.Verify(x => x.AddMsgsRcvd(1));
		Assert.That(nextCalled, Is.True);
		errorReporting.Verify(x => x.ReportErrorAsync(It.IsAny<Exception>()), Times.Once);
	}

	[Test]
	public void MessageWorkflowHandler_Stopping()
	{
		var errorReporting = new Mock<IErrorReporting>();
		var metrics = new Mock<IMetrics>();
		var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();
		hostApplicationLifetime.SetupGet(x => x.ApplicationStopping).Returns(new CancellationToken(true));
		var message = new Mock<IMessageContext>();
		bool nextCalled = false;
		var next = new MiddlewareDelegate(async _ => { nextCalled = true; await Task.FromException(new TaskCanceledException()); });

		var handler = new MessageWorkflowHandler(errorReporting.Object, metrics.Object, hostApplicationLifetime.Object);
		Assert.ThrowsAsync<TaskCanceledException>(() => handler.Invoke(message.Object, next));

		metrics.Verify(x => x.AddMsgsRcvd(1));
		Assert.That(nextCalled, Is.True);
		errorReporting.Verify(x => x.ReportErrorAsync(It.IsAny<Exception>()), Times.Never);
	}
}
