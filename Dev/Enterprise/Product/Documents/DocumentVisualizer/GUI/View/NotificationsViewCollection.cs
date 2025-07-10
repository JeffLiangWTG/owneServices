using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class NotificationsViewCollection : IViewCollection<INotificationView>
	{
		public NotificationsViewCollection(DocumentView documentView)
		{
			Argument.NotNull(documentView, nameof(documentView));

			this.documentView = documentView;
		}

		readonly DocumentView documentView;

		void IViewCollection<INotificationView>.Add(INotificationView notificationView)
		{
			var control = notificationView as Control;

			if (control != null)
			{
				control.Dock = DockStyle.Top;
				documentView.Controls.Add(control);
				documentView.PerformLayout();
			}
		}

		void IViewCollection<INotificationView>.Remove(INotificationView notificationView)
		{
			var control = notificationView as Control;

			if (control != null)
			{
				documentView.Controls.Remove(control);
			}
		}

		void IViewCollection<INotificationView>.Clear()
		{
			var notificationViews = this.ToArray().OfType<Control>();

			foreach (var control in notificationViews)
			{
				documentView.Controls.Remove(control);
				control.Dispose();
			}
		}

		IEnumerator<INotificationView> IEnumerable<INotificationView>.GetEnumerator()
		{
			return documentView.Controls.OfType<INotificationView>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<INotificationView>)this).GetEnumerator();
		}

		int IViewCollection<INotificationView>.Count => this.Count();
	}
}
