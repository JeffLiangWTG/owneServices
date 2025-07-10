using System;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class ServiceTaskErrorTrackerTest : TestCase
	{
		protected override void SetUp()
		{
			serviceHostMessageDispatcherMock = new Mock<IServiceHostMessageDispatcher>();
			serviceTaskErrorTracker = new ServiceTaskErrorTracker(serviceHostMessageDispatcherMock.Object);
		}

		[ExpectNoExceptions]
		public void TestTrackServiceTaskErrorDoesNothingIfTaskCodeIsEmpty()
		{
			// Arrange
			// Act
			serviceTaskErrorTracker.TrackServiceTaskError(null);
			serviceTaskErrorTracker.TrackServiceTaskError(string.Empty);

			// Assert
			serviceHostMessageDispatcherMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestTrackServiceTaskError()
		{
			CombineAssertions(() =>
			{
				Test("Code1");
				Test("Code2");
			});

			void Test(string code)
			{
				// Arrange
				// Act
				serviceTaskErrorTracker.TrackServiceTaskError(code);

				// Assert
				serviceHostMessageDispatcherMock.Verify(dispatcher => dispatcher.SendErrorReport(code), Times.Once);
				serviceHostMessageDispatcherMock.VerifyNoOtherCalls();
			}
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceTaskErrorTracker(null));
			AssertEquals("serviceHostMessageDispatcher", result.ParamName);
		}

		Mock<IServiceHostMessageDispatcher> serviceHostMessageDispatcherMock;
		ServiceTaskErrorTracker serviceTaskErrorTracker;
	}
}
