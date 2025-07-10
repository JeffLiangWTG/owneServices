using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	[TestedType(typeof(MailProcessingTask))]
	internal class MailProcessingTaskTest : MailProcessingTaskTestBase<MailProcessingTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "MAP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Mail Processing", hostedServiceAttribute.Description);
				AssertEquals("Category", "MAI", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1hour", hostedServiceAttribute.MaximumPeriod);
			});
		}

		public void TestConcurrencyError_Deleted()
		{
			var item = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "", "");
			Factory.Save();

			var mailProcessingTask = new MailProcessingTask();
			InitialiseTaskSchedule(mailProcessingTask);

			bool doUpdate = true;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (doUpdate)
				{
					Db.Connection.ExecuteNonQuery("Delete from dbo.MailDBItems");
					doUpdate = false;
				}
			});

			AssertNoExceptionThrown(() => { RunTaskScheduleWithProcessorFactory<TestFilter, MailItem>(mailProcessingTask); });

			AssertNull("the mail should be deleted.", new BusinessObjectFactory().Load<MailItem>(item.PK));
		}

		public void TestConcurrencyError_Updated()
		{
			var item = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "", "");
			Factory.Save();

			var mailProcessingTask = new MailProcessingTask();
			InitialiseTaskSchedule(mailProcessingTask);

			bool doUpdate = true;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (doUpdate)
				{
					Db.Connection.ExecuteNonQuery("UPDATE dbo.MailDBItems SET MI_Application='STD'");
					doUpdate = false;
				}
			});

			AssertNoExceptionThrown(() => { RunTaskScheduleWithProcessorFactory<TestFilter, MailItem>(mailProcessingTask); });

			var itemReloaded = new BusinessObjectFactory().Load<MailItem>(item.PK);
			AssertNotNull(itemReloaded);
			AssertEquals("STD", itemReloaded.MI_Application);
		}

		public void TestInitialise1()
		{
			MailItem item1 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, "", "");
			MailItem item2 = CreateMailItem(MailDirection.Receive, MailStatus.Processed, "", "");
			MailItem item3 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "", "");
			MailItem item4 = CreateMailItem(MailDirection.Receive, MailStatus.Failed, "", "");
			MailItem item5 = CreateMailItem(MailDirection.Transmit, MailStatus.Unprocessed, "", "");
			Factory.Save();

			MailProcessingTask mailProcessingTask = new MailProcessingTask();
			InitialiseTaskSchedule(mailProcessingTask);

			Factory.ReloadAll<MailItem>();
			AssertEquals(MailStatus.Unprocessed, item1.MI_Status);
			AssertEquals(MailStatus.Processed, item2.MI_Status);
			AssertEquals(MailStatus.Queued, item3.MI_Status);
			AssertEquals(MailStatus.Failed, item4.MI_Status);
			AssertEquals(MailStatus.Unprocessed, item5.MI_Status);
		}

		public void TestInitialise2()
		{
			IServiceTaskSchedule otherProcessorTask = Factory.New<IServiceTaskSchedule>();
			((StmScheduleTask)otherProcessorTask).S5_ScheduleType = MailProcessingTask.Code;

			MailItem item1 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, "", "");
			MailItem item2 = CreateMailItem(MailDirection.Receive, MailStatus.Processed, "", "");
			MailItem item3 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "", "");
			MailItem item4 = CreateMailItem(MailDirection.Receive, MailStatus.Failed, "", "");
			MailItem item5 = CreateMailItem(MailDirection.Transmit, MailStatus.Queued, "", "");
			Factory.Save();

			MailProcessingTask mailProcessingTask = new MailProcessingTask();
			InitialiseTaskSchedule(mailProcessingTask);

			Factory.ReloadAll<MailItem>();
			AssertEquals(MailStatus.Unprocessed, item1.MI_Status);
			AssertEquals(MailStatus.Processed, item2.MI_Status);
			AssertEquals(MailStatus.Queued, item3.MI_Status);
			AssertEquals(MailStatus.Failed, item4.MI_Status);
			AssertEquals(MailStatus.Queued, item5.MI_Status);
		}

		public void TestDoProcess()
		{
			MailProcessingTask mailProcessingTask = new MailProcessingTask();
			TestServiceLogger log = InitialiseTaskSchedule(mailProcessingTask);

			MailItem item1 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "FilterMethod1", "true");
			MailItem item2 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "FilterMethod2", "");
			MailItem item3 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, "FilterMethod1", "true");
			MailItem item4 = CreateMailItem(MailDirection.Receive, MailStatus.Failed, "FilterMethod2", "false");
			MailItem item5 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, "FilterMethod1", "false");
			MailItem item6 = CreateMailItem(MailDirection.Transmit, MailStatus.Queued, "FilterMethod2", "");
			Factory.Save();

			RunTaskScheduleWithProcessorFactory<TestFilter, MailItem>(mailProcessingTask);

			AssertEquals("FilterMethod2 failed!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();

			AssertEquals(3, log.Count);

			string log1 = null;
			string log2 = null;
			string log3 = null;
			for (int i = 0; i < log.Count; i++)
			{
				if (log[i] == "Information|true")
				{
					log1 = log[i];
				}
				else if (log[i].StartsWith(String.Format("Error|An unhandled exception occured during processing of MailItem(PK={{{0}}}).", item2.PK)))
				{
					log2 = log[i];
				}
				else if (log[i] == "Information|false")
				{
					log3 = log[i];
				}
			}

			AssertEquals("Information|true", log1);
			Assert(log2.StartsWith(String.Format("Error|An unhandled exception occured during processing of MailItem(PK={{{0}}}).", item2.PK)));
			AssertEquals("Information|false", log3);
			item1.Reload();
			item2.Reload();
			item3.Reload();
			item4.Reload();
			item5.Reload();
			item6.Reload();

			AssertEquals(MailStatus.Processed, item1.MI_Status);
			AssertEquals(MailStatus.Failed, item2.MI_Status);
			AssertEquals(MailStatus.Unprocessed, item3.MI_Status);
			AssertEquals(MailStatus.Failed, item4.MI_Status);
			AssertEquals(MailStatus.Failed, item5.MI_Status);
			AssertEquals(MailStatus.Queued, item6.MI_Status);

			log.ClearLog();
			RunTaskScheduleWithProcessorFactory<TestFilter, MailItem>(mailProcessingTask);
			AssertEquals(0, log.Count);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"Mail Processing Task",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.MailProcessingTask,
						MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
						MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued),
				};
			}
		}

		MailItem CreateMailItem(string direction, string status, string subject, string body)
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Direction = direction;
			mailItem.MI_Status = status;
			mailItem.MI_Subject = subject;
			mailItem.MI_Body = body;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now.AddSeconds(mailCount++);
			mailItem.MI_SendDateTime = ZDateTime.Now;
			mailItem.MI_Application = MailProcessingTask.Code; //TOOD: we'd add it ourselves, but 

			return mailItem;
		}

		int mailCount;

		class TestFilter
		{
			[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "FilterMethod1")]
			public bool FilterMethod1(MailItem item, ILogger logger)
			{
				logger.Log(LogType.Information, item.MI_Body);
				return item.MI_Body == "true";
			}

			[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "FilterMethod2")]
			public bool FilterMethod2(MailItem item)
			{
				throw new NotSupportedException("FilterMethod2 failed!");
			}
		}
	}
}
