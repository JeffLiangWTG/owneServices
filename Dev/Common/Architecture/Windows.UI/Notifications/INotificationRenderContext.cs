using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Information about how notifications are to be rendered on a control or grid column.
	/// </summary>
	public interface INotificationRenderContext
	{
		/// <summary>
		/// Notify that a bound control has been focused.
		/// </summary>
		void NotifyDataControlFocused(Control control, object item, string propertyName);

		/// <summary>
		/// Notify that a bound control has been focused for all properties on an item.
		/// </summary>
		void NotifyDataControlFocusedForAllProperties(Control control, object item);

		/// <summary>
		/// Notify that a piece of data has been deleted.
		/// </summary>
		void NotifyDataItemDeleted(Control control, object item);

		/// <summary>
		/// Can we always render all notifications without querying ShouldRenderNotification?
		/// </summary>
		bool ShouldAlwaysRenderAllNotifications();

		/// <summary>
		/// Should notifications on a particular item of data be rendered?
		/// </summary>
		bool ShouldRenderNotification(Control control, object item, string propertyName);

		/// <summary>
		/// Should any notifications at the object or property level of the given item be rendered?
		/// </summary>
		bool ShouldRenderAnyNotificationsFrom(Control control, object item);
	}

	public static class NotificationRenderContext
	{
		/// <summary>
		/// Notify that a bound control has been focused.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		public static void NotifyDataControlFocused(
				this INotificationRenderContext context,
				Control control, BindingManagerBase managerWithCurrentAsItem, string propertyName)
		{
			if (managerWithCurrentAsItem.Position >= 0 && managerWithCurrentAsItem.Current != null)
			{
				context.NotifyDataControlFocused(control, managerWithCurrentAsItem.Current, propertyName);
			}
		}

		/// <summary>
		/// Notify that a bound control has been focused.
		/// </summary>
		public static void NotifyDataControlFocused(
				this INotificationRenderContext context,
				Control control, CurrencyManager cm, int row, string propertyName)
		{
			if (row >= 0)
			{
				context.NotifyDataControlFocused(control, cm.List[row], propertyName);
			}
		}

		/// <summary>
		/// Should notifications on a particular item of data be rendered?
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		public static bool ShouldRenderNotification(this INotificationRenderContext context, Control control, BindingManagerBase managerWithItemAsCurrent, string propertyName)
		{
			return
					managerWithItemAsCurrent.Position >= 0 &&
					managerWithItemAsCurrent.Current != null &&
					context.ShouldRenderNotification(control, managerWithItemAsCurrent.Current, propertyName);
		}

		/// <summary>
		/// Should notifications on a particular item of data be rendered?
		/// </summary>
		public static bool ShouldRenderNotification(this INotificationRenderContext context, Control control, CurrencyManager cm, int row, string propertyName)
		{
			return
					row >= 0 &&
					context.ShouldRenderNotification(control, cm.List[row], propertyName);
		}
	}
}
