using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class ServiceStopperTest
	{
		[SetUp]
		public void SetUp()
		{
			delayProviderMock = new Mock<IDelayProvider>();
			cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

			serviceStopper = new ServiceStopper(delayProviderMock.Object, cancellationTokenProviderMock.Object);
		}

		[Test]
		public void TestWrongConstructorParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => new ServiceStopper(null, cancellationTokenProviderMock!.Object));
				Assert.That(result?.ParamName, Is.EqualTo("delayProvider"));

				result = Assert.Throws<ArgumentNullException>(() => new ServiceStopper(delayProviderMock!.Object, null));
				Assert.That(result?.ParamName, Is.EqualTo("cancellationTokenProvider"));
			});
		}

		[Test]
		public void TestWaitForServiceStopRequestCallsDelay()
		{
			// Arrange
			var cancellationToken = new CancellationToken();

			cancellationTokenProviderMock!.Setup(provider => provider.Token).Returns(cancellationToken);

			var timeSpan = TimeSpan.FromSeconds(10);

			// Act
			var result = serviceStopper!.WaitForServiceStopRequest(timeSpan);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				Assert.That(result, Is.EqualTo(false));
				delayProviderMock!.Verify(provider => provider.Delay(It.IsAny<TimeSpan>()), Times.Never);
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
				delayProviderMock.Verify(provider => provider.Delay(timeSpan, cancellationToken), Times.Once);
			});
		}

		[Test]
		public void TestWaitForServiceStopRequestCancellationTokenTimeout()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));

			cancellationTokenProviderMock!.Setup(provider => provider.Token).Returns(cancellationTokenSource.Token);

			var delayProvider = new DelayProvider();
			delayProviderMock
				!.Setup(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Callback<TimeSpan, CancellationToken>((ts, ct) => delayProvider.Delay(ts, ct));

			// Act
			var result = serviceStopper!.WaitForServiceStopRequest(TimeSpan.FromSeconds(15));

			// Assert
			Assert.That(result, Is.EqualTo(true));
		}

		[Test]
		public void TestWaitForServiceStopRequestTokenCancelled()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();

			cancellationTokenProviderMock!.Setup(provider => provider.Token).Returns(cancellationTokenSource.Token);

			delayProviderMock
				!.Setup(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Callback(cancellationTokenSource.Cancel);

			// Act
			var result = serviceStopper!.WaitForServiceStopRequest(TimeSpan.FromSeconds(15));

			// Assert
			Assert.That(result, Is.EqualTo(true));
		}

		Mock<IDelayProvider>? delayProviderMock;
		ServiceStopper? serviceStopper;
		Mock<ICancellationTokenProvider>? cancellationTokenProviderMock;
	}
}
