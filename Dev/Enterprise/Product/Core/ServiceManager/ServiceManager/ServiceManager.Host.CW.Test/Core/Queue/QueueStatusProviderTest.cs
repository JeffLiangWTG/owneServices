using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class QueueStatusProviderTest : TestCase
	{
		public void TestArgumentNullExceptions()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueStatusProvider(null, Mock.Of<IMemoryCache>(), Mock.Of<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object));
				AssertEquals("statusProvider", result.ParamName);
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueStatusProvider(statusProviderMock.Object, null, Mock.Of<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object));
				AssertEquals("memoryCache", result.ParamName);
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueStatusProvider(statusProviderMock.Object, Mock.Of<IMemoryCache>(), null, cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object));
				AssertEquals("schemaResolver", result.ParamName);
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueStatusProvider(statusProviderMock.Object, Mock.Of<IMemoryCache>(), Mock.Of<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, null, queuesProviderMock.Object));
				AssertEquals("errorReporterProxy", result.ParamName);
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueStatusProvider(statusProviderMock.Object, Mock.Of<IMemoryCache>(), Mock.Of<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, null));
				AssertEquals("hostedServiceQueuesProvider", result.ParamName);
			});
		}

		void SetupMockStatusProvider(string code, bool isActive, int runningCount = 0, int errorCount = 0)
		{
			var serviceTaskMock = new Mock<IRunnableServiceTask>();
			serviceTaskMock
				.Setup(x => x.Code)
				.Returns(code);
			serviceTaskMock
				.Setup(x => x.IsActive)
				.Returns(isActive);
			serviceTaskMock
				.Setup(x => x.ErrorCountLast24Hours)
				.Returns(errorCount);
			var serviceTaskCodeWithRunnerProcessIds = Enumerable.Range(0, runningCount)
				.Select(i => new ServiceTaskCodeWithRunnerProcessId(code, i)).ToList();
			var serviceTaskStatus = new ServiceTaskStatus(serviceTaskMock.Object, new List<IRunnableServiceTask>(), serviceTaskCodeWithRunnerProcessIds, new List<HostedServiceBusinessObjectBindingAttribute>(), false);
			statusProviderMock
				.Setup(x => x.GetTaskStatus(code))
				.Returns(serviceTaskStatus);
		}

		[UseSnapshotProtection]
		public void TestProviderWhenServiceTaskIsActive_SearchDTOQueueWithServiceTaskCode()
		{
			//Arrange
			SetupMockStatusProvider("LWM", true);
			Db.Connection.ExecuteNonQuery(@"delete from dbo.StmALogQueue;
				insert into dbo.stmalog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime)
				values ('3D700269-13C5-4388-95BB-8A5EC6CBE921', '3D700269-13C5-4388-95BB-8A5EC6CBE921', 'OrgHeader', 'ADD', 'BAS', '01-01-2020')");

			//Act
			var result = provider.GetQueueStatus();

			//Assert
			var queue = result.QueueList.Single(x => x.ServiceTaskCode == "LWM");
			AssertEquals(1, queue.ItemCount);
			AssertEquals("Log Walker Master", queue.QueueName);
			AssertEquals(true, queue.IsActive);
			AssertEquals(0, queue.RunningCount);
			AssertEquals(0, queue.ErrorCountLast24Hours);
		}

		[UseSnapshotProtection]
		public void TestProviderWhenServiceTaskIsActive_SearchDTOQueueWithQueueName()
		{
			//Arrange
			SetupMockStatusProvider("UMI", true);

			//Act
			Db.Connection.ExecuteNonQuery(@"delete from dbo.StmALogQueue;
				insert into dbo.stmalog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime)
				values ('3D700269-13C5-4388-95BB-8A5EC6CBE921', '3D700269-13C5-4388-95BB-8A5EC6CBE921', 'OrgHeader', 'ADD', 'BAS', getdate())");
			var result = provider.GetQueueStatus();
			var queue = result.QueueList.Single(x => x.QueueName == "XML Universal Event");

			//Assert
			AssertEquals(0, queue.ItemCount);
			AssertEquals("UMI", queue.ServiceTaskCode);
			AssertEquals(true, queue.IsActive);
			AssertEquals(0, queue.RunningCount);
			AssertEquals(0, queue.ErrorCountLast24Hours);
		}

		public void TestGeneratedQueryStringIsParametised()
		{
			// Arrange
			string queueResultTextAfterParameterisation;
			var expectedParameters = new List<string>
			{
				"@P0_0",
				"@P0_1",
				"@P0_2_0",
				"@P0_2_1",
				"@P0_2_2",
				"@P0_3_0",
				"@P0_3_1",
				"@P0_3_2",
				"@P0_4",
				"@P0_5",
			};

			var tableName = "EDIMessage";
			var predicates = new[]
			{
				"EM_MessageType!=TST",
				"EM_IsActive=1",
				"EM_ApplicationCode IN ('ABC', 'DEF', 'GHI')",
				"EM_SystemCreateUser NOT IN ('JKL', 'MNO', 'PQR')",
				"EM_ApplicationReference like %T",
				"EM_MessageText like %Test%",
				"EM_ExternalReferenceNumber like T%",
				"EM_SystemLastEditTimeUtc IS PASTORNULL",
				"EM_MessageData IS NULL"
			};

			var bindingMock = new Mock<IHostedServiceBusinessObjectBinding>();
			bindingMock.Setup(b => b.Table).Returns(tableName);
			bindingMock.Setup(b => b.ServiceTaskCode).Returns("TST");
			bindingMock.Setup(b => b.QueueName).Returns("Test Queue");
			bindingMock.Setup(b => b.Predicates).Returns(predicates);

			var subProviderMock = new Mock<IHostedServiceBusinessObjectBindingsSubProvider>();
			subProviderMock.Setup(s => s.BusinessObjectBindings)
				.Returns(new[] { bindingMock.Object });

			//Act
			using (ObjectFactory.Substitute("HostedServiceBusinessObjectBindingsSubProviders", new IHostedServiceBusinessObjectBindingsSubProvider[] { subProviderMock.Object }))
			using (Db.Connection.TrackExecutedCommands())
			{
				var result = provider.GetQueueStatus();
				var ii = Db.Connection.ExecutedCommands;
				queueResultTextAfterParameterisation = Db.Connection.ExecutedCommands.FirstOrDefault();
			}

			// Assert
			CombineAssertions(() =>
			{
				foreach (var parameter in expectedParameters)
				{
					AssertContains($"QueueResultFromQuery SQL Text does not include the parameter: {parameter}", parameter, queueResultTextAfterParameterisation);
				}
			});
		}

		[UseSnapshotProtection]
		public void TestProviderWhenServiceTaskIsNotActive()
		{
			//Arrange
			SetupMockStatusProvider("AVS", false);
			Db.Connection.ExecuteNonQuery(@"delete from dbo.StmALogQueue;
				insert into dbo.stmalog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime)
				values ('3D700269-13C5-4388-95BB-8A5EC6CBE921', '3D700269-13C5-4388-95BB-8A5EC6CBE921', 'OrgHeader', 'ADD', 'BAS', getdate())");

			//Act
			var result = provider.GetQueueStatus();

			//Assert
			var disabledServiceTask = result.QueueList.Single(x => x.ServiceTaskCode == "AVS");
			AssertEquals("Disabled AVS Queue IsEnabled", false, disabledServiceTask.IsActive);
		}

		[UseSnapshotProtection]
		public void TestProviderMaximumItemAge()
		{
			// Arrange
			var startDate = new DateTime(2020, 1, 1);
			Db.Connection.ExecuteNonQuery($@"
delete from dbo.StmPrintJob;
insert into dbo.StmPrintJob (SP_PK, SP_SystemLastEditTimeUtc, SP_RunDateTime, SP_JobType)
values ('3D700269-13C5-4388-95BB-8A5EC6CBE921', '{startDate}', '{startDate}', 'FAX');
");

			// Act
			var result = provider.GetQueueStatus();

			// Assert
			var queue = result.QueueList.Single(x => x.QueueName == "Print Jobs Scheduling");
			AssertNotNull(queue.MaximumItemAgeInSeconds);
			var expectedAge = DateTime.UtcNow - startDate;
			AssertCloseEnough((int)expectedAge.TotalSeconds, (int)queue.MaximumItemAgeInSeconds, 60);
		}

		public void TestProviderReturnsDistinctQueueNames()
		{
			// Act
			var result = provider.GetQueueStatus();

			// Assert
			var list = result.QueueList;
			var duplicates = list.GroupBy(q => q.QueueName).Where(g => g.Count() > 1);
			AssertContainsExactElementsInAnyOrder("There are duplicate queue names in returned list", Array.Empty<string>(), duplicates.Select(group => group.Key));
		}

		public void TestProviderReturnsValidQueueValues()
		{
			// Act
			var result = provider.GetQueueStatus();

			// Assert
			CombineAssertions(() =>
			{
				var queuesWithNegativeCounts = result.QueueList.Where(q => q.ItemCount < 0);
				AssertContainsExactElementsInAnyOrder("Some queue length counts are negative", Array.Empty<string>(), queuesWithNegativeCounts.Select(q => q.QueueName));
				var queuesWithNegativeAges = result.QueueList.Where(q => q.MaximumItemAgeInSeconds < 0);
				AssertContainsExactElementsInAnyOrder("Some queue maximum age's are negative", Array.Empty<string>(), queuesWithNegativeAges.Select(q => q.QueueName));
			});
		}

		[ExpectNoExceptions]
		public void TestProviderDisposesDbConnection()
		{
			// Using thread so we have a new Db.Connection
			var thread = new Thread(() =>
			{
				_ = provider.GetQueueStatus();
			});
			thread.Start();
			thread.Join();
		}

		[UseSnapshotProtection]
		public void TestEDIMessageQueue()
		{
			var branchPk = GlbBranch.CurrentBranch.PK;
			var departmentPk = GlbDepartment.CurrentDepartment.PK;
			var user = GlbStaff.CurrentUser.GS_Code;
			Db.Connection.ExecuteNonQuery($@"DELETE FROM dbo.EdiMessage;
INSERT INTO dbo.EdiMessage (EM_PK, EM_GB, EM_GE, EM_MessageType, EM_MessageSubType, EM_ReceiveTransmit, EM_ApplicationCode, EM_IsActive, EM_Status, EM_HeldUntilDate,EM_SystemCreateTimeUtc,EM_SystemCreateUser,EM_SystemLastEditTimeUtc,EM_SystemLastEditUser) values
	(NEWID(), '{branchPk}', '{departmentPk}', 'XDC', 'XUE', 'RCV', 'UDM', 1, 'QUE', null, GETUTCDATE(), '{user}', GETUTCDATE(), '{user}'),
	(NEWID(), '{branchPk}', '{departmentPk}', 'XDC', 'XUE', 'RCV', 'UDM', 1, 'QUE', DATEADD(DAY, -1, GETUTCDATE()), GETUTCDATE(), '{user}', GETUTCDATE(), '{user}'),
	(NEWID(), '{branchPk}', '{departmentPk}', 'XDC', 'XUE', 'RCV', 'UDM', 1, 'QUE', DATEADD(DAY, 1, GETUTCDATE()), GETUTCDATE(), '{user}', GETUTCDATE(), '{user}')");

			var result = provider.GetQueueStatus();
			var queue = result.QueueList.Single(x => x.QueueName == "XML Universal Event");
			AssertEquals(2, queue.ItemCount);
		}

		public void TestProviderReturnsValidRunningCountErrorCount()
		{
			CombineAssertions(() =>
			{
				AssertHandlerReturnsValidRunningCountErrorCount(0, 0);
				AssertHandlerReturnsValidRunningCountErrorCount(3, 0);
				AssertHandlerReturnsValidRunningCountErrorCount(0, 3);
				AssertHandlerReturnsValidRunningCountErrorCount(10, 5);
				AssertHandlerReturnsValidRunningCountErrorCount(20, 10);
			});
		}

		void AssertHandlerReturnsValidRunningCountErrorCount(int runningCount, int errorCount)
		{
			// Arrange
			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));
			var handlerForRunningCountErrorCount = new QueueStatusProvider(statusProviderMock.Object, memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);
			SetupMockStatusProvider("LWM", true, runningCount, errorCount);

			// Act
			var result = handlerForRunningCountErrorCount.GetQueueStatus();
			var queue = result.QueueList.Single(x => x.ServiceTaskCode == "LWM");

			// Assert
			AssertEquals(runningCount, queue.RunningCount);
			AssertEquals(errorCount, queue.ErrorCountLast24Hours);
		}

		public void TestMemoryCacheNewValueInsertion()
		{
			// Arrange
			var memoryCacheMock = new Mock<IMemoryCache>();
			var expectedResultValue = new QueueListDTO(Enumerable.Empty<QueueDTO>());
			memoryCacheMock
				.Setup(cache => cache.AddOrGetExisting(nameof(QueueRequestHandler), It.IsAny<Func<QueueListDTO>>(), TimeSpan.FromMinutes(1)))
				.Returns(expectedResultValue);

			var testMemoryCacheProvider = new QueueStatusProvider(statusProviderMock.Object, memoryCacheMock.Object, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);

			// Act
			var result = testMemoryCacheProvider.GetQueueStatus();

			//Assert
			AssertEquals(expectedResultValue, result);
			memoryCacheMock.Verify(cache => cache.AddOrGetExisting(It.IsAny<string>(), It.IsAny<Func<QueueListDTO>>(), TimeSpan.FromMinutes(1)), Times.Once);
			memoryCacheMock.VerifyNoOtherCalls();
		}

		protected override void SetUp()
		{
			base.SetUp();
			statusProviderMock = new Mock<ITaskStatusProvider>();
			cancellationTokenSource = new CancellationTokenSource();

			cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
			cancellationTokenProviderMock
				.SetupGet(cancellationTokenProvider => cancellationTokenProvider.Token)
				.Returns(cancellationTokenSource.Token);

			errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

			queuesProviderMock = new Mock<IHostedServiceQueuesProvider>();
			queuesProviderMock
				.Setup(m => m.Queues)
				.Returns(ObjectFactory
								.Get<IEnumerable>("HostedServiceQueuesSubProviders")
								.Cast<IHostedServiceQueuesSubProvider>()
								.ToArray()
								.SelectMany(x => x.Queues));

			provider = new QueueStatusProvider(statusProviderMock.Object, memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			cancellationTokenSource.Dispose();
			memoryCache.Dispose();
		}	

		class QueueSizeExceptionHandlingTest : TransactionedTestCase
		{
			[ExpectNoExceptions]
			public void TestExceptionFromQueueSizeIsReported()
			{
				CombineAssertions(() =>
				{
					foreach (var expectedException in exceptions)
					{
						Test(expectedException);
					}
				});

				void Test(Exception expectedException)
				{
					// Arrange
					provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None)), ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);
					queueMock.Invocations.Clear();

					queueMock
						.Setup(m => m.QueueResult)
						.Throws(expectedException);

					using (ObjectFactory.Substitute(queuesProviderMock.Object))
					{
						// Act
						provider.GetQueueStatus();

						// Assert
						errorReporterProxyMock.Verify(queue => queue.ReportOnce(It.IsAny<string>(), expectedException),
							Times.Once);
						errorReporterProxyMock.VerifyNoOtherCalls();
					}
				}
			}

			[ExpectNoExceptions]
			public void TestExceptionFromCreateQueueResultFromQuery()
			{
				//Arrange
				using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));
				provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);

				var taskCode = "PRC";
				var queueName = "PRC test queue";
				var businessObjectBindingMock = Mock.Of<IHostedServiceBusinessObjectBinding>(
					b => b.QueueName == queueName &&
						b.ServiceTaskCode == taskCode &&
						b.Table == "ThisTableDoesNotExist" &&
						b.Predicates == Array.Empty<string>());
				var businessObjectBindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
				businessObjectBindingsProviderMock
					.Setup(m => m.BusinessObjectBindings)
					.Returns(new[] { businessObjectBindingMock });

				var serviceQueueMock = Mock.Of<IHostedServiceQueue>(
					q => q.QueueResult == QueueResult.Zero &&
						q.Name == $"{queueName} 2" &&
						q.ServiceTaskCode == taskCode);
				queuesProviderMock
					.Setup(m => m.Queues)
					.Returns(new[] { serviceQueueMock });

				using (ObjectFactory.Substitute(queuesProviderMock.Object))
				using (ObjectFactory.Substitute(businessObjectBindingsProviderMock.Object))
				{
					// Act
					var result = provider.GetQueueStatus();

					// Assert
					errorReporterProxyMock.Verify(queue => queue.ReportOnce($"Exception occurred during queue size query for Queue [{queueName}], Service Task [{taskCode}].", It.Is<SqlException>(ex => ex.Number == 208)), Times.Once);
					errorReporterProxyMock.VerifyNoOtherCalls();

					var list = result.QueueList.ToList();
					var item = list.Single(q => q.QueueName == queueName);
					AssertEquals(2, list.Count);
					AssertEquals(QueueResult.Error.QueueSize, -1);
					AssertEquals(QueueResult.Error.MaximumItemAge, TimeSpan.Zero);
					AssertEquals(taskCode, item.ServiceTaskCode);
				}
			}

			public void TestValueIsReturnedOnExceptionFromQueueSize()
			{
				CombineAssertions(() =>
				{
					for (var i = 0; i < exceptions.Count; i++)
					{
						Test(exceptions[i], i);
					}
				});

				void Test(Exception exception, int index)
				{
					// Arrange
					provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None)), ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);
					var queueName = $"Queue name {index:D5}";
					var taskCode = $"{index:D3}";
					queueMock
						.Setup(m => m.QueueResult)
						.Throws(exception);
					queueMock
						.Setup(queue => queue.Name)
						.Returns(queueName);
					queueMock
						.Setup(queue => queue.ServiceTaskCode)
						.Returns(taskCode);

					using (ObjectFactory.Substitute(queuesProviderMock.Object))
					{
						// Act
						var result = provider.GetQueueStatus();

						// Assert
						var list = result.QueueList;
						var item = list.Single(q => q.QueueName == queueName);
						AssertEquals(QueueResult.Error.QueueSize, item.ItemCount);
						AssertEquals(QueueResult.Error.MaximumItemAge, TimeSpan.FromSeconds((int)item.MaximumItemAgeInSeconds));
						AssertEquals(taskCode, item.ServiceTaskCode);
					}
				}
			}

			public void TestExceptionsAreCritical()
			{
				CombineAssertions(() =>
				{
					foreach (var criticalException in criticalExceptions)
					{
						Assert(criticalException.IsCriticalException());
					}
				});
			}

			public void TestCriticalExceptionFromQueueSizeQueryWasThrown()
			{
				CombineAssertions(() =>
				{
					foreach (var criticalException in criticalExceptions)
					{
						Test(criticalException);
					}
				});

				void Test(Exception criticalException)
				{
					// Arrange
					provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None)), ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);

					queueMock
						.Setup(m => m.QueueResult)
						.Throws(criticalException);

					using (ObjectFactory.Substitute(queuesProviderMock.Object))
					{
						// Act
						// Assert
						var thrownException = AssertExceptionThrown<Exception>(() => { provider.GetQueueStatus(); });
						AssertEquals(criticalException, thrownException);
					}
				}
			}

			[ExpectNoExceptions]
			public void TestSqlCommandTimeoutInQueueSizeQuery()
			{
				// Arrange
				using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));
				provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);
				queueMock
					.Setup(m => m.QueueResult)
					.Returns(() => new QueueResult(Db.Connection.ExecuteScalar<int>("WAITFOR DELAY '00:00:16'"), TimeSpan.Zero));

				using (ObjectFactory.Substitute(queuesProviderMock.Object))
				{
					// Act
					provider.GetQueueStatus();

					// Assert
					errorReporterProxyMock.Verify(queue => queue.ReportOnce(It.IsAny<string>(), It.Is<SqlException>(ex => ex.Number == -2)), Times.Once);
					errorReporterProxyMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestQueueQueryStopProgressingWhenCancellationTokenIsSet()
			{
				// Arrange
				queueMock
					.Setup(queue => queue.Name)
					.Returns("PRC test queue");
				queueMock
					.Setup(queue => queue.ServiceTaskCode)
					.Returns("PRC");
				queueMock
					.Setup(m => m.QueueResult)
					.Returns(() => new QueueResult(Db.Connection.ExecuteScalar<int>("WAITFOR DELAY '00:00:10'; SELECT 1;"), TimeSpan.Zero));

				queuesProviderMock
					.Setup(m => m.Queues)
					.Returns(new[] { queueMock.Object, queueMock.Object, queueMock.Object });

				using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

				provider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object, queuesProviderMock.Object);

				using (ObjectFactory.Substitute(queuesProviderMock.Object))
				{
					try
					{
						// Act
						var queueStatusTask = Task.Run(() => provider.GetQueueStatus());
						var cancellationTask = Task.Delay(2000).ContinueWith(_ => { cancellationTokenSource.Cancel(); });

						queueStatusTask.Wait();
						cancellationTask.Wait();
					}
					catch (AggregateException ex)
					{
						// Assert
						AssertEquals(1, ex.InnerExceptions.Count);
						AssertEquals(typeof(OperationCanceledException), ex.InnerExceptions[0].GetType());
					}
				}
			}

			protected override void SetUp()
			{
				base.SetUp();

				queueMock = new Mock<IHostedServiceQueue>();

				queuesProviderMock = new Mock<IHostedServiceQueuesProvider>();
				queuesProviderMock
					.Setup(m => m.Queues)
					.Returns(new[] { queueMock.Object });

				cancellationTokenSource = new CancellationTokenSource();
				cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
				cancellationTokenProviderMock
					.SetupGet(cancellationTokenProvider => cancellationTokenProvider.Token)
					.Returns(cancellationTokenSource.Token);

				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			}

			protected override void TearDown()
			{
				base.TearDown();
				cancellationTokenSource?.Dispose();
			}

			readonly List<Exception> criticalExceptions = new List<Exception>
			{
				new OutOfMemoryException("Critical exception thrown."), new AppDomainUnloadedException("Critical exception thrown."),
			};

			readonly List<Exception> exceptions = new List<Exception>
			{
				new NotImplementedException("Exception thrown."),
				SqlExceptionBuilder.CreateSqlException(1, "Exception thrown."),
				new InvalidOperationException("Exception thrown."),
				new NullReferenceException("Exception thrown."),
			};

			QueueStatusProvider provider;
			Mock<IHostedServiceQueue> queueMock;
			Mock<IHostedServiceQueuesProvider> queuesProviderMock;
			Mock<ICancellationTokenProvider> cancellationTokenProviderMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
			CancellationTokenSource cancellationTokenSource;
		}

		QueueStatusProvider provider;
		Mock<ITaskStatusProvider> statusProviderMock;
		Mock<ICancellationTokenProvider> cancellationTokenProviderMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IHostedServiceQueuesProvider> queuesProviderMock;
		CancellationTokenSource cancellationTokenSource;
		MemoryCache memoryCache;
	}
}
