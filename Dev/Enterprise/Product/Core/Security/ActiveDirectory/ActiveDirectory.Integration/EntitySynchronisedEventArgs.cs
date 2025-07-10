using System;
using System.Collections.Generic;

namespace Enterprise.Security.ActiveDirectory
{
	public class EntitySynchronisedEventArgs : EventArgs
	{
		public IADEntity Entity { get; set; }
		public IList<ISyncEvent> SyncEvents { get; set; }
	}

	public interface ISyncEvent
	{
		string PropertyName { get; }
		object ADStartingValue { get; }
		object EnterpriseStartingValue { get; }
		object SynchronisedValue { get; }
		bool IsForcedToShowInReport { get; }
	}
}
