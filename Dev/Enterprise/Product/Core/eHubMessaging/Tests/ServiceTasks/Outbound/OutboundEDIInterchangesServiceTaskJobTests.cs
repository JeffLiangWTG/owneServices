using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound
{
	[TestsSubclassesOf(typeof(OutboundEDIInterchangesServiceTaskJob))]
	abstract class OutboundEDIInterchangesServiceTaskJobTests<TOutboundEDIInterchangesServiceTaskJob> : OutboundServiceTaskJobTest<TOutboundEDIInterchangesServiceTaskJob, EDIInterchange, LightweightOutboundInterchangeCandidate>
		where TOutboundEDIInterchangesServiceTaskJob : OutboundEDIInterchangesServiceTaskJob
	{
		protected override void TestExecuteLockCore(DbConnection extraConnection)
		{
			var company = CreateCompanyWithBranch();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });
			var serviceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			var mockServiceTaskJob1 = serviceTaskJob1;
			mockServiceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);
			var interchangeCandidates1 = new List<LightweightOutboundInterchangeCandidate>
				{
					new LightweightOutboundInterchangeCandidate(CreateTestInterchange(Factory))
				};
			var branchRecipientPairs1 = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object> { [EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK, [EDIInterchangeSchema.Constants.EI_To] = "RECIPIEN1", },
				};

			mockServiceTaskJob1.Setup(m => m.GetPendingItems(It.IsAny<int>())).Returns(interchangeCandidates1);
			mockServiceTaskJob1.Setup(m => m.GetBatchingCriteriaForTopPendingInterchanges()).Returns(branchRecipientPairs1);

			var serviceTaskJob2 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			var mockServiceTaskJob2 = serviceTaskJob2;
			mockServiceTaskJob2.Setup(m => m.DbConnection).Returns(extraConnection);
			var interchangeCandidates2 = new List<LightweightOutboundInterchangeCandidate>
				{
					new LightweightOutboundInterchangeCandidate(CreateTestInterchange(Factory)),
					new LightweightOutboundInterchangeCandidate(CreateTestInterchange(Factory)),
				};
			var branchRecipientPairs2 = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object> { [EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK, [EDIInterchangeSchema.Constants.EI_To] = "RECIPIEN1", },
					new Dictionary<string, object> { [EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK, [EDIInterchangeSchema.Constants.EI_To] = "RECIPIEN2", },
				};
			mockServiceTaskJob2.Setup(m => m.GetPendingItems(It.IsAny<int>())).Returns(interchangeCandidates2);
			mockServiceTaskJob2.Setup(m => m.GetBatchingCriteriaForTopPendingInterchanges()).Returns(branchRecipientPairs2);
			mockServiceTaskJob2.Setup(x => x.SendBatch(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>()))
				.Callback<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>(call =>
				{
					AssertEquals(company, serviceTaskJob2.Object.CurrentCompany);
					AssertEquals("RECIPIEN2", serviceTaskJob2.Object.CurrentRecipient);
				});

			mockServiceTaskJob1.Setup(x => x.SendBatch(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>()))
				.Callback<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>(call =>
				{
					AssertEquals(company, serviceTaskJob1.Object.CurrentCompany);
					AssertEquals("RECIPIEN1", serviceTaskJob1.Object.CurrentRecipient);
					serviceTaskJob2.Object.Execute(CancellationToken.None);
				});

			mockServiceTaskJob1.Object.Execute(CancellationToken.None);
		}

		public void TestCorrectBatchesAndLocksWithExtraColumns()
		{
			var company = CreateCompanyWithBranch();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });
			var mockServiceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			mockServiceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);

			var batchingCriteria = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>
					{
						[EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK,
						[EDIInterchangeSchema.Constants.EI_To] = "RECIPIEN1",
						[EDIInterchangeSchema.Constants.PK] = Guid.NewGuid(),
					},
					new Dictionary<string, object>
					{
						[EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK,
						[EDIInterchangeSchema.Constants.EI_To] = "RECIPIEN1",
						[EDIInterchangeSchema.Constants.PK] = Guid.NewGuid(),
					},
				};

			var batchIndex = 0;

			mockServiceTaskJob1.SetupGet(m => m.ExtraColumnsToBatchPendingInterchangesBy).Returns(new[] { EDIInterchangeSchema.PK, });
			mockServiceTaskJob1.Setup(m => m.GetBatchingCriteriaForTopPendingInterchanges()).Returns(batchingCriteria);
			mockServiceTaskJob1.Setup(m => m.GetPendingItems(It.IsAny<int>()))
				.Returns(new List<LightweightOutboundInterchangeCandidate>())
				.Callback(() =>
				{
					var job = mockServiceTaskJob1.Object;

					AssertEquals(company.FirstActiveBranch.PK, job.CurrentBranchPk);
					AssertEquals("RECIPIEN1", job.CurrentRecipient);
					AssertEquals($"{job.MutexPrefix}{job.CurrentBranchPk}_{job.CurrentRecipient}_{batchingCriteria[batchIndex][EDIInterchangeSchema.Constants.PK]}", job.CurrentBatchLock.Key);
					AssertEquals(batchingCriteria[batchIndex++], job.CurrentBatchCriteria);
				});

			mockServiceTaskJob1.Object.Execute(CancellationToken.None);

			AssertEquals(2, batchIndex);
		}

		public void TestCorrectBatchingCriteriaQueryWithExtraColumns()
		{
			var company = CreateCompanyWithBranch();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });
			var mockServiceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			mockServiceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);
			mockServiceTaskJob1.SetupGet(m => m.ExtraColumnsToBatchPendingInterchangesBy).Returns(new[] { EDIInterchangeSchema.EI_InterchangeNum, });

			var job = mockServiceTaskJob1.Object;

			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI001";
			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI002";
			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI003";

			Factory.Save();

			var result = job.GetBatchingCriteriaForTopPendingInterchanges().ToList();

			AssertEquals(3, result.Count);

			Assert("All batching criteria should Contain EI_GB, EI_TO, EI_InterchangeNum", result.All(i => i.ContainsKey(EDIInterchangeSchema.Constants.EI_GB) && i.ContainsKey(EDIInterchangeSchema.Constants.EI_To) && i.ContainsKey(EDIInterchangeSchema.Constants.EI_InterchangeNum)));

			Assert("One batching criteria should have EI_InterchangeNum value of 'EI001'", result.Any(i => i[EDIInterchangeSchema.Constants.EI_InterchangeNum]?.ToString() == "EI001"));
			Assert("One batching criteria should have EI_InterchangeNum value of 'EI002'", result.Any(i => i[EDIInterchangeSchema.Constants.EI_InterchangeNum]?.ToString() == "EI002"));
			Assert("One batching criteria should have EI_InterchangeNum value of 'EI003'", result.Any(i => i[EDIInterchangeSchema.Constants.EI_InterchangeNum]?.ToString() == "EI003"));
		}

		public void TestCorrectPendingItemsQueryWithExtraColumns()
		{
			var company = CreateCompanyWithBranch();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });
			var mockServiceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company, mockInterchangeCandidates: false);
			mockServiceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);
			mockServiceTaskJob1.SetupGet(m => m.ExtraColumnsToBatchPendingInterchangesBy).Returns(new[] { EDIInterchangeSchema.EI_InterchangeNum, });
			mockServiceTaskJob1.Setup(m => m.SendBatch(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>()));

			int index = 0;
			mockServiceTaskJob1.Setup(m => m.GetPendingItems(It.IsAny<int>())).CallBase().Callback((IInvocation invocation) =>
			{
				var pendingItems = (IReadOnlyCollection<LightweightOutboundInterchangeCandidate>)invocation.ReturnValue;
				AssertEquals(1, pendingItems.Count);
				AssertEquals($"EI00{++index}", pendingItems.First().FullItem.EI_InterchangeNum);
			});

			var job = mockServiceTaskJob1.Object;

			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI001";
			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI002";
			CreateTestInterchange(Factory, job.InterchangeQueuedStatus, company, "RECIPIEN1").EI_InterchangeNum = "EI003";

			Factory.Save();

			job.Execute(CancellationToken.None);

			AssertEquals(3, index);
		}

		protected override void TestExecuteLockLostCore(DbConnection extraConnection)
		{
			var mockServiceTaskJob = CreateMockJob_Moq();
			mockServiceTaskJob.Setup(x => x.DbConnection).Returns(extraConnection);
			mockServiceTaskJob.Setup(x => x.SendBatch(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>()))
				.Callback((IReadOnlyCollection<LightweightOutboundInterchangeCandidate> items) =>
				{
					extraConnection.CloseConnection();
				}).CallBase();

			AssertNoExceptionThrown(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			mockServiceTaskJob.Object.Notifier.AssertNotificationContains("Exclusive lock for company was lost.");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		[UseSnapshotProtection]
		public void TestExecuteDoesNotRequestAnotherIterationWhenNothingProcessed()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var company = CreateCompanyWithBranch();
				var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company });
				var mockAdapterFactory1 = new Mock<IAdaptorFactory>();
				var serviceTaskJob1 = CreateMockJob_Moq(mockAdapterFactory1.Object, companySettingsManager.Object, company);
				serviceTaskJob1.Setup(m => m.DbConnection).Returns(Db.Connection);

				var mockAdapterFactory2 = new Mock<IAdaptorFactory>();
				var serviceTaskJob2 = CreateMockJob_Moq(mockAdapterFactory2.Object, companySettingsManager.Object, company);
				serviceTaskJob2.Setup(m => m.DbConnection).Returns(extraConnection);

				mockAdapterFactory1.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Returns(new EHubAdapterMock())
					.Callback<string, string, INotifications, string>((licenceCode, password, notifier, serverAddress) =>
					{
						AssertEquals(company, serviceTaskJob1.Object.CurrentCompany);
						serviceTaskJob2.Object.Execute(CancellationToken.None);
						AssertEquals("Second run should not have processed any data so it should not request another run", false, serviceTaskJob2.Object.NextExecuteIterationIsScheduled);
					});

				serviceTaskJob1.Object.Execute(CancellationToken.None);
				AssertEquals("First run should have processed data and requested another run", true, serviceTaskJob1.Object.NextExecuteIterationIsScheduled);

				serviceTaskJob1.Verify();
				serviceTaskJob2.Verify();

				mockAdapterFactory2.Verify(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>()), Times.Never());
			}
		}

		public void TestNoDBQueriesForCompaniesWithNoMessages()
		{
			var adapterFactory = new OutboundAdaptorFactoryMock();
			var job = CreateMockJob_Moq(adapterFactory, new eAdaptorMessagingCompanySettingsManager(), company: null, mockInterchangeCandidates: false);

			var nTotalCompanies = 20;
			var companies = new List<GlbCompany>();
			for (int i = 0; i < nTotalCompanies; i++)
			{
				companies.Add(CreateCompanyWithBranch(i.ToString()));
			}

			var nCompaniesWithMessages = nTotalCompanies - 15;
			var nRecipientsPerCompany = 4;
			for (int i = 0; i < nCompaniesWithMessages; i++)
			{
				for (int j = 0; j < nRecipientsPerCompany; j++)
				{
					var interchange = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), companies[i], string.Format("RC {0} {1}", i.ToString(), j.ToString()));
					SetInterchangeProperties(interchange, companies[i].Branches[0].PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
				}
			}

			Factory.Save();
			var cmdCountBefore = Db.Connection.ExecutedCommandCount;
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Wrong number of Adapters created", nCompaniesWithMessages * nRecipientsPerCompany, adapterFactory.Adapters.Length);
			AssertLessThan("Too many DB Hits used when sending messages", Db.Connection.ExecutedCommandCount, cmdCountBefore + 255);
		}

		public void TestInterchangesWithInvalidCompaniesAreFailed()
		{
			var adapterFactory = new OutboundAdaptorFactoryMock();
			var company1 = CreateCompanyWithBranch();
			var company2 = CreateCompanyWithBranch();
			var companySettings2 = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettings();
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company1, company2 }, new[] { null, companySettings2 });
			var expiryTime = TimeSpan.FromSeconds(5);
			var job = CreateMockJob_Moq(adapterFactory, settingsManager.Object, null, mockInterchangeCandidates: false);
			var pendingItemsBatchSize = 5;
			job.Setup(m => m.PendingItemsBatchSize).Returns(pendingItemsBatchSize);
			job.Setup(m => m.PendingItemsSearchLimit).Returns(5);

			var currentTime = DateTime.UtcNow;
			var tenSecondsAgo = currentTime.AddSeconds(-10);
			var interchangesThatShouldFail = new EDIInterchange[pendingItemsBatchSize];
			for (int i = 0; i < pendingItemsBatchSize; i++)
			{
				interchangesThatShouldFail[i] = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, string.Format("RC {0}", i.ToString()));
				SetInterchangeProperties(interchangesThatShouldFail[i], company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
				interchangesThatShouldFail[i].EI_SystemCreateTimeUtc = tenSecondsAgo;
			}

			var interchangeThatShouldSucceed = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company2, "RC");
			interchangeThatShouldSucceed.EI_SystemCreateTimeUtc = currentTime;
			SetInterchangeProperties(interchangeThatShouldSucceed, company2.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());

			Factory.Save();
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after first execute", 0, adapterFactory.Adapters.Length);
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after second execute", 1, adapterFactory.Adapters.Length);
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute no more", false, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after third execute", 1, adapterFactory.Adapters.Length);

			Factory.ReloadAll<EDIInterchange>();
			foreach (var interchange in interchangesThatShouldFail)
			{
				AssertEquals("Interchange should have failed", EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			}
			AssertEquals("Interchange should have succeeded", job.Object.InterchangeSuccessStatus, interchangeThatShouldSucceed.EI_Status);
		}

		public void TestInterchangeWithCompanyNotFoundIsFailed()
		{
			var adapterFactory = new OutboundAdaptorFactoryMock();
			var company1 = CreateCompanyWithBranch();
			var company2 = CreateCompanyWithBranch();
			var companySettings2 = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettings();
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company2 }, new[] { companySettings2 });
			settingsManager.Setup(m => m.ClearCache());

			var expiryTime = TimeSpan.FromSeconds(5);
			var job = CreateMockJob_Moq(adapterFactory, settingsManager.Object, null, mockInterchangeCandidates: false);

			var interchangeThatShouldFail = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC1");
			SetInterchangeProperties(interchangeThatShouldFail, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());

			Factory.Save();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("EHO", canRunInAnyBranch: true))
			{
				job.Object.Execute(CancellationToken.None);
			}
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after first execute", 0, adapterFactory.Adapters.Length);
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute no more", false, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after second execute", 0, adapterFactory.Adapters.Length);

			Factory.ReloadAll<EDIInterchange>();
			AssertEquals("Interchange should have failed", EDIInterchangeStatusList.Codes.Failed, interchangeThatShouldFail.EI_Status);
		}

		public void TestInterchangeOnNewCompanyAddedWhileServiceTaskIsRunningIsProcessedSuccessfully()
		{
			var adapterFactory = new OutboundAdaptorFactoryMock();
			var company1 = CreateCompanyWithBranch();
			var job = CreateMockJob_Moq(adapterFactory, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
			var interchange1 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC1");
			SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
			Factory.Save();

			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after first execute", 1, adapterFactory.Adapters.Length);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals("Interchange should have succeeded", job.Object.InterchangeSuccessStatus, interchange1.EI_Status);

			var company2 = CreateCompanyWithBranch();
			var interchange2 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company2, "RC2");
			SetInterchangeProperties(interchange2, company2.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
			Factory.Save();

			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after second execute", 2, adapterFactory.Adapters.Length);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals("Interchange should have succeeded", job.Object.InterchangeSuccessStatus, interchange2.EI_Status);
			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should not be scheduled again as no data was processed", false, job.Object.NextExecuteIterationIsScheduled);
		}

		public void TestTableIndexHintsForInterchanges()
		{
			var adapterFactory = new OutboundAdaptorFactoryMock();
			var company1 = CreateCompanyWithBranch();
			var job = CreateMockJob_Moq(adapterFactory, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
			var interchange1 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC1");
			SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
			Factory.Save();

			job.Object.Execute(CancellationToken.None);
			AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after first execute", 1, adapterFactory.Adapters.Length);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals("Interchange should have succeeded", job.Object.InterchangeSuccessStatus, interchange1.EI_Status);
			var queries = ((ZQueryFactoryForTest)job.Object.QueryFactory).Queries;
			AssertEquals("Wrong number of ZQuery created", 1, queries.Count());
			var queryTableIndexHints = queries.Single().TableIndexHints;
			AssertEquals("Wrong number of table index hints", 1, queryTableIndexHints.Count);
			AssertEquals("Wrong index hint", GetExpectedTableIndexHint(), queryTableIndexHints.Single().IndexName);
			var collections = ((DynamicBusinessObjectCollectionFactoryForTest)job.Object.DynamicBizoFactory).Collections;
			AssertEquals("Wrong number of collections created", 1, collections.Count());
			var rawQuery = collections.Single().RawQuery;
			Assert("Could not find the required table index", rawQuery.Contains($"WITH (INDEX={GetExpectedTableIndexHint()})"));
		}

		public void TestInterchangeSentWhenThereIsABacklogForAnotherRecipient()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var adapterFactory = new OutboundAdaptorFactoryMock();
				var company1 = CreateCompanyWithBranch();
				var job = CreateMockJob_Moq(adapterFactory, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
				job.Setup(m => m.PendingItemsBatchSize).Returns(1);
				var interchange1 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC1");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
				var interchange2 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC2");
				interchange2.EI_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc.AddSeconds(5);
				SetInterchangeProperties(interchange2, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
				Factory.Save();

				extraConnection.TryGetLock(job.Object.MutexPrefix + company1.FirstActiveBranch.PK + "_" + interchange1.EI_To, out var mutex);
				using (mutex)
				{
					job.Object.Execute(CancellationToken.None);
					AssertEquals("Job should execute another time", true, job.Object.NextExecuteIterationIsScheduled);
					AssertEquals("Wrong number of Adapters created after first execute", 1, adapterFactory.Adapters.Length);
					Factory.ReloadAll<EDIInterchange>();
					AssertEquals("Interchange2 should have succeeded", job.Object.InterchangeSuccessStatus, interchange2.EI_Status);
				}
			}
		}

		public void TestAdapterSendeHubAdapterExceptionWithDictionary()
		{
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);

			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var interchange1 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			var interchange2 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			var interchange3 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			Factory.Save();

			var messageExceptionDictionary = new CargoWise.eHub.Common.SerializableDictionary<Guid, string> {
					{ interchange2.EI_SessionGUID.ToGuid(), "some other exception in eHub" },
					{ interchange3.EI_SessionGUID.ToGuid(), null }
				};

			var outbox = new TestMessageOutbox();

			var mockAdapter = new Mock<IeHubAdapter>();
			mockAdapter.Setup(m => m.Outbox).Returns(outbox);
			mockAdapter.Setup(m => m.SendMessages()).Throws(new eHubAdapterException("some exception in eHub", messageExceptionDictionary));
			mockAdapter.Setup(x => x.Dispose());

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockServiceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);

			AssertNoExceptionThrown(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("Error: 1 interchange(s) sent to {0}. 2 interchange(s) failed to send to {0}.\r\nInternal Exception: CargoWise.eHub.Adapter.eHubAdapterException: some exception in eHub", ServiceTaskName));
			CombineAssertions(() =>
			{
				AssertEquals(GetInterchangePendingStatus(), interchange1.EI_Status);
				AssertEquals(EDIInterchange.Status.Failed, interchange2.EI_Status);
				AssertEquals(EDIInterchange.Status.Failed, interchange3.EI_Status);
			});
		}

		public void TestAdapterSendeHubAdapterExceptionWithoutDictionary()
		{
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);

			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var interchange1 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			var interchange2 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			var interchange3 = CreateTestInterchange("B", interchangeNumberStrategy, messageNumberStrategy);
			Factory.Save();

			var outbox = new TestMessageOutbox();

			var mockAdapter = new Mock<IeHubAdapter>();
			mockAdapter.Setup(m => m.Outbox).Returns(outbox);
			mockAdapter.Setup(m => m.SendMessages()).Throws(new eHubAdapterException("some error on eHub"));
			mockAdapter.Setup(x => x.Dispose());

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockServiceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);

			AssertExceptionThrown<eHubAdapterException>(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			CombineAssertions(() =>
			{
				AssertEquals(GetInterchangeQueuedStatus(), interchange1.EI_Status);
				AssertEquals(GetInterchangeQueuedStatus(), interchange2.EI_Status);
				AssertEquals(GetInterchangeQueuedStatus(), interchange3.EI_Status);
			});
		}

		public void TestSendMessages()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy));
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(100);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			Factory.ReloadAll<EDIInterchange>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("15 interchange(s) sent to {0}.", ServiceTaskName));
			foreach (var item in interchangeList)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			foreach (var item in reloadedInterchanges)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}
		}

		public void TestSendMessages_MultipleBatchWhenExceedingOutBoxCountLimit_SendCountLimitInRegistryChanged()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy));
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(6);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			Factory.ReloadAll<EDIInterchange>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("6 interchange(s) sent to {0}.", ServiceTaskName), 2);
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("3 interchange(s) sent to {0}.", ServiceTaskName));
			foreach (var item in interchangeList)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			foreach (var item in reloadedInterchanges)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}
		}

		public void TestSendMessages_MultipleBatchWhenExceedingOutBoxLimit()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, interchangeSize: 1));
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(100);
			serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(9);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			Factory.ReloadAll<EDIInterchange>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("9 interchange(s) sent to {0}.", ServiceTaskName));
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("6 interchange(s) sent to {0}.", ServiceTaskName));
			foreach (var item in interchangeList)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			foreach (var item in reloadedInterchanges)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}
		}

		[TestDate]
		public void TestGetPendingItemsRetrievesRequestedAmountOfRecords()
		{
			var recipient = "HYETSTTST";
			CreateTestInterchanges(1000, recipient);
			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			mockServiceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			mockServiceTaskJob.Object.CurrentRecipient = recipient;
			AssertEquals("Wrong number of records retrieved", 5, mockServiceTaskJob.Object.GetPendingItems(5).Count);
			AssertEquals("Wrong number of records retrieved", 50, mockServiceTaskJob.Object.GetPendingItems(50).Count);
			AssertEquals("Wrong number of records retrieved", 500, mockServiceTaskJob.Object.GetPendingItems(500).Count);
		}

		[ExpectNoExceptions]
		public void TestGetPendingItemsAdapterLimitGreaterThanBatchSize()
		{
			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			mockServiceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(5).Verifiable();
			mockServiceTaskJob.Setup(m => m.PendingItemsBatchSize).Returns(2).Verifiable();
			mockServiceTaskJob.Setup(m => m.GetPendingItems(2)).Returns(GetCandidates(2)).Verifiable();
			mockServiceTaskJob.Setup(m => m.NotifyVerbose(It.IsAny<string>())).Verifiable();
			mockServiceTaskJob.Setup(m => m.ValidatePendingItems(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>()))
				.Returns(Array.Empty<LightweightOutboundInterchangeCandidate>()).Verifiable();
			mockServiceTaskJob.Object.ProcessMessagesCore();
			mockServiceTaskJob.Verify();
			mockServiceTaskJob.Verify(m => m.NotifyVerbose(It.IsAny<string>()), Times.Exactly(2));
		}

		[UseSnapshotProtection]
		public virtual void TestExecuteInternal_CorrectlyHandlesLocks_DuringJobRun()
		{
			var secondJobCompleted = new AutoResetEvent(false);
			var sendCountJob1 = 0;
			var pauseOfFirstJob = new AutoResetEvent(false);
			using (secondJobCompleted)
			using (pauseOfFirstJob)
			{
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var factory = new BusinessObjectFactory();
						var company1 = CreateCompanyWithBranch(factory);
						var interchangeNumbers = new TestMessageNumberStrategy();
						var messageNumbers = new TestMessageNumberStrategy();
						var interchange1 = CreateTestInterchange(factory, GetInterchangeQueuedStatus(), company1, "RC1", interchangeNumbers, messageNumbers);
						interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
						SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
						var interchange2 = CreateTestInterchange(factory, GetInterchangeQueuedStatus(), company1, "RC1", interchangeNumbers, messageNumbers);
						interchange2.EI_SystemCreateTimeUtc = DateTime.UtcNow;
						SetInterchangeProperties(interchange2, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
						var interchange3 = CreateTestInterchange(factory, GetInterchangeQueuedStatus(), company1, "RC2", interchangeNumbers, messageNumbers);
						interchange3.EI_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc.AddSeconds(5);
						SetInterchangeProperties(interchange3, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
						factory.Save();
					}
				}).Wait();

				var job1Task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var mockAdapter1 = new EHubAdapterMock();
						mockAdapter1.OnSend += (o, e) =>
						{
							++sendCountJob1;
							if (sendCountJob1 == 2)
							{
								pauseOfFirstJob.Set();
								Assert("secondJobCompleted event was not signalled in time", secondJobCompleted.WaitOne(TimeSpan.FromMinutes(1)));
							}
						};
						var mockAdapterFactory1 = new Mock<IAdaptorFactory>();
						mockAdapterFactory1.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Returns(mockAdapter1);
						var job1 = CreateMockJob_Moq(mockAdapterFactory1.Object, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
						job1.Setup(m => m.PendingItemsBatchSize).Returns(1);
						job1.Object.Execute(CancellationToken.None);
						mockAdapterFactory1.VerifyAll();
					}
				});

				Assert("pauseOfFirstJob event was not signalled in time", pauseOfFirstJob.WaitOne(TimeSpan.FromMinutes(1)));
				var sendCountJob2 = 0;
				var mockAdapter2 = new EHubAdapterMock();
				mockAdapter2.OnSend += (o, e) =>
				{
					++sendCountJob2;
				};
				var mockAdapterFactory2 = new Mock<IAdaptorFactory>();
				mockAdapterFactory2.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Returns(mockAdapter2);
				var job2 = CreateMockJob_Moq(mockAdapterFactory2.Object, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
				job2.Object.Execute(CancellationToken.None);
				secondJobCompleted.Set();
				job1Task.Wait();
				AssertEquals("Job 1 should have sent both messages as we waited for it's second send", 2, sendCountJob1);
				AssertEquals("Job 2 shouldn't send anything as the locks should have been held until the completion of job 1", 0, sendCountJob2);

				job2.Object.Execute(CancellationToken.None);
				AssertEquals("Job 2 should send the remaining interchange now that job 1 has completed", 1, sendCountJob2);
				mockAdapterFactory2.VerifyAll();
			}
		}

		IReadOnlyCollection<LightweightOutboundInterchangeCandidate> GetCandidates(int itemCount)
		{
			var items = new LightweightOutboundInterchangeCandidate[itemCount];
			for (int i = 0; i < itemCount; i++)
			{
				items[i] = new LightweightOutboundInterchangeCandidate(CreateTestInterchange(Factory));
			}
			return items;
		}

		protected abstract string GetExpectedTableIndexHint();

		protected abstract string GetInterchangeQueuedStatus();

		protected abstract string GetInterchangePendingStatus();

		protected override void AdditionalServiceTaskJobSetup_Moq(Mock<TOutboundEDIInterchangesServiceTaskJob> mockEdiInterchangesServiceTaskJob, GlbCompany company, bool mockInterchangeCandidates = true)
		{
			base.AdditionalServiceTaskJobSetup_Moq(mockEdiInterchangesServiceTaskJob, company, mockInterchangeCandidates);
			if (mockInterchangeCandidates)
			{
				var interchangeCandidates = new List<LightweightOutboundInterchangeCandidate>
						{
							new LightweightOutboundInterchangeCandidate(CreateTestInterchange(Factory))
						};
				var branchRecipientPairs = new List<Dictionary<string, object>>
						{
							new Dictionary<string, object> { [EDIInterchangeSchema.Constants.EI_GB] = company.FirstActiveBranch.PK, [EDIInterchangeSchema.Constants.EI_To] = "TEST_Recipient", },
						};
				mockEdiInterchangesServiceTaskJob.Setup(m => m.GetPendingItems(It.IsAny<int>())).Returns(interchangeCandidates);
				mockEdiInterchangesServiceTaskJob.Setup(m => m.GetBatchingCriteriaForTopPendingInterchanges()).Returns(branchRecipientPairs);
				mockEdiInterchangesServiceTaskJob.Setup(m => m.FillOutbox(It.IsAny<IMessageOutbox>(), It.IsAny<IReadOnlyList<KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>>>(), It.IsAny<int>()))
					.Returns(interchangeCandidates.Count);
				var messages = new List<KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>>(new[] { new KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>(interchangeCandidates[0], new Mock<IeHubMessage>().Object) });
				mockEdiInterchangesServiceTaskJob.Setup(m => m.CreateMessages(It.IsAny<IReadOnlyCollection<LightweightOutboundInterchangeCandidate>>())).Returns(messages);
			}
		}

		protected override Mock<TOutboundEDIInterchangesServiceTaskJob> CreateMockJob_Moq(IAdaptorFactory adaptorFactory, ICompanySettingsManager settingsManager, GlbCompany company, INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var mockServiceTaskJob = new Mock<TOutboundEDIInterchangesServiceTaskJob>(CreateMockServiceTaskSupport(ServiceTaskName, settingsManager), notifications ?? new NotificationBuffer(), adaptorFactory, new ZQueryFactoryForTest(), new DynamicBusinessObjectCollectionFactoryForTest()) { CallBase = true };
			AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			return mockServiceTaskJob;
		}

		public virtual ICompanySettingsManager CreateCompanySettingsManager() => new eHubMessagingCompanySettingsManager();

		public static EDIInterchange CreateTestInterchange(BusinessObjectFactory factory, string queuedStatus, GlbCompany from, string to)
		{
			return CreateTestInterchange(factory, queuedStatus, from, to, new TestMessageNumberStrategy(), new TestMessageNumberStrategy());
		}

		public static EDIInterchange CreateTestInterchange(BusinessObjectFactory factory)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_SessionGUID = interchange.PK;
			return interchange;
		}

		public static EDIInterchange CreateTestInterchange(BusinessObjectFactory factory, string queuedStatus, GlbCompany from, string to, IMessageNumberStrategy interchangeNumberStrategy, IMessageNumberStrategy messageNumberStrategy, bool onlyCreateInterchange = false, int interchangeSize = 1)
		{
			var interchange = CreateTestInterchange(factory);
			interchange.EI_From = from.LicenceKeyIdentifier;
			interchange.EI_To = to;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = queuedStatus;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.SetEI_BodyTextSource(new TextReaderSource(new MemoryStream(new UTF8Encoding(false).GetBytes(new string('_', interchangeSize))))); // This should bypass compression. Strangely enough there is no 42000 byte "magic number" on compression when going direct to the ZBlob property. It always compressses. Go figure!
			interchange.NumberStrategy = interchangeNumberStrategy;
			interchange.EI_SystemCreateTimeUtc = DateTime.UtcNow;
			interchange.EI_GB = from.FirstActiveBranch.PK;

			if (!onlyCreateInterchange)
			{
				var message1 = interchange.ContainedMessages.AddNew();
				message1.FillWithValidTestData();
				message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message1.MessageNumberStrategy = messageNumberStrategy;
				var message2 = interchange.ContainedMessages.AddNew();
				message2.FillWithValidTestData();
				message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message2.MessageNumberStrategy = messageNumberStrategy;
			}

			return interchange;
		}

		protected EDIInterchange CreateTestInterchange(string to, IMessageNumberStrategy interchangeNumberStrategy, IMessageNumberStrategy messageNumberStrategy, bool onlyCreateInterchange = false, int interchangeSize = 1)
		{
			return CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), CurrentCompany, to, interchangeNumberStrategy, messageNumberStrategy, onlyCreateInterchange, interchangeSize);
		}

		EDIInterchange CreateTestInterchange(GlbCompany from, string to, IMessageNumberStrategy interchangeNumberStrategy, IMessageNumberStrategy messageNumberStrategy, bool onlyCreateInterchange = false, int interchangeSize = 1)
		{
			return CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), from, to, interchangeNumberStrategy, messageNumberStrategy, onlyCreateInterchange, interchangeSize);
		}

		void CreateTestInterchanges(int nInterchanges, string recipient)
		{
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < nInterchanges; ++i)
			{
				CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy);
			}
			Factory.Save();
		}

		protected EDIInterchange[] CreateTestInterchanges(string recipient, IMessageNumberStrategy interchangeNumberStrategy, IMessageNumberStrategy messageNumberStrategy, string queueStatus, string successStatus, string failStatus)
		{
			var result = new EDIInterchange[10];

			//Normal Message
			result[0] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[0], CurrentCompany.Branches[0].PK, 0, true, EDIInterchange.Direction.Transmit, queueStatus);
			Factory.Save();

			//Normal Message InActive
			result[1] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[1], CurrentCompany.Branches[0].PK, 0, false, EDIInterchange.Direction.Transmit, queueStatus);
			Factory.Save();

			//Normal Message from different branch in same company
			result[2] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[2], CurrentCompany.Branches[1].PK, 0, true, EDIInterchange.Direction.Transmit, queueStatus);
			Factory.Save();

			// Retried less than 5 times within 5 minutes
			result[3] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[3], CurrentCompany.Branches[0].PK, 1, true, EDIInterchange.Direction.Transmit, queueStatus);
			SaveFactoryInDifferentTime(-1);

			// Retried less than 5 times over 5 minutes
			result[4] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[4], CurrentCompany.Branches[0].PK, 1, true, EDIInterchange.Direction.Transmit, queueStatus);
			SaveFactoryInDifferentTime(-6);

			// Retried more than 5 times within 60 minutes
			result[5] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[5], CurrentCompany.Branches[0].PK, 6, true, EDIInterchange.Direction.Transmit, queueStatus);
			SaveFactoryInDifferentTime(-30);

			// Retried more than 5 times over 60 minutes
			result[6] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[6], CurrentCompany.Branches[1].PK, 6, true, EDIInterchange.Direction.Transmit, queueStatus);
			SaveFactoryInDifferentTime(-61);

			// Processed message: Success
			result[7] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[7], CurrentCompany.Branches[0].PK, 0, true, EDIInterchange.Direction.Transmit, successStatus);
			Factory.Save();

			// Processed message: Fail
			result[8] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[8], CurrentCompany.Branches[0].PK, 0, true, EDIInterchange.Direction.Transmit, failStatus);
			Factory.Save();

			// Processed message: Fail
			result[9] = CreateTestInterchange(CurrentCompany, recipient, interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(result[9], CurrentCompany.Branches[0].PK, 0, true, EDIInterchange.Direction.Receive, queueStatus);
			Factory.Save();

			return result;
		}

		public static void SetInterchangeProperties(EDIInterchange testInterchange, ZGuid eI_GB, ZInt eI_RetryCount, ZBool eI_IsActive, ZString eI_receiveTransmit, ZString eI_Status)
		{
			testInterchange.EI_GB = eI_GB;
			testInterchange.EI_RetryCount = eI_RetryCount;
			testInterchange.EI_IsActive = eI_IsActive;
			testInterchange.EI_ReceiveTransmit = eI_receiveTransmit;
			testInterchange.EI_Status = eI_Status;
		}

		public static IeDoc AddDISDocument(IDocManagerSupport documentManager, byte[] documentBytes, DisposableList disposables)
		{
			var documentStream = (SubStreamableStream)new MemoryStream(documentBytes);
			var eDoc = documentManager.DocManagerInfo.AddFileOrDocument(documentStream, "Document", "DIS");
			disposables.Add((IDisposable)eDoc);
			return eDoc;
		}

		protected class TestMessageNumberStrategy : IMessageNumberStrategy
		{
			int number;
			public string GetMessageReferenceNumber()
			{
				return (++number).ToString();
			}
		}

		void SaveFactoryInDifferentTime(int timeDifferenceInMinute)
		{
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(timeDifferenceInMinute);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-timeDifferenceInMinute);
		}

		class TestMessageOutbox : IMessageOutbox
		{
			readonly List<IeHubMessage> messages = new List<IeHubMessage>();

			public IEnumerator<IeHubMessage> GetEnumerator()
			{
				return messages.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Clear()
			{
				messages.Clear();
			}

			public long SizeInKiloBytes
			{
				get { return Count; }
			}

			public int Count
			{
				get { return messages.Count; }
			}

			public void AddMessage(IeHubMessage message)
			{
				messages.Add(message);
			}
		}
	}
}
