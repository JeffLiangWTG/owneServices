using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyPageViewCollection : IViewCollection<IPageView>
	{
		readonly List<IPageView> pageViews = new List<IPageView>();

		public void Add(IPageView pageView)
		{
			if (pageView != null)
			{
				pageViews.Add(pageView);
			}
		}

		public void Remove(IPageView pageView)
		{
			if (pageView != null)
			{
				pageViews.Remove(pageView);
			}
		}

		public int Count
		{
			get { return pageViews.Count; }
		}

		void IViewCollection<IPageView>.Clear()
		{
			pageViews.Clear();
		}

		IEnumerator<IPageView> IEnumerable<IPageView>.GetEnumerator()
		{
			return pageViews.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return pageViews.GetEnumerator();
		}
	}
}