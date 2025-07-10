using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM460Provider : IIM460Provider
	{
		public IM460Provider(Im460 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im460 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn ?? ZString.Empty;

		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber ?? ZString.Empty;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn ?? ZString.Empty;

		public ZDate NotificationDate => (xmlObject.ImportOperation?.NotificationDate).ConvertToZDate();

		public ZString NotificationType => xmlObject.ImportOperation?.NotificationType ?? ZString.Empty;

		public ZDate AnticipatedControlDate => (xmlObject.ImportOperation?.AnticipatedControlDate).ConvertToZDate();

		public ZString Text => xmlObject.ImportOperation?.Text ?? ZString.Empty;

		public ZString OverallControlTypeCode => xmlObject.OverallControlType?.ControlTypeCoded ?? ZString.Empty;

		public IReadOnlyCollection<ControlTypeProvider> TypeOfControls => typeOfControls ?? (typeOfControls = xmlObject.TypeOfControls?.Select(x => new ControlTypeProvider() { SequenceNumber = x.SequenceNumber, Type = x.Type, Text = x.Remarks }).ToArray() ?? Array.Empty<ControlTypeProvider>());
		ControlTypeProvider[] typeOfControls;

		public IReadOnlyCollection<IM460RequestedDocumentsProvider> RequestedDocuments => requestedDocuments ?? (requestedDocuments = xmlObject.RequestedDocuments?.Select(x => new IM460RequestedDocumentsProvider(x)).ToArray() ?? Array.Empty<IM460RequestedDocumentsProvider>());
		IM460RequestedDocumentsProvider[] requestedDocuments;
	}
}
