namespace Enterprise.RemoteDesktopServices.Server.TrackingInfo
{
	public static class TrackingInfoLoggerExtensions
	{
		public static void LogDragDropEvents(this TrackingInfoLogger logger, string eventName, string controlType, string controlName)
		{
			var dragDropEventLogs = $@"##################################
{eventName} Event Raised.
Control Type: {controlType}
Control Name: {controlName}
##################################";
			logger.NewLog(() => dragDropEventLogs);
		}
	}
}
