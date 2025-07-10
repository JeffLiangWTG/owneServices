using System;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class SqlMutexLocksCleanerTest
	{
		[Test]
		public void TestReleaseServiceTaskLocksWithProcessIdAndServiceTaskCode()
		{
			// Arrange
			const int processId = 123;
			const string taskCode = "xxx";

			// Act
			sqlMutexLocksCleaner!.ReleaseLocksFromServiceTask(processId, taskCode);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				sqlMutexLockProviderMock
					!.Verify(x => x
							.ReleaseLocks(It.Is<SqlMutexLockInfo>(info =>
								info.Category == ExpectedCategory
								&& info.ProcessId == processId
								&& info.LockInfo == taskCode
								&& info.WorkStationName == System.Environment.MachineName)),
						Times.Once);
				sqlMutexLockProviderMock.VerifyNoOtherCalls();
			});
		}

		[Test]
		public void TestReleaseServiceTaskLocksWithJustProcessIdResultInNoActions()
		{
			// Arrange
			const int processId = 1234;

			// Act
			sqlMutexLocksCleaner!.ReleaseLocksFromServiceTask(processId, "");

			// Assert
			Assert.DoesNotThrow(() =>
			{
				sqlMutexLockProviderMock!.VerifyNoOtherCalls();
			});
		}

		[Test]
		public void TestReleaseLocksFromHost()
		{
			// Arrange
			// Act
			sqlMutexLocksCleaner!.ReleaseLocksFromHost();

			// Assert
			Assert.DoesNotThrow(() =>
			{
				sqlMutexLockProviderMock
					!.Verify(x => x
							.ReleaseLocks(It.Is<SqlMutexLockInfo>(info =>
								info.WorkStationName == System.Environment.MachineName)),
						Times.Once);
				sqlMutexLockProviderMock.VerifyNoOtherCalls();
			});
		}

		[Test]
		public void TestWrongCtorArgument()
		{
			// Arrange
			// Act
			// Assert
			Assert.Multiple(() =>
			{
				var exception = Assert.Throws<ArgumentNullException>(() => _ = new SqlMutexLocksCleaner(null));
				Assert.That(exception?.Message, Does.Contain("sqlMutexLockProvider"));
			});
		}

		[SetUp]
		public void SetUp()
		{
			sqlMutexLockProviderMock = new Mock<ISqlMutexLockProvider>();
			sqlMutexLocksCleaner = new SqlMutexLocksCleaner(sqlMutexLockProviderMock.Object);
		}

		SqlMutexLocksCleaner? sqlMutexLocksCleaner;
		Mock<ISqlMutexLockProvider>? sqlMutexLockProviderMock;
		const string ExpectedCategory = "PRC";
	}
}
