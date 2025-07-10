using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	public sealed class DummyNotificationViews : IViewCollection<INotificationView>
	{
		readonly IList<INotificationView> views = new List<INotificationView>();

		public IEnumerator<INotificationView> GetEnumerator()
		{
			return views.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count { get; }
		public void Add(INotificationView view)
		{
			views.Add(view);
		}

		public void Remove(INotificationView view)
		{
			views.Remove(view);
		}

		public void Clear()
		{
			views.Clear();
		}
	}
}