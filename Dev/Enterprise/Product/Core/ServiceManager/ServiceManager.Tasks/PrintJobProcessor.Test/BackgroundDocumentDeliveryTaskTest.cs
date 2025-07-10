using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(BackgroundDocumentDeliveryTask))]
	sealed class BackgroundDocumentDeliveryTaskTest : ServiceTaskTestCase<BackgroundDocumentDeliveryTask>
	{
		public void TestRunTask()
		{
			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			Factory.Save();
			AssertJobsCountToProcess(1);
			AssertJobsCountProcessed(0);

			var backgroundDocumentDeliveryTask = new BackgroundDocumentDeliveryTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(backgroundDocumentDeliveryTask);
			AssertJobsCountToProcess(0);
			AssertJobsCountProcessed(1);
			AssertEquals(5, log.Count);
			AssertEquals("Information|Background Document Delivery Service Task Run Started.", log[0]);
			AssertEquals("Information|Purging documents previously processed", log[1]);
			AssertEquals("Information|1 document(s) to process.", log[2]);
			AssertEquals("Information|1 document(s) processed.", log[3]);
			AssertEquals("Information|Background Document Delivery Service Task Run Completed.", log[4]);
		}

		public void TestRunTask_NoJobsToProcess()
		{
			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery1, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			documentDelivery1.SDL_IsProcessed = true;
			Factory.Save();
			AssertJobsCountToProcess(0);
			AssertJobsCountProcessed(1);

			var (documentDelivery2, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			documentDelivery2.SDL_RetryAttempts = 3;
			Factory.Save();
			AssertJobsCountToProcess(0);
			AssertJobsCountProcessed(1);
		}

		public void TestRunTask_PurgeOldJobs()
		{
			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery1, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			documentDelivery1.SDL_IsProcessed = true;
			Factory.Save();
			AssertJobsCountToProcess(0);
			AssertJobsCountProcessed(1);

			var backgroundDocumentDeliveryTask = new BackgroundDocumentDeliveryTask();
			InitialiseAndRunTaskSchedule(backgroundDocumentDeliveryTask);
			AssertJobsCountToProcess(0);
			AssertJobsCountProcessed(0);
		}

		void AssertJobsCountToProcess(int count)
		{
			var query = new ZQuery(StmDocumentDeliverySchema.SDL_IsProcessed, false);
			query.AddToFilter(StmDocumentDeliverySchema.SDL_RetryAttempts, SQLComparisonOperator.LessThan, (byte)3);
			AssertEquals(count, Factory.GetDatabaseCount(typeof(StmDocumentDelivery), query));
		}

		void AssertJobsCountProcessed(int count)
		{
			AssertEquals(count, Factory.GetDatabaseCount(typeof(StmDocumentDelivery), new ZQuery(StmDocumentDeliverySchema.SDL_IsProcessed, true)));
		}

		public void TestTryToLockJobs()
		{
			var type = typeof(BackgroundDocumentDeliveryTask);
			var methodTryGetLock = type.GetMethod("TryGetLock", BindingFlags.NonPublic | BindingFlags.Static);
			var methodTryToLockJobs = type.GetMethod("TryToLockJobs", BindingFlags.NonPublic | BindingFlags.Instance);

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);

			var (documentDelivery1, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			var (documentDelivery2, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			var (documentDelivery3, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			var (documentDelivery4, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);

			var jobsToProcess1 = new[] { documentDelivery1, documentDelivery2 };
			var jobsToProcess2 = new[] { documentDelivery3, documentDelivery4 };
			using (var mutexes = new DisposableList(0))
			using (var connection0 = Db.NewExtraConnectionToMainDb())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var instance = Activator.CreateInstance(type);
				var parameters = new object[] { connection0, documentDelivery1, null };
				AssertEquals("Precondition: Lock should be acquired", true, (bool)methodTryGetLock.Invoke(instance, parameters));

				using ((SqlApplicationLock)parameters[2])
				{
					var task1 = new BackgroundDocumentDeliveryTask();
					var jobsLocked1 = (StmDocumentDelivery[])methodTryToLockJobs.Invoke(task1, new object[] { jobsToProcess1, mutexes, connection1 });
					AssertEquals(1, jobsLocked1.Length);

					var task2 = new BackgroundDocumentDeliveryTask();
					var jobsLocked2 = (StmDocumentDelivery[])methodTryToLockJobs.Invoke(task2, new object[] { jobsToProcess2, mutexes, connection2 });
					AssertEquals(2, jobsLocked2.Length);
				}
			}

			using (var mutexes = new DisposableList(0))
			using (var connection0 = Db.NewExtraConnectionToMainDb())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var instance = Activator.CreateInstance(type);
				var parameters = new object[] { connection0, documentDelivery3, null };
				AssertEquals("Precondition: Lock should be acquired", true, (bool)methodTryGetLock.Invoke(instance, parameters));

				using ((SqlApplicationLock)parameters[2])
				{
					var task2 = new BackgroundDocumentDeliveryTask();
					var jobsLocked2 = (StmDocumentDelivery[])methodTryToLockJobs.Invoke(task2, new object[] { jobsToProcess2, mutexes, connection2 });
					AssertEquals(1, jobsLocked2.Length);

					var task1 = new BackgroundDocumentDeliveryTask();
					var jobsLocked1 = (StmDocumentDelivery[])methodTryToLockJobs.Invoke(task1, new object[] { jobsToProcess1, mutexes, connection1 });
					AssertEquals(2, jobsLocked1.Length);
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new[]
				{
					new TaskNudgeInformationForTest
					(
						StmDocumentDeliverySchema.Constants.TableName,
						"BDD Job Delivery",
						StmDocumentDeliverySchema.Constants.SDL_IsProcessed + "=0"
					)
				};
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "BDD", hostedServiceAttribute.Code);
				AssertEquals("Description", "Background Document Delivery", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
			});
		}

		public void TestServiceTaskShouldCatchAnyExceptionAndRaiseErrorReport()
		{
			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			Factory.Save();
			ErrorReporter.Clear();

			var backgroundDocumentDeliveryTask = new BackgroundDocumentDeliveryTaskAlwaysThrowExceptionForTest();

			TestServiceLogger log = InitialiseAndRunTaskSchedule(backgroundDocumentDeliveryTask);
			AssertEquals("Exception for test", ErrorReporter.LastExceptionReported.Message);
			AssertEquals("Service task BDD execute failed.", ErrorReporter.LastMessageReported);
			AssertEquals("Error|Background Document Delivery Service Task Run Failed. The exception type is System.Exception. The exception message is: Exception for test. If this service task failed often, please raise an incident for further investigation.", log[2]);

			ErrorReporter.Clear();
		}

		class BackgroundDocumentDeliveryTaskAlwaysThrowExceptionForTest : BackgroundDocumentDeliveryTask
		{
			protected override DocumentDeliveryManager DocumentDeliveryManager
			{
				get
				{
					if (_documentDeliveryManager == null)
					{
						_documentDeliveryManager = new DocumentDeliveryManagerAlwaysThrowException();
					}
					return _documentDeliveryManager;
				}
			}
			DocumentDeliveryManager _documentDeliveryManager;
		}

		class DocumentDeliveryManagerAlwaysThrowException : DocumentDeliveryManager
		{
			public override void Execute(StmDocumentDelivery[] jobs, ILogger logger = null)
			{
				throw new Exception("Exception for test");
			}
		}
	}
}
