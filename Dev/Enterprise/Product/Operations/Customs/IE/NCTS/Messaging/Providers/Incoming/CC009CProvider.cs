using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC009C;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;
using Extensions = Enterprise.Customs.IE.Messaging.Extensions;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC009CProvider
	{
		Cc009CType XmlObject { get; }

		public CC009CProvider(Cc009CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MovementReferenceNumber => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZString LocalReferenceNumber => XmlObject.TransitOperation?.Lrn ?? ZString.Empty;

		public ZString CustomsOfficeOfDeparture => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;

		public ZBool IsInvalidated => Extensions.IsTrueOrFalse(XmlObject.Invalidation?.Decision.GetXmlEnumAttributeValue());

		public ZDateTime InvalidationRequestDateAndTime => (XmlObject.Invalidation?.RequestDateAndTime).ConvertToZDateTime();

		public ZBool IsInitiatedByCustoms => Extensions.IsTrueOrFalse(XmlObject.Invalidation?.InitiatedByCustoms.GetXmlEnumAttributeValue());

		public ZDateTime InvalidationDecisionDateAndTime => (XmlObject.Invalidation?.DecisionDateAndTime).ConvertToZDateTime();

		public ZString Justification => XmlObject.Invalidation?.Justification ?? ZString.Empty;
	}
}
