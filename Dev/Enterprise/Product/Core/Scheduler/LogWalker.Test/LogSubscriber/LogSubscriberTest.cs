using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	[TestsSubclassesOf(typeof(LogSubscriber), new Type[0], new Type[] { typeof(MockSubscriber) })]
	/// <summary>
	/// How to write tests for LogSubscriber:
	///		You must in your tests:
	///		1. Prepare test data (create logs for your subscriber or better create objects which will insert logs)
	///		2. Use method RunLogWalkerCycleForTest() to run LogWalker cycle (read your logs from dbo.StmALog and queue they to StmJobQueue, then process them)
	///		3. Assert result after LogWalker cycle
	/// </summary>
	public abstract class LogSubscriberTest<T> : TestCaseWithFactory where T : LogSubscriber, new()
	{
		public void TestPrettyPrintSubscriberErrorInfo()
		{
			var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummyBusinessObject.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger Sample";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			trigger.ProcessTaskNotifications.AddNew();

			dummyBusinessObject.Logs.AddNew(Events.CustomisableEvent01);

			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.Factory).Returns(dummyBusinessObject.Factory);

			var subscriber = new MockSubscriber();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, subscriber, Events.MiscellaneousEventCode,
				GlbStaff.CurrentUser.GS_Code, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code,
				$"|PPK={dummyBusinessObject.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business");

			var actualMessage = subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(queuedLogMock.Object);
			AssertContains("Trigger Action - Trigger Sample", actualMessage);
		}

		public void TestRegisteredInSystemOrClientSpecificLogSubscribersList()
		{
			bool found = false;

			foreach (LogSubscriber subscriber in new SubscriberProvider().AllLogSubscribers)
			{
				if (subscriber.GetType() == typeof(T) ||
					subscriber.GetType().BaseType == typeof(T))
				{
					found = true;
					break;
				}
			}

			AssertEquals("LogSubscriber must be registered in SystemLogSubscribers class in the BatchProcessor solution", true, found);
		}

		public void TestProgrammerUnderstandsEDTEventIssue()
		{
			if (LogSubscriber.EventTypes != null)
			{
				foreach (string eventCode in LogSubscriber.EventTypes)
				{
					if (eventCode == Events.EditedARecord.Code)
					{
						string message = @"
EDT events are currently only logged when there is a form present.<BR>
Log walking on EDT events won't work if a batch processor or data import modifies a record.
<BR>
As a work around:<BR>
- Override ShouldCreateAutoLogIfOnlyChildrenHaveChanges and return true on the 'top level' auto-logged business object.<BR>
- Override OnSaving() of any dependent objects. For example, to log changes against JobVoyage, OnSaving is overridden for VoyageOrigin, VoyageDestination and JobSailing.<BR>
- Override LogSubscriberTest.IKnowEDTEventsAreOnlyLoggedWhenAFormIsPresent and return true to confirm your understanding.
";
						HtmlAssertEquals(message, true, IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent);
					}
				}
			}
			Assert(true);
		}

#if NETFRAMEWORK
		public void TestIsSerializable()
		{
			AssertEquals("The LogSubscriber must be [Serializable] as it's passed into a different AppDomain", true, typeof(T).IsSerializable);
		}
