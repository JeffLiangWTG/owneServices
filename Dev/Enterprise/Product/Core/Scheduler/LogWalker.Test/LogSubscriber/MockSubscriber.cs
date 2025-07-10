using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Testing
{
	[Serializable]
	class MockSubscriber : LogSubscriber
	{
		public MockSubscriber(string name, string[] eventTypes, string[] tableNames)
			: base()
		{
			this.name = name;
			this.eventTypes = eventTypes;
			this.tableNames = tableNames;
			TestLogger = new LoggerForTesting();
			SetDefaultLogger(TestLogger);
		}

		public MockSubscriber(string name)
			: this(name, new string[] { TestEventType }, new string[] { TestTableName })
		{
		}

		public MockSubscriber()
			: this(TestSubscriberName)
		{
		}

		public override string Name
		{
			get { return name; }
		}

		readonly string name;
		public const string TestSubscriberName = "MockEventSubscriber";

		public override string[] EventTypes
		{
			get { return eventTypes; }
		}

		readonly string[] eventTypes;
		public static readonly string TestEventType = Events.QueueChanged.Code;

		public override string[] TableNames
		{
			get { return tableNames; }
		}

		readonly string[] tableNames;
		public const string TestTableName = StmEventSchema.Constants.TableName;

		public override bool HasDynamicProperties => true;

		public static StmEvent TestParent(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<StmEvent>();
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			IQueuedLog queuedLog = queuedLogs[queuedLogs.Length - 1];
			lastProcessedLog_Pk = (queuedLog as StmJobQueue)?.PK.ToGuid() ?? Guid.Empty;
			lastProcessedLog_ParentId = queuedLog.SJ_ParentID.ToGuid();
			lastProcessedLog_EventTime = queuedLog.SJ_EventTime.ToDateTime();
			lastProcessedLog_User = queuedLog.SJ_GS_NKUser.ToString();
			lastProcessedLog_Reference = queuedLog.SJ_Reference.ToString();
			lastProcessedLog_FilterName = (queuedLog as StmJobQueue)?.SJ_FilterName.ToString() ?? string.Empty;
			lastProcessedLog_Event = queuedLog.SJ_SE_NKEvent.ToString();
			lastProcessedLog_TablePrefix = queuedLog.SJ_ParentTableCode.ToString();
			ProcessLogQueueItemsCore?.Invoke(queuedLogs);
		}

		public Action<IQueuedLog[]> ProcessLogQueueItemsCore { get; set; }

		public readonly LoggerForTesting TestLogger;

		#region Last Processed Log Properties

		public ZGuid LastProcessedLog_Pk
		{
			get { return lastProcessedLog_Pk; }
		}
		Guid lastProcessedLog_Pk;

		public ZGuid LastProcessedLog_ParentId
		{
			get { return lastProcessedLog_ParentId; }
		}
		Guid lastProcessedLog_ParentId;

		public ZDateTime LastProcessedLog_EventTime
		{
			get { return lastProcessedLog_EventTime; }
		}
		DateTime lastProcessedLog_EventTime;

		public ZString LastProcessedLog_User
		{
			get { return lastProcessedLog_User; }
		}
		string lastProcessedLog_User;

		public ZString LastProcessedLog_Reference
		{
			get { return lastProcessedLog_Reference; }
		}
		string lastProcessedLog_Reference;

		public ZString LastProcessedLog_FilterName
		{
			get { return lastProcessedLog_FilterName; }
		}
		string lastProcessedLog_FilterName;

		public ZString LastProcessedLog_Event
		{
			get { return lastProcessedLog_Event; }
		}
		string lastProcessedLog_Event;

		public ZString LastProcessedLog_TablePrefix
		{
			get { return lastProcessedLog_TablePrefix; }
		}
		string lastProcessedLog_TablePrefix;

		#endregion

		#region Queue New Logs

		public static StmJobQueue QueueNewLogForTestSubscriber(BusinessObjectFactory factory)
		{
			var evt = factory.NewWithValidTestData<StmEvent>();
			return QueueNewLogForGivenSubscriber(factory, MockSubscriber.TestSubscriberName, evt);
		}

		public static StmJobQueue QueueNewLogForGivenSubscriber(BusinessObjectFactory factory, string subscriberName, BusinessObject parent)
		{
			StmJobQueue log = factory.New<StmJobQueue>();
			log.SJ_FilterName = subscriberName;
			log.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log.SJ_PostedTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log.SJ_ParentID = parent.PK;
			log.SJ_ParentTableCode = parent.TablePrefix;
			log.SJ_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			log.SJ_SE_NKEvent = MockSubscriber.TestEventType;
			return log;
		}

		public static StmALog CreateNewLog(BusinessObjectFactory factory, ZString eventCode, BusinessObject parent)
		{
			var log = factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.MinSmallDateTimeValue;
				log.SL_Parent = parent.PK;
				log.SL_Table = parent.TableName;
				log.SL_SE_NKEvent = eventCode;
			}
			return log;
		}

		public StmJobQueue QueueNewLog(BusinessObjectFactory factory, BusinessObject parent)
		{
			return QueueNewLogForGivenSubscriber(factory, name, parent);
		}

		#endregion
	}
}
