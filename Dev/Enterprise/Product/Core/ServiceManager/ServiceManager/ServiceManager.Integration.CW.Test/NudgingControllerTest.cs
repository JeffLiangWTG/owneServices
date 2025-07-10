using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Exceptions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;
using NudgeEventArgs = ServiceManager.Integration.NudgingClient.Abstractions.EventArgs.NudgeEventArgs;
using NudgeFailedEventArgs = ServiceManager.Integration.NudgingClient.Abstractions.EventArgs.NudgeFailedEventArgs;

namespace Enterprise.ServiceManager.HostsController.Test
{
	class NudgingControllerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			nudgingFailedUserNotificationMock = new Mock<INudgingFailedUserNotification>();
			hostedServiceBusinessObjectBindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
			nudgeClientMock = new Mock<INudgeClient>();
			nudgeClientFactoryMock = new Mock<Func<INudgeClient>>();
			nudgeClientFactoryMock
				.Setup(func => func())
				.Returns(nudgeClientMock.Object);
			predicateFactoryMock = new Mock<IPredicateFactory>();
			nudgingController = new NudgingController(nudgingFailedUserNotificationMock.Object,
				hostedServiceBusinessObjectBindingsProviderMock.Object,
				predicateFactoryMock.Object,
				nudgeClientFactoryMock.Object);
		}

		//Remove the = null! after upgrading from NUnitCore to NUnit4, as CS8618 gets suppressed by NUnit3002 https://docs.nunit.org/articles/nunit-analyzers/NUnit3002.html
		Mock<IHostedServiceBusinessObjectBindingsProvider> hostedServiceBusinessObjectBindingsProviderMock = null!;
		Mock<Func<INudgeClient>> nudgeClientFactoryMock = null!;
		Mock<INudgeClient> nudgeClientMock = null!;
		NudgingController nudgingController = null!;
		Mock<INudgingFailedUserNotification> nudgingFailedUserNotificationMock = null!;
		Mock<IPredicateFactory> predicateFactoryMock = null!;

		public class MiscellaneousTest : NudgingControllerTest
		{
			[ExpectNoExceptions]
			public void TestAttachesToINudgingFailedUserNotificationInCtor()
			{
				nudgingFailedUserNotificationMock.Verify(notification => notification.Attach(It.IsAny<INudgingController>()), Times.Once);
				nudgingFailedUserNotificationMock.Verify(notification => notification.Attach(nudgingController), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestDoesInitializeNudgeClientOnDemand()
			{
				// Arrange

				// Act
				nudgingController.ScheduleTasks(Enumerable.Empty<string>());

				// Assert
				nudgeClientFactoryMock.Verify(func => func(), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestDoesNotInitializeNudgeClientInCtor()
			{
				nudgeClientFactoryMock.Verify(func => func(), Times.Never);
			}

			public void TestInstanceForProd()
			{
				// Arrange
				var nudgeClientField = typeof(NudgingController)
					.GetField("nudgeClient", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

				Globals.IsTest_ForTest.Value = false;
				using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
				{
					// Act
					var result = ObjectFactory.Get<INudgingController>();

					// Assert
					AssertType<NudgingController>(result);
					AssertNoExceptionThrown(() => result.ScheduleTasks(Enumerable.Empty<string>()));
					var nudgeClient = (Lazy<INudgeClient>)nudgeClientField!.GetValue(result)!;
					AssertEquals(expected: true, nudgeClient.IsValueCreated);
					AssertType<NudgeClient>(nudgeClient.Value);
				}
			}

			public void TestWrongParamsCall()
			{
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NudgingController(null!, hostedServiceBusinessObjectBindingsProviderMock.Object, predicateFactoryMock.Object, nudgeClientFactoryMock.Object));
					AssertEquals("nudgingFailedUserNotification", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NudgingController(nudgingFailedUserNotificationMock.Object, null!, predicateFactoryMock.Object, nudgeClientFactoryMock.Object));
					AssertEquals("bindingsProvider", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NudgingController(nudgingFailedUserNotificationMock.Object, hostedServiceBusinessObjectBindingsProviderMock.Object, null!, nudgeClientFactoryMock.Object));
					AssertEquals("predicateFactory", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NudgingController(nudgingFailedUserNotificationMock.Object, hostedServiceBusinessObjectBindingsProviderMock.Object, predicateFactoryMock.Object, null!));
					AssertEquals("nudgeClientFactory", result.ParamName);
				});
			}

			public class ReportNudgeTest : NudgingControllerTest
			{
				public void TestReportNudgeFailed_Description()
				{
					// Arrange
					var count = 0;
					global::ServiceManager.Integration.Abstractions.NudgeFailedEventArgs? resultNudgeFailedEventArgs = null;
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
						resultNudgeFailedEventArgs = args;
					};

					CombineAssertions(() =>
					{
						Test(new[] { "Code1" }, "SomeDesc1", 0);
						Test(new[] { "Code1", "Code2" }, "SomeDesc2", 2);
						Test(new[] { "Code1", "Code2", "Code3" }, "SomeDesc3", 3);
					});

					void Test(string[] taskCodes, string description, int retriesRemaining)
					{
						count = 0;
						resultNudgeFailedEventArgs = null;

						// Act
						nudgingController.ReportNudgeFailed(taskCodes, description, retriesRemaining);

						// Assert
						AssertEquals(1, count);
						AssertNull(resultNudgeFailedEventArgs?.Exception);
						AssertContainsExactElementsInAnyOrder(taskCodes, resultNudgeFailedEventArgs?.TaskCodes);
						AssertEquals(description, resultNudgeFailedEventArgs?.Description);
						AssertEquals(retriesRemaining, resultNudgeFailedEventArgs?.RetriesRemaining);
					}
				}

				public void TestReportNudgeFailed_Description_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeFailed(Enumerable.Empty<string>(), string.Empty, 13));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}

				public void TestReportNudgeFailed_Exception()
				{
					// Arrange
					var count = 0;
					global::ServiceManager.Integration.Abstractions.NudgeFailedEventArgs? resultNudgeFailedEventArgs = null;
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
						resultNudgeFailedEventArgs = args;
					};

					CombineAssertions(() =>
					{
						Test(new[] { "Code1" }, new Exception("SomeDesc1"), 0);
						Test(new[] { "Code1", "Code2" }, new InvalidOperationException("SomeDesc2"), 2);
						Test(new[] { "Code1", "Code2", "Code3" }, new ArgumentException("SomeDesc3"), 3);
					});

					void Test(string[] taskCodes, Exception exception, int retriesRemaining)
					{
						count = 0;
						resultNudgeFailedEventArgs = null;

						try
						{
							// Act
							nudgingController.ReportNudgeFailed(taskCodes, exception, retriesRemaining);

							// Assert
							AssertEquals(1, count);
							AssertContainsExactElementsInAnyOrder(taskCodes, resultNudgeFailedEventArgs?.TaskCodes);
							AssertEquals(exception, resultNudgeFailedEventArgs?.Exception);
							AssertEquals(retriesRemaining, resultNudgeFailedEventArgs?.RetriesRemaining);
							AssertEquals(exception.ToString(), resultNudgeFailedEventArgs?.Description);
							AssertEquals("Exception during nudging", ErrorReporter.LastMessageReported);
						}
						finally
						{
							ErrorReporter.Clear();
						}
					}
				}

				public void TestReportNudgeFailed_Exception_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					var exception = new Exception();

					using (new DisposableAction(ErrorReporter.Clear))
					{
						// Act
						var thread = new Thread(() => nudgingController.ReportNudgeFailed(Enumerable.Empty<string>(), exception, 13));
						thread.Start();
						thread.Join();

						// Assert
						AssertEquals(exception, ErrorReporter.LastExceptionReported);
						AssertEquals(1, ErrorReporter.TotalErrorCount);
					}
				}

				public void TestReportNudgeFailed_Exception_IsIgnoredExceptionType()
				{
					// Arrange
					var count = 0;
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
					};

					CombineAssertions(() =>
					{
						Test(new Exception(), expected: true);
						Test(new InvalidOperationException(), expected: true);
						Test(new ServiceHostCommunicationException(), expected: false);
						Test(new UnsupportedTaskException(), expected: false);
						Test(new TaskCanceledException(), expected: false);
						Test(new NoAvailableHostsException(), expected: false);
					});

					void Test(Exception exception, bool expected)
					{
						count = 0;

						try
						{
							// Act
							nudgingController.ReportNudgeFailed(Enumerable.Empty<string>(), exception, 0);

							// Assert
							AssertEquals(1, count);
							AssertEquals(expected ? 1 : 0, ErrorReporter.TotalErrorCount);
						}
						finally
						{
							ErrorReporter.Clear();
						}
					}
				}

				public void TestReportNudgeIgnored_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeIgnored(Enumerable.Empty<string>(), string.Empty));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}

				public void TestReportNudgeStarted_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeStarted(Enumerable.Empty<string>(), new StackTrace()));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}

				public void TestReportNudgeSucceeded_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeSucceeded(Enumerable.Empty<string>()));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}

				public void TestReportNudgeDeferred_Description()
				{
					// Arrange
					NudgeDeferredEventArgs? resultNudgeDeferredEventArgs = null;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						resultNudgeDeferredEventArgs = (NudgeDeferredEventArgs)args;
					};

					CombineAssertions(() =>
					{
						Test(new[] { "Code1" }, "SomeDesc1");
						Test(new[] { "Code1", "Code2" }, "SomeDesc2");
						Test(new[] { "Code1", "Code2", "Code3" }, "SomeDesc3");
					});

					void Test(string[] taskCodes, string description)
					{
						resultNudgeDeferredEventArgs = null;

						// Act
						nudgingController.ReportNudgeDeferred(taskCodes, description);

						// Assert
						AssertContainsExactElementsInAnyOrder(taskCodes, resultNudgeDeferredEventArgs?.TaskCodes);
						AssertEquals(description, resultNudgeDeferredEventArgs?.Description);
					}
				}

				public void TestReportNudgeDeferred_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeDeferred(Enumerable.Empty<string>(), string.Empty));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}

				public void TestReportNudgeAbandoned_Description()
				{
					// Arrange
					NudgeAbandonedEventArgs? resultNudgeAbandonedEventArgs = null;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						resultNudgeAbandonedEventArgs = (NudgeAbandonedEventArgs)args;
					};

					CombineAssertions(() =>
					{
						Test(new[] { "Code1" }, "SomeDesc1");
						Test(new[] { "Code1", "Code2" }, "SomeDesc2");
						Test(new[] { "Code1", "Code2", "Code3" }, "SomeDesc3");
					});

					void Test(string[] taskCodes, string description)
					{
						resultNudgeAbandonedEventArgs = null;

						// Act
						nudgingController.ReportNudgeAbandoned(taskCodes, description);

						// Assert
						AssertContainsExactElementsInAnyOrder(taskCodes, resultNudgeAbandonedEventArgs?.TaskCodes);
						AssertEquals(description, resultNudgeAbandonedEventArgs?.Description);
					}
				}

				public void TestReportNudgeAbandoned_DbAccess()
				{
					// Arrange
					nudgingController.NudgeTrackingEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");
					nudgingController.NudgeFailedEvent += (sender, args) => _ = Db.Connection.ExecuteScalar("SELECT @@servername");

					// Act
					var thread = new Thread(() => nudgingController.ReportNudgeAbandoned(Enumerable.Empty<string>(), string.Empty));
					thread.Start();
					thread.Join();

					// Assert
					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}
			}
		}

		public class HostedServiceBusinessObjectBindingsProviderUsageTest : NudgingControllerTest
		{
			[ExpectNoExceptions]
			public void TestLoadedOnce()
			{
				// Arrange
				hostedServiceBusinessObjectBindingsProviderMock
					.SetupGet(provider => provider.BusinessObjectBindings)
					.Returns(ImmutableList<HostedServiceBusinessObjectBindingAttribute>.Empty);

				// Act
				_ = nudgingController.ServiceTaskBindings;
				_ = nudgingController.ServiceTaskBindings;
				_ = nudgingController.ServiceTaskBindings;
				_ = nudgingController.ServiceTaskBindings;
				_ = nudgingController.ServiceTaskBindings;

				// Assert
				hostedServiceBusinessObjectBindingsProviderMock.VerifyGet(provider => provider.BusinessObjectBindings, Times.Once);
			}

			[ExpectNoExceptions]
			public void TestNotLoadedInCtor()
			{
				hostedServiceBusinessObjectBindingsProviderMock.VerifyGet(provider => provider.BusinessObjectBindings, Times.Never);
			}

			[ExpectNoExceptions]
			public void TestReturnsSameResults()
			{
				// Arrange
				hostedServiceBusinessObjectBindingsProviderMock
					.SetupGet(provider => provider.BusinessObjectBindings)
					.Returns(ImmutableList.Create(
						new HostedServiceBusinessObjectBindingAttribute("Code1", "StmServiceHost", new[] { "SH_IsActive=Y" }, null),
						new HostedServiceBusinessObjectBindingAttribute("Code2", "StmServiceHost", new[] { "SH_IsActive=N" }, null),
						new HostedServiceBusinessObjectBindingAttribute("Code3", "StmServiceHost", new[] { "SH_IsActive LIKE 'Y'" }, null)
					));

				// Act
				var results = new[]
				{
					nudgingController.ServiceTaskBindings, nudgingController.ServiceTaskBindings, nudgingController.ServiceTaskBindings,
				};

				// Assert
				AssertEquals(results[0], results[1]);
				AssertEquals(results[1], results[2]);
			}

			public void TestServiceTaskBindings()
			{
				CombineAssertions(() =>
				{
					Test();
					Test(("Code", "Table", Array.Empty<string>()));
					Test(("Code", "Table", new[] { "predicate" }));
					Test(
						("Code1", "Table1", new[] { "predicate1" }),
						("Code2", "Table2", new[] { "predicate2" }));
					Test(
						("Code1", "Table1", new[] { "predicate1" }),
						("Code2", "Table2", new[] { "predicate2" }),
						("Code3", "Table3", new[] { "predicate1", "predicate3" })
					);
				});

				void Test(params (string serviceTaskCode, string table, string[] predicates)[] hostedAttributes)
				{
					// Arrange
					hostedServiceBusinessObjectBindingsProviderMock.Reset();
					hostedServiceBusinessObjectBindingsProviderMock
						.SetupGet(provider => provider.BusinessObjectBindings)
						.Returns(ImmutableList.Create(hostedAttributes
							.Select(tuple => new HostedServiceBusinessObjectBindingAttribute(tuple.serviceTaskCode, tuple.table, tuple.predicates, null))
							.ToArray()));

					foreach (var (_, table, predicates) in hostedAttributes)
					{
						predicateFactoryMock
							.Setup(factory => factory.GeneratePredicates(It.IsAny<IEnumerable<string>>(), table, It.IsAny<int>()))
							.Returns<IEnumerable<string>, string, int>((enumerable, s, i) => enumerable.Select(s1 => new Predicate(s1)));
					}

					nudgingController = new NudgingController(nudgingFailedUserNotificationMock.Object, hostedServiceBusinessObjectBindingsProviderMock.Object, predicateFactoryMock.Object, nudgeClientFactoryMock.Object);

					// Act
					var result = nudgingController.ServiceTaskBindings
						.Select(binding => (
							binding.ServiceTaskCode,
							binding.TableName,
							predicates: binding.Predicates.Select(predicate => predicate.ParameterizedSqlCondition).ToArray()))
						.ToArray();

					// Assert
					AssertContainsExactElementsInAnyOrder(new Comparer(), hostedAttributes, result);
				}
			}

			class Comparer : IEqualityComparer<(string serviceTaskCode, string table, string[] predicates)>
			{
				public bool Equals((string serviceTaskCode, string table, string[] predicates) x, (string serviceTaskCode, string table, string[] predicates) y)
				{
					return x.serviceTaskCode == y.serviceTaskCode
							&& x.table == y.table
							&& !x.predicates.Except(y.predicates).Any()
							&& !y.predicates.Except(x.predicates).Any();
				}

				public int GetHashCode((string serviceTaskCode, string table, string[] predicates) obj)
				{
					throw new NotImplementedException();
				}
			}

			class Predicate : IPredicate
			{
				public Predicate(string predicate)
				{
					this.predicate = predicate;
				}

				public bool Matches<T>(T entityPropertyValue)
				{
					throw new NotImplementedException();
				}

				public string ParameterizedSqlCondition => predicate;

				public IReadOnlyCollection<PredicateParameter> Parameters => throw new NotImplementedException();

				public string ColumnName => throw new NotImplementedException();
				readonly string predicate;
			}
		}

		public class ScheduleTasksUsageTest : NudgingControllerTest
		{
			protected override void SetUp()
			{
				base.SetUp();
				nudgingController.ScheduleTasks(Enumerable.Empty<string>());
			}

			static void RunInTheThread(Action action)
			{
				var thread = new Thread(() => action());
				thread.Start();
				thread.Join();
			}

			public class ScheduleTasksTest : ScheduleTasksUsageTest
			{
				public void TestProvidesRightDefaultParams()
				{
					// Arrange
					nudgeClientMock.Reset();

					// Act
					nudgingController.ScheduleTasks(Enumerable.Empty<string>());

					// Assert
					AssertNoExceptionThrown(() =>
					{
						nudgeClientMock.Verify(client => client.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Once);
						nudgeClientMock.Verify(client => client.ScheduleTasks(It.IsAny<IEnumerable<string>>(), null, null), Times.Once);
					});
				}

				public void TestProvidesRightParams()
				{
					CombineAssertions(() =>
					{
						Test(Enumerable.Empty<string>(), null, null);
						Test(new[] { "code" }, echoes: false, TimeSpan.FromSeconds(15));
						Test(new[] { "code1", "code2" }, echoes: true, TimeSpan.FromSeconds(43));
					});

					void Test(IEnumerable<string> taskCodes, bool? echoes, TimeSpan? delay)
					{
						// Arrange
						nudgeClientMock.Reset();

						// Act
						nudgingController.ScheduleTasks(taskCodes, echoes, delay);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							nudgeClientMock.Verify(client => client.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Once);
							nudgeClientMock.Verify(client => client.ScheduleTasks(taskCodes, echoes, delay), Times.Once);
						});
					}
				}

				public void TestWrongParamsCall()
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => nudgingController.ScheduleTasks(null!));
					AssertEquals("taskCodes", result.ParamName);
				}
			}

			public class NudgeFailedEventTest : ScheduleTasksUsageTest
			{
				public void TestSubscribesToEvent()
				{
					// Arrange
					var count = 0;
					global::ServiceManager.Integration.Abstractions.NudgeFailedEventArgs? resultNudgeFailedEventArgs = null;
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
						resultNudgeFailedEventArgs = args;
					};

					CombineAssertions(() =>
					{
						Test(new TaskCanceledException("Ignored exception"), "Code");
						Test(new ServiceHostCommunicationException("Ignored exception"), "Code1", "Code2");
					});

					void Test(Exception exception, params string[] taskCodes)
					{
						count = 0;
						resultNudgeFailedEventArgs = null;

						// Act
						RunInTheThread(() => nudgeClientMock.Raise(
							client => client.NudgeFailed += null,
							this,
							new NudgeFailedEventArgs(exception, taskCodes.Select(s =>
							{
								var mock = new Mock<ITaskFailedNudge>();
								mock.SetupGet(nudged => nudged.TaskCode).Returns(s);
								return mock.Object;
							}))));

						// Assert
						AssertEquals(1, count);
						AssertContainsExactElementsInAnyOrder(taskCodes, resultNudgeFailedEventArgs?.TaskCodes);
					}
				}

				public void TestEmpty()
				{
					// Arrange
					var count = 0;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						count++;
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
					};

					// Act
					RunInTheThread(() => nudgeClientMock.Raise(
						client => client.NudgeFailed += null,
						this,
						new NudgeFailedEventArgs(new ServiceHostCommunicationException("Ignored exception"), Enumerable.Empty<ITaskFailedNudge>())));

					// Assert
					AssertEquals(0, count);
				}

				public void TestErrors()
				{
					// Arrange
					var resultNudgeFailedEventArgs = new Dictionary<int, global::ServiceManager.Integration.Abstractions.NudgeFailedEventArgs>();
					var unexpectedCount = 0;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						unexpectedCount++;
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						resultNudgeFailedEventArgs.Add(args.RetriesRemaining, args);
					};

					CombineAssertions(() =>
					{
						Test(new TaskCanceledException("Ignored exception"),
							("Code", TaskActionOutcomeDTO.Succeeded, 1));
						Test(new TaskCanceledException("Ignored exception"),
							("Code1", TaskActionOutcomeDTO.Succeeded, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2));
						Test(new TaskCanceledException("Ignored exception"),
							("Code1", TaskActionOutcomeDTO.Succeeded, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2),
							("Code3", TaskActionOutcomeDTO.EnqueuedAlready, 3));
						Test(new TaskCanceledException("Ignored exception"),
							("Code1", TaskActionOutcomeDTO.Succeeded, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2),
							("Code3", TaskActionOutcomeDTO.EnqueuedAlready, 3),
							("Code4", TaskActionOutcomeDTO.RequiresProductRegistration, 4));
					});

					void Test(Exception exception, params (string taskCode, TaskActionOutcomeDTO taskActionOutcome, int retriesRemained)[] values)
					{
						resultNudgeFailedEventArgs.Clear();
						var expectedResult = values
							.GroupBy(tuple => tuple.retriesRemained, tuple => tuple.taskCode)
							.ToDictionary(tuples => tuples.Key, tuples => tuples.ToList());

						// Act
						RunInTheThread(() => nudgeClientMock.Raise(
							client => client.NudgeFailed += null,
							this,
							new NudgeFailedEventArgs(exception, values.Select(tuple =>
							{
								var mock = new Mock<ITaskFailedNudge>();
								mock.SetupGet(nudged => nudged.TaskCode).Returns(tuple.taskCode);
								mock.SetupGet(nudged => nudged.RetriesRemained).Returns(tuple.retriesRemained);
								return mock.Object;
							}))));

						// Assert
						foreach (var pair in expectedResult)
						{
							AssertEquals(expected: true, resultNudgeFailedEventArgs.TryGetValue(pair.Key, out var result));
							AssertContainsExactElementsInAnyOrder(pair.Value, result?.TaskCodes);
						}

						AssertEquals(0, unexpectedCount);
					}
				}
			}

			public class NudgedEventTest : ScheduleTasksUsageTest
			{
				public void TestSubscribesToEvent()
				{
					// Arrange
					var count = 0;
					global::ServiceManager.Integration.Abstractions.NudgeEventArgs? nudgeEventArgs = null;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						count++;
						nudgeEventArgs = args;
					};

					CombineAssertions(() =>
					{
						Test("Code");
						Test("Code1", "Code2");
					});

					void Test(params string[] taskCodes)
					{
						count = 0;
						nudgeEventArgs = null;

						// Act
						RunInTheThread(() => nudgeClientMock.Raise(
							client => client.Nudged += null,
							this,
							new NudgeEventArgs(taskCodes.Select(s =>
							{
								var mock = new Mock<ITaskNudged>();
								mock.SetupGet(nudged => nudged.TaskActionOutcome).Returns(TaskActionOutcomeDTO.Succeeded);
								mock.SetupGet(nudged => nudged.TaskCode).Returns(s);
								return mock.Object;
							}))));

						// Assert
						AssertEquals(1, count);
						AssertContainsExactElementsInAnyOrder(taskCodes, nudgeEventArgs?.TaskCodes);
					}
				}

				public void TestTaskActionOutcomeEmpty()
				{
					// Arrange
					var count = 0;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						count++;
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						count++;
					};

					// Act
					RunInTheThread(() => nudgeClientMock.Raise(
						client => client.Nudged += null,
						this,
						new NudgeEventArgs(Enumerable.Empty<ITaskNudged>())));

					// Assert
					AssertEquals(0, count);
				}

				public void TestTaskActionOutcomeSuccess()
				{
					// Arrange
					var resultNudgeEventArgs = new List<global::ServiceManager.Integration.Abstractions.NudgeEventArgs>();
					var unexpectedCount = 0;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						resultNudgeEventArgs.Add(args);
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						unexpectedCount++;
					};

					CombineAssertions(() =>
					{
						Test(("Code", TaskActionOutcomeDTO.Succeeded, 1));
						Test(("Code1", TaskActionOutcomeDTO.Succeeded, 1),
							("Code2", TaskActionOutcomeDTO.EnqueuedNow, 2));
						Test(("Code1", TaskActionOutcomeDTO.Succeeded, 1),
							("Code2", TaskActionOutcomeDTO.EnqueuedNow, 2),
							("Code3", TaskActionOutcomeDTO.EnqueuedAlready, 3));
					});

					void Test(params (string taskCode, TaskActionOutcomeDTO taskActionOutcome, int retriesRemained)[] values)
					{
						resultNudgeEventArgs.Clear();
						var expectedResult = values
							.Select(tuples => tuples.taskCode);
						AssertEquals("Double check values", expected: true, values.All(tuple => tuple.taskActionOutcome.IsSuccessful()));

						// Act
						RunInTheThread(() => nudgeClientMock.Raise(
							client => client.Nudged += null,
							this,
							new NudgeEventArgs(values.Select(tuple =>
							{
								var mock = new Mock<ITaskNudged>();
								mock.SetupGet(nudged => nudged.TaskCode).Returns(tuple.taskCode);
								mock.SetupGet(nudged => nudged.TaskActionOutcome).Returns(tuple.taskActionOutcome);
								mock.SetupGet(nudged => nudged.RetriesRemained).Returns(tuple.retriesRemained);
								return mock.Object;
							}))));

						// Assert
						var result = resultNudgeEventArgs.SelectMany(args => args.TaskCodes);
						AssertContainsExactElementsInAnyOrder(expectedResult, result);
						AssertEquals(0, unexpectedCount);
					}
				}

				public void TestTaskActionOutcomeUnSuccess()
				{
					// Arrange
					var resultNudgeFailedEventArgs = new Dictionary<(int, string), global::ServiceManager.Integration.Abstractions.NudgeFailedEventArgs>();
					var unexpectedCount = 0;
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						unexpectedCount++;
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						resultNudgeFailedEventArgs.Add((args.RetriesRemaining, args.Description), args);
					};

					CombineAssertions(() =>
					{
						Test(("Code", TaskActionOutcomeDTO.RequiresProductRegistration, 1));
						Test(("Code1", TaskActionOutcomeDTO.RequiresProductRegistration, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2));
						Test(("Code1", TaskActionOutcomeDTO.RequiresProductRegistration, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2));
						Test(("Code1", TaskActionOutcomeDTO.RequiresProductRegistration, 1),
							("Code2", TaskActionOutcomeDTO.NoSuitableHostFound, 2),
							("Code3", TaskActionOutcomeDTO.UnknownTask, 3));
					});

					void Test(params (string taskCode, TaskActionOutcomeDTO taskActionOutcome, int retriesRemained)[] values)
					{
						resultNudgeFailedEventArgs.Clear();
						var expectedResult = values
							.GroupBy(tuple => (tuple.retriesRemained, tuple.taskActionOutcome.ToString("G")))
							.ToDictionary(tuples => tuples.Key, tuples => tuples.ToList());
						AssertEquals("Double check values", expected: true, values.All(tuple => !tuple.taskActionOutcome.IsSuccessful()));

						// Act
						RunInTheThread(() => nudgeClientMock.Raise(
							client => client.Nudged += null,
							this,
							new NudgeEventArgs(values.Select(tuple =>
							{
								var mock = new Mock<ITaskNudged>();
								mock.SetupGet(nudged => nudged.TaskCode).Returns(tuple.taskCode);
								mock.SetupGet(nudged => nudged.TaskActionOutcome).Returns(tuple.taskActionOutcome);
								mock.SetupGet(nudged => nudged.RetriesRemained).Returns(tuple.retriesRemained);
								return mock.Object;
							}))));

						// Assert
						foreach (var pair in expectedResult)
						{
							AssertEquals(expected: true, resultNudgeFailedEventArgs.TryGetValue(pair.Key, out var result));
							AssertContainsExactElementsInAnyOrder(pair.Value.Select(tuple => tuple.taskCode), result?.TaskCodes);
						}

						AssertEquals(0, unexpectedCount);
					}
				}

				public void TestTaskActionOutcomeUnSuccessAndSuccess()
				{
					// Arrange
					var resultNudgeEventArgs = new List<global::ServiceManager.Integration.Abstractions.NudgeEventArgs>();
					nudgingController.NudgeTrackingEvent += (sender, args) =>
					{
						resultNudgeEventArgs.Add(args);
					};
					nudgingController.NudgeFailedEvent += (sender, args) =>
					{
						resultNudgeEventArgs.Add(args);
					};

					var values = Enum.GetValues(typeof(TaskActionOutcomeDTO))
						.Cast<TaskActionOutcomeDTO>()
						.Select((taskActionOutcome, i) => (taskCode: $"Code{i:D5}", taskActionOutcome, retriesRemained: i))
						.ToList();
					var expectedResult = values.Select(tuple => tuple.taskCode);

					// Act
					RunInTheThread(() => nudgeClientMock.Raise(
						client => client.Nudged += null,
						this,
						new NudgeEventArgs(values.Select(tuple =>
						{
							var mock = new Mock<ITaskNudged>();
							mock.SetupGet(nudged => nudged.TaskCode).Returns(tuple.taskCode);
							mock.SetupGet(nudged => nudged.TaskActionOutcome).Returns(tuple.taskActionOutcome);
							mock.SetupGet(nudged => nudged.RetriesRemained).Returns(tuple.retriesRemained);
							return mock.Object;
						}))));

					// Assert
					var result = resultNudgeEventArgs
						.SelectMany(args => args.TaskCodes);
					AssertContainsExactElementsInAnyOrder(expectedResult, result);
				}
			}
		}
	}
}
