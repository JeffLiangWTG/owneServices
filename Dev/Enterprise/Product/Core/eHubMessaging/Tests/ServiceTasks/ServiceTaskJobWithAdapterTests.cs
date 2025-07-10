using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	[TestsSubclassesOf(typeof(ServiceTaskJobWithAdapter))]
	abstract class ServiceTaskJobWithAdapterTests<TServiceTaskJobWithAdapter> : MessageProcessingJobTests<TServiceTaskJobWithAdapter>
		where TServiceTaskJobWithAdapter : ServiceTaskJobWithAdapter
	{
		protected abstract void TestExecuteLockCore(DbConnection extraConnection);

		public static GlbCompany CreateCompanyWithBranch(BusinessObjectFactory factory)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			factory.Save();
			return company;
		}

		protected GlbCompany CreateCompanyWithBranch() => CreateCompanyWithBranch(Factory);
		protected GlbCompany CreateCompanyWithBranch(string namePostFix) => CreateCompanyWithBranch(namePostFix, Factory);

		protected GlbCompany CreateCompanyWithBranch(string namePostFix, BusinessObjectFactory factory)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = "C" + namePostFix;
			company.GC_Name = "COMP " + namePostFix;
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B" + namePostFix;
			branch.GB_BranchName = "BRANCH " + namePostFix;
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			return company;
		}

		protected virtual void TestExecuteLockLostCore(DbConnection extraConnection)
		{
			var mockServiceTaskJob = CreateMockJob_Moq();
			mockServiceTaskJob.Setup(x => x.DbConnection).Returns(extraConnection);
			mockServiceTaskJob.Setup(x => x.ProcessMessagesCore())
				.Callback(() =>
				{
					extraConnection.CloseConnection();
					new BusinessObjectFactory(extraConnection).LoadTop1<DummyBusinessObject>(new ZQuery());
				});

			AssertNoExceptionThrown(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			mockServiceTaskJob.Object.Notifier.AssertNotificationContains("Exclusive lock for company was lost.");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestExecute()
		{
			var company = CreateCompanyWithBranch();
			var companySettings = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettings();
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company }, new[] { companySettings });
			var mockAdapterFactory = new Mock<IAdaptorFactory>();
			var serviceTaskJob = CreateMockJob_Moq(mockAdapterFactory.Object, settingsManager.Object, company);
			mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()))
				.Returns(new EHubAdapterMock())
				.Callback(() =>
					{
						AssertEquals(company, serviceTaskJob.Object.CurrentCompany);
						AssertEquals(companySettings, serviceTaskJob.Object.CurrentCompanySettings);
					});
			serviceTaskJob.Object.Execute(CancellationToken.None);
			serviceTaskJob.Verify();
		}

		[UseSnapshotProtection]
		public void TestExecuteLock()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				TestExecuteLockCore(extraConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestExecuteLockLost()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				TestExecuteLockLostCore(extraConnection);
			}
		}

		public void TestSkipDeveloperCompany()
		{
			ObjectFactory.Get<IProductRegistration>().ResetKeyToDefault();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "EDI";
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.ServerCodeForTest = "DAT";

			var mockAdapterFactory = new Mock<IAdaptorFactory>();
			var serviceTaskJob = CreateMockJob_Moq(mockAdapterFactory.Object);
			if (ShouldSkipDeveloperCompany)
			{
				mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()));
			}
			else
			{
				mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Returns(new EHubAdapterMock());
			}

			serviceTaskJob.Object.Execute(CancellationToken.None);
			var skipNotification = string.Format("Skipping company '{0}' because it has a test license. To connect to eHub you must have a license that exists in ediProd.", GlbCompany.CurrentCompany.GC_Code);
			if (ShouldSkipDeveloperCompany)
			{
				serviceTaskJob.Object.Notifier.AssertNotificationExists(skipNotification);
				mockAdapterFactory.Verify(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()), Times.Never());
			}
			else
			{
				serviceTaskJob.Object.Notifier.AssertNotificationDoesNotExists(skipNotification);
				mockAdapterFactory.Verify(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()));
			}
		}

		public void TestExecute_CachedSettingNotFound()
		{
			var company = CreateCompanyWithBranch();
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company }, new ICompanySettings[] { null });
			var mockAdapterFactory = new Mock<IAdaptorFactory>();
			mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()));//.Repeat.Never();
			var serviceTaskJob = CreateMockJob_Moq(mockAdapterFactory.Object, settingsManager.Object, company);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("Warning: Messaging settings not found for {0}.", company.GC_Code));
			serviceTaskJob.Verify();
		}

		public void TestLoginErrorTreatedAsACompanyLevel()
		{
			var company1 = CreateCompanyWithBranch();
			var company2 = CreateCompanyWithBranch();
			var companySettings1 = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettings();
			var companySettings2 = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettings();
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company1, company2 }, new[] { companySettings1, companySettings2 });
			var notifications = new NotificationBuffer();
			var mockAdapterFactory = new Mock<IAdaptorFactory>();
			var serviceTaskJob = CreateMockJob_Moq(mockAdapterFactory.Object, settingsManager.Object, company1, notifications);
			serviceTaskJob.Setup(m => m.MutexPrefix).Returns("TEST");

			mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Throws(new MessageSecurityException("An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.", new FaultException("An error occurred when verifying security for the message.")));
			if (JobExecutesForMultipleCompanies())
			{
				mockAdapterFactory.SetupSequence(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()))
					.Throws(new MessageSecurityException("An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.", new FaultException("An error occurred when verifying security for the message.")))
					.Returns(new EHubAdapterMock());
			}

			serviceTaskJob.Object.Execute(CancellationToken.None);

			serviceTaskJob.Object.Notifier.AssertNotificationContains(@"An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.
System.ServiceModel.Security.MessageSecurityException: An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.");
			serviceTaskJob.Verify();
		}

		protected virtual bool JobExecutesForMultipleCompanies()
		{
			return true;
		}

		public void TestMessageSecurityException()
		{
			var exception = new MessageSecurityException("An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.", new FaultException("An error occurred when verifying security for the message."));

			var serviceTaskJob = CreateMockJobWithAdapterException(exception);
			AssertNoExceptionThrown(() => serviceTaskJob.Object.Execute(CancellationToken.None));
			serviceTaskJob.Object.Notifier.AssertNotificationContains(exception.StackTrace);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestRegistrationException_ThrottleMessage()
		{
			var exception = new RegistrationException("The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid CargoWise One licence code.");

			var serviceTaskJob = CreateMockJobWithAdapterException(exception);
			AssertNoExceptionThrown(() => serviceTaskJob.Object.Execute(CancellationToken.None));
			serviceTaskJob.Object.Notifier.AssertNotificationExists("Warning: The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. ");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestRegistrationException_GenericMessage()
		{
			var exception = new RegistrationException("I don't know what a proper registration exception message looks like.");

			var serviceTaskJob = CreateMockJobWithAdapterException(exception);
			AssertNoExceptionThrown(() => serviceTaskJob.Object.Execute(CancellationToken.None));
			serviceTaskJob.Object.Notifier.AssertNotificationExists(exception.Message + System.Environment.NewLine + exception);
			AssertEquals(exception.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCreateAdapterNullAdapterException()
		{
			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(null));
			AssertExceptionThrown<CreateAdapterException>(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Create adapter(https://www.test.com): FAIL");
		}

		public void TestCreateAdapterTypeInitializationException()
		{
			var exception = new TypeInitializationException("The type initializer for 'System.Net.ServicePointManager' threw an exception.", new TypeInitializationException("The type initializer for 'System.Net.ComNetOS' threw an exception.", new NullReferenceException()));
			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockThatThrowsOnCreate(exception));
			var exceptionThrown = AssertExceptionThrown<CreateAdapterException>(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			AssertEquals("Environmental Error", exceptionThrown.Message);
			AssertEquals(exception, exceptionThrown.InnerException);
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Create adapter(https://www.test.com): FAIL");
		}

		public void TestCreateAdapterExceptionRethrown()
		{
			TestCreateAdapterExceptionRethrown(new ConfigurationErrorsException(
				"Error creating the Web Proxy specified in the 'system.net/defaultProxy' configuration section.",
				new TypeInitializationException("The type initializer for 'System.Net.HybridWebProxyFinder' threw an exception.", new NullReferenceException())));

			TestCreateAdapterExceptionRethrown(new UriFormatException());

			TestCreateAdapterExceptionRethrown(new FileLoadException("", new IOException("", -2147023441)));
			TestCreateAdapterExceptionRethrown(new FileLoadException());

			TestCreateAdapterExceptionRethrown(new DllNotFoundException("", new IOException("", -2147023441)));
			TestCreateAdapterExceptionRethrown(new DllNotFoundException());
		}

		public void TesteHubAdapterExceptionRethrown()
		{
			var eHubAdapterException = new eHubAdapterException("some exception");
			var serviceTaskJob = CreateMockJobWithAdapterException(eHubAdapterException);
			var exceptionThrown = AssertExceptionThrown<eHubAdapterException>(() => serviceTaskJob.Object.Execute(CancellationToken.None));
			AssertEquals(eHubAdapterException, exceptionThrown);
		}

		public void TestJobNotScheduledAfterCompanyLevelException()
		{
			var exception = new RegistrationException("The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid CargoWise One licence code.");

			var serviceTaskJob = CreateMockJobWithAdapterException(exception);
			serviceTaskJob.Object.OnAtLeastOneMessageProcessed();
			AssertNoExceptionThrown(() => serviceTaskJob.Object.Execute(CancellationToken.None));
			serviceTaskJob.Object.Notifier.AssertNotificationExists("Warning: The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. ");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Assert("The job should not be rescheduled as there was an exception", !serviceTaskJob.Object.NextExecuteIterationIsScheduled);
		}

		void TestCreateAdapterExceptionRethrown<TException>(TException exception)
			where TException : Exception
		{
			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockThatThrowsOnCreate(exception));

			var exceptionThrown = AssertExceptionThrown<TException>(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			AssertEquals(exception, exceptionThrown);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			mockServiceTaskJob.Object.Notifier.AssertNotificationDoesNotExists("Create adapter(https://www.test.com): FAIL");
		}

		protected abstract eHubMessaging.ServiceTasks.AdapterType ServiceAdapterType { get; }

		protected override Mock<TServiceTaskJobWithAdapter> CreateMockJob_Moq(INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			return CreateMockJob_Moq(new AdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock()), notifications, mockInterchangeCandidates);
		}

		protected Mock<TServiceTaskJobWithAdapter> CreateMockJob_Moq(IAdaptorFactory adaptorFactory, INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var actualCompanySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { CurrentCompany });
			return CreateMockJob_Moq(adaptorFactory, actualCompanySettingsManager.Object, CurrentCompany, notifications, mockInterchangeCandidates);
		}

		protected virtual Mock<TServiceTaskJobWithAdapter> CreateMockJob_Moq(IAdaptorFactory adaptorFactory, ICompanySettingsManager settingsManager, GlbCompany company, INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var actualNotifications = notifications ?? new NotificationBuffer();
			var mockServiceTaskJob = new Mock<TServiceTaskJobWithAdapter>(CreateMockServiceTaskSupport(ServiceTaskName, settingsManager), actualNotifications, adaptorFactory);
			mockServiceTaskJob.CallBase = true;
			AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			return mockServiceTaskJob;
		}

		protected virtual Mock<TServiceTaskJobWithAdapter> CreateMockJobWithAdapterException(Exception e)
		{
			var mockAdapterFactory = new Mock<IAdaptorFactory>();
			mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Throws(e);
			var mockServiceTaskJob = CreateMockJob_Moq(mockAdapterFactory.Object);
			return mockServiceTaskJob;
		}

		protected void ExecuteJobWithServiceTaskContext(TServiceTaskJobWithAdapter job, string taskCode)
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext(taskCode, canRunInAnyBranch: true))
			{
				job.Execute(CancellationToken.None);
			}
		}

		protected GlbCompany CurrentCompany => currentCompany ?? (currentCompany = GlbCompany.GetCurrentCompany(Factory));

		GlbCompany currentCompany;
	}
}
