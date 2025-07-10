using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace CargoWise.ServiceManager.Runner.Test
{
	class ServiceTaskRunnerStrategyTest : TestCase
	{
		public class MiscellaneousTest : ServiceTaskRunnerStrategyTest
		{
			public void TestWrongConstructorParamsCall()
			{
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskRunnerStrategy(null, serviceTaskLockerMock.Object, serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object, hostedServiceAttributeProvider.Object));
					AssertEquals("runnerLogger", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskRunnerStrategy(runnerLoggerMock.Object, null, serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object, hostedServiceAttributeProvider.Object));
					AssertEquals("serviceTaskLocker", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskRunnerStrategy(runnerLoggerMock.Object, serviceTaskLockerMock.Object, null, hostedServiceAttributeProvider.Object));
					AssertEquals("serviceTaskRunnerWithNextRunTimeCheckFactory", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskRunnerStrategy(runnerLoggerMock.Object, serviceTaskLockerMock.Object, serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object, null));
					AssertEquals("hostedServiceAttributeProvider", result.ParamName);
				});
			}

			public void TestRunWrongParamCall()
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => serviceTaskRunnerStrategy.Run(null));
				AssertEquals("runCommandInfo", result.ParamName);
			}

			public void TestCancel()
			{
				// Arrange
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(true)
					.Callback(() => serviceTaskRunnerStrategy.Cancel());

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute("AAA"))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(new DirectRunCommandInfo(string.Empty, "AAA", Guid.Empty));

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
					serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.Is<CancellationTokenSource>(token => token.IsCancellationRequested)), Times.Once());
				});
			}
		}

		public class MultipleInstanceTest : ServiceTaskRunnerStrategyTest
		{
			[ExpectNoExceptions]
			public void TestRunMultipleInstancesDoesNotCheckLock()
			{
				// Arrange
				const string code = "code";
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(true);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(serviceTaskMock.Object.HostedServiceAttribute);

				// Act
				serviceTaskRunnerStrategy.Run(new DirectRunCommandInfo(string.Empty, code, Guid.Empty));

				// Assert
				serviceTaskLockerMock.VerifyNoOtherCalls();
			}

			public void TestRunMultipleInstances()
			{
				// Arrange
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(true);
				var commandInfo = new DirectRunCommandInfo(string.Empty, "AAA", Guid.Empty);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(commandInfo.Code))
					.Returns(serviceTaskMock.Object.HostedServiceAttribute);

				// Act
				var result = serviceTaskRunnerStrategy.Run(commandInfo);

				// Assert
				serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(commandInfo, It.IsAny<CancellationTokenSource>()), Times.Once);
				serviceTaskRunnerWithNextRunTimeCheckMock.VerifyNoOtherCalls();
				AssertEquals(ServiceTaskRunResult.Success, result);
			}
		}

		public class SingleInstanceWithoutGroupTest : ServiceTaskRunnerStrategyTest
		{
			public void TestRunSingleInstanceWithoutGroupDoesNotRunIfLockAlreadyAcquired_Direct()
			{
				RunSingleInstanceWithoutGroupDoesNotRunIfLockAlreadyAcquired<IDirectRunCommandInfo>();
			}

			public void TestRunSingleInstanceWithoutGroupDoesNotRunIfLockAlreadyAcquired_Indirect()
			{
				RunSingleInstanceWithoutGroupDoesNotRunIfLockAlreadyAcquired<IScheduledRunCommandInfo>();
			}

			void RunSingleInstanceWithoutGroupDoesNotRunIfLockAlreadyAcquired<T>() where T : class, IRunCommandInfo
			{
				// Arrange
				const string code = "code";
				var commandInfoMock = new Mock<T>();
				commandInfoMock.SetupGet(info => info.Code).Returns("code");
				commandInfoMock.SetupGet(info => info.Id).Returns(Guid.Empty);
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				var expectedLock = new Mock<IDisposable>().Object;
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock(code, out expectedLock)).Returns(false);
				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				var result = serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Never);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(It.IsAny<string>(), out expectedLock), Times.Once);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(code, out expectedLock), Times.Once);
				});
				AssertEquals(ServiceTaskRunResult.ServiceTaskLockNotAcquired, result);
			}

			public void TestRunSingleInstance()
			{
				// Arrange
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				var expectedLock = new Mock<IDisposable>().Object;
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock("AAA", out expectedLock)).Returns(true);
				var commandInfo = new DirectRunCommandInfo(string.Empty, "AAA", Guid.Empty);
				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute("AAA"))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				var result = serviceTaskRunnerStrategy.Run(commandInfo);

				// Assert
				serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
				serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(commandInfo, It.IsAny<CancellationTokenSource>()), Times.Once);
				AssertEquals(ServiceTaskRunResult.Success, result);
			}

			public void TestRunSingleInstanceDisposesLocks()
			{
				// Arrange
				const string code = "AAA";
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				var disposableLockMock = new Mock<IDisposable>();
				var expectedLock = disposableLockMock.Object;
				serviceTaskLockerMock.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out expectedLock)).Returns(true);
				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(new DirectRunCommandInfo(string.Empty, code, Guid.Empty));

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(It.IsAny<string>(), out expectedLock), Times.Once);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(code, out expectedLock), Times.Once);
					disposableLockMock.Verify(disposable => disposable.Dispose(), Times.Once);
				});
			}

			public void TestRunSingleInstanceWithoutGroupLogsLockAcquireAndDispose()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						Test(taskCode);
					}
				});

				void Test(string taskCode)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);

					var taskLock = new Mock<IDisposable>().Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out taskLock))
						.Returns(true);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						commandInfoMock.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockAcquired), Times.Once);
						commandInfoMock.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ExecutingCommand), Times.Once);
						commandInfoMock.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockReleased), Times.Once);
						runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Exactly(3));
						runnerLoggerMock.VerifyNoOtherCalls();
					});
				}
			}

			public void TestRunSingleInstanceWithoutGroupLogsLockAcquireAndDisposeError()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						Test(taskCode, new Exception());
						Test(taskCode, new InvalidOperationException());
						Test(taskCode, new ArithmeticException());
					}
				});

				void Test(string taskCode, Exception exception)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);

					var taskLockMock = new Mock<IDisposable>();
					var taskLock = taskLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out taskLock))
						.Returns(true);
					taskLockMock
						.Setup(disposable => disposable.Dispose())
						.Throws(exception);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);
					commandInfoMock
						.SetupGet(info => info.AssemblyName)
						.Returns("someAssembly");

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					var result = AssertExceptionThrown<ServiceTaskLockReleaseException>(() => serviceTaskRunnerStrategy.Run(commandInfoMock.Object));

					// Assert
					AssertNoExceptionThrown(() =>
					{
						commandInfoMock.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockAcquired), Times.Once);
						commandInfoMock.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ExecutingCommand), Times.Once);
						runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Exactly(2));
						runnerLoggerMock.Verify(l => l.Log(LogLevel.Error, $"Service task [someAssembly|{taskCode}]: Failed to release service task lock", exception), Times.Once);
						runnerLoggerMock.VerifyNoOtherCalls();
					});
					AssertContains(taskCode, result.Message);
					AssertEquals(exception, result.InnerException);
				}
			}

			[ExpectNoExceptions]
			public void TestRunSingleInstanceLockReleaseException()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						Test(taskCode, CreateLockReleaseException(1222));
					}
				});

				void Test(string taskCode, Exception exception)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);

					var taskLockMock = new Mock<IDisposable>();
					var taskLock = taskLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out taskLock))
						.Returns(true);
					taskLockMock
						.Setup(disposable => disposable.Dispose())
						.Throws(exception);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);
					commandInfoMock
						.SetupGet(info => info.AssemblyName)
						.Returns("someAssembly");

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					var result = serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

					// Assert
					AssertEquals(ServiceTaskRunResult.LockNotReleased, result);
					runnerLoggerMock.Verify(l => l.Log(LogLevel.Error, $"Service task [someAssembly|{taskCode}]: Failed to release service task lock", It.IsAny<SqlMutexLockReleaseException>()));
				}

				static SqlMutexLockReleaseException CreateLockReleaseException(params object[] args)
				{
					var type = typeof(SqlMutexLockReleaseException);
					var instance = type.Assembly.CreateInstance(
						type.FullName, false,
						BindingFlags.Instance | BindingFlags.NonPublic,
						null, args, null, null);
					return (SqlMutexLockReleaseException)instance;
				}
			}

			[ExpectNoExceptions]
			public void TestRunGroupInstanceLockReleaseException()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						Test(taskCode, CreateLockReleaseException(1222));
					}
				});

				void Test(string taskCode, Exception exception)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);
					hostedServiceConfigMock
						.SetupGet(config => config.MutuallyExclusiveTaskGroup)
						.Returns(MutuallyExclusiveServiceTaskGroups.Upgrade);

					var taskLockMock = new Mock<IDisposable>();
					var taskLock = taskLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out taskLock))
						.Returns(true);
					taskLockMock
						.Setup(disposable => disposable.Dispose())
						.Throws(exception);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);
					commandInfoMock
						.SetupGet(info => info.AssemblyName)
						.Returns("someAssembly");

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					var result = serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

					// Assert
					AssertEquals(ServiceTaskRunResult.LockNotReleased, result);
					runnerLoggerMock.Verify(l => l.Log(LogLevel.Error, $"Service task [someAssembly|{taskCode}]: Failed to release group lock [Upgrade]", It.IsAny<SqlMutexLockReleaseException>()));
				}

				static SqlMutexLockReleaseException CreateLockReleaseException(params object[] args)
				{
					var type = typeof(SqlMutexLockReleaseException);
					var instance = type.Assembly.CreateInstance(
						type.FullName, false,
						BindingFlags.Instance | BindingFlags.NonPublic,
						null, args, null, null);
					return (SqlMutexLockReleaseException)instance;
				}
			}

			public void TestRunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired_Direct()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<IDirectRunCommandInfo>(taskCode, x =>
						{
							x.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockNotAcquired), Times.Once);
							runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Once);
						});
					}
				});
			}

			public void TestRunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired_Indirect()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<IScheduledRunCommandInfo>(taskCode, x =>
						{
							x.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockNotAcquired), Times.Once);
							runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Once);
						});
					}
				});
			}

			void RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<T>(string taskCode, Action<Mock<T>> assert) where T : class, IRunCommandInfo
			{
				// Arrange
				runnerLoggerMock.Invocations.Clear();

				serviceTaskMock
					.SetupGet(task => task.HostedServiceAttribute)
					.Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock
					.SetupGet(config => config.AllowsMultipleInstances)
					.Returns(false);

				var expectedLock = new Mock<IDisposable>().Object;
				serviceTaskLockerMock
					.Setup(x => x.TryAcquireLock(taskCode, out expectedLock))
					.Returns(false);

				var commandInfoMock = new Mock<T>();
				commandInfoMock
					.SetupGet(info => info.Code)
					.Returns(taskCode);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					assert(commandInfoMock);
					runnerLoggerMock.VerifyNoOtherCalls();
				});
			}

			[ExpectNoExceptions]
			public void TestSingleInstanceServiceTaskIsLockedFromRunningOnMultipleInstances()
			{
				const string serviceTaskCode = "XXX";
				var serviceTaskIsLocked = 0;
				var commandInfo = new DirectRunCommandInfo(string.Empty, code: serviceTaskCode, Guid.Empty);

				hostedServiceConfigMock.SetupGet(config => config.Code).Returns(serviceTaskCode);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(serviceTaskCode))
					.Returns(hostedServiceConfigMock.Object);

				var disposableLockMock = new Mock<IDisposable>();
				var expectedLock = disposableLockMock.Object;
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock(serviceTaskCode, out expectedLock))
					.Returns(() => Interlocked.CompareExchange(ref serviceTaskIsLocked, 1, 0) == 0);

				var runningThreadCounter = 0;

				void StrategyRunner()
				{
					Interlocked.Increment(ref runningThreadCounter);

					using (Db.DisposableActionForDbConnection())
					{
						var runnerStrategy = new ServiceTaskRunnerStrategy(runnerLoggerMock.Object, serviceTaskLockerMock.Object, serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object, hostedServiceAttributeProvider.Object);
						runnerStrategy.Run(commandInfo);
					}
				}

				var tasks = Enumerable.Range(0, 10).Select(i => new Task(StrategyRunner)).ToArray();
				Parallel.ForEach(tasks, t => t.Start());
				tasks.ForEach(t => t.Wait());

				serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once, "Only one service task has been run");
				runnerLoggerMock.Verify(logger => logger.Log(
						LogLevel.Debug,
						It.Is<string>(x => x.Contains("Reenqueueing Nudge run command."))),
					Times.Exactly(tasks.Length - 1),
					"The rest of runners had failed to acquire a lock to run");
			}

			static IEnumerable<string> TaskCodes { get; } = new[] { "AAA", "BBB", };
		}

		public class SingleInstanceWithGroupTest : ServiceTaskRunnerStrategyTest
		{
			static IEnumerable<MutuallyExclusiveServiceTaskGroups> MutuallyExclusiveServiceTaskGroupsArray => new[]
			{
				MutuallyExclusiveServiceTaskGroups.Upgrade,
				MutuallyExclusiveServiceTaskGroups.BiEdw,
				MutuallyExclusiveServiceTaskGroups.BiAudit,
			};

			static IEnumerable<MutuallyExclusiveServiceTaskGroups> NonExclusiveMutuallyExclusiveServiceTaskGroupsArray => new[]
			{
				MutuallyExclusiveServiceTaskGroups.NoGroup,
				MutuallyExclusiveServiceTaskGroups.NoGroup,
			};

			public void TestRunSingleInstanceWithGroupDoesNotRunIfLockAlreadyAcquired_Direct()
			{
				foreach (var group in MutuallyExclusiveServiceTaskGroupsArray)
				{
					RunSingleInstanceWithGroupDoesNotRunIfLockAlreadyAcquired(group, true);
				}
			}

			public void TestRunSingleInstanceWithGroupDoesNotRunIfLockAlreadyAcquired_Indirect()
			{
				foreach (var group in MutuallyExclusiveServiceTaskGroupsArray)
				{
					RunSingleInstanceWithGroupDoesNotRunIfLockAlreadyAcquired(group, false);
				}
			}

			void RunSingleInstanceWithGroupDoesNotRunIfLockAlreadyAcquired(MutuallyExclusiveServiceTaskGroups group, bool directScheduleRequest)
			{
				// Arrange
				serviceHostMessageDispatcherMock.Invocations.Clear();
				serviceTaskMock.Invocations.Clear();
				serviceTaskLockerMock.Invocations.Clear();
				runnerLoggerMock.Invocations.Clear();
				const string code = "code";
				var commandInfo = directScheduleRequest
					? (IRunCommandInfo)new DirectRunCommandInfo(string.Empty, code, Guid.Empty)
					: new ScheduledRunCommandInfo(string.Empty, code, Guid.Empty, DateTime.Now, DateTime.Now);
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				hostedServiceConfigMock.SetupGet(config => config.MutuallyExclusiveTaskGroup).Returns(group);

				var expectedLock = new Mock<IDisposable>().Object;
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock(code, out expectedLock)).Returns(true);
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock(group.ToString(), out expectedLock)).Returns(false);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				var result = serviceTaskRunnerStrategy.Run(commandInfo);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Never);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(It.IsAny<string>(), out expectedLock), Times.Exactly(2));
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(code, out expectedLock), Times.Once);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(group.ToString(), out expectedLock), Times.Once);
				});

				AssertEquals(ServiceTaskRunResult.GroupLockNotAcquired, result);
			}

			public void TestRunSingleInstanceWithGroupNoneGroupDoesNotAcquireGroupLock()
			{
				foreach (var group in NonExclusiveMutuallyExclusiveServiceTaskGroupsArray)
				{
					RunSingleInstanceWithGroupNoneGroupDoesNotAcquireGroupLock(group);
				}
			}

			void RunSingleInstanceWithGroupNoneGroupDoesNotAcquireGroupLock(MutuallyExclusiveServiceTaskGroups group)
			{
				// Arrange
				serviceTaskRunnerWithNextRunTimeCheckMock.Invocations.Clear();
				serviceTaskLockerMock.Invocations.Clear();
				const string code = "code";
				var commandInfo = new DirectRunCommandInfo(string.Empty, code, Guid.Empty);
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				hostedServiceConfigMock.SetupGet(config => config.MutuallyExclusiveTaskGroup).Returns(group);

				var expectedLock = new Mock<IDisposable>().Object;
				serviceTaskLockerMock.Setup(x => x.TryAcquireLock(code, out expectedLock)).Returns(true);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(commandInfo);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskRunnerWithNextRunTimeCheckMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(code, out expectedLock), Times.Once);
					serviceTaskLockerMock.VerifyNoOtherCalls();
				});
			}

			public void TestRunSingleInstanceWithGroupDisposesLocks()
			{
				// Arrange
				const string code = "AAA";
				serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
				hostedServiceConfigMock.SetupGet(config => config.MutuallyExclusiveTaskGroup).Returns(MutuallyExclusiveServiceTaskGroups.Upgrade);

				var disposableLockMocks = new[] { new Mock<IDisposable>(), new Mock<IDisposable>() };
				var expectedLocks = disposableLockMocks.Select(mock => mock.Object).ToArray();
				serviceTaskLockerMock.Setup(locker => locker.TryAcquireLock(code, out expectedLocks[0])).Returns(true);
				serviceTaskLockerMock.Setup(locker => locker.TryAcquireLock(nameof(MutuallyExclusiveServiceTaskGroups.Upgrade), out expectedLocks[1])).Returns(true);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(code))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(new DirectRunCommandInfo(string.Empty, code, Guid.Empty));

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(It.IsAny<string>(), out expectedLocks[0]), Times.Exactly(2));
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(code, out expectedLocks[0]), Times.Once);
					serviceTaskLockerMock.Verify(locker => locker.TryAcquireLock(nameof(MutuallyExclusiveServiceTaskGroups.Upgrade), out expectedLocks[1]), Times.Once);
					disposableLockMocks[0].Verify(disposable => disposable.Dispose(), Times.Once);
					disposableLockMocks[1].Verify(disposable => disposable.Dispose(), Times.Once);
				});
			}

			// Need a snapshot to remove mutexes from dbo.StmServiceMutex table
			[UseSnapshotProtection]
			[ExpectNoExceptions]
			public void TestSingleInstanceServiceTaskWithGroupDoesNotReportDoubleUpgradeException()
			{
				// Note, this test, as of Oct 2022, is testing for the presence of
				//		Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false
				// in ServiceTaskRunnerStrategy.
				// It should fail if that line is removed.

				// Arrange
				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

				// Need to replace WinFormsAppDbConnectionGuiPlugin with a mock,
				// since it will restart the application if DatabaseUpgradeException is thrown twice.
				var dbEnv = new DbEnvironmentWithMockGuiPluginForTest();
				var existingEnv = DbEnv.Instance;
				var disposableMock = new Mock<IDisposable>();
				disposableMock
					.Setup(d => d.Dispose())
					.Callback(() => throw new DatabaseUpgradedException());
				var disposableObject = disposableMock.Object;
				serviceTaskLockerMock
					.Setup(locker => locker.TryAcquireLock(It.IsAny<string>(), out disposableObject))
					.Returns(true);
				DbEnv.SetDbEnvironment(dbEnv);
				try
				{
					var services = CompositionRoot
						.AddRegistrations(new ServiceCollection(), ApplicationLoggingTestHelper.MockLoggerFactory())
						.AddSingleton(runnerLoggerMock.Object)
						.AddSingleton(serviceHostMessageDispatcherMock.Object)
						.AddSingleton(serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object)
						.AddSingleton(errorReporterProxyMock.Object)
						.AddSingleton(serviceTaskLockerMock.Object)
						.AddSingleton(hostedServiceAttributeProvider.Object);
					using (var provider = services.BuildServiceProvider())
					{
						IServiceTaskRunnerStrategy runnerStrategy = provider.GetRequiredService<IServiceTaskRunnerStrategy>();

						var commandInfo = new DirectRunCommandInfo(string.Empty, "code", Guid.Empty);
						hostedServiceConfigMock.SetupGet(config => config.Code).Returns("code");
						hostedServiceConfigMock.SetupGet(config => config.AllowsMultipleInstances).Returns(false);
						hostedServiceConfigMock.SetupGet(config => config.MutuallyExclusiveTaskGroup).Returns(MutuallyExclusiveServiceTaskGroups.Upgrade);
						serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);

						hostedServiceAttributeProvider
							.Setup(h => h.GetClientHostedServiceAttribute(commandInfo.Code))
							.Returns(hostedServiceConfigMock.Object);

						serviceTaskRunnerWithNextRunTimeCheckMock
							.Setup(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()))
							.Returns<IRunCommandInfo, CancellationTokenSource>((info, arg3) =>
							{
								// An upgrade would be in another process and would kill our connection.
								// For testing it is enough to close the connection and mock a new db version.
								Db.Connection.CloseConnection();
								ObjectFactory.Substitute(versionMock.Object);
								return ServiceTaskRunResult.Success;
							});

						Exception runnerThreadException = null;
						var runnerThread = new Thread(() =>
						{
							try
							{
								using (Db.DisposableActionForDbConnection())
								{
									// Act
									runnerStrategy.Run(commandInfo);
								}
							}
							catch (Exception ex)
							{
								runnerThreadException = ex;
							}
						});
						runnerThread.Start();

						// Assert
						runnerThread.Join();
						errorReporterProxyMock.VerifyNoOtherCalls();
						AssertType<DatabaseUpgradedException>("DatabaseUpgradedException should bubble up", runnerThreadException.GetInnermostException());
					}
				}
				finally
				{
					DbEnv.SetDbEnvironment(existingEnv);
				}
			}

			public void TestRunSingleInstanceWithGroupLogsLockAcquireAndDispose()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						foreach (var groupCode in GroupCodes)
						{
							Test(taskCode, groupCode);
						}
					}
				});

				void Test(string taskCode, MutuallyExclusiveServiceTaskGroups groupCode)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);
					hostedServiceConfigMock
						.SetupGet(config => config.MutuallyExclusiveTaskGroup)
						.Returns(groupCode);

					var taskLockMock = new Mock<IDisposable>();
					var taskLock = taskLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(taskCode, out taskLock))
						.Returns(true);
					var groupLockMock = new Mock<IDisposable>();
					var groupLock = groupLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(groupCode.ToString(), out groupLock))
						.Returns(true);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockAcquired), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockReleased), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockAcquired, groupCode.ToString()), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ExecutingCommand), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockReleased, groupCode.ToString()), Times.Once);
						runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Exactly(5));
						runnerLoggerMock.VerifyNoOtherCalls();
					});
				}
			}

			public void TestRunSingleInstanceWithGroupLogsLockAcquireAndDisposeError()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						foreach (var groupCode in GroupCodes)
						{
							Test(taskCode, groupCode, new Exception());
							Test(taskCode, groupCode, new InvalidOperationException());
							Test(taskCode, groupCode, new ArithmeticException());
						}
					}
				});

				void Test(string taskCode, MutuallyExclusiveServiceTaskGroups groupCode, Exception exception)
				{
					// Arrange
					runnerLoggerMock.Invocations.Clear();

					serviceTaskMock
						.SetupGet(task => task.HostedServiceAttribute)
						.Returns(hostedServiceConfigMock.Object);
					hostedServiceConfigMock
						.SetupGet(config => config.AllowsMultipleInstances)
						.Returns(false);
					hostedServiceConfigMock
						.SetupGet(config => config.MutuallyExclusiveTaskGroup)
						.Returns(groupCode);

					var taskLockMock = new Mock<IDisposable>();
					var taskLock = taskLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(taskCode, out taskLock))
						.Returns(true);
					var groupLockMock = new Mock<IDisposable>();
					var groupLock = groupLockMock.Object;
					serviceTaskLockerMock
						.Setup(locker => locker.TryAcquireLock(groupCode.ToString(), out groupLock))
						.Returns(true);
					groupLockMock
						.Setup(disposable => disposable.Dispose())
						.Throws(exception);

					var commandInfoMock = new Mock<IRunCommandInfo>();
					commandInfoMock
						.SetupGet(info => info.Code)
						.Returns(taskCode);
					commandInfoMock
						.SetupGet(info => info.AssemblyName)
						.Returns("someAssembly");

					hostedServiceAttributeProvider
						.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
						.Returns(hostedServiceConfigMock.Object);

					// Act
					var result = AssertExceptionThrown<MutualExclusiveLockReleaseException>(() => serviceTaskRunnerStrategy.Run(commandInfoMock.Object));

					// Assert
					AssertNoExceptionThrown(() =>
					{
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockAcquired), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockReleased), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockAcquired, groupCode.ToString()), Times.Once);
						commandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.ExecutingCommand), Times.Once);
						runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Exactly(4));
						runnerLoggerMock.Verify(l => l.Log(LogLevel.Error, $"Service task [someAssembly|{taskCode}]: Failed to release group lock [{groupCode}]", exception), Times.Once);
						runnerLoggerMock.VerifyNoOtherCalls();
					});
					AssertContains(taskCode, result.Message);
					AssertContains(groupCode.ToString(), result.Message);
					AssertEquals(exception, result.InnerException);
				}
			}

			public void TestRunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired_Direct()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						foreach (var groupCode in GroupCodes)
						{
							RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<IDirectRunCommandInfo>(taskCode, groupCode, x =>
							{
								x.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockNotAcquired, groupCode.ToString()), Times.Once);
							});
						}
					}
				});
			}

			public void TestRunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired_Indirect()
			{
				CombineAssertions(() =>
				{
					foreach (var taskCode in TaskCodes)
					{
						foreach (var groupCode in GroupCodes)
						{
							RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<IScheduledRunCommandInfo>(taskCode, groupCode, x =>
							{
								x.Verify(y => y.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockNotAcquired, groupCode.ToString()), Times.Once);
							});
						}
					}
				});
			}

			void RunSingleInstanceWithoutGroupLogsWhenLockAlreadyAcquired<T>(string taskCode, MutuallyExclusiveServiceTaskGroups groupCode, Action<Mock<T>> assert) where T : class, IRunCommandInfo
			{
				// Arrange
				runnerLoggerMock.Invocations.Clear();

				serviceTaskMock
					.SetupGet(task => task.HostedServiceAttribute)
					.Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigMock
					.SetupGet(config => config.AllowsMultipleInstances)
					.Returns(false);

				hostedServiceConfigMock
					.SetupGet(config => config.MutuallyExclusiveTaskGroup)
					.Returns(groupCode);

				var taskLockMock = new Mock<IDisposable>();
				var taskLock = taskLockMock.Object;
				serviceTaskLockerMock
					.Setup(locker => locker.TryAcquireLock(taskCode, out taskLock))
					.Returns(true);
				var groupLockMock = new Mock<IDisposable>();
				var groupLock = groupLockMock.Object;
				serviceTaskLockerMock
					.Setup(locker => locker.TryAcquireLock(groupCode.ToString(), out groupLock))
					.Returns(false);

				var commandInfoMock = new Mock<T>();
				commandInfoMock
					.SetupGet(info => info.Code)
					.Returns(taskCode);

				hostedServiceAttributeProvider
					.Setup(x => x.GetClientHostedServiceAttribute(taskCode))
					.Returns(hostedServiceConfigMock.Object);

				// Act
				serviceTaskRunnerStrategy.Run(commandInfoMock.Object);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsAny<string>()), Times.Exactly(3));
					assert(commandInfoMock);
					runnerLoggerMock.VerifyNoOtherCalls();
				});
			}

			static IEnumerable<MutuallyExclusiveServiceTaskGroups> GroupCodes { get; } = new[] { MutuallyExclusiveServiceTaskGroups.Upgrade, MutuallyExclusiveServiceTaskGroups.BiAudit };
			static IEnumerable<string> TaskCodes { get; } = new[] { "AAA", "BBB", };
		}

		protected override void SetUp()
		{
			base.SetUp();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			serviceHostMessageDispatcherMock = new Mock<IServiceHostMessageDispatcher>();
			serviceTaskLockerMock = new Mock<ISqlMutexLocker>();
			serviceTaskRunnerWithNextRunTimeCheckMock = new Mock<IServiceTaskRunnerWithNextRunTimeCheck>();
			serviceTaskRunnerWithNextRunTimeCheckFactoryMock = new Mock<IServiceTaskRunnerWithNextRunTimeCheckFactory>();
			serviceTaskRunnerWithNextRunTimeCheckFactoryMock
				.Setup(x => x.CreateRunner())
				.Returns(serviceTaskRunnerWithNextRunTimeCheckMock.Object);
			hostedServiceAttributeProvider = new Mock<IClientHostedServiceAttributeProvider>();
			serviceTaskRunnerStrategy = new ServiceTaskRunnerStrategy(runnerLoggerMock.Object, serviceTaskLockerMock.Object, serviceTaskRunnerWithNextRunTimeCheckFactoryMock.Object, hostedServiceAttributeProvider.Object);

			serviceTaskMock = new Mock<IServiceTaskHandler>();
			hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
		}

		Mock<IHostedServiceAttribute> hostedServiceConfigMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<IServiceHostMessageDispatcher> serviceHostMessageDispatcherMock;
		Mock<ISqlMutexLocker> serviceTaskLockerMock;
		Mock<IServiceTaskHandler> serviceTaskMock;
		Mock<IServiceTaskRunnerWithNextRunTimeCheck> serviceTaskRunnerWithNextRunTimeCheckMock;
		Mock<IServiceTaskRunnerWithNextRunTimeCheckFactory> serviceTaskRunnerWithNextRunTimeCheckFactoryMock;
		Mock<IClientHostedServiceAttributeProvider> hostedServiceAttributeProvider;
		ServiceTaskRunnerStrategy serviceTaskRunnerStrategy;
	}

	class DbEnvironmentWithMockGuiPluginForTest : BaseDbEnvironment
	{
		readonly IDbConnectionGuiPlugin connectionGuiPlugin = new Mock<IDbConnectionGuiPlugin>().Object;

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
	}
}
