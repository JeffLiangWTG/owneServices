using System;
using System.Diagnostics;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;
using Environment = System.Environment;

namespace CargoWise.ServiceManager.Runner.Test.Locker
{
	class SqlMutexLockerTest : TestCase
	{
		public void TestTryAcquireLockTakesTheLock()
		{
			var expectedTimeSpan = TimeSpan.FromSeconds(Env.Registry.ServiceTaskHeartbeatDurationSeconds);
			var expectedUserCode = ((GlbStaff)Env.CurrentUser)?.GS_Code ?? User.ServiceUserCode;
			var expectedWorkStation = Environment.MachineName;
			var expectedProcessId = Process.GetCurrentProcess().Id;

			CombineAssertions(() =>
			{
				Test("XXX", true);
				Test("YYY", true);
				Test("ZZZ", false);

				void Test(string code, bool expectSuccess)
				{
					var expectedLock = new Mock<ISqlMutexLock>().Object;
					var lockProviderMock = new Mock<ISqlMutexLockProvider>();
					var loggerMock = new Mock<IRunnerLogger>();
					var mutexLocker = new SqlMutexLocker(lockProviderMock.Object, loggerMock.Object);

					int lockResult = 0;
					var returnMessage = string.Empty;
					lockProviderMock.Invocations.Clear();
					lockProviderMock.Setup(x => x
							.TryGetLock(
								out expectedLock,
								out lockResult,
								out returnMessage,
								It.IsAny<string>(),
								It.IsAny<string>(),
								It.IsAny<TimeSpan>(),
								It.IsAny<string>(),
								It.IsAny<string>(),
								It.IsAny<int?>(),
								It.IsAny<bool>()))
						.Returns(expectSuccess)
						.Verifiable();

					var result = mutexLocker.TryAcquireLock(code, out var disposableLock);
					AssertEquals(expectSuccess, result);
					AssertEquals(expectSuccess ? expectedLock : null, disposableLock);

					lockProviderMock.Verify(x => x
						.TryGetLock(
							out expectedLock,
							out lockResult,
							out returnMessage,
							code,
							ExpectedCategory,
							expectedTimeSpan,
							expectedUserCode,
							expectedWorkStation,
							expectedProcessId,
							true),
						Times.Once);

					lockProviderMock.VerifyNoOtherCalls();

					if (expectSuccess)
					{
						loggerMock.Verify(x => x.Log(It.IsAny<LogLevel>(), It.Is<string>(message => message.StartsWith($"Mutex lock acquired {ExpectedCategory}:{code}, ReturnMessage: "))), Times.Once);
					}
					else
					{
						loggerMock.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Never);
					}
				}
			});
		}

		public void TestTryAcquireLockFailedWithReportableError()
		{
			var expectedTimeSpan = TimeSpan.FromSeconds(Env.Registry.ServiceTaskHeartbeatDurationSeconds);
			var expectedUserCode = ((GlbStaff)Env.CurrentUser)?.GS_Code ?? User.ServiceUserCode;
			var expectedWorkStation = Environment.MachineName;
			var expectedProcessId = Process.GetCurrentProcess().Id;

			var expectedLock = new Mock<ISqlMutexLock>().Object;
			var lockProviderMock = new Mock<ISqlMutexLockProvider>();
			var loggerMock = new Mock<IRunnerLogger>();
			var mutexLocker = new SqlMutexLocker(lockProviderMock.Object, loggerMock.Object);

			CombineAssertions(() =>
			{
				Test(0, false);
				Test(2601, false);
				Test(701, true);
				Test(3906, true);
				Test(845, true);
				Test(802, true);
				Test(1205, true);

				void Test(int lockResult, bool reportable)
				{
					const string code = "ZZZ";
					var returnMessage = nameof(TestTryAcquireLockFailedWithReportableError);

					lockProviderMock.Invocations.Clear();
					lockProviderMock.Setup(x => x
							.TryGetLock(
								out expectedLock,
								out lockResult,
								out returnMessage,
								It.IsAny<string>(),
								It.IsAny<string>(),
								It.IsAny<TimeSpan>(),
								It.IsAny<string>(),
								It.IsAny<string>(),
								It.IsAny<int?>(),
								It.IsAny<bool>()))
						.Returns(false)
						.Verifiable();

					var result = mutexLocker.TryAcquireLock(code, out var disposableLock);
					AssertEquals(false, result);
					AssertNull(disposableLock);

					var expectedLogMessage = $"Failed to acquire a lock on PRC:{code}, reason: {lockResult}.{System.Environment.NewLine}{returnMessage}";
					if (reportable)
					{
						loggerMock.Verify(x => x.Log(LogLevel.Information, expectedLogMessage), Times.Once);
					}
					else
					{
						loggerMock.Verify(x => x.Log(LogLevel.Information, expectedLogMessage), Times.Never);
					}

					lockProviderMock.Verify(x => x
						.TryGetLock(
							out expectedLock,
							out lockResult,
							out returnMessage,
							code,
							ExpectedCategory,
							expectedTimeSpan,
							expectedUserCode,
							expectedWorkStation,
							expectedProcessId,
							true),
						Times.Once);
					lockProviderMock.VerifyNoOtherCalls();
				}
			});
		}

		public void TestLockWasAcquiredThenLost()
		{
			var expectedLockMock = new Mock<ISqlMutexLock>();
			var expectedLock = expectedLockMock.Object;
			var lockProviderMock = new Mock<ISqlMutexLockProvider>();
			var loggerMock = new Mock<IRunnerLogger>();
			var mutexLocker = new SqlMutexLocker(lockProviderMock.Object, loggerMock.Object);

			const string code = "xxx";
			var lockResult = 0;
			var returnMessage = string.Empty;

			// Arrange
			lockProviderMock.Setup(x => x
					.TryGetLock(
						out expectedLock,
						out lockResult,
						out returnMessage,
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<TimeSpan>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<int?>(),
						It.IsAny<bool>()))
				.Returns(true)
				.Verifiable();

			// Act
			var result = mutexLocker.TryAcquireLock(code, out var disposableLock);
			expectedLockMock.Raise(x => x.OnLockLost += null, new SqlMutexLockEventArgs(code, ExpectedCategory, -1));

			// Assert
			AssertEquals(true, result);
			AssertEquals(expectedLock, disposableLock);
			loggerMock.Verify(x =>
				x.Log(LogLevel.Error, FormattableString.Invariant($"Mutex lock lost to {ExpectedCategory}:{code}, reason: -1.")), Times.Once());
			disposableLock?.Dispose();
		}

		const string ExpectedCategory = "PRC";
	}
}
