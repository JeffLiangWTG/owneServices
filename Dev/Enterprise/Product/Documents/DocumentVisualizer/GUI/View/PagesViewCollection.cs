using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class PagesViewCollection : IViewCollection<IPageView>
	{
		public PagesViewCollection(DocumentView documentView)
		{
			Argument.NotNull(documentView, nameof(documentView));

			this.documentView = documentView;
		}

		readonly DocumentView documentView;

		void IViewCollection<IPageView>.Add(IPageView pageView)
		{
			if (pageView is Control control)
			{
				documentView.pagesLayoutPanel.Controls.Add(control);
				documentView.pagesLayoutPanel.PerformLayout();
			}
		}

		void IViewCollection<IPageView>.Remove(IPageView pageView)
		{
			var control = pageView as Control;

			if (control != null)
			{
				documentView.Controls.Remove(control);
			}
		}

		void IViewCollection<IPageView>.Clear()
		{
			var pages = this.ToArray();

			foreach (Control page in pages)
			{
				documentView.pagesLayoutPanel.Controls.Remove(page);
				page.Dispose();
			}
		}

		IEnumerator<IPageView> IEnumerable<IPageView>.GetEnumerator()
		{
			return documentView.pagesLayoutPanel.Controls.OfType<IPageView>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IPageView>)this).GetEnumerator();
		}

		int IViewCollection<IPageView>.Count
		{
			get { return this.Count(); }
		}
	}
}
