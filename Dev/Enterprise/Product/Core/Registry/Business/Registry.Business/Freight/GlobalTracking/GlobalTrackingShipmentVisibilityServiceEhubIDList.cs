namespace Enterprise.Registry.Business
{
	public class GlobalTrackingShipmentVisibilityServiceEhubIDList
	{
		public static class Code
		{
			public const string CA = "CA";
			public const string AWBA = "AWBA";
		}

		public static class Service
		{
			public const string CA = "CA";
			public const string AWBA = "AWBA";
		}

		public static class EhubID
		{
			public const string ContainerTracking = "CONTAINER_TRACKING";
			public const string FlightMonitoringSystem = "FLIGHT_MONITORING_SYSTEM";
		}

		public GlobalTrackingShipmentVisibilityServiceEhubIDList()
		{
		}

		GlobalTrackingShipmentVisibilityServiceEhubIDCollection DefaultGlobalTrackingShipmentVisibilityServiceEhubIDs
		{
			get
			{
				var list = new GlobalTrackingShipmentVisibilityServiceEhubIDCollection
				 {
				 { Code.CA, Service.CA, EhubID.ContainerTracking },
				 { Code.AWBA, Service.AWBA, EhubID.FlightMonitoringSystem }
				 };

				return list;
			}
		}

		public GlobalTrackingShipmentVisibilityServiceEhubIDCollection GetDefaultGlobalTrackingShipmentVisibilityServiceEhubIDs()
		{
			var result = new GlobalTrackingShipmentVisibilityServiceEhubIDCollection();
			result.AddRange(DefaultGlobalTrackingShipmentVisibilityServiceEhubIDs);
			return result;
		}
	}
}
