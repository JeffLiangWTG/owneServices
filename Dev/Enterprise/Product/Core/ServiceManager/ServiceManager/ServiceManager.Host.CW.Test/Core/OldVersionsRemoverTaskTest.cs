using System;
using System.Threading;
using CargoWise.Async;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class OldVersionsRemoverTaskTest : TestCase
	{
		public void TestOldVersionsRemoverTaskStartsOldVersionsRemover()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			// Act
			try
			{
				// Assert
				AssertNoExceptionThrown(() => new TestableOldVersionsRemoverTask(new Mock<IHostLogger>().Object).Run(cancellationToken));
			}
			finally
			{
				// Cleanup
				cancellationTokenSource.Cancel();
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		public void TestWrongConstructorParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new OldVersionsRemoverTask(null));
			AssertEquals("hostLogger", result.ParamName);
		}

		class TestableOldVersionsRemoverTask : OldVersionsRemoverTask
		{
			public TestableOldVersionsRemoverTask(IHostLogger hostLogger) : base(hostLogger)
			{
			}

			protected override void RunOldVersionsRemover(CancellationToken cancellationToken)
			{
				TestableOldVersionsRemover.Run(cancellationToken);
			}
		}

		static class TestableOldVersionsRemover
		{
			public static void Run(CancellationToken cancellationToken)
			{
				cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
			}
		}
	}
}
