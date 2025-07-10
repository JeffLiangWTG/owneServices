using System;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	public class CancellationTokenSourceWrapperTest
	{
		[Test]
		public void TestGetCancellationTokenWithDefaultReturnsToken()
		{
			// Arrange
			using var wrapper = new CancellationTokenSourceWrapper();

			// Act
			var result = wrapper.Token;

			// Assert
			Assert.That(result, Is.Not.EqualTo(CancellationToken.None));
		}

		[Test]
		public void TestCancelWithTokenTriggersCancellation()
		{
			// Arrange
			using var wrapper = new CancellationTokenSourceWrapper();

			// Act
			wrapper.Cancel();

			// Assert
			Assert.That(wrapper.Token.IsCancellationRequested, Is.EqualTo(true));
		}

		[Test]
		public void TestDisposeWithMultipleCallsNoException()
		{
			// Arrange
			var wrapper = new CancellationTokenSourceWrapper();

			// Act
			wrapper.Dispose();
			wrapper.Dispose();
		}

		[Test]
		public void TestCancelWithMultipleDisposeNoException()
		{
			// Arrange
			using var wrapper = new CancellationTokenSourceWrapper();

			// Act
			wrapper.Cancel();
			wrapper.Cancel();
		}

		[Test]
		public void TestGetCancellationTokenAfterDisposedThrowsException()
		{
			// Arrange
			var wrapper = new CancellationTokenSourceWrapper();
			wrapper.Dispose();

			// Act
			Assert.Throws<ObjectDisposedException>(() => _ = wrapper.Token);
		}
	}
}
