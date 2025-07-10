using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogDependentCollection : BaseStmALogDependentCollection<StmALog>
	{
		public StmALogDependentCollection(IStmALogParent master)
			: base(master)
		{
		}

		public StmALogDependentCollection(IStmALogParent master, BusinessObjectFactory logsFactory)
			: base(master, logsFactory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public StmALogDependentCollection(IStmALogParent master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
			SetReadOnlyIncludingChildren(true);
		}

		#region Implementation

		#region Add New
		public StmALog AddNew(Event eventType)
		{
			return AddNew(new EventValue(eventType));
		}

		public StmALog AddNew(Event eventType, ZDateTimeOffset dateTime)
		{
			return AddNew(new EventValue(eventType, dateTime));
		}

		public StmALog AddNew(Event eventType, ZString referenece, ZDateTimeOffset dateTime)
		{
			return AddNew(new EventValue(eventType, eventTime: dateTime, reference: referenece));
		}

		public StmALog AddNew(Event eventType, ZDateTimeOffset dateTime, ZBool isEstimate)
		{
			return AddNew(new EventValue(eventType, eventTime: dateTime, isEstimate: isEstimate));
		}

		public StmALog AddNew(Event eventType, ZString reference, ZDateTimeOffset dateTime, ZBool isEstimate)
		{
			return AddNew(new EventValue(eventType, reference: reference, eventTime: dateTime, isEstimate: isEstimate));
		}

		/// <summary>
		/// Adds a new StmALog to the collection. Use Events.* to specify an Event type.
		/// </summary>
		public new StmALog AddNew(Event eventType, ZString reference)
		{
			return AddNew(new EventValue(eventType, reference: reference));
		}

		/// <summary>
		/// Adds a new StmALog to the collection. Use Events.* to specify an Event type.
		/// </summary>
		public StmALog AddNew(EventValue eventValue)
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFields(true))
			{
				log.SL_SE_NKEvent = eventValue.Code;
				log.SL_IsEstimate = eventValue.IsEstimate;
				log.SL_Reference = eventValue.Reference.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength);
				if (eventValue.EventTime.IsValid)
				{
					log.SL_EventTimeOffset = eventValue.EventTime;
				}
				log.Master = (IStmALogParent)Master;
				Add(log);
			}
			return log;
		}
		#endregion

		#endregion

		#region Relationships

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var log = (StmALog)child;
			using (log.LockForUpdatingKeyFields(false))
			{
				base.SetCollectionRelationships(child);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var log = (StmALog)child;
			using (log.LockForUpdatingKeyFields(false))
			{
				base.SetDefaultsForNewChild(child);
			}
		}

		#endregion

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
			var log = (StmALog)dependent;
			using (log.LockForUpdatingKeyFields(false))
			{
				base.RemoveCollectionRelationshipsCore(log, forDelete);
			}
		}
	}
}
