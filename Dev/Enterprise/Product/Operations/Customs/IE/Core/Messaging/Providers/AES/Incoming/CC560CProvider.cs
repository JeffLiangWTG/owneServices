using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC560C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC560CProvider
	{
		public CC560CProvider(Cc560C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc560C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime ControlNotificationDateAndTime => new ZDateTime(xmlObject.ExportOperation?.ControlNotificationDateAndTime);

		public ZString ControlNotificationType => xmlObject.ExportOperation?.NotificationType;

		public ZString ControlNotificationText => xmlObject.ExportOperation?.Text;

		public IReadOnlyCollection<ControlTypeProvider> ControlTypes => controlTypes ?? (controlTypes = xmlObject.TypeOfControlsSpecified ? xmlObject.TypeOfControls.Select(x => new ControlTypeProvider()
		{
			SequenceNumber = x.SequenceNumber,
			Type = x.Type,
			Text = x.Text
		}).ToArray() : Array.Empty<ControlTypeProvider>());
		ControlTypeProvider[] controlTypes;

		public IReadOnlyCollection<RequestedDocumentProvider> RequestedDocuments => requestedDocuments ?? (requestedDocuments = xmlObject.RequestedDocumentSpecified ? xmlObject.RequestedDocument.Select(x => new RequestedDocumentProvider { SequenceNumber = x.SequenceNumber, DocumentType = x.DocumentType, Description = x.Description }).ToArray() : Array.Empty<RequestedDocumentProvider>());
		RequestedDocumentProvider[] requestedDocuments;
	}
}