#endif

		public void TestSubscriberNameDoesNotExceedJobQueueFilterNameLength()
		{
			string assertMessage = String.Format(
				"Subscriber.Name length ({0}) <= JobQueue.FilterName maxlengh ({1})",
				LogSubscriber.Name.Length.ToString(), StmJobQueueSchema.SJ_FilterName.MaxLength.ToString());
			AssertEquals(assertMessage, true, LogSubscriber.Name.Length <= StmJobQueueSchema.SJ_FilterName.MaxLength);
		}

		public void TestSubscriberName()
		{
			int maxLength = Math.Min(35, StmJobQueueSchema.SJ_FilterName.MaxLength);
			string assertMessage = String.Format(
				"Subscriber.Name length ({0}) <= JobQueue.FilterName maxlengh ({1})",
				LogSubscriber.Name.Length.ToString(), maxLength.ToString());
			AssertEquals(assertMessage, true, LogSubscriber.Name.Length <= maxLength);

			assertMessage = "LogSubscriber.Name starts with a letter and contains only letters and digits: " + LogSubscriber.Name;
			AssertEquals(assertMessage, true, Regex.IsMatch(LogSubscriber.Name, @"^[a-z]\w*$", RegexOptions.IgnoreCase));
		}

		public void TestFactoryName()
		{
			var subscriberFactoryName = "Subscriber: " + LogSubscriber.GetType().Name;
			AssertEquals(subscriberFactoryName, LogSubscriber.FactoryName);
		}

		public void TestSubscriberFriendlyName()
		{
			int maxLength = 50;
			string assertMessage = String.Format(
				"Subscriber.FriendlyName length ({0}) <= {1}",
				LogSubscriber.FriendlyName.Length.ToString(), maxLength.ToString());
			AssertEquals(assertMessage, true, LogSubscriber.FriendlyName.Length <= maxLength);
		}

		public virtual void TestIsRequired()
		{
			Assert("Should be required by default", LogSubscriber.IsRequired);
		}

		public virtual void TestStmJobQueueIsNotSubscribedForEvents()
		{
			foreach (string tableName in LogSubscriber.TableNames)
			{
				AssertEquals(
					"StmJobQueue is not allowed in the list of subscribed tables. This would cause an infinite loop.",
					false,
					String.Compare(tableName, StmJobQueueSchema.Constants.TableName, StringComparison.OrdinalIgnoreCase) == 0);
			}
		}

		protected virtual bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return false; }
		}

		/// <summary>
		///	Use this method after you prepared logs to read your logs from dbo.StmALog and run LogWalker cycle
		/// </summary>
		protected void RunLogWalkerCycleForTest()
		{
			logSubscriber = null;
			var subscribersBeingTested = Manager.TestSubscriberProvider.AllLogSubscribers.Select(s => s.Name);
			ZQuery notProcessedLogQuery = new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusQueued);
			notProcessedLogQuery.AddToFilter(StmJobQueueSchema.SJ_RetryCount, 0);
			notProcessedLogQuery.AddToFilter(StmJobQueueSchema.SJ_FilterName, subscribersBeingTested);

			AssertEquals("PRE-CONDITION: StmJobQueue queued record count", 0, Factory.Load<StmJobQueue>(notProcessedLogQuery).Length);

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				Manager.QueueAndProcessLogs(Notifier);
			}
			var notProcessedLogQueryExcludingWTE = notProcessedLogQuery.AddToFilter(StmJobQueueSchema.SJ_SE_NKEvent, SQLComparisonOperator.NotEqual, Events.WorkflowTriggerEventCode);

			AssertEquals("POST-CONDITION: StmJobQueue queued record count", 0, Factory.Load<StmJobQueue>(notProcessedLogQueryExcludingWTE).Length);
		}

		protected List<string> RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks()
		{
			logSubscriber = null;
			ILogger notifier = new LoggerForTesting();
			Manager.QueueAndProcessLogs(notifier);
			return ((LoggerForTesting)notifier).NotifiedEventList;
		}

		protected List<string> RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks(ILogger notifier)
		{
			logSubscriber = null;
			((LoggerForTesting)notifier).AllowDebug = true;
			Manager.QueueAndProcessLogs(notifier);
			return ((LoggerForTesting)notifier).NotifiedEventList;
		}

		#region Properties

		protected T LogSubscriber
		{
			get { return logSubscriber = logSubscriber ?? new T(); }
		}

		T logSubscriber;

		protected ILogger Notifier
		{
			get { return notifier ?? (notifier = new LoggerForTesting()); }
		}
		ILogger notifier;

		protected List<string> NotifiedEventList
		{
			get
			{
				return ((LoggerForTesting)Notifier).NotifiedEventList;
			}
		}

		OperationsManagerForTesting Manager
		{
			get { return new OperationsManagerForTesting(new LogSubscriber[] { LogSubscriber }); }
		}

		#endregion
	}
}
