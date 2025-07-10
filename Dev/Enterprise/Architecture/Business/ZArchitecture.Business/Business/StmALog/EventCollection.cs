using System.Collections;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business
{
	public class EventCollection : EventCollectionBase
	{
		public virtual void Add(Event @event)
		{
			AddInternal(@event);
		}

		public virtual void AddRange(params Event[] events)
		{
			foreach (Event @event in events)
			{
				Add(@event);
			}
		}

		public virtual void Remove(Event @event)
		{
			string code = @event.Code;
			if (List.ContainsKey(code))
			{
				List.Remove(code);
			}
		}
	}

	#region Event Collection Base

	public class EventCollectionBase : IEnumerable
	{
		public Event this[string code]
		{
			get
			{
				if (List.ContainsKey(code))
				{
					return List[code];
				}
				else
				{
					return null;
				}
			}
		}

		public bool Contains(Event @event)
		{
			return Contains(@event.Code);
		}

		public bool Contains(string code)
		{
			return List.ContainsKey(code);
		}

		public int Count
		{
			get { return List.Count; }
		}

		protected internal void AddInternal(Event @event)
		{
			string code = @event.Code;
			if (!List.ContainsKey(code))
			{
				List.Add(code, @event);
			}
		}

		#region List

		protected Dictionary<string, Event> List
		{
			get
			{
				if (fList == null)
				{
					fList = new Dictionary<string, Event>();
				}
				return fList;
			}
		}

		Dictionary<string, Event> fList;

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return List.Values.GetEnumerator();
		}

		#endregion
	}

	#endregion
}
