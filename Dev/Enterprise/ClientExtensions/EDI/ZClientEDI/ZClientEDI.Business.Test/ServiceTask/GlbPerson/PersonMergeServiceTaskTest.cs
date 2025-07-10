using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.Test
{
	[TestedType(typeof(PersonMergeServiceTask))]
	class PersonMergeServiceTaskTest : ServiceTaskTestCase<PersonMergeServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			ErrorReporter.Clear();
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";
			contact1b.OC_Email = "user.one@test.org";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "User One";
			contact2.OC_Email = "user.one@test.org";

			Factory.Save();

			var queueItem1 = Factory.New<EdiPersonMergeQueue>();
			queueItem1.EMQ_PER_RetainPerson = contact1a.OC_PER;
			queueItem1.EMQ_PER_DissolvePerson = contact1b.OC_PER;

			var queueItem2 = Factory.New<EdiPersonMergeQueue>();
			queueItem2.EMQ_PER_RetainPerson = contact1a.OC_PER;
			queueItem2.EMQ_PER_DissolvePerson = contact2.OC_PER;

			Factory.Save();

			var task = new PersonMergeServiceTask();
			task.ServiceLogger = new TestServiceLogger();
			AssertEquals($"Precondition failed, errors in error reporter:\n{string.Join("\n\n", ErrorReporter.ExceptionsThrown)}", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(task.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => task.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestRunTask()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";
			contact1b.OC_Email = "user.one@test.org";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "User One";
			contact2.OC_Email = "user.one@test.org";

			Factory.Save();

			var queueItem1 = Factory.New<EdiPersonMergeQueue>();
			queueItem1.EMQ_PER_RetainPerson = contact1a.OC_PER;
			queueItem1.EMQ_PER_DissolvePerson = contact1b.OC_PER;

			var queueItem2 = Factory.New<EdiPersonMergeQueue>();
			queueItem2.EMQ_PER_RetainPerson = contact1a.OC_PER;
			queueItem2.EMQ_PER_DissolvePerson = contact2.OC_PER;

			Factory.Save();

			var task = new PersonMergeServiceTask();
			task.ServiceLogger = new TestServiceLogger();
			task.RunTask();

			contact1a.Reload();
			contact1b.Reload();
			contact2.Reload();

			AssertEquals("Person should be merged", contact1a.OC_PER, contact1b.OC_PER);
			AssertEquals("Person should be merged", contact1a.OC_PER, contact2.OC_PER);

			AssertEquals("Contact 1a should be the person primary contact", contact1a.PK, contact1a.Person.PrimaryRelationship.PPR_PrimaryId);

			var queueItemsCount = new BusinessObjectFactory().GetDatabaseCount(typeof(EdiPersonMergeQueue), new ZQuery(EdiPersonMergeQueueSchema.PK, new ZGuid[] { queueItem1.PK, queueItem2.PK }));
			AssertEquals("Queue items should be cleared after process", 0, queueItemsCount);
		}

		public void TestRunTask_PersonAlreadyMerged()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";
			contact1b.OC_Email = "user.one@test.org";

			Factory.Save();

			var queueItem = Factory.New<EdiPersonMergeQueue>();
			queueItem.EMQ_PER_RetainPerson = contact1a.OC_PER;
			queueItem.EMQ_PER_DissolvePerson = contact1b.OC_PER;

			Factory.Save();

			using (var merger = new PersonMerger(contact1b.Person, contact1a.Person))
			{
				merger.Merge();
			}
			var task = new PersonMergeServiceTask();
			task.ServiceLogger = new TestServiceLogger();
			task.RunTask();

			var queueItemsCount = Factory.GetDatabaseCount(typeof(EdiPersonMergeQueue));
			AssertEquals("Queue items should be cleared after process", 0, queueItemsCount);
		}

		public void TestRunTask_InBatch()
		{
			SetupDataForBatching();

			var task = new PersonMergeServiceTask
			{
				ServiceLogger = new TestServiceLogger()
			};

			task.RunTask();
			var queueItemsCount = Factory.GetDatabaseCount(typeof(EdiPersonMergeQueue));
			AssertNotEquals("Should be queue items after first batch", 0, queueItemsCount);

			task.RunTask();
			queueItemsCount = Factory.GetDatabaseCount(typeof(EdiPersonMergeQueue));
			AssertEquals("Queue items should be cleared after process", 0, queueItemsCount);
		}

		[UseSnapshotProtection]
		public void TestRunTask_MultipleRunners()
		{
			using (RunNonTransactioned())
			{
				TestCaseHelper.RunClientDbCreateScripts();
				SetupDataForBatching();
				var task1 = RunTaskInNewInstance();
				var task2 = RunTaskInNewInstance();

				task1.Wait();
				task2.Wait();

				var queueItemsCount = Factory.GetDatabaseCount(typeof(EdiPersonMergeQueue));
				AssertEquals("Queue items should be cleared after process", 0, queueItemsCount);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		Task RunTaskInNewInstance()
		{
			var runner = new PersonMergeServiceTask
			{
				ServiceLogger = new TestServiceLogger()
			};
			return Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					runner.RunTask();
				}
			});
		}

		void SetupDataForBatching()
		{
			int totalCount = PersonMergeServiceTask.BatchSize * 2;
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var person1 = Factory.New<GlbPerson>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_PER = person1.PK;

			for (int i = 0; i < totalCount; i++)
			{
				var person = Factory.New<GlbPerson>();
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"User {i}";
				contact.OC_Email = $"user.{i}@test.org";
				contact.OC_PER = person.PK;

				var queueItem = Factory.New<EdiPersonMergeQueue>();
				queueItem.EMQ_PER_RetainPerson = contact1.OC_PER;
				queueItem.EMQ_PER_DissolvePerson = contact.OC_PER;
			}

			Factory.Save();
		}
	}
}
