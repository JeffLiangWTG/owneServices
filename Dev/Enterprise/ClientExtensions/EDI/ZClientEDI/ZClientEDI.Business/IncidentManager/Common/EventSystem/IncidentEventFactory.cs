namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class IncidentEventFactory
	{
		public static class Codes
		{
			public const string CargoWiseReopen = "RCW";
			public const string ChangeCriticality = "CCR";
			public const string ChangeProperty = "CPR";
			public const string DevelopmentEstimateRequested = "DER";
			public const string ERequestReopen = "REQ";
			public const string Escalate = "ESC";
			public const string FeatureAccepted = "AUT";
			public const string FormalQuotationAccepted = "FQA";
			public const string FormalQuotationDeclined = "FQD";
			public const string FormalQuotationExpired = "FQE";
			public const string FormalQuotationRequested = "FQR";
		}

		public static void TriggerEvent(string eventCode, IIncidentEventConsumer incident)
		{
			if (!((SupportIncident)incident).IncidentEventFactoryDisabled)
			{
				IIncidentEvent incidentEvent = GetEvent(eventCode, incident);
				if (incidentEvent != null)
				{
					incidentEvent.Trigger();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static IIncidentEvent GetEvent(string eventCode, IIncidentEventConsumer consumer)
		{
			var incident = consumer as SupportIncident;
			switch (eventCode)
			{
				case Codes.CargoWiseReopen: return new SupportIncidentCargoWiseReopenEvent(incident);
				case Codes.ChangeCriticality: return new SupportIncidentChangeCriticalityEvent(incident);
				case Codes.ChangeProperty: return new SupportIncidentChangePropertyEvent(incident);
				case Codes.ERequestReopen: return new SupportIncidentClientReopenEvent(incident);

				case Codes.DevelopmentEstimateRequested:
				case Codes.Escalate:
				case Codes.FeatureAccepted:
				case Codes.FormalQuotationAccepted:
				case Codes.FormalQuotationDeclined:
				case Codes.FormalQuotationExpired:
				case Codes.FormalQuotationRequested:
					return new SupportIncidentGenericEvent(incident, eventCode);

				default: return null;
			}
		}
	}
}
