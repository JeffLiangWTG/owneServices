using System;
using System.Reflection;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	[TestsSubclassesOf(typeof(MessageProcessingJob))]
	abstract class MessageProcessingJobTests<TServiceTaskJob> : TestCaseWithFactory
		where TServiceTaskJob : MessageProcessingJob
	{
		public void TestSqlException()
		{
			var error = SqlExceptionBuilder.CreateSqlError(983, 1, 1, "", "Unable to access database because its replica role is RESOLVING.", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var sqlException = SqlExceptionBuilder.CreateSqlException(errorCollection);

			var e = AssertExceptionThrown<SqlException>(() => ExecuteWithException(sqlException));
			AssertEquals(sqlException, e);
		}

		public void TestSqlExceptionWrapped()
		{
			var error = SqlExceptionBuilder.CreateSqlError(983, 1, 1, "", "Unable to access database because its replica role is RESOLVING.", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var sqlException = SqlExceptionBuilder.CreateSqlException(errorCollection);
			var actualException = new NativeXMLUserVisibleException(string.Format("Could not load EntitySet with Entity Set Name: Organization - {0}", sqlException.Message), sqlException);

			var e = AssertExceptionThrown<NativeXMLUserVisibleException>(() => ExecuteWithException(actualException));
			AssertEquals(actualException, e);
		}

		public void TestInvalidOperationException()
		{
			var exception = new InvalidOperationException();

			var exceptionThrown = AssertExceptionThrown<InvalidOperationException>(() => ExecuteWithException(exception));
			AssertEquals(exception, exceptionThrown);
		}

		public void TestGeneralException()
		{
			var exception = new Exception("some exception");

			var exceptionThrown = AssertExceptionThrown<Exception>(() => ExecuteWithException(exception));
			AssertEquals(exception, exceptionThrown);
		}

		public void TestStop()
		{
			var cts = new CancellationTokenSource();
			var mockServiceTaskJob = CreateMockJob_Moq();
			mockServiceTaskJob.Setup(x => x.ProcessMessagesCore())
				.Callback(() =>
				{
					cts.Cancel();
				}).CallBase();
			AssertExceptionThrown<OperationCanceledException>(() => mockServiceTaskJob.Object.Execute(cts.Token));
		}

		protected Mock<TServiceTaskJob> ExecuteWithException(Exception e)
		{
			var mockServiceTaskJob = CreateMockJob_Moq();
			mockServiceTaskJob.Setup(x => x.ProcessMessagesCore()).Throws(e);
			mockServiceTaskJob.Object.Execute(CancellationToken.None);
			return mockServiceTaskJob;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "NUL"; // to avoid IsDeveloperSystem() check
		}

		protected virtual void AdditionalServiceTaskJobSetup_Moq(Mock<TServiceTaskJob> mockServiceTaskJob, GlbCompany company, bool mockInterchangeCandidates = true) { }

		protected virtual bool ShouldSkipDeveloperCompany
		{
			get { return true; }
		}

		protected virtual Mock<TServiceTaskJob> CreateMockJob_Moq(INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var company = GlbCompany.GetCurrentCompany(new BusinessObjectFactory());
			return CreateMockJob_Moq(eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company }).Object, company, notifications, mockInterchangeCandidates);
		}

		protected virtual Mock<TServiceTaskJob> CreateMockJob_Moq(ICompanySettingsManager companySettingsManager, GlbCompany company, INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var actualCompanySettingsManager = companySettingsManager ?? eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company }).Object;
			var mockServiceTaskJob = new Mock<TServiceTaskJob>(CreateMockServiceTaskSupport(ServiceTaskName, actualCompanySettingsManager), notifications ?? new NotificationBuffer());
			mockServiceTaskJob.CallBase = true;
			AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			return mockServiceTaskJob;
		}

		protected IeHubServiceTaskSupport CreateMockServiceTaskSupport(string serviceTaskName, ICompanySettingsManager settingsManager)
		{
			var mockConfig = new Mock<IeHubServiceTaskSupport>(MockBehavior.Strict);
			mockConfig.Setup(m => m.ServiceTaskName).Returns(serviceTaskName);
			mockConfig.Setup(m => m.DefaultServerAddress).Returns("https://www.test.com");
			mockConfig.Setup(m => m.CompanySettingsManager).Returns(settingsManager);
			return mockConfig.Object;
		}

		protected void AssertCanExecute(TServiceTaskJob job, bool value)
		{
			AssertEquals("CanExecute", value, GetPropertyValue(job, "CanExecute"));
		}

		protected static object GetPropertyValue(TServiceTaskJob job, string propertyName)
		{
			return typeof(TServiceTaskJob).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(job, null);
		}

		protected const string ServiceTaskName = "MOCK SERVICE TASK";
	}
}
