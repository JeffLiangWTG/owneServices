using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC182C;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC182CProvider
	{
		Cc182CType XmlObject { get; }

		public CC182CProvider(Cc182CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MovementReferenceNumber => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZDateTime IncidentNotificationDateAndTime => (XmlObject.TransitOperation?.IncidentNotificationDateAndTime).ConvertToZDateTime();

		public ZString CustomsOfficeOfDeparture => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;

		public ZString CustomsOfficeOfIncidentRegistration => XmlObject.CustomsOfficeOfIncidentRegistration?.ReferenceNumber ?? ZString.Empty;

		public IReadOnlyCollection<CC182CIncidentProvider> Incidents => incidents ?? (incidents = XmlObject.Consignment?.Select(x => new CC182CIncidentProvider(x)).ToArray() ?? Array.Empty<CC182CIncidentProvider>());
		IReadOnlyCollection<CC182CIncidentProvider> incidents;
	}
}
