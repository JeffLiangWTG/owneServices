namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on a control that handles its own rendering of notification icons.
	/// </summary>
	public interface ISelfNotificationRendering
	{
		/// <summary>
		/// Set the notification rendering info on the control
		/// </summary>
		/// <param name="context"></param>
		void SetNotificationRenderContext(INotificationRenderContext context);

		/// <summary>
		/// Get the data items that can be focused on this control.
		/// </summary>
		object[] GetCurrentlyFocusableDataItems();

		/// <summary>
		/// Get whether we still support the default rendering of notifications provided by .net.
		/// </summary>
		bool DefaultRenderingEnabled { get; }

		/// <summary>
		/// Expose all the notifications on the control.
		/// </summary>
		void ExposeAllNotifications();
	}
}
