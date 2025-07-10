using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Integration.ServiceHostClient.Exceptions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.HostsController.Test
{
	abstract class CommunicationFailureNotificationTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			hostedServiceConfigProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			ObjectFactory.Substitute(hostedServiceConfigProviderMock.Object);
			serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			ObjectFactory.Substitute(serviceHostsCacheMock.Object);

			var serviceHostClientMock = new Mock<IServiceHostClient>();
			serviceHostsCacheMock
				.SetupGet(cache => cache.ConfiguredServiceHosts)
				.Returns(new[] { serviceHostClientMock.Object });
			serviceHostClientMock
				.SetupGet(client => client.HostName)
				.Returns(DefaultProcessControllerHostName);
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}

		void ArrangeServiceConfigProvider(NudgeFailedEventArgs nudgeFailedEventArgs)
		{
			if (nudgeFailedEventArgs.Exception != null)
			{
				return;
			}

			foreach (var taskCode in nudgeFailedEventArgs.TaskCodes)
			{
				hostedServiceConfigProviderMock
					.Setup(provider => provider.GetClientHostedServiceAttribute(taskCode))
					.Returns<string, bool>((s, b) =>
					{
						var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
						hostedServiceConfigMock
							.SetupGet(config => config.Description)
							.Returns($"Long description for {s}");
						return hostedServiceConfigMock.Object;
					});
			}
		}

		void ReportNudgeFailed(NudgeFailedEventArgs nudgeFailedEventArgs)
		{
			if (nudgeFailedEventArgs.Exception == null)
			{
				nudgingController
					.Raise(
						controller => controller.NudgeFailedEvent += null,
						nudgeFailedEventArgs);
			}
			else
			{
				nudgingController
					.Raise(
						controller => controller.NudgeFailedEvent += null,
						nudgeFailedEventArgs);
			}
		}

		const string DefaultProcessControllerHostName = "processControllerHost";

		static readonly (NudgeFailedEventArgs nudgeFailedEventArgs, string expectedHeader, string expectedBody)[] Source =
		{
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":("), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:(

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":["), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:[

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":(", new Exception(":[")), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:(
Inner Error=:[

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":[", new Exception(":(")), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:[
Inner Error=:(

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":(", new RequestTimeoutException(":[", new Exception(":O"))), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:(
Http Request Timeout Error=:[
Inner Error=:O

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
			(new NudgeFailedEventArgs(null, new ServiceHostCommunicationException(":[", new RequestTimeoutException(":(", new Exception(":P"))), 0),
				$"Nudge Failure - System: [{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}] Source Host: [{System.Environment.MachineName}]",
				$@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
:[
Http Request Timeout Error=:(
Inner Error=:P

Process controller host list:
{DefaultProcessControllerHostName}

You are receiving this email because your address is configured in registry System -> Process Controller -> Notifications -> Service Task Nudge Communication Errors Notification email."
			),
		};

		static readonly IEnumerable<string> Emails = new[]
		{
			"email@mail.com",
			"email1@mail.com",
			"email2@mail.com",
			"email3@mail.com",
		};

		//Remove the = null! after upgrading from NUnitCore to NUnit4, as CS8618 gets suppressed by NUnit3002 https://docs.nunit.org/articles/nunit-analyzers/NUnit3002.html
		CommunicationFailureNotification communicationFailureNotification = null!;
		Mock<IClientHostedServiceAttributeProvider> hostedServiceConfigProviderMock = null!;
		Mock<INudgingController> nudgingController = null!;
		Mock<IOutgoingMailManager> outgoingMailManagerMock = null!;
		Mock<IServiceHostsCache> serviceHostsCacheMock = null!;

		[UseSnapshotProtection]
		public class Test : CommunicationFailureNotificationTest
		{
			protected override void SetUp()
			{
				base.SetUp();
				outgoingMailManagerMock = new Mock<IOutgoingMailManager>(MockBehavior.Strict);
				ObjectFactory.Substitute(outgoingMailManagerMock.Object);
				nudgingController = new Mock<INudgingController>();
				communicationFailureNotification = new CommunicationFailureNotification();
				communicationFailureNotification.Attach(nudgingController.Object);
			}

			public void TestEmailIsSentToServiceTaskNudgeCommunicationErrorNotificationEmail()
			{
				var hostLists = new (IEnumerable<string> hostNames, string expectedHosts)[]
				{
					(new[] { DefaultProcessControllerHostName }, DefaultProcessControllerHostName),
					(new[] { "host" }, "host"),
					(new[] { "host1", "host2" }, "host1\r\nhost2"),
					(new[] { "host1", "host2", "host3" }, "host1\r\nhost2\r\nhost3"),
				};

				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						foreach (var (nudgeFailedEventArgs, expectedHeader, expectedBody) in Source)
						{
							foreach (var (hostNames, expectedHosts) in hostLists)
							{
								var serviceHostClientMocks = hostNames
									.Select(hostName =>
									{
										var serviceHostClientMock = new Mock<IServiceHostClient>();
										serviceHostClientMock
											.SetupGet(client => client.HostName)
											.Returns(hostName);
										return serviceHostClientMock;
									})
									.ToList();

								serviceHostsCacheMock
									.SetupGet(cache => cache.ConfiguredServiceHosts)
									.Returns(serviceHostClientMocks.Select(mock => mock.Object));

								var expectedEmailBody = expectedBody
									.Replace(DefaultProcessControllerHostName, expectedHosts);

								Test(nudgeFailedEventArgs, expectedHeader, expectedEmailBody, email);
							}
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, string expectedHeader, string expectedBody, string email)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email))
					{
						Factory.Save();

						EmailDef? emailDef = null;
						outgoingMailManagerMock.Reset();
						outgoingMailManagerMock
							.Setup(manager => manager.CreateAndSave(It.IsAny<EmailDef>()))
							.Callback<EmailDef>(def => emailDef = def);

						ArrangeServiceConfigProvider(nudgeFailedEventArgs);

						// Act
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Once));
						AssertEquals(expectedHeader, emailDef?.Subject);
						AssertEquals(expectedBody, emailDef?.Body);
						var result = emailDef
							?.Recipients
							.Cast<RecipientDef>()
							.Select(def => def.Email)
							.SingleOrDefault();
						AssertEquals(email, result);
					}
				}
			}

			public void TestEmailIsNotSentIfServiceTaskNudgeCommunicationErrorNotificationEmailIsEmpty()
			{
				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						foreach (var (nudgeFailedEventArgs, _, _) in Source)
						{
							Test(nudgeFailedEventArgs, email);
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, string email)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
					{
						Factory.Save();

						// Act
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never));
					}
				}
			}

			public void TestEmailIsNotSentIfNotCommunicationError()
			{
				var source = new[]
				{
					new NudgeFailedEventArgs(null, new UnsupportedTaskException(":("), 0),
					new NudgeFailedEventArgs(null, new UnsupportedTaskException(":["), 0),
					new NudgeFailedEventArgs(null, new Exception(":("), 0),
					new NudgeFailedEventArgs(null, new Exception(":["), 0),
					new NudgeFailedEventArgs(new[] { "Task" }, ":(", 0),
					new NudgeFailedEventArgs(new[] { "Task1", "Task2" }, ":[", 0),
				};

				CombineAssertions(() =>
				{
					foreach (var nudgeFailedEventArgs in source)
					{
						Test(nudgeFailedEventArgs);
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Emails.First()))
					{
						Factory.Save();

						// Act
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never));
					}
				}
			}

			public void TestEmailIsNotSentIfRetriesPositive()
			{
				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						for (var i = 0; i < Source.Length; i++)
						{
							var tuple = Source[i];
							Test(tuple.nudgeFailedEventArgs, i + 1, email);
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, int retriesRemaining, string email)
				{
					// Arrange
					nudgeFailedEventArgs = nudgeFailedEventArgs.Exception == null
						? new NudgeFailedEventArgs(nudgeFailedEventArgs.TaskCodes, nudgeFailedEventArgs.Description, retriesRemaining)
						: new NudgeFailedEventArgs(nudgeFailedEventArgs.TaskCodes, nudgeFailedEventArgs.Exception, retriesRemaining);
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email))
					{
						Factory.Save();

						// Act
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never));
					}
				}
			}

			[SnailTest]
			public void TestEmailIsNotSentIfAlreadyBeenSentAndNotTimedOutYet()
			{
				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						foreach (var (nudgeFailedEventArgs, _, _) in Source)
						{
							Test(nudgeFailedEventArgs, email);
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, string email)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email))
					{
						Factory.Save();

						outgoingMailManagerMock.Reset();
						outgoingMailManagerMock
							.Setup(manager => manager.CreateAndSave(It.IsAny<EmailDef>()));
						ReportNudgeFailed(nudgeFailedEventArgs);

						outgoingMailManagerMock.Invocations.Clear();

						// Act
						Thread.Sleep(TimeSpan.FromSeconds(5));
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never));
					}
				}
			}

			[SnailTest]
			public void TestEmailIsNotSentIfAlreadyBeenSentByTheOtherInstanceAndNotTimedOutYet()
			{
				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						foreach (var (nudgeFailedEventArgs, _, _) in Source)
						{
							Test(nudgeFailedEventArgs, email);
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, string email)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email))
					using (ApplySemaphoreLock())
					{
						Factory.Save();

						outgoingMailManagerMock.Reset();
						outgoingMailManagerMock
							.Setup(manager => manager.CreateAndSave(It.IsAny<EmailDef>()));

						// Act
						Thread.Sleep(TimeSpan.FromSeconds(5));
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never));
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDoesNotTryToApplySemaphoreIfFrequencyIsZero()
			{
				// Arrange
				using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Emails.First()))
				using (ApplySemaphoreLock())
				{
					Factory.Save();

					outgoingMailManagerMock
						.Setup(manager => manager.CreateAndSave(It.IsAny<EmailDef>()));
					Thread.Sleep(TimeSpan.FromSeconds(5));

					// Act
					ReportNudgeFailed(Source[0].nudgeFailedEventArgs);

					// Assert
					AssertNoExceptionThrown(() => outgoingMailManagerMock
						.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Once));
				}
			}

			[SnailTest]
			public void TestEmailIsSentIfAlreadyBeenSentAndTimedOutAlready()
			{
				CombineAssertions(() =>
				{
					foreach (var email in Emails)
					{
						foreach (var (nudgeFailedEventArgs, _, _) in Source)
						{
							Test(nudgeFailedEventArgs, email);
						}
					}
				});

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs, string email)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email))
					{
						Factory.Save();

						outgoingMailManagerMock.Reset();
						outgoingMailManagerMock
							.Setup(manager => manager.CreateAndSave(It.IsAny<EmailDef>()));
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Act
						Thread.Sleep(TimeSpan.FromSeconds(5));
						ReportNudgeFailed(nudgeFailedEventArgs);

						// Assert
						AssertNoExceptionThrown(() => outgoingMailManagerMock
							.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Exactly(2)));
					}
				}
			}

			public void TestSendingEmail_DoesNotTrigger_FactorySaveAlert()
			{
				foreach (var (nudgeFailedEventArgs, _, _) in Source)
				{
					Test(nudgeFailedEventArgs);
				}

				void Test(NudgeFailedEventArgs nudgeFailedEventArgs)
				{
					// Arrange
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Emails.First()))
					{
						Factory.Save();

						outgoingMailManagerMock.Reset();
						outgoingMailManagerMock
							.Setup(x => x.CreateAndSave(It.IsAny<EmailDef>()))
							.Callback(() => Factory.Save());

						using (new FactorySaveAlerter(() => "ReportNudgeFailed", nameof(TestSendingEmail_DoesNotTrigger_FactorySaveAlert)))
						{
							// Act
							ReportNudgeFailed(nudgeFailedEventArgs);
						}
					}

					// Assert
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}

			static IDisposable ApplySemaphoreLock()
			{
				var semaphoreDbManager = new SemaphoreDbManager();
				var uniqueId = Guid.Parse("00000000-0000-0000-0000-000000000001");
				semaphoreDbManager.CreateHeartbeatInDatabase(uniqueId,
					"hostName",
					1234,
					Guid.Empty,
					GlbStaffSchema.Constants.Prefix,
					10,
					"PRC",
					null);
				semaphoreDbManager.CreateSemaphoreHandleInTransaction(uniqueId,
					$"ComErrNotifier:{System.Environment.MachineName}",
					"PRC",
					1);
				return new DisposableAction(() =>
				{
					semaphoreDbManager.DeleteHeartbeatFromDatabase(uniqueId);
				});
			}
		}

		class IntegrationTest : CommunicationFailureNotificationTest
		{
			protected override void SetUp()
			{
				base.SetUp();
				nudgingController = new Mock<INudgingController>();
				communicationFailureNotification = new CommunicationFailureNotification();
				communicationFailureNotification.Attach(nudgingController.Object);
			}

			public void TestEmailIsSentToHostingWithoutRedirection()
			{
				// Arrange
				var expectedEmail = EnvProxy.Instance.Registry.HostedNotificationsEmailOverride;
				const string unexpectedEmail = "unexpected@ema.il";
				var (nudgeFailedEventArgs, _, expectedBody) = Source[0];

				using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				using (EnvProxy.Instance.Registry.RawRegistry.EmailDestinationOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unexpectedEmail))
				using (EnvProxy.Instance.Registry.RawRegistry.SystemEmailDestinationOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedEmail))
				{
					Factory.Save();

					ArrangeServiceConfigProvider(nudgeFailedEventArgs);

					// Act
					ReportNudgeFailed(nudgeFailedEventArgs);

					// Assert
					CombineAssertions(() =>
					{
						var result = Factory.Load<MailItem>(new ZQuery()).Single();
						AssertEquals(expectedBody, result.MI_Body);
						AssertEquals(expectedEmail, result.AllRecipients);
					});
				}
			}
		}
	}
}
