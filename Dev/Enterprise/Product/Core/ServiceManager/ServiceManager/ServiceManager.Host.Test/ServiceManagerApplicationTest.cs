using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ServiceManagerApplicationTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			serviceManagerApplicationInitializerMock = new Mock<IServiceManagerApplicationInitializer>();
			serviceManagerTasksMock = new Mock<IEnumerable<IServiceManagerTask>>();
			delayProviderMock = new Mock<IDelayProvider>();
			applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			serviceManagerApplication = new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object);
		}

		public void TestWrongConstructorParams()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerApplication(null, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceManagerApplicationInitializer"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, null, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("delayProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, null, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceManagerTasks"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, null, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("applicationEmergencyExit"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));
			});
		}

		public void TestRunApplicationInitializerWithoutExceptionOnDbAccess()
		{
			// Arrange
			serviceManagerApplicationInitializerMock
				.Setup(initializer => initializer.Initialize())
				.Callback(ExecuteTestSql);
			serviceManagerTasksMock
				.Setup(tasks => tasks.GetEnumerator())
				.Returns(() => Enumerable.Empty<IServiceManagerTask>().GetEnumerator());

			// Act
			serviceManagerApplication.Run(CancellationToken.None);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerApplicationInitializerMock.Verify(initializer => initializer.Initialize(), Times.Once());
				serviceManagerTasksMock.Verify(tasks => tasks.GetEnumerator(), Times.Once);
			});
		}

		public void TestFailedInitializeDoesNotRunDispatcher()
		{
			// Arrange
			serviceManagerApplicationInitializerMock.Setup(mock => mock.Initialize()).Throws<IndexOutOfRangeException>();

			// Act
			// Assert
			AssertNoExceptionThrown(() =>
			{
				AssertExceptionThrown<IndexOutOfRangeException>(() => serviceManagerApplication.Run(CancellationToken.None));
				serviceManagerApplicationInitializerMock.Verify(mock => mock.Initialize(), Times.Once());
				serviceManagerTasksMock.Verify(tasks => tasks.GetEnumerator(), Times.Never);
			});
		}

		public void TestServiceManagerTaskIsInitializedBeforeRun()
		{
			// Arrange
			var callList = new List<string>();
			using var cancellationTokenSource = new CancellationTokenSource();
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Initialise(It.IsAny<CancellationToken>()))
				.Callback(() => callList.Add(nameof(IServiceManagerTask.Initialise)));
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					callList.Add(nameof(IServiceManagerTask.Run));
					cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
					Task.Delay(TimeSpan.FromSeconds(15), cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
				});

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.Once());
				serviceManagerTaskMock.Verify(task => task.Initialise(cancellationTokenSource.Token), Times.Once());
				serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Once);
				serviceManagerTaskMock.Verify(task => task.Run(cancellationTokenSource.Token), Times.Once);
				NUnit.Framework.Assert.That(callList.ToArray(), Is.EqualTo(new[] { "Initialise", "Run" }));
			});
		}

		public void TestServiceManagerTasksAreInitializedBeforeRun()
		{
			// Arrange
			var callQueue = new ConcurrentQueue<string>();
			using var cancellationTokenSource = new CancellationTokenSource();
			const int count = 10;
			var runDictionary = new ConcurrentDictionary<int, bool>(
				Enumerable.Range(0, count)
					.ToDictionary(i => i, i => false));

			var serviceManagerTaskMocks = Enumerable.Range(0, count)
				.Select(i =>
				{
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					serviceManagerTaskMock.SetupGet(task => task.Name).Returns(i.ToString("D5"));
					serviceManagerTaskMock
						.Setup(task => task.Initialise(It.IsAny<CancellationToken>()))
						.Callback(() => callQueue.Enqueue($"{nameof(IServiceManagerTask.Initialise)}:{i:D5}"));
					serviceManagerTaskMock
						.Setup(task => task.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							callQueue.Enqueue($"{nameof(IServiceManagerTask.Run)}:{i:D5}");
							runDictionary.TryUpdate(i, true, false);
							if (runDictionary.Values.All(b => b))
							{
								cancellationTokenSource.Cancel();
							}

							Task.Delay(TimeSpan.FromSeconds(15), cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
						});
					return serviceManagerTaskMock;
				})
				.ToArray();

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => serviceManagerTaskMocks.Select(mock => mock.Object).GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				var callList = callQueue.ToList();
				foreach (var serviceManagerTaskMock in serviceManagerTaskMocks)
				{
					serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.Once);
					serviceManagerTaskMock.Verify(task => task.Initialise(cancellationTokenSource.Token), Times.Once);
					serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					serviceManagerTaskMock.Verify(task => task.Run(cancellationTokenSource.Token), Times.AtLeastOnce);
					NUnit.Framework.Assert.That(callList.IndexOf($"Initialise:{serviceManagerTaskMock.Object.Name}"), Is.LessThan(callList.IndexOf($"Run:{serviceManagerTaskMock.Object.Name}")));
				}

				NUnit.Framework.Assert.That(runDictionary.Values.All(b => b), Is.EqualTo(true));
				NUnit.Framework.Assert.That(callQueue, Is.EquivalentTo(Enumerable.Range(0, count).SelectMany(i => new[] { $"Initialise:{i:D5}", $"Run:{i:D5}" })));
			});
		}

		[ExpectNoExceptions]
		public void TestServiceManagerTasksAreRunInParallel()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			const int count = 10;
			var runDictionary = new ConcurrentDictionary<int, bool>(
				Enumerable.Range(0, count)
					.ToDictionary(i => i, i => false));

			var serviceManagerTaskMocks = Enumerable.Range(0, count)
				.Select(i =>
				{
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					serviceManagerTaskMock.SetupGet(task => task.Name).Returns(i.ToString("D5"));
					serviceManagerTaskMock
						.Setup(task => task.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							runDictionary.TryUpdate(i, true, false);
							if (runDictionary.Values.All(b => b))
							{
								// cancel only after all tasks are started
								cancellationTokenSource.Cancel();
							}

							// I will never finish without cancellation token being cancelled :)
							Task.Delay(TimeSpan.FromHours(1), cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
						});
					return serviceManagerTaskMock;
				})
				.ToArray();

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => serviceManagerTaskMocks.Select(mock => mock.Object).GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(runDictionary.Values.All(b => b), Is.EqualTo(true));
				foreach (var serviceManagerTaskMock in serviceManagerTaskMocks)
				{
					serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestServiceManagerTasksHaveAssignedThreadNames()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			const int count = 10;
			var taskNameAssertions = new ConcurrentBag<Action>();
			var serviceManagerTaskMocks = Enumerable
				.Range(0, count)
				.Select(i =>
				{
					var taskName = $"Task{i:D3}";
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					serviceManagerTaskMock
						.SetupGet(task => task.Name)
						.Returns(taskName);
					serviceManagerTaskMock
						.Setup(task => task.Run(It.IsAny<CancellationToken>()))
						.Callback<CancellationToken>(token =>
						{
							var currentThreadName = Thread.CurrentThread.Name;
							taskNameAssertions.Add(() => NUnit.Framework.Assert.That(currentThreadName, Is.EqualTo(taskName)));

							if (taskNameAssertions.Count == count)
							{
								cancellationTokenSource.Cancel();
							}

							Task.Delay(TimeSpan.FromHours(1), token).Wait(token);
						});
					return serviceManagerTaskMock;
				})
				.ToArray();
			serviceManagerTasksMock
				.Setup(tasks => tasks.GetEnumerator())
				.Returns(serviceManagerTaskMocks.Select(mock => mock.Object).GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(taskNameAssertions.Count, Is.EqualTo(count));
				foreach (var taskNameAssertion in taskNameAssertions)
				{
					taskNameAssertion();
				}
			});
		}

		public void TestServiceManagerTaskIsInitializedOnceAndRunRepeatedly()
		{
			// Arrange
			var callCount = 0;
			const int repeatAmount = 3;
			using var cancellationTokenSource = new CancellationTokenSource();
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (Interlocked.Increment(ref callCount) >= repeatAmount)
					{
						cancellationTokenSource.Cancel();
					}
				});

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.Once());
				serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.AtLeast(repeatAmount));
			});
		}

		[ExpectNoExceptions]
		public void TestServiceManagerTaskReinitialisationRequestIsHandled()
		{
			// Arrange
			var callCount = 0;
			const int repeatBeforeReInit = 1;
			const int stopIteration = 3;
			using var cancellationTokenSource = new CancellationTokenSource();
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			var exception = new InitialisationRequestException("test");
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (Interlocked.Increment(ref callCount) == repeatBeforeReInit)
					{
						throw exception;
					}

					if (callCount >= stopIteration)
					{
						cancellationTokenSource.Cancel();
					}
				});

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			serviceManagerTaskMock.Verify(
				task => task.Initialise(It.IsAny<CancellationToken>()),
				Times.Exactly(2));
			serviceManagerTaskMock.Verify(
				task => task.Run(It.IsAny<CancellationToken>()),
				Times.AtLeast(stopIteration));
			errorReporterProxyMock.Verify(
				reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()),
				Times.Once);
			errorReporterProxyMock.Verify(
				reporter => reporter.ReportOnce(It.IsAny<string>(), exception),
				Times.Once);
		}

		public void TestServiceManagerTaskIsDelayedAfterEachRun()
		{
			// Arrange
			var callCount = 0;
			const int repeatAmount = 3;
			var timeSpan = new TimeSpan();
			using var cancellationTokenSource = new CancellationTokenSource();
			using var runWasCalled = new AutoResetEvent(false);
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (runWasCalled.WaitOne(TimeSpan.Zero))
					{
						cancellationTokenSource.Cancel();
						return;
					}

					runWasCalled.Set();
				});
			serviceManagerTaskMock.SetupGet(task => task.RunDelay).Returns(timeSpan);

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			delayProviderMock
				.Setup(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (Interlocked.Increment(ref callCount) >= repeatAmount)
					{
						cancellationTokenSource.Cancel();
					}

					if (!runWasCalled.WaitOne(TimeSpan.Zero))
					{
						cancellationTokenSource.Cancel();
					}
				});

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(timeSpan, cancellationTokenSource.Token), Times.Exactly(repeatAmount));
				NUnit.Framework.Assert.That(runWasCalled.WaitOne(TimeSpan.Zero), Is.EqualTo(false));
			});
		}

		public void TestServiceManagerTaskExceptionDuringInitialization()
		{
			// Arrange
			var callCount = 0;
			const int repeatAmount = 3;
			var timeSpan = new TimeSpan();
			using var cancellationTokenSource = new CancellationTokenSource();
			var exception = new Exception(":(");
			errorReporterProxyMock
				.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback(ExecuteTestSql);
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Initialise(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (Interlocked.Increment(ref callCount) >= repeatAmount)
					{
						cancellationTokenSource.Cancel();
					}
				})
				.Throws(exception);
			serviceManagerTaskMock.SetupGet(task => task.ErrorDelay).Returns(timeSpan);

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(timeSpan, cancellationTokenSource.Token), Times.Exactly(repeatAmount));
				serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Never);
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), exception));
			});
		}

		public void TestServiceManagerTaskExceptionDuringInitializationDelayExceptionIsSafelyHandled()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Initialise(It.IsAny<CancellationToken>()))
				.Throws<Exception>();
			delayProviderMock
				.Setup(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Callback(() => cancellationTokenSource.Cancel())
				.Throws<Exception>();

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerApplication.Run(cancellationTokenSource.Token);
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
			});
		}

		public void TestServiceManagerTaskExceptionDuringRun()
		{
			// Arrange
			var callCount = 0;
			const int repeatAmount = 3;
			var timeSpan = new TimeSpan();
			using var cancellationTokenSource = new CancellationTokenSource();
			var exception = new Exception(":(");
			errorReporterProxyMock
				.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback(ExecuteTestSql);
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (Interlocked.Increment(ref callCount) >= repeatAmount)
					{
						cancellationTokenSource.Cancel();
					}
				})
				.Throws(exception);
			serviceManagerTaskMock.SetupGet(task => task.RunDelay).Returns(timeSpan);

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			serviceManagerApplication.Run(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(repeatAmount));
				delayProviderMock.Verify(provider => provider.Delay(timeSpan, cancellationTokenSource.Token), Times.Exactly(repeatAmount));
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), exception));
			});
		}

		public void TestServiceManagerTaskExceptionDuringRunDelayExceptionIsSafelyHandled()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
			serviceManagerTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Throws<Exception>();
			delayProviderMock
				.Setup(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Callback(() => cancellationTokenSource.Cancel())
				.Throws<Exception>();

			serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

			// Act
			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceManagerApplication.Run(cancellationTokenSource.Token);
				delayProviderMock.Verify(provider => provider.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
			});
		}

		Mock<IApplicationEmergencyExit> applicationEmergencyExitMock;
		readonly Mock<IDbConnectionSetup> dbConnectionSetupMock;
		Mock<IDelayProvider> delayProviderMock;
		ServiceManagerApplication serviceManagerApplication;
		Mock<IServiceManagerApplicationInitializer> serviceManagerApplicationInitializerMock;
		Mock<IEnumerable<IServiceManagerTask>> serviceManagerTasksMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;

		public class CriticalExceptionTest : TestCase
		{
			static IEnumerable<Exception> CriticalExceptionSource => new Exception[] { new OutOfMemoryException(), new AppDomainUnloadedException() };

			protected override void SetUp()
			{
				base.SetUp();
				serviceManagerApplicationInitializerMock = new Mock<IServiceManagerApplicationInitializer>();
				serviceManagerTasksMock = new Mock<IEnumerable<IServiceManagerTask>>();
				delayProviderMock = new Mock<IDelayProvider>();
				applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();

				serviceManagerApplication = new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, new Mock<IErrorReporterProxy>().Object);
			}

			[ExpectNoExceptions]
			public void TestSourceAreCriticalExceptions()
			{
				// Arrange
				// Act
				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					CriticalExceptionSource
						.ForEach(x => NUnit.Framework.Assert.That(x.IsCriticalException(), Is.True, $"{x.GetType()} is not critical exception"));
				});
			}

			[ExpectNoExceptions]
			public void TestRunAsyncExitsApplicationOnCriticalExceptions()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					CriticalExceptionSource
						.ForEach(x => TestRunAsyncExitsApplicationOnCriticalExceptions(x));
				});

				void TestRunAsyncExitsApplicationOnCriticalExceptions(Exception criticalException)
				{
					// Arrange
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					var cancellationTokenSource = new CancellationTokenSource();
					serviceManagerTaskMock
						.Setup(x => x.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							cancellationTokenSource.Cancel();
							throw criticalException;
						});

					var taskList = new List<IServiceManagerTask> { serviceManagerTaskMock.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					// Act
					serviceManagerApplication.Run(cancellationTokenSource.Token);

					// Assert
					applicationEmergencyExitMock.Verify(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), criticalException), Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
					serviceManagerTasksMock.Verify(x => x.GetEnumerator(), Times.AtLeastOnce);
					serviceManagerTaskMock.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestRunAsyncCriticalExceptionExitsApplicationWithoutDelay()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					CriticalExceptionSource
						.ForEach(x => TestRunAsyncCriticalExceptionExitsApplicationWithoutDelay(x));
				});

				void TestRunAsyncCriticalExceptionExitsApplicationWithoutDelay(Exception criticalException)
				{
					// Arrange
					var cancellationTokenSource = new CancellationTokenSource();
					var serviceManagerTask = new Mock<IServiceManagerTask>();
					serviceManagerTask
						.Setup(x => x.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							cancellationTokenSource.Cancel();
							throw criticalException;
						});

					var taskList = new List<IServiceManagerTask> { serviceManagerTask.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					// Act
					serviceManagerApplication.Run(cancellationTokenSource.Token);

					// Assert
					delayProviderMock.Verify(
						x => x.Delay(It.IsAny<TimeSpan>(),
							It.IsAny<CancellationToken>()),
						Times.Never);
					applicationEmergencyExitMock.Verify(
						proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), criticalException),
						Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestInitialiseAsyncExitsApplicationOnCriticalException()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					CriticalExceptionSource
						.ForEach(x => TestInitialiseAsyncExitsApplicationOnCriticalException(x));
				});

				void TestInitialiseAsyncExitsApplicationOnCriticalException(Exception criticalException)
				{
					// Arrange
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					var cancellationTokenSource = new CancellationTokenSource();
					serviceManagerTaskMock
						.Setup(x => x.Initialise(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							cancellationTokenSource.Cancel();
							throw criticalException;
						});

					var taskList = new List<IServiceManagerTask> { serviceManagerTaskMock.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					// Act
					serviceManagerApplication.Run(cancellationTokenSource.Token);

					// Assert
					applicationEmergencyExitMock.Verify(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), criticalException), Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
					serviceManagerTasksMock.Verify(x => x.GetEnumerator(), Times.AtLeastOnce);
					serviceManagerTaskMock.Verify(x => x.Initialise(It.IsAny<CancellationToken>()), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestInitialiseAsyncCriticalExceptionExitsApplicationWithoutDelay()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					CriticalExceptionSource
						.ForEach(x => TestInitialiseAsyncCriticalExceptionExitsApplicationWithoutDelay(x));
				});

				void TestInitialiseAsyncCriticalExceptionExitsApplicationWithoutDelay(Exception criticalException)
				{
					// Arrange
					var cancellationTokenSource = new CancellationTokenSource();
					var serviceManagerTask = new Mock<IServiceManagerTask>();
					serviceManagerTask
						.Setup(x => x.Initialise(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							cancellationTokenSource.Cancel();
							throw criticalException;
						});

					var taskList = new List<IServiceManagerTask> { serviceManagerTask.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					// Act
					serviceManagerApplication.Run(cancellationTokenSource.Token);

					// Assert
					delayProviderMock
						.Verify(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
					applicationEmergencyExitMock.Verify(
						proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), criticalException),
						Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
				}
			}

			Mock<IApplicationEmergencyExit> applicationEmergencyExitMock;
			Mock<IDelayProvider> delayProviderMock;
			ServiceManagerApplication serviceManagerApplication;
			Mock<IServiceManagerApplicationInitializer> serviceManagerApplicationInitializerMock;
			Mock<IEnumerable<IServiceManagerTask>> serviceManagerTasksMock;
		}

		public class UnhandledExceptionTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				serviceManagerApplicationInitializerMock = new Mock<IServiceManagerApplicationInitializer>();
				serviceManagerTasksMock = new Mock<IEnumerable<IServiceManagerTask>>();
				delayProviderMock = new Mock<IDelayProvider>();
				applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				serviceManagerApplication = new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object);
			}

			[ExpectNoExceptions]
			public void TestRunAsyncExitsApplicationOnExceptionInErrorReporter()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(new Exception("error"));
					Test(new InvalidOperationException());
					Test(SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
				});

				void Test(Exception exception)
				{
					// Arrange
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();

					serviceManagerTaskMock
						.Setup(x => x.Run(It.IsAny<CancellationToken>()))
						.Throws(exception);

					var taskList = new List<IServiceManagerTask> { serviceManagerTaskMock.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					errorReporterProxyMock.Reset();
					errorReporterProxyMock
						.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
						.Throws<Exception>();

					// Act
					serviceManagerApplication.Run(CancellationToken.None);

					// Assert
					applicationEmergencyExitMock.Verify(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), exception), Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
					serviceManagerTasksMock.Verify(x => x.GetEnumerator(), Times.AtLeastOnce);
					serviceManagerTaskMock.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
				}
			}

			[ExpectNoExceptions]
			public void TestInitializeAsyncExitsApplicationOnExceptionInErrorReporter()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(new Exception("error"));
					Test(new InvalidOperationException());
					Test(SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
				});

				void Test(Exception exception)
				{
					// Arrange
					var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
					serviceManagerTaskMock
						.Setup(x => x.Initialise(It.IsAny<CancellationToken>()))
						.Throws(exception);

					var taskList = new List<IServiceManagerTask> { serviceManagerTaskMock.Object };

					serviceManagerTasksMock
						.Setup(tasks => tasks.GetEnumerator())
						.Returns(() => taskList.GetEnumerator());

					errorReporterProxyMock.Reset();
					errorReporterProxyMock
						.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
						.Throws<Exception>();

					// Act
					serviceManagerApplication.Run(CancellationToken.None);

					// Assert
					applicationEmergencyExitMock.Verify(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), exception), Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
					serviceManagerTasksMock.Verify(x => x.GetEnumerator(), Times.AtLeastOnce);
					serviceManagerTaskMock.Verify(x => x.Initialise(It.IsAny<CancellationToken>()), Times.Once);
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestDoesNotHangUpOnExceptionInErrorReporter()
			{
				// Arrange
				var failedServiceManagerTaskMock = new Mock<IServiceManagerTask>();
				failedServiceManagerTaskMock
					.Setup(x => x.Run(It.IsAny<CancellationToken>()))
					.Throws<Exception>();

				var goodServiceManagerTaskMock = new Mock<IServiceManagerTask>();

				var taskList = new List<IServiceManagerTask>
				{
					failedServiceManagerTaskMock.Object, goodServiceManagerTaskMock.Object,
				};

				serviceManagerTasksMock
					.Setup(tasks => tasks.GetEnumerator())
					.Returns(() => taskList.GetEnumerator());

				errorReporterProxyMock.Reset();
				errorReporterProxyMock
					.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
					.Throws<Exception>();

				using var cancellationTokenSource = new CancellationTokenSource();
				applicationEmergencyExitMock
					.Setup(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback(() =>
					{
						cancellationTokenSource.Cancel();
					});

				// Act
				var result = Task.Run(() =>
					{
						serviceManagerApplication.Run(cancellationTokenSource.Token);
					})
					.Wait(TimeSpan.FromSeconds(15));

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(true), "Exited by itself");
			}

			[ExpectNoExceptions]
			public void TestRunAsyncExitsOnInitializationLockTimeoutException()
			{
				// Arrange
				var lockException = new InitializationLockTimeoutException();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();

				serviceManagerTaskMock
					.Setup(x => x.Run(It.IsAny<CancellationToken>()))
					.Throws(lockException);

				var taskList = new List<IServiceManagerTask> { serviceManagerTaskMock.Object };

				serviceManagerTasksMock
					.Setup(tasks => tasks.GetEnumerator())
					.Returns(() => taskList.GetEnumerator());

				errorReporterProxyMock.Reset();
				errorReporterProxyMock
					.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

				// Act
				serviceManagerApplication.Run(CancellationToken.None);

				// Assert
				applicationEmergencyExitMock.Verify(proxy => proxy.ExitApplicationUnsafe(It.IsAny<string>(), lockException), Times.Once);
				applicationEmergencyExitMock.VerifyNoOtherCalls();
				serviceManagerTasksMock.Verify(x => x.GetEnumerator(), Times.AtLeastOnce);
				serviceManagerTaskMock.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			}

			Mock<IApplicationEmergencyExit> applicationEmergencyExitMock;
			Mock<IDelayProvider> delayProviderMock;
			ServiceManagerApplication serviceManagerApplication;
			Mock<IServiceManagerApplicationInitializer> serviceManagerApplicationInitializerMock;
			Mock<IEnumerable<IServiceManagerTask>> serviceManagerTasksMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
		}

		public class DatabaseAccessTest : TransactionedTestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				serviceManagerApplicationInitializerMock = new Mock<IServiceManagerApplicationInitializer>();
				serviceManagerTasksMock = new Mock<IEnumerable<IServiceManagerTask>>();
				delayProviderMock = new Mock<IDelayProvider>();
				applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				serviceManagerApplication = new ServiceManagerApplication(serviceManagerApplicationInitializerMock.Object, delayProviderMock.Object, serviceManagerTasksMock.Object, applicationEmergencyExitMock.Object, errorReporterProxyMock.Object);
			}

			public void TestDatabaseAccessIsPermittedInInitialise()
			{
				// Arrange
				using var cancellationTokenSource = new CancellationTokenSource();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				serviceManagerTaskMock
					.Setup(task => task.Initialise(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						ExecuteTestSql();
						cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
					});
				serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

				// Act
				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				});
			}

			public void TestDatabaseAccessIsPermittedInRun()
			{
				// Arrange
				using var cancellationTokenSource = new CancellationTokenSource();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				serviceManagerTaskMock
					.Setup(task => task.Run(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						ExecuteTestSql();
						cancellationTokenSource.Cancel();
					});
				serviceManagerTasksMock.Setup(tasks => tasks.GetEnumerator()).Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

				// Act
				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				});
			}

			public void TestDatabaseAccessIsPermittedInInitialiseExceptionHandler()
			{
				// Arrange
				using var cancellationTokenSource = new CancellationTokenSource();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				serviceManagerTaskMock
					.Setup(x => x.Initialise(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new Exception("Test exception from mock.");
					});

				errorReporterProxyMock.Reset();
				errorReporterProxyMock
					.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback(ExecuteTestSql);

				serviceManagerTasksMock
					.Setup(tasks => tasks.GetEnumerator())
					.Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
				});
			}

			public void TestDatabaseAccessIsPermittedInInitialiseCriticalExceptionHandler()
			{
				// Arrange
				using var cancellationTokenSource = new CancellationTokenSource();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				serviceManagerTaskMock
					.Setup(x => x.Initialise(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new OutOfMemoryException();
					});

				applicationEmergencyExitMock
					.Setup(x => x.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback((string msg, Exception exception) =>
					{
						errorReporterProxyMock.Object.ReportOnce(msg, exception);
						ExecuteTestSql();
					});

				serviceManagerTasksMock
					.Setup(tasks => tasks.GetEnumerator())
					.Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

				// Act
				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceManagerTaskMock.Verify(task => task.Initialise(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
				});
			}

			public void TestDatabaseAccessIsPermittedInRunExceptionHandler()
			{
				// Arrange
				using var cancellationTokenSource = new CancellationTokenSource();
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				serviceManagerTaskMock
					.Setup(x => x.Run(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new Exception("Test exception from mock.");
					});

				errorReporterProxyMock
					.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback(ExecuteTestSql);

				serviceManagerTasksMock
					.Setup(tasks => tasks.GetEnumerator())
					.Returns(() => new[] { serviceManagerTaskMock.Object }.AsEnumerable().GetEnumerator());

				// Act
				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceManagerTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
					errorReporterProxyMock.Verify(task => task.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
				});
			}

			Mock<IApplicationEmergencyExit> applicationEmergencyExitMock;
			Mock<IDelayProvider> delayProviderMock;
			ServiceManagerApplication serviceManagerApplication;
			Mock<IServiceManagerApplicationInitializer> serviceManagerApplicationInitializerMock;
			Mock<IEnumerable<IServiceManagerTask>> serviceManagerTasksMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
		}

		internal static void ExecuteTestSql()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Db.Connection.ExecuteScalar("SELECT @@servername");
			}
		}
	}
}
