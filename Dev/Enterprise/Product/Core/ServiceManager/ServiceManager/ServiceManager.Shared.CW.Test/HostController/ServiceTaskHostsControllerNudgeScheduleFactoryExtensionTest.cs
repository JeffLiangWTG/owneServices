using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Public.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Integration.NudgingClient.Abstractions.EventArgs;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using NudgeEventArgs = ServiceManager.Integration.NudgingClient.Abstractions.EventArgs.NudgeEventArgs;
using NudgeFailedEventArgs = ServiceManager.Integration.NudgingClient.Abstractions.EventArgs.NudgeFailedEventArgs;

namespace Enterprise.ServiceManager.HostsController.Test
{
	sealed class ServiceTaskHostsControllerNudgeScheduleFactoryExtensionTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestFactoryIsNotUsedDuringUpgradeProcess()
		{
			var systemDataRegistryMock = new Mock<ISystemDataRegistry>();
			systemDataRegistryMock.Setup(m => m.ServiceTaskBusinessObjectBindingEnabled).Returns(true);

			DisposableAction DisposableTestEnvironment()
			{
				var originalHostedLocation = EnvProxy.HostedLocation;
				var savedPath = Env.Registry.BackupDirectoryPath;
				EnvProxy.SetHostedLocationForTest("SYD");
				Env.Registry.BackupDirectoryPath = Guid.NewGuid().ToString("D");
				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

				return new DisposableAction(() =>
				{
					EnvProxy.SetHostedLocationForTest(originalHostedLocation);
					Env.Registry.BackupDirectoryPath = savedPath;
					EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				});
			}

			var loggerMock = new Mock<Integration.ILogger>();
			var alwaysOnAutomation = ObjectFactory.Get<ISqlAlwaysOnAutomation>(nameof(ISqlAlwaysOnAutomation), loggerMock.Object);

			using (DisposableTestEnvironment())
			using (ObjectFactory.Substitute(systemDataRegistryMock.Object))
			using (((IDbUpgradeSupport)Db.Instance).SetUpgradeWorkingInProgress())
			{
				Assert("Database upgrade is in progress", Db.IsUpgradeWorkingInProgress);

				var nudgingStarted = false;
				ObjectFactory.Get<INudgingController>().NudgeTrackingEvent += (sender, args) => { nudgingStarted = true; };

				alwaysOnAutomation.AddDatabaseToAlwaysOnGroup("blah blah");

				AssertEquals("No emails created", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				Assert("Nudging should not have been started", !nudgingStarted);
			}

			Assert("Database upgrade process has completed", !Db.IsUpgradeWorkingInProgress);
		}

		public void TestRegistryItemIsChecked()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension();
				var prevNumCommmands = Db.Connection.ExecutedCommandCount;
				wrapper.OnFactoryBusinessObjectsSaved(Factory, new List<BusinessObject>());

				AssertCollectionContains("DB hit to read registry",
					$@"[{Db.DatabaseName}].[dbo].[DataRegGetValueNOD]
Params
@Name: 'ServiceTaskBusinessObjectBindingEnabled'
", Db.Connection.ExecutedCommands);
			}
		}

