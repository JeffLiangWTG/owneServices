using System.Collections.ObjectModel;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Implementation of a dynamic data collection based on <see cref="ObservableCollection&lt;T&gt;"/>,
	/// that provides ability to insert item at the beginning of the collection and at a position after the specified item.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection. Must be a subclass of <see cref="RibbonViewModelBase"/>.</typeparam>
	public class RibbonCollection<T> : ObservableCollection<T> where T : RibbonViewModelBase
	{
		public void InsertAfterItem(T nextItem, T item)
		{
			Insert(IndexOf(nextItem) + 1, item);
		}

		public void InsertAtStart(T item)
		{
			Insert(0, item);
		}
	}
}
