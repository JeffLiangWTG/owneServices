using System;
using System.Collections;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// Arguments to the ItemsAdded and ItemsRemoved events.
	/// </summary>
	public class CollectionItemsChangedEventArgs : EventArgs
	{
		public CollectionItemsChangedEventArgs(ICollection items)
		{
			this.items = items;
		}

		readonly ICollection items;

		/// <summary>
		/// The collection of items that changed.
		/// </summary>
		public ICollection Items
		{
			get { return items; }
		}
	}
}
