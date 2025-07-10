using System.Collections.Generic;
using System.Diagnostics;

namespace CargoWise.ComponentModel
{
	internal sealed class NotificationCollection_DebuggerTypeProxy
	{
		public NotificationCollection_DebuggerTypeProxy(NotificationCollection collection)
		{
			Collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public INotification[] Items
		{
			get
			{
				return new List<INotification>(Collection).ToArray();
			}
		}

		readonly NotificationCollection Collection;
	}
}