		[ExpectNoExceptions]
		public void TestOnFactoryBusinessObjectsSavedAccessesBusinessObjectsFromRightThread()
		{
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizO1 = Factory.New<DummyBizoThatPretendsToAccessDB>();
				var mockNudgeController = new Mock<INudgingController>();
				var mockPredicate = new Mock<IPredicate>();
				mockPredicate.Setup(p => p.ColumnName).Returns("Z0_PropertyThatAccessesDB");
				var predicates = new List<IPredicate>() { mockPredicate.Object };
				var serviceTaskBindings = new List<IServiceTaskBinding>() { new ServiceTaskBinding("TST", bizO1.TableName, predicates) };
				mockNudgeController.Setup(m => m.ServiceTaskBindings).Returns(serviceTaskBindings);
				mockNudgeController.Setup(c => c.ReportNudgeStarted(null, null));
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(mockNudgeController.Object, BusinessObjectToServiceTaskMapper.Instance);

				wrapper.OnFactoryBusinessObjectsSaved(Factory, new List<BusinessObject> { bizO1 });
				Thread.Sleep(1000); //Wait to see if crash occurs in the background.

				mockNudgeController.VerifyAll();
				mockPredicate.VerifyAll();
			}
		}

		public void TestOnFactoryBusinessObjectsSavedIsNotRuinedByClientSpecificServiceTasks()
		{
			// Arrange
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (SubstituteNudgingController())
			{
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension();

				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.None))
				{
					var bizO1 = Factory.New<DummyBizoThatPretendsToAccessDB>();

					// Act
					wrapper.OnFactoryBusinessObjectsSaved(Factory, new List<BusinessObject> { bizO1 });

					// Assert
					AssertNull(ErrorReporter.LastExceptionReported);
				}
			}

			static IDisposable SubstituteNudgingController()
			{
				var nudgingControllerInstance = new NudgingController(
					new CommunicationFailureNotification(),
					HostedServiceBusinessObjectBindingsProvider.Instance,
					new PredicateFactory(new NudgingSchemaResolver(new EnterpriseSchemaResolver())),
					() => new NudgeClientStub());
				var nudgingControllerSubstitution = ObjectFactory.Substitute<INudgingController>(nudgingControllerInstance);

				var providerSubstitution = ObjectFactory.Substitute<IHostedServiceBusinessObjectBindingsProvider>(new HostedServiceBusinessObjectBindingsProvider());
				var wrapperSubstitution = ObjectFactory.Substitute<IFactoryProcessingExtension>(new ServiceTaskHostsControllerNudgeScheduleFactoryExtension());

				return new DisposableAction(() =>
				{
					providerSubstitution.Dispose();
					wrapperSubstitution.Dispose();
					nudgingControllerSubstitution.Dispose();
				});
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestScheduleServiceTaskBusinessObjectBindings()
		{
			// Need a connection not in a transaction since nudges are sent after the outermost transaction is commited
			using (var disposable = new DisposableAction(
				() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
				() =>
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
				}))
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var controller = new Mock<INudgingController>();
				var mapper = new Mock<IBusinessObjectToServiceTaskMapper>();
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(controller.Object, mapper.Object);
				var mappedTasks = new Dictionary<string, ICollection<string>>
				{
					{ "BLA", null }
				};
				mapper.Setup(m => m.MapBusinessObjectsToServiceTasks(controller.Object, null)).Returns(mappedTasks);

				wrapper.OnFactoryBusinessObjectsSaved(new BusinessObjectFactory(Db.Connection), null);
				controller.Verify(c => c.ReportNudgeStarted(null, null));
				controller.Verify(c => c.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "BLA"), null, null));
			}
		}

		[ExpectNoExceptions]
		public void TestServiceTaskBindingsFailureReporting_WebExceptionDuringInvocation()
		{
			// Arrange
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var controller = new Mock<INudgingController>();
				var webException = new ServiceHostCommunicationException();
				controller.Setup(m => m.ServiceTaskBindings).Throws(webException);
				var bizO1 = Factory.New<DummyBusinessObject>();
				var bizOs = new List<BusinessObject> { bizO1 };
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(controller.Object, BusinessObjectToServiceTaskMapper.Instance);

				// Act
				wrapper.OnFactoryBusinessObjectsSaved(Factory, bizOs);

				// Assert
				controller.Verify(c => c.ReportNudgeStarted(null, null));
				controller.Verify(c => c.ReportNudgeFailed(It.IsAny<IEnumerable<string>>(), webException, It.IsAny<int>()));
			}
		}

		[ExpectNoExceptions]
		public void TestNudgeInTransaction_IsDeferredToOuterCommit()
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory(connection);
				var controller = new Mock<INudgingController>();
				var mapper = new Mock<IBusinessObjectToServiceTaskMapper>();
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(controller.Object, mapper.Object);
				var mappedTasks = new Dictionary<string, ICollection<string>>
				{
					{ "BLA", null }
				};
				mapper.Setup(m => m.MapBusinessObjectsToServiceTasks(controller.Object, null)).Returns(mappedTasks);
				var errorReporterMock = new Mock<IErrorReporter>();

				// Act/Assert
				connection.BeginTransaction();
				wrapper.OnFactoryBusinessObjectsSaved(factory, null);
				controller.Verify(c => c.ScheduleTasks(It.IsAny<IEnumerable<string>>(), null, null), Times.Never);
				controller.Verify(c => c.ReportNudgeDeferred(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Never);
				connection.CommitTransaction();
				controller.Verify(c => c.ScheduleTasks(It.IsAny<IEnumerable<string>>(), null, null), Times.Once);
				controller.Verify(c => c.ReportNudgeDeferred(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestNudgeInTransaction_IsAbandonedWhenTransactionIsRolledBack()
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory(connection);
				var controller = new Mock<INudgingController>();
				var mapper = new Mock<IBusinessObjectToServiceTaskMapper>();
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(controller.Object, mapper.Object);
				var mappedTasks = new Dictionary<string, ICollection<string>>
				{
					{ "BLA", null }
				};
				mapper.Setup(m => m.MapBusinessObjectsToServiceTasks(controller.Object, null)).Returns(mappedTasks);
				var errorReporterMock = new Mock<IErrorReporter>();
				connection.BeginTransaction();
				wrapper.OnFactoryBusinessObjectsSaved(factory, null);
				controller.Verify(c => c.ReportNudgeAbandoned(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Never);

				// Act
				connection.RollbackTransaction();

				// Assert
				controller.Verify(c => c.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Never);
				controller.Verify(c => c.ReportNudgeAbandoned(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Once);
			}
		}

		public void TestNudgeInTransaction_ReportNudgeFailed()
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory(connection);
				var controller = new Mock<INudgingController>();
				var exception = new InvalidOperationException("oops");
				controller.Setup(x => x.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()))
					.Throws(exception);
				var callbackWasCalled = false;
				controller.Setup(x => x.ReportNudgeFailed(null, exception, 0))
					.Callback(() =>
					{
						callbackWasCalled = true;
						// Verify the connection can be used to issue further commands
						connection.Command("-- do nothing")
							.ExecuteNonQuery();
					});
				var mapper = new Mock<IBusinessObjectToServiceTaskMapper>();
				var wrapper = new ServiceTaskHostsControllerNudgeScheduleFactoryExtension(controller.Object, mapper.Object);
				var mappedTasks = new Dictionary<string, ICollection<string>>
				{
					{ "BLA", null }
				};
				mapper.Setup(m => m.MapBusinessObjectsToServiceTasks(controller.Object, null)).Returns(mappedTasks);
				var errorReporterMock = new Mock<IErrorReporter>();
				errorReporterMock.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));
				connection.BeginTransaction();
				wrapper.OnFactoryBusinessObjectsSaved(factory, null);

				// Act/Assert
				AssertNoExceptionThrown(() => connection.CommitTransaction());
				controller.Verify(c => c.ReportNudgeFailed(null, exception, 0));
				AssertEquals("callback", true, callbackWasCalled);
			}
		}
	}

	class NudgeClientStub : INudgeClient
	{
		public void Dispose()
		{
		}

		void INudgeClient.ScheduleTasks(IEnumerable<string> taskCodes, bool? echoes, TimeSpan? delay)
		{
		}

		event EventHandler<NudgeEventArgs> INudgeClient.Nudged { add { } remove { } }
		event EventHandler<NudgeFailedEventArgs> INudgeClient.NudgeFailed { add { } remove { } }
		event EventHandler<ErrorEventArgs> INudgeClient.Error { add { } remove { } }
	}
	class DummyBizoThatPretendsToAccessDB : DummyBusinessObject
	{
		public DummyBizoThatPretendsToAccessDB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool Z0_PropertyThatAccessesDB
		{
			get
			{
				Factory.ThreadSentry.EnsureCurrentThreadIsOwner();
				return false;
			}
		}
	}
}
