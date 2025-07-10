using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class LogsForNominatedEvent : BusinessObjectCollectionView<StmALog>, IEnumerable
	{
		public LogsForNominatedEvent(Logs logs, Event @event, bool excludeEstimated = false)
			: base(logs.ElementsInternal)
		{
			if (@event == null)
			{
				throw new ApplicationException("LogsForNominatedEvent requires a non null event to be passed to it");
			}
			NominatedEvent = @event;
			this.Logs = logs;
			this.excludeEstimated = excludeEstimated;
			Rebuild();
		}

		bool NeedsSort;
		readonly Logs Logs;
		readonly bool excludeEstimated;

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			NeedsSort = true;
		}

		public new StmALog this[int index]
		{
			get
			{
				if (NeedsSort)
				{
					Sort();
					NeedsSort = false;
				}
				return (StmALog)Elements[index];
			}
		}

		protected void Sort()
		{
			Sort(StmALog.Schema.SL_EventTime, System.ComponentModel.ListSortDirection.Descending);
		}

		public override void Load()
		{
			throw new Exception("Call Load on the Collection you are viewing, rather than the view itself");
		}

		public readonly Event NominatedEvent;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			StmALog log = (StmALog)element;
			return log.SL_SE_NKEvent == NominatedEvent.Code && !log.SL_IsCancelled && (!excludeEstimated || !log.SL_IsEstimate);
		}

		protected override void RebuildOnConstruction()
		{
			// Do Nothing - Not yet ready to build
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			throw new Exception("Add a new object through Logs instead.");
		}

		public StmALog AddNew(ZString reference)
		{
			NeedsSort = true;
			return Logs.AddNew(NominatedEvent, reference);
		}

		public StmALog AddNew(ZString reference, ZDateTimeOffset eventTime)
		{
			NeedsSort = true;
			return Logs.AddNew(NominatedEvent, reference, eventTime);
		}

		protected override BusinessObject AddNewCore()
		{
			NeedsSort = true;
			return Logs.AddNew(NominatedEvent);
		}

		public void CancelAll()
		{
			while (this.Count > 0)
			{
				this[0].Cancel();
			}
		}

		public bool DescriptionExists(ZString description)
		{
			bool result = false;
			for (int i = 0; i < Count; i++)
			{
				result = this[i].SL_Reference.StartsWith(description);
				if (result)
				{
					break;
				}
			}
			return result;
		}

		public StmALog MostRecentLog
		{
			get { return this.Count > 0 ? this[0] : null; }
		}
	}
}
