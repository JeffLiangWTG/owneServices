using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.EnvironmentCheckers;

class UndisposedSqlLockCheckerTest : TestCase
{
	protected override void SetUp()
	{
		base.SetUp();
		serviceTaskConfigMock = Mock.Of<IHostedServiceAttribute>(x =>
			x.Code == taskCode &&
			x.TypeName == "typeName1" &&
			x.TypeAssemblyName == "assemblyName1"
		);
		serviceTaskMock = Mock.Of<IServiceTaskHandler>(x => x.HostedServiceAttribute == serviceTaskConfigMock);
	}

	public void TestCheckOnServiceTaskCompletionCleansUpLocksAndThrowsUndisposedSqlLockException()
	{
		// Arrange
		var lockKey1 = $"{lockKeyPrefix}{Db.Connection.GetSqlLockThresholds()}";
		var lockKey2 = $"{lockKeyPrefix}{Db.Connection.GetSqlLockThresholds() + 1}";
		Db.Connection.TryGetLock(lockKey1, out sqlLock);
		Db.Connection.TryGetLock(lockKey2, out sqlLock);

		// Act
		var exception = AssertExceptionThrown<UndisposedSqlLockException>(() =>
		{
			new UndisposedSqlLockChecker().CheckOnServiceTaskCompletion(serviceTaskMock);
		});

		// Assert
		AssertEquals(false, Db.Connection.HasSqlLocks);
		AssertEquals(true, exception.Message.Contains(taskCode));
		AssertEquals(true,
			exception.Message.StartsWith($@"The service task code: '{taskCode} - typeName1, assemblyName1' has one or more undisposed SQL locks at the end of the run."));
		AssertEquals(true, exception.Message.Contains($"Undisposed lock keys:{System.Environment.NewLine}{lockKey1}{System.Environment.NewLine}{lockKey2}"));
	}

	public void TestCheckOnServiceTaskExceptionCleansUpLocksAndThrowsUndisposedSqlLockException()
	{
		// Arrange
		var lockKey = lockKeyPrefix + Db.Connection.GetSqlLockThresholds();
		Db.Connection.TryGetLock(lockKey, out sqlLock);

		// Act
		var exception = AssertExceptionThrown<UndisposedSqlLockException>(() =>
		{
			new UndisposedSqlLockChecker().CheckOnServiceTaskException(serviceTaskMock);
		});

		// Assert
		AssertEquals(false, Db.Connection.HasSqlLocks);
		AssertEquals(true, exception.Message.Contains(taskCode));
		AssertEquals(true,
			exception.Message.StartsWith($@"The service task code: '{taskCode} - typeName1, assemblyName1' has one or more undisposed SQL locks at the end of the run."));
		AssertEquals(true, exception.Message.Contains($"Undisposed lock keys:{System.Environment.NewLine}{lockKey}"));
	}

	protected override void TearDown()
	{
		sqlLock.Dispose();
		base.TearDown();
	}

	IHostedServiceAttribute serviceTaskConfigMock;
	IServiceTaskHandler serviceTaskMock;

	SqlApplicationLock sqlLock;

	const string taskCode = "xxx";
	const string lockKeyPrefix = "Lock_Key_";
}
