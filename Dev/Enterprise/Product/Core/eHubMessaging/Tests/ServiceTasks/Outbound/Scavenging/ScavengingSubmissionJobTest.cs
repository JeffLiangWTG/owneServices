using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using SEB = CargoWise.Data.Testing.SqlExceptionBuilder;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	[TestsSubclassesOf(typeof(ScavengingSubmissionJob<,>))]
	abstract class ScavengingSubmissionJobTest<TScavengingSubmissionJob, TOuboundItem, TLightweightOutboundItem> : OutboundServiceTaskJobTest<TScavengingSubmissionJob, TOuboundItem, TLightweightOutboundItem>
		where TOuboundItem : BusinessObject
		where TLightweightOutboundItem : LightweightOutboundItem<TOuboundItem>
		where TScavengingSubmissionJob : ScavengingSubmissionJob<TOuboundItem, TLightweightOutboundItem>
	{
		protected override bool JobExecutesForMultipleCompanies()
		{
			return false;
		}

		public virtual void TesteHubMessage()
		{
			var job = CreateMockJob_Moq();
			job.Object.CurrentCompany = GlbCompany.CurrentCompany;
			var message = (eHubMessage)CallMethod(job.Object, "CreateMessage", null, null);
			AssertEquals("SCV", message.ApplicationCode);
			AssertEquals(ExpectedSchemaName, message.SchemaName);
		}

		public void TestScavengingRunsForOneCompanyOnly()
		{
			var companies = (IEnumerable<GlbCompany>)GetPropertyValue(CreateMockJob_Moq().Object, "Companies");
			AssertEquals("One company only", 1, companies.Count());
		}

		public void TestScavengingItemDeletedAfterSending()
		{
			var item = SetupForExecuteTest();

			var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
			mockOutbox
				.Setup(x => x.AddMessage(It.IsAny<IeHubMessage>()))
				.Callback(new Action<IeHubMessage>(m => AssertEHubMessageForItem(m, item))).Verifiable();
			mockOutbox.Setup(m => m.Count).Returns(1).Verifiable();
			mockOutbox.Setup(m => m.Clear()).Verifiable();
			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object).Verifiable();
			mockAdapter.Setup(m => m.SendMessages())
				.Callback(() => {
					mockOutbox.Setup(x => x.Count).Throws(new Exception("Count called after messages sent"));
				}).Verifiable();
			mockAdapter.Setup(m => m.Dispose()).Verifiable();
			var mockJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockJob.Object.Execute(CancellationToken.None);
			Assert("Next execute iteration scheduled", mockJob.Object.NextExecuteIterationIsScheduled);
			AssertScavengingItemWasDeleted(item.PK, true);

			mockOutbox.Verify();
			mockAdapter.Verify();
			mockJob.VerifyAll();
			mockJob.Object.Notifier.AssertNotificationExists("1 message(s) sent.");
		}

		public void TestExecuteUnknownException()
		{
			var item = SetupForExecuteTest();

			var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
			mockOutbox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()));
			mockOutbox.Setup(m => m.Count).Returns(1);
			mockOutbox.Setup(m => m.Clear());
			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object);
			mockAdapter.Setup(m => m.SendMessages()).Throws(new Exception("some exception"));
			mockAdapter.Setup(m => m.Dispose());
			var mockJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			var notificationBuffer = (NotificationBuffer)mockJob.Object.Notifier;
			AssertExceptionThrown<Exception>(() => mockJob.Object.Execute(CancellationToken.None));
			Assert(!notificationBuffer.HasErrors);
			AssertScavengingItemWasDeleted(item.PK, false);

			mockOutbox.VerifyAll();
			mockAdapter.VerifyAll();
			mockJob.VerifyAll();
		}

		public void TestExecuteSqlException()
		{
			var sqlException = SEB.CreateSqlException(SEB.CreateSqlErrorCollection(SEB.CreateSqlError(-2, 0, 0, "", "", "", 0)));
			AssertExceptionThrown<SqlException>(() => ExecuteWithDbConnectionException(sqlException));
		}

		public void TestExecuteSqlExceptionWrapped()
		{
			var sqlException = SEB.CreateSqlException(SEB.CreateSqlErrorCollection(SEB.CreateSqlError(-2, 0, 0, "", "", "", 0)));
			var actualException = new NativeXMLUserVisibleException(string.Format("Could not load EntitySet with Entity Set Name: Organization - {0}", sqlException.Message), sqlException);
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => ExecuteWithDbConnectionException(actualException));
		}

		void ExecuteWithDbConnectionException(Exception e)
		{
			var mockJob = CreateMockJob_Moq();
			mockJob.Setup(m => m.DbConnection).Throws(e);
			mockJob.Object.Execute(CancellationToken.None);
		}

		protected override void TestExecuteLockCore(DbConnection extraConnection)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });

			var serviceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			serviceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);

			var serviceTaskJob2 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			serviceTaskJob2.Setup(m => m.DbConnection).Returns(extraConnection);

			// No need to lock Scanvenging job on company as the service task does not support multiple instances.
			serviceTaskJob2.Setup(x => x.GetPendingItems())
				.Returns(new Func<IReadOnlyCollection<TLightweightOutboundItem>>(() =>
			{
				AssertEquals(company, serviceTaskJob2.Object.CurrentCompany);
				return Array.Empty<TLightweightOutboundItem>();
			}));

			serviceTaskJob1.Setup(x => x.GetPendingItems())
				.Returns(new Func<IReadOnlyCollection<TLightweightOutboundItem>>(() =>
			{
				AssertEquals(company, serviceTaskJob1.Object.CurrentCompany);
				serviceTaskJob2.Object.Execute(CancellationToken.None);
				return Array.Empty<TLightweightOutboundItem>();
			}));

			serviceTaskJob1.Object.Execute(CancellationToken.None);

			serviceTaskJob1.Verify();
			serviceTaskJob2.Verify();
		}

		protected override void TestExecuteLockLostCore(DbConnection extraConnection)
		{
			var mockServiceTaskJob = CreateMockJob_Moq();
			mockServiceTaskJob.Setup(m => m.DbConnection).Returns(extraConnection);
			mockServiceTaskJob.Setup(x => x.ProcessMessagesCore())
				.Callback(() =>
				{
					extraConnection.CloseConnection();
					new BusinessObjectFactory(extraConnection).LoadTop1<DummyBusinessObject>(new ZQuery());
				});

			AssertNoExceptionThrown(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			mockServiceTaskJob.Object.Notifier.AssertNotificationDoesNotExists("Exclusive lock for company was lost.");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		protected override eHubMessaging.ServiceTasks.AdapterType ServiceAdapterType => eHubMessaging.ServiceTasks.AdapterType.GatewayAdapter;

		protected abstract string ExpectedSchemaName { get; }

		protected abstract string ScavengingItemsTableName { get; }

		protected abstract string ScavengingItemsPKName { get; }

		protected abstract TLightweightOutboundItem SetupForExecuteTest();

		protected abstract void AssertEHubMessageForItem(IeHubMessage message, TLightweightOutboundItem item);

		protected void AssertScavengingItemWasDeleted(ZGuid pk, bool deleted)
		{
			var query = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} = '{2}'", ScavengingItemsTableName, ScavengingItemsPKName, pk);
			using (var command = Db.Connection.Command(query))
			{
				var count = command.ExecuteScalar();
				if (deleted)
				{
					AssertEquals("Scavenging item was deleted", 0, count);
				}
				else
				{
					AssertNotEquals("Scavenging item was not deleted", 0, count);
				}
			}
		}

		protected static object CallMethod(TScavengingSubmissionJob job, string methodName, params object[] parameters)
		{
			return GetMethod(typeof(TScavengingSubmissionJob), methodName).Invoke(job, parameters);
		}

		static MethodInfo GetMethod(Type type, string methodName)
		{
			while (type != null)
			{
				var method = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(m => m.Name == methodName);
				if (method != null)
				{
					return method;
				}

				type = type.BaseType;
			}
			throw new Exception("Method " + methodName + " not found");
		}
	}
}
